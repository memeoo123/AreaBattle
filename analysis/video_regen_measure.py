"""Bound pre-recording regeneration phase from original pixels, without score fitting."""
import json,re,subprocess
from pathlib import Path
import numpy as np
from PIL import Image
W=Path(__file__).resolve().parent.parent
V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928'
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
x,y,w,h=110,420,60,44
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-t','3.8','-an','-vf',f'crop={w}:{h}:{x}:{y},format=gray,showinfo','-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))]
frames=np.frombuffer(p.stdout,np.uint8).reshape(-1,h,w).astype(float)
templates=[np.asarray(Image.open(V/'half-second'/file).convert('L'))[y:y+h,x:x+w].astype(float) for file in ['003.png','004.png','008.png']]
errs=np.stack([((frames-t)**2).mean(axis=(1,2)) for t in templates],axis=1)
labels=np.asarray([19,20,21])[errs.argmin(axis=1)]
changes=[]
for i in range(1,len(pts)-3):
 if pts[i]>1 and labels[i]!=labels[i-1] and all(labels[i:i+3]==labels[i]) and errs[i].min()<100:
  changes.append(dict(previousPts=pts[i-1],pts=pts[i],before=int(labels[i-1]),after=int(labels[i]),mse=float(errs[i].min())))
assert len(changes)==2 and changes[0]['after']==20 and changes[1]['after']==21,changes
first=changes[0];resume=.806
out=dict(roi=[x,y,w,h],method='Native-frame grayscale nearest templates for manually read blue1 scores19/20/21; accepted MSE<100, three-frame persistence.',changes=changes,
 inferredPreRecordingSeconds=[10-(first['pts']-resume),10-(first['previousPts']-resume)],
 caveats=['Conditional on initial15, two-second regeneration, no earlier blue1 input, and pause resume near0.806.','Rendered score transition is not internal simulation timestamp. Prior AI/RNG/soldier states remain unknown.'])
(V/'regen-phase-evidence.json').write_text(json.dumps(out,indent=2)+'\n')
print(json.dumps(out,indent=2))
