from pathlib import Path
import json,hashlib,datetime
W=Path(__file__).resolve().parent.parent
T=W/'analysis/targets/wxcf1394487200e48f/43';O=T/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.datetime.now(datetime.timezone.utc).isoformat()
r=read(W/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==705
native=read(W/'analysis/ui-native-playmode-validation.json')
assert native['passed'] and len(native['checks'])==6,native
verification={'atUtc':now,'scope':'UIControl lifecycle/state transitions, original TopInfo prefab/part rendering, native MenuTab visibility and actual rendered end-of-frame loading','checksPassed':705,'nativePlayModeChecks':6,'freshPlayModeRun':True,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Full menu data/role/page callbacks, account/avatar/SDK integration, remaining32 controllers, complete Main or Player build','commands':['RecoveredHudImporter.ImportTopInfoBatch','BattleBuild.ValidateMechanicsOnly','OutgameUiPlayModeValidation.RunRendered'],'batchModeLimitation':'Batch-mode UI run timed out at first WaitForEndOfFrame; source wait unchanged, failed report retained in analysis/ui-native-batch-timeout.json'}
methods=read(O/'method-map.json');wanted={32739,32709,32735,32726,32704,32710,33920,33928,33932,33929,33469,33482,32420}
evidence=[{'metadata':m['metadata'],'path':m['path'],'sha256':hashlib.sha256((O/m['path']).read_bytes()).hexdigest()} for m in methods if m['metadata'] in wanted]
audit={'atUtc':now,'status':'partial-subsystem','sourceEvidence':evidence,'originalPrefab':{'source':'top-info-ui-evidence.json','import':'top-info-ui-import.json','nodes':36,'outlets':25,'sprites':11,'fonts':1},'productionFiles':['OutgameUiControl.cs','OutgameTopInfoPage.cs','OutgameMenuTabPage.cs','OutgameCoreControllerBindings.cs'],'findings':['UIControl init reads module three times and resets current registry singleton, not necessarily this instance. Dispose clears registry/top and state listener but retains menu and global roots.','GamePlayState2 shows top/menu,3 closes queried menu and hides top,8 renders top mask6 on popup layer,11 shows top alone. Cached menu branch opens children then sets current3.','TopInfo keys2/4/5 map gold/diamonds/stamina; renderer loop visits only1/2/4. Key5 remains untouched. Part dictionary null returns before visibility.','Part canvas order is9 for any nonempty requested layer even for unselected parts; empty layer uses0. Selected raycasters enabled; unselected disabled.','Menu close requests main, hides all menu children, sets current0 and applies virtual visibility. Native scale change precedes MenuTabDispose; logical flag and GF_VisibleUI follow callbacks.','Menu refresh exceptions preserve the original partial native/logical visibility state; no catch/rollback invented.','Default WaitForEndOfFrame loader completed original main/shop/commander/item-info order in rendered PlayMode with timeScale0.'],'verification':verification,'limits':['TopInfo account/avatar/buttons/value refresh and ellipsis remain incomplete.','Menu data refresh/request-main/role handlers in native test are fixtures. Mandatory production callback dependencies remain to be connected.','No full 38-controller Main, online login, complete playable outgame or new Player build.']}
write(O/'UI_CONTROLLER_NATIVE_AUDIT.json',audit)
a=read(O/'CONTROLLER_LIFECYCLE_AUDIT.json');a['atUtc']=now
if 4296 not in a['implementedControllerTypes']:a['implementedControllerTypes'].append(4296)
a['remainingControllerCount']=32;a['implementations']['4296']='OutgameUiControl.cs';a['uiControllerAudit']='UI_CONTROLLER_NATIVE_AUDIT.json';a['verification']=verification
a['sourceMethods']=list(dict.fromkeys(a['sourceMethods']+sorted(wanted)))
a['remaining'][0]='Restore and bind remaining32 lifecycle controllers, including Game/Level/Player/PVP/Arena/Rank/Dice; connect mandatory UI page/role/data callback dependencies.'
write(O/'CONTROLLER_LIFECYCLE_AUDIT.json',a)
s=read(T/'OUTGAME_RESTORE_STATE.json');s.update(lastUpdatedAtUtc=now,currentStage='ui-controller-native-binding',uiControllerNativeAudit='generated/outgame/UI_CONTROLLER_NATIVE_AUDIT.json',nextPriority='Restore Game31264/Level31503/Player34199 and remaining32 lifecycles. Connect UI menu roles, page data callbacks and TopInfo account/value refresh. Assemble native Main/account/menu; then full Player/persistence/return-flow acceptance. No fabricated login.')
s['milestones'].append({'id':'ui-controller-native-topinfo-menutab','atUtc':now,'status':'partial-subsystem','integratedChecks':705,'nativeChecks':6,'remainingControllers':32})
write(T/'OUTGAME_RESTORE_STATE.json',s)
m=read(W/'analysis/VALIDATION_MANIFEST.json');m.update(atUtc=now,caseCount=705,latestValidation=verification)
m.setdefault('validationHistory',[]).append(verification)
for item in ['analysis/ui-native-playmode-validation.json','analysis/ui-native-batch-timeout.json','analysis/unity-top-info-import-report.json']:
 if item not in m['reports']:m['reports'].append(item)
files=[p for p in (W/'UnityProject/Assets/AreaBattle').rglob('*') if p.is_file() and (p.suffix.lower() in ('.cs','.shader','.json','.prefab','.mat','.unity','.txt','.bytes','.ttf','.otf','.spriteatlas') or '/FirstPack/' in p.as_posix())]
m['sourceFingerprints']=[{'path':p.relative_to(W).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files)]
write(W/'analysis/VALIDATION_MANIFEST.json',m)
p=W/'analysis/unity-top-info-import-report.json';d=read(p);d['limitation']='Original TopInfoUI hierarchy, graphics and source fonts imported. Runtime part rendering uses recovered adapters; account/avatar/WXButton/text ellipsis and full page callbacks remain to be connected. No visual-match or complete lobby claim.';write(p,d)
text='\n\n## 2026-09-30 UIControl / TopInfo / MenuTab 增量\n- UIControl 生命周期、进出关状态处理和共享注册绑定已实现；控制器生命周期实现6/38，仍有32待接，业务完整度另行验收。\n- TopInfo 原预制体36节点/25绑定/11sprites/1font；原生 Canvas 位掩码/层级/射线和 MenuTab 关闭/异常回调顺序通过源证据检查。\n- 集成705项通过；带GameView渲染的独立PlayMode6项通过，实际 WaitForEndOfFrame 按 main/shop/commander/item-info 加载，timeScale0。\n- 批处理PlayMode停在第一个 WaitForEndOfFrame 后超时，保留 ui-native-batch-timeout.json。未替换生产等待行为。\n- 菜单角色/数据回调、TopInfo账号头像按钮与值刷新、Main/account整体装配、剩余32控制器和新Player构建仍未完成。审计 O/UI_CONTROLLER_NATIVE_AUDIT.json。\n'
for p in [T/'REVERSE_PROGRESS.md',T/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md']:
 with p.open('a',encoding='utf8') as f:f.write(text)
h=W/'AREA_BATTLE_HANDOFF.md';txt=h.read_text(encoding='utf8');txt=txt.replace('699 项 checks','705 项 checks').replace('caseCount=699；latestValidation 为 five-controller lifecycles + native binding','caseCount=705；latestValidation 为 UIControl + native TopInfo/MenuTab').replace('最新method-map4477；5控制器生命周期/共享绑定已实现并验证，剩33','最新method-map4497；6控制器生命周期/共享绑定已实现并验证，剩32')
txt+=text+'\n下一步不要重做批处理 WaitForEndOfFrame 检查；Use RunRendered。LevelControl4107 Update31526包含战斗actor循环，不可替换为空；PlayerControl4462 Update较大，与现有battle服务分离边界待核对。GameControl4064 OnInit31264先UseAnimationIns=false再按ConfigMgr.dicSceneSkin.Count创建GameObject数组，Update31254源码空，Dispose31257仅清slot。\n';h.write_text(txt,encoding='utf8')
print(json.dumps({'checks':705,'native':6,'fingerprints':len(files),'methods':len(methods),'remainingControllers':32},ensure_ascii=False))
