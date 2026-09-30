from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==568
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Imported original scene owner references and home camera switching','checksPassed':568,'outgameChecksPassed':301,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=568;write(p,s)
audit={'status':'imported_scene_references_connected_production_bootstrap_pending','source':['model-roots.json','disassembly/Type4064-31256.txt'],'verified':['Importer now preserves16 original transform nodes','Original GameCamera46 and CommanderCamera48 settings and ancestor transforms included with existing HomeCamera47','Scene_game28 and camera references serialized in OriginalModelRoots prefab','OutgameModelRoots.CreateHomeScenePresentation operates on imported objects','Existing model categories and backdrops preserved'],'remaining':['Remaining original scene children and production startup composition','Account/menu/data and actual state host ownership','Live rendering, persistence and Player E2E'],'checks':568,'freshPlayMode':False}
write(o/'HOME_SCENE_IMPORTED_OWNER_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 原始场景引用接入（568项）\n扩展prepare_outgame_model_roots.py与OutgameModelRootsImporter，导入原始Scene_game28、GameCamera46、CommanderCamera48及祖先变换，合计16节点；保留HomeCamera47和现有模型/背景引用。全部源文件哈希校验后重新生成OriginalModelRoots预制体，新增CreateHomeScenePresentation连接真实导入对象，568项集成通过，导入/验证Unity退出0。尚未连接生产场景启动，也未重跑PlayMode或Player；其余场景子对象及画面验收仍需完成。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
