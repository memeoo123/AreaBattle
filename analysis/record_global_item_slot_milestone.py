from pathlib import Path
import json,hashlib,datetime,subprocess
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';O=T/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
report=W/'analysis/unity-integrated-validation.json';r=read(report)
assert r['passed'] and len(r['checks'])==1087
assert 'AREABATTLE_INTEGRATED_PASS cases=1087' in (W/'analysis/global-item-slot-integrated.log').read_text(encoding='utf8')
checks=[x for x in r['checks'] if x['id'].startswith('source-global-slot-')];assert len(checks)==10 and all(x['result']=='pass' for x in checks)
now=datetime.datetime.now(datetime.timezone.utc).isoformat();mm=read(O/'method-map.json')
ids={34570,34577,34586,34587,34588,34590,34550,34554,34333,34334,34335,34336,34337}
methods=[dict(m,sha256=hashlib.sha256((O/m['path']).read_bytes()).hexdigest()) for m in mm if m['metadata'] in ids]
findings=[
'GlobalItemManager.get_Instance34590/f2040 constructs original4542, publishes static singleton, then resolves current ItemConfigMgr and invokes InitMgr. The final return rereads the current static slot, so callbacks may release or replace it. Configuration failure retains published owner and partial tables; a later getter does not retry while the slot is nonnull. Constructor failure never publishes.',
'Constructor34570/f12040 sets statistics id10020 at offset20 and constructs only Products32 and Items36. Snapshot40/44, Data28, rewardHost24, update handle8 and dirty72 are otherwise default. Removed eager ItemSnapshot/ProductSnapshot allocation from OutgameGlobalItemIndexes. InitializeProducts34586 and InitializeItems34587 remain responsible for creating snapshots. Existing snapshot-only fixtures now explicitly initialize them. Pre-init AddRewardsModel now retains live insertion/statistics then throws at null snapshot, matching source failure prefix.',
'OutgameGlobalItemSlot retains the lifecycle/reward pair representing one original owner; constructs through an explicit lifecycle factory, publishes, initializes actual legacy config, and returns current slot. No hidden record initialization or fake update loop. BindEntities and CreateFactory resolve current config/global/message/report dependencies, while full virtual/package/clock/product providers remain owned by their corresponding composition.',
'LifecycleHost composes actual current config.Dispose and owner-slot clear with externally supplied update registration/removal and current MsgDispatcher. Statistics registration sends object[]{int eventId,Func<object[],long> query}; original34588 array length2/slots16,20 verified. Release34577 failure order is remove->clear nullable handle->current config.Dispose->clear global; stale owner release clears replacement slots too.',
'Actual ItemManager.OnInit local callback now verified via lazy owner getter: four original tables load before record filtering; InitializeWithHost sets real manager host; time/update/index initialization precedes registration. Gold factory/model reward->original Int64 persistence->same manager release/re-init and independent manager restart verified. The test storage host explicitly supplies login10 only as a test precondition, not production login. Server download branch leaves config/global slots empty until explicit callback.',
'Ten new checks cover constructor defaults, source getter initialization, recursive access, partial config failure/no retry, constructor failure, null/replacement current return, stale release, release failure prefix, pre-init reward null snapshot, actual manager save/restart and server callback wait. Existing package/local Tool chain suite remains passing. This batch uses controlled update registration callbacks, not a new native UpdateManager/Player run.'
]
limits=[
'Full production Main/account/data-pool registration and product update/reset/price/config provider composition are still pending. The slot accepts an explicit lifecycle constructor; its focused tests do not substitute a production product-update implementation.',
'Full UserInfo/CommanderUI/Dice/report owners, 24 remaining lifecycle controllers and full outgame business/return flows remain incomplete.',
'No new native PlayMode, Player build or device validation. Previous native audio/resource18 remains historical. SDK delivery and original time-seeded RNG replay are not claimed.'
]
remaining='Compose actual GlobalItemManager product update/reset/price providers with shared config and original legacy resource reader; bind the restored lazy global slot, ItemManager, entity services and data-pool registration into production startup. Then full UserInfo/CommanderUI/Dice/report owners,24 controllers, Main/account/menu/business and full outgame Player. Do not redo getter34590 or eager-allocate snapshots.'
v=dict(atUtc=now,scope='GlobalItemManager singleton ownership, lazy legacy configuration, source constructor snapshots and actual ItemManager record restart chain',checksPassed=1087,newIntegratedChecks=10,nativePlayModeChecks=0,freshPlayModeRun=False,freshPlayerBuild=False,freshPlayerSmoke=False,commands=['BattleBuild.ValidateMechanicsOnly'],notClaimed='Full production Main/product-service composition, native update loop, login/SDK or complete outgame Player')
write(O/'GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json',dict(atUtc=now,methods=methods,criticalEvidence=findings,limits=limits))
a=dict(atUtc=now,status='singleton-owner-and-record-startup-verified-full-production-composition-pending',sourceEvidence='GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json',verification=v,checks=checks,implementedLifecycleControllers=14,remainingLifecycleControllers=24,findings=findings,remaining=[remaining]+limits,disassembledMethodMapCount=len(mm),artifacts=dict(integratedLog='analysis/global-item-slot-integrated.log'))
write(O/'GLOBAL_ITEM_SLOT_AUDIT.json',a)
p=T/'OUTGAME_RESTORE_STATE.json';d=read(p);assert not any(x.get('id')=='global-item-slot-owner' for x in d['milestones'])
d.update(lastUpdatedAtUtc=now,currentStage='global-item-slot-owner',globalItemSlotAudit='generated/outgame/GLOBAL_ITEM_SLOT_AUDIT.json',nextPriority=remaining);d['validation']['integratedChecksPassed']=1087;d['validation']['globalItemSlot']=v;d['milestones'].append(dict(id='global-item-slot-owner',atUtc=now,status=a['status'],integratedChecks=1087,remainingLifecycleControllers=24))
for subsystem in d['subsystems']:
 if subsystem['id']=='currency-items-packages':
  subsystem.update(evidenceStatus='Shared config, all nine entity factories, rewards/package rules and source singleton ownership confirmed by dedicated audits.',implementation='Actual item manager/module, shared config, inventory/rewards, nine entities and lazy global owner implemented. Full product/provider/Main composition remains pending.',validation='1087 integrated checks across the project; latest10 verify source singleton and ItemManager startup/save/restart. Package/Tool chain covered in preceding milestone. Full subsystem/runtime incomplete.')
write(p,d)
p=W/'analysis/VALIDATION_MANIFEST.json';d=read(p);d.update(atUtc=now,caseCount=1087,latestValidation=v);d['validationHistory'].append(v)
files=[f for f in (W/'UnityProject/Assets/AreaBattle').rglob('*') if f.is_file() and (f.suffix.lower() in ('.cs','.shader','.json','.prefab','.mat','.unity','.txt','.bytes','.ttf','.otf','.spriteatlas','.anim','.controller') or '/FirstPack/' in f.as_posix() or '/Recovered/Loading/' in f.as_posix() or '/Recovered/Audio/' in f.as_posix())];files.append(W/'UnityProject/ProjectSettings/TagManager.asset')
assert all(f.stat().st_mtime<=report.stat().st_mtime for f in files)
d['sourceFingerprints']=[dict(path=f.relative_to(W).as_posix(),sha256=hashlib.sha256(f.read_bytes()).hexdigest()) for f in sorted(files)];write(p,d)
text='''\n\n## 2026-09-30 GlobalItemManager 单例宿主增量\n- 1087集成通过（新增10）；analysis/global-item-slot-integrated.log；Unity47084已确认退出。14/38控制器生命周期不变，method-map5170。无新native/Player。\n- OutgameGlobalItemSlot.cs：一个原4542对象对应Lifecycle/Rewards一对实例；get34590先construct/publish，再currentConfig.InitLegacyUnityJson，最后重读slot返回。config初始化失败保留partialowner/tables，不因下次getter自动retry；构造失败不publish；初始化回调释放/替换时返回current null/newowner；旧owner Release照源码也清新槽。\n- ctor34570只设统计id10020和Products32/Items36。修正OutgameGlobalItemIndexes早建snapshots：40/44保持null直到InitializeProducts/Items；既有3个已初始化快照测试显式执行初始化。新增pre-init奖励case证实消息/插入后nullsnapshot失败，不默默补初始化。\n- LifecycleHost接current config.Dispose→clearglobal、update注册/移除、统计注册MsgDispatcher object[]{eventId,query}。BindEntities/CreateFactory跟随current slot。单例测试的更新为受控注册边界，不是完整产品tick宿主；生产product-update/reset/price/config供应者及Main/pool仍待组成。\n- 实际ItemManager首次本地callback触发4表加载再record过滤→host/time/update/index/statistics，Gold model-only→Int64保存→同manager release/reinit→独立实例重启；服务端分支仅请求，等显式callback才创建全局。login10是隔离测试前提，Tool另由既有真实链验证。\n- 10checks含失败/重入/null/current替换/stale release/更新移除及config释放失败prefix。证据O/GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json/AUDIT.json；完整关外未完成。\n- 下一步：实际product update/reset/price与共享config适配、旧资源reader、data pool/ItemManager/entity服务装配；继续24控制器及完整UserInfo/CommanderUI/Dice/report/Main/account/menu/business/Player。\n'''
for p in (T/'REVERSE_PROGRESS.md',T/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md'):
 with p.open('a',encoding='utf8') as f:f.write(text)
p=W/'AREA_BATTLE_HANDOFF.md';q=p.read_text(encoding='utf8').replace('1077','1087').replace('2076',str(len(files))).replace('`package-item-entities`','`global-item-slot-owner`').replace('最新完成的是道具工厂九种实体及礼包奖励链','最新完成的是 GlobalItemManager 单例宿主及 ItemManager 延迟初始化/保存重启链；此前九种道具实体和礼包奖励链也已通过')
q=q.replace('本次只整理文档，没有修改生产代码、重跑 Unity、生成新 Player 或改动用户存档。','本轮新增单例宿主并修正快照创建时机，已重跑集成验证；没有新 Player 或真实用户存档改动。')
q=q.replace('最直接的缺口是 **GlobalItemManager 单例宿主和完整启动装配**。现有独立验证显式初始化配置/记录，尚未替代生产 Main 的完整对象关系。','GlobalItemManager 单例发布、配置延迟初始化和 ItemManager 记录启动已实现并验证；**完整生产对象装配仍缺失**，包括实际商品更新/重置/价格供应者、旧资源读取器、数据池注册和 Main 接线。')
q=q.replace('最新增量为 14 项','最新增量为 10 项').replace('2026-09-30 19:24:27',datetime.datetime.fromtimestamp(report.stat().st_mtime).strftime('%Y-%m-%d %H:%M:%S')).replace('analysis/package-items-integrated-final.log','analysis/global-item-slot-integrated.log').replace('PID `72384`','PID `47084`')
q=q.replace('最新审计：O 中的','最新审计：O 中的 `GLOBAL_ITEM_SLOT_AUDIT.json`、`GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json`；此前包括')
q=q.replace('最近生产文件：','最近生产文件：`OutgameGlobalItemSlot.cs`、')
start=q.index('1. 先读最新状态');end=q.index('\n常用验证：',start)
q=q[:start]+'''1. 先读最新状态、`GLOBAL_ITEM_SLOT_AUDIT.json` 和 `OutgameGlobalItemSlot.cs`，核对当前工程；不用重做礼包或单例 getter。
2. 已确认 getter34590先创建并发布单例，再current ItemConfigMgr.InitMgr，最后返回current槽；失败保留已发布对象、不自动重试；回调可以清空/替换，旧实例释放也会清当前槽。这些已有10项检查覆盖。
3. ctor34570仅初始化统计id10020、实时Products32/Items36；快照40/44保持null直到记录初始化。不能重新提前创建快照，也不能把配置初始化塞入ItemConfigMgr自身getter。
4. 下一步把现有商品Update/Reset/Prices接到共享GameProductConfig及原旧资源reader，再将slot/ItemManager/实体/数据池注册接入完整生产启动。Slot当前接受显式lifecycle构造器，测试的update注册是受控边界，不能当完整native产品更新。
5. 继续余下宿主/24控制器/Main/账号/菜单/全部业务；完成实际操作、保存重启、失败回调及原生Player验收。最终目标没有缩小。
''' + q[end:]
q=q.replace('从 GlobalItemManager 单例发布及配置初始化宿主接线开始','从商品更新/价格供应者、旧资源读取器及数据池/Main对象装配开始')
p.write_text(q,encoding='utf8')
script='C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py';base=['python','-X','utf8',script];common=['--project-root',str(W),'--target','wxcf1394487200e48f/43']
for kind,path in [('validationManifest','analysis/VALIDATION_MANIFEST.json'),('validationReport','analysis/VALIDATION_REPORT.md')]:
 result=subprocess.run(base+['record-artifact']+common+['--kind',kind,'--path',path],capture_output=True,text=True,encoding='utf8');assert result.returncode==0,result.stderr
result=subprocess.run(base+['set-check']+common+['--name','outgameGlobalItemSlot','--result','pass','--evidence','analysis/targets/wxcf1394487200e48f/43/generated/outgame/GLOBAL_ITEM_SLOT_AUDIT.json','--depends-on','unityProject','--depends-on','validationManifest'],capture_output=True,text=True,encoding='utf8');assert result.returncode==0,result.stderr
assert all(hashlib.sha256((W/f['path']).read_bytes()).hexdigest()==f['sha256'] for f in d['sourceFingerprints'])
print(json.dumps(dict(checks=1087,controllers=14,remaining=24,fingerprints=len(files),sourceMethods=len(methods),methodMap=len(mm))))
