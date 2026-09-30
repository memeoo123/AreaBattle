"""Initialize resumable scope for the explicitly requested out-of-battle restoration."""
import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';g=t/'generated';o=g/'outgame';o.mkdir(exist_ok=True)
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat()
groups={
'entry-navigation':['Proj_xqzdStartUI','MenuTabUI','MainTabItemUI','TopInfoUI','UnknownTabUI','ProcessControl'],
'profile-save-login':['LocalDataManager','LocalData','LocalDataControl','DataManagerSave','LoginCtrl','UserInfoManager'],
'commander-progression':['CommanderManager','CommanderData','CommanderControl','CommanderUI','CommanderItem','CommanderSkillItem'],
'currency-items-packages':['ItemManager','ItemModuleControl','ToolControl','GlobalItemManager','PackageItemBase','PackageItem_OpenAuto','PackageItem_OpenManual'],
'store-payment':['StorePurchaseUI','StoreDataManager','ProductUserData','IapControl','FirstChargeData','ItemGetTypeBuyCount'],
'skins-cosmetics':['ShopUI','SkinManager','SkinData','PlayerSkinData','SceneSkinItem','SkinRewardUI'],
'normal-special-daily-entry':['LevelControl','LevelData','DailyChallengeActivity','DailyChallengeLevelPool','DailyChallengeUI'],
'arena-match-ranks':['ArenaControl','ArenaManager','ArenaUI','MatchControl','MatchManager','RankManager','RankControl','LevelRankControl'],
'daily-tasks-achievements':['TaskMgr','TaskPanelUI','TaskData','AchievementMgr','AchievementData','NoviceTaskManager'],
'seven-day-activity':['SevendayActivityControl','ActivityManager','ActivityControl','ChildTaskActivity','LimitTimeTaskActivity'],
'limited-offers-seasonal':['LimitedBagManager','DiamondLimitedControl','PiggyBankControl','FestActManager','ValentineUI','RaceActivityControl'],
'dice-select-card':['DiceGameControl','DiceShopControl','SelectCardControl','HczzqControl'],
'guidebook-notices-settings':['GuideBookControl','GuideBookUI','NoticeUI','Proj_xqzdStartSetupUI','AudioControl'],
'ads-platform-callbacks':['ADHelper','LoginSDK_XYX','IapControl','SkipAdUI','GiveGiftUI'],
'clock-refresh-offline':['ServerTimeModule','TimeToRefreshControl','DailyRefresher','WeeklyRefresher','ConstTimeRefresher'],
'rewards-battle-return':['RewardUI','CommonGetRewardUI','AwardGetUI','Proj_xqzdOverUI','Proj_xqzdFailUI','GameStatisticsManager']}
methods=read(g/'all-managed-method-map.json');catalog=[]
for k,names in groups.items():
 a=[m for m in methods if m['image']=='Assembly-CSharp.dll' and m['cls'] in names]
 catalog.append({'id':k,'classes':names,'sourceMethodCount':len(a),'reachability':'candidate; verify active config, callers and runtime entry','evidenceStatus':'in_progress' if k in ['entry-navigation','commander-progression'] else 'pending','implementation':'pending','validation':'pending'})
state={'schemaVersion':'1.0','target':'wxcf1394487200e48f/43','objective':'关外逻辑完全复原','status':'in_progress','declaredClaim':'incomplete','startedAtUtc':now,'lastUpdatedAtUtc':now,'currentStage':'source-scope-and-commander-contract','previousBattlefieldWorkPreserved':True,'subsystems':catalog,'acceptance':['All version-enabled out-of-battle entry points mapped to source and implemented or excluded with evidence.','Source-derived rules and config amounts; no invented initial balances, time rules or rewards.','Transactions, callback outcomes, save/reload, reset/date-boundary and battle-return cases verified.','Operable menus plus new Windows build and end-to-end player smoke.','Original services/platform-only outcomes must be explicitly unverified until evidenced; offline mocks do not establish server parity.'],'artifacts':{'methodMap':'generated/outgame/method-map.json','spec':'generated/outgame/OUTGAME_RESTORE_SPEC.json','goldenCases':'generated/outgame/golden-cases.json'},'nextActions':['Recover commander level/cost/skill rotation source contract.','Recover authoritative first-run profile and item mutation/save events.','Trace enabled menu buttons and navigation prerequisites.'],'unknowns':['Original fresh-profile defaults and migration formats','Source active A/B tables and remote flags','Payment/ad completion vs cancellation and persistence ordering','Time source, reset boundaries and activity enablement','Reachability of packaged shared modules']}
p=t/'OUTGAME_RESTORE_STATE.json'
if not p.exists():write(p,state)
else:state=read(p)
menu=read(g/'tables/MenuTabConfig.json')['Datas']
write(o/'scope-inventory.json',{'atUtc':now,'subsystems':catalog,'menuTable':menu,'menuFinding':'MenuTab4 HerosDetailUI isActive=false; do not expose simply because class/assets exist. Other commander entry reachability remains to inspect.','baseline':'BattleLoadout is a local held-state profile; acquisition, economy, ads and account sync are excluded by its existing contract.','sourceExtractedFunctions':len(read(o/'method-map.json')),'sourceTableCount':len(list((g/'tables').glob('*.json')))})
spec={'schemaVersion':'1.0','target':'wxcf1394487200e48f/43','implementationReady':False,'scope':'out-of-battle; per-subsystem gates below','subsystemGates':{},'unknowns':state['unknowns'],'source':'outgame/method-map.json'}
p=o/'OUTGAME_RESTORE_SPEC.json'
if not p.exists():write(p,spec)
p=o/'golden-cases.json'
if not p.exists():write(p,{'schemaVersion':'1.0','target':'wxcf1394487200e48f/43','cases':[]})
p=w/'ORCHESTRATION_STATE.json';s=read(p);s['currentWorkstream']={'id':'outgame','objective':'关外逻辑完全复原','statePath':str(t/'OUTGAME_RESTORE_STATE.json'),'atUtc':now};write(p,s)
print(json.dumps({'subsystems':len(catalog),'sourceMethodsCandidate':sum(x['sourceMethodCount'] for x in catalog),'state':str(t/'OUTGAME_RESTORE_STATE.json')},ensure_ascii=False))
