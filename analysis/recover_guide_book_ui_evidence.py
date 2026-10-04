"""Record original guide-book UI methods/fields and imported resource provenance."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
context = {"__file__": str(helper)}
exec(helper.read_text(encoding="utf8").split("rows=[]")[0], context)
u, metadata, pairs, types, name = [context[k] for k in ("u", "b", "pairs", "ts", "ms")]
output = context["p"] / "generated/outgame"
classes = []
for index in (4328, 4344, 4345, 4369, 4368):
    fields = []
    for field in range(types[index][18]):
        field_name, type_index, token = struct.unpack_from("<3i", metadata, pairs[11][0] + 12 * (types[index][8] + field))
        pointer = u(200288 + 4 * type_index)
        fields.append({"name": name(field_name), "offset": u(u(3823136 + 4 * index) + 4 * field),
                       "typeCode": (u(pointer + 4) >> 16) & 255, "typeData": u(pointer)})
    classes.append({"typeIndex": index, "name": name(types[index][0]), "fields": fields})
methods = [{"metadata": row["metadata"], "class": row["cls"], "method": row["method"],
            "path": row["path"], "sha256": hashlib.sha256((output / row["path"]).read_bytes()).hexdigest()}
           for row in json.loads((output / "method-map.json").read_text())
           if 33031 <= row["metadata"] <= 33052 or 33208 <= row["metadata"] <= 33218 or 33387 <= row["metadata"] <= 33405]
assert len(methods) == 52
calls = []
for address in (3953568, 3953376, 3953392, 3962780, 3962776, 3962784, 3997108, 3979380, 4018304):
    encoded = u(address)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", context["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    method = context["md"][spec[0]]
    calls.append({"usageAddress": address, "methodSpec": list(spec), "class": name(types[method[1]][0]), "method": name(method[0])})
report = {
    "metadataSha256": hashlib.sha256(metadata).hexdigest(), "memorySha256": hashlib.sha256(context["mem"]).hexdigest(),
    "classes": classes, "methods": methods, "genericCalls": calls,
    "resourceAcquisition": "../resource-snapshots/guide-book-ui-20261003/acquisition-manifest.json",
    "resourceImport": "guide-book-ui-import.json", "resourceHierarchy": "guide-book-ui-evidence.json",
    "implementedFindings": [
        "33032 plays voice2001 before null/claimed guards, reads reward[0]/[1], calls ToolChange(id,amount,false,guidebook,true) and ignores its bool.",
        "Only effect count is clamped10..100. Currency1001/1002 uses FlyMoney/FlyDiamonds(pageTransform, button world position, applyInventory=false, completion=null, updateDisplayedValue=true).",
        "After effects, source re-reads selected book field108, marks claim, refreshes red dots, refreshes the current list index104, and hides reward button196.",
        "33049 tip handler has null/claimed guards, reason guidetip, no page-level voice; after ToolChange it evaluates four parent accesses even though the result is unused.",
        "Tip effect uses caller position, then marks claim and refreshes red dots. TipBookItem33399 separately plays voice2001, refreshes item and re-applies selection after the page handler returns.",
        "33040 computes book availability once for both guide dots, then tip availability once for both tip dots.",
        "Native button binding uses exact original outlet paths; source RectMask2D padding/softness/enabled values are imported."
    ],
    "remainingUiSourceNotes": [
        "GuideBookUI constructor initializes ListData and tip-item list; Awake binds dynamic list, close/mask/OK/reward callbacks, tab delegates, creates rows in config dictionary order, selects guide tab and refreshes dots.",
        "GuideBookItem has locked/unlocked native buttons, names formatted id.name and claim red dot; locked toast uses recursive GetGuideUnlockLv.",
        "TipBookItem toggles expansion, single selection is tracked by page, closing tip tab clears it; SetSelect forces layout then starts original delayed size coroutine33402.",
        "RefreshPopGuide33041 handles guides1,7/8,9/10/11 specially and uses GuideSprite atlas;33050 supplies guide-specific extra hints and alignment.",
        "DynamicList/TabButton/TabButtonGroup framework behavior and original YD_0 skeleton binding remain to connect. No full UI completion claimed."
    ],
    "boundaries": ["Full popup/list/tab/TipBookItem/Spine ownership, Main/account integration and native-frame/visual acceptance remain required.",
                   "Effects/audio/report validation uses explicit observed endpoints; it does not establish native effect/SDK delivery."]
}
(output / "GUIDE_BOOK_UI_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"classes": len(classes), "methods": len(methods), "genericCalls": len(calls)}))
