"""Record original guide-book fields, methods, registration and generic call sites."""
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
for index in (4076, 4077, 4078, 3873):
    fields = []
    for field in range(types[index][18]):
        field_name, type_index, token = struct.unpack_from("<3i", metadata, pairs[11][0] + 12 * (types[index][8] + field))
        pointer = u(200288 + 4 * type_index)
        fields.append({"name": name(field_name), "offset": u(u(3823136 + 4 * index) + 4 * field),
                       "typeCode": (u(pointer + 4) >> 16) & 255, "typeData": u(pointer)})
    classes.append({"typeIndex": index, "name": name(types[index][0]), "fields": fields})
methods = [{"metadata": row["metadata"], "class": row["cls"], "method": row["method"],
            "path": row["path"], "sha256": hashlib.sha256((output / row["path"]).read_bytes()).hexdigest()}
           for row in json.loads((output / "method-map.json").read_text()) if 31327 <= row["metadata"] <= 31352 or row["metadata"] == 30167]
assert len(methods) == 27
calls = []
for address in (4004812, 3995100, 3974432, 3974444, 3974460, 3957384, 3957372, 3957352, 3957376, 3957388):
    encoded = u(address)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", context["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    method = context["md"][spec[0]]
    calls.append({"usageAddress": address, "methodSpec": list(spec), "class": name(types[method[1]][0]), "method": name(method[0])})
registration = next(row for row in json.loads((output / "data-manager-registration-roster.json").read_text())["managers"] if row["typeIndex"] == 4078)
report = {
    "metadataSha256": hashlib.sha256(metadata).hexdigest(), "memorySha256": hashlib.sha256(context["mem"]).hexdigest(),
    "classes": classes, "methods": methods, "genericCalls": calls, "registration": registration,
    "findings": [
        "4077 ctor is empty. CreateNewData31347 returns new GuideRewards/TipRewards lists; caller publishes BookData.",
        "UpdateDataCallBack31344 parses nonempty JSON, creates only if the record itself is null; there is no list repair.",
        "Claims31349/31351 append only if absent, then invoke virtual slot6 OnSave. Failed saves retain appended IDs; duplicates skip save.",
        "Control claims31327/31340 always send GetGuidBookReward after manager return, including duplicate claims. Source message is GuideMessage3873 static offset12 initialized by30167.",
        "IsTipUnlock31336 tests currentLevel >= unlockLevel; IsBookUnlock31338 tests currentLevel > GetGuideUnlockLv(guideId).",
        "GetGuideUnlockLv31337 returns0 on missing config; negative guildLv recurses at max(unchecked(id-1),0), preserving source recursion.",
        "Availability scans config values in original dictionary order, short-circuiting claimed entries before unlock queries.",
        "Controller OnInit fetches registered4078. OnDispose31332 sets only ActiveUpdate false, retaining registry and manager; manager OnRelease31345 is empty.",
        "Claims do not validate config/unlock state or deliver inventory items in this source controller/manager. UI reward operations remain separate work."
    ],
    "boundaries": ["Actual guide-book UI and item-grant call sites are not restored by this service group.",
                   "Full Main/account, level progression and remote SDK composition remain required."]
}
(output / "GUIDE_BOOK_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"classes": len(classes), "methods": len(methods), "genericCalls": len(calls)}))
