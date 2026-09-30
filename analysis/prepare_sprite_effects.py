import json,struct,hashlib
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43')
def load(p):return json.loads((R/p).read_text(encoding='utf-8'))
objects=load('generated/asset-evidence.json')['objects'];lookup={o['id']:o for o in objects};result=[]
for name,effect in [('hit',104),('LevelUp',105),('LevelUp_B',106)]:
 clip=next(x for x in objects if x['type']=='AnimationClip' and x['name']==name);d=load(clip['outputs']['typetree']);bindings=d['m_ClipBindingConstant'];assert len(bindings['genericBindings'])==1 and bindings['genericBindings'][0]['isPPtrCurve']==1
 words=d['m_MuscleClip']['m_Clip']['data']['m_StreamedClip']['data'];raw=struct.pack('<'+'I'*len(words),*words);at=0;frames=[]
 while at<len(raw):
  time,count=struct.unpack_from('<fI',raw,at);at+=8
  for _ in range(count):
   curve,*coeff=struct.unpack_from('<Iffff',raw,at);at+=20
   assert curve==0 and coeff[:3]==[0,0,0]
   index=int(coeff[3]);ptr=bindings['pptrCurveMapping'][index];assert ptr['m_FileID']==0
   sprite=lookup[clip['serializedFile']+':'+str(ptr['m_PathID'])];s=load(sprite['outputs']['typetree']);texture=lookup[next(x['target'] for x in sprite['references'] if x['property']=='.m_RD.texture')];tex=load(texture['outputs']['textureRaw'])
   frames.append({'time':max(0,time),'index':index,'spriteSource':sprite['id'],'path':sprite['outputs']['pngCanvas'],'sha256':hashlib.sha256((R/sprite['outputs']['pngCanvas']).read_bytes()).hexdigest(),'pivot':s['m_Pivot'],'ppu':s['m_PixelsToUnits'],'border':s['m_Border'],'filter':tex['m_TextureSettings']['m_FilterMode'],'sRGB':tex['m_ColorSpace']==1})
 assert at==len(raw)
 result.append({'name':name,'effectId':effect,'clipSource':clip['id'],'frames':frames,'stopTime':d['m_MuscleClip']['m_StopTime'],'loop':d['m_MuscleClip']['m_LoopTime'],'scope':'Source packed streamed single sprite curve; key times and PPtr identity preserved.'})
(R/'generated/presentation-prepared/sprite-effects-runtime.json').write_text(json.dumps({'effects':result},ensure_ascii=False,indent=2),encoding='utf-8')
print([(x['name'],len(x['frames']),x['stopTime'],x['loop']) for x in result])
