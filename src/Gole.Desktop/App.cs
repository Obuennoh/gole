using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Gole.Core;
using Gole.Infrastructure;
using Gole.Desktop.Updates;
using Velopack;

namespace Gole.Desktop;

internal static class App
{
    public static bool DevelopmentMode
    {
        get; private set;
    }

    [STAThread]
    private static int Main(string[] args)
    {
        VelopackApp.Build().Run();
        if (args.Length > 0 && args[0].StartsWith("--smoke-", StringComparison.Ordinal))
            return InstallationSmoke.Run(args);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
        if (args.Length == 2 && args[0] == "--render-sheet")
            return VisualTests.Render(args[1]);
        DevelopmentMode = args.Length == 2 && args[0] == "--development-data";
        if (args.Length != 0 && !DevelopmentMode)
            return 2;
        var folder = DevelopmentMode
            ? Path.GetFullPath(args[1])
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gole");
        var store = new JsonStateStore(folder);
        using var singleton = new Mutex(true, DevelopmentMode ? "Local\\Gole.Desktop.Development" : "Local\\Gole.Desktop", out var acquired);
        if (!acquired)
        {
            MessageBox.Show("O Gole já está ativo. Use o ícone perto do relógio para abrir o painel.", "Gole");
            return 0;
        }
        try
        {
            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Theme.Init(app);
            app.DispatcherUnhandledException += (_, e) =>
            {
                store.Log(e.Exception);
                e.Handled = true;
                MessageBox.Show("Não foi possível concluir a ação. Reabra o Gole. Os registros salvos foram preservados.", "Gole");
                app.Shutdown(1);
            };
            var loaded = store.Load(DateTime.UtcNow);
            // Pause controls were removed; consume any pause retained by older installations.
            if (loaded.State.PauseEnd != 0)
                loaded.State.Resume(DateTime.UtcNow);
            var session = new HydrationSession(loaded.State);
            LegacyUpdateCredentials.Remove(folder);
            using var updates = new DesktopUpdateService(new GithubUpdateBackend());
            var pet = new CompanionWindow(session, store, updates);
            app.Exit += (_, _) => { pet.Save(); updates.ApplyOnExit(); updates.Dispose(); };
            pet.Show();
            pet.OpenPanel(DevelopmentMode ? 1 : 0);
            if (!DevelopmentMode)
                _ = updates.RunAsync();
            return app.Run();
        }
        catch (Exception ex)
        {
            store.Log(ex);
            MessageBox.Show("Não foi possível abrir o Gole. " + ex.Message, "Gole");
            return 1;
        }
        finally { singleton.ReleaseMutex(); }
    }
}
