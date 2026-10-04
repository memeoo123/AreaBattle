"""Publish reviewed task-row, day filtering and source button-state comparator evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(33149,33160))|{32880,32897};index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 shutil.copy2(stage/row['path'],out/row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((27313,27314,27428,27429,30048,33179,33182,34647))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4337,4575):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
usages=[]
for address in (4069372,3992872,3992876,4095012,4098412,4075028,4086516,4084120,4084136,4084140,4097840,4049012,3962736,3919616,3955352,4027116):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
assets=json.loads((out/'limit-task-ui-import.json').read_text());page=next(p for p in assets['prefabs']if p['name']=='CommonLimitTimeTaskUI')
component=next(comp for node in page['nodes']if node['path']=='middleArea/Scroll View/Viewport/Content'for comp in node['components']if comp['className']=='DynamicList')
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,originalTaskList=component,
 findings=[
  'Task item4337 OnCreate casts source selection provider. Initialize resolves original des/progress/claim/goto/claimed/tiplayout/TipItem/progressLayout/progress-template/reward-template in field order. Source offset84 tiplayout is reward parent and92 progressLayout is progress parent. Awake removes all runtime claim/goto button listeners before attaching own. No button-message wrapper or implicit sound.',
  'OnRenderer33158 first clears old rewards, then obtains provider data, current ActivityControl parent1301001 and exact task activityId child dictionary entry. Reward loop rereads current Data.RewardsData around clone/SetData callbacks, and adds a reward item to held list only after successful rendering. A failed sprite callback leaves a native clone not yet tracked in the list.',
  'After rewards, renderer captures Config then rebuilds progress: dispose each tracked progress item and attempt Destroy again with its now-cleared GameObject, clear list, clone one for each live condition, obtain condition record and last configured conditionParams value, render and append. Missing configuration retains source partial cleanup/created object behavior.',
  'Description arguments use each conditionParams row last Int32 value, not current progress. Captured config.des.key goes through LangModule.GetFormat after child creation. Claim/goto/claimed buttons separately reread live Data.BtnState after localization; failures or callback changes remain observable. Empty InitializeSkin and goto33159 are preserved.',
  'Claim33152 calls current Child.TaskComplete(current Data.id), then sends MsgDispatcher SevendayFinishTask with live Data.Config.day and live Data.id after activity callbacks. Rejected/duplicate task still sends message if activity returns; thrown reward/report error prevents it. A report callback can rebind row before final event. Row itself neither forces refresh nor performs additional award/save.',
  'Dispose33153 executes DynamicBaseItem reference clearing then only clears rewards. It does not destroy root, clear progress items, remove button listeners, or erase Data/Parent/Child references. Re-render clears old progress; owning page ultimately destroys its native hierarchy.',
  'CommonLimitTimeTaskUI32880 holds child day-task list, clears task provider data, tests live ShowCondition entries with GameValue then rereads target after callback, and includes only visible tasks. Claimed and not-yet-unlocked day rows are not independently filtered here. Comparator32897 compares BtnState enum only, not model CompareTo/id. Center index0 duration0 precedes DynamicList.UpdateList.',
  'Task list uses original component-1916771998422009902 and correct Content outlet: fixed single column, center alignment, spacing(0,8), leading10, non-recycling and non-normal-list mode. Adjacent day-list binding remains independent.',
  'Native original claim traverses recovered task activity, original liveness item factory/reward model and source common notification. Test forwarding of common task refresh to recovered RenderDay observes required page wiring without claiming full page lifecycle. Native manager save/restart covers task record and derived progress; global inventory persistence remains a separate host contract.'
 ],remaining=['Restore accumulator item and preview, then complete CommonLimitTimeTaskUI lifecycle/refresh/close/countdown and menu/red entry.','Required sprite/localization/item-info hosts remain observed fixtures here; no complete original audiovisual or platform acceptance claimed.','Full Main/account/SDK/network/scene, remaining19 controllers/all business and Player still required.'])
(out/'LIMIT_TASK_ROWS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
