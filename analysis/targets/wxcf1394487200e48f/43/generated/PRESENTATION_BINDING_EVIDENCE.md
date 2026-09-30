# Presentation binding audit

Target: `wxcf1394487200e48f/43`. Static preparation only; no visual acceptance and no Unity project edits.

`presentation-binding-evidence.json` contains 80 decoded original functions and 9 confirmed rules. `presentation-prepared/runtime-data.json` is the compact import contract; every path is relative to this target directory. Detailed per-soldier descriptors retain CAB/PathID, original material values, texture sampler settings and output SHA-256 hashes.

## Default soldiers

| Model | Local scale | Run start | Frame length | Duration | Anim texture |
|---|---:|---:|---:|---:|---|
| soldier_100 | .0520000011 | 299 | 37 | .600000024 s | 120 × 334 |
| soldier_200 | .0520000011 | 506 | 37 | .600000024 s | 118 × 541 |
| soldier_300 | .0520000011 | 49 | 35 | .566699982 s | 274 × 84 |

Soldier.InitGameObject/PlayAni selects `run`, looping. Do not use prefab `autoPlayAnimation=idle` as the gameplay selection. The rightward branch (`type <= 3 && start.x < end.x`) applies model local Euler angles `(-camera.localEulerAngles.x,-180,0)`; the other branch applies `(camera.localEulerAngles.x,0,0)`. Scale remains positive. Exact function spans are embedded in the evidence JSON.

Animation uses `_AnimTime=(startFrame,lengthFrames,1/max(lengthSeconds,.01),startTime)`, `_AnimLoop=1`, and the original GLES formula preserved in `soldier-shader.json`. Do not shorten run ranges to texture height: the original V clamp handles the final overrun rows for models 100/200. Animation clock is `Time.time` when a default render pipeline exists, otherwise `Time.timeSinceLevelLoad`.

The three `.anim.rgba32` files copy original Texture2D image bytes exactly; import them with `LoadRawTextureData`, width/height from the contract, RGBA32, linear, bilinear, clamp, no mipmaps. They avoid the image-export Y-orientation ambiguity. Main PNGs are decoded from original ASTC_RGB_6x6, with sRGB, bilinear, repeat and no mipmaps; validate their final mesh orientation on first Unity import.

Shader default queue is Transparent/3000 because materials specify custom queue -1. Normal pass uses Blend One OneMinusSrcAlpha, ZWrite Off, ZTest LEqual, Cull Off; the full original states including stencil properties and caster pass are preserved. Runtime sets sorting layer Default/order -5, overriding serialized order 0. Runtime also sets `_Color` from ColorHelper at index `max(camp,1)-1`; the exact six colors and bytecode constant offsets are in the runtime contract.

## Tower score projection

TowerCanvas.RefreshPos adds WORLD up `.1 + (DispatchConfig.maxLine-1)*.05`, yielding .1/.15/.2 for grade 0/1/2. It projects through the battle camera, converts screen coordinates into the tower UI root's **parent** RectTransform using the parent Canvas camera of UIControl.root, then sets tower root anchoredPosition. The score child remains at its prefab offset `(0,85.6)` with font size 40 and best-fit 10–40. See `tower-score-projection.json`, `hud-evidence.json` and `dispatch-model.json`.

## Integrity correction and limits

The original exporter omitted pointer maps represented as Python tuples. Incremental references were rebuilt from saved JSON, recovering Material texture and SpriteAtlas map dependencies; the original baseline was not overwritten. The incremental snapshot verifier passed, hashing 547 files and confirming baselineUnchanged=true. GuideUI/PauseUI/soldier300 closures resolve completely.

Remaining unknowns: accepted original live render baseline, complete current-device camera/canvas adaptation, actual live account skin choice and special +3000 skin selection. Static source recovery alone does not close visual acceptance. No missing value was replaced by a guessed runtime value.

Reproduction: run `analysis/presentation_disassemble.py`, `analysis/presentation_prepare.py`, then `analysis/asset_incremental_verify.py` with the pinned Python runtime and UnityPy 1.25.2. The prepare validator resolves all three meshes/materials and six texture bindings and checks every raw animation byte count.
