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
