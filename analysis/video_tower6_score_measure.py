"""Measure original right-bottom tower digits; unknown frames remain unknown."""
import json, re, subprocess
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

W = Path(__file__).resolve().parent.parent
V = W / 'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928'
ff = W / 'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
video = V.parent.parent / 'evidence/original-reference/video-20260928/original.mp4'
x, y, w, h = 578, 772, 22, 26
p = subprocess.run([str(ff), '-hide_banner', '-nostats', '-i', str(video), '-an',
    '-vf', f"select='between(t,52,74)',crop={w}:{h}:{x}:{y},format=gray,showinfo",
    '-fps_mode', 'passthrough', '-f', 'rawvideo', 'pipe:1'],
    stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True)
pts = [float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)', p.stderr.decode(errors='replace'))]
frames = np.frombuffer(p.stdout, np.uint8).reshape(-1, h, w).astype(float)
assert len(pts) == len(frames)
# Contact sheets manually reviewed: 5, 3, 1, 0, 2; digit 3 at native57.17905.
references = {5: 52.00987, 3: 57.17905, 1: 64.116, 0: 61.04827, 2: 66.11704}
templates = {k: frames[min(range(len(pts)), key=lambda i: abs(pts[i]-t))] for k,t in references.items()}
values = list(templates)
errors = np.stack([((frames-t)**2).mean((1,2)) for t in templates.values()], axis=1)
labels = [values[int(e.argmin())] if e.min() < 110 else None for e in errors]
changes = []
for i in range(1,len(pts)-3):
    if labels[i] != labels[i-1] and all(v == labels[i] for v in labels[i:i+3]):
        changes.append(dict(previousPts=pts[i-1], pts=pts[i], before=labels[i-1], after=labels[i], mse=float(errors[i].min())))
out = dict(roi=[x,y,w,h], references=references, thresholdMse=110,
    method='Native frames; manually reviewed score templates; three-frame persistence. Null is unclassified, never zero.', changes=changes)
out['samples'] = [dict(pts=t, score=s, mse=float(e.min())) for t,s,e in zip(pts,labels,errors)]
(V/'tower6-score-pixels.json').write_text(json.dumps(out,indent=2)+'\n')
unknown = [i for i in range(len(pts)) if labels[i] is None and 57 < pts[i] < 58][::6]
sheet = Image.new('RGB',(600,((len(unknown)+3)//4)*125),'white'); draw=ImageDraw.Draw(sheet)
for j,i in enumerate(unknown):
    xx=(j%4)*150; yy=(j//4)*125
    sheet.paste(Image.fromarray(frames[i].astype(np.uint8)).resize((124,88)),(xx,yy))
    draw.text((xx,yy+92),str(pts[i]),fill='black')
if unknown: sheet.save(V/'tower6-unclassified-review.png')
print(json.dumps(out,indent=2))

