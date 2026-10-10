# 《冲向那座塔》复原项目当前状态与接续说明

更新日期：2026-09-30。本文为适合单独上传的当前状态快照，不是最终验收报告。

## 1. 当前结论

项目仍在进行中，尚未完成完整关外复原，也尚未由用户验收。已有大量原包证据、生产模块和自动验证；最新完成的是 GlobalItemManager 单例宿主及 ItemManager 延迟初始化/保存重启链；此前九种道具实体和礼包奖励链也已通过。生产入口仍为独立 `Battle.unity`，不能据此认定完整大厅、账号登录、关外到战斗再返回的流程已经可玩。

- 最新集成验证：**1087 项通过**，包含既有战斗与关外模块检查，并非 1087 条完整用户流程。
- 本次整理重新核对：验证清单中 **2078 个源文件指纹全部匹配**，没有发现验证后源文件内容变化。
- 38 个启动控制器中，已有 **14 个实际生命周期/共享绑定实现并验证，剩余 24 个**；这不是业务完成率。
- 反汇编方法索引：**5170 项**；另有礼包权重泛型的专项提取证据。
- 最新阶段：`global-item-slot-owner`；状态：`in_progress`，完整目标未完成。
- 本轮新增单例宿主并修正快照创建时机，已重跑集成验证；没有新 Player 或真实用户存档改动。

## 2. 目标与约束

目标：在现有工程中，依据授权原包和恢复源码，完整复原《冲向那座塔》的入口/主界面、关卡与模式、养成装备、经济商店、任务活动奖励、离线每日时间、广告平台回调、持久化和返回关外生命周期；最终交付可运行构建及明确验收报告。

目标 AppID：`wxcf1394487200e48f`；版本：`43`。工作目录：`E:/Projects/AreaBattle`，Git 仓库 `memeoo123/AreaBattle`，分支 `main`；Windows PowerShell；Unity `6000.0.68f1`。

保留已有战斗实现和真实用户数据；未知规则必须保留为未知，不能编造规则或伪造登录成功。用户不允许启动子代理。测试使用隔离记录、路径或后端，不能用真实存档做破坏性测试。

## 3. 已实现的主要范围

| 范围 | 当前实现与边界 |
| --- | --- |
| 战斗基础 | 保留既有布局、生产、AI、战斗、技能、Boss、暂停、结算与重试实现和验证；本阶段持续补关外，不重新宣称全部画面/设备一致。 |
| 启动与流程 | 已恢复核心模块启动、配置启动、Main 初始化/更新/失焦/退出、PreLoad/StarGame/ExitGame、FSM/逻辑模块和关外入口顺序。完整宿主对象和账号服务仍待装配。 |
| 配置与资源 | 旧资源路线、原始配置、字体、图集、部分原生 UI、资源/音频/时间/Update 服务已有实现和专项检查。主配置含 58 表/2724 行；共享道具配置单独建模，不能混用同名项目类型。 |
| 数据与存储 | 数据管理器池、存储排队/压缩/去重、物品记录、时间同步、索引快照、工具派发和部分养成数据已有源码对应实现。隔离的复原存档格式不等于原线上协议。 |
| 关外界面与养成 | 已有菜单切换、指挥官页面/卡片/技能详情、部分皮肤模板及对应规则。完整入口、页面服务和用户流程尚未闭环。 |
| 道具与奖励 | 已实现共享配置管理、ItemModuleControl、ItemManager、工厂、全局奖励、上报边界、物品基类、七种虚拟道具和自动/手动礼包；实体副作用已连接到相关生产服务。 |

最近道具批次的关键结果：

- 原始共享配置读取：79 个道具、4 个礼包组记录、16 个商品、10 个奖励记录；保留原字段默认值、重复键、缓存和初始化顺序。
- 工厂支持的九种实体均有具体实现：金币、钻石、体力、指挥官、英雄碎片、头像框、游戏积分、自动礼包、手动礼包。源码不支持的类型仍返回空。
- 礼包恢复了概率判断、按权重不放回选择、含上界的数量随机、原奖励顺序，以及嵌套礼包在转换过程中立即生效的顺序。
- 验证原始礼包 `20000` 和嵌套礼包 `20001`，覆盖暂存队列、消息/报告/发奖失败时保留的部分副作用。
- 已验证真实模块链：礼包 → ItemModule/ItemManager → 实体/全局库存 → Tool/本地货币 → 保存与重读。不是仅用空接口模拟发奖。
- 随机黄金用例使用可注入的确定性随机源；没有声称与原 WASM 的时间种子随机序列逐位相同。

## 4. 尚未完成的范围

GlobalItemManager 单例发布、配置延迟初始化和 ItemManager 记录启动已实现并验证；**完整生产对象装配仍缺失**，包括实际商品更新/重置/价格供应者、旧资源读取器、数据池注册和 Main 接线。

后续仍包括：完整 UserInfo、CommanderUI、Dice、报告宿主和剩余 24 个控制器；账号与真实菜单启动；商店/支付，普通/特殊/每日模式，竞技匹配排行，任务成就，七日/限时活动，骰子选卡，图鉴公告设置，广告平台回调，离线每日刷新，以及战斗奖励与返回关外闭环。部分基础组件已存在，不能把“整条业务流程未验收”解释为所有底层都未实现。

报告和平台接口仍有明确外部边界，未验证的服务端行为不能冒充成功。旧资源路线是目前源码默认路径；现代资源获取路线仍未完成，尚无证据表明它是当前默认启动必经路径。

还需真实帧更新、输入、完整用户操作、重启/失败回调、可运行关外 Player、视觉音频与原版对比及最终验收。本阶段没有这些完整结果。

## 5. 验证证据及限制

最新集成检查时间：**2026-09-30 19:34:44（北京时间）**。最新增量为 10 项，累计 1087 项。主要是 Unity 批处理下的源规则、边界条件和服务链验证；不能替代完整原生场景或设备测试。

- `analysis/unity-integrated-validation.json`：`passed=true`，1087 项检查。
- `analysis/global-item-slot-integrated.log`：对应最新通过日志。
- `analysis/VALIDATION_MANIFEST.json`：验证范围、历史、2078 个文件指纹及未完成事项。
- 最近的原生 PlayMode 结果是历史音频资源链 18 项，见 `analysis/audio-resource-native.log`；**不是最新礼包批次的原生重跑**。
- 历史 Player/启动检查只证明当时范围，不能用作当前完整关外构建验收。最近批次没有新 Player 构建或 smoke 测试。
- 本任务上次启动的 Unity 验证进程 PID `47084` 已确认退出；此信息为上次观察，不代表其他 Unity 实例状态。

## 6. 文件导航

所有下列路径相对工作目录 `E:/Projects/AreaBattle`；单独上传本文可了解状态，继续修改则仍需要工程与证据文件。

- 生产代码：`UnityProject/Assets/AreaBattle/Scripts/`；验证代码：`UnityProject/Assets/AreaBattle/Editor/`。
- 目标状态：`analysis/targets/wxcf1394487200e48f/43/OUTGAME_RESTORE_STATE.json`。
- 原包反汇编、方法索引与专项审计：`analysis/targets/wxcf1394487200e48f/43/generated/outgame/`，下文简称 O。
- 最新审计：O 中的 `GLOBAL_ITEM_SLOT_AUDIT.json`、`GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json`；此前包括 `PACKAGE_ITEMS_AUDIT.json`、`PACKAGE_ITEMS_SOURCE_EVIDENCE.json`、`package-random-generics.json`；此前还包括 `SHARED_ITEM_CONFIG_AUDIT.json`、`GLOBAL_ITEM_REWARDS_AUDIT.json`、`VIRTUAL_ITEMS_AUDIT.json`。
- 最近生产文件：`OutgameGlobalItemSlot.cs`、`OutgameItemConfigManager.cs`、`OutgameItemModuleControl.cs`、`OutgameItemManager.cs`、`OutgameItemFactory.cs`、`OutgameGlobalItemLifecycle.cs`、`OutgameGlobalItemRewards.cs`、`OutgameItemBase.cs`、`OutgameVirtualItems.cs`、`OutgamePackageItems.cs`。
- 明细历史：目标目录 `REVERSE_PROGRESS.md`、`RESTORE_PROGRESS.md`，以及 `analysis/VALIDATION_REPORT.md`。

状态文件的 `subsystems` 中有早期概括尚未逐项刷新；判断最新细节时，应结合最新 `milestones`、`nextPriority` 和专项审计，不能只看早期 pending 或旧检查数。

## 7. 下一步从哪里开始

1. 先读最新状态、`GLOBAL_ITEM_SLOT_AUDIT.json` 和 `OutgameGlobalItemSlot.cs`，核对当前工程；不用重做礼包或单例 getter。
2. 已确认 getter34590先创建并发布单例，再current ItemConfigMgr.InitMgr，最后返回current槽；失败保留已发布对象、不自动重试；回调可以清空/替换，旧实例释放也会清当前槽。这些已有10项检查覆盖。
3. ctor34570仅初始化统计id10020、实时Products32/Items36；快照40/44保持null直到记录初始化。不能重新提前创建快照，也不能把配置初始化塞入ItemConfigMgr自身getter。
4. 下一步把现有商品Update/Reset/Prices接到共享GameProductConfig及原旧资源reader，再将slot/ItemManager/实体/数据池注册接入完整生产启动。Slot当前接受显式lifecycle构造器，测试的update注册是受控边界，不能当完整native产品更新。
5. 继续余下宿主/24控制器/Main/账号/菜单/全部业务；完成实际操作、保存重启、失败回调及原生Player验收。最终目标没有缩小。

常用验证：Unity 可执行文件为 `C:/Program Files/Unity/Hub/Editor/6000.0.68f1/Editor/Unity.exe`；使用参数 `-batchmode -accept-apiupdate -projectPath E:/Projects/AreaBattle/UnityProject -executeMethod AreaBattle.EditorTools.BattleBuild.ValidateMechanicsOnly -logFile E:/Projects/AreaBattle/analysis/<本轮名称>.log`。从 PowerShell 启动时使用 `Start-Process -WindowStyle Hidden -PassThru` 并跟踪自己的 PID，不结束其他 Unity 进程。

## 8. Git 上传与暂停状态

完整复原目标已按用户要求暂停；本次仅做仓库上传准备，不继续改玩法代码。远端为 `https://github.com/memeoo123/AreaBattle.git`（SSH 地址同库；当前机器采用 HTTPS 认证）。`.gitignore` 排除 Unity 缓存、Build、测试临时工作目录与本地存档；源码、资源及 .meta、Packages、ProjectSettings、证据和状态文档保留。原始参考视频使用 Git LFS；Spine 参考代码使用固定提交子模块。克隆后运行 `git lfs pull` 和 `git submodule update --init --recursive`。

## 9. 接续提示

> 请读取 AREA_BATTLE_HANDOFF.md，继续同一版本的完整关外复原。先核对当前工程与最新审计，从商品更新/价格供应者、旧资源读取器及数据池/Main对象装配开始；保留已有战斗和用户数据，不启动子代理，不编造未知规则或登录结果，不把局部检查通过当作完整验收。

此前较长交接文档已按原字节归档：`analysis/handoff-history/AREA_BATTLE_HANDOFF-20260930-192957.md`，SHA256：`1cb05c730af6bf5715eca97fbdc6f703ca350121a1c927e1f1fb08f4842e7811`。它仅供查历史；当前入口以本文为准。
