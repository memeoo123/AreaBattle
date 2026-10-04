"""Record project RedDotControl/Item source and the actual imported MenuTabUI event."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
out = c["p"] / "generated/outgame"
u, name, ts, md, b, pairs = (c[k] for k in ("u", "ms", "ts", "md", "b", "pairs"))
fields = {}
for typ in (4453, 4457):
    fields[str(typ)] = []
    for i in range(ts[typ][18]):
        fn, ft, token = struct.unpack_from("<3i", b, pairs[11][0] + 12 * (ts[typ][8] + i))
        fields[str(typ)].append(dict(offset=u(u(3823136 + 4 * typ) + 4 * i), name=name(fn), fieldType=ft))
assert u(u(200288 + 4 * ts[4457][4])) == 6829 and name(ts[6829][0]) == "MonoBehaviour"
generics = []
for at in (3976332, 3976336, 3976340, 3976344, 3976348, 3986972, 3989352):
    encoded = u(at)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", c["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    m = md[spec[0]]
    generics.append(dict(usage=at, methodSpec=spec, method=name(ts[m[1]][0]) + "." + name(m[0])))
assert generics[-1]["method"] == "Array.Empty"
assert generics[-2]["method"] == "UnityEvent`1.Invoke"
manifest = json.loads((out / "ui-import.json").read_text())
menu = next(p for p in manifest["prefabs"] if p["name"] == "MenuTabUI")
items = [(n, x) for n in menu["nodes"] for x in n["components"] if x["className"] == "RedDotItemBase"]
assert len(items) == 1
node, component = items[0]
serialized = json.loads(component["data"])
dot = next(n for n in menu["nodes"] if n["sourceId"].split(":")[-1] == str(serialized["DotPrefab"]["m_PathID"]))
call = serialized["CheckActionBool"]["m_PersistentCalls"]["m_Calls"][0]
assert call["m_Target"]["m_PathID"] == 0 and call["m_MethodName"] == "Tower_CheckReddot" and call["m_CallState"] == 2
methods = [r for r in json.loads((out / "method-map.json").read_text()) if 34163 <= r["metadata"] <= 34191]
assert len(methods) == 29
evidence = dict(namespace="Proj_hdzd.RedDot", controllerType=4453, itemType=4457, eventType=4454, parentType=6829,
    fields=fields, generics=generics, serializedItem=dict(node=node["path"], sourceId=component["sourceId"], target=dot["path"], fields=serialized),
    sourceMethods=[dict(metadata=r["metadata"], path=r["path"], sha256=hashlib.sha256((out / r["path"]).read_bytes()).hexdigest()) for r in methods],
    findings=[
        "Project RedDotControl4453 is separate from GameFramework.RedDotModule3655 and its other same-named item base3654. Runtime component4457 derives MonoBehaviour.",
        "Constructor detection mode1; OnInit replaces item List and disables ActiveUpdate, retaining mode and elapsed. InitRedDot sets enabled/mode without resetting time. OnDispose clears singleton only.",
        "Updata adds unscaledDeltaTime only when active. Mode0 refreshes each call and retains elapsed; mode1 requires elapsed>=1 then zeros timer (one refresh, discard overshoot); other modes only accumulate. NaN does not meet threshold.",
        "Source3989352 is Array.Empty<object>, not an external time module. Its result is discarded before live foreach refresh. Component.gameObject is accessed before Unity-null check, then virtual CheackRedDot5 and ShowRedDot4. No snapshot, deduplication, active/disabled filter or per-item recovery.",
        "Add allows duplicates, then logs green with one object argument; Remove checks Contains, removes one and logs even if absent. A null list suppresses both entirely, but null item with initialized list mutates before the gameObject log dereference fails.",
        "Item.Start adds only when CheckActionBool is non-null. No OnEnable/OnDisable registration. OnDestroy resolves controller and removes unconditionally. CheackRedDot resets flag before UnityEvent.Invoke; Show calls DotPrefab.SetActive with current flag. Parameter and property setters do not trigger rendering.",
        "MenuTabUI serialized component has an existing DotPrefab child, Scale one, zero offset, anchor0 and one persistent Tower_CheckReddot call with null target/empty type. Binding preserves that event record and target absence; no invented business callback. Native probe listeners are explicitly test-only.",
        "Actual OutgameMenuView can receive the owning registry resolver and attach this source component before native Start. Source menu logic still hides the item tab; native fixture explicitly activates it to inspect lifecycle, not as an original menu visibility claim."
    ],
    remaining=["Activity/SevenDay/statistics predicates and full Main/account/controller startup, generic framework red-dot module, Player/original audiovisual acceptance remain pending.",
        "Obfuscated alias34168 (HeadportChange logging path), reached only through duplicate private helper34177/34186 among indexed source, contains zero-length object-array allocation followed by an unchecked store at index1. It is not normalized into a successful logger; exact unused WASM/managed boundary remains outside restored public menu lifecycle. Equivalent private/property aliases are represented by their shared core behavior, not exported obfuscated identifiers."])
(out / "RED_DOT_SOURCE_EVIDENCE.json").write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(methods=len(methods), menuNode=node["path"], event=call["m_MethodName"], missingTarget=True)))
