"""Reproducible excerpt mastering of the CC0 sources documented in Assets/Audio/Wildlife/LICENSE.txt.
Inputs: Temp/{bird,squirrel,frog,bat,wings}-original.mp3; no network or save access.
"""
from pathlib import Path
import subprocess, numpy as np, wave, json
root=Path(__file__).resolve().parent.parent
ffmpeg=root/'Temp/ffmpeg-tools/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
out=root/'Assets/Audio/Wildlife';out.mkdir(parents=True,exist_ok=True)
rate=44100; report=[]
def decode(name):
    raw=subprocess.check_output([str(ffmpeg),'-v','error','-i',str(root/f'Temp/{name}-original.mp3'),'-ac','1','-ar',str(rate),'-af','highpass=f=180,lowpass=f=8500','-f','f32le','-'])
    return np.frombuffer(raw,dtype='<f4').copy()
def master(x):
    x=x-x.mean(); rms=np.sqrt(np.mean(x*x))
    # Soft compression retains the recorded call envelope while controlling isolated
    # field-recording transients that otherwise make peak normalization too quiet.
    x=.85*np.tanh(x*(.22/max(rms,1e-6)))
    n=min(2205,len(x)//4);x[:n]*=np.linspace(0,1,n);x[-n:]*=np.linspace(1,0,n)
    return x
def excerpt(x,seconds,used):
    n=int(seconds*rate); candidates=range(0,max(1,len(x)-n),rate//4)
    best=max((i for i in candidates if all(abs(i-j)>n+rate for j in used)),key=lambda i:float(np.mean(x[i:i+n]**2)))
    used.append(best);return master(x[best:best+n].copy()),best/rate
def save(name,x,start):
    with wave.open(str(out/f'{name}.wav'),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes((np.clip(x,-1,1)*32767).astype('<i2').tobytes())
    report.append(dict(clip=name,start=start,seconds=len(x)/rate,peak=float(np.max(np.abs(x))),rms=float(np.sqrt(np.mean(x*x)))))
for name in ['bird','squirrel','frog']:
    x=decode(name);used=[]
    for i in range(2):
        y,start=excerpt(x,2.3,used);save(f'{name}-{i+1}',y,start)
chirp,bc=excerpt(decode('bat'),2.7,[]);flutter,wc=excerpt(decode('wings'),2.7,[])
save('bat-flight',master(chirp*.75+flutter*1.1),dict(bat=bc,wings=wc))
(root/'Docs/CR061-062/audio-mastering.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
