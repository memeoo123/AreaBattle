"""Read video pixels/audio into bounded measurements; preserve the supplied recording."""
import json,re,subprocess,wave
from pathlib import Path
import numpy as np
from PIL import Image
W=Path(__file__).resolve().parent.parent;R=W/'analysis/targets/wxcf1394487200e48f/43';G=R/'generated/video-20260928'
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
video=R/'evidence/original-reference/video-20260928/original.mp4'
roi=(602,1108,42,32)
x,y,w,h=roi
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-an','-vf',f'crop={w}:{h}:{x}:{y},format=gray,showinfo','-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode('utf-8',errors='replace'))]
frames=np.frombuffer(p.stdout,np.uint8).reshape(-1,h,w);assert len(pts)==len(frames),(len(pts),len(frames))
templates=[]
for count,file in [(27,'071.png'),(26,'073.png'),(25,'107.png'),(24,'130.png')]:
    im=Image.open(G/'half-second'/file).convert('L');templates.append(np.asarray(im)[y:y+h,x:x+w].astype(float))
errs=np.stack([((frames.astype(float)-t)**2).mean(axis=(1,2)) for t in templates],axis=1)
labels=np.asarray([27,26,25,24])[errs.argmin(axis=1)]
events=[]
last=None
for i,(t,label) in enumerate(zip(pts,labels)):
    value=int(label) if errs[i].min()<100 else None
    if 2<t<88 and (last is None or value!=last):
        if value is not None or last is not None:
            events.append(dict(frame=i,pts=t,stock=value,mse=float(errs[i].min()),validStock=value is not None))
    if 2<t<88:last=value
result=dict(frameCount=len(pts),timeBase='original decoded video PTS',roi=roi,stockTransitions=events)
(G/'frame-pts.json').write_text(json.dumps(pts),encoding='utf-8')
(G/'runtime-measurements.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(json.dumps(result,indent=2))
