using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Validation;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class StackValidatorTests
{
    [Fact]
    public void Validate_rejects_duplicate_packages()
    {
        var stack = new StackDefinition(
            "sample",
            "Sample",
            "net8.0",
            [
                new PackageEntry("Serilog", "3.*"),
                new PackageEntry("Serilog", "3.*")
            ]);

        var result = new StackValidator().Validate(stack);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("declared more than once", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_invalid_group_names()
    {
        var stack = new StackDefinition(
            "sample",
            "Sample",
            "net8.0",
            [new PackageEntry("Serilog", "3.*", "Bad_Group")]);

        var result = new StackValidator().Validate(stack);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("invalid group", StringComparison.OrdinalIgnoreCase));
    }
}
