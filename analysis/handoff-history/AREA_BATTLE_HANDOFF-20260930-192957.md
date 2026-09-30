# AreaBattle 关外完整复原交接

更新：2026-09-30；用户要求“交接一下，并开启新对话，目标也迁移过去”。
工作目录 W = E:/Projects/AreaBattle（非 Git 仓库）；PowerShell；Unity 6000.0.68f1。
目标 T = W/analysis/targets/wxcf1394487200e48f/43；证据 O = T/generated/outgame。
下面路径相对 W，除非标有绝对路径。此次交接不修改生产代码或用户存档。

## 迁移目标（完整保留，新对话创建无预算限制 goal）
在 E:/Projects/AreaBattle 中自主完成《冲向那座塔》wxcf1394487200e48f/43 的关外逻辑完全复原：依据授权原包与恢复源码建立完整入口/主界面、关卡与模式选择、养成与装备、经济与商店、任务活动奖励、离线/每日/时间、广告及平台回调、存档与返回关外生命周期的证据和覆盖清单；补齐 Unity 生产实现、原始数据和可操作流程，运行源证据黄金用例、持久化/重启/失败回调与完整流程验证，交付可运行构建和明确验收报告。保持已有战斗实现与用户数据；未知规则不得编造，未验证服务端或平台行为明确记录，不把局部通过当成完全复原。

用户已授权自主持续执行，尚未验收；不可把局部通过报告为完成。
旧对话 01a0eb28-b674-79d3-b948-4cae8535bbfe 的目标迁移前为 usageLimited，不是完成。
本次旧对话只做交接；新对话接管同一目录，避免双方同时编辑。未授权另开子代理。
适用技能：wechat-minigame-reconstruction-orchestrator、wechat-minigame-reverse-expert、unity-minigame-restorer；按需读取。

## 核验基线和局限
- analysis/unity-integrated-validation.json：passed=true，1077 项 checks。
- analysis/VALIDATION_MANIFEST.json：caseCount=1077；latestValidation 为 package entities and weighted random。
- 最新集成日志 analysis/package-items-integrated-final.log。1077集成通过；最近原生为analysis/audio-resource-native.log的18项旧资源音频链，非本轮重跑；历史输入原生日志 analysis/input-manager-native-final.log，10 InputManager原生通过；历史Player10项通过；历史Level重置10项通过；历史Level准备10项通过；历史加载页/实体池/资源链/Game/UI/基础控制器原生分别23/13/8/7/6/10项通过。无本任务待续进程。
- 状态 T/OUTGAME_RESTORE_STATE.json：in_progress / package-item-entities，包含全部子系统和 nextPriority。
- 690 项验证的 38 控制器是 trace fixture；实际控制器生命周期和生产服务仍待接线。
- 最近批次没有新 Player build/smoke。生产入口仍为独立 Battle.unity，不能声称完整可玩关外。
- 历史原生 PlayMode：analysis/firstpack-native-startup-validation.json，18 项；本地模拟 patch URL、编辑器推进调度器。
- 该 PlayMode 的账号/SDK 边界注入，在线值缺失，10 个材质请求仅收集；不能代替真实服务端/设备验证。
- 最新method-map5170；14控制器生命周期/共享绑定已实现并验证，剩24（仅生命周期方法计数，并非完整业务图）。详细边界见末尾接续增量。

## 生产代码地图
代码位于 UnityProject/Assets/AreaBattle/Scripts，验证位于 UnityProject/Assets/AreaBattle/Editor。
- OutgameMainPreGameStartup.cs：Main30617/30622/30608，设置→注册 PreLoad/StarGame/ExitGame→38 控制器→audioInit→InitCtrl(true)→标志/字体/报告→ExitGame 监听→开始 PreLoad。
- 注册每个控制器前必须重新取得 logic()；工厂必须提供真实实例，无空实现 fallback；初始化失败保留源码部分写入。
- OutgameMainLifecycleState/MainLifecycle：Update 的 resources-only 分支在 catch 外；退出保存 prefs→data pool；标志为原字段投影。
- OutgameMainFocus：EnterGame gate、GamePause、PlayUI/模式6/PVP条件暂停界面、失焦保存；未知字段按 offset命名。
- OutgameMainExit：audioDestroy→退出标志→清 IAP procedure→Destroy Main/UpdateManager→保存 prefs→替换消息 dispatcher→EnterGame=false。
- OutgameProcedurePreLoad：用户网络初始化→ArenaRank统计回调→异步统计→Activity.Init(null)→原生 WaitUntil 登录flags64&&65→旧指挥官修复→StarGame。
- 等待没有虚构登录成功、超时或取消；真实 host 服务仍待装配。
- OutgameProcedureStarGame：广告映射、DisableUnload=true、退出/暂停监听、18 debug注册、模式0/跨日检查/OutgameStartupEntry。
- StarGame计时每次>=30只减30一次；暂停 flush 截断整数；原函数只有 ReportManager singleton 访问，不要虚构上报调用。
- OutgameProcedureExitGame：OnEnter prefs.OnSave→发送 ExitGame；清理由 Main 监听承担。
- 三个状态的泛型 owner 是 IOutgameProcedureManager，不能改成具体 OutgameProcedureManager。
- OutgameStartupEntry：菜单→奖励/旧数据迁移→prefab→GamePlay场景→EnterGame/lateInit/redDot；完成回调连接 home报告/rank/7day/playstate/loading。
- OutgameLegacyCommanderRepair：dealOldComm精确==1跳过；先写标志；firstCharge奖励bit触发commander3；使用live dictionary.Values；升级float cap28且保留未覆盖技能。
- OutgameFsm<T>/OutgameFsmManager：按 Type.FullName 注册、源码异常/部分失败/状态切换次序；FSM event/data API 仍未还原。
- OutgameProcedureManager Priority90；OutgameFsmManager Priority80；OutgameLogicModule Priority12。
- OutgameLogicModule：Initialize新list；初始化live indexed可追加；Update/Dispose foreach保留修改异常；释放pool先于Dispose。
- OutgameCoreModuleStartup：version→assetbundle→resources→UI→lang/FSM/logic/time/effect/pool/procedure；实际完整模块图待接。
- OutgameLegacyConfigManager：58 tables/2724 rows + 6 values，66 schemas；64读取→在线8get/7parse→颜色/材质→全局→派生索引→flag。
- OutgameMainConfigStartup：LoadingShown→SDK gameStart→加载data/config→SetConfigABRes→Initialize→ConfigLoaded→后续启动。
- OutgamePreGameSettings：report mode2、关闭multitouch、LargeNumSymbols首个枚举key、60FPS，保留空表失败顺序。
- firstpack原生资源：85原始config、2字体、2atlas共63sprites、UIRoot/TipUI、TipUI2动画6曲线27keys，91地址已验证。
- analysis/firstpack-native-bundles/firstpack.unity3d：4635198 bytes，SHA256 0df3a9abb648c34ce9dd9d3d1a20c16026c917bfef082825d5c038cb8ab20899。
- 已有其它关外规则/UI/存储实现请通过状态与相关 audit定位，不重复重写；独立JSON存储不是原始线上wire格式。

## 下一步（从这里继续）
1. 读 O/MAIN_PREGAME_COMPOSITION_AUDIT.json、PRE_GAME_REGISTRATION_ROSTER.json 及状态 nextPriority。
2. 已提取38控制器所有方法到 O/disassembly；生成生命周期矩阵，对照现有实现缺口，不再次整库提取。
3. 已修正：38类型都声明生命周期；type4561用显式GameFramework.IControl方法名。矩阵按slot4/5/6定位，不仅匹配短名。
4. 父类 IL2CPP 类型码21 GENERICINST，简单只处理码18的 parent helper不适用。type4561父genericclass@3700524=(3575116,26632,0,0)，首字段指向 Il2CppType。
5. ControlBase`1 type3497仅get_I26934/ctor26935；继续追其基类生命周期。3指令空方法仅说明该方法空，不能推断整个控制器为空。
6. 优先实际数据/流程控制器：Commander.OnInit30993取pool CommanderManager写field8；Process.OnInit30379重置16/20/flag8并InitProcedure11030。
7. 其余重初始化重点：LocalData31592、UI32739、Level31503、Player34199、Game31264、ServerTime31067、PVP30830、Arena30875、DiceGame31122、Rank31663。
8. 将38真实控制器绑定resolver，再装配native Main、模块update、core/config/pregame、实际login/menu；完成全流程、保存/重启/失败回调及构建验收。
9. 商店/支付、模式/排行、任务活动、广告时间等仍有未完成范围，以状态全部subsystems逐项闭环，不能只满足本轮接线。

## 最少工具与命令
- Python统一 `python -X utf8`，混淆类名含非ASCII。
- analysis/outgame_disassemble.py 接受源码类名；O/method-map.json当前4476索引。仅追加必要证据。
- metadata helper：以 __file__ 设置 analysis/recover_outgame_manager_registry.py 绝对路径，执行文件文本 split('rows=[]')[0] 得到 b/mem/u/pairs/ts/md/ms。
- types=88字节 <16i8H2I> pairs19；methods=36字节 <7i4H> pairs5；fields=12字节 <3i> pairs11。
- ts字段start8/count18、methodstart9、parent4；md declaring1、paramstart4、token6、slot[-2]、参数数[-1]。
- fieldOffset=u(u(3823136+4*type)+4*j)；typePtr=u(200288+4*typeIndex)，typeCode=(u(ptr+4)>>16)&255。
- usage v=u(addr)：kind=v>>29，index=(v&0x1ffffffe)>>1；kind1 type、kind3 method、kind6 methodSpec@483008+12*index。
- genericInst ptr=u(72592+4*inst)，count=u(ptr)、typesArray=u(ptr+4)；泛型源码提取器 analysis/extract_procedure_fsm_generics.py。
- Unity可执行文件 C:/Program Files/Unity/Hub/Editor/6000.0.68f1/Editor/Unity.exe。
- 验证参数：-batchmode -accept-apiupdate -projectPath E:/Projects/AreaBattle/UnityProject -executeMethod AreaBattle.EditorTools.BattleBuild.ValidateMechanicsOnly -logFile E:/Projects/AreaBattle/analysis/CONTINUATION-validation.log。
- PowerShell Start-Process 必须 -WindowStyle Hidden -PassThru；需访问工程外Unity缓存时申请工具执行提升，轮询同一session，不因超时重启/杀其它Unity。
- 写验证manifest需保留所有原字段及history；避免旧 record_validation_manifest.py 重置scope。
- fingerprint覆盖 Assets/AreaBattle 的 .cs/.shader/.json/.prefab/.mat/.unity/.txt/.bytes/.ttf/.otf/.spriteatlas，加所有 /FirstPack/ 文件；先断言源文件mtime<=报告mtime。

## 按需证据与历史
- O/MAIN_FOCUS_AUDIT.json、MAIN_LIFECYCLE_AUDIT.json、MAIN_EXIT_CLEANUP_AUDIT.json。
- O/PRELOAD_PROCEDURE_AUDIT.json、STAR_GAME_PROCEDURE_AUDIT.json、EXIT_GAME_PROCEDURE_AUDIT.json。
- O/COMMANDER_103_REPAIR_AUDIT.json、PROCEDURE_FSM_LIFECYCLE_AUDIT.json、LOGIC_MODULE_LIFECYCLE_AUDIT.json。
- O/STAR_GAME_DEBUG_REGISTRATION.json记录18个debug处理器，处理器实际实现未全部接入。
- 日志历史 RESTORE_PROGRESS.md、analysis/VALIDATION_REPORT.md、T/REVERSE_PROGRESS.md，仅按需读取。
- 旧交接原文归档：[完整历史](E:/Projects/AreaBattle/analysis/handoff-history/AREA_BATTLE_HANDOFF-20260930-140125-080328.md)，SHA256 046dd837f0d729cf7b4874ccee8f43e662abf0140c02b3d22b1c57f86fd65e13。

## 新对话接续增量（2026-09-30）
- 本对话目标active；已实现5控制器真实生命周期和共享绑定，剩33。优先UI/Game/Level/Player，然后Arena/PVP/Rank/Dice等。
- O/CONTROLLER_LIFECYCLE_MATRIX.json全114方法；O/CONTROLLER_LIFECYCLE_AUDIT.json当前行为/测试/剩余边界；analysis/audit_outgame_controller_lifecycles.py可重建矩阵。
- OutgameCoreControllerBindings + OutgameControllerRegistry：绑定4561/4034/4027/3903/4118，未绑定类型直接失败。平台必须显式供应，不生成登录成功。
- OutgamePrefabPoolControl完整原simple pool（区别NormalPool）；OutgameProcessControl原渠道/旧用户流程；OutgameLocalDataControl首充/状态/购买；OutgameServerTimeControl原时钟；CommanderControl位于Registry文件。
- analysis/controller-native-playmode-validation.json原生10项通过，平台/存储fixture；原生root持久/暂停/消息/延迟Destroy已验证，完整38控制器Main与Player仍未完成。
- 源码方法索引4477；analysis/extract_controller_generics.py补原get_I/GetModel共享实现。验证manifest保留history并刷新了全部sourceFingerprints。


## 2026-09-30 UIControl / TopInfo / MenuTab 增量
- UIControl 生命周期、进出关状态处理和共享注册绑定已实现；控制器生命周期实现6/38，仍有32待接，业务完整度另行验收。
- TopInfo 原预制体36节点/25绑定/11sprites/1font；原生 Canvas 位掩码/层级/射线和 MenuTab 关闭/异常回调顺序通过源证据检查。
- 集成705项通过；带GameView渲染的独立PlayMode6项通过，实际 WaitForEndOfFrame 按 main/shop/commander/item-info 加载，timeScale0。
- 批处理PlayMode停在第一个 WaitForEndOfFrame 后超时，保留 ui-native-batch-timeout.json。未替换生产等待行为。
- 菜单角色/数据回调、TopInfo账号头像按钮与值刷新、Main/account整体装配、剩余32控制器和新Player构建仍未完成。审计 O/UI_CONTROLLER_NATIVE_AUDIT.json。

下一步不要重做批处理 WaitForEndOfFrame 检查；使用 RunRendered；需要编辑器偏好目录访问，受限启动会在快捷键初始化失败。获批提权运行已通过，PID37348已退出。LevelControl4107 Update31526包含战斗actor循环，不可替换为空；PlayerControl4462 Update较大，与现有battle服务分离边界待核对。GameControl4064 OnInit31264先UseAnimationIns=false再按ConfigMgr.dicSceneSkin.Count创建Texture2D数组，Update31254源码空，Dispose31257仅清slot。


## 2026-09-30 GameControl 原生场景共享状态增量
- GameControl4064 生命周期、InitScene/皮肤与背景加载完成/相机与特效切换/模型清理已实现；与注册器绑定。生命周期7/38，剩31；这不代表完整关外已完成。
- 纠正旧交接：arr_homeGroud 元素类型是 Texture2D，不是 GameObject。当前场景id由主页/战斗共享，纹理回调保留源码捕获索引与当前id的不同读取时机。
- 从原GameSceneMono59序列化引用补齐4相机、17字段和21个选中节点，恢复兄弟顺序，使soldierRoot.GetChild(1)对应原模型背景。
- 集成711项通过；Game原生PlayMode7项通过（原场景引用、原皮肤7/8/9特效复用、回主页、下一帧销毁）。当前资源验证使用本地同步适配器，LoadPrefabControl/11001真实加载仍待接。
- 审计 O/GAME_CONTROLLER_NATIVE_AUDIT.json 与 GAME_CONTROLLER_FIELD_EVIDENCE.json；日志 analysis/game-control-integrated.log、analysis/game-controller-native.log。
- 下一步 LevelControl4107/PlayerControl4462与资源控制器；仍需完整Main/account/menu接线、所有关外系统闭环和新Player构建验收。


## 2026-09-30 LoadPrefabControl 原生资源接线增量
- Path13种地址、ToFileName、ResLoadHelper旧资源gate/前缀、LoadPrefabControl三类加载/模板缓存/回调参数已实现。生命周期8/38，剩30；LoadPrefabControl实体池与预加载业务仍未完成。
- GameControl经真实4117控制器→legacy scheduler/package/download runtime→原生AssetBundle完成资源11001与主页/战斗纹理、原始特效加载。Game接口补传原始args数组。
- 718集成与8原生检查通过，原生链路5次本地file请求（manifest+4bundle），缓存命中不发新下载。原始远程URL和完整Main/Player不在此验证范围。
- 原生harness首次因预制体自带Sprite而过早检查缓存失败；已改为等GameSprites写入，保留 prefab-loader-native-early-readiness-failure.json，生产时序未改。
- 审计 O/PREFAB_LOADER_SOURCE_EVIDENCE.json、PREFAB_LOADER_NATIVE_AUDIT.json。method-map4560。日志 analysis/prefab-loader-integrated.log / prefab-loader-native.log。
- 下一步恢复4117的PrepareStart/PreLoad/PrepareEntity/GetEntity/GetEntityNow/GetEntityRoot及helper，再Level4107/Player4462，保持完整关外目标。


## 2026-09-30 实体池与预加载增量
- LoadPrefabControl4117实体获取/同步获取/模板准备/根节点/PrepareStart/PreLoad及嵌套回调已按源实现。EntityRoot纠正为GameObject，实体命名、activeSelf复用、移除销毁项后跳项、模板空缓存不替换、同步miss返回null均有测试。
- 保留原作异常：失败计数不递增、成功只清局部；默认0可持续重试。有限轮黄金用例验证，无虚构重试上限或成功回调。
- 预加载固定10id→士兵皮肤/随机皮肤→Boss821→障碍物EnityID；PrepareStart共享计数与加载页reset、PreLoad局部索引与CanLoadNext。具体LevelControl/PlayerControl/加载页host仍需接线。
- 729集成+13原生PlayMode通过；原生仍5个本地file请求，验证真实AssetBundle预缓存/复用/帧末销毁。Native test原始生成脚本assert导致缺失harness的一次启动日志已保留。
- O/ENTITY_PRELOAD_AUDIT.json / PREFAB_LOADER_SOURCE_EVIDENCE.json（52方法/19usage）；method-map4565。日志analysis/entity-preload-integrated.log、entity-pool-native.log。生命周期仍8/38，剩30，不新增完整Player声明。
- 下一步Level4107/Player4462与加载页，绑定IOutgamePrefabPreloadHost，再完整Main/account/menu/全部关外验收。无待续进程。


## 2026-09-30 关卡配置、皮肤实体与原始加载页增量
- 新增源配置/资源片段 OutgameLevelConfigAccess、OutgameLevelResourceState、OutgameLevelConfigAsset(ScriptableObject)、OutgamePlayerSkinEntities；真实 tuple/皮肤/加载页由 OutgamePrefabPreloadServices 连接。完整 Level4107/Player4462 生命周期仍未完成，8/38不变。
- 恢复原 Resources 加载页10节点/3引用/11sprites/1font；原六帧12fps/.5秒循环动画、AutoCanvasLayer偏移25、进度/超时/重试/返回。UIControl Open/Close/Reset与原资源路径→恢复prefab映射已实现。全Main/UI host仍待接。
- 741集成（新增12）+23加载页原生检查通过。原生验证Animator/UGUI/默认OutQuad/825位移/严格超时>/state10、11时序/延迟销毁/旧页清新Singleton/CLR已销毁对象字段写。
- 集成关卡测试用原始关卡1/2重建原生bundle；模板缓存显式测试fixture，不能当成所有实体预加载已可用。没有新Player构建。首次导入fake-null、测试fixture缺依赖、动画额外尾键失败日志保留，修复后通过。
- 审计O/LEVEL_LOADING_AUDIT.json、LEVEL_LOADING_SOURCE_EVIDENCE.json；method-map4612。日志analysis/level-resource-integrated-fixed.log和loading-page-native-fixed.log。
- 下一步完整Level生命周期/update/Prepare后初始化与Player生命周期、真实loading host/Main/所有缺失实体资源，继续全部关外与持久化/构建验收。无本任务待续进程。


## 2026-09-30 Level生命周期、阵营统计与准备完成流程增量
- OutgameLevelControl 实现 OnInit/Updata/OnDispose，玩家阵营源为Global.PlayCampID；OutgameLevelRuntimeState与既有PlayStateDispatcher共享field8。BindLevel已验证。生命周期方法层面9/38、剩29，不能理解为9个业务控制器全部复原。
- CampInfo池/索引/颜色/逐塔计数与分数截断/neutral排除/CampChange后胜负判断按源实现；Update逐塔读取live时间倍率、foreach异常和一次timer刷新保留。
- InitTowers顺序、live总数和最后Boss；InitObstacles实际nativeEntity81变换、append与flag；Prepare后场景/PlayUI/state4/相机/协程顺序已实现。完整Tower/Soldier factory、InitGameData、WayLine/AI和Main接线仍待补。
- 两次WaitForSeconds(.2)协程源已恢复；756集成（新增15）+10原生检查通过。原生验证暂停scaled等待、原始障碍物mesh、真实BindLevel与销毁保留。WayLine/AI是明确记录fixture效果，无完整原生战斗或新Player声明。
- 首次障碍物测试错误要求Collider；原始恢复visual prefab只有mesh，改为验证MeshFilter，未添加虚构碰撞器。失败日志保留。
- O/LEVEL_CONTROL_AUDIT.json / LEVEL_CONTROL_SOURCE_EVIDENCE.json（35方法及原字段布局）；method-map4681。日志analysis/level-control-integrated-final.log、level-start-native.log。无本任务待续进程。
- 下一步Level InitGameData/剩余集合与factory、完整Tower/Soldier/WayLine/AI与prepared/state UI/Main，Player4462和其他29生命周期，再全关外验收。


## 2026-09-30 Level重置、复用、特效集合与查询增量
- InitGameData按源顺序清理：Boss引用→隐藏障碍物并清列表→check flags→active Camp/Tower.Clear→全部Soldier.Clear(true)/Effects.Destory→WayLine.field168=0。保留池/关卡配置/状态/其它flags与计时，不补造重置。
- 对象池按inactive/士兵field112/Arrow type4/Boss过滤；完整具体Tower/Soldier仍待恢复。EffectCellection九方法已实现，原生帧末销毁、根丢失时保留句柄、单child重复命名和异常顺序均保留；真实EffectModule仍待接入。
- 778集成（本批新增22）通过；10原生检查通过：原始Entity81重置后立即复用同实例、特效子树帧末销毁、实体与SO保留。原生EffectModule/Soldier回调是明确fixture，不声称完整业务对象完成。
- 查询共享field76/80结果、neutral差异、仅排除Arrow、Unity destroyed==null、range平方距离对外半径平方/内参数开方及NaN比较、Random.Next(count)均按源。补SetLevelSpeed、refreshflag、GetBossObj和现有皮肤/节日选择到真实GameControl的场景路由。
- O/LEVEL_REUSE_QUERY_AUDIT.json与LEVEL_REUSE_QUERY_SOURCE_EVIDENCE.json（26方法/71字段+2泛型随机函数）；method-map4681，生命周期仍9/38剩29。日志analysis/level-query-integrated.log、level-reuse-native.log；无本任务待续进程。
- 下一步剩余Level API与完整Tower/Soldier/WayLine/AI/effect、Player4462及剩29生命周期，实际Main/account/menu接线与全目标验收。没有新Player构建，完整关外目标保持active。


## 2026-09-30 Player控制器、模型交互与原生生命周期增量
- OutgamePlayerControl4462已绑定共享registry，连接真实GameControl/Level/UI/皮肤/模型与加载器；OnInit/Updata/OnDispose按源顺序。LoadGameScreen先Game.InitScene再InitLoadModel；重复InitLoadModel先换模型字典、后Roots.Add重复失败；不虚构去重/清对象。
- 主页3动画计时先全部累加，再严格>11.7/13.7/14.7清零刷新；周期刷新捕获Exception并保留源warning。Page2拖动独立gate、200像素、严格>.3切换、3position后3scale、ChooseSoldier先于finger清除。事件旋转与拖动共享状态。
- TouchBegin gate state2/page3，HomeCamera原生射线、Scenes11=31、floatMax距离、xy更新/z保留与Animator.Trigger；原包TagManager图层8..31补回空槽，未覆盖已有冲突图层。
- Models回调位置188与scale184 live读取，raypoint172分离；animation实时SkinCatalog，native destroyed animator按Unity null处理。OnDispose只清animation缓存与监听，保留root/model，迟到动画完成仍走源fallback。
- 792集成（新增14）+10 Player原生PlayMode通过。原生真实LogicModule/场景/原始模型/烘焙动画；鼠标touch和点击Collider/AnimatorController为明确probe，不声称设备输入、原始动态场景或账号接通。
- O/PLAYER_CONTROL_NATIVE_AUDIT.json、PLAYER_CONTROL_SOURCE_EVIDENCE.json（28方法/26字段+原始图层证据）；日志analysis/player-control-integrated-final.log、player-control-native.log。无本任务待续进程。
- 修复生命周期matrix生成器遗漏此前Level4107的问题；当前10/38方法级实现，剩28，不代表10完整业务控制器；method-map4681。
- 下一步共享InputManager、剩28实际控制器、Main/module/account/menu；保留Level/Tower/Soldier/WayLine/AI/effect剩余范围、全部关外子系统与持久化/重启/失败/返回/Player验收。完整goal保持active，无新Player构建。


## 2026-09-30 共享InputManager原生输入增量
- 新增OutgameInputManager(type3603)：原生MonoBehaviour Update/OnGUI、命名单例Find/GetComponent/AddComponent/DontDestroyOnLoad，原生UnityInput默认数据源；PlayerInput默认构造已直接接入共享输入监听。
- Android/iPhone/WebGL走touch，其余mouse；鼠标Down>Up>Held、独立位置读数、精确坐标移动、Click先于End；touch End/Canceled先于Click、严格<50、不补UI屏蔽/WebGL鼠标回退。多指重建有效列表、id=数组index、Began也走Hold、位移后缩放delta/50、zero count触发MultiEnd。
- KeyDown重复仍派发Hold（WASM内层block核对），KeyUp先回调后移除live HeldKey，Hold传live list。Shutdown只清14个委托，保留历史/单例。注册Begin先日志后Combine，重复监听按次移除；CLog原始过滤策略待完整host恢复，当前可注入日志适配器默认Debug.Log。
- 811集成（新增19）+10 InputManager原生PlayMode通过。原生Update->默认PlayerInput->真实Player.OnTouchBegin射线gate，Dispose注销/Shutdown持续更新/持久场景/销毁重建验证。设备读取为明确脚本fixture，不宣称OS触摸或SDK/完整入口接通。
- O/INPUT_MANAGER_SOURCE_EVIDENCE.json、INPUT_MANAGER_NATIVE_AUDIT.json；日志analysis/input-manager-integrated-final.log、input-manager-native-final.log；无本任务待续进程。method-map4821。
- InputManager是框架类，不增加业务控制器计数：仍10/38方法级实现，剩28；完整关外goal active，无新Player构建。下一步剩余控制器与真实Main/module/account/menu生产组合、Level/AI等余项及完整目标验收。


## 2026-09-30 音频控制器、设置与UI路由增量
- OutgameAudioControl3904实际registry绑定；9方法表面/生命周期复原，Init/Updata源码为空、Dispose清当前slot。状态2播parallel1001、3StopAll、5播1002、6/7仅已有parallel暂停false/true、8/9停Music1、10停0、其他无操作。未改已有BattleAudio。
- OutgameAudioSettings直接复用live UserDataPrefs，原始SoundGameVolume/MusicGameVolume键、默认1、严格>.99；Sound开关只存，Music开关存后GlobalVolumeChange(typed VoiceType,float)。非法voice仍通知/不写、值不clamp、回调失败保留已写数据。
- OutgameUiAudioManager路由lazy初始化，3种node独立lazy缓存；Play保留ID数组/返回GUID数组引用。StopAll按parallel/sequence/single顺序stop成功才清引用；日志失败先于Stop；Pause/GUID不建node；Destroy只清共享owner，旧实例仍可清新slot。
- 827集成通过（新增16），含analysis/audio-settings-restart/UserData.txt真实隔离磁盘保存/重读，保留无关key。节点是验证probe，本批无原生音频/发声/PlayMode/Player构建主张。
- 生命周期11/38方法级，余27；method-map4969。O/AUDIO_CONTROL_SOURCE_EVIDENCE.json、AUDIO_CONTROL_AUDIT.json；日志analysis/audio-control-integrated.log；本任务无待续进程。
- 下一步AudioManager/native根节点/listener/资源/config、AudioCompositeBase与Parallel/Sequence/Single、AudioActionBase与Once/Loop/Fade/LoopFade后端，接上当前required node factory并验证原生音频。相关源码已抽取，尚未实现。之后剩余27控制器与真实Main/account/menu/全部关外范围、重启失败返回和可运行构建验收。


## 2026-09-30 音频节点与动作基类增量
- OutgameAudioNodes：真实native根对象/parent/local变换/layer；Parallel、Sequence、Single源调度、暂停、Stop/Destroy、开始结束消息。要求显式newAudio动作factory，尚不提供假成功或替换音频后端。
- Parallel开始回调才入字典，未开始的异步load不在Stop覆盖内；Stop源码lock+keys快照+Remove(i)循环索引bug保留（不是Remove(keys[i])）；Pause live字典；销毁后迟到callback仍发消息。
- Sequence type2只转1、首动作Play先于Enqueue、同步结束可留下已结束队首；End先Dequeue/消息/再Peek.Play。Stop置flag并Dequeue，只有已有native AudioSource才Stop，忽略voice，尾部Destroy清queue/resetflag；失败保留部分状态。Single firstID/null=>0，Pending先写再停Current；End清Current->消息->Pending.Play->清Pending；Destroy保留Pending，迟到End仍可能启动。
- OutgameAudioAction抽象生产基类：this.GetHashCode GUID、OnAwake/update注册/remove-add音量监听、async void Play先捕获Root再取Data、<=0音量=1/NaN保留、await OnPlay后写source/开始回调，无cancel/dispose版本gate。Dispose removeHandle后归零、监听、虚拟清理、Unload后Data=null，失败顺序和反复Dispose保留。
- 860集成通过（20 node+13 base新增），日志analysis/audio-action-integrated.log，O/AUDIO_NODE_ACTION_SOURCE_EVIDENCE.json与AUDIO_NODE_ACTION_AUDIT.json。native GameObject/AudioSource属性用于编辑器测试，加载/update/具体动作/async context为明确probe；本批没有新PlayMode/发声/Player验证。本任务无待续进程。
- 仍11/38控制器生命周期方法，余27；method-map4969。下一步具体Once/Loop/Fade/LoopFade、原始资源/SDK/倒计时，以及AudioManager root/listener/config/node与Composite.NewAudio override/factory，接通UI/controller原生音频。完整关外goal继续active。
- 下轮源码索引：基类3405/26260..26283，Play状态机3404/26284（1420bytes）；Once3415/26327..26334，Loop3410/26301..26307，Fade3408/26286..26295，LoopFade3413/26310..26321；嵌套OnPlay状态机已经在method-map，但按declaring type取，不要猜嵌套type顺序。AudioManager3421/26345..26369，UI3431/26421..26435，AudioData3420 offsets8 id/12Voice/16Atype/20Ptype/24Start/28End/32Vol/36ResPath。


## 2026-09-30 单次/循环原生音频增量
- OutgameAudioPlaybackActions.cs：实际Once/Loop动作，真实native AudioSource.Play/Pause/UnPause/Stop，helper子GO/local/layer和DestroyGO；基类services新增显式new/legacy资源task、OutgameAssetHandle、log/SDK-stop、默认Time.realtimeSinceStartup/原生Destroy。资源与共享update host尚未完整接线。
- Loop加载前捕获config/setting/name，await后使用live Root；clip null直接return，data null才Stop；OnUpdate/OnPause为空。Stop原生停播→end→handle.Release→Dispose；不清handle引用。停止期间load不取消，晚到可以重新开始但无update/data/listener；Root销毁后helper null、设置属性原样报错。
- Once保留EndTime/StartTime在await前写入，空clip报错后deadline=实时+0；deadline必须>0且elapsed>0才结束；0时刻空clip永久pending。已有source End<=0以!isPlaying&&!paused完成；End>0分支到时即停，无paused gate。Stop重置deadline/SDKhandle再end/release/dispose。
- 该微信WASM OnPlay只调度state2/3资源等待，Android state0/1无调度来源；SDK state48只初始化false且无后续写入，不人为打开SDK播放。SdkDuration和SdkHandle在可达路径保持0。
- 主界面1002原始AAC已存在asset-evidence但旧prepare_audio.py仅战斗跳过它。analysis/prepare_outgame_main_audio.py独立解码PCM float32无重采样，Resources/Recovered/Audio/1002.wav，O/MAIN_AUDIO_ASSET_EVIDENCE.json存source/decoded hashes。旧战斗代码/audio-runtime manifest未改。
- 875集成通过（新增15），analysis/audio-playback-integrated.log。13原生PlayMode通过，analysis/audio-playback-native-validation.json / audio-playback-native.log：原始1002/2001实际游标/起始seek/暂停恢复/音量/Stop/释放/帧后销毁/正EndTime暂停门限/自然结束。无设备录音或Player新build；loader/update在验证中显式fixture，不能冒充完整host。无本任务待续进程。
- O/AUDIO_PLAYBACK_SOURCE_EVIDENCE.json、AUDIO_PLAYBACK_AUDIT.json。状态audio-once-loop-native；875/4979/11/27。下步Fade/LoopFade+countdown、完整AudioManager root/listener/config/node ownership、Composite.NewAudio hook/factory、源资源与UpdateManager接通，之后其他完整关外范围。
- 已新抽取helper类型3424（26370..26379）和嵌套3422/26376、3423/26378。26371 CreateAudioSource/26372 Destroy已实现；LoadAudioClip26373/LoadAudioClipForNewRes26374需继续读嵌套；UnLoadAudioClip26375把path.ToLower()+.unity3d后只有!NewRes时AssetBundleManager.UnloadAssetBundle。analysis/audio_disassembly_slice.py PATH STARTHEX ENDHEX可打印紧凑源码及br目标，便于确认复杂状态机。


## 2026-09-30 淡入淡出与完整动作工厂增量
- OutgameAudioFadeActions.cs：真实Fade/LoopFade动作，公共body经原始opcode对比仅name/loop值差异；volume0播放、clipLength、调度Fade后base才写source/Start。required SetCountDownByMillisecond(int,Action<int>,Action<int,int>) / RemoveTime，尚未伪装成已恢复TimeModule。
- Fade targetVolume在await前写入并可由等待中的音量消息更新；每帧in=target*(duration-remaining)/duration，out=target*remaining/duration；Complete先RemoveTime(id)，source销毁也调用end callback；fadein只写target，fadeout写0后调用callback。TimerId从不归0，Dispose重复Remove保留。
- Stop先置Stopping，target=当前native.volume*setting（二次乘设置）；再fadeout。第一次Stop时source空只留下flag；第二次Stop直接release/Dispose，无EndCall、不resetflag。正常CompleteStop按Stop/End/release/Dispose/flag=false；所有失败保留已完成步骤。
- End<=0要求isPlaying且time>=length-1且!Stopping。显式End>0只比较time>=End-1，无stopping/playing gate：Fade第二次Update走重复Stop直接销毁不End；LoopFade每次Update重置渐弱计时。LoopFade渐弱结束只flag=false+渐强，无seek/Play或结束消息。Pause hook空，native暂停不影响外部计时。
- OutgameAudioActionFactory：真实1Once/2Loop/3Fade/4LoopFade，invalid=>null；GetType查Data缺失=>1否则Ptype；static Override typed Func<GameObject,OutgameAudioActionType,int,Action<int>,Action<int>,OutgameAudioAction>先执行，返回null才fallback。元数据typeRef6088证实普通值参数，WASM generic invoker的栈地址不是可变ref规则。type3 ctor注释误标LoopFade但usage3928512明确3408Fade。
- 893集成通过（新18），13新原生PlayMode通过：实际AudioControl state2/7/6/8→UI→Parallel→factory→原始1001 LoopFade及2001 Fade，游标、暂停恢复、声音开关/分数音量、渐弱、end/句柄释放/帧后对象销毁。原生验证的倒计时由明确实时driver提供、update/resource仍fixture，不能声称完整TimeModule/AudioManager/Main已接。
- analysis/audio-fade-integrated.log、audio-fade-native.log、audio-fade-native-validation.json；O/AUDIO_FADE_SOURCE_EVIDENCE.json和AUDIO_FADE_AUDIT.json。状态audio-fade-actions-factory-native；893/4979/11/27，无本任务待续进程。无新Player build。
- 下一步TimeModule3534：已抽取46方法（methodmap此前已有）。SetCountDownByMillisecond27283转AddCoundDown227275；RemoveTime27284转Remove27276。Add要求complete非空否则-1；++field40计时id，先dict56.Add(id,complete)，GetNowTime+duration后dict60.Add(id,end)，有Every则dict68.Add(id,new everyinfo{callback8,type12=1,last16=0})。GetNowTime27271和CheckMillisecond27280/CheckComplete27278/Remove27276、clock/pause需继续核对，不能替换成普通Unitydelta timer。随后shared UpdateManager和AudioManager root/listener/config/node registry/资源load/unload完整接线。


## 2026-09-30 源码倒计时驱动音频增量
- OutgameTimeCountdowns.cs含OutgameTimeClock（shared本地DateTime、1970-01-01 08:00 epoch、毫秒加TimeDifference/整数秒不加）、OutgameTimeEvery（float累积>=interval最多调用一次，成功后归零）、OutgameTimeCountdowns（TimeModule倒计时部分）。未冒充完整startup TimeModule，loop/Unity计时与事件lifecycle仍待实现。
- Add要求complete非null否则-1，unchecked++ID/complete.Add先于clock/deadline.Add，每毫秒句柄interval1。Complete快照keys+callbacks，live查deadline<=now；先callback后写当前实例RemoveList，所有callback完毕才统一Remove。回调抛出导致前面已完成timer仍在下次重播；替换callback不替换快照，删除deadline会跳过待完成callback。
- Check顺序complete/second/ms/minute；later every pass可看到complete回调新注册timer。Every快照对象即便被其他callback从live字典删除也继续调用（缺deadline剩余0）；每次call重新读取Last字段，保留重入影响。remaining先double→int(WASM溢出int.MinValue)再整数除单位，elapsed先float再除单位。
- second>=1000、ms严格>，空字典仍推进Last；minute>=60000且字典非空才推进Last。Initialize/Clear重置LastTime/LastMinute/TimeId，保留LastMillisecond/UnityRealTime。没有focus/pause gate；源码GamePause只触及loopTimers，完整事件所有权待接。
- BindAudio连接实际源倒计时到四种音频服务。913集成通过（新增20），初次1个测试因误判Dictionary复用空槽遍历顺序失败，修测试隔离后通过；最终analysis/time-countdown-integrated-final.log。15新原生通过，time-countdown-native-validation.json / time-countdown-native.log：真实DateTime.Now与源码countdown驱动1001/2001原生音频链，替换前次计时driver；loader/shared update仍fixture，无设备录音/完整Main/Player新build。
- O/TIME_COUNTDOWN_SOURCE_EVIDENCE.json、TIME_COUNTDOWN_AUDIT.json、TIME_MODULE_METADATA.json（3534/3535/3536/3537字段和泛型method解析）。状态time-countdowns-native-audio；913/4993/11/27，无本任务待续进程。
- 下一步完整TimeModule3534：27252priority60、27253init、27254Start监听GF_NewGamePause/GF_GameFocus、27255Update(本地DateTime.CheckTime→CheckLoopTimer→CheckUnityTimer)、27256GamePause、27257Focus、27258Clear、27259..27269 loop、27285..27290 Unity、27291Shutdown移监听/Clear/flagfalse。新抽helper3535/27298..27306、3536/27307..27309、3537/27310..27311，methodmap4993。GamePausetrue枚举loopTimers.Values调用helper.GamePause，false不操作；其他原始细节继续核对，不能以通用Unitytimer代替。
- Countdown component初始化/清理是完整Initialize/Clear的字段投影，未来整合要保留源所有字段重建/清理顺序与callback时机。TimeClock.IsInit static与模块Initialized flag是两个不同字段，不要混同。analysis/audio_disassembly_slice.py已修复忽略混淆类名NEL拆行，支持Time源码br target。


## 2026-09-30 完整TimeModule增量
- OutgameTimeModule.cs实现3534生命周期与3535循环/3536Unity计时器；继承已有OutgameTimeCountdowns共享ID，base拆出protected InitializeCountdownMaps/ClearCountdownMaps/ResetTimeFields，保留完整初始化与清理字段顺序。IOutgameStartupModule ready，完整应用所有权待装配。
- Start注册GF_NewGamePause/GF_GameFocus，允许重复注册；Shutdown每种只移除一次→ClearTimer→IsInitialized=false。GamePause bool true仅flush loop elapsed，false无操作；Focus写static。Update实际DateTime.Now/Unity realtime→countdown→loop(delta/unscaled)→Unity。
- loop先移除队列，再Dictionary.Add待加入，再全体AddTimes，再全体Check。autoPlay=false仍默认未暂停；仅unscaled>1丢弃。AddTimes由pause/focus gating，Check无gate。一次callback后丢弃超额elapsed；GamePause先callback后reset，与Check先reset后callback不同。GetValue返回total，remove先排队再callback。
- Unity先合并cached移除并去重→移除3个map→清complete→按main/second/minute加入→按second/main/minute tick。主timer完成仅排入下帧移除，其sameID秒timer同步移除。helper先Every再live判断完成；Complete先callback再按liveLoop减Duration或置完成；循环保留超额但每步一次。
- ClearTimer/Initialize均保留pendingAdds/Unity移除队列，并重置TimeId，源可能重启ID冲突如实保留。Clear不Destroy loop回调，不reset lastMilli/realtime。所有异常保留已提交的部分状态。
- 938集成通过（新增25），19全新native通过；最终log analysis/time-module-integrated-final.log / time-module-native-final.log，report time-module-native-validation.json。Native使用完整module.Update、实际Unity delta和DateTime驱动原音频；Focus冻结loop/Unity但countdown继续，Shutdown移监听验证。loader/shared audio update仍fixture。无完整Main/Player build声明。
- 初次integrated直接调用ValidateIntegrated返回report但不退出；验证完成后仅终止本任务62448。首次native71540因项目锁退出，无有效结果；最终native62124/integrated51160已正常退出，无待续进程。下次集成请用ValidateMechanicsOnly自动退出。
- O/TIME_MODULE_SOURCE_EVIDENCE.json、TIME_MODULE_AUDIT.json；状态time-module-native-audio；938/4993/11/27。next：shared UpdateManager + AudioManager3421（config/root/listener/node owner，3424异步clip资源helpers），随后其余27控制器和完整应用生产装配。


## 2026-09-30 原生UpdateManager增量
- OutgameUpdateManager.cs实现Type2920实际公开API/Unity消息调用链28方法，MonoBehaviour singleton按Unity fake-null→Find("UpdateManager")→建GO/name→GetComponent/AddComponent→DontDestroyOnLoad。无虚构Awake/OnDestroy清理；销毁后getter重建。BindAudio连接静态AddHandle/RemoveHandleById，每次操作访问真实owner。
- ctor pending id/handle lists和debugData dict、TimePreFrame16；正常/force/fixed lists和IndexDict lazy。共享ID unchecked递增跳过已占key，返回id>2147483645时将storedNext清0。Add立即liveList.Add→IndexDict.Add。
- Update pause gate→移除→force→normal。每pass捕获count，但每次getter/live index；force新增normal同帧可跑，同pass新项下次。FixedUpdate独立pause gate/count，nullskip，无移除处理。普通/force无nullguard/catch。
- 移除按id/委托都先IndexOf去重入队，Update先ID再delegate，nonempty处理后替换list而非Clear。RemoveNow委托只移normal第一项或force第一项，另移fixed第一项；IndexDict按委托相等找第一key仅>=0才删。重复delegate按较晚ID移除可能删除较早key，但debugData按原请求ID删；delegate移除留debugData，缺失ID不删debugData，负key留Index。异常保留prefix。
- O/UPDATE_MANAGER_CALL_GRAPH.json、METADATA、SOURCE_EVIDENCE、AUDIT。65新提取（63manager+2UpdateHandle delegate），methodmap5058。35混淆alternate有不同阈值/边界/循环，不能同名归并；未接productionalternate，未来实际调用需追确切metadata。
- 22806删除foreach的0018bb0d br_if0是循环回0018ba09，helper显示end位置不是实际backedge；逻辑为不相等continue，找到后取key退出。
- 959集成通过（新增21），23原生通过，analysis/update-manager-integrated.log / update-manager-native.log / update-manager-native-validation.json。Native源audioAction由真正MonoBehaviour自动Update驱动，Fixed也真实；TimeModule仍editor pump。原始1001/2001音频fade/play/end/Dispose与源码延迟update移除结合，singleton销毁重建通过。resource/config/AudioManager host仍fixture或待接。
- 验证Unity44220/59144已正常退出，无待续进程。状态update-manager-native-audio；959/5058/11/27，无完整关外Player build。next实际AudioManager3421（26345..26369、多个alternate）和3424异步加载helpers，再其余27控制器/完整主入口与流程验收。


## 2026-09-30 AudioManager真实配置与节点所有权增量
- OutgameAudioManager.cs含Services/Manager/Slot和AudioConfigReader（UnityJson AudioConfig + OnceAudioConfig专用解析）。原始firstpack已有55音频行+53Once时长，本轮读真实数据，未新增伪配置。AudioManager构造连接Actions.GetData和UIaudioSlot.CreateNode，工厂/节点/动作都真实。
- Init26365先flag33 guard；缺ConfigResource且!NewRes才errorreturn，不改旧map。log后replaceAudioMap→live rows，duplicate保留first但仍scanpath.ToLower.Contains audio/once→stickyUseOnce32。replaceOnceMap→若sticky加载（float.Parse当前culture+Dict.Add，duplicate/error保留prefix）→flag33true→EnsureRoot/MoveListener1。root/listener失败flag已提交，repeat不自动repair。Clear不resetsticky/Once/Android缓存。
- AudioRoot new+DontDestroy；AudioListener独立new，没有独立DontDestroy，Move先getUIModule3558.Camera(offset24)再EnsureListener，type1&&camera parent默认worldPositionStays=true；其它type不detach。CreateNode若Root fake-null才Ensure；defaultparentRoot；id viaRandomId(1,10000,keys)，mode1/2/3命名parallel_/sequence_/single_；Nodes[id]=node。
- Utils.RandomId wasm6831直接调用Unity.Random.Range@1883 metadata50652；碰到excluded递归，但丢弃递归返回值，最后返回首次id，允许overwrite追踪并遗留oldnode。证据analysis/audio-random-id-disassembly.txt。原样实现/测试，不修成唯一ID。
- ClearNode liveNodes foreach虚槽6=Stop(0)，不是Destroy；所有Stop成功后clearNodes/AudioData→flagfalse。Destroy随后root destroy/null→listener.gameObject destroy/null（只有listener live时）→UiSlot.Instance.Destroy。AudioManager singleton本身不清，UIaudio singleton清。
- 978集成通过（新增19），18原生通过；analysis/audio-manager-integrated.log / audio-manager-native.log / audio-manager-native-validation.json。原生按真实1001Ptype4、1002Ptype2、2001Ptype1跑controller/UI/manager/action/sharedUpdate/timer，root/listener/真实销毁后重入验证；此前2001Ptype3仅fade测试fixture，不能误作原表。
- O/AUDIO_MANAGER_METADATA.json、SOURCE_EVIDENCE、AUDIT。状态audio-manager-native-ownership；978/5058/11/27，fingerprints2054。Unity72600/32848已正常退出，无待续进程；仍无完整关外Player build。
- 下一步clip资源helpers3424：26373wrapper→3422/26376MoveNext(3764bytes)旧资源；26374→3423/26378(1349bytes)新资源。新分支已核对path.Substring(lastSlash+1).Split('.')[0]结果unused，再path.ToLower→NewResLoadHelper.LoadAssetAsync<AudioClip> metadata26176(genericusage4008644) await并返回handle；无额外release。旧分支还需完整阅读，包括firstpack/newroute/fallback。Unload26375先path.ToLower()+'.unity3d'再if!NewRes AssetBundleManager.UnloadAssetBundle。必须接真实源资源owner，当前native传输与TextAsset加载还是显式fixture adapter。
- 配置通用generic3993488=ConfigRead26612<AudioData3420>，4004888=JsonUtility.FromJson<AudioList2893>（AudioInfos，Name/Length string），4000632=GetModule<UIModule3558>。AudioConfigReader只实现已证据化且Main选择的UnityJson路径，modern/binary路由不冒充完成。


## 2026-09-30 音频资源helper与原生旧资源加载增量
- 当前994集成（新增16）+18原生通过，source method-map5081；controller仍11/38，27剩余。日志audio-resource-integrated.log/audio-resource-native.log，原生报告audio-resource-native-validation.json；状态audio-resource-native-legacy，完整关外Player仍未构建。
- OutgameAudioResources实现26373/26376、26374/26378和Unload26375，Bind到AudioActionServices。basename保持原大小写、仅forwardSlash和firstdot；path.ToLower当前culture；modern handle MainObject转AudioClip无release；legacy先loglower和basename，packDict ContainsKey→log→再次getdict/getItem→nativeLoadAudioClip，存在但空/缺资源不fallback；否则AssetbundleModule._LoadAsset<AudioClip>(lower+.unity3d,basename,null)。new-onlyhelper无自身modeguard；Unload归一化先于mode检查。
- OutgameNewResourceAsyncHelper恢复26176/26186的guard26181（false日志+null）、generic类型和await；具体modern module仍delegate边界。GetNewResModule26169按AppSetting static40 true选KeWanResLoadModule3511(generic4000600)，false选YOResourcesModule3527(4000640)，下一步追查各module typed async load和主程序实际配置。
- 新OutgameLegacyAssetOperation对应23789/91/92、generic23788：bundle lookup→native AssetBundle.LoadAssetAsync；request一旦存在Update立即false，manager移除但await继续看IsDone。error!=null包括"0"日志+done；未获request的failed op仍会留manager list。GetAsset只在request!=null&&done后as T。OutgameLegacyBundleRuntime.LoadAssetAsync恢复23827 check→remap→Acquisition.Load→register。OutgameAssetbundleAsyncLoader恢复26239/26253 ignored falseguard→nulloperation silentnull→await→GetAsset→native-null日志返回。
- IEnumerator await源22357→22389→generic22371/22374/22377已经提取（analysis/extract_audio_resource_generics.py共9sharedbodies；O/audio-resource-generics.json）。OutgameUnityAwait新增仅对sealed OutgameLegacyAssetOperation的overload：手动MoveNext catch并完成awaiter，以保留原异常传播/同步完成；此类型Current始终null、无$this coroutine parents，不宣称恢复通用nested-wrapper。原generic single-yield会丢异常，本轮asset default不再使用。
- 原生测试BuildPipeline将已恢复原始2001音频重建Windows AB。首包audio分支映射为显式测试fixture，不冒称原始firstpack包含音频；真实Config读取已有rebuilt91asset firstpack内AudioConfig/OnceAudioConfig（55+53）。actualfile UWR+manifest+native AssetBundleRequest+AudioManager+UpdateManager+OnceAction，ref2→1→0、自然结束卸载、missing asset null诊断、nativeUnLoad销毁clip、再次真实下载、同步MoveNext异常传播均验证。Modern typed module acquisition未完成，不用fake成功。
- O/AUDIO_RESOURCE_SOURCE_EVIDENCE.json/AUDIT.json保存source hashes/usage IDs/边界。本轮自建Unity37040/55736/66928/48228全部正常退出；无需要续等的进程。最新fingerprints由脚本重算。
- 下一步modern模块loading以及full Main资源/配置owner；随后继续27controller/可操作业务/存档与完整Player验收。保持整体goal active，无阻塞。


## 2026-09-30 Buff/Tool控制器注册与奖励入口增量
- 当前1008集成通过（新增14），无新native/Player；controller lifecycle13/38，剩25；source method-map5089；状态buff-tool-controller-registration。验证日志analysis/buff-tool-controller-integrated.log；本轮Unity752已退出。最新native仍上一轮audio-resource18，不能当本轮新native。
- 重新读取O/RESOURCE_MODE_AUDIT.json，确认AppSetting static36默认false且原有90641body扫描未找到直接enable写入。决定走source-default legacy主线，现代模块仍记录未完成但不阻塞原始Main必经controller接线；不把现代加载当必须先完成的默认入口。
- 新OutgameBuffControl.cs（含抽象OutgameBuffBase）：4025构造Dictionary<int,BuffBase>；Init订阅当前GamePlayState，重复会重复；30981回调/30983Update源nop/end。Dispose当前dispatcher RemoveListener一次→existingdict.Clear→registry.Clear4025，异常保留prefix，不遍历Buff.Clear。BuffBase.Equals源TargetId.GetHashCode()==obj.GetHashCode无null/type防护，GetHashCode使用CLR identity代替WASM地址hash（明确运行时差异），Clear空。
- 新OutgameToolControl.cs：组合已有真实OutgameToolDispatcher，恢复4256源GetReward/GetMultiReward live List<ListArrayInt> indexed循环，row.datas[0]/[1]→currentPurchaseCost(reason)→Change(...notifytrue)，忽略bool、逐条保存、无事务；multi unchecked Int32乘法。GetItemCountById始终local；GetItemNum id>=8001走原ItemModule Int64count后wrapint，低IDlocal；IsEnough signed>=。Main OnInit/Update/Dispose源全空（特别Dispose不clear singleton）。CoreBindings.Bind加4025，BindTools加4256；既有战斗文件不改。
- 14checks包括真实registry/LogicModule生命周期、重复订阅、当前dispatcher切换、旧对象清新slot、字典null失败次序、Buff unsafehash比较、live奖励追加/reason变更、拒绝扣款仍继续、部分保存失败、malformed row前缀、乘法overflow/zero、8001边界64→32、local替换、报告flag。effects为明确sink，不冒充完整平台/存档。
- matrix generator analysis/audit_outgame_controller_lifecycles.py已加4025/4256并重跑，O/CONTROLLER_LIFECYCLE_MATRIX/AUDIT与状态同步13/38、25余；O/BUFF_TOOL_CONTROLLER_SOURCE_EVIDENCE.json/AUDIT.json记录证据hash和边界。
- 本轮预查ItemModuleControl3875（尚未生产实现）：OnInit30185→InitMgr30181先ItemConfigMgr.Instance.dicGameItem(field8)存this24，再helper30180 foreach currentconfig.Values；type1==4 dictProp(field12).Add(id,row)、5 dictSkill(field16).Add、6 dictHero(field20)[id]=row，构造30187新三个dict但reinit不clear。getItemMgr30182 lazy DataManagerPool.GetModel<ItemManager4500> generic3995108，null允许下次重查。Dispose30188只clear slot。GetItemNum30186→ItemManager.GetItemNum34339→GlobalItemManager.GetItemCount。GetItemConfig30183→ItemConfigMgr.GetGameItemConfig。ChangeItemNum30179 longcurrent+signedintdelta unchecked，结果<0 warning道具 {0} 数量不足!!! returnfalse；否则manager.GetItem(config).虚槽3(delta)→Event.Item_Change static52(id)→pool.SaveData→true，具体工厂尚需完成。
- ItemConfigMgr共享GameItemConfig为type4527，不能混用现有OriginalConfig.GameItemConfig3942（project-specific）。ItemManager4500已source提取但未runtime wrapper；OnInit34334虚槽10 UpdateData(true)、GetItem34342 newFactoryBase4486(config.id)→IFactory.Produce；FactoryBase34292仅type1==1和9生产VirtualItem/Package factories，其它返回null；constructor34293从当前ItemConfigMgr字典找id，缺失error不throw但后续可能null。新提取BuffBase+FactoryBase补method-map至5089。后续优先补ItemModule/ItemManager/config/factory真实owner，不用globalitem字段直接冒充wrapper。
- Source tool generic32536首参为enum，数值转发已实现；32533为int完整change。GetReward reason为Global4272.PurchaseCost static52，cctor32608字符串PurchaseCost。GlobalItemCount provider仍显式边界，不虚构数量/登录。完整目标保持active无阻塞。


## 2026-09-30 共享道具配置增量
- 1020集成全通过（新增12），log analysis/shared-item-config-integrated.log；本轮Unity69928已退出，无新native/Player。method-map5147；controller仍13/38、25余。本轮没有声称ItemModule生命周期完成。
- 新OutgameSharedItemSchemas.cs由analysis/generate_shared_item_schemas.py按原metadata生成6类（共享4527/4529/4531/4533、Lang3490、ListArrayInt3452），独立AreaBattle.SharedItemConfig命名空间，防止与项目玩法类型3942混用。同名原始firstpack JSON真实读取79/4/16/10条。共享product字段groupID/itemId原JSON不存在则保持0，16条全入group0，不编造字段映射。
- 新OutgameItemConfigManager.cs：构造六个dictionary（package/reward index初始null）、slot lazy创建不自动Init；InitLegacyUnityJson显式选择原始Unity JSON支路，四表顺序item/package/reward/product读后BuildGroups，不清表。MemoryPack/alternate parser尚未绑定且未伪装支持。
- BuildGroups先替换package index→foreach按packageId/packageRewardId indexer覆盖，再替换reward index→按rewardId/rewardItemId覆盖，再Products.Values按groupID追加既有List（不clear）。泛型usage3954552/3955676/3954568/3955688均set_Item，3954820商品外层Add。失败保留已执行前缀，后续阶段不运行。
- Package/Reward查询miss新建空dictionary不cache，hit返回原引用。GetRewardItemRanges34560缓存原GameRewardConfig引用与weight snapshot（RandomObject3392 int offset8）；cache成功构建才publish，返回浅拷贝List/复用元素；group rebuild不clear cache。GetGameItemConfig使用receiver，GetGameProductConfig使用当前slot。Dispose只clear两个cache后clear当前singleton，保留原四表和两组；旧receiver仍会清掉新slot，异常保留前缀。
- 新OutgameItemConfigValidation12checks覆盖以上原表解析/重复初始化109重复日志和商品32项、分组重复id覆盖、浅拷贝和权重快照、空cache不失效、失败前缀、getter旧对象/当前singleton差异、dispose顺序。通过真实Unity JsonUtility+Resources加载恢复JSON，但无native资源传输新验证。
- O/SHARED_ITEM_CONFIG_SOURCE_EVIDENCE.json/AUDIT.json、SHARED_ITEM_CONFIG_SCHEMAS.json记录源方法hash、元数据和范围；analysis/inspect_shared_item_metadata.py可重查字段/泛型。新抽取GamePackageConfig/GameRewardConfig/RewardRangeData至methodmap5147。
- 下一步ItemManager4500 wrapper和FactoryBase4486→VirtualItem_Factory4488/PackageItem_Factory4487→实际item entities，再ItemModuleControl3875接线。工厂和manager源已在O/disassembly，尚未实现，不用GlobalItemLifecycle直接冒充。全目标继续active。


## 2026-09-30 ItemModule/ItemManager/工厂分派增量
- 1034集成全通过（新增14），日志analysis/item-module-integrated.log；Unity65828已退出，无新native/Player。method-map5152；controller生命周期14/38，剩24；不是业务图完成。状态item-module-manager-factory-routing。
- 新OutgameItemModuleControl.cs，CoreBindings.BindItems注册3875。Init二次读取current config（第一次保存Items，第二次遍历Values），4/5类别Add、6覆盖，无clear；Dispose仅清registry3875，保留map/cachedmanager。getItemMgr懒取pool.GetModel4500、null重试。Change先long当前数+signedint unchecked、negative警告退出；否则Manager.GetItem(currentconfig).AddItem(delta)→current message Item_Change(id)→current pool.SaveData→返回先前next>=0。不加clamp/recheck/零值跳过。源虚表220/224是slot4 AddItem（不是此前summary粗略的slot3）。
- 新OutgameItemManager.cs：IOutgameDataManager接已有Storage和GlobalItemLifecycle，OnInit UpdateData(true)，Callback global.Initialize(text,GetNowDateTime)，OnSave global.Save→storage.SaveLocalData，OnRelease global.Release；Update/ExpendReward源nop。GetNowDateTime为local DateTime.Now provider，不走server。独立OutgameItemManagerSlot懒取pool4500，OnInit/Release不清slot。AddRewards live indexed list跳过GoodsType6，其余取current ToolControl后low32 reward count→ToolChange(...false,empty,false)忽略bool。GetPackageOpenReward新List.AddRange原LinkedList后原表Clear，共享行引用。RewardData4549字段int id,long count,int order。
- 新OutgameItemFactory.cs恢复FactoryBase ctor双次currentConfiglookup、缺失log保留null，Produce只支持type1=1虚拟/9礼包。子工厂再次源ctor查表；type2映射1 Gold4496、2 Diamond4494、4 Strength4499、5 Commander4493、8 HeroPiece4498、10 HeadBox4497、11 GamePoints4495；Package1 OpenAuto4490、2 OpenManual4491，其他null。实体构造依赖Func<sourceType,id,IOutgameItemEntity>明确尚需实现，不做通用库存fallback。
- 新OutgameItemModuleValidation14checks：真实Global/Storage内存backend重启和Int64持久化、真实login门槛（测试显式progress10、生产无假登录）、serverdownload、manager/currentGlobal及lazy缓存、release顺序、Tool真实奖励live list/type6skip/long截断/保存失败、package行引用、9实体类型分派、缺失和工厂二次查表、module分类/reinit异常prefix、dispose保留状态、余额不足/Int64overflow、zero/消息/保存顺序以及entity/message失败。实体只recording fixture验证调用，不声称各实体效果已恢复。
- O/ITEM_MODULE_SOURCE_EVIDENCE.json/AUDIT.json记录证据和界限，matrix/audit14/38同步。完整生产Main/平台/关外Player仍未完成。下一步实现ItemBase4541→GlobalItemManager奖励引擎→九个具体实体，接入construct provider。
- 本轮新抽取ItemBase4541五方法（34564ctor、34565AddItem、34566AddItemOnlyModel、34567ItemToReward、34568Use）。Ctor直接currentConfig.Items[id]；ItemToReward非nullConfig返回一个RewardData(id,longcount,order0)，nullConfig返回empty。AddItem先resolve global，再virtual ItemToReward(count)→global.AddRewards；OnlyModel同理AddRewardsModel。Use先Item_Use(id,longcount)消息，再构造NewReport_item_use report设置id/item_game/type1/type2 nullable fields并上报；尚未生产实现。VirtualItemBase.AddItem等小方法反汇编的共享函数label可能是ActivityVirtualItemBase/TaskLivenessPoint，不能按label误判实际继承，metadata确认父为ItemBase4541。Global.AddRewards f12045、AddRewardsModel f12046，方法map检索字段非name（用已有结构字段）。


## 2026-09-30 Global奖励引擎与金币/钻石实体增量
- 1050集成全通过（新增16），最终日志analysis/global-item-rewards-integrated-verified.log；Unity43036已退出。首次70128编译因Initialize(null)重载歧义失败，改为InitializeWithHost后64040跑1049通过；复核snapshot函数发现本轮初稿/初始说明把34596误认商品快照，已按字段36/44修成道具快照并加nullhost检查，最终43036跑1050通过。只有最终结果用于manifest。无新native/Player，14/38 lifecycle不变；method-map5152。
- 新OutgameGlobalItemRewards.cs：AddRewards/AddRewardsModel委托Action<List<RewardData>,int>覆盖整个默认路径、第二参0；默认live indexed rows逐条Change，regular末尾currentIItemManager.AddRewards(original)，model无host。Expend unchecked负long再Change，无不足检查，末尾host.ExpendReward原正数list。AddRewardsByItemSelf逐条current config→GetItem→非null AddItemOnlyModel，再host.AddRewards一次。GetItem nullconfig直接null、common非null优先、common返回null回退host。
- Change helper34589既有row先unchecked longadd后统计；newrow先构造/统计再holdTime0/Items.Add，重入插入会让原Add抛异常。随后正确SnapshotItem34596/f12037（不是SnapshotProduct34583）→ItemUI_RefreshUserItem→Report→dirty=true；O/GLOBAL_ITEM_REWARD_FIELD_AUDIT.json与analysis/audit_global_item_reward_fields.py以metadata泛型和offset证明live36/snapshot44 ItemUserData4548，而product32/40 ProductUserData4547。旧说明和本轮初版商品快照推断作废。
- Report helper34581先Item_ItemChange(id,longdelta)→ReportDel(id,longdelta,reason)如有则替代默认。默认currentconfig.Items[id]（missing抛、presentnull日志返回）；报告factory先执行，填id/item_game/type1/type2；delta>=1用get，否则cost（0也cost），delta出signed32范围转0，intMin abs按WASM位算仍intMin；balance factory后读取，出范围0；再次resolve报告服务发送。IOutgameItemReports明确只承载源拥有字段和创建/发送顺序，尚未实际SDK发送。
- 新OutgameItemBase.cs：ItemBase ctorcurrentconfig.Items[id]，ItemToReward新list，config非null单row(id,longcount,order0)否则empty；Add/OnlyModel先capture currentengine再virtualToReward→AddRewards/Model。Use只Item_Use(id,longcount)→create/填use report→send，不扣数量。VirtualBase转发；真实Gold4496/Diamond4494纯构造继承。OutgameItemEntityServices.Construct这两类真创建，剩七类继续required AdditionalConstructors，不fallback。IOutgameItemEntity新增AddItemOnlyModel，旧recordingfixture仅兼容接口。
- GlobalLifecycle新增RewardHost和InitializeWithHost(text,host)，先发布host再既有record初始化，hostnull允许localclockfallback；ManagerCallback改用此入口，actualManager实现IOutgameGlobalItemRewardHost，解决此前只传clock丢失奖励host问题。
- 15新GlobalRewardschecks+1Module真实链：统计新/旧prefix、重入Add冲突、正确ItemSnapshot先于UI、四委托替代、live奖励list、report覆盖、zero/long溢出/intMin金额、negative库存/longMin负号、配置missing/null、create回调改余额、report失败、commonfallback、BySelf模型路径、Base空配置/Use不扣、virtualToReward前捕获engine、nullhost。实际module→manager→factory→Gold→global→manager→Tool/local→pool/storage保存，再实例化读取，金币+7/-3两套库存均4；无实体recording替身但报告/生命周期为显式hostfixture。
- O/GLOBAL_ITEM_REWARDS_SOURCE_EVIDENCE.json/AUDIT.json、状态/manifest/交接已同步；完整目标继续active。下一步具体实体Strength4499、Commander4493、HeroPiece4498、HeadBox4497、GamePoints4495、PackageOpenAuto4490/Manual4491，源body均已抽取（Type449x-343xx）。注意共享WASM函数label会显示ActivityVirtualItemBase/TaskLivenessPoint，要按metadata继承解释。


## 2026-09-30 五类虚拟道具实体增量
- 1063集成全通过（新增13），日志analysis/virtual-items-integrated.log；Unity51948已退出，无新native/Player。method-map5170（新抽DiceGameDataManager22函数其中18新增）；控制器14/38、24余未变。状态virtual-item-entities。
- 新OutgameVirtualItems.cs恢复Commander4493、HeadBox4497、HeroPiece4498、Strength4499、GamePoints4495，OutgameItemEntityServices.Construct现在七种virtual实体真实创建，只PackageAuto4490/Manual4491走AdditionalConstructors。原始共享表不支持的type2=6/7/9仍null，不填充。
- Commander Add/OnlyModel虚调用Use(count)，Use忽略quantity，currentCommanderManager.UnlockCommander(paramInt)→currentUI.RefreshCommanderItem再次读paramInt。新增真正manager.GetCommanderData31013 TryGetValue missnull；Unlock31015无条件curLevel=1（CommanderData4030.offset16确认），高等级也重置，未知id nullfault，界面失败保留先前解锁，不加价格、库存或report。UI接口IOutgameCommanderItemRefresh仍须fullUI owner提供。
- HeadBox Add/OnlyModel同样virtualUse；Use currentHeadBoxInventory.Unlock(paramInt)。窄服务复原UserInfoControl32198 guard→UserInfoManager32228 List.Contains→32229 List.Add→UserInfo_HeadBoxUnlock(id)。controller去重、manager直接Add允许重复；List由真实UserInfoData owner provider供给，当前测试独立List，未声称完整UserInfoManager生命周期/存档接线。
- HeroPiece三操作严格base，Use不扣库。Strength Add→baseAdd后SP_CHANGE(nullargs)，Use→baseUse后SP_CHANGE；OnlyModel继承无SP。Event4271静态56/cctor32607证据已核对。
- GamePoints Add/OnlyModel respectivebase后currentDicePoints.AddPoint(low32count)。源DiceGameControl31102实际丢弃参数调ChangePoint(zeros)，DiceData31135又忽略delta，GetPoint31144=(int)currentGlobal.GetItemCount(11001)，return/message max(point,0)→Mxtz_DataChange(1,result)，不再次加/修复原long库存。这三个窄方法由OutgameDicePoints生产服务实现，尚非全DiceController。
- 13checks真实原始79行表、7类型工厂、unsupportednull、Commander高level重置/negative/zero/未知id/UI失败/currentparam/虚Use数量传递、头像去重/live记录/消息失败prefix、Strengthnormal/model/use差异与当前dispatcher切换、HeroPiece真实base、积分不双加/低32截断/clamp仅消息及失败prefix。复用GlobalRewardsValidation的internalfixture，实际奖励引擎/manager/原表，有明确报告/界面sink，不冒充全平台。
- 新O/VIRTUAL_ITEMS_SOURCE_EVIDENCE.json/AUDIT.json、state/manifest/交接同步。下一步PackageItemBase4489.ItemToReward34300(1599bytes)、Use34302(179bytes)、AddItem34304/OnlyModel34303(basecalls)；OpenAuto4490.ItemToReward34305(2078bytes)、Add34306/OnlyModel34307(39bytes虚Use?须核对slot)，Manual4491仅ctor。已抽源body可继续读，不要凭常规礼包机制假设概率/循环。全目标保持active。


## 2026-09-30 礼包实体和权重随机增量
- 1077集成通过（新增14），最终log analysis/package-items-integrated-final.log；Unity72384已退出。初68908编译因Random歧义失败，Editor加System.Random alias修复。无新native/Player；14/38 lifecycle不变，method-map5170（generic另存3不入map）。
- 新OutgamePackageItems.cs：PackageItemBase4489/Auto4490/Manual4491及OutgamePackageServices。工厂现在9实体全具体创建，移除AdditionalConstructors；未知原type2仍源null。Packages.Manager须currentItemManager provider；NewChanceRandom默认每group newSystem.Random；weight/quantity共用GameRandomSource.Shared。
- Base34300和Auto34305：Int32 outerindex unchecked增量，signextend与longcount比，非正空，不擅改long循环/上限（>intMax可能源wrap，不测试不可终止情况）。每次currentConfig.GetPackageRewards(paramInt) KVP枚举，每row freshRandom.Next(0,10000)<rewardRandom才获currentGetRewardRanges；初始空跳过。j按live rewardItemCount，weightedselect→remove→quantity Inclusive(min,max)→order；不每轮检查候选空，overdraw会空selector[index0]异常，不补位/截断。
- GenericRandom26203 f10956+lambda26209 f15316从registration提取至O/disassembly/PackageRandomGeneric-{26203,26206,26209}.txt及package-random-generics.json；26206为另一个sortlambda仅证据不使用。weightednull日志权重随机集合不能为空并null，empty不日志但Next(0,0)后index0异常；sum unchecked，roll shared.Next(0,total)，live cumulative严格roll<sum，fallback0；不修负/溢出权重。analysis/extract_package_random_generic.py可重抽。
- Auto与Base差异：选中后Auto先currentconfig.Items[itemId]再type1==9则currentglobal.GetItem→Inclusive数量→非null virtual AddItem，立即副作用，不appendnestedrow；nullentity也消耗数量RNG。Base不查itemconfig、不展开。slot4由helper2866调table2088→wasm14832明确VirtualActionInvoker，概率Next helpertable6324→f1901槽6。
- Use34302先baseUse（Item_Use/report）→virtualToReward→逐row currentManager.PackageOpenRewardsTemp.AddLast（共享row）→currentGlobal.AddRewardsByItemSelf。Base Add/OnlyModel调用baseItemBase，因此也virtualToReward，但不Use队列/报告。Manual仅ctor继承。Auto Add/OnlyModel virtualUse，模型路径也会hostAdd。无补造礼包扣库或保存。
- 13Packagechecks+1Module真实链：原20000每份2无放回（min RNG gold10 diamond5）、共享cache不删、概率边界、null/empty/zero/overflowweight、overdraw、inclusive max+1和intMax overflow、原20001嵌套8000立刻奖8101而外11001稍后、队列nested先/引用、manualAdd/Use差异、autoOnlyModel fullUse、失败prefix、每row currentManager、auto缺配置 vsbase、live枚举异常；真实module20000→实际pkg/global/manager/Tool/local/存档重读gold10/diamond5，未插入礼包库存。
- 关键下一步：rg发现ItemConfigMgr.InitMgr唯一已抽caller为GlobalItemManager.get_Instance34590/f2040：ifglobalnull newctor→先publishglobal→currentItemConfigMgr.InitMgr，再returnstaticcurrentglobal。不能由ItemConfigSlot getter自动init；现需把此owner的failure/reentrant语义、Release/reset和config/reward/entity实际服务组成起来，接source-default legacyDataManager启动。查Type4542-34570 ctor及34590 getter/34577Release。
- O/PACKAGE_ITEMS_SOURCE_EVIDENCE.json/AUDIT.json、state/manifest/交接同步，完整目标仍active，后续仍需UserInfo/CommanderUI/Dice/report owners、24control及完整Main/关外Player。
