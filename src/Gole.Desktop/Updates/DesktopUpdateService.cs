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
    bool RestartToApply();
}

internal enum UpdatePhase
{
    Development, Idle, Checking, Current, Ready, Unavailable
}

internal sealed class DesktopUpdateService(IUpdateBackend backend, Func<TimeSpan, CancellationToken, Task> delay = null) : IDisposable
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

    internal async Task<bool> PrepareLaunchAsync(TimeSpan? timeout = null)
    {
        if (!backend.IsInstalled)
            return false;
        using var attempt = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        attempt.CancelAfter(timeout ?? TimeSpan.FromSeconds(15));
        bool entered = false;
        try
        {
            await gate.WaitAsync(attempt.Token).ConfigureAwait(false);
            entered = true;
            SetStatus(UpdatePhase.Checking, "Verificando atualizações antes de abrir…");
            var version = await backend.StageLatestAsync(attempt.Token).ConfigureAwait(false);
            if (version == null)
            {
                SetStatus(UpdatePhase.Current, "Você está na versão mais recente.");
                return false;
            }
            SetStatus(UpdatePhase.Ready, "Versão " + version + " pronta para instalar automaticamente.");
            return backend.RestartToApply();
        }
        catch (OperationCanceledException) when (attempt.IsCancellationRequested)
        {
            if (!lifetime.IsCancellationRequested)
                SetStatus(UpdatePhase.Unavailable, "Abrindo o Gole. A atualização continuará automaticamente.");
            return false;
        }
        catch (Exception)
        {
            SetStatus(UpdatePhase.Unavailable, "Abrindo o Gole. Vou tentar atualizar novamente automaticamente.");
            return false;
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

    internal async Task RunAsync(bool checkImmediately = true)
    {
        if (!backend.IsInstalled)
            return;
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                if (checkImmediately)
                    await CheckAsync().ConfigureAwait(false);
                checkImmediately = true;
                var interval = Phase == UpdatePhase.Unavailable ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(15);
                await (delay == null ? Task.Delay(interval, lifetime.Token) : delay(interval, lifetime.Token)).ConfigureAwait(false);
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
