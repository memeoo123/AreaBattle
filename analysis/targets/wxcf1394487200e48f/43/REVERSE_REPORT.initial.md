# 冲向那座塔：本地逆向分析报告

## 目标与范围

- [已确认] 当前运行进程 AppID：`wxcf1394487200e48f`，本地缓存版本 `43`。微信小游戏窗口标题为“冲向那座塔”。定位记录：`evidence/locator-handoff.json`。
- 授权依据：用户要求“用这个skill ，执行一下当前运行的微信小游戏”。仅处理该目标本地包、资源缓存及 Unity 数据；源文件保留。
- 当前结果：包解密/解包、引擎识别、加载路径分析、配置表恢复和 IL2CPP 符号提取已完成；精确玩法还原未完成。

## 已确认结果

| 项目 | 结果 | 证据 |
|---|---|---|
| 引擎 | Unity 2021.3.56f2，竖屏 | `generated/modules/unity-namespace.js:16`；`app-config.json:1`；`evidence/webdata-unpack.json` 中 data.unity3d 的 UnityFS 版本头 |
| 微信插件 | UnityPlugin 1.2.83；与引擎版本分开记录 | `work/unpacked/__WITHOUT_MULTI_PLUGINCODE__/app-config.json:1` |
| 解包 | 3 个 wxapkg，共 27 个文件 | `evidence/unpack-*.json`，包含源包与各条目的 SHA-256、偏移和大小 |
| WASM | 主分片 13,867,396 字节，次分片 23,108,695 字节，均通过格式校验 | `evidence/wasm-decode.json`；未执行 WASM |
| Unity 数据包 | 从目标 temp 文件恢复 15,103,186 字节，和加载入口声明一致 | `evidence/temp-data-probe.json`；`generated/modules/game.js:20` |
| 元数据 | IL2CPP v31；11,003 类型、75,896 方法、46,852 字段；提取 420 个 Proj_hdzd 类型 | `generated/gameplay-symbols.json`，含二进制偏移、方法 token 与字段引用 |
| 本地资源 | 122 个缓存文件；121 个 Unity 资源包解析成功，盘点 6,244 个对象 | `evidence/cache-inventory.json`、`evidence/asset-inventory.json` |
| 配置 | 导出 89 个 TextAsset；整理 83 份 Config JSON，包含 76 张记录表和 7 个单例配置对象 | `generated/table-schemas.json`，含来源包、PathID、哈希、字段类型与样例 |
| 资源清单 | 2,040 条资源映射；639 个关卡布局文件，本地布局命中 0 | `generated/resource-catalog.json` |

Unity 数据包中的 `data.unity3d` 另盘点 4,109 个对象，详见 `evidence/data-unity3d-inventory.json`；此数量不与缓存对象数量混为去重总数。

## 核心配置

| 表 | 行数 | 已确认内容 |
|---|---:|---|
| LevelConfig | 561 | 关卡 ID、LevelType、SceneId 至 SceneId10 的映射 |
| LevelBConfig | 561 | 另一组映射；当前运行采用哪组仍待追踪，不能据此宣称有 1,122 个不同关卡 |
| SoldierConfig | 8 | EntityID、shipType、hp、attack、occupy、reinforce、voyage、param1/2 |
| AIConfig | 212 | AI 参数记录；字段含义与使用方式需要结合方法体确认 |
| CampConfig | 10 | 阵营 ID、颜色、模型参数 |
| EntityModelConfig | 155 | 8 个兵种中 5 条 EntityID 可直接匹配；501、650、601 未直接匹配，需追踪皮肤/模型重映射 |
| GlobalValueConfig | 9 | 原始全局参数；尚不推断时间单位或精确计算公式 |

例如 SoldierConfig 的 id=1 记录为 hp=1、attack=1、occupy=1、reinforce=1、voyage=5；id=2 为 2、2、1、2、5。这些是配置值，不是已验证的完整伤害公式。

## 玩法证据与追踪入口

[已确认：游戏内说明] LanguageConfig 的 StrategyDes 系列描述出兵连线、塔等级影响出兵速度、小兵碰撞、友军支援、攻城和箭塔。说明文本中写有每座城堡最多 3 条出兵路线。原始条目另存 `generated/gameplay-help-evidence.json`；尚未将说明文本等同于全部运行时边界行为。

[已确认：符号] `Tower` 包含 `SoldierCollision`、`ChangeScore`、`ChangeCamp`、`get_SpawnTime` 等方法；`BattleControl` 包含 `GetSpawnTime`、`GetDispatchLineNum`；`LevelControl` 包含 `InitGameByLevelConfig`、`OnLevelCfgLoad`、`OnGamePlayState`。部分字段和私有方法名已混淆。元数据提供声明，不提供方法实现。

[已确认：布局结构声明] `LevelInfoCfg` 含 `CampInfoCfgs`、`StarInfoCfgs`、`ObstacleInfoCfgs`。`StarInfoCfg` 含 pos、angle、scale、CampID、StartScore、ShipID、isBoss、bossSkillId；类型记录位于 global-metadata.dat 偏移 7,784,976，长度 88 字节。此结构不等于已获取关卡实例。

## 还原门槛与下一步

`implementationReady=false`。尚未生成可用于实现的 RESTORE_SPEC.json 或伪造数值 golden cases。

1. 补齐代表关卡布局，先取清单中的 level_0、level_1、level_2，预计合计 4,510 字节。确切文件名、清单行号、推导 URL 和目标路径已列入 `generated/next-download-plan.json`，尚未联网。
2. 将元数据 token 对应到 WASM 方法体，恢复碰撞扣值、占领、出兵间隔、升级阈值和胜负条件。
3. 对照原游戏验证代表关卡与至少三组数值结果，再输出 RESTORE_SPEC 和 golden cases。

## 验证与复现

`evidence/validation.json` 记录源包/副本哈希、27 个解包条目哈希、122 个缓存副本哈希、两个 WASM 格式校验、Unity 数据长度匹配、83 份配置 JSON 校验和兵种实体外键检查。文件完整性检查通过；兵种模型的直接外键匹配发现上述 3 个待查 ID，因此不能声明全部语义校验通过。

分析脚本位于 `analysis/`：`inspect_target.js`、`inspect_temp.js`、`inventory_assets.py`、`recover_schema.py`、`finalize_analysis.py`。工具版本及命令记录见 manifest.json。所有脚本均静态读取与导出，不启动目标游戏代码。
