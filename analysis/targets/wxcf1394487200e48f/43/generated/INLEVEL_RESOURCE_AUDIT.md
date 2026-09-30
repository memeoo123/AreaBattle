# In-level resource audit

Integrity: PASS. Checked 240 acquired files and 1363 Unity GUID references. No source bundles, baseline asset-evidence or Unity assets changed by this audit.

## Current source-backed coverage

| Area | State | Assets |
|---|---|---|
| Ordinary towers | imported and wired | 72 source sprite canvases with source pivot/PPU; normal/defense/attack/arrow grade/camp variants |
| Default soldiers | imported and wired | soldier100/200/300 native mesh, original packed animation textures/material tuples, run/dead ranges, camp tint |
| Battlefield background | imported and wired | HD4_CJ_1 original geometry/material plus runtime SceneSkin binding |
| Obstacles | imported and wired | 15 original visual prefabs and geometry/material textures; independent source collision topology |
| Way lines and arrow/gestures | imported and wired; Editor-only trail history limitation retained | 2 wayline prefabs; 3 projectile/drag-circle/cut-gesture prefabs |
| 18 commander skills | 21 roots and 3 ordinary embedded effects imported and wired | 24 editable native prefabs, packed Animation/Animator, particle fields, exact reconstructed compiled custom shader formulas |
| Bosses and Boss soldiers | 4 models and7 embedded prefabs imported; source ice/marker consumers passed; fireball flight/hit/material/retry passed | Boss801/802 and Soldier9001/9002, original atlas/skeleton/IK/path data, cloud/rain; entity821 subset and shadow9034 |
| HUD, pause and results | 7 prefabs imported and wired | TowerCanvas/SkillUI/PlayTopBar/VictoryUI/DefeatUI/GuideUI/PauseUI; original sprites/fonts/rectangles and source Button/Slider links |
| Result entrance animation | source-default autoplay restored and validated | 3 Animation components/5 clips/28 retained curves/531 keys |
| Guide demonstration and hand | source Spine, hand, popup text and Pitch all6 integrated cases passed | YD_0 4.1.16, 47 attachment frames, 2 atlas materials; original guideHand Image; original tip/Pitch nodes |
| Battle/skill/result audio | 38 source clips decoded and imported, required ID closure present | BGM/capture/connect/cut/arrow/skill/results2009+2010/button sounds; source AAC decoded without resampling |
| Tower skill-target markers and guide107 | 3 ordinary prefabs/8 resources imported; all ordinary/Boss mode and107 production cases passed | hero_sanjiao_blue/red and hdzd_effect_yindao_03; Boss7-prefab closure includes correct source markers and skill_TM |

## Integrated validation

267 of 267 integrated cases passed. Source reports: analysis/unity-integrated-validation.json, unity-tower-marker-import-report.json, unity-boss-embedded-import-report.json.

- All6 Guide cases passed, including original popup text/Pitch lifecycle.
- Ordinary marker import passed:3 prefabs/8 resources. Production all-tower marker modes, guide confirmation107 and retry cases passed.
- Boss embedded import passed:7 prefabs/71 resources,1272 packed curves/160842 curve samples. Correct skill_TM ice and marker bindings passed.

## Pending confirmed consumers

No remaining failures among the currently confirmed imported consumers. Original matched-frame visual acceptance remains separate.

## Unconfirmed consumers and validation limits

- tower-buff: Older skill contract has no confirmed TowerBuff producer. This is not an established missing asset consumer.
- nondefault-skins-and-mode3000: Only source default soldier100/200/300 restored. Current original selected skins and GameControl +3000 flag remain unobserved; optional cosmetic catalog membership alone does not establish a required active consumer.
- skill14-nondefault-skin: Current StartRecruits emits EntityId1000/SkinType1, view uses soldier100; source alternative selected-skin producer is not independently confirmed. Default original asset present; nondefault variants not claimed.
- boss821-remaining-unused-children: Regions and SphereTrails still lack a confirmed active consumer. Red/blue markers and skill_TM now have imported and production-validated source consumers.
- Original synchronized rendered-frame comparison remains pending across all new visuals.
- Native TrailRenderer temporal history in Editor-only AdvanceFrame without engine updates is not equivalent to engine runtime emission.
- Source ParticleSystem autoRandomSeed retained; pixel-identical random particle frames not established.
- Six constant startDelay empty min/max curves normalize to 12 dormant two-key arrays in Unity6; source active numeric values unchanged and strict diff retained.
- Unity6 regenerates mipmaps from exact exported base PNGs; original compressed mip bytes remain in bundles.

## Historical evidence corrections

Older standalone evidence files are preserved. Current closure resolves their previous Pause/Guide cache gap, missing audio2010, result entrance animation, topbar/Boss slider, and Boss action/rain/shadow import statements. The machine-readable audit records each correction.

No matched original-game visual baseline acceptance is granted by this audit.
