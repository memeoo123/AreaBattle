"""Record source activity config loading and factory/child API; preserve unrelated indexes."""
from pathlib import Path
import argparse,json,hashlib,struct,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
index=json.loads((out/'method-map.json').read_text());known={r['metadata'] for r in index};added=[]
selected=set(range(35281,35291))|set(range(35187,35197))
for row in json.loads((source/'method-map.json').read_text()):
    if row['metadata'] in selected and row['metadata'] not in known:
        shutil.copy2(source/row['path'],out/row['path']);index.append(row);known.add(row['metadata']);added.append(row['metadata'])
if added:
    path=out/'method-map.json';raw=path.read_bytes();text=json.dumps(index,ensure_ascii=False,indent=2)+('\n' if raw.endswith(b'\n') else '')
    path.write_bytes((text.replace('\n','\r\n') if b'\r\n' in raw else text).encode())
selected.update([35131,35134,35135,35138,35139,35142,35144,35149,35162])
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected]
generics=json.loads((source/'activity-owner-generics.json').read_text());assert len(generics['methods'])==4
for row in generics['methods']:
    shutil.copy2(source/row['path'],out/row['path']);row['sha256']=hashlib.sha256((out/row['path']).read_bytes()).hexdigest();row['implemented']=row['metadata'] in [35163,35194]
(out/'activity-owner-generics.json').write_text(json.dumps(generics,ensure_ascii=False,indent=2)+'\n')
fields=[]
for owner in (4565,4626,4632,4656):
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
        fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,genericEvidence='activity-owner-generics.json',
    findings=[
        'ActivityConfigMgr ReadConfig35138 stores completion, replaces reader with concrete4633, initializes reflected subclasses of ActivityConfigCustomMgr, then initializes reader. Custom list is appended; Expected starts2 and adds entire live list count before OnInit iteration. Abstract base/non-subclasses skipped, construction errors propagate. Common completion normalizes current singleton Activities then adds2; custom completion adds1. Callback only on equality, no once guard/counter reset on read.',
        'CompleteCommonConfig35135 enumerates current owner Activities.Values and for exactly open1 replaces empty closeType with[-1]/closeParams[999999], then empty overType similarly. Null arrays/entries or enumeration mutation throw, preserving prefix. Completed increment happens only after enumeration finishes. Receiver owns callback/counters even if current manager differs.',
        'GetActivityConfig35131 TryGetValue boxed Int32 ID then logs two arguments on null, including existing null. Dispose35139 clears activity dictionary, disposes reader, live custom list null-safe items, clears custom list, then resets Completed0/Expected2. Retains Settings, reader and callback; release failure prevents later effects. Constructor35144 also has business dictionaries which are explicitly not restored by this common-loading milestone.',
        'Reader4633.35196 captures callback, gets online common_ActivityConfig. Null/empty uses independently selected ConfigRead26610 or26615 table route; nonempty parses online generic35194 from first brace into list and adds by id with duplicate-error/keep-first behavior. Whitespace without brace throws. Settings independently rereads binary flag and uses26614/26627. Method invokes original callback argument after settings assignment, even if stored CompleteAction changed.',
        'Concrete reader Dispose35193 is an original no-op. Base reader35190 only stores callback; custom manager OnInit/OnDispose are abstract (no body). Explicit Unity JSON adapter uses original PubActivityConfig/PubActivitySettingConfig names and first-brace parsing; original recovered resource rows/settings used in verification. Binary source is a required endpoint, not a fallback or completed MemoryPack implementation.',
        'ActivityFactoryBase35286 resolves ItemConfigMgr twice on success, logs missing item without throwing, stores shared row. Produce35287 dispatches type1 1/2/9 to currency/exp/package factories with config.id; each constructor rereads current catalog. Their original Produce35282/35284/35290 bodies all return null. Unsupported type1 returns null, missing base config dereference throws. No invented reward entity.',
        'ActivityBase35162 virtual slot4 returns a new factory. GetChildActivity35163 checks ChildrenById.ContainsKey then indexes and casts to runtime T; missing/default and stored-null return null, incompatible type throws. Both now available on recovered base.',
        'Verification composes actual recovered online config manager, normalized conditions, ActivityManager/offline strategy/file storage, statistics, ActivityBase/four states and UGUI widgets. New owner/config/data graphs reload saved notice state/warm timestamp from actual compressed file. Control/pop/UI endpoints remain explicit fixture boundaries.'
    ],boundaries=[
        'Business task/achievement/liveness/novice dictionaries and grouping/query surfaces of ActivityConfigMgr remain pending; constructor evidence includes these fields without claiming their implementation.',
        'ActivityControl owner/reflection/pop queues, concrete custom config managers/activities, SevenDay/ProcedurePreLoad/Main and original page integration remain pending. Generic ActivityControl.GetActivity35231/35236 extracted but explicitly not implemented this milestone.',
        'No new native or Player run; prior activity state native10 remains historical evidence. Controller lifecycle registry unchanged18/38.'
    ])
(out/'ACTIVITY_OWNER_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),fields=len(fields),genericMethods=len(generics['methods']))))
