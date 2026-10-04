"""Publish reviewed day-item and shared list selection source contracts."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
target=c['p'];out=target/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index};selected=set(range(33160,33173))
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((30048,34647,34657,34658,34659))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
generic=json.loads((out/'dynamic-selection-generic.json').read_text());assert len(generic['methods'])==11
for row in generic['methods']:row['sha256']=hashlib.sha256((out/row['path']).read_bytes()).hexdigest()
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')]
fields=[]
for owner in (4338,4552,4553):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
usages=[]
for address in (3933852,3933376,3962732,3962740,3962744,3988816,3988864,3919612,4084192,4092912,4084244,4092916,4095388,4084164,4092908,4049064,4049060,3992880,3992884,3992888,4048968,4101900,4056000):
 v=u(address);kind=v>>29;idx=(v&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
asset=json.loads((out/'limit-task-ui-import.json').read_text());page=next(p for p in asset['prefabs']if p['name']=='CommonLimitTimeTaskUI')
day=next(b for b in page['bindings']if b['name']=='dynamicList'and b['owner']=='');tasks=next(b for b in page['bindings']if b['name']=='Content'and b['owner']=='')
assert day['path']=='middleArea/Scroll View__1700397003357398003/Viewport/dynamicList'
assert tasks['path']=='middleArea/Scroll View/Viewport/Content'
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,generic=generic,fields=fields,usages=usages,
 originalDayList=dict(node=day['path'],component=next(c for n in page['nodes']if n['path']==day['path']for c in n['components']if c['className']=='DynamicList'),taskOutlet=tasks['path']),
 findings=[
  'Selection4554 uses data-owned ISelectData.Selected and creates nested renderer4553 with both base provider and typed selection provider. Negative SetSelect index returns before any dereference; positive index goes through GetData before looking for an existing visible renderer. Offscreen data receives its flag without callback.',
  'Selected visible renderer writes true before OnSelect only on transition. Provider then captures count after callback and walks live data; selected rows are deselected only when target.GetHashCode()!=row.GetHashCode(), not reference identity or Equals. Hash collisions can retain multiple selected rows. Deselect writes false before callback; failures preserve prior state. KillSelect ignores hash and visits all currently selected rows within captured count.',
  'Optional centering occurs only after the scan and preserves passed duration. Page button calls compiler wrapper table117964/function12477, which supplies float0 duration to shared SetSelect body12476; the page passes center=false.',
  'Renderer Refresh first invokes base item.OnRenderer(current index), rereads Region, then gets current data and unconditionally invokes OnSelect if selected. It does not invoke OnDeSelect for an unselected render. Item setter caches IDynamicRenderSelect via interface cast; a missing receiver fails only when a selected callback is attempted.',
  'Page4338 OnCreate casts provider. Awake removes all runtime normal-button listeners then adds own; removes and adds RefreshList/Ext callbacks before resolving current actual parent1301001. Initialize binds normal Button, selected/locked Image, three text fields and red object in source order.',
  'OnRenderer publishes data then current child by activityId, invokes localization(pageName,day), compares reread day against child ext.dayId, updates three graphics, then overwrites all names with original literal {0}天 and queries Child.ExitRewardWaitGet. Localization side effects and exceptions are observable despite its text later being overwritten.',
  'OnSelect sets selected/normal/locked graphics, sends MsgDispatcher SelectPage(activityId,day), then hides its native root when live provider count==1. OnDeSelect unconditionally shows normal regardless of lock; later rendering restores day gate. No explicit dirty write or unlock recheck.',
  'RefreshList requires data/non-null args/length>=1 but then reads and unboxes args1 before comparing args0. A one-element array or wrong second type fails even for another activityId. ExtRefresh requires only first exact Int32. Matching callbacks update red only.',
  'Page Dispose follows DynamicBaseItem: clear owner fields and mark disposed without destroying native row, remove both common listeners, remove all runtime normal-button listeners. It retains child/data/button/rect references; owning DynamicList/page handles native lifetime.',
  'Original page has two siblings named Scroll View. Preparation now maps outlet target object IDs to the same disambiguated paths used for imported nodes. Runtime outlet adapter resolves original sibling topology before yielding bindings, preserving original names. Day binding restores original component6678087315481976053 spacing(0,30), leading30, single fixed column, non-recycling, non-normal-list mode.'
 ],remaining=['Task rows, accumulator/preview and full CommonLimitTimeTaskUI lifecycle/claims still pending; day binding alone does not implement page business.','Production seven-day menu, Main/account/platform, remaining19 controllers, all business/rewards/return, Player and original audiovisual acceptance remain required.'])
(out/'LIMIT_TASK_DAYS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),genericMethods=len(generic['methods']),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
