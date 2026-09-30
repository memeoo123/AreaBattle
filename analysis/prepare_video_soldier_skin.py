"""Prepare cached original soldier102 without changing original exports or gameplay data."""
import json,hashlib
from pathlib import Path
T=Path(__file__).resolve().parent/'targets/wxcf1394487200e48f/43';G=T/'generated';P=G/'presentation-prepared'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def find(folder,pattern):return next((G/'unity-assets'/folder).glob(pattern))
hpath=G/'unity-assets/prefabs/soldier_102.hierarchy.json';h=read(hpath)
anim=next(c['data'] for c in h['root']['components'] if (c.get('script')or{}).get('m_ClassName')=='SpineAnimator')
matpath=find('Material','soldier_102Material*.json');mat=read(matpath);colors=dict(mat['m_SavedProperties']['m_Colors']);tex=dict(mat['m_SavedProperties']['m_TexEnvs'])
inventory=read(G/'asset-evidence.json')['objects']
def texture(prop):
 id=tex[prop]['m_Texture']['m_PathID'];o=next(o for o in inventory if o['type']=='Texture2D' and o['id'].endswith(':'+str(id)));return o,read(T/o['outputs']['textureRaw'])
at,ad=texture('_AnimTex');mt,md=texture('_MainTex');raw=bytes.fromhex(ad['image data']['bytesHex']);assert ad['m_TextureFormat']==4 and len(raw)==ad['m_Width']*ad['m_Height']*4
rawpath=P/'soldier_102.anim.rgba32';rawpath.write_bytes(raw)
run=next(a for a in anim['animationData'] if a['name']=='run');dead=next(a for a in anim['animationData'] if a['name']=='dead')
meshpath=find('Mesh','soldier_102Mesh*.mesh.json');native=read(meshpath)
mesho=next(o for o in inventory if o.get('name')=='soldier_102Mesh' and o['type']=='Mesh')
row=dict(name='soldier_102',prefabId=1002,soldierType=1,meshSourceId=mesho['id'],animTexturePath=rawpath.relative_to(T).as_posix(),animTextureSha256=hashlib.sha256(raw).hexdigest(),animWidth=ad['m_Width'],animHeight=ad['m_Height'],animLinear=ad['m_ColorSpace']==0,animFilter=ad['m_TextureSettings']['m_FilterMode'],animWrap=ad['m_TextureSettings']['m_WrapU'],mainTexturePath=mt['outputs']['png'],mainSRGB=md['m_ColorSpace']==1,mainFilter=md['m_TextureSettings']['m_FilterMode'],mainWrap=md['m_TextureSettings']['m_WrapU'],animAdd=[colors['_AnimAdd'][k] for k in 'rgba'],animMul=[colors['_AnimMul'][k] for k in 'rgba'],runStart=run['startFrame'],runFrames=run['lengthFrames'],runSeconds=run['lengthSeconds'],deadStart=dead['startFrame'],deadFrames=dead['lengthFrames'],deadSeconds=dead['lengthSeconds'],scale=.052000001072883606,sortingOrder=-5,renderQueue=3000)
data=read(P/'runtime-data.json');data['soldiers']=[s for s in data['soldiers'] if s['name']!=row['name']]+[row];write(P/'runtime-data.json',data)
m=dict(name='soldier_102',sourcePath=meshpath.relative_to(T).as_posix(),sourceId=mesho['id'],sourceSha256=hashlib.sha256(meshpath.read_bytes()).hexdigest(),coordinateSpace=native['coordinateSpace'])
for key,axes in [('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')]:
 values=native.get(key);m[key]=[dict(zip(axes,v)) for v in values] if values is not None else [];m['has'+key[0].upper()+key[1:]]=values is not None
m['submeshes']=[dict(indices=[i for tri in triangles for i in tri]) for triangles in native['submeshTriangles']]
meshes=read(P/'soldier-meshes-runtime.json');meshes['meshes']=[x for x in meshes['meshes'] if x['name']!=m['name']]+[m];write(P/'soldier-meshes-runtime.json',meshes)
skin=next(x for x in read(G/'tables/SkinConfig.json')['Datas'] if x['id']==102);assert skin['prefabId']==1002 and skin['skinType']==1
write(G/'video-soldier-skin-evidence.json',dict(status='source-asset-confirmed-video-match-inferred',skin=skin,prefab=h['assetPath'],runtime=row,
 sourceBinding='generated/model-binding-evidence.json: dynamic-soldier-model/player-selected-skin/enemy-skin',
 inference='Pointed asymmetric hat and star staff in cached skin04 atlas match the recorded ordinary soldiers. Visual identification, not original account read; camp-specific original skin selection remains inferred.',
 sources=[dict(path=p.relative_to(T).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [hpath,matpath,meshpath,T/at['outputs']['textureRaw'],T/mt['outputs']['png']]]))
print('Prepared soldier102:',len(m['vertices']),'vertices;',run)
