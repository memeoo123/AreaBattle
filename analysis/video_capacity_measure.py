import subprocess,re,json
from pathlib import Path
import numpy as np
W=Path.cwd();V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928';ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
f="[0:v]select='lt(t,36)',split=2[a][b];[a]crop=74:54:100:430[a1];[b]crop=74:54:546:430[b1];[a1][b1]hstack=inputs=2,format=rgb24,showinfo[out]"
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-filter_complex',f,'-map','[out]','-an','-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))];a=np.frombuffer(p.stdout,np.uint8).reshape(-1,54,148,3);mask=(a[:,:,:,2]>180)&(a[:,:,:,0]<90)&(a[:,:,:,1]>70)
rows=[]
for tower,x in [(1,0),(2,74)]:
 scores=mask[:,:,x:x+74].sum(axis=(1,2));state=np.rint(scores/140).astype(int);changes=[]
 for i in range(1,len(pts)-3):
  if pts[i]>1 and state[i]!=state[i-1] and all(state[i:i+3]==state[i]):changes.append(dict(previousPts=pts[i-1],pts=pts[i],before=int(state[i-1]),after=int(state[i]),pixels=int(scores[i])))
 rows.append(dict(tower=tower,changes=changes))
out=dict(method='Filled blue capacity circles count: native RGB mask pixels/140 rounded; manually checked against half-second frames. Capacity changes establish committed outgoing counts; line direction/pair identified from adjacent reference frames.',roi=[[100,430,74,54],[546,430,74,54]],results=rows)
(V/'early-committed-line-inputs.json').write_text(json.dumps(out,indent=2));print(json.dumps(out,indent=2))
