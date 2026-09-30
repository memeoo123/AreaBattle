# Incremental UI and soldier resource evidence

Target: `wxcf1394487200e48f/43`. This snapshot supplements the original asset evidence without overwriting it or the original cache. No original game code was executed and no Unity project files were changed for this stage.

The original Unity AssetBundleManifest gives a transitive closure of 10 bundles for GuideUI, Proj_xqzdPauseUI and soldier_300. Three already existed locally; seven catalogued files totaling 662,662 bytes were downloaded. Every file matches the original catalogue size and MD5 and has a SHA-256 fingerprint. The separate npc03.png catalogue bundle was not fetched: the soldier material bundle already contains its required texture and the actual reference closure is complete.

- `acquisition-manifest.json`: exact catalogue lines, dependency edges, source paths, URLs and hashes. The sandbox socket denial is preserved separately; the authorized escalated download succeeded.
- `unity-consumption-index.json`: entry point for subsequent editable Unity import.
- `prefabs/guideui.flat.json`: 36 nodes, 10 UIOutlet bindings, 293 serialized dependency objects.
- `prefabs/proj_xqzdpauseui.flat.json`: 29 nodes, 14 bindings, 266 dependency objects.
- `prefabs/soldier_300.flat.json`: original transform/components, native mesh, material/texture dependencies and SpineAnimator animation records.
- `asset-evidence-incremental.json`: new objects plus explicit baseline reuse; original CAB/PathID and external GUID values retained. Cooked assets do not supply original .meta GUIDs.
- `export-validation.json`: zero unresolved dependency references or partial prefab components, PNG decode checks, native mesh index bounds and unchanged baseline fingerprint.
- `file-hashes.json`: fingerprints for every snapshot file except the hash list itself.

Soldier_300 has 274 vertices and 273 triangles with native coordinates and 274 UV1 records. Prefer `.mesh.json` over the UnityPy OBJ derivative, which mirrors X. Use `.texture.json` as lossless packed animation data and sampling metadata; PNGs alone are not an animation import contract. Sprite `.canvas.png` files restore original rect dimensions and trim offsets; preserve original pivot, PPU and borders.

This verifies extraction and references only. Native import, material/shader equivalence, runtime UI adaptation, animation selection and comparison with the original game remain subsequent gates. Runtime-loaded tutorial effects outside these three manifest roots are not claimed complete.

Correction from subsequent presentation audit: the original exporter did not recurse into UnityPy tuple map entries. The incremental exporter now rebuilds references from persisted JSON, including Material texture environments and SpriteAtlas render data; 58 object reference sets were corrected. Closure counts above include the additional bindings and soldier_300 now has 11 dependency objects. All still resolve locally without further downloads. The old baseline evidence remains unchanged, with its limitation explicitly preserved.

Reproduce with the pinned bundled Python and `-B`, from the workspace root:

1. `analysis/asset_incremental.py --download` (verified existing files are reused).
2. `analysis/asset_incremental_export.py`.
3. `analysis/asset_incremental_flatten.py`.
4. `analysis/asset_incremental_verify.py`.

All four stages completed with exit code 0 after the recorded sandbox network denial. UnityPy version: 1.25.2.
