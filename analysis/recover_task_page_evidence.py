"""Publish reviewed controller4507, AchiTaskSubUI3986 and original main task entrance evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(33864,33884))|{33679,33726,30716,30742,30734,30728};index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
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
for owner in (3990,3989,4416,3544):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4034536,4071740,3986040,3986048,4000568,4029836,4016616,4016608,4018224):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeData=u(ptr),typeCode=(u(ptr+4)>>16)&255)
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)

generic=json.loads((stage/'task-singleton-generics.json').read_text());contexts=[]
for slot in range(5):
 kind,address=struct.unpack_from('<2I',mem,2208224+8*(6+slot));idx=u(address);row=dict(slot=slot,kind=kind,address=address,index=idx)
 if kind==3:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),method=ms(ts[md[spec[0]][1]][0])+'.'+ms(md[spec[0]][0]))
 if kind==5:row.update(constrainedType=idx,encodedMethod=u(address+4),methodIndex=(u(address+4)&0x1ffffffe)>>1)
 contexts.append(row)
assert contexts[4]['methodIndex']==30755
for row in generic['methods']:
 shutil.copy2(stage/row['path'],out/row['path']);row['sha256']=hashlib.sha256((out/row['path']).read_bytes()).hexdigest()
for fn in (2550,5061):shutil.copy2(stage/f'disassembly/TaskSingletonWrapper-{fn}.txt',out/f'disassembly/TaskSingletonWrapper-{fn}.txt')
shutil.copy2(stage/'task-singleton-generics.json',out/'task-singleton-generics.json')
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)],fields=fields,usages=usages,generic=generic,taskSingletonContext=dict(module=2209104,range=2207936,start=6,count=5,definitions=2208224,rows=contexts),findings=[
 'TaskSingleton3990 get_I resolves Activator.CreateInstance, assigns shared field BEFORE constrained ITaskSubUI.OnInit30755. Initialization failure keeps published instance, reentrant getter sees it, Clear is unconditional. Concrete generic instances3674/3675 are AchiTaskSubUI3986/DailyTaskSubUI3988. OutgameTaskSubviews is shared by all task pages in one application composition, alongside existing scoped controller owners.',
 'TaskPanel ctor33870 calls BaseUI then writes Layer3 (UITip) and clears two animation fields. Original namespace Proj_hdzd.UI.MainMenu and resource TaskPanelUI original manifest retained. InitializeComponent33869 binds18 outlets in source order. Existing BaseUI load/open/close/resource/registry mechanisms own actual page lifecycle.',
 'Awake33876 adds achievement then daily change handlers, daily then achievement generic pointer audio, acquires daily singleton, renders TopInfo mask6 at page layer, initializes original preview and liveness tiers then clears time text. OpenLater33878 initializes current view before registering close Button and Mask. Generic Toggle pointer uses source EventListener replacement semantics, with no Button global click message.',
 'InitializeCurrent assigns RootUI and sets LateInited true BEFORE invoking OnLateInit, then daily red then achievement red. Render failure preserves true flag and partial rows; no implicit retry. GetTaskItem allocates unbound row before comparing against lazy achievement getter, then daily getter; comparisons can create unvisited subviews. Unknown current returns uninstantiated row. Known view instantiates original embedded template under selected content.',
 'ToggleChanged early return only when true and selected matches. Otherwise effect close and content toggles run even on false. False then returns retaining selected/current. True stores selected, chooses known subview then InitializeCurrent. Effect failure precedes resetting handle and content changes. Existing row/late-init behavior is preserved across switches.',
 'Countdown uses original Task.Refresh language key and String.Format after Daily view formats clock. SetTaskEmpty uses lazy singleton comparisons and routes only current view. Red dots use actual TaskControl original conditions and child2 under Toggle.',
 'Close callback plays Audio.Play(1,new int[]{2001}) before CloseSelf. Button adds GF_UIButtonClick only after successful callback; Mask pointer does not. Base disposal precedes TaskPanel Dispose33875: TopInfo mask7 empty layer, lazy achievement OnDestroy, lazy daily OnDestroy, TaskControl.SetRedDot. No extra effect/preview/row disposal or list clearing is invented.',
 'Original StartUI33679 registers taskBtn closure33726 then TaskControl.SetRedDot. Closure plays voice1/2001 then GameDefine.ShowUI<TaskPanelUI>(true). Shared32617 first gets existing UI, otherwise opens with Array.Empty<object>; existing page SetVisible only if visibility differs. New OutgameTaskEntryBinding uses actual original LeftBar/taskBtn and original registry, preserves existing-hidden show behavior, and does not invent refresh subscriptions or unlock gates.',
 'Integrated and fresh native verification recorded separately. Explicit acquisition/audio/localization/sprite/effect/report test endpoints do not constitute complete production Main/account/platform composition.'
 ],remaining=['Production Main/account/data-pool/controller composition and all remaining18 controller lifecycles/business.','Actual audio/localization/effect1016/particle/report/platform hosts, final Player and original audiovisual acceptance.'])
(out/'TASK_PAGE_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(report['methods']),fields=len(fields),usages=len(usages),genericMethods=len(generic['methods']),indexedMethods=len(index))))
