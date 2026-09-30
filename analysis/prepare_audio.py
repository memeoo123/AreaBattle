import json,sys,subprocess,hashlib
from pathlib import Path
sys.path.insert(0,str(Path('analysis/vendor/audio').resolve()))
import imageio_ffmpeg
R=Path('analysis/targets/wxcf1394487200e48f/43');E=json.loads((R/'generated/asset-evidence.json').read_text(encoding='utf-8'));rows=json.loads((R/'generated/tables/AudioConfig.json').read_text(encoding='utf-8'))['Datas'];clips={o['name']:o for o in E['objects'] if o['type']=='AudioClip'}
for export in (R/'generated/resource-snapshots').glob('*/audio-*-export.json'):
 entry=json.loads(export.read_text(encoding='utf-8'));clip=entry.get('audioClip')
 if entry.get('passed') and clip:clips[clip['name']]=clip
for export in (R/'generated/resource-snapshots').glob('*/asset-evidence-incremental.json'):
 for clip in json.loads(export.read_text(encoding='utf-8'))['objects']:
  if clip['type']=='AudioClip' and 'audio0' in clip.get('outputs',{}):clips[clip['name']]=clip
items=[];missing=[]
for row in rows:
 if row['id']==1002:continue
 name=Path(row['ResPath']).stem;o=clips.get(name)
 if o is None:missing.append({'id':row['id'],'sourcePath':row['ResPath']});continue
 source=R/o['outputs']['audio0'];dest=R/f'generated/presentation-prepared/audio/{row["id"]}.wav';dest.parent.mkdir(parents=True,exist_ok=True)
 subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-nostdin','-y','-i',str(source),'-c:a','pcm_f32le',str(dest)],check=True)
 items.append({'id':row['id'],'name':name,'source':o['id'],'originalPath':o['outputs']['audio0'],'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'path':dest.relative_to(R).as_posix(),'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'volume':row['Vol'],'voiceType':row['VoiceType'],'encoding':'PCM float32 WAV decoded from original AAC; no resampling'})
(R/'generated/presentation-prepared/audio-runtime.json').write_text(json.dumps({'clips':items,'missing':missing},indent=2),encoding='utf-8')
print('decoded',len(items),'not-yet-acquired',len(missing))
