"""Derive reconstruction-friendly prefab/dependency evidence from local exports."""
import collections,json,re
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
OUT=ROOT/'generated/unity-assets'
report=json.loads((ROOT/'generated/asset-evidence.json').read_text(encoding='utf8'))
objs={o['id']:o for o in report['objects']}
def tree(o):
    p=o.get('outputs',{}).get('typetree')
    return json.loads((ROOT/p).read_text(encoding='utf8')) if p else {}
def target(o,prop):
    r=next((r for r in o.get('references',[]) if r['property']==prop),None)
    return objs.get(r['target']) if r else None
def hierarchy(go):
    cs=[objs[r['target']] for r in go.get('references',[]) if r['property'].startswith('.m_Component') and r['target'] in objs]
    tf=next((c for c in cs if c['type'] in ('Transform','RectTransform')),None)
    node={'id':go['id'],'name':go['name'],'active':tree(go).get('m_IsActive'),'layer':tree(go).get('m_Layer'),'components':[],'children':[]}
    for c in cs:
        data=tree(c); item={'id':c['id'],'type':c['type'],'data':data,'references':c.get('references',[])}
        if c['type']=='MonoBehaviour':
            script=target(c,'.m_Script');item['script']=tree(script) if script else None
            item['schemaPartial']=c.get('typetreePartial',False)
        node['components'].append(item)
    if tf:
        node['transform']=tree(tf)
        for r in tf.get('references',[]):
            if r['property'].startswith('.m_Children') and r['target'] in objs:
                child=target(objs[r['target']],'.m_GameObject')
                if child:node['children'].append(hierarchy(child))
    return node
def walk_refs(ids):
    found=set();pending=list(ids)
    while pending:
        k=pending.pop()
        if k in found or k not in objs:continue
        found.add(k);o=objs[k]
        pending.extend(r['target'] for r in o.get('references',[]) if r.get('target'))
        if o.get('resolvedAtlasByRenderDataKey'):pending.append(o['resolvedAtlasByRenderDataKey'])
    return sorted(found)
roots=[]
for c in report['containers']:
    if c['assetPath'].endswith('.prefab') and re.search('bastion|soldiercommon/|scene/gamescene/|wayline/|proj_xqzdplayui|proj_xqzdoverui|/hit.prefab|/levelup.prefab',c['assetPath']):
        go=objs[c['object']];node=hierarchy(go);deps=walk_refs([go['id']]);dest=OUT/'prefabs'/(go['name']+'.hierarchy.json');dest.parent.mkdir(exist_ok=True)
        data={'assetPath':c['assetPath'],'root':node,'dependencies':[objs[k] for k in deps]}
        dest.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
        roots.append({'assetPath':c['assetPath'],'root':go['id'],'output':dest.relative_to(ROOT).as_posix(),'dependencyCount':len(deps),'unresolved':[{'owner':k,**r} for k in deps for r in objs[k].get('references',[]) if not r['resolved']]})
atlas=next(o for o in objs.values() if o['type']=='SpriteAtlas' and o['name']=='TowerUI')
towers=[]
for r in atlas['references']:
    if r['property'].startswith('.m_PackedSprites') and r['target'] in objs:
        s=objs[r['target']];t=tree(s)
        towers.append({'name':s['name'],'object':s['id'],'png':s['outputs'].get('pngCanvas',s['outputs'].get('png')),'trimmedPng':s['outputs'].get('png'),'trimOffset':s.get('trimOffset'),'rect':t.get('m_Rect'),'pivot':t.get('m_Pivot'),'pixelsToUnits':t.get('m_PixelsToUnits'),'border':t.get('m_Border')})
animations=[]
for r in roots:
    if '/soldiercommon/' not in r['assetPath']:continue
    d=json.loads((ROOT/r['output']).read_text(encoding='utf8'))
    for c in d['root']['components']:
        if c.get('script',{} ) and c['script'].get('m_ClassName')=='SpineAnimator':
            animations.append({'assetPath':r['assetPath'],'script':c['script'],'animations':c['data'].get('animationData'),'autoplay':c['data'].get('autoPlayAnimation')})
manifest={'prefabs':roots,'towerSprites':towers,'soldierAnimations':animations,'meshCoordinateSpace':'Unity native in meshNative outputs; OBJ mirrors X.','sprites':'PNG exported through exact atlas renderDataKey including packing rotations and tight mesh; original rect/pivot/PPU/border remains in typetree.','animationEvidence':'generated/unity-assets/ShaderEvidence/Spine_SkeletonMeshBaker.platform0.segment0.strings.txt','camps':json.loads((ROOT/'generated/tables/CampConfig.json').read_text(encoding='utf8'))}
(OUT/'reconstruction-index.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'prefabs':roots},ensure_ascii=False))
