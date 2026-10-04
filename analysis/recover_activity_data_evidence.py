"""Mirror newly extracted activity bodies without rewriting older disassembly and record data-path evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path)
validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)}
exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
index=json.loads((out/'method-map.json').read_text());known={r['metadata'] for r in index}
classes={'ActivityData','CommonModuleManagerBase','ActivityNetStrategyBase','ActivityOffNetStrategy','ActivityItemData','ActivityUtils','PubActivityConfig','PubActivityConfigFormatter'}
added=[]
for row in json.loads((source/'method-map.json').read_text()):
    if row['cls'] in classes and row['metadata'] not in known:
        shutil.copy2(source/row['path'],out/row['path']);index.append(row);known.add(row['metadata']);added.append(row['metadata'])
if added:
    path=out/'method-map.json';original=path.read_bytes();text=json.dumps(index,ensure_ascii=False,indent=2)
    if original.endswith(b'\n'):text+='\n'
    if b'\r\n' in original:text=text.replace('\n','\r\n')
    path.write_bytes(text.encode())
selected={35261,35267,*range(35270,35274),*range(35291,35303),*range(35304,35319)}
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected]
assert len(methods)==33
fields=[]
for owner in (4635,4641,4642,4658,4660):
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
        fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
generics=[]
for usage in (4004748,3957608,3966600,3966604,3971888,3971892,3971896,3987716):
    encoded=c['u'](usage);assert encoded>>29==6
    spec=struct.unpack_from('<3i',c['mem'],483008+12*((encoded&0x1ffffffe)>>1));m=c['md'][spec[0]]
    generics.append(dict(usage=usage,methodSpec=list(spec),sourceClass=c['ms'](c['ts'][m[1]][0]),method=c['ms'](m[0])))
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,generics=generics,
    collectionTypes=[dict(genericClass=3705900,type='Dictionary<object,PubActivityConfig4635>'),dict(genericClass=3758316,type='List<ActivityItemData4642>')],
    findings=[
        'CommonModuleManagerBase DataKey35304 concatenates CommonGameModule and runtime type name;35305 InitStrategy is empty;35306 SaveData forwards to virtual SaveLocalData. Restored names use explicit original SourceClassName to retain original save keys.',
        'ActivityManager OnInit35294 empty; Init35299 invokes InitData35296 which replaces strategy with concrete ActivityOffNetStrategy, assigns Manager before InitData(RefreshData). OffNet35315 stores callback then calls Manager.UpdateData(true).',
        'RefreshData35297 assigns Data before null check. Null logs one argument 活动数据为null and returns; nonnull ManagerInit then invokes ActivityControl35228. Source activity ID defaults0. OnSave35300 forwards latest data only if strategy exists. OnRelease35301 only clears isInitStrategy; retains data/strategy/dirty.',
        'ManagerInit35298 checks isInitStrategy then resolves registration delegate twice (test and invocation). Without delegate, original Assembly.GetTypes order, IsSubclassOf, then Activator construction BEFORE config null check. Config dictionary is reread, activityID checked as boxed key; AddModel uses actual runtime type and source attribute, then child.InitStrategy even on duplicate. Ready flag set after loop/delegate; failures/reentry retain original partial state.',
        'ActivityData firstLoginDay is int, lastRefreshTimeStamp long and constructor allocates List<ActivityItemData>. Item ctor35267 state=1; ID int, uniqueId STRING, flags bool, LaunchTimeStamp/WarmTimeStamp long. Private lazy config/condition caches are not part of the persisted DTO implementation.',
        'ActivityUtils35270..73 UTF8 gzip/base64,1024-byte read buffer, compress leaveOpen=true and Close before ToArray. Unlike StatistUtils, streams are disposed via nested finally scopes. Empty string returns empty; failed string decompression logs 解压缩出错: plus exception.Message and returns original input.',
        'OffNet.Load35318 catches Object around decompression/FromJson<ActivityData> and retries raw JSON. Inner catch Exception logs Message and StackTrace then leaves null data; recovery creates fresh data. Logging failures propagate. This differs from statistics strategy, whose second parse failure propagates.',
        'Fresh activity data enumerates current config Values without condition checks, adds default state1 rows with config.id/config.uniqueId, then TimeModule.GetNowTimeInt into firstLoginDay, then sets activity dirty. Existing rows/clocks are retained, unmatched configs append records and mark dirty; orphan and duplicate save records are not removed. Only first matching ID is examined; old uniqueId is not overwritten.',
        'Existing matching row: if launch conditions true, only state4 with over false resets to1. Else if notice conditions true, state4 with over false resets, or state3 rechecks launch and resets only if still false. Other states/conditions unchanged. Each reset assigns state before SetDirty(true). No close-condition evaluation and no clearing popup/timestamp fields. Callback invoked only after all reconciliation succeeds, nullable delegate allowed.',
        'Conditions35309 loops live types, reads parameters[i] first; ID10000 exact yyyyMMddHHmmss invariant DateTimeStyles.None. Invalid parse skipped. Otherwise int.TryParse (overflow ignored) then signed current>=target. Date branch reads statistics10000 before converting date to original timestamp; Array.Empty<object> for providers. Empty conditions true, malformed arrays fail, repeated state3 query permits provider reentry.',
        'OnSave35317 serializes supplied Data directly, compresses, and calls Manager.SaveLocalData; no data guard, pruning or dirty reset. Tests use actual inherited storage/pool and a new backend graph for restart. Registry source attribute4658 is CommonGameModule/autoSyn=true/compressData=false.'
    ],boundaries=[
        'ActivityControl and ActivityConfigMgr full runtime/config selection, condition caches and activity subclasses/SevenDay.EnterGameInit are pending. Required callbacks/config getter/reflected source factories are explicit hosts; Editor fixtures are not the production Main graph.',
        'Original assembly identity/order and runtime source types must be provided by the production common-manager factory mapping. No default empty production registration or fabricated server response is supplied.',
        'No fresh native PlayMode, Player or original audiovisual acceptance in this data-only milestone; controller lifecycle binding count remains18/38.'
    ])
(out/'ACTIVITY_DATA_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(newMethods=len(added),indexedMethods=len(index),evidenceMethods=len(methods),fields=len(fields),generics=len(generics))))
