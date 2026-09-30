# 特殊关身份与关内输入边界审计

所有结论限定 wxcf1394487200e48f/43。完整关内与表现目标尚未完成。

## special-skill-unlock-uses-current-layout

SkillItem.Init compares LevelControl.CurLevel >= AllSkillConfig.unLockLevel. In SpecialLevel!=0 CurLevel is direct layout field16, not saved normal progress. Preserve this original behavior.

证据：wasmcode.wasm:0x4bc0c5-0x4bc0dd; wasmcode.wasm:0x67c06-0x67c21

## special-guide-no-blanket-disable

InitGameByLevelConfig writes isGuide52=ConfigMgr.IsGuideLv(CurLevel); StartPlay checks only isGuide52 before EnterNextStage. IsGuideLv uses direct LevelConfig identity and LevelType index. Boss layouts99998/99999 have no row and do not trigger guide; a daily layout whose ID matches a guide row may do so.

证据：wasmcode.wasm:0x37a6e2-0x37a6f5; wasmcode.wasm:0x37a2fb-0x37a311; wasmcode.wasm:0x3a948e-0x3a9580

## special-normal-progress-suppressed

Normal result opening requests CurLevel+1, but CurLevel setter does nothing whenever SpecialLevel!=0. A direct special entry must mark BattleProgress.Special and clear stale RequestedNormalLevel so victory cannot overwrite normal progression.

证据：wasmcode.wasm:0x1ffedb-0x1fff34; wasmcode.wasm:0x7a3768-0x7a3783; UnityProject/Assets/AreaBattle/Scripts/BattleView.cs:InitializeSpecialScene

## pvp-fireball-player-ui-origin

Skill2 coroutine has no skillCamp branch for launch origin. It reads SkillControl.arr_skillItem92 array element+20 (second item), its transform+16, projects position via UI camera WorldToScreenPoint, then gameplay camera ScreenToWorldPoint with depth5. Enemy Skill2 uses the same visible player second-slot projection. Host can bind PvPFireballOrigin to SkillProjectileOrigin(1).

证据：wasmcode.wasm:0x707679-0x707790; analysis/special_fireball_move.txt

## layout5-ordinary-gate

Layout5 has six ShipID1 towers, no Boss flag and no obstacles. LevelConfig5.SceneId5, LevelType[0,0,0]. Commander1 skills1/2/3 unlock at6/14/21. Normal level5 without active skills is a source-supported ordinary representative; it does not prove every soldier type, arrows, skills, Boss, daily or PVP.

证据：generated/all-levels/level_5.json; generated/tables/LevelConfig.json:id5; generated/tables/AllSkillConfig.json:ids1/2/3

## layout5-idle-stability

Initial idle1800.58s remainedRunning: Camp3 owns four towers and player two, all65. Camp3 AI1403 has AIType0 which excludes player targets from Occupy/Defend; Camp2 AI1503 type1 had been eliminated. Idle defeat must not be required. Legal player input first eliminates Camp3 and then cuts own outflow; remaining Camp2 naturally defeats player.

证据：analysis/representative-idle1800-first-observation.json; generated/tables/AIConfig.json:1403/1503; generated/flow-evidence.json:AI-reinforce-and-occupy; analysis/representative-replay-validation.json

## daily-pools-bounded-reachability

All recovered mapped Assembly-CSharp direct call/table-constant scan found no callers of DailyChallengeLevelPool constructors66164/66165. Constructors allocate empty list or accept supplied list. Daily RandomLevels consumes saved pools but supplies no refill. This does not prove absence of reflection/deserialization or external initialization.

证据：analysis/special_daily_callers.py; analysis/special_daily_callers.json; generated/pvp-evidence.json:daily-map-consumption

## loadout-stock-consumption

Accepted nonfree cast consumes specific gameItem=2000+skill first, then generic1005. Input reads LocalDataManager stock. The original item acquisition and purchase/award paths are distinct upstream sources; no requirement to implement a store follows from implementing this in-level consumer.

证据：generated/skill-input-evidence.json:stock-precedence; wasmcode.wasm:0x1e68d7-0x1e6924; wasmcode.wasm:0x74db39-0x74db77

## 最小关内 loadout 契约

入口模式、普通存档进度、直接布局ID分别保存；Commander ID与三个独立技能等级；专属技能道具字典与通用1005库存；成功消耗后的本地保存回调。特殊技能解锁仍按原CurLevel，不能人为改成普通进度。

目前固定3道具/全部技能等级1只覆盖可控fixture。下一关或重启程序不应悄悄补充已消耗库存。商店、广告和账号奖励可以作为上游输入来源，不必据此添加未恢复的商业流程。

## 代表关验证

生产BattleView.InitializeScene(5)物理拓扑和AdvanceFrame适配器：自然胜利24.65秒；合法先灭Camp3、随后停止防守，自然失败92.55秒；暂停/切线/重试原始状态均通过。实际时间和输入事件以analysis/representative-replay-trace.json为准。该证据不是原版同步回放或视觉比对。
