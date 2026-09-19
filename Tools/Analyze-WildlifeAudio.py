"""Measure known recorded-call presence in actual listener mixes (not subjective listening).
Uses matched waveform correlation with the logged playback pitch. Output includes the
estimated matched component relative to residual engine/music. No OS endpoint capture.
"""
from pathlib import Path
import wave, json, re, sys
import numpy as np
root=Path(__file__).resolve().parent.parent
folder=root/(sys.argv[1] if len(sys.argv)>1 else 'Docs/CR061-062/final-audio')
def read(path):
    with wave.open(str(path),'rb') as w:
        x=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(float)/32768
        return x.reshape(-1,w.getnchannels()).mean(axis=1),w.getframerate()
log=(folder/'audio.txt').read_text();rows=[]
for species in ['Bird','Squirrel','Frog','Bats']:
    if species=='Bats':clip='bat-flight';pitch=1
    else:
        match=re.search(species+r' clip=(\S+) pitch=([\d.]+)',log)
        if not match:continue
        clip,pitch=match[1],float(match[2])
    mix,rate=read(folder/f'{species}-mix.wav');source,sr=read(root/f'Assets/Audio/Wildlife/{clip}.wav')
    # Use the central recording so a DSP-block start offset cannot remove its onset.
    source=source[int(.25*sr):int(1.9*sr)]
    ref=np.interp(np.arange(int(len(source)*rate/(sr*pitch)))*sr*pitch/rate,np.arange(len(source)),source)
    ref-=ref.mean();mix-=mix.mean();n=len(ref);size=1<<((len(mix)+n-2).bit_length())
    corr=np.fft.irfft(np.fft.rfft(mix,size)*np.fft.rfft(ref[::-1],size),size)[n-1:len(mix)]
    energy=np.cumsum(np.r_[0,mix*mix]);windows=energy[n:]-energy[:-n];ref_energy=np.sum(ref*ref)
    normalized=corr/np.sqrt(np.maximum(windows*ref_energy,1e-20));i=int(np.argmax(np.abs(normalized)))
    gain=corr[i]/ref_energy;matched=ref*gain;segment=mix[i:i+n];residual=segment-matched
    mr=float(np.sqrt(np.mean(matched*matched)));rr=float(np.sqrt(np.mean(residual*residual)))
    rows.append(dict(species=species,clip=clip,pitch=pitch,correlation=float(normalized[i]),matchedRms=mr,residualRms=rr,matchedToResidualDb=float(20*np.log10(max(mr,1e-12)/max(rr,1e-12))),peak=float(np.max(np.abs(mix))),matchAtSeconds=i/rate,interpretation='Waveform evidence at listener before OS endpoint; not human audibility or listening acceptance'))
(folder/'waveform-presence.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows,indent=2))
