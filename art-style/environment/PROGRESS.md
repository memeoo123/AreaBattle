# 背景、障碍与士兵评估

2026-10-09：已完成进阶模式背景与墙体材质替换，士兵仅评估。

- 背景：内置ImageGen生成 `battlefield-ground.png`，提示词 `ground-prompt.txt`。浅草地/土地，无固定道路，中央保持开放。CompactEnvironment在相机后方放置纯视觉背景，按视口覆盖；不添加碰撞。
- 障碍：15种原墙体继续使用原网格、Prefab组合与变换，运行时仅换 WarmStoneWall 材质，暖色石墙配色与塔统一；不是重建障碍或增加树石。
- 生效范围：BasicTowerExperiment/进阶正式关卡与试验场。旧规则入口继续原美术。已恢复的容量圆点保持显示。
- 新代码：Scripts/CompactEnvironment.cs、Shaders/WarmStoneWall.shader、Editor/CompactEnvironmentBuild.cs。BattleView负责分流新旧视觉。
- 验证：`environment-build.log` 中 `ADVANCEMENT_PASS checks=163 build=Succeeded`。覆盖15种障碍无新增Collider、矩阵/尺寸/网格不变，材质受支持，背景纯视觉与竖屏覆盖，以及原161项回归。
- 实际截图：`analysis/captures/compact-environment-layout104.png`（正式关116，对应布局104）；`tower-lab-labels-battle.png`含出兵、数字与圆点。已查看；旧HUD与普通士兵仍是原风格。
- 首轮修复：背景适配在编辑器渲染时更新不稳定，加入ExecuteAlways与显式视口同步；初版墙偏暗，调为浅暖石色。第二轮通过并完成构建。
- 入口仍为 Play-Campaign.cmd / Play-TowerLab.cmd。未改用户真实存档，未提交推送。

士兵方案见 `SOLDIER_RESTYLE_PLAN.md`。建议先一个普通小兵分层样板，验证美术尺寸、跑动/死亡/暂停与性能，用户尚未确认替换动画路线。本轮没有生成新士兵或改变现有动画。

integration目录是文件写入暂存副本。后续以UnityProject实际代码为准，不回拷旧暂存覆盖新改动。
