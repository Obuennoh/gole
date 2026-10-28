using System.IO;
using System.Text;
using System.Buffers.Binary;
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
        Assert.InRange(size / 88200.0, .1, 2);
        Assert.Contains(reader.ReadBytes((int)size), sample => sample != 0);
    }

    [Fact]
    public void Volume_changes_only_sample_amplitude_without_mutating_the_source()
    {
        using var resource = typeof(WaterSound).Assembly.GetManifestResourceStream(WaterSound.ResourceName(3));
        Assert.NotNull(resource);
        using var buffer = new MemoryStream();
        resource.CopyTo(buffer);
        byte[] original = buffer.ToArray();
        byte[] snapshot = (byte[])original.Clone();
        byte[] muted = WaveVolume.Apply(original, 0);
        byte[] half = WaveVolume.Apply(original, 50);
        Assert.Equal(original, WaveVolume.Apply(original, 100));
        Assert.Equal(snapshot, original);
        Assert.Equal(original[..44], muted[..44]);
        for (int i = 44; i < original.Length; i += 2)
        {
            Assert.Equal(0, BinaryPrimitives.ReadInt16LittleEndian(muted.AsSpan(i, 2)));
            short source = BinaryPrimitives.ReadInt16LittleEndian(original.AsSpan(i, 2));
            Assert.Equal((short)Math.Round(source * .5), BinaryPrimitives.ReadInt16LittleEndian(half.AsSpan(i, 2)));
        }
    }
}
