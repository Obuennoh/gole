using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Gole.Desktop.Audio;

internal static class WaveVolume
{
    internal static byte[] Apply(byte[] wave, int volume)
    {
        if (wave.Length < 44 || Encoding.ASCII.GetString(wave, 0, 4) != "RIFF" || Encoding.ASCII.GetString(wave, 8, 4) != "WAVE")
            throw new InvalidDataException("Invalid wave resource.");
        var output = (byte[])wave.Clone();
        bool pcm16 = false;
        for (int offset = 12; offset <= wave.Length - 8;)
        {
            string type = Encoding.ASCII.GetString(wave, offset, 4);
            int size = BinaryPrimitives.ReadInt32LittleEndian(wave.AsSpan(offset + 4, 4));
            int start = offset + 8;
            if (size < 0 || size > wave.Length - start)
                throw new InvalidDataException("Invalid wave chunk.");
            if (type == "fmt " && size >= 16)
                pcm16 = BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(start, 2)) == 1
                    && BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(start + 14, 2)) == 16;
            if (type == "data")
            {
                if (!pcm16 || size % 2 != 0)
                    throw new InvalidDataException("Expected 16-bit PCM audio.");
                double gain = Math.Clamp(volume, 0, 100) / 100.0;
                for (int i = start; i < start + size; i += 2)
                {
                    short sample = BinaryPrimitives.ReadInt16LittleEndian(wave.AsSpan(i, 2));
                    BinaryPrimitives.WriteInt16LittleEndian(output.AsSpan(i, 2), (short)Math.Round(sample * gain));
                }
                return output;
            }
            offset = checked(start + size + (size & 1));
        }
        throw new InvalidDataException("Missing wave audio data.");
    }
}
