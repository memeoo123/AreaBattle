"""Record verified statistics ownership seams; concrete offnet implementation is still required."""
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
def read(path):
    return json.loads(path.read_text())
def write(path, value):
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
assert full["passed"] and len(full["checks"]) == 1184
assert all(row["result"] == "pass" for row in full["checks"])
assert "AREABATTLE_INTEGRATED_PASS cases=1184" in (validation / "statistics-lifecycle-integrated-final.log").read_text()
focused = dict(full, checks=[row for row in full["checks"] if row["id"].startswith("statistics-lifecycle-")],
    limitations="Thirteen source manager/control/expansion/base-strategy checks through real pool and manually driven UpdateManager. Explicit Probe strategy is test-only. Opaque text storage test does not establish source offnet codec/day refresh or production Main. No new native/Player run.")
assert len(focused["checks"]) == 13
assert read(out / "CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"] == 18
evidence = read(out / "STATISTICS_LIFECYCLE_SOURCE_EVIDENCE.json")
assert len(evidence["methods"]) == 62 and len(evidence["runtimeEvidence"]) == 3
for row in evidence["methods"] + evidence["runtimeEvidence"]:
    assert hashlib.sha256((out / row["path"]).read_bytes()).hexdigest() == row["sha256"]
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "statistics-manager-control-lifecycle"
assert not any(row.get("id") == milestone for row in state["milestones"])
manifest = read(analysis / "VALIDATION_MANIFEST.json")
paths = {row["path"] for row in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/" + name for name in (
    "Scripts/OutgameStatisticsManager.cs", "Scripts/OutgameStatisticsControl.cs", "Scripts/OutgameStatisticsStrategy.cs", "Editor/OutgameStatisticsLifecycleValidation.cs"))
fingerprints = []
for name in sorted(paths):
    data = (workspace / name).read_bytes()
    assert data == (validation / name).read_bytes(), name
    fingerprints.append(dict(path=name, sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints) == 2288
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
next_priority = "Implement concrete GameStatisticsOffNetStrategy4623 and StatistUtils codec/server-local clock/day-refresh coroutine, then wire actual CommonGameModule/entry services into manager/control. Continue ActivityManager/ActivityControl and SevenDay.EnterGameInit, actual Main/account/remaining20 controllers/all business/reward-return/save-restart, then Player and source audiovisual acceptance."
record = dict(atUtc=now, scope="GameStatisticsManager4617/control4620/expansion4618/abstract strategy4624 and DataManagerPool.AddModel26734",
    checksPassed=1184, newIntegratedChecks=13, targetedChecks=13,
    freshPlayModeRun=False, nativeChecks=0, freshPlayerBuild=False, freshPlayerSmoke=False,
    editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly"],
    observedBoundaries=[
        "Actual DataManagerPool fallback inserts manager before OnInit, preserves source AutoSyn-only manual registration and duplicate-return behavior. Existing managers reuse strategy; synchronous completion precedes update/disposal subscriptions.",
        "Nineteen captured strategy providers register in original order with first-wins semantics. Messages, count mutation/dirty ordering, resolver reentry, static-save callback rereads, failed load/release boundaries checked.",
        "Source get_initOver returns field16 Action; field20 is a separate constructor bool. Disposal subscribes to GameFrameEntry3433.disposableActions, not a fabricated quit action.",
        "Initial1183 run preceded final cast-helper review. Runtime table1428/function938 proves explicit cast with invalid-type exception, so implementation/assertion corrected and synchronous-completion case added. Final1184 on matching inputs supersedes initial run.",
        "Probe is explicitly test-only; source concrete offnet strategy is a required factory dependency. Abstract base source-empty LoadData/OnSave bodies are preserved, not used to fabricate a working offline implementation.",
        "Real file storage forwards opaque text unchanged. This validates inherited storage seam, not statistics compression/JSON/day-refresh save-restart. Manual UpdateManager.Update verifies real delegate removal logic without a native PlayMode claim."
    ], notClaimed="Concrete offnet codec/clock/day refresh and production CommonGameModule/GameFrameEntry/Main composition; activities/remaining20 controllers/full business/Player/audiovisual acceptance.")
write(analysis / "statistics-lifecycle-validation.json", focused)
shutil.copy2(validation / "analysis/unity-integrated-validation.json", analysis / "unity-integrated-validation.json")
shutil.copy2(validation / "statistics-lifecycle-integrated-final.log", analysis / "statistics-lifecycle-integrated-final.log")
write(out / "STATISTICS_LIFECYCLE_AUDIT.json", dict(status="manager-control-base-verified-concrete-offnet-pending", atUtc=now,
    sourceEvidence="STATISTICS_LIFECYCLE_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameStatisticsManager.cs", "OutgameStatisticsControl.cs", "OutgameStatisticsStrategy.cs", "OutgameDataManagerPool.cs"], checks=focused["checks"], remaining=next_priority))
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority,
    statisticsLifecycleAudit="generated/outgame/STATISTICS_LIFECYCLE_AUDIT.json")
state["validation"].update(integratedChecksPassed=1184, statisticsLifecycle=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="manager-control-base-verified-concrete-offnet-pending",
    integratedChecks=1184, newChecks=13, nativeChecks=0, remainingLifecycleControllers=20))
write(target / "OUTGAME_RESTORE_STATE.json", state)
by_path = {row["path"]: row for row in fingerprints}
ordered = [by_path.pop(row["path"]) for row in manifest["sourceFingerprints"] if row["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1184, latestValidation=record,
    sourceFingerprints=ordered + [by_path[name] for name in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Statistics manager/control/expansion/abstract strategy and actual pool/UpdateManager seams;1184 full checks,2288 matching inputs; concrete offnet/production entry pending and38-roster count unchanged18")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1184, freshNative=0, fingerprints=len(fingerprints), controllers=18, remaining=20)))
