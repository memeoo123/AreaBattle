"""Publish reviewed original main seven-day entry methods and lifecycle slices."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
selected={33690,33692,33695,33704,33711,33722,22718}
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4399,4502):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  offset=u(u(3823136+4*owner)+4*k)
  if owner==4399 and offset not in (132,136):continue
  fields.append(dict(owner=owner,name=ms(name),offset=offset,typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
usages=[]
for address in (4010040,4010052,4010064,4069380,4069372,4069376,4069368,4068052,4090072,4018384):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 findings=[
  'Complete source methods Proj_xqzdStartUI4399 InitializeSevenDay33722, click33704, red33695 and visibility33690 are restored as an owned binding. Source Awake33711 resolves field132 Button at RigthBar/btn_sevenDay and field136 GameObject imgReddot, then later invokes InitializeSevenDay. Only this slice of full Awake/Dispose33692 is claimed; full main-page assembly is still required.',
  'Initialization captures current Sevenday.IsUnlock. If true, AddClick(Button) appends native handler, registers red handler for SevendayUnlock/SevendayFinishTask/SevendayGetAccReward, registers visibility handler for SevendayClose, and immediately refreshes red. Finally Button.SetActive uses the captured initial bool even if callbacks changed clock/control. False initial unlock registers nothing; a future Unlock message alone does not invent registration.',
  'Click33704 calls Extension.PlayerVoice(group1,id2001), then current UIModule.Show<CommonLimitTimeTaskUI>(Array.Empty<object>()). Generic usage4018384 decodes metadata27397/methodInst6064/type4310. Restored binding routes to actual recovered page registry/owner. UIExtension Button wrapper sends GF_UIButtonClick only after callback normal return. Voice/owner failures stop later work; source click has no independent current unlock guard.',
  'Red33695 captures managed field136 before checking nonzero pointer. If nonnull it asks current Sevenday.IsHaveAnyRed and sets captured object active. This is not Unity Object equality: destroyed-but-managed-nonnull reference can still fail on SetActive. Incoming event args are ignored. Visibility33690 asks current IsUnlock first then sets current Button.gameObject; it does not refresh red or close a popup.',
  'Dispose33692 removes the four entry listeners only if current Sevenday.IsUnlock is true at disposal. An ended interval skips removal. No registration cache, unconditional cleanup, initialization guard or button-listener removal was added. Repeated initialization adds duplicate handlers; one disposal removes one delegate occurrence through original MsgDispatcher.',
  'Native original entry opens complete recovered activity page using actual registry and module loading. Task completion red refresh uses actual SevendayFinishTask after economic/task state update. Accumulator common refresh rebuilds page but entry red waits for currency completion SevendayGetAccReward. Page expiry SevendayClose hides entry while the page itself remains open.',
  'Actual activity manager automatic save and independent disk restart are verified through entry-to-page flow, with explicit seeded prerequisite task/accumulator states. Inventory persistence, all original gameplay prerequisites, real sound/effect/report/resource hosts, complete main-page/Main assembly and original audiovisual acceptance are not inferred.'
 ],remaining=['Complete Proj_xqzdStartUI and production Main/account/resource/audio/effect/report/SDK/HTTP/scene hosts.','Restore ordinary Task/Achievement manager/activity/models/UI and remaining19 controllers/all business/reward/return flows.','Final Player and original visual/audio/timing/external callback acceptance remain incomplete.'])
(out/'SEVENDAY_ENTRY_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
