"""Decode source fields and generic call sites used by original AI name generation."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
context = {"__file__": str(helper)}
exec(helper.read_text(encoding="utf8").split("rows=[]")[0], context)
u, metadata, pairs, types, name = [context[k] for k in ("u", "b", "pairs", "ts", "ms")]
output = context["p"] / "generated/outgame"
notes = json.loads((output / "AI_NAME_RESTORE_NOTES.json").read_text())
for source in notes["source"]:
    assert hashlib.sha256((output / source["path"]).read_bytes()).hexdigest() == source["sha256"]
calls = []
for address in (3956984, 3957152, 3957156, 3998496, 3955792, 3955812, 3970292, 3970296, 3977256, 3977228):
    encoded = u(address)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", context["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    method = context["md"][spec[0]]
    calls.append({"usageAddress": address, "methodSpec": list(spec),
                  "class": name(types[method[1]][0]), "method": name(method[0])})
fields = []
for field, expected_name, expected_offset in ((3, "dicAIName", 20), (35, "dicCountryConfig", 148)):
    field_name, type_index, token = struct.unpack_from("<3i", metadata, pairs[11][0] + 12 * (types[3907][8] + field))
    fields.append({"typeIndex": 3907, "fieldIndex": field, "name": name(field_name),
                   "offset": u(u(3823136 + 4 * 3907) + 4 * field)})
    assert fields[-1]["name"] == expected_name and fields[-1]["offset"] == expected_offset
report = {
    "metadataSha256": hashlib.sha256(metadata).hexdigest(),
    "memorySha256": hashlib.sha256(context["mem"]).hexdigest(),
    "source": notes["source"], "fields": fields, "genericCalls": calls,
    "findings": [row.replace("fields148/20", "field offsets148/20") for row in notes["observations"][:-1]],
    "boundaries": ["Source algorithms and call order reconstructed; original seeded RNG sequence and cross-runtime Dictionary iteration parity not claimed.",
                   "Platform timestamp/install state/account/report endpoints require full Main composition."]
}
(output / "AI_NAME_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"sources": len(report["source"]), "fields": fields, "genericCalls": calls}, ensure_ascii=False))
