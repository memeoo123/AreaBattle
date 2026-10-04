"""Record the source BattleControl and matching full regression, without new native claims."""
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
assert full["passed"] and len(full["checks"]) == 1153
assert "AREABATTLE_INTEGRATED_PASS cases=1153" in (validation / "battle-controller-integrated.log").read_text()
focused = dict(full, checks=[r for r in full["checks"] if r["id"].startswith("battle-control-")], limitations="Five source controller/config/lifecycle cases from full1153 run; no new native or Player claim.")
assert len(focused["checks"]) == 5
matrix = read(out / "CONTROLLER_LIFECYCLE_MATRIX.json")
assert matrix["implementedLifecycleControllers"] == 17
assert all(m["implementation"] == "OutgameBattleControl.cs" for r in matrix["controllers"] if r["typeIndex"] == 4060 for m in r["lifecycle"])
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "battle-controller-config-lifecycle"
assert not any(r.get("id") == milestone for r in state["milestones"])
manifest = read(analysis / "VALIDATION_MANIFEST.json")
paths = {r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/" + name for name in ("Scripts/OutgameBattleControl.cs", "Editor/OutgameBattleControlValidation.cs"))
fingerprints = []
for name in sorted(paths):
    data = (workspace / name).read_bytes()
    assert data == (validation / name).read_bytes(), name
    fingerprints.append(dict(path=name, sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints) == 2276
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
next_priority = "Compose actual Main/account/data-pool and remaining21 controller factories with recovered services; connect page owners to production resource, SkillControl, localization, atlas, audio/effects modules. Advance startup→lobby→battle→reward→return→restart, all remaining business flows, then Player/original audiovisual acceptance."
record = dict(atUtc=now, scope="Original BattleControl4060 config references, default branches, partial initialization and shared registry/logic lifecycle",
    checksPassed=1153, newIntegratedChecks=5, targetedChecks=5, freshPlayModeRun=False, freshPlayerBuild=False, freshPlayerSmoke=False,
    previousNativeEvidence="guide-page-native-validation.json:26 checks at prior matching1148 revision; not rerun for this pure controller change",
    editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly"],
    notClaimed="Full Main/account/all controllers or complete battle owner composition; no new native/Player/source audiovisual acceptance.")
write(analysis / "battle-controller-validation.json", focused)
shutil.copy2(validation / "analysis/unity-integrated-validation.json", analysis / "unity-integrated-validation.json")
shutil.copy2(validation / "battle-controller-integrated.log", analysis / "battle-controller-integrated.log")
write(out / "BATTLE_CONTROLLER_AUDIT.json", dict(status="source-controller-bound-main-composition-pending", atUtc=now,
    sourceEvidence="BATTLE_CONTROLLER_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameBattleControl.cs", "OutgameCoreControllerBindings.cs"], checks=focused["checks"], remaining=next_priority))
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, battleControllerAudit="generated/outgame/BATTLE_CONTROLLER_AUDIT.json")
state["validation"].update(integratedChecksPassed=1153, battleController=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="verified-main-composition-pending", integratedChecks=1153, newChecks=5, remainingLifecycleControllers=21))
write(target / "OUTGAME_RESTORE_STATE.json", state)
by_path = {r["path"]: r for r in fingerprints}
ordered = [by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1153, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Original BattleControl and concrete registry/logic binding;1153 full checks,17/38 controllers; preserved existing battle kernel parity")
execution["remainingWorkPackages"] = [r.replace("22 controllers", "21 controllers") for r in execution["remainingWorkPackages"]]
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1153, fingerprints=len(fingerprints), controllers=17, remaining=21)))
