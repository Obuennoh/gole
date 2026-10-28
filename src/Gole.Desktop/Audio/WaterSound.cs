using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using Gole.Core;

namespace Gole.Desktop.Audio;

internal interface IWaterSound : IDisposable
{
    void Play(int soundId = SoundCatalog.DefaultId, bool preview = false);
}

internal sealed class WaterSound : IWaterSound
{
    private static readonly string[] Files = { "water-drops", "bubbles", "water-stream", "single-drop", "soft-chime" };
    internal static string ResourceName(int id) => "Gole.Desktop.Audio." + Files[SoundCatalog.Normalize(id)] + ".wav";
    private readonly Dictionary<int, (Stream Stream, SoundPlayer Player)> loaded = new();
    private SoundPlayer active;
    private long? lastPlay;

    public void Play(int soundId = SoundCatalog.DefaultId, bool preview = false)
    {
        if (!preview && lastPlay.HasValue && Stopwatch.GetElapsedTime(lastPlay.Value).TotalSeconds < 1.3)
            return;
        soundId = SoundCatalog.Normalize(soundId);
        try
        {
            if (!loaded.TryGetValue(soundId, out var audio))
            {
                var stream = typeof(WaterSound).Assembly.GetManifestResourceStream(ResourceName(soundId));
                if (stream == null)
                    return;
                var player = new SoundPlayer(stream);
                try
                {
                    player.Load();
                }
                catch { player.Dispose(); stream.Dispose(); throw; }
                audio = (stream, player);
                loaded.Add(soundId, audio);
            }
            active?.Stop();
            active = audio.Player;
            lastPlay = Stopwatch.GetTimestamp();
            active.Play();
        }
        catch (Exception) { /* Audio device failures must not interrupt reminders. */ }
    }

    public void Dispose()
    {
        active?.Stop();
        foreach (var audio in loaded.Values)
        {
            audio.Player.Dispose();
            audio.Stream.Dispose();
        }
        loaded.Clear();
    }
}
