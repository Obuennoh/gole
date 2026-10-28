using System.IO;
using System.Text;
using Gole.Desktop.Audio;

namespace Gole.Desktop.Tests;

public sealed class WaterSoundTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Embedded_audio_is_short_valid_pcm_without_playing_it(int soundId)
    {
        using var stream = typeof(WaterSound).Assembly.GetManifestResourceStream(WaterSound.ResourceName(soundId));
        Assert.NotNull(stream);
        using var reader = new BinaryReader(stream);
        Assert.Equal("RIFF", Encoding.ASCII.GetString(reader.ReadBytes(4)));
        Assert.Equal(stream.Length - 8, reader.ReadUInt32());
        Assert.Equal("WAVEfmt ", Encoding.ASCII.GetString(reader.ReadBytes(8)));
        Assert.Equal(16u, reader.ReadUInt32());
        Assert.Equal(1, reader.ReadUInt16()); // PCM
        Assert.Equal(1, reader.ReadUInt16()); // mono
        Assert.Equal(44100u, reader.ReadUInt32());
        reader.ReadUInt32();
        Assert.Equal(2, reader.ReadUInt16());
        Assert.Equal(16, reader.ReadUInt16());
        Assert.Equal("data", Encoding.ASCII.GetString(reader.ReadBytes(4)));
        uint size = reader.ReadUInt32();
        Assert.Equal(stream.Length - stream.Position, size);
        Assert.InRange(size / 88200.0, .1, 1.5);
        Assert.Contains(reader.ReadBytes((int)size), sample => sample != 0);
    }
}
