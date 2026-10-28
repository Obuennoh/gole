using System;
using System.IO;
using System.Text.Json;
using Velopack;

namespace Gole.Desktop.Updates;

internal static class InstallationSmoke
{
    internal static int Run(string[] args)
    {
        if (args.Length == 2 && args[0] == "--smoke-status")
        {
            var manager = GithubUpdateBackend.CreateManager();
            File.WriteAllText(args[1], JsonSerializer.Serialize(new
            {
                Installed = manager.IsInstalled,
                Version = manager.CurrentVersion?.ToString()
            }));
            return manager.IsInstalled ? 0 : 1;
        }
        if (args.Length == 3 && args[0] == "--smoke-update")
        {
            var manager = new UpdateManager(Path.GetFullPath(args[1]));
            using var updates = new DesktopUpdateService(new GithubUpdateBackend(manager));
            updates.CheckAsync().GetAwaiter().GetResult();
            if (updates.Phase != UpdatePhase.Ready || manager.UpdatePendingRestart == null)
                return 3;
            File.WriteAllText(args[2], manager.UpdatePendingRestart.Version.ToString());
            updates.ApplyOnExit();
            return 0;
        }
        if (args.Length == 2 && args[0] == "--smoke-public-feed")
        {
            try
            {
                var manager = GithubUpdateBackend.CreateManager();
                manager.CheckForUpdatesAsync().GetAwaiter().GetResult();
                File.WriteAllText(args[1], "Public feed reached without credentials.");
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(args[1], ex.ToString());
                return 4;
            }
        }
        return 2;
    }
}
