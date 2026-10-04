"""Publish reviewed HTTP helper/agent/task/pool source bodies from an isolated extraction."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent
rel=Path('analysis/targets/wxcf1394487200e48f/43/generated/outgame');out=root/rel;source=stage/rel
def read(p):return json.loads(p.read_text())
def write(p,value):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(value,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
ids=[28308,28317,28321,28322,28329,28330,28331,28335]+list(range(28365,28377))+[28391,28392,28393,28398,28409,28410,28414,28418,28423,28426,28430,28431,28435,28444]+[28511,28512,28513,28515,28516,28517,28519,28520,28521,28522,28523]
index=read(out/'method-map.json');by={r['metadata']:r for r in index};available={r['metadata']:r for r in read(source/'method-map.json')};methods=[]
for id in ids:
    row=available[id];target=out/row['path'];data=(source/row['path']).read_bytes()
    if id in by:
        assert by[id]==row and target.read_bytes()==data,id
    else:
        assert not target.exists()or target.read_bytes()==data,id
        target.write_bytes(data);index.append(row)
    methods.append(dict(row,sha256=hashlib.sha256(data).hexdigest()))
generic=read(source/'web-request-task-pool-generics.json');assert len(generic['methods'])==51
for row in generic['methods']:
    data=(source/row['path']).read_bytes();target=out/row['path'];assert not target.exists()or target.read_bytes()==data
    target.write_bytes(data);row['sha256']=hashlib.sha256(data).hexdigest()
write(out/'web-request-task-pool-generics.json',generic)
contexts=read(source/'web-request-task-pool-rgctx.json');assert len(contexts['rows'])==94
write(out/'web-request-task-pool-rgctx.json',contexts)
# Decode original field offsets directly; no layout inferred from new C#.
ns={'__file__':str(root/'analysis/recover_outgame_manager_registry.py')}
exec((root/'analysis/recover_outgame_manager_registry.py').read_text().split('rows=[]')[0],ns)
ts,md,u,ms,b,mem,pairs=[ns[n]for n in ['ts','md','u','ms','b','mem','pairs']];fields=[]
for owner in [3634,3635,3640,3642,3645,3808,3812,3804]:
    for k in range(ts[owner][18]):
        name,typ,tok=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
        fields.append(dict(owner=owner,sourceClass=ms(ts[owner][0]),name=ms(name),typeIndex=typ,typeKind=(u(ptr+4)>>16)&255,typeData=u(ptr),offset=u(u(3823136+4*owner)+4*k)))
findings=[
    'GET requires both callbacks, logs missing-handler error and creates no request. POST silently returns when either handler is absent. POST uses UploadHandlerRaw and DownloadHandlerBuffer, JSON UTF8 Content-Type, optional X-Encrypted:true, then caller header overrides. Request field is published before handler/header setup; no implicit disposal of a previous request or disposed-state gate.',
    'Unity Update checks real completion/network/http errors and download-handler completion. ReportGameNetInfo is invoked before allocating the callback event, with unscaled f32 milliseconds and original WASM invalid/overflow cast sentinel. Status-code read catches exceptions, logs with another status read, then uses-9999. Success/failure events are newly allocated and Clear only after callback returns. Helper alone retains the request, allowing repeated callbacks; actual agent resets it before notifying manager. Callback/report exceptions skip later work.',
    'Task3645 allocates fresh, increments shared unchecked serial, copies headers, aliases bytes/user data/callback; Clear retains header dictionary. Agent3640 publishes task/Doing before start callback, dispatches null body GET versus nonnull POST, resets elapsed after dispatch. Success/failure resets helper before status/callback, rereads current Task to set Done after callback. Timeout uses unscaled delta and >=deadline, reports Fail/status0/Timeout using configured timeout before reset/callback.',
    'Task pool3812 uses descending priority and stable equal-priority order. Paused gates working update, waiting dispatch and free expiry. Tasks completed during agent.Update are reclaimed on a later pool.Update; reclamation resets agent, returns to free FIFO, removes working node, then clears task. Start publishes working ownership before callback and removes waiting only afterward. Source Start result0/3 recycle and clear,1 retains working,2 recycles but keeps waiting; values outside0..3 retain working and waiting. Capacity is a manager policy; pool itself does not enforce it.',
    'Source GameFrameworkLinkedList3804 caches BCL nodes per-list in FIFO order, clears Value on release and delegates enumeration to BCL. Queue uses saved/current node sequence exactly. RemoveTask searches waiting before active; RemoveAllTasks clears waiting before resetting active. Free agents expire at>=300 scaled seconds, with Shutdown before removal. Generic method contexts independently resolve AddAfter/AddFirst/AddLast/Next/Previous and exact phase order.',
    'This audit covers reviewed canonical methods, not obfuscation aliases with altered constants. WebRequestManager creation/header/debug/callback facade, HttpManager/NetTool/platform domain-key/report upload and production Main/RankUI/OverUI remain pending. Native validation uses an explicit loopback server and test composition; it does not establish external platform success.'
]
evidence=dict(status='source-restored-http-helper-agent-task-queue',metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,genericEvidence='web-request-task-pool-generics.json',genericContexts='web-request-task-pool-rgctx.json',sharedGenericBodies=51,genericContextEntries=94,findings=findings)
write(out/'WEB_REQUEST_SOURCE_EVIDENCE.json',evidence);write(out/'method-map.json',sorted(index,key=lambda r:r['metadata']))
print(json.dumps(dict(normalMethods=len(methods),genericBodies=51,contexts=94,fields=len(fields),indexedMethods=len(index))))
