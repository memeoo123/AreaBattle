"""Measure the native lightning onset without retiming gameplay or fitting delays."""
import json,re,subprocess
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';v=t/'generated/video-20260928';ff=w/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(t/'evidence/original-reference/video-20260928/original.mp4'),'-an','-vf',"select='between(t,62.25,62.8)',crop=320:950:200:180,format=rgb24,showinfo",'-fps_mode','passthrough','-f','rawvideo','pipe:1'],stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
pts=[float(x) for x in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))];frames=np.frombuffer(p.stdout,np.uint8).reshape(-1,950,320,3);assert len(pts)==len(frames)
counts=[]
for a in frames:
 roi=a[:300].astype(int);r,g,b=roi[:,:,0],roi[:,:,1],roi[:,:,2];counts.append(int(((r>200)&(g>140)&(r-b>50)&(g-b>35)).sum()))
first=next(i for i,n in enumerate(counts) if n>10)
start=max(0,first-4);end=min(len(pts),first+8);sheet=Image.new('RGB',(6*160,2*505),'#172337');d=ImageDraw.Draw(sheet)
for j,i in enumerate(range(start,end)):
 x=j%6*160;y=j//6*505;sheet.paste(Image.fromarray(frames[i]).resize((160,475)),(x,y));d.text((x+3,y+480),f'{pts[i]:.5f}s yellow={counts[i]}',fill='white')
sheet.save(v/'lightning-onset-contact.png')
result={'classification':'observed-onset-not-a-fixed-delay-contract','damageFirstObservedPts':62.29858,'lastFrameWithoutBeam':pts[first-1],'firstFrameWithBeam':pts[first],'observedSeparationSeconds':round(pts[first]-62.29858,6),'detector':{'videoCrop':[200,180,320,950],'emptySkyRoiWithinCrop':[0,0,320,300],'yellow':'R>200 G>140 R-B>50 G-B>35; count>10'},'samples':[dict(pts=t,yellowPixels=n) for t,n in zip(pts,counts)],'source':'Type4171.Execute f14822 @006e75de EffectModule.Show before immediate ChangeScore @006e7650. Effect readiness can be asynchronous; this observation alone cannot set a constant delay.','local':'Current replay shows lightning in committed damage frame; local resource readiness is synchronous.','limitations':['One recording with unknown original asset-cache/load state.','Audio4401 correlation candidate begins62.35533 but is not a sample-accurate DSP measurement.','No gameplay event, particle delay, or input timestamp was changed.']}
(v/'lightning-onset-audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print(json.dumps({k:x for k,x in result.items() if k!='samples'},ensure_ascii=True,indent=2))
