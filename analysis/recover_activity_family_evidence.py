"""Retain shared family bodies, NoviceTaskManager data methods and exact activity roster."""
from pathlib import Path
import argparse,hashlib,json,struct,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);stage=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=stage/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def read(p):return json.loads(p.read_text())
def write(p,v):
 text=json.dumps(v,ensure_ascii=False,indent=2)+'\n';crlf=p.exists() and b'\r\n' in p.read_bytes();p.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def copy_body(path):
 dest=out/path;crlf=dest.exists() and b'\r\n' in dest.read_bytes()
 text='\n'.join(line.rstrip() for line in (source/path).read_text().splitlines())+'\n'
 dest.write_bytes((text.replace('\n','\r\n') if crlf else text).encode())
selected=set(range(35592,35602))|{35555,35556,35560,35572,35585}
index=read(out/'method-map.json');known={r['metadata'] for r in index};methods=[];added=[]
for row in read(source/'method-map.json'):
 if row['metadata'] not in selected:continue
 copy_body(row['path']);methods.append(dict(row,sha256=sha(out/row['path'])))
 if row['metadata'] not in known:index.append(row);added.append(row['metadata']);known.add(row['metadata'])
assert {r['metadata'] for r in methods}==selected
write(out/'method-map.json',index)
generic=read(source/'activity-family-generics.json');shared=[]
for row in generic['methods']:
 copy_body(row['path']);shared.append(dict(row,sha256=sha(out/row['path'])))
assert len(shared)==11
for name in ('activity-family-generics.json','activity-family-rgctx.json'):shutil.copy2(source/name,out/name)
b,mem,ts,md,ms,u,pairs=[c[x] for x in ('b','mem','ts','md','ms','u','pairs')]
def typeid(ptr):
 data=u(ptr);code=u(ptr+4)>>16&255
 if code in (17,18):return data
 if code==21:return typeid(u(data))
 return None
def chain(i):
 result=[]
 while i is not None and i not in result:
  result.append(i);parent=ts[i][4];i=typeid(u(200288+4*parent)) if parent>=0 else None
 return result
activities=[]
for t in range(c['im'][2],c['im'][2]+c['im'][3]):
 ancestors=chain(t)
 if 4629 not in ancestors:continue
 attr=c['attrs'].get(ts[t][-1]);registration=None
 if attr:
  raw=b[pairs[24][0]+attr[1]:pairs[24][0]+attr[2]]
  assert raw[:9]==bytes.fromhex('01b089000001000008') and len(raw)==13
  encoded=((raw[9]&63)<<24)|(raw[10]<<16)|(raw[11]<<8)|raw[12];assert encoded&1==0
  registration=dict(activityId=encoded>>1,constructorMetadata=35248,rawHex=raw.hex(),attributeRangeIndex=attr[0],dataOffset=attr[1],dataEnd=attr[2])
 activities.append(dict(typeIndex=t,name=ms(ts[t][0]),parentChain=ancestors,registration=registration))
assert [(r['typeIndex'],r['registration']['activityId']) for r in activities if r['registration']]==[(4685,1300001),(4699,1301001),(4714,1601001)]
assert len(activities)==9
write(out/'activity-registration-roster.json',dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),activities=activities,qualification='Complete Assembly-CSharp ActivityBase ancestry, including generic parent resolution; only three direct ActivityControlRegister attributes. ChildLimitTimeTaskActivity is constructed by the father, not automatically registered.'))
fields=[]
for owner in (4639,4640,4701,4702,4703,4704,4707,4710,4711):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
value=u(3997964);assert value>>29==6;spec=struct.unpack_from('<3i',mem,483008+12*((value&0x1ffffffe)>>1));assert spec[0]==43255 and ms(md[spec[0]][0])=='First'
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,sharedGenerics=shared,fields=fields,rgctx='activity-family-rgctx.json',activityRoster='activity-registration-roster.json',firstConfigMethod=dict(address=3997964,spec=list(spec),method='Enumerable.First'),
 findings=[
  'Child4639 shared Update/OnInit/Dispose are empty. EarlyOnInit assigns moduleData only. Father4640 constructs four dictionaries before base; Update/OnInit/Dispose/SortDataDic are empty. Runtime generic contexts distinguish Dictionary.Add from data-map set_Item, resolve Activator.CreateInstance and method-generic config ContainsKey.',
  'ResetChildActivity replaces only Self/OtherViewActivities. It enumerates current common config Values and gates by supplied config.ContainsKey(row.id). Nonempty parent arrays construct child before scanning, first matching parent appends Children then Adds parent-id map, self view, global child map; matching module binds then virtual Refresh. Unmatched parent construction is discarded; a module match creates another child for global/other view. Missing self module still leaves child attached/unrefreshed. No implicit cleanup/deduplication/null guards.',
  'Filtering uses child.Data.state==3 and child.Data.id as view-data key. Self data set_Item occurs before virtual SortDataDic, then other-data filtering. Previous data maps and child ownership remain, including duplicate/Add failure prefixes and missing-module null failures.',
  'NoviceTaskManager4711 ActivityId1301001, OnInit UpdateData(true), empty InitStrategy/OnRelease. Registration CommonGameModule autoSyn=true compressData=false matches existing manager roster. Data/child/task/condition persisted fields and constructor defaults are retained; private runtime task caches and properties are not serialized.',
  'Load first parses decompressed JSON and on Exception parses raw input; second failure propagates and retains prior Data. Parsed/null-new Data publishes before reconciliation. Initial children derive from NoviceTasksByActivity, dayId1, lastClickDayId from dictionary insertion-first Enumerable.First, zero with diagnostic on empty group; child datas remain empty until concrete child initialization.',
  'Removal loop resolves current config even at terminal iteration and removes every missing NoviceActivities row with diagnostic/i--. Addition enumerates that terminal config owner but resolves current owner again for new task groups; Exists keeps duplicates. Live Data is reread after callbacks. No dirty mark or save in reconciliation. UpdateCallback clears index then overwrites by activityId, last duplicate wins, followed by captured activity.RefreshData.',
  'OnSave invokes captured activity.OnSave before rereading Data, JSON serialization, activity gzip compression and virtual SaveLocalData. Storage uses original CommonGameModuleNoviceTaskManager key and inherited server/local synchronization boundary. No invented successful network response.'
 ],boundaries=['Concrete LimitTimeTaskActivity/ChildLimitTimeTaskActivity, task progress/rewards/getters, pages and SevenDay/Main composition remain pending; required activity endpoints are explicit and have no production default.','Shared family and module manager are outside LogicModule38 roster; lifecycle18/38 remains. No fresh native PlayMode/Player build or audiovisual acceptance.'])
write(out/'ACTIVITY_FAMILY_SOURCE_EVIDENCE.json',report)
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),sharedGenerics=len(shared),fields=len(fields),activities=len(activities))))
