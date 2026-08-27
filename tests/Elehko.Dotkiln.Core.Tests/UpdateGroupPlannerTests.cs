using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Updates.Planning;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class UpdateGroupPlannerTests
{
    [Fact]
    public void Plan_groups_packages_by_stack_group()
    {
        var stack = new StackDefinition(
            "sample",
            "Sample",
            "net8.0",
            [
                new PackageEntry("Microsoft.EntityFrameworkCore.SqlServer", "8.0.*", "ef-core"),
                new PackageEntry("Microsoft.EntityFrameworkCore.Tools", "8.0.*", "ef-core"),
                new PackageEntry("Swashbuckle.AspNetCore", "6.*", "api-docs")
            ]);

        var groups = new UpdateGroupPlanner().Plan(stack);

        Assert.Equal(2, groups.Count);
        Assert.Contains(groups, group => group.Name == "ef-core" && group.Packages.Count == 2);
    }
}
