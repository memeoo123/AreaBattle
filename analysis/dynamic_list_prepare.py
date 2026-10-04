"""Pin DynamicList registration contexts, native prefab settings and recovered path evidence."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
out = c["p"] / "generated/outgame"
mem, u, ts, md, name = (c[k] for k in ("mem", "u", "ts", "md", "ms"))
assert struct.unpack_from("<4I", mem, 2209104 + 32) == (24, 2207936, 110, 2208224)
tokens = {md[i][6]: str(i) for i in (30086, 30087, 30095)}
tokens[ts[4552][-1]] = "4552"
contexts = []
for r in range(24):
    token, start, count = struct.unpack_from("<3I", mem, 2207936 + 12 * r)
    if token not in tokens:
        continue
    for i in range(count):
        kind, address = struct.unpack_from("<II", mem, 2208224 + 8 * (start + i))
        index = u(address)
        row = dict(owner=tokens[token], token=token, slot=i, kind=kind, dataAddress=address, index=index)
        if kind == 3:
            spec = struct.unpack_from("<3i", mem, 483008 + 12 * index)
            method = md[spec[0]]
            row.update(methodSpec=spec, method=name(ts[method[1]][0]) + "." + name(method[0]))
        contexts.append(row)
assert next(r for r in contexts if r["owner"] == "4552" and r["slot"] == 8)["methodSpec"][0] == 30086
snapshot = json.loads((out / "guide-book-ui-import.json").read_text())
node = next(n for n in snapshot["prefabs"][0]["nodes"] if n["path"] == "guideSV/Viewport/dynamicList")
component = next(c for c in node["components"] if c.get("className") == "DynamicList")
settings = json.loads(component["data"])
assert settings["AutoMask"] == settings["IsNormalList"] == settings["m_direction"] == 0
assert settings["Recycle"] == settings["AutoAdapt"] == 1
methods = [r for r in json.loads((out / "method-map.json").read_text()) if r["metadata"] in (33208,33213) or r["path"].split("/")[-1].split("-")[0] in ("Type3868", "Type4555", "Type4556", "Type4557")]
generic = json.loads((out / "dynamic-list-generic.json").read_text())
paths = sorted({r["path"] for r in methods + generic["methods"]})
evidence = dict(
    metadataSha256=hashlib.sha256(c["b"]).hexdigest(),
    wasmMemorySha256=hashlib.sha256(mem).hexdigest(),
    sources=[dict(path=p, sha256=hashlib.sha256((out / p).read_bytes()).hexdigest()) for p in paths],
    contexts=contexts, guideBookPrefabSettings=settings,
    recovered=[
        "Provider constructor/list/count/indexer/IndexOf, forwarding to generic UpdateList(provider,false), single item update and renderer factory.",
        "Awake child hiding/ScrollRect listeners via explicit recovered-reference BindAwake; InitRendererList stores provider before initialized guard, creates managed items before native roots.",
        "Cached prefab size, axis/alignment/inverse/adaptive columns, inclusive space prefixes, persistent column reduction when AutoMask=false, two extra rows of renderer capacity.",
        "Generic normal-list growth, native slot creation via original PrefabPoolControl, independent item/region/root references, LateUpdate clears dirty before visible processing.",
        "Single updates only refresh bound rows; scrolling reuses first free renderer and does not invoke OnHidden. Empty provider and nonempty offscreen list retain different index sentinels.",
        "Dispose invokes all bound hidden callbacks, then item disposal, then reverse native pool recycling; source retains provider, item/region arrays and initialized state.",
        "GuideBookItem33208 retains its provider, and33213 uses provider.GetData. Corrects the previous fixed-list adapter so replacing the provider public data list remains visible.",
        "GuideBookListBinding uses exact original component settings and dictionary ordering; dynamic-item adapter binds recovered row UI after native spawn. Reward binding accepts provider.UpdateItemData as its required refresh endpoint."
    ],
    remaining=[
        "Centered2Top/Bottom/WithIndex/WithTwoIndex tween paths and provider Centered2Top; selection-specific renderer/provider subclasses.",
        "Full GuideBookUI/BaseUI resource/open/close owner, real Main/account/SkillControl/language/async atlas/audio/effects services, all remaining business flows and Player/original audiovisual acceptance."
    ])
(out / "DYNAMIC_LIST_SOURCE_EVIDENCE.json").write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(sources=len(paths), contexts=len(contexts))))
