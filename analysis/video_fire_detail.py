from pathlib import Path
import subprocess,json,re
from PIL import Image,ImageDraw
W=Path.cwd();V=W/'analysis/targets/wxcf1394487200e48f/43/generated/video-20260928';out=V/'fire-detail';out.mkdir(exist_ok=True)
ff=W/'analysis/vendor/audio/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';vid=V.parent.parent/'evidence/original-reference/video-20260928/original.mp4'
p=subprocess.run([str(ff),'-hide_banner','-nostats','-i',str(vid),'-vf',"select='between(t,52.3,58)*isnan(prev_selected_t)+between(t,52.3,58)*gte(t-prev_selected_t,0.1)',crop=300:300:300:1000,showinfo",'-fps_mode','vfr',str(out/'%03d.png')],stderr=subprocess.PIPE,check=True)
(out/'extract.log').write_bytes(p.stderr)
pts=[float(t) for t in re.findall(r' n:\s*\d+\s+pts:.*?pts_time:([\d.]+)',p.stderr.decode(errors='replace'))]
for batch in range((len(pts)+23)//24):
 sheet=Image.new('RGB',(900,4*173),'white');d=ImageDraw.Draw(sheet)
 for k,t in enumerate(pts[batch*24:(batch+1)*24]):
  i=batch*24+k;im=Image.open(out/f'{i+1:03}.png');im.thumbnail((150,150));x=k%6*150;y=k//6*173;sheet.paste(im,(x,y+20));d.text((x+2,y+3),f'{i+1:03} {t:.3f}',fill='black')
 sheet.save(out/f'sheet-{batch+1}.png')
print('frames',len(pts))
