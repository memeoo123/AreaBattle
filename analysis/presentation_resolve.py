from pathlib import Path
s=Path('analysis/guide_resolve.py').read_text(encoding='utf8').replace('wanted={949,10103,7025,7029,1348,3531,5573}','wanted={1200,3504,3253,1264,5605,7772,1130,7555,3847,3581,5896,2926,946,3846,2784,11425,7100,4786,7315,4374}')
exec(compile(s,'presentation_resolve','exec'))


