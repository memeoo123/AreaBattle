"""Prepare cached original display soldier400 without changing original exports or gameplay data."""
import json,hashlib
from pathlib import Path
T=Path(__file__).resolve().parent/'targets/wxcf1394487200e48f/43';G=T/'generated';P=G/'outgame/model400-prepared';P.mkdir(exist_ok=True)
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def find(folder,pattern):return next((G/'unity-assets'/folder).glob(pattern))
hpath=G/'unity-assets/prefabs/soldier_400.hierarchy.json';h=read(hpath)
anim=next(c['data'] for c in h['root']['components'] if (c.get('script')or{}).get('m_ClassName')=='SpineAnimator')
matpath=find('Material','soldier_400Material*.json');mat=read(matpath);colors=dict(mat['m_SavedProperties']['m_Colors']);tex=dict(mat['m_SavedProperties']['m_TexEnvs'])
inventory=read(G/'asset-evidence.json')['objects']
def texture(prop):
 id=tex[prop]['m_Texture']['m_PathID'];o=next(o for o in inventory if o['type']=='Texture2D' and o['id'].endswith(':'+str(id)));return o,read(T/o['outputs']['textureRaw'])
at,ad=texture('_AnimTex');mt,md=texture('_MainTex');raw=bytes.fromhex(ad['image data']['bytesHex']);assert ad['m_TextureFormat']==4 and len(raw)==ad['m_Width']*ad['m_Height']*4
rawpath=P/'soldier_400.anim.rgba32';rawpath.write_bytes(raw)
run=dead=dict(startFrame=0,lengthFrames=0,lengthSeconds=0) # display-only prefab has no run/dead clips
meshpath=find('Mesh','soldier_400Mesh*.mesh.json');native=read(meshpath)
mesho=next(o for o in inventory if o.get('name')=='soldier_400Mesh' and o['type']=='Mesh')
row=dict(name='soldier_400',prefabId=4000,soldierType=3,meshSourceId=mesho['id'],animTexturePath=rawpath.relative_to(T).as_posix(),animTextureSha256=hashlib.sha256(raw).hexdigest(),animWidth=ad['m_Width'],animHeight=ad['m_Height'],animLinear=ad['m_ColorSpace']==0,animFilter=ad['m_TextureSettings']['m_FilterMode'],animWrap=ad['m_TextureSettings']['m_WrapU'],mainTexturePath=mt['outputs']['png'],mainSRGB=md['m_ColorSpace']==1,mainFilter=md['m_TextureSettings']['m_FilterMode'],mainWrap=md['m_TextureSettings']['m_WrapU'],animAdd=[colors['_AnimAdd'][k] for k in 'rgba'],animMul=[colors['_AnimMul'][k] for k in 'rgba'],runStart=run['startFrame'],runFrames=run['lengthFrames'],runSeconds=run['lengthSeconds'],deadStart=dead['startFrame'],deadFrames=dead['lengthFrames'],deadSeconds=dead['lengthSeconds'],scale=1.0,sortingOrder=-5,renderQueue=3000)
data=dict(soldiers=[],campColors=read(G/'presentation-prepared/runtime-data.json')['campColors']);data['soldiers']=[s for s in data['soldiers'] if s['name']!=row['name']]+[row];write(P/'runtime-data.json',data)
m=dict(name='soldier_400',sourcePath=meshpath.relative_to(T).as_posix(),sourceId=mesho['id'],sourceSha256=hashlib.sha256(meshpath.read_bytes()).hexdigest(),coordinateSpace=native['coordinateSpace'])
for key,axes in [('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')]:
 values=native.get(key);m[key]=[dict(zip(axes,v)) for v in values] if values is not None else [];m['has'+key[0].upper()+key[1:]]=values is not None
m['submeshes']=[dict(indices=[i for tri in triangles for i in tri]) for triangles in native['submeshTriangles']]
meshes=dict(meshes=[]);meshes['meshes']=[x for x in meshes['meshes'] if x['name']!=m['name']]+[m];write(P/'soldier-meshes-runtime.json',meshes)

clips=read(G/'outgame/baked-model-clips.json');clips['models']=[r for r in clips['models'] if r['name']!='soldier_400'];clips['models'].append(dict(name='soldier_400',sourcePrefab='Recovered/Outgame/ModelAssets/soldier_400',source=hpath.relative_to(T).as_posix(),sha256=hashlib.sha256(hpath.read_bytes()).hexdigest(),autoPlay=anim['autoPlayAnimation'],clips=[dict(name=c['name'],startFrame=c['startFrame'],lengthFrames=c['lengthFrames'],lengthSeconds=c['lengthSeconds'],isNeedFix=bool(c['isNeedFix'])) for c in anim['animationData']]));assert all(not c['isNeedFix'] for c in anim['animationData']);write(G/'outgame/baked-model-clips.json',clips)
print('Prepared source soldier_400 display model',len(m['vertices']))
