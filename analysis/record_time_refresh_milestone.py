"""Record checked source refresh scheduling and native offnet automatic cross-day persistence."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import shutil

parser=argparse.ArgumentParser();parser.add_argument("validation_root",type=Path)
validation=parser.parse_args().validation_root.resolve()
workspace=Path(__file__).resolve().parent.parent;analysis=workspace/"analysis"
target=analysis/"targets/wxcf1394487200e48f/43";out=target/"generated/outgame"
def read(p):return json.loads(p.read_text())
def write(p,value):
    crlf=p.exists() and b"\r\n" in p.read_bytes();text=json.dumps(value,ensure_ascii=False,indent=2)+"\n"
    p.write_bytes((text.replace("\n","\r\n") if crlf else text).encode())
full=read(validation/"analysis/unity-integrated-validation.json");native=read(validation/"analysis/time-refresh-native-validation.json")
assert full["passed"] and len(full["checks"])==1203 and all(r["result"]=="pass" for r in full["checks"])
assert native["passed"] and len(native["checks"])==15 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1203" in (validation/"time-refresh-integrated.log").read_text()
assert "OutgameTimeToRefreshPlayModeValidation.Run" in (validation/"time-refresh-native.log").read_text()
focused=dict(full,checks=[r for r in full["checks"] if r["id"].startswith("time-refresh-")])
focused["limitations"]="Ten original refresh scheduling/V1/V2 mutation cases. Native15 independently verifies actual Unity Update/offnet readiness/automatic cross-day/compressed file restart and scaled cleanup. SDK inputs controlled; production Main/account/platform/Player remain pending."
assert len(focused["checks"])==10
assert read(out/"CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"]==18
assert len(read(out/"method-map.json"))==5593
evidence=read(out/"TIME_REFRESH_SOURCE_EVIDENCE.json");assert len(evidence["methods"])==23
for row in evidence["methods"]:assert hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()==row["sha256"]
manifest=read(analysis/"VALIDATION_MANIFEST.json");paths={r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/"+name for name in ("Scripts/OutgameTimeToRefreshControl.cs","Editor/OutgameTimeToRefreshValidation.cs","Editor/OutgameTimeToRefreshPlayModeValidation.cs"))
fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2295
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone="time-refresh-native-automatic-crossday"
state=read(target/"OUTGAME_RESTORE_STATE.json");assert not any(r.get("id")==milestone for r in state["milestones"])
next_priority="Wire concrete statistics/refresh/common-module runtime to real GameFrameEntry/account/SDK/HTTP owners; restore ActivityManager/ActivityControl/SevenDay.EnterGameInit. Complete Main/remaining20 controllers/all business/save-restart/reward-return, then Player/source audiovisual acceptance."
record=dict(atUtc=now,scope="Source TimeToRefreshControl4589 public scheduler/V1/V2 plus native automatic offnet daily reset, file restart and scaled cleanup",
    checksPassed=1203,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=True,nativeChecks=15,
    freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full["unityVersion"],platform="OSXEditor",projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation/"UnityProject"),matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly","OutgameTimeToRefreshPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=[
        "Duration-based refresh, hour alignment/previous-day fallback, duplicate due countdowns, single-pass scaled timer, release-clock fallback, V1/V2 coalescing/removal and source existing-key countdown quirk verified.",
        "Live list/dictionary mutation, callback exceptions with committed boundary, null keyed callbacks, unvalidated intervals and reference-identity array keys preserve source public-path behavior. Obfuscated alternate loops are not asserted equivalent.",
        "Native actual Update advances scheduler; concrete offnet WaitUntil registers with singleton; automatic initial/next-day callbacks reset and save gzip file. New owner graph restores timestamps/counts without same-day duplicate refresh.",
        "Native ordinary ExitGame clears immediately; awaited WaitForSeconds(.1f) cleanup remains pending during pause, completes after scaled resume, and leaves singleton; destruction clears/unsubscribes.",
        "SDK timestamps/release flags and ad/item result messages are controlled explicit inputs. Binding to actual production GameFrameEntry/SDK/HTTP/account/Main/activity owners remains pending.",
        "Native log retains independent UnityEditor.Search startup ArgumentOutOfRangeException, separate from15 passing assertions. No fresh Player or source audiovisual acceptance in this batch."
    ],notClaimed="Production SDK/HTTP/account/Main/common-module composition, activities/remaining20 controllers/full business, Player/original audiovisual acceptance.")
write(analysis/"time-refresh-validation.json",focused)
for name in ("unity-integrated-validation.json","time-refresh-native-validation.json"):shutil.copy2(validation/"analysis"/name,analysis/name)
for name in ("time-refresh-integrated.log","time-refresh-native.log"):shutil.copy2(validation/name,analysis/name)
write(out/"TIME_REFRESH_AUDIT.json",dict(status="native-automatic-crossday-verified-main-pending",atUtc=now,sourceEvidence="TIME_REFRESH_SOURCE_EVIDENCE.json",verification=record,
    implementation=["OutgameTimeToRefreshControl.cs"],checks=focused["checks"],nativeChecks=native["checks"],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,timeRefreshAudit="generated/outgame/TIME_REFRESH_AUDIT.json")
state["validation"].update(integratedChecksPassed=1203,timeRefresh=record)
state["milestones"].append(dict(id=milestone,atUtc=now,status="native-automatic-crossday-verified-main-pending",integratedChecks=1203,newChecks=10,nativeChecks=15,remainingLifecycleControllers=20))
write(target/"OUTGAME_RESTORE_STATE.json",state)
by_path={r["path"]:r for r in fingerprints};ordered=[by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1203,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)])
manifest["validationHistory"].append(record);write(analysis/"VALIDATION_MANIFEST.json",manifest)
execution=read(analysis/"RESTORATION_EXECUTION_STATE.json");execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution["completedThisRun"].append("Source TimeToRefreshControl public scheduler/V1/V2 with native automatic offnet cross-day reset, gzip save/restart and scaled cleanup;1203 full plus15 native,2295 matching inputs; production Main/account/SDK/activity composition pending")
write(analysis/"RESTORATION_EXECUTION_STATE.json",execution)
print(json.dumps(dict(integrated=1203,native=15,fingerprints=len(fingerprints),indexedMethods=5593,controllers=18,remaining=20)))
