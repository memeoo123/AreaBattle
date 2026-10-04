"""Trace BattleControl's three captured DispatchConfig rows and original branches."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
out = c["p"] / "generated/outgame"
u, name, ts, md, b, pairs = (c[k] for k in ("u", "ms", "ts", "md", "b", "pairs"))
encoded = u(3957220)
assert encoded >> 29 == 6
spec = struct.unpack_from("<3i", c["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
method = md[spec[0]]
assert name(ts[method[1]][0]) == "Dictionary`2" and name(method[0]) == "get_Item"
fields = {}
for typ in (3907, 3937, 4060):
    rows = []
    for i in range(ts[typ][18]):
        fn, ft, token = struct.unpack_from("<3i", b, pairs[11][0] + 12 * (ts[typ][8] + i))
        offset = u(u(3823136 + 4 * typ) + 4 * i)
        if typ != 3907 or name(fn) == "dicDispatch":
            rows.append(dict(offset=offset, name=name(fn), fieldType=ft))
    fields[str(typ)] = rows
assert fields["3907"] == [dict(offset=28, name="dicDispatch", fieldType=4617)]
assert [r["offset"] for r in fields["4060"]] == [8, 12, 16]
methods = [r for r in json.loads((out / "method-map.json").read_text()) if 31246 <= r["metadata"] <= 31253]
assert len(methods) == 8
evidence = dict(typeIndex=4060, className="BattleControl", fields=fields,
    dictionaryGetter=dict(usage=3957220, methodSpec=spec, declaringType="Dictionary`2", method="get_Item"),
    methods=[dict(metadata=r["metadata"], path=r["path"], sha256=hashlib.sha256((out / r["path"]).read_bytes()).hexdigest()) for r in methods],
    findings=[
        "OnInit31249 resolves ConfigMgr separately for each dictionary lookup1/2/3, assigning instance fields8/12/16 after each successful get_Item. Missing keys throw; earlier assignments persist and later old references remain.",
        "Getters select field8 for grade0, field12 for grade1 and field16 for every other integer, including negatives. They observe captured row mutation but not dictionary replacement until reinitialized.",
        "GetSpawnTime31252 selects swanpSpaceOne for lines1, swanpSpaceTwo for2 and swanpSpaceThree for all others. It casts the signed integer to float before dividing by1000f; no clamp or validation.",
        "GetDispatchAddScoreTime31253 uses addSpace cast to float then /1000f. Score and line count are returned without modification.",
        "Updata31246 is an empty source body. OnDispose31250 unconditionally clears the singleton slot and retains all captured config fields; an old instance may clear a newer registry instance.",
        "Existing BattleSimulation already independently implements these formulas. New validation checks both against recovered config, while preserving the current battle implementation and user data."
    ], remaining="Actual Main/account/all-controller composition, battle owner routing and full business/Player acceptance remain pending.")
(out / "BATTLE_CONTROLLER_SOURCE_EVIDENCE.json").write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(methods=len(methods), sourceType=4060, dictionaryMethod=spec[0])))
