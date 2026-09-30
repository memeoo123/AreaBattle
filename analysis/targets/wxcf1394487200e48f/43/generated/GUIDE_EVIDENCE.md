# First-three tutorial evidence

First three zero-based levels: 0,1,2; their first small-level branch

All conclusions are static. Original functions and offsets are embedded in guide-evidence.json.

## guide-entry

ConfigMgr.IsGuideLv reads LevelConfig.LevelType[min(SmallLevelIndex,Length-1)] == 1. Levels 0,1,2 have [1,0,0]. GetGuideId finds GuideConfig guildLv==CurLevel AND guildsmallLv==SmallLevelIndex; initial stages are 1,4,5 respectively. EnterNextStage advances by nextGuildStage, maps -1/missing to13, and closes GuideUI/restores playState6 if no config or stage13. It does not check whether user has previously acknowledged these first-three prompts.

Evidence: f8085 ConfigMgr.IsGuideLv, f12792 ConfigMgr.GetGuideId, f4406 GuideControl.EnterNextStage

## guide-popup

ShowUI pauses running battle by playState6->7, shows original title/explain/picture/button from config, sets one-second OK countdown and disables the button. Completion restores original button label and interaction. OnOK hides popup, sets playState6, emits GuideMessage+4 then dispatches stage action. For existing GuideUI, empty title auto-invokes OnOK; stage2 therefore skips the visible prompt. No extra invented skip button or persisted unlock condition.

Evidence: f7740 GuideUI.ShowUI, f19280 GuideUI.^wmU]Q, f11888 GuideUI.l~LU, f4406 GuideControl.EnterNextStage

## stage1-select

On stage1 OK, take first tower from LevelControl.GetCampAllTower(playerCamp). Retrieve its complete potential adjacency, collect opposite endpoints in adjacency order, choose endpoint with greatest GameObject transform world Z (initial threshold -100; replace only strict greater). For every collected neighbor set its tower GameObject and tower-UI GameObject active iff that neighbor is selected endpoint. This does not mutate Tower.IsActive or remove candidates from the simulation. Other non-neighbor towers are untouched.

Evidence: f11897 GuideControl.GetStartAndEndInConnect

## stage1-input-and-progress

LineAdd event hides hand and sets WayLineControl.CanDraw=false only during stage1 or stage5. Stage1 does NOT advance on connection. GuideUI listens to Event.StarOccupy (static offset28); when current stage1 it schedules GuideCanDrawLine. That delay is float32 0.699999988079071; callback first enables CanDraw then calls EnterNextStage. Callback does not check captured tower identity or current stage again.

Evidence: f19754 Event..cctor, f7739 GuideUI.pkm{l, f19274 GuideUI.xUsvVw, f19282 GuideUI.]~]xjz, f6007 GuideControl.GuideCanDrawLine, f19294 GuideControl.ny]vld

## stage2-cut

Stage2 uses the previous source/target guide positions, rotates both around their midpoint by -90 degrees about Vector3.forward (0,0,1) to display a cross-line swipe. A LineCut event while stage2 immediately sets CanDraw=false and sends SetGuideActive(false), reactivates all stored neighbor tower GameObjects and tower-UI GameObjects, then schedules the same0.7s unlock+next-stage transition. No endpoint identity test exists inside OnLineCutEvent.

Evidence: f11887 GuideUI.ZWnoN, f19754 Event..cctor, f19273 GuideUI.zcT]|d, f11895 GuideControl.OnLineCutEvent

## stage3-and4-exit

Stage3 victory-objective popup and stage4 camp popup OK both invoke inherited UI close virtual slot15. Dispose kills hand tween, removes timers/listeners, and ClearState sets config/source/target=null, stage0, clears stored neighbor and candidate-line lists, and zeroes stored positions. They do not require another connection or victory to dismiss their prompt.

Evidence: f11887 GuideUI.ZWnoN, f19288 GuideUI.Dispose, f11899 GuideControl.ClearState

## stage5-disable-growth

On stage5 OK, SetTowerUpgradeStage writes Tower.IsAutoAddScore(+40)=false for every active tower. Growth remains disabled after target reaches max: OnStarMaxScore only restores GameObjects and schedules the next stage; ClearState also never writes this flag. Do not automatically re-enable regeneration at stage6.

Evidence: f11893 GuideControl.SetTowerUpgradeStage, f11894 GuideControl.OnStarMaxScore, f11899 GuideControl.ClearState

## stage5-select

GetStartAndEndInUpgrade starts with first player tower then replaces source for every other player tower whose trunc(score) >= trunc(currentSource.score), so maximum with ties choosing last. It appends opposite endpoints of source adjacency to endGuideList, chooses endGuideList[0] as target (nearest from confirmed adjacency length sort), then appends opposite endpoints of target adjacency to same list. For each entry, tower and UI GameObjects remain active iff entry is source or target; all others in this list are deactivated. No camp filter or dedup is applied to that collected list.

Evidence: f11896 GuideControl.GetStartAndEndInUpgrade

## stage5-input-and-completion

Existing controls touch-down special gate: if CurLevel==2 AND guideStage==5 AND trunc(source.score)<source.maxScore, clear selected source. Ordinary capacity/camp/physics checks still apply. Successful LineAdd disables input/hides hand as in stage1. AddLoopTime registers TimeModule.AddLoopTimer(1,OnStarMaxScore,true,false); poll checks trunc(tower_end.score)>=tower_end.maxScore, independent of event payload/currentStage. On success restore all collected GameObjects, schedule0.7s unlock+EnterNextStage and remove that loop timer. There is no forced score assignment in guide code.

Evidence: f19274 GuideUI.xUsvVw, f19278 GuideUI.Yum[nOc, f19286 GuideUI.MYulqp, f11894 GuideControl.OnStarMaxScore, f10103 GuideCoreProbe.function10103, f7025 GuideCoreProbe.function7025

## stage6-free-battle

At stage6 enter, GuideLogicControl creates Guide_TowerMax, configured text key GuideUI.tipAdd.max=全军出击. Stage6 OK hides popup, restores playState6 and emits GuideMessage+4 so TextGuideLogic shows this tip; stage action switch maps stage6 to its no-op exit. It does NOT immediately close GuideUI, clear stage or require another connection. Input was unlocked by delayed callback before the stage6 popup; normal modal pause applies until OK. Keep guideStage6 until normal GuideUI lifecycle disposal.

Evidence: f11891 GuideLogicControl.CreateLogic, f11880 Guide_TowerMax..ctor, f15651 TextGuideLogic.Start, f15649 TextGuideLogic.YXuaNX, f11888 GuideUI.l~LU, f11887 GuideUI.ZWnoN

## guide-hand-only

GuideUI hand tween places Transform at first guide point and calls DOMove(second,2.0,false), repeating2147483647 times. Guide functions contain no WayLine.SetLineState/AddActiveLine call for these stages. The hand is a visual instruction, not automated player input. Potential adjacency construction is not an active line.

Evidence: f6006 GuideUI.Qqjki

## guide-timer-kernel

Resolved AddLoopTimer overload stores LoopSteepTime int1 at timer+12, UseTimeScale=false at+22, enables AutoPlay. Timer AddTimes accumulates deltaTime or unscaledDeltaTime according to+22; unscaled delta>1 is ignored. Check fires once at accumulator>=float(LoopSteepTime), clears accumulator to0 (does not carry remainder) then calls callback with step int. The guide poll therefore uses unscaled1.0 timer units and no catch-up loop.

Evidence: f10103 GuideCoreProbe.function10103, f7025 GuideCoreProbe.function7025, f14988 GuideCoreProbe.function14988, f14987 GuideCoreProbe.function14987

## guide-delay-scaled-and-adjacency-alias

TimeHelper.Delay async MoveNext f9901 allocates UnityEngine.WaitForSeconds(duration), proving scaled time. GuideControl stores GetStarAllLine result directly at+28 without copying; ClearState calls List.Clear on that same object. Thus stage3 disposal clears source adjacency; stage6 disposal clears target adjacency, while WayLine entities remain in the global list.

Evidence: f9901 GuideCoreProbe.function9901, f11897 GuideControl.GetStartAndEndInConnect, f11896 GuideControl.GetStartAndEndInUpgrade, f11899 GuideControl.ClearState

## Original text

- `GuideUI.tipAdd.max`: 全军出击
- `guide/11`: 操作
- `guide/12`: 通过拖动操作，在你的城堡和其他城堡之间建立连接；通过滑动来断开你所建立的连接。
- `guide/13`: 知道了
- `guide/31`: 胜利目标
- `guide/32`: 占领所有城堡获得胜利。
- `guide/61`: 阵营
- `guide/62`: 每种颜色代表不同的城堡阵营，消灭其他阵营获得胜利。
- `guide/71`: 城堡进化
- `guide/72`: 城堡积攒足够分数时会进化到下一阶段，能同时发出更多条兵线且速度更快。
- `guide/73`: 当城堡到达最高分时，<color=#000000>会将所有接收的士兵派发出去。</color>

## Limits

- live-tutorial-capture: No live first-three tutorial recording/input trace. Static constraints and native Unity collider interaction still need end-to-end tests.
- external-automatic-links: No automatic active-line creation exists in inspected GuideControl/GuideUI; root/flow analysis likewise found none in loader. This is bounded static absence, not a live behavior claim.
- guide-world-points: Hand points use TowerUI GameObject world position minus Vector3 static offset24 (Vector3.up, 0,1,0); original camera/UI world conversion and prefab hand size need live presentation comparison.

# Later tutorial extension

All nine main-table tutorial entries: levels 0,1,2,6,7,9,14,21,25; small-level index0

## all-guide-entries

GuideConfig maps main levels0/1/2/6/7/9/14/21/25 to stages1/4/5/9/7/8/10/11/12 respectively; every row has guildsmallLv0. Stage7/8/9/10/11/12 nextGuildStage=0.

## later-tower-guides

Stage7 Defense and8 Attack acknowledge then immediately invoke inherited UI close (same slot15 as stages3/4). Stage12 Arrow maps to stage-action no-op, so remains until normal disposal, showing GuideUI.tipAdd.arrow. GuideLogicControl maps6 Max,7 Defense,8 Attack,9 Ice,10 Fire,11 Lightning,12 Arrow.

## skill-guide-prompt

For stages9..11, format title/explanation key with (stage-9)+3*SkillControl.mode-2, unless result==-1; format picture key with mode. SkillControl mode is +108. Do not always display ice/fire/lightning names: alternate commanders have different skills.

## skill-guide-hand

Stage9/10 show hand at SkillControl.items[0/1].GameObject.transform.position; scale to Vector3(1.2,1.2,1.2) over1s with15loops and completion deactivating hand GameObject. Stage11 starts at items[2] then calls GetEndPosInSkill, hides hand for zero endpoint else DOMove2s with15loops. GetEndPosInSkill walks active towers in list order: mode1 selects first nonplayer tower; mode2 selects first player tower; other modes returnzero. Endpoint is TowerCanvas GameObject world position minus Vector3.up. No automatic skill execution or forced battle-input lock in these stage actions.

## skill-guide-exit

GuideUI subscribes Event static+36 UseIceOrUp,+40 UseFireOrDown,+44 UseLightOrRevert. Their callbacks only test current stage==9/10/11 respectively then EnterNextStage immediately. No payload, target, score, animation completion or delay condition. Wrong slot event leaves tutorial unchanged.
