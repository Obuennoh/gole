using System;
using System.Diagnostics;
using System.IO;
using System.Media;

namespace Gole.Desktop.Audio;

internal interface IWaterSound : IDisposable
{
    void Play();
}

internal sealed class WaterSound : IWaterSound
{
    internal const string ResourceName = "Gole.Desktop.Audio.water-drops.wav";
    private readonly Stream stream;
    private readonly SoundPlayer player;
    private long? lastPlay;

    internal WaterSound()
    {
        try
        {
            stream = typeof(WaterSound).Assembly.GetManifestResourceStream(ResourceName);
            if (stream != null)
            {
                player = new SoundPlayer(stream);
                player.Load();
            }
        }
        catch (Exception) { player?.Dispose(); player = null; }
    }

    public void Play()
    {
        if (player == null || (lastPlay.HasValue && Stopwatch.GetElapsedTime(lastPlay.Value).TotalSeconds < 1))
            return;
        try
        {
            lastPlay = Stopwatch.GetTimestamp();
            player.Play();
        }
        catch (Exception) { /* Missing audio devices must not interrupt reminders. */ }
    }

    public void Dispose()
    {
        player?.Stop();
        player?.Dispose();
        stream?.Dispose();
    }
}
