"""Publish only reviewed progress/reward UI methods and original asset provenance."""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import struct

parser = argparse.ArgumentParser()
parser.add_argument("validation_root", type=Path)
validation = parser.parse_args().validation_root.resolve()
helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
c = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], c)
target = c["p"]
output = target / "generated/outgame"
staged = validation / "analysis/targets/wxcf1394487200e48f/43/generated/outgame"
index_path = output / "method-map.json"
index = json.loads(index_path.read_text())
by_id = {r["metadata"]: r for r in index}
selected = set(range(33173, 33189))
incoming = [r for r in json.loads((staged / "method-map.json").read_text()) if r["metadata"] in selected]
assert {r["metadata"] for r in incoming} == selected
for row in incoming:
    shutil.copy2(staged / row["path"], output / row["path"])
    if row["metadata"] in by_id:
        assert row == by_id[row["metadata"]]
    else:
        index.append(row)
        by_id[row["metadata"]] = row
text = json.dumps(index, ensure_ascii=False, indent=2) + "\n"
crlf = b"\r\n" in index_path.read_bytes()
index_path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())
b, mem, ts, md, ms, u, pairs = [c[k] for k in ("b", "mem", "ts", "md", "ms", "u", "pairs")]
selected.update((22716, 22726, 27313))
methods = [dict(by_id[j], sha256=hashlib.sha256((output / by_id[j]["path"]).read_bytes()).hexdigest()) for j in sorted(selected)]
fields = []
for owner in (3942, 4339, 4340):
    for k in range(ts[owner][18]):
        name, typ, token = struct.unpack_from("<3i", b, pairs[11][0] + 12 * (ts[owner][8] + k))
        ptr = u(200288 + 4 * typ)
        fields.append(dict(owner=owner, name=ms(name), offset=u(u(3823136 + 4 * owner) + 4 * k), typeCode=(u(ptr + 4) >> 16) & 255, typeData=u(ptr)))
usages = []
for address in (4101468, 4095008, 4095004, 4091092, 4091100, 3992892, 3992896, 4031220):
    encoded = u(address)
    kind, idx = encoded >> 29, (encoded & 0x1ffffffe) >> 1
    row = dict(address=address, kind=kind, index=idx)
    if kind == 5:
        length, offset = struct.unpack_from("<II", b, pairs[0][0] + idx * 8)
        row["literal"] = b[pairs[1][0] + offset:pairs[1][0] + offset + length].decode()
    elif kind == 3:
        row.update(sourceClass=ms(ts[md[idx][1]][0]), method=ms(md[idx][0]))
    usages.append(row)
snapshot = target / "generated/resource-snapshots/limit-task-ui-20261003"
acquisition = json.loads((snapshot / "acquisition-manifest.json").read_text())
assert len(acquisition["items"]) == 7
for row in acquisition["items"]:
    data = (target / row["path"]).read_bytes()
    assert len(data) == row["catalog"]["size"]
    assert hashlib.md5(data).hexdigest() == row["catalog"]["md5"]
    assert hashlib.sha256(data).hexdigest() == row["sha256"]
assets = json.loads((output / "limit-task-ui-import.json").read_text())
report = dict(
    metadataSha256=hashlib.sha256(b).hexdigest(), memorySha256=hashlib.sha256(mem).hexdigest(),
    methods=methods, fields=fields, usages=usages,
    acquisition="generated/resource-snapshots/limit-task-ui-20261003/acquisition-manifest.json",
    assetManifest="limit-task-ui-import.json",
    assetCounts=dict(verifiedBundles=7, prefabs=len(assets["prefabs"]), sprites=len(assets["sprites"]), fonts=len(assets["fonts"]), nodes={p["name"]: len(p["nodes"]) for p in assets["prefabs"]}),
    findings=[
        "Four original prefabs and SevendayActivity atlas acquired with original seven-bundle dependency closure. All catalog sizes/MD5 and recorded SHA256 checked. Static import restores source UGUI, geometry, fonts, images, scroll references and outlet evidence; unknown custom DynamicList/UIOutlet/SpriteAtlasList scripts remain explicitly skipped until runtime binding.",
        "Progress33179 reads condition.value as signed Int64, caps only above target, converts both operands to float for Image.fillAmount, then formats source literal '{0} / {1}'. Negative values remain in text while UGUI clamps the image. A zero target is not guarded and can deliver NaN to native Image.",
        "Reward33182 publishes Data first, captures current ConfigMgr.dicGameItem, looks up input itemId, and only on success calls SetSprite(icon, GameItemConfig.icon, GameItemConfig.atlasName, false). Fields are offsets20/24, not the separate ItemIcon offset28. It then reads itemCount from the original input after the sprite callback. Missing configuration leaves the old displayed icon/count with new Data.",
        "Reward33183 captures optional onClick, invokes it with self, then requests current UIControl.PopItemInfo using retained rect and live Data.itemId. Reentry can change selection; callback failure prevents detail request. Sprite failure preserves published Data and old count; null input is assigned before dereference.",
        "Awake33178/33186 use GameObject AddClick22716, replacing EventListener pointer delegate through closure22726. There is no GF_UIButtonClick or button-sound message. Progress click only invokes its optional callback.",
        "Both Refresh and InitializeSkin are source no-ops. Dispose33175/33185 calls BaseItem27313, which destroys the owned object before clearing object-list/GameObject/Transform and marking disposed, then clears onClick. Rect reference and reward Data remain. Native Destroy is deferred; an exception from destruction prevents the later cleanup."
    ],
    remaining=[
        "Actual task/day selection providers, accumulator item, reward preview and CommonLimitTimeTaskUI lifecycle/refresh/countdown/claim binding remain pending, as do production seven-day menu and red-dot entry.",
        "SetSprite and PopItemInfo are required host endpoints observed by tests; their complete production delivery is not claimed here. Native tests validate original items and deferred destruction, not full-page visual/audio equivalence.",
        "Controller lifecycle count remains19/38. Complete Main/account/platform, other activities/business, Player and original audiovisual acceptance remain required."
    ])
(output / "LIMIT_TASK_UI_ITEMS_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(methods=len(methods), fields=len(fields), usages=len(usages), indexedMethods=len(index), assets=report["assetCounts"])))
