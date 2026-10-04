"""Record the guide-book service group after matching isolated integration validation."""
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
outgame = target / "generated/outgame"
def read(path):
    return json.loads(path.read_text(encoding="utf8"))
def write(path, value):
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
focused = read(validation / "analysis/guide-book-validation.json")
assert full["passed"] and len(full["checks"]) == 1113
assert focused["passed"] and len(focused["checks"]) == 8
assert "AREABATTLE_INTEGRATED_PASS cases=1113" in (validation / "guide-book-integrated.log").read_text()
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or "/FirstPack/" in p.as_posix() or "/Recovered/Loading/" in p.as_posix() or "/Recovered/Audio/" in p.as_posix())]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
record = {"atUtc": now, "scope": "GuideBookDataManager/control/runtime, original config unlock rules and real persistence",
          "checksPassed": 1113, "newIntegratedChecks": 8, "targetedChecks": 8,
          "freshPlayModeRun": False, "freshPlayerBuild": False, "freshPlayerSmoke": False,
          "previousNativeEvidence": "item-runtime-native-validation.json and item-profile-native.log:23 checks, previous service-group batch",
          "editorVersion": full["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
          "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints),
          "commands": ["OutgameGuideBookValidation.Validate", "BattleBuild.ValidateMechanicsOnly"],
          "notClaimed": "Guide-book UI/item-grant operations, full Main/account/22 remaining controllers, SDK delivery or full Player"}
state = read(target / "OUTGAME_RESTORE_STATE.json")
assert not any(m.get("id") == "guide-book-manager-control-runtime" for m in state["milestones"])
for filename in ("unity-integrated-validation.json", "guide-book-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-book.log", "guide-book-integrated.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(outgame / "GUIDE_BOOK_AUDIT.json", {"status": "source-manager-controller-and-persistence-verified-ui-main-pending", "atUtc": now,
    "sourceEvidence": "GUIDE_BOOK_SOURCE_EVIDENCE.json", "verification": record,
    "implementation": ["OutgameGuideBookManager.cs", "OutgameGuideBookControl.cs", "OutgameGuideBookRuntime.cs", "OutgameCoreControllerBindings.cs"],
    "checks": focused["checks"], "remaining": ["Actual guide-book UI and reward item-grant call sites.", "Main/account/level progression production composition and full Player acceptance."]})
matrix = read(outgame / "CONTROLLER_LIFECYCLE_MATRIX.json")
matrix["implementedLifecycleControllers"] = 16
for controller in matrix["controllers"]:
    if controller["typeIndex"] == 4076:
        for method in controller["lifecycle"]:
            method.update(implementationStatus="implemented-in-core-controller-bindings", implementation="OutgameGuideBookControl.cs")
write(outgame / "CONTROLLER_LIFECYCLE_MATRIX.json", matrix)
next_priority = "Complete Main/account/data-pool composition using recovered ItemRuntime, UserInfoRuntime and GuideBookRuntime; recover remaining22 controllers/CommanderUI/Dice/report owners and all business UI/reward flows. Full Player and original visual/audio/timing acceptance remain required."
state.update(lastUpdatedAtUtc=now, currentStage="guide-book-manager-control-runtime", nextPriority=next_priority, guideBookAudit="generated/outgame/GUIDE_BOOK_AUDIT.json")
state["validation"].update(integratedChecksPassed=1113, guideBook=record)
state["milestones"].append({"id": "guide-book-manager-control-runtime", "atUtc": now, "status": "verified-ui-full-main-composition-pending", "integratedChecks": 1113, "newChecks": 8, "remainingLifecycleControllers": 22})
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {f["path"]: f for f in fingerprints}
ordered = [by_path.pop(f["path"]) for f in manifest["sourceFingerprints"] if f["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1113, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("GuideBook manager/control/runtime and actual claim-record save/restart")
execution["remainingWorkPackages"] = [p.replace("23 controllers", "22 controllers") for p in execution["remainingWorkPackages"]]
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps({"integrated": 1113, "fingerprints": len(fingerprints), "controllers": 16, "remaining": 22}))
