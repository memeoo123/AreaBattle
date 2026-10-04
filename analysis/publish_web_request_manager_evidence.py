"""Publish reviewed canonical WebRequestManager/HTTP event/factory source evidence."""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;rel=Path('analysis/targets/wxcf1394487200e48f/43/generated/outgame');out=root/rel;source=stage/rel
def read(p):return json.loads(p.read_text())
def write(p,value):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(value,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
ids=[28446,28449,28455,28460,28461,28462,28464,28465,28467,28469,28470,28471,28474,28475,28476,28477,28478,28484,28485,28486,28487,28488,28491,28492,28496,28497,28498,28500,28501,28502,28503,28506,28510]+list(range(28343,28365))+[26581,27665,26090]
index=read(out/'method-map.json');by={r['metadata']:r for r in index};available={r['metadata']:r for r in read(source/'method-map.json')};methods=[]
for id in ids:
    row=by.get(id,available[id]);target=out/row['path']
    if id not in by:
        data=(source/row['path']).read_bytes();assert not target.exists()or target.read_bytes()==data,id;target.write_bytes(data);index.append(row)
    data=target.read_bytes();methods.append(dict(row,sha256=hashlib.sha256(data).hexdigest()))
ns={'__file__':str(root/'analysis/recover_outgame_manager_registry.py')};exec((root/'analysis/recover_outgame_manager_registry.py').read_text().split('rows=[]')[0],ns)
ts,md,u,ms,b,mem,pairs=[ns[n]for n in ['ts','md','u','ms','b','mem','pairs']];fields=[]
for owner in [3644,3636,3637,3638,3639]:
    for k in range(ts[owner][18]):
        name,typ,tok=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
        fields.append(dict(owner=owner,sourceClass=ms(ts[owner][0]),name=ms(name),typeIndex=typ,typeKind=(u(ptr+4)>>16)&255,typeData=u(ptr),offset=u(u(3823136+4*owner)+4*k)))
findings=[
    'Source singleton searches GameObject named WebRequestManager, obtains existing component or adds/publishes it before explicit initialization, then DontDestroyOnLoad. Initialize logs source helper type before replacing task pool and setting30s timeout. Native host owns this lifecycle; platform services factory is an explicit composition boundary.',
    'Manager Update is gated by AppSetting.IsKeWan(field40), not game pause. Debug elapsed uses scaled time, changes>=60 to-1 on a subsequent update; counters reset only on next debug request. Actual queue uses unscaled timeout while game time is frozen.',
    'Core AddWebRequest rejects null/empty URL before creating helper. It creates if total<=capacity and free<=0, while AddAgentHelper rejects total>=capacity. Equality therefore creates a real unused helper without disposing it. The rejection calls CLog.Log category131072 via27665, not CLog.Warning. Original Helper26581 creates a new GameObject and AddComponent(Type).',
    'Object overload28465 uses LitJSON, catches serialization exception, logs literal JSON序列化失败：$ prefix, substitutes{}, UTF8 encodes and debug-logs byte length>14336 before core request. Priority object overload28487 has no catch. String overload28488 has inverted IsNullOrEmpty branch: nonempty produces GET, empty byte POST, null throws during UTF8 encoding. Byte, callback, priority and user-data overloads preserve routing.',
    'SetRequestHeader logs before allocating/changing dictionary; queued tasks copy headers. Start/success/failure allocate fresh events with serial/URI/user-data and bytes/error; invoke current task callback0/1/2 then Clear. Exceptions skip event clear and later task Done. Actual RankTransmitter consumes these events unchanged.',
    'Debug request monitor thresholds5 /data/user/ext,12 /data/private/update and12 /toplist/knock. It matches first Contains route, warns starting with sixth/thirteenth attempt before increment, toasts only at first threshold crossing, and increments after warning/toast. Current /mini-toplist/knock does not match legacy /toplist/knock. Failures preserve prefix and prevent enqueue.',
    'Shutdown first shuts queue/agents, then clears static singleton and schedules owner GameObject destruction. Original unused helper remains outside queue ownership; test fixture explicitly cleans it only after asserting source behavior. Corrected queue empty RemoveFirst exception text to original First is invalid.; earlier1731 audit remains a historical checkpoint.',
    'HttpManager/NetTool platform domain/key/login/server-time and report upload, actual Main/RankUI/OverUI/all business/final Player remain pending. Local native HTTP/AES tests demonstrate transport/owner behavior, not external platform success.'
]
write(out/'WEB_REQUEST_MANAGER_SOURCE_EVIDENCE.json',dict(status='source-restored-manager-events-native-host-platform-pending',metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,dependencies=['WEB_REQUEST_SOURCE_EVIDENCE.json','RANK_TRANSMITTER_SOURCE_EVIDENCE.json'],findings=findings))
write(out/'method-map.json',sorted(index,key=lambda r:r['metadata']));print(json.dumps(dict(methods=len(methods),fields=len(fields),indexedMethods=len(index))))
