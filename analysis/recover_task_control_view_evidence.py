"""Publish reviewed controller4507, AchiTaskSubUI3986 and original main task entrance evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(34423,34437))|set(range(30716,30734))|{33723,32609};index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
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
for owner in (3985,3986,4507,4273,3969):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4054820,4078632,4061088,4023344,4023352,4023340,4023348,3988776,3988780,4026340,3944708,3953440,3953576,3953600,3988852,3988868,3955368,4049024,3956312,3956316,3956320,3956308,3956304,3986036,3925716,3972940,3972944):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeData=u(ptr),typeCode=(u(ptr+4)>>16)&255)
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),
 methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)],fields=fields,usages=usages,
 findings=[
  'Controller4507 is now registered by concrete OutgameCoreControllerBindings.BindTasks into source resolver. OnInit subscribes MsgDispatcher GF_AdsPlayCallBack and WarWin, registers source900001 value function returning Int64 zero. InitHczzqEvent/Updata are empty. Source OnInit does NOT subscribe LoadStartingUI; external source callers must invoke RefreshData. Dispose removes three callbacks including that unused registration, clears two cached activity fields, retains controller registry slot and statistics provider.',
  'Daily getter caches first nonnull TaskActivity1300001.TaskViewActivities[ActivityID.Daily] direct dictionary result; Achi getter caches ActivityControl1601001. ActivityID4273 cctor32609 literal130001 is verified. Config StatisticEventConfig offsets24/28/80 resolve DailyCompleteLv/DailyWatchAds/ArenaRank. Ads exact-unboxes bool and only success increments configured daily event; WarWin always increments configured daily completion event, via actual StatisticsExpansion.',
  'RefreshData reads current LevelControl.CurLevel twice. Before10 it zeroes all recorded daily condition values with no claim/state/liveness reset. If second read>=10 SetRedDot runs and unlock=true. Only final entrance-visibility call guards absent page/root; earlier red call has no guard. Source StartUI33723 node0 sets task button active, node1 sets child0 red active, other indices no-op. Native original LeftBar/taskBtn hierarchy used.',
  'Daily red scans all actual tasks CanComplete then cached liveness rows state0/threshold, independent of task8 UI filter and receive state. Achievement red scans actual map; positive ArenaRank records are overwritten with raw EventCount and immediately skipped for that scan; all other records use CanComplete. SetRedDot short-circuits Achi OR Daily and sets entrance node1.',
  'AchiTaskSubUI3986 OnInit captures actualAchievement owner and subscribes common RefreshList. OnLateInit triggers actual manager RefreshSortList. OnDestroy clears required singleton owner BEFORE unsubscribing; retains RootUI, activity and rows. Full TaskSingleton generic ownership remains next work, not mocked as production complete.',
  'Filter allocates list and type dictionary. First record of each config.type with state!=1 reserves type BEFORE show conditions, so hidden first record suppresses later same-type rows. Show predicates require exact GameValue(key,Array.Empty<object>())==target, ignoring copied middle args. State1 skips without reserving. Ordering comparator30733 computes both eligibility first, prioritizes ready then falls back CompareTo (which computes both again).',
  'RefreshRows sets achievement tab red before unpacking/casting args0. Filter/sort, then only when row dictionary entirely empty creates unclaimed type rows, applies projections and Refresh. Existing nonempty dictionary never rebinding/adding missing rows in this method; it only SetAsLastSibling per visible type, catches Exception and warns boxedtype. Empty filtered list shows placeholder; nonempty list does not explicitly hide it. These non-obvious source rules preserved.',
  'CreateRow requests original page row then publishes Rows[type] before SetData(Achievement,Claim) and IsClaim=false. Projection shares Lang/reward objects, liveness0, lowInt32 target/progress, content/contentType/id and ActionCache=true; UID unchanged. Claim invokes actual strategy BEFORE re-reading claimed config type, first state0 actual type representative and row lookup; reuses/creates row or sets ActionCachefalse,hides,removes type mapping without disposal.',
  'Validation composes actual ordinary Task and Achievement before ActivityControl.Init, retains existing fixture defaults and restores both model scopes. Real original main/task prefabs and shared row assets used.1512 integrated including12 new; fresh11 native checks real EventSystem pointer, settled source layout, scaled pause/resume .7s slide, original200 gold/time, same row advance and source report reading next id after callback, data-pool independent restart and unsubscribe.',
  'Native first attempt measured initial layout-group placement as tween motion; final validation waits for real initial rendered layout before pointer/motion check, without changing production code. Full TaskSingleton/TaskPanel lifecycle/tabs and Main opening/closing route remain pending. Lifecycle20/38 is only controller binding coverage, not total restoration percentage.'
 ],remaining=['Recover TaskSingleton3990 concrete generic owner and full TaskPanelUI4416 lifecycle/tab toggle/close/top-info/audio/preview ownership, Main task button opening route.','Production Main/account/platform/effect/report hosts, remaining18 controller lifecycles/all business and original audiovisual/Player acceptance.'])
(out/'TASK_CONTROL_VIEW_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(report['methods']),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
