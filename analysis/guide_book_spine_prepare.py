"""Export GuideBookUI's own YD_0 subtree, including its distinct baked mesh."""
from pathlib import Path
import hashlib
import json
import UnityPy

root = Path(__file__).resolve().parent / "targets/wxcf1394487200e48f/43"
snapshot = root / "generated/resource-snapshots/guide-book-ui-20261003"
output = snapshot / "guide-spine"
output.mkdir(exist_ok=True)
objects = {r["id"]: r for r in json.loads((snapshot / "asset-evidence-incremental.json").read_text())["objects"]}
cab = "CAB-6bfa8786befb1cf66d6a7f790d2e5d84"
rows = []
for path_id in (435856469966727484, 5200670246001647623):
    obj = objects[f"{cab}:{path_id}"]
    env = UnityPy.load(str(root / obj["source"]))
    reader = next(o for o in env.objects if o.path_id == path_id and o.assets_file.name == cab)
    text = reader.read().m_Script
    data = text.encode("utf8", errors="surrogateescape") if isinstance(text, str) else bytes(text)
    path = output / (obj["name"] + ".txt")
    path.write_bytes(data)
    rows.append({"id": obj["id"], "name": obj["name"], "path": path.relative_to(root).as_posix(), "sha256": hashlib.sha256(data).hexdigest()})
(output / "spine-sources.json").write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n")
script = (Path(__file__).parent / "asset_guide_spine_prepare.py").read_text()
script = script.replace("hud-soldier300-20260928", "guide-book-ui-20261003")
script = script.replace("CAB-71a96badbcffab7b7451a783f271bc4b:-5489945122626182919", cab + ":-4364533787690783647")
script = script.replace("GuideUI.Awake f19289 first sets YD_0 inactive; ShowUI stage1 activates it and disables guideIcon Image. Later same-instance ShowUI does not explicitly deactivate it. mainbg visibility controls popup visibility.",
                        "GuideBookUI.RefreshPopGuide33041 activates YD_0 only for guide1 and sets guideIcon.sprite=null after requesting the source sprite; all other guide ids deactivate YD_0.")
exec(compile(script, str(Path(__file__).parent / "asset_guide_spine_prepare.py"), "exec"))
