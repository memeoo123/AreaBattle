import subprocess,re,json
from pathlib import Path
import numpy as np
W=Path.cwd();V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928';ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
f="[0:v]select='lt(t,36)',split=3[a][b][c];[a]crop=32:32:215:634[a1];[b]crop=32:32:535:634[b1];[c]crop=300:32:200:576[c1];[a1][b1][c1]hstack=inputs=3,format=rgb24,showinfo[out]"
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-filter_complex',f,'-map','[out]','-an','-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))];a=np.frombuffer(p.stdout,np.uint8).reshape(-1,32,364,3);mask=(a[:,:,:,2]>170)&(a[:,:,:,0]<140)&(a[:,:,:,1]>70)
rows=[]
for name,x,w,threshold in [('1-5',0,32,95),('2-5',32,32,95),('1-2',64,300,1000)]:
 scores=mask[:,:,x:x+w].sum(axis=(1,2));active=scores>=threshold;changes=[]
 for i in range(1,len(pts)-10):
  if active[i]!=active[i-1] and all(active[i:i+10]==active[i]):changes.append(dict(lastOppositePts=pts[i-1],firstVisibleStatePts=pts[i],blueSegmentVisible=bool(active[i]),bluePixels=int(scores[i])))
 rows.append(dict(pair=name,threshold=threshold,changes=changes))
out=dict(method='Native-frame persistent blue-pixel visibility in fixed line ROIs. Includes drag preview; not exact input/click times or direction proof.',rois=[[215,634,32,32],[535,634,32,32],[200,576,300,32]],results=rows)
(V/'early-line-visibility.json').write_text(json.dumps(out,indent=2));print(json.dumps(out,indent=2))
