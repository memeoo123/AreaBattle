"""Publish reviewed ordinary task activities, strategies and registration evidence."""
from pathlib import Path
import argparse,hashlib,json,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def copy_reviewed(name):
 data=(stage/name).read_bytes();destination=out/name
 if name.endswith('.txt'):
  ending=b'\r\n'if destination.exists()and b'\r\n'in destination.read_bytes()else b'\n';data=ending.join(line.rstrip()for line in data.splitlines())+ending
 destination.write_bytes(data)
selected=set(range(35375,35380))|set(range(35398,35411))|set(range(35414,35473));selected-={35448,35450,35451,35452}
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 copy_reviewed(row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
copy_reviewed('disassembly/TaskEventWrapper-8807.txt')
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4669,4670,4676,4678,4680,4681,4683,4685,4579,4581,4583):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3939852,3939868,3939860,4048952,4049052,4049048,4049068,4049056,4049080,4049084,4049072,4048996,4048992,4101892,4103184,4106340,4102724,4105236,4010580,3988868,3995144,3988820,4030836,4030844,4016600,3992468,4027052,4027056,4027060,4027064,4011340,4011352,4011360):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==1:
  ptr=u(200288+idx*4);owner=u(ptr);row.update(typeCode=(u(ptr+4)>>16)&255,owner=owner,sourceClass=ms(ts[owner][0]),sourceNamespace=ms(ts[owner][1]))
 elif kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
abstract=[]
for i in (35448,35450,35451,35452):assert md[i][7]&1024;abstract.append(dict(metadata=i,name=ms(md[i][0]),slot=md[i][-2],flags=md[i][7]))
ptr=u(72592+4409*4);assert u(ptr)==1;arg=u(u(ptr+4));assert u(arg)==4678
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,abstractMethods=abstract,
 weightedGeneric=dict(method=26203,methodInstantiation=4409,argumentType=4678,sharedEvidence='package-random-generics.json',body='disassembly/PackageRandomGeneric-26203.txt'),
 eventWrapper=dict(path='disassembly/TaskEventWrapper-8807.txt',sha256=hashlib.sha256((out/'disassembly/TaskEventWrapper-8807.txt').read_bytes()).hexdigest(),purpose='Original Int64 uid/value split-word wrapper invokes virtual slot8 with boxed object[] arguments.'),
 findings=[
  'TaskActivity4685 derives directly from ActivityBase, not FatherActivityBase. Source registration1300001 is preserved. OnInit gets TaskMgr, replaces self/other child maps, enumerates activity configs with TaskActivities membership and literal parent1300001, preserves registered limited-task children. Matching child attaches to parent maps before global map/module/Refresh. Nonmatching parent arrays create an unused child then a distinct other-view child. Source OtherViewDic getter creates a new dictionary on every access.',
  'Self view retains old entries, adds launched modules by module activityID and stable-sorts by child Config.param into replacement dictionary; failure leaves original dictionary. Other-view writes are ephemeral. Launch event Add/sort/notifies self once, but other-view transient existence check permits repeated notifications. RefreshData updates self view for each matching parent entry and rebinds only actual parent-owned children. SaveData visits self then other map values; Dispose removes parent listener only.',
  'ChildTaskActivity4676 indexes task UID and group ID with Add, retaining failure prefixes. OnInit creates and initializes a new concrete strategy without disposing previous strategy. UpdateData replaces module, clears/reindexes maps, removes/readds existing strategy listeners and sends current activity/list. Reset/launch/claims/save/dispose map exact source virtual slots; child TaskComplete passes notify=true.',
  'Metadata usages3939852/3939868 distinguish parent TaskFactory4673 (2/3 liveness) from child TaskItemFactory4669 (10/4 TaskIapRefreshItem4670). Both factories share ctor function3197, whose symbol annotation alone is ambiguous. Child item regular Add performs base economic award then virtual Use; model-only Add bypasses Use and source Use only reports. No implicit task refresh/debit or platform purchase is invented.',
  'TaskNetStrategy.Init binds child, invokes strategy OnInit then adds listeners. Every Add registers time even if key list already nonempty. Configured condition keys deduplicate event listeners; removal uses current saved condition keys and clears key list, preserving source mismatched-key listener leakage. Statistics catches Exception and logs separate message/stack; local progress requires child state3 and all tasks require receiveState!=0, lifetime statistics can update inactive tasks. Exact three args selects first Int32 filter; delta unboxes Int64; ChangeTaskProgress accepts boxed object[] and uses Int32.Equals(object).',
  'Progress writes first matching condition before report. Threshold uses current condition row indexed by FIRST configured row length-1, not its own last index; mismatched row lengths can throw after assignment. Reports use activity name and task ID in DetailType; old/new lowInt32 values select slot0 for condition index>=3. Old-at-target or unchanged value skips report. Parent dirty follows successful report; no match leaves dirty unchanged.',
  'Task claim guards found task/CanComplete/current config, sets state1 BEFORE award, applies real item self-model rewards then configured costs without balance guard, optionally sorts/notifies list, marks dirty, reports lowInt32 legacy progress. All-complete report requires no task dictionary value state0; noncanonical state2 counts complete. Liveness endpoint has no threshold/repeat guard, awards first, marks first matching threshold tier/bit rather than matching ID, reports/notifies then dirty.',
  'Initialization adds liveness event, sorts tasks, captures actual parent1300001 and invokes time handler before base event binding. Daily/weekly only reset when now>next; interval resets on equality and advances from prior next with integer duration steps. Seconds/minutes multiplication wraps Int32 before widening; zero interval can divide by zero. Calendar next selection uses original parameter order, local epoch+8 DateTime, strictly later hour/weekday. Daily/weekly announcement follows next timestamp and dirty update before reset.',
  'ResetAll clears video/free/TRefresh counters and replaces extra vector before ResetTasks(true). ResetTasks optionally auto-claims ready autoGet0 tasks, then removes selected records after EntityRemove event, preserving source autoRefresh/autoRefreshComplete retain flags. Group loop bound is group count minus captured remaining TASK count. Group chance is inclusive Random(0,10000) with > threshold rejection. Eligible weighted choices exclude existing IDs, evaluate show conditions with boxed args, require configured count and select without replacement using shared26203 integer accumulator/managed random.',
  'Generated tasks use each selected ID but FIRST selected config expiry/receiveMode for all group records. Zero UID presence is checked before assigning signed hash UID; collision on final Add can fail. New group is indexed and appended before counter reset; existing group free count resets. Task appended after group reset. After optional automatic liveness redemption/reset, task list is replaced from live UID dictionary and sorted, listeners rebound, per-task EntityAdd then ext/list messages emitted before dirty.',
  'SaveData replaces ext.groupDatas from group dictionary values. Manager snapshot-before-activity-save behavior from previous milestone remains; periodic activity Update saves only when dirty/pool ready and clears dirty after manager normal return. Concrete TaskRuntime installs actual class/manager/factory and model services while preserving other registrations and required account/report/item hosts.',
  'Original eight-task configuration and concrete ordinary/seven-day joint graph are exercised with real event dispatch, item reward model and independent file restart. Task page/native UI claim pointers and complete production Main/account/platform are not included in this source milestone.'
 ])
(out/'TASK_ACTIVITY_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),abstractMethods=len(abstract),indexedMethods=len(index))))
