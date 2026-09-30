from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==573
v=read(w/'analysis/outgame-menu-view-validation.json');assert v['passed']
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Concrete shop menu data refresh binding','checksPassed':573,'outgameChecksPassed':306,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Production scene startup, complete main UI lifecycle or authenticated account'};write(p,s)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='production-entry-composition';s['validation']['integratedChecksPassed']=573;write(p,s)
audit={'status':'shop_visibility_to_native_cards_connected_production_owner_pending','source':['disassembly/Type4408-33798.txt','disassembly/Type4408-33793.txt','shop-refresh-generics.json'],'verified':['Shop refresh calls InitFirstAdsItem(0) before any card refresh','Soldier cards refreshed by category groups then scene cards','OutgameShopMenuBinding connects actual MenuView lifecycle to bound native cards','Initial hide, show and later hide refresh current soldier/scene new flags; Dispose removes owned subscription'],'remaining':['Concrete first-ad reference owner; this validation uses explicit reset callback fixture','Main and commander data refresh composition','Production startup/account integration and Player E2E'],'checks':573,'freshPlayMode':False}
write(o/'SHOP_MENU_REFRESH_AUDIT.json',audit)
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8')+'\n\n## 商店菜单数据刷新接线（573项）\nOutgameShopMenuBinding把实际菜单显示/隐藏事件接入已绑定的OutgameShopSkinLists；RefreshSkinStatus按33798先调用InitFirstAdsItem(0)，再逐组刷新兵种卡片，最后刷新场景卡片。实际导入菜单与卡片在初次隐藏、显示、再次隐藏时反映数据isNew变化，释放绑定后不再触发。573项集成通过；首次广告记录清空仍由显式回调宿主提供，验证使用fixture；生产启动、主页/指挥官数据连接与Player端到端仍未完成，本次无新PlayMode或Player。\n',encoding='utf8')
print('Recorded shop menu data refresh; production composition remains incomplete.')
