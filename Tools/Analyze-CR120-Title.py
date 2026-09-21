"""Bounded waveform evidence for full voice and ORIGINAL theme onset; not listening."""
from pathlib import Path
import json, wave
import numpy as np
root=Path(__file__).resolve().parents[1]
evidence=root/'Docs/CR120-121'
def read(p):
    with wave.open(str(p),'rb') as w:
        assert w.getsampwidth()==2 and w.getframerate()==48000
        return np.frombuffer(w.readframes(w.getnframes()),'<i2').astype(float).reshape(-1,w.getnchannels())/32768
voice=read(root/'Assets/Resources/Title/Voice.wav')
theme=read(root/'Temp/cr118-theme-original.wav')
opening=read(root/'Assets/Resources/Title/ThemeOpening.wav')
loop=read(root/'Assets/Resources/Title/ThemeLoop.wav')
def align(output,ref):
    r=ref.mean(axis=1);x=output.mean(axis=1)
    n=1<<(len(x)+len(r)-1).bit_length()
    c=np.fft.irfft(np.fft.rfft(x,n)*np.conj(np.fft.rfft(r,n)),n)
    lag=int(np.argmax(np.abs(c[:len(x)-len(r)+1])))
    y=x[lag:lag+len(r)]
    return dict(start_seconds=lag/48000,full_correlation=float(np.corrcoef(r,y)[0,1]),
                first_200ms_correlation=float(np.corrcoef(r[:9600],y[:9600])[0,1]),
                last_350ms_correlation=float(np.corrcoef(r[-16800:],y[-16800:])[0,1]),
                fitted_gain=float(y@r/(r@r)))
report={'opening_exact_original':bool(np.array_equal(opening,theme[:11520])),
        'existing_loop_starts_at_original_seconds':.24,
        'loop_body_offset_max_difference':float(np.max(np.abs(loop[:-11520]-theme[11520:len(loop)]))),
        'limitation':'Listener DSP only; no OS-endpoint capture or human audition available. Dan audio acceptance remains open.'}
for mode in ('untouched','advance'):
    output=read(evidence/('title-'+mode)/'final-player-output.wav')
    item={'voice':align(output,voice)}
    if mode=='untouched': item['original_theme_first_second']=align(output,theme[:48000])
    report[mode]=item
(evidence/'audio-signal-comparison.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
