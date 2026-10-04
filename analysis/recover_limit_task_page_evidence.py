"""Publish reviewed complete limited-task page and Chinese countdown evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(32879,32894))|{32896,32655};index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((27328,27338,27428,27429,27434,27435,27443,27678,32897,33152,33167,33232,33324,33329))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4310,3249):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
usages=[]
for address in (4027112, 3992900, 3992904, 3992908, 3992912, 3992916, 4103200, 4069368, 4105232, 4056000, 4049064, 4049060, 4048960, 4048964, 4011344, 4069364, 4102540, 4084072, 4082384, 4095016, 4086856, 4060780, 4098924, 4049324, 4098416, 4043524, 4104376, 4105500, 4103664, 4103104, 3988816, 3988864, 3962728, 3962732):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 findings=[
  'CommonLimitTimeTaskUI4310 all15 methods restored, including previously reviewed task filtering32880. Day comparator32896 compares signed day only. Constructor allocates both selection providers and accumulator dictionary, sets popup layer2/open0/close0. Original namespace Proj_hdzd.UI.MainMenu and MainMenu/CommonLimitTimeTaskUI resource path compose recovered module registry/BaseUI owners.',
  'InitializeComponent resolves nine source outlets in order. DynamicList native components are attached there but renderer providers initialize in Awake InitializeLists32882 after countdown. Preview uses original embedded object; source ModeLabels dictionary is created before parent1301001/child1301 and PubNT config lookup. Close button uses original UIExtension Button callback then GF_UIButtonClick.',
  'Awake registers SelectPage on MsgDispatcher, accumulator rebuild on CommonExt, task refresh on CommonRefreshList, then clock10000 countdown. It invokes countdown, initializes lists, then creates initialized report_activity_enter3249, sets ActivityName literal Chinese seven-day carnival and source parent report string, then sends through current required report host. No invented platform/report delivery success.',
  'InitializeLists localizes title, initializes task then day renderer, clears day data, selects actual ExitToGetRewardDay, snapshots sorted-dictionary keys, appends day/activity1301 models and sorts day only. UpdateList precedes SetSelect(selectedDay-1,true); an offscreen selection sets data flag and later native render sends actual page-selection message.',
  'SelectPage ignores null/short/mismatched activity, writes selected day and ext.lastClickDayId without Dirty, renders tasks and rebuilds accumulator. Task refresh checks len>=1 but accesses args1 for matching1301, preserving one-element failure. Accumulator refresh ignores payload and disposes all held values then attempts Destroy of cleared root before clearing dictionary; clones current rewards as Node+i under original layout, SetData before dictionary Add with reread current reward id.',
  'Progress requires exactly2 args and matching selected day, casts Ext, performs otherwise-unused ModeLabels[accType] lookup, writes AccProgress text, captures target then rereads progress for float slider division with no zero guard. Preview forces root active/visible, copies origin world position, reads live RewardsData and invokes source asynchronous Refresh.',
  'Countdown asks current SevenDay.IsInActivity then rereads current control cached end and current statistics10000; unchecked Int64 subtraction, signed division1000 and lowInt32 feed TimeUtility32655. Formatter uses Chinese suffixes; >=86400 returns only days+zero-padded hours, below returns hours+minutes+seconds including negative source remainders. Outside activity writes ended and sends SevendayClose each call, without closing itself.',
  'Dispose first clears BaseUI lifetime, removes four page listeners, KillSelect day provider, then disposes nonnull accumulator items. It does not clear providers/dictionary, dispose preview, or remove close button handlers. Edit fixture retains root through disposal to model native deferred Destroy; native runner verifies actual close coroutine and original resource release.',
  'Composed page calls real task/accumulator activity and responds through own listeners, without test forwarding. Resource acquisition, language/sprite/detail, currency animation/skin UI and report transport are explicit required host boundaries. Production Main/menu/platform and original audiovisual acceptance remain separate requirements.'
 ],remaining=['Restore production seven-day menu entry/red/countdown subscriptions and complete Main/account/SDK/HTTP/scene assembly.','Continue Task/Achievement, remaining19 controllers and all requested business/reward/return flows.','Final Player build, original visual/audio/timing and real external callback acceptance remain incomplete.'])
(out/'LIMIT_TASK_PAGE_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
