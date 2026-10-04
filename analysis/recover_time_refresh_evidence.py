"""Record original4589 public paths, DTOs and scaled async cleanup without alias substitution."""
from pathlib import Path
import hashlib
import json
import struct

helper=Path(__file__).resolve().parent/"recover_outgame_manager_registry.py"
c={"__file__":str(helper)}
exec(helper.read_text().split("rows=[]")[0],c)
u,b,mem,pairs,ts,md,ms=[c[k] for k in ("u","b","mem","pairs","ts","md","ms")]
out=c["p"]/"generated/outgame"
methods=[]
for row in json.loads((out/"method-map.json").read_text()):
    if 34782<=row["metadata"]<=34804:
        methods.append(dict(metadata=row["metadata"],sourceClass=row["cls"],method=row["method"],path=row["path"],sha256=hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()))
assert len(methods)==23
fields=[]
for owner in range(4586,4590):
    for k in range(ts[owner][18]):
        n,t,token=struct.unpack_from("<3i",b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*t)
        fields.append(dict(owner=owner,name=ms(n),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr)))
params=[]
for index in (34785,34787,34788,34793):
    method=md[index];values=[]
    for k in range(method[10]):
        n,token,t=struct.unpack_from("<3i",b,pairs[10][0]+12*(method[4]+k));ptr=u(200288+4*t);code=(u(ptr+4)>>16)&255;data=u(ptr)
        values.append(dict(name=ms(n),typeCode=code,typeData=data,typeName=ms(ts[data][0]) if code in (17,18) else None))
    params.append(dict(metadata=index,method=ms(method[0]),parameters=values))
cleanup=[]
for addr in (3941920,3940196):
    encoded=u(addr);index=(encoded&0x1ffffffe)>>1;data=u(u(200288+4*index))
    assert encoded>>29==1
    cleanup.append(dict(usage=addr,type=data,name=ms(ts[data][0])))
assert cleanup[0]["name"]=="WaitForSeconds"
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,parameters=params,cleanupTypes=cleanup,
    findings=[
        "Public Update34784 accumulates scaled delta, tests >=1 and subtracts1 once. Release server time is accepted only when >0, otherwise local DateTime.Now converted by original epoch helper. Non-release never reads server time.",
        "Due means now-last >= interval, not calendar date inequality. Due boundary is baseDate1/1/1 +years/months/days +hour, falling back to previous day if boundary is future. No hour/interval sanitization. Short intervals can repeatedly refresh the identical day boundary.",
        "Due commits LastRefreshTime before callbacks. Ordering: V1 refresh; calculate remaining using latest row fields; V1 countdown; directV2 refresh/countdown with Array.Empty; enumerate keyed refresh and associated current countdown. Then recalculate remaining and run V1/directV2/keyed countdown again for every row, including due rows.",
        "Keyed countdown iteration uses refresh dictionary keys only. Null keyed refresh throws. Live dictionary iteration and live group/record index traversal retain mutation, reentry and exception behavior; callback failures propagate with committed prefix state.",
        "Add34787/88 groups by hour and merges first row matching exact DateTime conversion plus TimeSpan(seconds*10000000L). No immediate callback. V1 delegates combine. V2 null args use direct delegates; nonnull arrays are reference-identity dictionary keys. Existing-row keyed countdown branch combines the refresh argument instead of countdown when key already exists; preserved source quirk.",
        "RemoveV134793 checks null refresh BEFORE subtraction: remove first already-null row and return entire method, otherwise subtract callbacks across live matching group rows. Newly nulled rows and empty groups remain. Args unused.",
        "RemoveV234785 requires hour/date/interval match. Null args subtract direct delegates; nonnull args remove entire key regardless passed delegate. Remove row/return when directV2 refresh null and keyed refresh empty, ignoring V1 and countdown subscribers. Empty groups remain.",
        "DTO34801 allocates record list;34802 allocates both dictionaries. Singleton34799 finds/creates exact TimeToRefreshControl root, reuses/adds component, subscribes GameFrameEntry disposableActions and ordinary ExitGame clear, DontDestroyOnLoad. Does not reset static timestamp/groups.",
        "OnDestroy34783 clears groups and subtracts async disposal delegate; it does not remove ExitGame listener or explicitly null static instance. Static Unity-null comparison enables future recreation.",
        "Async34803 awaits new WaitForSeconds(.1f), proven usage3941920 type6854. After await completion it clears static groups; does not destroy singleton/unsubscribe. Restored through existing original-compatible OutgameUnityAwait, preserving scaled pause behavior. ExitGame handler34797 clears immediately."
    ],boundaries=[
        "Restored normal public API/Unity messages; obfuscated alternate loops34786/89/90/92/94/95/96/34800 are retained as evidence and not substituted for public paths or asserted equivalent.",
        "SDK release/time and GameFrameEntry ordinary message/disposable accessors remain required explicit host dependencies. BindStatistics wires actual refresh singleton to concrete offnet services; complete production Main/account/activity composition remains pending.",
        "Native tests use controlled SDK timestamps and explicit result messages; actual scheduler Update, offnet coroutine, compressed file IO/restart and scaled cleanup run in Unity. No platform-success, Player or audiovisual claim."
    ])
(out/"TIME_REFRESH_SOURCE_EVIDENCE.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
print(json.dumps(dict(methods=len(methods),fields=len(fields),parameters=len(params),cleanup=cleanup)))
