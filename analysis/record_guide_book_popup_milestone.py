"""Record original popup/Spine integration after matching full and native verification."""
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
native = read(validation / "analysis/guide-book-popup-native-validation.json")
imported = read(validation / "analysis/unity-guide-book-spine-import-report.json")
assert full["passed"] and len(full["checks"]) == 1133
assert native["passed"] and len(native["checks"]) == 14
assert imported["passed"] and imported["prefabCount"] == 1 and imported["resourceCount"] == 13
assert "AREABATTLE_INTEGRATED_PASS cases=1133" in (validation / "guide-book-popup-integrated.log").read_text()
ids = re.findall(r'check\("([^"]+)"', (workspace / "UnityProject/Assets/AreaBattle/Editor/OutgameGuideBookPopupValidation.cs").read_text())
focused = dict(full, checks=[c for c in full["checks"] if c["id"] in ids], limitations="Six popup cases extracted from full1133 run; separate rendered PlayMode14 cases. Local atlas lookup, SkillControl/voice/close test endpoints and isolated camera do not prove full Main or original visual match.")
assert len(ids) == len(focused["checks"]) == 6
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "guide-book-popup-original-spine"
assert not any(m.get("id") == milestone for m in state["milestones"])
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or any(folder in p.as_posix() for folder in ("/Recovered/GuideBook/", "/Recovered/GuideBookSpine/", "/FirstPack/", "/Recovered/Loading/", "/Recovered/Audio/")))]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
captures = []
for name in ("open", "animated", "defense", "skill"):
    filename = "guide-book-popup-native-" + name + ".png"
    data = (validation / "analysis" / filename).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n"
    captures.append({"path": "analysis/" + filename, "sha256": hashlib.sha256(data).hexdigest()})
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
record = {"atUtc": now, "scope": "Original GuideBook popup content/commander mapping/hints/close, own YD_0 Spine subtree and item-to-popup-to-reward state",
          "checksPassed": 1133, "newIntegratedChecks": 6, "targetedChecks": 6, "freshPlayModeRun": True, "nativeChecks": 14,
          "freshPlayerBuild": False, "freshPlayerSmoke": False, "editorVersion": full["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
          "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints), "captures": captures,
          "commands": ["RecoveredGuideSpineImporter.ImportGuideBookBatch", "OutgameGuideBookPopupValidation.Validate", "OutgameGuideBookPopupPlayModeValidation.Run (non-batch Game view)", "BattleBuild.ValidateMechanicsOnly"],
          "observedBoundaries": ["First isolated import lacked copied texture/mesh payloads; corrected dependency copy before successful import.", "First native test checked wall time before Unity frames ran; final test also requires actual frame advancement. Native14 passed and generated four inspected captures.", "Rendered log includes UnityEditor.Search startup exception, separate from product checks."],
          "notClaimed": "Complete DynamicList/ListData, BaseUI/Main/account ownership, actual SkillControl/async atlas/language module/voice/close/effects services, remaining22 controllers, all business flows, SDK delivery or Player/visual-match acceptance"}
write(analysis / "guide-book-popup-validation.json", focused)
for filename in ("unity-integrated-validation.json", "guide-book-popup-native-validation.json", "unity-guide-book-spine-import-report.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-book-popup.log", "guide-book-popup-spine-import.log", "guide-book-popup-spine-import-final.log", "guide-book-popup-native.log", "guide-book-popup-native-final.log", "guide-book-popup-native-verified.log", "guide-book-popup-integrated.log"):
    shutil.copy2(validation / filename, analysis / filename)
shutil.copytree(validation / "analysis/guide-book-spine-roundtrip", analysis / "guide-book-spine-roundtrip", dirs_exist_ok=True)
write(outgame / "GUIDE_BOOK_POPUP_AUDIT.json", {"status": "popup-own-spine-verified-full-page-main-pending", "atUtc": now, "sourceEvidence": "GUIDE_BOOK_POPUP_SOURCE_EVIDENCE.json", "verification": record,
    "implementation": ["OutgameGuideBookPopupBinding.cs", "OutgameGuideBookSprites.cs", "RecoveredGuideSpineImporter.cs"], "checks": focused["checks"], "nativeChecks": native["checks"],
    "resourceImport": imported, "remaining": record["notClaimed"]})
next_priority = "Restore original DynamicList/ListData recycling and compose full GuideBookUI/BaseUI resource/open/close/dispose lifecycle with real services. Continue Main/account/22 remaining controllers and all business flows; then full Player and original visual/audio/timing acceptance."
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, guideBookPopupAudit="generated/outgame/GUIDE_BOOK_POPUP_AUDIT.json")
state["validation"].update(integratedChecksPassed=1133, guideBookPopup=record)
state["milestones"].append({"id": milestone, "atUtc": now, "status": "verified-full-page-main-composition-pending", "integratedChecks": 1133, "newChecks": 6, "nativeChecks": 14, "remainingLifecycleControllers": 22})
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {f["path"]: f for f in fingerprints}
ordered = [by_path.pop(f["path"]) for f in manifest["sourceFingerprints"] if f["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1133, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Original GuideBook popup content/commander/hints/close and native own Spine subtree; actual reward persistence and four rendered inspection captures")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps({"integrated": 1133, "native": 14, "fingerprints": len(fingerprints), "controllers": 16, "remaining": 22}))
