"""Record activity runtime condition caches and the original FSM shared-generic event surface."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
index=json.loads((out/'method-map.json').read_text());known={r['metadata'] for r in index};added=[]
classes={'ActivityBase','ActivityStateBase','ActivityCloseState','ActivityNoticeState','ActivityLaunchState','ActivityOverState'}
for row in json.loads((source/'method-map.json').read_text()):
    if row['cls'] in classes and row['metadata'] not in known:
        shutil.copy2(source/row['path'],out/row['path']);index.append(row);known.add(row['metadata']);added.append(row['metadata'])
if added:
    path=out/'method-map.json';raw=path.read_bytes();text=json.dumps(index,ensure_ascii=False,indent=2)
    if raw.endswith(b'\n'):text+='\n'
    path.write_bytes((text.replace('\n','\r\n') if b'\r\n' in raw else text).encode())
selected={35262,35263,35264,35265,35266,35369}
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected];assert len(methods)==6
generics=json.loads((source/'fsm-event-generics.json').read_text());assert len(generics['methods'])==13
for row in generics['methods']:
    shutil.copy2(source/row['path'],out/row['path']);row['sha256']=hashlib.sha256((out/row['path']).read_bytes()).hexdigest()
(out/'fsm-event-generics.json').write_text(json.dumps(generics,ensure_ascii=False,indent=2)+'\n')
fields=[]
for owner in (4642,4643,3767):
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
        fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,genericEvidence='fsm-event-generics.json',
    findings=[
        'ActivityItemData.Config35262 caches nonnull ActivityConfigMgr.GetActivityConfig(id); null retries, changing id/owner catalog does not invalidate previously cached reference. Required application-domain ConfigurationResolver exposes that original lookup dependency, with no default catalog.',
        '35263..35266 use identical logic and separate caches44/48/52/56 for notice/launch/over/close. Each allocates Condition[] using PARAMETER length before parsing, repeatedly reads cached Config fields, writes key before converting. Short type arrays or null text retain partially filled cache; failures before allocation can retry. No cache repair on later access.',
        'Condition4643 is a value type with int key, long value and object[] arg (unboxed layout size24). Type10000 exact invariant yyyyMMddHHmmss yields original timestamp or0; leaves arg null. Other types Split underscore, allocate args length-1, int.TryParse every preceding token (invalid/overflow becomes boxed0), long.TryParse last token (invalid/overflow becomes0). No logging or silent condition skipping on this runtime path.',
        'ActivityStateBase35369 checks owner.config.open first; any nonzero enabled,0 returns false without array reads. Loops supplied type IDs rather than cached keys, passes cached arg array identity to GameStatisticsExpansion.GameValue and reads cached target after provider returns. Stops at first actual<target; empty enabled array true. This differs from ActivityNetStrategyBase35309 plain numeric/date loading-time checks.',
        'Fsm FireEvent29267/29268 captures current state, rejects null with Current state is invalid., calls state.OnEvent with same FSM/sender/eventId and null or exact userData. Event dispatch does not change current time, catch errors or queue work.',
        'FsmState29292 owns new Dictionary<int,FsmEventHandler<T>>. Subscribe29299 rejects null handler, sets missing key or Delegate.Combine existing value. Unsubscribe29300 rejects null even when key missing, Delegate.Remove one matching occurrence, retains null-valued key. OnEvent29302 TryGetValue then invokes captured nonnull multicast delegate, preserving snapshot/reentry and exception stop order.',
        'Base state29293..29297 methods, including BOTH OnEnter overloads, are independent no-ops. OnDestroy29298 clears event dictionary, without FSM validation. ChangeState29301 casts original IFsm to concrete FSM, throws FSM is invalid. on null/incompatible, otherwise forwards generic target/args. Recovered API already uses concrete OutgameFsm and therefore preserves null validation/forwarding without an alternate IFsm implementation surface.',
        'Verification includes original local PubActivityConfig fields, private-cache exclusion from JSON and reconstruction; actual recovered statistics item count10700 with parsed argument8/target4 drives real OutgameFsm transition and FsmManager shutdown. The state scenario is an Editor fixture, not the full activity-state restoration.'
    ],boundaries=[
        '76 ActivityBase/four concrete states/base-state methods extracted for subsequent implementation; extraction is not completion. Only runtime condition method35369 is implemented in OutgameActivityConditions in this milestone.',
        'Full ActivityControl/config owner/read lifecycle/ActivityBase/state-specific widgets/common-statistics listeners/SevenDay/ProcedurePreLoad/Main are pending. Legacy non-event IOutgameFsmState adapters have no registered event handlers and dispatch as an empty base state.',
        'No native PlayMode or Player in this data/framework milestone. No activity UI or original audiovisual claim. Controller bindings remain18/38.'
    ])
(out/'ACTIVITY_CONDITIONS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(newMethods=len(added),indexedMethods=len(index),methods=len(methods),fields=len(fields),sharedGenerics=len(generics['methods']))))
