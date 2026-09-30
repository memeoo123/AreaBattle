from pathlib import Path
import json,sys,subprocess,hashlib,shutil
sys.path.insert(0,str(Path('analysis/vendor/audio').resolve()));import imageio_ffmpeg
T=Path('analysis/targets/wxcf1394487200e48f/43');e=json.loads((T/'generated/asset-evidence.json').read_text(encoding='utf8'));o=next(o for o in e['objects'] if o['type']=='AudioClip' and o['name']=='hcrzd_MainBGM');src=T/o['outputs']['audio0'];dst=T/'generated/presentation-prepared/audio/1002.wav'
subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-nostdin','-y','-i',str(src),'-c:a','pcm_f32le',str(dst)],check=True)
prod=Path('UnityProject/Assets/AreaBattle/Resources/Recovered/Audio/1002.wav');shutil.copy2(dst,prod)
row=next(r for r in json.loads((T/'generated/tables/AudioConfig.json').read_text())['Datas'] if r['id']==1002)
(T/'generated/outgame/MAIN_AUDIO_ASSET_EVIDENCE.json').write_text(json.dumps(dict(config=row,source=o,sourceSha256=hashlib.sha256(src.read_bytes()).hexdigest(),decodedPath=dst.as_posix(),productionPath=prod.as_posix(),sha256=hashlib.sha256(dst.read_bytes()).hexdigest(),encoding='PCM float32 WAV decoded from original AAC; no resampling'),ensure_ascii=False,indent=2),encoding='utf8')
print(dict(source=o['id'],size=prod.stat().st_size))
