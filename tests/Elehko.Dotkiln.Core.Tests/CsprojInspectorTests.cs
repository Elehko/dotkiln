using Elehko.Dotkiln.Engine.ProjectFiles;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class CsprojInspectorTests
{
    [Fact]
    public void GetInstalledPackages_resolves_versions_from_nearest_Directory_Packages_props()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />
              </ItemGroup>
            </Project>
            """);
        workspace.WriteFile("src/Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="8.0.4" />
                <PackageVersion Include="Polly" Version="8.2.0" />
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("src/Sample/Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" />
                <PackageReference Include="Polly" />
              </ItemGroup>
            </Project>
            """);

        var packages = new CsprojInspector().GetInstalledPackages(project);

        Assert.Contains(packages, package => package.Id == "Serilog.AspNetCore" && package.Version == "8.0.4");
        Assert.Contains(packages, package => package.Id == "Polly" && package.Version == "8.2.0");
    }

    [Fact]
    public void GetInstalledPackages_prefers_local_version_over_central_version()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" Version="8.0.5" />
              </ItemGroup>
            </Project>
            """);

        var package = Assert.Single(new CsprojInspector().GetInstalledPackages(project));

        Assert.Equal("Serilog.AspNetCore", package.Id);
        Assert.Equal("8.0.5", package.Version);
    }

    [Fact]
    public void GetInstalledPackages_uses_version_override_when_present()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />
              </ItemGroup>
            </Project>
            """);
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" VersionOverride="8.0.6" />
              </ItemGroup>
            </Project>
            """);

        var package = Assert.Single(new CsprojInspector().GetInstalledPackages(project));

        Assert.Equal("Serilog.AspNetCore", package.Id);
        Assert.Equal("8.0.6", package.Version);
    }

    [Fact]
    public void GetInstalledPackages_ignores_ProjectReference_entries()
    {
        using var workspace = new TemporaryWorkspace();
        var project = workspace.WriteFile("Sample.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
                <ProjectReference Include="..\Library\Library.csproj" />
              </ItemGroup>
            </Project>
            """);

        var package = Assert.Single(new CsprojInspector().GetInstalledPackages(project));

        Assert.Equal("Serilog.AspNetCore", package.Id);
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
