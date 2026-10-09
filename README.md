# AreaBattle：冲向那座塔复原

目标：还原 wxcf1394487200e48f / 43 的完整关内机制和表现。已接入原始士兵、两种 Boss 的 Spine 动画、18 个技能、15 类障碍、连线/箭矢、塔容量标记、教程动画与手势、暂停及结算界面。代码和资源按原包证据恢复；当前仍未通过原机同场景画面、音频和时序对照，完整还原目标保持进行中。

## 当前进展与获取工程

当前工作已扩展到完整关外复原，尚未全部完成；最新状态与接续说明见 [AREA_BATTLE_HANDOFF.md](AREA_BATTLE_HANDOFF.md)，验证范围见 [analysis/VALIDATION_MANIFEST.json](analysis/VALIDATION_MANIFEST.json)。

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

## 塔进阶试验（codex/project-changes）

双击 `Play-TowerLab.cmd`（或 `Play-Advancement.cmd`）运行独立构建 `Build/Advancement/AreaBattle.exe` 的新关“进阶试验场”（布局99001）。开局9座塔全部是基础塔，蓝方3座、中立3座、红方3座；统一使用基础士兵，屏蔽原防御塔、进攻塔、箭塔。所有塔均可在归属玩家且达到10点后进阶；中立或敌塔被占领后也可以进阶。没有永久不可进阶的塔。重开全部恢复基础塔。该关不会推进普通关卡存档，本地档案位于独立构建目录。本试验不代表原游戏规则。

新关只使用“基础塔→进阶塔”的成长关系，所有位置都能成长。基础塔外观统一，选择路线后才显示单线、分流或箭塔标记。旧关卡仍保留原塔种数据，新关不使用它们。

试验关蓝方初始数值为8、5、3。所有基础塔始终只有1条进攻线；我方塔首次达到10点后强制暂停，显示当前塔的位置和三张路线卡。选中卡片后必须确认，不能跳过。多塔同时达标依次选择，期间战斗、技能和输入全部冻结。20～60点沿路线自动成长，塔旁显示收益提示，不再弹出三选一。

| 路线 | 10级能力 | 后续自动成长 |
|---|---|---|
| 单线 | 1条线，总出兵效率120% | 20级起每10级增加5个百分点，60级总效率145% |
| 分流 | 2条线，每条线独立出兵，效率100%，增加线路不减速 | 20级解锁第3条线；20级起每10级每路增加5个百分点，60级每路125% |
| 箭塔 | 不能出兵；复用原箭塔优先射敌兵、无敌兵时射敌塔且不能占领的规则 | 10级射击间隔1.3秒、射程1；每10级射速倍率增加20%，射程每10级增加0.1，60级射程1.5、间隔0.65秒，伤害不变 |

效率基准为同档基础塔的一条线；士兵生命、攻击和占领能力不变。突击和分流使用单箭头、分叉箭头标记；箭塔使用原箭塔外观。进阶随当前数值升降：低于每10点门槛撤销对应能力，突击与分流低于10点停用进阶能力并恢复基础塔外观，超出容量的出线自动断开。同一所有者保留路线记忆，重新达到10点自动恢复，不弹窗；只有所有者变更才清空路线记忆，达到10点后重新选择。重开恢复基础塔并清空记忆。箭塔在同一所有者下低于10仍保留箭塔形态、1.0射程与1.3秒基础射击间隔，不能出兵；被攻占后照常清除路线。已派出的士兵保持出发时的能力。敌方达到10点自动选择路线：受到进攻且附近有可射击敌兵时选择箭塔；否则有多个非友方相邻目标且未受进攻时偏向分流，否则选择单线。20～60点使用与玩家相同的自动成长，不触发弹窗或暂停。敌方初始数值已达门槛的塔会在首个战斗帧进阶。数值需试玩调整。普通关卡保留旧实验目录与原塔种规则。

验证与重新构建入口：`AreaBattle.EditorTools.AdvancementValidation.RunBatch`（Unity batchmode），报告 `analysis/advancement-validation.json`。包含进阶、转发、占领、重开、HUD选择检查，以及原战斗/技能/教程/箭塔回归检查。

## 进阶规则正式关卡版本

双击 `Play-Campaign.cmd` 从新版独立进度进入正式关卡，胜利后点击下一关可连续推进，再次启动自动续玩。使用原关卡顺序、布局、阵营与障碍；各非中立阵营的普通塔开局统一为5～8点基础塔，每方原配置兵力最高的一座设为8，其余交替6、5，中立塔占领成本与Boss生命值保留原值，达到10点选择突击、分流或箭塔；Boss保留原机制。旧兵种强制教程在这个入口关闭，前几关显示新规则与操作提示。新版存档为构建目录的 `EvolutionProfile.json`，旧版 `LocalProfile.json` 与试验场入口不变。

指定关卡可使用 `AreaBattle.exe -battle-evolution -battle-level 30`，此入口仍按正式关卡记录新版进度。该版本开局不触发进阶弹窗，需通过自然增长或援军培养到10点。连接出兵的塔仍不自然增长，整体难度尚需试玩调整。

新版普通敌塔AI：每0.6秒检查来袭线路，满出线时优先撤掉非对攻线路以反击；开场4.5秒后每1.5秒为每个敌方阵营安排一次扩张或支援，优先弱目标，也会支援受攻击且兵力不足10的友塔。后方保留一座低于10的成长塔，避免全部出兵导致长期无法进阶。Boss阵营仍保留原AI行为。单线与分流现已采用上表的低倍率方案，成长按百分点相加，不再乘算放大。

最新玩法进度、规则和试玩待办：[DEVELOPMENT_PROGRESS.md](DEVELOPMENT_PROGRESS.md)。
