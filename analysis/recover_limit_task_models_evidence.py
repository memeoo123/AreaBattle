"""Retain source task/accumulator/page models and liveness item behavior."""
from pathlib import Path
import argparse,hashlib,json,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);stage=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=stage/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
selected=set(range(35557,35585))|set(range(35586,35592))|set(range(35278,35281))
index=read(out/'method-map.json');known={r['metadata'] for r in index};methods=[];added=[]
for row in read(source/'method-map.json'):
 if row['metadata'] not in selected:continue
 dest=out/row['path'];crlf=dest.exists() and b'\r\n' in dest.read_bytes();text='\n'.join(line.rstrip() for line in (source/row['path']).read_text().splitlines())+'\n';dest.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
 methods.append(dict(row,sha256=hashlib.sha256(dest.read_bytes()).hexdigest()))
 if row['metadata'] not in known:index.append(row);known.add(row['metadata']);added.append(row['metadata'])
assert {r['metadata'] for r in methods}==selected and len(methods)==37
write(out/'method-map.json',index)
b,mem,ts,md,ms,u,pairs=[c[x] for x in ('b','mem','ts','md','ms','u','pairs')]
fields=[]
for owner in (3452,4527,4541,4643,4653,4703,4704,4705,4706,4707,4708,4709):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
assert any(r['owner']==4527 and r['name']=='paramInt' and r['offset']==44 for r in fields)
assert any(r['owner']==3452 and r['name']=='datas' and r['offset']==8 for r in fields)
usages=[]
for address in (3956064,3956044,3954608,3954620,3956052,3984788,3987572,3972936,3972944,3972952,3972956,3972964,3978976,3976584,3976604):
 encoded=u(address);assert encoded>>29==6;spec=struct.unpack_from('<3i',mem,483008+12*((encoded&0x1ffffffe)>>1));method=md[spec[0]]
 usages.append(dict(address=address,spec=list(spec),sourceClass=ms(ts[method[1]][0]),method=ms(method[0])))
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,methodUsages=usages,
 findings=[
  'LimitTaskItemData4704 activityId scans NoviceTasksByActivity for first group containing task id, not Config.activityId; no match reports and returns0. NoviceAccRewardItemData4705 similarly scans NoviceAccByActivity. Child getters capture activity1301001 before membership lookup; Ext4703 uses stored activityId directly. Config getters cache only nonnull rows.',
  'Task ShowCondition publishes its List<Condition4643> before filling. Each original ListArrayInt.datas array supplies key at0, signed last Int32 promoted to Int64 target, and boxed Int32 middle arguments via Array.Copy. Malformed later rows retain cache prefix and do not retry. ResetProgress clears persisted reference conditions, creates key/middle int[] with value0; state/Selected/show cache unchanged.',
  'Task CanComplete first compares each saved condition value to matching config final threshold, then each show condition to GameValue. Show target is reread after provider callback. It then compares child ext.dayId with Config.day and finally requires state0. BtnState is2 for exact state1 without eligibility access; otherwise0 when eligible,1 when not. Comparator evaluates both eligibility getters before state snapshots; claimable then pending then nonzero, ascending signed config.id within groups. Own config field read follows other config resolution.',
  'Task and accumulator RewardsData lazily publish a mutable list and decode configured [itemId,intAmount] into RewardItemData with signed Int64 amount/order0. Task source redundantly allocates the list twice. Malformed rows retain prefix; no clamping/retry/automatic delivery. Private caches and Selected backing fields are absent from source JSON save schema.',
  'Accumulator Progress resolves activity accType:0 counts exact state1 tasks across all sorted day groups; second lookup must equal1 to sum rewards whose GameItemConfig type1=2/type2=4. Any other mode returns0 silently. Ext.AccProgress switches a single config lookup, uses same claimed count for0, but for1 additionally requires item.paramInt==ext.activityId; invalid mode emits source diagnostic and returns0. Sums use unchecked Int64 then truncate Int32. Both traverse claimed task rewards, not inventory or ext.accFlag.',
  'Ext.TargetProgress performs initial activity-config lookup even though its return is unused, then rereads config owner for group membership and again for values. Present group returns max accValue from initial0, including empty/all-negative0. Missing group reports and returns999. Accumulator CanComplete only blocks state1 before progress/threshold lookup. Accumulator CompareTo sorts signed accValue with callback-sensitive own-field reread. Page constructor arguments are(day,activityId), Selected defaults false.',
  'ActivityVirtualItemBase4653 delegates constructor/AddItem/Use to original ItemBase. LimitTimeTaskFactory4708 routes only2/4 using held factory config, then entity constructor reads current item config again. LivenessPoint4709.AddItemOnlyModel calls base model rewards first, then sends CommonGameModule_LimitTimeTask_LivenessPointAdd plus live paramInt, with boxed unchecked Int32 count. AddItem remains ordinary reward route, Use reports without debit. Model errors stop event; event errors retain prior inventory/report/dirty mutations.'
 ],boundaries=['Model services require actual config/statistics/item/control owners; no fallback child graph or rewards. Tests use explicit child graph interfaces until concrete ChildLimitTimeTaskActivity4698/LimitTimeTaskActivity4699 initialization/listeners/claims are restored.','Source item/report events verified through existing real item engine; external report/UI/platform delivery remains host-owned. No new native PlayMode/Player or full page acceptance; controller lifecycle remains18/38.'])
write(out/'LIMIT_TASK_MODELS_SOURCE_EVIDENCE.json',report)
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),fields=len(fields),usages=len(usages))))
