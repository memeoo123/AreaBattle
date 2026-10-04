"""Record exact item/tab/click branches used by GuideBookUI, including generic image click."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
context = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], context)
u, metadata, pairs, types, name = [context[k] for k in ("u", "b", "pairs", "ts", "ms")]
output = context["p"] / "generated/outgame"
classes = []
for index in (2907, 2911, 2912, 2918, 3538, 4251, 4252, 4345, 4368, 4369):
    fields = []
    for field in range(types[index][18]):
        field_name, type_index, token = struct.unpack_from("<3i", metadata, pairs[11][0] + 12 * (types[index][8] + field))
        pointer = u(200288 + 4 * type_index)
        fields.append({"name": name(field_name), "offset": u(u(3823136 + 4 * index) + 4 * field),
                       "typeCode": (u(pointer + 4) >> 16) & 255, "typeData": u(pointer)})
    classes.append({"typeIndex": index, "name": name(types[index][0]), "fields": fields})
assert name(types[4368][0]) != "ItemInfoUI"
yield_usage = u(3941916)
assert yield_usage >> 29 == 1
yield_type = u(u(200288 + 4 * ((yield_usage & 0x1ffffffe) >> 1)))
assert name(types[yield_type][0]) == "WaitForEndOfFrame"
methods = []
for row in json.loads((output / "method-map.json").read_text()):
    if row["cls"] in {c["name"] for c in classes} or row["metadata"] in (33031, 33034, 33045, 33047, 33048, 33052, 30048):
        methods.append({"metadata": row["metadata"], "class": row["cls"], "method": row["method"], "path": row["path"],
                        "sha256": hashlib.sha256((output / row["path"]).read_bytes()).hexdigest()})
report = {
    "metadataSha256": hashlib.sha256(metadata).hexdigest(), "memorySha256": hashlib.sha256(context["mem"]).hexdigest(),
    "classes": classes, "methods": methods, "genericImageClick": json.loads((output / "ui-click-generic.json").read_text()),
    "frameYield": {"usageAddress": 3941916, "typeIndex": yield_type, "typeName": name(types[yield_type][0]), "iteratorTypeIndex": 4368, "moveNextMetadata": 33402},
    "findings": [
        "GuideBookItem33213 captures data[index].config and index, then renders the current config; each name separately invokes Lang.Value. Unlock precedes claimed query; locked toast recursively resolves guide level only if the guide config exists.",
        "ButtonExtension30048 removes all runtime listeners before adding GuideBookItem handlers; these callbacks do not send GF_UIButtonClick.",
        "UIExtension22718 appends a Button callback, invokes action, then sends GF_UIButtonClick with that Button. An action exception prevents the message. This also corrects the earlier GuideBookUI reward-button binding.",
        "Image AddClick22717 uses EventListener.Get(Component), replaces the pointer delegate and invokes its action without a global button message. Generic closure22728 shares function6941 with the non-generic gameobject overload.",
        "TipBookItem33389 renders both names and description before claimed/unlocked queries; reward parent depends only on claim, not unlock. SetData then deselects.",
        "SetSelect33391 updates selection and three objects, rebuilds description layout, and starts a coroutine on UpdateManager. Iterator4368 waits WaitForEndOfFrame, rereads current selection/description height, changes root vertical size, and rebuilds parent layout. Pending calls are not cancelled/coalesced.",
        "Tip unlock toggles before looking up GuideBookUI and notifying selection; locked toast formats unchecked(unlockLevel-1). Claim voices2001 before page lookup, captures row and world position, calls page claim, then rereads row and selection for render/resize.",
        "GuideBookUI33047 clears the managed tip list and creates BaseItem instances in config dictionary order; BaseItem27314 clones, SetParent(false), then UIObject normalization/activation. BaseItem27313 destroys owned native object before clearing references; TipBookItem additionally clears its unused public onClick delegate.",
        "GuideBookUI33031 deselects a different previous tip then retains the clicked reference even if the item just collapsed. Leaving tip tab deselects and clears it. Tab true callbacks voice2001 before scroll activation.",
        "TabButton.SetSelect assigns bool, invokes selection delegate, then renders. Silent mode only assigns/renders. Awake adds native click; OnDestroy removes only its own callback.",
        "TabButtonGroup Awake discovers children; Start combines click delegates. Click stores current, notifies group before old=false/new=true only if changed, then rereads current into previous. Programmatic select visits all children and always notifies; silent version omits notifications. Destroy removes its delegates and clears array/selection references."
    ],
    "remaining": [
        "Full DynamicList/ListData recycling and GuideBookUI popup33041/additional text33050/Spine binding remain required; browse binding forwards book-open to its required page dependency.",
        "Language/voice/toast/effects in validation are observed fixtures, not full production service delivery. Main/account and remaining22 controllers are still pending.",
        "EventListener implementation currently covers pointer click only; extracted unrelated event/obfuscated alternate methods are evidence, not claimed implemented features."
    ]
}
(output / "GUIDE_BOOK_BROWSE_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"classes": len(classes), "methods": len(methods), "frameYield": report["frameYield"]}))
