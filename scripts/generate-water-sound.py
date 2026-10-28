"""Rebuild the original notification sounds using only the standard library."""
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


def write_sound(name, values):
    target = output.with_name(name + '.wav')
    with wave.open(str(target), 'wb') as audio:
        audio.setparams((1, 2, rate, 0, 'NONE', 'not compressed'))
        audio.writeframes(b''.join(struct.pack('<h', int(max(-0.8, min(0.8, value)) * 32767)) for value in values))
    print(f'Created {target.name}: {len(values) / rate:.2f}s, mono PCM, {rate} Hz.')


def drops(duration, notes):
    values = [0.0] * int(rate * duration)
    for start, frequency, gain in notes:
        phase = 0.0
        for index in range(int(rate * 0.35)):
            position = int(start * rate) + index
            if position >= len(values):
                break
            time = index / rate
            phase += 2 * math.pi * frequency * (1 + 0.9 * (1 - math.exp(-time * 35))) / rate
            envelope = (1 - math.exp(-time * 650)) * math.exp(-time * 21)
            values[position] += gain * envelope * (math.sin(phase) + 0.12 * math.sin(phase * 2.4))
    return values


write_sound('single-drop', drops(0.55, [(0.01, 850, 0.40)]))
write_sound('bubbles', drops(0.95, [(0.02, 240, 0.38), (0.15, 310, 0.35), (0.32, 270, 0.28), (0.48, 420, 0.26), (0.60, 350, 0.22)]))

stream = drops(1.25, [(0.04, 1200, 0.10), (0.25, 900, 0.09), (0.54, 1500, 0.08), (0.79, 1150, 0.08)])
randomizer = random.Random(128)
filtered = 0.0
for index in range(len(stream)):
    time = index / rate
    filtered = 0.78 * filtered + 0.22 * randomizer.uniform(-1, 1)
    envelope = min(1, time / 0.12) * max(0, min(1, (1.2 - time) / 0.25))
    stream[index] += filtered * 0.38 * envelope * (0.8 + 0.2 * math.sin(time * 28))
write_sound('water-stream', stream)

chime = [0.0] * int(rate * 1.0)
for start, frequency in [(0.01, 660), (0.18, 880)]:
    for index in range(int(rate * 0.78)):
        position = int(start * rate) + index
        if position >= len(chime):
            break
        time = index / rate
        envelope = (1 - math.exp(-time * 450)) * math.exp(-time * 8)
        chime[position] += 0.24 * envelope * (math.sin(2 * math.pi * frequency * time) + 0.13 * math.sin(2 * math.pi * frequency * 2.01 * time))
write_sound('soft-chime', chime)
