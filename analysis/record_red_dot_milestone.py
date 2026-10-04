"""Record actual RedDot native/full verification with matching source inputs."""
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
out = target / "generated/outgame"
def read(p):
    return json.loads(p.read_text())
def write(p, value):
    crlf = p.exists() and b"\r\n" in p.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    p.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
native = read(validation / "analysis/red-dot-native-validation.json")
assert full["passed"] and len(full["checks"]) == 1161
assert native["passed"] and len(native["checks"]) == 17 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1161" in (validation / "red-dot-integrated-final.log").read_text()
assert "OutgameRedDotPlayModeValidation.Run" in (validation / "red-dot-native-final.log").read_text()
focused = dict(full, checks=[r for r in full["checks"] if r["id"].startswith("red-dot-")], limitations="Eight source red-dot/lifecycle/menu cases from final1161 run. Native17 tests actual callbacks with explicit test listener and activation of the normally hidden item tab; no invented original event target or full Main/Player claim.")
assert len(focused["checks"]) == 8
matrix = read(out / "CONTROLLER_LIFECYCLE_MATRIX.json")
assert matrix["implementedLifecycleControllers"] == 18
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "red-dot-controller-menu-lifecycle"
assert not any(r.get("id") == milestone for r in state["milestones"])
manifest = read(analysis / "VALIDATION_MANIFEST.json")
paths = {r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/" + name for name in (
    "Scripts/OutgameRedDotControl.cs", "Scripts/OutgameRedDotItem.cs", "Scripts/OutgameMenuRedDotBinding.cs",
    "Editor/OutgameRedDotValidation.cs", "Editor/OutgameRedDotPlayModeValidation.cs"))
fingerprints = []
for name in sorted(paths):
    data = (workspace / name).read_bytes()
    assert data == (validation / name).read_bytes(), name
    fingerprints.append(dict(path=name, sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints) == 2281
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
next_priority = "Recover real activity/statistics ownership needed by SevenDay.EnterGameInit and menu predicates; complete actual Main/account/data-pool and remaining20 controller factories with recovered services. Connect real resources/SkillControl/language/audio/effects, startup→lobby→battle→reward→return→restart and all business flows, then Player/source audiovisual acceptance."
record = dict(atUtc=now, scope="Project RedDotControl4453, original item/event4457/4454, concrete registry/logic and MenuView binding",
    checksPassed=1161, newIntegratedChecks=8, targetedChecks=8, freshPlayModeRun=True, nativeChecks=17,
    freshPlayerBuild=False, freshPlayerSmoke=False, editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly", "OutgameRedDotPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=[
        "Initial edit-only test used SendMessage on an inactive object; Unity suppresses delivery. It now directly invokes the source method for that edit assertion; native harness separately verifies actual deferred OnDestroy.",
        "Final native path instantiates actual OutgameMenuView and recovered Main/Shop/Commander/MenuTab prefabs. Normally hidden item tab is explicitly activated by the fixture for lifecycle inspection; no claim to source menu visibility or screenshot matching.",
        "Original persistent Tower_CheckReddot record retains null target, empty assembly type and runtime-only state2. No predicate is fabricated. A separate runtime listener is test observation only.",
        "Framework RedDotModule3655 is distinct. Obfuscated34168 HeadportChange array-write boundary is documented in source evidence and is not claimed restored by the public menu lifecycle.",
        "Final native log retains a UnityEditor.Search startup indexing ArgumentOutOfRangeException, separate from all17 passing product assertions.",
        "Source3989352 was resolved to Array.Empty<object>, so it introduces no external clock dependency. Native timing is actual Time.unscaledDeltaTime at timeScale0."
    ], notClaimed="Activity/statistics and other business predicates, full Main/account/remaining20 controllers, new Player or original audiovisual acceptance.")
write(analysis / "red-dot-validation.json", focused)
for name in ("unity-integrated-validation.json", "red-dot-native-validation.json"):
    shutil.copy2(validation / "analysis" / name, analysis / name)
for name in ("red-dot-integrated.log", "red-dot-integrated-final.log", "red-dot-native.log", "red-dot-native-final.log"):
    shutil.copy2(validation / name, analysis / name)
write(out / "RED_DOT_AUDIT.json", dict(status="controller-menu-native-verified-main-activity-pending", atUtc=now,
    sourceEvidence="RED_DOT_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameRedDotControl.cs", "OutgameRedDotItem.cs", "OutgameMenuRedDotBinding.cs", "OutgameMenuView.cs", "OutgameCoreControllerBindings.cs"],
    checks=focused["checks"], nativeChecks=native["checks"], remaining=next_priority))
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, redDotAudit="generated/outgame/RED_DOT_AUDIT.json")
state["validation"].update(integratedChecksPassed=1161, redDot=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="verified-main-activity-composition-pending", integratedChecks=1161, newChecks=8, nativeChecks=17, remainingLifecycleControllers=20))
write(target / "OUTGAME_RESTORE_STATE.json", state)
by_path = {r["path"]: r for r in fingerprints}
ordered = [by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1161, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Project RedDotControl/item/event and actual MenuView binding;1161 full plus17 native checks,18/38 controller lifecycle bindings")
execution["remainingWorkPackages"] = [r.replace("21 controllers", "20 controllers") for r in execution["remainingWorkPackages"]]
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1161, native=17, fingerprints=len(fingerprints), controllers=18, remaining=20)))
