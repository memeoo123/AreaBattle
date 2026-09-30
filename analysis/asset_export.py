"""Export local authorized Unity evidence; never executes game code or downloads."""
import collections, hashlib, json, re, sys
from pathlib import Path
VENDOR = Path('E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
sys.path.insert(0, str(VENDOR))
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
from UnityPy.classes import PPtr
from PIL import Image
ROOT = Path(__file__).parent / 'targets/wxcf1394487200e48f/43'
CACHE = ROOT / 'work/cache/StreamingAssets/WebGL/Proj_hdzd'
OUT = ROOT / 'generated/unity-assets'
OUT.mkdir(parents=True, exist_ok=True)

def jsondefault(v):
    if isinstance(v, bytes): return {'bytesHex':v.hex()}
    if hasattr(v, '__dict__'): return vars(v)
    return str(v)
def writejson(path, data):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(data,ensure_ascii=False,indent=2,default=jsondefault),encoding='utf-8')
def safe(s): return re.sub(r'[^\w-]', '_', s or 'unnamed')[:100]
def relative(p): return p.relative_to(ROOT).as_posix()
def guidstr(x): return x.hex() if isinstance(x,bytes) else str(x)
env=UnityPy.Environment()
sources={}; bundles=[]
paths=sorted(CACHE.rglob('*.unity3d'))+[ROOT/'work/webdata/data.unity3d',ROOT/'work/webdata/Resources/unity_default_resources']
for p in paths:
    if not p.exists(): continue
    before=set(env.cabs)
    try:
        env.load_file(str(p))
        new=set(env.cabs)-before
        for cab in new: sources[cab]=relative(p)
        bundles.append({'source':relative(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'serializedFiles':sorted(new)})
    except Exception as ex: bundles.append({'source':relative(p),'error':str(ex)})
objects=list(env.objects)
print('Loaded',len(bundles),'bundles;',len(objects),'objects',flush=True)
lookup={(o.assets_file.name.lower(),o.path_id):o for o in objects}
lookup.update({('unity default resources',o.path_id):o for o in objects if o.assets_file.name=='unity_default_resources'})
atlases={}
for o in objects:
    if o.type.name=='SpriteAtlas':
        a=o.read()
        for rk,rd in a.m_RenderDataMap: atlases[str(rk)]=(o,rd)
records=[]; dependencies=[]
def key(o): return o.assets_file.name+':'+str(o.path_id)
def pointer_refs(tree,o,path=''):
    if isinstance(tree,dict):
        if 'm_FileID' in tree and 'm_PathID' in tree and tree['m_PathID']:
            fid=tree['m_FileID']; pid=tree['m_PathID']; cab=o.assets_file.name; ext=None
            if fid:
                ext=o.assets_file.externals[fid-1]
                cab=ext.path.rsplit('/',1)[-1]
            target=lookup.get((cab.lower(),pid))
            return [{'property':path,'fileId':fid,'pathId':pid,'serializedFile':cab,'externalGuid':guidstr(ext.guid) if ext else None,'resolved':target is not None,'target':key(target) if target else None}]
        return [r for k,v in tree.items() for r in pointer_refs(v,o,path+'.'+k)]
    if isinstance(tree,list): return [r for i,v in enumerate(tree) for r in pointer_refs(v,o,path+f'[{i}]')]
    return []
tree_types={'GameObject','Transform','RectTransform','Camera','MeshFilter','MeshRenderer','SkinnedMeshRenderer','SpriteRenderer','Material','MonoBehaviour','MonoScript','AnimationClip','Animator','AnimatorController','ParticleSystem','ParticleSystemRenderer','LineRenderer','TrailRenderer','Sprite','SpriteAtlas','Font','BoxCollider','SphereCollider','CapsuleCollider','Rigidbody','Canvas','CanvasRenderer','Light','RenderSettings','AudioSource'}
for n,o in enumerate(objects):
    name=o.peek_name() or ''
    folder=OUT/safe(o.type.name)
    base=folder/(safe(name)+'__'+safe(o.assets_file.name)+'__'+str(o.path_id))
    row={'id':key(o),'type':o.type.name,'name':name,'pathId':o.path_id,'serializedFile':o.assets_file.name,'source':sources.get(o.assets_file.name.lower(),sources.get(o.assets_file.name)),'originalAssetGuid':None,'outputs':{}}
    records.append(row)
    try:
        if o.type.name in tree_types:
            try: tree=o.read_typetree()
            except ValueError:
                if o.type.name!='MonoBehaviour': raise
                tree=o.read_typetree(check_read=False)
                row['typetreePartial']=True
                row['typetreeLimitation']='Stripped custom MonoBehaviour schema; header only. Raw serialized bytes retained.'
                dest=base.with_suffix('.bin');dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(o.get_raw_data());row['outputs']['raw']=relative(dest)
            row['references']=pointer_refs(tree,o)
            dest=base.with_suffix('.json'); writejson(dest,tree);row['outputs']['typetree']=relative(dest)
        if o.type.name=='Mesh':
            data=o.read(); handler=MeshHandler(data);handler.process()
            mesh={'coordinateSpace':'Unity native; no axis or winding conversion','vertices':handler.m_Vertices,'normals':handler.m_Normals,'uv0':handler.m_UV0,'uv1':handler.m_UV1,'colors':handler.m_Colors,'tangents':handler.m_Tangents,'boneIndices':handler.m_BoneIndices,'boneWeights':handler.m_BoneWeights,'submeshTriangles':list(handler.get_triangles()),'bindPose':o.read_typetree().get('m_BindPose')}
            dest=base.with_suffix('.mesh.json');writejson(dest,mesh);row['outputs']['meshNative']=relative(dest)
            obj=data.export();dest=base.with_suffix('.obj');dest.write_text(obj,encoding='utf-8');row['outputs']['obj']=relative(dest)
            row['objCoordinateNote']='UnityPy OBJ mirrors X and reverses triangle winding; prefer meshNative for Unity.'
            row['vertexCount']=handler.m_VertexCount
        elif o.type.name in {'Texture2D','Sprite'}:
            data=o.read()
            if o.type.name=='Sprite' and not data.m_SpriteAtlas and str(data.m_RenderDataKey) in atlases:
                ao,rd=atlases[str(data.m_RenderDataKey)]
                data.m_SpriteAtlas=PPtr(m_FileID=0,m_PathID=ao.path_id,assetsfile=ao.assets_file)
                row['resolvedAtlasByRenderDataKey']=key(ao)
            if o.type.name=='Texture2D':
                row['textureFormat']=data.m_TextureFormat
                dest=base.with_suffix('.texture.json');writejson(dest,o.read_typetree());row['outputs']['textureRaw']=relative(dest)
                if data.m_Width==0 or data.m_Height==0:
                    row['runtimeGeneratedEmptyTexture']=True
                    continue
            dest=base.with_suffix('.png');dest.parent.mkdir(parents=True,exist_ok=True);im=data.image;im.save(dest);row['outputs']['png']=relative(dest);row['imageSize']=list(im.size)
            if o.type.name=='Sprite':
                rd=atlases[str(data.m_RenderDataKey)][1] if str(data.m_RenderDataKey) in atlases else data.m_RD
                offset=rd.textureRectOffset
                row['trimOffset']={'x':offset.x,'y':offset.y}
                canvas=Image.new('RGBA',(round(data.m_Rect.width),round(data.m_Rect.height)))
                canvas.paste(im,(round(offset.x),round(data.m_Rect.height-offset.y-im.height)))
                dest=base.with_suffix('.canvas.png');canvas.save(dest);row['outputs']['pngCanvas']=relative(dest)
                row['canvasNote']='Original sprite rect dimensions with atlas trimOffset restored; use original normalized pivot and PPU.'
        elif o.type.name=='Shader' and o.assets_file.name.startswith('CAB-'):
            data=o.read();row['name']=data.m_ParsedForm.m_Name
            dest=base.with_suffix('.compiled.txt');dest.parent.mkdir(parents=True,exist_ok=True);dest.write_text(data.export(),encoding='utf-8');row['outputs']['compiledShaderText']=relative(dest)
            row['shaderNote']='Compiled platform evidence; not an importable source shader.'
        elif o.type.name=='Font':
            raw=bytes(o.read().m_FontData)
            if raw:
                dest=base.with_suffix('.otf' if raw[:4]==b'OTTO' else '.ttf');dest.write_bytes(raw);row['outputs']['font']=relative(dest)
            else: row['fontDataEmpty']=True
        elif o.type.name=='AudioClip':
            data=o.read(); row['audioFrequency']=getattr(data,'m_Frequency',None);row['audioChannels']=getattr(data,'m_Channels',None)
            for i,(sample,raw) in enumerate(data.samples.items()):
                dest=base.with_name(base.name+'__'+safe(Path(sample).stem)+Path(sample).suffix);dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(raw);row['outputs'][f'audio{i}']=relative(dest)
    except Exception as ex: row['exportError']=type(ex).__name__+': '+str(ex)[:800]
    if n%500==0: print('Processed',n,flush=True)
for cab in sorted(set(o.assets_file.name for o in objects)):
    af=next(o.assets_file for o in objects if o.assets_file.name==cab)
    dependencies.append({'serializedFile':cab,'externals':[{'fileId':i+1,'path':e.path,'guid':guidstr(e.guid),'type':e.type,'available':any(k[0]==e.path.rsplit('/',1)[-1].lower() for k in lookup)} for i,e in enumerate(af.externals)]})
containers=[{'assetPath':name,'object':key(ptr.deref())} for name,ptr in env.container.items() if ptr]
report={'schemaVersion':1,'target':{'appId':'wxcf1394487200e48f','version':'43'},'tool':{'name':'UnityPy','version':UnityPy.__version__,'vendor':str(VENDOR)},'sourceGuidNote':'Original .meta GUIDs are absent from cooked bundles; serialized CAB+PathID identity and external GUIDs are preserved without inventing original asset GUIDs.','bundles':bundles,'dependencies':dependencies,'containers':containers,'objects':records,'counts':dict(collections.Counter(r['type'] for r in records))}
writejson(ROOT/'generated/asset-evidence.json',report)
print(json.dumps({'counts':report['counts'],'errorCount':sum('exportError'in r for r in records),'exports':sum(bool(r['outputs']) for r in records)},ensure_ascii=False),flush=True)
