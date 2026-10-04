"""Publish reviewed ordinary-task storage/model/factory evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(35473,35494))|set(range(35411,35414))|set(range(35389,35395));index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())

methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4677,4686,4687,4688,4689,4690,4691):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4049000,3972972,3972976,3972984,3972988,3978972,3978976,3997852,3998384,4000604,3956032,3974920,3974924,3976580,3976584):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 cloneNativeWrapper=dict(table=109131,module='wasmcode1',function=1361,objectMemberwiseCloneMetadata=3953,previousVerifiedEvidence='GLOBAL_ITEM_MANAGER_AUDIT.json:indexImplementation.cloneEvidence'),
 findings=[
  'TaskData4686/ChildTaskData4687/Ext4688/GroupData4689/TaskItemData4690/Condition4691 and LivenessItemData4677 fields recovered directly from metadata and runtime offsets. Public storage includes task UID/progress/expiry Int64, state/receiveState, condition key/value/args, activity/group refresh counters and nextRefreshTimeStamp. Config/video caches and Selected backing field are private; livenessItemDatas is assembly-internal. Constructors create specified lists only, receiveState1, private video cache-1. Group.extraRefreshTimes and liveness cache remain null.',
  'TaskData.Clone35473 returns Object via native table109131, shared wasmcode1 function1361 with Object.MemberwiseClone3953 and earlier verified item clone. It creates a new outer record while sharing datas and all nested objects; no deep clone invented.',
  'TaskItem.Config35486, Child.Config35475, Group.Config35482 and Liveness.Config35411 cache first nonnull current manager result and retry null. Changing record IDs/table entries after cache publication does not invalidate retained config.',
  'TaskItem.ResetProgress35484 appends without clearing. Each current config row supplies key first, copied argument middle and omitted final target; value defaults0. It leaves prior records/state/selection intact and retains successful prefix if later malformed row throws. CanComplete35485 captures config, compares each recorded Int64 value with signed final Int32 target, then requires exact state0. No receive/progress/expiry/show/day checks and empty recorded list passes.',
  'GetDes35487 captures config, snapshots final target ints, boxes into object array and invokes required language formatter on config.des.key. CompareTo35490 computes both completion values first, then source state/receive branches: ready before other, incomplete unreceived before received, claimed last, ties by cached config.id. Noncanonical states retain source non-total ordering, including state2 self-comparison1.',
  'Condition.KeyEqual35492 compares key then null/length/ordered argument contents, ignoring value. Null args differ from empty; both null match; null other throws. Source uses live current arrays inside comparison loop.',
  'Child/Group ResetExtraRefreshTimes35476/35481 replace the counter vector before reading cached configuration.extraRefreshTimes length and append zeros only; other total/free/video counters stay unchanged. Null config/array failure leaves published empty list. Raw original table scalar0 is not silently turned into array behavior by these methods.',
  'Ext.GetLivenessItems35478 publishes list before refresh; direct Refresh35479 before initialization throws. Refresh clears same list, asks current config group, and enumerates dictionary Values in existing order, creates id/threshold/state rows from unsigned livenessAward shift with Int32 mask31. No id sort or auto-update on subsequent cached reads. Missing group warns through config manager then leaves empty list.',
  'Liveness.Rewards35412 publishes cache before loop, reads configured reward id/count pairs, sign-extends Int32 quantity into Int64 and uses default rewardOrder0. Invalid later row retains partial cache and prevents automatic reconstruction; negative quantities are preserved.',
  'TaskFactory4673 produces only type1=2/type2=3. TaskLivenessPoint4674 Add/Use delegate original base. AddItemOnlyModel first applies full Int64 item model/reports, then sends CommonGameModule_Task_LivenessPointAdd plus current ItemConfig.paramInt with boxed lowInt32 count. Event failure retains model award/report; regular Add does not emit this activity-only event.',
  'Validation loads actual8 ordinary task records, one activity, one group and3 liveness tiers. Storage schema is verified with independent temporary file JSON serialization/deserialization; this is not TaskMgr/strategy automatic persistence. Concrete ordinary Task/Achievement managers/activities/UI/Main remain required.'
 ],remaining=['Restore ChildTaskOffNetStrategy4681, TaskNetStrategyBase4683, TaskOffStrategy4696, TaskMgr4692 and actual ChildTaskActivity4676/TaskActivity4685 assembly and automatic persistence/refresh/claim flows.','Restore Achievement models/manager/strategy/activity/UI and remaining19 controllers/all business/production Main hosts.','Final Player and original visual/audio/timing/external callback acceptance remain incomplete.'])
(out/'TASK_MODELS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
