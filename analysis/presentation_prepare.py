"""Prepare evidence-backed presentation descriptors; no Unity project writes."""
import hashlib,json,re,struct,sys
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
from UnityPy.enums import TextureFormat
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
GEN=ROOT/'generated'
OUT=GEN/'presentation-prepared'
OUT.mkdir(exist_ok=True)
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf8')
evidence=read(GEN/'presentation-binding-evidence.json')
base=read(GEN/'asset-evidence.json')
inc=read(GEN/'resource-snapshots/hud-soldier300-20260928/asset-evidence-incremental.json')
objects={o['id']:o for o in base['objects']};objects.update({o['id']:o for o in inc['objects']})
def tree(o):return read(ROOT/o['outputs']['typetree'])
def resource(o):
    return {k:o[k] for k in ('id','type','name','outputs') if k in o}|{'outputHashes':{k:hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for k,p in o.get('outputs',{}).items() if isinstance(p,str) and (ROOT/p).is_file()}}
def source(fn):
    f=next(f for f in evidence['functions'] if f['function']==fn and f['module']=='wasmcode')
    return {k:f[k] for k in ('class','method','module','function','token','body')}
shader=read(GEN/'unity-assets/ShaderEvidence/Spine_SkeletonMeshBaker.typetree.json')
shader_descriptor={'name':shader['m_ParsedForm']['m_Name'], 'source':'generated/unity-assets/ShaderEvidence/Spine_SkeletonMeshBaker.typetree.json','glesSource':'generated/unity-assets/ShaderEvidence/Spine_SkeletonMeshBaker.platform0.segment0.strings.txt','subshaders':[{'tags':s.get('m_Tags'),'passes':[{'state':p['m_State'],'tags':p['m_Tags']} for p in s['m_Passes']]} for s in shader['m_ParsedForm']['m_SubShaders']], 'effectiveQueue':3000,'queueTag':'Transparent','normalPass':{'blend':'One OneMinusSrcAlpha','blendOp':'Add','zWrite':False,'zTest':'LEqual','cull':'Off','stencilCompProperty':'_StencilComp','stencilRefProperty':'_StencilRef'}, 'vertexFormula':{'time':'t=(_Time.y-_AnimTime.w)*_AnimTime.z','phase':'lerp(clamp(t,0,1),frac(t),_AnimLoop)','uv':'((uv1.x+.5)/textureWidth,(_AnimTime.x+.5+phase*_AnimTime.y)/textureHeight)','decodeXY':'(sample.xz*65536+sample.yw)*_AnimMul.xy*(1/65535)+_AnimAdd.xy','decodeZ':'_AnimAdd.z'},'fragmentFormula':'s=texture(_MainTex,uv0)*vertexColor; a=smoothstep(.1,.5,length(s.rgb-(.308999985,.41170001,.729399979))); output=lerp(_Color,s,a)','ordinaryKeywords':[]}
write(OUT/'soldier-shader.json',shader_descriptor)
# ColorHelper.cctor has seven tower colors followed by six soldier colors.
lines=next(f['disassembly'] for f in evidence['functions'] if f['function']==20214)
constants=[(l[:8],int(re.search(r'\[(-?\d+)\]',l)[1])) for l in lines if 'i64.const' in l and '[32]' not in l]
assert len(constants)==52
colors=[]
for i in range(28,52,4):
    a,b,c,d=constants[i:i+4];assert a[1]==b[1] and c[1]==d[1]
    rgba=list(struct.unpack('<ff',struct.pack('<Q',c[1])))+list(struct.unpack('<ff',struct.pack('<Q',a[1])))
    colors.append({'camp':len(colors)+1,'rgba':rgba,'constantOffsets':[a[0],c[0]]})
soldiers=[]
for name in ('soldier_100','soldier_200','soldier_300'):
    path=GEN/(f'resource-snapshots/hud-soldier300-20260928/prefabs/{name}.flat.json' if name.endswith('300') else f'unity-assets/prefabs/{name}.hierarchy.json')
    prefab=read(path);node=prefab['nodes'][0] if 'nodes' in prefab else prefab['root']
    components=node['components']
    renderer=next(c for c in components if c['type']=='MeshRenderer')
    meshfilter=next(c for c in components if c['type']=='MeshFilter')
    animator=next(c for c in components if (c.get('script')or{}).get('m_ClassName')=='SpineAnimator')
    material=objects[next(r['target'] for r in renderer['references'] if r['property'].startswith('.m_Materials'))]
    mt=tree(material)
    textures=[]
    for prop,texenv in mt['m_SavedProperties']['m_TexEnvs']:
        ref=next(r for r in material['references'] if r['pathId']==texenv['m_Texture']['m_PathID'] and objects[r['target']]['type']=='Texture2D')
        o=objects[ref['target']];tt=read(ROOT/o['outputs']['textureRaw'])
        textures.append({'property':prop,'resource':resource(o),'sourcePointer':texenv['m_Texture'],'transform':{k:v for k,v in texenv.items() if k!='m_Texture'},'width':tt['m_Width'],'height':tt['m_Height'],'format':tt['m_TextureFormat'],'formatName':TextureFormat(tt['m_TextureFormat']).name,'mipCount':tt['m_MipCount'],'readable':tt['m_IsReadable'],'settings':tt['m_TextureSettings'],'colorSpace':tt['m_ColorSpace'],'sRGB':tt['m_ColorSpace']==1})
    mesh=objects[next(r['target'] for r in meshfilter['references'] if r['property']=='.m_Mesh')]
    anim=animator['data'];run=next(a for a in anim['animationData'] if a['name']=='run')
    descriptor={'name':name,'sourcePrefab':str(path.relative_to(ROOT)).replace('\\','/'),'sourceObject':node.get('id',node.get('object')),'mesh':resource(mesh),'material':resource(material),'materialSavedValues':mt,'rendererSerialized':renderer['data'],'textures':textures,'animationData':anim['animationData'],'serializedAutoPlay':anim['autoPlayAnimation'],'runtimeAnimation':'run','runtimeLoop':True,'run':run,'runtimeLocalScale':[0.052000001072883606]*3,'runtimeOrientation':{'flipCondition':'soldierType<=3 && start.x<end.x','flippedLocalEuler':'(-battleCamera.transform.localEulerAngles.x,-180,0)','normalLocalEuler':'(battleCamera.transform.localEulerAngles.x,0,0)'},'runtimeSortingLayer':'Default','runtimeSortingOrder':-5,'runtimeCampColorIndex':'max(camp,1)-1','runtimeCampColors':colors,'shaderDescriptor':'generated/presentation-prepared/soldier-shader.json','animationPropertyBlock':{'_AnimLoop':1,'_AnimTime':['startFrame','lengthFrames','1/max(lengthSeconds,.01)','animationStartTime']}}
    write(OUT/(name+'.json'),descriptor);soldiers.append(descriptor)
rules=[]
def rule(id,claim,functions=(),**values):rules.append({'id':id,'status':'confirmed-static','claim':claim,'sources':[source(f) for f in functions],**values})
rule('soldier-scale-orientation','Default type1/2/3 model uniform localScale .052; flip is localEuler rotation, not negative scale. Camera angle is localEulerAngles.x.',[10312,7127],scale=0.052000001072883606,largeTypeScale=0.01600000075995922)
rule('soldier-run-selection','InitGameObject calls PlayAni(false), selecting run with loop=true. Death selects dead with loop=false. Serialized idle does not define moving gameplay animation.',[15869,7125])
rule('soldier-animation-properties','SpineAnimator sets _AnimLoop and _AnimTime=(startFrame,lengthFrames,1/max(lengthSeconds,.01),startTime) through Renderer property block; isNeedFix chooses clip mesh, otherwise cached original mesh.',[15842,7108])
rule('soldier-animation-clock','Cached GraphicsSettings.defaultRenderPipeline != null selects Time.time; otherwise Time.timeSinceLevelLoad. Both are scaled clocks.',[4255,7107])
rule('soldier-render-state','Material custom queue -1 uses shader Transparent queue 3000. Normal pass Blend One OneMinusSrcAlpha, ZWrite Off, ZTest LEqual, Cull Off. Original raw pass states retained.',shaderDescriptor='generated/presentation-prepared/soldier-shader.json')
rule('soldier-runtime-color-sort','SetSoldierColor overrides _Color using ColorHelper soldier palette at max(camp,1)-1; runtime renderer sortingLayerName Default and sortingOrder -5 override serialized order0.',[5663,10316,12875,20214,11448,18723],palette=colors)
rule('soldier-texture-sampling','All three AnimTex: RGBA32, bilinear, clampUVW, no mip, linear. MainTex: original format retained in descriptors, bilinear, repeatUVW, no mip, sRGB. Shader samples animation texture LOD0 and uv1.x vertex index.')
rule('soldier-frame-edge','Use original start/length without truncating to available rows. Run100 start299 length37 in textureHeight334; run200 start506 length37 in height541; V clamp preserves original edge behavior.',runs=[{'name':s['name'],'run':s['run'],'animTextureHeight':next(t['height'] for t in s['textures'] if t['property']=='_AnimTex')} for s in soldiers])
projection={'worldPoint':'savedBaseWorld + Vector3.up*(.1+(DispatchConfig.maxLine-1)*.05)','gradeHeights':[.1,.15,.2],'screenPoint':'battleCamera.WorldToScreenPoint(worldPoint)','localPoint':'RectTransformUtility.ScreenPointToLocalPointInRectangle(towerRootRect.parent.GetComponent<RectTransform>(),screenPoint,UIControl.root.parent.GetComponent<Canvas>().worldCamera,out localPoint)','assignment':'towerRootRect.anchoredPosition=localPoint','scoreChildAnchoredPosition':[0,85.5999984741211],'scoreChildSize':[100,42.85266876220703],'scoreFontSize':40,'scoreBestFit':[10,40],'scoreAlignment':'MiddleCenter','dispatchSource':'generated/dispatch-model.json','hudSource':'generated/hud-evidence.json'}
write(OUT/'tower-score-projection.json',projection)
rule('tower-score-projection','RefreshPos adds grade height .1/.15/.2 in WORLD up, projects through battle camera, converts screen to tower parent RectTransform with UI canvas camera, writes root anchoredPosition. Score child retains original anchored offset.',[10060],descriptor='generated/presentation-prepared/tower-score-projection.json')
evidence['rules']=rules
evidence['unknowns']=[{'id':'live-render-baseline','detail':'No accepted original live screenshot/GPU capture. Static descriptors are preparation, not visual acceptance.'},{'id':'runtime-camera-and-adaptation','detail':'Camera/platform canvas adaptive runtime selection not fully audited here. Projection formula is exact; final current-device pixel placement requires camera/adaptation evidence.'},{'id':'saved-skin-selection','detail':'100/200/300 are default controlled-fixture skins. Actual live account selected skins require authorized state evidence; +3000 special-mode assets are outside this bounded audit.'},{'id':'texture-import-roundtrip','detail':'PNG exports use image-space orientation; native texture raw JSON retained. Unity importer must validate raw-row/PNG vertical orientation and linear sampling by a known frame comparison.'}]
evidence['preparedDescriptors']=[str(p.relative_to(ROOT)).replace('\\','/') for p in sorted(OUT.glob('*.json'))]
evidence['helperIdentities']={'1200':'Transform.set_localScale','3504':'Transform.get_localEulerAngles','3253':'Transform.set_localEulerAngles','1264':'Time.get_time','5605':'Time.get_timeSinceLevelLoad','7772':'GraphicsSettings.get_defaultRenderPipeline','1130':'Transform.get_parent','7555':'MeshFilter.set_mesh','3847':'Renderer.GetPropertyBlock','3581':'MaterialPropertyBlock.SetFloat','5896':'MaterialPropertyBlock.SetVector','2926':'Renderer.SetPropertyBlock','946':'Object.op_Inequality','3846':'Renderer.set_sortingLayerName','2784':'Renderer.set_sortingOrder','11425':'MaterialPropertyBlock.SetColor'}
evidence['dependencyCorrection']={'source':'generated/resource-snapshots/hud-soldier300-20260928/asset-evidence-incremental.json','detail':'Incremental report rebuilds references from saved JSON to include tuple-based Material texture and SpriteAtlas maps; baseline files untouched.'}
write(GEN/'presentation-binding-evidence.json',evidence)
runtime=[]
for s in soldiers:
    anim=next(t for t in s['textures'] if t['property']=='_AnimTex')
    main=next(t for t in s['textures'] if t['property']=='_MainTex')
    raw=read(ROOT/anim['resource']['outputs']['textureRaw'])['image data']['bytesHex']
    pixels=bytes.fromhex(raw)
    assert len(pixels)==anim['width']*anim['height']*4
    rawpath=OUT/(s['name']+'.anim.rgba32')
    rawpath.write_bytes(pixels)
    vals=dict(s['materialSavedValues']['m_SavedProperties']['m_Colors'])
    runtime.append({'name':s['name'],'prefabId':int(s['name'].split('_')[1])*10,'soldierType':int(s['name'].split('_')[1])//100,'meshPath':s['mesh']['outputs']['meshNative'],'meshSourceId':s['mesh']['id'],'animTexturePath':str(rawpath.relative_to(ROOT)).replace('\\','/'),'animTextureSha256':hashlib.sha256(pixels).hexdigest(),'animTextureEncoding':'original Texture2D image data RGBA32; LoadRawTextureData, no Y flip','animWidth':anim['width'],'animHeight':anim['height'],'animLinear':True,'animFilter':1,'animWrap':1,'mainTexturePath':main['resource']['outputs']['png'],'mainWidth':main['width'],'mainHeight':main['height'],'mainSRGB':True,'mainFilter':1,'mainWrap':0,'mipmaps':False,'animAdd':[vals['_AnimAdd'][k] for k in ('r','g','b','a')],'animMul':[vals['_AnimMul'][k] for k in ('r','g','b','a')],'runStart':s['run']['startFrame'],'runFrames':s['run']['lengthFrames'],'runSeconds':s['run']['lengthSeconds'],'deadStart':s['animationData'][0]['startFrame'],'deadFrames':s['animationData'][0]['lengthFrames'],'deadSeconds':s['animationData'][0]['lengthSeconds'],'scale':s['runtimeLocalScale'][0],'sortingOrder':-5,'renderQueue':3000})
write(OUT/'runtime-data.json',{'pathRoot':'target directory containing generated/','soldiers':runtime,'campColors':colors,'orientation':'if type<=3 and start.x<end.x: localEuler=(-cameraLocalX,-180,0); else (cameraLocalX,0,0)','animationClock':'defaultRenderPipeline != null ? Time.time : Time.timeSinceLevelLoad','shaderSource':shader_descriptor['glesSource'],'shaderDescriptor':'generated/presentation-prepared/soldier-shader.json'})
meshes=[]
for s in soldiers:
    nativePath=ROOT/s['mesh']['outputs']['meshNative'];native=read(nativePath)
    dest={'name':s['name'],'sourcePath':s['mesh']['outputs']['meshNative'],'sourceId':s['mesh']['id'],'sourceSha256':hashlib.sha256(nativePath.read_bytes()).hexdigest(),'coordinateSpace':native['coordinateSpace']}
    for key,axes in (('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')):
        values=native.get(key)
        dest[key]=[{a:value for a,value in zip(axes,row)} for row in values] if values is not None else []
        dest['has'+key[0].upper()+key[1:]]=values is not None
        assert values is None or [[row[a] for a in axes] for row in dest[key]]==values
    dest['submeshes']=[{'indices':[i for triangle in triangles for i in triangle]} for triangles in native['submeshTriangles']]
    for original,sub in zip(native['submeshTriangles'],dest['submeshes']):
        assert [sub['indices'][i:i+3] for i in range(0,len(sub['indices']),3)]==original
        assert all(0<=i<len(dest['vertices']) for i in sub['indices'])
    assert not native.get('boneIndices') and not native.get('boneWeights') and not native.get('bindPose'), 'Unexpected skeletal data needs explicit schema'
    meshes.append(dest)
meshpath=OUT/'soldier-meshes-runtime.json'
write(meshpath,{'meshes':meshes})
write(OUT/'soldier-meshes-validation.json',{'passed':True,'outputSha256':hashlib.sha256(meshpath.read_bytes()).hexdigest(),'checks':'Every vector channel reconstructed component-for-component; every triangle/index/submesh order identical; no coordinate/winding conversion; missing channels remain absent, no fabricated normals/colors.','meshes':[{'name':m['name'],'vertices':len(m['vertices']),'submeshes':len(m['submeshes']),'indexCount':sum(len(s['indices']) for s in m['submeshes']),'hasColors':m['hasColors'],'hasNormals':m['hasNormals'],'sourceSha256':m['sourceSha256']} for m in meshes]})
evidence['preparedDescriptors']=[str(p.relative_to(ROOT)).replace('\\','/') for p in sorted(OUT.glob('*.json'))]
write(GEN/'presentation-binding-evidence.json',evidence)
files=list(OUT.glob('*.json'))
assert len(soldiers)==3 and all(len(s['textures'])==2 for s in soldiers)
for s in soldiers:
    assert 'meshNative' in s['mesh']['outputs']
    assert next(t for t in s['textures'] if t['property']=='_AnimTex')['width']>0
write(OUT/'validation.json',{'passed':True,'descriptorCount':len(files),'soldiers':3,'textureBindings':6,'rules':len(rules),'unknowns':len(evidence['unknowns']),'validation':'Resolved every mesh/material/texture, hashed existing outputs, preserved exact clip values and original shader pass state.'})
print(json.dumps({'rules':len(rules),'soldiers':3,'palette':colors,'passed':True},ensure_ascii=False))

