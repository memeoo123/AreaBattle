"""Record the concrete original offline statistics/codec contract and required refresh seam."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
u, metadata, memory, pairs, types, methods, name = [c[k] for k in ("u", "b", "mem", "pairs", "ts", "md", "ms")]
out = c["p"] / "generated/outgame"
source = []
for row in json.loads((out / "method-map.json").read_text()):
    if row["metadata"] == 35005 or 35039 <= row["metadata"] <= 35042 or 35061 <= row["metadata"] <= 35098 or row["metadata"] in (34787,34793):
        source.append(dict(metadata=row["metadata"], sourceClass=row["cls"], method=row["method"], path=row["path"], sha256=hashlib.sha256((out / row["path"]).read_bytes()).hexdigest()))
fields = []
for index in (3760,4623):
    for k in range(types[index][18]):
        n,t,token=struct.unpack_from("<3i",metadata,pairs[11][0]+12*(types[index][8]+k))
        fields.append(dict(owner=index,name=name(n),offset=u(u(3823136+4*index)+4*k),typeCode=(u(u(200288+4*t)+4)>>16)&255,typeData=u(u(200288+4*t))))
parameters=[]
for index in (34787,34793,34779):
    method=methods[index];values=[]
    for k in range(method[10]):
        n,token,t=struct.unpack_from("<3i",metadata,pairs[10][0]+12*(method[4]+k));pointer=u(200288+4*t);code=(u(pointer+4)>>16)&255;data=u(pointer)
        values.append(dict(name=name(n),typeCode=code,typeData=data,typeName=name(types[data][0]) if code in (17,18) else None))
    parameters.append(dict(metadata=index,method=name(method[0]),parameters=values))
calls=[]
for addr in (3973824,3987604,3955712,4004804):
    encoded=u(addr);assert encoded>>29==6
    spec=struct.unpack_from("<3i",memory,483008+12*((encoded&0x1ffffffe)>>1));method=methods[spec[0]]
    calls.append(dict(usage=addr,methodSpec=list(spec),sourceClass=name(types[method[1]][0]),method=name(method[0])))
literals=[]
for addr in (4106924,4106928):
    encoded=u(addr);index=(encoded&0x1ffffffe)>>1;length,offset=struct.unpack_from("<II",metadata,pairs[0][0]+8*index)
    raw=metadata[pairs[1][0]+offset:pairs[1][0]+offset+length]
    literals.append(dict(usage=addr,rawHex=raw.hex(),text=raw.decode("utf8")))
report=dict(metadataSha256=hashlib.sha256(metadata).hexdigest(),memorySha256=hashlib.sha256(memory).hexdigest(),
    methods=source,fields=fields,refreshParameters=parameters,genericCalls=calls,originalCorruptLiterals=literals,
    findings=[
        "StatistUtils35039/40 uses UTF8, GZipStream(Compress,leaveOpen=true), Close then MemoryStream.ToArray and Base64.35041/42 reads gzip with1024-byte buffer; null/empty strings return empty; failed decode logs Debug.LogWarning with Exception.Message and returns original input.",
        "InitData calls base then Manager.UpdateData(true) before registering six ordinary listeners: Interst,AdsPlayCallBack,ReceiveServerTime,ItemChange,ProductReset,ProductReward. Synchronous local completion occurs before those subscriptions.",
        "LoadData tries JsonUtility.FromJson(gzip-or-original); catch-all retries raw input and does not swallow that second parse failure. Assignment is after successful parse. Fresh data requires server>=1 for firstStart/SetEvent10000; otherwise local time supplies firstStart. Fresh creation marks dirty.",
        "Clock refresh accepts any nonzero server time (including negative), otherwise uses local timestamp. Anchors store original float(realtimeSinceStartup*1000) converted to signed64; non-finite/out-of-range values become MinValue. Now reads clock anchor before realtime, realtime anchor after it, then unchecked arithmetic. HTTP request only if HttpManager3760 instance field12 helper exists. No response is fabricated.",
        "After clock refresh, LoadData invokes stored update callback then starts WaitUntil(pool.IsEnableSaveData) only if coroutine handle is null. Completion does not clear the handle. Resuming registers refresh type0 with lastRefresh, RefreshDay, null second RefreshDelegate and86400 seconds, then Add10902(1).",
        "Refresh APIs are not bool/offset arguments: metadata20643 resolves to RefreshDelegate4584, Invoke takes one Int64. Add(type,last,firstCallback,secondCallback,seconds) and Remove(type,firstCallback,secondCallback,args) both receive null as second callback in this source path.",
        "Update reads scaled delta three times into separate timers. At>=1 subtracts1 once, sets10000 from anchored clock, marks dirty only if dirtyTimer>30 subtracting30 once, checks calendar queries and sends appended-key-only messages on differences. At onlineTimer>=30 subtracts30 once, Add10800(30), then refreshes clock. No catch-up loop or readiness gate in strategy.",
        "RefreshDay writes lastRefresh, marks dirty, Add10901, resets10004/10005/10009/10010/10016/10801 in order, then Set10900 from unchecked deltaMilliseconds*10000 ticks divided by864000000000, narrowed signed32 then widened64.",
        "OnSave checks only records/data non-null, not dirty. It clears dirty before allocating/replacing datas, enumerates live dictionary.Values, normalizes empty item lists to null, omits zero-count/no-item rows from serialized list without removing dictionary entries, then ToJson/gzip/base storage. Failures retain partial changes and cleared dirty.",
        "Interstitial event increments10001 then10004 regardless args; AdsPlayCallBack unboxes bool and only success increments10002 then10005. ItemChange requires two args, int id/long delta, negatives increment spent10007 using unchecked negation, nonnegative including0 increment received10008.",
        "ProductReset only accepts arity1/2 (uses first/second id). ProductReward unboxes second arg even at arity2; arity>=3 adds10017[third]/10018[first] by signed int second; exactly3 resets10019[first], >=4 resets10019[fourth]; arity1/2 resets first. Extra args retain original routing.",
        "10007/10008 providers guard only null, log actual corrupt source strings, and return0; empty arrays fail. Other providers directly query exact parent ids.10000 uses anchored time in release mode and local DateTime in non-release mode.",
        "Cleanup removes Interst,Ads,ItemChange,ProductReset,ProductReward,ServerTime listeners in source order, removes refresh callback with null second callback and Array.Empty args, then stops and clears non-null coroutine handle. Data/clock/timer fields persist."
    ],boundaries=[
        "TimeToRefreshControl4589 full dispatcher/boundary arithmetic is newly extracted (23methods), not yet implemented. Tests explicitly provide its registration/removal endpoint and invoke the daily callback; automatic original cross-day scheduling is not claimed.",
        "Actual SDK/HTTP/platform/Main/account composition remains required. Native test input messages are explicit fixtures for success/failure branches, not claimed platform results.",
        "UTF8 gzip interoperability tested with independently generated Python gzip fixture. Cross-runtime compressed-byte identity and original Unity2021 serialization parity are not claimed; original project pin remains6000.0.68f1, tests run6000.3.7f1."
    ])
(out/"STATISTICS_OFFNET_SOURCE_EVIDENCE.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
print(json.dumps(dict(methods=len(source),fields=len(fields),refreshApis=len(parameters))))
