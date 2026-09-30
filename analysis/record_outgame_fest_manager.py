from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==581
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Fest activity reward manager and storage lifecycle','checksPassed':581,'outgameChecksPassed':314,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=581;write(p,s)
import hashlib
source=t/'generated/tables/SummerRewardConfig.json';asset=w/'UnityProject/Assets/AreaBattle/Resources/Data/Outgame/SummerRewardConfig.json';assert source.read_bytes()==asset.read_bytes()
audit={'status':'source_reward_manager_storage_verified_production_registration_pending','source':['disassembly/Type4505-34412.txt','disassembly/Type4505-34414.txt','disassembly/Type4505-34415.txt','disassembly/Type4505-34419.txt','fest-rewards-generics.json'],'tableSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'verified':['Original SummerRewardConfig row1 length3 and row2 length2','Manager local/server routing, original SummerData load/save and special flags','Eligibility reward index bounds and strictly now>nextGetAwardTime','Grant original flags first, next midnight second, report third, reward index increment last; grant return ignored','Source grant-triggered save before reward progress and subsequent explicit save/reload verified with storage backend'],'remaining':['Production manager pool registration and account lifecycle','Real ToolControl/reward UI and report service binding','Controller OnLateInit and full Player E2E'],'checks':581,'freshPlayMode':False}
write(o/'FEST_MANAGER_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 节日领奖管理器与原始存储（581项）\nOutgameFestActManager接入现有DataManagerStorage，恢复配置奖励数量、独立领取资格、原始发奖参数、次日午夜、上报和奖励编号递增顺序，以及特殊关卡位标记。SummerRewardConfig原样导入。验证发奖调用保存早于编号递增，后续显式保存/重载保留进度；581项集成通过。生产注册、真实ToolControl/UI/上报、控制器OnLateInit和完整Player流程仍待完成，无新PlayMode或Player。\n',encoding='utf8')
print('Recorded fest reward manager; production composition remains incomplete.')
