using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Engine.Apply;
using Elehko.Dotkiln.Engine.NuGetResolution;
using Elehko.Dotkiln.Engine.Processes;
using Elehko.Dotkiln.Engine.ProjectFiles;
using System.Xml.Linq;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class ApplyEngineTests
{
    [Fact]
    public async Task ApplyAsync_adds_missing_package_reference_and_central_version_for_cpm_project()
    {
        using var workspace = new TemporaryWorkspace();
        var centralPackages = workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
              </ItemGroup>
            </Project>
            """);
        var stack = CreateStack(new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging"));

        var result = await CreateEngine().ApplyAsync(project, stack);

        Assert.True(result.Succeeded);
        Assert.Contains(XDocument.Load(project).Descendants("PackageReference"), package => package.Attribute("Include")?.Value == "Serilog.AspNetCore" && package.Attribute("Version") is null);
        Assert.Contains(XDocument.Load(centralPackages).Descendants("PackageVersion"), package => package.Attribute("Include")?.Value == "Serilog.AspNetCore" && package.Attribute("Version")?.Value == "8.0.3");
    }

    [Fact]
    public async Task ApplyAsync_updates_existing_central_version_for_cpm_project()
    {
        using var workspace = new TemporaryWorkspace();
        var centralPackages = workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="7.0.0" />
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" />
              </ItemGroup>
            </Project>
            """);
        var stack = CreateStack(new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging"));

        var result = await CreateEngine().ApplyAsync(project, stack);

        Assert.True(result.Succeeded);
        Assert.Contains(XDocument.Load(centralPackages).Descendants("PackageVersion"), package => package.Attribute("Include")?.Value == "Serilog.AspNetCore" && package.Attribute("Version")?.Value == "8.0.3");
    }

    [Fact]
    public async Task ApplyAsync_updates_version_override_without_changing_central_version()
    {
        using var workspace = new TemporaryWorkspace();
        var centralPackages = workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="7.0.0" />
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" VersionOverride="7.0.0" />
              </ItemGroup>
            </Project>
            """);
        var stack = CreateStack(new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging"));

        var result = await CreateEngine().ApplyAsync(project, stack);

        Assert.True(result.Succeeded);
        Assert.Contains(XDocument.Load(project).Descendants("PackageReference"), package => package.Attribute("Include")?.Value == "Serilog.AspNetCore" && package.Attribute("VersionOverride")?.Value == "8.0.3");
        Assert.Contains(XDocument.Load(centralPackages).Descendants("PackageVersion"), package => package.Attribute("Include")?.Value == "Serilog.AspNetCore" && package.Attribute("Version")?.Value == "7.0.0");
    }

    [Fact]
    public async Task ApplyAsync_dry_run_does_not_write_cpm_files()
    {
        using var workspace = new TemporaryWorkspace();
        var centralPackages = workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
              </ItemGroup>
            </Project>
            """);
        var originalProject = File.ReadAllText(project);
        var originalCentralPackages = File.ReadAllText(centralPackages);
        var stack = CreateStack(new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging"));

        var result = await CreateEngine().ApplyAsync(project, stack, dryRun: true);

        Assert.True(result.Succeeded);
        Assert.Equal(originalProject, File.ReadAllText(project));
        Assert.Equal(originalCentralPackages, File.ReadAllText(centralPackages));
        Assert.Contains(result.Messages, message => message.Contains("Would add PackageReference", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("Would set central PackageVersion", StringComparison.Ordinal));
    }

    private static ApplyEngine CreateEngine()
    {
        return new ApplyEngine(new CsprojInspector(), new FixedVersionResolver("8.0.3"), new ThrowingProcessRunner());
    }

    private static StackDefinition CreateStack(params PackageEntry[] packages)
    {
        return new StackDefinition("sample", "Sample", "net8.0", packages);
    }

    private sealed class FixedVersionResolver(string version) : INuGetVersionResolver
    {
        public Task<string?> ResolveLatestMatchingAsync(PackageEntry package, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(version);
        }
    }

    private sealed class ThrowingProcessRunner : IProcessRunner
    {
        public Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("CPM apply should not run dotnet add package.");
        }
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), $"dotkiln-tests-{Guid.NewGuid():N}");

        public TemporaryWorkspace()
        {
            Directory.CreateDirectory(root);
        }

        public string WriteFile(string relativePath, string contents)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? root);
            File.WriteAllText(path, contents);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
