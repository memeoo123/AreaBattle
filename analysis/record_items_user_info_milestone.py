"""Record this batch only after a matching isolated Unity validation succeeds."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import shutil

parser = argparse.ArgumentParser()
parser.add_argument("validation_root", type=Path)
args = parser.parse_args()
workspace = Path(__file__).resolve().parent.parent
validation = args.validation_root.resolve()
analysis = workspace / "analysis"
target = analysis / "targets/wxcf1394487200e48f/43"
outgame = target / "generated/outgame"

def read(path):
    return json.loads(path.read_text(encoding="utf8"))

def write(path, value):
    crlf = path.exists() and b"\r\n" in path.read_bytes()
    text = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    path.write_bytes(text.replace("\n", "\r\n").encode() if crlf else text.encode())

report = read(validation / "analysis/unity-integrated-validation.json")
native = read(validation / "analysis/item-runtime-native-validation.json")
assert report["passed"] and len(report["checks"]) == 1098
assert native["passed"] and len(native["checks"]) == 16
assert "AREABATTLE_INTEGRATED_PASS cases=1098" in (validation / "items-user-info-integrated.log").read_text()
files = [p for p in (workspace / "UnityProject/Assets/AreaBattle").rglob("*") if p.is_file() and (
    p.suffix.lower() in (".cs", ".shader", ".json", ".prefab", ".mat", ".unity", ".txt", ".bytes", ".ttf", ".otf", ".spriteatlas", ".anim", ".controller")
    or "/FirstPack/" in p.as_posix() or "/Recovered/Loading/" in p.as_posix() or "/Recovered/Audio/" in p.as_posix())]
files.append(workspace / "UnityProject/ProjectSettings/TagManager.asset")
for path in files:
    assert path.read_bytes() == (validation / path.relative_to(workspace)).read_bytes(), str(path)
fingerprints = [{"path": p.relative_to(workspace).as_posix(), "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
now = datetime.datetime.now(datetime.timezone.utc).isoformat()

for filename in ("unity-integrated-validation.json", "product-services-validation.json", "item-runtime-native-validation.json"):
    shutil.copy2(validation / "analysis" / filename, analysis / filename)
for filename in ("product-services.log", "product-services-rerun.log", "item-runtime-native.log", "product-services-integrated.log",
                 "product-services-integrated-graphics.log", "items-user-info-integrated.log", "user-info.log"):
    shutil.copy2(validation / filename, analysis / filename)
user_checks = [check for check in report["checks"] if check["id"].startswith("user-info-")]
assert len(user_checks) == 7 and all(check["result"] == "pass" for check in user_checks)
write(analysis / "user-info-validation.json", {"passed": True, "unityVersion": report["unityVersion"],
    "provenance": "Extracted from the matching full integrated report, not the earlier standalone failed fixture run.", "checks": user_checks})

validation_record = {
    "atUtc": now, "scope": "Production item/product service group and UserInfo manager/controller with real file persistence",
    "checksPassed": 1098, "newIntegratedChecks": 11, "nativePlayModeChecks": 16,
    "nativePlayModeScope": "ItemRuntime native config bundle, native UpdateManager, daily product reset, file save/restart and delayed download callback",
    "freshPlayModeRun": True, "freshPlayerBuild": False, "freshPlayerSmoke": False,
    "editorVersion": report["unityVersion"], "platform": "OSXEditor", "projectVersionPreserved": "6000.0.68f1",
    "isolatedProject": str(validation / "UnityProject"), "matchingSourceFingerprints": len(fingerprints),
    "commands": ["OutgameProductServicesValidation.Validate", "OutgameItemRuntimePlayModeValidation.Run", "BattleBuild.ValidateMechanicsOnly"],
    "notClaimed": "Complete Main/account/menu/23 remaining controllers, original RandAIInfo, server/SDK/report delivery, native powf parity, original-device parity or full Player"
}
product_audit = {
    "atUtc": now, "status": "item-product-service-group-verified-full-main-composition-pending", "verification": validation_record,
    "implementation": ["OutgameProductConfigProvider.cs", "OutgameProductServices.cs", "OutgameItemRuntime.cs"],
    "source": ["PRODUCT_PRICE_AUDIT.json", "product-config-fields.json", "product-reset-generics.json", "global-item-update-generics.json", "data-manager-registration-roster.json"],
    "findings": [
        "Shared price provider retains live original rows and write-through buyTypeOrder default; nested parameter access stays inside the source catch.",
        "GlobalItemSlot creates actual product update/reset/pricing composition; actual ItemManager.Update host and native UpdateManager own ticking/removal.",
        "ItemManager4500 registration preserves Proj_hdzd/autoSyn=true/compressData=false; entity factories/package manager/controller resolve actual shared owners.",
        "Native legacy AssetBundle config loading, strict tick threshold, day reset, error continuation, message order, disk save/restart, login gate and server callback wait verified.",
        "Original project JSON lacks some shared price fields; source errors are retained instead of synthesizing prices."
    ],
    "remaining": ["Main/account/resource acquisition/report/virtual-item owners still need full application composition.",
                  "Unity Mathf.Pow is used by the runtime; original WASM powf boundary equivalence and source RNG sequence remain unverified."]
}
write(outgame / "ITEM_PRODUCT_SERVICES_AUDIT.json", product_audit)
write(outgame / "USER_INFO_AUDIT.json", {
    "atUtc": now, "status": "manager-controller-and-avatar-entity-persistence-verified-main-name-provider-pending",
    "sourceEvidence": "USER_INFO_SOURCE_EVIDENCE.json", "verification": validation_record, "checks": user_checks,
    "implementation": ["OutgameUserInfoManager.cs", "OutgameUserInfoControl.cs", "OutgameCoreControllerBindings.cs"],
    "remaining": ["Main/account model registration and actual name generator32592, timestamp/AppInfo and role-report owners remain to compose.",
                  "New record field defaults are source-derived; name provider in verification is explicit fixture, not a production random-name implementation."]})

matrix_path = outgame / "CONTROLLER_LIFECYCLE_MATRIX.json"
matrix = read(matrix_path)
matrix["implementedLifecycleControllers"] = 15
for controller in matrix["controllers"]:
    if controller["typeIndex"] == 4228:
        for method in controller["lifecycle"]:
            method["implementationStatus"] = "implemented-in-core-controller-bindings"
            method["implementation"] = "OutgameUserInfoControl.cs"
write(matrix_path, matrix)

next_priority = "Compose production Main/account/data-pool factories using actual ItemRuntime and UserInfo manager/control. Recover ConfigHelper.RandAIInfo32592 and remaining CommanderUI/Dice/report owners, then remaining23 lifecycle controllers and all menu/business/battle-return flows. Native full Player and original visual/audio/timing acceptance remain required."
state_path = target / "OUTGAME_RESTORE_STATE.json"
state = read(state_path)
state.update(status="in_progress", lastUpdatedAtUtc=now, currentStage="items-user-info-production-services", nextPriority=next_priority,
             itemProductServicesAudit="generated/outgame/ITEM_PRODUCT_SERVICES_AUDIT.json", userInfoAudit="generated/outgame/USER_INFO_AUDIT.json")
state["validation"].update(integratedChecksPassed=1098, itemProductServices=validation_record, userInfo=validation_record)
for milestone, checks in (("item-product-service-composition", 4), ("user-info-manager-control", 7)):
    assert not any(row.get("id") == milestone for row in state["milestones"])
    state["milestones"].append({"id": milestone, "atUtc": now, "status": "verified-full-main-composition-pending",
                                "integratedChecks": 1098, "newChecks": checks, "remainingLifecycleControllers": 23})
for subsystem in state["subsystems"]:
    if subsystem["id"] == "currency-items-packages":
        subsystem["implementation"] = "ItemRuntime binds shared config, actual product update/reset/price, native update manager, item/entity factories and model registration; full Main composition pending."
        subsystem["validation"] = "1098 integrated checks across project; 4 new product integration cases and 16 native service-group checks. Full outgame runtime remains incomplete."
write(state_path, state)

manifest_path = analysis / "VALIDATION_MANIFEST.json"
manifest = read(manifest_path)
# Keep the existing file order across Windows and POSIX Path sorting differences.
by_path = {row["path"]: row for row in fingerprints}
ordered = [by_path.pop(row["path"]) for row in manifest["sourceFingerprints"] if row["path"] in by_path]
fingerprints = ordered + [by_path[key] for key in sorted(by_path)]
manifest.update(atUtc=now, passed=True, caseCount=1098, latestValidation=validation_record, sourceFingerprints=fingerprints)
manifest["validationHistory"].append(validation_record)
write(manifest_path, manifest)
write(analysis / "RESTORATION_EXECUTION_STATE.json", {
    "status": "active", "resumedByUserAt": "2026-10-03", "updatedAtUtc": now,
    "objective": "Complete source-grounded version43 outgame restoration, integrate existing battle, deliver runnable build and explicit acceptance report.",
    "constraints": ["No subagents", "Preserve user data and battle", "No invented source rules/platform success", "Partial checks do not establish full acceptance"],
    "completedThisRun": ["Actual item/product service composition", "Native item file-save/restart and update loop", "UserInfo manager/controller and avatar-item persistence"],
    "verification": validation_record, "nextPriority": next_priority,
    "remainingWorkPackages": ["Main/account/23 controllers", "Commander/skins/shop/payment", "Normal/special/daily modes and arena/ranks", "Tasks/achievements/seven-day/seasonal activities", "Dice/cards/guidebook/notices/settings", "Platform callbacks/offline/day refresh", "Battle rewards/return and full save/restart", "Player build/device visual-audio-timing acceptance"]
})
print(json.dumps({"integrated": 1098, "native": 16, "fingerprints": len(fingerprints), "controllers": 15, "remaining": 23}))
