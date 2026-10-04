"""Resolve popup commander field, additional-tip switch and GuideSprite identity."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
root, u = c["p"], c["u"]
out = root / "generated/outgame"
snapshot = root / "generated/resource-snapshots/guide-book-ui-20261003"
objects = {o["id"]: o for o in json.loads((snapshot / "asset-evidence-incremental.json").read_text())["objects"]}
atlas = next(o for o in objects.values() if o["type"] == "SpriteAtlas" and o["name"] == "GuideSprite")
sprite_ids = {r["target"] for r in atlas["references"] if r["property"].startswith(".m_PackedSprites[")}
manifest = json.loads((out / "guide-book-ui-import.json").read_text())
sprites = [{"name": row["name"], "id": row["id"]} for row in manifest["sprites"] if row["id"] in sprite_ids]
assert len(sprites) == len(sprite_ids) == 25
destination = Path(__file__).resolve().parent.parent / "UnityProject/Assets/AreaBattle/Resources/Recovered/GuideBook/guide-sprites.json"
destination.write_text(json.dumps({"atlas": atlas["id"], "sprites": sprites}, ensure_ascii=False, indent=2) + "\n")
encoded = u(3953560)
spec = struct.unpack_from("<3i", c["mem"], 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
assert spec == (26934, 572, -1)
inst = u(72592 + 4 * spec[1])
owner = u(u(u(inst + 4)))
assert owner == 4165
fields = []
for f in range(c["ts"][owner][18]):
    offset = u(u(3823136 + 4 * owner) + 4 * f)
    field_name, field_type, token = struct.unpack_from("<3i", c["b"], c["pairs"][11][0] + 12 * (c["ts"][owner][8] + f))
    if offset == 108: fields.append({"offset": offset, "name": c["ms"](field_name)})
assert fields == [{"offset": 108, "name": "CurCommanderId"}]
methods = []
for row in json.loads((out / "method-map.json").read_text()):
    if row["metadata"] in (33035, 33036, 33039, 33041, 33043, 33044, 33045, 33050):
        methods.append(dict(metadata=row["metadata"], path=row["path"], sha256=hashlib.sha256((out / row["path"]).read_bytes()).hexdigest()))
report = {"methods": methods, "commander": {"usage": 3953560, "methodSpec": spec, "genericInst": 572, "owner": owner, "class": "SkillControl", "field": fields[0]},
          "hintSwitch": {"6": ["max", "MiddleCenter"], "7": ["defanse", "retain"], "8": ["attack", "retain"], "9": ["ice", "MiddleCenter"], "10": ["fire", "MiddleCenter"], "11": ["lighting", "MiddleCenter"], "12": ["arrow", "retain"], "default": ["empty", "retain"]},
          "atlas": {"id": atlas["id"], "sprites": sprites},
          "findings": ["OnItemClick voices2001 before selected book/index, shows popup before refreshing; missing guide leaves prior popup fields unchanged.",
                       "Refresh captures picture/explain/title, then guides9..11 compute unchecked(id-8+(CurCommanderId-1)*3). If result==-1 it skips all formatting; picture formatting rereads CurCommanderId.",
                       "Title and description localization precede sprite request; guide1 then activates YD_0 and clears Image.sprite, without disabling the Image component. Other guides deactivate YD_0.",
                       "Guides7/8 split param on ';' including empty segments, write child(i)/child(2) text without clearing unused children, then clear description. No bounds clamp or null repair.",
                       "Additional hints preserve previous alignment for7/8/12/default;6/9/10/11 set MiddleCenter. Reward visibility is set last from the passed book reference's id.",
                       "Popup mask and OK play voice before hiding then clearing selected book, retaining selected index. Only Button overload sends global click after callback returns. Page close voices then invokes source virtual Close through its page dependency."],
          "spine": "../resource-snapshots/guide-book-ui-20261003/guide-spine/native/native-import.json",
          "remaining": ["Complete DynamicList/ListData and BaseUI/Main owner; actual SkillControl/localization/async atlas/audio/effects delivery and full visual/audio Player acceptance."]}
(out / "GUIDE_BOOK_POPUP_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"methods": len(methods), "atlasSprites": len(sprites), "commanderOwner": owner}))
