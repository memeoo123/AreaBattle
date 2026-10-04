"""Record the verified statistics record foundation without claiming its unfinished owner chain."""
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
assert full["passed"] and len(full["checks"]) == 1171
assert all(row["result"] == "pass" for row in full["checks"])
assert "AREABATTLE_INTEGRATED_PASS cases=1171" in (validation / "statistics-records-integrated.log").read_text()
focused = dict(full, checks=[row for row in full["checks"] if row["id"].startswith(("common-msg-", "statistics-record-"))],
    limitations="Ten CommonMsgDispatcher/record/JSON/key-cache checks. JSON round-trip is not actual statistics disk persistence. Full statistics manager/controller/offline strategy, CommonGameModule ownership and activity/Main/Player are unfinished. Prior red-dot native17 were not rerun for this batch.")
assert len(focused["checks"]) == 10
assert len(read(out / "method-map.json")) == 5570
assert read(out / "CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"] == 18
evidence = read(out / "STATISTICS_SOURCE_EVIDENCE.json")
assert len(evidence["sourceMethods"]) == 123
for row in evidence["sourceMethods"]:
    assert hashlib.sha256((out / row["path"]).read_bytes()).hexdigest() == row["sha256"]
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "statistics-records-common-messages"
assert not any(row.get("id") == milestone for row in state["milestones"])
manifest = read(analysis / "VALIDATION_MANIFEST.json")
paths = {row["path"] for row in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/" + name for name in (
    "Scripts/OutgameCommonMessageDispatcher.cs", "Scripts/OutgameStatisticsRecords.cs", "Editor/OutgameStatisticsRecordsValidation.cs"))
fingerprints = []
for name in sorted(paths):
    data = (workspace / name).read_bytes()
    assert data == (validation / name).read_bytes(), name
    fingerprints.append(dict(path=name, sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints) == 2284
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
next_priority = "Complete source GameStatisticsManager/control/offnet strategy and CommonGameModule data-pool ownership, then ActivityManager/ActivityControl and SevenDay.EnterGameInit. Finish actual Main/account/remaining20 controller factories and full business/reward-return/save-restart flow, then Player and source audiovisual acceptance."
record = dict(atUtc=now, scope="CommonMsgDispatcher4569 and mutable statistics records35015..35022, DTOs/key cache/index loop",
    checksPassed=1171, newIntegratedChecks=10, targetedChecks=10,
    freshPlayModeRun=False, nativeChecks=0, freshPlayerBuild=False, freshPlayerSmoke=False,
    editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly"],
    observedBoundaries=[
        "Distinct common/game message domains; replacing common singleton preserves captured old instances. Multicast mutation, null callbacks, shallow copy/key append and exception propagation checked.",
        "Parent/child independence, null versus empty child-list lookup, first duplicate child, itemId0 first-creation asymmetry, null-dictionary overload differences, signed64 overflow and notification deltas checked.",
        "JSON field/long values round-trip, indexing references/partial mutation and culture-dependent first-key caching checked; actual statistics disk save/restart not yet implemented.",
        "CommonGameModule source registration identified; record foundation does not register an unfinished statistics manager or fabricate default activity values.",
        "Prior red-dot17 native and guide-page26 native evidence remain historical suites; no fresh native run, captures or Player in this pure record/delegate batch."
    ], notClaimed="Full statistics manager/controller/offline/day refresh, activity/seven-day, Main/account/remaining20 controllers, Player or original audiovisual acceptance.")
write(analysis / "statistics-records-validation.json", focused)
shutil.copy2(validation / "analysis/unity-integrated-validation.json", analysis / "unity-integrated-validation.json")
shutil.copy2(validation / "statistics-records-integrated.log", analysis / "statistics-records-integrated.log")
write(out / "STATISTICS_RECORDS_AUDIT.json", dict(status="record-foundation-verified-full-owner-chain-pending", atUtc=now,
    sourceEvidence="STATISTICS_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameCommonMessageDispatcher.cs", "OutgameStatisticsRecords.cs"], checks=focused["checks"], remaining=next_priority))
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority,
    statisticsRecordsAudit="generated/outgame/STATISTICS_RECORDS_AUDIT.json")
state["validation"].update(integratedChecksPassed=1171, statisticsRecords=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="record-foundation-verified-owner-chain-pending",
    integratedChecks=1171, newChecks=10, nativeChecks=0, remainingLifecycleControllers=20))
write(target / "OUTGAME_RESTORE_STATE.json", state)
by_path = {row["path"]: row for row in fingerprints}
ordered = [by_path.pop(row["path"]) for row in manifest["sourceFingerprints"] if row["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1171, latestValidation=record,
    sourceFingerprints=ordered + [by_path[name] for name in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Source common-message domain and mutable statistics records/DTO/key-cache foundation;1171 full checks,2284 matching inputs; full statistics owner chain and controller count remain pending/18 of38")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1171, freshNative=0, fingerprints=len(fingerprints), indexedMethods=5570, controllers=18, remaining=20)))
