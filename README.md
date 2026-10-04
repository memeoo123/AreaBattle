# AreaBattle：冲向那座塔复原

目标：依据 wxcf1394487200e48f / 43 原包复原完整战斗与关外业务，完成生产Main/账号/平台装配、可运行构建和原版画面、音频及时序验收。已有战斗系统和部分关外运行时按原包证据恢复；完整复原目标保持进行中。

## 当前进展与获取工程

当前执行范围（用户2026-10-04调整）：**服务器相关工作先搁置，客户端恢复继续**。保留已完成的网络代码；网络登录、云存档同步、服务器时间、在线排行和远端上报暂不作为本阶段交付前置条件。优先本地用户资料/头像界面、结算奖励返回、本地保存重启、剩余控制器和Main本地装配，再完成客户端构建与视听验证。服务器事项仍属延期未完成，不计为完成，也不生成虚假成功结果。

当前完整复原目标已按用户2026-10-04“继续”恢复执行，尚未完成整体验收。最新1772项集成检查全部通过，本批新增23项；2883份验证输入一致，新增登录传输原生8项通过。已恢复NetTool域名配置/缓存、服务器密钥信封解码和LoginTransmitter的18个协议及存档合并上传；通过实际WebRequestManager验证加密存档HTTP往返与服务器时间明文GET。生命周期仍为22/38、剩16，不代表业务完成率；普通方法索引6606。服务器相关装配按用户要求延期；客户端RankUI/OverUI、Main本地装配及剩余业务/客户端构建仍待完成。 接续请先阅读 [AREA_BATTLE_HANDOFF.md](AREA_BATTLE_HANDOFF.md)。

使用 Git LFS 获取原始参考视频：克隆后运行 `git lfs pull`。Spine 参考源码为固定版本子模块，使用 `git submodule update --init --recursive` 获取。通过 Unity Hub 打开 `UnityProject`（6000.0.68f1）。Unity 的 Library、Temp、Logs、UserSettings、本地 Build 与测试临时目录均不提交，首次打开由 Unity 重建缓存。代码、Assets 及 .meta、Packages、ProjectSettings、恢复证据和交接文档保留在仓库中。

## 运行

先通过下面的构建命令生成本地 Player，再双击 `Build/Windows/AreaBattle.exe`（Build 不随 Git 提交）。完整构建默认从普通第0关和教程开始，已有本地进度时继续；蓝塔拖向其他塔建立连接，从空地划过兵线断线。进度保存在可执行文件旁的 `LocalProfile.json`，不使用微信存档。

指定普通关卡与指挥官组合（PowerShell）：

```powershell
& .\Build\Windows\AreaBattle.exe -battle-level 25 -battle-commander 1
```

普通关卡按原 LevelConfig.SceneId 映射；例如普通25关读取布局501，保留第25关的箭塔教程。指挥官组合范围1–6，每组三个原技能。技能按原配置在6/14/21关解锁，前两项点击施放，第三项拖向目标或地面。

本地档案保存关卡、所选指挥官、18种技能各自等级和库存。仅新建档案时使用每种道具3个、技能等级1的明确测试配置；成功使用后保存扣除，换关或重启不补充。旧版仅有关卡进度的档案迁移后库存为0。可编辑自己的 LocalProfile.json 检查不同持有状态；它不代表原账号，不执行购买或广告奖励。

直接检查特殊布局：`-battle-layout 99998`（Boss998），或 `-battle-layout 99999`（Boss999）；特殊关不会推进普通存档。竞技检查入口：`-battle-pvp-map 1 -battle-level 30`，按原 SpecialLevelConfig 将地图1映射为布局2001，技能解锁保留普通进度30。此CLI使用明确的对手指挥官1、三个技能等级1、AgentSkillUseConfig1配置；真实匹配、排行和对手账号未实现。代码可通过 InitializePvPMap 注入不同已知对手配置。

## 编辑与验证

Unity工程：`UnityProject`，编辑器6000.0.68f1。原程序版本2021.3.56f2，当前兼容性以实际导入、编译与Windows构建为依据。

Unity菜单 `AreaBattle / Import, Validate and Build` 导入已授权原始资源、运行机制检查并生成Windows版本。CLI入口：`AreaBattle.EditorTools.BattleBuild.BuildAndValidate`。仅跑机制检查：`AreaBattle.EditorTools.BattleBuild.ValidateMechanicsOnly`。

查看 `RESTORE_PROGRESS.md` 和 `analysis/VALIDATION_MANIFEST.json` 获取已验证范围。所有原包、目录哈希、反汇编证据、639份关卡及恢复规格独立保存在 `analysis/targets/wxcf1394487200e48f/43`；原微信缓存未修改。

`analysis/captures` 保存还原工程的实际 GPU 画面，包括技能、教程和 Boss。它们是还原版检查图，不能充当原游戏对照图。用户现已提供第871关开场原图，已完成单帧对照；记录见 `analysis/targets/wxcf1394487200e48f/43/generated/original871-visual-comparison.json`。完整动态对照尚未通过。日常关卡池首次生成、部分异步资源加载与重试竞态，以及原始随机序列仍保留为未验证项。

指定截图中的第871关：`Build/Windows/AreaBattle.exe -battle-level 871 -battle-commander 1`。对应原布局120；截图对照工具使用单独的库存测试档案，不修改正常本地存档。


## 2026-10-03 原始限时任务页面资源及奖励/进度条目

- 校验原始7包依赖闭包，新增取回5包；大小/MD5/SHA256一致。导入4个预制体（任务24节点、日页9、奖励4、页面46）、49张精灵和1份字体。14个原自定义组件仍明确列为待运行时绑定，静态资源不代表页面交互完成。
- 恢复 CommonLimitTimeTaskProgressItem4339 与 RewardItem4340。进度保留有符号64位、只截上限、负数文本与零分母；奖励读取原始 icon/atlasName，保留缺配置时旧界面、新数据，以及图标回调重入后读取原始参数数量的顺序。
- GameObject 点击替换指针委托；奖励先回调，再按当前数据请求道具详情。失败保留原始已执行状态。销毁先调用 BaseItem，再清对象/回调，保留 RectTransform 和奖励数据；原生帧末销毁已验证。
- 新增10项检查，完整集成1363项通过；6项原生PlayMode通过。2468份验证输入与隔离工程逐字节一致。源证据19方法、16字段、8调用/字符串，方法索引5978；生命周期仍19/38，剩19。
- 集成日志有32条既有ShouldRunBehaviour断言和一次Curl42；原生日志有一次UnityEditor.Search启动索引越界及一次Curl42，无编译错误。图标投递/详情弹窗为明确测试端点；未构建新Player或验收整页视听。
- 证据：LIMIT_TASK_UI_ITEMS_SOURCE_EVIDENCE.json、LIMIT_TASK_UI_ITEMS_AUDIT.json、analysis/unity-limit-task-import-report.json、limit-task-ui-items-validation.json、limit-task-ui-items-integrated.log、limit-task-ui-items-native-validation.json及native.log。
- 下一步补任务/日页选择数据源、任务/日页/累计奖励/预览及CommonLimitTimeTaskUI完整装配，再接七日大厅入口与红点；继续Task/Achievement、完整Main/账号/SDK/HTTP/场景、剩19控制器与全部业务奖励返回，最后验收Player和原版视听。完整目标保持active/in_progress。


## 2026-10-03 共享列表选择与原始七日日页装配

- 恢复 DynamicListProviderSelection4554 与嵌套选择渲染器4553 的11个共享泛型方法及默认0时长包装函数。选择状态保存在数据上；可见条目先写状态再回调，随后按回调后的数量遍历当前数据，使用原始GetHashCode比较取消其他选择；哈希碰撞、重入、异常前状态及滚动复用已验证。
- 恢复 CommonLimitTimeTaskPageItem4338 全部13方法，接真实1301子活动及原始7天数据。保留本地化调用后覆写中文天数、日期锁定、选择消息、回调后单行隐藏、日页/活动消息过滤、长度1数组和精确拆箱失败，以及无额外解锁检查的直接程序选择。
- 原始页面两个同名Scroll View导致旧导出出口歧义。准备脚本改为按原始对象ID映射唯一路径，运行时按保留的兄弟层级解析，保留原始名称和布局。日页绑定使用其独立ScrollRect及原始单列、间距30、首段30、非回收配置。
- 动态条目Dispose清引用与两种消息监听、清空普通按钮运行时监听，保留原生行对象、矩形和数据引用；页面/列表继续拥有最终销毁。日页重绘先更新文字/锁态，再重发已选页面消息。
- 新增14项检查，完整集成1377项通过；9项真实PlayMode验证LateUpdate渲染、原始按钮指针选择、锁定日按钮、统计10015→原始任务→红点、列表重绘选中消息、实际活动消息刷新和释放后监听/对象存活。2482份输入一致，普通方法索引5991，源证据18普通方法/11泛型方法/15字段/23调用，生命周期仍19/38。
- 最终集成日志保留32条既有ShouldRunBehaviour断言和一次Curl42；原生日志保留一次UnityEditor.Search启动索引越界及一次Curl42，无编译错误。首次测试变量重名编译失败已修正，initial-compile.log留存；未构建新Player或验收原版整页视听。
- 证据：LIMIT_TASK_DAYS_SOURCE_EVIDENCE.json、LIMIT_TASK_DAYS_AUDIT.json、dynamic-selection-generic.json、analysis/limit-task-days-validation.json、limit-task-days-integrated.log、limit-task-days-native-validation.json及native.log。
- 下一步补任务行、累计奖励、预览及CommonLimitTimeTaskUI完整生命周期/刷新/领奖，再接七日大厅入口；继续Task/Achievement、完整Main/账号/SDK/网络/场景、剩19控制器、全部业务奖励返回，最后验收Player和原版视听。完整目标保持active/in_progress。


## 2026-10-03 原始任务行、领取与原生保存重启

- 恢复 CommonLimitTimeTaskItem4337 全部11方法，以及 CommonLimitTimeTaskUI32880任务过滤/排序/居中和32897按钮状态比较器。任务行连接实际父活动1301001及任务对应子活动，按原始出口克隆奖励和进度条目，描述使用配置目标值，随后分别读取实时按钮状态。
- 保留先清奖励再取数据、奖励成功渲染后才入列表、进度单独重建、异常留下已执行状态，以及Dispose仅清奖励而保留进度/原生行/按钮/数据的原始不对称行为。“前往”按钮的原函数为空，未添加导航逻辑。
- 领取先执行实际Child.TaskComplete，再从回调后的当前任务数据发送SevendayFinishTask。拒绝或重复领取仍发送消息；报告异常阻止后续消息但不回滚已有奖励/状态；回调重绑任务会改变最终消息。完整页面自身的回调注册仍待装配，验证使用明确转发端点调用已恢复RenderDay。
- 任务列表保留单列居中、间距8、首段10、非回收设置；按实时显示条件筛选，GameValue回调后重读目标；仅按BtnState排序，保留已领取行，先定位第0行再更新列表。
- 新增9项检查，完整集成1386项通过；10项原生验证原49任务/7天分组、原始按钮指针领奖、活跃度奖励10、同帧防重复、条目帧末销毁、真实UpdateManager自动保存、独立存档重启和原始已领取按钮。背包持久化未由任务重启测试推定完成。
- 2490份输入与隔离工程一致，源证据21方法/27字段/16调用，普通方法索引6004，生命周期仍19/38。集成日志保留32条既有ShouldRunBehaviour断言及一次Curl42；原生日志保留一次UnityEditor.Search启动索引越界及一次Curl42，无编译错误。未构建新Player或验收整页原版视听。
- 证据：LIMIT_TASK_ROWS_SOURCE_EVIDENCE.json、LIMIT_TASK_ROWS_AUDIT.json、analysis/limit-task-rows-validation.json、limit-task-rows-integrated.log、limit-task-rows-native-validation.json及native.log。
- 下一步补累计奖励条目与奖励预览，再完成CommonLimitTimeTaskUI生命周期/关闭/倒计时和七日大厅入口；继续Task/Achievement、完整Main/账号/SDK/网络/场景、剩19控制器、全部业务奖励返回及最终Player/原版视听验收。完整目标保持active/in_progress。


## 2026-10-03 原始任务条目、每日视图与原生领取

- 恢复TaskItemItem4367/TaskItemItemData4366与DailyTaskSubUI3988，接实际普通任务活动/配置/经济模型。显示数据共享配置奖励数组，按原始首条件第二项取目标、低32位取进度；GameValue900001逐条过滤任务8，已领奖行隐藏，复用行不擅自重新显示。
- 原始12资源包全部通过目录大小/MD5与SHA256核验，导入TaskPanelUI84节点、TaskItemItem12节点、40精灵与1来源字体。原包两处对象含重复Image，Unity AddComponent不接受；导入器保留首图形并把第二图形映射到后置满矩形子节点，保留来源ID及原节点顺序。这是明确的兼容适配，原粒子/动画和视听等价仍待验收。
- 条目保留成就模式单向尺寸修改、特殊描述分支、进度截断/排名二态显示、精灵回调前奖励数量快照、随机替换及共享配置修改、回调重入后读取当前数据上报、销毁后保留数据/委托等原始行为。每日视图先更新进度再建行，实际策略领奖先于隐藏/活跃度/红点刷新。
- 真实Button点击先禁用，飞币请求不直接入账，按0.7秒OutSine滑至localX1600后才调用实际任务领奖；完成时先启用按钮，再领奖、隐藏和上报。原始IsClaim未被条目置位，防止重复发奖依赖实际任务活动state门槛。保留回调异常前已执行状态。
- 全量1463项（新增12项）和14项真实PlayMode通过；覆盖指针回调、防重复点击、timeScale0延后领奖、实际金币/活跃度、原生自动保存、独立文件重启防重领及延迟销毁。飞币/精灵/奖品弹层/随机奖励/报告/倒计时格式仍为明确必需宿主端点，尚不代表完整生产装配。
- 2640份输入与隔离工程逐字节一致；36审阅方法含33新增索引，普通方法总数6194，53字段、25引用。集成日志保留32条既有ShouldRunBehaviour断言与一次Curl42；原生日志保留一次UnityEditor.Search启动ArgumentOutOfRangeException和一次Curl42，无编译错误。初次重复图形导入失败留作诊断，修正后导入/集成/原生验证通过。
- 下一步完整TaskPanelUI4416生命周期、TaskSingleton泛型所有者、每日/成就页签、LivenessPreviewItem4350及活跃度档位交互、大厅任务入口与实际Achievement业务。页签/整页/原始特效未宣称完成；Main/账号/平台/全部业务、其余19控制器、最终Player和原版视听继续推进，完整目标保持active。
- 证据：TASK_ROWS_SOURCE_EVIDENCE.json、TASK_ROWS_AUDIT.json、analysis/task-rows-validation.json、task-rows-integrated.log、task-rows-native-validation.json、task-rows-native.log与unity-task-panel-import-report.json。record_task_rows_milestone.py已执行，不可重复执行；source evidence脚本可幂等重跑。


## 2026-10-03 活跃度奖励预览、领取与原始宝箱动画

- 恢复LivenessPreviewItem4350及其异步状态机4349；保留UIObject先注册Visible更新、Awake再直接隐藏对象的顺序。SetData快照使用ItemIcon而非icon；预览先显示精灵/名称/x数量，再等原生帧，按名称宽度只扩不缩；保持重入、重叠刷新、销毁期间等待和监听释放的原始行为。
- 恢复TaskPanelUI活跃度档位绑定：原始30/60/100阈值、状态图形、初次预览回调、可领取时移除旧运行时监听并绑定领取、红点与最低阈值判断、Glow画布层。预览缺配置时，先前可见性/位置改变保留；已领状态的旧回调/Glow不擅自修正。
- 领取先捕获奖励ID/类型/低32位数量、隐藏Glow、请求Effect1016并播放宝箱Animation；等新WaitForEndOfFrame后才显示飞币或奖品、禁用按钮、调用真实每日活跃度策略发奖与位标记、刷新红点。原版未在等待帧之前禁用按钮，也没有内部重复领取门槛；保留多个待续回调可重复发奖的源行为，不虚构去重规则。失败/重入前缀均验证。
- 从已验证资源直接补读6个原生Animation的完整字段；恢复2个共享legacy动画，18曲线/119关键帧含切线/权重精确回读。宝箱1.7秒手动播放、Glow1.5秒自动播放。原clip在3处引用原预制体本就不存在的hdzd_eff_bxGlow，保留为未绑定轨道，不补造节点。粒子、Effect1016实际运行时/资源和整页视听仍待验收。
- 全量1475项（新增12项）和14项真实PlayMode通过：实际EventSystem选择/点击详情锚点、UpdateManager自动收起、帧后宽度扩展、timeScale0时宝箱暂停但WaitForEndOfFrame仍发真实奖励、恢复动画/自动存档及独立重启档位状态。飞币/精灵/弹层/报告等仍是明确必需生产宿主端点。
- 2656份输入逐字节一致；30审阅方法含24新增索引，索引6218，63字段、20引用。集成日志保留32条既有ShouldRunBehaviour断言与一次Curl42；原生日志保留一次UnityEditor.Search启动ArgumentOutOfRangeException和一次Curl42，无编译错误。本批无新Player，生命周期19/38不变。
- 下一步先补任务整页依赖的实际Achievement模型/管理器/策略/活动与任务控制器4507，再接TaskSingleton3990、TaskPanelUI4416完整生命周期/每日与成就页签/红点及大厅入口。继续Main/账号/平台、其余19控制器/全部业务、特效粒子和最终构建/原版视听；完整目标保持active。
- 证据：TASK_LIVENESS_SOURCE_EVIDENCE.json、TASK_LIVENESS_AUDIT.json、analysis/task-liveness-validation.json、task-liveness-integrated.log、task-liveness-native-validation.json、task-liveness-native.log与task-panel-animation-import-report.json。record_task_liveness_milestone.py已执行，不可重跑；source evidence脚本可幂等重跑。


## 2026-10-03 完整任务页与大厅入口

- TaskSingleton3990泛型注册及约束接口调用已核实；共享实例先发布再OnInit，保留失败实例与重入语义。TaskPanel4416绑定18原始出口，Layer3/UITip、每日/成就首次初始化、false页签事件、原始倒计时/音频/关闭顺序与TopInfo恢复已接。
- 原始大厅taskBtn连接实际页面注册器，复用隐藏页面时恢复显示；资源加载、CloseUI与销毁/句柄释放沿用BaseUI所有权。关闭时不额外发明源代码不存在的预览、特效或行列表清理。
- 1521项集成（新增9项）、12项真实PlayMode通过，2687份输入一致，普通方法6332。原生验证任务领取50金币及20活跃度、页签切换、关闭释放、重开新实例、任务领取记录/活跃度独立文件重启。本轮奖励库存仍是内存夹具，不宣称账号金币重启恢复；首次错误扩大重启断言范围的失败报告与日志保留。
- 集成仍有32条既有ShouldRunBehaviour断言、Curl35证书失败及Unity云配置超时；原生有独立UnityEditor.Search启动异常与Curl42。无编译错误。控制器20/38，无新Player；生产Main/账号平台/真实道具存储、全部业务及原版视听仍待完成。
- 证据：generated/outgame/TASK_PAGE_SOURCE_EVIDENCE.json（26方法、38字段、9引用、3泛型方法及约束上下文）与TASK_PAGE_AUDIT.json，analysis/task-page-integrated.log、task-page-native-validation.json。下一步任务奖励与真实ItemManager/LocalDataManager和账号存储整合。


## 2026-10-03 账号经济与活动奖励持久化联调

- 新OutgameAccountItemRuntime组合实际LocalDataManager4119、ItemRuntime/ItemManager4500与ToolControl4256，沿用原注册参数与账号存储门槛。ToolChange实际调用整个数据池SaveData；本地数据回调替换inventory后，后续工具操作绑定当前记录。
- OutgameActivityRewardBinding在活动初始化前接入共享引擎，让原始GetCommonItem注册覆盖每日活跃度工厂。任务、成就及可选限时任务共用真实道具配置/实体。配置来自原始AssetBundle，未沿用模型测试中1004的合成类型。
- 每日任务先写领取状态，self-model奖励更新金币与活跃度，再由ItemManager调用Tool保存双方经济记录；成就先发奖保存经济数据，再标记领取，后续账号保存补齐领取记录。这两个源顺序、登录门槛、禁用保存和报告失败前缀分别验证。
- 1531项集成通过（新增10项），13项真实PlayMode通过，2696份输入逐字节一致。原生页面领取成就200金币和每日50金币后独立重启，ItemManager和LocalDataManager均恢复250，并保留双方领取状态与活跃度。礼包20000通过低/高随机输入覆盖金币、钻石、积分11001和嵌套8000→8401碎片，保存重启一致。
- 初始集成发现测试装配顺序、EditMode调用静态DontDestroyOnLoad，以及每日/成就不同顺序的错误预期，已按源修正；初始诊断保留。最终集成仍有32条既有ShouldRunBehaviour断言与Curl35；原生保留独立Search启动异常/Curl42，无编译错误。
- 控制器生命周期20/38、普通方法6332不变；本批为19已有方法、6证据依赖与2原始注册的组合审阅。报告、账号登录状态、皮肤/视觉/平台等宿主仍明确为测试端点，生产Main仍未装配，无新Player或原版逐帧验收声明。
- 权威证据：ACCOUNT_REWARDS_SOURCE_EVIDENCE.json、ACCOUNT_REWARDS_AUDIT.json、analysis/account-rewards-integrated.log与account-rewards-native-validation.json。下一步恢复EffectControl4058及效果模块/任务1016资源生命周期，继续Main/账号/剩18控制器/全部业务与最终构建验收。


## 2026-10-03 原始 EffectModule 与特效生命周期

- 恢复EffectModule3483以及BaseEffect/UIEffect/FlyEffect/LineEffect，审阅52份普通方法、1份CreateEffect共享泛型、59字段与24处调用/字符串，普通方法索引6384。模块优先级20，保留配置重复键、空表、初始化/关闭不重置读表标记及根节点创建顺序。
- 实际AssetOperationHandle接入实例化/释放；现代与旧加载入口为明确服务边界。保留局部位置/Euler旋转、原预制体缩放、父Canvas排序继承、Renderer绝对排序与Canvas累加。EffectCellection排序层参数纠正为整数ID0，并与真实模块联调。
- 主动Close先移除句柄再Dispose；正常结束先销毁/释放再移除句柄，失败保留原有前缀状态。源码异步加载无取消：提前关闭后晚到对象可能留存，已明确验证。Line.Play调用Base.Play并启动自己的第二个计时器；重复Dispose/Release提示按原包保留。没有声称这些源行为已经修复。
- 原包1016配置确认路径Effect/UI/hdzd_eff_bxGlow、持续5秒。真实PlayMode验证WaitUntil、时间缩放暂停、原生帧末销毁、实际飞行Tween和连线UI投影长度；使用明确测试预制体/资源提供者，尚未恢复1016原始粒子画面或接入完整任务页资源链。
- 新增15项检查，完整集成1546项通过；本批原生11项通过，2707份验证输入与隔离副本逐字节匹配。生产固定Unity6000.0.68f1未变，验证用6000.3.7f1，无新Player；生命周期仍20/38。集成日志32条既有ShouldRunBehaviour断言/Curl35，原生Search启动异常/Curl42；最终无编译错误，初始接口编译及Canvas测试前提诊断保留。
- 证据：EFFECT_MODULE_SOURCE_EVIDENCE.json、EFFECT_MODULE_AUDIT.json、analysis/effect-module-validation.json、effect-module-integrated.log、effect-module-native-validation.json及native.log。
- 下一步获取并恢复原始1016预制体/粒子依赖，接现代/旧资源及任务页；补EffectControl4058的NormalPool/序列/飞币所有权。继续完整生产Main/账号登录/平台、剩18控制器、全部业务及最终Player/原版视听验收。目标保持active/in_progress。


## 2026-10-03 原始任务宝箱粒子与领奖链路

原始 effect1016 / hdzd_eff_bxGlow 的10个资源包依赖全部校验，其中新获取7包82086字节。恢复4组粒子、4材质、4纹理和原生Shader引用，7362个粒子数值及曲线回读一致。恢复新旧资源包装和EffectConfig读取，将真实EffectModule接入TaskPanel领取及页签回调，保留原始重复领取/覆盖句柄/自然完成后页内旧句柄语义。

1554项完整集成（新增8项）和14项原生检查通过；实际Canvas/相机中粒子开关同帧差异486像素，验证有实际渲染。两档奖励累计150金币同时进入账号经济记录；保存后独立重启恢复两档领取状态。2738份输入与隔离工程逐字节一致；方法索引6384、控制器生命周期20/38未变。

验证使用本地原生AssetBundle获取宿主，声音/飞币/报告/平台仍为明确测试端点，未完成生产Main/账号登录/完整资源目录或原版逐帧视听验收。集成保留32条既有ShouldRunBehaviour和Curl35；原生保留Search启动异常/Curl42，无编译错误。最初缺少Canvas排序前提的失败日志保留；最终使用原始UIRoot和已完成布局的宿主Canvas，并检查页面位于相机前方。

证据：generated/outgame/TASK_BOX_EFFECT_SOURCE_EVIDENCE.json、TASK_BOX_EFFECT_AUDIT.json；analysis/task-box-effect-integrated.log、task-box-effect-native-validation.json 和 captures/task-box-effect-{with,without}-particles.png。下一步完整EffectControl4058/NormalPool/序列/飞币所有权，再继续其余18控制器、Main/账号/平台/全部业务和Player验收。目标保持active。


## 2026-10-03 EffectControl、对象池创建与原生领奖飞币

恢复EffectControl4058并接入注册器/LogicModule：原始moneyPool容量300、释放间隔/到期5秒、root=null、不缓存资源；重初始化只重建协程/时间字典，保留池、待清理项和检查时钟。NormalPool创建保留无管理器不写字段及失败时部分初始化顺序。销毁先调用池再清单例，不添加取消或状态重置。

恢复原始金币/钻石序列、回调后重读数组、索引递增和异常顺序；缺少位置回调抛错，其他物品不会自动继续。复用已有原生飞币协程、Tween与清理算法；补齐UIControl货币Text惰性缓存和原TopInfo出口，保留Dispose后缓存字段。

1570项集成（新增16项）、14项真实PlayMode通过；2748份验证输入匹配，方法索引6384。生命周期21/38、剩17。原生从任务页点击触发原始宝箱粒子及飞币，验证五金币图标移动、暂停时两个奖励并发、10个实例复用、金币→钻石实际回调序列、池对象销毁和独立文件重启。

两份经济记录按原路径分别验收：任务奖励50进入ItemManager和LocalData，随后直接FlyMoney7仅通过ToolChange把LocalData改为57，ItemManager仍50；直接钻石为3。序列不重复发奖，重启保持以上数值。初始误以为两份记录均57的测试失败已保留并修正预期，未改生产奖励规则。

池管理器获取/资源获取、声音/报告/平台仍为明确本地宿主；完整ObjectPoolManager模块、Main/真实账号登录/全部业务及Player/原版视听验收待完成。最终集成保留32条既有ShouldRunBehaviour和Curl35；原生Search启动异常及Curl35/42，无编译错误。证据：EFFECT_CONTROL_SOURCE_EVIDENCE.json（45方法/32字段/9调用）、EFFECT_CONTROL_AUDIT.json、analysis/effect-control-native-validation.json、captures/effect-control-task-native.png。下一步原始ObjectPoolManager模块与真实Frame/资源所有权，再推进完整生产装配；目标保持active。


## 2026-10-03 原始ObjectPoolManager与实际Frame所有权

恢复ObjectPoolManager3672全部模块生命周期及共享泛型工厂，依据11个普通方法、5个共享方法、4段泛型上下文和4处调用/字符串证据。Priority70、构造字典、初始化回调、实时枚举Update/Shutdown、类型FullName+池名格式、重复池异常、构造后Add及先Shutdown后Remove均按源实现。使用原始对象类型名，保留空名合并/点号碰撞和失败后的部分状态。

NormalPool生命周期已通过实际Frame.GetModule和ObjectPoolManager管理，不再使用联调宿主的池注册字典。注意销毁也调用GetModule；Frame清空后再次销毁可能重新创建未初始化管理器，这是原始行为。Frame按管理器70→Logic12更新，退出按Logic→管理器清理。资源卸载/获取、声音/外部报告仍是明确宿主端点。

1581项集成（新增11项）、16项原生通过；2755份输入与隔离工程一致，6384普通方法索引、控制器21/38未变。原生原始任务页、宝箱粒子、金币钻石序列经真实Frame更新，10个图标复用归还；timeScale=0时真实对象池自动到期释放，随后Frame清理和独立存档重启通过。原始ItemManager50/LocalData57及钻石3的分支差异仍保留。

最终集成32条既有ShouldRunBehaviour/Curl35；原生Search启动异常/Curl35/42，无编译错误。初始重复声明已有框架异常类的编译日志保留，最终复用原有类。证据：POOL_MANAGER_SOURCE_EVIDENCE.json、POOL_MANAGER_AUDIT.json、analysis/pool-manager-native-validation.json、captures/pool-manager-task-native.png。下一步TopInfo账号及UIControl生产绑定、GuideControl4065与其余17控制器/完整Main资源账号平台和全部业务，最终Player及原版视听验收；目标保持active。


## 2026-10-03 顶部栏原始特效与积分数字格式

原始effect1007/1019的11包依赖已在本地，通过目录大小/MD5与SHA256核验，无新增下载。导入两个预制体、四材质、四贴图及原生内置着色器引用；5个粒子系统9085个数值字段/曲线通过Unity读回，保留金币子节点200倍、钻石子节点150倍和根节点单位缩放。钻石根粒子渲染器原本禁用且无材质，保留由三个子渲染器显示的结构。

恢复BigNumExtension27612数字显示：未配置先告警、百进制缩放、四字符截断、后缀饱和、负号参与长度和原始异常均保留。小数检查循环只保留最后一位是否为零的结果，因此120/150均显示1，123显示1.23；没有改成四舍五入或推测修正。

1593项集成（新增12项）、10项真实PlayMode通过；2786份输入与隔离工程逐字节一致，6384普通方法索引、生命周期21/38未变。真实AssetBundle及EffectModule/Provider向原始顶部栏图片挂载特效，金币131、钻石244个可见差异像素分别取自其自然发射帧；持续显示、独立关闭、延迟销毁和模块退出释放通过。截图中余额为原预制体文本，此轮未将余额显示声称为真实账号刷新。

初始测试误要求禁用根渲染器也有材质，已按源结构纠正；验证场景先刷新相机画布再运行原始UIRoot挂载，避免根节点位于相机后方。原始粒子的发射/透明度曲线没有修改，测试等待真实可见帧。初始失败日志及相机诊断留存。最终集成32条既有ShouldRunBehaviour和1条Curl35；原生1条Search启动索引异常和1条Curl42，无编译错误。未构建新Player、未完成原版视听等价验收。

证据：TOP_INFO_ASSETS_SOURCE_EVIDENCE.json、TOP_INFO_ASSETS_AUDIT.json、analysis/top-info-assets-validation.json、top-info-assets-integrated.log、top-info-assets-native-validation.json及native.log；原始参数报告位于resource-snapshots/top-info-effects-20261003/particle-roundtrip-validation.json。下一步完成TopInfoUI账号/昵称红点/头像授权、原始Await和页面开闭，再接UIControl生产装配；继续Rank/Match依赖、Guide及其余17控制器、Main/账号/平台/全部业务和最终Player验收。完整目标保持active/in_progress。


## 2026-10-03 TopInfo完整页面生命周期与真实账号重启

恢复TopInfoUI4425全部17方法及两异步状态方法的对应逻辑，25个原始绑定点通过已有BaseUI加载/关闭宿主接入。Awake按源顺序注册两个消息、捕获LocalDataManager、绑定两个个人资料指针、隐藏体力/匹配资料、刷新余额、启动特效等待再刷新资料。OpenLater仅创建原始Canvas/GraphicRaycaster分组；虚拟Refresh为空。

余额读取捕获管理器的当前账号投影；昵称、红点、积分及头像/头像框依次重新解析所需服务，保留回调重入和异常留下的前序结果。原始Await只等待Transform可用或IsDisposed，活对象发GF_LoadUIOver；不等待打开动画。两次特效调用独立解析模块，保留第二次失败后的第一个ID、旧ID及Dispose后的迟到继续行为。Dispose仅移除两消息并关闭两特效，不调用基类、不清字典/参数/原生对象引用或指针委托。

授权事件参数仅作门槛；保留长度1/空元素的原始失败，昵称与头像地址从必需平台端点取得。按源顺序写当前Match记录、请求头像、刷新匹配昵称、保存UserDataPrefs，再保存数据池。Match的完整存档管理器尚待恢复，此轮使用明确的活记录投影接口，未伪造微信授权或持久化成功；完整UserInfoUI打开目标、Rank分数和通用头像加载也仍是必需端点。

1611项集成（新增18项）、15项真实PlayMode通过；2794份输入逐字节一致，24方法/46字段/8调用证据，普通方法索引6384、生命周期21/38未变。真实UIControl持有延迟加载的页面并重试货币缓存；实际Tool变更立即显示LocalData金币37/钻石5，UserInfo姓名更改刷新红点与原始头像。实际指针、明确测试授权/头像回调、原生两帧加载、WaitUntil、开闭与特效/主页面句柄释放通过；独立文件账号重启恢复37/5及“恢复测试”。未将直接Tool变更推断为ItemManager同步，也未将测试Match记录推断为真实平台存档。

初始余额测试错误地传入refreshTop=false，修正测试参数后通过，经济函数未改；原生验证器关闭方法名修正为现有CloseForName，初始编译诊断保留。最终集成保留32条既有ShouldRunBehaviour及1条Curl35；原生保留1条Search启动索引异常及1条Curl42，无编译错误。未构建新Player或验收原版整页视听。

证据：TOP_INFO_PAGE_SOURCE_EVIDENCE.json、TOP_INFO_PAGE_AUDIT.json、analysis/top-info-page-validation.json、top-info-page-integrated.log、top-info-page-native-validation.json及native.log；实际页面截图为analysis/captures/top-info-page-native.png。下一步恢复MatchManager/MatchControl、WXAvatar/UserDataPrefs、RankControl和完整UserInfoUI，补生产UIControl/资源加载、Guide及剩余17控制器、Main/账号/平台/全部业务和最终Player验收。完整目标保持active/in_progress。


### Rank计分、原始浮点与真实账号重启（1662项）

恢复RankManager4137的数据默认、LitJSON读取、分数解析、排名上限与时间写入、保存前格式化和源码字符串展开顺序；RankControl加减分、排名改善、离线回退均使用原版配置及随机边界。顶部信息栏读取实际计分控制器，账号池保存和独立重启通过。

直接执行原包未改写FloatToInt WASM，7组结果接入回归。f32输入1.28经源码循环得到12799999和7位小数；K的展开长度5导致源码除以20，最终存639999。真实页面保存前显示1.28K，重启恢复639999并显示6.39K。初次测试错误预期128000已根据源程序结果纠正，运行代码没有为测试改变规则。

1662项集成全部通过，新增17项，真实PlayMode25项；2845个输入与隔离副本逐字节一致，普通方法索引6435。本轮23方法/28字段，生命周期仍21/38。完整RankControl初始化、大厅/AI列表、引用池行及RanklistTransmitter继续恢复，不计入已完成生命周期。

证据：RANK_SCORE_SOURCE_EVIDENCE.json、RANK_SCORE_AUDIT.json、analysis/rank-score-validation.json、rank-score-integrated.log、rank-score-native-validation.json、native.log及analysis/captures/rank-score-native.png。保留初次浮点预期失败日志。record_rank_score_milestone.py已成功执行，不要重复运行。完整目标保持active/in_progress。


### 排行榜列表依赖：引用池、权重与国家回调（1672项）

按原始ReferencePool4142及RankItemData4260恢复类型隔离的FIFO引用池和行清理，保留Clear先于重复释放检查、默认允许重复释放、获取不再次清理和异常已发生部分状态。它与原框架同名池3449、GameObject对象池不同。

恢复RandomHelper共享泛型26201/26203及比较器26206：全部抽取直接返回原列表；部分抽取使用随机优先值加weight+1后降序排序，保留原对象引用、零/负权重与整数溢出。国家帮助器每次请求先清空ID，真实回调按配置精确代码匹配，异步未返回时保持-1；保留晚到回调覆盖和已有ID导致的提前退出，不编造平台成功。

累计1672项集成通过，新增10项；2849输入与隔离副本逐字节一致，普通方法索引6443。本轮11普通方法/5共享泛型/16字段。无新原生运行，最近Rank计分原生25项记录仍保留；生命周期21/38未增加。这些依赖尚待接入完整RankControl大厅/AI列表/初始化，再接RanklistTransmitter、UserInfoUI、Main/平台和全部剩余业务。

证据：RANK_LIST_SUPPORT_SOURCE_EVIDENCE.json、RANK_LIST_SUPPORT_AUDIT.json、analysis/rank-list-support-validation.json及rank-list-support-integrated.log。record_rank_list_support_milestone.py已成功执行，不要重复运行。完整目标保持active/in_progress。


### RankControl完整生命周期与大厅/结算数据（1688项）

RankControl4134按原版接入registry，恢复构造、颜色和配置顺序、头像框权重、默认分差、离线排名、100行大厅数据与结算AI名单。刷新使用实际ReferencePool FIFO复用行；退出仅清当前单例，保留列表字段，旧实例也能清除新实例的单例位置。

保持原版边界：上方固定最多50名，不使用配置upPlayerScoreNum；大厅玩家国家字段为空；全部权重抽取别名可改变默认列表排序；结算上方负分不归零；分差比较器相等时返回1；名称在两个随机字段成功后才移除。国家回调清除单例、头像框空表和配置失败的部分状态经过验证。

1688项集成全部通过，新增16项；真实PlayMode31项通过。原生验证100行初始化/复用、结算名单、退出、顶部信息栏及独立账号重启恢复分数和昵称并重建列表。截图只展示真实TopInfo，不声称RankUI/OverUI已渲染。2855输入与隔离副本一致，普通方法索引6443，本轮26方法/24字段，生命周期22/38、剩16。

证据：RANK_CONTROL_SOURCE_EVIDENCE.json、RANK_CONTROL_AUDIT.json、analysis/rank-control-validation.json、rank-control-native-validation.json、integrated/native日志及analysis/captures/rank-control-native.png。record_rank_control_milestone.py已成功执行，不要重复运行。下一步补RankUI/OverUI行呈现和RanklistTransmitter4482、实际国家传输，再补UserInfoUI/Main/平台/其余16控制器及完整Player验收；目标仍active/in_progress。


### 用户要求暂停时的检查点（2026-10-03）

当前状态：**paused / 未完成**。完整复原目标保留，等待用户明确要求继续；此前章节中的active、继续开发或下一步描述属于历史记录，不构成恢复工作的授权。

- 最新累计1704项集成检查全部通过；本轮排行榜传输协议新增16项。2859份源码/验证输入与隔离副本逐字节匹配。
- 最近一次原生PlayMode验证为RankControl的31项；本轮协议/AES变更仅做集成验证，未运行新的原生场景或构建Player。
- 控制器生命周期完成22/38、剩16。生产入口仍为独立Battle.unity，完整Main/账号/平台及全部业务未完成，不能以测试数量或控制器比例作为整体完成率。
- 已补RanklistTransmitter与BaseHttpNetTransmitter协议、请求缓存、错误分发、真实Match响应/保存重启，以及原版AES-CBC加解密；独立OpenSSL向量一致。网络响应为显式测试事件，未宣称远端HTTP成功。
- 保留原版上传遇业务错误仍回调、网络失败不回调、查询按data.count选择个人/列表、缓存请求沿用旧category/name/offset等行为。真实HTTP请求队列/agent、平台域名/密钥和错误上报仍待接入。
- 最新报告已归档：analysis/rank-transmitter-validation.json、rank-transmitter-integrated.log；初次测试依赖访问的编译错误日志也保留。41份相关原始方法快照存于generated/outgame/rank-transmitter-source-snapshot，暂停检查点为RANK_TRANSMITTER_PAUSE_CHECKPOINT.json。普通方法索引仍6443；正式传输层SOURCE_EVIDENCE/AUDIT发布尚未完成。

恢复后优先：完成传输层正式证据归档，恢复WebRequestManager/HttpManager/NetTool实际装配；补RankUI/OverUI和UserInfoUI呈现；继续Main/账号/平台、其余16控制器与全部业务，最终做Player、完整用户流程、保存重启、失败回调及原版视听时序验收。已完成里程碑记录脚本不要重复运行。本次仅更新文档/证据/状态，没有继续修改运行代码，没有提交版本。
