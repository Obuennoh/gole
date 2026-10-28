"""Rebuild the original, short water-drop notification using only the standard library."""
import math
from pathlib import Path
import random
import struct
import wave

rate = 44100
randomizer = random.Random(74)
samples = [0.0] * int(rate * 0.85)
for start, frequency, gain in [(0.02, 730, 0.38), (0.19, 960, 0.30), (0.39, 820, 0.24)]:
    phase = 0.0
    previous_noise = 0.0
    for index in range(int(rate * 0.40)):
        position = int(start * rate) + index
        if position >= len(samples):
            break
        time = index / rate
        phase += 2 * math.pi * frequency * (1 + 1.2 * (1 - math.exp(-time * 28))) / rate
        envelope = (1 - math.exp(-time * 650)) * math.exp(-time * 19)
        noise = randomizer.uniform(-1, 1)
        splash = (noise - previous_noise) * math.exp(-time * 105) * 0.12
        previous_noise = noise
        tone = math.sin(phase) + 0.18 * math.sin(phase * 2.73) * math.exp(-time * 35)
        samples[position] += gain * (tone * envelope + splash)

output = Path(__file__).resolve().parents[1] / 'src/Gole.Desktop/Audio/water-drops.wav'
output.parent.mkdir(parents=True, exist_ok=True)
with wave.open(str(output), 'wb') as audio:
    audio.setparams((1, 2, rate, 0, 'NONE', 'not compressed'))
    audio.writeframes(b''.join(struct.pack('<h', int(max(-0.8, min(0.8, sample)) * 32767)) for sample in samples))
print(f'Created {output.name}: {len(samples) / rate:.2f}s, mono PCM, {rate} Hz.')
