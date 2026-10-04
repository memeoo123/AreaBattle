"""Record verified original limited-task assets and source progress/reward item behavior."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import shutil

parser = argparse.ArgumentParser()
parser.add_argument("validation_root", type=Path)
validation = parser.parse_args().validation_root.resolve()
workspace = Path(__file__).resolve().parent.parent
analysis = workspace / "analysis"
target = analysis / "targets/wxcf1394487200e48f/43"
output = target / "generated/outgame"
def read(path): return json.loads(path.read_text())
def write(path, value):
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
assert full["passed"] and len(full["checks"]) == 1363 and all(r["result"] == "pass" for r in full["checks"])
native = read(validation / "analysis/limit-task-ui-items-native-validation.json")
assert native["passed"] and len(native["checks"]) == 6
integrated_log = (validation / "analysis/limit-task-ui-items-integrated.log").read_text()
native_log = (validation / "analysis/limit-task-ui-items-native.log").read_text()
assert "AREABATTLE_INTEGRATED_PASS cases=1363" in integrated_log
assert "error CS" not in integrated_log and "error CS" not in native_log
focused = dict(full, checks=[r for r in full["checks"] if r["id"].startswith("limit-task-ui-")])
assert len(focused["checks"]) == 10
focused["limitations"] = native["scope"]
evidence = read(output / "LIMIT_TASK_UI_ITEMS_SOURCE_EVIDENCE.json")
assert (len(evidence["methods"]), len(evidence["fields"]), len(evidence["usages"])) == (19, 16, 8)
for row in evidence["methods"]:
    assert hashlib.sha256((output / row["path"]).read_bytes()).hexdigest() == row["sha256"]
assert len(read(output / "method-map.json")) == 5978
assert read(output / "CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"] == 19
imported = read(validation / "analysis/unity-limit-task-import-report.json")
assert (imported["prefabs"], imported["sprites"], imported["fonts"], len(imported["skippedComponents"])) == (4, 49, 1, 14)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
paths = {r["path"] for r in manifest["sourceFingerprints"]}
files = (
    "Scripts/OutgameLimitTaskUiItems.cs", "Scripts/OutgameUiPointerClick.cs",
    "Editor/OutgameLimitTaskUiItemsValidation.cs", "Editor/OutgameLimitTaskUiItemsPlayModeValidation.cs",
    "Editor/RecoveredHudImporter.cs", "Editor/BattleBuild.cs"
)
paths.update("UnityProject/Assets/AreaBattle/" + name for name in files)
resources = workspace / "UnityProject/Assets/AreaBattle/Resources/Recovered/LimitTask"
paths.update(str(p.relative_to(workspace)) for p in resources.rglob("*") if p.is_file())
paths.add(str(resources.with_suffix(".meta").relative_to(workspace)))
for name in files:
    paths.add("UnityProject/Assets/AreaBattle/" + name + ".meta")
fingerprints = []
for name in sorted(paths):
    data = (workspace / name).read_bytes()
    assert data == (validation / name).read_bytes(), name
    fingerprints.append(dict(path=name, sha256=hashlib.sha256(data).hexdigest()))
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
milestone = "limit-task-original-assets-and-ui-items"
state = read(target / "OUTGAME_RESTORE_STATE.json")
assert not any(r.get("id") == milestone for r in state["milestones"])
next_priority = "Restore source task/day selection provider, CommonLimitTimeTaskItem/PageItem, accumulator/preview and CommonLimitTimeTaskUI page lifecycle/refresh/claims, then production seven-day menu/red entry. Continue Task/Achievement, production Main/account/SDK/HTTP/scene hosts, remaining19 controllers and all business/rewards/return, final Player and original audiovisual acceptance."
record = dict(
    atUtc=now, scope="Original four limited-task prefabs and source progress/reward UI items",
    checksPassed=1363, newIntegratedChecks=10, targetedChecks=10, nativeChecks=6,
    freshPlayModeRun=True, freshPlayerBuild=False, freshPlayerSmoke=False,
    editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints),
    implementedLifecycleControllers=19, remainingLifecycleControllers=19,
    commands=["RecoveredHudImporter.ImportLimitTaskBatch", "BattleBuild.ValidateMechanicsOnly", "OutgameLimitTaskUiItemsPlayModeValidation.Run"],
    observedBoundaries=[
        "Original seven-bundle closure verified by catalog size/MD5 and acquired SHA256. Four native prefabs retain24/9/4/46 source nodes,49 sprites and one font. Four source hierarchy/UGUI/geometry/font/scroll-reference checks pass. Import explicitly skips14 custom components; runtime page binding remains pending.",
        "Progress4339 and Reward4340 recovered from16 newly indexed normal methods. Evidence includes19 methods,16 fields,8 decoded usages. Normal index5978. Reward uses original GameItemConfig.icon/atlasName, signed Int64 counts, source missing-row retained UI, callback-before-live-item popup and reentrant original-argument count. Progress preserves upper-only cap, negative text and zero denominator reaching native Image as NaN.",
        "GameObject AddClick replaces native pointer delegate with no button message. BaseItem disposal destroys before clearing fields/delegates; rect and reward data stay held. Native6 verifies real frames, imported text/fill, native pointer callbacks/live reread and deferred Object.Destroy.",
        "Integrated1363 including10 new all pass. Integrated log contains32 existing ShouldRunBehaviour assertions and one Curl42; native log one UnityEditor.Search startup indexing ArgumentOutOfRangeException and one Curl42, no compiler errors. Native-only runner added after integrated run, then compiled and passed native checks; integrated gameplay/test sources unchanged.",
        "All recorded inputs byte-match isolated validation project. Sprite delivery and PopItemInfo are required observed fixture endpoints, not asserted complete production services. No new Player, complete task page/menu, platform success or original audiovisual equivalence claimed."
    ],
    notClaimed="Task/day/accumulator/preview/page binding, production seven-day menu, full Main/account/SDK/network/scene, remaining19 controllers/all business, Player and original audiovisual acceptance."
)
write(analysis / "limit-task-ui-items-validation.json", focused)
for name in ("unity-integrated-validation.json", "unity-limit-task-import-report.json", "limit-task-ui-items-integrated.log", "limit-task-ui-items-native-validation.json", "limit-task-ui-items-native.log"):
    shutil.copy2(validation / "analysis" / name, analysis / name)
write(output / "LIMIT_TASK_UI_ITEMS_AUDIT.json", dict(
    status="original-assets-and-two-item-types-verified-full-page-pending", atUtc=now,
    sourceEvidence="LIMIT_TASK_UI_ITEMS_SOURCE_EVIDENCE.json", verification=record,
    implementation=list(files), importReport=imported, checks=focused["checks"], native=native, remaining=next_priority))
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, limitTaskUiItemsAudit="generated/outgame/LIMIT_TASK_UI_ITEMS_AUDIT.json")
state["validation"].update(integratedChecksPassed=1363, limitTaskUiItems=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="original-assets-and-two-item-types-verified-full-page-pending", integratedChecks=1363, newChecks=10, nativeChecks=6, freshPlayModeRun=True, remainingLifecycleControllers=19))
write(target / "OUTGAME_RESTORE_STATE.json", state)
by_path = {r["path"]: r for r in fingerprints}
ordered = [by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1363, latestValidation=record, sourceFingerprints=ordered + [by_path[n] for n in sorted(by_path)])
manifest["validationHistory"].append(record)
for name in ("analysis/limit-task-ui-items-validation.json", "analysis/limit-task-ui-items-native-validation.json", "analysis/unity-limit-task-import-report.json"):
    if name not in manifest["reports"]: manifest["reports"].append(name)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["remainingWorkPackages"] = [s.replace("Main/account/20 controllers", "Main/account/19 controllers") for s in execution["remainingWorkPackages"]]
execution["completedThisRun"].append(f"Original limited-task four prefabs/49 sprites/one font verified; source progress/reward items and GameObject pointer overload restored.1363 integrated including10 new,6 native checks,{len(fingerprints)} matching inputs,5978 method index. Controller lifecycle19/38; complete page/menu/Main/platform/business/Player acceptance pending.")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1363, newChecks=10, native=6, fingerprints=len(fingerprints), indexedMethods=5978, controllers=19, remaining=19)))
