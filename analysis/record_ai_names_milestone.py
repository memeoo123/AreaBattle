"""Record AI-name/profile composition only against matching isolated validation inputs."""
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
native = read(validation / "analysis/item-runtime-native-validation.json")
names = read(validation / "analysis/ai-name-validation.json")
assert full["passed"] and len(full["checks"]) == 1105
assert native["passed"] and len(native["checks"]) == 23
assert names["passed"] and len(names["checks"]) == 14
assert "AREABATTLE_INTEGRATED_PASS cases=1105" in (validation / "ai-names-integrated-final.log").read_text()
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or "/FirstPack/" in p.as_posix() or "/Recovered/Loading/" in p.as_posix() or "/Recovered/Audio/" in p.as_posix())]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
record = {"atUtc": now, "scope": "Original AI-name generation and production UserInfoRuntime with native item/profile persistence integration",
          "checksPassed": 1105, "newIntegratedChecks": 7, "nativePlayModeChecks": 23, "targetedChecks": 14,
          "freshPlayModeRun": True, "freshPlayerBuild": False, "freshPlayerSmoke": False,
          "editorVersion": full["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
          "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints),
          "commands": ["OutgameAiNamesValidation.Validate", "OutgameItemRuntimePlayModeValidation.Run", "BattleBuild.ValidateMechanicsOnly"],
          "notClaimed": "Full Main/account/23 remaining controllers, remote SDK/report delivery, original seeded RNG sequence, dictionary iteration cross-runtime parity or full Player"}
state = read(target / "OUTGAME_RESTORE_STATE.json")
assert not any(m.get("id") == "original-ai-names-user-info-runtime" for m in state["milestones"])
for filename in ("unity-integrated-validation.json", "item-runtime-native-validation.json", "ai-name-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("ai-names.log", "ai-names-integrated.log", "ai-names-integrated-final.log", "item-profile-native.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(outgame / "AI_NAME_AUDIT.json", {
    "status": "source-algorithm-and-service-composition-verified-full-main-pending", "atUtc": now,
    "sourceEvidence": "AI_NAME_SOURCE_EVIDENCE.json", "verification": record,
    "implementation": ["GameRandomSource.cs", "OutgameAiNames.cs", "OutgameUserInfoRuntime.cs"],
    "verified": ["Original country remapping, saturation removal/retry, half-open distinct name sampling and source insertion order.",
                 "Unity country RNG and shared managed name/insertion RNG remain separate; exact original error and lookup order preserved.",
                 "Real original tables drive default profile names; item10004 shares the actual UserInfoControl manager.",
                 "Native bundle/frame execution, login-gated writes, avatar unlock, actual file save/restart and explicit server callback verified."],
    "remaining": ["Full Main/account, timestamp/install-state and report host composition.",
                  "Original seeded random sequence and cross-runtime dictionary order parity are unverified."]})
notes = read(outgame / "AI_NAME_RESTORE_NOTES.json")
notes.update(status="implemented-and-validated-see-ai-name-audit", nextImplementation="Full Main/account host composition", audit="AI_NAME_AUDIT.json")
notes["observations"][0] = notes["observations"][0].replace("fields148/20", "field offsets148/20")
notes["observations"][-1] = "Generic method specs and actual config field indexes/offsets recorded in AI_NAME_SOURCE_EVIDENCE.json; implementation validation recorded in AI_NAME_AUDIT.json."
notes["acceptanceStillRequired"] = ["Full Main/account/server-time/install-state/report composition", "Original-device RNG/iteration/runtime parity and full Player acceptance"]
write(outgame / "AI_NAME_RESTORE_NOTES.json", notes)
user = read(outgame / "USER_INFO_AUDIT.json")
user.update(status="manager-controller-original-names-and-native-item-persistence-verified-main-pending", latestVerification=record,
            nameProviderAudit="AI_NAME_AUDIT.json", remaining=["Full Main/account/time/AppInfo/report host composition remains required; no SDK delivery claimed."])
write(outgame / "USER_INFO_AUDIT.json", user)
next_priority = "Compose full Main/account/data-pool factories using ItemRuntime and UserInfoRuntime. Recover remaining CommanderUI/Dice/report owners and23 controllers; complete all menu/business/battle-return flows, fresh Player and original visual/audio/timing acceptance."
state.update(lastUpdatedAtUtc=now, currentStage="original-ai-names-user-info-runtime", nextPriority=next_priority, aiNameAudit="generated/outgame/AI_NAME_AUDIT.json")
state["validation"].update(integratedChecksPassed=1105, aiNamesAndUserInfoRuntime=record)
state["milestones"].append({"id": "original-ai-names-user-info-runtime", "atUtc": now, "status": "verified-full-main-composition-pending", "integratedChecks": 1105, "newChecks": 7, "nativeChecks": 23, "remainingLifecycleControllers": 23})
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {f["path"]: f for f in fingerprints}
ordered = [by_path.pop(f["path"]) for f in manifest["sourceFingerprints"] if f["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1105, latestValidation=record,
                sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].extend(["Source AI name generation and actual UserInfo runtime provider", "Native item/profile shared reward save/restart and deferred server callback"])
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps({"integrated": 1105, "native": 23, "fingerprints": len(fingerprints), "controllers": 15, "remaining": 23}))
