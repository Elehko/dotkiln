using Elehko.Dotkiln.Core.Parsing;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class StackYamlParserTests
{
    [Fact]
    public void Parse_reads_stack_metadata_and_packages()
    {
        const string yaml = """
            schemaVersion: "0.1"
            name: aspnet-webapi-standard
            description: Baseline
            targetFramework: net8.0

            packages:
              - id: Serilog.AspNetCore
                version: "8.0.*"
                group: logging
              - id: Swashbuckle.AspNetCore
                version: "6.*"
                group: api-docs
            """;

        var stack = new StackYamlParser().Parse(yaml);

        Assert.Equal("aspnet-webapi-standard", stack.Name);
        Assert.Equal("net8.0", stack.TargetFramework);
        Assert.Equal(2, stack.Packages.Count);
        Assert.Equal("logging", stack.Packages[0].Group);
    }

    [Fact]
    public void Parse_requires_name_and_target_framework()
    {
        const string yaml = """
            packages:
              - id: Serilog.AspNetCore
                version: "8.0.*"
            """;

        Assert.Throws<StackParseException>(() => new StackYamlParser().Parse(yaml));
    }
}
