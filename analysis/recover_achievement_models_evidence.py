"""Publish reviewed Achievement model/factory methods, without publishing unrelated stage extractions."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(35625,35641))|set(range(35155,35160));index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4715,4716,4717,4718,4719):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3927880,3988852,4049016,3972936,3972932,3978972,3978976,3923040,3926996,3976584,3976580,3923804,3938108):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeData=u(ptr),typeCode=(u(ptr+4)>>16)&255)
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),
 methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)],fields=fields,usages=usages,
 findings=[
  'Source4715 SaveData initializes Ext, ids List<Int32>, times List<Int64>;4716 Data initializes Ext and AchievementItemData list.4717 Ext holds Int64 statePts and Int32 accPts.4718 records public id/progress/state/time with private config/selection/show cache.4719 cumulative records public index/acc with private config. Constructors leave progress/state/time zero. JSON keeps exact public field names and excludes private selection/caches.',
  'CanComplete35630 reads cached config.number Int64, compares signed progress>=number then exact state0. No show-condition, selection, time or expiry checks. Config35631/35638 cache first nonnull and retry missing; cumulative config lookup uses acc rather than index.',
  'Rewards35632 allocates fresh list and reward objects on EVERY call, captures config once and reads signed Int32 row count into Int64 itemCount; rewardOrder defaults0. Malformed row throws with no cache publication, so repaired config can be read on next call.',
  'ShowCondition35634 publishes empty cache first, captures config for rows but rereads cached Config for each Count. Conditions use first key, final signed target and Array.Copy middle Int32 values boxed into object[]. Failure retains successful prefix and prevents further automatic decode. Config array mutation does not alter copied args.',
  'CompareTo35633 computes both CanComplete values before inspecting states. State0 ready first, state0 unready next, all nonzero states last, ties by cached config.id. Noncanonical nonzero states still use same tie branch (unlike ordinary task CompareTo). Null other/config failures propagate.',
  'Cumulative CanComplete35637 resolves ActivityControl.GetActivity<AchievementActivity>(1601001).Manager.Data.ext each call, compares signed accPts>=acc then rereads owner for State. State35640 unsigned-shifts Int64 statePts by index low6bits and masks1; signed/large indices wrap like source WebAssembly. Compare35639 computes both eligibility values then rereads both states before same readiness/state/config.id branches.',
  'Factory35158 accepts only type1=2/type2=2. Allocation usage3927880 resolves Type4627 AchievementPoint; disassembly constructor annotation LimitTimeTaskLivenessPoint is a shared native body, not the allocated runtime type. Add35157 calls original ActivityVirtualItemBase.AddItem(table18424) before CommonModule_Achievement_PointAdd containing boxed lowInt32 count. Thus full signed Int64 economy, report and regular delivery precede event; inherited AddItemOnlyModel does not emit it. Use35155 delegates base reporting without independently debiting.',
  'Original config contains127 Achievement records and3 cumulative thresholds30/60/90. Runtime cumulative config Type4562 declares only id despite extra raw JSON reward fields; no new cumulative claim behavior or unknown rewards inferred. Tests cover data and factory with actual item engine; owner is an explicit fixture until Achievement runtime is restored.'
 ],remaining=['Implement concrete AchievementMgr4722, AchievementStrategyBase4727, AchievementOffStrategy4725 and AchievementActivity4714 over actual ActivityControl and automatic account storage.','Complete TaskController4507/AchiTaskSubUI3986/TaskPanelUI4416 and daily/achievement tab/Main ownership.','Production Main/account/platform/remaining19 controller lifecycles, other business and Player/original audiovisual acceptance remain incomplete.'])
(out/'ACHIEVEMENT_MODELS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(report['methods']),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
