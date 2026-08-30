using Elehko.Dotkiln.Engine.Git;
using Elehko.Dotkiln.Engine.Processes;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class GitWorkingTreeGuardTests
{
    [Fact]
    public async Task EnsureCleanAsync_throws_when_git_status_has_output()
    {
        var runner = new StubProcessRunner(
            new ProcessResult(0, "true", string.Empty),
            new ProcessResult(0, " M Sample.csproj", string.Empty));

        await Assert.ThrowsAsync<DirtyWorkingTreeException>(() =>
            new GitWorkingTreeGuard(runner).EnsureCleanAsync(Environment.CurrentDirectory, force: false));
    }

    [Fact]
    public async Task EnsureCleanAsync_allows_dirty_tree_when_force_is_set()
    {
        var runner = new StubProcessRunner(
            new ProcessResult(0, "true", string.Empty),
            new ProcessResult(0, " M Sample.csproj", string.Empty));

        await new GitWorkingTreeGuard(runner).EnsureCleanAsync(Environment.CurrentDirectory, force: true);

        Assert.Equal(0, runner.Calls);
    }

    private sealed class StubProcessRunner(params ProcessResult[] results) : IProcessRunner
    {
        private int index;

        public int Calls { get; private set; }

        public Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(results[index++]);
        }
    }
}
