import json,re
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
v=Path('analysis/targets/wxcf1394487200e48f/43/generated/video-20260928')
rows=json.loads(re.search(r'const rows=(.*?);const \$',(v/'visual-review.html').read_text(encoding='utf8')).group(1))
regions={'upperBackground':[110,145,615,330],'leftDecoration':[1,125,103,372],'levelTitle':[297,67,422,106],'toolPoints':[495,1047,648,1082]}
results=[]
for i,r in enumerate(rows,1):
 o=np.asarray(Image.open(v/r['original']).convert('RGB').crop((0,54,720,1334))).astype(float)
 b=np.asarray(Image.open(v/'before-capture-refresh'/r['replay']).convert('RGB')).astype(float)
 a=np.asarray(Image.open(v/r['replay']).convert('RGB')).astype(float)
 c=np.asarray(Image.open(v/'entry-aspect-candidate'/f'checkpoint-{i:02}.png').convert('RGB').resize((720,1280),Image.Resampling.LANCZOS)).astype(float)
 scores={}
 for name,(x0,y0,x1,y1) in regions.items():
  scores[name]={key:round(float(np.abs(o[y0:y1,x0:x1]-im[y0:y1,x0:x1]).mean()),4) for key,im in [('before',b),('textRefresh',a),('entryAspectCandidate',c)]}
 results.append({'checkpoint':i,'pts':r['pts'],'regions':scores})
out={'status':'diagnostic-capture-fixed-source-viewport-inferred','sourceViewportCandidate':[723,1282],'canonicalViewport':[720,1280],'backgroundRule':'WASM f7819 uses exact aspect comparison against0.5625; wider branch scale=2*2.1*.5625/2.5875000953674316, no gameplay parameter change.','candidateProvenance':'Existing entry screenshot registration original871-visual-comparison.json. This does not prove the video internal framebuffer dimensions.','textRootCause':'Cached static Text geometry generated before RenderTexture attachment at12/10px versus correct32/26px. CanvasScaler refresh alone did not regenerate it; SetAllDirty and ForceUpdateCanvases did.','numericReplayUnchanged':json.loads((v/'full-replay-diagnostic.json').read_text())==json.loads((v/'full-replay-before-skin102.json').read_text()),'regions':regions,'metrics':results,'limitations':['Metrics describe static ROIs, not full visual acceptance.','Entry-aspect images are an explicit hypothesis; canonical9:16 images are retained.','Numbers, soldiers, original RNG and account data were not fitted.','No production scripts, font files, shaders or materials changed.']}
(v/'capture-registration-audit.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
# Analytical contact sheet: same static pixels across reference / old / refreshed / source-viewport candidate.
idx=1;r=rows[idx]
ims=[Image.open(v/r['original']).convert('RGB').crop((0,54,720,1334)),Image.open(v/'before-capture-refresh'/r['replay']).convert('RGB'),Image.open(v/r['replay']).convert('RGB'),Image.open(v/'entry-aspect-candidate'/f'checkpoint-{idx+1:02}.png').convert('RGB').resize((720,1280),Image.Resampling.LANCZOS)]
canvas=Image.new('RGB',(4*360,730),'#172337');d=ImageDraw.Draw(canvas)
for j,(im,label) in enumerate(zip(ims,['Original video','Before capture fix','Text mesh refreshed','Entry aspect candidate'])):
 d.text((j*360+10,10),label,fill='white')
 canvas.paste(im.crop((0,0,720,440)).resize((360,220)),(j*360,35))
 canvas.paste(im.crop((285,61,433,113)).resize((296,104)),(j*360+25,290))
 canvas.paste(im.crop((490,1040,655,1089)).resize((330,98)),(j*360+15,440))
 d.text((j*360+10,570),'Title and points: 2x pixel crop',fill='white')
canvas.save(v/'capture-registration-comparison.png')
print(json.dumps(results[1],ensure_ascii=False,indent=2))
