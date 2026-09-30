from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==569
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Menu close/reopen visibility lifecycle','checksPassed':569,'outgameChecksPassed':302,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=569;write(p,s)
audit={'status':'visibility_lifecycle_verified_concrete_host_pending','source':['disassembly/Type4379-33469.txt','disassembly/Type4379-33482.txt'],'verified':['Close invisible menu no-op','Close requests main then closes pages then resets page0 then virtual visibility(false)','VisibleImp calls base first; hide sends MenuTabDispose','Show calls OpenAllMenuItemUI, main, start-panel check, commander lock/red dots, level gate, new skin refresh in source order'],'remaining':['Source async OpenAllMenuItemUI implementation','Concrete menu visibility host and actual production entry','Account/data integration, persistence and Player E2E'],'checks':569,'freshPlayMode':False}
write(o/'MENU_VISIBILITY_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 菜单显示关闭生命周期（569项）\n新增OutgameMenuVisibility还原33469/33482，关闭先请求主页、关闭页面、清页号、通过虚拟可见性路径隐藏；隐藏发送MenuTabDispose，显示依次打开页面、返回主页、检查弹窗并刷新锁/红点/级别门槛/新皮肤。569项集成通过，未新跑PlayMode或Player。OpenAllMenuItemUI原始实现为async仍待恢复，具体宿主与生产入口尚未接通，不可简化为SetActive完成。\n',encoding='utf8')
print('Recorded main-start binding; production composition remains incomplete.')
