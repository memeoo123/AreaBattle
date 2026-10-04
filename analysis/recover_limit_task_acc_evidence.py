"""Publish reviewed accumulator, preview and source async state-machine evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(33232,33242))|set(range(33317,33331));index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((27313,27314,27428,27429,27434,27435,27443,27678,33182))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4348,4361,4360,4272):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
usages=[]
for address in (4018296,4018528,4069376,4063596,4013816,3990420,3990644,3972880,3972884,3972888,3972892,3972896,3976600,3976604,4089952,4090060,4090044,4083892,4097976,4089916,4089844,4095708,4089840,4097972,4090200,4090196,4090284,4095712,4049012):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 findings=[
  'LimitTimeTaskAccItem4348 all10 methods restored. SetData publishes data/child then Refresh. Last-reward branch compares config.id against global NoviceAccList.Count, not current child final position. State0 red eligibility uses derived child ext AccProgress; state1 claimed flags. Odd ids use boxY65/countY30; even boxY-65/countY-100. Final reward icon uses first reward legacy row icon/atlasName.',
  'Click33239 invokes optional callback before live progress/data reads. Insufficient progress with state0 invokes current CommonLimitTimeTaskUI.PreviewAccReward using locked-box transform. Otherwise Claim33232 guards progress and exact state0, invokes actual Child.GetAccReward, then rereads first live reward id and lowInt32 count. Activity economic long is not truncated by display.',
  'Current page absence skips first reward display only. GoodsType1/2 uses FlyMoney/FlyDiamonds with applyInventory=false/display=true, held page transform and locked-box world position. Only completion sends SevendayGetAccReward. Other goods map first legacy row paramInt to ToolChange(paramInt,1,true,CoinCost,true), then Show<SkinRewardUI>(paramInt), ignoring ToolChange result.',
  'Every secondary live reward invokes ToolChange(itemId,0,true,Activity,true): original zero delta is preserved rather than granting configured quantity again. Final current IsLast flips either reward red/got or locked/unlocked objects. Display/report exceptions retain already-applied claim and stop later work. Original GlobalConstData4272 static offsets56/76 identify CoinCost/Activity required reason hosts.',
  'SevendayAccPreviewItem4361 all12 methods plus async4360 MoveNext/SetStateMachine restored. Original outlets are img_bgLeft/img_bgRight/img_touch/CommonLimitTimeTaskRewardItem/rewardP. BaseItem initialization uses UIObject default visible=true; every SetVisible(true) registers update, every false queues removal. UIObject visibility keeps native object active and sets localScale one/zero; no BaseUI GF_VisibleUI message.',
  'Preview update reads EventSystem.current.currentSelectedGameObject. Keep-visible checks exact touch name, reward-template-name substring, or case-sensitive literal Node. Null/unmatched selection hides through source visibility. Pointer invokes hook only. SetData does not refresh. Left/right backgrounds have no additional source placement behavior in this class.',
  'Refresh33328/async33329 first clears tracked items: each Dispose then Destroy of its cleared GameObject, then list clear. Await actual WaitForEndOfFrame; destroyed root exits. After await, live Data.Count/Data[i] drive cloned rewards parented under rewardP with scaleone/active; SetData precedes list append. Concurrent awaits append both sets, hidden preview still builds, sprite failure leaves an untracked native clone. No invented cancellation or coalescing.',
  'Preview Dispose33327 executes BaseItem destroy/reference clearing and OnClick=null only. It retains item references and does not itself unregister update. Actual native test allows update-driven hide/removal before page fixture disposal. Existing original UpdateManager handles duplicate callback registration and deferred removal semantics.',
  'Integrated tests use controlled frame endpoint and observed currency/skin/sprite/detail hosts. Native runner verifies actual Unity frame await, EventSystem selection, pointer claim, original activity reward, automatic activity save and independent restart. It seeds claimed tasks as an explicit prerequisite; it does not claim a complete playthrough or inventory persistence.'
 ],remaining=['Complete CommonLimitTimeTaskUI lifecycle/refresh/close/countdown and production seven-day menu/red entry.','Actual currency animation/skin UI/localization/sprite/detail hosts and original audiovisual acceptance remain required.','Full Main/account/SDK/network/scene, remaining19 controllers/all business and Player remain incomplete.'])
(out/'LIMIT_TASK_ACC_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
