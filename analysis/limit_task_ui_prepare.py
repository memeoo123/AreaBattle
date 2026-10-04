"""Prepare original limited-task page and item templates through the established UI import contract."""
from pathlib import Path
import collections
import hashlib
import json

helper = Path(__file__).with_name("outgame_ui_prepare.py")
context = {"__file__": str(helper.resolve())}
exec(helper.read_text().split("for p in read(G/'outgame/ui-evidence.json')")[0], context)
root, generated, read, objects = [context[k] for k in ("R", "G", "read", "objects")]
snapshot = read(generated / "resource-snapshots/limit-task-ui-20261003/asset-evidence-incremental.json")
objects.update({o["id"]: o for o in snapshot["objects"]})
focus = Path(__file__).with_name("asset_focus.py")
tree_context = {"__file__": str(focus.resolve())}
exec(focus.read_text().split("roots=[]")[0], tree_context)
tree_context["objs"] = objects
prefabs = []
for entry in snapshot["containers"]:
    if not entry["assetPath"].endswith(("/commonlimittimetaskui.prefab", "/commonlimittimetaskitem.prefab", "/commonlimittimetaskpageitem.prefab", "/commonlimittimetaskrewarditem.prefab")):
        continue
    if objects[entry["object"]]["type"] != "GameObject":
        continue
    source = tree_context["hierarchy"](objects[entry["object"]])
    nodes, by_id = [], {}
    def walk(node, parent=""):
        path = parent + "/" + node["name"]
        by_id[node["id"]] = path
        components = []
        for component in node["components"]:
            by_id[component["id"]] = path
            kind = (component.get("script") or {}).get("m_ClassName", component["type"])
            if kind in ("Transform", "RectTransform"):
                continue
            assert not component.get("schemaPartial", False), component["id"]
            components.append({"id": component["id"], "class": kind, "data": component["data"], "references": component["references"]})
        nodes.append({"path": path, "object": node["id"], "active": node["active"], "layer": node["layer"],
                      "transform": node["transform"], "components": components})
        for child in node["children"]:
            walk(child, path)
    walk(source)
    bindings = []
    for node in nodes:
        for component in node["components"]:
            if component["class"] != "UIOutlet":
                continue
            for index, outlet in enumerate(component["data"].get("OutletInfos", [])):
                ref = next(r for r in component["references"] if r["property"] == f".OutletInfos[{index}].Object")
                bindings.append({"name": outlet["Name"], "componentType": outlet["ComponentType"], "object": ref["target"],
                                 "path": by_id[ref["target"]], "outletOwner": node["path"]})
    prefabs.append({"assetPath": entry["assetPath"], "root": entry["object"], "nodes": nodes, "outletBindings": bindings})
    context["add"](source["name"], nodes, bindings, nodes[0]["path"])
    # Two sibling Scroll Views have the same source name. The import contract
    # disambiguates node paths; outlet paths must follow object identity too.
    imported = context["prefabs"][-1]
    imported_paths = {node["sourceId"]: node["path"] for node in imported["nodes"]}
    owner_by_object = {}
    owners_by_path = collections.defaultdict(list)
    for node in nodes:
        owner_by_object[node["object"]] = node["object"]
        owners_by_path[node["path"]].append(node["object"])
        for component in node["components"]:
            owner_by_object[component["id"]] = node["object"]
    for original, binding in zip(bindings, imported["bindings"]):
        binding["path"] = imported_paths[owner_by_object[original["object"]]]
        owners = owners_by_path[original["outletOwner"]]
        assert len(owners) == 1, original
        binding["owner"] = imported_paths[owners[0]]
assert len(prefabs) == 4
# Include every original SevendayActivity atlas sprite for dynamic item states.
for obj in objects.values():
    if obj["type"] != "Sprite" or "/uiatlas/sevendayactivity.spriteatlas_" not in obj.get("source", "").lower():
        continue
    data = read(root / obj["outputs"]["typetree"])
    png = obj["outputs"].get("pngCanvas", obj["outputs"].get("png"))
    assert png, obj["id"]
    context["sprites"][obj["id"]] = {"id": obj["id"], "name": obj["name"], "path": png,
        "sha256": hashlib.sha256((root / png).read_bytes()).hexdigest(), "pivot": data["m_Pivot"], "ppu": data["m_PixelsToUnits"], "border": data["m_Border"]}
out = {"target": {"appId": "wxcf1394487200e48f", "version": "43"}, "prefabs": context["prefabs"],
       "sprites": list(context["sprites"].values()), "fonts": list(context["fonts"].values()), "unknowns": context["unknown"]}
(generated / "outgame/limit-task-ui-evidence.json").write_text(json.dumps({"prefabs": prefabs}, ensure_ascii=False, indent=2))
(generated / "outgame/limit-task-ui-import.json").write_text(json.dumps(out, ensure_ascii=False, indent=2))
print(json.dumps({"prefabs": [(p["name"], len(p["nodes"])) for p in out["prefabs"]], "sprites": len(out["sprites"]),
                  "components": dict(collections.Counter(c["className"] for p in out["prefabs"] for n in p["nodes"] for c in n["components"]))}))
