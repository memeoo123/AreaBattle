# AreaBattle：第一例，2026-10-09 盘点基线

这是项目专属实例，不是跨项目规则。路径均相对 AreaBattle 项目根目录；使用时从当前工作区定位，别假定其他机器存在 E 盘路径。

## 本轮范围与进度

用户先要求关内美术规划，随后将第一个目标明确为创建美术风格替换 skill。已试做紧凑四塔、地面和墙材质并接入试玩；普通兵仅生成单图概念，仍使用原动画。2026-10-09用户指出缺少先选风格的环节，并要求按强制阶段流程重新开始。当前处于 style-selection，style_decision=pending；既有资源为历史试验，不是已选整体风格。以项目最新交接与 art-style/style-restart/STYLE_BRIEF.md 为准。

清爽卡通城堡、玩具沙盘、精致手绘地图都是讨论候选，**不是用户已确认方向**。下一次实际试用可以先沿用户选择做样板；若请求只是盘点，则输出清单即止。

风格重选仍保留已确认约束：塔体紧凑，分流塔不能用并排副塔扩宽；普通塔比进阶塔朴素；四塔靠造型区分而非路线名称/徽章；塔顶线路容量和出线状态圆点是玩法信息，必须保留。判断遮挡应测非透明可见轮廓并在游戏分辨率检查，不能只比较贴图画布尺寸。

先读 `AREA_BATTLE_HANDOFF.md` 和 `DEVELOPMENT_PROGRESS.md`，检查实际 Git 状态。本次基线为 `codex/project-changes` / `14850a10`；交接文档及 `analysis/handoff-history/` 中的历史归档是用户需要保留的本地改动。不要切回旧还原流程，不启动子代理；新会话以用户最新指令为准。

## 已核实的渲染链

| 对象 | 证据路径（UnityProject/Assets/AreaBattle/ 下） | 当前实现及换美术含义 |
|---|---|---|
| 普通塔/箭塔 | Scripts/BattleView.cs 的 RefreshPresentation | Recovered/Towers 下的 Sprite 按族、级别、阵营加载；可替换图片并校准锚点/尺寸 |
| 路线标记 | Scripts/AdvancementVisual.cs | 动态平面 Mesh，多边形图标；不是完整新塔美术 |
| 普通士兵 | Scripts/RecoveredSoldierVisual.cs；Shaders/RecoveredSoldier.shader；Resources/Recovered/Soldiers/soldier_100.prefab | MeshFilter/MeshRenderer，Shader 读取动画纹理得到二维位置，固定深度；跑动/死亡由时间参数控制，不是立体模型或实时 Spine |
| 士兵资源导入 | Editor/RecoveredSoldierImporter.cs | 存在恢复资源导入逻辑；尚未证明具有新角色重烘焙所需的完整源资产与工具 |
| Boss/召唤兵 | Scripts/RecoveredBossVisual.cs | SkeletonAnimation 实时 Spine；Boss召唤兵类型11/12走此路径 |
| 兜底士兵 | Scripts/BattleView.cs | 无对应Prefab时可创建球体；这是兜底，不能据此描述常规士兵为3D |
| 背景/连线 | Scripts/BattleView.cs | 从 Recovered/Background 与 Recovered/WayLines 加载Prefab，需要继续检查内部组件后再定替换规格 |

`analysis/captures/campaign-level30.png` 是已查看的历史截图，可用于风格比较基线，不能证明后续版本的实际表现。其他截图按需读取。

## 本项目必须保留的玩法语义

- 基础、突击、分流、箭塔四类视觉，需要同时识别阵营与类型。
- 分流每条线路独立产兵，不能因美术或连线重构改成平分总量。
- 同一所有者记住路线；突击/分流低于10表现为基础塔，回到10恢复路线。
- 箭塔低于10仍保持箭塔和基础射击能力，射程随等级增长；范围圈必须跟随真实值。
- 换所有者才清空路线记忆。正式关卡布局、障碍、Boss机制及用户真实存档保留。

## 首个可玩样板建议

背景局部、四类塔、一种普通士兵、连线/拖线/射程圈、兵力标签与进阶面板。具体关卡从现有项目选择，验证密集连线、多阵营和障碍；静态关卡30截图不能替代全部状态测试。Boss列入后续批量，不宣称首样覆盖Boss。

在该样板验证：普通士兵运动/死亡与暂停，10点进阶，分流两路/三路，箭塔低于10，等级增大后的真实射程，失守换阵营，多塔数字遮挡。新资源与旧规则入口的共享依赖在写入前检查。

只修改 skill 或规划时不启动 Unity、不重跑构建。实际接入时先检查现有 Unity 实例；编辑器位置从项目/环境重新发现。`Editor/AdvancementValidation.cs` 是已有验证入口，须阅读当前实现后决定适用范围，不把历史160项通过当作新美术验收。

## 验证边界

上表记录最初盘点链，后续试玩新增 CompactTowerVisual、CompactEnvironment 与相应材质，详见项目当前代码。历史环境版163项通过仅适用于旧试验，不表示新风格已通过，也不是性能验收。新士兵动画制作仍未验证。
