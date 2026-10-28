"""Prepare the licensed recordings listed in docs/audio-credits.md.

Development only: numpy and soundfile==0.13.1. The installed app uses embedded WAVs.
"""
import argparse
import hashlib
import json
from pathlib import Path
import wave

import numpy as np
import soundfile as sf

parser = argparse.ArgumentParser()
parser.add_argument('source_dir', type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
clips = [
    ('loop_water_01.ogg', 'water-drops', 1, 0.9, 0.025, 0.08),
    ('loop_bubbles_1.ogg', 'bubbles', 0.35, 1.25, 0.025, 0.09),
    ('loop_water_02.ogg', 'water-stream', 2, 1.5, 0.06, 0.15),
    ('bell_01.ogg', 'soft-chime', 0, 1.3, 0.003, 0.08),
]
for source, name, start, length, attack, release in clips:
    path = args.source_dir / source
    signal, original_rate = sf.read(path, always_2d=True)
    signal = signal.mean(axis=1)
    signal = signal[int(start * original_rate):int((start + length) * original_rate)]
    signal -= signal.mean()
    if original_rate != 44100:
        taps = np.arange(-31, 32, dtype=float)
        cutoff = min(0.5, 44100 / original_rate * 0.46)
        kernel = 2 * cutoff * np.sinc(2 * cutoff * taps) * np.hamming(len(taps))
        kernel /= kernel.sum()
        signal = np.convolve(signal, kernel, mode='same')
        signal = np.interp(np.arange(int(len(signal) * 44100 / original_rate)) * original_rate / 44100, np.arange(len(signal)), signal)
    rms = float(np.sqrt(np.mean(signal ** 2)))
    peak = float(np.max(np.abs(signal)))
    signal *= min(0.07 / max(rms, 1e-8), 0.75 / max(peak, 1e-8), 6)
    envelope = np.minimum(np.arange(len(signal)) / (attack * 44100), 1)
    envelope *= np.minimum(np.arange(len(signal) - 1, -1, -1) / (release * 44100), 1)
    signal *= envelope
    output = root / 'src/Gole.Desktop/Audio' / (name + '.wav')
    pcm = np.round(np.clip(signal, -0.99, 0.99) * 32767).astype('<i2')
    with wave.open(str(output), 'wb') as audio:
        audio.setparams((1, 2, 44100, 0, 'NONE', 'not compressed'))
        audio.writeframes(pcm.tobytes())
    print(json.dumps({'file': output.name, 'source_sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'duration': len(signal) / 44100, 'peak': float(np.max(np.abs(signal))), 'rms': float(np.sqrt(np.mean(signal ** 2)))}))
