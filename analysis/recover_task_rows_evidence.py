"""Publish reviewed TaskItemItem and DailyTaskSubUI methods and immutable asset provenance."""
from pathlib import Path
import argparse,hashlib,json,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
target=c['p'];out=target/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(33372,33387))|set(range(30734,30752))
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 dest=out/row['path'];data=(stage/row['path']).read_bytes();ending=b'\r\n'if dest.exists()and b'\r\n'in dest.read_bytes()else b'\n';dest.write_bytes(ending.join(line.rstrip()for line in data.splitlines())+ending)
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
selected.update((27313,22718,68862))
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4366,4367,3987,3988,4508,3969):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4075052,4075284,4056032,4060860,4059920,4089912,4101768,4048900,4048884,4071776,4071772,4066660,4066636,4034536,4034704,4016584,4016588,4016592,4049072,3994852,3994856,4027388,3944708,3925720,3998088):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==1:
  ptr=u(200288+4*idx);row.update(typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
enum=[]
for k in range(pairs[7][1]//12):
 field,typ,offset=struct.unpack_from('<3i',b,pairs[7][0]+12*k)
 if ts[4508][8]<=field<ts[4508][8]+ts[4508][18]:
  name=ms(struct.unpack_from('<i',b,pairs[11][0]+12*field)[0]);encoded=b[pairs[8][0]+offset];assert encoded in (0,2);enum.append(dict(name=name,value=encoded>>1,encoded=encoded))
snapshot=target/'generated/resource-snapshots/task-panel-ui-20261003';acquisition=json.loads((snapshot/'acquisition-manifest.json').read_text());assert len(acquisition['items'])==12
for row in acquisition['items']:
 data=(target/row['path']).read_bytes();assert row['status']=='verified'and len(data)==row['catalog']['size']and hashlib.md5(data).hexdigest()==row['catalog']['md5']and hashlib.sha256(data).hexdigest()==row['sha256']
assets=json.loads((out/'task-panel-ui-import.json').read_text());duplicates=[]
for prefab in assets['prefabs']:
 for node in prefab['nodes']:
  graphics=[c for c in node['components']if c['className']in ('Image','Text')]
  if len(graphics)>1:duplicates.append(dict(prefab=prefab['name'],node=node['path'],sourceIds=[c['sourceId']for c in graphics],adaptation='First source Graphic remains on original owner; later Graphic represented by full-rect child appended after all original siblings. Visual equivalence and source animations still require acceptance.'))
assert len(duplicates)==2
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,taskType=enum,
 acquisition='generated/resource-snapshots/task-panel-ui-20261003/acquisition-manifest.json',assetManifest='task-panel-ui-import.json',assetCounts=dict(bundles=12,prefabs=len(assets['prefabs']),sprites=len(assets['sprites']),fonts=len(assets['fonts']),nodes={p['name']:len(p['nodes'])for p in assets['prefabs']}),duplicateGraphics=duplicates,
 findings=[
  'TaskItemItemData4366 contains Int32 display progress and Int64 uid. Daily projection retains source config reward list/Lang references, uses FIRST condition row element1 as number and element0 as contentType, sets content argument0, truncates first saved Int64 value with original i64.store32 (not an overwrite of adjacent contentType), and retains old display progress when saved conditions empty.',
  'TaskItemItem SetData publishes Data/callback/type then achievement-only hides liveness parent and sets slider parent530x26/title530x57; switching back to daily does not undo this. ReplaceData only assigns Data. Dispose executes BaseItem destruction/owner clearing, retaining task data/delegate/rect fields.',
  'Refresh writes liveness first; commander/use-prop localized descriptions use CommanderName.argument or Commander.SkillName.(argument%1000). Ordinary branch formats absolute signed target, catches Exception, warns current language/id then continues. Special description failures propagate. Source absolute value uses unchecked bit arithmetic, including Int32 minimum. Reward rendering captures first ID/count before GoodsType/sprite callbacks, supports gold/diamond/configured item6 sprites, blank count at1, then refreshes live progress.',
  'Normal progress clamps upper bound by mutating display data, retains negative values and divides without zero guard. ArenaRank compares target<=negativeAbs(progress), renders zero or absolute target with fill0/1 and ready visibility, without mutating progress. The native Image clamp is retained.',
  'Click disables Button before reading first reward/type. Currency fly uses original count, null root, icon world position, applyInventory=false, top-refresh completion and display=true. Other goods resolve zero ID through required random-reward endpoint; resolved1001 flies old source count, otherwise award popup uses quantity1 inside logged Exception catch. All successful branches schedule localX1600/.7s/ease3 OutSine and completion.',
  'Slide completion enables Button BEFORE ClaimReward then applies current ActionCache visibility. ClaimReward only guards IsClaim (never sets it), mutates shared first reward ID and for daily max(count,randomMinimum), invokes captured callback with current id/uid, then reports current Data id/count and captured reward ID/Type.ToString. Reentry, failure prefixes and source mutation ordering verified. Actual TaskActivity state guard prevents duplicate economy awards.',
  'DailyTaskSubUI OnInit captures actual daily child, registers countdown before Max configured liveness threshold. OnLateInit writes progress before row refresh; source does not set LateInited itself. Filter queries GameValue900001 for EVERY row and excludes only taskid8 when zero. Claimed rows hide with ActionCachefalse; existing rows are not reactivated or removed. No unclaimed filtered rows toggles empty text. Claim forwards strategy(uid,true) before row lookup/hide/progress/red refresh; liveness forwards strategy directly.',
  'Countdown checks root first then unboxes both id/Int64 remaining BEFORE matching required daily ID; converts signed milliseconds/1000 to lowInt32 seconds and calls required original TimeHelper formatter host. OnDestroy clears singleton owner via required callback before listener removal; generic singleton creation and full page lifecycle remain separate work.',
  'Recovered scalar tween accepts explicit easing without modifying default OutQuad. OutSine uses original EaseManager68862 single-precision t*pi/2 multiplication followed by double sine and float conversion. Native scaled time delays actual daily reward until .7s motion completion; disabled native Button suppresses repeated pointer dispatch.',
  'Twelve original resource bundles verified by catalog size/MD5 and SHA256; static TaskPanelUI84-node and TaskItemItem12-node imports preserve original UGUI/assets/outlets. Two original nodes contain duplicate Image components rejected by Unity AddComponent. Importer preserves both using explicit full-rect child adaptation with source IDs and unchanged original sibling order. No original audiovisual equivalence claimed; source particles/animation/preview still pending.'
 ],remaining=['Full TaskPanelUI lifecycle, singleton owner, daily/achievement tab switch, liveness reward preview/claim controls and main entry; Achievement business and page.','Production language/sprite/fly/award/report/random-reward/countdown host assembly and original audiovisual comparison, account/Main/platform/remaining19 controllers/all business, final Player.'])
(out/'TASK_ROWS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index),assets=report['assetCounts'])))
