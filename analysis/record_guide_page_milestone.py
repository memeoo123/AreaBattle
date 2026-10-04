"""Record composed GuideBook page and source centering after matching final native/full runs."""
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
def read(p):
    return json.loads(p.read_text())
def write(p, value):
    crlf = p.exists() and b"\r\n" in p.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    p.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode())

full = read(validation / "analysis/unity-integrated-validation.json")
native = read(validation / "analysis/guide-page-native-validation.json")
assert full["passed"] and len(full["checks"]) == 1148
assert native["passed"] and len(native["checks"]) == 26 and not native["error"]
assert "AREABATTLE_INTEGRATED_PASS cases=1148" in (validation / "guide-page-integrated.log").read_text()
assert "OutgameGuideBookPagePlayModeValidation.Run" in (validation / "guide-page-native-verified.log").read_text()
ids = []
for name in ("OutgameGuideBookPageValidation", "OutgameDynamicCenterValidation"):
    ids += re.findall(r'check\("([^"]+)"', (workspace / "UnityProject/Assets/AreaBattle/Editor" / (name + ".cs")).read_text())
focused = dict(full, checks=[r for r in full["checks"] if r["id"] in ids], limitations="Eight source page/centering cases extracted from final1148 full run. Real UI bootstrap/open/close and scaled frame-end26 checks are separate. Fixture acquisition/account/SkillControl/language/atlas/audio/effects are explicit boundaries; full Main/Player/original audiovisual acceptance remains pending.")
assert len(ids) == len(focused["checks"]) == 8
state = read(target / "OUTGAME_RESTORE_STATE.json")
milestone = "guide-book-page-lifecycle"
assert not any(r.get("id") == milestone for r in state["milestones"])
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or any(folder in p.as_posix() for folder in ("/Recovered/GuideBook/", "/Recovered/GuideBookSpine/", "/FirstPack/", "/Recovered/Loading/", "/Recovered/Audio/")))]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for p in files:
    assert p.read_bytes() == (validation / p.relative_to(workspace)).read_bytes(), str(p)
fingerprints = [dict(path=p.relative_to(workspace).as_posix(), sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in sorted(files)]
captures = []
for name in ("open", "tips", "reopened"):
    filename = "guide-page-native-" + name + ".png"
    data = (validation / "analysis" / filename).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n"
    captures.append(dict(path="analysis/" + filename, sha256=hashlib.sha256(data).hexdigest()))
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
remaining = "Complete actual Main/account/data-pool/remaining22 controllers and all business flows; replace fixture acquisition/SkillControl/language/atlas/audio/effects with owning production services; Player build, failure/visual/audio/timing acceptance. Selection-specific list subclasses are outside GuideBook and remain for their business owners."
record = dict(atUtc=now, scope="Composed GuideBook page/BaseUI/UIModule lifecycle and source DynamicList positioning coroutine",
    checksPassed=1148, newIntegratedChecks=8, targetedChecks=8, freshPlayModeRun=True, nativeChecks=26,
    freshPlayerBuild=False, freshPlayerSmoke=False, editorVersion=full["unityVersion"], platform="OSXEditor", projectVersionPreserved="6000.0.68f1",
    isolatedProject=str(validation / "UnityProject"), matchingSourceFingerprints=len(fingerprints), captures=captures,
    commands=["BattleBuild.ValidateMechanicsOnly", "OutgameGuideBookPagePlayModeValidation.Run (non-batch Game view)"],
    observedBoundaries=["Edit-mode initial cases lacked Unity Awake delivery; split recovered component preparation from source page Awake, and explicitly replay component Awake only in edit fixture. Native tests use actual callbacks.",
        "Metadata confirms namespace Proj_hdzd.UI.MainMenu and inherited UIPath MainMenu/GuideBookUI; corrected initial guessed path before final matching verification.",
        "Native validation uses original recovered scene canvas/UIRoot. Asset provider acquires local recovered prefab; it does not establish full remote/production resource routing.",
        "Initial native centering fixture had a centered content pivot, so ScrollRect elastic correction moved the completed target from237.5 to244.48 (velocity16.93). Corrected only the isolated native fixture to top-anchored content; completion now follows the actual coroutine handle and finishes237.5 with zero velocity. Failed diagnostics/logs retained.",
        "Frame-end centering preserves scaled-time pause, original single/pair boundary differences and older-routine completion stopping the latest stored handle.",
        "UnityEditor.Search startup indexing exception remains separately recorded in native logs, not a passing product assertion."], notClaimed=remaining)
write(analysis / "guide-page-validation.json", focused)
for filename in ("unity-integrated-validation.json", "guide-page-native-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("guide-page.log", "guide-page-integrated.log", "guide-page-native.log", "guide-page-native-final.log", "guide-page-native-diagnostic.log", "guide-page-native-verified.log"):
    shutil.copy2(validation / filename, analysis / filename)
write(out / "GUIDE_BOOK_PAGE_AUDIT.json", dict(status="page-lifecycle-centering-verified-main-services-pending", atUtc=now,
    sourceEvidence="GUIDE_BOOK_PAGE_SOURCE_EVIDENCE.json", verification=record,
    implementation=["OutgameGuideBookPage.cs", "OutgameGuideBookListBinding.cs", "OutgameGuideBookBrowseBinding.cs", "OutgameDynamicList.cs"],
    checks=focused["checks"], nativeChecks=native["checks"], remaining=remaining))
next_priority = "Compose actual Main/account/data-pool and remaining22 controller factories with recovered services; connect page owners (including GuideBook) to production resource, SkillControl, localization, atlas, audio/effects modules. Advance startup→lobby→battle→reward→return→restart, all remaining business flows, then Player/original audiovisual acceptance."
state.update(lastUpdatedAtUtc=now, currentStage=milestone, nextPriority=next_priority, guideBookPageAudit="generated/outgame/GUIDE_BOOK_PAGE_AUDIT.json")
state["validation"].update(integratedChecksPassed=1148, guideBookPage=record)
state["milestones"].append(dict(id=milestone, atUtc=now, status="verified-main-services-composition-pending", integratedChecks=1148, newChecks=8, nativeChecks=26, remainingLifecycleControllers=22))
write(target / "OUTGAME_RESTORE_STATE.json", state)
manifest = read(analysis / "VALIDATION_MANIFEST.json")
by_path = {r["path"]: r for r in fingerprints}
ordered = [by_path.pop(r["path"]) for r in manifest["sourceFingerprints"] if r["path"] in by_path]
manifest.update(atUtc=now, passed=True, caseCount=1148, latestValidation=record, sourceFingerprints=ordered + [by_path[p] for p in sorted(by_path)])
manifest["validationHistory"].append(record)
write(analysis / "VALIDATION_MANIFEST.json", manifest)
execution = read(analysis / "RESTORATION_EXECUTION_STATE.json")
execution.update(updatedAtUtc=now, verification=record, nextPriority=next_priority)
execution["completedThisRun"].append("Composed GuideBook page resource/open/registry/async close and source centering;1148 full plus26 native checks with original UI bootstrap and three captures")
write(analysis / "RESTORATION_EXECUTION_STATE.json", execution)
print(json.dumps(dict(integrated=1148, native=26, fingerprints=len(fingerprints), controllers=16, remaining=22)))
