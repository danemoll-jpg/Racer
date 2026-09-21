"""Compare the supplied imported PCM with packaged PCM and actual listener DSP output.
This is signal evidence, not a human or OS-endpoint listening claim.
"""
from pathlib import Path
import json, wave
import numpy as np

root = Path(__file__).resolve().parents[1]
with wave.open(str(root / 'Assets/Resources/Title/Voice.wav'), 'rb') as wav:
    rate, channels = wav.getframerate(), wav.getnchannels()
    reference = np.frombuffer(wav.readframes(wav.getnframes()), '<i2').astype(float).reshape(-1, channels) / 32768
reports = []
for mode in ('untouched', 'advance'):
    folder = root / 'Docs/CR112-118/final' / ('title-' + mode)
    packaged = np.fromfile(folder / 'packaged-voice-float32.raw', '<f4').reshape(-1, channels)
    with wave.open(str(folder / 'final-player-output.wav'), 'rb') as wav:
        output_rate, output_channels, width = wav.getframerate(), wav.getnchannels(), wav.getsampwidth()
        assert width == 2 and output_rate == rate, (width, output_rate, rate)
        output = np.frombuffer(wav.readframes(wav.getnframes()), '<i2').astype(float).reshape(-1, output_channels) / 32768
    r, x = reference.mean(axis=1), output.mean(axis=1)
    n = 1 << (len(r) + len(x) - 1).bit_length()
    correlation = np.fft.irfft(np.fft.rfft(x, n) * np.conj(np.fft.rfft(r, n)), n)
    lag = int(np.argmax(np.abs(correlation[:len(x) - len(r) + 1])))
    observed = x[lag:lag + len(r)]
    gain = float(observed @ r / (r @ r))
    tail = int(.35 * rate)
    report = dict(mode=mode, rate=rate, packaged_samples=len(packaged), reference_samples=len(reference),
                  packaged_max_abs_difference=float(np.max(np.abs(packaged - reference))),
                  output_seconds=len(x) / rate, voice_start_seconds=lag / rate,
                  complete_voice_end_seconds=(lag + len(r)) / rate, fitted_gain=gain,
                  full_correlation=float(np.corrcoef(r, observed)[0, 1]),
                  final_350ms_correlation=float(np.corrcoef(r[-tail:], observed[-tail:])[0, 1]),
                  final_350ms_reference_peak=float(np.max(np.abs(r[-tail:]))),
                  final_350ms_output_peak=float(np.max(np.abs(observed[-tail:]))),
                  limitation='Listener DSP capture only; audio-input audition unavailable; no human or OS-endpoint listening claim.')
    report['complete_tail_signal_pass'] = report['full_correlation'] > .98 and report['final_350ms_correlation'] > .98 and gain > .01
    reports.append(report)
destination = root / 'Docs/CR112-118/title-final-output-comparison.json'
destination.write_text(json.dumps(reports, indent=2))
print(json.dumps(reports, indent=2))
