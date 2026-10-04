"""Record DynamicList scroll/pool work only after final isolated and native verification."""
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
out = target / "generated/outgame"
def read(path):
    return json.loads(path.read_text())
def write(path, value):
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    path.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
native = read(validation / "analysis/dynamic-list-native-validation.json")
assert full["passed"] and len(full["checks"]) == 1140
assert native["passed"] and len(native["checks"]) == 15
assert "AREABATTLE_INTEGRATED_PASS cases=1140" in (validation / "dynamic-list-integrated-final.log").read_text()
assert "dynamic-list-native-validation.json" not in native.get("error", "") and not native.get("error")
ids = re.findall(r'check\("([^"]+)"', (workspace / "UnityProject/Assets/AreaBattle/Editor/OutgameDynamicListValidation.cs").read_text())
focused = dict(full, checks=[r for r in full["checks"] if r["id"] in ids], limitations="Seven cases extracted from final1140 full run; native15 separate. Source scroll/recycle path only, centering/tweens, full GuideBookUI/BaseUI/Main and actual external services still pending.")
assert len(ids) == len(focused["checks"]) == 7
milestone = "dynamic-list-scroll-pool"
state = read(target / "OUTGAME_RESTORE_STATE.json")
assert not any(r.get("id") == milestone for r in state["milestones"])
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or any(folder in p.as_posix() for folder in ("/Recovered/GuideBook/", "/Recovered/GuideBookSpine/", "/FirstPack/", "/Recovered/Loading/", "/Recovered/Audio/")))]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for p in files:
    assert p.read_bytes() == (validation / p.relative_to(workspace)).read_bytes(), str(p)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
captures = []
for name in ("top", "scrolled", "reopened"):
    filename = "dynamic-list-native-" + name + ".png"
    data = (validation / "analysis" / filename).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n"
    captures.append(dict(path="analysis/" + filename, sha256=hashlib.sha256(data).hexdigest()))
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
remaining = "Centering/tween and selection-specific list paths; complete GuideBookUI/BaseUI resource/open/close ownership with actual services; Main/account/remaining22 controllers/all business flows; Player and original visual/audio/timing acceptance."
record = dict(atUtc=now, scope="Original DynamicList provider/layout/LateUpdate/scroll/recycle and GuideBook row binding",
    checksPassed=1140, newIntegratedChecks=7, targetedChecks=7, freshPlayModeRun=True, nativeChecks=15,
    freshPlayerBuild=False, freshPlayerSmoke=False, editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints), captures=captures,
    commands=["OutgameDynamicListValidation.Validate", "BattleBuild.ValidateMechanicsOnly", "OutgameDynamicListPlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=["Sandboxed editor could not connect to local license service; validation ran with approved native access on the isolated copy.",
        "First native capacity assertion incorrectly assumed reserve slots must be fewer than records; corrected to original capacity and viewport behavior.",
        "Initial750-wide canvas cropped original835-wide rows; final fixture uses1080x1920 reference canvas with750x1334 capture. This is not original-frame visual acceptance.",
        "Final native log contains UnityEditor.Search startup ArgumentOutOfRangeException, separate from15 passing product checks.",
        "Source33208/33213 stores provider and calls GetData; corrected previous row adapter to observe public data-list replacement."],
    notClaimed=remaining)
write(analysis / "dynamic-list-validation.json", focused)
for filename in ("unity-integrated-validation.json", "dynamic-list-native-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("dynamic-list.log", "dynamic-list-verified.log", "dynamic-list-native.log", "dynamic-list-native-final.log", "dynamic-list-native-verified.log", "dynamic-list-native-final-source.log", "dynamic-list-integrated.log", "dynamic-list-integrated-final.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(out / "DYNAMIC_LIST_AUDIT.json", dict(status="scroll-provider-pool-verified-full-page-pending", atUtc=now,
    sourceEvidence="DYNAMIC_LIST_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameDynamicList.cs", "OutgameGuideBookListBinding.cs", "OutgameGuideBookItem.cs"], checks=focused["checks"], nativeChecks=native["checks"], remaining=remaining))
next_priority = "Finish source DynamicList centering/tween paths and compose complete GuideBookUI/BaseUI lifecycle with actual services. Continue Main/account/22 remaining controllers and all business flows, then Player/original audiovisual acceptance."
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, dynamicListAudit="generated/outgame/DYNAMIC_LIST_AUDIT.json")
state["validation"].update(integratedChecksPassed=1140, dynamicList=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="verified-full-page-main-composition-pending", integratedChecks=1140, newChecks=7, nativeChecks=15, remainingLifecycleControllers=22))
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {r["path"]: r for r in fingerprints}
ordered = [by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1140, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Original DynamicList provider/layout/scroll/pool and GuideBook real row callbacks; final1140 full plus15 native checks, three rendered captures")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1140, native=15, fingerprints=len(fingerprints), sourceMethods=len(read(out / "method-map.json")), controllers=16, remaining=22)))
