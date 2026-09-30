"""Validate exported evidence and summarize unresolved runtime presentation work."""
import collections,json,hashlib
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
rp=ROOT/'generated/asset-evidence.json';r=json.loads(rp.read_text(encoding='utf8'))
idx=json.loads((ROOT/'generated/unity-assets/reconstruction-index.json').read_text(encoding='utf8'))
paths={};fail=[];counts=collections.Counter()
for o in r['objects']:
    for kind,p in o['outputs'].items():
        if p in paths:fail.append('Duplicate output '+p)
        paths[p]=o['id']
        q=ROOT/p
        if not q.is_file():fail.append('Missing '+p);continue
        counts[kind]+=1
        if kind=='png':
            with Image.open(q) as im: im.verify()
        elif kind=='meshNative':
            m=json.loads(q.read_text(encoding='utf8'));v=m['vertices'];tris=[i for sm in m['submeshTriangles'] for tri in sm for i in tri]
            if not v or any(i<0 or i>=len(v) for i in tris):fail.append('Bad mesh '+p)
assert not fail,fail
unresolved=sum(len(p['unresolved']) for p in idx['prefabs'])
assert unresolved==0
summary={'status':'passed-export-integrity-only','errors':fail,'outputs':dict(counts),'prefabCount':len(idx['prefabs']),'prefabUnresolvedReferences':unresolved,'towerSpriteCount':len(idx['towerSprites']),'soldierAnimationSets':len(idx['soldierAnimations']),'partialMonoBehaviourCount':sum(o.get('typetreePartial',False) for o in r['objects']),'runtimeGeneratedEmptyTextures':sum(o.get('runtimeGeneratedEmptyTexture',False) for o in r['objects']),'assetEvidenceSha256':hashlib.sha256(rp.read_bytes()).hexdigest(),'notValidated':['Unity import','runtime shader equivalence','runtime animation selection','matched original-game visual baseline']}
(ROOT/'generated/unity-assets/export-validation.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
md=f'''# Local asset restoration evidence

Target: `wxcf1394487200e48f/43`. Static extraction only; no target code executed and no network access.

## Confirmed exports

- UnityPy 1.25.2, pinned existing vendor: `E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2`.
- 122 input containers, 10,436 serialized objects. All source container SHA-256 values and CAB + PathID identities preserved in `asset-evidence.json`.
- {counts['meshNative']} meshes in native Unity coordinates, with UV0/UV1, vertex colors, submesh triangles, available tangents/weights/bindposes; OBJ copies mirror X and reverse winding by UnityPy convention.
- 698 Sprite PNGs; texture PNGs plus lossless raw texture type trees. Nine empty runtime font textures are explicitly classified, not treated as missing source content.
- 74 Material type trees retain exact shader references, colors, texture links, scale/offset, float properties and render queues. Font binary and 15 audio sample exports retain original names and provenance.
- 21 focused prefab hierarchies include tower, eight soldier variants, battlefield background, waylines, hit/upgrade effects, PlayUI and OverUI. All static references in these 21 dependency closures resolve locally after canonicalizing the built-in `unity default resources` alias.
- Tower visual is `Bastion/Main/Quad1`, `Quad2`, `Quad3`, using dynamically assigned SpriteRenderer sprites. `TowerUI` contains 72 sprites: 普通塔/进攻塔/防御塔/箭塔 × levels 1/2/3 × 灰/蓝/红/绿/黄/青. The native sprite pivots, pixels-per-unit and crop rects are in `reconstruction-index.json`.

## Soldier animation evidence

`prefabs/soldier_*.hierarchy.json` preserves the complete `Bake.SpineAnimator.animationData` records. Do not derive clip duration or frame indexing from filenames. In soldier_100, `run` has startFrame 299, lengthFrames 37, lengthSeconds 0.6000000238418579; the baked texture is 120 × 334. This source boundary mismatch is retained, not silently normalized. Runtime looping/clamping must use the captured shader and texture settings.

The original `Spine/SkeletonMeshBaker` GLES program was recovered from its LZ4 compiled shader blob (pack2 CAB `CAB-68a3634b9a99c5a2a10a36b772e4e945`, PathID `-1870220313943124997`). UnityPy's top-level Shader.export only returned the property/pass skeleton; `asset_shader_probe.py` recovers the actual program independently.

Evidence: `unity-assets/ShaderEvidence/Spine_SkeletonMeshBaker.platform0.segment0.strings.txt:22` onward.

```
t = (time - _AnimTime.w) * _AnimTime.z
phase = lerp(saturate(t), frac(t), _AnimLoop)
uv.x = (mesh.uv1.x + 0.5) / textureWidth
uv.y = (_AnimTime.x + 0.5 + phase * _AnimTime.y) / textureHeight
packed = tex2Dlod(_AnimTex, uv)
position.xy = (packed.xz * 65536 + packed.yw) * _AnimMul.xy / 65535 + _AnimAdd.xy
position.z = _AnimAdd.z
sample = tex2D(_MainTex, mesh.uv0) * vertexColor
mix = smoothstep(0.1, 0.5, distance(sample.rgb, float3(0.309, 0.4117, 0.7294)))
output = lerp(_Color, sample, mix)
```

The `_STRAIGHT_ALPHA_INPUT` shader variant premultiplies texture RGB by texture alpha before vertex color and tint calculation. Preserve raw texture import sampling/wrap/sRGB settings from `.texture.json`; animation textures are packed numerical data.

## Missing or unvalidated scope

- The 21 selected local prefab closures need no additional remote dependency. Uncached optional skins, other backgrounds and other level-specific content are outside this local export scope; this is not a declaration that all 639 levels have all presentation assets.
- {summary['partialMonoBehaviourCount']} stripped MonoBehaviours in player/WebData have only a header schema. Their raw bytes are preserved and their type-tree records are labeled partial. Cached AssetBundle prefab gameplay presentation records such as TowerBase and SpineAnimator have full schemas.
- Cooked bundles do not retain original `.meta` asset GUIDs. We retain real serialized identities and external GUIDs; do not manufacture purported original GUIDs.
- Export verification passes structural PNG decoding, every output's existence/unique path, every mesh's index bounds, and focused reference resolution. This does not validate Unity 6 import, original animation selection behavior, or matched visuals.

## Reproduce

Run the bundled Python runtime with `-B` in `E:/Projects/AreaBattle`:

1. `analysis/asset_export.py`
2. `analysis/asset_shader_probe.py`
3. `analysis/asset_focus.py`
4. `analysis/asset_verify.py`

All four commands exited 0. `unity-assets/export-validation.json` contains precise counts and the asset evidence fingerprint.
'''
(ROOT/'generated/ASSET_EVIDENCE.md').write_text(md,encoding='utf8')
print(json.dumps(summary,ensure_ascii=False))
