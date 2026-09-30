from pathlib import Path
import json,hashlib,datetime,struct
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';O=T/'generated/outgame'
h=W/'analysis/recover_outgame_manager_registry.py';n={'__file__':str(h)};exec(h.read_text().split('rows=[]')[0],n)
u,b,pairs,ts,ms=n['u'],n['b'],n['pairs'],n['ts'],n['ms'];mm=json.loads((O/'method-map.json').read_text(encoding='utf8'))
ids={31515,31472,31478,31531,31469,31474,31475,31481,31492,31493,31498,31501,31510,31511,31519,31521,31523}
methods=[dict(m,sha256=hashlib.sha256((O/m['path']).read_bytes()).hexdigest()) for m in mm if m['metadata'] in ids or m['path'].startswith('disassembly/Type4221-')]
fields=[]
for i in [4107,4221,4225]:
 t=ts[i]
 for j in range(t[18]):
  f=struct.unpack_from('<3i',b,pairs[11][0]+12*(t[8]+j));fields.append({'typeIndex':i,'class':ms(t[0]),'field':ms(f[0]),'type':f[1],'offset':u(u(3823136+4*i)+4*j)})
external=[]
for relative in ['generated/combat-disassembly/Raw-2673.txt','generated/combat-disassembly/Raw-10954.txt']:
 p=T/relative;external.append({'path':relative,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'meaning':'RandomHelper.Random<T>(List<T>) -> shared System.Random.Next(count), then list[index]'})
d={'status':'source-evidence-for-reset-pools-effect-collection-queries-scene-start','atUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'methods':methods,'fields':fields,'externalGenericEvidence':external,'rules':[
 {'id':'reset-order','metadata':[31515],'detail':'Boss=null; hide each obstacle then clear list; flags98/96/97=false; active camps.Clear; active towers.Clear; all soldiers.Clear(true) then Effects.Destory; WayLine field168=0. Retains pools, level tuple, play state, other flags/timers/scores; callback failures retain prefix writes.'},
 {'id':'pool-selection','metadata':[31472,31478,31531],'detail':'Soldier: !active&&field112. Boss: first inactive Boss. Normal type4: first inactive nonBoss Arrow; other types: first inactive nonBoss nonArrow. Otherwise instantiate and append; factory callbacks still explicit until full concrete objects are restored.'},
 {'id':'effect-collection','class':4221,'detail':'SetEntity searches named child only for childCount>=2. Add uses Dictionary.Add; Display resolves live module before key access. Spawn existing key reactivates, else Show(effectId,zero,parent,false,option). Remove closes before deleting. Clear gated by native Root existence; close handles before dictionary clear before reverse child Destroy. Destory additionally destroys root and clears held root, retains entity. Native Destroy is deferred.'},
 {'id':'query-buffer-sharing','metadata':[31481,31493,31510,31511],'detail':'Same-camp methods reuse field76, nonArrow excludes only ArrowTower (usage3928280 ->type4211). Other/enemy reuse field80; Other includes neutral, Enemy excludes0. All foreach/active/native source order and partial results retained.'},
 {'id':'native-object-lookup','metadata':[31521,31475],'detail':'First active Tower with Unity Object equality at field16; no extra nonnull guard. GetBossObj gets actual pooled native object, null diagnostic then null; success resets localScale=one only.'},
 {'id':'range','metadata':[31519,31523],'detail':'Fresh list each call. Active tower with sameCamp==(CampId==camp). Squared xyz distance to stored Tower.field68. Skip if squared>range*range OR sqrt(minimum)>squared, preserving original asymmetric formula and NaN acceptance. One result uses shared RandomHelper.Next(count) only if count>=1; singleton consumes random too.'},
 {'id':'speed-refresh','metadata':[31469,31492,31498],'detail':'Refresh setter only writes flag44=true (timer untouched); speed setter writes field104 then Unity Time.timeScale. Getter field104.'},
 {'id':'scene-start','metadata':[31474,31501],'detail':'LoadCurrentLevel calls LoadLevel(CurLevel) then InitSpecialScene. Capture specialfield24 before resolving GameControl. Exactly1 uses FestRewardSelection.GetAwardId(5), every other value uses SkinManager.GetUserSkinId(4), then actual GameControl.LoadGameScene. No fallback/default fabricated.'}
]}
(O/'LEVEL_REUSE_QUERY_SOURCE_EVIDENCE.json').write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print({'methods':len(methods),'fields':len(fields),'methodMap':len(mm)})
