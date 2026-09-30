from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==572
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Page-specific main/shop/commander visibility overrides','checksPassed':572,'outgameChecksPassed':305,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=572;write(p,s)
audit={'status':'page_overrides_integrated_refresh_consumers_pending','source':['disassembly/Type4399-33678.txt','disassembly/Type4408-33793.txt','disassembly/Type4307-32828.txt','ui-import.json'],'verified':['Main alone uses base scale visibility and on show hides bound btnPermit then RigthBar/btn_ads before RefreshUIInfo event','Shop uses SetActive and refreshes skin status on both show and hide','Commander uses SetActive, hides skill detail on hide, refreshes selected item then info on show','Actual imported menu routes through page-specific overrides','OutletInfos selects outer ads button despite duplicate name; original Commander scale1.01 is preserved'],'remaining':['Bind actual page data refresh consumers to emitted lifecycle events','Full resource loading and async menu host','Production entry, live platform and Player E2E'],'supersedes':'571 incorrectly generalized base visibility to Shop and Commander; 572 restores their source overrides','checks':572,'freshPlayMode':False}
write(o/'PAGE_VISIBILITY_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 页面专属可见性覆盖（572项）\n已纠正571把基类缩放规则套到所有页面的问题：主页33678缩放隐藏并在显示时关闭btnPermit和RigthBar/btn_ads后请求刷新；商店33793使用SetActive且显示/隐藏都刷新；指挥官32828使用SetActive，隐藏关闭技能详情，显示先刷新选中项后刷新信息。原始OutletInfos证实广告按钮绑定外层同名节点；指挥官原始横向缩放1.01保持不变。572项集成通过。当前刷新通过事件交给页面数据控制器，实际消费者与生产启动尚未接通；未新跑PlayMode或Player。\n',encoding='utf8')
print('Recorded page visibility overrides; production composition remains incomplete.')
