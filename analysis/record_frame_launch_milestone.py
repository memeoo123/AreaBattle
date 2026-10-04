"""Record original frame launch/settings and native Mono pause/focus/time integration."""
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
full=read(validation/"analysis/unity-integrated-validation.json");native=read(validation/"analysis/frame-launch-native-validation.json")
assert full["passed"] and len(full["checks"])==1224 and all(r["result"]=="pass" for r in full["checks"])
assert native["passed"] and len(native["checks"])==15 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1224" in (validation/"frame-launch-integrated.log").read_text()
assert "OutgameFrameLaunchPlayModeValidation.Run" in (validation/"frame-launch-native.log").read_text()
focused=dict(full,checks=[r for r in full["checks"] if r["id"].startswith("frame-launch-")])
focused["limitations"]="Ten original frame launch/settings/pause/focus/sampler/domain cases; native15 verifies actual singleton/Unity messages/TimeModule/timeScale. SDK/transition/banner/global-name/scene endpoints explicit; actual account/Main/activity/Player remain pending."
assert len(focused["checks"])==10
assert read(out/"CONTROLLER_LIFECYCLE_MATRIX.json")["implementedLifecycleControllers"]==18
assert len(read(out/"method-map.json"))==5723
evidence=read(out/"FRAME_LAUNCH_SOURCE_EVIDENCE.json");assert len(evidence["methods"])==22
for row in evidence["methods"]:assert hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()==row["sha256"]
manifest=read(analysis/"VALIDATION_MANIFEST.json");paths={r["path"] for r in manifest["sourceFingerprints"]}
paths.update("UnityProject/Assets/AreaBattle/"+name for name in ("Scripts/OutgameFrameLaunch.cs","Scripts/OutgameFrameWorkMono.cs","Editor/OutgameFrameLaunchValidation.cs","Editor/OutgameFrameLaunchPlayModeValidation.cs"))
fingerprints=[]
for name in sorted(paths):
    data=(workspace/name).read_bytes();assert data==(validation/name).read_bytes(),name
    fingerprints.append(dict(path=name,sha256=hashlib.sha256(data).hexdigest()))
assert len(fingerprints)==2303
now=datetime.datetime.now(datetime.timezone.utc).isoformat();milestone="frame-launch-mono-pause-native"
state=read(target/"OUTGAME_RESTORE_STATE.json");assert not any(r.get("id")==milestone for r in state["milestones"])
next_priority="Restore extracted ActivityControl/ActivityManager/SevenDay.EnterGameInit and connect real ProcedurePreLoad activity initialization; wire actual account/SDK/HTTP/transition/scene/permission/config owners into full Main. Complete remaining20 controllers/all business/reward-return, then Player/source audiovisual acceptance."
record=dict(atUtc=now,scope="Source GameFrameEntry launch/settings/pause and native GameFrameWorkMono focus/priority handling integrated with actual TimeModule",
    checksPassed=1224,newIntegratedChecks=10,targetedChecks=10,freshPlayModeRun=True,nativeChecks=15,
    freshPlayerBuild=False,freshPlayerSmoke=False,editorVersion=full["unityVersion"],platform="OSXEditor",projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation/"UnityProject"),matchingSourceFingerprints=len(fingerprints),
    commands=["BattleBuild.ValidateMechanicsOnly","OutgameFrameLaunchPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=[
        "CommonSettings source SDK/DPI96/fps60/sleep-1/culture/ads/sampling order, transition-deferred StartGame ReadyExitGame/banner/name/scene prefix failures, sampler sticky disable and public domain last-match lookup verified.",
        "Mono singleton always adds a component on existing root; only newly created roots become persistent. Init readiness/thread/support-log order, focus-before-pause, source0/1/2 priorities, silent escalation and retained source after resume preserved.",
        "Native component Unity SendMessage and actual Time.timeScale/TimeModule via frame Update verify flush/reset, scaled pause, focus gate after resume, focus restoration, changeTimeScale=false and shutdown/unregistration/recreation.",
        "Source proof4000760 resolves AddComponent; original DomainData list/config schema and public26530 restored. Obfuscated alternatives remain evidence only.",
        "SDK/transition/banner/global-name/scene endpoints are explicit required hosts; controlled tests do not claim platform callbacks, OS focus behavior or original transition visuals. Main/account/activity and real scene assembly remain pending.",
        "Native UnityEditor.Search startup indexing exception retained separately from15 passing product assertions. No fresh Player or original audiovisual acceptance."
    ],notClaimed="Production Main/account/SDK/HTTP/transition/GameFrameworkLoad/activity/business graph, remaining20 controllers, Player and original audiovisual acceptance.")
write(analysis/"frame-launch-validation.json",focused)
for name in ("unity-integrated-validation.json","frame-launch-native-validation.json"):shutil.copy2(validation/"analysis"/name,analysis/name)
for name in ("frame-launch-integrated.log","frame-launch-native.log"):shutil.copy2(validation/name,analysis/name)
write(out/"FRAME_LAUNCH_AUDIT.json",dict(status="native-pause-focus-verified-main-pending",atUtc=now,sourceEvidence="FRAME_LAUNCH_SOURCE_EVIDENCE.json",verification=record,
    implementation=["OutgameFrameLaunch.cs","OutgameFrameWorkMono.cs"],checks=focused["checks"],nativeChecks=native["checks"],remaining=next_priority))
state.update(lastUpdatedAtUtc=now,currentStage=milestone,nextPriority=next_priority,frameLaunchAudit="generated/outgame/FRAME_LAUNCH_AUDIT.json")
state["validation"].update(integratedChecksPassed=1224,frameLaunch=record)
state["milestones"].append(dict(id=milestone,atUtc=now,status="native-pause-focus-verified-main-pending",integratedChecks=1224,newChecks=10,nativeChecks=15,remainingLifecycleControllers=20))
write(target/"OUTGAME_RESTORE_STATE.json",state)
by_path={r["path"]:r for r in fingerprints};ordered=[by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now,passed=True,caseCount=1224,latestValidation=record,sourceFingerprints=ordered+[by_path[n] for n in sorted(by_path)])
manifest["validationHistory"].append(record);write(analysis/"VALIDATION_MANIFEST.json",manifest)
execution=read(analysis/"RESTORATION_EXECUTION_STATE.json");execution.update(updatedAtUtc=now,verification=record,nextPriority=next_priority)
execution["completedThisRun"].append("Frame launch/settings/transition callback and source Mono pause/focus/sampling/domain lookup;1224 full plus15 native,2303 matching inputs; actual Main/account/SDK/transition/scene/activity host assembly pending")
write(analysis/"RESTORATION_EXECUTION_STATE.json",execution)
print(json.dumps(dict(integrated=1224,native=15,fingerprints=len(fingerprints),indexedMethods=5723,controllers=18,remaining=20)))
