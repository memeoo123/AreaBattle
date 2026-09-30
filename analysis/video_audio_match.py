"""Normalized correlation of supplied recording with recovered original sound clips."""
import subprocess,json,wave
from pathlib import Path
import numpy as np
W=Path(__file__).resolve().parent.parent;G=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928'
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
with wave.open(str(G/'original-audio-12k.wav')) as r:
    rate=r.getframerate();audio=np.frombuffer(r.readframes(r.getnframes()),'<i2').astype(float)/32768
results=[]
for sound,start,end in [(2016,34,39),(2018,48,54),(3121,51,58),(3122,52,61),(2021,52,61),(4401,61,64),(2009,84,90),(2001,0,2)]:
    path=W/f'UnityProject/Assets/AreaBattle/Resources/Recovered/Audio/{sound}.wav'
    if not path.exists():continue
    p=subprocess.run([str(ff),'-hide_banner','-loglevel','error','-i',str(path),'-ac','1','-ar',str(rate),'-f','f32le','pipe:1'],stdout=subprocess.PIPE,check=True)
    clip=np.frombuffer(p.stdout,'<f4').astype(float)
    significant=np.flatnonzero(np.abs(clip)>.002)
    if not len(significant):continue
    lead=int(significant[0]);clip=clip[lead:min(int(significant[-1])+1,lead+rate*3)]
    clip-=clip.mean();segment=audio[int(start*rate):int(end*rate)]
    length=len(segment)+len(clip)-1;n=1<<(length-1).bit_length()
    convolution=np.fft.irfft(np.fft.rfft(segment,n)*np.fft.rfft(clip[::-1],n),n)[:length]
    corr=convolution[len(clip)-1:len(segment)]
    squared=np.r_[0,np.cumsum(segment*segment)];sums=np.r_[0,np.cumsum(segment)]
    energy=squared[len(clip):]-squared[:-len(clip)]-(sums[len(clip):]-sums[:-len(clip)])**2/len(clip)
    norm=corr/np.sqrt(np.maximum(energy,1e-12)*np.sum(clip*clip))
    peaks=[]
    for _ in range(5):
        at=int(np.argmax(norm));score=float(norm[at]);peaks.append(dict(startSeconds=start+(at-lead)/rate,correlation=round(score,5)))
        norm[max(0,at-rate//3):min(len(norm),at+rate//3)]=-1
    results.append(dict(soundId=sound,window=[start,end],referenceClip=str(path.relative_to(W)).replace('\\','/'),trimmedLeadingSamples=lead,templateSamples=len(clip),peaks=peaks))
out=dict(method='12kHz mono normalized waveform correlation; compressed recording/mixed background lower scores; low scores are not confirmed matches.',results=results)
(G/'audio-matches.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
print(json.dumps(out,indent=2))
