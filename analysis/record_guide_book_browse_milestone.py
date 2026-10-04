"""Record source-backed guide-book item/tab behavior after full and rendered-frame validation."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import re
import shutil

parser = argparse.ArgumentParser()
parser.add_argument("validation_root", type=Path)
validation = parser.parse_args().validation_root.resolve()
workspace = Path(__file__).resolve().parent.parent
analysis = workspace / "analysis"
target = analysis / "targets/wxcf1394487200e48f/43"
outgame = target / "generated/outgame"
def read(path):
    return json.loads(path.read_text())
def write(path, value):
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
native = read(validation / "analysis/guide-book-browse-native-validation.json")
assert full["passed"] and len(full["checks"]) == 1127
assert native["passed"] and len(native["checks"]) == 23
assert "AREABATTLE_INTEGRATED_PASS cases=1127" in (validation / "guide-book-browse-integrated.log").read_text()
ids = re.findall(r'check\("([^"]+)"', (workspace / "UnityProject/Assets/AreaBattle/Editor/OutgameGuideBookBrowseValidation.cs").read_text())
focused = dict(full, checks=[c for c in full["checks"] if c["id"] in ids],
               limitations="Extracted seven guide-book browse checks from the full1127 run. Native23 checks are a separate rendered, non-batch PlayMode run. Localization/voice/toast/effects/open-popup remain fixtures.")
assert len(focused["checks"]) == len(ids) == 7
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "guide-book-items-tabs-native-layout"
assert not any(m.get("id") == milestone for m in state["milestones"])
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or "/Recovered/GuideBook/" in p.as_posix() or "/FirstPack/" in p.as_posix() or "/Recovered/Loading/" in p.as_posix() or "/Recovered/Audio/" in p.as_posix())]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
record = {"atUtc": now, "scope": "Original guide-book row rendering, tip collection/selection, TabButton/Group, click overloads and native end-of-frame sizing/destruction",
          "checksPassed": 1127, "newIntegratedChecks": 7, "targetedChecks": 7, "freshPlayModeRun": True, "nativeChecks": 23,
          "freshPlayerBuild": False, "freshPlayerSmoke": False,
          "editorVersion": full["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
          "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints),
          "commands": ["OutgameGuideBookBrowsePlayModeValidation.Run (non-batch, visible Game view)", "BattleBuild.ValidateMechanicsOnly (-batchmode)"],
          "observedBoundaries": ["Batch PlayMode failed the frame-end height check; rendered non-batch Game view passed actual130->330 expansion and all23 native checks.",
                                 "Rendered log includes an unrelated UnityEditor.Search.SearchDatabase startup exception; product checks passed. No Player or full-page visual acceptance claimed."],
          "notClaimed": "Full DynamicList/popup/Spine/page lifecycle, actual localization/voice/toast/effects, Main/account/22 remaining controllers, SDK delivery or full Player"}
write(analysis / "guide-book-browse-validation.json", focused)
for filename in ("unity-integrated-validation.json", "guide-book-browse-native-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-book-browse.log", "guide-book-browse-final.log", "guide-book-browse-native.log", "guide-book-browse-native-rendered.log", "guide-book-browse-integrated.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(outgame / "GUIDE_BOOK_BROWSE_AUDIT.json", {"status": "items-tabs-native-layout-verified-full-page-main-pending", "atUtc": now,
    "sourceEvidence": "GUIDE_BOOK_BROWSE_SOURCE_EVIDENCE.json", "verification": record,
    "implementation": ["OutgameGuideBookItem.cs", "OutgameTipBookItem.cs", "OutgameGuideBookBrowseBinding.cs", "OutgameTabButton.cs", "OutgameTabButtonGroup.cs", "OutgameUiPointerClick.cs", "OutgameGuideBookRewardBinding.cs"],
    "sourceCorrection": "Previous UI evidence selected type4370 ItemInfoUI for the iterator; corrected to actual nested4368. Existing reward binding now sends source GF_UIButtonClick after successful callback return.",
    "checks": focused["checks"], "nativeChecks": native["checks"],
    "remaining": ["Complete original popup33041/additional hints33050 and DynamicList/ListData behavior; actual YD_0 Spine and localization/audio/effects services.", "Full GuideBookUI/BaseUI disposal and actual Main/account production ownership; full goal still includes all other business flows and Player acceptance."]})
next_priority = "Finish GuideBookUI popup33041/hints33050, DynamicList/ListData recycling and original Spine binding; compose complete BaseUI page with real services and Main. Continue full Main/account/22 remaining controllers and all business flows, then Player and original visual/audio/timing acceptance."
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, guideBookBrowseAudit="generated/outgame/GUIDE_BOOK_BROWSE_AUDIT.json")
state["validation"].update(integratedChecksPassed=1127, guideBookBrowse=record)
state["milestones"].append({"id": milestone, "atUtc": now, "status": "verified-full-page-main-composition-pending", "integratedChecks": 1127, "newChecks": 7, "nativeChecks": 23, "remainingLifecycleControllers": 22})
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {f["path"]: f for f in fingerprints}
ordered = [by_path.pop(f["path"]) for f in manifest["sourceFingerprints"] if f["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1127, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Original GuideBookItem/TipBookItem rendering and clicks, tab ownership, native frame-end layout and item destruction; corrected global reward click notification")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps({"integrated": 1127, "native": 23, "fingerprints": len(fingerprints), "controllers": 16, "remaining": 22}))
