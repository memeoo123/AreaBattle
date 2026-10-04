"""Record frame module dispatch, pool release and native concrete statistics runtime teardown/reload."""
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
full=read(validation/"analysis/unity-integrated-validation.json");native=read(validation/"analysis/frame-entry-native-validation.json")
assert full["passed"] and len(full["checks"])==1214 and all(r["result"]=="pass" for r in full["checks"])
assert native["passed"] and len(native["checks"])==15 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1214" in (validation/"frame-entry-integrated-final.log").read_text()
assert "OutgameFrameEntryPlayModeValidation.Run" in (validation/"frame-entry-native.log").read_text()
focused=dict(full,checks=[r for r in full["checks"] if r["id"].startswith("frame-")])
focused["limitations"]="Eleven source frame module and pool release cases; native15 verifies actual module/runtime/pool shutdown and compressed file reload. Full entry launch/settings/pause, account/SDK/HTTP/activity/Main and Player remain pending."
assert len(focused["checks"])==11
assert read(out/"CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"]==18
assert len(read(out/"method-map.json"))==5718
evidence=read(out/"FRAME_ENTRY_SOURCE_EVIDENCE.json");assert len(evidence["methods"])==20
for row in evidence["methods"]+evidence["generics"]:assert hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()==row["sha256"]
manifest=read(analysis/"VALIDATION_MANIFEST.json");paths={r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/"+name for name in ("Scripts/OutgameFrameEntry.cs","Scripts/OutgameStatisticsRuntime.cs","Editor/OutgameFrameEntryValidation.cs","Editor/OutgameFrameEntryPlayModeValidation.cs"))
fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2299
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone="frame-entry-statistics-runtime-native-teardown"
state=read(target/"OUTGAME_RESTORE_STATE.json");assert not any(r.get("id")==milestone for r in state["milestones"])
next_priority="Complete GameFrameEntry launch/settings/pause and actual account/SDK/HTTP host wiring; restore extracted ActivityManager/ActivityControl/SevenDay.EnterGameInit. Complete Main/remaining20 controllers/all business/save-restart/reward-return, then Player/source audiovisual acceptance."
record=dict(atUtc=now,scope="Source GameFrameEntry module core, actual data-pool release and concrete statistics runtime native teardown/file reload",
    checksPassed=1214,newIntegratedChecks=11,targetedChecks=11,freshPlayModeRun=True,nativeChecks=15,
    freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full["unityVersion"],platform="OSXEditor",projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation/"UnityProject"),matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly","OutgameFrameEntryPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=[
        "Source priority sorting, exact-type lookup, copied module list, callback-before-initialize, live node update and reverse shutdown retain source exception/reentry ordering. Existing Logic/FSM/Procedure/Time modules now implement shared frame contract.",
        "DataManagerPool closes readiness then SaveData/OnRelease/Clear. Per-manager save errors are logged; release errors propagate and preserve dictionary. LocalData/Skin source OnRelease are nop and are explicitly restored.",
        "StatisticsRuntime composes real control/manager/offnet/shared common messages/UpdateManager/refresh and uses same frame disposable field. Native actual LogicModule pool release occurs before frame global tail and stats/refresh cleanup; independent runtime restores compressed file.",
        "Full GameFrameEntry launch/settings/pause, GameFrameWorkMono, actual account/SDK/HTTP/permission/config owners, activity/full Main remain pending. Factories supply existing concrete modules; SDK and host inputs are explicit native fixtures.",
        "Initial compile found one older login-sync PoolManager fixture lacked new OnRelease interface member; fixture corrected, final1214 run is authoritative. Initial compile log retained.",
        "Native log retains independent UnityEditor.Search startup indexing exception separate from15 passing assertions. No new Player or original audiovisual acceptance."
    ],notClaimed="Complete production GameFrameEntry/Main/account/SDK/HTTP/activity/business graph, remaining20 controllers, Player and original audiovisual acceptance.")
write(analysis/"frame-entry-validation.json",focused)
for name in ("unity-integrated-validation.json","frame-entry-native-validation.json"):shutil.copy2(validation/"analysis"/name,analysis/name)
for name in ("frame-entry-integrated.log","frame-entry-integrated-final.log","frame-entry-native.log"):shutil.copy2(validation/name,analysis/name)
write(out/"FRAME_ENTRY_AUDIT.json",dict(status="native-frame-teardown-verified-main-pending",atUtc=now,sourceEvidence="FRAME_ENTRY_SOURCE_EVIDENCE.json",verification=record,
    implementation=["OutgameFrameEntry.cs","OutgameStatisticsRuntime.cs","OutgameDataManagerPool.cs"],checks=focused["checks"],nativeChecks=native["checks"],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,frameEntryAudit="generated/outgame/FRAME_ENTRY_AUDIT.json")
state["validation"].update(integratedChecksPassed=1214,frameEntry=record)
state["milestones"].append(dict(id=milestone,atUtc=now,status="native-frame-teardown-verified-main-pending",integratedChecks=1214,newChecks=11,nativeChecks=15,remainingLifecycleControllers=20))
write(target/"OUTGAME_RESTORE_STATE.json",state)
by_path={r["path"]:r for r in fingerprints};ordered=[by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1214,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)])
manifest["validationHistory"].append(record);write(analysis/"VALIDATION_MANIFEST.json",manifest)
execution=read(analysis/"RESTORATION_EXECUTION_STATE.json");execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution["completedThisRun"].append("FrameEntry module core and real data-pool release; concrete statistics runtime native shutdown/gzip file reload;1214 full plus15 native,2299 matching inputs; full launch/settings/pause and Main/account/SDK/activity graph pending")
write(analysis/"RESTORATION_EXECUTION_STATE.json",execution)
print(json.dumps(dict(integrated=1214,native=15,fingerprints=len(fingerprints),indexedMethods=5718,controllers=18,remaining=20)))
