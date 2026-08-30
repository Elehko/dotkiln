using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Engine.ProjectFiles;
using Elehko.Dotkiln.Engine.Status;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class StatusEngineTests
{
    [Fact]
    public void GetStatus_reports_extra_packages_without_counting_them_as_drift()
    {
        using var workspace = new TemporaryWorkspace();
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
                <PackageReference Include="AutoMapper" Version="13.0.1" />
              </ItemGroup>
            </Project>
            """);
        var stack = new StackDefinition(
            "sample",
            "Sample",
            "net8.0",
            [new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging")]);

        var status = new StatusEngine(new CsprojInspector()).GetStatus(project, stack);

        Assert.False(status.HasDrift);
        Assert.True(status.HasExtraPackages);
        Assert.Contains(status.ExtraPackages, package => package.Id == "AutoMapper" && package.Version == "13.0.1");
    }

    [Fact]
    public void GetStatus_filters_extra_packages_declared_in_dotkilnignore()
    {
        using var workspace = new TemporaryWorkspace();
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
                <PackageReference Include="AutoMapper" Version="13.0.1" />
                <PackageReference Include="Polly" Version="8.2.0" />
              </ItemGroup>
            </Project>
            """);
        workspace.WriteFile(".dotkilnignore", """
            # project-specific extras
            AutoMapper
            Polly
            """);
        var stack = new StackDefinition(
            "sample",
            "Sample",
            "net8.0",
            [new PackageEntry("Serilog.AspNetCore", "8.0.*", "logging")]);

        var status = new StatusEngine(new CsprojInspector()).GetStatus(project, stack);

        Assert.False(status.HasDrift);
        Assert.False(status.HasExtraPackages);
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
