"""Source UI entry evidence and integrity checks for the local result adapter."""
import json,hashlib
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';G=R/'generated'
s=Path('analysis/presentation_disassemble.py').read_text(encoding='utf8')
s=s.replace("classes={'Soldier','SpineAnimator','TowerCanvas','TowerUpgradeConfig','ShipConfig','ColorHelper','MPBExtension'}","classes={'Proj_xqzdFailUI','Proj_xqzdPlayUI','Proj_xqzdOverUI'}")
s=s.replace('generated/presentation-binding-evidence.json','generated/result-ui-evidence.json')
exec(compile(s,'result_ui_static','exec'))
p=G/'result-ui-evidence.json';e=json.loads(p.read_text(encoding='utf8'))
e['rules']=[{'id':'separate-result-panels','status':'confirmed','claim':'Victory uses Proj_xqzdOverUI; failure uses Proj_xqzdFailUI. OverUI.OpenLater emits WarWin and increments the ordinary current level. Failure UI is not an OverUI state.','functions':[18080,18092]}, {'id':'failure-retry','status':'confirmed','claim':'FailUI OpenLater wires btn_again to callback18091; callback reaches helper7407 which invokes LevelControl.SetPlaySate(10).','functions':[18092,18091,7407]}, {'id':'victory-continuation','status':'confirmed-with-local-adapter','claim':'Original victory buttons collect rewards before continuation; OverUI.CloseSelf invokes SetPlaySate(11). Local restored VictoryUI retains the original go_common/btn_normalGold2 graphic and RectTransform, removes coin icon, changes caption to 下一关 and emits NextRequested. No reward grant is claimed or performed. Parent explicitly authorized this adapter.','functions':[18073,18074,18083]}, {'id':'play-top-bar','status':'confirmed-static','claim':'TopRoot hierarchy, PauseBtn, LevelBg/LevelText are recovered. PlayUI level label helper11010 uses startUI/level translation and current level. Original speed purchase and ad skip branches remain excluded. Camp/boss progress rules are not guessed.','functions':[11010,18059,18060,18054]}]
e['implementation']={'importer':'UnityProject/Assets/AreaBattle/Editor/RecoveredHudImporter.cs','runtime':'UnityProject/Assets/AreaBattle/Scripts/BattleHud.cs','sourceManifest':'generated/presentation-prepared/hud-import.json','retainedSlices':{'PlayTopBar':42,'VictoryUI':11,'DefeatUI':13},'omitted':['advertisements','ranking panel','reward multipliers','lobby navigation','reward economy'],'adapters':['Victory caption 下一关 / special-mode caption 重新挑战 with no currency grant','Camp and boss progress left inactive until dynamic source rule is audited']}
e['unknowns']=[{'id':'result-entrance-animation','detail':'Original win/fail UI animation clips are retained as evidence; UGUI adapter currently shows source static layout without replaying those clips.'},{'id':'topbar-progress-consumer','detail':'Camp widths, sorting and boss meter consumer not yet traced; inactive source branches remain inactive.'},{'id':'special-header','detail':'Current local adapter uses source 挑战关 label without reconstructing full special-mode difficulty suffix.'}]
p.write_text(json.dumps(e,ensure_ascii=False,indent=2),encoding='utf8')
snap=G/'resource-snapshots/result-ui-20260928';a=json.loads((snap/'acquisition-manifest.json').read_text(encoding='utf8'));flat=json.loads((snap/'prefabs/proj_xqzdfailui.flat.json').read_text(encoding='utf8'))
assert not flat['unresolvedReferences'] and not flat['partialComponents']
assert hashlib.sha256((R/a['baselineAssetEvidence']['path']).read_bytes()).hexdigest()==a['baselineAssetEvidence']['sha256']
for item in a['items']:
 b=(R/item['path']).read_bytes();assert len(b)==item['size'] and hashlib.md5(b).hexdigest()==item['md5'] and hashlib.sha256(b).hexdigest()==item['sha256']
files=[{'path':str(p.relative_to(R)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size} for p in snap.rglob('*') if p.is_file() and p.name!='validation.json']
(snap/'validation.json').write_text(json.dumps({'passed':True,'baselineUnchanged':True,'nodes':len(flat['nodes']),'bindings':len(flat['outletBindings']),'unresolved':0,'files':files},ensure_ascii=False,indent=2),encoding='utf8')
print('Result UI static evidence and snapshot hashes validated:',len(files),'files')
