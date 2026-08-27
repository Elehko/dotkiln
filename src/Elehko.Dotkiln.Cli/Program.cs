using System.Text.Json;
using Elehko.Dotkiln.Core.Loading;
using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Parsing;
using Elehko.Dotkiln.Core.Validation;
using Elehko.Dotkiln.Engine.Apply;
using Elehko.Dotkiln.Engine.NuGetResolution;
using Elehko.Dotkiln.Engine.Processes;
using Elehko.Dotkiln.Engine.ProjectFiles;
using Elehko.Dotkiln.Engine.Status;
using Elehko.Dotkiln.Updates.Execution;
using Elehko.Dotkiln.Updates.Isolation;
using Elehko.Dotkiln.Updates.Planning;
using Elehko.Dotkiln.Updates.Verification;

var exitCode = await new CliApp().RunAsync(args);
return exitCode;

internal sealed class CliApp
{
    private readonly CsprojInspector inspector = new();
    private readonly StackLoader stackLoader = new(new StackYamlParser());
    private readonly StackValidator validator = new();

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || Has(args, "-h") || Has(args, "--help"))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "new" => await NewAsync(args),
                "apply" => await ApplyAsync(args),
                "status" => await StatusAsync(args),
                "update" => await UpdateAsync(args),
                "validate" => await ValidateAsync(args),
                "registry" => await RegistryAsync(args),
                _ => UsageError($"Unknown command '{args[0]}'.")
            };
        }
        catch (Exception exception)
        {
            if (Has(args, "--json"))
            {
                WriteJson(new { succeeded = false, error = exception.Message });
            }
            else
            {
                Console.Error.WriteLine(exception.Message);
            }

            return 3;
        }
    }

    private async Task<int> NewAsync(string[] args)
    {
        if (args.Length < 3)
        {
            return UsageError("Usage: dotkiln new <stack> <project-name> [--template webapi] [--dry-run] [--json]");
        }

        var stack = await LoadAndValidateAsync(args[1]);
        var projectName = args[2];
        var template = Option(args, "--template") ?? "webapi";
        var dryRun = Has(args, "--dry-run");
        var json = Has(args, "--json");
        var runner = new ProcessRunner();

        if (dryRun)
        {
            return Report(json, new { succeeded = true, command = $"dotnet new {template} -n {projectName}", stack = stack.Name }, $"Would run: dotnet new {template} -n {projectName}");
        }

        var create = await runner.RunAsync("dotnet", $"new {template} -n \"{projectName}\"", Environment.CurrentDirectory);
        if (create.ExitCode != 0)
        {
            return Report(json, new { succeeded = false, output = create.Output }, create.Output, 3);
        }

        var projectPath = inspector.ResolveProjectPath(projectName);
        var apply = await CreateApplyEngine().ApplyAsync(projectPath, stack);
        PrintSnippet(stack, args[1]);
        return Report(json, new { apply.Succeeded, projectPath, apply.Messages }, string.Join(Environment.NewLine, apply.Messages), apply.Succeeded ? 0 : 3);
    }

    private async Task<int> ApplyAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return UsageError("Usage: dotkiln apply <stack> [project.csproj] [--dry-run] [--json]");
        }

        var stack = await LoadAndValidateAsync(args[1]);
        var projectPath = Positional(args, 2) ?? Environment.CurrentDirectory;
        var result = await CreateApplyEngine().ApplyAsync(projectPath, stack, Has(args, "--dry-run"));
        PrintSnippet(stack, args[1]);
        return Report(Has(args, "--json"), new { result.Succeeded, result.Messages }, string.Join(Environment.NewLine, result.Messages), result.Succeeded ? 0 : 3);
    }

    private async Task<int> StatusAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return UsageError("Usage: dotkiln status <stack> [project.csproj] [--json]");
        }

        var stack = await LoadAndValidateAsync(args[1]);
        var projectPath = Positional(args, 2) ?? Environment.CurrentDirectory;
        var status = new StatusEngine(inspector).GetStatus(projectPath, stack);
        if (Has(args, "--json"))
        {
            WriteJson(status);
            return status.HasDrift ? 1 : 0;
        }

        Console.WriteLine($"Stack: {status.StackName}");
        foreach (var group in status.Groups)
        {
            Console.WriteLine($"  {group.Name,-12} {(group.IsUpToDate ? "up to date" : "drift detected")}");
            foreach (var package in group.Packages.Where(package => package.State != "up-to-date"))
            {
                Console.WriteLine($"    {package.State,-12} {package.Id} {package.InstalledVersion ?? "(missing)"} -> {package.RequestedVersion}");
            }
        }

        return status.HasDrift ? 1 : 0;
    }

    private async Task<int> UpdateAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return UsageError("Usage: dotkiln update <stack> [project.csproj] [--group name] [--dry-run] [--json]");
        }

        var stack = await LoadAndValidateAsync(args[1]);
        var projectPath = Positional(args, 2) ?? Environment.CurrentDirectory;
        var processRunner = new ProcessRunner();
        var workflow = new UpdateWorkflow(
            inspector,
            new TempCopyIsolator(),
            new BuildAndTestVerifier(processRunner),
            CreateApplyEngine(processRunner));

        var results = await workflow.RunAsync(projectPath, stack, Option(args, "--group"), Has(args, "--dry-run"));
        if (Has(args, "--json"))
        {
            WriteJson(results);
            return results.All(result => result.Succeeded) ? 0 : 1;
        }

        Console.WriteLine($"Planning updates ({results.Count} groups)...");
        foreach (var result in results)
        {
            Console.WriteLine($"  {result.Group.Name}: {result.Message}");
            if (result.LogPath is not null)
            {
                Console.WriteLine($"    See {result.LogPath}");
            }
        }

        return results.All(result => result.Succeeded) ? 0 : 1;
    }

    private async Task<int> ValidateAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return UsageError("Usage: dotkiln validate <stack-file> [--json]");
        }

        var stack = await stackLoader.LoadAsync(args[1]);
        var result = validator.Validate(stack);
        if (Has(args, "--json"))
        {
            WriteJson(new { result.IsValid, result.Errors, stack.Name, PackageCount = stack.Packages.Count });
            return result.IsValid ? 0 : 2;
        }

        if (result.IsValid)
        {
            Console.WriteLine($"Stack '{stack.Name}' is valid ({stack.Packages.Count} packages).");
            return 0;
        }

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error);
        }

        return 2;
    }

    private async Task<int> RegistryAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return UsageError("Usage: dotkiln registry <search|publish> ...");
        }

        return args[1].ToLowerInvariant() switch
        {
            "search" => RegistrySearch(args),
            "publish" => await RegistryPublishAsync(args),
            _ => UsageError($"Unknown registry command '{args[1]}'.")
        };
    }

    private int RegistrySearch(string[] args)
    {
        if (args.Length < 3)
        {
            return UsageError("Usage: dotkiln registry search <term> [--registry-dir stacks] [--json]");
        }

        var term = args[2];
        var registryDir = Option(args, "--registry-dir") ?? "stacks";
        var matches = Directory.Exists(registryDir)
            ? Directory.GetFiles(registryDir, "*.dotkiln.yaml")
                .Where(path => Path.GetFileName(path).Contains(term, StringComparison.OrdinalIgnoreCase) || File.ReadAllText(path).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName)
                .ToArray()
            : [];

        if (Has(args, "--json"))
        {
            WriteJson(matches);
        }
        else
        {
            foreach (var match in matches)
            {
                Console.WriteLine(match);
            }
        }

        return 0;
    }

    private async Task<int> RegistryPublishAsync(string[] args)
    {
        if (args.Length < 3)
        {
            return UsageError("Usage: dotkiln registry publish <stack-file> [--registry-dir stacks] [--dry-run] [--json]");
        }

        var source = args[2];
        var stack = await LoadAndValidateAsync(source);
        var registryDir = Option(args, "--registry-dir") ?? "stacks";
        var destination = Path.Combine(registryDir, $"{stack.Name}.dotkiln.yaml");

        if (Has(args, "--dry-run"))
        {
            return Report(Has(args, "--json"), new { stack.Name, destination }, $"Would publish {stack.Name} to {destination}");
        }

        Directory.CreateDirectory(registryDir);
        File.Copy(source, destination, overwrite: true);
        return Report(Has(args, "--json"), new { stack.Name, destination }, $"Published {stack.Name} to {destination}");
    }

    private async Task<StackDefinition> LoadAndValidateAsync(string source)
    {
        var stack = await stackLoader.LoadAsync(source);
        var result = validator.Validate(stack);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Errors));
        }

        return stack;
    }

    private ApplyEngine CreateApplyEngine(IProcessRunner? processRunner = null)
    {
        return new ApplyEngine(inspector, new NuGetVersionResolver(), processRunner ?? new ProcessRunner());
    }

    private static string? Option(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static bool Has(string[] args, string name)
    {
        return args.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    private static string? Positional(string[] args, int index)
    {
        if (args.Length <= index || args[index].StartsWith("--", StringComparison.Ordinal))
        {
            return null;
        }

        return args[index];
    }

    private static void PrintSnippet(StackDefinition stack, string stackSource)
    {
        if (string.IsNullOrWhiteSpace(stack.Snippet) || Uri.TryCreate(stackSource, UriKind.Absolute, out _))
        {
            return;
        }

        var snippetPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stackSource)) ?? Environment.CurrentDirectory, stack.Snippet);
        if (!File.Exists(snippetPath))
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Suggested starter code:");
        Console.WriteLine(File.ReadAllText(snippetPath));
    }

    private static int Report(bool json, object payload, string text, int exitCode = 0)
    {
        if (json)
        {
            WriteJson(payload);
        }
        else if (!string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine(text);
        }

        return exitCode;
    }

    private static void WriteJson(object payload)
    {
        Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int UsageError(string message)
    {
        Console.Error.WriteLine(message);
        return 2;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Dotkiln - package stack manager for .NET");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  new <stack> <project-name>");
        Console.WriteLine("  apply <stack> [project.csproj]");
        Console.WriteLine("  status <stack> [project.csproj]");
        Console.WriteLine("  update <stack> [project.csproj] [--group name]");
        Console.WriteLine("  validate <stack>");
        Console.WriteLine("  registry search <term>");
        Console.WriteLine("  registry publish <stack-file>");
        Console.WriteLine();
        Console.WriteLine("Global flags: --dry-run, --json");
    }
}
