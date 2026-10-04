"""Record UserInfo4228/4229/4230 fields and source method fingerprints."""
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
for index in (4228, 4229, 4230):
    fields = []
    for field in range(types[index][18]):
        field_name, type_index, token = struct.unpack_from(
            "<3i", metadata, pairs[11][0] + 12 * (types[index][8] + field))
        pointer = u(200288 + 4 * type_index)
        fields.append({"name": name(field_name), "offset": u(u(3823136 + 4 * index) + 4 * field),
                       "typeCode": (u(pointer + 4) >> 16) & 255, "typeData": u(pointer)})
    classes.append({"typeIndex": index, "name": name(types[index][0]), "fields": fields})

method_map = json.loads((output / "method-map.json").read_text(encoding="utf8"))
methods = [{"metadata": row["metadata"], "class": row["cls"], "method": row["method"],
            "path": row["path"], "sha256": hashlib.sha256((output / row["path"]).read_bytes()).hexdigest()}
           for row in method_map if 32198 <= row["metadata"] <= 32231]
assert len(methods) == 34
calls = []
for address in (4004864, 3995148, 3977288, 3957416, 3957396, 3957424):
    encoded = u(address)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", context["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    method = context["md"][spec[0]]
    calls.append({"usageAddress": address, "methodSpec": list(spec),
                  "class": name(types[method[1]][0]), "method": name(method[0])})
report = {
    "classes": classes, "methods": methods, "genericCalls": calls,
    "metadataSha256": hashlib.sha256(metadata).hexdigest(),
    "findings": [
        "Constructor32231 sets uname=String.Empty, headportId=1, hdboxId=1, hasHeadport=3, hasHdBox=1. Lists are not created there.",
        "CreateNewData32226 builds timestamp, portraits1/2/3/4 and frames1/2 before publishing the record.",
        "UpdateDataCallBack32222 repairs both lists if either decoded list is empty, fills only null/empty names via RandAIInfo(1,false)[0], then checks install version.",
        "Icon/IconBox setters notify before assignment and reread the current data after notification; failures prevent the assignment.",
        "ApplyHeadport/ApplyHeadbox require config membership, not unlocked membership; successful selection invokes manager OnSave.",
        "ApplyName stores and saves before ADHelper.Report_ams_createRole; unlock itself does not save.",
        "UserInfoManager registration is source type4229, Proj_hdzd, autoSyn=true, compressData=false.",
        "UserInfoControl lifecycle is real pool lookup followed by CheckInstallVersion, empty Update and singleton clear on dispose."
    ],
    "boundaries": ["ConfigHelper.RandAIInfo32592 remains an explicit provider; its RNG algorithm is not reconstructed by this batch.",
                   "Server time, AppInfo installation state, login/storage host and external role report still require Main/account composition."]
}
(output / "USER_INFO_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf8")
print(json.dumps({"classes": len(classes), "methods": len(methods), "genericCalls": len(calls)}))
