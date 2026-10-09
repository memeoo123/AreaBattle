# AreaBattle restoration progress

Updated 2026-09-28T10:36:21.347857+00:00. Target: 冲向那座塔 / wxcf1394487200e48f / 43.

The active goal is complete in-level mechanics and presentation. Overall status: incomplete.

## Verified implementation and runtime fixtures

- Editable Unity project: `UnityProject/`, Unity 6000.0.68f1. Original editor was 2021.3.56f2; migration tested by actual compile/build.
- Windows executable: `Build/Windows/AreaBattle.exe` (requires its adjacent Data/runtime files).
- 42 production mechanics cases pass, including 18 direct original WASM numeric oracle cases.
- 262 integrated cases pass across ordinary, arrow, all nine tutorial entrances, all-layout data/obstacle import, level mapping, local progression, 18 active skills, Boss998/999 and PVP.
- 600-frame actual Windows binary smoke passes, with spawning before/after retry and identical topology.
- Ordinary layout5 and fixtures cover production, AI, damage, capture, forwarding, opposing contacts, pause, victory, defeat and retry.
- All 639 layout bundles verified against the source catalog; 5,752 towers and 2,862 obstacles recovered.

## Remaining full-goal work

- Verify source-derived runtime against the original session, including special modes and asynchronous callback timing; source-proven skill2/12/15 retry fixtures already pass.
- All 18 skills have production input traversal fixtures. Boss Spines, native status/skill particles, guide animation/hand, capacity marks and audio lifecycle are restored; original timing comparison remains open.
- Original default soldiers, Bosses, waylines, all 15 obstacle entities, UGUI and result animations use recovered assets. Async resource-load races and original RNG alignment remain outside proven coverage.
- User confirmed the level871 reference is entry without input. Its recovered layout120, seven towers, startup AI topology, blue highlights, background, UI and commander are now compared in `generated/original871-visual-comparison.json`. Full timed replay remains pending; desktop screenshot helper is still unavailable.

Evidence: `analysis/unity-integrated-validation.json`, `analysis/player-smoke.json`, `analysis/unity-build-report.json`, `analysis/captures`, and `analysis/targets/wxcf1394487200e48f/43/generated/INLEVEL_RESOURCE_AUDIT.md`.


## Original recording validation — 2026-09-28

Preserved the supplied90.52-second,720x1334 original recording and SHA256. Native PTS locate ice36.08522s, fire52.37694s, lightning62.29858s; generic stock27→26→25→24. Adjacent lightning frames show green45→0 without capture. Ice active-state duration supports level10; lightning10 is a strong isolated-cast candidate; fire remains unknown (data1 counts projectiles, every level hits for2). Source-confirmed SkillUI closes its commander immediately and fades graphics with0.3s OutQuad; production and retry reset corrected. 259 integrated checks, build0errors/0warnings and fresh600-frame smoke pass. Recording starts already paused, so initial clocks/RNG and full matching remain open. See `analysis/VIDEO_VALIDATION_REPORT.md` and `generated/video-20260928/original-video-evidence.json`.


## Video fire and input refinement

2026-09-28T10:36:21.746070+00:00. Fire level previously unknown is now a strong level10 inference:25 distinct heads visually reviewed from24 detector runs (first run contains two), with config210 and25 production launches at recorded frame steps. Seven committed line operations identified through filled capacity circles, separated from drag previews. All259 integrated checks pass; fresh600-frame smoke; build0errors/0warnings. No ordinary save modified. This does not establish original RNG, pre-recording clocks or full matched replay. Evidence:`generated/video-20260928/fire-and-input-evidence.json`.


## 按录像时间轴的连续回放诊断

已用生产 BattleView、实际物理连线拓扑和原版配置，执行前 62.32 秒内已确认的 7 次连线操作与 3 次技能。10 次输入均接受，7 个检查点共 49 个塔阵营全部相同；塔数值仅 27/49 项相同，最大差值 8。这是一份**未匹配的诊断结果**，不是整局验收通过。

原片蓝塔 19→20 首帧为 1.26736 秒（上一帧 1.25060），20→21 为 3.26884 秒（上一帧 3.25180）。结合初始 15、每两秒增兵以及约 0.806 秒恢复暂停，条件性推断录像前运行时间为 9.53864–9.55540 秒；本次用中点 9.54702 秒。该推断依赖此前无蓝塔操作，且尚有画面延迟。

先前使用 9 秒假设的基线结果也完整保留。校准增兵相位后并未让整局全部吻合，因此没有选择“误差更小”的时长或搜索随机种子，也没有修改正常存档、生产机制或原始配置。

塔编号顺序：左上、右上、中左、外左、中右、外右、底部。

| 原片秒数 | 原版数值 | 回放数值 | 最大差值 |
|---|---|---|---|
| 1.017 | 19, 19, 5, 5, 9, 5, 65 | 19, 19, 5, 4, 9, 5, 64 | 1 |
| 35.384 | 30, 32, 5, 5, 3, 5, 54 | 30, 32, 5, 4, 5, 5, 52 | 2 |
| 36.385 | 30, 32, 5, 5, 5, 5, 52 | 30, 32, 5, 4, 8, 5, 52 | 3 |
| 52.010 | 44, 32, 5, 5, 43, 5, 52 | 45, 32, 5, 4, 47, 5, 52 | 4 |
| 61.048 | 57, 38, 0, 0, 65, 0, 46 | 55, 46, 0, 0, 65, 3, 48 | 8 |
| 62.282 | 58, 38, 0, 0, 65, 0, 45 | 56, 46, 0, 0, 65, 3, 48 | 8 |
| 62.299 | 58, 38, 0, 0, 65, 0, 0 | 56, 46, 0, 0, 65, 3, 3 | 8 |

第一个检查点（1.017 秒）已经有两座塔相差 1，早于 4.103 秒的第一次已知玩家操作。原版未知的已有士兵、AI 历史、兵线时钟和随机状态仍需进一步区分；不能用这份差异表单独认定连线、冰冻或伤害公式错误。

可重复执行：`AreaBattle.EditorTools.OriginalVideoReplayDiagnostic.Run`；输入与结果在 `generated/video-20260928/timed-replay-*.json`，增兵像素测量脚本为 `analysis/video_regen_measure.py`。范围止于雷击之后，尚未完成后半段玩家输入与胜利全过程同步。


## 完整录像操作回放（未匹配）

后半段新增 15 次已提交连线变化，合计 22 次连线变化与 3 次技能；切线滑动中的多个变化可能属于同一个手势。提交时刻由相邻原片帧的箭头、预览消失和容量圆点交叉确认。已识别并排除兵线穿过容量圆点区域导致的像素计数误判。

生产场景沿录像时间轴运行至末尾，自然胜利发生在 **86.745 秒**，原片为 **86.044 秒**，晚约 **0.701 秒**。没有强行改分、修改阵营或挑选随机种子来实现结算。

但这仍未通过同步验收：25 次输入只接受 24 次；75.639 秒的 6→7 连线在原片有效，回放当时 6 号塔仍属红方，因而正常拒绝。后续 81.942 秒的 6→4 虽接受，容量也因此比原片少 1。

12 个采样时刻，阵营 84/84 项一致、数值 38/84 项一致。采样阵营相同不代表两帧之间的占领时刻相同，上述拒绝正是反例；胜利时间接近也不能视作完整机制一致。

| 后半段原片秒数 | 操作 |
|---|---|
| 64.48295 | 切断 2 → 5 |
| 64.79981 | 连接 5 → 3 |
| 65.86681 | 连接 5 → 7 |
| 67.03434 | 连接 5 → 6 |
| 73.65431 | 连接 2 → 5 |
| 74.92175 | 连接 2 → 6 |
| 75.63934 | 连接 6 → 7 |
| 76.57274 | 连接 3 → 4 |
| 77.50691 | 连接 7 → 4 |
| 78.74020 | 连接 3 → 7 |
| 79.65754 | 切断 5 → 7 |
| 79.70780 | 切断 3 → 7 |
| 79.77415 | 切断 3 → 4 |
| 81.94212 | 连接 6 → 4 |
| 83.72654 | 连接 3 → 4 |

首个检查点的三组敌方对向兵线（3↔7、4↔7、6↔7）与原片一致，士兵位置和塔数值仍有差异。已导出回放的兵线时钟、士兵位置与画面，便于继续追查。

皮肤差异已进一步处理：录像对照现显式选择原始 soldier102 的尖帽、星形法杖网格、图集与动画。这个匹配来自画面和资产对照，不是读取原版账号；各阵营的实际存档选择仍未确认。默认通用皮肤选项保留。

本次未更改生产战斗参数或正常存档。执行 `AreaBattle.EditorTools.OriginalVideoReplayDiagnostic.Run` 现在会读取 `full-replay-fixture.json` 与 `full-input-trace.json`；前一轮 62 秒诊断 JSON 保留为历史记录。


## 录像普通兵皮肤修正

已从缓存原件导入 `soldier_102`：SkinConfig102→prefab1002，skin04 图集含尖帽和星形法杖，94 顶点原始网格，原始 RGBA32 GPU 动画（run 从266帧起、37帧、0.6秒）。原始包及导出数据保持不变。

新增显式外观选项 `-battle-skin 102`；`Play-Level871.cmd` 用此选项打开第871关。只改变蓝、红两方普通 type1 士兵的显示；绿色方保留原片可见的圆头 soldier100，其他兵种与正常存档不变。录像诊断使用此选项；用户/敌方账号皮肤选择仍是画面推断。

更换前后完整回放 JSON 完全一致，新增生产场景测试验证实际生成士兵使用 soldier102 网格、攻击/占领/增援值不变且重试保留选项。当前 259 项集成检查通过。后续仍需解决初始士兵、占领时机及画面时序差异，不能将外观修正视为整局验收。


## AI 回应及占领时刻追查

新增完整模拟事件时间轴（出兵、碰撞、抵达、分数、占领），与原片像素变化对照。

| 事件 | 原片首个可见帧 | 还原事件 | 差值 |
|---|---|---|---|
| 红塔5→2回连 | 4.11854 | 4.26918 | +0.151秒 |
| 红塔5→1回连 | 6.11963 | 6.26981 | +0.150秒 |
| 右下6号塔变蓝 | 73.63778 | 76.64023 | +3.002秒 |

两次 AI 回应均约晚0.15秒，间隔与原片相同，值得继续核对计时相位；尚不能归因为兵速或碰撞公式。右下塔变蓝约晚3秒，明确导致75.639秒玩家连线被拒绝。未为了配录像调整原始数值，也未搜索随机种子。

纠正上一轮皮肤范围：原片5.520秒清晰帧显示绿色兵仍是圆头，102尖帽皮肤现在只应用于蓝、红阵营。生产场景验证同时生成蓝方102与绿方100，完整数值回放仍与更换皮肤前一致。后续仍需分离原版随机火球落点、开局士兵与AI计时状态。


## 原始 AI 计时函数直接执行验证

直接执行未改动的原始 WASM `AICamp.Updata`（函数21038，offset9492148，149字节），通过独立内存与明确的配置/动作桩隔离运行；不执行原始初始化、网络或账号代码。配置桩只返回指定间隔，动作桩只计数，因此此验证覆盖计时逻辑，不覆盖随机选择和AI策略。

9组原始结果与生产 BattleSimulation 调度器逐项一致：正延迟、延迟跨零、延迟恰好归零、行动计时恰好归零、首个运行帧、负延迟后的帧、剩余计时、大帧只执行一次、负计时遇到零dt。原始边界是延迟>0则扣减后直接返回，行动计时<0才执行，保留剩余量，每帧最多一次。

第871关 AI99 的原始启动延迟为400毫秒，行动间隔从[200,400]毫秒随机取值，不能理解成固定0.2秒。这排除了已测试的计时公式/边界错误，尚不能解释原片与固定种子回放的初始相位差。生产AI逻辑未被人为提前0.15秒。

目前 259 项集成检查通过，包含这9项新加入的原始函数对照。完整录像仍未匹配；后续应检查初始会话状态、随机调用顺序及火球目标，不应凭录像误差调整原始计时公式。


## 火球落点导致的右上塔数值差异

右上蓝塔2在火球期间无入兵，已有出兵线使其不自行增兵。逐帧数字匹配得到原片三次各加2：

| 原片首个可见帧 | 数值 |
|---|---|
| 57.07979秒 | 32→34 |
| 57.49640秒 | 34→36 |
| 57.91323秒 | 36→38 |

还原回放在53.827、54.027、54.845、55.278、56.529、57.163、58.213秒共发生7次加2，32→46；原片32→38。4次额外治疗恰好解释该塔的8点误差，不需要修改单次治疗量。数字被后续绿色治疗特效遮挡的帧标记为未知，没有当作0或新增治疗。

源码确认火球从活跃塔列表经共享RandomHelper选取；此外原版入关的敌方皮肤初始化有9次随机选择，而本地录像对照使用显式皮肤组合，未复现原始账号/会话随机状态。即使随意给相同种子，也不能据此要求落点逐个相同。本轮未为拟合录像增加虚构随机调用或更换种子。

结论只解释右上塔8点差异。25发火球完整目标序列及右下塔占领偏晚的分项原因仍未全部确认；生产伤害、治疗、随机选择代码均未改动，因此继续沿用已通过的259项检查与构建结果。


## 右下塔火球、回血与占领差异

原片右下塔6在火球期间可见5→3→1→0，本地仅一次扣2，5→3。首个原片5→3位于57.11237～57.12886秒。后两次变化受特效遮挡，只确认数字顺序；零值存在截断，不能据此认定总共恰好三发命中。

原片回血0→1发生在63.61593～63.63302秒，1→2发生在65.61685～65.63439秒，相隔2.00137秒。本地3→4在66.06693秒，4→5在68.06817秒，相隔2.00124秒。两边回血周期一致，但首个回血相位尚未对齐。

后续原片2→1发生在69.23584～69.25234秒；本地68.36832秒已有一次入兵，69.25234秒是第二次。原片数字降零在73.62087秒先可见，蓝色塔顶在73.63778秒先可见，本地占领76.64023秒，按塔顶颜色相差3.00245秒。数字和换色属于不同画面事件，不能把两者当作同一个精确时刻。

已确认火球结束后3点差异、相同回血周期，以及不同入兵历史；尚未证明3点差异能完整解释3秒延迟。没有修改移速、回血、伤害或随机种子来拟合原片。下一步需要核对首次回血相位及双向兵线碰撞历史。此次只新增分析脚本和证据，沿用既有259项集成检查及600帧烟雾测试；没有重新执行或新增生产测试，也没有通过完整画面验收。


## 右下塔首次回血延迟分解

原片红色出兵圆点在62.88203～62.89878秒消失，接触表确认此时底部塔仍为绿色。本地只读追踪显示65.26664秒才撤去右下塔出兵线，晚2.36786秒。首次回血分别为63.63302与66.06693秒，晚2.43391秒；其中撤线后的等待差仅0.06605秒。原版画面事件和本地模拟事件有显示时差，因此不把最后几帧差异当作公式错误。

本地撤线前回血累计为1.18333秒，出兵期间保持不变，撤线后继续累计至2秒。首次回血延迟主要来自战场/AI撤线时机。底部塔被占领并非立即触发回血的条件；原片在底部塔仍绿时就已撤线。AI保护分支会先撤线，再按敌方入线重新连接，受来源塔洗牌和当时入线影响；本轮未恢复原片的随机状态。

只改了编辑器诊断器，新增逐帧输出，未改生产逻辑。Unity批处理回放成功退出0，12个检查点及完整数值报告与既有基线逐项相同。既有259项机制检查和600帧测试作为此前结果保留，本轮未声称重跑；未修改伤害、速度、回血或随机种子。完整战场验收仍未通过。


## AI 撤线分支验证（262 项检查通过）

重新核对原始 AIGoToAction 的0x6318d1～0x631bda：保护分支先撤出线；入线列表为空则退出整轮行动；否则按敌方入线重新连接。新增三个受控顺序场景，验证前序塔使后续塔暂不处理、右下塔先处理后续上原回血进度、有敌方入线则重新接线并阻止回血。三项均通过，完整集成检查262项通过，Unity退出码0。

本轮只新增编辑器回归验证，未改生产AI、随机数或回血逻辑。源码与实现的该分支一致，不能为追平录像将return改成continue。该验证解释一种导致撤线延迟的原版机制，不代表恢复了原片随机顺序，也未消除完整录像数值和视觉差异。此前Windows构建与600帧烟雾测试结果保留，本轮未重跑。


## 12 个录像检查点的视觉对照

新增 generated/video-20260928/visual-review.html：12个同录像时刻的原片与720×1280回放图，并排/叠加查看，原片去除顶部54像素微信栏，附每塔数值。另存胜利后约0.7秒和回放结束帧。Unity完整回放退出0，数值报告与既有基线逐项相同；页面脚本通过Node语法检查，12张图尺寸核对通过。

确认未解决的视觉差异：36.385秒，本地冰晶有大片白色高光，原片更透明、塔身更清晰。已核对原始GLSL的Fresnel/颜色/透明度公式、材质HDR参数与混合状态，以及0.5秒生长曲线；目前未确定根因。未通过人为调暗材质来拟合。下一步检查实际材质属性块和粒子顶点数据。

结算使用恢复的基础动画与下一关入口；原片排行榜、账号货币和广告奖励不在本次范围内，因此简化结算不可声明为完整原版画面匹配。本轮只扩展编辑器截图诊断；生产代码、材质和Shader未改。262项机制检查及Windows构建/600帧测试为此前通过结果，本轮未重跑。视觉验收仍未通过。


## 修复原始 GameCamera 的 HDR/MSAA 设置遗漏

原始GameObject8为GameCamera，Camera46的m_HDR=false、m_AllowMSAA=false；BattleView此前新建相机未设置这两项。现显式恢复源设置，未修改冰晶Shader、材质颜色、透明度或伤害。UI相机本来已关闭两项。

36.385秒固定右下塔冰晶区域[534,775,638,898]：接近纯白像素（RGB各通道均>248）原片10，修复前2883，修复后177；该区域RGB平均绝对误差25.599→17.474。区域同时包含塔身、背景和粒子，仅用于局部前后对比，不是全局视觉验收。见 generated/video-20260928/ice-camera-fix-comparison.png 和 camera-hdr-fix-audit.json。

完整录像数值报告与既有基线逐项一致。重新执行262项集成检查全部通过；Windows构建成功0错误、5条既有Spine废弃API警告；新二进制600帧烟雾测试通过，重试前后均正常出兵。资源审计240文件、1363 GUID引用通过。边缘、粒子、完整随机会话和其它未验证关内路径仍待核对，目标继续保持未完成。


## 离屏文字模糊修复与背景视口分支审计（2026-09-29）

已复现截图诊断器的静态Text缓存错误：连接720×1280 RenderTexture前生成的标题/道具点字形为12/10像素，切换后CanvasScaler已经正确但旧网格仍被放大；仅再次调用CanvasScaler.Handle不能修复。截图前SetAllDirty并ForceUpdateCanvases后生成32/26像素字形，原字体、字号和生产HUD均不变。实验见generated/video-20260928/capture-audit/report.json；修复在OriginalVideoReplayDiagnostic.Capture，12检查点已重新输出，旧图保存在before-capture-refresh。

背景f7819（0x391440～0x391490）按精确0.5625分支；较宽视口采用2*2.1*.5625/2.5875000953674316倍率。已有入口截图723×1282注册来自original871-visual-comparison.json。额外生成12张entry-aspect-candidate候选，标准720×1280回放保留。35.384秒上方空背景RGB MAE10.0073→2.8781，左侧装饰39.5625→6.1231；这支持宽屏分支假设，但不证明录像内部Framebuffer尺寸，不能据此改生产相机常数。对照页新增视口选择器。

完整数值报告与full-replay-before-skin102.json逐项一致。重新运行262项集成检查全部通过，资源完整性240文件/1363 GUID引用通过，编辑器编译及回放退出0，网页JS语法通过。只有编辑器源码改变，没有重新构建Windows或重跑600帧smoke；此前二进制及smoke有效保留。没有改分、搜索随机种子或修改生产数值。

视觉验收继续未通过。冰晶边缘/粒子、原始录像视口、随机会话与未验证特殊模式/异步资源/每日池仍待查。不能把截图工具修复算作新发现的生产游戏缺陷。


## 音乐循环淡入淡出修复与雷击首帧审计（2026-09-29）

[已确认并修复] 原AudioConfig1001为Ptype4、Vol0.4、StartTime/EndTime均0，音轨约40.00798秒。AudioLoopFadeAction f20576在播放位置到达音轨长度减1秒时，经f13128启动1000ms淡出，f20574回调随后启动1000ms淡入；AudioSource.loop保持true，不重启或seek。此前BattleAudio只实现首次淡入和结算淡出，遗漏每次自然循环的尾首淡变。现AudioLoopFadeEnvelope补齐该行为，并保留暂停期间的墙钟计时、自然淡出期间Stop直接完成、普通Stop从当前音量乘设置值淡出。源码摘录在generated/audio-loop-fade-disassembly，结论见audio-loop-fade-fix-audit.json。

新增3项源分支测试：多轮尾首淡变、暂停前后墙钟推进、自然淡出及初始淡入期间终止。265项集成检查全部通过，重新构建Windows成功（0错误、5条已有Spine警告），新二进制600帧smoke通过、重试前后各9次出兵；240文件/1363 GUID资源审计通过。完整录像数值报告仍与full-replay-before-skin102.json相同。未修改伤害、兵力、随机种子或移动速度。

[已测量，尚不改固定时序] 原片62.29858秒首次显示雷击扣兵，62.46562秒仍无闪电，62.48236秒首次出现闪电，观察间隔0.18378秒。原f14822先EffectModule.Show再立即ChangeScore，资源回调允许后续才出现画面；一次录像不能证明恒定0.184秒加载耗时。见video-20260928/lightning-onset-audit.json、lightning-onset-contact.png。没有把这个差值加入生产常数。

[未发现复用缺陷] 冰晶在停用后复用，0.3秒的Animator状态hash、归一化时间、全部子节点缩放与新实例一致；未改相机HDR、材质或Animator重置。粒子随机性和全部过渡帧不在该验证结论内。完整视觉/听觉验收仍未通过。


## 特效模型就绪与寿命修复（2026-09-29，267项检查）

源码追踪确认：托管特效BaseEffect具有新/旧两条异步资源分支；旧AssetbundleModule先查缓存，命中时绕过加载等待，未证明所有请求必须延迟一帧。故不能把普通ResourcesModule待处理队列直接当作雷击显示规则，也不能加入录像测得的固定0.18378秒延迟。

已修复BattleSkillPresentation.Step的寿命错误：原版BaseEffect.Start在模型就绪后建立WaitForSeconds；本地此前在Flush创建模型后立即把同一Step已经过去的dt计入Age。现在只跳过创建Step的寿命扣除，已经就绪的模型仍正常推进，暂停仍使用scaled delta。未修改伤害、投射物时钟、兵力、种子；资源加载本身仍为同步，尚未复原原版冷加载。

新增两项检查覆盖长帧创建不立即过期、立即扣兵与展示独立、暂停、完整持续时长和已就绪模型正常到期。267项全部通过；Windows构建0错误/5条已有Spine警告，新二进制600帧smoke通过；240文件/1363 GUID检查通过。完整录像数值报告与修复前相同，截图已重生成。

可重跑证据：analysis/effect_resource_audit.py、effect_resource_lifecycle_audit.py；结论generated/effect-resource-readiness-audit.json。还需确认原会话使用的新旧资源分支、缓存状态、冷加载调度和Dispose早于加载完成时的竞态；完整视觉/听觉验收仍未通过。


## 当前新目标：关外逻辑完全复原（2026-09-29）
用户明确设定新的自主执行目标，目标工具已激活，无token预算。主任务转为关外完整逻辑；旧战斗复原成果及未完成视觉差异保留。ORCHESTRATION_STATE.currentWorkstream指向目标内OUTGAME_RESTORE_STATE.json，原battlefield phase/验收级别没有篡改。

已建16子系统清单，候选1417个源方法（候选不是全部本版本可达），首批249个函数已反汇编到generated/outgame/disassembly；可复跑analysis/outgame_disassemble.py并附类名。MenuTabConfig中4号HerosDetailUI是isActive=false，不要因为类存在就启用；其他指挥官入口仍需追踪。

首个实现OutgameCommanderProgression.cs：读取显式提供的CommanderConfig/UpgradeConfig及持有状态，复原NextCost、关卡门槛/资源判定分离、解锁和升级的扣款/级别/技能轮转核心。28级封顶，1→28共27次升级消费18750（A表）、技能轮流升到10；满级NextCost查询原版返回解锁费用，但实际升级拒绝满级。没有猜测新账号余额/初始持有，尚未接入UI、统计事件和保存。B表选择待核实，配置文件原样保留在Resources/Data/Outgame。

7项关外核心检查+原267项=274项通过，编辑器编译通过。未重新构建Windows；现有可执行文件仍是之前关内版本，不能宣称已有可玩关外大厅。源码/测试路径见outgame-commander-validation.json与generated/outgame/golden-cases.json。

下一步按OUTGAME_RESTORE_STATE继续：原LocalData/Commander初始化、ToolControl/ItemManager奖励入账和保存事件，再接主菜单及养成界面。不要继续以录像粒子/雷击延迟为主线，也不要把仅7项养成规则通过称为关外完成。保留既有用户profile，先建立兼容迁移契约。


## 关外库存核心完成，整体仍在进行（2026-09-29，280项）
本轮有实质进展：新增OutgameLocalInventory.cs，对应LocalDataManager.InitToolDictData/GetToolNum/SetToolNum（f11482/f11484/f4958）。AllSkillConfig中18个gameItem缺失记录补count2，已有0或其它余额保留；1001金币、1002钻石、1004体力、1005通用道具走标量字段；其它ID查已有ToolCount记录。未知道具写入失败，不静默新建；i32加法后负值拒绝整次变更，精确扣至0成功。注意这只是原始LocalData库存层，不能绕过ToolControl把1003礼包当体力/币处理。
新增6项验证，覆盖缺失补齐、耗尽后JSON重载不补货、四类标量映射、未知道具、余额不足原子拒绝、整数溢出、指挥官解锁+升级真实库存扣费。总280项通过，无C#错误。现有Windows程序仍是之前关内构建，本轮没有伪称可玩大厅或重建程序。未操作用户已有存档。
源证据OUTGAME_RESTORE_SPEC.json新增local-inventory-core gate；黄金用例追加到generated/outgame/golden-cases.json。首批271个源函数可在outgame/method-map.json查找。继续方向：ToolControl.ToolChange(f1799)分发/统计/事件/SaveData，再做完整本地profile和可操作入口。ItemHelper.GetGoodsType已提取到Type4286-32643.txt，DataManagerSave及ItemModuleControl已提取。初始完整账号、旧数据迁移、平台流程均未验收。


## 关外ToolControl分发完成（2026-09-29，288项）
新增OutgameToolDispatcher.cs，对照ItemHelper.GetGoodsType f2585和ToolControl.ToolChange f1799。复原1001/1002/1005特殊分类、技能2001..2999、士兵皮肤100..399、场景皮肤1..99，其余GameItem字典分类。金币/钻石按flag刷新TopInfo；只有成功的负金币变更记消费统计；技能分支无条件通知，再按统一flag二次通知。正向变更即使库存溢出拒绝也上报Get，负向失败不上报Cost；全部正常返回路径均调用Save，未知type0返回默认true但不变库存。源码特殊行为按实保留，没有擅自去重通知或加成功才保存条件。
IOutgameToolEffects提供实际接入点；当前测试用可观察sink证明顺序，尚无完整账号保存实现，不得把接口调用当作持久化或平台验收。场景配置不存在抛出，士兵配置不存在跳过；皮肤分支忽略数量；普通实体走signed delta，缺实体警告但默认true。缺失消费者不是自由授予奖励的理由。
8项新增检查+原280=288通过，编辑器编译通过。未重建Windows/未宣称大厅可玩。GameItem/SceneSkin/Skin原表复制至Resources/Data/Outgame。下一步真正推进账号默认值和磁盘store，并绑定IOutgameToolEffects，再做关外可操作入口；尤其CommanderManager.InitData/DealOldData仍需追踪，不得猜默认拥有英雄或金币。


## 关外指挥官记录初始化与磁盘存档（293项）
OutgameProfileStore.cs新增独立重建版存档封装，显式路径，与已有战斗用户档隔离；完整pending写入flush后replace，保留上一代backup。不是原平台存档格式，不宣称迁移兼容。源码确认CommanderManagerData默认选中1，InitData补建指挥官等级0、每技能等级1；有效已有记录保留，耗尽工具不补回。完整账号的新手奖励/登录授予仍未确定。
OutgameProfileValidation新增5项：默认记录、初始化保留进度、实际磁盘解锁升级扣款/耗尽重载、损坏文件保留、未知版本保留。Unity批处理退出0，集成293通过。测试仅写analysis/outgame-store-tests独立随机目录，没有触碰用户存档。当前尚未接生产大厅，也没有重建玩家程序。
下一步追踪可达MainTab/MenuTab/Commander入口及门槛，再将profile接到真实入口；IOutgameToolEffects的皮肤、实体、统计消费者需有源码依据，不可空实现后宣称完成。完整目标仍active/incomplete。


## 运行时菜单证据修正与切换状态（297项）
重要修正：MenuTabConfig id4 isActive=false仅描述旧HerosDetailUI记录，不能据此判定指挥官不可达。新提取UIControl共52方法（总323），OpenMenuItemUI/GetMainUITrans直接路由2→field28 ShopUI、3→field32 Proj_xqzdStartUI、4→field36 CommanderUI，三个getter确证。MenuTabUI点击4调用CheckUI(4)再CommanderControl.ShowCommanderGuild；使用配置禁用4会错误丢失真实入口。旧表保持原样，生产OutgameMenuNavigation按真实运行时映射。
已实现首次即时显示、当前页重复点击拒绝、过渡期间锁定、目标方向与源码0.3秒时长、完成回调解锁、返回主页面3，以及CommanderConfig[1].unlockLevel驱动的锁定覆盖层判据（5锁/6开）。实际动画/easing、RectTransform重挂和页面呈现尚未接入，不得宣称大厅可玩。1/5仍需追踪按钮隐藏与弹窗，不能因为表isActive=true就列为可滑动页面。
4项新检查通过，集成297，Unity批处理退出0。新增runtime-entry-map.json及spec gate。下一步把三页实际视图接到存档，追踪CommanderUI选择/升级操作与统计/保存，再补皮肤/商店实体消费者。无新玩家构建，完整关外目标保持active/incomplete。


## 指挥官页面操作顺序（302项）
新增OutgameCommanderActions.cs及5项验证，原始StatisticEventConfig复制到Outgame资源并按字段读取统计ID。选中未解锁英雄只预览，已解锁则立即出战并ReportUse；同一选择不重复操作。锁定英雄点击解锁先关卡门槛后价格；已拥有升级不重验解锁关卡。
成功后按源码顺序统计DailyCommUpgrade/CommanderUpgrade/CommanderUpgradeTotal，再UserPrefs保存与Manager保存，控制器ReportLevelUp使用当前出战英雄等级（不是被升级英雄等级），解锁UI随后弹窗、自动出战、ReportUnlock/ReportUse。真实磁盘回调测试确认保存发生在自动出战之前：磁盘有已扣款和已解锁，但仍为旧usedID，内存为新usedID。因此需要后续生命周期保存；不可悄悄添加即时保存并宣称源码一致。
IOutgameCommanderEffects为必需依赖，测试观测统计/报告顺序，尚无真实统计管理器或平台上报消费者；实际页面也未绑定。Unity退出0，集成302通过，尚无新Windows构建。完整目标active/incomplete。下一步必须推进可操作页面绑定和退出/返回生命周期，避免仅完成模型层。


## 原始关外UI Prefab导入（306项）
analysis/outgame_ui_evidence.py从原始asset-evidence提取MenuTabUI68节点/17绑定，Proj_xqzdStartUI78/33，CommanderUI115/43，ShopUI165/59；共426节点。outgame_ui_prepare.py生成outgame/ui-import.json：101sprites、1font、2explicit material unknown。原始图像和字体哈希校验后由共用RecoveredHudImporter.ImportOutgame导入Resources/Recovered/Outgame四个Prefab。旧战斗HUD仍用Import，参数化输入和目标，未替换旧prefab。
修复导入器Text空字体引用：原MenuTabUI btnMask两个Text确为无font，保留null，不猜字体。原始同名兄弟节点保持同名，校验按节点顺序/父映射解析，不强迫改名为内部唯一path。OutgameUiImportValidation核对426节点名称/层级顺序、锚点/pivot及active状态；4项+原302=306通过，编译退出0。仅静态导入；未做屏幕视觉对照、动态列表、事件接线或新玩家构建，仍非可玩大厅。
关键下一步：复用这些原始Prefab接OutgameMenuNavigation和CommanderActions，避免继续只写模型。按钮原始outlet路径在ui-import.json bindings。完整统计/奖励/商店/活动/平台/lifecycle仍待完成，完整目标active/incomplete。


## 原始菜单视图接线（308项）
OutgameMenuView.cs实例化四原始Prefab，绑定objSkin/Main/OtherUnSelected原按钮，主界面首次显示，incoming挂到outgoing下做页面过渡，0.3秒后恢复父级/关闭outgoing/解锁切页；选中和未选中底栏状态同步。imgCommanderLock按钮发送锁定通知，显式level<CommanderConfig[1].unlockLevel拦截指挥官入口；测试用level5/6，不当作新账号默认。MenuTabUI.Awake源码00801093..008010aa隐藏field148=ShopUnSelected和184=ItemUnSelected；运行视图保持相应无效页隐藏。
过渡使用可配置AnimationCurve，默认线性明确为临时呈现，原DOTween默认easing尚未验证，不宣称曲线一致。动态页面控制器从PageShown事件接入，当前尚未绑定指挥官列表、皮肤内容或账号。两项真实Prefab Button.onClick检查验证往返、父级/active状态、完成解锁、锁定覆盖、快速点击保护；308集成通过、编译退出0。尚无独立大厅场景/新Windows构建或端到端UI点击烟测。
下一步CommanderUI原列表/详情接OutgameCommanderActions和持久化账号，再启动入口/生命周期/消费者。关外完整目标active/incomplete。


## 原始列表滚动/遮罩/自适应修复（308项，扩大检查覆盖）
继续动态列表前发现RecoveredHudImporter跳过ScrollRect/Mask/ContentSizeFitter，会导致原始CommanderUI和ShopUI不能滚动/裁剪/扩展。已按原始序列化字段实现三类组件，ScrollRect第二遍解析原始content/viewport PPtr绑定，复制horizontal/vertical/movement/inertia/elasticity/deceleration/sensitivity及scrollbar设置；有非空scrollbar引用且未实现时明确报错，不默默丢弃。当前4Prefab全部重新导入退出0。
扩展OutgameUiImportValidation既有4项检查，校验原始引用及控制参数：CommanderUI11组件，ShopUI16组件，共27；仍308条但覆盖更广，不虚增计数。综合编译/回归通过。CommanderItem新提取14方法，总method-map337，定位了原始CommanderItem模板、CommanderContent和技能模板，待实际绑定动态数据。没有新玩家构建、没有完成的大厅。
下一步必须接原始CommanderItem模板数据与选择/升级交互，不再受缺少滚动组件阻碍。原始节点路径和bindings在outgame/ui-import.json；完整目标仍active/incomplete。


## 原始指挥官动态卡片与操作接线（309项）
OutgameCommanderView.cs克隆原CommanderItem模板到CommanderContent，按传入档案顺序生成卡片，绑定原始独立头像（IconName原配置offset44），选择/锁定/出战/等级状态与源码红点规则，并绑定原btnUpgrade到OutgameCommanderActions。成功解锁/升级后刷新卡片与按钮。原素材准备脚本扩展所有CommanderConfig.IconName，107sprites（原101+6）重新导入。
新集成用例用真实Prefab Button.onClick：锁定卡片预览→10000金币解锁自动出战→250升级→磁盘重载余额0/等级2/used2；验证动态头像不同、锁与出战标记及文字刷新。显式fixture金币10250/level999仅测试，不是账号默认。统计/报告使用观测sink，实际磁盘Save回调执行。309集成通过，编译退出0。
技能详情/3D模型/价格说明仍未接，当前标题/等级由外部localize函数提供；真实启动入口、本地化消费者、统计和平台消费者、退出生命周期仍待完成。暂无新Windows构建或视觉完整验收，目标active/incomplete。


## 指挥官价格与中文文案（310项）
OutgameCommanderView补齐原textLv、textUnlockName/textUpgradeName/textMaxLevel状态，goUpgrade价格区随满级及unlockType2隐藏；首次解锁显示unlockPrice，已拥有显示NextCost，第二价仅锁定且priceCount>1显示；首价item>=8001按原{owned}/{cost}格式及0.5缩放。当前A表实际仅1001金币/1002钻石，第二价/实体显示分支没有真实A表端到端样本，不能扩大验收。证据CommanderUI.RefreshSelectedCommanderInfo f5148 offsets001aecae..001af312及InitializeComponent字段映射。
新增OutgameLocalization读取原LanguageConfig中文列，复制完整原表未改内容；动态视图测试改用实际中文，验证时光守护者、10000解锁→250升级、500钻石价、满级隐藏价格显示满级。新增1项+原309=310，Unity退出0。UI预处理增加价格物品图标候选，共116sprites；图集同名资源的精确消歧仍待核对，不宣称像素一致。
技能/3D/解锁获取说明、真实启动和生命周期、平台/统计消费者仍未完成，无新Windows构建。目标active/incomplete。


## 关外实际渲染检查与详情面板生命周期（311项）
新增OutgamePreviewCapture.Run编辑器独立渲染fixture，创建540x960屏幕空间Camera+原始Prefab主界面和指挥官页，输出analysis/captures/outgame-main-fixture.png、outgame-commander-fixture.png。不加载/创建用户存档；level6/10250金币仅预览夹具，不是新账号数据。已实际打开检查两图：主界面仍缺动态Logo（白块）、场景背景，Level100是原编辑文案尚未刷新；指挥官缺模型和技能内容，顶部/底部图标及动态可见性还未全接，不能称完整可玩大厅。
渲染揭示技能详情原Prefab默认打开显示New Text，源码CommanderUI.Awake f201?在008a60c7调用h^ZMRew f6196关闭field276=objSkillDetail，VisibleImp隐藏分支008a544c..008a5456同样关闭。已在Bind、OnDisable和成功升级关闭详情，并重新渲染确认占位详情不再覆盖页面。
新增一项初始化/显式disable回调检查：编辑器非PlayMode SetActive不自动运行普通MonoBehaviour的OnDisable，因此测试通过反射显式调用，不能当作真实玩家生命周期烟测。311集成通过、Unity退出0，截图反映当前仍不完整。下一步优先技能/模型与真实启动动态UI，随后实际Player生命周期验收。完整目标active/incomplete。


## 指挥官技能卡片（312项）
新增OutgameCommanderSkillCards，克隆原CommanderSkillItem到objSkillContent，来源CommanderSkillData.UnlockLevel/Unlock及CommanderSkillItem.SetData/RefreshData/RefreshUpgradeInfo。解锁使用独立RealCurLevel>=AllSkillConfig.unLockLevel（6/14/21），不是CurLevel或英雄等级；未命中配置返回0。原技能图标来自GameItem[id+2000].ItemIcon，卡片锁与等级背景互斥、等级中文前缀、升级箭头仅owned&&!max且slot==(heroLevel-1)%3。技能点击暴露SkillSelected(slot)，实际详情数据仍未绑定，不宣称完成。
新用例CurLevel999而RealCurLevel6仅第一技能解锁，RealCurLevel14第二解锁第三保持锁，真实英雄升级刷新技能1到等级2/箭头切到slot1。准备脚本补技能图标，共134sprites。实际渲染并查看确认三张卡片出现。渲染发现字面
，补提取LangModule共21方法，Get f1243原000674be..000674d2明确String.Replace(escaped newline,newline)，OutgameLocalization按源码修复。method-map现370。重新渲染确认换行提示正确。
312集成检查通过，Unity退出0。完整技能详情/技能效果说明、英雄模型、真实启动与所有剩余关外系统仍待完成，无新玩家构建，完整目标active/incomplete。


## 指挥官技能详情与数值表（315项）
OutgameSkillDescription按原CommanderUI f8108/f12849绑定技能名称、目标、操作、说明和锁定提示。SkillConfig[id*100+skillLevel]与describeData决定说明数值；速度类dataType4/5转为abs(value-1)*100；一位小数ToEven加原空格。
数值表完整恢复f12846：遍历dataType而非describeData，0隐藏，1持续时间/2技能数量/3火力数量/4减速/5增速。level<=9显示下一技能等级差值的绝对值，变化值保留两位ToEven并用原黄色+格式；不变或满级仅当前值。原Awake字段字典与ctor格式串已核对，已移除临时隐藏整表逻辑。
CommanderItem.OnClick f20191调用RefreshSelectedSkillInfo(-1)，故切换英雄保留已选技能槽及面板开关并刷新内容；增加验证防止旧英雄技能残留。Close按f6196只隐藏，保留槽位。
Unity验证退出0，315集成检查全部通过（原267+关外48），指挥官视图7项。实际渲染并查看outgame-skill-detail-fixture.png：时光守护者减速30%+10%、持续10秒、所有敌方塔/点击、第14关解锁提示，原皮肤与布局。此为隔离编辑器fixture，不是生产账户/玩家构建；所有数值测试存档仍在随机隔离目录。
完整关外目标仍active/incomplete，后续优先原登录/账号/关卡提供者与真实入口，及英雄模型、真实事件消费者和余下关外子系统。


### 账户入口取证已启动
新增提取LoginCtrl/UserInfoManager/ProcessControl/LevelControl（含嵌套）132方法，累计502。ACCOUNT_STARTUP_AUDIT.json记录已证事实：RealCurLevel恒取持久LevelID，CurLevel受SpecialLevel分流；普通模式setter写LevelID+stat10015(level-1)+save，特殊模式忽略；LocalData ctor不授予LevelID或货币，后续登录/迁移流程仍需取证。没有据此把0当用户首关、没有添加假账户或绕过平台。上一轮315项生产代码验证保持有效，当前只新增源码证据。


## 关卡提供者与真实启动取证（318项）
新增OutgameLevelProgression，OutgameProfile持久化levelID（原LocalData标量0默认，未宣称完成首次账户初始化）。RealCurrentLevel恒为levelID；CurrentLevel在eSpecialState=0时取levelID，否则取SpecialLevel。核对原getter/setter后修正ACCOUNT_STARTUP_AUDIT：field24是eSpecialState，field16才是SpecialLevel，之前审计命名错误已纠正。正常setter原顺序写值→stat10015(value-1)→SaveLocalData；特殊模式setter无副作用；保留原unchecked int32减法且不Clamp。
新增3项验证覆盖真实隔离磁盘重启/统计顺序、特殊模式不污染普通进度、零值不篡改。原指挥官视图验证改用真实提供者，SpecialState1/SpecialLevel999/持久LevelID6和14验证技能门槛。Unity退出0，318集成全部通过（原267+关外51）。
补提取LoginProcedureBase31、流程51、ConfigMgr56方法，累计640。已证入口先迁移皮肤与指挥官再加载GamePlay，回调初始化rank/七日活动后状态1→2；状态2用CurrentLevel解析关卡。原LevelConfig确有id0/SceneId0，不能擅自把0改1。原登录网络/首次数据/完整入口仍待接通，下一步补完整调用顺序与生产消费者。没有新玩家构建，目标仍active/incomplete。


## 原本地数据导入与旧道具迁移（322项）
新增OutgameOriginalLocalData.Apply，读取提供的原始LocalData JSON，仅提交已恢复的LevelID、货币、collectNum/collectAdNum、toolCounts。按DealUnsafeStr全串bank→collect替换，sourceLocalDataJson原样留存，包括未知字段，供后续皮肤/指挥官等消费者使用；不对真实用户文件执行导入。
空/缺省数据只初始化原标量0和缺失技能道具2，不添加虚构首关/货币奖励。仅原toolCounts为空启用旧六字段迁移；OldToolPropertyClass ctor所有字段2，2001冰/2002支援/2003toolnum_4/2004火/2005升级/2006toolnum_5，-1跳过覆盖，其他负数按源码保留；现代非空列表优先，保留数量0。解析成功后才提交到重建profile，失败保留当前与磁盘存档，此为导入器保护策略、不声称原坏JSON容错完全相同。
四项新验证通过：六字段+bank迁移、新旧优先级/-1默认、空数据、原文和未知字段持久化/坏JSON不污染。Unity退出0，322集成检查（原267+关外55）全部通过。原方法索引642。完整登录、其他旧数据迁移、主入口与关外其余系统仍未完成，没有新玩家构建，目标保持active/incomplete。


## 原英雄旧存档迁移（326项）
新增OutgameOriginalCommanders.ApplyLegacy，按CommanderManager.DealOldData f12859：null/decoded-null/空列表不改当前数据，非空commanderDatas整体替换，再执行原InitData补齐缺失配置英雄；不按最高等级合并，不篡改已选ID、不额外保存。原isNew/Id/curLevel/skillsData(skillId,skillLevel)全映射，UsedCommanderId默认1。低等级旧数据会按原规则替换高等级当前记录，输入仅隔离fixture、未导入任何用户文件。
修复此前重建仅保存skillLevels的不足：OutgameCommanderState新增skillIds/isNew，缺失记录取配置技能，导入记录保留技能编号与顺序；升级、技能卡片和详情使用held SkillIds。旧重建envelope未存skillIds时兼容回退配置。未命中英雄配置的原记录留在数据列表但不生成页面卡片，与原InitData有效字典范围一致。
新增四项测试：整体替换与记录保留、空旧数据不覆盖、按保留技能槽升级并重启、原UI详情/卡片使用导入技能ID与等级（测试故意调换顺序防止假阳性）。Unity退出0，326集成检查通过（原267+关外59）。完整皮肤/其他迁移、真实登录与关外入口仍待完成，无新玩家构建，目标active/incomplete。


## 皮肤记录与旧数据迁移（330项）
新增OutgameSkinCatalog与OutgameSkinState，提供原SkinManager载入、缺失记录补齐与DealOldData。缺省manager数据初始装备按types1..4从lockState2选择：100/200/300/1；存在但空的{}不擅自补装备。持有的u保留、isNew取newSkins成员关系，缺失配置记录才按lockState2赋初始解锁，config刷新type/sort/special/castType，未知存档记录保留但不进有效索引。元数据SkinManagerData/SkinData/OldData等字段证据写入skin-record-fields.json。
旧list_playerskin(skinId,isUnlock)/sceneMapDatas(sceneId,isUnlock)只覆盖对应皮肤解锁标记，包含true→false，未知ID忽略，不改装备或新获得标记，不额外保存。原方法索引688。
4新用例覆盖缺省与已有空数据差异、持有记录与配置刷新、旧标记双向覆盖与装备不归一化、隔离磁盘重启。Unity退出0，330集成检查通过（原267+关外63）。完整皮肤排序/获取/装备/原save投影、ShopUI和账户启动仍待完成，未构建新玩家，完整目标active/incomplete。


## 皮肤操作与保存投影（333项）
已补原 SkinManager 的首次解锁、重复/缺失忽略、兵种拥有数量统计、场景无统计、装备字典直接赋值、清除新标记恒返回 false，以及 OnSave 重建 newSkins/usedSkin。兵种计数包括免费皮肤；统计在解锁状态写入后执行。未知存档记录的新标记也参与保存投影。新增3项验证含隔离磁盘重启；Unity退出0，333集成检查通过（原267+关外66）。排序、商店价格/支付回调、原UI、启动与完整构建仍未完成，目标保持 active。


## 皮肤购买与广告回调（337项）
新增 OutgameSkinItemActions，按原 SkinItem 保留金币/钻石价格、扣款失败提示、成功统计/发放/装备事件/清新标记顺序，以及广告失败无动作、成功发奖。选择仅 curItemState==0 执行，保留可选 onClick 顺序。原存档在 ToolChange 内发生，早于装备与清新标记；隔离重启验证该边界及后续保存。4新检查通过，Unity退出0，337集成检查（原267+关外70）。源方法索引750。真实UI绑定、ChooseSkin视觉监听、广告原reason静态值和平台提供者、启动与完整构建仍待完成。


## 原兵种皮肤卡片（339项）
新增 OutgameSkinItemView，绑定原 ShopUI 的 SkinItem 模板：价格/货币图标、标签、购买/选择/广告回调、使用中与新标记、活动入口。保留 special0/1购买、special2按活动状态切换、special3特殊入口、未知special保留既有状态；活动1301读取七日活动，其他读取status!=5。广告请求编号skinType+1001。恢复原预制体直接调用按钮事件，验证真实解锁/装备与活动状态转换。2新检查通过，Unity退出0，339集成检查（原267+关外72）。完整ShopUI生成/排序/监听、独立SceneSkinItem、真实图标与活动/广告提供者、账户启动和构建仍待完成。


## 场景皮肤独立操作（341项）
新增 OutgameSceneSkinActions，保留独立BuyScene统计与装备/场景切换先于解锁保存的顺序；选择只判当前装备ID，不沿用兵种curItemState检查，也不发送ChooseSkin事件。验证购买扣款失败、完整成功顺序、隔离重启保存边界、相同选择无动作和原处理器无ownership检查。2新检查通过，Unity退出0，341集成检查（原267+关外74），原方法索引756。场景原预制体存在btn_grandUnlock而缺继承方法所查btn_actLimit，已记录待核对，未猜测别名。场景实际视觉与原页面全流程、启动及构建仍未完成。


## 场景卡片原模板路径确认（342项）
已解决上一检查点的模板名称差异：UIObject.Get未发现名称别名；ShopUI创建协程 Type4407.MoveNext在分配SceneSkinItem后，传入ShopUI字段172（SkinItem模板）和字段196（Content_scene）创建场景卡片，并非名为sceneSkinItem的旧模板。OutgameSkinItemView据此支持独立场景操作控制器，共享原模板显示逻辑。原模板克隆进场景Content，实际购买/选择按钮验证BuyScene统计、场景切换、装备与使用中标记。Unity退出0，342集成检查通过（原267+关外75），原方法索引771。完整商店生成/排序/监听、实际活动/广告/场景视觉与账户启动仍未完成。


## 皮肤原排序（343项）
静态泛型元数据证明原初始化使用 Enumerable.OrderBy(GetOrder).ToDictionary，而非推测ThenBy。已加入有序兵种/场景索引，默认项派生值1优先-99，其他项按非零sortNo或id余数规则排序；同值保留已持有记录/缺省补齐插入顺序，存档列表不重排。真实配置与乱序持有记录用例通过；Unity退出0，343集成检查（原267+关外76）。完整ShopUI创建/过滤/事件、提供者、启动和新构建仍待完成。


## 原商店皮肤列表生成（344项）
新增OutgameShopSkinLists，按原有序皮肤记录创建全部兵种/场景卡片，当前兵种类别优先创建，映射normal/defense/attack/scene内容容器，场景共享SkinItem模板并绑定独立控制器。每卡片要求显式提供依赖绑定，不以无动作默认值冒充生产行为。原ShopUI预制体验证全部配置数量、分组、当前类别优先、装备刷新和重建无重复；Unity退出0，344集成检查（原267+关外77）。源协程逐帧创建时序、图标与toggle/选择监听、真实提供者、账户启动与完整构建仍未完成。


## 商店页签回调（345项）
新增OutgameShopTabs：true切换播放2001，兵种类别更新保留的旋转类别，场景/商店保留前一个兵种类别；更新红点、发送旋转通知并本地化标题。选中页签隐藏红点但不清除皮肤isNew，false回调无动作。初次验证发现原Toggle未导入，已补原组件基础状态/targetGraphic导入并重新导入页面，复验Unity退出0，345集成检查（原267+关外78）。Toggle图形/group/持久事件和内容切换、启动选中、原图标与实际提供者仍需补齐，完整关外未完成。


## 原Toggle内容绑定（346项）
恢复ToggleGroup互斥配置、Toggle.graphic/group引用与原onValueChanged动态bool SetActive持久事件，保持原目标、顺序和CallState；不支持的事件/外部引用明确拒绝。原预制体事件已指向分类内容和Label_on。隔离编辑器实例临时允许RuntimeOnly事件在编辑器执行后，真实isOn变化验证互斥、内容和选中标签切换；保存的预制体保留原调用状态。Unity退出0，346集成检查（原267+关外79）。初始页签选择、真实图标/活动/广告/场景与账户生命周期、新构建仍未完成。


## 原商店初始选择（347项）
补ShopUI.Awake初始分支：先隐藏四类皮肤和Shop内容；持有类别2/3选对应Toggle，其他值归1，显式启用normal并重复原变更回调，再刷新红点。保留原默认分支可能产生两次通知的顺序。使用新原预制体分别验证2/3/99初值与fallback，场景/Shop保持隐藏。Unity退出0，347集成检查（原267+关外80）。真实图标、完整生命周期/事件与平台提供者、账户启动和新构建仍未完成。


## 原皮肤图标资源（348项）
96个SkinConfig/SceneSkinConfig图标已全部由原skinandtoolicon/sceneskin图集精确定位并导入，资源清单从134到230个Sprite。场景tubiao01..04与引导图集存在同名冲突，改用skinId到原Sprite ID映射，记录图集/包路径/像素文件哈希与导入参数，运行时缺失明确失败。全皮肤卡片img_poster绑定验证通过，Unity退出0，348集成检查（原267+关外81）。尚需组装商店视觉检查、生产生命周期/事件和平台提供者、账户启动及新构建；完整目标继续active。


## 商店实际渲染预览（348项复验）
OutgamePreviewCapture增加兵种/场景商店隔离渲染：outgame-shop-soldier-fixture.png与outgame-shop-scene-fixture.png，已逐张查看，原卡片图标/价格/标签/页签显示正常。上方3D展示仍缺失，保留静态占位，未声称完整视觉复原。渲染发现空标签key导致原重建本地化字典抛错，核对LangModule.Get f5950确认缺失键回传键名；已修复OutgameLocalization并覆盖空/缺失键验证。Unity预览与集成复验退出0，348项通过。真实账户/提供者和完整构建仍待完成。


## 商店装备事件联动（349项）
新增OutgameShopEquipment：按原SkinConfig查找，写装备、重置该类别广告候选、刷新同类别所有卡片、请求展示模型，缺失/场景ID无动作；不加保存或所有权判断。真实卡片列表验证旧/新使用中状态在模型回调前一致更新。Unity退出0，349项检查（原267+关外82）。PlayerControl新增28个源方法索引，总799，缓存/加载/首广告重置证据写PLAYER_DISPLAY_AUDIT.json；模型加载完成回调、实际父节点/摄像机/动画与完整生产接线仍待完成。


## 商店模型加载生命周期（352项）
新增 OutgamePlayerModels：原实体/附属资源两阶段加载、父节点与局部变换、递归层级、分类模型缓存和显示切换。泛型元数据确认缓存使用 set_Item；保留加载中重复请求、晚回调覆盖显示与缺配置无加载回调。3项新增异步顺序测试通过，Unity退出0，集成352项（原267+关外85）。动画适配器、真实模型资源与展示根节点尚未接入，完整目标仍未完成。下一步恢复动画虚方法与具体资源/根节点。


## 商店模型动画调度（354项）
元数据虚槽确认 Reset/Play，原播放器使用烘焙网格而非 SkeletonAnimation。新增 OutgamePlayerAnimation：按当前装备取模型，先着色再检查 activeSelf，缓存动画组件，Reset→relax(false)→完成时重新读装备并idle(true)，保留无动画组件缓存和原错误。新增2项检查，集成354项通过（原267+关外87），Unity退出0。源方法索引822。实际烘焙动画播放器、模型资源/展示根节点、生产生命周期待完成；完整目标保持未完成。


## 关外烘焙模型动画（356项）
新增 OutgameBakedAnimator 与独立模型导入器，以原 animationData 和现有原始网格/材质生成关外 soldier_100/102/200/300 四个预制体；不改战斗预制体。实现渲染属性、默认播放、缺动画回退、暂停/恢复、完成回调顺序。严格完成边界测试发现并修复 Mono 中间精度与 WASM f32 加法的差异。Unity导入和验证退出0，356项通过（原267+关外89）。这是四个模型及动画控制验证，其他皮肤/攻击展示变体、原始场景节点、生产接线和完整关外仍待完成。


## 原始模型展示节点与摄像机（357项）
直接从 BuildPlayer-GamePlay/GameSceneMono 原始引用提取10个展示组/摄像机祖先节点，导入 OriginalModelRoots.prefab，保留局部变换、层级、active/layer和HomeCamera投影参数。OutgameModelRoots 按原构造 scale1/localPosition0 创建模型控制器。验证通过，357项（原267+关外90），Unity退出0。源方法888。已确认着色具有camp标签相同则完全跳过的规则，标签常量/当前camp、攻击展示变体、9033/9034资源与完整生产接线下一步继续；关外整体未完成。


## 模型阵营着色（358项）
新增 OutgameSoldierColor，恢复Tag5..10阵营映射、Tag9默认值、同标签完全跳过、首个子MeshRenderer颜色与根Renderer排序，保留动画属性块。原始palette复用，注册6个源标签，真实模型验证通过。Unity退出0，358项（原267+关外91），源方法893。实体表确认展示默认攻击模型为4000/soldier_400，阴影9033/BBDyShadow、9034/QBDyShadow；后者已有Recovered/BossEmbedded预制体。下一步导入400/9033并实际连接场景与模型。完整关外目标未完成。


## 默认模型真实资源接线（360项）
导入原始soldier_400展示模型（256顶点，idle/relax），独立ModelAssets目录，不改战斗预制体。原BBDyShadow的SpriteRenderer、Animator、完整动画曲线导入Recovered/BossEmbedded；9034沿用原QBDyShadow。OutgameModelAssets和OutgameModelRoots.Connect将原节点、加载、阴影、阵营着色、烘焙动画接通，真实默认三兵种与102切换验证通过。两项新增，Unity退出0，集成360项（原267+关外93）。未做新可玩构建/视觉验收；下一步原摄像机+商店截图检查、旋转/宽高比和真实camp/账号生命周期。准备阴影脚本必须使用C:/Users/jiachengwei/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe（UnityPy vendor为cp312，默认3.14缺_brotli）。


## 商店展示旋转与可见预览（361项）
新增OutgameModelRotation，按原f18219直接交换位置/缩放，初始3，支持前后环绕，同项/非法项不动。预览真实接入tab回调，加入原HomeCamera模型层。初次截图模型被下方UI挡住；camera-only证明资源正常，追踪MenuTab f11385→GameControl.MoveCamera(true,.5)后应用soldierRoot向上.5的原值，模型在商店上方可见。已查看soldier/attack两张截图；普通居中/骑兵居中切换正确。仍有默认skybox、ghost UI占位和未完整恢复的场景内容，不能当视觉验收。MoveCamera还控制soldierRoot原child1=Transform40及Scene_home_CJroot49；当前导入子集只有child0=43，下一步须完整补绑定而非直接GetChild(1)。361项（原267+关外94），Unity退出0，完整目标未完成。


## 商店场景显隐与比例分支（363项）
OutgameScenePresentation恢复MoveCamera完整transform/active分支，精确Scene_home世界坐标比较（不是近似Vector3比较），返回主页归零。恢复原InitScene宽屏/窄屏不同缩放规则及窄屏摄像机尺寸。OriginalModelRoots扩为13个原节点并绑定背景，避免子集GetChild序号错误。背景节点尚无MeshRenderer/材质导入；新增的是源节点与逻辑，未声称背景视觉完成。AppSetting type3375静态offset4为额外屏幕高度，取值待追踪。2项新增检查，Unity退出0，363项通过（原267+关外96），完整目标未完成。


## 原生场景背景（364项）
两个同名meshHomeScene源节点4/18的MeshFilter/MeshRenderer原生导入为唯一后缀预制体，保留builtin Quad10210、shader10752、共享ScenehomeSkin材质及scene_skin_idle1纹理。模型节点导入器使用原生背景实例，重新截图已查看：森林背景正确显示，默认三兵种仍可见。测试修正了导出纹理按ID命名导致的名称断言，改为精确资源引用验证。Unity退出0，364项（原267+关外97）。源方法928；高度修正值为AppSetting.BangsPixel，metadata14510/offset4已证实，初始零，平台赋值待追踪。全关外未完成。


## 场景皮肤贴图缓存（365项）
新增OutgameSceneStyle恢复f6059/f12185/f15203：旧效果隐藏、当前ID、id-1贴图缓存、共享材质替换及原异步回调顺序；晚回调可写旧贴图但效果取当前ID，同ID仍可刷新Shop。新增真实Renderer隔离材质的异步顺序检查，365项（原267+关外98）通过，Unity退出0。15场景配置仅idle1在当前Texture2D PNG导出中，其余14是导出覆盖缺口，不等于包内缺失；scene-texture-coverage.json已记录。下一步查资源缓存及效果helper5063，完整生产接线/账号生命周期仍未完成。


## 全15场景贴图资源（366项）
依据原catalog/manifest获取26包最小闭包（3827007字节），全量size/MD5通过；独立snapshot outgame-scenes-20260929，包含15场景idle纹理及3个idle效果依赖。首次沙箱网络拒绝后，获准提权执行并成功获取。导出并导入全部15纹理，保留尺寸/sRGB/filter/wrap/mip标记，不压缩；OutgameSceneTextures实际资源提供器和SceneStyle真实材质测试逐ID通过，10..15对应idle11..16。Unity退出0，366项（原267+关外99）。3个场景附加效果仍仅获取/导出，未Unity导入/接线；helper5063及回调为下一步。完整关外目标未完成。


## 场景附加效果缓存逻辑（368项）
新增 OutgameSceneEffects，使用原始 SceneEffectConfig，保留 offset+sceneId 缓存、复用只激活不重设父节点、异步回调无选择检查、重复加载完成 Dictionary.Add 失败行为；根 SkeletonAnimation 初始化后播放 animation 循环。新增异步顺序与缓存隔离测试，集成368项（原267+关外101），Unity退出0。三组实际效果资源已下载，Spine/粒子原组件导入与生产场景接线仍待完成；整体目标继续。


## 三组原始场景附加效果（369项）
导入古堡、沼泽 Spine 4.1.16 与火山六个原始粒子系统，共3个原生Prefab、34项依赖；保留原始材质、纹理、层级和组件数据。新增实际资源加载器、Spine循环动画/生成网格/缓存复用验证，集成369项（原267+关外102），Unity退出0。已生成并查看 scene7/8/9 隔离预览，火山粒子与沼泽动画可渲染。未声称正式账号接线或原版逐帧对照完成；整体目标继续。


## 菜单请求与场景切换接线（370项）
OutgameMenuView 发出导航成功事件，OutgameMenuSceneBinding 按 MenuTabUI f5890/f11385 在 CheckUI 成功返回后立即调用 MoveCamera；拒绝请求、重复页面、锁定统帅不产生场景副作用。预览已使用实际绑定替代手动移动。原生页面与模型根联合验证通过，370项（原267+关外103），Unity退出0；正式账号入口与完整生命周期仍未完成。


## 登录数据同步入口追踪（源方法992）
核实 LoginProcedureBase 注册的9个FSM类型，新增具体登录与GameDataVersionMgr源码索引，共992方法。确认同步消息订阅/解绑、结果回调和UserLogin入口；不能将本地档案构造视为登录成功。下一步解析同步泛型调用与重载/上传/下载分支，首次关卡赋值仍待证明。本轮为源码证据推进，最近运行时验证仍370项，不声称完整启动通过。


## 登录同步状态（374项）
纠正前轮消息标注：GF_ReceiveDataUploaded是上传完成而非失败。新增OutgameLoginSync，原样保留新玩家等待上传、旧玩家直接进入成功状态、回调读取当前IsNewPlayer、30秒超时回退用户ID/路径/关闭同步和离开时解绑。泛型解析确认ChangeState<LoginSuccess>，源字段确认Uid/IsNewDevice/IsNewPlayer。新增4项边界与顺序验证，集成374项（原267+关外107），Unity退出0。SDK/传输/父级错误处理需真实host接线，未声称登录完成。


## 登录失败分支（376项）
新增OutgameLoginFailure，保留先发送LoginProgress4、模式0清除成功标志转LoginSuccess后LoginFail5，以及非0重置计时转IdelState后原错误码通知的区别。泛型目标和LoginFail虚槽14均已解析。2项顺序/字段保持测试通过，集成376项（原267+关外109），Unity退出0。另确认HTTP错误经虚槽10，基类只记日志，不能直接套用终止处理；实际FSM/SDK/数据服务接线仍待完成。


## 登录成功状态（378项）
新增OutgameLoginSuccess，严格先注册FSM事件3/5/7，再按成功标志调用LoginComplete(true,empty)。事件3进入LoginSDK_XYX，5/7进入IdelState；离开显式解绑3/5后调用基类，保留原版未显式解绑7的行为。重入完成回调和切换失败假成功防护验证通过，集成378项（原267+关外111），Unity退出0。完整FSM事件生命周期、账号数据传输及实际入口仍待接通。


## 登录数据重载决策（380项）
新增OutgameLoginDataPlan，确认本地同步标志0且新玩家时强制上传并跳过内存重载，优先于新设备/切换账号；其余情况仅不同账号或新设备触发重载。同账号字符串按原版精确比较。新增账号/设备/新玩家/同步标志矩阵验证，集成380项（原267+关外113），Unity退出0。该决策尚未代替完整数据版本与传输实现；整体未完成。


## 数据版本状态（383项）
新增OutgameDataVersionState，保留首次版本-1递增到0、整数溢出、空键拒绝、双待处理列表门控、强制上传标志更新和先持久化后完成事件。关闭同步只清指定标志/队列，保留两份版本字典且不发完成事件。3项验证通过，集成383项（原267+关外116），Unity退出0；源方法1004。原版版本文件IO、传输与账号入口仍待补齐。


## 原始SDK存储队列（385项）
追踪至原包unity-sdk/storage.js，确认版本文件实际使用游戏名+GameDataVer键，WebGL分支丢弃计算的txt路径。新增OutgameSdkStringStorage，保留即时缓存/异步FIFO、失败不回滚、完成后推进、缺失哨兵与删除语义。2项失败/重启/队列测试通过，集成385项（原267+关外118），Unity退出0；源码1040方法。测试后端隔离，真实平台存储与版本JSON接线未完成。


## 版本JSON与隔离持久化接通（387项）
新增OutgameDataVersionStorage，按原数组格式读写版本、先清字典再Add、保存前标记field29；int32精度与缺失版本-1保持。新增显式目录OutgameFileStorageBackend，通过现有SDK队列实际写入隔离测试目录，并用全新实例重载验证；未访问实际用户存档。2项JSON/重复键/重启测试通过，集成387项（原267+关外120），Unity退出0。DataContract JSON用于已验证记录格式，完整LitJson异常兼容和真实平台仍待验证；整体未完成。


## 管理器版本初始化与清理（389项）
还原GameDataVersionMgr f12180：只纳入offset9为真的管理器，缺失键版本-1；已有版本不递增，按Local.Keys快照删除失效或停用键；无变化也统一保存一次。LoadManagers将其接入原格式读档；新增保留/重复/停用/空注册表及加载后保存验证。Unity集成389项（原267+关外122）通过，退出0。新增DataManagerBase/Pool共34个源码索引；具体注册表、初始化保存限制与完整启动生产连接继续推进，整体未完成。


## 原始管理器注册表及保存生命周期（393项）
按元数据v31解码ModelRegister属性并完整枚举Assembly-CSharp继承树：25类，16个Proj_hdzd、5个CommonGameModule、4个无标记，全部标记项同步=true、压缩=false。注册表加入Resources/Data/OutgameManagerRegistration.json。新增OutgameDataManagerPool：先赋标志并OnInit后登记，按原gameName筛选；初始化重置禁用及字典；保存跳过null，单项异常Debug.LogError后继续，并保持IsEnableSaveData查询与SaveData实际检查的区别。新增4项注册/异常/守卫/原元数据选择检查；393项（原267+关外126）通过，Unity退出0。实际管理器工厂及完整生产启动尚未接通，目标未完成。


## 原始管理器存档流水（396项）
新增OutgameDataManagerStorage连接SDK字符串缓存/队列及版本状态：按原标识符规则校验（无效仍警告后写入），UTF8小写MD5去重，特定登录标志绕过去重；压缩发生在保存前，本地写入要求登录进度>=10，版本与上传条件分别保持。测试覆盖进度9/10、去重跨登录进度、版本后上传顺序、禁用本地但仍上传、非同步本地写入、压缩读写、键校验与重复警告。396项（原267+关外129）通过，Unity退出0。账号/压缩/上传仍由必需host提供，具体管理器序列化及完整生产启动未完成。


## 统帅管理器原格式存档与实际文件重载（399项）
新增OutgameCommanderManager实现注册接口，按原初始化分流到服务器请求或本地读取；空记录才恢复UsedCommanderId1等默认值并补配置，保存将有效字典值重建原commanderDatas列表。扩展OutgameOriginalCommanders当前格式读写，保留等级/技能/isNew/出战ID；无效配置记录在保存时不再输出。测试覆盖旧记录更新重载、服务器等待回调、非同步本地初始化；另用原6条统帅配置与ModelRegister标记，经Pool→SDK队列写入隔离实际文件，以全新管理器重载验证。399项（原267+关外132）通过，Unity退出0。未接完其他管理器或完整账号主界面闭环，目标仍未完成。


## 皮肤管理器原格式存档与实际文件重载（402项）
新增OutgameSkinManager接入注册接口、实例发布、服务器等待/本地读取、原迁移与统一保存。元数据证实SkinData私有s/u为SerializeField，另外5字段NonSerialized；OutgameSkinCatalog新增独立原格式序列化，避免将恢复侧运行时字段写入原存档，newSkins/usedSkin按原PrepareSave构造且保留未知持有记录。原配置/注册标记经Pool→SDK队列写入隔离实际文件，全新管理器恢复拥有/新标记/装备；迁移不自动保存或改装备得到验证。402项（原267+关外135）通过，Unity退出0。LocalData及其余管理器与完整生产启动仍未完成。


## 本地数据完整序列化与限时礼包（405项）
新增OutgameLocalRecord，覆盖原LocalData全部18个序列化字段及首充/跳关/礼包嵌套结构；第19个dailyChallengeData原标记NonSerialized不写入。新增LocalDataManager对接原读取、限时礼包→道具→旧迁移→RegisterSaveData顺序与统一保存。限时礼包原规则：GameProductConfig存在且isActive==1，非零时间以1970-01-01 08:00固定起点加毫秒且严格晚于DateTime.Now；保存从有效字典重建列表，保留Add重复键异常。原商品配置已导入。测试覆盖完整字段/int64精度、时限等号/零值/无效商品/重复键，以及实际隔离文件重载进度/货币/首充/购买/礼包。405项（原267+关外138）通过，Unity退出0。DataManagerSave注册provider、其余管理器和完整启动仍待接通，整体未完成。


## 原始保存注册与三管理器启动迁移（407项）
新增OutgameDataSaveRegistry还原原始字符串/哈希注册，不触发保存；静态WASM确认String.GetLegacyNonRandomizedHashCode的UTF16交替累积、零字符截断及溢出算法，避免宿主随机哈希。保留初始化前null、初始化后缺键异常、覆盖旧值及null写入先修改原文后抛错的顺序。新增OutgameLegacyMigration按ProcedureStarGame f11032先皮肤后统帅迁移，并补统帅迁移后有效字典重建。测试通过原注册表选取本地/皮肤/统帅三实际管理器，一起加载、迁移、统一保存；本地注册原文不被保存覆盖，迁移结果正确落盘。407项（原267+关外140）通过，Unity退出0。完整页面初始化/资源初始化/场景进入、其余管理器及账号同步尚未完成。


## 数据就绪后页面与场景入口顺序（409项）
新增OutgameStartupEntry按ProcedureStarGame原顺序创建菜单→SetVisible(false)→通用奖励界面→旧迁移→预制初始化→GamePlay加载请求。请求返回后才设置EnterGame、LateInit和红点源字段；完成回调按原发送LoadGameScreen事件、报告当前关卡、排行/七日初始化、SetPlaySate(1)、关闭加载、报告可交互。验证了延后与同步回调顺序、回调时重读关卡、重复回调及迁移/加载抛错中止行为。409项（原267+关外142）通过，Unity退出0。实际UI/加载/activity host与完整OnEnter前置条件仍未接通，未宣称可运行完整关外。


## 原始模型缓存初始化与实际对象清理（412项）
新增OutgamePrefabCache还原LoadPrefabControl：ClearModelEntityCache仅销毁1000..6999及9033/9034编号模型根，先Destroy后清空并移除对应对象列表，最后集中移除根索引；保留其他编号实际对象。InitData随后清空三份字典，加载LevelEditor并仅关闭IsEditor。Unity实际对象验证覆盖边界、子对象销毁、外部列表引用、重复调用、缺资源及null列表的部分更新顺序。412项（原267+关外145）通过，Unity退出0；源方法索引1232。共享缓存/原资源生产接线、OnEnter前置与完整关外仍未完成。


## 原格式偏好存档与跨日检查（416项）
新增OutgameUserPreferences实现原UserDataList/Key/StrVar/IntVar/FloatVar结构，保留托管初始化一次、平台接管时延后、重复键覆盖、类型列共存、null字符串忽略与保存MD5提前更新。新增OutgameNewDay按SDK首次启动天数比较并重读，先写Day再发送GameEnterNewDay(new,old)，回退天数也触发，异常不回滚或重发。真实隔离UserData.txt保存/重启与失败后去重通过；416项（原267+关外149），Unity退出0，源方法1296。DataContract用于已验证JSON格式，完整原解析器边界/平台天数/文件路径及完整启动仍未接通，整体目标未完成。


## 偏好存档接通原WebGL文件与SDK队列（418项）
新增OutgameWebGlFileStorage按FileManager→ISDK303/304→微信Bridge→WXBase字符串存储传递原键，确认UserData.txt不拼接游戏名或账号目录。通过CreatePreferences接通现有原格式偏好数据；读取日志发生在取值后、写入日志在入队前。实际隔离文件验证缓存即时可见/落盘前新实例不可见/完成后全新实例恢复；异步失败保留缓存与MD5，报告不等待落盘，未变内容不重试而新值可继续保存。418项（原267+关外151）通过，Unity退出0；源方法1301。平台初始化归属、具体SDK天数/完整应用入口仍未完成。


## 微信平台偏好初始化与天数继承分支（419项）
完整继承链证实XYXCommon_DBT_ISDK直接继承ISDK的GetDaysByFirstLaunch=0，Bridge_WX_XYXFunction继承IXYXFunction.CanReadLocalData=true。新增OutgameWxPreferenceSession将此实际分支接入原偏好存档/文件键/SDK队列/跨日事件，不擅自引入自然日计时。验证旧Day12更新为0并发(0,12)，重复检查不重发，显式保存后新实例不重发；419项（原267+关外152）通过，Unity退出0，源方法1302。完整应用平台激活与登录、其他每日系统仍待接通。


## 原始广告位置表与桥接映射（421项）
导入WXAMSConfig全部59条原配置，新增OutgameVideoMapping按启动逻辑用ReportLable→PlacementId和Dictionary.Add构造映射；桥接保存原字典引用。恢复OnNewEvent中位置解析部分：正数直用，缺失/非正数分别记录原错误并回退68；未初始化字典及错误回调异常不吞掉。原表代表性商店/皮肤/道具位置、引用更新、重复标签及回退验证通过；421项（原267+关外154），Unity退出0，源方法1428。完整事件过滤/JSON上报与实际广告播放奖励、OnEnter连接仍待补齐。


## 激励广告请求与回调生命周期（424项）
新增OutgameRewardVideo恢复ADModule：构造就绪true/点击时间-1，先替换回调再按在线参数选择播放锁或严格1秒防重；接受后先写状态/时间，首日禁广告或未就绪返回false。底层未准备好时取消旧计时并等待缩放时间10秒，仍发起原生请求；展示失败保留回调，关闭先复位再回调/清理/发事件。字符串1/0回调后清空，-1报告unknow保留等待，未知结果不发奖；异常部分更新保持。424项（原267+关外157）通过，Unity退出0；源方法1499。平台适配器初始化、实际原生广告与业务领奖链路尚待接通。


## 底层广告控制器请求分流（426项）
新增OutgameAdRequestRouter恢复NewAdsManager：微信/小米遇遗留插屏状态先关闭视频再关闭插屏，支付宝/字节分支仅关闭插屏；关闭回调后重读状态。插屏仍显示调用closed(false)，控制器缺失调用shown(false)，有效控制器先写原offset20选项再Show，不添加准备状态门控。验证平台分支、关闭顺序与回调改变状态、缺控制器/阻塞分流及显式Ready调用；426项（原267+关外159）通过，Unity退出0；源方法1504。实际控制器/模块启用与完整领奖仍未完成。


## 广告入口至控制器调用链（429项）
新增OutgameAdModuleEntry还原WXFunctionManger缓存查找和启用门控，禁用时只警告不回调，Ready额外警告，警告后重读启用字段。新增OutgameRewardVideoControllerHost连接原ADModule与NewAdsManager：查底层Ready、清field64、Show选项false。集成覆盖入口→模块→路由→控制器→展示/关闭回调，展示成功不领奖、提前关闭失败、结束成功；缺控制器保留原待处理回调并取消超时，不发明完成事件。429项（原267+关外162）通过，Unity退出0。实际微信控制器创建/原生调用与具体业务领奖入口仍未完成。


## 实际视频控制器完成标志追踪
提取SerialVideoController/EchelonVideoController及闭包，源方法索引1516。确认Show先清byte92完成标志，奖励回调忽略传入bool而置true，关闭闭包忽略传入bool、转发当前完成标志；强制Close仅处理state4适配器，finish→清插屏显示→关闭回调，可能逐项重复。BaseController.isReady额外辅助调用及showAd选择/重试尚待解析，不能用已通过的路由测试替代实际控制器。保存video-controller-callback-audit.json；本轮仅证据推进，最新运行时仍429项通过，完整目标未完成。


## 广告就绪虚调用与适配器时间策略（431项）
解析table1746→wasm1545为虚槽分派，传入slot8对应isCacheRequest。新增OutgameAdReadiness保持(state2&&canShow)||isCacheRequest，不能只按加载状态判定。新增OutgameAdAdapterState保持默认cache=true/guarantee=false，仅保底非BANNER执行间隔判断，原否定大于表达式保留等号/NaN与日志后重读共享时间；finish先虚清理再state0，异常不重置。431项（原267+关外164）通过，Unity退出0；源码1635方法。BaseController.showAd完整选择/重试与实际适配器仍待恢复，目标未完成。


## 广告缓存排序与选择（433项）
新增OutgameAdCacheSelection，保持原地排序、最高价格选择、优先级轮转、保底排除、平台回退和短路条件。比较器保留null比较为0与有符号整型比较，避免相减溢出。新增竞价门槛、极值价格、轮转回绕、保底回退和平台优先覆盖；433项（战斗267+关外166）通过，Unity退出0。allPlatHasPrice暂为显式策略输入，完整请求队列、原生适配器和生产接线未完成，目标继续有效。


## 广告价格校验与本地价格缓存（435项）
默认缓存选择现已使用原始allPlatHasPrice规则：日志拼接后再次读价格，只拒绝0，负数有效，失败只记录已遍历项。新增OutgameAdPriceCache，按realPrice+zkey+platformId拼键，开关恰为1时追加本地日期；0反复读取，非0跨日期仍留在内存，显式清零后才重读。435项（战斗267+关外168）通过，Unity退出0。明确Utils_PlayerPrefs不是账户UserData，实际存储桥与原生适配器仍待接线。已解析虚槽9为isHighPriority，下一步恢复双遍请求队列。完整关外目标未完成。


## 广告真实请求队列（437项）
新增OutgameAdRequestQueue，按原始双遍getPlatList顺序收集非缓存高优先级/普通候选，Contains去重；非续请求先清队列，非空且zkey含INTERSTITAL才报告轮转请求。Handle先于移除，回调后重读队列并Remove原对象，失败保留待请求项。补齐适配器isHighPriority默认false。437项（战斗267+关外170）通过，Unity退出0。已确认微信回退候选还要求isGuarantee、state2、canShow；下一步组合完整showAd控制流与视频回调。完整目标未完成。


## 广告showAd组合流程（440项）
新增OutgameAdShowFlow，将原始忙碌检查、请求队列、微信保底回退、续请求失败报告、缓存选择、状态检查和展示后移除串通。保持INTERSTITAL首报告早于忙碌检查，VIDEO忙碌才回调false；INTERSTITAL4无回退续请求close(true)后报告失败，仍继续尝试缓存。展示前视频时间=-1，展示抛异常不移除缓存。440项（战斗267+关外173）通过，Unity退出0。实际原生广告生命周期、监听事件和视频控制器接线仍未完成，整体关外目标继续有效。


## 视频控制器与奖励入口接线（443项）
新增OutgameVideoController实现路由控制器接口，连接已恢复showAd流程，保持Serial/Echelon共有show/close语义。show先读取时钟再清完成标志，完成回调忽略bool并置true，关闭回调忽略传入bool读取当前完成标志；close仅处理state4，依次finish、共享showing=false、当前close回调，多候选不擅自去重。奖励入口→模块→路由→恢复控制器→展示流程已用受控适配器验证完整调用链和提前关闭、重复关闭、清理异常。443项（战斗267+关外176）通过，Unity退出0。仍缺实际SDK事件、初始化变体与配置连接，完整目标未完成。


## 微信原生视频适配器源码定位
新增WX_VIDEO_ADAPTER_AUDIT.json和字段证据，恢复索引1653方法。确认WxVideoAdapter直接使用WXRewardedVideoAd；空关闭结果不奖励，成功回调迟于关闭时不再次notifyShowAd，但仍报WXAms展示事件；ShowVideoFail空错误信息只记日志，不触发错误通知。已明确自动加载复用、注销关闭监听、销毁后清引用的顺序。下一步解析配置单例字段76、请求分支和基类notify/listener生命周期，再接入控制器。本轮无运行代码变更，最近Unity验证仍443项，原生SDK未验证，整体目标未完成。


## 微信视频加载与自动加载复用（445项）
新增OutgameWxVideoLoading，恢复单广告位关闭multiton、adunit-键、加载/错误监听、state5自动加载标志及0.1秒缩放延迟复用。清理遵守先Destroy后清引用，异常保留引用。解析3984736为SingleScriptObject<WXBuildSetting>.I，字段76确认为MiniGameCommonPlugin。445项（战斗267+关外178）通过，Unity退出0；源码索引1653。实际原生handle桥、DAU基类notify监听链和展示关闭分析上报仍待接入，目标未完成。


## 适配器状态通知与监听器顺序（447项）
新增OutgameAdNotifyingAdapter，和现有适配器共享SourceState，恢复请求成功/失败、展示、关闭、奖励通知。解析IADListener槽0/1/2/5/6；明确共享函数4475在此为stopLoadTimeout。保持监听回调后的条件重读、忙碌时先清理后state3、展示仅state2、关闭state5去重、奖励无新增去重规则。447项（战斗267+关外180）通过，Unity退出0。点击/展示错误通知、完整原生适配器及监听器实际接线仍待完成，整体目标继续有效。


## 展示错误分流与重复点击（449项）
补齐NotifyShowAdError和NotifyClickAd。展示错误先state5/停展示超时，按原文本、广告类型、平台458/459走关闭或bool展示失败监听器；保留VIDEO空消息绕过特殊分支、banner上报开关、无监听器不补reportShowFail。重复点击仍报new-click(true)，主点击与插屏等级事件仅首次。449项（战斗267+关外182）通过，Unity退出0。下一步仍需实际控制器监听器和原生回调组合，完整目标未完成。


## 控制器监听器与初始化路径源码审计
新增AD_CONTROLLER_LISTENER_AUDIT.json；Serial/Echelon及NewAdsManager.initAd共新增57个索引方法，总1710。确认共享展示失败路径先showFailCallback(true)，再就绪检查，重试清共享标志并showAd(false,true)；非缓存加载失败续showAd(true,false)。Serial关闭先业务close回调，再计时/共享标志、插屏链、视频预加载处理，必须保留重入顺序。控制器虚槽5/9和预加载同地址函数元数据仍需解析，初始化实际配置未定。本轮仅源码证据变更，最近Unity验证449项不变，整体目标未完成。


## 串行控制器关闭、奖励与重试（451项）
解析控制器slot5=load、slot9=checkRequest；Serial关闭中的共享3370为stopVideoShowLoad。新增OutgameSerialControllerLifecycle，连接现有视频控制器和show流程，保留业务回调先行、回调后重读isFromClose、插屏链、预加载停止及cacheAmount更新、load后恢复banner。showFail先回调true，再重读类型/就绪，重试showAd(false,true)。451项（战斗267+关外184）通过，Unity退出0，源码索引1710。Host边界下的实际预加载、报表和原生SDK仍待接入，整体目标未完成。


## 串行预加载扫描与协程（453项）
新增OutgameSerialAdPreload，恢复requestPlatList扫描、缺失监听器绑定、state0/3/5首候选先移除后Handle。播放预加载候选数至少2才执行，重读数量设cacheAmount=count-1，停旧协程后等WaitForSeconds(2)调用load，完成句柄保留至显式停止；停止抛异常保留句柄。453项（战斗267+关外186）通过，Unity退出0。仍需完整load配置、实际适配器候选与控制器Host连接，整体目标未完成。


## 串行load入口与候选重建（455项）
OutgameSerialAdPreload补齐Load/RequestAdapters/InitRequestList/IsRequestEnd/GetNowCacheNum。先给非缓存候选绑定缺失监听器，再重取列表选缓存state0/3/5；仅cacheAmount=1且非BANNER才首项截断，并非按任意容量截断。非空非banner先报告再serialLoad。455项（战斗267+关外188）通过，Unity退出0。完整控制器事件、原生适配器及实际配置连接仍待完成，目标未完成。


## 串行加载成功/失败事件（457项）
补齐OnReceiveAdSuccess/Failed，连到现有缓存和展示流程。缓存先去重添加再reqCallback(true)/CheckRequest，非缓存插屏只更新有配置的优先级；INTERSTITAL4先消耗自动展示标志再调用展示。VIDEO发送加载事件后重读时间，只有883/142且差值<=3秒、时间不为-1才自动show。457项（战斗267+关外190）通过，Unity退出0。实际适配器事件绑定与生产Host仍未完成，目标继续有效。


## 展示成功监听器组合（459项）
恢复Serial OnShowAd/OnRealShowAd，slot11确认为waitCloseReportShow。CreateListener将适配器通知连接到缓存、展示/关闭/奖励/失败处理。保持shown业务回调优先、类型重读、banner/timer、插屏上报、共享showing及视频预加载次序。459项（战斗267+关外192）通过，Unity退出0，已用真正的通知基类与监听器链验证；实际微信适配器、SDK与生产Host仍未接通，完整目标未完成。


## 广告请求与超时生命周期（461项）
新增OutgameAdTimeouts恢复Handle/show启动计时、停止句柄、WaitForSecondsRealtime超时与清理通知次序。加载超时仅state1处理；展示超时保留音频中断VIDEO转notifyShowAd(true,true)，普通路径清理→state5→关闭监听→超时上报。解析CustomParam.video_ad_timeout，保留空值5、解析失败0、负数和WASM浮点转整型边界。461项（战斗267+关外194）通过，Unity退出0。尚需实际微信桥/报表及控制器Host组合，整体目标未完成。


## 微信视频展示/关闭原生回调（463项）
新增OutgameWxVideoPresentation，将加载原生句柄与状态通知连接。关闭先isClosed/OffClose/清autoload，再按结果奖励、计数、请求预测，最后通知close；空结果只计总次数，提前关闭按MiniGameCommonPlugin分支报告。保留迟到shown仍报AMS、空ShowFail消息只日志、非空错误转通知。463项（战斗267+关外196）通过，Unity退出0。预测响应处理、真实SDK句柄和生产Host仍待完成，目标未完成。


## 微信视频适配器组合与控制器链（466项）
新增OutgameWxVideoAdapter，以同一对象连接加载、展示、通知、超时和价格缓存，并同时参与预加载队列与控制器缓存。通知取消直接作用于适配器自身的协程句柄。受控原生句柄通过真实Serial监听器完成加载→展示→奖励→关闭，另验证加载超时清理/重试和自动加载延迟复用。466项（战斗267+关外199）通过，Unity退出0。真实微信Runtime、原始控制器配置选择与预测响应仍待完成，未声称SDK/生产入口或完整关外完成。


## 视频控制器初始化与惰性平台列表（470项）
原包NewAdsManager VIDEO分支确认直接选择SerialVideoController并覆盖cacheAmount=1，先setAdz后bidding，NewAdsRequestDelayEnable为false时立即load。补充InitializeSerial初始化顺序与OutgameAdPlatformList：空列表每次重建，非空列表保留，INTERSTITAL变体归一，factory非空后init再canRequest。修正源构造默认cache1/videoShowTime-1/DAU-日志前缀；4个旧用例只调整已确认的日志前缀断言。470项（战斗267+关外203）通过，Unity退出0。完整配置解析、工厂、adapter init/canRequest和生产setAdz仍待完成，远端实际配置未知，目标未完成。


## 平台适配器源初始化与配置引用（473项）
补齐Wx继承DAU的init/canRequest：先保存IdsInfo/Adz引用和追加平台日志，再按day-clear字段或BANNER专属字段决定JSON解析；保留空字段旧值与TryParse失败写0。构造默认interval20/bannerError1；区分IdsInfo.platformId与原生883。价格键、优先级、请求id和展示超时读取当前配置，展示时重新解析customParam。473项（战斗267+关外206）通过，Unity退出0。DTO当前覆盖已恢复生命周期消费字段，完整远端配置与平台工厂、生产入口尚待接通，目标未完成。


## 平台适配器工厂与源列表连接（476项）
还原ADModule.createAdAdapter首日免广告门控、>=10001平台编号除100归一、横幅/插屏/视频/原生适配器类型映射；WXFunctionManger工厂入口使用缓存模块且不经过Active检查。验证88301配置保留原始平台编号，并通过工厂创建WxVideo、源init、惰性平台列表、Serial预加载到原生loaded回调进入缓存。476项（战斗267+关外209）通过，Unity退出0。非视频类型目前只确认工厂映射，构造实现未完成；真实SDK及完整生产启动连接仍待完成，目标未完成。


## 串行控制器配置绑定与共享平台列表（479项）
新增OutgameSerialAdConfiguration执行原始base setAdz后serial setAdz顺序：base标签、保留配置、初始化平台列表、仅非零数量时下调缓存上限，最后替换serial标签。show流程和惰性创建器共用同一可替换列表；工厂重入替换列表时追加到当前字段。保留空列表重复创建、非空列表重绑定不重建、配置失败后的中间状态，并让广告类型读取当前配置引用。479项（战斗267+关外212）通过，Unity退出0。真实生产服务组合、完整配置获取与全关外入口仍待完成，目标未完成。


## 广告请求完成检查与自动重载（482项）
新增OutgameAdRequestCompletion还原base checkRequest及reload：展示中计入1个容量；满容量除BANNER外报告成功并返回；不足时按非零/零缓存报告成功或失败，VIDEO零缓存事件false，之后重载。重载采用BANNER banRefreshTime或reqInterTime，WASM浮点截断后最少15秒，WaitForSecondsRealtime，保留完成协程句柄；替换前停止旧句柄。串行checkRequest可直接调用该组件，验证原生失败→检查→定时重试→serialLoad→成功入缓存。482项（战斗267+关外215）通过，Unity退出0。真实生产服务组合与完整关外仍未完成。


## 串行视频运行组合（485项）
新增OutgameSerialVideoSession组合工厂、源配置、控制器、监听器、预加载与请求完成/重试。Load、CheckRequest、show预加载、取消及平台数量由组合对象内部连接，不再交给报告Host空桩。受控原生句柄验证完整/提前关闭后自动重载、原生失败后定时重试，以及双适配器展示中预加载、关闭恢复容量并补满缓存。485项（战斗267+关外218）通过，Unity退出0。仍需真实SDK/报告服务与游戏启动配置接线；未声称完整平台或完整关外完成。


## 商店账户管理器源证据（无运行代码变更）
转回完整关外账户入口，提取StoreDataManager与StoreData/CoinData/FreeDiamondData及产品用户数据方法，方法索引增至1732。确认OnInit发布I再UpdateData(true)、OnRelease清I、OnSave原JSON；空输入先创建再反序列化，结果空再创建；新数据先重置钻石再金币。原SettingConfig为金币广告次数5、钻石次数3、奖励数组[20,20,20]。保存STORE_MANAGER_AUDIT.json及字段证据。仍需解析序列化属性/构造数组、实现管理器并接注册Provider与购买/领取/每日重置消费者。本轮未修改运行代码、未重跑测试；最近运行验证仍为485项，不能用其证明商店实现完成。


## 商店序列化与构造数组证据（无运行代码变更）
新增store-serialization-fields.json：coinData/diamondData、各count和lastRewardTime为公开序列化字段；maxCount私有且仅CompilerGenerated，无SerializeField；numArray标志0x81为私有+NotSerialized。新增store-default-array.json：usage4032304→fieldRef6→字段21124→默认数据80816，原字节恢复int[20,20,20]。已解除这两项实现未知，下一步按原始wire实现并验证Unity缺字段/空嵌套反序列化。未改运行代码、不重跑测试，最近485项验证不覆盖尚未实现的商店管理器。


## 商店数据模型与隔离存档实现（488项）
新增OutgameStoreDataManager及原结构StoreRecord/Coin/Diamond，保留全局配置读取、构造默认数组、Reset顺序、private/NonSerialized字段、empty create/decode/create、下载延后和实例发布/清空。Unity验证源配置5/3/[20,20,20]、保留领取次数/Int64时间、运行字段不入JSON、真实隔离文件重启。发现并纠正先前接注册计划：原StoreDataManager4092的registration=null，不自动注入账户管理器池；独立实现未证明当前游戏可达。488项（战斗267+关外221）通过，Unity退出0。后续需查手动注册/消费者及每日触发；注册池实际已实现管理器仍为原3个，不算新增激活。目标未完成。


## 已注册ItemManager与全局物品存档（490项）
确认ItemManager4500为当前游戏注册项，回调/init/save/release转GlobalItemManager，提取其完整方法并新增global-item-fields.json，方法索引1774。新增OutgameItemRecords恢复ItemManagerData/ItemUserData/ProductUserData/购买方式次数的原字段、列表与构造默认值；物品数量、价格/价格数组、时间戳为Int64，独立于LocalData Int32库存。空解码sentinel=-1与已有{}零默认不同，重复购买方式记录顺序保留。490项（战斗267+关外223）通过，Unity退出0。尚未接全局字典、时间/统计、奖励消费者与真实4500Provider；原注册池已接管理器仍3个。StoreDataManager手动可达性继续待查，未推断完全无用。


## 全局物品/商品索引与快照（493项）
新增global-item-init-generics.json证实Clear/Add/ContainsKey/列表迭代。OutgameGlobalItemIndexes恢复配置过滤、保留原记录引用、字典Clear后重建和重复键异常、快照字典替换及浅复制。Product UID零时缓存GetHashCode；更新快照仅更新原字段，保留haveBuyTimes写回live自身的原逻辑及未更新price/time。MemberwiseClone由与Object.MemberwiseClone同函数1361的原Clone确认。493项（战斗267+关外226）通过，Unity退出0。源初始化总流程、保存列表重建、时间/统计/奖励以及实际4500Provider尚待完成，目标未完成。


## 全局物品保存与时钟（496项）
新增global-item-save-generics.json，恢复先商品后物品的live字典列表重建、完成列表后更新时间戳和JSON序列化。原时钟有host时调用两次，否则使用本地时间；保留回调替换Data时旧对象接收时间戳而新对象序列化的行为。496项（战斗267+关外229）通过，Unity退出0。初始化总流程、时间转换/统计/奖励及实际4500Provider尚待完成，目标未完成。


## 原始物品时间戳（497项）
TimeHelper与TimeModule静态构造确认原起始时间为1970-01-01 08:00:00、Unspecified。直接DateTime相减后TotalMilliseconds截断Int64，不转UTC；反向AddMilliseconds。OutgameGlobalItemPersistence默认接入原转换，验证时区Kind、正负亚毫秒截断、64位往返与真实保存路径。497项（战斗267+关外230）通过，Unity退出0。全局初始化、更新/统计/奖励及实际4500Provider仍待完成。


## 全局物品初始化与释放（500项）
新增OutgameGlobalItemLifecycle组合原记录、时间戳、更新句柄、商品/物品索引与统计注册。统计事件10020查询接受首个boxed Int32参数，返回live Int64数量；null/空参数返回0。Nullable<Int32>句柄允许0，重复初始化不重复注册；保存Add失败、重复键异常、Remove失败的原中间状态。释放依次移除句柄、释放配置、清实例。500项（战斗267+关外233）通过，Unity退出0。Update/奖励具体实现、统计总线生产Host、实际4500Provider尚待完成，生产门未开启。


## 商品定时更新（504项）
新增OutgameProductUpdates并由生命周期构造入口注册。原Update累计deltaTime严格大于1才处理，每帧仅减1；仅比较DayOfYear。buyLimit1跨日刷新，3间隔、4指定小时、5星期调度，完成后及未过阈值帧调用IItemManager.Update。保留间隔Int32乘1000溢出、严格时间边界、仅一次补刷新、空参数先访问异常、小时模式day+1月底异常以及星期AddDays跨月。504项（战斗267+关外237）通过，Unity退出0。购买计数重置/价格重算/UI消息仍是待补齐的必需回调，生产Provider未连接。


## 商品购买计数重置与原记录修正（506项）
泛型证据纠正HaveBuyCounts为List<ProductBuyCount>，每项key+buyCount(List<ItemGetTypeBuyCount>)，内层才是key/value；旧扁平模型与黄金fixture已替换。新增OutgameProductResets，保留每日先buyCount后价格、3/4/5先价格后buyCount、模式2直接返回、未知模式仅通知；快照键按源config.id而非UID，通知依次RefreshStore/ProductReset。已验证定时更新串接真实计数重置、消息顺序及价格回调失败中间态。506项（战斗267+关外239）通过，Unity退出0。价格公式本体、奖励、平台及真实账号Provider仍待完成。


## 商品价格源公式审计（实现待完成）
PRODUCT_PRICE_AUDIT.json记录原数组/单一价格的六分支、计数来源、参数首项语义、Int32计算后扩展Int64与浮点转整型规则。product-price-generics.json确认ToList/RemoveAt/随机列表调用及ListArrayInt参数包装。源数组仅首次分配，已收集buyTypeOrder索引在循环内读取后丢弃，实际价格参数使用压缩循环索引；缺失购买计数查询返回未加入列表的新对象。原native power表109076映射wasmcode函数1055，已提取反汇编待精度验证。此轮为源证据推进，未新增运行时价格或测试声明，最近验证仍506项；随机边界、异常分支范围和完整价格接入待继续。


## 商品价格公式实现（509项）
新增OutgameProductPrices恢复数组与单一价格六模式、不同计数来源、压缩索引、仅null分配、缺失计数不入列表、Int32溢出及原浮点转整型门。原控制流证明数组单项异常记录后继续，单一价格异常保持旧值。与实际ProductResets组合验证，重置后价格在UI消息前更新。随机整数调用证实Next(min,unchecked(max+1))，随机列表/原生power精度与真实平台序列仍待对齐，当前作为显式依赖。509项（战斗267+关外242）通过，Unity退出0；首次新增用例循环价格预期算错，已按3%(3-1)+1=2修正fixture后重验通过。全目标仍未完成。


## 原共享随机源（511项）
价格泛型table7643映射wasmcode2673再调用10954，确认Next(count)后列表索引。RandomHelper静态构造创建唯一无参System.Random。提取GameRandomSource，BattleView默认及价格构造入口共用Managed，战斗显式RandomSourceOverride保持。验证包含上界、max+1 Int32溢出、空列表先Next(0)再索引异常与战斗/价格序列交错。511项（战斗267+关外244）通过，Unity退出0；不宣称原会话种子/时序一致，幂运算精度、真实配置及账号Provider尚待完成。


## 商品同名配置与加载证据
发现Proj_hdzd.GameProductConfig3943与ItemModule.GameProductConfig4531同名异结构。原表拥有项目商店字段而缺失common价格/refreshPeriod字段；禁止推造映射。item-config-loader-generics.json确认ItemConfigMgr三个格式分支都绑定4531。UnityJson的1333→12774使用Type.Name(slot8)查TextAsset而非FullName；ConfigRead静态默认UnityJson=false，故该路径仅为候选而非当前激活结论。ITEM_CONFIG_LOADING_AUDIT.json保留双类型字段、原表哈希/覆盖及下一步：追实际格式标志和generic/MemoryPack加载，同时回到项目商店消费者查验。此轮源证据推进，运行时未修改，最近验证仍511项，全目标未完成。


## 项目ShopItem原表与购买（513项）
新增OutgameProjectShopItem及Proj_hdzd原表DTO，读取原16条商品数据；SetData仅第一itemConfig，先显示price再Int32.Parse，保留失败中间态。点击先真实ToolDispatcher扣1002，成功才发第一奖励、Voice(1,2017)、额外ReportToolGet(category1,Global.CoinCost,当前库存)和Refresh。已验证余额不足仍保留分发器原通知/保存但不发奖，第二奖励不处理。513项（战斗267+关外246）通过，Unity退出0。控制器测试不代表完整页面可用，StorePurchaseUI卡片分类、原Prefab/账号绑定和其他礼包回调待继续。


## ShopUI原生商品卡片（514项）
追踪ShopUI协程33808确认仅storeType11进入商品列表，原表对应10001～10004；旧StorePurchaseUI协程33860只隐藏模板并等待一帧，没有构建商品列表。新增OutgameShopProductList和OutgameProjectShopItemView，按原Prefab的bottom/ShopItem与Content_Shop路径绑定价格、数量、图标请求和按钮。真实按钮验证依次扣1440钻石、加36张道具卡，余额不足不发奖，重建不重复绑定，不增设isActive过滤。514项（战斗267+关外247）通过，Unity退出0。完整ShopUI初始化、账号与图标/音频/上报提供者尚待生产连接，本次没有新Player构建。


## ShopUI数量刷新与广告飞行动效入账边界（515项）
新增OutgameShopFeedback绑定原txt_toolValue，按ShopUI.toolvalue1+ToolValue刷新；ToolChange忽略事件参数。两个广告完成回调成功才请求50金币/20钻石，保留原effectRoot、按钮世界坐标和true/0/true参数。追踪EffectControl.NewFlyTool确认目标为空不发奖，否则先分配ID/补默认root，再真实ToolDispatcher入账、通知/上报/保存，最后启动并登记动画/时间；不刷新顶部数值。新增OutgameFlyToolStart恢复此入口，验证动画启动异常不会回滚已到账奖励。515项（战斗267+关外248）通过，Unity退出0。实际动画协程、音频包装、平台视频注册及完整页面/账号连接仍待恢复；未生成新Player构建。


## 飞行图标收集与完成回调（516项）
新增OutgameFlyToolCollection，按源码恢复图标数量、显示初值、整数分摊、尾数补齐及到达顺序（回收图标→计数→目标动画→可选数字更新→完成回调）。收集不重复发奖。修正上一阶段临时Mode整数参数：原元数据字段为callback，实际为Action；商店传null，末尾布尔控制addMoneyShow。金币/钻石显示等于库存时才减去本次奖励作为动画初值，保留原Int32.Parse异常。516项（战斗267+关外249）通过，Unity退出0。动画池、实际Tween/清理协程和资源连接仍待恢复，没有新Player构建。


## 原版飞行货币资源（517项）
补齐钻石/体力原始资源包（11470字节），并连同已缓存金币包按原目录大小/MD5校验。恢复goldItem、diamondItem、strengthItem三个单节点Prefab与Image。钻石/体力图片以原renderDataKey、rect、pivot、ppu、border逐项一致且匹配副本像素哈希一致为依据复用导出像素，证据保留在fly-ui-import.json。新增OutgameFlyCurrencyAssets，将原加载路径映射到真实恢复Prefab。517项（战斗267+关外250）通过，Unity退出0；验证原尺寸、中心pivot、单位缩放和实际Sprite绑定。动画宿主、清理调度和完整关外连接仍待继续，本次没有新Player构建/动态视觉验收。


## 飞行动效清理控制器（518项）
新增OutgameFlyToolCleanup，恢复StopEffect仅排队、下一次Update先清理再扫超时的顺序；严格Time.time>检查阈值且年龄>4秒才排队，阈值保留原累计式old+(Time.time+1)。清理按子ID逐项Kill(complete=true)，再停协程、移除协程/时间字典及子ID。保留重复排队与异常中间态，不擅自去重。解析原泛型调用确认OnComplete/SetUpdate/SetAutoKill/SetId/Pause/SetEase。518项（战斗267+关外251）通过，Unity退出0。生产动画池、Tween及调度宿主仍待连接；无新Player构建。


## 飞行动效协程流程（519项）
新增OutgameFlyToolAnimation，按原协程串起图标生成、散开0.4s/OutCirc21、归位0.5s/InCirc20、收集和结束清理。确认等待类型为WaitForSecondsRealtime：逐个生成共享0.025s等待对象，末尾共享1.1s等待对象；Tween独立更新。真实恢复Prefab/Text参与测试，尾部先StopEffect排队再Kill全局Target_Tween(complete=false)。519项（战斗267+关外252）通过，Unity退出0。测试直接推进IEnumerator并由测试宿主触发Tween完成，尚未验证实际动画计时/动态画面；具体Tween/池宿主、随机散点、音频及页面组合待继续。


## 动效编号与圆形缓动（520项）
恢复OutgameFlyEffectIds：全局共享实例、ID从1开始、加锁后自增注册、分阶段fly_id_sub子编号、缺失计数-1及移除不复用。新增OutgameFlyEasing，按原EaseManager68862的浮点运算顺序恢复20/InCirc和21/OutCirc，保留原始求值器不夹紧越界输入。520项（战斗267+关外253）通过，Unity退出0。尚待具体逐帧Tween执行器、目标跳动、随机散点及生产页面组合，未做动态画面验收或Player构建。


## 飞行动效原生逐帧执行子集（521项）
新增OutgameFlyTweenRunner，真实Transform逐帧执行单次向前位置/缩放Tween，支持独立更新时间、暂停目标跳动和按ID停止/强制完成。源码确认已完成PlayForward不重启，目标放大保留不自动销毁，完成后另建缩小Tween；默认OutQuad来自原DOTween静态初始化。521项（战斗267+关外254）通过，Unity退出0，验证真实位置缓动、暂停时独立前进、目标完成不重启和强制完成仅一次回调。本实现仅覆盖所需向前单循环子集，不宣称完整DOTween等价；配置覆盖、安全模式细节、动画池/散点及完整页面组合与动态验收待继续。


## 飞行动效宿主组合与散点（522项）
新增OutgameFlyRuntime将真实入账入口、协程、原生Tween、共享编号和清理组合为MonoBehaviour；对象池与音频要求显式提供，未模拟成功。恢复散点原共享随机顺序：角0～360、半径10～20，再按angle*0.017f和radius/10f算y sin/x cos。原生math包装已提取，Mathf适配的全输入位级一致性仍未验证。522项（战斗267+关外255）通过，Unity退出0；验证散点调用顺序及宿主目标为空时先音效、不入账/不动画。尚未验证宿主成功路径的真实时间动画；NormalPool加载/回收、页面生命周期和账号连接待继续。


## 飞行动效真实Play Mode验证（522项机械检查 + 独立8项动态检查）
新增隔离批处理OutgameFlyPlayModeValidation，直接进入Unity Play Mode，真实协程/Update/Tween与恢复Prefab在timeScale=0下运行金币+钻石两路奖励。10个图标全部回收，真实Text到150/27，两次完成回调，库存只启动时保存2次；结束时活动动效和待清理均0。动态报告passed、Unity退出0，原522项机械检查计数不冒充增加为530。首轮因编辑器快捷键偏好权限异常打断delayCall，已改直接EnteredPlayMode启动并成功重跑；没有改用户偏好。池仍是明确的实例化/停用夹具，音频也是夹具，不代表原NormalPool、平台、账号或整页已恢复；未做匹配截图或新Player构建。


## NormalPool对象生命周期（523项 + 独立8项动态检查）
新增OutgamePooledPrefab，恢复原3665对象构造/生成/回收/释放：回收先记父节点、隐藏、SetParent(poolRoot,false)，取用时恢复尚存父节点再显示，不重置局部位置/缩放，释放转发原对象。验证父节点销毁分支与原局部变换；Play Mode池夹具的单纯隐藏回调已替换为此恢复生命周期。523项（战斗267+关外256）机械检查通过，8项真实Play Mode检查重跑通过，两个Unity进程均退出0。底层池复用选择/容量/到期、实际资源加载和释放提供者仍未恢复完整，未宣称完整池或关外已完成。


## 池对象基类初始化（524项 + 独立8项动态检查）
依据ObjectBase28691/28692，补齐空名称转换、Locked=false、Priority=0、DateTime.Now初始时间；构造只拒绝托管空引用，已销毁Unity包装对象仍可构造，生命周期继续使用Unity空判断。异常消息保留，异常类型暂用InvalidOperationException代替原GameFrameworkException，此差异明确未视为完全一致。新增边界检查，524项集成检查与独立8项真实Play Mode检查均通过，Unity退出0。原底层池single-spawn泛型构造已由metadata解析确认，具体复用选择、容量与过期仍待提取。关外完整目标继续进行。


## 原底层池泛型方法与占用计数（525项 + 独立8项动态检查）
新增extract_normal_pool_generics.py，从静态registration解析23个Object<T>/ObjectPool<T>共享方法，验证6294个函数指针。OutgamePoolEntry恢复初始0/1占用计数、取用先增计数再更新时间并调用OnSpawn、回收先OnUnspawn再更新时间并减计数、IsInUse=count>0以及原版重复回收负数行为。实际飞币Play Mode夹具接入此对象包装；525项集成检查和8项动态检查通过，两次Unity退出0。异常类型差异保留记录。下步解析池容器与泛型上下文，落实取用选择、容量/到期和真实资源加载；账号/页面生产流程、完整构建验收仍未完成。


## 原池集合、复用与释放（527项 + 独立8项动态检查）
通过GFRunning模块的50项ObjectPool RGCTX确认LinkedList、AddLast、枚举、节点读写、移除与对象方法绑定。新增OutgameObjectPool，恢复注册顺序的首个同名空闲取用、托管引用归还、容量/到期筛选、原选择排序、非缩放自动释放与Shutdown先移除后释放。动态飞币夹具已由独立包装改用此真实池实现。527项集成及8项真实Play Mode均通过，Unity均退出0。异常类和诊断日志仍有已知差异；资源加载仍为隔离依赖，原NormalPool加载/释放、页面账号接入与完整构建验收未完成。


## NormalPool加载缓存与回调（528项 + 独立8项动态检查）
新增OutgameNormalPool，恢复池命中、缓存资源与非缓存路径、新旧加载器分流、加载完成注册以及回收转发。源28687/28688确认空异步结果不回调，并发缺失不合并，先完成句柄留在缓存，每个回调仍使用自己的结果创建实例；28681/28684异常资源告警回调后继续的行为也保留。新增逆序完成与复用检查，实际飞币通过此适配层访问已恢复原资源，528项集成及8项动态检查通过，Unity退出0。底层资源加载当前为显式恢复资源夹具；旧加载分支尚仅静态恢复，DestroyObjectPool资源卸载顺序、原提供者、生产账号页面和完整构建验收待完成。


## NormalPool销毁与缓存释放（529项 + 独立8项动态检查）
解析28679及泛型绑定，新增DestroyObjectPool显式生命周期依赖：先请求管理器销毁，再销毁Unity存活根节点，新加载器逐个Release缓存句柄然后UnloadUnusedAssets，旧加载器逐个UnloadUnusedBundle；成功末尾只Clear旧ResourcesInfo字典。保留原版新句柄缓存以及重复调用、异常中断行为。529项集成检查通过，8项既有飞币Play Mode回归通过（动态8项不代表销毁验证，销毁顺序由集成探针验证）。额外提取ReleaseUIForm28674与GlobalUnityEngineAPI.Destroy27679，确认其最终是Unity空检查后Object.Destroy。原始方法索引2046。管理器/句柄原生提供者、旧分支动态覆盖、生产页面账号及完整构建仍待完成。


## 资源句柄生命周期（530项 + 独立8项动态检查）
提取AssetOperationHandle、OperationHandleBase、InstantiateOperation共31个方法，原方法索引2077。新增OutgameAssetHandle：正常模式有效性检查、provider释放后断开、失败保留连接、重复释放告警；兼容模式直读mainObject且不释放。参数默认Instantiate实际激活克隆并恢复原名，已接入飞币Play Mode的显式兼容模式资源适配。530项集成及8项动态检查通过，Unity退出0。兼容模式是验证夹具明确设置，尚不证明原启动模式；原加载事件、provider引用计数/平台加载和完整生产页面构建未完成。


## 资源加载完成事件（531项 + 独立8项动态检查）
依据22907/22900/22899恢复OutgameAssetHandle.Completed：无效句柄告警后抛原Exception；已完成订阅立即同步调用且不存储；等待状态保存多播委托；完成派发保留列表且不吞异常。验证回调内退订的快照语义、重复派发保留、立即订阅、抛错截断和释放后退订。531项集成通过，8项飞币Play Mode回归通过；后者不代表加载事件动态链已接通。provider完成触发、原加载器/引用计数、生产页面账号及完整构建仍待完成。


## Provider完成通知与引用（532项 + 独立8项动态检查）
提取provider2954的37个方法及CreateHandle泛型方法，原常规索引2114。恢复OutgameAssetProvider引用计数、List.Remove失败路径、终态判断、完成句柄快照、释放后跳过通知、Task延迟创建和回调后完成顺序。验证回调释放下一个句柄、异常阻止Task完成、全局抑制分支及引用错误。飞币Play Mode由显式兼容模式夹具改为正常provider创建句柄并派发完成事件，形成provider→句柄→NormalPool→动画链路，532项集成、8项动态通过。资源底层仍为恢复资源适配，原调度/启动开关、bundle依赖和完整关外生产流程未完成。


## Provider资源依赖释放（533项）
提取依赖组2939的8个方法，索引2122；泛型解析确认List<bundle2938>逐项枚举。新增OutgameBundleReferences，并在provider补齐Destroy23028：先置销毁，再扣Owner并清空，随后逐项释放依赖，成功后清空依赖组。保留重复项、unchecked负数计数、失败前部分修改和重试继续扣减，不擅自检查CanDestroy。533项集成通过，Unity退出0。此前8项Play Mode报告保留，本轮未重跑且不用于证明依赖释放。bundle获取/管理器调度、原平台加载与关外完整页面/账号验收仍待完成。


## ShopUI页面生命周期连接（534项）
核对运行脚本发现商品/奖励部件尚缺页面事件连接，新增OutgameShopLifecycle恢复OpenLater33786注册顺序：ChooseSkin、ChooseSoldier、ValnetineStatueChanged、ToolChange、金币视频、钻石视频。Dispose33773按序退订四消息，停止非空初始化协程但不清字段，最后访问SelectCardControl（泛型解析确认）。实际原ShopUI预制体已验证事件驱动ToolValue文本更新及回调身份。534项集成通过，Unity退出0；此前8项飞币动态报告保留不作为本轮页面端到端证明。原MsgDispatcher/UIVideoBtn宿主、初始化可见生命周期与真实账号菜单入口仍未连接完整，目标继续。


## 商店消息分发器接入（536项）
提取MsgDispatcher共28方法，索引2150，并解析Dictionary<string,EventHandler>操作绑定。恢复字符串事件Add/Remove/Send的多播规则、null委托异常、重复监听和回调报错中断；Shared惰性创建，ClearEvent替换实例。新增具体OutgameShopEventHost，原ShopUI经真实消息分发器驱动ToolValue刷新并在Dispose后停止接收。536项集成通过，Unity退出0；此前8项飞币Play Mode未重跑，不用于证明完整商店流程。索引型事件、原UIVideoBtn宿主、账号菜单入口及全流程构建验收仍待完成。


## 商店视频完成回调（538项）
提取UIVideoBtn及嵌套类35方法，索引2185。解析UnityEvent<bool>泛型绑定，恢复完成上报与奖励事件顺序、activeSelf条件、显式移除监听和销毁清理。原ShopUI视频组件经生命周期注册接入库存发奖：成功50金币/20钻石，失败无奖励，上报异常中断发奖。538项集成通过（关外271项），Unity退出0。此前8项飞币PlayMode未重跑。点击、冷却、DelayReport、广告平台及生产账号菜单入口仍待恢复，完整目标保持进行中。


## 视频按钮点击与延迟上报（542项）
恢复UIVideoBtn点击核心：无配置直接返回；音效先于冷却拦截；单按钮1秒、全局播放5秒，Update以unscaledTime严格超过截止时间才解锁；点击报告、UnityEvent和原Button回调后重新判断state2，再设置播放锁、上报和请求广告。异常保留此前副作用。确认DelayReport参数为Int32并修正之前Action<bool>接口，恢复scaled WaitForSeconds及创建报告→SDK开关→就绪报告顺序。542项集成通过，Unity退出0；原8项PlayMode未重跑。测试中的广告结果为显式fixture，不代表真实SDK。原Button适配、配置/状态/上报、广告平台、生产账号菜单及完整构建仍未完成。


## 原生视频Button组件（545项）
新增OutgameVideoButton继承Unity Button，恢复配置读取/锁定、重设ID、显示状态、音效、上报、回调及原生点击事件顺序。显式宿主提供SDK和上报接口，需在页面激活前绑定。验证普通Button不可交互仅抑制其自身onClick，原视频逻辑仍请求广告；成功上报使用覆盖参数，而分类事件仍使用原配置ReportLable；TrackVideo失败不推进报告状态。解析成功事件switch表，确认ShopUI金币videoID1001/钻石1006及AutoCallBtnShow。545项集成通过（关外278项），Unity退出0；此前8项飞币PlayMode未重跑。新组件OnEnable/Start动态流程、原页面组件接入、VideoBtnManager、真实SDK及完整账号菜单构建仍未完成。


## 商店原预制体视频按钮接入（547项）
更新共用导入器识别UIVideoBtn，ShopUI五个原视频组件保留ID、自动上报标记、Selectable颜色/导航、动画trigger及targetGraphic；原persistent onClick均为空，不支持的非空事件会显式拒绝。重导入关外四页面成功。新增OutgameShopVideoBindings在激活前绑定SDK宿主，并将原金币/钻石回调接入商店生命周期。实际ShopUI指针点击传出videoID1001，失败/未完成不发奖，成功50金币并通知/上报/存档。547项集成通过，Unity退出0。提取VideoBtnManager四方法，索引2189，解析ConfigRead<VideoBtnData>及字典调用；真实配置解析、SDK/账号入口仍待完成。此前8项飞币PlayMode未重跑，本轮SDK结果明确为fixture。


## 视频配置原数据与缓存管理器（549项）
解析ConfigRead共享泛型及RGCTXData，确认Datas对象JSON分支使用Unity JsonUtility.FromJson。原始VideoBtnConfig65条数据按字节复制到Resources，SHA256证据已保存；保留字段名，不臆造videoParam1/2到Video_param1/2的别名。新增OutgameVideoButtonManager，恢复惰性缓存、重复键首条优先、缺失日志、Reset仅清初始化标记、资源未就绪继续读旧缓存、失败重试以及null表初始化规则。原ShopUI点击到奖励验证改用原始表。549项集成通过，Unity退出0；此前8项飞币PlayMode未重跑。全局配置模式选择、二进制分支、真实SDK/上报与账号菜单入口及完整构建仍待完成。


## 广告视频请求与结果分发（551项）
提取AdsManager及嵌套类89方法，索引2278。恢复ShowVide替换当前回调→4秒unscaled请求门槛→共享0.2秒WaitForSecondsRealtime→GF_ShowAdsVideo→平台请求顺序。被门槛拒绝的请求仍替换回调。原结果字典仅ToString()==0成功；成功可选上报，失败将间隔设为当前时间，再广播GF_AdsPlayCallBack后调用当前委托；不清除回调、不去重。afterVideo先清SDK播放标记再解析，afterVideoFailed仅日志。原ShopUI加原配置的发奖验证已通过恢复后的广告核心。551项集成通过，Unity退出0；此前8项飞币PlayMode未重跑。原始字符串解析、Unity等待运行器、具体SDK/就绪逻辑和生产账号菜单仍待完成；平台和等待在本次测试中仍为显式fixture。


## 平台回调JSON解析（553项）
提取Util、IEnumeratorAwaitExtensions、SimpleCoroutineAwaiter和Json及嵌套解析器，索引2353。恢复GFRunning.Json/Util.DataParse：先日志再解析并as字典；重复键覆盖；不含小数点走Int64.TryParse，含小数点走Double.TryParse，按原默认culture且失败为0；保留字符串转义、Unicode、未知转义忽略、未闭合字符串返回部分值、多余逗号及尾随内容规则。原ShopUI、原配置、广告核心发奖验证改为输入平台格式JSON字符串经AfterVideo解析。553项集成通过，Unity退出0；此前8项飞币PlayMode未重跑。Unity等待运行器及真实PlayMode时序、具体SDK/就绪和生产账号菜单仍未完成。


## Unity广告等待运行器与实际PlayMode（553+独立7项）
恢复原单yield ReturnVoid、SimpleCoroutineAwaiter断言/异常传播和Unity同步上下文派发，运行器hiddenFlags61并DontDestroyOnLoad。新增真实PlayMode验证使用原ShopUI和原视频表：Start绑定就绪、原生点击后经真实0.2秒等待发送show事件、原始JSON完成回调；timeScale0期间实测请求等待0.2048666秒，scaled1秒上报保持暂停，恢复时间后正常执行。7项动态检查通过，Unity退出0。集成回归仍553项通过，未把独立动态检查累加为集成计数。此前8项飞币PlayMode未重跑。索引2389；具体SDK/就绪/上报平台、生产账号菜单和完整构建仍未完成，动态SDK返回明确为fixture。


## SDK按钮注册与就绪刷新（554项+独立7项）
提取DBTSDKManager/ISDK，索引2755，并解析List<Button>/字典操作。新增OutgameSdkButtonRegistry，保留重复注册/原生监听、注册→显示状态→平台通知顺序及Unity销毁对象倒序清理；CheckVideoIsReady和ChangeVideoBtnState分别检查功能5，先设置功能15，再刷新原视频状态接口为2或1。原生OutgameVideoButton实现状态接口，PlayMode宿主不再直接改按钮状态，改经恢复后的注册表。554项集成和重新运行的7项真实PlayMode通过，Unity退出0；原8项飞币PlayMode未重跑。就绪来源、完整SDK功能状态和具体平台播放仍待恢复，生产账号菜单及完整构建尚未完成。


## SDK功能状态缓存与刷新语义（555项+独立7项）
确认SDKExtension.IsOpen的bool参数为IsRefresh，不是默认值；修正之前关于SDK15 defaulttrue/defaultfalse的审计描述和生成脚本。新增OutgameSdkFunctionStates，缓存未命中或强制刷新时调用GetState来源，Set状态先按新增/覆盖上报日志再写入。PlayMode宿主通过恢复后的缓存和按钮注册表读取/更新功能15，替代直接恒定IsOpen返回。555项集成和重新运行的7项实际PlayMode通过，Unity退出0。源索引2757。完整GetState平台能力分发、其他刷新API、具体平台初始化与生产账号菜单、完整构建尚未完成；当前能力来源仍明确为fixture。


## SDK能力分发（556项+独立7项）
新增OutgameSdkCapabilityResolver，按原始GetState还原功能0到22及未知值处理；覆盖固定真、零值取反、绘制视频状态必须等于1、共享隐私策略查询和缓存强制刷新。556项集成与重新运行的7项实际Ads PlayMode通过，Unity退出0；历史8项Fly PlayMode未重跑。泛型证据确认SDK初始化依次添加Ads、Iap、Report、AppInfo、AppUser、SDKTool管理器。具体平台实现、完整刷新、生产账号菜单、持久化端到端和构建仍未完成；测试中的平台响应为显式fixture。


## SDK按钮状态刷新（557项+独立7项）
按23911还原ReshsdkFunctionBtnState：未注册无操作，逐项跳过Unity已销毁对象，读取缓存能力并设置激活状态；不强制查询平台、不清除列表。验证隐藏及重新激活、重复注册和已销毁对象处理。557项集成和重新运行7项实际Ads PlayMode通过，Unity退出0；历史8项Fly PlayMode未重跑。初始化协程证据确认先WaitForEndOfFrame，再按5/7/8/9/10/11/12/17/19/20/21/22注册动作，最后WaitForUpdate，其keepWaiting恒false。源索引2759；初始化协程实现、具体平台和完整生产链路仍待完成。


## SDK动作初始化（558项+独立7项）
新增OutgameSdkFunctionInitialization，按原始协程先WaitForEndOfFrame，再依次Add十二个功能动作，最后WaitForUpdate（keepWaiting=false）。验证帧边界前无注册、原生按钮消费同一动作字典、对应动作分发、重复键导致部分注册保留后抛异常。558项集成与7项新Ads PlayMode回归通过，Unity退出0；初始化自身的调度时序目前为迭代器契约验证，尚未接入真实生产启动。具体动作宿主、平台实现、完整关外生产与构建仍待完成。


## SDK动作宿主（559项+独立7项）
新增OutgameSdkFunctionActions，恢复原始十二个动作：CloseAdsTips静态偏移5先行阻止提示，随后检查缓存功能15并读取Sdk_NoAdsTips本地化文字；登录固定true，分享为空，其余严格转发原管理器方法。559项集成及新7项Ads PlayMode回归通过，Unity退出0。管理器接口仍是明确边界，尚未接入具体平台服务；不得据此宣称真实登录、内购或广告已完成。下一步恢复具体SDK管理器及启动组合，再继续生产关外全流程与构建。


## 小游戏广告回调桥接（560项+独立7项）
确认WX_DBT_ISDK、XYXCommon_DBT_ISDK与ISDK继承关系，并抽取XYXADControl/XYXLogin相关源证据，索引2885。新增OutgameXyxVideoBridge：广告请求先日志，独立捕获videoFlag，平台bool转换为字符串result（成功0失败1）和原JSON，重复回调继续转发。PlayMode不再手写原始JSON，而由该转换进入AdsVideoFlow和原生按钮完成链。560项集成及新7项Ads PlayMode通过；外部微信广告仍是显式bool fixture，实际BridgeManager平台选择与传输、生产账号菜单、完整构建仍待完成。


## SDK至微信模块链路（561项+独立7项）
确认Bridge_WX_XYXFunction64920/64921转发至WXFunctionManger，复用现有OutgameAdModuleEntry、RewardVideo、RequestRouter和ControllerHost，新增XyxVideoBridge直接连接入口的构造路径。验证SDK请求穿过所有恢复层，控制器取消/完成分别经JSON转换返回原始SDK失败/成功回调，保留门控时间语义。561项集成及新7项Ads PlayMode回归通过；完整链路测试的控制器为fixture，生产启动选择及外部微信原生服务仍未接入。源索引2888，目标未完成。


## 生产入口缺口审计（非新增通过项）
重新检查当前构建设置及场景序列化：唯一构建场景Assets/AreaBattle/Scenes/Battle.unity仅引用BattleView；关外启动与页面模块尚未组成实际生产入口。新增analysis/audit_outgame_production_entry.py和PRODUCTION_ENTRY_AUDIT.json，记录场景哈希、脚本GUID及九个关键类型的运行时代码引用。已有561项集成及7项Ads PlayMode结果保持有效但不能证明可玩的完整关外。后续优先恢复原始启动/账号数据/页面生命周期的生产组合、关卡返回与持久化，再进行完整Player构建。避免继续以可选SDK小接口增加覆盖替代实际入口打通。


## 生产入口：主界面开始按钮连接（562项）
为OutgameMenuView增加页面首次激活前的组合回调；新增OutgameMainStartBinding，使用原始btnStart/btnNewStart，按33685先SetPlaySate(3,false)后音效(1,2001)。实际导入预制体按钮验证与集成共562项通过，Unity退出0。此次未重跑Ads/Fly PlayMode，历史7/8项不得算作新证据。生产入口仍未接通；下一步优先恢复LevelControl.SetPlaySate状态3的执行链并接到现有BattleView，账号数据及主界面其余生命周期不可假定完成。


## 开始关卡过渡（563项）
新增OutgameLevelStartTransition还原OnGamePlayState状态3分支：加载界面字段48=true、40=5f，初始化敌方皮肤，重置LevelControl字段53，关闭现有菜单，0.5秒后读取当时的当前关卡、加载关卡并初始化特殊场景。源31479的第二bool参数实际上不参与分发，返回true；不能自行添加该参数的效果。563项集成通过，未新跑PlayMode或Player。此类仅恢复状态3分支；公共状态通知、特殊模式绕过、实际延时宿主与生产入口仍待连接。


## 玩法状态公共分发（564项）
新增OutgamePlayStateDispatcher，按31525先存当前状态、PauseGame(state==7)，依次通知Audio/WayLine/AI/Skill，再广播原请求值。解析ControlBase泛型确认PVPController和DiceGameControl的激活条件，任一成立跳过普通状态分支，仅调整摄像机。验证通知顺序、特殊模式短路、忽略原第二bool参数，以及回调重入后字段读取与原请求消息不同的语义。564项集成通过，未新跑PlayMode或Player。具体宿主、完整状态分支和生产入口仍待完成。


## 重试、下一关与返回主页（565项）
新增OutgameLevelContinuation还原状态10/11/12/14/15及31502回调，保留初始化与主/小关递增顺序、两种重试的索引差异，以及返回主页等待Hide后清理PlayUI/GuideUI、清除特殊状态、切状态2再Show的时序。泛型证据明确UI类型。565项集成通过，未新跑PlayMode或Player。完整分支、具体宿主和生产入口仍待完成，不能视为实际场景已能返回主页。


## 完整玩法状态分支表（566项）
新增OutgameLevelStateBranches补齐1/2/4/5/6/7/8/9/13，组合现有3和10/11/12/14/15。保留失败奖励资格、教学与普通开战、恢复/暂停音频、胜负界面与统计顺序、未知状态无操作。元数据确认音频虚槽7为AudioCompositeBase.Pause26386。566项集成通过；这里只证明完整分发表及契约，具体宿主仍未连接实际生产场景。下一步集中恢复主页关卡加载完成与实际入口宿主，不再把分支测试当作关外可玩验收。


## 主页场景对象切换（567项）
新增OutgameHomeScenePresentation直接操作Unity场景对象，复原GameControl31255/31256：可选字典100+offset60对象隐藏，Scene_game关闭，Scene_home/HomeCamera/CommanderCamera开启，GameCamera关闭。泛型证据确认Dictionary<int,GameObject>，测试真实Unity对象及无字典/缺失键路径，567项集成通过。另确认主页配置读取回调31533只覆盖配置并启动预加载，不应在该回调中臆造打开菜单。具体原始场景引用与生产入口仍未接入，未新跑PlayMode或Player。


## 原始场景引用接入（568项）
扩展prepare_outgame_model_roots.py与OutgameModelRootsImporter，导入原始Scene_game28、GameCamera46、CommanderCamera48及祖先变换，合计16节点；保留HomeCamera47和现有模型/背景引用。全部源文件哈希校验后重新生成OriginalModelRoots预制体，新增CreateHomeScenePresentation连接真实导入对象，568项集成通过，导入/验证Unity退出0。尚未连接生产场景启动，也未重跑PlayMode或Player；其余场景子对象及画面验收仍需完成。


## 菜单显示关闭生命周期（569项）
新增OutgameMenuVisibility还原33469/33482，关闭先请求主页、关闭页面、清页号、通过虚拟可见性路径隐藏；隐藏发送MenuTabDispose，显示依次打开页面、返回主页、检查弹窗并刷新锁/红点/级别门槛/新皮肤。569项集成通过，未新跑PlayMode或Player。OpenAllMenuItemUI原始实现为async仍待恢复，具体宿主与生产入口尚未接通，不可简化为SetActive完成。


## 菜单页面异步创建与复用（570项）
新增OutgameMenuItems还原32741/9802，主页、商店、指挥官依次创建，每个新页面后等待共享帧末指令；已有页面直接显示，最后按需创建物品信息页。关闭顺序保留两处旧页面字段且不关闭物品信息页。570项集成通过；真实帧调度、具体页面宿主、生产入口及Player端到端仍待验证，本次未跑PlayMode或Player。


## 原版UI缩放隐藏修正（571项）
恢复UIObject27434/27435、BaseUI27322与ExtensionMethods27678：先VisibleBefore再VisibleImp，之后提交可见标志并发送GF_VisibleUI；重复调用不短路。隐藏对象保持active且localScale为zero，显示恢复one。实际OutgameMenuView切页已替换错误的SetActive隐藏。571项集成通过（含真实导入页面和销毁对象处理），未新跑PlayMode或Player。完整BaseUI加载、页面专属刷新、异步页面宿主与生产入口仍未完成。此前MENU_ITEMS审计关闭源码路径误写为函数号9802，已更正为metadata32716。


## 页面专属可见性覆盖（572项）
已纠正571把基类缩放规则套到所有页面的问题：主页33678缩放隐藏并在显示时关闭btnPermit和RigthBar/btn_ads后请求刷新；商店33793使用SetActive且显示/隐藏都刷新；指挥官32828使用SetActive，隐藏关闭技能详情，显示先刷新选中项后刷新信息。原始OutletInfos证实广告按钮绑定外层同名节点；指挥官原始横向缩放1.01保持不变。572项集成通过。当前刷新通过事件交给页面数据控制器，实际消费者与生产启动尚未接通；未新跑PlayMode或Player。


## 商店菜单数据刷新接线（573项）
OutgameShopMenuBinding把实际菜单显示/隐藏事件接入已绑定的OutgameShopSkinLists；RefreshSkinStatus按33798先调用InitFirstAdsItem(0)，再逐组刷新兵种卡片，最后刷新场景卡片。实际导入菜单与卡片在初次隐藏、显示、再次隐藏时反映数据isNew变化，释放绑定后不再触发。573项集成通过；首次广告记录清空仍由显式回调宿主提供，验证使用fixture；生产启动、主页/指挥官数据连接与Player端到端仍未完成，本次无新PlayMode或Player。


## 主页实际节点数据刷新（574项）
OutgameMainInfoBinding把主页显示事件接到原始关卡双标签、引导文字、存钱罐和复活节入口。按33720保留服务读取顺序、LoadStartingUI通知时机与模式1重复选择普通面板；活动状态大于5保持显隐。实际菜单切页返回验证刷新、释放订阅及原始节点状态，574项集成通过。级别/存钱罐/活动服务与计时文本目前仍以显式provider接入，验证用fixture，尚非完整生产入口；本次无新PlayMode或Player。


## 存钱罐实际数据与主页服务连接（575项）
新增OutgamePiggyBank恢复30关门槛、胜利8累积20、数量上限1000、报告请求值后提交、重置数量和广告次数；使用原有profile.inventory.collectNum/collectAdNum，保存重载验证通过。主页原始UI集成改用OutgameLevelProgression与存钱罐真实状态提供器，验证满额及关卡标签。575项集成通过；生产游戏状态事件订阅、报告服务、活动时间与完整启动尚待接通，无新PlayMode或Player。


## 存钱罐游戏状态消息连接（576项）
OutgamePiggyBank恢复OnInit/OnDispose并连接现有OutgameMessageDispatcher的GamePlayState通道：初始化启用ActiveUpdate，胜利消息更新实际库存字段，退出解绑。保留原版重复订阅与单次退订语义，以及关卡门槛先于事件参数读取的顺序。576项集成通过。实际生产owner创建与发布状态、真实报告服务和完整进出关存档流程仍待完成；本次无新PlayMode或Player。


## 原版活动倒计时与主页文字（577项）
新增OutgameActivityCountdown恢复控制器4504的34377：状态3倒数至startTimeStamp，状态4倒数至endTimeStamp，使用服务器毫秒时间戳，保留负值、整数截断和分钟short转换。OutgameMainActivityTimer在真实主页Text上使用原始语言表按天/小时、小时/分钟、分钟/秒显示；状态小于2不改文字，其他非3/4状态归零且不读时钟。577项集成通过。活动状态与时间戳配置、ServerTimeModule、周期刷新及生产owner仍待完成，无新PlayMode或Player。


## 服务器时钟生命周期与倒计时连接（578项）
OutgameServerClock恢复初始化双刷新、暂停恢复、实时时长严格大于1秒才更新、SDK正值二次读取及非正值本地时间回退。首次日期观察不广播；后续跨日按RefreshNetTime、Time_NewDay顺序发送，调试偏移仅影响长时间戳接口。活动倒计时已验证读取此具体时钟。578项集成通过；SDK时间/网络/发布状态仍通过明确提供器注入，真实平台和生产tick owner未接通，活动数据与完整启动仍待完成；无新PlayMode或Player。


## 活动状态边界与原始周期存档（579项）
OutgameFestActivityState恢复4504的状态规则，读取具体ServerClock与LevelProgression：notice/start/end精确边界、关卡严格大于解锁级别、预热/解锁/完成报告顺序、断网3/4转6及重连行为。OutgameFestActivityData保留SummerData原始字段名和周期重置边界，JSON往返验证通过。579项集成通过；原始103002活动配置加载、完整FestActManager存储/奖励、事件初始化及生产owner尚未完成，无新PlayMode或Player。


## 原始活动配置读取与绑定（580项）
原样导入PubActivityConfig9行，OutgameActivityConfig恢复ReadActivityConfig32590与严格yyyyMMddHHmmss解析。活动103002原名情人节活动，开始2022-02-13、结束2022-02-27、预告2023-02-10、等级6，保留不一致日期；配置已能设置活动状态和倒计时对象。580项集成通过。当前只恢复本地配置读取，在线/本地策略选择、OnLateInit、活动存储奖励及生产入口仍待完成，无新PlayMode或Player。


## 节日领奖管理器与原始存储（581项）
OutgameFestActManager接入现有DataManagerStorage，恢复配置奖励数量、独立领取资格、原始发奖参数、次日午夜、上报和奖励编号递增顺序，以及特殊关卡位标记。SummerRewardConfig原样导入。验证发奖调用保存早于编号递增，后续显式保存/重载保留进度；581项集成通过。生产注册、真实ToolControl/UI/上报、控制器OnLateInit和完整Player流程仍待完成，无新PlayMode或Player。


## 活动后期初始化与数据池联合验证（582项）
OutgameFestActivityInitialization恢复OnLateInit34397及统计回调34396。配置、活动周期检查、状态计算、入口刷新、统计订阅按原序执行；实际FestActManager按原始4505注册标记加入DataManagerPool并保存共享状态。原始历史日期进入结束态；另用明确未来时间fixture验证旧状态先刷新入口、再计算到期、下次回调退订的顺序。582项集成通过。真实统计发布器、完整WarWin监听、生产启动owner和Player流程仍未完成，本次无新PlayMode或Player。


## 活动战斗结果与监听生命周期（583项）
OutgameFestCombatBinding恢复34390/34398/34405及34367销毁顺序。胜利先退订再按所选物品类型写入限定皮肤标记，读取原始奖励表解锁；场景皮肤依次播放语音、解锁、以已核实Global.Activity字符串上报。重复启用、回调重入、返回状态11、未知类型和原版销毁不移除WarWin的行为均验证。583项集成通过；真实生产owner、所选奖励入口、SkinManager/ADHelper组合和Player流程仍待完成，无新PlayMode/Player。


## 活动特殊关卡选择到胜利奖励（584项）
OutgameFestRewardSelection恢复默认所选奖励-1、索引选择及物品类型查询。OutgameFestSpecialLevelStart恢复FestActUI34020的监听、语音、特殊状态、配置关卡、镜头、直接关卡状态3和CloseSelf顺序。原表99998/99999经共享选择状态驱动既有胜利回调分别解锁310/6，普通关卡31保持。584项集成通过，实际活动页资源及列表按钮、生产owner/镜头/关卡组合和Player仍待完成，无新PlayMode/Player。


## 原始活动页面与实际按钮接线（584项重验）
取得ValentineUI预制体、背景及Shop_frame明确引用的Store2UI图集，新增183956字节均按原目录大小/MD5验证，依赖闭包7包。导入79节点、13绑定点、16精灵、1字体；无资源导出错误。OutgameFestSpecialLevelButtons恢复34033/34037的未完成位筛选、原始子节点和挑战按钮绑定。既有联合用例改为实例化真实预制体并触发Button.onClick，驱动选择/启动/胜利奖励；584项重验通过，未虚增计数。CanvasGroup导出字段为空、部分排版脚本未接、完整活动生命周期/真实指针/视觉比较/生产owner/Player仍待完成。本次无新PlayMode或Player。


## 活动每日领奖与原版可见状态（585项）
OutgameFestDailyRewardBinding恢复普通领取、广告失败/成功回调、当前奖励索引及倍数，调用实际FestActManager后按顺序刷新状态和列表。OutgameFestRewardVisibility绑定真实预制体每日按钮/已领取和皮肤位标志；保留原版第二皮肤完成仍操作第一行Claim/Mask/Chooseed的非对称行为。实际Button及恢复的VideoCallBack联合验证资格重检、原表2/4数量、跨日后当前奖励和授奖阶段保存旧索引，585项通过。列表图标/数量/天数刷新仍待恢复，广告底层宿主在该验证中为fixture，无新PlayMode/Player，生产入口未完成。


## 活动奖励列表刷新接入（585项重验）
OutgameFestRewardItems按34024恢复每日列表图标请求、天数/数量文本、当前奖励Image高亮、子节点5再4的完成遮罩，随后请求兵种/场景奖励图标。真实预制体联合用例在领取回调后执行该刷新，确认高亮与遮罩推进。585项重验通过；该用例图标setter/皮肤图标元组明确为fixture边界，尚未验证实际图标像素或完整生产入口，无新PlayMode/Player。


## 活动奖励原始图标落地（585项重验）
核实原Sprite.m_AtlasTags和已验证像素导入，建立FestRewardArt原始身份映射。OutgameFestRewardArt恢复34376图标配置查询，使用实际导入PublicIcon的heroSkill1/2/3、SkinAndToolIcon的qibing10和SceneSkin的tubiao06；联合用例已替换此前图标fixture，确认真实Image引用。585项重验通过。当前SetImportedSprite是本地已导入资源适配器，未冒充原异步UIExtension加载器；完整活动生命周期、生产入口、指针和视觉/Player仍未验收。


## 活动页Awake组合与关闭/跳转（586项）
OutgameFestPageLifecycle组合原版34026的预告/主页面状态门、按钮绑定、日期显示、进入上报、列表与可见性刷新，并恢复34034关闭语音和34019更多皮肤路由。原始预制体使用原配置2022.02.13-2022.02.27验证状态3/4页面切换；预告阅读直接关闭，更多皮肤按语音/关闭/查找菜单/打开皮肤页执行。586项通过。尚未建立实际UIObject销毁宿主和生产启动owner，无新PlayMode/Player。


## 页面立即关闭与活动销毁通知（587项）
OutgameUiLifetime/OutgameFestUiLifetime恢复BaseUI.CloseUINow27328、Dispose27338及活动Dispose34025；CloseBefore/销毁标志/Destroy请求/清理/排序通知顺序验证，保留原版rectTransform未清空及重复Dispose重复通知。UIModule39方法已提取，索引3110；CloseForName27401先从注册表移除再调用异步_closeUI，下一步需恢复27347动画/资源链。587项通过；该次Destroy为顺序探针，未声称真实延迟销毁或完整关闭生产接线，无新PlayMode/Player。


## 异步关闭状态机（588项）
OutgameUiAsyncClose恢复BaseUI27347动画分支、disposed标志、两次独立WaitForEndOfFrame、隐藏/Destroy/Dispose、现代/旧资源分支、动态与自定义资源释放和CloseUI事件。受控Task验证每个等待边界、两种资源路径及动画异常不中途伪造清理。588项通过；本次修正验证文件缺失Tasks命名空间后重跑成功。具体动画/await/资源宿主、CloseSelf/UIModule生产注册表接线及真实帧时序仍待完成，无新PlayMode/Player。


## 页面动态/自定义资源记录释放（589项）
元数据解析确认UIObject27441/27442记录类型为Dictionary<UnityEngine.Object,AssetOperationHandle>，OutgameUiResourceLists恢复逐值非空Release、UnloadUnusedAssets、最后置空字段顺序。实际AssetHandle/AssetProvider验证两组独立引用释放、空值跳过、外部字典不被Clear、null列表无卸载、空非null仍卸载和卸载异常保留字段。589项通过；卸载调度和生产页面所有权仍待接线，无新PlayMode/Player。


## 资源卸载入口与十轮清理门（590项）
追踪NewResLoadHelper/YOResourcesModule/ResourcePackage/资源系统，索引3236。恢复包为空跳过、Update后Unload、sourceFlag24为false原警告、true执行十轮清理；重复请求不合并。590项验证通过。内部资源系统Update22867及单轮清理22864仍为明确边界待恢复，未声称真实资源卸载完全实现，生产owner/Player仍未完成。


## 单轮资源清理与注册索引（591项）
OutgameResourceCleanupPass恢复22864两次倒序遍历：先TryDestroyAllProviders，再CanDestroy；预先保存键，Destroy后RemoveAt再Dictionary.Remove。验证倒序及跨阶段顺序、销毁中修改键仍移除原键、异常保留索引，接入十轮清理入口。591项通过。加载器内部22963/22962仍待恢复，资源系统Update22867和生产owner/Player未完成。方法索引3263。


## 加载器提供器清理与销毁门（592项）
OutgameCleanupLoader恢复2938的22963/22962：状态1/2、整组CanDestroy、提供器数量覆盖引用数、全体Destroy再注销并Clear；实际AssetProvider/BundleReference验证被持有句柄阻断整组和释放后的引用归零。自身CanDestroy检查终态、无正引用及关联ID逐一释放。592项通过；全局注销22860、关联查找22858、具体Bundle.Destroy与Update仍待接入，生产owner/Player未完成。


## 提供器全局索引注销（592项重验）
OutgameProviderRegistry恢复22860的列表Remove、读取提供器键、字典Remove顺序，泛型元数据核实。加载器联合用例改为具体注册表注销，验证实际提供器销毁后两份全局索引先清理、加载器列表再Clear。592项重验通过；原始键构建/注册、关联查找和具体资源包销毁/生产owner仍待完成，无新PlayMode/Player。


## 页面注册表关闭与重入顺序（593项）
OutgameUiCloseRegistry恢复27401先ContainsKey/get_Item/Remove再启动关闭，以及27326的CloseAction先于模块/原始类型名查找。泛型元数据核实Dictionary<string,BaseUI>；验证回调替换页面、重入/缺失关闭、异常不回填和实际异步关闭状态机两次帧等待。593项通过；生产页面创建/注册与原始类型名映射、具体关闭宿主和完整入口仍待完成，无新PlayMode/Player。清单移除已被新目标覆盖的旧“关外排除”标记，保留历史验证记录。


## 原版页面动画运行组件（594项）
OutgameUiAnimation恢复27424/27454及默认时长：CanvasGroup优先且getter固定0/1，缺失时倒序Graphic淡入淡出；缩放3/4保留原始各轴，分别OutBack/InBack。泛型和原始DOTween默认overshoot位值核实，组件接入Unity缩放时间Update与独立WaitForSeconds协程；直接驱动验证暂停、初值、默认时长、半程回弹和终点。594项通过，修正测试Image命名空间后重跑退出0。当前是有限原生Tween适配器，真实协程/帧序、完整关闭宿主与生产入口尚未验收，无新PlayMode/Player；索引3286。


## 具体关闭宿主与真实帧末验证（595项 + PlayMode6项）
OutgameUiCloseHost组合原生动画、共享生命周期、页面可见性和实际资源句柄；UiPage支持生命周期getter，Dispose后所有者与页面同时失去GameObject引用。现代/旧资源路径联合验证保留原顺序，595项通过。新增非batch隔离PlayMode：原始ValentineUI实例在暂停时保持资源，恢复后第125帧隐藏/Dispose、第126帧CloseUI；真实Unity.Destroy已生效且三组句柄归零，6项通过。首次受沙箱偏好写入和API更新提示阻塞，经批准启动参数处理后退出0；核对未发生无关C#修改。完整生产注册/账号入口、原生资源包和Player流程仍未完成。


## 页面加载完成与打开期间关闭竞态（596项）
OutgameUiOpenLifecycle按27321/27323/27343恢复现代/旧资源加载完成、迟到对象DestroyImmediate、空结果、缓存模式、初始化/Canvas/关闭Loading/Refresh顺序及打开协程。验证动画前捕获对象名、关闭后跳过OpenLater/OpenUI、OpenLater内关闭仍照原版发送事件。596项通过；初始化与调度宿主在该用例为明确探针，未声称生产页面加载已完成。具体Outlet初始化、Canvas和UIModule注册继续待接；本轮无新PlayMode/Player，关闭PlayMode6项为595检查点历史证据。


## 具体Canvas与UIObject初始化（598项）
OutgameUiCanvas恢复窗口层1、Canvas/Raycaster复用、UIWindow排序层和10+count*3或100+count*200两套间距。修正测试为真实父Canvas层级后通过。OutgameUiObjectInitialization按27428恢复共享对象/Transform、Normalize、RectTransform、Outlet字典Add、激活/初始化标志及组件/Initialize/可见性/皮肤/Awake顺序；非GameObject引用为null，重复键不回滚。Lifetime支持加载前空对象与Attach并共享Dispose清理。598项通过，原IUIoutlet序列化适配器和生产模块所有权仍待完成；本轮无新PlayMode/Player。


## 打开宿主与完整页面生命周期（599项 + PlayMode9项）
OutgameUiOpenHost组合共享生命周期、对象初始化、Canvas与原生协程；OutgameImportedUiOutlets从已导入原始manifest按owner/name/path精确恢复13项，不做模糊查找。缓存路径和真实打开/关闭联合验证通过：非batch PlayMode119帧OpenUI、193帧Hide/Dispose、194帧CloseUI；暂停门、原始虚方法顺序、原生销毁和句柄释放9项通过。599项集成通过。业务hook/资源提供器仍为隔离边界，生产账号、注册/加载、完整Player尚未完成。启动修正：官方-accept-apiupdate仅batch有效，先batch接受API更新并验证，再启动非batch真实帧测试；未发生无关源修改。


## 打开注册表与最大窗口序号（601项）
OutgameUiOpenRegistry按27398恢复类型解析后以短类型名查询、重复实例直接返回、新实例先Add再_openUI；Exception仅包围创建/注册/打开，记录Message+StackTrace并保留原状态。验证重入、与关闭共享字典、空值键Add失败不修复、构造失败及类型解析错误外抛。GetWindowNum27404核实是已加载层1页面的最大序号，不是个数，隐藏页面仍计入、Unity已销毁对象不计；OutgameUiWindowIndex已实现并接入Canvas用例。601项通过，实际类型目录/页面factory及资源路由、生产入口未完成，无新PlayMode/Player。


## _openUI请求与现代页面实例化（602项）
OutgameUiLoadRequest按27320保存Arguments→showLoading→捕获资源flag→模块/UIPath/层名→分支加载；现代主句柄严格在加载调用返回后赋值。OutgameUiModernLoader按27391/27409/27411实例化真实AssetHandle，隐藏、SetParent默认重载、铺满Rect锚点/边距，按27418两次yield null后回调。原始ValentineUI/13Outlet/Canvas/缓存初始化串联通过，另验旧资源分支不覆盖主句柄。602项通过；获取资源、_GetUINode和旧LoadUI回调仍为明确边界，完整生产入口未完成。本轮无新PlayMode/Player，真实页生命周期9项仍为599检查点。


## UI节点缓存与加载挂接（603项）
OutgameUiNodes按27394恢复Dictionary<string,Transform>查找、Canvas下精确Find、缺失时报原错误并缓存模块根Transform；不自动创建节点，不重查已缓存的缺失回退或已销毁节点。现代真实预制体加载用例已接入此缓存进入UIWindow子层，603项通过。启动27385还要求GFUICanvas标签根、UICamera及UI/UIRoot，当前目录无uiroot命名条目且TagManager缺该标签；尚待定位内置/打包根资源，未猜造生产根。无新PlayMode/Player，完整目标仍未完成。


## 原始UI根资源定位与静态导入（603项重验）
已从firstpack找到UI/UIRoot，level0:19/33定位启动UICanvas/UICamera，并保留resources.assets副本供比较。新增ui_root_evidence.py和原始根manifest；导入13节点、9个Canvas、8个GraphicRaycaster、3个遮罩图像，保存后重新加载核对通过。修正Canvas序列化事件/排序覆盖以及CanvasRenderer的Unity空值判断；603项集成回归通过。AdaptiveBangs、启动CanvasScaler/CanvasAdaptive/AtlasLoader部分schema及LoadUIRootOver仍待恢复，未接入生产启动、未新增PlayMode/Player。证据见generated/outgame/UI_ROOT_AUDIT.json。


## 原始UI根加载回调与启动字段（604项）
提取共享泛型LoadUIRootOver27386，恢复实例化UIRoot/true、存储RectTransform、SetParent/缩放/锚点边距、竖屏且CanvasScaler.match=1且固定宽度开关且宽>720时对称内缩、先初始化标志后完成回调。纠正OutgameUiNodes的Canvas类型为RectTransform；现代页面加载用例现在使用真实导入UIRoot和UIWindow分层。604项回归通过，原始分层挂在父Canvas后overrideSorting有效。ui_bootstrap_schema.py解出5类组件在场景/资源副本共10份payload：场景1080x1920/match1/固定宽false，资源副本match0/固定宽true，不能混用。CanvasAdaptive/AdaptiveBangs/AtlasLoader已提取，方法索引3380；行为和生产启动路径仍待实现，无新PlayMode/Player。


## 原版画布自适应与比例变化（605项）
OutgameCanvasAdaptation恢复74278-74283：获取Scaler、复用/添加AspectRatioFitter且保留enabled，设计比例覆盖序列化常数并发布横竖屏/固定宽度字段，按原分支选择宽/高因子。Update仅比例变化时适配→日志→GameAspectChange(null)，延迟诊断等待1秒缩放时间后读取当前尺寸。OutgameCanvasAdaptive提供原生Awake/Update桥接；生产bootstrap仍未挂接。验证同宽高比不同尺寸、1920/1080重复帧不误触发、禁用Fitter、缺失Fitter和横屏设计分支，605项通过。显式float舍入确保Update比较与源f32一致。下步导入启动Canvas/Camera并恢复AdaptiveBangs/AtlasLoader、确认场景与资源副本选择；无新PlayMode/Player。


## 原始启动画布与相机导入（605项重验）
RecoveredUiBootstrapImporter按ui-bootstrap-import.json分别导入SceneUICanvas/UICanvas与ResourceUICanvas/UICanvas，保留各自RectTransform、Scaler/Aspect/Adaptive参数。原m_Tag20010依据TagManager第10项确定GFUICanvas，已增补标签而不替换已有设置。原生相机/Canvas JSON使用类型包装与正确序列化版本，避免旧格式迁移改变视口/剔除掩码；相机引用、0..1视口、mask32、near0/far50、ortho6.4/depth10等重载验证通过。临时作者对象保持inactive，在持久化prefab上设置active，避免编辑器Canvas驱动覆盖原始根几何。现代页面联合用例现用真实SceneUICanvas→UIRoot→ValentineUI，605项通过。AtlasLoader保留原配置但行为未挂接，AdaptiveBangs与生产模块/账号入口仍待完成，无新PlayMode/Player。


## 原版刘海屏Start链路（606项）
恢复AdaptiveBangs28034→28028→28022→28012/28029：启动快照尺寸与初始offset，应用先恢复baseline；显式像素>=0按UIModule.canvas.sizeDelta/启动屏幕轴换算向上取整，否则按横竖屏长宽比>2使用85。发布AppSetting.BangsPixel/IsBangs后按Need/DoubleEnded改上下或左右边距。OutgameAdaptiveBangs已按原字段接入UIRoot，根加载回调绑定module canvas。验证重复应用不累计、双端、横竖屏、零值及阈值，606项通过。真实平台SetBangsPixel传输、原生Start时序和AtlasLoader/生产模块账号入口仍待完成，无新PlayMode/Player，未声称所有混淆辅助方法恢复。


## 原始启动根真实PlayMode（606项 + 12项）
扩展OutgameUiLifecyclePlayModeValidation使用SceneUICanvas→原始UIRoot→UIWindow下ValentineUI，真实Awake发布配置、模块挂接先于Bangs.Start。非batch运行通过12项：bootstrapFrame1，bangsFrame4/89单位（注入100px按真实画布换算），open57，hide/dispose130，close131；暂停时Start仍执行而打开动画等待，后续资源归零与帧末顺序保持。606项预检通过。真实平台高度输入、页面业务/加载provider和完整生产账号入口仍未覆盖，无新Player。已追到AtlasLoader现代模式不注册、旧模式订阅atlasRequested，缓存/helper22833及闭包4026764待继续，见ATLAS_LOADER_INTAKE.json。


## 图集请求/订阅与共享回调（607项）
OutgameAtlasLoader恢复22819/22842/22811及闭包22847：按当前资源mode订阅/退订；禁用时回调null；小写bundle查询键与保留tag大小写的reload路径；缓存命中或禁止autoLoad直接完成。异步共用实例callback字段，后发覆盖先发，两个完成都投递至最新回调；ResourcesInfo为空先警告但继续原失败访问。607项通过，SpriteAtlas泛型证据确认。缓存helper22833已解析firstpack优先且不回退，再查LoadedAssetBundle；实际缓存实现/资源获取/native组件绑定仍待补。无新PlayMode/Player；606检查点的12项启动PlayMode为历史证据。


## 图集首包/资源包缓存查询（608项）
OutgameAtlasCache按22833恢复typed Dictionary<string,AssetBundle>优先查询与原生LoadAsset<SpriteAtlas>调用；firstpack键存在即不走manager回退，null条目保留失败。debug开关下原版仅构造并丢弃字符串，保留bundle.name访问；普通缓存缺wrapper报包未加载，wrapper含null bundle报图集加载失败。实际cache缺失已串到OutgameAtlasLoader reload，608项通过。成功原生bundle加载分支尚未执行，包管理器/首包字典获取、atlasRequested原生绑定和生产账号入口待完成。无新PlayMode/Player，606检查点12项为历史证据。


## 原版UI模块初始化入口（609项 + PlayMode13项）
OutgameUiModuleInitialization恢复27385：重复初始化只调用当前完成回调；标签GFUICanvas/子UICamera发现、缺失错误、DontDestroyOnLoad和资源分支顺序；现代根加载接具体RootInitialization，旧路径LoadPrefab后重新取模块Update(0,0)，元数据slot7确认29880。609项通过。非batch PlayMode使用真实标签查找和跨场景保留，13项通过；原始画布/根/活动页完整通用生命周期继续通过。资源获取/泛型Instantiate仍由显式适配器提供，实际资源mode及Atlas owner、生产账号/菜单入口和Player待完成，未声称全目标完成。


## 原资源模式默认值与主入口证据
UseNewRes26113读AppSetting静态byte36，cctor26095未赋该字段，托管默认false。find_resource_mode_writers.py扫描两个WASM共90641函数体；唯一候选MineGameMain.Awake30610的store8 offset36实际写this字段，非AppSetting。尚未找到直接启用写入；扫描不排除间接/反射/外部覆盖，未声称观测到原运行时flag。生产baseline应按有证据的默认旧资源路径推进，之前强制modern的测试只证明分支行为。新增MineGameMain35方法，索引3415；Awake调用SDK.Init、设置AppSetting静态28、持久化主对象、UserDataPrefs.OnInit，再初始化4000636泛型模块并绑定4008020回调。后续优先解这两个引用及旧资源Instantiate/Owner/Atlas接线。运行时代码未变，609回归与PlayMode13项仍为上一检查点证据，无新测试/Player。


## 主入口模块初始化顺序（610项）
main-startup-generics核实11个模块类型；OutgameCoreModuleStartup恢复MineGameMain30610/30609/30629/30613/30625：VersionMondule→AssetbundleModule→ResourcesModule→UIModule逐回调推进，随后LangModule/FsmManager/MineGameLogicModule/TimeModule/EffectModule/ObjectPoolManager依次以null回调初始化，不逐个等待，最后ProcedureManager完成推进coreReady30612。UI模块实现共同初始化接口。验证回调先赋值、旧回调被null覆盖、顺序与重复完成不去重，610项通过；该用例其他模块是明确边界探针，完整provider/版本资源模块未构造。后续接Awake的SDK/prefs前序及30612后的Loading/config/账号入口。无新PlayMode/Player；609检查点13项是历史证据。


## 旧资源实例化与原生 UI 加载（612项）
恢复ResourcesInfo.Instantiate29863/29871与UIModule27392/27413/27415：每次按请求名加载，Unity-null/非GameObject返回null；复制后按active参数激活，保留原资产name，未虚构注册/所有权逻辑。OutgameUiLegacyLoader按UI/+path加载，日志使用资源AssetBundle对象，取basename并Instantiate(false)，默认SetParent、拉伸RectTransform、两次yield null后携带同一资源回调。OutgameUiLegacyRootResources将同一资源工厂接到UIRoot的Instantiate(true)与原零delta资源泵。两项新增集成覆盖空资产/错类型/销毁资产/别名/重复实例化/根与页面接线，612项全部通过。新真实非batch PlayMode通过8项：第1帧加载、第3帧完成，timeScale=0仍推进，原版页面13个出口绑定、启用与Canvas排序验证通过。测试资源来自明确的本地导入资产适配器，原生AssetBundle传输/所有权与完整关外账号主界面仍未完成；无新Player。详见LEGACY_UI_RESOURCE_AUDIT.json与analysis/outgame-ui-legacy-playmode-validation.json。


## 旧资源模块调度与回调（614项）
提取ResourcesModule43、AssetResLoader13、ResLoader8方法，method-map3489。legacy-resource-generics核实HashSet/列表/字典及原列表池调用。OutgameLegacyResourceScheduler恢复29880-29893/29905：初始100并发，路径小写与.prefab/.unity3d拼接、两次route检查、缓存loader、合并回调覆盖arguments、HashSet去重待启动、快照先登记全部current再Start(false)、FIFO容量调度、完成与错误记账。OutgameLegacyResLoader恢复ID计数和状态完成判断、FireEvent先清空Completed/Progress再调用、随后module完成或错误；回调异常不伪造finally。验证同路径合并/modern门控/回调新请求/缓存完成原样增容量/101请求容量/错误释放/回调异常后状态，614项全部通过。快照本地List代替原泛型池只保证顺序不声称分配一致。具体AssetResLoader获取包、ResourcesModule.Initialize/firstpack、生产关外入口仍未接通；本轮无新PlayMode或Player，612检查点旧UI原生8项为历史证据。


## 旧资源加载器生命周期与注册（616项）
OutgameLegacyAssetLoader恢复29849-29854状态分支：Start(false)入队，immediate原样直接Complete；Complete首次创建登记资源、设置Ready/卸载回调、清pendingBundle，复制arguments后基础回调；Error基础回调后重置state。ResourcesInfo卸载29864按原序清参数/Sprite/mainObject/bundle，通知后清回调，最后标记已卸载，Ready不重置。Scheduler补29894/29895/29898创建/逐值精确名称查询/卸载再删字典。验证缓存复用参数覆盖、卸载后重载、重复卸载警告、失败回调、卸载抛错的中间状态、immediate分支，共616项通过。原生包获取仍为显式Action边界；未实现Type3825 MoveNext的firstpack/异步operation协程，不能据此声称传输完成。无新PlayMode/Player。下一步核实MoveNext wasmcode15150的异常控制流与AssetBundleManager异步结果，再接传输。


## 旧资源包协程控制流（618项）
OutgameLegacyPackageLoad恢复AssetResLoader.LoadFromPackage29855及MoveNext29858/wasmcode15150：state2退出，firstpack优先且bundle.name日志位于try外；LoadAssetBundleAsync空operation则Error后退出，否则yield StartCoroutine(operation)，resume后try GetAssetBundle().Bundle、空bundle Error，catch发送GF_ResLoadError和错误日志，最后无条件Complete。metadata核实GetAssetBundle为slot10/23782。验证空operation、yield token、空bundle的Error→Complete双记账但单回调、结果异常日志事件后Complete、首包空项不fallback，618项通过。真实AssetBundleManager下载/依赖/operation提供者仍为显式边界，正向原生包加载未运行；无新PlayMode/Player。后续提取AssetBundleLoadOperationFull23795-23799和AssetBundleManager23839并接通正向原生包→资源→UI。


## 原异步包操作与登记（620项）
新增OutgameLegacyBundleOperation23795-23799实现包协程接口：构造只捕获一次UnityWebRequest；Update调用GetLoadedAssetBundle(name,out error,out int referenceCount)，只按wrapper是否null返回；MoveNext仅!IsDone，不查询管理器；结果wrapper非null则完成且进度1，不看其Bundle；无request或error=null或error="0"保持pending，其他error（包括空串）记录并完成，日志异常catch记录后pending。Progress取原生downloadProgress。OutgameLegacyBundleRequests23839保留debug日志原名→remap→LoadBundle(mapped,false)→构造→加入operation列表顺序。620项通过，测试创建/销毁原生UnityWebRequest但未发送网络请求。GetLoadedAssetBundle第三out参数确认为int；method-map3572。管理器Update/依赖/下载器仍未接通；无新PlayMode/Player。下一步23812/23843与正向原生包→UI。


## 包依赖查询与操作更新（622项）
OutgameLegacyBundleRegistry恢复23812：错误字典存在主包key即阻断（值null也阻断），缺主包返回null；主包有依赖则逐个检查wrapper，缺时第三out int置1，其余0。纠正前轮将该参数称referenceCount的解释，实为dependencyMissing，之前未使用此值。原循环重查主包error而非依赖error，不递归、不检查wrapper.Bundle，null依赖数组会抛错。新增Requests.UpdateOperations恢复23843的operation列表阶段：Update返回true推进索引，false原位移除，当轮能处理新增项。622项通过，包括注册表→operation从pending到done，连续完成移除和新增项。完整Update下载登记/清理尚未恢复，不能将此局部循环称完整manager。无新PlayMode/Player。下一步23843下载完成/登记/释放段与原生正向包加载。


## 下载完成登记与更新阶段（624项）
OutgameLegacyDownloadUpdate恢复23843下载遍历/登记/移除→operation更新→末尾cleanup顺序；原生OutgameLegacyWebDownload封装UnityWebRequest.isDone/isNetworkError/isHttpError/error和DownloadHandlerAssetBundle.GetContent。失败保留旧错误，否则写固定{name} is not a valid asset bundle.；成功先GetContent再检查已有记录，新wrapper的Bundle/name/refcount1/realtime字段按原版登记（无null bundle拦截）。遍历后才移除完成项，无本方法Dispose；异常传播保留先前登记，移除与后续阶段不执行。624项通过；用例下载对象是明确探针，原生适配器已编译但未完成实际请求。cleanup8246仍显式边界。无新PlayMode/Player。下一步用原版导入prefab重打本机包，实际UnityWebRequestAssetBundle→登记→operation→资源→UI跑正向链，再继续原版请求创建/依赖/variant/cleanup与完整入口。


## 正向原生资源包到页面验证（624项 + 原生6项）
OutgameNativeBundleUiValidation.BuildFixture从恢复的原版ValentineUI打StandaloneWindows64测试包analysis/native-legacy-ui-bundles/valentine；真实非batch PlayMode通过UnityWebRequestAssetBundle(file URL)读取2377959字节，走WebDownload→DownloadUpdate登记/移除→Registry/Operation→PackageLoad→AssetLoader→ResourcesInfo.AssetBundle.LoadAsset/Instantiate→UiLegacyLoader两帧→OpenLifecycle/ObjectInitialization。第1帧请求、第3帧资源就绪、第5帧页面回调，timeScale=0；13原版出口绑定、活动/Canvas排序、sprite/Text组件与资源owner均通过。PlayMode6项通过，随后624项集成回归通过，源码指纹已更新。仅本机重建Windows包映射一个源key，UI根仍来自导入Resources，业务hooks/尾部cleanup/unload边界隔离；不证明原远程URL/依赖/variant/平台传输或完整关外入口，无新Player。详见NATIVE_BUNDLE_UI_AUDIT.json与analysis/outgame-native-bundle-ui-validation.json。


## 包卸载与原生释放（626项 + 原生8项）
OutgameLegacyBundleUnload恢复23821/UnloadDependencies13169/内部8248/cleanup23851(8246)：先主包后直接依赖，内部按禁用门控与ready查询，dependencyMissing=1回退loaded，引用减1仅等于0排队，延迟来自Utility(type3130 usage3941128)静态int0；配置值尚待核实。cleanup倒序减Time.deltaTime，<=0加入临时列表，然后Unload(true)→按名移除loaded→可选日志→移除pending；不重查refcount。验证暂停、缺依赖仍释放、负refcount、不重复排队、倒序、异常保留状态。真实非batch NativeBundleUi测试替换先前noop卸载/cleanup，注入0秒delay，ResourcesInfo.Dispose→loader reset→refcount0排队→原生AssetBundle.Unload(true)→bundle Unity-null/loaded空，8项通过；626项集成通过。只证明本机重建包链，生产delay/disable/获取时取消队列、原URL/依赖/variant和完整入口仍待恢复。


## 再次引用取消延迟卸载（627项）
OutgameLegacyBundleReuse恢复23863/13170：主资源ref++，UnloadRemaining写原f32位1325400064=2147483648，pending.Contains后Remove一次；按root.BundleName取直接依赖，逐个GetLoadedAssetBundle返回非null才相同retain，不fallback未就绪依赖，不去重。验证关闭排队→再次retain→cleanup不销毁，原主/依赖计数与sentinel、blocked依赖跳过、重复依赖增加两次；627项通过。暂无新PlayMode，626检查点真实bundle卸载8项仍为历史证据。LoadInternal23836已读：loaded命中调用retain返回true，downloading命中时按第三bool决定是否增加静态48字典计数并返回true；否则13171解析记录，其field16非空字符串才GetAssetBundle/Add下载字典/SendWebRequest，返回false。下一步完整实现并接线，别把独立retain方法称完整获取链。


## 资源获取入口接入原生链（628项 + 原生8项）
OutgameLegacyBundleAcquisition恢复23824/23836：非manifest先校验manifest存在；loaded命中retain后true，downloading命中true并按第三bool是否false增加静态48的int字典计数；否则解析URL，非空时GetAssetBundle→Add下载表→SendWebRequest后false，URL空也false；Load只有internal=false且非manifest才LoadDependencies。元数据泛型证实Add/Contains/计数读写。真实NativeBundleUi已改用该入口，8项通过；628集成通过。Fixture仍明确提供本机URL、hasManifest=true、空依赖和0秒卸载，无新Player。读到resolver23813：ResourcesModule.bundleServices接口查记录，byte28<<4保存manager静态ulong40，field16 URL为空则日志[{0}]本地资源未找到 offset={1}并返回null。下一步恢复真实services/依赖/variant/配置，再原firstpack与关外启动。


## 依赖加载与变体选择接入（630项 + 原生9项）
OutgameLegacyBundleDependencies恢复23828：manifest门控优先，缓存数组直接重载；GetAllDependencies空不缓存，非空逐项原地remap后Add，再逐项LoadInternal(false)启用重复请求计数，无去重。Variants23871按dot第0/1段比较base/variant，active数组索引优先，rank相等取第一个；无active命中仍选第一个同base并记录原拼写Ambigious警告；同base畸形条目会抛错。630项集成通过。真实NativeBundleUi读取已构建manifest包，采用真实GetAllAssetBundlesWithVariant/GetAllDependencies，经恢复类接acquisition；9项PlayMode通过。Fixture把源key映射为valentine且无外部依赖/variant，非空分支仅集成源码用例验证。无新Player。后续bundleServices/record/resolver23813、Utility配置、原ResourcesModule firstpack与完整账号主入口。


## 原资源地址解析与卸载初始值（632项）
OutgameLegacyBundleResolver恢复23813：每次查询当前ResourcesModule.bundleServices(IBundleServices3819.GetAssetBundleInfo29828)，不改name，原样解引用record，将bool IsEncrypAB换算16/0写offset，再检查LocalPath null/empty警告并返回null；空白路径有效，异常不吞。仅投影BundleAssetInfo消费字段16/28，不声称完整记录构造。Utility3130 cctor23872确认AssetUnloadinterval初始60秒，运行期writers仍待审计，未修改native零延迟fixture。632项集成全部通过，源码指纹更新；无新PlayMode/Player，630检查点native9项仍历史证据。提取3817/3818/3820及Utility，method-map3658；已读两个services通过PatchManifest.TryGetValue构造记录与missing警告/空path行为。后续实现services初始化/记录、ResourcesModule firstpack、完整配置账号大厅入口。全目标未完成。


## 补丁清单与两种资源服务（635项 + 原生10项）
恢复PatchManifest29838-29840：首行版本/日期/GFVersion，第4列存在即加密，CR/空白保留；资源行少于4忽略，恰5独立版本，6列以上回退头版本，UInt64和Dictionary.Add异常保留。generic probe核实Add/TryGetValue。恢复services3818/3820：patch root无条件加/，streaming每次取path且不加/；成功完整填BundleAssetInfo字段与原debug日志，missing警告后空path记录，resolver再警告；metadata getter原split/trim，空manifest对象与未初始化区别保留，替换构造失败保留旧manifest。原生fixture改成显式synthetic patch文本→恢复services/resolver→真实文件URL请求；635集成与10真实非batch PlayMode通过，1/3/5帧，2377959字节，页面13出口/卸载仍通过。method-map3664，新增LEGACY_PATCH_MANIFEST_SERVICES_AUDIT.json。没有新Player；未声称原清单位置/远程配置或完整大厅就绪。下一步ResourcesModule.Initialize29875/firstpack协程与service实际选择/配置，再完整账号主入口。全目标继续进行。


## 首包协程与共享包别名登记（638项）
OutgameLegacyFirstPack恢复29876/29913：首次MoveNext日志加载firstpack→LoadAssetBundleAsync(firstpack.unity3d)，null操作报FirstPackError后无完成；yield StartCoroutine(operation)token，resume解引用GetAssetBundle().Bundle，无catch/空guard，实时遍历List<string>逐项registry登记，日志后完成回调。OutgameLegacyPackRegistry恢复29877/29878，Unity包相等判断、重复/冲突原拼写日志（aseetBundleName字面占位）和先读newPack.name顺序保留，不覆盖旧owner，new key允许null。新增泛型探针核实Dictionary<string,AssetBundle> Add/Contains/get和List<string> Count/get，JsonMapper.ToObject与Resources.Load<TextAsset>。638集成通过，其中真实已有测试包LoadFromFile验证重复/冲突登记并finally卸载；没有新PlayMode/Player，635原生10项历史证据。ResourcesModule.Initialize29875已完整读取：flag在firstpack之前置true，重复Initialize立刻回调；路径非空才Resources firstpack文本→JSON列表→协程。ResourcesMono47方法已提取，method-map3711；下轮继续Initialize与Mono实际连接、配置/账号大厅。全目标未完成。


## 资源模块初始化入口（640项）
新增OutgameLegacyResourceInitialization:IOutgameStartupModule恢复29875，route→manager accessor→ResourcesMono对象/GetComponent→flag=true→services firstpack.LocalPath；非空Resources.Load<TextAsset>(firstpack).text经注入原JSON解析边界得到List→FirstPack.Run→start后return，空path日志/当前callback。重复Initialize立即当前callback；等待首包/读取失败后flag仍true，测试验证替换callback与再次通知。默认OutgameLegacyResourcesMono.Awake按29955 DontDestroyOnLoad(this)，其余helper未声明完整复原；测试隔离createMono，native Awake尚未新PlayMode验证。640集成通过，源码指纹更新，无新PlayMode/Player。manager accessor/JSON解析器与原firstpack资源清单仍待生产接线；generated/assets按firstpack文件名有界搜索未找到，不等同全缓存缺失。下一步webdata Resources提取真实首包列表、服务/manager配置接线、原生启动验证，再配置账号大厅。全目标仍in_progress。


## 原版Resources首包/二包清单导入（641项）
使用既有UnityPy1.25.2 vendor和bundled Python3.12，从work/webdata/data.unity3d读取resources.assets TextAsset40 firstpack(235字节/7别名)、44 pack2(4321字节/85别名)，验证ResourceManager容器指针。recover_legacy_pack_lists.py原始字节导入Assets/AreaBattle/Resources/firstpack.txt及pack2.txt，证据ORIGINAL_PACK_LISTS_AUDIT.json。首包全部7alias在firstpack_ff021...真实container匹配（data/config对应85配置资产）；其余UIRoot、TipUI、2字体、2atlas均匹配。pack2含Proj_xqzdStartUI/MenuTabUI及公共战斗资源。OutgameLegacyPackListJson以JsonUtility包装数组解析两份已知合法原JSON，不声称完整LitJson错误输入行为；初始化null deserializer用此适配，默认Resources.Load取原数据。641集成通过，native TextAsset bytes SHA256与原文一致、alias数量顺序、真实默认reader→列表→firstpack工厂接线验证通过。指纹扩展包含.txt。无新PlayMode/Player；下一步实际manager/services配置与原生完整资源初始化，再主配置账号大厅。全目标未完成。


## 统一旧资源运行管线（642项 + 原生10项）
OutgameLegacyBundleRuntime将loaded/download/errors/dependencies/duplicateRequests/operations/pendingUnload共用状态，接resolver→acquisition→dependencies/variants→requests→download/update→reuse/unload。初始manifestnull、variants空、disableUnloadfalse（23806已核实）、delay60。GetRequest按23864字典查询当前native request。新增集成验证manifest门控仍创建operation、cached结果pump、60秒排队和再次retain取消卸载。NativeBundleUiValidation替换手工管线为runtime，保留显式本机patch/manifest映射、0秒delay，10真实非batch PlayMode通过；642集成全部通过。无新Player。读取Initialize23815/23856：每次创建AssetBundleManager对象、存static36、DontDestroyOnLoad；LoadManifest23835：已有loaded卸载true/移除→Load(name,true)→ManifestOperation(AssetBundleManifest)入队，尚待实现。runtime只代表一个共享state实例，未宣称原static/native启动owner完整；下一步manager创建和manifest operation，再资源初始化/原firstpack/配置账号大厅。


## Manifest异步操作与混合队列（644项）
提取AssetBundleLoadAssetOperationFull23789-23792与ManifestOperation23793/23794，method-map3717。OutgameLegacyManifestOperation按baseUpdate查wrapper/创建LoadAssetAsync(AssetBundleManifest,typeof)一次，derived等request.done发布manifest→false；IsDone无request时error!=null即日志/true，包括0和空串，Update却继续true。新增IOutgameLegacyManagerOperation共用普通包/manifest更新列表，不改变原remove-at循环。Runtime.LoadManifest恢复23835已有loaded卸载true再remove，Acquisition.Load(name,true)，新op入队，不提前清旧Manifest。644集成通过，原生请求适配编译；无新PlayMode/Player。泛型23788 GetAsset在23894经12088虚调用：helper已提取disassembly/GenericManifestGetAsset-23788.txt，但具体泛型body未解析；当前request.Asset as AssetBundleManifest投影的wrong-type/null精确语义须后续核实，已在audit明确，不声称完全。下一步真实manifest包异步下载→原生asset request→发布→页面链（替换fixture同步manifest），再native manager/firstpack/账号大厅。


## 原生异步manifest启动（644项 + 原生13项）
NativeBundleUiValidation去除同步LoadFromFile manifest，初始Manifest=null，patch加入manifestkey，Runtime.LoadManifest实际UnityWebRequest→download登记→原生LoadAssetAsync(AssetBundleManifest)→操作Update发布→页面加载。首轮第1帧manifest请求/第3帧发布，页面第3请求/第5就绪/第7回调；最终run通过13检查，manifest1570字节、page2377959字节，页面卸载后仅保留manifest记录。extract_manifest_asset_generic.py用原generic注册表解析23788为wasm20679；null/unfinished→null，asset经983/4119安全type-test再938checkedcast，wrongtype最终null。新增GetAsset保留泛型第二次isDone查询并补wrongtype单测，644集成与最终13原生全部通过。NATIVE_MANIFEST_STARTUP_AUDIT.json记录证据，旧getter未知项已消除。未新Player，localpatch/名字映射/0卸载仍fixture；尚未实际manager对象自动Update与原首包全启动，下一步native/static owner、ResourcesModule/firstpack，再配置账号大厅。目标持续in_progress。


## 原生管理器自动更新与ResourcesMono（644项 + 原生16项）
OutgameLegacyBundleManager恢复23815/23856：Initialize无单例guard创建AssetBundleManager对象/组件、保存最近ManagerObject、DontDestroyOnLoad；Update驱动SharedRuntime，composition先设置runtime。NativeBundleUi取消Editor Poll手动runtime.Update/卸载cleanup，manifest下载/发布/页面包登记/零delay卸载全由真实Mono.Update驱动，Poll仅scheduler和断言。资源初始化真实manager.GetComponent/default ResourcesMono.Awake，合成patch明确不含firstpack，验证原空路径callback分支；两个对象进入DontDestroyOnLoad场景，不声称另跑场景切换测试。644集成与16原生通过，第1manifest请求/第3发布、第3page请求/第5就绪/第7回调。新增NATIVE_RESOURCE_MANAGER_AUDIT.json。无新Player，非空firstpack7alias对应真实导入资产重打包与完整服务/config/账号大厅仍待接，Shutdown未完成；目标保持in_progress。


## 首包原始配置与字体资产（645项）
recover_firstpack_raw_assets.py从原firstpack容器提取85 TextAsset和2 Font payload，总3661101字节；新导入Resources/Recovered/FirstPack/Config/*.bytes及Recovered/Fonts/defaultNullFont.otf(1532字节)，已有HYZhuZiMuTouRenW.ttf(2879500字节)与源字节一致未改写。FIRSTPACK_RAW_ASSETS_AUDIT.json记录source container path、CAB/pathId、输出路径、size/SHA。Unity检查85 Resources.Load<TextAsset>.bytes与原一致、两Font可导入且磁盘字节一致，645集成全部通过；不声称字体renderer设置/视觉完全一致。sourceFingerprints扩展.bytes/.ttf/.otf。无新PlayMode/Player。UIRoot已恢复；PublicUI/PublicBtn原SpriteAtlas与TipUI prefab在项目仍缺，下一步补这些真实资产，才能重打首包7alias并验证非空Initialize→FirstPack→UI/config。禁止以Valentine包冒充全部原首包资产。完整账号大厅目标保持in_progress。


## 首包公共SpriteAtlas重建（645项 + 63 Sprite校验）
prepare_firstpack_atlases.py从既有asset-evidence提取PublicUI50/PublicBtn13 sprite原canvasPNG、name/pivot/border/PPU/rect，原始PNG字节复制Resources/Recovered/FirstPack/Atlases。OutgameFirstPackAtlasImport.Run生成两个原生.spriteatlas并PackAtlases(StandaloneWindows64)，全部63名称GetSprite/尺寸(绝对误差<.0001像素)/pivot/border/PPU通过，645集成回归通过。Shop_frame_A原宽127.999992对应128PNG，显式容差；首次打包因无默认平台maxsize导致native崩溃，已修复DefaultTexturePlatform4096/RGBA32未压缩并成功重跑。采用FullRect、不旋转、不tight、padding4，是重建pack设置，不声称原layout/compression/filtering完全一致。FIRSTPACK_ATLAS_AUDIT.json记录局限，指纹包括新atlas/png/importer.meta。无新PlayMode/Player，atlas在真实bundle/GetSprite尚待验证。下一步TipUI原prefab，再首包全部7alias实际重打与非空初始化，配置账号大厅仍未完成。


## 首包TipUI静态资产（646项）
prepare_firstpack_tipui.py从原asset-evidence层级产生tip-ui-import.json，13节点、6 UIOutlet（按source component→node ID解析）、5 sprites、1font及原fallback；RecoveredHudImporter新增ImportTipUiBatch到Recovered/FirstPack/TipUI，避开无关GuideSpine导入并独立限制说明。节点/控件/6outlets/字体和button targetGraphic真实实例化检查通过，646集成全部通过，importReport无静态skipped。Animation被显式标pending未混入静态成功：原导出schema为空，经UnityPy直读Animation3296681634379552887，enabled0，playAutomaticallytrue，default5580032477376658009，clipIds3221267864326137363/5580032477376658009，外部CAB-68a...源名hdzd_setPanelClose_ani/hdzd_setPanel_ani，原clipJSON已有，项目无对应.anim。TIP_UI_SOURCE_AUDIT.json记录；下一步按原曲线生成native disabled Animation并保留两clip，再首包完整build/nonempty启动，业务/视觉未验收。无新PlayMode/Player，目标保持in_progress。


## TipUI原生动画恢复（647项）
tip-ui-animation-import.json指向原两clipJSON并绑定SHA；OutgameTipUiAnimationImport导入hdzd_setPanelClose_ani/hdzd_setPanel_ani为native legacy AnimationClip，6 scale curves/27 keyframes（time/value/tangents/weights/mode）校验；InfinityMode复用既有Unity序列化探针。Content上Animation保留enabledfalse、playAutomaticallytrue、wrap0/physicsfalse/culling0，default开场clip，两个clip引用保存成功。ImportTipUiBatch后自动Attach以防后续静态重导入丢动画。新集成SampleAnimation检查open0/.3/1（0→1.2XY→1）、close0/1（1→0XY且Z1），两个clip.length均1秒，647全部通过。TIP_UI_SOURCE_AUDIT更新pending为业务/匹配视觉；无新PlayMode/Player，原实时播放触发仍需主UI生命周期接线。下一步首包原7alias资产已具备可重建版本，实际native bundle整体build→非空ResourceInitialization→alias→UIRoot/config验证，账号大厅目标仍in_progress。


## 首包原生构建与非空初始化链（647项 + 原生15项）
OutgameFirstPackBundleBuild将85配置、2字体、2atlas、UIRoot/TipUI共91原container地址重打Windows firstpack.unity3d（4635198字节，SHA 0df3a9abb648c34ce9dd9d3d1a20c16026c917bfef082825d5c038cb8ab20899），原生LoadFromFile全部验证。OutgameFirstPackStartupValidation真实非batch PlayMode：native manager.Update异步manifest→首包→ResourceInitialization非空分支→原Resources firstpack列表→7alias共享同一包；第4帧初始化完成，scheduler通过alias加载UIRoot/85配置并逐字节SHA检查，第7帧TipUI两帧隐藏加载后OpenLifecycle绑定6outlets/激活，native两动画和PublicUI/PublicBtn GetSprite通过。只有manifest+firstpack两请求，无alias额外下载，15项通过；647集成回归通过，源码指纹更新。明确fixture使用local synthetic patch、local scene canvas和Editor scheduler pump，不是完整生产入口；未新Player，不声明原服务/账号/大厅完成。NATIVE_FIRSTPACK_STARTUP_AUDIT记录边界。下一步Main30603配置回调/ConfigMgr与30617实际接线、平台服务选择及账号大厅，目标保持in_progress。


## 配置入口完整清单与泛型读取证据（源码分析检查点）
recover_config_initialization.py解析ConfigMgr.Initialize30413所有执行调用（不是cctor预热常量顺序），将64个MethodSpec/类型/owner字段offset关联原始配置SHA：58张表共2724行，6单对象；其余首包21份配置未在本入口直接加载。CONFIG_INITIALIZATION_ROSTER保存原序、metadata字段/类型编码、UniqueID方法路径与payload字段差异。extract_config_read_generics.py提取13个shared generic bodies，method-map3889（含64配置173方法）。SetConfigABRes30427写ConfigRead静态field4；Initialize每次先ConfigRead bool0=true、AppSetting bool39=false，再按instancebool8门控；表load→6value赋值→30401顺序调用30439/30406/30398/30426/30428→最后flagtrue。30603日志ConfigLoadOver→SetConfigABRes→Initialize→Mainbool36=true→30617。读取26610按typeof(T)名称取TextAsset再parser(type1)，随后read-count++；26614取文本首个{起FromJson，缺失日志并返回null。未实现runtime配置管理器，不声明解析/字段忽略完全等价；下一步泛型字典helper上下文/UniqueID、typed schemas、五后处理与Main接线。此次未改Unity代码，无新Unity/Player检查；647集成/15首包PlayMode仍上检查点历史验证。目标in_progress。


## 原版类型配置解析落地（650项）
ConfigRead RGCTX确认26610→26626(type1)→26629→26619→JsonUtility.FromJson<Datas wrapper>；Dictionary.ContainsKey/Add和List访问已核实。generate_original_config_schemas.py从metadata生成OriginalConfig命名空间66类型（64入口类型+嵌套Lang/其他引用），全部字段primitive/array/List按原类型解析；58表UniqueID getter均field8装箱Int32，64配置ctor均nop。OutgameLegacyConfigRead实现类型basename TextAsset读取、首个{截取、Datas列表→既有Dictionary<object,T>；重复只日志保留旧值、缺失日志后table计数++、异常不计数；value缺失null、不增加计数。真实重建firstpack LoadFromFile验证全部64类型，58表总2724行及intkeys；重复/重读不清空/缺失/异常计数边界检查通过，650集成全部通过。LEGACY_CONFIG_READ_AUDIT与CONFIG_SCHEMA_GENERATION保存证据。未新PlayMode/Player，注入load仍需ConfigRead静态owner/现代route及ConfigMgr有序接线。注意30439是平台设置应用，不能将五后处理全称索引；下一步30439/30406/30398/30426/30428与Main30603/30617实际composition，完整账号大厅尚未完成。


## 在线配置覆盖与三个派生处理（655项）
OutgameConfigOnlineOverrides恢复30439：先按Hero/Boss/PreUnlock/Valentine/afterTool/afterVideo/everyLevel/NoRemoveAds读8项，最后值丢弃；再Boss Int32.Parse→Hero Boolean.Parse→其余5int，null/empty不改，空白尝试Parse，错误保留早前写入，不catch。测试8次全读后改、bool数字1失败、缺失不解引用nullconfig、末尾provider异常阻止所有修改。OutgameConfigDerivedIndexes恢复30398清dic_res后每SceneSkin idleIconName/gameIconName Add raw PathEnum12（原generic<string,PathEnum>此边界以int表示，重复/空key异常不回滚）；30426先清this316再取singletonGuideConfig156.Values，guildLv仅-1跳过、indexer最后覆盖；30428中文名逐项Append、不清空含null。5新case+650既有=655全通过，无新PlayMode/Player。CONFIG_POSTPROCESS_AUDIT记录。余30406仍含30410阵营色/30435 GlobalValue常量，不能用noop完成ConfigMgr；完成它们后64读取有序owner/SetConfigABRes、Main30603/30617、账号大厅生产接线仍待做，目标in_progress。


## 全局常量与阵营颜色/材质调度（657项）
OutgameConfigPresentation恢复30435原顺序：GlobalValue101→static8 gameTimeScale→copy4，102→12 ship，110→16 star，113分号parts0→20 wide，parts1*gameScale→28 move→copy24，parts2→32 tiling，wide*.5→36，最后动态Global.ShareURL取Content1→40。Global4272cctor确认CampSumNum9、ShareURLkey200。30410两独立inclusive循环：0..当前campSumNum逐次nativeTryParse(#LineColor/三soldierColor)，不清dict，全部完成后再逐项BasitionMaterial请求附args[id,path]，不等待完成。实际原配置常量/10阵营、所有颜色先于材质请求、旧key保留、非法色TryParse输出、float中途失败保留早前写入均通过，657集成全通过。CONFIG_PRESENTATION_STARTUP_AUDIT记载。注意30407材质completion还缺ResourcesInfo泛型参数、ToFileName、LoadAsset<Material>及dict280接线；当前loadMaterial是明确边界，不宣称实际材质加载完成。没有新PlayMode/Player。下一步30407及64配置owner有序初始化、Main30603/30617、账号大厅，目标in_progress。


## 统一配置初始化与材质回调（659项）
ResourceAssetGeneric恢复ResourcesInfo29869 GetArg<T>缺数组/超过末尾default、存在直接cast；29862 LoadAsset<T> nativeBundle Unity-null→null，否则native泛型加载，不使用importadapter。UtilitHelper.ToFileName只LastIndexOf(/)后Substring再Split(.)[0]，保留反斜杠/首dot语义。OutgameConfigMaterialCompletion30407按intArg0/stringArg1→basename→LoadAsset<Material>→dict[camp]（包括null）。OutgameLegacyConfigManager用明确shared legacy read state，SetConfigABRes写state资源；Initialize先写UnityJsontrue/MemoryPackfalse再guard，生成58原名dic+6value有序读取，online→camp→globals→scene→singletonGuide→names→最后flag。generator全部OriginalConfig限定，修复与既有AIConfig/DispatchConfig/AgentSkillUseConfig同名冲突。原生firstpack LoadFromFile验证全64配置manager，10材质请求收集、30场景资源、引导/中文名/速度；repeat重写flags但读count仍58，不等材质，晚到null回调缓存通过，659全通过。LEGACY_CONFIG_MANAGER_AUDIT记录：material fixture仅收集未真实非nullnative加载，online为fixture注入无值，modernroute未恢复；无新PlayMode/Player。下一步异步firstpack→Main30603统一config接线、LoadMaterial真实route、Main30617账号/大厅；完整目标in_progress。


## 异步首包接入Main配置回调（660项 + 原生18项）
OutgameMainConfigStartup恢复30607 LoadingUIShow日志→SDK U3DGameStart边界→LoadAsset(data/config,callback,Array.Empty<object>)，usage3989352泛型确认3356 Array.Empty；30603 ConfigLoadOver→当前manager.SetConfigABRes→再次取manager.Initialize→Main.ConfigLoaded=true→30617边界。异常不设flag/不推进。FirstPackStartupValidation实际异步下载后的config alias资源经Main回调交统一manager，58表+6value完成及派生索引/全局常量验证；第4帧首包就绪、第7帧TipUI，只有manifest+firstpack两下载，18项真实非batch PlayMode通过，660集成全通过。MAIN_CONFIG_STARTUP_AUDIT记载：SDK/online缺省/10材质请求收集及30617仍fixture边界，未实际平台/非null材质/账号大厅。无新Player。30617后续源码调用已定位：30628 settings→30622 procedures→30608 controllers→Audio→InitCtrl(true)→Main flags38false/24true→LangFont→CommonSettings→Report→ExitGame监听→泛型procedure转换。下步恢复该composition，完整目标in_progress。


## 进游戏设置与完整注册清单（662项）
OutgamePreGameSettings恢复30628：ReportManager静态4=2→nativeInput.multiTouchEnabled=false→GetLargNum30432新Dictionary Add(mag,magName)→BigNumExtension.SetBigNumSymbol→nativeApplication.targetFrameRate=60。BigNum Set原8189先Values.ToList写list，再保留原dictionary引用，最后Enumerable.First().Key（不是最小key）；空map会在前两写入后抛错、保留旧FirstMagnitude、不设fps。两case验证原LargeNum首3/K、native设置并finally恢复、字典alias/listcopy/顺序及异常，662集成全部通过。recover_pre_game_registration按实际935→1622执行调用解析38个ControlBase<T>.get_I依序注册（含4混淆类原type保留），非预热常量顺序；30622原procedure顺序PreLoad/StarGame/ExitGame，30617末usage4009776为StartProcedure<ProcedurePreLoad>，PRE_GAME_REGISTRATION_ROSTER保存。新method-map3893。尚未真正注册全部controller/procedure或接通30617，ReportManager也仅明确callback边界。无新PlayMode/Player；下一步ProcedureManager/Register/Start及MineGameLogicModule/Register/Init生命周期、预加载/账号大厅，目标in_progress。


## 控制器注册与生命周期（665项）
OutgameLogicModule依据3499/26939..26946恢复Priority12、无guard重新Initialize清注册列表并先flag后callback；Register Contains重复警告、先Add后可选OnInit；InitCtrl先写auto再DataPool.OnInit，false日志快速模式并跳过控制器，true使用live index/count让初始化中新增对象同轮初始化。Update使用foreach并传两delta，列表修改抛异常；Shutdown先DataPool release再逐个Dispose，只有正常结束才Clear，保留initialized标志。3新case验证重复/快速/live追加、枚举修改、释放顺序和初始化/释放异常保留注册，665集成全部通过。LOGIC_MODULE_LIFECYCLE_AUDIT保存。尚无具体38控制器实例注册、DataPool释放接线、ProcedureManager FSM或完整30617账号大厅；无新PlayMode/Player。已提取ProcedureManager28657..28663，下一步解析Register泛型CreateFsm与StartProcedure，再实际组合，完整目标in_progress。


## 流程管理器与泛型状态机生命周期（669项）
extract_procedure_fsm_generics从原IL2CPP表提取32共享方法（含Text.GetFullName），method-map3973。OutgameFsm<T>恢复构造逐项FullName-key注册并OnInit、Start/Change状态检查及先退出后写状态清时间、空args选无参Enter、有args保留数组、scaled累计时间后Update、Shutdown先Leave(true)清当前再全部Destroy、正常后清表/置destroyed。OutgameFsmManager恢复Priority80、类型+非空name点分key、ctor成功后发布、live index更新、先shutdown再移除、模块shutdown清map/list/flag。OutgameProcedureManager恢复Priority90、Initialize flag/callback、Register从service取manager创建FSM、StartProcedure空object数组、Update为空（由FSM manager驱动）、Shutdown先Destroy再清字段；两明确原文错误。4新增case验证流程顺序、失败保留状态/时间/注册、重复/缺失状态、动态新增FSM同帧更新，共669集成通过。PROCEDURE_FSM_LIFECYCLE_AUDIT记录；FSM事件/数据API尚未恢复，不声称完整泛型框架。还无原始三procedure具体实例、账号大厅/30617/38控制器生产接线或新PlayMode/Player。下一步PreLoad34122->34120先LoginCtrl.InitUserNetModule，再34123注册ConfigMgr260的统计key80，GameStatisticsControl.OnInit回调34126异步，MoveNext34132与谓词34131等待LoginCtrl bool64&&65；继续源证据解析，不能捏造登录成功。目标in_progress。


## 预加载异步流程与FSM接口所有者修正（672项）
确认原generic inst1598是type3659 IProcedureManager，修正前轮OutgameProcedureManager/验证所有state owner泛型为IOutgameProcedureManager（原先具体类不精确），并修正审计标签。OutgameProcedurePreLoad实现34127保存FSM，34122日志后34120 InitUserNetModule→34123 RegisteredValueFunc(statisticEventConfig.ArenaRank原field80，Func<object[],long>忽略args且每次重读当前key)→GameStatisticsControl.OnInit。其asyncvoid回调34126/34132先ActivityControl.Init(null ActivityInitData)，再await原生WaitUntil，谓词34131短路LoginCtrl flags64&&65，然后Warning LoginComplete→LocalDataControl31593 Handle1_03VersionBug→存储FSM ChangeState<ProcedureStarGame>(emptyargs)，泛型目标usage3968504已确认type4452。默认wait接既有OutgameUnityAwait，测试用可控Task而非真帧；无自动成功flag/timeout/cancel。3新case验证真实WaitUntil谓词、动态统计key、实际FSM切换、挂起状态与失败不修复不切换；672集成全通过。PRELOAD_PROCEDURE_AUDIT明确Host服务（登录/统计/活动/版本修复/base事件清理）与后续StarGame Type依旧组合边界，无新PlayMode/Player。下一步31593旧版本修复及34154StarGame OnEnter、真实服务组合/Main30617，完整目标in_progress。


## 旧版本指挥官修复（675项）
OutgameLegacyCommanderRepair恢复LocalDataControl31593：AppInfoManager.Instance.IsInstallVersion先返回；dealOldComm仅==1跳过，否则先置1再warning；重新取firstChargeData，hasCharge且rewardState bit0为1时取指挥官3，level<=0先写1再SetEventCount(当前CommanderUpgrade,3,1)。随后捕获statisticEventConfig.CommanderUpgrade(field72)，枚举CommanderManager.GetCommanderDatas的原字典Values，GameValue(event,new object[]{Id})仅大于当前level才按float min28且低于0置0写level；第一循环i<level将skill[i%3]level设1，第二循环i<level-1递增。小等级不触碰其余技能；无保存、rollback或异常后重置标志。OutgameCommanderManager增加原get_Values getter，usage3955480确认为10507/inst733。3新case覆盖首充odd状态、统计key变动、28上限/2级技能、marker序列化、安装/精确byte guard、warning与非法skills部分失败，675集成通过。COMMANDER_103_REPAIR_AUDIT记录，无用户存档写入或新PlayMode/Player；PreLoad Host后续需注入真实repair/managers/statistics，接下来ProcedureStarGame34154及已有StartupEntry组合、Main30617/38控制器/大厅生产链。完整目标in_progress。


## 进入大厅的具体流程（677项）
OutgameProcedureStarGame恢复34154 OnEnter：base日志→dicWXAMS.Values逐项Add(ReportLable,PlacementId)建立字典→WXAMS.SetVideoMapping→AssetBundleManager static52 CloseAutoUnLoadAssetBundle=true（接共享runtime.DisableUnload）→ReadyExitGame监听→18个debug注册原顺序→GamePause监听→StartGameModel(0)边界→CheckNewDay边界→已有OutgameStartupEntry.Enter（菜单/奖励/迁移/预制体/GamePlay）。STAR_GAME_DEBUG_REGISTRATION按实际call2194前METHOD/STRING提取，不用预热常量顺序，具体debug处理函数仍待绑定。OnInit保存FSM；ReadyExitGame先发MineGameExitLogic再ChangeState出口type；OnLeave先base日志再移除GamePause、ReadyExitGame；无elapsed或资源flag重置。34161每Update累加scaled时间，>=30仅减一次30且仅触及ReportManager单例，无实际上报call；pause真截断整秒非0才减，保留小数，保留越界intMin投影。2新case验证完整入口顺序/18注册/既有scene completion/退出监听/计时和重复广告标签中止副作用，677全通过。STAR_GAME_PROCEDURE_AUDIT记录，无新PlayMode/Player，服务/具体退出流程/base事件/真实生产Main及38控制器仍需接通，完整目标in_progress。


## 退出流程接通偏好保存（680项）
OutgameProcedureExitGame依据34113..34118，OnInit保存owner，OnEnter先base原名日志再直接调用既有OutgameUserPreferences.OnSave，正常返回后由当前OutgameMessageDispatcher发送ExitGame(nullargs)。无擅自资源释放、自动回大厅、计时恢复；OnLeave base日志、OnDestroy事件清理边界，保留owner。3新case验证实际prefs序列化/新实例重载，write/report先于ExitGame且回调已处exitstate，write失败保持state但不发通知，SaveDisabled早退仍发通知；680集成全通过。EXIT_GAME_PROCEDURE_AUDIT保存，无用户数据写入或新PlayMode/Player。Main30617注册的usage4008012已确认MineGameMain30618，原body有退出完成日志/AudioManager.Destroy/字段37及12/MsgDispatcher.ClearEvent，下一步逐项解析其资源释放和实际Main组合，不把ProcedureExitGame当成完整退出清理完成。完整目标in_progress。


## 主入口退出清理（682项）
OutgameMainExit依据MineGameMain30618按序log客户端逻辑退出完成→AudioManager.Destroy→Main field37=true→IapManager.Instance.IapProcedure(instance12)=null→Object.Destroy(main.gameObject)→取UpdateManager.Instance.gameObject再Destroy→再次UserDataPrefs.OnSave→MsgDispatcher.ClearEvent替换shared→Main.EnterGame(static0)=false。默认Destroy为原生延迟销毁，依赖以真实服务回调/对象getter注入，无finally继续执行。2新case通过ProcedureExitGame真实消息串起MainExit与prefs，原生GameObject fixture注入DestroyImmediate验证调用顺序，两次save摘要去重仅write1，消息替换早于EnterGamefalse，故障保留先前flag但不销毁/保存/替换。682集成全通过，MAIN_EXIT_CLEANUP_AUDIT明确无native延迟帧验证、无实际audio/IAP/生产owner接线、无用户存档修改或Player。Main无OnDestroy方法索引，30624 Update/30620 OnApplicationQuit尚需核对；下一步Main30617三具体procedure/38controls真实服务组合，完整目标in_progress。


## 主入口逐帧与应用退出回调（685项）
OutgameMainLifecycle恢复30624：field37 ExitRequested先return；field38 ResourcesOnly取native dt/udt调用ResourcesModuleUpdate在try外；之后读实时field24 ModulesReady，true再取一次dt/udt并GameFrameEntry.Update，只有后者try/catch Exception把原对象交warning。OutgameMainLifecycleState提供24/37/38/static0共享投影，尚需实际Main组合。30620无flag门控，prefs.OnSave→DataManagerPool.SaveData→日志游戏退出，数据存储完成；prefs异常中止后两步，pool本身既有逐manager catch仍保留。3新增case验证资源更新中改变ready同帧运行/两次采样、exit不取clock、两路径不同异常处理、实际prefs+pool应用退出保存顺序和错误，685集成通过。MAIN_LIFECYCLE_AUDIT记录，无新PlayMode/Player，尚非实际MonoBehaviour。30604 Focus已读源码但未实现：先logfocus，EnterGame gate，GlobalData static28为false才Send GamePause(!focus)，失焦额外UI检查/暂停弹窗与prefs→pool保存。下一步恢复focus并Main30617共享状态/服务组合，完整目标in_progress。


## 主入口焦点切换（688项）
OutgameMainFocus恢复30604：先Main日志OnApplicationFocus：bool，EnterGamefalse直接结束；GlobalData static28 false才GamePause(!focus)，focustrue结束；失焦时GetUI<Proj_xqzdPlayUI>非空&&LevelControl field8==6&&!PVPController flag8才Show<Proj_xqzdPauseUI>(Array.Empty<object>)；之后原Color.red失焦上传日志→实际prefs.OnSave→实际pool.SaveData→Main数据存储完成。使用原始flag标注，不擅猜GlobalData/PVP字段业务含义。三个case覆盖sharedstate gating（不看ExitRequested）、消息/UI/保存顺序、原生red参数、全局flag仅抑制消息且不抑制UI/保存、短路UI条件、listener失败传播，688集成通过。MAIN_FOCUS_AUDIT含泛型类型证据；未用真实OS焦点或原始UI/platform owner，无新PlayMode/Player。下一步把已恢复core/config/pre-game/logic/FSM/三流程/exit/focus/update/quit纳入实际Main30617共享state服务图，不用no-op填齐38控制器，完整目标in_progress。


## 配置完成后的主启动组合（690项）
OutgameMainPreGameStartup按30617/30622/30608串接：初始化日志→既有settings→factory创建PreLoad/StarGame/ExitGame并真实ProcedureManager.Register→原38type顺序逐个获取logic module再取controller Register(false)→audio init→再次logic.InitCtrl(true)→shared ResourcesOnlyfalse/ModulesReadytrue→LangFont→开始游戏日志→CommonSettings→StartMineGame→ExitGame绑定已有MainExit→再取procedure manager StartProcedure<OutgameProcedurePreLoad>。保留重复执行/无rollback语义；resolver必须提供实际服务，无fallback。2case使用真实三procedure（预加载Host只记录并挂起统计callback）与38trace control fixtures验证source JSON逐项顺序、39次logic lookup、38次OnInit、stage顺序及第二controller factory失败保留1注册和已建FSM但不audio/init/写flags，690全通过。MAIN_PREGAME_COMPOSITION_AUDIT明确当前不是38实际控制器实例或完整生产入口；无新PlayMode/Player。下一步审计并绑定实际control lifecycle和nativeMain/core/config/servicegraph，不可将fixture注册通过称全复原。目标in_progress。


## 控制器矩阵与首批真实接线（699项 + 原生10项）
CONTROLLER_LIFECYCLE_MATRIX以原metadata接口槽4/5/6核验38控制器114方法（31个已验证空体），修正旧交接：type4561并非继承生命周期，而是显式GameFramework.IControl三方法。新增controller-generics.json解析26934单例/26750模型查询，追加ABProcessConst34450，method-map4477。OutgameControllerRegistry及OutgameCoreControllerBindings绑定真实type4561对象池、4034时间、4027指挥官、3903渠道流程、4118本地数据；无未绑定fallback。复原原生Pool FIFO/预热/回收/延迟释放、Channel原表六分支与旧账号流程1、Commander真实pool引用、LocalData首充/购买key/GamePlayState与重复监听/释放、ServerTime严格>1秒单次刷新/两次SDK读/跨日通知顺序/调试offset差异。当前其余33生命周期和完整Main账户服务仍未接通。
集成BattleBuild.ValidateMechanicsOnly最新699通过；本轮编译曾因新测试未传OutgameDataVersionState构造参数失败，修复后通过。OutgameControllerPlayModeValidation原生10检查通过：共享实际manager、时间/流程、DontDestroyOnLoad隐藏root、active场景切换、真实消息到共享clock、暂停帧Update、释放解绑、后帧Destroy及再次建root。平台和存储使用明确fixture；没有实际SDK/服务器或完整关外Player声明。未修改用户数据；全部目标in_progress。详见CONTROLLER_LIFECYCLE_AUDIT.json；下一步UI/Game/Level/Player及其余33控制器。

## 进阶玩法分支 — 2026-10-09

当前玩法迭代与正式关卡接入见 [DEVELOPMENT_PROGRESS.md](DEVELOPMENT_PROGRESS.md)。该文档记录已确认规则、最新数值、构建入口、160项验证结果和待试玩事项；本文件前文继续保留原版还原进度，不将新设计视作原版还原证据。
