"""Decode supplied originals, remove codec padding, make a periodic PCM title loop."""
from pathlib import Path
import numpy as np, subprocess, wave, json, hashlib
root=Path(__file__).resolve().parent.parent
ffmpeg=root/'Temp/audio-tools/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
rate=48000
def decode(name):
    p=root/'Assets/TitleSource'/name
    raw=subprocess.check_output([str(ffmpeg),'-v','error','-i',str(p),'-vn','-ac','2','-ar',str(rate),'-f','f32le','-'])
    return np.frombuffer(raw,dtype='<f4').reshape(-1,2).copy()
def save(name,x):
    with wave.open(str(root/'Assets/Resources/Title'/name),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes((np.clip(x,-1,1)*32767).astype('<i2').tobytes())
voice=decode('Woodstock Rush Spoken.mp3');theme=decode('Woodstock Rush.mp3')
# FFmpeg honors MP3 skip-samples / encoder delay. Retain speech pauses and full phrase.
save('Voice.wav',voice)
# Preserve the supplied tune; overlap the tail and head over a short 240ms musical
# transition. Circular rotation puts the splice away from startup. No codec gap,
# silence insertion, resynthesis, TTS, or modification of the original MP3 files.
n=int(.24*rate);t=np.linspace(0,1,n)[:,None]
splice=theme[-n:]*(1-t)+theme[:n]*t
loop=np.concatenate((theme[n:-n],splice))
save('ThemeLoop.wav',loop)
report=dict(sampleRate=rate,voiceSeconds=len(voice)/rate,sourceThemeSeconds=len(theme)/rate,loopSeconds=len(loop)/rate,
    overlapSeconds=n/rate,boundaryMaxStep=float(np.max(np.abs(loop[0]-loop[-1]))),
    sourceHashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (root/'Assets/TitleSource').glob('*.mp3')},
    method='MP3 encoder delay honored by FFmpeg; PCM full tune with 240ms circular linear tail/head overlap; subjective musical/listening review remains open')
(root/'Docs/CR097-100/title-audio-mastering.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
