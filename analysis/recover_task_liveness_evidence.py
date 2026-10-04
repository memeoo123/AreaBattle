"""Publish reviewed liveness preview/tier methods and original native animation provenance."""
from pathlib import Path
import argparse,hashlib,json,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
target=c['p'];out=target/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(33242,33256))|{33877,33880,33881,33882,33883}|set(range(33887,33892))
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata']not in selected:continue
 dest=out/row['path'];data=(stage/row['path']).read_bytes();ending=b'\r\n'if dest.exists()and b'\r\n'in dest.read_bytes()else b'\n';dest.write_bytes(ending.join(line.rstrip()for line in data.splitlines())+ending)
 if row['metadata']not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((27313,27428,27434,27443,22716,22718))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4350,4349,4414,4415,4416,3942):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4090196,4090200,4090192,4098248,4098252,4090284,4057420,4063596,4101024,4030860,4030864,4016604,3986048,4001088,3990392,3990424,3990648,3941916,3993272,3993068):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
animation=json.loads((out/'task-panel-animations.json').read_text());assert animation['validation']==dict(allReferencesResolved=True,animationCount=6,clipCount=2,curveCount=18,keyCount=119)
for clip in animation['clips']:assert hashlib.sha256((target/clip['sourceJson']).read_bytes()).hexdigest()==clip['sourceSha256']
imported=json.loads((validation/'analysis/task-panel-animation-import-report.json').read_text());assert imported['passed']and len(imported['unboundSourcePaths'])==3
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 animation=dict(manifest='task-panel-animations.json',sha256=hashlib.sha256((out/'task-panel-animations.json').read_bytes()).hexdigest(),validation=animation['validation'],unboundOriginalTracks=imported['unboundSourcePaths'],sourceAcquisition='generated/resource-snapshots/task-panel-ui-20261003/acquisition-manifest.json'),
 findings=[
  'LivenessPreviewItem4350 constructor allocates one reused WaitForEndOfFrame. UIObject initialization invokes SetVisible(true)/VisibleBefore before Awake; Awake captures original IconBg/root/name rect width, directly SetActive(false), and installs root click and IconBg detail handlers. Visible remains true and initial update handle remains registered. No visibility idempotence guard; each SetVisible adds/removes same update delegate before native active/scale changes.',
  'SetData publishes legacy GameItemConfig then snapshots ItemIcon offset28 (not icon20), atlasName24, Lang name32 and Int32 quantity. Null input is assigned before dereference. Root click invokes optional onClick only; IconBg click requests detail using its RectTransform and current Data.id. Selection uses managed reference check, exact touch/IconBg names or case-sensitive Node substring; otherwise SetVisible(false).',
  'Refresh async requests sprite with size=false, localizes current Name, writes x{amount} BEFORE awaiting reused frame instruction. Reentry can change later name/count. After await Unity destroyed/null root exits; otherwise current name rect width minus retained width drives grow-only background halves(+delta/2 each) and root(+delta), updating retained width BEFORE sizes. Concurrent refreshes share width and are not cancelled/coalesced. Dispose destroys/clears base ownership, then onClick, retaining data and update registration as original.',
  'TaskPanelUI33877 captures daily list and liveness then iterates original ProgressBg child1 nodes. Captured item threshold formats with IntToString(value,false); state toggles original closed/open children. Eligible state0 removes runtime click listeners, attaches async claim, enables glow and assigns Canvas sorting layer from current layer. Ineligible only adds preview on initial=true. Source does not clear old ready callbacks/glow or re-enable disabled buttons for state1/later refresh.',
  'Preview closure first activates preview native root, sets visible, sets world position to captured tier node, then reads first configured reward and dictionary-indexes legacy item config. Missing config therefore leaves prior preview data after new visibility/position. First reward only is previewed.',
  'RefreshPointDailyLivenessBar invokes required daily red-dot method before reading current tier list/liveness; only when value>=minimum threshold calls Refresh(false). Full TaskPanelUI red-dot controller and generic singleton owner remain pending required production endpoints.',
  'Tier claim captures first reward id, GoodsType, lowInt32 count before hiding glow, requesting EffectModule.Show(1016,node), storing returned handle and playing first active child native Animation. It then awaits a NEW WaitForEndOfFrame per invocation. Gold/diamond effect uses captured count, null root/current world position, applyInventory=false and top-refresh callback; other reward opens award display quantity1. It disables Button only AFTER effect/award display, calls current DailyTaskSubUI.ClaimLiveness with live captured item.id, then daily red-dot refresh.',
  'No source eligibility/repeat guard exists inside async claim. Two pending callbacks can both award through original unguarded liveness strategy; validation preserves this behavior instead of inventing deduplication. Effect/wait/fly failure stops before disable/award, while post-award red failure preserves economic/state prefix. Current effect close33880 is restored as helper, clearing handle only after successful Close; the rest of33880 tab-content switching remains full-page work.',
  'Original native Animation fields were read directly from verified source bundle because baseline static exporter omitted native Animation schema. Six players share two legacy clips: three manual1.7s hdzd_eff_jsbx_ani and three auto1.5s hdzd_eff_taskNode. All18 source curves/119 keys including tangents/weights preserved and verified in editor. Source box clip targets absent hdzd_eff_bxGlow three times; unbound original tracks retained, no invented nodes. ParticleSystems and EffectModule1016 asset/runtime still pending.',
  'Fresh native validation verifies real EventSystem selection, source pointer callbacks/update visibility, actual end-of-frame continuation at timeScale0 while original box animation remains paused, restored animation completion after resume, real inventory/mask auto-save and independent restart. Full TaskPanelUI lifecycle/tabs/entry and original audiovisual comparison are not included.'
 ],remaining=['Full TaskPanelUI/TaskSingleton/Daily and Achievement tab ownership, actual task controller/red-dot predicates, main task entry, concrete Achievement runtime.','Required Effect1016/fly/popup/sprite/language/report host assembly, original particles and matched visual/audio session; complete Main/account/platform/all remaining19 controllers/business and final Player.'])
(out/'TASK_LIVENESS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index),animation=animation['validation'])))
