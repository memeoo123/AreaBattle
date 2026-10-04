"""Recover page namespace/defaults, UIPath derivation and DynamicList coroutine evidence."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
out = c["p"] / "generated/outgame"
b, pairs, mem, u, ts, md, name = (c[k] for k in ("b", "pairs", "mem", "u", "ts", "md", "ms"))
namespace = name(ts[4328][1])
assert namespace == "Proj_hdzd.UI.MainMenu"
ui_path = namespace[namespace.rfind(".") + 1:] + "/" + name(ts[4328][0])
assert ui_path == "MainMenu/GuideBookUI"
fields = []
for i in range(ts[3544][18]):
    fn, ft, token = struct.unpack_from("<3i", b, pairs[11][0] + 12 * (ts[3544][8] + i))
    fields.append(dict(offset=u(u(3823136 + 4 * 3544) + 4 * i), name=name(fn)))
defaults = {}
for off in range(pairs[6][0], sum(pairs[6]), 12):
    parameter, typ, data = struct.unpack_from("<3i", b, off)
    defaults[parameter] = (typ, data)
parameters = []
for method in (30054, 30065, 30078, 30097, 34633):
    row = md[method]
    for i in range(row[-1]):
        index = row[4] + i
        fn, token, typ = struct.unpack_from("<3i", b, pairs[10][0] + 12 * index)
        default = defaults.get(index)
        parameters.append(dict(method=method, parameter=index, name=name(fn), hasDefault=default is not None,
            defaultFloat=struct.unpack_from("<f", b, pairs[8][0] + default[1])[0] if default else None))
assert all(p["defaultFloat"] == 0 for p in parameters if p["hasDefault"])
assert not any(p["hasDefault"] for p in parameters if p["method"] == 30078)
usage = u(3941916)
yield_type = u(u(200288 + 4 * ((usage & 0x1ffffffe) >> 1)))
assert name(ts[yield_type][0]) == "WaitForEndOfFrame"
stop = [(i, name(m[0])) for i, m in enumerate(md) if name(ts[m[1]][0]) == "ScrollRect" and m[-2] == 41]
assert len(stop) == 1 and stop[0][1] == "StopMovement"
ids = set(range(33031, 33053)) | set(range(30106, 30112)) | {30054, 30065, 30068, 30078, 30097, 27318, 27320, 27321, 27322, 27323, 27326, 27327, 27328, 27332, 27333, 27334, 27336, 27338, 27339, 27428, 27431, 27432}
methods = [r for r in json.loads((out / "method-map.json").read_text()) if r["metadata"] in ids]
evidence = dict(namespace=namespace, typeName="GuideBookUI", sourcePath=ui_path, resourceRequest="UI/" + ui_path,
    legacyUnload="UI/" + ui_path + ".prefab", baseFields=fields,
    constructor=dict(layer=2, openAnimation=1, closeAnimation=0, openTime=0, closeTime=0, enableLoading=False, cached=False, showTop=True, canReportOpen=True, uiModel="None"),
    centeringParameters=parameters, yieldType=dict(usage=3941916, typeIndex=yield_type, name="WaitForEndOfFrame"), stopMovement=dict(metadata=stop[0][0], slot=41),
    sources=[dict(metadata=r["metadata"], path=r["path"], sha256=hashlib.sha256((out / r["path"]).read_bytes()).hexdigest()) for r in methods],
    findings=[
        "GuideBookUI namespace ends in MainMenu, so inherited BaseUI.UIPath is MainMenu/GuideBookUI; not Proj_hdzd/GuideBookUI.",
        "Awake33045 initializes renderer first, then close/mask/OK/reward handlers, tab delegates, guide dictionary data, tip items, selected guide tab and final red dots. Refresh33042 is empty.",
        "Dispose33052 clears BaseUI fields first, then clears the provider public list, disposes/clears tips, then unsubscribes tab selection delegates. Native DynamicList.OnDestroy retains responsibility for pooled roots.",
        "Page composes existing UIObject initialization, modern/legacy loader completion, open animation, registry CloseSelf and two-frame asynchronous close; only actual owning module services may supply resource/account/platform dependencies.",
        "Single index centering warns then clamps, includes inclusive space prefix/inverse/horizontal negative position, stops ScrollRect movement for immediate writes and explicitly sets dirty.",
        "Pair centering throws on invalid indices, averages without single-item clamp/spacing/inverse, retains positive horizontal direction and special half-visible boundary branches; no explicit dirty write in caller.",
        "Iterator30108 first yields a fresh WaitForEndOfFrame, then advances using scaled Time.deltaTime and Vector2.Lerp. Completion stops the currently held coroutine handle, even if an older routine finishes after another was started; handle is cleared before final position/dirty write."
    ],
    remaining=["Main/account/remaining22 controllers and all business flows; actual page acquisition, SkillControl, localization, atlas, audio/effects platform services; Player and source audiovisual acceptance.",
        "Selection-specific DynamicList subclasses are outside the GuideBook path and remain to be composed when required by their owning business pages."])
(out / "GUIDE_BOOK_PAGE_SOURCE_EVIDENCE.json").write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(methods=len(methods), path=ui_path, yieldType=yield_type)))
