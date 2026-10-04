"""Publish reviewed concrete Achievement manager/strategy/activity evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=(set(range(35608,35625))|set(range(35642,35690)))-{35683,35684};index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 destination=out/row['path'];data=(stage/row['path']).read_bytes()
 if destination.exists()and b'\r\n'in destination.read_bytes():data=data.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')
 destination.write_bytes(data)
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4714,4721,4722,4723,4724,4725,4726,4727):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3927868,3927864,3927872,3927884,3981452,3981456,3981460,3981464,3981468,3988852,3995068,4049016,4049020,4049024,4049028,4049076,4104080,4104084,4104088,4104096,4106280,4089704,4102724,3988788,3988792,3988796,4026356,4026364,4026368,4004740,4004744):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeData=u(ptr),typeCode=(u(ptr+4)>>16)&255)
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
ptr=u(72592+3386*4);assert u(ptr)==3;argv=u(ptr+4);types=[u(u(argv+4*k))for k in range(3)];assert types==[4716,4714,4722]
# Same verified shared generic bodies as ordinary TaskManager; concrete instantiation differs.
generic=json.loads((out/'TASK_MANAGER_SOURCE_EVIDENCE.json').read_text());genericFiles=generic['genericFiles']
for row in genericFiles:assert hashlib.sha256((out/row['path']).read_bytes()).hexdigest()==row['sha256']
review=json.loads((out/'ACHIEVEMENT_RUNTIME_REVIEW.json').read_text())
review['status']='Source review applied to concrete Achievement runtime;1500 integrated checks pass, full UI/Main remains pending.'
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),
 methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)],fields=fields,usages=usages,
 abstractMethods=[dict(metadata=i,method=ms(md[i][0]),slot=md[i][-2])for i in (35683,35684)],
 concreteGenericInstantiation=dict(index=3386,types=types),genericFiles=genericFiles,runtimeGenericContext=generic['runtimeGenericContext'],
 findings=[
  'Actual ActivityRuntime installs AchievementActivity4714 and AchievementMgr4722 before ActivityControl initialization. Models resolve actual1601001 owner. Manager is registered under CommonGameModuleAchievementMgr and uses existing account-aware storage/data pool save gates; no new per-frame autosave invented. Source Update35616 and Launch35660 are empty.',
  'Generic Init34693 binds activityID/callback, pool manager, then virtualslot5 OnInit when configured. Concrete instantiation3386 resolves AchievementData4716/Activity4714/Mgr4722; shared canonical generic bodies and wrappers already verified in TASK_MANAGER_SOURCE_EVIDENCE. OffStrategy OnInit calls manager.UpdateData(true) BEFORE re-registering statistics-refresh/point callbacks, so parse/callback failure prevents later registration.',
  'Manager callback publishes input, map allocate/clear, type dictionary clear, then last duplicate id wins. Source grouped representatives: singleton direct index; multi scans first state!=1, but last row is added even claimed only when ENTIRE type list is empty. Thus later all-claimed groups may have no visible representative. Raw35645 branches verified. Sorting rebuilds map in sorted order, reports all-claimed including empty, then list/type/ext messages preserve live fields/captured input.',
  'Read compact ids/times when ids nonnull nonempty: create Data before assigning shared ext, pad short times with0, append claimed records; otherwise deserialize legacy full Data. Both schemas preserve source null/error behavior, no compression invented. Fresh load enumerates127 configs; existing removes obsolete ids and appends missing from snapshot Values. Refresh reads lifetime GameValue(contentType,[boxed content]) for every record including claimed, then publishes callback.',
  'Statistics listeners gather distinct keys only when MessageKeys empty. Dispose removes from live configured rows but does NOT clear MessageKeys, so AddListeners after Dispose short-circuits. Statistics-refresh and point listeners remove/add separately. StatisticsEvent exact-unboxes Int64 delta, optional Int32 filter for len>=3 and converts final key. Delta absolute value wraps Int64.MinValue, no clamp. Source live Data index loop plus ChangeProgress replacement/sort can revisit/skip records; preserved and tested.',
  'ChangeProgress assigns before logging/report. Success report happens whenever old<target and value!=old, even below target; then Data.datas replaced with actual map values and sorted. Point event exact-unboxes Int32, mutates accPts before report and picks last qualifying cumulative config in dictionary enumeration, not numeric maximum; sends no ext message.',
  'Claim checks actual map and source CanComplete. Every reward entity regular Add happens BEFORE state1/time. Delivery failure therefore leaves changed inventory but unclaimed record; retry may award again. After claim/time, report precedes group representative replacement, whole sort/type sort/ext notification. Nonrepresentative claim can award/setstate then fail list[-1]; no invented transaction, rollback or hidden guard.',
  'Reset clears ext and all item progress/state/time; rebuilds cumulative rows in same lazily initialized list, sorts CURRENT type lists and sends ext. It deliberately does not restore initial progressive representative or re-query lifetime stats. Activity disposal clears cumulative cache then manager releases sort list and strategy listeners.',
  'Save copies actual map.Values into Data.datas then serializes shared ext and only state1 ids/times in REVERSE order, plain JSON, through captured manager.SaveLocalData. Verified real data-pool/account file restart with127 original rows, claimed timestamps/point mask, progress recomputation, legacy repair/padding and empty-claim fallback. Data pool disabled-save gate is retained.',
  '1500 full integrated checks including13 new. No fresh native or Player run. UI/controller/Main/account/platform delivery hosts and final original audiovisual acceptance remain incomplete; lifecycle19/38 unchanged.'
 ],reviewedSource=review,remaining=['Complete AchiTaskSubUI3986/TaskController4507/TaskSingleton3990/TaskPanelUI4416 ownership, daily/achievement tabs/red dots and Main entry.','Connect full production Main/account/platform/effect/report hosts and remaining19 controller lifecycles/business; final original audiovisual and Player acceptance.'])
(out/'ACHIEVEMENT_RUNTIME_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(report['methods']),fields=len(fields),usages=len(usages),indexedMethods=len(index),genericTypes=types)))
