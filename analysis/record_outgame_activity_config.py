from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==580
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original local activity configuration parsing and binding','checksPassed':580,'outgameChecksPassed':313,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=580;write(p,s)
import hashlib
source=t/'generated/tables/PubActivityConfig.json';asset=w/'UnityProject/Assets/AreaBattle/Resources/Data/Outgame/PubActivityConfig.json';assert source.read_bytes()==asset.read_bytes()
audit={'status':'original_local_activity_config_imported_online_selection_pending','source':['disassembly/Type4265-32590.txt','disassembly/Type4265-32593.txt','generated/tables/PubActivityConfig.json'],'tableSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'verified':['All9 local activity rows imported without edits','Launch condition10000 parsed exact yyyyMMddHHmmss;10015 level parsed as integer','First overParams/noticeParams parsed regardless of their condition type; missing row gives zero tuple','Activity103002 is 情人节活动: start2022-02-13 end2022-02-27 notice2023-02-10 unlock6; inconsistency preserved','Configuration binds actual state and countdown timestamps; failed date logs source message'],'remaining':['ActivityConfigMgr online/local strategy selection','Controller OnLateInit, FestActManager storage/reward integration','Production startup and Player E2E'],'checks':580,'freshPlayMode':False}
write(o/'ACTIVITY_CONFIG_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 原始活动配置读取与绑定（580项）\n原样导入PubActivityConfig9行，OutgameActivityConfig恢复ReadActivityConfig32590与严格yyyyMMddHHmmss解析。活动103002原名情人节活动，开始2022-02-13、结束2022-02-27、预告2023-02-10、等级6，保留不一致日期；配置已能设置活动状态和倒计时对象。580项集成通过。当前只恢复本地配置读取，在线/本地策略选择、OnLateInit、活动存储奖励及生产入口仍待完成，无新PlayMode或Player。\n',encoding='utf8')
print('Recorded original activity config; production composition remains incomplete.')
