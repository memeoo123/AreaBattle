# Controls and adjacency evidence

Target `wxcf1394487200e48f/43`. Static local WASM evidence; no game code executed.

## adjacency-cache

[已确认] GetStarAllLine(towerIndex) lazily creates one list in the dictionary at WayLineControl+136; it is a candidate adjacency list, not the active outgoing-line count.

- `generated/controls-disassembly/WayLineControl-GetStarAllLine-100665552.txt` @ `0x00154469`–`0x001544c3`

## adjacency-generation

[已确认] InitStarLine async MoveNext waits for level readiness and one EndOfFrame, then iterates active tower pairs i<j and calls GetWayLineEntity. It next builds each tower adjacency by collecting every pooled WayLine.IsContainTower(tower), then sorts by ascending LineLength.

- `generated/controls-disassembly/__zX_lp-MoveNext-100665604.txt` @ `0x00486cc2`–`0x00486ce0`
- `generated/controls-disassembly/__zX_lp-MoveNext-100665604.txt` @ `0x00487425`–`0x00487579`
- `generated/controls-disassembly/__zX_lp-MoveNext-100665604.txt` @ `0x00486e1c`–`0x004871ef`
- `generated/controls-disassembly/__c-TnyuYV_-100665602.txt` @ `0x0070f0fb`–`0x0070f111`
- `generated/controls-disassembly/CoreProbe-function2297-2297.txt` @ `0x000ab9c2`–`0x000ab9f1`

## capsule-blocking

[已确认] Potential lines use tower world positions and Physics overlap-capsule radius 0.029999999329447746 in the GamePlayer1 layer. Any collider whose GameObject differs from both endpoint GameObjects blocks creation. Existing pair lookup happens before this new-line test.

- `generated/controls-disassembly/WayLineControl-GetWayLineEntity-100665560.txt` @ `0x00270e25`–`0x00270ed5`
- `generated/controls-disassembly/WayLineControl-GetOverlapCapsuleGameObj-100665583.txt` @ `0x0018a3f7`–`0x0018a407`
- `generated/controls-disassembly/WayLineControl-_oWgo__-100665571.txt` @ `0x001d699e`–`0x001d6a16`
- `generated/controls-disassembly/Layers-.cctor-100665864.txt` @ `0x00821183`–`0x0082118b`

## line-direction-states

[已确认] WayLine canonical endpoints are small/large tower index. State 0 none, 1 small→large, 2 large→small, 3 both. SetLineState rejects already-both and already-same-direction. Adding the reverse direction between same-camp towers replaces direction; different camps become bidirectional.

- `generated/controls-disassembly/WayLine-SetLineState-100665522.txt` @ `0x0027025e`–`0x002702a4`
- `generated/controls-disassembly/WayLine-IsFromThisTower-100665529.txt` @ `0x00122c56`–`0x00122ca4`

## remove-outgoing-only

[已确认] RemoveStarAllLine(tower) iterates WayLines but removes only IsFromThisTower=true. In a bidirectional line RemoveLine(tower) leaves the opposite direction. Inbound-only lines remain. This corrects any earlier interpretation that camp-change deletes every touching line.

- `generated/controls-disassembly/WayLineControl-RemoveStarAllLine-100665555.txt` @ `0x00270b18`–`0x00270b38`
- `generated/controls-disassembly/WayLine-RemoveLine-100665524.txt` @ `0x00270675`–`0x002706a8`

## line-count-outgoing

[已确认] GetTowerAllLine filters IsFromThisTower; SetTowerLineNum writes that list count through virtual slot 9. Incoming lines therefore do not consume the tower outgoing count.

- `generated/controls-disassembly/WayLineControl-GetTowerAllLine-100665577.txt` @ `0x0027123c`–`0x00271246`
- `generated/controls-disassembly/WayLineControl-SetTowerLineNum-100665597.txt` @ `0x002709b3`–`0x002709ed`

## ai-connect-validation

[已确认] AITryConnectLine rejects ArrowTower, source state 4 (Unlinkable per combat evidence), self target, and failed virtual get_IsCanAddLine. It obtains the pair and calls SetLineState only if a non-null line exists.

- `generated/controls-disassembly/WayLineControl-AITryConnectLine-100665563.txt` @ `0x006e334c`–`0x006e33aa`

## press-player-source

[已确认] Touch-down handler f14747 is gated by CanDraw and an overlay/input-block flag, raycasts tower/obstacle layer GamePlayer1, resolves the GameObject to a tower, requires source camp == LevelControl.playerCamp, and requires virtual get_IsCanAddLine. Rejected selection clears source fields. It preserves original tutorial-specific level-2 gate instead of inventing generic behavior.

- `generated/controls-disassembly/WayLineControl-M____rR-100665576.txt` @ `0x006e2988`–`0x006e2a81`

## drag-preview-release-commit

[已确认] Touch-move f9378 projects pointer to Scenes10 ground, updates preview and potential target; it does not call SetLineState. Touch-up f14746 calls f9379 only when source and target objects exist, then resets selection, active flag, cutting marker and preview. f9379 requires distinct valid towers, stored selected source identity, and source capacity, then GetWayLineEntity and SetLineState.

- `generated/controls-disassembly/WayLineControl-_uZly_h-100665574.txt` @ `0x004579dc`–`0x00457b4f`
- `generated/controls-disassembly/WayLineControl-O_PYk_S-100665554.txt` @ `0x006e28e6`–`0x006e2928`
- `generated/controls-disassembly/WayLineControl-_xt_lM_-100665584.txt` @ `0x00458628`–`0x004586e8`
- `generated/controls-disassembly/WayLineControl-n__fO_a-100665570.txt` @ `0x0018a1f7`–`0x0018a247`

## nearest-drag-collider

[已确认] Preview helper f9377 excludes the source GameObject and chooses the nearest collider by world-space Euclidean transform-position distance, starting at 1000. It replaces on <=, so equal-distance candidates choose the last collider in the engine-provided result order.

- `generated/controls-disassembly/WayLineControl-q_VST__-100665580.txt` @ `0x00457859`–`0x00457955`

## cut-player-direction

[已确认] When dragging from empty ground, f9378 checks the segment between previous/current world points against GameNPC1, obtains the WayLine from the hit object, and calls RemoveLine(LevelControl.playerCamp). That overload removes direction(s) whose source endpoint camp matches the player; enemy outbound direction survives a bidirectional cut.

- `generated/controls-disassembly/WayLineControl-_uZly_h-100665574.txt` @ `0x00458525`–`0x004585dd`
- `generated/controls-disassembly/WayLine-RemoveLine-100665494.txt` @ `0x00457451`–`0x00457483`

## interaction-feedback

[已确认] Press tower plays audio ID 2015; successful pair obtained on release plays 2007; a cut hit plays 2008. Connection/cut paths request a 50 ms vibration (type 2).

- `generated/controls-disassembly/WayLineControl-M____rR-100665576.txt` @ `0x006e2a02`–`0x006e2a0e`
- `generated/controls-disassembly/WayLineControl-_xt_lM_-100665584.txt` @ `0x004586ba`–`0x004586dd`
- `generated/controls-disassembly/WayLineControl-_uZly_h-100665574.txt` @ `0x0045859e`–`0x004585c1`

## Limits

[待确认] Preview line geometry and all tutorial-specific branches require the next bounded checks listed in controls-evidence.json. Static findings do not certify a live visual match.

Reproduce: bundled Python `-B analysis/controls_disassemble.py`, then `-B analysis/controls_evidence.py`. Both exit 0. Every cited offset is checked against the generated disassembly.
