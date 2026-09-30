from pathlib import Path
s=Path('analysis/guide_resolve.py').read_text(encoding='utf8')
s=s.replace('wanted={949,10103,7025,7029,1348,3531,5573}','wanted={2198,2610,1112,2077,1223,2516,3259,1506}')
exec(compile(s,'hud_resolve','exec'))
