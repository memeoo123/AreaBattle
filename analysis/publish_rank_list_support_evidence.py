"""Publish narrowly reviewed rank-list support evidence from an isolated extraction."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
p=argparse.ArgumentParser();p.add_argument('validation_root',type=Path);stage=p.parse_args().validation_root.resolve()
root=Path(__file__).resolve().parent.parent;out=root/'analysis/targets/wxcf1394487200e48f/43/generated/outgame';src=stage/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
assert stage!=root
selected={31707,31708,31709,31710,31712,31713,32547,32548,32591,32594,32598}
rows=json.loads((out/'method-map.json').read_text());by={r['metadata']:r for r in rows};methods=[]
for r in json.loads((src/'method-map.json').read_text()):
 if r['metadata']not in selected:continue
 data=(src/r['path']).read_bytes()
 if r['metadata']in by:
  r=by[r['metadata']];held=(out/r['path']).read_bytes();assert held.replace(b'\r\n',b'\n')==data.replace(b'\r\n',b'\n');data=held
 else:shutil.copy2(src/r['path'],out/r['path']);rows.append(r)
 methods.append(dict(r,sha256=hashlib.sha256(data).hexdigest()))
assert len(methods)==11
for name in ('extract_rank_support_generics.py','extract_rank_pool_generics.py'):shutil.copy2(stage/'analysis'/name,root/'analysis'/name)
generics=[]
for name in ('rank-support-generics.json','rank-pool-generics.json','rank-weight-generics.json'):
 if not(out/name).exists():shutil.copy2(src/name,out/name)
 for r in json.loads((out/name).read_text())['methods']:
  if not(out/r['path']).exists():shutil.copy2(src/r['path'],out/r['path'])
  data=(out/r['path']).read_bytes();generics.append(dict(r,sha256=hashlib.sha256(data).hexdigest()))
p=out/'method-map.json';s=json.dumps(sorted(rows,key=lambda r:r['metadata']),ensure_ascii=False,indent=2)+'\n';p.write_bytes(s.replace('\n','\r\n').encode()if b'\r\n'in p.read_bytes()else s.encode())
ctx={'__file__':str(root/'analysis/recover_outgame_manager_registry.py')};exec(Path(ctx['__file__']).read_text().split('rows=[]')[0],ctx)
ts,md,u,ms,b,mem,pairs=[ctx[n]for n in ('ts','md','u','ms','b','mem','pairs')];fields=[]
for owner in (4141,4142,4260,4265):
 for k in range(ts[owner][18]):
  name,typ,tok=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ);fields.append(dict(owner=owner,name=ms(name),typeKind=(u(ptr+4)>>16)&255,typeData=u(ptr),offset=u(u(3823136+4*owner)+4*k)))
findings=[
'RankItemData4260 constructor leaves rankIndex/headBoxId zero and strings null. Clear32548 sets integers0 and countryN/name/score String.Empty. ReferencePool4142 is distinct from framework ReferencePool3449 and the GameObject pools.',
'ReferencePool31706 shared generic resolves type then locked type->collection dictionary. Collection31711 verifies type equality, increments using/acquire counters before queue lock, returns FIFO dequeued object without clearing; if empty, increments added count then generic new(). Release31707 rejects null, optional type validation, resolves collection. Collection31713 invokes Clear BEFORE queue lock and strict duplicate check, enqueues, then increments release/decrements using. Original strict flag defaultsfalse, so repeated release enqueues same identity twice. Exceptions preserve mutations already executed. Owner instance enables isolated test pools; Shared supplies original global app lifetime.',
'RandomHelper26201 invalid null/count<1/count>rows.Count returnsnull. count==rows.Count returns SAME list with no RNG. Otherwise unchecked sum(weight+1), each row captures weight before Managed.Next(0,sum), priority=random+weight+1. Comparator26206 returns unchecked(right.Value-left.Value). Select first requested original row references after sort; does not remove from source. Negative/zero weights and overflow are not repaired.26203 single selection follows cumulative weight and default first row; null logs source message, empty eventually index throws.',
'ConfigHelper.GetPlayerCountry32594 ALWAYS resets playerCountryID=-1 and pendingflagfalse, sets flagtrue BEFORE invoking AppInfoManager online OS country callback. Return currentID immediately; pending yields-1 and synchronous callback can set it. No culture/locale fallback. Callback32591 logs Color(0,1,0,1) countryCode:<code> before config access, scans country rows and code arrays in insertion order using exact equality, sets row.id on match, breaks outer iteration whenever existing currentID!=-1, finally sets flagtrue. Late callback has no generation protection and may overwrite current ID or terminate after first unmatched row due to already-set ID.',
'Full RankControl lifecycle/home/AI-list integration and actual AppInfoManager transport remain pending. These are source prerequisites only, roster21/38 unchanged. Runtime ports do not synthesize platform success.'
]
e=dict(status='rank-list-source-prerequisites-implemented-validation-pending',metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,sharedGenerics=generics,fields=fields,findings=findings)
(out/'RANK_LIST_SUPPORT_SOURCE_EVIDENCE.json').write_text(json.dumps(e,ensure_ascii=False,indent=2)+'\n');print(json.dumps(dict(methods=len(methods),sharedGenerics=len(generics),fields=len(fields),indexedMethods=len(rows))))
