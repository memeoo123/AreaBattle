# Flow / level evidence

Target wxcf1394487200e48f / 43. Static evidence only.

Reproduce: `python analysis/flow_evidence.py`. Decoder source: `analysis/flow_disassemble.py`. Every body is stored in generated/flow-evidence.json with exact byte offsets.

## player-camp [confirmed]

1

Evidence: generated/wasm/wasmcode.wasm:0x846bfb-0x846c0b; generated/wasm/wasmcode.wasm:0x81fb98-0x81fbb6

## coordinate-decimal-places [confirmed]

2

Evidence: generated/wasm/wasmcode.wasm:0x846bdb-0x846beb

## tower-world-position [confirmed]

{
  "formula": "position = (pos.x, pos.y, pos.z) / 100.0",
  "coordinateSpace": "Unity world position",
  "axes": "unchanged x/y/z",
  "scaleStatus": "StarInfoCfg.SetTransform consumes pos and caller-provided euler angles; serialized scale not consumed here"
}

Evidence: generated/wasm/wasmcode.wasm:0x29238a-0x29241f; generated/wasm/wasmcode.wasm:0x18c9dc-0x18ca37; python analysis/flow_api_names.py: wasmcode function1348 = UnityEngine.Transform.set_position

## normal-level-tower-init [confirmed]

{
  "count": "StarInfoCfgs.Length",
  "towerIds": "array index + 1",
  "camp": "StarInfoCfg.CampID",
  "score": "float(StarInfoCfg.StartScore)",
  "ship": "StarInfoCfg.ShipID",
  "legacyDispathchID": "not a field in current StarInfoCfg metadata; Tower.Init consumes ShipID, not legacy DispathchID"
}

Evidence: generated/wasm/wasmcode.wasm:0x56ac0c-0x56acef; generated/wasm/wasmcode.wasm:0x725bc8-0x725bea; generated/wasm/wasmcode.wasm:0x725c66-0x725c71; generated/wasm/wasmcode.wasm:0x725ccc-0x725cd9; gameplay-symbols.json: StarInfoCfg fields

## ship4-arrow-tower [confirmed]

{
  "rule": "ShipID==4 creates or reuses ArrowTower; other non-boss ShipIDs create/reuse Tower",
  "legacyDispathchID99": "No current typed field consumes it; ShipID controls tower subclass"
}

Evidence: generated/wasm/wasmcode.wasm:0x56ac50-0x56ac59; generated/wasm/wasmcode.wasm:0x56a9f1-0x56aa0c; generated/wasm/wasmcode.wasm:0x56aa4e-0x56aa62

## battle-state-enum [confirmed]

{
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

Evidence: global-metadata.dat: field indices18349..18364; compressed default bytes00,02,...1e; generated/wasm/wasmcode.wasm:0x37a144-0x37a15e

## state-transitions [confirmed]

{
  "initial": "First -> StartUI",
  "enter": "Enter opens loading then callback requests current-level configuration",
  "ready": "load callback creates towers/obstacles, RefreshCampInfo, BeforePlay unless callback flag disables transition",
  "beforePlay": "BeforePlay -> StartPlay after pre-game UI checks",
  "start": "StartPlay -> Running for non-guide; guide advances guide stage",
  "again": "Again invokes InitGameData, resets SmallLevelIndex to0, enters Enter",
  "runningUpdate": "Tower updates only if current state==6"
}

Evidence: generated/wasm/wasmcode.wasm:0x37a15f-0x37a1c4; generated/wasm/wasmcode.wasm:0x708548-0x7085fa; generated/wasm/wasmcode.wasm:0x37a2cc-0x37a33d; generated/wasm/wasmcode.wasm:0x37a510-0x37a555; generated/wasm/wasmcode.wasm:0x81f516-0x81f51e

## victory-defeat [confirmed]

{
  "evaluation": "RefreshCampInfo; skips StartUI and special-controller active guard; requires player camp record",
  "defeat": "player camp active tower count == 0 -> Defeat(9)",
  "victory": "total initial StarInfoCfg count <= player camp active tower count -> Victory(8)",
  "bossVictory": "if curBoss exists and trunc(curBoss.score/HP) <= 0 -> Victory(8)",
  "refresh": "score-sum dirty flag and countdown<=0 cause RefreshCampInfo in Running; countdown +=1 second"
}

Evidence: generated/wasm/wasmcode.wasm:0x163a78-0x163abc; generated/wasm/wasmcode.wasm:0x163b10-0x163b6b; generated/wasm/wasmcode.wasm:0x379ee5-0x379ef1; generated/wasm/wasmcode.wasm:0x81f6fb-0x81f736

## AI-active-camps [confirmed]

"AI controller initializes only CampID !=0 and CampID !=PlayerCampID; AIGrade passed to AICamp.Init"

Evidence: generated/wasm/wasmcode.wasm:0x631540-0x631575

## AI-timer [confirmed]

{
  "initialDelay": "AIConfig.DelayTime / 1000.0; absent config => float.MaxValue",
  "initialTimer": 0,
  "update": "if delay>0: delay-=delta; return. Otherwise timer-=delta; if timer<0: timer+=actionTime; AIGoToAction(campId, AI_id). At most one action per Update.",
  "actionInterval": "pick one element of AIConfig.ActionTime, convert /1000; no config or empty array => float.MaxValue",
  "actionCount": "RandomHelper.Random(1,AIConfig.ActionNum); absent config=>0",
  "RandomHelper": "function1621 forwards lower and upper+1 into a virtual random method; inclusive upper is inferred from standard System.Random.Next contract"
}

Evidence: generated/wasm/wasmcode.wasm:0x631256-0x631287; generated/wasm/wasmcode.wasm:0x90d6d3-0x90d748; generated/wasm/wasmcode.wasm:0x5e0bea-0x5e0c1a; generated/wasm/wasmcode.wasm:0x5e0db7-0x5e0ea3; generated/wasm/wasmcode.wasm:0x82a72-0x82a99

## AI-action-choice [confirmed]

{
  "type0or1": "ConfigMgr.GetAIAction(AI_id,trunc(score),maxScore)",
  "type2or3": "q = int(maxScore/5); score<q =>1; score<2q=>2; score<3q=>4; else=>3",
  "otherType": "0",
  "weightedConfigActionIds": {
    "Protect": 1,
    "Defend": 2,
    "Reinforce": 4,
    "Occupy": 3
  },
  "weightedEligibility": "score >= pair[0] * (maxScore/100.0); cumulative weight += pair[1]",
  "weightedSelection": "RandomHelper.Random(0,last cumulative), first cumulative > sample; exact terminal equality does not match any entry"
}

Evidence: generated/wasm/wasmcode.wasm:0x90da27-0x90db45; generated/wasm/wasmcode.wasm:0x5e0ca2-0x5e0d02; generated/wasm/wasmcode.wasm:0x1add55-0x1addda; generated/wasm/wasmcode.wasm:0x5e0d42-0x5e0db3

## level-table-access [confirmed]

{
  "method": "GetLevelConfig(level) normalizes GetLevelRealID then queries ConfigMgr instance+120 (dicLevel). It does not query instance+124 (dicLevelB).",
  "limitWrap": "if level>maxLevelNum: n=maxLevelNum-36; r=(level-maxLevelNum)%n; replace zero r with n; h=(n+1)/2 integer; if r<=h return 2*r+35 else return 2*(r-h)+36"
}

Evidence: generated/wasm/wasmcode.wasm:0x21cd63-0x21cda9; generated/wasm/wasmcode.wasm:0x5e032e-0x5e03a6

## camera-adaptation [confirmed]

{
  "aspect": "Screen.width/Screen.height",
  "orthographicSize": "2.1 * (0.5625 / aspect)",
  "defaultAspect": 0.5625,
  "sceneScale": "ratio based correction applied to GameSceneMono field+32 Transform.localScale; wider-than-default uses 2*aspect*orthographicSize/2.5875"
}

Evidence: generated/wasm/wasmcode.wasm:0x3913e2-0x391411; generated/wasm/wasmcode.wasm:0x391440-0x3914d1; python analysis/flow_api_names.py

## AI-source-and-target-order [confirmed]

{
  "sources": "GetCampAllTowerNonArrow(camp): active camp-owned towers excluding ArrowTower",
  "shuffle": "for i=0..Count-1: j=UnityEngine.Random.Range(0,Count); swap source[i],source[j] if unequal; whole list range each iteration",
  "budget": "GetAIActionNum; if budget<=0 at next-source boundary, return",
  "targetOrder": "No target distance or score sorting in AIGoToAction. Enumerates current incoming tower list for Protect/Defend; current GetStarAllLine physical-line list for Reinforce/Occupy. WayLineControl initializes the physical list sorted by LineLength ascending (independent asset_pipeline static proof); therefore Reinforce/Occupy prefer shortest eligible physical edges.",
  "lineContains": "Outgoing exclusion uses List.Contains semantics: helper1209->5918 invokes index-search and compares !=-1."
}

Evidence: generated/wasm/wasmcode.wasm:0x631713-0x6318b5; generated/wasm/wasmcode.wasm:0x46f9e8-0x46faae; generated/wasm/wasmcode.wasm:0x1ff354-0x1ff3c7; controls-disassembly/__c-TnyuYV_-100665602.txt: comparer LineLength field+52; CoreProbe-function2297-2297.txt less=-1 greater=1

## AI-protect [confirmed]

{
  "action": 1,
  "steps": [
    "RemoveStarAllLine(source) removes source outgoing directions only; incoming connections survive.",
    "GetAllTowerPointThis(source); if incoming.Count==0 return whole AIGoToAction.",
    "For each incoming target: break if source.IsCanAddLine is false; skip same camp; otherwise AITryConnectLine(source,target). Its return is ignored.",
    "After loop budget-- once; continue next source."
  ],
  "playerExclusion": "No AIType player-exclusion filter in this action."
}

Evidence: generated/wasm/wasmcode.wasm:0x6318d1-0x631bda; WayLineControl.RemoveStarAllLine function6780@0x270b18 -> WayLine.IsFromThisTower; asset_pipeline independent decode

## AI-defend [confirmed]

{
  "action": 2,
  "preconditions": "source.IsCanAddLine and GetStarAllLine(source).Count>0; otherwise next source",
  "steps": [
    "Enumerate GetAllTowerPointThis(source) in list order.",
    "Skip same camp, skip player-camp targets for AIType0 or2, skip targets already in outgoing list.",
    "AITryConnectLine(source,target); ignore boolean result; budget-- for each attempt.",
    "No inner budget/capacity recheck; budget can become negative before next-source boundary. Continue next source."
  ]
}

Evidence: generated/wasm/wasmcode.wasm:0x631be0-0x631f73

## AI-reinforce-and-occupy [confirmed]

{
  "preconditions": "source.IsCanAddLine and at least one physical WayLine",
  "commonFilter": "For every line get its opposite endpoint; skip absent endpoint and endpoint already in source outgoing list.",
  "enemyFilter": "different camp; additionally AIType0/2 exclude player-camp targets",
  "reinforce4": "Primary targets same camp. First AddActiveLine success decrements budget then RETURNS WHOLE AI CALL. If no success, only AIType2/3 fallback to enemyFilter; first fallback success decrements budget and continues next source.",
  "occupy3": "Primary targets enemyFilter. First AddActiveLine success decrements budget then RETURNS WHOLE AI CALL. If no success, only AIType2/3 fallback to same-camp; first fallback success decrements budget and continues next source.",
  "failedAttempts": "AddActiveLine failure does not consume budget; continue enumeration.",
  "noEligibleTarget": "continue next source; no budget consumed.",
  "loopEvidence": "flow_ai_trace.py annotates branch destinations; distinguishes whole-call return0x632d15 from next-source0x6317a5."
}

Evidence: generated/wasm/wasmcode.wasm:0x631f75-0x6326ff; generated/wasm/wasmcode.wasm:0x632701-0x632d15; generated/wasm/wasmcode.wasm:0x632278-0x6323e9; generated/wasm/wasmcode.wasm:0x632497-0x6326f3; generated/wasm/wasmcode.wasm:0x6329fd-0x632d15

## normal-level-file-mapping [confirmed]

{
  "normal": "GetLevelConfig(requestedLevel) yields typed LevelConfig row: SceneId(+16) is file ID; id(+8) is level identity. Load Path.GetPath(Level_ + SceneId,4).",
  "special": "If LevelControl.SpecialLevel(+24)!=0, uses supplied level directly as file ID.",
  "maxLevelNum": "dicLevel.Count-1 (dictionary count minus free count minus1); 561 rows0..560 imply560",
  "legacySceneIdColumns": "Current LevelConfig typed fields are id,LevelType,SceneId only; SceneId2..10 table columns are not represented.",
  "sublevel": "callback payload contains path,rowId,SmallLevelIndex; no claim that old SceneId2..10 are used."
}

Evidence: generated/wasm/wasmcode.wasm:0x56a2d0-0x56a31a; generated/wasm/wasmcode.wasm:0x56a31c-0x56a748; generated/wasm/wasmcode.wasm:0x7c584f-0x7c5862; gameplay-symbols.json: LevelConfig fields

## tower-transform-defaults [confirmed]

{
  "rotation": "Tower function15519 initializes zero vector and calls StarInfoCfg.SetTransform with it, so tower root world Euler=(0,0,0); serialized angle ignored by this caller.",
  "scale": "StarInfoCfg.SetTransform never reads scale; therefore absent serialized scale does not imply zero tower scale. Prefab/model initialization controls actual scale.",
  "constructors": "StarInfoCfg and ObstacleInfoCfg constructors are no-op bodies (3 bytes); no explicit unit-scale default or nonzero angle default is installed by constructors."
}

Evidence: generated/wasm/wasmcode.wasm:0x725af9-0x725b1d; generated/wasm/wasmcode.wasm:0x29238a-0x29241f; StarInfoCfg..ctor and ObstacleInfoCfg..ctor in captured disassemblies

## pause-and-update-gates [confirmed]

{
  "gate": "LevelControl tower updates, WayLineControl simulation updates and AICampController updates each require state==Running(6).",
  "pause": "OnGamePlayState common preamble calls GlobalData.PauseGame(state==7), forwarding changeTimeScale=true to GameFrameEntry.PauseGame; Time.timeScale=0 on Pause and1 for any non-Pause transition. Case7 also pauses audio.",
  "running": "Running calls SetLevelSpeed(saved speed); SetLevelSpeed stores speed and calls UnityEngine.Time.set_timeScale.",
  "towerDt": "LevelControl passes dt*ConfigMgr static+8 to active Tower virtual update slot18, second time argument unchanged.",
  "lineDt": "WayLineControl dt multiplied by same ConfigMgr static+8, and movement further uses static+12.",
  "order": "AICampController -> LevelControl (active towers and victory refresh) -> WayLineControl. MineGameMain registers them in this order and MineGameLogicModule.Update traverses the insertion list."
}

Evidence: generated/wasm/wasmcode.wasm:0x81f516-0x81f51e; generated/wasm/wasmcode.wasm:0x81f638-0x81f660; generated/wasm/wasmcode.wasm:0x6e2534-0x6e25ac; generated/wasm/wasmcode.wasm:0x90d816-0x90d826; generated/wasm/wasmcode.wasm:0x37a393-0x37a404; generated/wasm/wasmcode.wasm:0x1a33de-0x1a33ec; generated/wasm/wasmcode.wasm:0x37a032-0x37a039; generated/wasm/wasmcode.wasm:0x1a754b-0x1a7551; generated/wasm/wasmcode.wasm:0x5a1118-0x5a1127; generated/wasm/wasmcode.wasm:0x5516ee-0x5516fd; generated/wasm/wasmcode.wasm:0x55179c-0x5517ab; generated/wasm/wasmcode.wasm:0x55184a-0x551859; generated/wasm/wasmcode.wasm:0x82b54-0x82b63; generated/wasm/wasmcode.wasm:0x7f77f4-0x7f78d3; flow_pause_api.py resolves2762=UnityEngine.Time.set_timeScale,1821=UIAudioManager.get_Instance

## retry-initial-score [confirmed]

{
  "again": "Again(10): InitGameData clears active tower/soldier state, resets SmallLevelIndex0, enters Enter(3).",
  "reinitialize": "Load callback creates/reuses towers and invokes Tower.Init; Tower.Init writes float(StarInfoCfg.StartScore) unconditionally. Retry thus restores serialized starting score, not surviving battle score.",
  "clear": "InitGameData resets curBoss; clears camp active/counts; calls active tower virtual slot17; invokes Soldier.Clear(true) then soldier effect destruction."
}

Evidence: generated/wasm/wasmcode.wasm:0x37a510-0x37a555; generated/wasm/wasmcode.wasm:0xfc1f3-0xfc1f7; generated/wasm/wasmcode.wasm:0xfc49a-0xfc4d6; generated/wasm/wasmcode.wasm:0xfc674-0xfc68d; generated/wasm/wasmcode.wasm:0xfc836-0xfc876; generated/wasm/wasmcode.wasm:0x725c66-0x725c71

## camera-base-scale [confirmed]

{
  "vector": [
    0.25,
    0,
    0.4000000059604645
  ],
  "destination": "GameSceneMono field+32 Transform.localScale",
  "ratio": "aspect<0.5625 uses0.5625/aspect; aspect==0.5625 uses1; aspect>0.5625 uses2*aspect*orthoSize/2.5875",
  "qualification": "This is the specific scene object adjusted by SetCameraSize, not every tower root."
}

Evidence: generated/wasm/wasmcode.wasm:0x86533e-0x865361; generated/wasm/wasmcode.wasm:0x391429-0x3914d1

## ordinary-wave-scope [inferred]

{
  "observation": "Recovered ordinary level path creates the serialized tower set once. Recovered LevelControl.Updata only updates active towers and periodically refreshes camp totals; it contains no independent wave-spawn timer. AI acts through tower connections.",
  "limit": "This supports no additional scheduled enemy waves for the selected ordinary slice. It is not a global absence proof across boss, commander, skill, or special mode code."
}

Evidence: generated/wasm/wasmcode.wasm:0x56ac0c-0x56acef; generated/wasm/wasmcode.wasm:0x81f516-0x81f736; generated/wasm/wasmcode.wasm:0x631633-0x632d15

## Remaining unknowns

[
  {
    "id": "levelB-channel-use",
    "status": "unknown",
    "plan": "Trace dicLevelB references across callers if alternate channel is required; observed GetLevelConfig directly uses dicLevel."
  },
  {
    "id": "original-runtime-golden",
    "status": "unknown",
    "plan": "Compare original running level tower positions and one victory/defeat replay; all current findings are static verified, not live parity."
  }
]

The first stdout attempt hit GBK UnicodeEncodeError on obfuscated symbols; decoder now explicitly writes UTF-8. PowerShell numeric @ selectors require single quotes. No source artifacts were modified.