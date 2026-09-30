"""Opcode-validated references to global effect cleanup; never executes target WASM."""
from pathlib import Path
src=Path(__file__).with_name('combat_lifecycle_audit.py').read_text(encoding='utf8')
ns={'__file__':str(Path(__file__).with_name('combat_lifecycle_audit.py'))}
exec(src[:src.index('queries=')],ns)
ns['queries']=[('effect-shutdown-direct','wasmcode1',16,64168),('effect-shutdown-table',None,65,24648),('effect-clearchild-direct','wasmcode1',16,8643),('effect-clearchild-table',None,65,24649)]
ns['queries'] += [('tag-unload-direct','wasmcode1',16,45985),('tag-unload-table',None,65,20595),('force-unload-direct','wasmcode1',16,46006),('force-unload-table',None,65,20649)]
exec(src[src.index('queries=list'):src.index("b=(ROOT/'work/webdata")],ns)
ns['OUT'].joinpath('combat-visual-lifecycle-references.json').write_text(ns['json'].dumps(ns['results'],ensure_ascii=False,indent=2),encoding='utf8')
for r in ns['results']:
 print(r['query'],r['module'],ns['json'].dumps([(h['function'],h['names']) for h in r['verifiedInstructionHits']],ensure_ascii=True))
