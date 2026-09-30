import json,re,subprocess
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
W=Path.cwd();V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928';ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';video=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(video),'-an','-vf',"select='between(t,52.3,58)',crop=220:150:320:990,format=rgb24,showinfo",'-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))]
frames=np.frombuffer(p.stdout,np.uint8).reshape(-1,150,220,3)
# Fire tips are saturated orange/red with white caps; fixed UI is below y1160.
mask=(frames[:,:,:,0]>190)&(frames[:,:,:,1]<135)&(frames[:,:,:,2]<115)
counts=mask[:,45:90,:].sum(axis=(1,2))
runs=[];begin=None
for i,c in enumerate(counts):
 if c>=75 and begin is None:begin=i
 if c<75 and begin is not None:runs.append((begin,i-1));begin=None
if begin is not None:runs.append((begin,len(pts)-1))
rows=[dict(start=pts[a],end=pts[b],frames=b-a+1,maxPixels=int(counts[a:b+1].max()),peak=pts[a+int(counts[a:b+1].argmax())]) for a,b in runs]
(V/'fire-tip-upper-pulses.json').write_text(json.dumps(dict(roi=[320,1035,220,45],threshold=dict(rMin=190,gMax=135,bMax=115,minPixels=75),candidates=rows,samples=[dict(pts=t,pixels=int(c)) for t,c in zip(pts,counts)]),indent=2))
sheet=Image.new('RGB',(1000,((len(runs)+7)//8)*125),'white');d=ImageDraw.Draw(sheet)
for i,(a,b) in enumerate(runs):
 k=a+int(counts[a:b+1].argmax());im=Image.fromarray(frames[k]);im.thumbnail((125,100));x=i%8*125;y=i//8*125;sheet.paste(im,(x,y+24));d.text((x,y),f'{i+1}: {pts[k]:.3f}',fill='black')
sheet.save(V/'fire-tip-upper-candidates.png')
print(json.dumps(rows,indent=2))
