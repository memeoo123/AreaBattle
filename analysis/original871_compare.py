"""Read original and reconstructed pixels for metrics; never alter source images."""
from PIL import Image
import json, hashlib
from pathlib import Path
R=Path(__file__).resolve().parent/'targets/wxcf1394487200e48f/43'
W=Path(__file__).resolve().parent.parent
p=R/'evidence/original-reference/level871-entry-user.png'
q=W/'analysis/captures/original871-reconstruction-1_5seconds.png'
a=Image.open(p).convert('RGB').load();b=Image.open(q).convert('RGB').load()
regions={'visible-battlefield':[1,0,722,1263],'upper-empty-background':[110,145,615,340],
 'left-background-decoration':[1,125,103,372],'topbar':[20,40,484,126],
 'blue-score-and-capacity':[115,367,163,430],'bottom-skill-icons':[255,1122,651,1240],
 'commander-animation':[90,1020,243,1256]}
rows=[]
for name,(x,y,x2,y2) in regions.items():
 total=within=n=0
 for yy in range(y,y2):
  for xx in range(x,x2):
   d=[abs(c-e) for c,e in zip(a[xx,yy+55],b[xx,yy])]
   total+=sum(d);within+=max(d)<=16;n+=1
 rows.append({'region':name,'rect':[x,y,x2,y2],'meanAbsoluteRGBError':round(total/(n*3),4),'fractionPixelsWithin16':round(within/n,4)})
out={'reference':'evidence/original-reference/level871-entry-user.png','capture':q.relative_to(W).as_posix(),
 'referenceSha256':hashlib.sha256(p.read_bytes()).hexdigest(),'captureSha256':hashlib.sha256(q.read_bytes()).hexdigest(),
 'registration':{'viewport':[0,55,723,1282],'visibleHeight':1263,'status':'inferred from supplied window dimensions, source aspect branch and static anchors; bottom19px occluded'},
 'fixture':{'seconds':1.5,'input':'none','managedSeed':4305,'unitySeed':4305,'genericPoints':27,'specificStock':0,'sourceFrameTime':'unknown'},
 'regions':rows,'classification':'single-frame representative comparison; full synchronized replay pending',
 'limits':['User confirms no player input, not exact elapsed time.','Matching entry topology occurs in this local seeded run; original seed remains unknown.','Metrics include antialiasing, border registration and animation phase differences; not a gameplay accuracy percentage.','No reference pixels were modified; pixels were read only for numeric comparison.']}
(R/'generated/original871-visual-comparison.json').write_text(json.dumps(out,indent=2),encoding='utf-8')
print(json.dumps(rows))
