"""Record original guide-book assets and reward binding after matching integration validation."""
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
focused = read(validation / "analysis/guide-book-rewards-validation.json")
assert full["passed"] and len(full["checks"]) == 1120
assert focused["passed"] and len(focused["checks"]) == 7
assert "AREABATTLE_INTEGRATED_PASS cases=1120" in (validation / "guide-book-rewards-integrated.log").read_text()
imported = read(validation / "analysis/unity-guide-book-import-report.json")
assert imported["prefabs"] == 2 and imported["sprites"] == 58 and imported["fonts"] == 1
acquisition = read(target / "generated/resource-snapshots/guide-book-ui-20261003/acquisition-manifest.json")
assert len(acquisition["items"]) == 7
for item in acquisition["items"]:
    data = (target / item["path"]).read_bytes()
    assert item["status"] == "verified" and len(data) == item["catalog"]["size"]
    assert hashlib.md5(data).hexdigest() == item["catalog"]["md5"]
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or "/Recovered/GuideBook/" in p.as_posix() or "/FirstPack/" in p.as_posix() or "/Recovered/Loading/" in p.as_posix() or "/Recovered/Audio/" in p.as_posix())]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
record = {"atUtc": now, "scope": "Original GuideBookUI resources and native reward-button binding with actual LocalData/GuideBook persistence",
          "checksPassed": 1120, "newIntegratedChecks": 7, "targetedChecks": 7, "nativeUguiButtonChecks": True,
          "freshPlayModeRun": False, "freshPlayerBuild": False, "freshPlayerSmoke": False,
          "previousNativeEvidence": "item-runtime-native-validation.json and item-profile-native.log:23 checks, previous service-group batch",
          "editorVersion": full["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
          "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints),
          "commands": ["OutgameGuideBookRewardsValidation.Validate", "BattleBuild.ValidateMechanicsOnly"],
          "notClaimed": "Full guide-book popup/list/tab/Spine/native-frame/effects, Main/account/22 remaining controllers, SDK delivery or full Player"}
record["importedResources"] = {"prefabs": 2, "nodes": [81, 10], "sprites": 58, "fonts": 1, "verifiedSourceBundles": 7,
    "manifestSha256": hashlib.sha256((outgame / "guide-book-ui-import.json").read_bytes()).hexdigest(),
    "pendingComponents": imported["skippedComponents"]}
state = read(target / "OUTGAME_RESTORE_STATE.json")
assert not any(m.get("id") == "guide-book-original-assets-reward-binding" for m in state["milestones"])
for filename in ("unity-integrated-validation.json", "guide-book-rewards-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-book-rewards.log", "guide-book-rewards-integrated.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(outgame / "GUIDE_BOOK_UI_REWARD_AUDIT.json", {"status": "source-assets-and-reward-binding-verified-full-page-main-pending", "atUtc": now,
    "sourceEvidence": "GUIDE_BOOK_UI_SOURCE_EVIDENCE.json", "verification": record,
    "implementation": ["OutgameGuideBookRewards.cs", "OutgameGuideBookRewardBinding.cs", "RecoveredHudImporter.cs"],
    "checks": focused["checks"], "remaining": ["Actual popup/list/tab/TipBookItem/Spine lifecycle and native effects; reward callback and economic/claim persistence are implemented.", "Main/account/level progression production composition and full Player acceptance."]})
next_priority = "Finish original GuideBookUI popup/list/tab/TipBookItem/Spine ownership using recovered52 methods and imported resources; connect actual effects. Continue complete Main/account/22 controllers and all business flows, then fresh Player and original visual/audio/timing acceptance."
state.update(lastUpdatedAtUtc=now, currentStage="guide-book-original-assets-reward-binding", nextPriority=next_priority, guideBookUiRewardAudit="generated/outgame/GUIDE_BOOK_UI_REWARD_AUDIT.json")
state["validation"].update(integratedChecksPassed=1120, guideBookUiRewards=record)
state["milestones"].append({"id": "guide-book-original-assets-reward-binding", "atUtc": now, "status": "verified-ui-full-main-composition-pending", "integratedChecks": 1120, "newChecks": 7, "remainingLifecycleControllers": 22})
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {f["path"]: f for f in fingerprints}
ordered = [by_path.pop(f["path"]) for f in manifest["sourceFingerprints"] if f["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1120, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Original GuideBookUI assets, native reward-button dispatch and actual economic/claim record persistence")
execution["remainingWorkPackages"] = [p.replace("23 controllers", "22 controllers") for p in execution["remainingWorkPackages"]]
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
for filename in ("unity-guide-book-import-report.json",):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-book-import.log", "guide-book-import-final.log"):
    shutil.copy2(validation / filename, analysis / filename)
print(json.dumps({"integrated": 1120, "fingerprints": len(fingerprints), "controllers": 16, "remaining": 22}))
