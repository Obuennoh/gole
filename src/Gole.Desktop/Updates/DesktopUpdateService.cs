using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gole.Desktop.Updates;

internal interface IUpdateBackend
{
    bool IsInstalled
    {
        get;
    }
    string CurrentVersion
    {
        get;
    }
    Task<string> StageLatestAsync(CancellationToken cancellationToken);
    void ApplyOnExit();
}

internal enum UpdatePhase
{
    Development, Idle, Checking, Current, Ready, Unavailable
}

internal sealed class DesktopUpdateService(IUpdateBackend backend) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly CancellationTokenSource lifetime = new();
    internal event Action Changed;
    internal bool IsInstalled => backend.IsInstalled;
    internal string Version => backend.CurrentVersion;
    internal UpdatePhase Phase { get; private set; } = backend.IsInstalled ? UpdatePhase.Idle : UpdatePhase.Development;
    internal string Status
    {
        get; private set;
    } = backend.IsInstalled
        ? "Atualizações automáticas ativadas."
        : "Atualização automática disponível na versão instalada.";

    private void SetStatus(UpdatePhase phase, string text)
    {
        Phase = phase;
        Status = text;
        Changed?.Invoke();
    }

    internal async Task CheckAsync()
    {
        if (!backend.IsInstalled)
            return;
        bool entered = false;
        try
        {
            await gate.WaitAsync(lifetime.Token).ConfigureAwait(false);
            entered = true;
            SetStatus(UpdatePhase.Checking, "Verificando e baixando atualizações…");
            string version = await backend.StageLatestAsync(lifetime.Token).ConfigureAwait(false);
            SetStatus(version == null ? UpdatePhase.Current : UpdatePhase.Ready,
                version == null ? "Você está na versão mais recente." : "Versão " + version + " pronta. Será instalada ao sair do Gole.");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception)
        {
            SetStatus(UpdatePhase.Unavailable, "Sem conexão para atualizar. Vou tentar novamente automaticamente.");
        }
        finally { if (entered) gate.Release(); }
    }

    internal void ApplyOnExit()
    {
        if (!backend.IsInstalled)
            return;
        try
        {
            backend.ApplyOnExit();
        }
        catch (Exception) { /* Startup retries any staged update if applying on exit fails. */ }
    }

    internal async Task RunAsync()
    {
        if (!backend.IsInstalled)
            return;
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                await CheckAsync().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromHours(6), lifetime.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
    }

    public void Dispose()
    {
        Changed = null;
        lifetime.Cancel();
    }
}
