# 冲向那座塔：逆向分析报告

目标 `wxcf1394487200e48f / 43`，Unity `2021.3.56f2`。已完成本地提取、3 个代表布局下载与解析，并开始追踪原始 WASM 方法体。尚未达到可玩还原门槛，`implementationReady=false`。

## 本轮新增结果

用户回复“允许”后，仅下载已列明的 level_0、level_1、level_2，合计 4,510 字节。三个请求均 HTTP 200，字节数及 MD5 与资源清单一致，另登记 SHA-256。下载证据：`evidence/level-downloads.json`；TextAsset 的包名、PathID 和哈希见 `evidence/level-extraction.json`。

| 布局资源 ID | 塔数量 | 各阵营初始 StartScore | 障碍物 |
|---|---:|---|---:|
| 0 | 4 | Camp 0: 3、3、3；Camp 1: 10 | 0 |
| 1 | 4 | Camp 0: 5、5；Camp 1: 15；Camp 2: 5 | 0 |
| 2 | 3 | Camp 1: 55、65；Camp 2: 10 | 0 |

[已确认] 三个文件均含 CampInfoCfgs、StarInfoCfgs、ObstacleInfoCfgs；塔位使用原始 XYZ 数据且 Y 均为 0。`generated/levels/` 保存原始 JSON 和格式化版本，未归一化坐标，未补造出兵连线。

[已确认] 阵营、ShipID、AIGrade 和 EnityID 在对应本地表中都能找到引用。详细校验：`evidence/level-validation.json`。

[待确认] 关卡 2 的 `DispathchID=99` 不在 DispatchConfig 中。当前 StarInfoCfg 元数据也未声明这个拼写的字段，不能直接据此断定数据错误；需验证该字段是否属于旧版导出内容及当前序列化行为。布局还包含其他未见于该类型声明的碰撞字段，原样保留。

## 已追到原始 WASM 函数

[已确认] 通过静态数据段中的 Assembly-CSharp CodeGenModule、元数据 method token 和 WASM element table，映射了 3,493 个玩法方法，其中 3,445 个存在本地函数体。主模块与次模块的表覆盖关系已记录。映射证据：`generated/gameplay-method-map.json`，包含元数据偏移、内存指针地址、表索引、模块、函数号和函数体字节区间。

这修正了首轮“只有符号，未定位方法体”的状态。已有 14 个核心方法的指令级反汇编，保存在 `generated/disassembly/`。源游戏代码未在宿主中运行。

### 基础出兵参数

[已确认：静态代码] BattleControl.OnInit 选择 DispatchConfig 的 ID 1、2、3。GetSpawnTime 按档位与连线数读取 swanpSpaceOne/Two/Three，执行 float32 转换并除以 1000；GetDispatchAddScoreTime 对 addSpace 执行相同换算。

| 档位索引 | 配置 ID | scoreLimit | maxLine | 一条线 | 两条线 | 三条线 | addSpace / 1000 |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 0 | 1 | 9 | 1 | 1.111 | 1.282 | 1.515 | 2.0 |
| 1 | 2 | 29 | 2 | 1.032 | 1.185 | 1.391 | 2.0 |
| 2 | 3 | 65 | 3 | 0.961 | 1.098 | 1.282 | 2.0 |

这些是函数基础返回值，表中列出的三种连线输入不表示每档实际都允许三条线；允许数量见 maxLine。最终出兵节奏的调用方式、时间尺度、技能与 Buff 修正尚未全部核验。

证据：GetSpawnTime 位于主 WASM 文件偏移 6,269,135，函数体长 197 字节；除法指令位于 0x005fa98b–0x005fa991。`generated/dispatch-model.json` 保留明确规则与局限；`dispatch-derived-cases.json` 的三个数值用例来自静态代码，不是原游戏运行验证过的 golden cases。

[已确认：局部路径] Tower.SoldierCollision 的一段常规分支对异阵营兵种值取负，对同阵营读取支援值，再经虚调用更新塔并清理士兵。完整方法还包含满塔转发等分支；当前不将这一局部观察扩展为完整战斗公式。反汇编位置见 `Tower-SoldierCollision.txt` 的 0x00725747–0x0072578e。

## 已保留的基础成果

- 3 个 wxapkg 解包为 27 文件；源包与副本哈希一致。
- 两个 WASM 解压后为 13,867,396 和 23,108,695 字节，均通过 WebAssembly 格式校验。
- 目标临时缓存恢复 Unity WebData 15,103,186 字节，提取 IL2CPP v31 元数据。
- 121 个缓存 Unity 资源包盘点 6,244 对象；主 data.unity3d 另盘点 4,109 对象，未把两者声称为去重总数。
- 整理 83 份 Config JSON：76 张记录表与 7 个单例配置对象。
- LevelConfig 和 LevelBConfig 各 561 行；当前运行表的选择尚未确认，不能合称 1,122 个不同关卡。
- SoldierConfig 8 行、AIConfig 212 行、EntityModelConfig 155 行。
- 资源清单有 2,040 条，其中布局 639 个；目前已恢复指定的 3 个布局。
- 兵种 EntityID 501、650、601 仍无法直接匹配 EntityModelConfig，需追踪皮肤/模型映射。

首轮报告归档为 REVERSE_REPORT.initial.md；本报告及 analysis-status.json 为当前状态。

## 还原门槛与后续

1. 继续追踪 Tower.ChangeScore、SoldierCollision、Buff 和满塔转发，恢复完整占领与战斗路径。
2. 核对坐标变换、初始玩家阵营、LevelConfig/LevelBConfig 的选择与当前实际关卡画面。
3. 校验至少三组原游戏数值结果及胜负状态，之后生成正式 RESTORE_SPEC 与 golden cases。

没有创建可玩工程，没有声称完成完整还原。当前的精确布局、配置与方法字节位置均已落盘，可直接续做。
