"""Publish reviewed NetTool/key envelope/LoginTransmitter source; exclude altered aliases."""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;rel=Path('analysis/targets/wxcf1394487200e48f/43/generated/outgame');out=root/rel;source=stage/rel
def read(p):return json.loads(p.read_text())
def write(p,value):
    old=p.read_bytes()if p.exists()else b'';s=json.dumps(value,ensure_ascii=False,indent=2)+'\n';p.write_bytes((s.replace('\n','\r\n')if b'\r\n'in old else s).encode())
ids=[28199,29098,29105,29106,29110,29111,29112,29113,29229,29230,29233,29234,29236,29237,29239,29241,29242,29244,29245,29253,29254,29256,34268]
index=read(out/'method-map.json');by={r['metadata']:r for r in index};available={r['metadata']:r for r in read(source/'method-map.json')};methods=[]
for id in ids:
    row=by.get(id,available[id]);target=out/row['path']
    if id not in by:
        data=(source/row['path']).read_bytes();assert not target.exists()or target.read_bytes()==data,id;target.write_bytes(data);index.append(row)
    methods.append(dict(row,sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
ns={'__file__':str(root/'analysis/recover_outgame_manager_registry.py')};exec((root/'analysis/recover_outgame_manager_registry.py').read_text().split('rows=[]')[0],ns)
ts,md,u,ms,b,mem,pairs=[ns[n]for n in ['ts','md','u','ms','b','mem','pairs']];fields=[]
for owner in [3138,4476,3762,3732,3733,3734,3735]:
    for k in range(ts[owner][18]):
        name,typ,tok=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
        fields.append(dict(owner=owner,sourceClass=ms(ts[owner][0]),name=ms(name),typeIndex=typ,typeKind=(u(ptr+4)>>16)&255,typeData=u(ptr),offset=u(u(3823136+4*owner)+4*k)))
findings=[
    'NetTool34268 caches first nonempty platform result. NetDtype absent/empty/exact0 defaults RD1; other strings use TryParse out value, including invalid->0 and spaced0->0. Warning boxes original enum RD1/TD2/RTD3 before querying SDK with hcrzd-u-cn-wx and default. RD adds https:// and :20150 even for null result. Assignment precedes final warning; failed lookup is retryable, failed final warning retains cache.',
    'PngDataHandler28199 decodes Base64 UTF8, subtracts signed(timestamp/26)%26 from UTF16 units, reverses, parses two-character length, removes prefix, subtracts signed timestamp%26, reverses again and truncates to length. Catch Exception logs Decoding error: plus Message then returns empty; logging exceptions escape. No AES/report-error call in this decoder. Independent fixed vectors cover signed shifts, length truncation, Chinese and UTF16 surrogate pairs.',
    'Base SetUrl29098 publishes URL before reading global encryption setting and ignores supplied bool. The prior single-argument convenience API remains; two-argument source signature added. Corrected Update29105 to parameterless virtual slot6 and Dispose29106 to virtual slot4, preparing original owner dispatch.',
    'LoginTransmitter29245 constructs all18 URLs before registration, binds protocols2..19 to source paths/callbacks, and keeps duplicate-registration behavior. Actual HttpNetAcion business response handlers remain explicit composition dependencies; this milestone supplies no fake login success.',
    'Canonical queue29229 overwrites repeated keys; only transition from no-pending sets countdown3. Updates29253/29254 decrement3->2->1->0, send on fourth, clear pending BEFORE Send4 and replace dictionary only AFTER normal return. Throw preserves old dictionary with pending false; disposed/missing protocol normal -1 still clears data. Reentrant queue during send mutates outgoing alias, sets pending and countdown, then loses data on replacement, preserving source behavior.',
    'Immediate upload29233 sends RequestSendGameData.Datas by alias and returns transport serial. Canonical requests use server time2/null, login3/object, versions12/RequestList.keys, player-data6/keys, cutover14/userId+guestUserId+guestUid, user extension16/userExt and uploaded18/null. DTO3733 constructor allocates dictionary before caller replaces it. Versions29239 enumerates live pool.Values, filters AutoSyn, preserves order/duplicates/null keys and propagates null/mutation failures.',
    'Altered obfuscation aliases such as29231/29232/29238 and Decode28203 are excluded. HttpManager owner/global key mutation, ServerTimeSync, HttpNetAcion business callbacks, real platform SDK/report composition, Main, remaining16 controllers and final Player/original audiovisual acceptance remain pending.'
]
write(out/'LOGIN_TRANSPORT_SOURCE_EVIDENCE.json',dict(status='source-restored-login-transmitter-domain-key-decoder-owner-platform-pending',metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,domainEnum=dict(RD=1,TD=2,RTD=3),dependencies=['WEB_REQUEST_MANAGER_SOURCE_EVIDENCE.json','RANK_TRANSMITTER_SOURCE_EVIDENCE.json'],findings=findings))
write(out/'method-map.json',sorted(index,key=lambda r:r['metadata']));print(json.dumps(dict(methods=len(methods),fields=len(fields),indexedMethods=len(index))))
