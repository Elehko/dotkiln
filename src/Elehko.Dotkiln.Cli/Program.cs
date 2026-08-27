using Dotkiln.Core.Parsing;
using Dotkiln.Core.Models;
using Dotkiln.Core.Validation;
using Dotkiln.Engine.Apply;
using Dotkiln.Engine.ProjectFiles;
using Dotkiln.Updates.Planning;

var exitCode = await RunAsync(args);
return exitCode;

static async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help")
    {
        PrintHelp();
        return 0;
    }

    return args[0].ToLowerInvariant() switch
    {
        "validate" => Validate(args),
        "status" => Status(args),
        "update" => Update(args),
        _ => UsageError($"Unknown command '{args[0]}'.")
    };
}

static int Validate(string[] args)
{
    if (args.Length < 2)
    {
        return UsageError("Usage: dotkiln validate <stack-file>");
    }

    var stack = LoadStack(args[1]);
    var result = new StackValidator().Validate(stack);

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

static int Status(string[] args)
{
    if (args.Length < 3)
    {
        return UsageError("Usage: dotkiln status <stack-file> <project.csproj>");
    }

    var stack = LoadStack(args[1]);
    var plan = new ApplyEngine(new CsprojInspector()).Plan(args[2], stack);

    Console.WriteLine($"Stack: {stack.Name}");
    Console.WriteLine(plan.HasChanges ? "Drift detected." : "Project matches stack.");

    foreach (var package in plan.MissingPackages)
    {
        Console.WriteLine($"  missing      {package.Id} {package.Version}");
    }

    foreach (var package in plan.OutOfRangePackages)
    {
        Console.WriteLine($"  out-of-range {package.Id} {package.Version}");
    }

    return plan.HasChanges ? 1 : 0;
}

static int Update(string[] args)
{
    if (args.Length < 2)
    {
        return UsageError("Usage: dotkiln update <stack-file> [--group name]");
    }

    var requestedGroup = ReadOption(args, "--group");
    var stack = LoadStack(args[1]);
    var groups = new UpdateGroupPlanner().Plan(stack, requestedGroup);

    Console.WriteLine($"Planning updates ({groups.Count} groups)...");
    foreach (var group in groups)
    {
        Console.WriteLine($"  {group.Name} ({group.Packages.Count} packages)");
    }

    return 0;
}

static StackDefinition LoadStack(string path)
{
    return new StackYamlParser().Parse(File.ReadAllText(path));
}

static string? ReadOption(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static int UsageError(string message)
{
    Console.Error.WriteLine(message);
    return 2;
}

static void PrintHelp()
{
    Console.WriteLine("Dotkiln - package stack manager for .NET");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  validate <stack-file>");
    Console.WriteLine("  status <stack-file> <project.csproj>");
    Console.WriteLine("  update <stack-file> [--group name]");
}
