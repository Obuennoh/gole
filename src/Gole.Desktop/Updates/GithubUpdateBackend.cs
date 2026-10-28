using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Gole.Desktop.Updates;

internal sealed class GithubUpdateBackend(UpdateManager manager = null) : IUpdateBackend
{
    internal const string RepositoryUrl = "https://github.com/Obuennoh/gole";
    private readonly UpdateManager local = manager ?? new(new GithubSource(RepositoryUrl, null, false));
    public bool IsInstalled => local.IsInstalled;
    public string CurrentVersion => local.CurrentVersion?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "desenvolvimento";

    public async Task<string> StageLatestAsync(CancellationToken cancellationToken)
    {
        var manager = local;
        if (manager.UpdatePendingRestart != null)
            return manager.UpdatePendingRestart.Version.ToString();
        var update = await manager.CheckForUpdatesAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
        if (update == null)
            return null;
        await manager.DownloadUpdatesAsync(update, cancelToken: cancellationToken).ConfigureAwait(false);
        // Velopack applies a staged update at the next startup, before WPF or the mutex starts.
        return update.TargetFullRelease.Version.ToString();
    }

    public void ApplyOnExit()
    {
        var pending = local.UpdatePendingRestart;
        if (pending != null)
            local.WaitExitThenApplyUpdates(pending, silent: true, restart: false);
    }
}
