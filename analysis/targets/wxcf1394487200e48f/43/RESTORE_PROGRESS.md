当前执行范围（用户2026-10-04调整）：**服务器相关工作先搁置，客户端恢复继续**。保留已完成的网络代码；网络登录、云存档同步、服务器时间、在线排行和远端上报暂不作为本阶段交付前置条件。优先本地用户资料/头像界面、结算奖励返回、本地保存重启、剩余控制器和Main本地装配，再完成客户端构建与视听验证。服务器事项仍属延期未完成，不计为完成，也不生成虚假成功结果。

当前完整复原目标已按用户2026-10-04“继续”恢复执行，尚未完成整体验收。最新1772项集成检查全部通过，本批新增23项；2883份验证输入一致，新增登录传输原生8项通过。已恢复NetTool域名配置/缓存、服务器密钥信封解码和LoginTransmitter的18个协议及存档合并上传；通过实际WebRequestManager验证加密存档HTTP往返与服务器时间明文GET。生命周期仍为22/38、剩16，不代表业务完成率；普通方法索引6606。服务器相关装配按用户要求延期；客户端RankUI/OverUI、Main本地装配及剩余业务/客户端构建仍待完成。



## 2026-09-30 UIControl / TopInfo / MenuTab 增量
- UIControl 生命周期、进出关状态处理和共享注册绑定已实现；控制器生命周期实现6/38，仍有32待接，业务完整度另行验收。
- TopInfo 原预制体36节点/25绑定/11sprites/1font；原生 Canvas 位掩码/层级/射线和 MenuTab 关闭/异常回调顺序通过源证据检查。
- 集成705项通过；带GameView渲染的独立PlayMode6项通过，实际 WaitForEndOfFrame 按 main/shop/commander/item-info 加载，timeScale0。
- 批处理PlayMode停在第一个 WaitForEndOfFrame 后超时，保留 ui-native-batch-timeout.json。未替换生产等待行为。
- 菜单角色/数据回调、TopInfo账号头像按钮与值刷新、Main/account整体装配、剩余32控制器和新Player构建仍未完成。审计 O/UI_CONTROLLER_NATIVE_AUDIT.json。


## 2026-09-30 GameControl 原生场景共享状态增量
- GameControl4064 生命周期、InitScene/皮肤与背景加载完成/相机与特效切换/模型清理已实现；与注册器绑定。生命周期7/38，剩31；这不代表完整关外已完成。
- 纠正旧交接：arr_homeGroud 元素类型是 Texture2D，不是 GameObject。当前场景id由主页/战斗共享，纹理回调保留源码捕获索引与当前id的不同读取时机。
- 从原GameSceneMono59序列化引用补齐4相机、17字段和21个选中节点，恢复兄弟顺序，使soldierRoot.GetChild(1)对应原模型背景。
- 集成711项通过；Game原生PlayMode7项通过（原场景引用、原皮肤7/8/9特效复用、回主页、下一帧销毁）。当前资源验证使用本地同步适配器，LoadPrefabControl/11001真实加载仍待接。
- 审计 O/GAME_CONTROLLER_NATIVE_AUDIT.json 与 GAME_CONTROLLER_FIELD_EVIDENCE.json；日志 analysis/game-control-integrated.log、analysis/game-controller-native.log。
- 下一步 LevelControl4107/PlayerControl4462与资源控制器；仍需完整Main/account/menu接线、所有关外系统闭环和新Player构建验收。


## 2026-09-30 LoadPrefabControl 原生资源接线增量
- Path13种地址、ToFileName、ResLoadHelper旧资源gate/前缀、LoadPrefabControl三类加载/模板缓存/回调参数已实现。生命周期8/38，剩30；LoadPrefabControl实体池与预加载业务仍未完成。
- GameControl经真实4117控制器→legacy scheduler/package/download runtime→原生AssetBundle完成资源11001与主页/战斗纹理、原始特效加载。Game接口补传原始args数组。
- 718集成与8原生检查通过，原生链路5次本地file请求（manifest+4bundle），缓存命中不发新下载。原始远程URL和完整Main/Player不在此验证范围。
- 原生harness首次因预制体自带Sprite而过早检查缓存失败；已改为等GameSprites写入，保留 prefab-loader-native-early-readiness-failure.json，生产时序未改。
- 审计 O/PREFAB_LOADER_SOURCE_EVIDENCE.json、PREFAB_LOADER_NATIVE_AUDIT.json。method-map4560。日志 analysis/prefab-loader-integrated.log / prefab-loader-native.log。
- 下一步恢复4117的PrepareStart/PreLoad/PrepareEntity/GetEntity/GetEntityNow/GetEntityRoot及helper，再Level4107/Player4462，保持完整关外目标。


## 2026-09-30 实体池与预加载增量
- LoadPrefabControl4117实体获取/同步获取/模板准备/根节点/PrepareStart/PreLoad及嵌套回调已按源实现。EntityRoot纠正为GameObject，实体命名、activeSelf复用、移除销毁项后跳项、模板空缓存不替换、同步miss返回null均有测试。
- 保留原作异常：失败计数不递增、成功只清局部；默认0可持续重试。有限轮黄金用例验证，无虚构重试上限或成功回调。
- 预加载固定10id→士兵皮肤/随机皮肤→Boss821→障碍物EnityID；PrepareStart共享计数与加载页reset、PreLoad局部索引与CanLoadNext。具体LevelControl/PlayerControl/加载页host仍需接线。
- 729集成+13原生PlayMode通过；原生仍5个本地file请求，验证真实AssetBundle预缓存/复用/帧末销毁。Native test原始生成脚本assert导致缺失harness的一次启动日志已保留。
- O/ENTITY_PRELOAD_AUDIT.json / PREFAB_LOADER_SOURCE_EVIDENCE.json（52方法/19usage）；method-map4565。日志analysis/entity-preload-integrated.log、entity-pool-native.log。生命周期仍8/38，剩30，不新增完整Player声明。
- 下一步Level4107/Player4462与加载页，绑定IOutgamePrefabPreloadHost，再完整Main/account/menu/全部关外验收。无待续进程。


## 2026-09-30 关卡配置、皮肤实体与原始加载页增量
- 新增源配置/资源片段 OutgameLevelConfigAccess、OutgameLevelResourceState、OutgameLevelConfigAsset(ScriptableObject)、OutgamePlayerSkinEntities；真实 tuple/皮肤/加载页由 OutgamePrefabPreloadServices 连接。完整 Level4107/Player4462 生命周期仍未完成，8/38不变。
- 恢复原 Resources 加载页10节点/3引用/11sprites/1font；原六帧12fps/.5秒循环动画、AutoCanvasLayer偏移25、进度/超时/重试/返回。UIControl Open/Close/Reset与原资源路径→恢复prefab映射已实现。全Main/UI host仍待接。
- 741集成（新增12）+23加载页原生检查通过。原生验证Animator/UGUI/默认OutQuad/825位移/严格超时>/state10、11时序/延迟销毁/旧页清新Singleton/CLR已销毁对象字段写。
- 集成关卡测试用原始关卡1/2重建原生bundle；模板缓存显式测试fixture，不能当成所有实体预加载已可用。没有新Player构建。首次导入fake-null、测试fixture缺依赖、动画额外尾键失败日志保留，修复后通过。
- 审计O/LEVEL_LOADING_AUDIT.json、LEVEL_LOADING_SOURCE_EVIDENCE.json；method-map4612。日志analysis/level-resource-integrated-fixed.log和loading-page-native-fixed.log。
- 下一步完整Level生命周期/update/Prepare后初始化与Player生命周期、真实loading host/Main/所有缺失实体资源，继续全部关外与持久化/构建验收。无本任务待续进程。


## 2026-09-30 Level生命周期、阵营统计与准备完成流程增量
- OutgameLevelControl 实现 OnInit/Updata/OnDispose，玩家阵营源为Global.PlayCampID；OutgameLevelRuntimeState与既有PlayStateDispatcher共享field8。BindLevel已验证。生命周期方法层面9/38、剩29，不能理解为9个业务控制器全部复原。
- CampInfo池/索引/颜色/逐塔计数与分数截断/neutral排除/CampChange后胜负判断按源实现；Update逐塔读取live时间倍率、foreach异常和一次timer刷新保留。
- InitTowers顺序、live总数和最后Boss；InitObstacles实际nativeEntity81变换、append与flag；Prepare后场景/PlayUI/state4/相机/协程顺序已实现。完整Tower/Soldier factory、InitGameData、WayLine/AI和Main接线仍待补。
- 两次WaitForSeconds(.2)协程源已恢复；756集成（新增15）+10原生检查通过。原生验证暂停scaled等待、原始障碍物mesh、真实BindLevel与销毁保留。WayLine/AI是明确记录fixture效果，无完整原生战斗或新Player声明。
- 首次障碍物测试错误要求Collider；原始恢复visual prefab只有mesh，改为验证MeshFilter，未添加虚构碰撞器。失败日志保留。
- O/LEVEL_CONTROL_AUDIT.json / LEVEL_CONTROL_SOURCE_EVIDENCE.json（35方法及原字段布局）；method-map4681。日志analysis/level-control-integrated-final.log、level-start-native.log。无本任务待续进程。
- 下一步Level InitGameData/剩余集合与factory、完整Tower/Soldier/WayLine/AI与prepared/state UI/Main，Player4462和其他29生命周期，再全关外验收。


## 2026-09-30 Level重置、复用、特效集合与查询增量
- InitGameData按源顺序清理：Boss引用→隐藏障碍物并清列表→check flags→active Camp/Tower.Clear→全部Soldier.Clear(true)/Effects.Destory→WayLine.field168=0。保留池/关卡配置/状态/其它flags与计时，不补造重置。
- 对象池按inactive/士兵field112/Arrow type4/Boss过滤；完整具体Tower/Soldier仍待恢复。EffectCellection九方法已实现，原生帧末销毁、根丢失时保留句柄、单child重复命名和异常顺序均保留；真实EffectModule仍待接入。
- 778集成（本批新增22）通过；10原生检查通过：原始Entity81重置后立即复用同实例、特效子树帧末销毁、实体与SO保留。原生EffectModule/Soldier回调是明确fixture，不声称完整业务对象完成。
- 查询共享field76/80结果、neutral差异、仅排除Arrow、Unity destroyed==null、range平方距离对外半径平方/内参数开方及NaN比较、Random.Next(count)均按源。补SetLevelSpeed、refreshflag、GetBossObj和现有皮肤/节日选择到真实GameControl的场景路由。
- O/LEVEL_REUSE_QUERY_AUDIT.json与LEVEL_REUSE_QUERY_SOURCE_EVIDENCE.json（26方法/71字段+2泛型随机函数）；method-map4681，生命周期仍9/38剩29。日志analysis/level-query-integrated.log、level-reuse-native.log；无本任务待续进程。
- 下一步剩余Level API与完整Tower/Soldier/WayLine/AI/effect、Player4462及剩29生命周期，实际Main/account/menu接线与全目标验收。没有新Player构建，完整关外目标保持active。


## 2026-09-30 Player控制器、模型交互与原生生命周期增量
- OutgamePlayerControl4462已绑定共享registry，连接真实GameControl/Level/UI/皮肤/模型与加载器；OnInit/Updata/OnDispose按源顺序。LoadGameScreen先Game.InitScene再InitLoadModel；重复InitLoadModel先换模型字典、后Roots.Add重复失败；不虚构去重/清对象。
- 主页3动画计时先全部累加，再严格>11.7/13.7/14.7清零刷新；周期刷新捕获Exception并保留源warning。Page2拖动独立gate、200像素、严格>.3切换、3position后3scale、ChooseSoldier先于finger清除。事件旋转与拖动共享状态。
- TouchBegin gate state2/page3，HomeCamera原生射线、Scenes11=31、floatMax距离、xy更新/z保留与Animator.Trigger；原包TagManager图层8..31补回空槽，未覆盖已有冲突图层。
- Models回调位置188与scale184 live读取，raypoint172分离；animation实时SkinCatalog，native destroyed animator按Unity null处理。OnDispose只清animation缓存与监听，保留root/model，迟到动画完成仍走源fallback。
- 792集成（新增14）+10 Player原生PlayMode通过。原生真实LogicModule/场景/原始模型/烘焙动画；鼠标touch和点击Collider/AnimatorController为明确probe，不声称设备输入、原始动态场景或账号接通。
- O/PLAYER_CONTROL_NATIVE_AUDIT.json、PLAYER_CONTROL_SOURCE_EVIDENCE.json（28方法/26字段+原始图层证据）；日志analysis/player-control-integrated-final.log、player-control-native.log。无本任务待续进程。
- 修复生命周期matrix生成器遗漏此前Level4107的问题；当前10/38方法级实现，剩28，不代表10完整业务控制器；method-map4681。
- 下一步共享InputManager、剩28实际控制器、Main/module/account/menu；保留Level/Tower/Soldier/WayLine/AI/effect剩余范围、全部关外子系统与持久化/重启/失败/返回/Player验收。完整goal保持active，无新Player构建。


## 2026-09-30 共享InputManager原生输入增量
- 新增OutgameInputManager(type3603)：原生MonoBehaviour Update/OnGUI、命名单例Find/GetComponent/AddComponent/DontDestroyOnLoad，原生UnityInput默认数据源；PlayerInput默认构造已直接接入共享输入监听。
- Android/iPhone/WebGL走touch，其余mouse；鼠标Down>Up>Held、独立位置读数、精确坐标移动、Click先于End；touch End/Canceled先于Click、严格<50、不补UI屏蔽/WebGL鼠标回退。多指重建有效列表、id=数组index、Began也走Hold、位移后缩放delta/50、zero count触发MultiEnd。
- KeyDown重复仍派发Hold（WASM内层block核对），KeyUp先回调后移除live HeldKey，Hold传live list。Shutdown只清14个委托，保留历史/单例。注册Begin先日志后Combine，重复监听按次移除；CLog原始过滤策略待完整host恢复，当前可注入日志适配器默认Debug.Log。
- 811集成（新增19）+10 InputManager原生PlayMode通过。原生Update->默认PlayerInput->真实Player.OnTouchBegin射线gate，Dispose注销/Shutdown持续更新/持久场景/销毁重建验证。设备读取为明确脚本fixture，不宣称OS触摸或SDK/完整入口接通。
- O/INPUT_MANAGER_SOURCE_EVIDENCE.json、INPUT_MANAGER_NATIVE_AUDIT.json；日志analysis/input-manager-integrated-final.log、input-manager-native-final.log；无本任务待续进程。method-map4821。
- InputManager是框架类，不增加业务控制器计数：仍10/38方法级实现，剩28；完整关外goal active，无新Player构建。下一步剩余控制器与真实Main/module/account/menu生产组合、Level/AI等余项及完整目标验收。


## 2026-09-30 音频控制器、设置与UI路由增量
- OutgameAudioControl3904实际registry绑定；9方法表面/生命周期复原，Init/Updata源码为空、Dispose清当前slot。状态2播parallel1001、3StopAll、5播1002、6/7仅已有parallel暂停false/true、8/9停Music1、10停0、其他无操作。未改已有BattleAudio。
- OutgameAudioSettings直接复用live UserDataPrefs，原始SoundGameVolume/MusicGameVolume键、默认1、严格>.99；Sound开关只存，Music开关存后GlobalVolumeChange(typed VoiceType,float)。非法voice仍通知/不写、值不clamp、回调失败保留已写数据。
- OutgameUiAudioManager路由lazy初始化，3种node独立lazy缓存；Play保留ID数组/返回GUID数组引用。StopAll按parallel/sequence/single顺序stop成功才清引用；日志失败先于Stop；Pause/GUID不建node；Destroy只清共享owner，旧实例仍可清新slot。
- 827集成通过（新增16），含analysis/audio-settings-restart/UserData.txt真实隔离磁盘保存/重读，保留无关key。节点是验证probe，本批无原生音频/发声/PlayMode/Player构建主张。
- 生命周期11/38方法级，余27；method-map4969。O/AUDIO_CONTROL_SOURCE_EVIDENCE.json、AUDIO_CONTROL_AUDIT.json；日志analysis/audio-control-integrated.log；本任务无待续进程。
- 下一步AudioManager/native根节点/listener/资源/config、AudioCompositeBase与Parallel/Sequence/Single、AudioActionBase与Once/Loop/Fade/LoopFade后端，接上当前required node factory并验证原生音频。相关源码已抽取，尚未实现。之后剩余27控制器与真实Main/account/menu/全部关外范围、重启失败返回和可运行构建验收。


## 2026-09-30 音频节点与动作基类增量
- OutgameAudioNodes：真实native根对象/parent/local变换/layer；Parallel、Sequence、Single源调度、暂停、Stop/Destroy、开始结束消息。要求显式newAudio动作factory，尚不提供假成功或替换音频后端。
- Parallel开始回调才入字典，未开始的异步load不在Stop覆盖内；Stop源码lock+keys快照+Remove(i)循环索引bug保留（不是Remove(keys[i])）；Pause live字典；销毁后迟到callback仍发消息。
- Sequence type2只转1、首动作Play先于Enqueue、同步结束可留下已结束队首；End先Dequeue/消息/再Peek.Play。Stop置flag并Dequeue，只有已有native AudioSource才Stop，忽略voice，尾部Destroy清queue/resetflag；失败保留部分状态。Single firstID/null=>0，Pending先写再停Current；End清Current->消息->Pending.Play->清Pending；Destroy保留Pending，迟到End仍可能启动。
- OutgameAudioAction抽象生产基类：this.GetHashCode GUID、OnAwake/update注册/remove-add音量监听、async void Play先捕获Root再取Data、<=0音量=1/NaN保留、await OnPlay后写source/开始回调，无cancel/dispose版本gate。Dispose removeHandle后归零、监听、虚拟清理、Unload后Data=null，失败顺序和反复Dispose保留。
- 860集成通过（20 node+13 base新增），日志analysis/audio-action-integrated.log，O/AUDIO_NODE_ACTION_SOURCE_EVIDENCE.json与AUDIO_NODE_ACTION_AUDIT.json。native GameObject/AudioSource属性用于编辑器测试，加载/update/具体动作/async context为明确probe；本批没有新PlayMode/发声/Player验证。本任务无待续进程。
- 仍11/38控制器生命周期方法，余27；method-map4969。下一步具体Once/Loop/Fade/LoopFade、原始资源/SDK/倒计时，以及AudioManager root/listener/config/node与Composite.NewAudio override/factory，接通UI/controller原生音频。完整关外goal继续active。
- 下轮源码索引：基类3405/26260..26283，Play状态机3404/26284（1420bytes）；Once3415/26327..26334，Loop3410/26301..26307，Fade3408/26286..26295，LoopFade3413/26310..26321；嵌套OnPlay状态机已经在method-map，但按declaring type取，不要猜嵌套type顺序。AudioManager3421/26345..26369，UI3431/26421..26435，AudioData3420 offsets8 id/12Voice/16Atype/20Ptype/24Start/28End/32Vol/36ResPath。


## 2026-09-30 单次/循环原生音频增量
- OutgameAudioPlaybackActions.cs：实际Once/Loop动作，真实native AudioSource.Play/Pause/UnPause/Stop，helper子GO/local/layer和DestroyGO；基类services新增显式new/legacy资源task、OutgameAssetHandle、log/SDK-stop、默认Time.realtimeSinceStartup/原生Destroy。资源与共享update host尚未完整接线。
- Loop加载前捕获config/setting/name，await后使用live Root；clip null直接return，data null才Stop；OnUpdate/OnPause为空。Stop原生停播→end→handle.Release→Dispose；不清handle引用。停止期间load不取消，晚到可以重新开始但无update/data/listener；Root销毁后helper null、设置属性原样报错。
- Once保留EndTime/StartTime在await前写入，空clip报错后deadline=实时+0；deadline必须>0且elapsed>0才结束；0时刻空clip永久pending。已有source End<=0以!isPlaying&&!paused完成；End>0分支到时即停，无paused gate。Stop重置deadline/SDKhandle再end/release/dispose。
- 该微信WASM OnPlay只调度state2/3资源等待，Android state0/1无调度来源；SDK state48只初始化false且无后续写入，不人为打开SDK播放。SdkDuration和SdkHandle在可达路径保持0。
- 主界面1002原始AAC已存在asset-evidence但旧prepare_audio.py仅战斗跳过它。analysis/prepare_outgame_main_audio.py独立解码PCM float32无重采样，Resources/Recovered/Audio/1002.wav，O/MAIN_AUDIO_ASSET_EVIDENCE.json存source/decoded hashes。旧战斗代码/audio-runtime manifest未改。
- 875集成通过（新增15），analysis/audio-playback-integrated.log。13原生PlayMode通过，analysis/audio-playback-native-validation.json / audio-playback-native.log：原始1002/2001实际游标/起始seek/暂停恢复/音量/Stop/释放/帧后销毁/正EndTime暂停门限/自然结束。无设备录音或Player新build；loader/update在验证中显式fixture，不能冒充完整host。无本任务待续进程。
- O/AUDIO_PLAYBACK_SOURCE_EVIDENCE.json、AUDIO_PLAYBACK_AUDIT.json。状态audio-once-loop-native；875/4979/11/27。下步Fade/LoopFade+countdown、完整AudioManager root/listener/config/node ownership、Composite.NewAudio hook/factory、源资源与UpdateManager接通，之后其他完整关外范围。
- 已新抽取helper类型3424（26370..26379）和嵌套3422/26376、3423/26378。26371 CreateAudioSource/26372 Destroy已实现；LoadAudioClip26373/LoadAudioClipForNewRes26374需继续读嵌套；UnLoadAudioClip26375把path.ToLower()+.unity3d后只有!NewRes时AssetBundleManager.UnloadAssetBundle。analysis/audio_disassembly_slice.py PATH STARTHEX ENDHEX可打印紧凑源码及br目标，便于确认复杂状态机。


## 2026-09-30 淡入淡出与完整动作工厂增量
- OutgameAudioFadeActions.cs：真实Fade/LoopFade动作，公共body经原始opcode对比仅name/loop值差异；volume0播放、clipLength、调度Fade后base才写source/Start。required SetCountDownByMillisecond(int,Action<int>,Action<int,int>) / RemoveTime，尚未伪装成已恢复TimeModule。
- Fade targetVolume在await前写入并可由等待中的音量消息更新；每帧in=target*(duration-remaining)/duration，out=target*remaining/duration；Complete先RemoveTime(id)，source销毁也调用end callback；fadein只写target，fadeout写0后调用callback。TimerId从不归0，Dispose重复Remove保留。
- Stop先置Stopping，target=当前native.volume*setting（二次乘设置）；再fadeout。第一次Stop时source空只留下flag；第二次Stop直接release/Dispose，无EndCall、不resetflag。正常CompleteStop按Stop/End/release/Dispose/flag=false；所有失败保留已完成步骤。
- End<=0要求isPlaying且time>=length-1且!Stopping。显式End>0只比较time>=End-1，无stopping/playing gate：Fade第二次Update走重复Stop直接销毁不End；LoopFade每次Update重置渐弱计时。LoopFade渐弱结束只flag=false+渐强，无seek/Play或结束消息。Pause hook空，native暂停不影响外部计时。
- OutgameAudioActionFactory：真实1Once/2Loop/3Fade/4LoopFade，invalid=>null；GetType查Data缺失=>1否则Ptype；static Override typed Func<GameObject,OutgameAudioActionType,int,Action<int>,Action<int>,OutgameAudioAction>先执行，返回null才fallback。元数据typeRef6088证实普通值参数，WASM generic invoker的栈地址不是可变ref规则。type3 ctor注释误标LoopFade但usage3928512明确3408Fade。
- 893集成通过（新18），13新原生PlayMode通过：实际AudioControl state2/7/6/8→UI→Parallel→factory→原始1001 LoopFade及2001 Fade，游标、暂停恢复、声音开关/分数音量、渐弱、end/句柄释放/帧后对象销毁。原生验证的倒计时由明确实时driver提供、update/resource仍fixture，不能声称完整TimeModule/AudioManager/Main已接。
- analysis/audio-fade-integrated.log、audio-fade-native.log、audio-fade-native-validation.json；O/AUDIO_FADE_SOURCE_EVIDENCE.json和AUDIO_FADE_AUDIT.json。状态audio-fade-actions-factory-native；893/4979/11/27，无本任务待续进程。无新Player build。
- 下一步TimeModule3534：已抽取46方法（methodmap此前已有）。SetCountDownByMillisecond27283转AddCoundDown227275；RemoveTime27284转Remove27276。Add要求complete非空否则-1；++field40计时id，先dict56.Add(id,complete)，GetNowTime+duration后dict60.Add(id,end)，有Every则dict68.Add(id,new everyinfo{callback8,type12=1,last16=0})。GetNowTime27271和CheckMillisecond27280/CheckComplete27278/Remove27276、clock/pause需继续核对，不能替换成普通Unitydelta timer。随后shared UpdateManager和AudioManager root/listener/config/node registry/资源load/unload完整接线。


## 2026-09-30 源码倒计时驱动音频增量
- OutgameTimeCountdowns.cs含OutgameTimeClock（shared本地DateTime、1970-01-01 08:00 epoch、毫秒加TimeDifference/整数秒不加）、OutgameTimeEvery（float累积>=interval最多调用一次，成功后归零）、OutgameTimeCountdowns（TimeModule倒计时部分）。未冒充完整startup TimeModule，loop/Unity计时与事件lifecycle仍待实现。
- Add要求complete非null否则-1，unchecked++ID/complete.Add先于clock/deadline.Add，每毫秒句柄interval1。Complete快照keys+callbacks，live查deadline<=now；先callback后写当前实例RemoveList，所有callback完毕才统一Remove。回调抛出导致前面已完成timer仍在下次重播；替换callback不替换快照，删除deadline会跳过待完成callback。
- Check顺序complete/second/ms/minute；later every pass可看到complete回调新注册timer。Every快照对象即便被其他callback从live字典删除也继续调用（缺deadline剩余0）；每次call重新读取Last字段，保留重入影响。remaining先double→int(WASM溢出int.MinValue)再整数除单位，elapsed先float再除单位。
- second>=1000、ms严格>，空字典仍推进Last；minute>=60000且字典非空才推进Last。Initialize/Clear重置LastTime/LastMinute/TimeId，保留LastMillisecond/UnityRealTime。没有focus/pause gate；源码GamePause只触及loopTimers，完整事件所有权待接。
- BindAudio连接实际源倒计时到四种音频服务。913集成通过（新增20），初次1个测试因误判Dictionary复用空槽遍历顺序失败，修测试隔离后通过；最终analysis/time-countdown-integrated-final.log。15新原生通过，time-countdown-native-validation.json / time-countdown-native.log：真实DateTime.Now与源码countdown驱动1001/2001原生音频链，替换前次计时driver；loader/shared update仍fixture，无设备录音/完整Main/Player新build。
- O/TIME_COUNTDOWN_SOURCE_EVIDENCE.json、TIME_COUNTDOWN_AUDIT.json、TIME_MODULE_METADATA.json（3534/3535/3536/3537字段和泛型method解析）。状态time-countdowns-native-audio；913/4993/11/27，无本任务待续进程。
- 下一步完整TimeModule3534：27252priority60、27253init、27254Start监听GF_NewGamePause/GF_GameFocus、27255Update(本地DateTime.CheckTime→CheckLoopTimer→CheckUnityTimer)、27256GamePause、27257Focus、27258Clear、27259..27269 loop、27285..27290 Unity、27291Shutdown移监听/Clear/flagfalse。新抽helper3535/27298..27306、3536/27307..27309、3537/27310..27311，methodmap4993。GamePausetrue枚举loopTimers.Values调用helper.GamePause，false不操作；其他原始细节继续核对，不能以通用Unitytimer代替。
- Countdown component初始化/清理是完整Initialize/Clear的字段投影，未来整合要保留源所有字段重建/清理顺序与callback时机。TimeClock.IsInit static与模块Initialized flag是两个不同字段，不要混同。analysis/audio_disassembly_slice.py已修复忽略混淆类名NEL拆行，支持Time源码br target。


## 2026-09-30 完整TimeModule增量
- OutgameTimeModule.cs实现3534生命周期与3535循环/3536Unity计时器；继承已有OutgameTimeCountdowns共享ID，base拆出protected InitializeCountdownMaps/ClearCountdownMaps/ResetTimeFields，保留完整初始化与清理字段顺序。IOutgameStartupModule ready，完整应用所有权待装配。
- Start注册GF_NewGamePause/GF_GameFocus，允许重复注册；Shutdown每种只移除一次→ClearTimer→IsInitialized=false。GamePause bool true仅flush loop elapsed，false无操作；Focus写static。Update实际DateTime.Now/Unity realtime→countdown→loop(delta/unscaled)→Unity。
- loop先移除队列，再Dictionary.Add待加入，再全体AddTimes，再全体Check。autoPlay=false仍默认未暂停；仅unscaled>1丢弃。AddTimes由pause/focus gating，Check无gate。一次callback后丢弃超额elapsed；GamePause先callback后reset，与Check先reset后callback不同。GetValue返回total，remove先排队再callback。
- Unity先合并cached移除并去重→移除3个map→清complete→按main/second/minute加入→按second/main/minute tick。主timer完成仅排入下帧移除，其sameID秒timer同步移除。helper先Every再live判断完成；Complete先callback再按liveLoop减Duration或置完成；循环保留超额但每步一次。
- ClearTimer/Initialize均保留pendingAdds/Unity移除队列，并重置TimeId，源可能重启ID冲突如实保留。Clear不Destroy loop回调，不reset lastMilli/realtime。所有异常保留已提交的部分状态。
- 938集成通过（新增25），19全新native通过；最终log analysis/time-module-integrated-final.log / time-module-native-final.log，report time-module-native-validation.json。Native使用完整module.Update、实际Unity delta和DateTime驱动原音频；Focus冻结loop/Unity但countdown继续，Shutdown移监听验证。loader/shared audio update仍fixture。无完整Main/Player build声明。
- 初次integrated直接调用ValidateIntegrated返回report但不退出；验证完成后仅终止本任务62448。首次native71540因项目锁退出，无有效结果；最终native62124/integrated51160已正常退出，无待续进程。下次集成请用ValidateMechanicsOnly自动退出。
- O/TIME_MODULE_SOURCE_EVIDENCE.json、TIME_MODULE_AUDIT.json；状态time-module-native-audio；938/4993/11/27。next：shared UpdateManager + AudioManager3421（config/root/listener/node owner，3424异步clip资源helpers），随后其余27控制器和完整应用生产装配。


## 2026-09-30 原生UpdateManager增量
- OutgameUpdateManager.cs实现Type2920实际公开API/Unity消息调用链28方法，MonoBehaviour singleton按Unity fake-null→Find("UpdateManager")→建GO/name→GetComponent/AddComponent→DontDestroyOnLoad。无虚构Awake/OnDestroy清理；销毁后getter重建。BindAudio连接静态AddHandle/RemoveHandleById，每次操作访问真实owner。
- ctor pending id/handle lists和debugData dict、TimePreFrame16；正常/force/fixed lists和IndexDict lazy。共享ID unchecked递增跳过已占key，返回id>2147483645时将storedNext清0。Add立即liveList.Add→IndexDict.Add。
- Update pause gate→移除→force→normal。每pass捕获count，但每次getter/live index；force新增normal同帧可跑，同pass新项下次。FixedUpdate独立pause gate/count，nullskip，无移除处理。普通/force无nullguard/catch。
- 移除按id/委托都先IndexOf去重入队，Update先ID再delegate，nonempty处理后替换list而非Clear。RemoveNow委托只移normal第一项或force第一项，另移fixed第一项；IndexDict按委托相等找第一key仅>=0才删。重复delegate按较晚ID移除可能删除较早key，但debugData按原请求ID删；delegate移除留debugData，缺失ID不删debugData，负key留Index。异常保留prefix。
- O/UPDATE_MANAGER_CALL_GRAPH.json、METADATA、SOURCE_EVIDENCE、AUDIT。65新提取（63manager+2UpdateHandle delegate），methodmap5058。35混淆alternate有不同阈值/边界/循环，不能同名归并；未接productionalternate，未来实际调用需追确切metadata。
- 22806删除foreach的0018bb0d br_if0是循环回0018ba09，helper显示end位置不是实际backedge；逻辑为不相等continue，找到后取key退出。
- 959集成通过（新增21），23原生通过，analysis/update-manager-integrated.log / update-manager-native.log / update-manager-native-validation.json。Native源audioAction由真正MonoBehaviour自动Update驱动，Fixed也真实；TimeModule仍editor pump。原始1001/2001音频fade/play/end/Dispose与源码延迟update移除结合，singleton销毁重建通过。resource/config/AudioManager host仍fixture或待接。
- 验证Unity44220/59144已正常退出，无待续进程。状态update-manager-native-audio；959/5058/11/27，无完整关外Player build。next实际AudioManager3421（26345..26369、多个alternate）和3424异步加载helpers，再其余27控制器/完整主入口与流程验收。


## 2026-09-30 AudioManager真实配置与节点所有权增量
- OutgameAudioManager.cs含Services/Manager/Slot和AudioConfigReader（UnityJson AudioConfig + OnceAudioConfig专用解析）。原始firstpack已有55音频行+53Once时长，本轮读真实数据，未新增伪配置。AudioManager构造连接Actions.GetData和UIaudioSlot.CreateNode，工厂/节点/动作都真实。
- Init26365先flag33 guard；缺ConfigResource且!NewRes才errorreturn，不改旧map。log后replaceAudioMap→live rows，duplicate保留first但仍scanpath.ToLower.Contains audio/once→stickyUseOnce32。replaceOnceMap→若sticky加载（float.Parse当前culture+Dict.Add，duplicate/error保留prefix）→flag33true→EnsureRoot/MoveListener1。root/listener失败flag已提交，repeat不自动repair。Clear不resetsticky/Once/Android缓存。
- AudioRoot new+DontDestroy；AudioListener独立new，没有独立DontDestroy，Move先getUIModule3558.Camera(offset24)再EnsureListener，type1&&camera parent默认worldPositionStays=true；其它type不detach。CreateNode若Root fake-null才Ensure；defaultparentRoot；id viaRandomId(1,10000,keys)，mode1/2/3命名parallel_/sequence_/single_；Nodes[id]=node。
- Utils.RandomId wasm6831直接调用Unity.Random.Range@1883 metadata50652；碰到excluded递归，但丢弃递归返回值，最后返回首次id，允许overwrite追踪并遗留oldnode。证据analysis/audio-random-id-disassembly.txt。原样实现/测试，不修成唯一ID。
- ClearNode liveNodes foreach虚槽6=Stop(0)，不是Destroy；所有Stop成功后clearNodes/AudioData→flagfalse。Destroy随后root destroy/null→listener.gameObject destroy/null（只有listener live时）→UiSlot.Instance.Destroy。AudioManager singleton本身不清，UIaudio singleton清。
- 978集成通过（新增19），18原生通过；analysis/audio-manager-integrated.log / audio-manager-native.log / audio-manager-native-validation.json。原生按真实1001Ptype4、1002Ptype2、2001Ptype1跑controller/UI/manager/action/sharedUpdate/timer，root/listener/真实销毁后重入验证；此前2001Ptype3仅fade测试fixture，不能误作原表。
- O/AUDIO_MANAGER_METADATA.json、SOURCE_EVIDENCE、AUDIT。状态audio-manager-native-ownership；978/5058/11/27，fingerprints2054。Unity72600/32848已正常退出，无待续进程；仍无完整关外Player build。
- 下一步clip资源helpers3424：26373wrapper→3422/26376MoveNext(3764bytes)旧资源；26374→3423/26378(1349bytes)新资源。新分支已核对path.Substring(lastSlash+1).Split('.')[0]结果unused，再path.ToLower→NewResLoadHelper.LoadAssetAsync<AudioClip> metadata26176(genericusage4008644) await并返回handle；无额外release。旧分支还需完整阅读，包括firstpack/newroute/fallback。Unload26375先path.ToLower()+'.unity3d'再if!NewRes AssetBundleManager.UnloadAssetBundle。必须接真实源资源owner，当前native传输与TextAsset加载还是显式fixture adapter。
- 配置通用generic3993488=ConfigRead26612<AudioData3420>，4004888=JsonUtility.FromJson<AudioList2893>（AudioInfos，Name/Length string），4000632=GetModule<UIModule3558>。AudioConfigReader只实现已证据化且Main选择的UnityJson路径，modern/binary路由不冒充完成。


## 2026-09-30 音频资源helper与原生旧资源加载增量
- 当前994集成（新增16）+18原生通过，source method-map5081；controller仍11/38，27剩余。日志audio-resource-integrated.log/audio-resource-native.log，原生报告audio-resource-native-validation.json；状态audio-resource-native-legacy，完整关外Player仍未构建。
- OutgameAudioResources实现26373/26376、26374/26378和Unload26375，Bind到AudioActionServices。basename保持原大小写、仅forwardSlash和firstdot；path.ToLower当前culture；modern handle MainObject转AudioClip无release；legacy先loglower和basename，packDict ContainsKey→log→再次getdict/getItem→nativeLoadAudioClip，存在但空/缺资源不fallback；否则AssetbundleModule._LoadAsset<AudioClip>(lower+.unity3d,basename,null)。new-onlyhelper无自身modeguard；Unload归一化先于mode检查。
- OutgameNewResourceAsyncHelper恢复26176/26186的guard26181（false日志+null）、generic类型和await；具体modern module仍delegate边界。GetNewResModule26169按AppSetting static40 true选KeWanResLoadModule3511(generic4000600)，false选YOResourcesModule3527(4000640)，下一步追查各module typed async load和主程序实际配置。
- 新OutgameLegacyAssetOperation对应23789/91/92、generic23788：bundle lookup→native AssetBundle.LoadAssetAsync；request一旦存在Update立即false，manager移除但await继续看IsDone。error!=null包括"0"日志+done；未获request的failed op仍会留manager list。GetAsset只在request!=null&&done后as T。OutgameLegacyBundleRuntime.LoadAssetAsync恢复23827 check→remap→Acquisition.Load→register。OutgameAssetbundleAsyncLoader恢复26239/26253 ignored falseguard→nulloperation silentnull→await→GetAsset→native-null日志返回。
- IEnumerator await源22357→22389→generic22371/22374/22377已经提取（analysis/extract_audio_resource_generics.py共9sharedbodies；O/audio-resource-generics.json）。OutgameUnityAwait新增仅对sealed OutgameLegacyAssetOperation的overload：手动MoveNext catch并完成awaiter，以保留原异常传播/同步完成；此类型Current始终null、无$this coroutine parents，不宣称恢复通用nested-wrapper。原generic single-yield会丢异常，本轮asset default不再使用。
- 原生测试BuildPipeline将已恢复原始2001音频重建Windows AB。首包audio分支映射为显式测试fixture，不冒称原始firstpack包含音频；真实Config读取已有rebuilt91asset firstpack内AudioConfig/OnceAudioConfig（55+53）。actualfile UWR+manifest+native AssetBundleRequest+AudioManager+UpdateManager+OnceAction，ref2→1→0、自然结束卸载、missing asset null诊断、nativeUnLoad销毁clip、再次真实下载、同步MoveNext异常传播均验证。Modern typed module acquisition未完成，不用fake成功。
- O/AUDIO_RESOURCE_SOURCE_EVIDENCE.json/AUDIT.json保存source hashes/usage IDs/边界。本轮自建Unity37040/55736/66928/48228全部正常退出；无需要续等的进程。最新fingerprints由脚本重算。
- 下一步modern模块loading以及full Main资源/配置owner；随后继续27controller/可操作业务/存档与完整Player验收。保持整体goal active，无阻塞。


## 2026-09-30 Buff/Tool控制器注册与奖励入口增量
- 当前1008集成通过（新增14），无新native/Player；controller lifecycle13/38，剩25；source method-map5089；状态buff-tool-controller-registration。验证日志analysis/buff-tool-controller-integrated.log；本轮Unity752已退出。最新native仍上一轮audio-resource18，不能当本轮新native。
- 重新读取O/RESOURCE_MODE_AUDIT.json，确认AppSetting static36默认false且原有90641body扫描未找到直接enable写入。决定走source-default legacy主线，现代模块仍记录未完成但不阻塞原始Main必经controller接线；不把现代加载当必须先完成的默认入口。
- 新OutgameBuffControl.cs（含抽象OutgameBuffBase）：4025构造Dictionary<int,BuffBase>；Init订阅当前GamePlayState，重复会重复；30981回调/30983Update源nop/end。Dispose当前dispatcher RemoveListener一次→existingdict.Clear→registry.Clear4025，异常保留prefix，不遍历Buff.Clear。BuffBase.Equals源TargetId.GetHashCode()==obj.GetHashCode无null/type防护，GetHashCode使用CLR identity代替WASM地址hash（明确运行时差异），Clear空。
- 新OutgameToolControl.cs：组合已有真实OutgameToolDispatcher，恢复4256源GetReward/GetMultiReward live List<ListArrayInt> indexed循环，row.datas[0]/[1]→currentPurchaseCost(reason)→Change(...notifytrue)，忽略bool、逐条保存、无事务；multi unchecked Int32乘法。GetItemCountById始终local；GetItemNum id>=8001走原ItemModule Int64count后wrapint，低IDlocal；IsEnough signed>=。Main OnInit/Update/Dispose源全空（特别Dispose不clear singleton）。CoreBindings.Bind加4025，BindTools加4256；既有战斗文件不改。
- 14checks包括真实registry/LogicModule生命周期、重复订阅、当前dispatcher切换、旧对象清新slot、字典null失败次序、Buff unsafehash比较、live奖励追加/reason变更、拒绝扣款仍继续、部分保存失败、malformed row前缀、乘法overflow/zero、8001边界64→32、local替换、报告flag。effects为明确sink，不冒充完整平台/存档。
- matrix generator analysis/audit_outgame_controller_lifecycles.py已加4025/4256并重跑，O/CONTROLLER_LIFECYCLE_MATRIX/AUDIT与状态同步13/38、25余；O/BUFF_TOOL_CONTROLLER_SOURCE_EVIDENCE.json/AUDIT.json记录证据hash和边界。
- 本轮预查ItemModuleControl3875（尚未生产实现）：OnInit30185→InitMgr30181先ItemConfigMgr.Instance.dicGameItem(field8)存this24，再helper30180 foreach currentconfig.Values；type1==4 dictProp(field12).Add(id,row)、5 dictSkill(field16).Add、6 dictHero(field20)[id]=row，构造30187新三个dict但reinit不clear。getItemMgr30182 lazy DataManagerPool.GetModel<ItemManager4500> generic3995108，null允许下次重查。Dispose30188只clear slot。GetItemNum30186→ItemManager.GetItemNum34339→GlobalItemManager.GetItemCount。GetItemConfig30183→ItemConfigMgr.GetGameItemConfig。ChangeItemNum30179 longcurrent+signedintdelta unchecked，结果<0 warning道具 {0} 数量不足!!! returnfalse；否则manager.GetItem(config).虚槽3(delta)→Event.Item_Change static52(id)→pool.SaveData→true，具体工厂尚需完成。
- ItemConfigMgr共享GameItemConfig为type4527，不能混用现有OriginalConfig.GameItemConfig3942（project-specific）。ItemManager4500已source提取但未runtime wrapper；OnInit34334虚槽10 UpdateData(true)、GetItem34342 newFactoryBase4486(config.id)→IFactory.Produce；FactoryBase34292仅type1==1和9生产VirtualItem/Package factories，其它返回null；constructor34293从当前ItemConfigMgr字典找id，缺失error不throw但后续可能null。新提取BuffBase+FactoryBase补method-map至5089。后续优先补ItemModule/ItemManager/config/factory真实owner，不用globalitem字段直接冒充wrapper。
- Source tool generic32536首参为enum，数值转发已实现；32533为int完整change。GetReward reason为Global4272.PurchaseCost static52，cctor32608字符串PurchaseCost。GlobalItemCount provider仍显式边界，不虚构数量/登录。完整目标保持active无阻塞。


## 2026-09-30 共享道具配置增量
- 1020集成全通过（新增12），log analysis/shared-item-config-integrated.log；本轮Unity69928已退出，无新native/Player。method-map5147；controller仍13/38、25余。本轮没有声称ItemModule生命周期完成。
- 新OutgameSharedItemSchemas.cs由analysis/generate_shared_item_schemas.py按原metadata生成6类（共享4527/4529/4531/4533、Lang3490、ListArrayInt3452），独立AreaBattle.SharedItemConfig命名空间，防止与项目玩法类型3942混用。同名原始firstpack JSON真实读取79/4/16/10条。共享product字段groupID/itemId原JSON不存在则保持0，16条全入group0，不编造字段映射。
- 新OutgameItemConfigManager.cs：构造六个dictionary（package/reward index初始null）、slot lazy创建不自动Init；InitLegacyUnityJson显式选择原始Unity JSON支路，四表顺序item/package/reward/product读后BuildGroups，不清表。MemoryPack/alternate parser尚未绑定且未伪装支持。
- BuildGroups先替换package index→foreach按packageId/packageRewardId indexer覆盖，再替换reward index→按rewardId/rewardItemId覆盖，再Products.Values按groupID追加既有List（不clear）。泛型usage3954552/3955676/3954568/3955688均set_Item，3954820商品外层Add。失败保留已执行前缀，后续阶段不运行。
- Package/Reward查询miss新建空dictionary不cache，hit返回原引用。GetRewardItemRanges34560缓存原GameRewardConfig引用与weight snapshot（RandomObject3392 int offset8）；cache成功构建才publish，返回浅拷贝List/复用元素；group rebuild不clear cache。GetGameItemConfig使用receiver，GetGameProductConfig使用当前slot。Dispose只clear两个cache后clear当前singleton，保留原四表和两组；旧receiver仍会清掉新slot，异常保留前缀。
- 新OutgameItemConfigValidation12checks覆盖以上原表解析/重复初始化109重复日志和商品32项、分组重复id覆盖、浅拷贝和权重快照、空cache不失效、失败前缀、getter旧对象/当前singleton差异、dispose顺序。通过真实Unity JsonUtility+Resources加载恢复JSON，但无native资源传输新验证。
- O/SHARED_ITEM_CONFIG_SOURCE_EVIDENCE.json/AUDIT.json、SHARED_ITEM_CONFIG_SCHEMAS.json记录源方法hash、元数据和范围；analysis/inspect_shared_item_metadata.py可重查字段/泛型。新抽取GamePackageConfig/GameRewardConfig/RewardRangeData至methodmap5147。
- 下一步ItemManager4500 wrapper和FactoryBase4486→VirtualItem_Factory4488/PackageItem_Factory4487→实际item entities，再ItemModuleControl3875接线。工厂和manager源已在O/disassembly，尚未实现，不用GlobalItemLifecycle直接冒充。全目标继续active。


## 2026-09-30 ItemModule/ItemManager/工厂分派增量
- 1034集成全通过（新增14），日志analysis/item-module-integrated.log；Unity65828已退出，无新native/Player。method-map5152；controller生命周期14/38，剩24；不是业务图完成。状态item-module-manager-factory-routing。
- 新OutgameItemModuleControl.cs，CoreBindings.BindItems注册3875。Init二次读取current config（第一次保存Items，第二次遍历Values），4/5类别Add、6覆盖，无clear；Dispose仅清registry3875，保留map/cachedmanager。getItemMgr懒取pool.GetModel4500、null重试。Change先long当前数+signedint unchecked、negative警告退出；否则Manager.GetItem(currentconfig).AddItem(delta)→current message Item_Change(id)→current pool.SaveData→返回先前next>=0。不加clamp/recheck/零值跳过。源虚表220/224是slot4 AddItem（不是此前summary粗略的slot3）。
- 新OutgameItemManager.cs：IOutgameDataManager接已有Storage和GlobalItemLifecycle，OnInit UpdateData(true)，Callback global.Initialize(text,GetNowDateTime)，OnSave global.Save→storage.SaveLocalData，OnRelease global.Release；Update/ExpendReward源nop。GetNowDateTime为local DateTime.Now provider，不走server。独立OutgameItemManagerSlot懒取pool4500，OnInit/Release不清slot。AddRewards live indexed list跳过GoodsType6，其余取current ToolControl后low32 reward count→ToolChange(...false,empty,false)忽略bool。GetPackageOpenReward新List.AddRange原LinkedList后原表Clear，共享行引用。RewardData4549字段int id,long count,int order。
- 新OutgameItemFactory.cs恢复FactoryBase ctor双次currentConfiglookup、缺失log保留null，Produce只支持type1=1虚拟/9礼包。子工厂再次源ctor查表；type2映射1 Gold4496、2 Diamond4494、4 Strength4499、5 Commander4493、8 HeroPiece4498、10 HeadBox4497、11 GamePoints4495；Package1 OpenAuto4490、2 OpenManual4491，其他null。实体构造依赖Func<sourceType,id,IOutgameItemEntity>明确尚需实现，不做通用库存fallback。
- 新OutgameItemModuleValidation14checks：真实Global/Storage内存backend重启和Int64持久化、真实login门槛（测试显式progress10、生产无假登录）、serverdownload、manager/currentGlobal及lazy缓存、release顺序、Tool真实奖励live list/type6skip/long截断/保存失败、package行引用、9实体类型分派、缺失和工厂二次查表、module分类/reinit异常prefix、dispose保留状态、余额不足/Int64overflow、zero/消息/保存顺序以及entity/message失败。实体只recording fixture验证调用，不声称各实体效果已恢复。
- O/ITEM_MODULE_SOURCE_EVIDENCE.json/AUDIT.json记录证据和界限，matrix/audit14/38同步。完整生产Main/平台/关外Player仍未完成。下一步实现ItemBase4541→GlobalItemManager奖励引擎→九个具体实体，接入construct provider。
- 本轮新抽取ItemBase4541五方法（34564ctor、34565AddItem、34566AddItemOnlyModel、34567ItemToReward、34568Use）。Ctor直接currentConfig.Items[id]；ItemToReward非nullConfig返回一个RewardData(id,longcount,order0)，nullConfig返回empty。AddItem先resolve global，再virtual ItemToReward(count)→global.AddRewards；OnlyModel同理AddRewardsModel。Use先Item_Use(id,longcount)消息，再构造NewReport_item_use report设置id/item_game/type1/type2 nullable fields并上报；尚未生产实现。VirtualItemBase.AddItem等小方法反汇编的共享函数label可能是ActivityVirtualItemBase/TaskLivenessPoint，不能按label误判实际继承，metadata确认父为ItemBase4541。Global.AddRewards f12045、AddRewardsModel f12046，方法map检索字段非name（用已有结构字段）。


## 2026-09-30 Global奖励引擎与金币/钻石实体增量
- 1050集成全通过（新增16），最终日志analysis/global-item-rewards-integrated-verified.log；Unity43036已退出。首次70128编译因Initialize(null)重载歧义失败，改为InitializeWithHost后64040跑1049通过；复核snapshot函数发现本轮初稿/初始说明把34596误认商品快照，已按字段36/44修成道具快照并加nullhost检查，最终43036跑1050通过。只有最终结果用于manifest。无新native/Player，14/38 lifecycle不变；method-map5152。
- 新OutgameGlobalItemRewards.cs：AddRewards/AddRewardsModel委托Action<List<RewardData>,int>覆盖整个默认路径、第二参0；默认live indexed rows逐条Change，regular末尾currentIItemManager.AddRewards(original)，model无host。Expend unchecked负long再Change，无不足检查，末尾host.ExpendReward原正数list。AddRewardsByItemSelf逐条current config→GetItem→非null AddItemOnlyModel，再host.AddRewards一次。GetItem nullconfig直接null、common非null优先、common返回null回退host。
- Change helper34589既有row先unchecked longadd后统计；newrow先构造/统计再holdTime0/Items.Add，重入插入会让原Add抛异常。随后正确SnapshotItem34596/f12037（不是SnapshotProduct34583）→ItemUI_RefreshUserItem→Report→dirty=true；O/GLOBAL_ITEM_REWARD_FIELD_AUDIT.json与analysis/audit_global_item_reward_fields.py以metadata泛型和offset证明live36/snapshot44 ItemUserData4548，而product32/40 ProductUserData4547。旧说明和本轮初版商品快照推断作废。
- Report helper34581先Item_ItemChange(id,longdelta)→ReportDel(id,longdelta,reason)如有则替代默认。默认currentconfig.Items[id]（missing抛、presentnull日志返回）；报告factory先执行，填id/item_game/type1/type2；delta>=1用get，否则cost（0也cost），delta出signed32范围转0，intMin abs按WASM位算仍intMin；balance factory后读取，出范围0；再次resolve报告服务发送。IOutgameItemReports明确只承载源拥有字段和创建/发送顺序，尚未实际SDK发送。
- 新OutgameItemBase.cs：ItemBase ctorcurrentconfig.Items[id]，ItemToReward新list，config非null单row(id,longcount,order0)否则empty；Add/OnlyModel先capture currentengine再virtualToReward→AddRewards/Model。Use只Item_Use(id,longcount)→create/填use report→send，不扣数量。VirtualBase转发；真实Gold4496/Diamond4494纯构造继承。OutgameItemEntityServices.Construct这两类真创建，剩七类继续required AdditionalConstructors，不fallback。IOutgameItemEntity新增AddItemOnlyModel，旧recordingfixture仅兼容接口。
- GlobalLifecycle新增RewardHost和InitializeWithHost(text,host)，先发布host再既有record初始化，hostnull允许localclockfallback；ManagerCallback改用此入口，actualManager实现IOutgameGlobalItemRewardHost，解决此前只传clock丢失奖励host问题。
- 15新GlobalRewardschecks+1Module真实链：统计新/旧prefix、重入Add冲突、正确ItemSnapshot先于UI、四委托替代、live奖励list、report覆盖、zero/long溢出/intMin金额、negative库存/longMin负号、配置missing/null、create回调改余额、report失败、commonfallback、BySelf模型路径、Base空配置/Use不扣、virtualToReward前捕获engine、nullhost。实际module→manager→factory→Gold→global→manager→Tool/local→pool/storage保存，再实例化读取，金币+7/-3两套库存均4；无实体recording替身但报告/生命周期为显式hostfixture。
- O/GLOBAL_ITEM_REWARDS_SOURCE_EVIDENCE.json/AUDIT.json、状态/manifest/交接已同步；完整目标继续active。下一步具体实体Strength4499、Commander4493、HeroPiece4498、HeadBox4497、GamePoints4495、PackageOpenAuto4490/Manual4491，源body均已抽取（Type449x-343xx）。注意共享WASM函数label会显示ActivityVirtualItemBase/TaskLivenessPoint，要按metadata继承解释。


## 2026-09-30 五类虚拟道具实体增量
- 1063集成全通过（新增13），日志analysis/virtual-items-integrated.log；Unity51948已退出，无新native/Player。method-map5170（新抽DiceGameDataManager22函数其中18新增）；控制器14/38、24余未变。状态virtual-item-entities。
- 新OutgameVirtualItems.cs恢复Commander4493、HeadBox4497、HeroPiece4498、Strength4499、GamePoints4495，OutgameItemEntityServices.Construct现在七种virtual实体真实创建，只PackageAuto4490/Manual4491走AdditionalConstructors。原始共享表不支持的type2=6/7/9仍null，不填充。
- Commander Add/OnlyModel虚调用Use(count)，Use忽略quantity，currentCommanderManager.UnlockCommander(paramInt)→currentUI.RefreshCommanderItem再次读paramInt。新增真正manager.GetCommanderData31013 TryGetValue missnull；Unlock31015无条件curLevel=1（CommanderData4030.offset16确认），高等级也重置，未知id nullfault，界面失败保留先前解锁，不加价格、库存或report。UI接口IOutgameCommanderItemRefresh仍须fullUI owner提供。
- HeadBox Add/OnlyModel同样virtualUse；Use currentHeadBoxInventory.Unlock(paramInt)。窄服务复原UserInfoControl32198 guard→UserInfoManager32228 List.Contains→32229 List.Add→UserInfo_HeadBoxUnlock(id)。controller去重、manager直接Add允许重复；List由真实UserInfoData owner provider供给，当前测试独立List，未声称完整UserInfoManager生命周期/存档接线。
- HeroPiece三操作严格base，Use不扣库。Strength Add→baseAdd后SP_CHANGE(nullargs)，Use→baseUse后SP_CHANGE；OnlyModel继承无SP。Event4271静态56/cctor32607证据已核对。
- GamePoints Add/OnlyModel respectivebase后currentDicePoints.AddPoint(low32count)。源DiceGameControl31102实际丢弃参数调ChangePoint(zeros)，DiceData31135又忽略delta，GetPoint31144=(int)currentGlobal.GetItemCount(11001)，return/message max(point,0)→Mxtz_DataChange(1,result)，不再次加/修复原long库存。这三个窄方法由OutgameDicePoints生产服务实现，尚非全DiceController。
- 13checks真实原始79行表、7类型工厂、unsupportednull、Commander高level重置/negative/zero/未知id/UI失败/currentparam/虚Use数量传递、头像去重/live记录/消息失败prefix、Strengthnormal/model/use差异与当前dispatcher切换、HeroPiece真实base、积分不双加/低32截断/clamp仅消息及失败prefix。复用GlobalRewardsValidation的internalfixture，实际奖励引擎/manager/原表，有明确报告/界面sink，不冒充全平台。
- 新O/VIRTUAL_ITEMS_SOURCE_EVIDENCE.json/AUDIT.json、state/manifest/交接同步。下一步PackageItemBase4489.ItemToReward34300(1599bytes)、Use34302(179bytes)、AddItem34304/OnlyModel34303(basecalls)；OpenAuto4490.ItemToReward34305(2078bytes)、Add34306/OnlyModel34307(39bytes虚Use?须核对slot)，Manual4491仅ctor。已抽源body可继续读，不要凭常规礼包机制假设概率/循环。全目标保持active。


## 2026-09-30 礼包实体和权重随机增量
- 1077集成通过（新增14），最终log analysis/package-items-integrated-final.log；Unity72384已退出。初68908编译因Random歧义失败，Editor加System.Random alias修复。无新native/Player；14/38 lifecycle不变，method-map5170（generic另存3不入map）。
- 新OutgamePackageItems.cs：PackageItemBase4489/Auto4490/Manual4491及OutgamePackageServices。工厂现在9实体全具体创建，移除AdditionalConstructors；未知原type2仍源null。Packages.Manager须currentItemManager provider；NewChanceRandom默认每group newSystem.Random；weight/quantity共用GameRandomSource.Shared。
- Base34300和Auto34305：Int32 outerindex unchecked增量，signextend与longcount比，非正空，不擅改long循环/上限（>intMax可能源wrap，不测试不可终止情况）。每次currentConfig.GetPackageRewards(paramInt) KVP枚举，每row freshRandom.Next(0,10000)<rewardRandom才获currentGetRewardRanges；初始空跳过。j按live rewardItemCount，weightedselect→remove→quantity Inclusive(min,max)→order；不每轮检查候选空，overdraw会空selector[index0]异常，不补位/截断。
- GenericRandom26203 f10956+lambda26209 f15316从registration提取至O/disassembly/PackageRandomGeneric-{26203,26206,26209}.txt及package-random-generics.json；26206为另一个sortlambda仅证据不使用。weightednull日志权重随机集合不能为空并null，empty不日志但Next(0,0)后index0异常；sum unchecked，roll shared.Next(0,total)，live cumulative严格roll<sum，fallback0；不修负/溢出权重。analysis/extract_package_random_generic.py可重抽。
- Auto与Base差异：选中后Auto先currentconfig.Items[itemId]再type1==9则currentglobal.GetItem→Inclusive数量→非null virtual AddItem，立即副作用，不appendnestedrow；nullentity也消耗数量RNG。Base不查itemconfig、不展开。slot4由helper2866调table2088→wasm14832明确VirtualActionInvoker，概率Next helpertable6324→f1901槽6。
- Use34302先baseUse（Item_Use/report）→virtualToReward→逐row currentManager.PackageOpenRewardsTemp.AddLast（共享row）→currentGlobal.AddRewardsByItemSelf。Base Add/OnlyModel调用baseItemBase，因此也virtualToReward，但不Use队列/报告。Manual仅ctor继承。Auto Add/OnlyModel virtualUse，模型路径也会hostAdd。无补造礼包扣库或保存。
- 13Packagechecks+1Module真实链：原20000每份2无放回（min RNG gold10 diamond5）、共享cache不删、概率边界、null/empty/zero/overflowweight、overdraw、inclusive max+1和intMax overflow、原20001嵌套8000立刻奖8101而外11001稍后、队列nested先/引用、manualAdd/Use差异、autoOnlyModel fullUse、失败prefix、每row currentManager、auto缺配置 vsbase、live枚举异常；真实module20000→实际pkg/global/manager/Tool/local/存档重读gold10/diamond5，未插入礼包库存。
- 关键下一步：rg发现ItemConfigMgr.InitMgr唯一已抽caller为GlobalItemManager.get_Instance34590/f2040：ifglobalnull newctor→先publishglobal→currentItemConfigMgr.InitMgr，再returnstaticcurrentglobal。不能由ItemConfigSlot getter自动init；现需把此owner的failure/reentrant语义、Release/reset和config/reward/entity实际服务组成起来，接source-default legacyDataManager启动。查Type4542-34570 ctor及34590 getter/34577Release。
- O/PACKAGE_ITEMS_SOURCE_EVIDENCE.json/AUDIT.json、state/manifest/交接同步，完整目标仍active，后续仍需UserInfo/CommanderUI/Dice/report owners、24control及完整Main/关外Player。


## 2026-09-30 GlobalItemManager 单例宿主增量
- 1087集成通过（新增10）；analysis/global-item-slot-integrated.log；Unity47084已确认退出。14/38控制器生命周期不变，method-map5170。无新native/Player。
- OutgameGlobalItemSlot.cs：一个原4542对象对应Lifecycle/Rewards一对实例；get34590先construct/publish，再currentConfig.InitLegacyUnityJson，最后重读slot返回。config初始化失败保留partialowner/tables，不因下次getter自动retry；构造失败不publish；初始化回调释放/替换时返回current null/newowner；旧owner Release照源码也清新槽。
- ctor34570只设统计id10020和Products32/Items36。修正OutgameGlobalItemIndexes早建snapshots：40/44保持null直到InitializeProducts/Items；既有3个已初始化快照测试显式执行初始化。新增pre-init奖励case证实消息/插入后nullsnapshot失败，不默默补初始化。
- LifecycleHost接current config.Dispose→clearglobal、update注册/移除、统计注册MsgDispatcher object[]{eventId,query}。BindEntities/CreateFactory跟随current slot。单例测试的更新为受控注册边界，不是完整产品tick宿主；生产product-update/reset/price/config供应者及Main/pool仍待组成。
- 实际ItemManager首次本地callback触发4表加载再record过滤→host/time/update/index/statistics，Gold model-only→Int64保存→同manager release/reinit→独立实例重启；服务端分支仅请求，等显式callback才创建全局。login10是隔离测试前提，Tool另由既有真实链验证。
- 10checks含失败/重入/null/current替换/stale release/更新移除及config释放失败prefix。证据O/GLOBAL_ITEM_SLOT_SOURCE_EVIDENCE.json/AUDIT.json；完整关外未完成。
- 下一步：实际product update/reset/price与共享config适配、旧资源reader、data pool/ItemManager/entity服务装配；继续24控制器及完整UserInfo/CommanderUI/Dice/report/Main/account/menu/business/Player。


## 2026-10-03 恢复完整目标：商品服务与用户资料
用户明确授权恢复并继续完整复原，目标 active。新增商品共享配置/刷新/重置/价格及 ItemRuntime 真实服务组，原生帧更新和文件保存重启验证16项通过；新增UserInfoManager/Control与头像框实体记录保存链7项。完整集成1098项通过，2086个输入文件与隔离验证副本逐字节一致；控制器15/38，余23。验证环境为Mac Unity6000.3.7f1隔离副本，原工程仍6000.0.68f1。无新Player或完整大厅验收。早期无图形截图崩溃及Mac触控读回断言已定位，最终启用图形完整检查通过。详见ITEM_PRODUCT_SERVICES_AUDIT、USER_INFO_AUDIT、USER_INFO_SOURCE_EVIDENCE和AREA_BATTLE_HANDOFF.md。下一步Main/account/model factories、RandAIInfo32592、CommanderUI/Dice/report及余23控制器；全部关外业务和原机视听时序验收仍未完成。


## 2026-10-03 原始昵称与用户资料/道具原生联调

恢复 ConfigHelper.RandAIInfo32592 与 RandomHelper.Randoms26196，保留国家映射、饱和重试、无放回抽样、插入顺序和两套随机源。新增 OutgameUserInfoRuntime 将原始昵称生成接入实际资料 manager，头像框奖励与 ItemRuntime 共用同一份资料。

隔离 Unity 6000.3.7f1 完整回归 1105 项、定向检查 14 项、真实 PlayMode 23 项通过；2089 个验证输入与工作区逐字节一致。原工程仍为 6000.0.68f1。原生检查验证登录门槛、真实磁盘保存/独立重载，以及资料和道具的显式服务端回调。账号/报告为显式测试宿主，不代表真实平台接入。

控制器仍为 15/38，完整 Main/账号/其余 23 控制器和全部关外业务、Player 构建及原版视听验收仍未完成，目标保持 active。证据见 AI_NAME_SOURCE_EVIDENCE.json、AI_NAME_AUDIT.json、analysis/ai-names-integrated-final.log、analysis/item-profile-native.log。


## 2026-10-03 图鉴资料与控制器

新增 OutgameGuideBookManager/Control/Runtime，接入真实数据池与控制器注册。根据原始方法恢复解锁边界、递归引导查询、领取记录、保存/通知顺序和释放行为；实际文件存档独立重载通过。图鉴 UI 和实际道具发放仍未接通。

8 项定向检查、1113 项完整回归通过，2093 个验证输入逐字节匹配。控制器生命周期绑定 16/38，剩余22；反汇编索引5179。原生23项为上一批资料/道具验证，本批未重跑。完整 Main/账号/全部业务及 Player/视听验收仍未完成，目标保持 active。证据：GUIDE_BOOK_SOURCE_EVIDENCE.json、GUIDE_BOOK_AUDIT.json、analysis/guide-book-integrated.log。


## 2026-10-03 图鉴原始资源与实际领奖按钮

原包清单7个依赖包全部通过大小/MD5复核，导入原始 GuideBookUI/GuideBookItem（81/10节点）、58精灵、1字体和原生视口遮罩。新增实际领奖按钮接线，调用真实 ToolControl/LocalData/GuideBook 保存链，保留经济数据→效果→领取记录→UI顺序及失败/重入行为。7项定向检查、1120项完整回归通过；2224个验证输入与隔离工程一致。效果/声音/报告为观测测试端点；没有新的 PlayMode/Player 构建或完整原机验收。

控制器仍16/38；原始方法索引5231。下一步恢复完整图鉴弹窗、动态列表、TipBookItem、标签和Spine生命周期，继续Main/账号/其余22控制器及全部业务。目标保持active，不将按钮级验证视为完整页面完成。证据：GUIDE_BOOK_UI_SOURCE_EVIDENCE.json、GUIDE_BOOK_UI_REWARD_AUDIT.json、analysis/guide-book-rewards-integrated.log。


## 2026-10-03 图鉴条目、页签与原生帧布局

恢复 GuideBookItem/TipBookItem 的原始名称/红点/锁定提示/指针回调，接入原始提示列表、单选展开和 TabButton/Group 生命周期；补正领奖按钮回调正常返回后的 GF_UIButtonClick。协程类型证据纠正为4368，保持 WaitForEndOfFrame 延迟、读取最新选中状态与 BaseItem 原生销毁。

7项新增回归、1127项完整检查通过；真实可见 Game 视图下23项 PlayMode检查通过，覆盖展开130→330、页签切换、领奖后布局、实际文件重启和销毁。批处理帧末检查失败另存，不能替代此原生结果。2232个验证输入文件与隔离副本逐字节一致。控制器仍16/38，方法索引5359。

完整图鉴动态列表/弹窗/Spine/整页生命周期、本地化声音效果真实服务、Main/账号/其余22控制器及全部业务、Player/原机验收仍未完成；目标保持active。最新证据：GUIDE_BOOK_BROWSE_SOURCE_EVIDENCE.json、GUIDE_BOOK_BROWSE_AUDIT.json、analysis/guide-book-browse-integrated.log、analysis/guide-book-browse-native-rendered.log。


## 2026-10-03 图鉴原始弹窗与Spine

完成图鉴原始弹窗标题/说明/图片、指挥官技能映射、攻防数值与附加提示、遮罩/OK关闭与领奖显隐；解析实际SkillControl.CurCommanderId，保留重入、缺失配置和失败顺序。图鉴自身YD_0与13项依赖已导入，保留独立网格、材质、纹理、骨骼/atlas及原生播放设置。GuideSprite本地查找包含25个原始精灵引用。

6项新增检查、1133项完整回归通过；14项原生PlayMode通过，验证真实骨骼帧、隐藏/恢复、点击/领奖与文件重启。4张原生渲染图已检查；测试相机/画布、SkillControl/voice/close端点和本地atlas查找不代表完整Main或原包视听匹配。2266个验证输入与隔离副本逐字节一致，控制器仍16/38。

继续完整DynamicList/ListData和BaseUI页面生命周期，再完成Main/账号/其余22控制器及全部业务、Player与原机验收。目标保持active。最新证据为GUIDE_BOOK_POPUP_SOURCE_EVIDENCE.json、GUIDE_BOOK_POPUP_AUDIT.json、analysis/guide-book-popup-integrated.log和guide-book-popup-native-verified.log。

## 2026-10-03 DynamicList 滚动/回收与图鉴实交互

完整目标继续 active/in_progress。恢复原始 DynamicList provider、布局、可见槽位复用、LateUpdate、单项刷新和反向对象池回收；图鉴行已接入实际弹窗/领奖回调，修正 provider.Data 替换后的引用行为。新增7组，最终1140项完整集成通过；15项原生 Game 视图检查通过（指针拖动、复用后点击、领奖、销毁回收、重开和保存重读）。2270份输入逐字节匹配；方法索引5434。三张截图已检查，验证画布不是原版视听验收。证据见 DYNAMIC_LIST_AUDIT.json、analysis/dynamic-list-integrated-final.log、analysis/dynamic-list-native-final-source.log。

控制器仍16/38；尚有居中/tween列表路径、完整GuideBookUI/BaseUI/Main/account和剩余22控制器及全部业务闭环，最终Player/视听验收未完成。本批不增加完成率，也不将完整目标缩为图鉴。


## 2026-10-03 图鉴整页资源生命周期与原始定位协程

完整目标保持 active/in_progress。恢复 GuideBookUI 页面所有权，组合原始两帧加载、24出口、列表/弹窗/奖励/页签 Awake、注册与异步关闭/Dispose/三类句柄释放；UIPath 按元数据校正为 MainMenu/GuideBookUI。新增单/双索引居中与 scaled WaitForEndOfFrame，保留旧协程完成停止最新句柄的源行为。

最终1148项集成（新增8）、26项真实Game视图检查通过；2274份输入逐字节一致。验证原始UIRoot/Canvas、点击领奖、关闭/原生销毁、对象池重开和独立存档重读，三张截图已检查。原生定位夹具的中心轴心引起ScrollRect回弹，已修正夹具布局并按实际协程完成验收；失败诊断保留，生产算法未改。证据 GUIDE_BOOK_PAGE_AUDIT.json、analysis/guide-page-integrated.log、analysis/guide-page-native-verified.log。

控制器仍16/38、方法索引5434；资源取得与账号/SkillControl/语言/atlas/音频/效果仍有明确fixture边界。下一步实际Main/account/data-pool/剩余22控制器与完整启动→大厅→战斗→奖励→返回→重启闭环，继续其余业务及Player/原机视听验收。没有新Player构建，不把页面通过当成全目标完成。


## 2026-10-03 BattleControl 原始配置与真实注册

补齐 BattleControl4060 全部8方法，接入 CoreControllerBindings/LogicModule，三次ConfigMgr解析捕获原始Dispatch配置1/2/3；保留行引用、缺键后部分赋值、空行、重新初始化及Dispose清槽的原始行为。出兵/增长间隔、线路和分数在正常、负值和极端参数下与现有BattleSimulation一致，现有战斗实现和存档未改。

5项新增检查、1153项完整回归通过，2276份输入与隔离副本一致。生命周期绑定17/38，余21；方法索引5434。本批没有新PlayMode/Player：此前图鉴1148版本的26项原生结果保留。证据 BATTLE_CONTROLLER_SOURCE_EVIDENCE.json、BATTLE_CONTROLLER_AUDIT.json、analysis/battle-controller-integrated.log。

完整目标保持active，下一步Main/account/data-pool与剩余21控制器、真实页面资源/技能/语言/音效服务、启动到战斗奖励返回重启及全部其余业务、Player与原机视听验收。局部通过不能替代最终交付。


## 2026-10-03 项目红点与真实菜单生命周期

恢复Proj_hdzd.RedDotControl4453及item4457/event4454，接入实际registry/LogicModule/MenuView；保留模式、unscaled计时、重复列表、Check→Show、异常与释放顺序。原MenuTabUI的Tower_CheckReddot空target事件记录完整保留，不补造业务条件。

最终1161完整检查（新增8）及17原生检查通过；2281份输入一致，生命周期18/38，余20，方法索引5452。原生验证Start、timeScale0仍刷新、隐藏/disabled仍检查、帧末销毁注销、真实菜单重开。测试为观察生命周期显式激活原本隐藏的道具页签，运行时listener仅为probe；无截图/原版菜单可见性或Player主张。初次inactive SendMessage夹具问题已修正，失败报告保留；UnityEditor.Search启动异常单列。

证据RED_DOT_SOURCE_EVIDENCE.json/RED_DOT_AUDIT.json、analysis/red-dot-integrated-final.log、analysis/red-dot-native-final.log。混淆未调用别名34168的WASM数组边界仍单列待验收。下一步真实ActivityControl/ActivityManager/Statistics和七日启动依赖，继续Main/account/data-pool/剩20控制器、全部业务、保存返回重启与Player/原机视听；完整目标保持active。


## 2026-10-03 公共消息与统计记录基础增量

- OutgameCommonMessageDispatcher4569独立于游戏MsgDispatcher；单例替换、旧实例、参数浅拷贝/key追加、重入/异常行为已恢复。
- GameStatistics记录DTO与35015..35022计数/设置/重置/读取、35011索引循环、35036..38消息key缓存已实现。保留父子计数独立、null/空列表差异、itemId0首次写总量而后写子项、溢出、缺失字典及失败顺序。
- 新增10项、1171项完整集成通过，2284份输入与隔离副本一致；本批无新原生或Player，控制器仍18/38。追加118份反汇编，方法索引5570；STATISTICS_SOURCE_EVIDENCE.json123方法、STATISTICS_RECORDS_AUDIT.json与analysis/statistics-records-integrated.log为证据。
- 该批只完成记录基础；完整统计manager/control/offnet、真实文件保存/时钟/每日刷新和CommonGameModule生产装配未完成。下一步沿此链恢复ActivityManager/ActivityControl、SevenDay.EnterGameInit，再Main/account/其余20控制器与完整业务/构建验收。


## 2026-10-03 统计管理器/控制器/抽象策略增量

- 新增真实manager/control/expansion/抽象策略，接数据池GetModel/AddModel和UpdateManager注册/延迟移除；19个provider绑定、消息/dirty/回调/存储/释放顺序及失败状态已验证。
- 源证据明确initOver为field16 Action、GameFrameEntry.disposableActions为释放入口；手动AddModel先插入再OnInit，仅应用AutoSyn；table1428为强制转换。初始1183检查后修正类型转换并新增同步回调顺序，最终1184项通过，2288份输入一致。
- STATISTICS_LIFECYCLE_SOURCE_EVIDENCE.json（62方法+3runtime证据+27字段）、STATISTICS_LIFECYCLE_AUDIT.json与statistics-lifecycle-integrated-final.log已归档。无新原生/Player，仍18/38；原方法索引5570，新增泛型证据独立存储。
- 具体OffNetStrategy/StatistUtils/时间锚/每日刷新和CommonGameModule/GameFrameEntry生产组装尚未完成。Probe明确仅供测试；下一步此链→Activity/SevenDay→Main/account/其余20控制器与全业务/存档重启/Player/视听验收。


## 2026-10-03 具体离线统计与原生压缩存档增量

- OffNetStrategy4623/StatistUtils4619已接真实统计owner，恢复时间锚/周期计数/六类消息/每日回调/压缩保存加载/失败和释放顺序。原生WaitUntil等待池就绪，实际UpdateManager触发保存，独立实例文件重启保留统计与时间戳。
- 新增9项，1193项完整检查和15项实际Game视图原生通过，2292份输入逐字节一致。刷新接口另一个参数经元数据确认是可空RefreshDelegate，最终两套检查均基于修正后的签名重跑。独立UnityEditor.Search启动异常单独记录。
- STATISTICS_OFFNET_SOURCE_EVIDENCE.json（45方法/13字段/原字符串与泛型）和STATISTICS_OFFNET_AUDIT.json已归档；补取TimeToRefreshControl23方法，索引5593。原始自动调度器尚未实现，原生测试每日回调明确由夹具触发；平台时间/HTTP仍为显式端点。
- 下一步原版每日刷新调度和统计真实入口宿主，再活动/七日/Main/account/其余20控制器及全部业务/存档重启/Player/视听。18/38生命周期计数不变，没有新Player或全关外完成声明。


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

## 2026-10-04 登录传输、平台域名与密钥解码

原版NetTool保持配置缺失/精确`"0"`默认RD、解析失败归0、首次非空结果缓存和日志异常前后顺序。恢复服务器时间密钥的Base64/UTF8、两次UTF16反转及有符号偏移。LoginTransmitter注册18个协议，恢复请求字段、同步管理器筛选和存档合并；首次提交后第4次更新发送，重复提交不重置倒计时，发送异常/重入保留原版顺序。基类Update修正为原版无参数虚方法，SetUrl保留忽略传入加密标志、读取全局设置的行为。

1772项集成、23项新增定向、8项真实PlayMode全部通过；2883份输入与隔离工程一致。原生测试在游戏时间暂停时按帧推进传输器，经过实际WebRequestManager与Unity HTTP往返本地服务，验证加密合并数据和明文服务器时间GET。真实平台服务、HttpManager更新宿主及HttpNetAcion业务回调仍是待装配边界，没有宣称真实账号登录成功。

首次原生测试误将空对象请求预期为POST，已按原管理器空数据GET分支修正断言；失败日志另存`analysis/login-transport-initial-*`，未为迎合测试改动运行时。最终原生日志含UnityEditor.Search启动索引异常、退出时Curl42；集成日志含Curl35证书错误，均保留原始记录。检查全部通过，不据此宣称外部网络已验证。

证据：`analysis/login-transport-validation.json`、`analysis/login-transport-native-validation.json`、`generated/outgame/LOGIN_TRANSPORT_SOURCE_EVIDENCE.json`和`LOGIN_TRANSPORT_AUDIT.json`。普通方法索引6606，生命周期22/38不变。下一步恢复HttpManager所有权/密钥安装、HttpNetAcion业务响应与ServerTimeSync，再继续真实平台/Main/剩余业务和最终Player验收。
