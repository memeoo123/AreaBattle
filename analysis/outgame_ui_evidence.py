"""Recover local HUD prefab hierarchy and serialized binding data, without target execution."""
import json,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
base=Path('analysis/asset_focus.py').read_text(encoding='utf8')
ctx={'__file__':str(Path('analysis/asset_focus.py').resolve())}
exec(compile(base.split('roots=[]')[0],'asset_read_only', 'exec'),ctx)
ROOT,objs,hierarchy,report,tree=[ctx[k] for k in ('ROOT','objs','hierarchy','report','tree')]
out=[]
for c in report['containers']:
    if not c['assetPath'].endswith('.prefab') or not any(s in c['assetPath'].lower() for s in ('mainmenu/menutabui','mainmenu/proj_xqzdstartui','mainmenu/commanderui','mainmenu/shopui')):continue
    node=hierarchy(objs[c['object']]);nodes=[];byid={}
    def walk(n,parent=''):
        path=parent+'/'+n['name'];byid[n['id']]=path
        components=[]
        for comp in n['components']:
            byid[comp['id']]=path
            typ=(comp.get('script')or{}).get('m_ClassName',comp['type'])
            if typ in ('Transform','RectTransform'):continue
            components.append({'id':comp['id'],'class':typ,'partial':comp.get('schemaPartial',False),'data':comp['data'],'references':comp['references']})
        nodes.append({'path':path,'object':n['id'],'active':n['active'],'layer':n['layer'],'transform':{k:v for k,v in n.get('transform',{}).items() if not k.startswith(('m_GameObject','m_Children','m_Father'))},'components':components})
        for ch in n['children']:walk(ch,path)
    walk(node)
    bindings=[]
    for n in nodes:
        for comp in n['components']:
            if comp['class']=='UIOutlet':
                for i,x in enumerate(comp['data'].get('OutletInfos',[])):
                    ref=next((r for r in comp['references'] if r['property']==f'.OutletInfos[{i}].Object'),None)
                    ident=ref.get('target') if ref else n['object'].split(':')[0]+':'+str(x['Object']['m_PathID'])
                    bindings.append({'name':x['Name'],'componentType':x['ComponentType'],'object':ident,'path':byid.get(ident),'outletOwner':n['path']})
    out.append({'assetPath':c['assetPath'],'root':c['object'],'nodes':nodes,'outletBindings':bindings})
p=ROOT/'generated/outgame/ui-evidence.json';r=json.loads(p.read_text(encoding='utf8')) if p.exists() else {}
r.update(target={'appId':'wxcf1394487200e48f','version':'43'},mode='static-only',prefabs=out)
r.setdefault('rules',[]);r.setdefault('unknowns',[])
p.write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
print([(p['assetPath'],len(p['nodes']),len(p['outletBindings'])) for p in out])
