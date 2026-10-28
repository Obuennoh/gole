using System.IO;
using Gole.Desktop.Updates;

namespace Gole.Desktop.Tests;

public sealed class UpdateServiceTests
{
    private sealed class Backend : IUpdateBackend
    {
        public bool Installed = true, Fail;
        public string? Staged;
        public int Calls, Concurrent, MaximumConcurrent, ApplyCalls;
        public bool IsInstalled => Installed;
        public string CurrentVersion => "0.1.1";
        public async Task<string> StageLatestAsync(CancellationToken cancellationToken)
        {
            Calls++;
            Concurrent++;
            MaximumConcurrent = Math.Max(MaximumConcurrent, Concurrent);
            try
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
                if (Fail)
                    throw new IOException("server detail");
                return Staged!;
            }
            finally { Concurrent--; }
        }
        public void ApplyOnExit()
        {
            ApplyCalls++;
            if (Fail)
                throw new IOException("apply failure");
        }
    }

    [Fact]
    public async Task Installed_apps_download_updates_without_credentials_or_setup()
    {
        var backend = new Backend { Staged = "0.1.2" };
        using var updates = new DesktopUpdateService(backend);
        Assert.Equal(UpdatePhase.Idle, updates.Phase);
        await updates.CheckAsync();
        Assert.Equal(UpdatePhase.Ready, updates.Phase);
        Assert.Contains("ao sair", updates.Status);
        Assert.Equal(1, backend.Calls);
        updates.ApplyOnExit();
        Assert.Equal(1, backend.ApplyCalls);
    }

    [Fact]
    public async Task Development_builds_never_download_or_replace_files()
    {
        var backend = new Backend { Installed = false };
        using var updates = new DesktopUpdateService(backend);
        await updates.CheckAsync();
        await updates.RunAsync();
        updates.ApplyOnExit();
        Assert.Equal(0, backend.Calls);
        Assert.Equal(0, backend.ApplyCalls);
        Assert.Equal(UpdatePhase.Development, updates.Phase);
    }

    [Fact]
    public async Task Network_errors_are_nonfatal_and_later_checks_retry()
    {
        var backend = new Backend { Fail = true };
        using var updates = new DesktopUpdateService(backend);
        await updates.CheckAsync();
        Assert.Equal(UpdatePhase.Unavailable, updates.Phase);
        Assert.DoesNotContain("server detail", updates.Status);
        backend.Fail = false;
        await updates.CheckAsync();
        Assert.Equal(UpdatePhase.Current, updates.Phase);
        Assert.Equal(2, backend.Calls);
    }

    [Fact]
    public void Applying_failure_does_not_block_shutdown()
    {
        using var updates = new DesktopUpdateService(new Backend { Fail = true });
        updates.ApplyOnExit();
    }

    [Fact]
    public async Task Concurrent_manual_and_automatic_checks_are_serialized()
    {
        var backend = new Backend();
        using var updates = new DesktopUpdateService(backend);
        await Task.WhenAll(updates.CheckAsync(), updates.CheckAsync(), updates.CheckAsync());
        Assert.Equal(1, backend.MaximumConcurrent);
        Assert.Equal(UpdatePhase.Current, updates.Phase);
    }

    [Fact]
    public async Task Closing_cancels_pending_checks_without_reporting_an_error()
    {
        var backend = new Backend();
        var updates = new DesktopUpdateService(backend);
        updates.Dispose();
        await updates.CheckAsync();
        Assert.Equal(0, backend.Calls);
        Assert.NotEqual(UpdatePhase.Unavailable, updates.Phase);
    }

    [Fact]
    public void Legacy_authentication_files_are_removed_without_touching_preferences()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Gole.Update.Tests")) + Path.DirectorySeparatorChar;
        string folder = Path.Combine(root, Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "updates-auth.dat"), "legacy encrypted data");
            File.WriteAllText(Path.Combine(folder, "updates-auth.dat.tmp"), "legacy temporary data");
            File.WriteAllText(Path.Combine(folder, "preferences.json"), "preferences marker");
            LegacyUpdateCredentials.Remove(folder);
            Assert.False(File.Exists(Path.Combine(folder, "updates-auth.dat")));
            Assert.False(File.Exists(Path.Combine(folder, "updates-auth.dat.tmp")));
            Assert.Equal("preferences marker", File.ReadAllText(Path.Combine(folder, "preferences.json")));
            LegacyUpdateCredentials.Remove(folder);
        }
        finally
        {
            if (Path.GetFullPath(folder).StartsWith(root, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }
}
