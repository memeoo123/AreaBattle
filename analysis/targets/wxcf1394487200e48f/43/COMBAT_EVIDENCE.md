# COMBAT_EVIDENCE

Target: wxcf1394487200e48f / 43. Generated: 2026-09-28T04:47:52.309051+00:00.
静态分析；未运行目标代码。这里的派生案例不是原游戏黄金验证，不能宣称完整关内已还原。

## 可实施的普通关内机制

### soldier-stats

[已确认] Soldier.Init(configID,campID,originTowerID): HP=config.hp, Attack=config.attack, Occupy=config.occupy, Reinforce=config.reinforce, Voyage=config.voyage; Speed=1.0f; soldier type=config.shipType. Runtime offsets are Camp24, HP28, Attack32, Occupy36, Reinforce40, Voyage44, Speed48, origin12.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Soldier-100665360.txt:22 (through line 72); generated/wasm/wasmcode.wasm function 7126; body [0x2956c2,0x2957ec).
- generated/tables/SoldierConfig.json

### tower-basic-fields

[已确认] Tower: camp20, grade24, index28, spawnTime32, regenInterval36, IsAutoAddScore40(byte), maxScore44, score48(float), outgoingLineCount52, maxLines56, soldierType60, position68/72/76, collisionRadius80, forwarding-history list84, state88.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665408.txt:3 (through line 269); generated/wasm/wasmcode.wasm function 15520; body [0x725b74,0x725e17).
- generated/combat-disassembly/Tower-100665432.txt:3 (through line 68); generated/wasm/wasmcode.wasm function 15513; body [0x725301,0x7253a3).
- generated/combat-disassembly/Tower-100665416.txt:3 (through line 7); generated/wasm/wasmcode.wasm function 1454; body [0x738f5,0x738fd).

### tower-capacity-grade

[已确认] Ordinary Tower.Init reads maximum score from DispatchConfig ID3 (65), grade thresholds from ID1/ID2 (9/29). Grade=0 when trunc(score)<=9, 1 when 9<trunc(score)<=29, 2 when trunc(score)>29. This also demotes on loss. Max outgoing lines=1/2/3 by grade. Initialization sets collisionRadius=0.1f even though layout CollisionRadius is 0.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665408.txt:95 (through line 148); generated/wasm/wasmcode.wasm function 15520; body [0x725b74,0x725e17).
- generated/combat-disassembly/Tower-100665420.txt:26 (through line 73); generated/wasm/wasmcode.wasm function 15522; body [0x725e68,0x726054).
- generated/combat-disassembly/Tower-100665432.txt:3 (through line 68); generated/wasm/wasmcode.wasm function 15513; body [0x725301,0x7253a3).
- generated/tables/DispatchConfig.json

### tower-damage

[已确认] On soldier arrival to an enemy camp, call ChangeScore(soldier.CampID, -float(soldier.Occupy), false), then Soldier.Clear(true). Attack is used against soldiers, NOT for tower score damage.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665389.txt:372 (through line 408); generated/wasm/wasmcode.wasm function 15514; body [0x7253a5,0x72579a).

### tower-reinforce

[已确认] On arrival to a same-camp non-full tower, ChangeScore(soldier.CampID,+float(soldier.Reinforce),false), then Clear(true). Full test uses trunc(score)>=maxScore. Positive results are clamped to maxScore.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665389.txt:46 (through line 87); generated/wasm/wasmcode.wasm function 15514; body [0x7253a5,0x72579a).
- generated/combat-disassembly/Tower-100665389.txt:372 (through line 408); generated/wasm/wasmcode.wasm function 15514; body [0x7253a5,0x72579a).
- generated/combat-disassembly/Tower-100665425.txt:189 (through line 205); generated/wasm/wasmcode.wasm function 10056; body [0x499cfa,0x499f9e).

### tower-capture

[已确认] If delta!=0, score=f32(score+delta). If score<=0 and preventCapture==false and tower is not Boss: score=-score; ChangeCamp(sourceCamp), including exact-zero capture. Otherwise score<=0 is clamped to0 without capture. Positive branch clamps to[0,maxScore]. Capture branch retains overflow magnitude without a second cap.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665425.txt:31 (through line 34); generated/wasm/wasmcode.wasm function 10056; body [0x499cfa,0x499f9e).
- generated/combat-disassembly/Tower-100665425.txt:139 (through line 205); generated/wasm/wasmcode.wasm function 10056; body [0x499cfa,0x499f9e).
- {"staticMemoryAddress": 3928972, "encodedMetadataUsage": "0x20006bc7", "decodedByvalTypeIndex": 13795, "metadataTypeIndex": 4216, "typeName": "Boss", "metadataTypeOffset": 7795096}

### camp-change

[已确认] ChangeCamp first removes outgoing directions from this tower (incoming directions remain; bidirectional lines lose only this side), sets new camp, refreshes camp-specific visuals; after a real camp change clears state to Normal and refreshes global camp info. Capture/loss voice IDs2005/2006 are player-related and time-gated; Tree clear voice2018.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665375.txt:3 (through line 185); generated/wasm/wasmcode.wasm function 10057; body [0x499fa0,0x49a173).
- generated/combat-disassembly/WayLineControl-100665555.txt:3 (through line 232); generated/wasm/wasmcode.wasm function 6780; body [0x2709f4,0x270c27).
- generated/combat-disassembly/WayLine-100665529.txt:3 (through line 46); generated/wasm/wasmcode.wasm function 3766; body [0x122c55,0x122ca6).
- generated/combat-disassembly/WayLine-100665524.txt:3 (through line 81); generated/wasm/wasmcode.wasm function 6776; body [0x270646,0x270709).

### full-friendly-forward

[已确认] A full same-camp tower leaves score unchanged, decrements Voyage by1. If originTowerID==tower.IndexID OR Voyage<=0, clear soldier. Else obtain all outgoing lines, choose first line not in tower forwarding-history, add it to history and call line.AddSoldier(tower,soldier). If every outgoing line is visited, clear history and repeat enumeration. If no outgoing line, clear history and soldier. Thus round-robin follows list order; history is tower-owned, not soldier-owned.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665389.txt:88 (through line 370); generated/wasm/wasmcode.wasm function 15514; body [0x7253a5,0x72579a).
- generated/combat-disassembly/WayLineControl-100665577.txt:119 (through line 136); generated/wasm/wasmcode.wasm function 6782; body [0x2710ff,0x271361).
- {"tableIndex": 9521, "resolvedMethod": "WayLine.AddSoldier", "resolvedModule": "wasmcode", "resolvedFunction": 14739}

### regeneration

[已确认] When camp!=0, outgoingLineCount==0, IsAutoAddScore and state!=Tree: accumulator+=dt; if accumulator>=regenInterval, delta=1*product(buff.regenMultiplier), accumulator-=regenInterval once. Calls ChangeScore(ownCamp,delta,true). No loop to catch up multiple intervals in one update. Baseline regenInterval=2 seconds. Neutral towers do not regen. Accumulator pauses rather than resets when condition is false.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665404.txt:112 (through line 175); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).
- generated/combat-disassembly/Tower-100665404.txt:207 (through line 376); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).
- generated/combat-disassembly/Tower-100665404.txt:433 (through line 452); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).
- generated/dispatch-model.json

### rain-state

[已确认] Rain state value3: a separate accumulator adds dt and at >=1s subtracts trunc(GameSceneControl field40) from delta, retaining 1s remainder. This shares ChangeScore(...,preventCapture=true); rain cannot transfer ownership.

Scope: Rain state; config binding confirmed by boss-skill-parameters

- generated/combat-disassembly/Tower-100665404.txt:378 (through line 452); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).

### spawn-clock

[已确认] For each enabled direction (state1 small->large; state2 large->small; state3 both), if source.state!=Tree, compute multiplier, add dt to direction timer; spawn only when timer>source.SpawnTime/multiplier; set timer=0 (discard overshoot). Timer does not run while its direction is disabled or source is Tree. At most one batch per direction per update. Source score is not consumed in base GetSoldierEntity/AddSoldier/spawn state machine.

Scope: ordinary Tower virtual slot7 GetSoldierEntity and unmodified base dispatch

- generated/combat-disassembly/WayLine-100665496.txt:3 (through line 102); generated/wasm/wasmcode.wasm function 9363; body [0x455f29,0x455fff).
- generated/combat-disassembly/Tower-100665382.txt:3 (through line 32); generated/wasm/wasmcode.wasm function 15521; body [0x725e18,0x725e66).
- generated/combat-disassembly/WayLine-100665513.txt:3 (through line 158); generated/wasm/wasmcode.wasm function 14739; body [0x6e20a2,0x6e21f1).
- generated/combat-disassembly/Type4233-100665536.txt:3 (through line 509); generated/wasm/wasmcode.wasm function 9885; body [0x485d16,0x4861eb).

### spawn-multiplier

[已确认] Base spawn-speed multiplier1. Rain replaces base with GameSceneControl float field36. Applies camp-specific virtual modifier, then multiplies all buff f64 field8 values converted to f32. The camp modifier is CommanderBase.GetSpawnRate (vtable slot8; base identity), with Commander2Skill and Commander4Skill overrides; buff fields are TowerBuff.spawnRate.

Scope: formula structure confirmed; buff and camp modifier values unresolved

- generated/combat-disassembly/WayLine-100665532.txt:3 (through line 318); generated/wasm/wasmcode.wasm function 6775; body [0x270320,0x270644).

### spawn-batches

[已确认] Scan source buffs for max(trunc(buff field24)) and max(trunc(buff field28)), initially0. Test Random.Next(0,100)<maxField28: if true batch3; else test Random.Next(0,100)<maxField24: if true batch2; else batch1. Async spawn loops batch times: obtain source.GetSoldierEntity via virtual slot7, AddSoldier, await configured0.2s wait. Undead5 gives generated soldier HP100000 and effect418; Comm04_2=6 attaches another EffectID. Base spawn has no score debit.

Scope: batch mechanics; randomness source and modified soldier effect details not fully mapped

- generated/combat-disassembly/WayLine-100665500.txt:3 (through line 398); generated/wasm/wasmcode.wasm function 6772; body [0x26fd72,0x270153).
- generated/combat-disassembly/Raw-4876.txt:3 (through line 42); generated/wasm/wasmcode.wasm function 4876; body [0x199732,0x19979b).
- generated/combat-disassembly/WayLine-100665515.txt:3 (through line 85); generated/wasm/wasmcode.wasm function 9368; body [0x456e73,0x456f26).
- generated/combat-disassembly/Type4233-100665536.txt:112 (through line 264); generated/wasm/wasmcode.wasm function 9885; body [0x485d16,0x4861eb).
- generated/combat-disassembly/WayLine-100665497.txt:39 (through line 48); generated/wasm/wasmcode.wasm function 9367; body [0x456de1,0x456e71).
- generated/combat-disassembly/Raw-1545.txt:3 (through line 20); generated/wasm/wasmcode.wasm function 1545; body [0x75f30,0x75f55).

### line-init-and-directions

[已确认] WayLine holds sorted small/large tower endpoints and separate opposing soldier lists. Init computes normalized straight-line direction with magnitude epsilon1e-5 and stores distance. AddSoldier checks soldier active and source is a currently enabled line direction; otherwise clears soldier. It resets soldier start/end positions and records the destination directional list.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/WayLine-100665495.txt:3 (through line 212); generated/wasm/wasmcode.wasm function 9373; body [0x4574f5,0x4576de).
- generated/combat-disassembly/WayLine-100665513.txt:3 (through line 158); generated/wasm/wasmcode.wasm function 14739; body [0x6e20a2,0x6e21f1).

### update-order

[已确认] Active WayLine.Updata performs spawn timers, then moves all listed soldiers, then tower arrivals, then pair collisions between opposite lists. Arrival processing precedes opposite-unit combat in the same update.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/WayLine-100665534.txt:3 (through line 19); generated/wasm/wasmcode.wasm function 14734; body [0x6e16a6,0x6e16c9).
- generated/combat-disassembly/WayLine-100665501.txt:3 (through line 937); generated/wasm/wasmcode.wasm function 9365; body [0x4563e9,0x456d05).

### movement-frame

[已确认] Only PlayState Running(value6) updates lines. WayLineControl computes scaledDelta=dt*ConfigMgr.gameTimeScale, movementLength=scaledDelta*ConfigMgr.shipTimeScale, movement vector=(0,0,1)*movementLength. Soldier movement along line uses magnitude of this vector times normalized direction and a camp-specific virtual modifier of Soldier.Speed (base1). ConfigMgr defaults resolve to gameTimeScale=1 and shipTimeScale=0.8 from GlobalValueConfig Content1 IDs101/102. Base CommanderBase.GetSoldierSpeed returns input unchanged. Skill overrides may change it.

Scope: base speed0.8 units/second; commander-dependent overrides

- generated/combat-disassembly/WayLineControl-100665573.txt:33 (through line 144); generated/wasm/wasmcode.wasm function 14742; body [0x6e24dc,0x6e282f).
- generated/combat-disassembly/ConfigMgr-100663651.txt:3 (through line 20); generated/wasm/wasmcode1.wasm function 66825; body [0x14334a6,0x14334d8).
- generated/combat-disassembly/ConfigMgr-100663652.txt:3 (through line 20); generated/wasm/wasmcode1.wasm function 66824; body [0x1433473,0x14334a5).
- generated/combat-disassembly/WayLine-100665526.txt:3 (through line 541); generated/wasm/wasmcode.wasm function 9369; body [0x456f28,0x45744f).
- generated/combat-disassembly/Soldier-100665351.txt:3 (through line 103); generated/wasm/wasmcode.wasm function 15868; body [0x747376,0x74746c).

### arrival-boundary

[已确认] Soldier arrives if distance(current,target)<=target.CollisionRadius OR distance(current,legStart)>=distance(legStart,legEnd). Ordinary tower radius0.1f. The latter prevents missing arrival after overshoot. Forwarding resets leg start/end and distance.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Soldier-100665348.txt:3 (through line 81); generated/wasm/wasmcode.wasm function 7122; body [0x295422,0x2954c9).
- generated/combat-disassembly/Soldier-100665337.txt:198 (through line 256); generated/wasm/wasmcode.wasm function 7127; body [0x2957ee,0x295a31).
- generated/combat-disassembly/WayLine-100665501.txt:100 (through line 172); generated/wasm/wasmcode.wasm function 9365; body [0x4563e9,0x456d05).
- generated/combat-disassembly/Tower-100665408.txt:126 (through line 129); generated/wasm/wasmcode.wasm function 15520; body [0x725b74,0x725e17).

### movement-no-position-jitter

[已确认] WayLine movement gate function6771 is only whether either directional soldier list is nonempty. Soldier.InitPosInfo directly copies passed start/end vectors into fields60..68/72..80, calls position setter10313 with the start vector, and computes leg distance. AddSoldier passes tower positions; this path adds no random lateral offset. InitPosInfo subsequent transform changes are model scale/orientation, not logical position jitter.

Scope: normal WayLine.AddSoldier positioning

- generated/combat-disassembly/WayLine-100665516.txt:3 (through line 38); generated/wasm/wasmcode.wasm function 6771; body [0x26fd1b,0x26fd70).
- generated/combat-disassembly/Soldier-100665337.txt:3 (through line 262); generated/wasm/wasmcode.wasm function 7127; body [0x2957ee,0x295a31).
- generated/combat-disassembly/Soldier-100665342.txt:3 (through line 60); generated/wasm/wasmcode.wasm function 10313; body [0x4b5501,0x4b5586).
- generated/combat-disassembly/WayLine-100665513.txt:3 (through line 158); generated/wasm/wasmcode.wasm function 14739; body [0x6e20a2,0x6e21f1).

### soldier-combat

[已确认] Compare opposite-direction soldiers on same WayLine with distinct camps. If distance<=ShipCollideDis, simultaneously subtract enemy Attack from each HP. Each HP<=0 is cleared; surviving HP persists. ShipCollideDis=max(movementLength,0.1f), so collision threshold adapts to frame movement. No separate attack cooldown appears in this path. One opposing pair is handled per first-list soldier per update (inner loop breaks after a hit). Same-camp soldiers pass through.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/WayLine-100665501.txt:578 (through line 823); generated/wasm/wasmcode.wasm function 9365; body [0x4563e9,0x456d05).
- generated/combat-disassembly/WayLineControl-100665573.txt:129 (through line 144); generated/wasm/wasmcode.wasm function 14742; body [0x6e24dc,0x6e282f).

### effects

[已确认] Grade increases play effectID105 and voice2014; decreases still switch grade model but do not play upgrade effect. Grade representation enables only the matching model list index. Enemy score-change hit effectID104 is rate limited to elapsed>0.5 seconds. Player-related tower arrival sound2012; lethal soldier contact sound2013.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/Tower-100665420.txt:3 (through line 209); generated/wasm/wasmcode.wasm function 15522; body [0x725e68,0x726054).
- generated/combat-disassembly/Tower-100665391.txt:3 (through line 72); generated/wasm/wasmcode.wasm function 10053; body [0x499b94,0x499c42).
- generated/combat-disassembly/Tower-100665425.txt:35 (through line 138); generated/wasm/wasmcode.wasm function 10056; body [0x499cfa,0x499f9e).
- generated/combat-disassembly/WayLine-100665501.txt:3 (through line 937); generated/wasm/wasmcode.wasm function 9365; body [0x4563e9,0x456d05).

### absolute-base-speed

[已确认] ConfigMgr initializer token100663684 reads dictionary field116 (dicGlobalValue), ID101.Content1 parsed float to static fields8(gameTimeScale) and4(base scale); ID102.Content1 parsed float to static12(shipTimeScale). Cached values are1 and0.8, respectively. CommanderBase virtual slots8(GetSpawnRate) and9(GetSoldierSpeed) return input unchanged. Hence unmodified baseline unit speed0.8 world units/s; collision threshold=max(dt*1*0.8,0.1).

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/ConfigMgr-100663684.txt:23 (through line 122); generated/wasm/wasmcode.wasm function 12780; body [0x5decf4,0x5df11a).
- generated/combat-disassembly/ConfigMgr-100663651.txt:3 (through line 20); generated/wasm/wasmcode1.wasm function 66825; body [0x14334a6,0x14334d8).
- generated/combat-disassembly/ConfigMgr-100663652.txt:3 (through line 20); generated/wasm/wasmcode1.wasm function 66824; body [0x1433473,0x14334a5).
- generated/combat-disassembly/CommanderBase-100665065.txt:3 (through line 6); generated/wasm/wasmcode.wasm function 7866; body [0x3973cb,0x3973d0).
- generated/combat-disassembly/CommanderBase-100665066.txt:3 (through line 6); generated/wasm/wasmcode.wasm function 7866; body [0x3973cb,0x3973d0).
- generated/tables/GlobalValueConfig.json

### tower-buff-fields

[已确认] TowerBuff constructor assigns spawnRate=f64 offset8, riseRate=f64 offset16, spawnX2=f32 offset24 from integer argument, spawnX3=f32 offset28 from integer argument. Spawn-rate and rise-rate stack multiplicatively; X2/X3 chance thresholds take maximum across buffs, and X3 is tested before X2.

Scope: ordinary towers and unmodified soldiers

- generated/combat-disassembly/TowerBuff-100664234.txt:3 (through line 30); generated/wasm/wasmcode1.wasm function 44849; body [0xf45c11,0xf45c4f).
- generated/combat-disassembly/WayLine-100665500.txt:3 (through line 398); generated/wasm/wasmcode.wasm function 6772; body [0x26fd72,0x270153).
- generated/combat-disassembly/WayLine-100665532.txt:3 (through line 318); generated/wasm/wasmcode.wasm function 6775; body [0x270320,0x270644).
- generated/combat-disassembly/Tower-100665404.txt:3 (through line 499); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).

### arrow-basic

[已确认] ArrowTower denies outgoing lines (get_IsCanAddLine always false). Update runs Tower.Update and then arrow targeting timer. Timer resets0 when elapsed>attackRate[grade], even if no target. SettingConfig defines attackRate=[1.8,1.3,0.8], attackRegion=[0.7,1,1.4], visual regionBaseScale0.44. Target search first requests one enemy soldier, then one enemy tower only when no soldier found, using grade range and minimum distance0.2.

Scope: ArrowTower behavior; ShipID4 routing must be cross-checked in LevelControl factory

- generated/combat-disassembly/Type4211-100665255.txt:3 (through line 6); generated/wasm/wasmcode.wasm function 972; body [0x5ba7d,0x5ba82).
- generated/combat-disassembly/Type4211-100665260.txt:3 (through line 13); generated/wasm/wasmcode.wasm function 20698; body [0x8dac03,0x8dac19).
- generated/combat-disassembly/Type4211-100665259.txt:3 (through line 261); generated/wasm/wasmcode.wasm function 13201; body [0x60a1ff,0x60a4b7).
- generated/combat-disassembly/Type4211-100665252.txt:3 (through line 175); generated/wasm/wasmcode.wasm function 20697; body [0x8da9e9,0x8dac02).
- generated/tables/SettingConfig.json

### arrow-hit

[已确认] Projectile helper receives duration0.2f and an on-completion callback. Soldier hit callback directly calls ClearInSkill (kills regardless of HP), then hides and returns projectile to pool. Tower callback calls target.ChangeScore(source.currentCamp,-1,true): cannot capture a normal tower, only reduce to0. Source camp is read at callback time.

Scope: callbacks and duration arguments confirmed; helper3531/1459 exact engine method names not resolved

- generated/combat-disassembly/Type4207-100665263.txt:138 (through line 159); generated/wasm/wasmcode.wasm function 14057; body [0x69fae9,0x69fc8d).
- generated/combat-disassembly/Type4208-100665265.txt:3 (through line 35); generated/wasm/wasmcode.wasm function 15163; body [0x7069fb,0x706a57).
- generated/combat-disassembly/Type4209-100665267.txt:116 (through line 137); generated/wasm/wasmcode.wasm function 15588; body [0x72aebd,0x72b030).
- generated/combat-disassembly/Type4210-100665269.txt:3 (through line 60); generated/wasm/wasmcode.wasm function 15172; body [0x70751f,0x7075b9).

### skill-config-binding

[已确认] CommanderBase.Upgrade reads CommanderConfig.skills in order; per-skill level from CommanderData.skills[i].level. SkillConfig key=skillID*100+level. Sets skill.cdTime32=float(duration), skillCamp12=commander.camp16, no8=i, args36=float[3]{data1,data2,data3}, targetType24=AllSkillConfig.targetType. SkillBase.Execute starts timer0 and inSkill16=true only if not already active. Updata calls SkillInUpdate1 always; active skills increment timer, invoke SkillInUpdate2(dt), then when timer>=duration reset timer/inSkill, invoke completion callback, and SkillEnd. Duration is active lifetime, not an inferred separate cooldown.

Scope: all commander skills

- generated/combat-disassembly/CommanderBase-100665055.txt:3 (through line 191); generated/wasm/wasmcode.wasm function 8119; body [0x3ab8a9,0x3aba8f).
- generated/combat-disassembly/SkillBase-100665074.txt:3 (through line 19); generated/wasm/wasmcode1.wasm function 48053; body [0x1014442,0x1014467).
- generated/combat-disassembly/SkillBase-100665075.txt:3 (through line 15); generated/wasm/wasmcode1.wasm function 11537; body [0x3f2d85,0x3f2d9e).
- generated/combat-disassembly/SkillBase-100665078.txt:3 (through line 123); generated/wasm/wasmcode.wasm function 10353; body [0x4bc635,0x4bc754).
- generated/tables/CommanderConfig.json
- generated/tables/SkillConfig.json
- generated/tables/AllSkillConfig.json

### commander2-speed

[已确认] Commander2.GetSpawnRate: for queried camp==commander.camp use skills[0] (skill4), otherwise skills[1] (skill5). If selected skill.inSkill, multiply input by args[0]; otherwise identity. Thus skill4 modifies own camp dispatch, skill5 modifies every other camp. Both Execute methods use base active timer semantics.

Scope: Commander2 skills4/5

- generated/combat-disassembly/Type4176-100665126.txt:3 (through line 66); generated/wasm/wasmcode.wasm function 20208; body [0x8a704b,0x8a70e4).
- generated/combat-disassembly/Type4176-100665127.txt:3 (through line 76); generated/wasm/wasmcode.wasm function 20207; body [0x8a6f78,0x8a7049).
- generated/combat-disassembly/Type4173-100665128.txt:3 (through line 61); generated/wasm/wasmcode.wasm function 15168; body [0x707092,0x707132).
- generated/combat-disassembly/Type4174-100665132.txt:3 (through line 61); generated/wasm/wasmcode.wasm function 15156; body [0x705e96,0x705f36).

### commander2-reinforce

[已确认] Skill6 Execute reads selected target from SkillControl.TargetTower for player skillCamp1; when target exists calls its virtual ChangeScore(LevelControl.PlayerCampID,+trunc(args[0]),true), clears target reference, plays voice4501. This adds score and respects target maxScore; it cannot capture. Target eligibility is delegated to SkillControl.

Scope: Commander2 skill6

- generated/combat-disassembly/Type4175-100665134.txt:3 (through line 116); generated/wasm/wasmcode.wasm function 14258; body [0x6b1f2e,0x6b2053).
- generated/combat-disassembly/Type4175-100665135.txt:3 (through line 22); generated/wasm/wasmcode.wasm function 14257; body [0x6b1ef7,0x6b1f2c).

### commander4-speed

[已确认] Commander4.GetSpawnRate and GetSoldierSpeed apply only for queried camp==commander.camp and skills[1] (skill11) active. Spawn rate multiplies args[1]; soldier speed multiplies args[0]. When inactive/othercamp both are identity. Skill11.Execute sets all current player-camp towers to state6 (Comm04_2); SkillEnd resets all current player-camp tower states to0 unconditionally. Execute attaches effect420 to selected existing soldier effect holders using helper9548; -9999 is an effect duration/argument, NOT damage. Helper resolves wasmcode7935 and invokes GameModule.get_Effect, never ChangeScore/HP mutation.

Scope: Commander4 skill11

- generated/combat-disassembly/Type4192-100665158.txt:3 (through line 74); generated/wasm/wasmcode.wasm function 20203; body [0x8a6d07,0x8a6dd3).
- generated/combat-disassembly/Type4192-100665159.txt:3 (through line 45); generated/wasm/wasmcode.wasm function 20204; body [0x8a6dd4,0x8a6e42).
- generated/combat-disassembly/Type4192-100665161.txt:3 (through line 45); generated/wasm/wasmcode.wasm function 20205; body [0x8a6e43,0x8a6eb1).
- generated/combat-disassembly/Type4189-100665184.txt:3 (through line 460); generated/wasm/wasmcode.wasm function 15189; body [0x708844,0x708cd0).
- generated/combat-disassembly/Type4189-100665185.txt:3 (through line 207); generated/wasm/wasmcode.wasm function 15188; body [0x70863e,0x708842).
- generated/combat-disassembly/Raw-7935.txt:3 (through line 113); generated/wasm/wasmcode.wasm function 7935; body [0x39c85e,0x39c971).
- generated/combat-disassembly/EffectID-100664494.txt:3 (through line 30); generated/wasm/wasmcode1.wasm function 64169; body [0x1386d0b,0x1386d61).

### commander4-dot

[已确认] Skill12 active SkillInUpdate2 adds dt to its own damage timer; at >=1 second subtracts1 once and invokes selectedTarget.ChangeScore(skillCamp,-trunc(args[0]),true). No catch-up loop. SkillEnd nulls target and zeroes damage timer. Execute coroutine binds selected target and starts the base skill timer; targeting/visual travel details remain partial.

Scope: Commander4 skill12 damage kernel

- generated/combat-disassembly/Type4191-100665189.txt:3 (through line 68); generated/wasm/wasmcode.wasm function 17913; body [0x79872d,0x7987c4).
- generated/combat-disassembly/Type4191-100665191.txt:3 (through line 16); generated/wasm/wasmcode.wasm function 17914; body [0x7987c5,0x7987e0).
- generated/combat-disassembly/Type4190-100665194.txt:3 (through line 1148); generated/wasm/wasmcode.wasm function 10983; body [0x512694,0x5131f6).

### commander1-freeze

[已确认] Skill1 Execute iterates active non-neutral towers of a different camp, invokes tower virtual effect slot26 with6/cdTime and sets state1(Tree); removes all enemy soldiers via WayLineControl.RemoveAllEnemySoldier. Tree pauses generation and regen. SkillEnd clears state toNormal only on currently active different-camp towers whose current state isTree. Current ownership/state are checked at end, not a stored affected list.

Scope: Commander1 skill1

- generated/combat-disassembly/Type4166-100665106.txt:3 (through line 287); generated/wasm/wasmcode.wasm function 13678; body [0x64ab3d,0x64ae12).
- generated/combat-disassembly/Type4166-100665107.txt:3 (through line 259); generated/wasm/wasmcode.wasm function 13677; body [0x64a8c8,0x64ab3b).
- generated/combat-disassembly/Tower-100665404.txt:3 (through line 499); generated/wasm/wasmcode.wasm function 6993; body [0x289e8a,0x28a361).
- generated/combat-disassembly/WayLine-100665496.txt:3 (through line 102); generated/wasm/wasmcode.wasm function 9363; body [0x455f29,0x455fff).

### commander1-direct-damage

[已确认] Skill3 selected-target strike calls ChangeScore(LevelControl.PlayerCampID,-trunc(args[0]),true), plays effect413 and voice4401. Damage cannot capture. The selected target field is44.

Scope: Commander1 skill3 damage kernel; target selection elsewhere

- generated/combat-disassembly/Type4171-100665124.txt:268 (through line 372); generated/wasm/wasmcode.wasm function 14822; body [0x6e7298,0x6e76ba).

### boss-init-and-soldier

[已确认] Boss.Init reads CampConfig[camp].bossModelId to field256 and bossSoldierId to field184. Camp5..9 map model801..805 and soldier11..15. Boss maxScore44=trunc(initial StarInfo.Score), radius80=0.15f, autoRegen40=false; it retains ordinary grade thresholds and resets timers/state. Boss.GetSoldierEntity initializes a pooled Soldier using field184,camp,index. ChangeScore calls base with preventCapture=false but base explicitly recognizes Boss and clamps score<=0 to0 without ownership change; then publishes score notification.

Scope: Boss runtime; layout factory selection not inferred from isBoss alone

- generated/combat-disassembly/Boss-100665274.txt:111 (through line 329); generated/wasm/wasmcode1.wasm function 68533; body [0x14ad666,0x14ade63).
- generated/combat-disassembly/Boss-100665278.txt:3 (through line 35); generated/wasm/wasmcode1.wasm function 68534; body [0x14ade64,0x14adebf).
- generated/combat-disassembly/Boss-100665286.txt:3 (through line 14); generated/wasm/wasmcode1.wasm function 68536; body [0x14ae045,0x14ae060).
- generated/combat-disassembly/Boss-100665277.txt:3 (through line 81); generated/wasm/wasmcode1.wasm function 31304; body [0xb88ef8,0xb88fc5).
- generated/combat-disassembly/Tower-100665425.txt:3 (through line 272); generated/wasm/wasmcode.wasm function 10056; body [0x499cfa,0x499f9e).
- generated/tables/CampConfig.json

### boss-skill-parameters

[已确认] Boss.InitSkillData(actionID) reads BossConfig[actionID]: fields260/264/268/272=skill1_Hit/duration/first/second; fields276/280/284/288=skill2_Hit/duration/first/second. For initial camp6 only, writes GameSceneControl.rainSpawnMultiplier36=skill1_first and rainDamage40=skill1_Hit. Rain values therefore come from the active boss action config, not global guessed constants.

Scope: Boss config binding

- generated/combat-disassembly/Boss-100665280.txt:3 (through line 122); generated/wasm/wasmcode1.wasm function 31311; body [0xb89220,0xb89359).
- generated/tables/BossConfig.json

### boss-rain-area

[已确认] While Boss field224 rainActive, Boss.Update adds dt to scanTimer248; when scanTimer>0.5 subtracts0.5 once and enumerates active towers. Only current player-camp or neutral towers qualify. If distance(tower transform position, rainCenter fields228..236)<skill1_second(field272), sets state3 Rain. Separately durationTimer220+=dt; when durationTimer>skill1_duration264, ends visual/audio rain, resets timers/active flag and sets EVERY current player-camp or neutral tower stateNormal. Out-of-radius towers are not cleared on each scan; cleanup occurs at the rain end.

Scope: Boss rain update kernel; center and scheduling documented separately

- generated/combat-disassembly/Boss-100665287.txt:54 (through line 562); generated/wasm/wasmcode1.wasm function 68529; body [0x14acdd9,0x14ad46b).

### boss-action-dispatch

[已确认] BossUseSkill(action): first stops any stored running coroutine. action1 starts Type4212 strike, action2 starts Type4214 projectile volley, action5 calls Boss3UseSkill1 (rain), action6 starts Type4215 strike. Values3/4 and values outside1..6 have no branch after stopping the prior coroutine.

Scope: BossUseSkill API; higher-level schedule is separate

- generated/combat-disassembly/Boss-100665284.txt:3 (through line 73); generated/wasm/wasmcode1.wasm function 31312; body [0xb8935b,0xb8940d).
- generated/combat-disassembly/Boss-100665279.txt:3 (through line 28); generated/wasm/wasmcode1.wasm function 31309; body [0xb891b6,0xb891fb).
- generated/combat-disassembly/Boss-100665273.txt:3 (through line 28); generated/wasm/wasmcode1.wasm function 31305; body [0xb88fc6,0xb8900b).
- generated/combat-disassembly/Boss-100665285.txt:3 (through line 28); generated/wasm/wasmcode1.wasm function 31307; body [0xb89070,0xb890b5).

### boss-single-strikes

[已确认] Action1 and6 play cast animation then yield UnityEngine.WaitForSeconds(1). On resume rebuild a list of active current player-camp towers, select one via RandomHelper-backed generic helper2673/10954, and apply ChangeScore(target.currentCamp,-hit,true). Action1 uses skill1_Hit260 and effect311 (position+up*0.3); action6 uses skill2_Hit276 and effect303. Both damage without capture. Selection has no explicit empty-list guard in these functions.

Scope: Boss actions1/6

- generated/combat-disassembly/Type4212-100665291.txt:3 (through line 527); generated/wasm/wasmcode1.wasm function 46042; body [0xf9c28e,0xf9c7f8).
- generated/combat-disassembly/Type4215-100665305.txt:3 (through line 485); generated/wasm/wasmcode1.wasm function 42687; body [0xea741c,0xea7918).
- generated/combat-disassembly/Raw-2673.txt:3 (through line 25); generated/wasm/wasmcode.wasm function 2673; body [0xcc0c1,0xcc0ed).
- generated/combat-disassembly/Raw-10954.txt:3 (through line 99); generated/wasm/wasmcode.wasm function 10954; body [0x50f87b,0x50f95c).
- {"staticMemoryAddress": 3941920, "encodedMetadataUsage": "0x2000bbb7", "metadataTypeIndex": 6854, "typeName": "UnityEngine.WaitForSeconds"}
- {"staticMemoryAddress": 3937580, "encodedMetadataUsage": "0x20009fb1", "metadataTypeIndex": 3391, "typeName": "GameFramework.RandomHelper"}

### boss-rain-center

[已确认] Action5 Boss3UseSkill1 resets scan/duration timers to0, chooses one tower from LevelControl.GetCampAllTower(PlayerCampID) through helper2673/10954, copies its transform position into rainCenter228..236, sets rainActive224=true and enables rain visual. Plays voice3310. The affected region is later evaluated by boss-rain-area.

Scope: Boss action5

- generated/combat-disassembly/Boss-100665275.txt:3 (through line 212); generated/wasm/wasmcode1.wasm function 31313; body [0xb8940f,0xb8963d).
- generated/combat-disassembly/Raw-2673.txt:3 (through line 25); generated/wasm/wasmcode.wasm function 2673; body [0xcc0c1,0xcc0ed).
- generated/combat-disassembly/Raw-10954.txt:3 (through line 99); generated/wasm/wasmcode.wasm function 10954; body [0x50f87b,0x50f95c).

### boss-projectile-volley

[已确认] Action2: cast animation, WaitForSeconds(1), then for integer i=0 while float(i)<skill2_first284: rebuild candidate list of all active non-Boss towers (all camps); select random target; launch pooled projectile from boss fields208..216 to target. Movement helper receives arc-height0.5 and duration=distance/2. Yield WaitForSeconds(skill2_duration280/skill2_first284) between launches. Callback rechecks target current camp: if camp==5, ChangeScore(5,+skill2_Hit276,true), effect404; otherwise ChangeScore(target.currentCamp,-skill2_Hit276,true), effect408. This can heal camp5 towers and harm any other camp, always without capture.

Scope: Boss action2; motion helper exact engine name unresolved

- generated/combat-disassembly/Type4214-100665299.txt:3 (through line 736); generated/wasm/wasmcode1.wasm function 40243; body [0xe16940,0xe170ad).
- generated/combat-disassembly/Type4213-100665296.txt:3 (through line 264); generated/wasm/wasmcode1.wasm function 42662; body [0xea5c31,0xea5ece).
- generated/combat-disassembly/Raw-2673.txt:3 (through line 25); generated/wasm/wasmcode.wasm function 2673; body [0xcc0c1,0xcc0ed).
- generated/combat-disassembly/Raw-10954.txt:3 (through line 99); generated/wasm/wasmcode.wasm function 10954; body [0x50f87b,0x50f95c).

### buff-lifecycle-stubs

[已确认] Tower.InitBuff and BuffControl.Updata bodies are nop; they do not initialize or tick a buff in this build. Existing nonempty buff containers still affect the confirmed generation/regen formulas. No caller for the non-default TowerBuff constructor was identified by direct-call/table-index scan; this alone does not prove all buff containers remain empty, because inlined or generic creation may exist.

Scope: negative static evidence only; does not assert buffs absent at runtime

- generated/combat-disassembly/Tower-100665388.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 913; body [0x58966,0x58969).
- generated/combat-disassembly/BuffControl-100664232.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 1636; body [0x83194,0x83197).
- generated/combat-disassembly/TowerBuff-100664234.txt:3 (through line 30); generated/wasm/wasmcode1.wasm function 44849; body [0xf45c11,0xf45c4f).

### boss-schedule-creation

[已确认] AICampController.InitInfo first creates ordinary AICamp entries from non-neutral non-player CampInfoCfgs; then separately iterates StarInfoCfgs. Only Star.isBoss(byte56) creates/activates BossActionEntity using Star.CampID44 and Star.bossSkillId60, and calls current Boss.InitSkillData(bossSkillId). CampInfoCfgs.BossActionId is not read by this path. AICampController.Updata ticks the stored BossActionEntity only when ActiveUpdate and PlayState.Running6.

Scope: serialized layout initialization; pre-init layout mutation must be considered separately

- generated/combat-disassembly/AICampController-100664055.txt:3 (through line 150); generated/wasm/wasmcode.wasm function 13412; body [0x6314db,0x631631).
- generated/combat-disassembly/AICampController-100664046.txt:3 (through line 254); generated/wasm/wasmcode.wasm function 21040; body [0x90d7be,0x90da22).

### boss-schedule-clock

[已确认] BossActionEntity.Init stores camp16,configID12,active8=true, delay20=BossConfig.DelayTime/1000, countdown24=0. Each Update: if delay>0 subtract dt and RETURN (overshoot is discarded); otherwise countdown-=dt. Only when countdown<0 (strict) add one GetBossActionTime interval and invoke AICampController.BossUseSkill(camp,configID). Thus first action occurs the frame after delay reaches/crosses0; each update emits at most one action and interval overshoot is retained. GetBossActionTime selects ActionTime[index]/1000; missing config/list returns float.MaxValue. Existing config rows have one-element ActionTime arrays.

Scope: BossActionEntity exact timer semantics

- generated/combat-disassembly/BossActionEntity-100664061.txt:3 (through line 69); generated/wasm/wasmcode1.wasm function 31314; body [0xb8963f,0xb896e1).
- generated/combat-disassembly/BossActionEntity-100664063.txt:3 (through line 26); generated/wasm/wasmcode1.wasm function 31315; body [0xb896e2,0xb8971c).
- generated/combat-disassembly/ConfigMgr-100663653.txt:3 (through line 25); generated/wasm/wasmcode1.wasm function 30617; body [0xb2b255,0xb2b287).
- generated/combat-disassembly/ConfigMgr-100663680.txt:3 (through line 65); generated/wasm/wasmcode1.wasm function 30619; body [0xb2b422,0xb2b4b7).
- generated/tables/BossConfig.json

### boss-weighted-action

[已确认] ConfigMgr.GetBossAction(configID,trunc(boss.score),boss.maxScore) evaluates each pair Skill1/Skill2=[minimumHpPercent,weight]. Include pair if currentHp>=minimumHpPercent*(maxHp/100f), add weight to cumulative list, then sample RandomHelper.Random(0,totalWeight) and choose first cumulative>sample. Return skill slot1/2, or0 if none. Weight0 contributes no selectable interval. AICampController maps slots to actions: camp5→1/2, camp7→3/4, camp6→5/6, camp8→7/8,camp9→9/10. It acts on the current active Boss reference LevelControl+60; absent/inactive boss returns.

Scope: Boss weighted action decision

- generated/combat-disassembly/ConfigMgr-100663641.txt:3 (through line 153); generated/wasm/wasmcode1.wasm function 30618; body [0xb2b289,0xb2b420).
- generated/combat-disassembly/ConfigMgr-100663648.txt:3 (through line 156); generated/wasm/wasmcode1.wasm function 14399; body [0x4ce1d2,0x4ce366).
- generated/combat-disassembly/AICampController-100664056.txt:3 (through line 151); generated/wasm/wasmcode1.wasm function 32361; body [0xbe0d65,0xbe0ebc).

### commander1-fireball

[已确认] Skill2 coroutine launches ceil(args[0]) balls (integer i while i<args0) at cdTime/args0 intervals while active. Every ball rebuilds a list of all active towers (all camps), picks one via RandomHelper, then Bullet.Init with arc1.5, end=target position+up*0.2, and duration=distance(origin,UNOFFSET target position)/10+0.3. On hit, compare target CURRENT camp to skillCamp: friendly adds trunc(args[2]), otherwise subtracts trunc(args[1]); ChangeScore uses target.currentCamp and preventCapture=true. It cannot capture. First-level config201 duration5,args[10,2,2].

Scope: Commander1 skill2

- generated/combat-disassembly/Type4169-100665118.txt:3 (through line 305); generated/wasm/wasmcode.wasm function 15173; body [0x7075bb,0x7078b5).
- generated/combat-disassembly/Type4167-100665113.txt:3 (through line 578); generated/wasm/wasmcode.wasm function 14588; body [0x6b4a4a,0x6b5016).
- generated/combat-disassembly/Type4168-100665115.txt:3 (through line 311); generated/wasm/wasmcode.wasm function 15161; body [0x706573,0x706863).
- generated/tables/SkillConfig.json

### commander3-undead

[已确认] Skill8 Execute sets existing same-camp soldiers HP=10000 and effect418; sets current player-camp towers state5 Undead, causing newly generated soldiers HP=100000 via spawn state machine. These are large finite HP values, not an invulnerability flag. SkillEnd sets affected same-camp soldiers HP back to SoldierConfig.hp (full baseline HP), removes effect418, and resets current player-camp tower states toNormal.

Scope: Commander3 skill8

- generated/combat-disassembly/Type4180-100665149.txt:3 (through line 448); generated/wasm/wasmcode.wasm function 14241; body [0x6b00b3,0x6b0524).
- generated/combat-disassembly/Type4180-100665150.txt:3 (through line 473); generated/wasm/wasmcode.wasm function 14240; body [0x6afc0d,0x6b00b1).
- generated/combat-disassembly/Type4233-100665536.txt:3 (through line 509); generated/wasm/wasmcode.wasm function 9885; body [0x485d16,0x4861eb).

### commander5-inspiration

[已确认] Skill13 Execute subscribes Event.SoldierInit (Event static offset60) and activates duration; SkillEnd unsubscribes. Callback only modifies newly initialized same-camp soldiers: effect511; Occupy*=trunc(1+args[0]/100f); Attack+=99. It does not iterate pre-existing soldiers in Execute. Thus localized description claiming all friendly soldiers is broader than this path.

Scope: Commander5 skill13

- generated/combat-disassembly/Type4193-100665199.txt:3 (through line 57); generated/wasm/wasmcode.wasm function 14252; body [0x6b1be9,0x6b1c83).
- generated/combat-disassembly/Type4193-100665200.txt:3 (through line 40); generated/wasm/wasmcode.wasm function 14251; body [0x6b1b73,0x6b1be7).
- generated/combat-disassembly/Type4193-100665201.txt:3 (through line 70); generated/wasm/wasmcode.wasm function 14250; body [0x6b1ac8,0x6b1b72).

### commander6-fatigue

[已确认] Skill16 Execute subscribes Event.SoldierInit and activates duration; SkillEnd unsubscribes. Callback only modifies newly initialized DIFFERENT-camp soldiers: effect611,Attack=0,Reinforce=0. Occupy and HP remain unchanged. It does not retroactively iterate all existing soldiers in Execute.

Scope: Commander6 skill16

- generated/combat-disassembly/Type4200-100665224.txt:3 (through line 57); generated/wasm/wasmcode.wasm function 15178; body [0x707a27,0x707ac1).
- generated/combat-disassembly/Type4200-100665225.txt:3 (through line 39); generated/wasm/wasmcode.wasm function 15176; body [0x707951,0x7079b0).
- generated/combat-disassembly/Type4200-100665227.txt:3 (through line 40); generated/wasm/wasmcode.wasm function 15177; body [0x7079b1,0x707a25).

### commander3-unlinkable

[已确认] Skill7 Execute targets all active non-neutral different-camp towers. Each bat arrives through callback100665147: while Running, if target.state is not4 set Unlinkable4 and remove outgoing directions only. If already4 and now same skill camp, return bat to pool and setNormal; if already4 and still enemy, leave unchanged. SkillEnd returns tracked bat objects and clears ALL active towers currently state4, irrespective of camp.

Scope: Commander3 skill7; incoming lines survive

- generated/combat-disassembly/Type4179-100665140.txt:3 (through line 387); generated/wasm/wasmcode.wasm function 13627; body [0x645362,0x645755).
- generated/combat-disassembly/Type4179-100665142.txt:3 (through line 282); generated/wasm/wasmcode.wasm function 8512; body [0x3f757b,0x3f7861).
- generated/combat-disassembly/Type4178-100665147.txt:3 (through line 66); generated/wasm/wasmcode.wasm function 15207; body [0x70a159,0x70a1f9).
- generated/combat-disassembly/Type4179-100665141.txt:3 (through line 265); generated/wasm/wasmcode.wasm function 13625; body [0x6450bc,0x645348).

### commander3-drain

[已确认] Skill9 picks the player selected friendly target (field48), or random active same-camp tower for AI. For every active non-neutral different-camp source tower, source visual helper subtracts trunc(args0) with preventCapture=true before launching the absorption bullet. Travel duration argument is0.7. On arrival only while Running and selected target still belongs to skill camp, add trunc(args0) to that target (preventCapture=false); otherwise no heal. SkillEnd nulls selected target. The amount healed does not depend on source actual score loss after clamp.

Scope: Commander3 skill9

- generated/combat-disassembly/Type4182-100665152.txt:3 (through line 677); generated/wasm/wasmcode1.wasm function 42680; body [0xea6733,0xea6e35).
- generated/combat-disassembly/Type4182-100665151.txt:3 (through line 366); generated/wasm/wasmcode1.wasm function 20212; body [0x7a8521,0x7a88c2).
- generated/combat-disassembly/Type4182-100665153.txt:3 (through line 83); generated/wasm/wasmcode1.wasm function 42679; body [0xea666a,0xea6731).
- generated/combat-disassembly/Type4182-100665155.txt:3 (through line 10); generated/wasm/wasmcode.wasm function 15184; body [0x7084ef,0x708500).

### commander4-arrow-rain

[已确认] Skill10 runs independent arrow visuals/collision and tower damage coroutines. Tower coroutine waits0.3s, then for integer i starting0 while i<=cdTime and active, applies -trunc(args1) with preventCapture=true to GetOtherTower(skillCamp), then waits1s. Visual coroutine emits two arrows per inner iteration, i<args0*0.5, waiting2/args0 between iterations. Each landed arrow queries different-camp soldiers in radius0.15 (minimum0), and clears only the FIRST returned soldier with ClearInSkill; it does not subtract HP. Visual random landing x=Random(-20,20)/20 and z=Random(-40,40)/15+3; launch height10, fall duration1.

Scope: Commander4 skill10; inclusive integer endpoints confirmed by random-point-distribution

- generated/combat-disassembly/Type4188-100665163.txt:3 (through line 74); generated/wasm/wasmcode.wasm function 15181; body [0x707ebf,0x707f75).
- generated/combat-disassembly/Type4187-100665180.txt:3 (through line 375); generated/wasm/wasmcode.wasm function 15160; body [0x7061e0,0x706571).
- generated/combat-disassembly/Type4186-100665174.txt:3 (through line 253); generated/wasm/wasmcode.wasm function 14253; body [0x6b1c85,0x6b1ecd).
- generated/combat-disassembly/Type4184-100665169.txt:3 (through line 189); generated/wasm/wasmcode.wasm function 18798; body [0x816c86,0x816e8c).
- generated/combat-disassembly/Type4185-100665171.txt:3 (through line 108); generated/wasm/wasmcode.wasm function 14148; body [0x6a8148,0x6a8256).
- generated/combat-disassembly/Soldier-100665335.txt:3 (through line 30); generated/wasm/wasmcode.wasm function 7128; body [0x295a32,0x295a77).

### commander5-recruit

[已确认] Skill14 coroutine emits trunc(args0) special DeadSoldier objects at cdTime/trunc(args0) intervals, first emission immediate. Each chooses a random GetEnemyTower(skillCamp); if none, random neutral tower; no candidates skips that emission. Starts at UnityMathfTool.randomPoint(target.x,target.z,0.3,0.5). Init receives damage1,skillCamp,callbacknull. Each active unit moves normalized fixed direction by WayLineControl.movementLength*0.5 per update; hit when distanceSquared<0.01. On hit applies +1 if current target camp equals skillCamp, otherwise -1, with preventCapture=false, so it CAN capture ordinary towers. These are separate from ordinary WayLine soldier lists.

Scope: Commander5 skill14; discrete ring sampling confirmed by random-point-distribution

- generated/combat-disassembly/Type4194-100665209.txt:3 (through line 447); generated/wasm/wasmcode.wasm function 15182; body [0x707f77,0x7083ef).
- generated/combat-disassembly/Type4198-100665220.txt:3 (through line 263); generated/wasm/wasmcode.wasm function 12623; body [0x5cf58c,0x5cf7f3).
- generated/combat-disassembly/Type4198-100665219.txt:3 (through line 199); generated/wasm/wasmcode.wasm function 20019; body [0x895928,0x895ad2).
- generated/combat-disassembly/Type4198-100665221.txt:3 (through line 19); generated/wasm/wasmcode.wasm function 12622; body [0x5cf55a,0x5cf58a).
- generated/combat-disassembly/Type4195-100665205.txt:3 (through line 212); generated/wasm/wasmcode.wasm function 15204; body [0x709c1d,0x709e28).

### commander5-serial-score

[已确认] Skill15 selects one tower; if target camp equals skill camp, calls ChangeScoreInSerial(skillCamp,+trunc(args0),cdTime/args0,null); otherwise negative amount. Tower serial coroutine stores initial target camp, applies one +1/-1 immediately and then after each WaitForSeconds(gap), integer counter while counter<abs(amount). Every unit change uses preventCapture=true. If target becomes inactive it stops; if target camp changes and callback is null it stops. Skill15 has null callback, clears selected reference after starting sequence, and can never capture by its own damage. Friendly/enemy effect IDs532/531.

Scope: Commander5 skill15 scoring; selected target validation remains in controls

- generated/combat-disassembly/Type4196-100665216.txt:3 (through line 1169); generated/wasm/wasmcode.wasm function 11177; body [0x540529,0x5410b2).
- generated/combat-disassembly/Tower-100665413.txt:3 (through line 89); generated/wasm/wasmcode.wasm function 15523; body [0x726056,0x726116).
- generated/combat-disassembly/Type4224-100665434.txt:3 (through line 520); generated/wasm/wasmcode.wasm function 11298; body [0x54d19f,0x54d680).

### commander6-recovery

[已确认] Skill17 coroutine snapshots active same-camp towers once and attaches effect612. Starting immediately, each integer second i with i<cdTime iterates that stored list: if still same camp, ChangeScore(ownCamp,+args0,false), preserving float amount; if camp changed, remove corresponding effect. WaitForSeconds1 follows each pass. Newly captured friendly towers are not added to the snapshot. SkillEnd cleans remaining effects.

Scope: Commander6 skill17

- generated/combat-disassembly/Type4202-100665228.txt:3 (through line 30); generated/wasm/wasmcode.wasm function 13877; body [0x69d7b2,0x69d7f0).
- generated/combat-disassembly/Type4201-100665234.txt:3 (through line 497); generated/wasm/wasmcode.wasm function 14638; body [0x6dbd24,0x6dc1fb).
- generated/combat-disassembly/Type4202-100665231.txt:3 (through line 62); generated/wasm/wasmcode.wasm function 13876; body [0x69d717,0x69d7b1).

### commander6-poison

[已确认] Skill18 constructor radius56 is f32 bits1060320051 =0.7. While active, damageTimer44,killTimer48,elapsed52 each add dt. damageTimer>=1 subtracts1 once and applies -trunc(args0),preventCapture=true to GetTowerInRange(skillCamp,false,center88..96,radius0.7,min0). Separately only elapsed>1 and killTimer>0.2 resets killTimer0 and clears ALL different-camp soldiers in radius0.7 through ClearInSkill, regardless of HP. No catch-up loops. Drag-end accepts only |x|<2.5 and0.5<z<5 before launching bottle; center is selected ground position. SkillEnd resets timers and stops poison effect/audio.

Scope: Commander6 skill18; precise projectile-to-duration start relation still needs runtime verification

- generated/combat-disassembly/Type4205-100665238.txt:3 (through line 113); generated/wasm/wasmcode.wasm function 9298; body [0x45152f,0x45164b).
- generated/combat-disassembly/Type4205-100665244.txt:3 (through line 558); generated/wasm/wasmcode.wasm function 14641; body [0x6dc395,0x6dc8e7).
- generated/combat-disassembly/Type4205-100665239.txt:3 (through line 234); generated/wasm/wasmcode.wasm function 14645; body [0x6dcf12,0x6dd121).
- generated/combat-disassembly/Type4205-100665240.txt:3 (through line 297); generated/wasm/wasmcode.wasm function 14642; body [0x6dc8e9,0x6dcbb6).
- generated/combat-disassembly/Soldier-100665335.txt:3 (through line 30); generated/wasm/wasmcode.wasm function 7128; body [0x295a32,0x295a77).

### tower-buff-read-only

[已确认] Tower.GetBuff generic table9527 resolves through wasmcode1 f44848 to f20732. That specialization loads tower dictionary offset136, retrieves the requested key via generic dictionary helper, then performs generic cast/type handling. There is no allocation or insertion in this function. Combined with InitBuff nop, this removes GetBuff itself as a buff creation source, while leaving external/inlined creation unresolved.

Scope: generic GetBuff specialization; negative source evidence

- generated/combat-disassembly/Raw-21405.txt:3 (through line 10); generated/wasm/wasmcode.wasm function 21405; body [0x91083c,0x91084c).
- generated/combat-disassembly/Raw-1044848.txt:3 (through line 9); generated/wasm/wasmcode1.wasm function 44848; body [0xf45c03,0xf45c10).
- generated/combat-disassembly/Raw-1020732.txt:3 (through line 37); generated/wasm/wasmcode1.wasm function 20732; body [0x7f55b3,0x7f5610).
- generated/combat-disassembly/Tower-100665388.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 913; body [0x58966,0x58969).

### skill-update-clock

[已确认] SkillControl.Updata only dispatches its current Commander when LevelControl.PlayState==Running6. CommanderControl.Updata is nop. CommanderBase.Updata loops its skills and passes the first dt argument twice to SkillBase.Updata; it does not apply ConfigMgr.gameTimeScale.

Scope: SkillBase duration and damage timers

- generated/combat-disassembly/SkillControl-100665084.txt:3 (through line 46); generated/wasm/wasmcode.wasm function 15934; body [0x74d306,0x74d36f).
- generated/combat-disassembly/CommanderControl-100664247.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 1636; body [0x83194,0x83197).
- generated/combat-disassembly/CommanderBase-100665057.txt:3 (through line 45); generated/wasm/wasmcode.wasm function 20198; body [0x8a69f4,0x8a6a5e).

### bullet-scaled-update

[已确认] Bullet.Update f20382 increments elapsed12 by f1246, whose resolved internal-call name at static-memory1911954 is UnityEngine.Time::get_deltaTime(). Bullet is a MonoBehaviour-driven quadratic Bezier projectile, not a DOTween timer. Bullet.Init duration argument4 goes to60; bool argument8 is stored at84 and controls whether its component is disabled after arrival callbacks. Skill7 passes false so its landed bat callback repeats each Update until pooled/disabled. Skill2 also passes false, but its hit callback returns the object to its pool; skill9 passes true. Arrival tests squared-distance to end<1e-10 after evaluating the curve.

Scope: skills2/7/9 and other Bullet-based effects; scaled Unity time

- generated/combat-disassembly/Bullet-100664285.txt:3 (through line 281); generated/wasm/wasmcode.wasm function 20382; body [0x8c3583,0x8c37d6).
- generated/combat-disassembly/Bullet-100664289.txt:3 (through line 533); generated/wasm/wasmcode.wasm function 4506; body [0x1728ca,0x172d38).
- generated/combat-disassembly/Raw-1246.txt:3 (through line 22); generated/wasm/wasmcode.wasm function 1246; body [0x67652,0x67682).
- generated/combat-disassembly/Type4179-100665142.txt:3 (through line 282); generated/wasm/wasmcode.wasm function 8512; body [0x3f757b,0x3f7861).
- {"staticMemoryAddress": 1911954, "decodedString": "UnityEngine.Time::get_deltaTime()"}

### bullet-control-point

[已确认] Bullet.Init stores start at20..28, control at32..40, end at44..52, duration60 and arc76. Nonzero arc sets control=(start+end)/2+Vector3.up*arc. Zero arc computes normalized Cross(start-end,ProjectOnPlane(start-end,Vector3.up))*0.5 and adds/subtracts it from midpoint by inclusive RandomHelper.Random(1,100) parity. Static Vector3 usage3941368 resolves to metadata type6789; static field24 is upVector, not forwardVector. GetBezierPoint f4700 clamps t to[0,1] and evaluates the quadratic Bezier. Skill9 arc calls f5798 UnityEngine.Random::get_value, distinct from RandomHelper. Skill2 end is tower.position+up*0.2, but duration is Distance(origin,tower.position)/10+0.3 before applying that offset.

Scope: original Bullet geometry for skills2/7/9 and Boss volleys

- generated/combat-disassembly/Bullet-100664289.txt:191 (through line 500); generated/wasm/wasmcode.wasm function 4506; body [0x1728ca,0x172d38).
- generated/combat-disassembly/Raw-4700.txt:3 (through line 87); generated/wasm/wasmcode.wasm function 4700; body [0x18ca3a,0x18cae5).
- generated/combat-disassembly/Raw-5798.txt:3 (through line 22); generated/wasm/wasmcode.wasm function 5798; body [0x1f24a7,0x1f24d7).
- generated/combat-disassembly/Type4182-100665151.txt:3 (through line 366); generated/wasm/wasmcode1.wasm function 20212; body [0x7a8521,0x7a88c2).
- generated/combat-disassembly/Type4167-100665113.txt:3 (through line 578); generated/wasm/wasmcode.wasm function 14588; body [0x6b4a4a,0x6b5016).
- {"staticMemoryAddress": 3941368, "metadataTypeIndex": 6789, "staticFieldOffset": 24, "field": "UnityEngine.Vector3.upVector"}
- {"staticMemoryAddress": 1902483, "decodedString": "UnityEngine.Random::get_value()"}

### random-point-distribution

[已确认] UnityMathfTool.randomPoint(x,z,minimumRadius,maximumRadius) samples angle=RandomHelper.Random(0,100)*0.01*pi*2 and radius=RandomHelper.Random(0,100)*0.01*(maximum-minimum)+minimum; returns x+radius*cos(angle),z+radius*sin(angle). Raw1621 implements RandomHelper.Random(min,max) by calling underlying Random.Next(min,max+1), so integer upper endpoint is inclusive. The two samples are discrete and radial distance is uniform, not uniform area.

Scope: skill14 recruitment ring and helper-driven integer sampling

- generated/combat-disassembly/Raw-9709.txt:3 (through line 65); generated/wasm/wasmcode.wasm function 9709; body [0x46f955,0x46f9e6).
- generated/combat-disassembly/Raw-1621.txt:3 (through line 42); generated/wasm/wasmcode.wasm function 1621; body [0x82a36,0x82a9e).
- generated/combat-disassembly/Raw-1108.txt:3 (through line 173); generated/wasm/wasmcode.wasm function 1108; body [0x61e13,0x61fac).
- generated/combat-disassembly/Raw-1114.txt:3 (through line 173); generated/wasm/wasmcode.wasm function 1114; body [0x62123,0x622b6).

### commander-passive-stat-boundary

[已确认] CommanderBase.Init stores CommanderData at12 and camp16, allocates a3-skill list, invokes subclass skill construction and Upgrade. Upgrade binds each skill config from saved skill levels; no separate hero-level HP/Attack/Occupy/regen passive multiplier is applied in these paths. Base GetSpawnRate/GetSoldierSpeed return the input unchanged. Confirmed active multipliers are Commander2 skills4/5 and Commander4 skill11; SoldierInit callbacks13/16 are separate stat mutations.

Scope: bounded absence of a passive stat multiplier in confirmed battle paths

- generated/combat-disassembly/CommanderBase-100665054.txt:3 (through line 48); generated/wasm/wasmcode.wasm function 6202; body [0x21d4b2,0x21d52e).
- generated/combat-disassembly/CommanderBase-100665055.txt:3 (through line 191); generated/wasm/wasmcode.wasm function 8119; body [0x3ab8a9,0x3aba8f).
- generated/combat-disassembly/CommanderBase-100665065.txt:3 (through line 6); generated/wasm/wasmcode.wasm function 7866; body [0x3973cb,0x3973d0).
- generated/combat-disassembly/CommanderBase-100665066.txt:3 (through line 6); generated/wasm/wasmcode.wasm function 7866; body [0x3973cb,0x3973d0).
- generated/combat-disassembly/Type4176-100665126.txt:3 (through line 66); generated/wasm/wasmcode.wasm function 20208; body [0x8a704b,0x8a70e4).
- generated/combat-disassembly/Type4192-100665158.txt:3 (through line 74); generated/wasm/wasmcode.wasm function 20203; body [0x8a6d07,0x8a6dd3).
- generated/combat-disassembly/Type4192-100665159.txt:3 (through line 45); generated/wasm/wasmcode.wasm function 20204; body [0x8a6dd4,0x8a6e42).

### skills12-18-start-before-visual

[已确认] Skill12 async MoveNext initial state sets inSkill=true and base timer0 before selecting target44; it spawns effect417 at target.position+up*0.7, then awaits0.8 before effect419/audio2032/2033. SkillInUpdate2 reads target44 and accumulates damage from the active start with no visual-ready gate. Skill18.Execute likewise sets active/timer immediately at006dd280..296, copies selected point76..84 to damage center88..96 at006dd297..2c3 (AI then replaces center with player tower position plus discrete jitter), before launching the bottle. Skill18.SkillInUpdate2 accumulates all three damage/kill timers unconditionally and queries that center; it has no bottle-landed gate. Therefore duration and damage start on Execute, not at visual completion.

Scope: mechanical startup timing confirmed; visual asset load/callback frame timing still unverified

- generated/combat-disassembly/Type4190-100665194.txt:93 (through line 105); generated/wasm/wasmcode.wasm function 10983; body [0x512694,0x5131f6).
- generated/combat-disassembly/Type4190-100665194.txt:588 (through line 760); generated/wasm/wasmcode.wasm function 10983; body [0x512694,0x5131f6).
- generated/combat-disassembly/Type4191-100665189.txt:3 (through line 68); generated/wasm/wasmcode.wasm function 17913; body [0x79872d,0x7987c4).
- generated/combat-disassembly/Type4205-100665242.txt:50 (through line 157); generated/wasm/wasmcode.wasm function 14647; body [0x6dd1db,0x6dd58d).
- generated/combat-disassembly/Type4205-100665244.txt:49 (through line 125); generated/wasm/wasmcode.wasm function 14641; body [0x6dc395,0x6dc8e7).

### commander-active-guard

[已确认] CommanderBase function3652 finds a skill by SkillBase.type20 and returns its inSkill16 flag, orfalse if absent. This is active-duration status, not a separate cooldown countdown. Execute8120 finds the same type, stores completion callback40 and invokes virtual Execute. GetSkill6203 indexes list[type-1]. Effective5153 returns true fornull target; otherwise dispatches matching skill virtual target predicate.

Scope: CommanderBase API consumed by SkillControl

- generated/combat-disassembly/CommanderBase-100665061.txt:3 (through line 58); generated/wasm/wasmcode.wasm function 3652; body [0x118493,0x118519).
- generated/combat-disassembly/CommanderBase-100665060.txt:3 (through line 70); generated/wasm/wasmcode.wasm function 8120; body [0x3aba91,0x3abb37).
- generated/combat-disassembly/CommanderBase-100665062.txt:3 (through line 23); generated/wasm/wasmcode.wasm function 6203; body [0x21d52f,0x21d565).
- generated/combat-disassembly/CommanderBase-100665064.txt:3 (through line 78); generated/wasm/wasmcode.wasm function 5153; body [0x1af59c,0x1af64c).

### commander-multiplier-consumer

[已确认] WayLine spawn multiplier f6775 loads SkillControl singleton usage3953560.currentCommander116 and invokes only that Commander virtual GetSpawnRate with tower camp. Soldier.MoveTo f15868 likewise invokes only the same Commander.GetSoldierSpeed. Neither consumer enumerates enemy Commanders. PVPController independently owns enemyCommander16 and does not assign SkillControl116 (special_pvp_all audit). Therefore AI skills4/5/11 may activate but their Commander multiplier overrides are not consumed by these movement/dispatch paths; direct Skill11 player-tower state6 side effects remain.

Scope: player Commander consumers including PVP; enemy passive multiplier absence bounded to audited paths

- generated/combat-disassembly/WayLine-100665532.txt:60 (through line 83); generated/wasm/wasmcode.wasm function 6775; body [0x270320,0x270644).
- generated/combat-disassembly/Soldier-100665351.txt:21 (through line 45); generated/wasm/wasmcode.wasm function 15868; body [0x747376,0x74746c).

### controller-update-registration-order

[已确认] MineGameLogicModule registration occurs PVPController at005516d1, AIControl at005516ee, LevelControl at0055179c, SkillControl at005517f3 and WayLineControl at0055184a. PVP agents use scaled game dt followed by enemyCommander raw dt; player SkillControl raw-dt skill expiration occurs after the tower pass and before line spawn/movement. A player freeze expiring this frame therefore still blocks that frame tower regeneration, then allows line movement/spawn after cleanup.

Scope: controller order from flow-agent static registration audit; Unity async/projectile relative ordering remains unverified

- generated/pvp-evidence.json
- generated/flow-evidence.json
- generated/combat-disassembly/SkillControl-100665084.txt:3 (through line 46); generated/wasm/wasmcode.wasm function 15934; body [0x74d306,0x74d36f).
- generated/combat-disassembly/CommanderBase-100665057.txt:3 (through line 45); generated/wasm/wasmcode.wasm function 20198; body [0x8a69f4,0x8a6a5e).

### towerbuff-no-active-producer-found

[已确认] Tower constructor creates an empty buff dictionary136; Tower.Clear helper removes buffs and clears that dictionary. Tower.InitBuff is nop. BuffControl constructor creates another empty dictionary; its event handler and Updata are nop. A two-module opcode-validated xref scan finds no direct call to TowerBuff parameter constructor f44849, no constant table14444 call site, no constant table15528 BuffEntity getter call site, and no TowerBuff TypeInfo encoded usage in aligned static memory. This supports an unenabled residual feature in the audited ordinary path, not a proof against every possible reflection/inlined producer. Do not invent buff values or block a fresh ordinary no-skill baseline on the mere existence of these fields.

Scope: ordinary first-load buff emptiness; global dynamic reachability remains bounded

- generated/combat-disassembly/Tower-100665380.txt:77 (through line 87); generated/wasm/wasmcode.wasm function 5595; body [0x1de1ac,0x1de2e4).
- generated/combat-disassembly/Tower-100665384.txt:34 (through line 41); generated/wasm/wasmcode.wasm function 6992; body [0x289da0,0x289e88).
- generated/combat-disassembly/Tower-100665388.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 913; body [0x58966,0x58969).
- generated/combat-disassembly/BuffControl-100664230.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 1028; body [0x5df05,0x5df08).
- generated/combat-disassembly/BuffControl-100664232.txt:3 (through line 5); generated/wasm/wasmcode.wasm function 1636; body [0x83194,0x83197).
- generated/combat-disassembly/BuffControl-100664231.txt:3 (through line 32); generated/wasm/wasmcode.wasm function 20395; body [0x8c4665,0x8c46bf).
- generated/combat-lifecycle-xrefs.json

### serial-wait-runner-owner

[已确认] Skill15 Tower.ChangeScoreInSerial awaits a UnityEngine.WaitForSeconds object through table2540->GetAwaiter5995->11853. This allocates SimpleCoroutineAwaiter, dispatches an Action to Unity synchronization context via5015, then closure14594 calls AsyncCoroutineRunner.Instance8244.StartCoroutine(ReturnVoid(...)). ReturnVoid state machine15141 yields the supplied wait, then completes the awaiter. Runner getter creates a GameObject named AsyncCoroutineRunner; Awake20638 sets hideFlags61 and calls Object.DontDestroyOnLoad1111. The wait is owned by that runner, not the Tower or Skill object. Opcode scan finds the only explicit StopAllCoroutines call in ResourcesModule.RemoveAll on its field20 ResourcesMono, a separate GameObject created in Initialize17743.

Scope: skill15 and same GFRunning await helper; scaled wait owner/lifetime

- generated/combat-disassembly/Type4224-100665434.txt:3 (through line 520); generated/wasm/wasmcode.wasm function 11298; body [0x54d19f,0x54d680).
- generated/combat-disassembly/Raw-5995.txt:3 (through line 8); generated/wasm/wasmcode.wasm function 5995; body [0x20364f,0x203659).
- generated/combat-disassembly/Raw-11853.txt:3 (through line 48); generated/wasm/wasmcode.wasm function 11853; body [0x58a0d9,0x58a160).
- generated/combat-disassembly/Raw-5015.txt:3 (through line 99); generated/wasm/wasmcode.wasm function 5015; body [0x1a4fee,0x1a50fb).
- generated/combat-disassembly/Raw-14594.txt:3 (through line 15); generated/wasm/wasmcode.wasm function 14594; body [0x6b5264,0x6b5280).
- generated/combat-disassembly/Raw-15141.txt:3 (through line 35); generated/wasm/wasmcode.wasm function 15141; body [0x704dbd,0x704e03).
- generated/combat-disassembly/Raw-8244.txt:3 (through line 64); generated/wasm/wasmcode.wasm function 8244; body [0x3bc056,0x3bc113).
- generated/combat-disassembly/Raw-20638.txt:3 (through line 30); generated/wasm/wasmcode.wasm function 20638; body [0x8d1abb,0x8d1b04).
- generated/combat-disassembly/Raw-1023540.txt:3 (through line 61); generated/wasm/wasmcode1.wasm function 23540; body [0x8bbe09,0x8bbec2).
- generated/combat-disassembly/Raw-17743.txt:3 (through line 234); generated/wasm/wasmcode.wasm function 17743; body [0x77fa57,0x77fcee).
- generated/combat-lifecycle-xrefs.json
- {"staticMemoryAddress": 3941920, "type": "UnityEngine.WaitForSeconds"}
- {"staticMemoryAddress": 4045044, "literal": "AsyncCoroutineRunner"}
- {"staticMemoryAddress": 4067944, "literal": "ResourcesMono"}

### skill15-restart-conditional-survival

[已确认] CommanderBase.Reset calls each Skill.Reset then SkillEnd; Skill15 has no Reset override. Tower.Clear calls helper6992 which sets Active8=false and clears visuals/buffs but has no cancellation-token or serial-task handle. Serial MoveNext holds the Tower object, initial camp and remaining loop count: after each wait it stops if inactive; if active and same initial camp it applies the next signed1; if camp differs and callbacknull (Skill15), it stops. Thus a restarted/reused Tower observed active with the same initial camp at continuation time can receive remaining old serial ticks. If the continuation observes the inactive interval or a different camp, it terminates. No explicit serial cancellation on Reset is evidenced. Reconstruction now preserves only the independently proven Skill15 waits, retaining delay/count and the original Tower reference across Restart. It checks Active/current camp at continuation time; actual original replay resource-loading timing still requires observation.

Scope: conditional source behavior; exact replay/reuse timing not measured

- generated/combat-disassembly/CommanderBase-100665058.txt:3 (through line 79); generated/wasm/wasmcode.wasm function 20199; body [0x8a6a60,0x8a6b20).
- generated/combat-disassembly/Type4196-100665216.txt:3 (through line 1169); generated/wasm/wasmcode.wasm function 11177; body [0x540529,0x5410b2).
- generated/combat-disassembly/Tower-100665402.txt:3 (through line 48); generated/wasm/wasmcode.wasm function 10055; body [0x499c84,0x499cf8).
- generated/combat-disassembly/Tower-100665384.txt:42 (through line 53); generated/wasm/wasmcode.wasm function 6992; body [0x289da0,0x289e88).
- generated/combat-disassembly/Type4224-100665434.txt:189 (through line 274); generated/wasm/wasmcode.wasm function 11298; body [0x54d19f,0x54d680).

### soldier-skin-stat-separation

[已确认] Soldier.Init obtains combat fields only from SoldierConfig and then emits SoldierInit event. GetSoldierObj for shipType<=9 chooses player equipped or enemy random skin through PlayerControl, loads the prefab, invokes InitGameObject and recolors it. shipType>9 uses SoldierConfig.EntityID. InitGameObject/config helper sets scale, animation component, shadow and active state; it does not overwrite HP/Attack/Occupy/Reinforce/Voyage. SkinConfig IDs104/204/304 map to visual prefab1004/2004/3004; their04 labels do not establish an extra combat type. SoldierConfig has only IDs1,2,3,11..15, while EntityModelConfig soldier_400..427 have no matching SkinConfig type4 or SoldierConfig4.

Scope: configured soldier variants and visual-only skin selection

- generated/combat-disassembly/Soldier-100665360.txt:3 (through line 113); generated/wasm/wasmcode.wasm function 7126; body [0x2956c2,0x2957ec).
- generated/combat-disassembly/Soldier-100665336.txt:3 (through line 195); generated/wasm/wasmcode.wasm function 10317; body [0x4b5770,0x4b5965).
- generated/combat-disassembly/Soldier-100665349.txt:3 (through line 127); generated/wasm/wasmcode.wasm function 15869; body [0x74746e,0x7475a5).
- generated/combat-disassembly/Soldier-100665355.txt:3 (through line 149); generated/wasm/wasmcode.wasm function 10312; body [0x4b5380,0x4b54ff).
- generated/tables/SoldierConfig.json
- generated/tables/SkinConfig.json
- generated/tables/EntityModelConfig.json

## 修正记录

ChangeCamp先前“移除全部关联线”的表述不准确：只撤去本塔出兵方向，入线保留，双向线降为单向入线。证据见 camp-change 条目及 JSON corrections。Vector3静态field24也已由forward修正为up；技能2的autoDisable为false，但命中回调会回池。

## 枚举实际值

{
  "SoldierType": {
    "None": 0,
    "Normal": 1,
    "Defense": 2,
    "Attack": 3,
    "Arrow": 4
  },
  "TowerState": {
    "Normal": 0,
    "Tree": 1,
    "Rain": 3,
    "Unlinkable": 4,
    "Undead": 5,
    "Comm04_2": 6
  },
  "LineState": {
    "None": 0,
    "SmallToLarge": 1,
    "LargeToSmall": 2,
    "Both": 3
  },
  "PlayStateEnum": {
    "None": 0,
    "First": 1,
    "StartUI": 2,
    "Enter": 3,
    "BeforePlay": 4,
    "StartPlay": 5,
    "Running": 6,
    "Pause": 7,
    "Victory": 8,
    "Defeat": 9,
    "Again": 10,
    "Exit": 11,
    "Jump": 12,
    "EnterSmallLevel1": 13,
    "EnterSmallLevel2": 14,
    "Relive": 15
  }
}

## 派生案例（尚未原游戏验证）

- L0-neutral-capture: {"pre": {"tower": 0, "camp": 0, "score": 3, "shipConfigID": 1, "incomingCamp": 1, "incomingCount": 3}, "expected": {"afterEachScore": [2, 1, 0], "afterEachCamp": [0, 0, 1]}}
- L0-source-grade: {"pre": {"level": 0, "tower": 3, "score": 10}, "expected": {"grade": 1, "maxOutgoingLines": 2, "oneLineBaseSpawnTime": 1.032, "twoLineBaseSpawnTime": 1.185}}
- capture-overflow: {"pre": {"camp": 0, "score": 1, "incomingCamp": 1, "incomingShip": 3, "occupy": 2}, "expected": {"camp": 1, "score": 1}}
- friendly-cap: {"pre": {"camp": 1, "score": 64, "incomingCamp": 1, "incomingShip": 2, "reinforce": 2}, "expected": {"score": 65, "soldierConsumed": true}}
- equal-hp-contact: {"pre": {"A": {"hp": 1, "attack": 1, "camp": 1}, "B": {"hp": 1, "attack": 1, "camp": 2}, "withinThreshold": true}, "expected": {"hpA": 0, "hpB": 0, "bothClear": true}}
- tank-contact: {"pre": {"A": {"hp": 2, "attack": 2, "camp": 1}, "B": {"hp": 1, "attack": 1, "camp": 2}, "withinThreshold": true}, "expected": {"hpA": 1, "hpB": -1, "clearB": true}}
- grade-edges: {"pre": {"scores": [9, 9.9, 10, 29, 29.9, 30, 65]}, "expected": {"grades": [0, 0, 1, 1, 1, 2, 2]}}
- base-regen: {"pre": {"camp": 1, "score": 9, "outgoing": 0, "regenAccumulator": 0, "deltaTime": 2, "buffs": [], "state": 0}, "expected": {"score": 10, "grade": 1, "regenAccumulator": 0}}
- spawn-no-catchup: {"pre": {"baseInterval": 1.032, "timer": 1.02, "deltaTime": 0.03, "multiplier": 1}, "expected": {"batchCount": 1, "timer": 0, "sourceScoreDebit": 0}}
- forward-budget: {"pre": {"fullFriendly": true, "voyage": 1, "originIsCurrent": false}, "expected": {"voyage": 0, "cleared": true, "forwarded": false}}
- boss-zero-no-capture: {"pre": {"isBossRuntime": true, "initialScore": 150, "score": 1, "camp": 6, "incomingCamp": 1, "delta": -2}, "expected": {"score": 0, "camp": 6, "maxScore": 150, "autoRegen": false, "radius": 0.15}}
- skill4-level1: {"pre": {"skillConfigID": 401, "sameCamp": true, "active": true, "inputSpawnRate": 1}, "expected": {"spawnRate": 1.8, "duration": 10}}
- skill5-level1: {"pre": {"skillConfigID": 501, "sameCamp": false, "active": true, "inputSpawnRate": 1}, "expected": {"spawnRate": 0.7, "duration": 10}}
- skill11-level1: {"pre": {"skillConfigID": 1101, "sameCamp": true, "active": true, "baseWorldSpeed": 0.8}, "expected": {"spawnRateMultiplier": 1.8, "soldierSpeedMultiplier": 1.8, "worldSpeedApprox": 1.44, "duration": 10}}
- skill3-cannot-capture: {"pre": {"skillConfigID": 301, "targetCamp": 2, "score": 10, "playerCamp": 1}, "expected": {"score": 0, "camp": 2, "damage": 20}}
- skill12-no-catchup: {"pre": {"skillConfigID": 1201, "damageTimer": 0, "dt": 2.5, "score": 20, "camp": 2}, "expected": {"score": 15, "damageTimer": 1.5, "camp": 2}}
- skill9-clamp-not-conservation: {"pre": {"skillConfigID": 901, "enemySourceScore": 2, "friendlyTargetScore": 10, "targetStillFriendlyOnArrival": true}, "expected": {"enemySourceScore": 0, "enemyCampUnchanged": true, "friendlyTargetScore": 15}}
- skill13-new-only: {"pre": {"skillConfigID": 1301, "newSoldier": {"occupy": 1, "attack": 1}, "existingSoldier": {"occupy": 1, "attack": 1}}, "expected": {"newSoldier": {"occupy": 2, "attack": 100}, "existingSoldier": {"occupy": 1, "attack": 1}}}
- skill14-recruit-capture: {"pre": {"skillConfigID": 1401, "emittedRecruitDamage": 1, "targetCamp": 2, "targetScore": 1, "skillCamp": 1, "hitDistanceSquared": 0.005}, "expected": {"targetScore": 0, "targetCamp": 1, "spawnCount": 25, "spawnInterval": 0.2}}
- skill15-does-not-capture: {"pre": {"skillConfigID": 1501, "targetCamp": 2, "targetScore": 2, "skillCamp": 1, "ticks": 3}, "expected": {"scores": [1, 0, 0], "camp": 2, "totalUnitTicks": 30, "gapApprox": 0.0566666667}}
- skill16-occupy-not-attack: {"pre": {"skillConfigID": 1601, "newEnemy": {"attack": 2, "reinforce": 2, "occupy": 1, "hp": 2}}, "expected": {"attack": 0, "reinforce": 0, "occupy": 1, "hp": 2}}
- skill17-snapshot: {"pre": {"skillConfigID": 1701, "initialFriendlyTowerIds": [1], "newlyCapturedTowerId": 2, "score1": 10, "score2": 10, "healingPasses": 1}, "expected": {"score1": 13, "score2": 10}}
- skill18-damage-no-catchup: {"pre": {"skillConfigID": 1801, "damageTimer": 0, "elapsed": 0, "killTimer": 0, "dt": 2.5, "inRangeEnemyScore": 10, "inRangeEnemySoldierHp": 100000}, "expected": {"enemyScore": 8, "damageTimer": 1.5, "killTimer": 0, "soldierCleared": true}}
- boss-delay-overshoot: {"pre": {"delay": 0.1, "countdown": 0, "frameDeltas": [0.2, 0.01], "interval": 17}, "expected": {"actionsPerFrame": [0, 1], "finalCountdownApprox": 16.99}}

## 待确认项

- [待确认] 原游戏运行时数值与时序验证。最小下一步：在原游戏代表关卡记录连线、三次命中占领、升级/降级、对冲和满塔转发；与静态派生测试逐项比对。
- [待确认] 技能12/18表现资源加载与异步回调实际帧时序。最小下一步：数值核已确认在Execute立即起算，不能等待表现落地再计时；资源延迟与画面表现需原游戏代表配置对照。
- [待确认] TowerBuff是否存在未发现的反射或内联创建来源。最小下一步：普通初始化/控制器更新均无producer，2模块xref未找到构造调用或TypeInfousage；当前空buff普通基线可验收。仅在真实save/runtime出现buff时重开调查。
- [待确认] Boss空候选目标行为、特殊模式加载是否改写Star.isBoss/bossSkillId；Bastion是否独立机制。最小下一步：BossActionEntity和调度已闭环；普通加载未发现字段patch。需审计特殊模式入口。仅2份布局有isBoss=true，不可把CampInfo.BossActionId配置均当已启用；CampConfig.BasitionMaterial仅证明材质字段。
- [待确认] 异步跨Restart复用原塔的实际恢复帧时序。最小下一步：table2540/2542已闭合为Unity WaitForSeconds、常驻AsyncCoroutineRunner；skill15条件存活已证实，需观察旧continuation是否在inactive窗口触发。
- [待确认] grade模型、动画时长、命中特效资产完整绑定及UI精确位置。最小下一步：接合资源分析产物与运行截图；本子任务只交付机制与效果调用证据。

## 重现

```powershell
python analysis/combat_disassemble.py
python analysis/combat_extra_map.py
python analysis/combat_evidence.py
```

所有函数与元数据读取均为本地静态解析。原始包不变。共享 manifest、总报告、总状态未修改。
