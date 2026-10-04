"""Retain concrete limit-task activity source and callback/report field evidence."""
from pathlib import Path
import argparse,hashlib,json,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);stage=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=stage/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
selected=set(range(35512,35555));index=read(out/'method-map.json');known={r['metadata'] for r in index};methods=[];added=[]
for row in read(source/'method-map.json'):
 if row['metadata'] not in selected:continue
 dest=out/row['path'];crlf=dest.exists() and b'\r\n' in dest.read_bytes();text='\n'.join(line.rstrip() for line in (source/row['path']).read_text().splitlines())+'\n';dest.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
 methods.append(dict(row,sha256=hashlib.sha256(dest.read_bytes()).hexdigest()))
 if row['metadata'] not in known:index.append(row);known.add(row['metadata']);added.append(row['metadata'])
assert {r['metadata'] for r in methods}==selected
write(out/'method-map.json',index)
b,mem,ts,md,ms,u,pairs=[c[x] for x in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (3252,3253,3256,4697,4698,4699):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3992456,3992460,3992464,4011360,4011352,4011340,3954620,3956052,3987632,3965936,3965940):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind)
 if kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row['spec']=list(spec);method=md[spec[0]]
 elif kind==3:row['metadata']=idx;method=md[idx]
 else:raise AssertionError((address,kind))
 row.update(sourceClass=ms(ts[method[1]][0]),method=ms(method[0]));usages.append(row)
literals=[]
for address in (4105012,4086280,4049064,4082380,4104328,4105212,4049060,4086276,4102820,4106280,4089704,4106284,4105168,4105552,4102872,4048980,4105216,4105236,4034796,4102724):
 encoded=u(address);assert encoded>>29==5;idx=(encoded&0x1ffffffe)>>1;n,off=struct.unpack_from('<II',b,pairs[0][0]+8*idx)
 literals.append(dict(address=address,value=b[pairs[1][0]+off:pairs[1][0]+off+n].decode()))
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,methodUsages=usages,literals=literals,
 findings=[
  'Parent4699 caches nonnull pool manager4711, binds manager.Activity, registers statistic10021 and resets generic children using actual NoviceActivities/manager index. Update saves only when Dirty and pool enables save, clears Dirty after success; OnSave traverses current Children. Source35551 uses parent ActivityId for child lookup, not argument/100, and argument exact boxed Int32 modulo100 for day. This original missing-child failure is retained.',
  'Child4698 initialization retains sorted day dictionary, clears views, reuses persisted task objects, diagnoses but retains unknown saved IDs, then creates missing configured tasks without appending to saved list. Config owner is read on the terminal saved loop iteration. Accumulator flags decode64-bit positions in config insertion order; display list shares objects then sorts. Initialization sends current ext.dayId once per group, not the group key, then log, RemoveListeners, AddListeners and ext refresh.',
  'SaveModule builds seen set from existing saved IDs, appends runtime entries only when a condition has nonzero value or task state is nonzero. State test is inside condition loop, so zero-condition claimed tasks are not appended. The seen set is not updated during append; duplicate unsaved runtime IDs can append twice. Existing persisted records are never pruned here.',
  'Listeners are added only while message-key list is empty, deduplicated from configured condition first values; time10000 and liveness are then registered. Removal traverses recorded condition keys without deduplication, removes one time listener, clears key list then removes liveness. Changed condition keys can leave old listeners; tasks with no conditions permit duplicate time/liveness registration.',
  'Statistics callback converts final key/filter with Convert.ToInt32, but delta must unbox Int64. Local records require Data.state3, lifetime records use GameValue. Exactly3 args enables first-argument filter; other lengths use empty condition args. Values clamp to zero only in event handler. Touched tasks/days trigger sorting and notifications. Callback catches Exception and reports Message+CRLF+StackTrace while preserving prefix mutations.',
  'ChangeTaskProgress matches key and full recorded argument array, assigns value before reporting, tests threshold using conditionParams[i].datas[conditionParams[0].datas.Length-1]. Reports only while old progress is below that threshold and changed. Report slots0..2 map directly, later indices map0. Dirty is set even when no matching condition; failures before final dirty retain mutated progress.',
  'Launch holds ext, sets day1, samples time10000, records keyed launch29000 using reread module ext time, dirties parent. RefreshDay uses source8am timestamp conversion against UTC-midnight epoch and calendar-day difference, upper-clamps only to greatest configured day, sends ext notification before dirty. Empty task groups diagnose only after nonzero launch. Clock rollback can produce negative day.',
  'DayComplete ignores hidden tasks and accepts any nonzero visible state, without progress/day gating. Navigation scans integer days from minimum through ext.dayId and returns first BtnState0, otherwise lastClickDayId; missing intermediate days and empty dictionaries preserve source failures. Reset clears saved tasks/progress/state/dayFlag and sets day1, but retains accFlag/launchTime/lastClickDayId/listeners; rebuilds accumulator flags, sends group keys then sorts accumulator view and ext/dirty.',
  'TaskComplete awards via actual global AddRewardsByItemSelf, builds and expends configured costs even when empty, then marks state1 and ORs sign-extended Int32 bit( id-1 ) into Int64 dayFlag. It increments statistic10021 except a single10021-condition task, then reward/day/all reports, optional count-progress report, task sort/list refresh and ext/dirty. No rollback or invented affordability/duplicate-transaction guard.',
  'Accumulator claim computes insertion-order position before award, marks state1 after award, ORs sign-extended Int32 bit(position-1) into Int64 accFlag, then reports and notifies/dirties. Positions32/33 expose original sign-extension/32-bit aliasing versus64-bit decode. Liveness handler unboxes Int32, reports derived old and old+delta before ext/dirty; it stores no separate progress and actual factory event arrives before task claimed state.',
  'Reports use required current host creation/delivery. Success DTO3252: parent12/name16/detailType20/detailId24, new nullable32/40/48 and old nullable56/64/72. Reward3253 uses parent12/name16/detailType20/detailId24/nullable32/40/48. Complete3256 uses parent12/name20/detailType28/detailId32. Other source DTO fields remain host-owned. Localized common activity name and business NoviceActivities name have distinct source routes; literal liveness label is 活跃度.'
 ],boundaries=['Runtime binding installs only recovered LimitTimeTaskActivity and NoviceTaskManager alongside existing registrations; model owners resolve actual control/config/statistics/items. Task/Achievement managers, SevenDay integration, original task pages and production Main remain pending.','Native validation uses diagnostic Button pointer endpoint with original49-task/item config and real dispatch/rewards/save/restart. This does not claim original-page appearance, account/report delivery, full inventory restart, Player build, remaining20 controller lifecycles or full game acceptance.'])
write(out/'LIMIT_TASK_ACTIVITIES_SOURCE_EVIDENCE.json',report)
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),fields=len(fields),usages=len(usages),literals=len(literals))))
