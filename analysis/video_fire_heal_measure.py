"""Measure tower2 scores during fire, without guessing original projectile sequence."""
import json,re,subprocess
from pathlib import Path
import numpy as np
from PIL import Image
W=Path(__file__).resolve().parent.parent;V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928'
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
x,y,w,h=558,398,52,36
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-an','-vf',f"select='between(t,52,59)',crop={w}:{h}:{x}:{y},format=gray,showinfo",'-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))]
frames=np.frombuffer(p.stdout,np.uint8).reshape(-1,h,w).astype(float)
templates=[np.asarray(Image.open(V/'half-second'/f).convert('L'))[y:y+h,x:x+w].astype(float) for f in ['104.png','115.png','116.png']]
# Native frame57.07979 was separately enlarged and manually read as34.
templates.insert(1,frames[min(range(len(pts)),key=lambda i:abs(pts[i]-57.07979))])
errors=np.stack([((frames-t)**2).mean((1,2)) for t in templates],axis=1);values=[32,34,36,38]
labels=[values[int(e.argmin())] if e.min()<100 else None for e in errors]
changes=[]
for i in range(1,len(pts)-3):
 if labels[i]!=labels[i-1] and all(v==labels[i] for v in labels[i:i+3]):changes.append(dict(previousPts=pts[i-1],pts=pts[i],before=labels[i-1],after=labels[i],mse=float(errors[i].min())))
out=dict(roi=[x,y,w,h],method='Manually read score templates32/34/36/38;34 uses native57.07979 reviewed separately. Native-frame MSE<100 and3-frame persistence. Null means unclassified, not zero; later healing particles can obscure digits.',changes=changes)
(V/'fire-blue2-score-pixels.json').write_text(json.dumps(out,indent=2)+'\n')
print(json.dumps(out,indent=2))
for i in range(len(pts)-3):
 if labels[i] is None and 57<pts[i]<58 and all(v is None for v in labels[i:i+3]):
  Image.fromarray(frames[i].astype(np.uint8)).resize((260,180)).save(V/'fire-blue2-unclassified-digit.png');print('unclassified sample',pts[i]);break
