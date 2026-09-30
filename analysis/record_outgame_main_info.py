from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==574
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Concrete main page refresh binding','checksPassed':574,'outgameChecksPassed':307,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=574;write(p,s)
audit={'status':'native_main_refresh_connected_source_providers_pending','source':['disassembly/Type4399-33720.txt','disassembly/Type4399-33681.txt','ui-import.json'],'verified':['Main visibility events refresh original level labels and tutorial text','Piggy state0 hides parent without resetting child state;1 normal;2 full','Easter states3/4 show and refresh timer; <=5 other states hide; >5 preserve visibility','LoadStartingUI emitted before start-panel selection; design mode1 repeats ordinary-panel selection','Actual tab return updates original UI nodes; binding dispose removes callback'],'remaining':['Concrete level/account, Piggy/Easter service ownership and timer renderer','Full production bootstrap and page/controller composition','Live platform and Player E2E'],'checks':574,'freshPlayMode':False}
write(o/'MAIN_INFO_REFRESH_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 主页实际节点数据刷新（574项）\nOutgameMainInfoBinding把主页显示事件接到原始关卡双标签、引导文字、存钱罐和复活节入口。按33720保留服务读取顺序、LoadStartingUI通知时机与模式1重复选择普通面板；活动状态大于5保持显隐。实际菜单切页返回验证刷新、释放订阅及原始节点状态，574项集成通过。级别/存钱罐/活动服务与计时文本目前仍以显式provider接入，验证用fixture，尚非完整生产入口；本次无新PlayMode或Player。\n',encoding='utf8')
print('Recorded main page data refresh; production composition remains incomplete.')
