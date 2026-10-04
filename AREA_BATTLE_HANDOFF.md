# 《冲向那座塔》复原项目当前状态与接续说明

更新日期：2026-10-04。用户已要求“继续”，仓库执行状态恢复 **active**；完整目标尚未完成。

当前执行范围（用户2026-10-04调整）：**服务器相关工作先搁置，客户端恢复继续**。保留已完成的网络代码；网络登录、云存档同步、服务器时间、在线排行和远端上报暂不作为本阶段交付前置条件。优先本地用户资料/头像界面、结算奖励返回、本地保存重启、剩余控制器和Main本地装配，再完成客户端构建与视听验证。服务器事项仍属延期未完成，不计为完成，也不生成虚假成功结果。

## 1. 当前结论

生产入口仍是独立 `Battle.unity`，尚未交付覆盖全部业务的最终Player。

- **1772项集成检查全部通过**，本批新增23项；**2883份验证输入**与隔离副本逐字节匹配。
- **新增登录传输原生8项通过**：实际WebRequestManager/Unity HTTP验证暂停游戏时的逐帧合并上传、最新存档值、AES请求/响应、服务器时间明文GET、请求辅助器复用和释放。此前HTTP队列21项、管理器17项原生验证保留。
- 已恢复 `OutgameNetTool.cs`、`OutgameLoginTransmitter.cs`，并在 `OutgameRankTransmitter.cs` 中补齐密钥信封解码、源SetUrl签名和虚Update/Dispose。域名来源仍由真实平台适配器提供；测试明确使用本地服务。
- 登录传输器18个协议及请求字段、管理器版本列表、存档合并已恢复；首次提交后第4次更新发送，重复写入不重置计时。发送失败、重复注册、回调重入和清理顺序按原版保留。
- **服务器相关的HttpManager、HttpNetAcion、ServerTimeSync与平台网络/报告装配按用户要求延期**。原生测试显式提供传输器更新调用和响应观察函数，不代表生产账号登录成功。
- **生命周期22/38，剩16**，不是业务完成率；Main总装配、RankUI/OverUI页面、其余业务与最终Player/视听验收仍未完成。
- 普通方法索引 **6606**。本批审计23份方法证据（新增20份）、14个字段；上一检查点1749/2875/6586为历史记录。
- 最新证据：[登录传输审计](analysis/targets/wxcf1394487200e48f/43/generated/outgame/LOGIN_TRANSPORT_AUDIT.json)、[定向报告](analysis/login-transport-validation.json)、[原生报告](analysis/login-transport-native-validation.json)、[验证总清单](analysis/VALIDATION_MANIFEST.json)。
- 初次原生测试误将null请求预期为POST，已修正为原管理器GET路径，失败日志单独保留。最终原生日志保留UnityEditor.Search启动索引异常及退出Curl42，集成日志含Curl35证书错误；原生8项和集成1772项均通过，外部平台网络未验收。
- 已成功执行 `record_login_transport_milestone.py`；全部已成功的 `record_*_milestone.py` **不要重跑**。后续变更需新验证/新记录。`extract_http_transmitter_factory_generics.py`只生成了隔离区证据，HttpManager工厂尚未实现/正式发布。

### 接下来的执行顺序

1. 优先UserInfoUI及本地头像/头像框/改名保存，再补OverUI结算和战斗奖励返回；涉及网络数据的页面先恢复可独立完成的客户端部分。
2. 补齐剩余16控制器中的本地逻辑与生命周期，连接已有本地数据池、页面和战斗流程，推进Main本地入口装配。
3. 验证实际操作、本地保存重启和失败分支，生成可运行客户端构建，并推进原版视觉/音频/时序验收。
4. 服务器事项列入延期清单：网络登录、云存档、服务器时间、在线排行、远端上报及其他真实服务端响应依赖。用户重新要求时再恢复，不将延期事项标为已完成。

原有1772项集成/8项原生/2883份输入证据保持不变；本次仅调整执行范围与优先级，没有新的运行时修改或验证结果。

## 2. 环境与约束

目标 AppID `wxcf1394487200e48f`，版本 `43`。当前工作区 `/Users/mt/Documents/AreaBattle`，分支 `main`。项目固定版本仍为 Unity `6000.0.68f1`。

本机另有 Unity `6000.3.7f1`，路径：
`/Applications/Unity6000.3.7f1/Unity-6000.3.7f1/Unity.app/Contents/MacOS/Unity`。
本轮在 `/private/tmp/areabattle-validation-20261003/UnityProject` 隔离副本验证，没有升级原工程。副本的依赖/缓存变化不代表目标版本验证。

保留现有战斗和真实用户数据；不启动子代理；未知原版规则、账号/广告/支付/服务端结果不得编造。测试使用独立临时存档。用户已于2026-10-04要求继续，随后明确服务器相关工作可以先不用；当前优先客户端本地恢复，服务器部分延期。用户随后明确要求上传Git，已授权提交当前工作并推送到现有远端。

当前接续优先项按上方执行顺序推进。历史暂停检查点保留作证据，不代表当前停止。

## 3. 本轮已完成

### 道具与商品服务

- `OutgameProductConfigProvider` 连接共享 GameProductConfig，保留原数组引用和 buyTypeOrder 默认值回写。嵌套价格参数延迟读取，保留逐条异常捕获与后续价格计算。
- `OutgameProductServices` 串起已有 Update → Reset → Price，以及实际 ItemManager.Update 和消息派发。
- `OutgameItemRuntime` 连接旧资源 reader、GlobalItemSlot、实体工厂、礼包 Manager、数据池注册和 ItemModuleControl；使用真实 UpdateManager 和 Unity 时间。
- ItemManager4500 注册参数来自原始 attribute：Proj_hdzd、autoSyn=true、compressData=false。
- 原始项目 JSON 缺失部分共享价格字段，仍按源码记录错误，不补造价格。
- 349 项相关回归检查及 16 项原生 PlayMode 检查通过。未声称平台报告/登录/支付已接通。

### 用户资料与头像框

- `OutgameUserInfoManager` 恢复原始数据字段、构造默认值、初始化/读取、头像列表修复、安装时间、通知顺序、保存与释放。
- `OutgameUserInfoControl` 接入实际数据池和 registry，恢复头像/头像框配置查询、选择、改名与报告顺序。
- 头像框道具和控制器使用同一份 manager 记录，验证真实文件保存和重读。
- UserInfo 的 7 项集成检查保留；随后增加 7 项昵称/服务组检查。ConfigHelper.RandAIInfo32592 和 RandomHelper.Randoms26196 已按原始算法恢复，OutgameUserInfoRuntime 使用真实名称配置，原生道具服务通过同一资料 manager 解锁头像框。实际账号/时间/AppInfo/报告宿主仍需接入。

### 图鉴资料与控制器

- 新增 OutgameGuideBookManager、OutgameGuideBookControl、OutgameGuideBookRuntime 与实际 registry/pool 绑定。
- 按源码恢复记录默认值、服务端等待、提示与图鉴的不同解锁边界、递归引导查询、重复领取、保存/通知失败顺序和释放行为。
- 8 项定向检查及 1113 项完整回归通过。之后已恢复原始页面/条目静态资源和领奖按钮发奖链（见下一节）；后续已接条目/提示列表/标签（见下文），动态列表仍未接入；弹窗/演示动画已由最新一批补齐（见下文）。资料 manager 仍仅保存领取记录，发奖遵循独立 UI 调用顺序。
- 新增 8 份反汇编，方法索引为 5179。详见 GUIDE_BOOK_SOURCE_EVIDENCE.json / GUIDE_BOOK_AUDIT.json。

### 图鉴原始资源与领奖按钮

- 从原始清单取回 GuideBookUI/GuideBookItem 及依赖，7 个包全部通过原始大小/MD5 校验；新增454905字节。独立临时 Python 环境为 `/private/tmp/areabattle-unitypy-env`，UnityPy 固定1.25.2。
- 静态导入2个预制体（81/10节点）、58精灵和1字体到 `Resources/Recovered/GuideBook`，保留精确出口与 RectMask2D 参数；跳过组件保存在 `analysis/unity-guide-book-import-report.json`。
- `OutgameGuideBookRewards` 按原代码先 ToolChange 发奖并保存经济数据，再播放粒子效果，最后保存领取记录和刷新 UI；保留重复点击、false返回、回调重入和失败边界。
- `OutgameGuideBookRewardBinding` 连接原始 btnBox 与红点，实际 UGUI Button 事件、LocalDataManager/GuideBookDataManager 文件保存及独立重载验证通过（7项）。效果/声音/报告仅观测测试端点，不宣称已在原生帧里完整呈现。
- 52个新增方法与原始字段/泛型调用见 `GUIDE_BOOK_UI_SOURCE_EVIDENCE.json`；分析/导入证据与领奖审计见 `GUIDE_BOOK_UI_REWARD_AUDIT.json`。TipBookItem、TabButton/Group已在后续接入；弹窗和Spine已由最新一批补齐；完整动态列表和Main仍待完成。

### 图鉴条目、提示列表和页签

- `OutgameGuideBookItem` 恢复索引/配置引用、两次名称本地化、锁定按钮与递归关卡提示、领取红点；按钮采用原始 RemoveAllListeners 后注册。
- `OutgameTipBookItem` 保留解锁关卡减1提示、名称/说明刷新、独立 Image 指针回调与 Button 全局点击消息差异、单选展开及领取后刷新。`OutgameGuideBookBrowseBinding` 按原始字典顺序建立提示列表，退出提示页签时收起并清除选中条目。
- `OutgameTabButton/Group` 使用真实 Awake/Start/OnDestroy；点击当前页签不重复通知，程序设定则遍历全部页签，通知早于视觉状态刷新，保留回调重入顺序。
- 新证据修正旧图鉴证据表的协程类型：4368，而非相邻的4370 ItemInfoUI。确认 source33402 等待 `WaitForEndOfFrame`，读取最新选择状态，调整高度并重建父布局。
- 新 `OutgameUiClick` 同时修正上一批领奖按钮缺失的 `GF_UIButtonClick`：仅回调正常返回后发送（包括重复领取早退），异常则不发送。
- 7项新增回归、1127项完整检查通过；非批处理真实 Game 视图下23项原生检查通过（展开130→330、切换/领取/收起、重启、对象销毁）。批处理帧末检查失败日志保留；可见 Game 视图是该原生验证所需环境。UnityEditor.Search 启动索引异常与产品检查分开记录。
- 证据：`GUIDE_BOOK_BROWSE_SOURCE_EVIDENCE.json` / `GUIDE_BOOK_BROWSE_AUDIT.json`。弹窗33041/附加提示33050与Spine已在下一批接入；完整 DynamicList/ListData、BaseUI整页生命周期及真实模块服务/Main仍未完成。

### 图鉴弹窗与独立Spine原始子树

- `OutgameGuideBookPopupBinding` 接入实际条目打开回调、原始标题/说明/图片/攻防四项数值/附加提示、奖励显隐、遮罩与OK关闭、页关闭依赖。保留缺失配置、空参数异常、voice失败和异步图片完成顺序。
- 原始3953560泛型单例解析为 `SkillControl4165.CurCommanderId` 字段108；技能9/10/11使用其计算文案序号，图片再次读取指挥官。附加提示7/8/12保留原对齐，6/9/10/11设置MiddleCenter，其余清空文字并保留对齐。
- `OutgameGuideBookSprites` 显式使用原始GuideSprite图集25个引用构建本地资源查找；它不是原始异步atlas模块的替代完成声明。本批显示实际中文配置，但完整LangModule/SkillControl/Main服务宿主仍待接入。
- 从图鉴包自身导出并导入YD_0及13个资源，含独立网格、两材质、两纹理、骨骼与atlas文本；使用官方Spine4.1 runtime，数据版本4.1.16。图鉴1开启动画并清空Image.sprite，其他图鉴关闭动画；不借用战斗GuideUI的显示控制器。
- 6项新增定向/1133项完整回归通过；14项真实Game视图PlayMode通过，包括骨骼帧更新、隐藏停止/重开恢复、原生领奖与文件重启。已检查4张渲染图（open/animated/defense/skill），不是原包画面逐帧验收。测试画布/相机、声音/SkillControl/页关闭端点和本地atlas适配边界保留。
- `GUIDE_BOOK_POPUP_SOURCE_EVIDENCE.json` / `GUIDE_BOOK_POPUP_AUDIT.json` 为最新证据。下一步完整DynamicList/ListData与BaseUI整页资源打开/关闭/Dispose装配，再进入实际Main业务闭环。

### 原始 DynamicList 滚动与图鉴列表

- `OutgameDynamicList` 恢复数据源转发、缓存条目尺寸、自动列数、横竖布局、对齐、反向和额外间距；槽位容量包含两行预留。AutoMask=false 时保留数据缩短导致的列数缩减。
- 使用已有 `OutgamePrefabPoolControl`，按原始顺序建立托管条目、延迟创建原生对象、LateUpdate 清脏后刷新、优先复用首个空槽。单项刷新只更新当前绑定条目；滚动不触发 OnHidden，Dispose 才先通知、再释放条目、最后反向回收对象。
- `OutgameGuideBookListBinding` 使用原始 DynamicList 组件参数和配置字典顺序，接入实际条目/弹窗/奖励回调。修正旧条目适配器：source33208 保存 provider，33213 调用 GetData，因此替换 provider.Data 也必须可见。
- 7组新增检查及1140项完整回归通过；15项真实Game视图检查通过，包括 ScrollRect 指针拖动、回收后的正确点击、领奖红点刷新、原生页面销毁后的对象存活、新页面池复用与存档恢复。2270份输入与隔离副本一致。
- 已检查 top/scrolled/reopened 三张截图；最终验证画布1080×1920、截图750×1334。初始750宽验证画布裁切及错误容量断言已修正，历史失败日志保留；最终日志有独立的 UnityEditor.Search 启动索引异常，产品检查通过。无新 Player，也未宣称原版逐帧视听对照通过。
- 证据 `DYNAMIC_LIST_SOURCE_EVIDENCE.json`（84份方法证据、16条泛型上下文）、`DYNAMIC_LIST_AUDIT.json`。仍缺居中/tween API及选择专用列表子类、完整 GuideBookUI/BaseUI 服务所有权与 Main 装配。

### 图鉴整页生命周期与列表定位

- 新增 OutgameGuideBookPage，组合已有 BaseUI 资源加载、UIObject 出口、Open/CloseRegistry、开关动画和资源所有权。原始 namespace 为 Proj_hdzd.UI.MainMenu，UIPath 为 MainMenu/GuideBookUI；延迟两帧加载后按源顺序绑定24出口、列表、弹窗、奖励、页签、提示和红点。
- 异步关闭保留先从注册表移除、隐藏、业务 Dispose、原生销毁，再释放主资源/动态/自定义句柄和发 CloseUI 的帧顺序。真实重开复用池对象，经济数据和领取记录独立磁盘重启保持。
- 单索引与双索引定位保留不同边界/横轴方向/间距规则；WaitForEndOfFrame 使用 scaled deltaTime，旧协程完成时停止当前保存的最新协程句柄，不额外修正原逻辑。
- 8项新增检查、1148项完整回归、26项真实Game视图检查通过，2274份输入逐字节一致。原生夹具默认中心轴心造成 ScrollRect 回弹，诊断记录位置244.48/速度16.93；仅将定位夹具设为顶部锚点并按实际协程完成验收，最终位置237.50/速度0。失败日志保留，生产动画未为测试改写。
- 原始UIRoot/场景Canvas和三张图已检查；资源取得使用本地恢复预制体，账号/SkillControl/语言/atlas/音频/效果仍含明确测试端点。图鉴页面生命周期通过不代表 Main 或所有真实模块已装配，亦无新Player或原版视听验收。
- 证据 GUIDE_BOOK_PAGE_SOURCE_EVIDENCE.json（50方法证据）、GUIDE_BOOK_PAGE_AUDIT.json；下一步实际Main/account/data-pool及其余22控制器、真实页面服务与完整业务闭环。

### BattleControl 原始配置与注册生命周期

- OutgameBattleControl4060 接入实际 CoreControllerBindings/LogicModule，按源分三次解析 ConfigMgr，捕获 dicDispatch[1/2/3] 的记录引用。缺键中断保留前序赋值；空行在访问时才失败，重初始化才更新替换后的记录。
- 出兵间隔、分数、线路数、增长间隔保留 grade0/1/其余和 lines1/2/其余分支，包括负值；整数先转float再除1000，不加范围修正。Dispose 只清共享注册槽，旧对象可清除新实例的槽，保留数据引用。
- 5项新增检查、1153项完整回归通过；2276份输入一致，生命周期绑定17/38。真实配置与原有BattleSimulation在正常及边界输入完全一致；保留现有战斗实现和存档，本批无新的PlayMode或Player主张。
- BATTLE_CONTROLLER_SOURCE_EVIDENCE.json（8方法/原始字段/get_Item泛型）、BATTLE_CONTROLLER_AUDIT.json 与 analysis/battle-controller-integrated.log 为证据。同步修正矩阵生成器遗漏UserInfo/GuideBook的旧映射，保持源文件换行。

### 项目红点控制器与原生菜单组件

- OutgameRedDotControl4453 接入注册器/LogicModule；原始列表、重复注册/单个移除、绿色日志、异常顺序、更新模式、未缩放计时和Dispose仅清单例均保留。source3989352实为Array.Empty<object>，不是外部时间服务；项目控制器与框架RedDotModule3655分开。
- OutgameRedDotItem4457 按真实Start/OnDestroy注册退出，Check先清flag再UnityEvent、Show再SetActive；隐藏/disabled不注销，也不滤过检查。Scale、DotPrefab和参数字段按原始布局保留。
- OutgameMenuView接受所属注册器resolver后，在创建原MenuTabUI时恢复组件。原始Tower_CheckReddot持久事件的空target、方法名、空assembly、RuntimeOnly状态完整保留，未编造奖励条件。菜单本身仍隐藏道具页签，原生夹具显式激活它以检查生命周期；观测listener仅属于测试。
- 8项新增/1161项完整检查、17项原生通过；2281份输入逐字节一致，生命周期绑定18/38。首次编辑器测试误用SendMessage向inactive对象发OnDestroy，改为直接调用方法；真实销毁另由native检查证明。原生日志有独立UnityEditor.Search启动索引异常，产品断言全部通过。
- 29份方法/字段/泛型/原资源证据见RED_DOT_SOURCE_EVIDENCE.json、RED_DOT_AUDIT.json。未调用的混淆HeadportChange别名34168含原WASM空数组越界写，其精确运行边界未当作已复原；通用框架红点、活动统计/业务条件、Main/Player/原机视听仍待完成。
- 七日启动依赖已定位：4502.EnterGameInit34359 → ActivityControl4637.GetActivity(1301001) → ActivityBase.GetChildActivity(1301)；后续读取ChildLimitTimeTaskActivity4698/ActivityItemData4642并写GameStatisticsExpansion4618。需要真实活动/统计宿主，不能将空红点事件接到猜测业务。

### 公共消息与统计记录基础

- 新增独立 OutgameCommonMessageDispatcher4569，保留替换单例后旧实例可用、原数组/追加key浅拷贝差异、重复监听、重入和异常传播。
- OutgameStatisticsRecords 恢复35015..35022及35011索引循环；总量/子项互不累加、null与空子项列表回退不同、重复子项只写首个、itemId0首次创建与后续写入差异、各重载对null字典处理不同、long溢出和变更后通知均保留。
- 原JSON字段/long往返、记录引用与部分索引失败、共享消息key首次区域格式缓存已验证。只是记录基础，不代表完整GameStatisticsManager/Control/OffNetStrategy、实际统计文件保存或每日刷新完成。
- 新增10项，1171项完整检查通过；2284份输入一致。没有本批原生或Player运行，18/38控制器计数不变。追加118份方法证据，索引5570；STATISTICS_SOURCE_EVIDENCE.json覆盖123份统计/公共消息来源，尚未实现的owner/strategy明确列出。
- 统计与活动manager原始注册属于CommonGameModule，独立于Proj_hdzd；下一步真实管理器、控制器、离线策略/时钟/存档及活动宿主，再SevenDay.EnterGameInit和Main。统计管理器35014入口已追到：自定义provider存在时不设记录，value.ToString后先long.TryParse再int.TryParse，仅2/3个参数派发；未实现部分不注入虚构活动数据。

### 统计管理器、控制器和抽象策略生命周期

- 新增 OutgameStatisticsManager、OutgameStatisticsControl/Expansion、OutgameStatisticsStrategy。数据池真实GetModel/AddModel与UpdateManager注册/延迟移除已连通并检查；数据池手动AddModel保留先发布后OnInit、仅应用AutoSyn、重复返回传入对象。源CommonGameModule注册和38控制器名册保持分离。
- 管理器保留加载后公共刷新、19项provider注册顺序与先注册者优先、首次刷新先清isInit再回调、静态SaveDataDel后重读字段、失败释放的部分状态。Expansion保留消息到计数链、dirty写入顺序和10000例外、GameValue参数回退与仅捕获自定义provider异常。
- 修正字段理解：35060 get_initOver读field16 Action；field20另为默认true的bool。释放回调接GameFrameEntry3433.disposableActions，不是SDK退出事件。抽象策略Update直接调用UpdateDate(Array.Empty<object>())，无数据池就绪门槛；池门槛由控制器保存分支承担。
- 13项新增、1184项全量通过，2288份输入与隔离副本逐字节一致。首次1183运行后核对table1428/function938证明强制转换会抛异常，修正注册实现/断言并增加同步完成先于调度的检查；最终1184为有效验收记录，旧日志明确保留为pre-cast-review。
- STATISTICS_LIFECYCLE_SOURCE_EVIDENCE.json含62方法、3份泛型/桥接/类型转换底层证据及27字段；STATISTICS_LIFECYCLE_AUDIT.json为审计。实际文件检查仅验证opaque文本经过继承存储链，UpdateManager为手动驱动，未声称统计codec/真实帧时序或完整重启完成。
- 具体GameStatisticsOffNetStrategy4623、StatistUtils压缩/时间锚/每日刷新协程仍待实现；测试Probe只在Editor夹具中，生产没有默认替代策略。后续先补此链并接CommonGameModule/GameFrameEntry真实宿主，再ActivityManager/Control/SevenDay/Main与全部业务。控制器仍18/38，本批无新原生或Player构建。

### 具体离线统计、压缩存档与原生等待

- 新增 OutgameStatisticsOffNetStrategy4623 与 OutgameStatisticsCodec4619，连接已有真实manager/control/pool/UpdateManager；压缩JSON/Base64文件保存与新实例重读、旧明文JSON回退、无效JSON双次失败及空数据初始化按源恢复。
- 统计时钟保留float实时毫秒锚点、负服务器值与0回退差异、非发布本地时钟、溢出/NaN边界；每帧三次scaled delta读取，周期仅减一次、dirty严格>30、在线时长>=30加30。HTTP只有原helper存在才请求，不编造响应。
- 恢复六类广告/道具/商品/服务器时间消息的真实参数/计数/异常顺序、每日回调的六项重置与生命周期天数、保存前清dirty/规范空items/零行仅从序列化列表省略，以及原始乱码日志。原Save不检查dirty，仅检查data/records非null。
- 真实WaitUntil等待数据池可保存，注册每日回调后加启动计数；协程完成后保留句柄，重载不再重复注册，释放才停止并置null。元数据最终确认Add/Remove刷新API中的另一个参数是可空RefreshDelegate，而非bool/offset；最终原生/集成均以修正后的类型重跑。
- 9项新增/1193项完整检查、15项实际Game视图原生检查通过，2292份输入一致。独立Python gzip样本可读，真实文件long/时间戳重启保持；未宣称跨运行时压缩字节完全一致。原生日志保留独立UnityEditor.Search启动索引异常，产品断言全部通过。
- STATISTICS_OFFNET_SOURCE_EVIDENCE.json含45方法、13字段、刷新参数和原乱码字节；STATISTICS_OFFNET_AUDIT.json及statistics-offnet-native-final.log为证据。源码补取TimeToRefreshControl23方法，索引5593；其实际调度和跨天边界尚未恢复，当前原生每日回调由明确夹具触发，不能称自动跨天完成。
- 下一步TimeToRefreshControl4589真实调度→统计Runtime/Main/account/SDK/HTTP/CommonGameModule宿主→Activity/SevenDay及其余20控制器/全部业务→Player/视听验收。38名册仍18/38，本批无新Player或截图验收。

### 验证环境差异

- 完整集成包含相机截图，不能使用 `-nographics`；该模式曾在 Unity 原生渲染处崩溃。
- 旧测试直接要求 Input.multiTouchEnabled=false 的读回值为 false，在当前 Mac 编辑器失败。已改为同时断言源码调用参数/顺序和平台实际读回行为；生产 OutgamePreGameSettings 未改。
- Unity JsonUtility 在当前环境将 JSON 的 null 列表读为空列表；新增测试使用真实解析结果，生产代码保留源码访问顺序。

## 4. 尚未完成

下一步把实际服务组放入完整 Main/账号启动图。仍需原始配置/资源获取、所有数据管理器的生产 factory、CommanderUI/Dice/report 宿主、剩余 20 个控制器，以及完整关外页面操作。

业务仍包括商店/支付、普通/特殊/每日模式、竞技匹配排行、任务成就、七日/限时活动、骰子选卡、图鉴公告设置、广告回调、离线每日刷新及战斗奖励返回闭环。许多基础组件已有实现，不应重新从零重做。

此外必须完成实际用户操作、保存重启、失败回调、新 Player 与 smoke、原版视觉音频时序对照。原 WASM powf 边界、随机序列、远端配置及平台行为仍有未验证项，不能将局部通过当成完整复原。

## 5. 接续入口

1. 读取 `analysis/RESTORATION_EXECUTION_STATE.json` 与目标 `OUTGAME_RESTORE_STATE.json` 的最新 milestone/nextPriority。
2. 道具服务见 `ITEM_PRODUCT_SERVICES_AUDIT.json`、`OutgameItemRuntime.cs`、`OutgameProductServices.cs`。不要重做已完成的 getter、快照、礼包、商品刷新算法。
3. 用户资料见 `USER_INFO_SOURCE_EVIDENCE.json`、`USER_INFO_AUDIT.json`、`OutgameUserInfoManager.cs`、`OutgameUserInfoControl.cs`。source32592 名字生成已完成，见 `AI_NAME_SOURCE_EVIDENCE.json`、`AI_NAME_AUDIT.json` 和 `OutgameUserInfoRuntime.cs`；下一步继续真实 Main/账号/时间/报告宿主接线。
4. 图鉴源码与实现见 GUIDE_BOOK_SOURCE_EVIDENCE.json、GUIDE_BOOK_AUDIT.json 和 OutgameGuideBookRuntime；领奖入口与原始资源已恢复，条目/提示列表/页签详见 GUIDE_BOOK_BROWSE_AUDIT，弹窗/Spine见GUIDE_BOOK_POPUP_AUDIT；滚动/回收由DYNAMIC_LIST_AUDIT补齐，居中和整页BaseUI由GUIDE_BOOK_PAGE_AUDIT补齐，接下来装配实际Main及模块服务；不要给记录接口添加源码不存在的奖励发放。
5. 建立完整 Main/account/data-pool 实际服务图，保持模块初始化和原始异常顺序；不能用空控制器或伪造登录状态补齐。
6. 优先跑通“启动→大厅→普通战斗→结算奖励→返回大厅→重启恢复”，再完成全部其余业务及验收；完整目标没有缩小。

上述审计路径相对 `analysis/targets/wxcf1394487200e48f/43/generated/outgame/`。生产脚本在 `UnityProject/Assets/AreaBattle/Scripts/`，验证脚本在 `Editor/`。

## 6. 验证与证据

- `analysis/unity-integrated-validation.json`：1292项全部通过；最新日志 `analysis/activity-control-integrated.log`，最新原生13项见`analysis/activity-control-native-validation.json`。
- `analysis/activity-business-validation.json`：新增10组，完整配置三加载器/九类字段/分组查询及释放重读；本批无新原生/Player。`record_activity_business_milestone.py`已执行，不可重复同一milestone。
- `analysis/activity-owner-validation.json`：新增10组，公共配置加载、工厂/子查询及真实配置→存档→状态恢复；本批无新原生/Player。`record_activity_owner_milestone.py`已执行，不可重复同一milestone。
- `analysis/activity-states-validation.json` / `activity-states-native-validation.json`：新增10组、原生10项；日志 `activity-states-native-verified.log`。`record_activity_states_milestone.py`已执行，不可重复同一milestone。
- `analysis/activity-conditions-validation.json`：新增10组，四类运行时条件缓存及FSM事件；本批无新原生/Player。`record_activity_conditions_milestone.py`已执行，不可重复同一milestone。
- `analysis/activity-data-validation.json`：新增12组，活动manager/offnet、48组状态、真实gzip文件保存及重启；本批无新原生/Player。`record_activity_data_milestone.py`已执行，不可重复同一milestone。
- `analysis/frame-launch-validation.json` / `frame-launch-native-validation.json`：新增10组、原生15项；`record_frame_launch_milestone.py`已执行，不可重复同一milestone。
- `analysis/frame-entry-validation.json` / `frame-entry-native-validation.json`：新增11组、原生15项；`record_frame_entry_milestone.py`已执行，不可重复同一milestone。
- `analysis/time-refresh-validation.json` / `time-refresh-native-validation.json`：新增10组、原生15项；`record_time_refresh_milestone.py`已执行，不可重复同一milestone。
- `analysis/red-dot-validation.json` / `red-dot-native-validation.json`：新增8组与17项原生；日志 `red-dot-native-final.log`。`record_red_dot_milestone.py` 已执行，不可重复同一milestone。
- `analysis/battle-controller-validation.json`：新增5项；`analysis/record_battle_controller_milestone.py` 已执行，不可重复同一milestone。图鉴前一批1148完整日志与26项原生证据保留。
- `analysis/guide-page-validation.json` / `guide-page-native-validation.json`：新增8组与原生26项；原生日志 `guide-page-native-verified.log`，三张图 `guide-page-native-{open,tips,reopened}.png`。
- `analysis/record_guide_page_milestone.py` 已执行，不能重复写入同一 milestone。失败定位诊断见 `guide-page-native-centering-diagnostic.json`，历史日志完整保留。
- `analysis/dynamic-list-validation.json` / `dynamic-list-native-validation.json`：新增7组与原生15项；原生日志 `dynamic-list-native-final-source.log`，三张图 `dynamic-list-native-{top,scrolled,reopened}.png`。
- `analysis/guide-book-popup-integrated.log`：此前1133项完整集成日志；browse-integrated.log保留1127项历史。
- `analysis/guide-book-popup-validation.json` / `guide-book-popup-native-validation.json`：最新6项与14项；原生日志为guide-book-popup-native-verified.log，渲染图为guide-book-popup-native-{open,animated,defense,skill}.png。
- `analysis/guide-book-browse-validation.json`：从最新完整回归提取7项新增条目/页签检查；`guide-book-browse-native-validation.json` 与 `guide-book-browse-native-rendered.log` 是前一批23项原生帧检查。
- `analysis/product-services-validation.json`：349 项相关检查。
- `analysis/item-runtime-native-validation.json` / `analysis/item-profile-native.log`：23 项真实 PlayMode（昵称/资料/道具批次，与最新图鉴原生23项是独立套件）。
- `analysis/user-info-validation.json`：从上一批完整集成报告提取的 7 项；`analysis/ai-name-validation.json` 为本批 14 项定向检查。
- `analysis/VALIDATION_MANIFEST.json`：本轮时间、平台、指纹、历史和验收边界。
- `analysis/record_guide_book_popup_milestone.py`：检查1133完整回归、14原生结果、13资源导入和2266文件匹配后记录最新结果；已执行，不能重复写入同一 milestone。此前 recorder 保留。
- `analysis/guide-book-validation.json` / `analysis/guide-book.log`：图鉴资料8项；最新领奖7项见 `analysis/guide-book-rewards-validation.json` / `.log`。
- 本轮早期失败日志保留：product-services.log（新增样本数组问题）、user-info.log（新增样本 null 列表预期问题）、product-services-integrated.log（无图形模式崩溃）、product-services-integrated-graphics.log（旧 Mac 触控断言）；该批最终通过日志为 items-user-info-integrated.log；最新一批为 guide-book-popup-integrated.log；browse.log为早期编辑器测试生命周期样本失败，browse-final.log为6项通过，最新7项以完整回归提取结果为准。

完整检查入口仍为 `AreaBattle.EditorTools.BattleBuild.ValidateMechanicsOnly`。使用 `-batchmode -accept-apiupdate`，启用图形设备；所有副本文件路径应保持 Workspace/UnityProject 与 Workspace/analysis 的相对布局。原 Windows BuildAndValidate 仍固定构建 Windows，不代表已有 Mac Player。

## 7. 仓库与历史

Git LFS 3.8.0 已安装并仅在当前仓库启用。原始参考视频已取回，114604266 字节，SHA256 `439e6d33828fb2b98be78f2085ce28dbbbcc06f4706be8e9f0ac5ba2a5493bd0` 与 LFS 指针一致；Spine 子模块已检出。Unity 缓存、Build、真实 LocalProfile 不提交。本轮代码、资源、恢复证据和进度文档按用户“先上传一下git吧”的要求纳入Git检查点；后续变更继续单独记录。

9月30日交接原文保存在 `analysis/handoff-history/AREA_BATTLE_HANDOFF-20260930-pre-resume.md`。更早历史保留在既有 handoff-history 目录。旧审计的 pending/检查数仅代表当时范围，最新接续以本文和当前状态为准。

最新弹窗失败/修正记录：首次Spine导入因隔离副本缺少纹理网格文件失败，补齐后成功；首次原生检查误把编辑器加载耗时当作动画运行时间，现同时等待真实帧数，最终14项通过。所有相关日志保留；没有修改生产动画时序来适配测试。


## 2026-10-03 每日刷新调度器与自动跨天链路

- 恢复TimeToRefreshControl4589正常公开路径：按时长到期、指定小时/前一日对齐、每帧最多减1秒、V1/V2合并与注销、按数组引用的字典回调、活列表/字典重入和异常前缀状态。保留原版到期倒计时调用两次，以及已有V2键误合并refresh回调的实际行为；不把混淆别名循环当作公开路径。
- BindStatistics将实际单例接到具体离线统计；真实Unity Update触发首次/跨天刷新，经过已有manager/control压缩保存，独立销毁重建后恢复时间戳与计数，重启后同日不会重复刷新。SDK时间/结果消息为受控输入，生产Main/account/SDK宿主仍待接入。
- 原生单例复用、ExitGame立即清空、GameFrameEntry处置委托await WaitForSeconds(0.1f)、暂停等待/恢复清理及OnDestroy注销通过；未把该等待改为实时计时。
- 新增10项，全量1203项通过；非批处理Game视图原生15项通过；2295份输入与隔离副本逐字节一致。控制器仍18/38，剩20；方法索引5593；项目版本6000.0.68f1未改，验证编辑器6000.3.7f1。
- TIME_REFRESH_SOURCE_EVIDENCE.json记录23方法、19字段、4签名及等待对象类型；TIME_REFRESH_AUDIT.json和analysis/time-refresh-{validation,native-validation}.json保存验收。原生日志UnityEditor.Search启动索引异常独立记录，产品断言通过。
- 下一步接具体统计/每日刷新/CommonGameModule与GameFrameEntry、账号、SDK/HTTP实际宿主，恢复ActivityManager/ActivityControl/SevenDay.EnterGameInit，再完成Main/全部业务/战斗返回与Player和视听验收。本批没有新Player或整体验收声明。


## 2026-10-03 框架入口模块调度与统计Runtime实际退出链

- 新OutgameFrameEntry恢复优先级稳定排序、精确类型Get/Have、GetAll快照、Initialize回调/参数顺序、Start枚举及实际链表Update/反向Shutdown；已有Logic/FSM/Procedure/Time接共同模块接口。构造依赖由明确工厂提供，不创建空占位模块。尚未实现完整入口CommonSettings/StartGame/PauseGame/GameFrameWorkMono。
- 新OutgameStatisticsRuntime组装具体control/manager/offnet、共享common消息、实际UpdateManager与每日刷新单例；同一frame字段承载统计和刷新处置委托。平台使用现有IOutgameControllerPlatform，HTTP/账号/下载依赖仍要求实际宿主。
- 补齐DataManagerPool26740.OnRelease：先关闭就绪标志，调用已有SaveData，再遍历真实manager.OnRelease并Clear，保留字典引用。保存异常按已有逐项日志继续，释放异常传播且保留集合；LocalData/Skin原始OnRelease为nop，已显式补齐接口。
- 本批11项新增，全量1214通过；原生15项验证真实模块->数据池保存/释放->入口全局尾部->统计清理->缩放等待，以及独立Runtime文件重读。原生SDK/账号/下载/权限及config标志端点是受控夹具，不声称实际平台接通。
- 2299份输入与隔离副本逐字节匹配；源方法索引5718（新增125份框架/活动及回调，提取不是实现）。控制器仍18/38，剩20；原工程固定6000.0.68f1，验证6000.3.7f1。本批未构建Player或完成完整Main/业务/视听验收。
- FRAME_ENTRY_SOURCE_EVIDENCE.json含20原始方法、3份共享泛型及13字段；FRAME_ENTRY_AUDIT.json与analysis/frame-entry-*.json/.log为证据。首次编译旧PoolManager夹具缺新增接口，已修正；最终1214有效。原生UnityEditor.Search启动索引异常独立记录，15产品断言通过。
- 下一步完成入口启动设置/场景切换/暂停和真实SDK/账号宿主，恢复已提取ActivityControl/ActivityManager/SevenDay.EnterGameInit，继续完整Main、20控制器、所有业务与Player/视听验收。状态保持active/in_progress。


## 2026-10-03 启动设置、过渡回调与原生暂停/焦点链

- 补齐GameFrameEntry.CommonSettings/StartGame/PauseGame/IsPauseGame：SDK初始化、DPI<=0回退96、fps60/sleep-1、文化设置、广告名称与抽样顺序；等待过渡回调后ReadyExitGame、隐藏banner、写场景状态并LoadScene(GameFrameworkLoad)。外部SDK/过渡/banner/全局名与场景宿主仍要求真实接线，测试不伪造平台完成。
- 恢复GameFrameWorkMono正常单例/Init/焦点/暂停/抽样路径及DomainData公开查询。证据确认单例在已有同名对象上仍AddComponent，仅新根执行DontDestroyOnLoad；构造focus/sample标志true。暂停来源0/1/2递增，升级不重复通知，低优先级不能恢复，恢复保留旧来源，日志重入后消息读实时状态。
- 网络抽样仅值-1才使用已有共享RNG生成1..100并先保存；阈值默认20，非空非法配置TryParse归0；标志只能被关闭。DomainConfig六个字符串字段按原名，公开GetValue类型2/3读取TD/RD，最后匹配含null覆盖。混淆别名不当作正常路径。
- 新增10项/全量1224通过，真实Unity运行15项通过：原生组件、Unity SendMessage、timeScale和实际TimeModule由Frame.Update驱动，验证暂停flush、缩放冻结、恢复缩放后仍需焦点、changeTimeScale=false的源行为和注销/重建。未声称操作系统真实焦点事件或原始过渡动画视听验收。
- 2303份输入与隔离副本逐字节一致；索引5723（新增DomainData5方法）；控制器仍18/38，剩20。FRAME_LAUNCH_SOURCE_EVIDENCE.json记录22方法、28字段与AddComponent泛型证据，FRAME_LAUNCH_AUDIT.json和analysis/frame-launch-*.json/.log为验收。原生UnityEditor.Search启动异常独立记录，产品断言通过。本批无新Player。
- 下一步恢复已提取ActivityControl/ActivityManager/SevenDay.EnterGameInit并接ProcedurePreLoad活动初始化；继续真实账号/SDK/HTTP/过渡/场景/权限/config宿主、完整Main与全部剩余业务/Player/视听验收。目标保持active/in_progress。


## 2026-10-03 活动管理器、离线存档与条件协调

- 恢复ActivityManager4658、CommonModuleManagerBase4659、ActivityNetStrategyBase4660及具体ActivityOffNetStrategy4661，连接已有真实数据池和账号存储路径。活动配置/注册工厂/owner回调仍要求实际宿主；没有默认空业务或伪造服务端完成。
- 数据保持原始int首次登录时间、long刷新/启动/预告时间、字符串uniqueId及弹窗标志；活动codec使用自身嵌套流释放。支持gzip和旧明文JSON，二次解析失败按原包记录Message/StackTrace后重新建数据，日志失败仍传播。
- 现存记录仅处理首个匹配ID，保留重复行、孤立行和旧uniqueId；按launch/notice/over原条件协调state3/4，必要时重新查询launch。无效日期/整数条件被跳过，短数组失败，原顺序与重入保留。保存不清dirty，释放仅重置管理器注册标志。
- 新增12项，全量1236通过，包含48组条件/状态组合、真实gzip文件保存与新backend/manager独立重读、注册/存储/回调失败前缀状态。2308份输入与隔离副本逐字节一致；源方法索引5757，控制器仍18/38、剩20。本批无新PlayMode/Player；最近入口15项原生是此前1224版本的独立证据。
- ACTIVITY_DATA_SOURCE_EVIDENCE.json记录33方法、43字段与8条泛型证据；ACTIVITY_DATA_AUDIT.json、analysis/activity-data-validation.json及activity-data-integrated.log为验收。日志保留32条既有ShouldRunBehaviour编辑器断言及一次Unity云请求超时，与此前完整日志数量一致；产品1236检查全部通过且无编译错误。
- 下一步恢复ActivityControl/ActivityConfigMgr、活动条件缓存与子类型图，接SevenDay.EnterGameInit及ProcedurePreLoad；继续完整Main/账号/SDK/HTTP/场景宿主、其余20控制器和全部业务/奖励返回，最后Player与原版视听验收。目标保持active/in_progress。


## 2026-10-03 活动运行时条件缓存与状态机事件

- 补齐ActivityItemData.Config及Notice/Launch/Over/Close四类条件缓存：参数数组长度决定容量，下划线拆分前段为boxed int统计参数、末段为long目标；日期精确转换、无效值归0、缓存提前赋值、失败后保留部分数组、缺配置重试与已缓存配置不失效均遵循原代码。此路径区别于离线加载时跳过无效文本的检查，未合并两者规则。
- 恢复ActivityStateBase35369比较：open为0立即false，其余值允许；使用传入类型数组而非缓存key，透传缓存arg引用，provider返回后才读实时target。原始本地PubActivityConfig四组字段已验证，私有缓存不会进入JSON，重读后按配置重新构建。
- 通过原始泛型注册表补齐Fsm.FireEvent和FsmState订阅/退订/分发/销毁：当前状态校验、null/原样payload、重复委托仅减一个、保留null字典键、重入快照、异常中止及OnDestroy清空。两个OnEnter重载独立为空，ChangeState helper保留参数数组和null校验。
- 新增10项，全量1246通过；实际统计10700/道具8计数达到目标4后驱动真实OutgameFsm切换并由FsmManager销毁。该状态场景为明确Editor夹具，完整活动四状态/UI/监听器仍待实现。2311份输入与隔离副本逐字节一致，控制器仍18/38、剩20；本批无新PlayMode/Player。
- ACTIVITY_CONDITIONS_SOURCE_EVIDENCE.json记录6方法、16字段；fsm-event-generics.json记录13共享泛型证据。另提取76份ActivityBase/四状态/基类方法用于后续，方法索引5833；提取不等于实现。ACTIVITY_CONDITIONS_AUDIT.json、analysis/activity-conditions-validation.json及集成日志为验收。
- 完整日志仍有32条既有ShouldRunBehaviour编辑器断言及一次Unity云请求超时，数量与此前相同，产品1246项全部通过且无编译错误。最近原生15项仍是此前入口启动/暂停套件，不当作活动UI验收。
- 下一步实现已提取ActivityBase和Close/Notice/Launch/Over状态，装配ActivityControl/config宿主，接SevenDay.EnterGameInit和ProcedurePreLoad；继续完整Main/账号/SDK/HTTP/场景/其余20控制器/全部业务及Player和原版视听验收。目标保持active/in_progress。

## 2026-10-03 活动四状态、按钮与自动倒计时

- 恢复 `OutgameActivityBase` 生命周期与关闭/预告/开启/结束四状态：保存状态选择、嵌套转换、统计监听、进度重置、弹窗标记、子活动刷新/更新/释放顺序均沿用原包。Factory方法与泛型子活动查询仍待恢复，未宣称完整ActivityBase API。
- 实际UGUI Button/Text接入类型指定的UI宿主与弹窗请求；预告倒计时由统计10000自动消息刷新。保留源码开启状态注销数组交叉、预告切换开启时可能重复弹窗、超过一天只显示天/时/分、负倒计时不归零和描述参数数组不自动扩容等边界。
- 新增10组集成检查，全量1256通过；原生10项通过，实际指针点击、四状态循环、销毁重建和跨帧自动倒计时均已验证。2316份输入与隔离副本一致；控制器仍18/38、剩20，方法索引5833。
- 首次原生运行在第8项后超时：夹具时钟距预告目标超过一天，格式不显示秒，30秒等待不足以保证文字变化。将测试服务器时间设在阈值前一小时后10项通过，生产逻辑未改；初次报告和日志 `activity-states-native-initial-countdown-*` / `activity-states-native-initial-countdown.log` 保留。
- `ACTIVITY_STATES_SOURCE_EVIDENCE.json`记录77方法引用及25字段，其中工厂明确标为待实现；审计见 `ACTIVITY_STATES_AUDIT.json`。全量日志仍有32条既有ShouldRunBehaviour编辑器断言和一次Curl42退出中止；原生日志有UnityEditor.Search启动异常及一次Curl42，无编译错误，不能称日志无异常。
- ActivityControl/config/弹窗队列/真正页面打开仍为待接必需宿主；未取得原版活动页面渲染、平台或Player验收。接下来恢复工厂/子活动查询及控制器/配置所有者，装配已实现离线数据与四状态，继续七日活动/ProcedurePreLoad/Main。

## 2026-10-03 公共活动配置加载、工厂与存档连接

- 恢复ActivityConfigMgr公共加载/完成/释放路径，保留自定义配置管理器反射筛选、累计计数与相等完成条件、重复读取不清零、当前配置所有者与回调所属对象的区别。启用活动的空关闭/结束条件按原包补为`-1 / 999999`；原始本地9条活动配置与设置字段已验证。
- 恢复本地读取器：`common_ActivityConfig`非空时覆盖活动表，设置表仍单独选择读取路径；在线JSON从首个花括号解析、重复键保留首条并输出原始诊断。Unity JSON资源读取已实现；二进制获取/解码仍为显式必需宿主，未宣称平台配置已接通。
- 补齐ActivityBase虚拟工厂与泛型子活动查询。基础工厂按type1的1/2/9创建对应子工厂并重读道具配置；原始三个子工厂Produce均明确返回null，未添加虚构奖励实体。缺失配置、类型转换及释放失败顺序保留。
- 新增10组，全量1266项通过；真实配置所有者→ActivityManager离线数据→统计/FSM/UGUI预告→gzip文件保存→新对象图重启恢复验证通过。2320份输入与隔离工程一致，控制器仍18/38、剩20；最新原生10项仍为上一批活动状态套件，本批未重跑，无新Player。
- `ACTIVITY_OWNER_SOURCE_EVIDENCE.json`记录25方法与41字段，另4共享泛型中2项控制器查询只提取未实现；新增16份普通方法，索引5849。`ACTIVITY_OWNER_AUDIT.json`、`analysis/activity-owner-validation.json`及完整日志为验收证据。日志仍有32条既有ShouldRunBehaviour断言和一次Curl42退出中止，产品检查全部通过且无编译错误。
- 尚需恢复ActivityConfigMgr任务/成就/新手/活跃度配置字典与分组查询、具体自定义配置管理器；继续ActivityControl注册/状态记录/弹窗队列/每日与道具钩子，并装配七日活动、ProcedurePreLoad与Main。完整目标保持active，未以公共配置加载替代完整控制器与业务验收。

## 2026-10-03 完整活动业务配置与三种加载器

- 按原始元数据恢复九类业务配置结构、ConditionPriority值类型键、全部剩余ActivityConfigMgr分组和查询。保留有键但值为null与缺失键的不同日志行为、原版诊断文字、行对象和奖励数组共享引用、64位目标值；共享PubAchievementAccConfig只有id，未将项目JSON额外字段虚构进共享结构。
- 恢复NoviceTaskConfigMgr、TaskConfigMgr、AchievementConfigMgr；完整继承清单确认只有这三种，OutgameActivityConfigRuntime按原始元数据顺序连接公共配置管理器。新手/任务一次捕获读取模式，成就始终走Unity JSON；二进制获取仍为显式必需宿主。
- 原始资源加载49条新手任务、8条累计奖励、1条新手活动、8条任务、3条活跃度、1条任务组、1条任务活动、127条成就、3条成就累计配置，随后加载9条公共活动及设置；真实配置运行时完成计数5/5。
- 保留原始分组和释放行为：任务列表重复构建追加，字典分组重复id抛错，新手活动会创建空任务组；成就按type及contentType/content组合分组，再按priority排序。TaskConfigMgr释放保留TasksByGroup，释放后重读会向旧8条引用追加新8条；这些边界已明确验证。
- 新增10组，全量1276通过，2325份输入与隔离工程一致；40份方法证据/102字段/9结构/完整3加载器清单记录于ACTIVITY_BUSINESS_SOURCE_EVIDENCE.json。新增22份被实际使用的反汇编，方法索引5871。完整日志仍有32条既有ShouldRunBehaviour断言及一次Curl42退出中止，无编译错误。
- 控制器仍18/38、剩20；配置运行时完成不代表具体活动、ActivityControl或Main已接通。最近原生10项为此前1256活动状态版本，本批无新原生/Player。下一步实施ActivityControl的活动注册、状态记录、弹窗队列、帧更新、每日与道具钩子，并继续七日入口和完整生产闭环。


## 2026-10-03 活动控制器与实际配置、存档、状态机装配

- 恢复公共 ActivityControl4637 的全部36个方法（34个普通方法、2个共享泛型），以及排序回调、空初始化数据、活动注册属性和父活动报告名称缓存。连接已恢复的完整配置运行时、离线数据、统计、状态机、Unity更新循环及框架释放；公共活动控制器独立于38个项目启动控制器，数量仍18/38。
- 按原版恢复父子注册与共享引用、延后覆盖、存档索引、按钮区域排序和重绑、道具工厂钩子、两类弹窗队列、开启/预告/结束记录与报告顺序、分钟/每日统计和保存。额外子活动刷新由Type.Equals(ActivityBase)槽126控制，仅精确基础类型执行；已按元数据纠正最初的子类判断并增加针对验证。
- 保留原版边界：关闭队列向前移除会跳过相邻项；自动弹窗移除后关闭可能保留NoticeUi阻止下一弹窗；Cleanup保留道具钩子、队列和部分字段；重复Init新建manager即使数据池保留旧实例。报告/UI仍由明确宿主承接，无伪造平台成功。
- 新增16项，全量1292通过；修正后代码重新通过13项真实Unity PlayMode，覆盖自动帧更新/自动弹窗/重入OpenUI、实际按钮指针事件、压缩保存/独立重启、四状态与退出。2329份输入与隔离工程逐字节一致。方法索引5873；源证据43个普通方法、2个共享泛型、44字段。
- 集成日志仍有32条既有ShouldRunBehaviour断言及一次Curl42退出中止；原生日志保留一条UnityEditor.Search启动ArgumentOutOfRangeException，产品检查全部通过、无编译错误。初次测试编译错误、未改变存档导致写入去重的失败测试，以及类型判断修正前的运行证据均保留。
- 证据：ACTIVITY_CONTROL_SOURCE_EVIDENCE.json、ACTIVITY_CONTROL_AUDIT.json，analysis/activity-control-validation.json、activity-control-integrated.log、activity-control-native-validation.json、activity-control-native.log。项目固定Unity版本未改，无新Player或原版整页视听验收。
- 下一步：恢复带注册属性的具体活动/manager/工厂完整清单和FatherActivityBase/ChildActivityBase、ChildLimitTimeTaskActivity4698；接SevenDay.EnterGameInit与ProcedurePreLoad，再完成Main/账号/SDK/HTTP/场景、其余20控制器及所有业务/奖励返回/Player验收。目标保持active/in_progress。


## 2026-10-03 父子活动框架与七日任务存档

- 恢复共享 FatherActivityBase/ChildActivityBase，包括创建子活动、绑定存档、注册顺序、可见数据筛选和虚拟排序边界；保留原版重复注册、缺失模块及异常后的部分状态，不额外清理或造任务。
- 恢复 NoviceTaskManager4711 的原始活动ID1301001、CommonGameModule存档键、服务端/本地更新、压缩/原文JSON加载、删除失效子活动、补入新子活动、重复数据索引和保存回调顺序。原版49条任务配置参与实际数据池初始化；独立文件后端重启保留任务状态、条件参数及全部ext标记。
- 解码全部9种活动继承关系（包含泛型父类），确认仅任务1300001、限时任务1301001、成就1601001带自动注册属性；七日任务子活动由父活动创建。归档11个共享泛型、27项运行时泛型上下文、15个普通方法和28字段；普通方法索引5884。
- 新增14项检查，全量1306项通过；2332份验证输入与隔离工程逐字节一致。集成日志保留32条既有ShouldRunBehaviour断言及一次Curl42退出中止，无编译错误。本批没有重跑原生PlayMode或构建Player；此前活动控制器13项原生验证属于1292版本的历史证据。
- 证据：ACTIVITY_FAMILY_SOURCE_EVIDENCE.json、ACTIVITY_FAMILY_AUDIT.json、activity-registration-roster.json、analysis/activity-family-validation.json、analysis/activity-family-integrated.log。项目固定Unity版本未改；控制器生命周期仍18/38，剩20，不代表业务完成率。
- 下一步恢复 ChildLimitTimeTaskActivity4698 / LimitTimeTaskActivity4699 的进度、每日解锁、累计奖励和相关任务/道具逻辑，再接SevenDay.EnterGameInit及ProcedurePreLoad；继续Main/账号/SDK/HTTP/场景、其余20控制器、完整业务及Player/原版视听验收。具体活动接口当前由明确测试端点验证，未冒充生产页面、奖励或平台成功。完整目标保持active/in_progress。


## 2026-10-03 限时任务模型、累计进度与活跃点道具

- 恢复LimitTaskItemData、NoviceAccRewardItemData、Ext、LimitTaskPageItem全部37个相关模型/工厂/基类方法：任务归属与配置缓存、记录/显示条件、重置进度、可完成判定、按钮状态、排序、奖励列表、累计进度/目标及页签选择。原始49条新手任务配置均参与验证。
- 按原版先检查记录进度，再查询实时统计，再检查已解锁天数，最后判断state0；已领取state1按钮直接返回2。条件参数保持原始int[]或装箱Int32，失败后的部分缓存不重建。重置进度不清领奖状态、选择状态或显示条件缓存；存档只包含原版公开字段。
- 累计进度的两个查询保留差异：均只看已领取state1任务，accType0计数；accType1累计type1=2/type2=4奖励，其中Ext额外筛选道具paramInt等于本活动ID，累计奖励模型不筛归属。不是读取背包余额。非法模式分别为静默0/报错0；缺失目标组报错并返回999，空组为0；溢出按原版64位累加/32位截断。
- 恢复ActivityVirtualItemBase、LimitTimeTaskFactory和LivenessPoint。工厂只处理2/4道具，实体构造重新读取当前配置；模型添加先走真实道具引擎，再按实时paramInt派发活动消息，参数截断为Int32。普通添加和使用保持原版基类路径；使用不自行扣款。测试覆盖消息失败前已写入的背包、报告和dirty状态。
- 新增17项检查（含24组任务状态/记录/统计/天数组合），最终全量1323项通过；2335份验证输入与隔离工程一致。源证据37方法、44字段、15泛型调用，普通方法索引5919。按回调顺序细化比较器与消息参数后已重跑全量；初次通过日志单独保留。
- 集成日志仍有32条既有ShouldRunBehaviour断言及一次Curl42退出中止，无编译错误。本批未重跑原生PlayMode、未构建Player；此前活动控制器13项原生验证为历史证据。控制器生命周期仍18/38，剩20，不等于业务完成率。
- 证据：LIMIT_TASK_MODELS_SOURCE_EVIDENCE.json、LIMIT_TASK_MODELS_AUDIT.json、analysis/limit-task-models-validation.json、analysis/limit-task-models-integrated.log。模型服务必须绑定实际配置/统计/道具/活动宿主；当前子活动图由明确测试接口提供，没有生产默认成功、模拟领奖或平台成功。
- 下一步恢复ChildLimitTimeTaskActivity4698的初始化/重置、统计监听、进度更新、实际领奖和每日解锁，以及LimitTimeTaskActivity4699父活动装配；连接本批模型与工厂，再接SevenDay/ProcedurePreLoad。继续Main/账号/SDK/HTTP/场景、其余20控制器、任务成就及所有剩余业务，最终Player与原版视听验收。目标保持active/in_progress。


## 2026-10-03 限时任务实际活动、领奖与原生重启

- 恢复 ChildLimitTimeTaskActivity4698 和 LimitTimeTaskActivity4699 的完整活动逻辑，并新增运行时装配，将实际父子活动、NoviceTaskManager、模型、统计消息、配置与活跃度工厂连接到公共活动系统。原始49条任务按7天初始化；应用宿主继续提供存储、报告和道具端点，原有注册保留。
- 初始化沿用存档任务对象、保留并诊断失效ID，新增任务只加入运行时列表。保存按条件中的进度/状态筛选追加，保留零条件任务不追加及重复ID行为。重置保留accFlag、launchTime和lastClickDayId；初始列表通知用当前dayId，重置通知用各分组day。
- 统计事件、生命周期过滤、参数筛选、终身统计、进度截断、报告阈值/槽位及异常前状态均按原始方法恢复。监听添加取配置键，移除取存档条件键；空条件允许重复注册时钟监听。完成计数提供者使用父活动自身ID的原始查找行为，以及导航遇到缺失日期的失败行为均保留。
- 实际领奖先发奖励，再扣材料，再写领取状态和位标记，最后报告/通知/dirty；没有添加原版不存在的余额判断或事务回滚。任务与累计奖励使用32位移位后符号扩展到64位存档，初始化却按64位解码；第32/33位边界已验证。活跃度道具事件在任务写领取状态前发生，实际背包和派生累计进度均验证通过。
- 新增21项定向检查，完整集成1344项通过；9项原生PlayMode通过，覆盖原配置实际活动、统计进度、测试按钮指针领奖、原生帧自动保存、独立文件重启、防重复发奖、时钟跨日和再次自动保存。测试按钮不是原始任务页面，尚未完成页面及视听验收。
- 2339份验证输入与隔离工程逐字节匹配。源证据43方法、41字段、11回调/泛型调用、20原始字符串，方法索引5962。集成日志含32条既有ShouldRunBehaviour断言和一次Curl42；原生日志含一次UnityEditor.Search启动索引越界和一次Curl42，无编译错误。初次1339项中的一个测试预期失败日志保留；修正排序后异常场景并增补边界后1344通过。
- 证据：LIMIT_TASK_ACTIVITIES_SOURCE_EVIDENCE.json、LIMIT_TASK_ACTIVITIES_AUDIT.json、analysis/limit-task-activities-validation.json、limit-task-activities-integrated.log、limit-task-activities-native-validation.json及native.log。未构建新Player，项目固定Unity版本未改；控制器生命周期仍18/38，剩20，不等于业务完成率。
- 下一步恢复SevendayActivityControl.EnterGameInit和ProcedurePreLoad装配，再接原始限时任务页面、普通任务/成就活动及管理器。继续Main/账号/SDK/HTTP/场景、其余20控制器、全部业务奖励返回和最终Player/原版视听验收。完整目标保持active/in_progress。


## 2026-10-03 七日控制器与真实公共预加载装配

- 完成SevendayActivityControl4502全部方法并绑定控制器注册表，生命周期进展19/38，剩19。OnInit/Updata保持原始空实现，Dispose只清单例；进入游戏场景回调才执行EnterGameInit，位置保持在排行榜初始化之后、游戏状态/关闭加载/可交互报告之前。
- 七日控制器实际绑定1301001父活动下的1301子活动；原配置中1301001自身挂在1300001下，必须保留公共控制器从ChildActivities查找的路径。日期取首次结束条件目标的低32位减1、活动LaunchTimeStamp对应日期的零点，再进行32位毫秒乘法后扩展；严格排除起止相等时刻，缓存与显式重新进入的差异保留。
- 补写统计顺序为皮肤数量、当前关卡减1且非负、指挥官总等级。指挥官总等级已接真实manager字典的有符号32位累加。红点使用原版天数范围及累计奖励state0，解锁后日任务和累计奖励两条查询都会执行；回调修改后续配置键和中途异常的状态已验证。
- 新增OutgameCommonPreLoadHost，接实际StatisticsRuntime与ActivityRuntime.Init(null)。保留真实ProcedurePreLoad的统计回调、ArenaRank动态查询、WaitUntil双登录标志、修复及FSM转换顺序。账号标志、网络、修复、场景、UI和报告由必需应用端点提供，未制造平台成功。
- 新增9项检查，完整集成1353项通过；11项原生PlayMode通过，验证跨帧等待统计响应、两个登录标志分别阻止启动、真实等待完成后的FSM转换、延迟场景回调、原81条皮肤配置的3个初始解锁皮肤、原49条任务中通关进度补写为10及真实帧自动压缩保存。
- 2344份输入与隔离工程逐字节一致；源证据34个相关既有方法、28字段、13调用映射，方法索引5962。集成日志仍含32条既有ShouldRunBehaviour断言和一次Curl42；原生含一次UnityEditor.Search启动索引越界和一次Curl42，无编译错误。最初1355项编辑模式尝试记录保留：完整公共运行时依赖PlayMode常驻对象，已移到真实原生验证；父活动测试改为同时处理子活动索引。
- 证据：SEVENDAY_STARTUP_SOURCE_EVIDENCE.json、SEVENDAY_STARTUP_AUDIT.json、analysis/sevenday-startup-validation.json、sevenday-startup-integrated.log、sevenday-startup-native-validation.json及native.log。未构建新Player，固定项目Unity版本未改，完整Main/账号和原版页面视听验收尚未完成。
- 下一步恢复CommonLimitTimeTaskUI及任务/日页/累计奖励/奖励条目原始资源与交互，接大厅七日入口与红点；继续普通任务/成就管理器、剩19控制器、完整生产Main/SDK/账号/网络/场景装配和全部业务奖励返回，最后进行Player与原版视听验收。完整目标保持active/in_progress。


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

## 2026-10-03 原始累计领奖、异步奖励预览与原生保存重启

- 恢复 LimitTimeTaskAccItem4348 全10方法、SevendayAccPreviewItem4361 全12方法及异步4360两方法。新增24个普通方法，索引6028；源证据33方法/46字段/29调用。
- 累计条目保留全局奖励表数量判断最后档、奇偶箱子布局、严格state0条件及回调后实时数据读取。先实际发奖，再表现；金币/钻石表现仅截断显示数量，不截断经济long。非货币按paramInt发旧工具和打开皮肤弹窗；附加奖励保持原版ToolChange零增量，避免擅自重复发奖。表现异常不回滚已领取状态。
- 预览使用原始层级和UIObject显隐：对象保持激活，以缩放一/零显示隐藏；每次显示注册更新，隐藏排队移除。按EventSystem当前选中对象的精确触摸名、奖励模板子串或区分大小写的Node子串决定保留。点击预览只触发回调。
- 刷新立即清理旧奖励，等待WaitForEndOfFrame后读取实时Data。连续刷新会追加两组，隐藏后仍可创建；没有添加取消/合并。Dispose按原包只销毁自身及清回调，不自动清条目引用或取消更新。失败时保留原始已执行部分。
- 完整集成1401项通过，新增15项；11项原生检查通过：真实指针与帧末等待、详情回调、销毁、并发刷新、实际UpdateManager显隐、金币领取、防重复、效果完成消息、自动活动保存及独立文件重启。领取任务作为明确前置数据；背包持久化、完整活动通关与表现服务未由此推定完成。
- 2500份验证输入与隔离工程逐字节一致；集成32条既有ShouldRunBehaviour断言和一次Curl42，原生一次UnityEditor.Search索引越界和一次Curl42，无编译错误。生命周期19/38不变；没有新Player或原机视听验收。
- 证据：LIMIT_TASK_ACC_SOURCE_EVIDENCE.json、LIMIT_TASK_ACC_AUDIT.json、analysis/limit-task-acc-validation.json、analysis/limit-task-acc-integrated.log、analysis/limit-task-acc-native-validation.json及native.log。
- 下一步完成CommonLimitTimeTaskUI完整页面生命周期/刷新/倒计时/关闭和七日大厅入口，再继续普通任务/成就、Main/账号/平台/剩19控制器、全部业务奖励返回及最终构建验收。目标保持active/in_progress。

## 2026-10-03 七日活动整页与原生加载/关闭/保存重启

- 完成 CommonLimitTimeTaskUI4310 全15方法、日页比较32896和TimeUtility32655，接原始9个出口、命名空间/资源路径、弹窗layer2/open0/close0及恢复的BaseUI加载/关闭。新增16普通方法，索引6044；源证据31方法/24字段/34调用。
- Awake遵循预览、模式字典、活动与配置、关闭按钮/4类消息、倒计时、列表初始化、进入报告顺序。日页选择写lastClickDayId而不自行置Dirty；真实活动领奖通知由页面自身监听刷新任务/累计奖励，无测试转发。领取时同步重建当前累计条目，保留源回调在原生帧末销毁前继续的顺序。
- 中文倒计时沿TimeUtility而非TimeHelper：一天以上显示天+时，以下时分秒；保留溢出/负数及每次结束发SevendayClose但不直接关闭。进度保留严格消息参数、未使用模式字典读取和无零分母保护。预览定位到原箱子世界坐标并等待帧末刷新。
- Dispose先清BaseUI引用，移除四类页面监听、清日页选择、销毁累计条目；保留数据源/字典/预览引用，不添加原包不存在的清理。编辑态测试延后页面根销毁，原生验证真实异步关闭与资源释放。
- 1410项完整集成通过（新增9项），17项原生整页检查通过：原始bootstrap/加载、真实日页和领奖指针、自身消息刷新、异步预览、重入重建、防重复、自动保存、结束不关窗、关闭回调/注册表/隐藏/销毁/资源释放顺序，独立存档重启与整页重开。
- 2506份输入与隔离工程一致；集成32条既有ShouldRunBehaviour断言及一次Curl42，原生一次UnityEditor.Search启动越界及一次Curl42，无编译错误或MissingReferenceException。生命周期19/38不变；无新Player或原版视听验收。
- 来源/报告：LIMIT_TASK_PAGE_SOURCE_EVIDENCE.json、LIMIT_TASK_PAGE_AUDIT.json、analysis/limit-task-page-validation.json、analysis/limit-task-page-integrated.log、analysis/limit-task-page-native-validation.json和native.log。
- 下一步生产大厅七日入口及红点/消息联动，然后普通任务/成就、完整Main/账号/平台、剩19控制器和全部业务奖励返回，最终构建与原版视听验收。完整目标保持active/in_progress。

## 2026-10-03 原始大厅七日入口、红点与原生页面联动

- 恢复Proj_xqzdStartUI4399的33722初始化、33704点击、33695红点、33690显隐四个完整方法及Awake33711/Dispose33692相关片段。绑定原RigthBar/btn_sevenDay与imgReddot；源证据7方法/5字段/10调用，方法索引6044不变。没有宣称整个大厅生命周期完成。
- 初始化先捕获IsUnlock，为真才追加按钮及4类监听、刷新红点，最后使用捕获值设置显隐。未解锁初始化不监听后续解锁消息；重复初始化保留原版重复注册。点击按声音宿主(1,2001)→真实CommonLimitTimeTaskUI注册器→GF_UIButtonClick顺序，无额外活动期判断。
- 红点监听SevendayUnlock/FinishTask/GetAccReward，取真实任务/累计资格；SevendayClose仅重查入口显隐。红点捕获托管对象后查询控制器，保留托管空引用与Unity已销毁对象差异。Dispose按当前IsUnlock条件移除4类监听，结束后跳过移除；按钮回调不额外移除。
- 1419项集成通过（新增9项），14项原生通过：原始大厅指针进入完整页、重复打开复用、实际领奖更新红点、累计领取等待效果完成事件、自动保存、关闭资源释放、独立文件重启与入口/整页重建、到期隐藏入口但弹窗保持打开。声音与效果输出仍为明确观测宿主，领取前置数据明确播种。
- 2512份输入与隔离工程一致；最终无编译错误或MissingReferenceException。集成保留32条既有ShouldRunBehaviour断言与一次Curl42，原生一次UnityEditor.Search启动越界与一次Curl42。生命周期19/38，未新增Player或完成原版视听验收。
- 证据：SEVENDAY_ENTRY_SOURCE_EVIDENCE.json、SEVENDAY_ENTRY_AUDIT.json、analysis/sevenday-entry-validation.json、analysis/sevenday-entry-integrated.log、analysis/sevenday-entry-native-validation.json及native.log。
- 下一步普通Task/Achievement模型、manager、活动和UI，完整大厅/Main/账号/平台及剩19控制器、全部业务奖励返回和最终构建/视听验收。已定位ChildTaskActivity4676(35398..35410)、TaskActivity4685(35458..35469)、TaskItemData4690(35484..35491)、AchievementActivity4714(35608..35624)、AchievementItemData4718(35628..35635)。目标保持active/in_progress。

## 2026-10-03 普通任务模型、存档结构与活跃度工厂

- 恢复TaskData4686、ChildTaskData4687、Ext4688、GroupData4689、TaskItemData4690、Condition4691、LivenessItemData4677及TaskFactory4673/TaskLivenessPoint4674，共30个源方法。证据30方法/42字段含可见性属性/15调用，普通索引6074。
- 普通任务ResetProgress追加条件而非清空；保留已有进度/领取/选择状态及异常前已追加记录。CanComplete仅按实际条件值和严格state0，不增添接取/过期/显示判断。描述取目标值；排序区分可领、未接取、已接取、已领取，按配置id比较，保留非标准state的原始非全序行为。
- 配置缓存只重试null。子活动与任务组额外刷新次数先替换列表后读配置；活跃度列表懒初始化，显式刷新按字典顺序与32位移位循环位标记生成；奖励缓存先发布，保留负数量和中途失败前缀。TaskData.Clone为MemberwiseClone，整个嵌套图仍共享。
- 原始2/3类别工厂创建普通任务活跃度实体；先写实际Int64道具模型与报告，再发CommonGameModule_Task_LivenessPointAdd+paramInt、参数为低Int32。事件失败保留已入账模型；普通Add不发此模型专用事件，Use不自行扣库存。
- 1431项集成通过（新增12项）；实际8任务/1活动/1组/3档配置验证，独立临时文件JSON重读证明存档字段、大整数和私有缓存/Selected不序列化。明确不等于TaskMgr自动保存/每日刷新/完整业务重启。本批无新原生或Player运行。
- 2518份输入一致；最终无编译错误，保留32条既有ShouldRunBehaviour断言与一次Curl42。生命周期19/38不变。来源：TASK_MODELS_SOURCE_EVIDENCE.json、TASK_MODELS_AUDIT.json、analysis/task-models-validation.json及task-models-integrated.log。
- 下一步TaskMgr4692/TaskOffStrategy4696和子策略4681/4683、父子活动4676/4685。已核对TaskMgr活动ID1300001，OnInit创建离线策略，InitStrategy接Data回调；ChildTaskActivity按uid/groupID建索引再创建ChildTaskOffNetStrategy，TaskComplete委派策略并带true。方法已在隔离副本提取，workspace仅发布本批审阅30方法，不得全量覆盖索引。继续成就、完整Main/账号/平台及所有业务/构建/视听验收，目标保持active。


## 2026-10-03 普通任务管理器与离线存档

- 恢复TaskMgr4692/TaskOffStrategy4696：OnInit只创建策略，InitStrategy经数据池解析manager并按父活动配置决定更新；服务器路径等待真实回调。经理回调先发布数据，再以Add建立子活动索引，保留重复键异常后的部分状态。
- 离线加载保留压缩/原文JSON回退、新档与旧档的不同分支、失效子活动/分组/任务清理、刷新计数器迁移标记、旧Int64进度迁入首条条件、补入缺失条件以及UID初始化。错误/日志回调中断保留原版已发生的数据变化。
- 保存先JSON往返克隆并压缩快照，再调用缓存活动SaveData，最后经真实manager存储。活动保存期间的修改进入后续快照；活动不存在/回调抛错时不写盘。独立文件后端和新manager重启验证通过。
- 新增8项，全量1439项通过；2522份验证输入逐字节一致。源证据18普通方法（新增索引14）、5共享泛型、3包装函数、10字段、11引用、7泛型上下文；普通方法索引6088。日志保留32条既有ShouldRunBehaviour断言和一次Curl42，无编译错误。
- 本批无新PlayMode/Player。TaskActivity回调由明确测试端点承接；实际父子任务活动、每日刷新/领奖/UI、生产Activity运行时安装、成就及完整Main/账号/平台/全部业务仍待完成。生命周期19/38不变，完整目标保持active。
- 证据：TASK_MANAGER_SOURCE_EVIDENCE.json、TASK_MANAGER_AUDIT.json、analysis/task-manager-validation.json、analysis/task-manager-integrated.log。


## 2026-10-03 普通任务活动、策略与七日共同运行

- 恢复TaskActivity4685/ChildTaskActivity4676/TaskNetStrategyBase4683/ChildTaskOffNetStrategy4681及TaskRuntime，连接真实TaskMgr、配置、道具经济、公共事件与存档。原始8条普通任务经权重抽取生成，与49条七日任务在同一实际父活动树中运行。
- 保留源规则及失败顺序：配置/记录键的监听差异、未接取任务进度门槛、终生统计、条件首行长度参与上报阈值索引、领奖先标记再奖励/成本/通知、活跃度入口不内置重复领取门槛、每日/每周严格大于与间隔等于边界差异、到期删除、自动代领与刷新。
- 生成逻辑保留组数减剩余任务数的循环边界、首个抽中配置决定整组到期时间/接取状态、赋hash前检查零UID等源码行为。父活动OtherViewDic每次生成新字典，显示按Config.param排序；未擅自修正原规则。
- 元数据确认父工厂TaskFactory4673处理2/3，子工厂TaskItemFactory4669处理10/4；共享构造函数符号曾导致混淆，最终已修正并重跑验证。子道具普通Add先发放再Use上报，模型Add不调用Use；没有虚构直接任务刷新、扣款或支付成功。
- 全量1451项、新增12项通过；最终真实PlayMode14项通过，覆盖共享活动树、数据池未就绪等待、原生帧自动保存、两个任务系统领取、独立文件重启、跨日重建8任务和保存。领奖通过实际活动方法调用；普通任务UI/用户指针仍待接。
- 2536份输入与隔离工程逐字节一致；新增73普通方法，索引6161；源证据55字段、33引用、4抽象声明和事件/权重泛型解析。集成日志32条既有ShouldRunBehaviour断言和一次Curl42；原生日志一条UnityEditor.Search启动ArgumentOutOfRangeException及一次Curl42，无编译错误。初次别名编译错误及工厂修正前结果单独保留。
- 生命周期19/38不变。完整生产Main/账号/平台/全部业务、普通任务界面、成就、最终Player与原版视听验收尚未完成，完整目标保持active。
- 证据：TASK_ACTIVITY_SOURCE_EVIDENCE.json、TASK_ACTIVITY_AUDIT.json、analysis/task-activity-validation.json、task-activity-integrated.log、task-activity-native-validation.json、task-activity-native.log。


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


## 2026-10-03 成就模型、存档结构与积分道具

- 恢复原始4715..4719模型、4627/4628成就积分道具/工厂，审阅21方法、17字段、13引用。实际127条成就配置、30/60/90三档累计配置逐条接入。累计配置运行时类型仅声明id，不据原JSON额外字段补造领取规则。
- 成就资格先取配置再读当前Int64进度，要求精确state0；不增加显示条件/时间门槛。排序为可领、未完成、已领，平级按缓存config.id。配置只缓存首个非空结果；奖励每次新建，保留有符号数量及失败后重试；显示条件先发布缓存，保留失败前缀和盒装Int32参数。
- 累计模型实时访问1601001所有者，statePts使用无符号64位移位及低6位索引；达到阈值后才再次查询领取位。当前模型测试的所有者是明确fixture，实际成就Activity/Manager/Strategy仍待装配，未宣称自动保存。
- 原始2/2工厂分配AchievementPoint（已解析类型引用，未被共享构造函数名误导）。普通Add先完成真实Int64库存/报告/发奖宿主，再发低32位积分事件；模型添加不发此事件，Use仅报告。保留中途失败已发生的前缀。
- 最新1487项集成通过，新增12项；2662份源码/资源输入与隔离工程逐字节一致，普通方法索引6239。无编译错误，日志保留32条既有ShouldRunBehaviour断言与一次Curl42。本批无新PlayMode/Player；上一批14项活跃度原生验证仅作为历史结果。独立文件验证的是模型JSON结构，未冒充成就运行时自动保存重启。
- 下一步补实际AchievementMgr4722/StrategyBase4727/OffStrategy4725/Activity4714的统计进度、领取失败顺序、紧凑/旧档读取和独立重启，再接TaskController4507/AchiTaskSubUI3986/TaskSingleton3990/TaskPanelUI4416及大厅入口；继续完整目标。生产仍为独立Battle.unity，生命周期19/38。
- 证据：ACHIEVEMENT_MODELS_SOURCE_EVIDENCE.json、ACHIEVEMENT_MODELS_AUDIT.json、analysis/achievement-models-validation.json和achievement-models-integrated.log。record_achievement_models_milestone.py已执行，不可重复执行；source evidence脚本可幂等重跑。


## 2026-10-03 实际成就活动、统计进度、领取与数据池存档

- 恢复AchievementActivity4714、AchievementMgr4722、StrategyBase4727与OffStrategy4725，在ActivityRuntime.Init之前注册到实际活动/数据池；累计模型访问真实1601001所有者，原始127条成就与3档累计记录接通。泛型实例3386已确认Data4716/Activity4714/Manager4722。
- 恢复统计消息的精确Int64增量/Int32筛选、绝对值溢出和实时排序顺序。ChangeProgress会替换并排序正在按索引遍历的列表，某条可能再次访问而另一条跳过；保留源行为。全量统计刷新按配置content参数重算，已领取记录保留state/time。
- 成就领取先逐条发真实奖励，再设state/time并上报，随后推进同组代表项、排序和通知。宿主发奖失败保留已变库存但尚未设领取位，报告失败则保留领取位而尚未推进列表；不加原包没有的事务/回滚/额外门槛。积分事件使用低32位，累计阶段按字典最后符合项而非最大值。
- 管理器索引重复ID为最后一条覆盖；多阶段全领回退判断整个类型列表是否为空，可能省略后续已领组。Reset只清值、刷新累计列表和当前类型排序，不擅自退回最初代表项。Dispose移除监听但保留MessageKeys，后续AddListeners的原始短路行为通过验证。
- 成就Update与Launch原本为空，沿用数据池的账号保存触发点，无新造逐帧自动保存。Save从真实map复制列表，倒序写入仅已领ID/time与ext的紧凑JSON；读取紧凑和旧完整结构、补齐缺失时间、删过期ID/添新增配置、重算进度。真实数据池保存后的独立文件重启、空领取存档和禁用保存门槛均通过。
- 全量1500项通过（本批13项），2668份源码/资源输入匹配隔离副本，63审阅方法含59新增索引，索引6298，17字段、31引用。无编译错误；32条既有ShouldRunBehaviour断言与一次Curl42保留。本批无新原生/Player；并未把数据池主动SaveData测试说成完整Main自动退出保存验收。
- 下一步完整AchiTaskSubUI3986/TaskController4507/TaskSingleton3990/TaskPanelUI4416每日与成就页签、红点、Main入口；继续生产Main/账号/平台/特效与报告宿主、其余19控制器、全部业务和最终Player/原版视听。生产仍为独立Battle.unity，完整目标active。
- 证据：ACHIEVEMENT_RUNTIME_SOURCE_EVIDENCE.json、ACHIEVEMENT_RUNTIME_AUDIT.json、analysis/achievement-runtime-validation.json、achievement-runtime-integrated.log。record_achievement_runtime_milestone.py已成功执行，不可再次执行；source evidence脚本可幂等重跑。


## 2026-10-03 任务控制器、成就子页与原生领取

- 恢复控制器4507，并由OutgameCoreControllerBindings.BindTasks注册实际resolver；同时连接普通Task与Achievement活动。生命周期覆盖20/38，剩18。这只是生命周期绑定数，不是业务完成率。
- OnInit监听GF_AdsPlayCallBack/WarWin并注册900001恒0值函数，源InitHczzqEvent/Updata为空；Dispose只清两个活动缓存并移除三个事件，不清控制器槽/统计函数。原OnInit没有订阅LoadStartingUI，即使Dispose移除它，也不补造订阅。每日ActivityID静态初值130001从cctor确认。
- 第10关入口规则、两次实时关卡读取、未解锁时清每日条件进度、真实原始LeftBar/taskBtn及child0红点已接。每日红点不套用任务8的UI过滤/receiveState；成就红点会重写正进度ArenaRank并跳过当前检查，保留源行为；Achi OR Daily短路顺序不变。
- AchiTaskSubUI3986按类型去重，显示条件精确相等且使用空参数查询；不满足条件的首条也先占类型。仅当Rows全空时建行，后续刷新只重排，不擅自补行/重绑；缺行警告、空提示只开启不隐式关闭。投影使用目标/进度低32位，共享奖励/Lang，保留UID。领取先执行实际策略，再复用下一代表项或隐藏并移除类型映射。OnDestroy先清单例宿主再解绑，保留字段/行对象。
- 全量1512项通过（新增12），11项真实PlayMode通过：原始入口显隐、实际EventSystem指针、稳定布局后的timeScale0滑动暂停、恢复后原始200金币/领取时间、同一行推进下一成就、回调后报告读取新行ID、数据池保存后独立运行时/页面重启及监听清理。首轮原生测量受到首帧布局影响，测试等待排版稳定后通过，生产代码未因此修改；初轮记录保留。
- 2677份输入逐字节匹配隔离副本；34审阅方法含19新增索引、30字段、27引用，普通方法6317。集成保留32条既有ShouldRunBehaviour断言和一次Curl42；原生保留一次UnityEditor.Search启动ArgumentOutOfRangeException和一次Curl42，无编译错误。本批无新Player。
- 下一步TaskSingleton3990具体泛型所有权、完整TaskPanelUI4416页签/生命周期/关闭/TopInfo/音频/预览所有权及原始Main任务按钮打开路由；TaskController和AchiTaskSubUI已完成本批范围。继续完整生产Main/账号/平台/特效报告宿主、剩18控制器/全部业务及Player/原版视听，目标active。
- 证据：TASK_CONTROL_VIEW_SOURCE_EVIDENCE.json、TASK_CONTROL_VIEW_AUDIT.json、analysis/task-control-view-validation.json、task-control-view-integrated.log、task-control-view-native-validation.json和task-control-view-native.log。record_task_control_view_milestone.py已成功执行，不可重跑；source evidence脚本幂等。


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
