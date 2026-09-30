"""Measure the filled red capacity dot disappearing in the original video."""
import subprocess,re,json
from pathlib import Path
import numpy as np
W=Path(__file__).resolve().parent.parent;V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928'
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
x,y,w,h=578,804,22,22
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-an','-vf',f"select='between(t,60,66)',crop={w}:{h}:{x}:{y},format=rgb24,showinfo",'-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))]
a=np.frombuffer(p.stdout,np.uint8).reshape(-1,h,w,3);counts=((a[:,:,:,0]>180)&(a[:,:,:,1]<130)&(a[:,:,:,2]<140)).sum((1,2));filled=counts>35
changes=[]
for i in range(1,len(pts)-3):
 if filled[i]!=filled[i-1] and all(filled[i:i+3]==filled[i]):changes.append(dict(previousPts=pts[i-1],pts=pts[i],before=bool(filled[i-1]),after=bool(filled[i]),beforePixels=int(counts[i-1]),pixels=int(counts[i])))
out=dict(roi=[x,y,w,h],method='Red capacity-dot RGB threshold, minimum36pixels, three-frame persistence; original contact sheet independently confirms the6-to7line disappears while tower7 remains green.',changes=changes)
(V/'tower6-outgoing-pixels.json').write_text(json.dumps(out,indent=2)+'\n');print(json.dumps(out,indent=2))
