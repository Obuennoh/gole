using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Gole.Core;

namespace Gole.Infrastructure;

public sealed record StateLoadResult(HydrationState State, bool Recovered, bool Reset);

public sealed class JsonStateStore(string directory)
{
    private readonly string directory = Path.GetFullPath(directory);
    private bool preserveBackup;
    public string FilePath => Path.Combine(directory, "preferences.json");
    public string BackupPath => FilePath + ".bak";
    public bool SaveFailed
    {
        get; private set;
    }
    public bool Recovered
    {
        get; private set;
    }
    public string? LastError
    {
        get; private set;
    }

    private static DataContractJsonSerializer Serializer() => new(typeof(HydrationState),
        new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 1_000_000 });

    public StateLoadResult Load(DateTime utcNow)
    {
        var damaged = false;
        foreach (var path in new[] { FilePath, BackupPath })
        {
            if (!File.Exists(path))
                continue;
            try
            {
                using var stream = File.OpenRead(path);
                if (stream.Length > 4 * 1024 * 1024)
                    throw new SerializationException("State file exceeds the supported size.");
                var state = Serializer().ReadObject(stream) as HydrationState ?? throw new SerializationException("Empty state.");
                if (state.Version > HydrationState.CurrentVersion)
                    throw new NotSupportedException("Os dados foram salvos por uma versão mais recente do Gole.");
                state.Validate(utcNow);
                Recovered = path == BackupPath;
                preserveBackup = damaged;
                return new(state, Recovered, false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SerializationException or ArgumentException)
            {
                damaged = true;
                LastError = ex.Message;
            }
        }
        Recovered = damaged;
        preserveBackup = damaged;
        return new(HydrationState.Fresh(utcNow), false, damaged);
    }

    public bool Save(HydrationState state)
    {
        string temp = FilePath + ".tmp";
        try
        {
            Directory.CreateDirectory(directory);
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Serializer().WriteObject(stream, state);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(FilePath) && !preserveBackup)
                File.Replace(temp, FilePath, BackupPath);
            else
                File.Move(temp, FilePath, overwrite: true);
            preserveBackup = false;
            SaveFailed = false;
            LastError = null;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SerializationException)
        {
            SaveFailed = true;
            LastError = ex.Message;
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { LastError ??= ex.Message; }
        }
    }

    public void Log(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, "error.log"), $"{DateTime.UtcNow:O} {exception}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { LastError = ex.Message; }
    }
}
