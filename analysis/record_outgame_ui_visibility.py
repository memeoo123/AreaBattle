from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==571
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Concrete Unity UI visibility and menu transition correction','checksPassed':571,'outgameChecksPassed':304,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=571;write(p,s)
audit={'status':'concrete_visibility_integrated_full_page_lifecycle_pending','source':['disassembly/Type3559-27434.txt','disassembly/Type3559-27435.txt','disassembly/Type3544-27322.txt','disassembly/Type3594-27678.txt','disassembly/MenuPageGeneric-32617.txt'],'verified':['Before and virtual implementation run before logical visible flag changes; GF_VisibleUI is sent afterward','Repeated SetVisible calls retain callbacks and messages','Hidden GameObjects remain active with zero localScale, visible objects receive unit localScale','Actual imported menu page transitions now use recovered visibility instead of disabling page objects','Unity-destroyed object is skipped but logical state/message still update'],'remaining':['Full BaseUI resource load and page-specific VisibleImp handlers','Concrete menu creation/visibility host and production startup','Fresh PlayMode lifecycle/coroutine validation and Player E2E'],'checks':571,'freshPlayMode':False}
write(o/'UI_VISIBILITY_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 原版UI缩放隐藏修正（571项）\n恢复UIObject27434/27435、BaseUI27322与ExtensionMethods27678：先VisibleBefore再VisibleImp，之后提交可见标志并发送GF_VisibleUI；重复调用不短路。隐藏对象保持active且localScale为zero，显示恢复one。实际OutgameMenuView切页已替换错误的SetActive隐藏。571项集成通过（含真实导入页面和销毁对象处理），未新跑PlayMode或Player。完整BaseUI加载、页面专属刷新、异步页面宿主与生产入口仍未完成。此前MENU_ITEMS审计关闭源码路径误写为函数号9802，已更正为metadata32716。\n',encoding='utf8')
print('Recorded source UI visibility; production composition remains incomplete.')
