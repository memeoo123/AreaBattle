"""Record concrete offnet persistence/native verification, keeping refresh/Main boundaries explicit."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import shutil

parser=argparse.ArgumentParser()
parser.add_argument("validation_root",type=Path)
validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent
analysis=workspace/"analysis"
target=analysis/"targets/wxcf1394487200e48f/43"
out=target/"generated/outgame"
def read(path):return json.loads(path.read_text())
def write(path,value):
    crlf=path.exists() and b"\r\n" in path.read_bytes()
    text=json.dumps(value,ensure_ascii=False,indent=2)+"\n"
    path.write_bytes((text.replace("\n","\r\n") if crlf else text).encode())

full=read(validation/"analysis/unity-integrated-validation.json")
native=read(validation/"analysis/statistics-offnet-native-validation.json")
assert full["passed"] and len(full["checks"])==1193 and all(r["result"]=="pass" for r in full["checks"])
assert native["passed"] and len(native["checks"])==15 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1193" in (validation/"statistics-offnet-integrated-final.log").read_text()
assert "OutgameStatisticsOffNetPlayModeValidation.Run" in (validation/"statistics-offnet-native-final.log").read_text()
focused=dict(full,checks=[r for r in full["checks"] if r["id"].startswith("statistics-offnet-")],
    limitations="Nine offnet/codec/persistence cases; native15 separately verifies actual UpdateManager/WaitUntil/file restart. SDK/HTTP and daily refresh endpoints are explicit fixtures; original TimeToRefreshControl/Main/platform/Player remain pending.")
assert len(focused["checks"])==9
assert read(out/"CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"]==18
assert len(read(out/"method-map.json"))==5593
evidence=read(out/"STATISTICS_OFFNET_SOURCE_EVIDENCE.json")
assert len(evidence["methods"])==45
for row in evidence["methods"]:assert hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()==row["sha256"]
state=read(target/"OUTGAME_RESTORE_STATE.json")
milestone="statistics-offnet-codec-native-persistence"
assert not any(r.get("id")==milestone for r in state["milestones"])
manifest=read(analysis/"VALIDATION_MANIFEST.json")
paths={r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/"+name for name in (
    "Scripts/OutgameStatisticsCodec.cs","Scripts/OutgameStatisticsOffNetStrategy.cs",
    "Editor/OutgameStatisticsOffNetValidation.cs","Editor/OutgameStatisticsOffNetPlayModeValidation.cs"))
fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2292
now=datetime.datetime.now(datetime.timezone.utc).isoformat()
next_priority="Restore actual TimeToRefreshControl4589 scheduling/boundaries (23 methods extracted), wire concrete statistics runtime to real entry/account/SDK/HTTP/common-module owners, then ActivityManager/ActivityControl/SevenDay.EnterGameInit. Complete Main/remaining20 controllers/all business/save-restart/reward-return, then Player/source audiovisual acceptance."
record=dict(atUtc=now,scope="Concrete statistics offnet4623, StatistUtils4619 and native owner/pool/update/coroutine/compressed file persistence",
    checksPassed=1193,newIntegratedChecks=9,targetedChecks=9,freshPlayModeRun=True,nativeChecks=15,
    freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full["unityVersion"],platform="OSXEditor",
    projectVersionPreserved="6000.0.68f1",isolatedProject=str(validation/"UnityProject"),matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly","OutgameStatisticsOffNetPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=[
        "Real concrete strategy used throughout; native manager/control loads compressed files, waits for actual pool readiness and saves through actual UpdateManager frames. Independent owner teardown/recreation restores signed64 counts and timestamps.",
        "Paused native scaled timers and advancing realtime anchor, completed coroutine handle retention, deferred server data, explicit failed/success ad messages, source daily callback, restart and queued update removal all verified.",
        "TimeToRefreshControl itself is not yet restored. Fixture records exact source registration/removal and invokes daily callback explicitly; this is not automatic cross-day scheduling or a platform-result claim.",
        "Final metadata review resolved parameter20643 to RefreshDelegate4584, not a bool or offset. Registration/removal signatures now take nullable second Action<long>; final native/full runs use matching corrected inputs. Earlier logs are retained as pre-callback-type-review evidence.",
        "StatistUtils gzip handles external Python gzip fixture and source raw-JSON fallback. Save clears dirty before normalization/serialization/storage; failing writes/invalid rows retain source partial state. Byte-identical compressed output across runtimes is not claimed.",
        "Native final log retains independent UnityEditor.Search startup indexing ArgumentOutOfRangeException, separate from15 passing product assertions. No screenshot/Player or original audiovisual acceptance in this batch."
    ],notClaimed="Original automatic refresh dispatcher, real SDK/HTTP/account/Main/common-module composition, activities/remaining20 controllers/full business, Player/original audiovisual acceptance.")
write(analysis/"statistics-offnet-validation.json",focused)
for name in ("unity-integrated-validation.json","statistics-offnet-native-validation.json"):
    shutil.copy2(validation/"analysis"/name,analysis/name)
for name in ("statistics-offnet-integrated.log","statistics-offnet-integrated-final.log","statistics-offnet-native.log","statistics-offnet-native-final.log"):
    shutil.copy2(validation/name,analysis/name)
write(out/"STATISTICS_OFFNET_AUDIT.json",dict(status="concrete-offnet-native-persistence-verified-refresh-main-pending",atUtc=now,
    sourceEvidence="STATISTICS_OFFNET_SOURCE_EVIDENCE.json",verification=record,
    implementation=["OutgameStatisticsCodec.cs","OutgameStatisticsOffNetStrategy.cs"],checks=focused["checks"],nativeChecks=native["checks"],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,statisticsOffNetAudit="generated/outgame/STATISTICS_OFFNET_AUDIT.json")
state["validation"].update(integratedChecksPassed=1193,statisticsOffNet=record)
state["milestones"].append(dict(id=milestone,atUtc=now,status="native-persistence-verified-refresh-main-pending",integratedChecks=1193,newChecks=9,nativeChecks=15,remainingLifecycleControllers=20))
write(target/"OUTGAME_RESTORE_STATE.json",state)
by_path={r["path"]:r for r in fingerprints}
ordered=[by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1193,latestValidation=record,sourceFingerprints=ordered+[by_path[name] for name in sorted(by_path)])
manifest["validationHistory"].append(record);write(analysis/"VALIDATION_MANIFEST.json",manifest)
execution=read(analysis/"RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution["completedThisRun"].append("Concrete offnet statistics/codec with real compressed file restart, native readiness coroutine and UpdateManager;1193 full plus15 native,2292 matching inputs; original daily scheduler/Main pending")
write(analysis/"RESTORATION_EXECUTION_STATE.json",execution)
print(json.dumps(dict(integrated=1193,native=15,fingerprints=len(fingerprints),indexedMethods=5593,controllers=18,remaining=20)))
