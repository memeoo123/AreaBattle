from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==562
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original main start button binding and pre-activation menu composition','checksPassed':562,'outgameChecksPassed':295,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=562;write(p,s)
audit={'status':'main_start_binding_verified_production_entry_pending','source':['disassembly/Type4399-33679.txt','disassembly/Type4399-33681.txt','disassembly/Type4399-33685.txt'],'verified':['Fields228 and164 identify btnStart and btnNewStart','Both handlers request SetPlaySate(3,false), then PlayerVoice(1,2001)','Menu composition callback binds all pages before initial main activation','Imported original start buttons invoke source command order'],'scope':'Native Button binding is reconstruction integration; full UIExtension.AddClick behavior and remaining OpenLater actions are not claimed restored.','remaining':['Production scene startup owner','Original LevelControl.SetPlaySate state3 lifecycle and battle transition','Account/data owner, complete main-page lifecycle, return/persistence/E2E'],'checks':562,'freshPlayMode':False}
write(o/'MAIN_START_BINDING_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 生产入口：主界面开始按钮连接（562项）\n为OutgameMenuView增加页面首次激活前的组合回调；新增OutgameMainStartBinding，使用原始btnStart/btnNewStart，按33685先SetPlaySate(3,false)后音效(1,2001)。实际导入预制体按钮验证与集成共562项通过，Unity退出0。此次未重跑Ads/Fly PlayMode，历史7/8项不得算作新证据。生产入口仍未接通；下一步优先恢复LevelControl.SetPlaySate状态3的执行链并接到现有BattleView，账号数据及主界面其余生命周期不可假定完成。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
