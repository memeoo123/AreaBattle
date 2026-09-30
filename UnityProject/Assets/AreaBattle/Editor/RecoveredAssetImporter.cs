using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    /// <summary>Imports only locally recovered, provenance-indexed assets. No network or game code.</summary>
    public static class RecoveredAssetImporter
    {
        const string Destination = "Assets/AreaBattle/Resources/Recovered";
        static string projectRoot, workspaceRoot, targetRoot;
        static ImportReport report;

        [MenuItem("AreaBattle/Import recovered assets")]
        public static void Import()
        {
            projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            workspaceRoot = Directory.GetParent(projectRoot).FullName;
            targetRoot = Path.GetFullPath(Path.Combine(workspaceRoot, "analysis/targets/wxcf1394487200e48f/43"));
            report = new ImportReport { status = "running", editorVersion = Application.unityVersion };
            try
            {
                var spec = Read<Spec>("generated/RESTORE_SPEC.json");
                if (!spec.implementationReady || spec.target.appId != "wxcf1394487200e48f" || spec.target.version != "43")
                    throw new InvalidOperationException("The restoration specification has not passed the implementation gate for this target.");
                if (!File.Exists(Path.Combine(workspaceRoot, "ORCHESTRATION_STATE.json")))
                    throw new FileNotFoundException("ORCHESTRATION_STATE.json is required.");

                var index = Read<Index>("generated/unity-assets/reconstruction-index.json");
                var evidence = Read<Evidence>("generated/asset-evidence.json");
                if (index.towerSprites == null || index.towerSprites.Length != 72)
                    throw new InvalidDataException("Expected exactly 72 recovered tower sprites.");
                var atlasTexture = evidence.objects.Single(o => o.type == "Texture2D" && o.name.Contains("TowerUI"));
                var atlasSettings = Read<TextureSource>(atlasTexture.outputs.textureRaw);
                var spriteRecords = evidence.objects.Where(o => o.type == "Sprite").ToDictionary(o => o.id);
                foreach (var tower in index.towerSprites)
                {
                    ValidateName(tower.name);
                    var source = spriteRecords[tower.@object];
                    if (string.IsNullOrEmpty(source.outputs.pngCanvas))
                        throw new InvalidDataException("Missing untrimmed canvas for " + tower.name);
                    string assetPath = Destination + "/Towers/" + tower.name + ".png";
                    ImportSprite(source.outputs.pngCanvas, assetPath, tower.pivot, tower.pixelsToUnits,
                        tower.border, atlasSettings, source.id, "confirmed-original-rect-pivot-ppu");
                    report.towerSprites++;
                }

                var font = evidence.objects.First(o => o.type == "Font" && o.name == "HYZhuZiMuTouRenW" && o.source.Contains("firstpack"));
                string fontPath = Destination + "/Fonts/HYZhuZiMuTouRenW.ttf";
                CopySource(font.outputs.font, fontPath, font.id, "confirmed");
                AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceSynchronousImport);
                if (AssetDatabase.LoadAssetAtPath<Font>(fontPath) == null)
                    throw new InvalidDataException("Unity did not import the recovered font: " + fontPath);
                SetProvenance(fontPath, font.outputs.font, font.id, "confirmed");
                report.font = fontPath;

                ImportBackground(evidence);
                PreserveSoldierData(evidence, "soldier_100");
                PreserveSoldierData(evidence, "soldier_200");
                AssetDatabase.SaveAssets();
                report.status = "passed-minimum-asset-import";
                report.limitations.Add("Soldier data retained as editable source evidence; no soldier shader or animation import is claimed.");
                report.limitations.Add("Asset import is not a matched original-game visual baseline or complete battlefield acceptance.");
                WriteReport();
                Debug.Log("Recovered assets imported: " + report.towerSprites + " tower sprites, font and background prefab.");
            }
            catch (Exception ex)
            {
                report.status = "failed";
                report.error = ex.ToString();
                WriteReport();
                throw;
            }
        }

        static void ImportBackground(Evidence evidence)
        {
            const string hierarchyPath = "generated/unity-assets/prefabs/HD4_CJ_1.hierarchy.json";
            var hierarchy = Read<Hierarchy>(hierarchyPath);
            var texture = evidence.objects.Single(o => o.type == "Texture2D" && o.name == "scene_skin_game1");
            var textureSettings = Read<TextureSource>(texture.outputs.textureRaw);
            var skins = Read<SceneSkins>("generated/tables/SceneSkinConfig.json");
            var defaultSkin = skins.Datas.Single(s => s.id == 1);
            if (defaultSkin.gameIconName != texture.name)
                throw new InvalidDataException("Default scene skin no longer matches the recorded HD4_CJ_1 binding.");
            const string spritePath = Destination + "/Background/scene_skin_game1.png";
            // GameControl.LoadGameScene loads entity11001; f7818 loads gameIconName.
            // Callback15169 creates the full texture sprite with center pivot; wrapper7100 uses100 PPU.
            ImportSprite(texture.outputs.png, spritePath, new Vector2(.5f, .5f), 100f,
                Vector4.zero, textureSettings, texture.id, "confirmed-runtime-default-scene-binding");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Required original built-in Sprites/Default shader unavailable.");
            const string materialPath = Destination + "/Background/Sprites-Default.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.name = "Sprites-Default";
            material.color = Color.white; // Exact original m_SavedProperties._Color.
            material.SetFloat("PixelSnap", 0f);
            material.SetFloat("_EnableExternalAlpha", 0f);
            material.SetVector("_Flip", Vector4.one);
            material.SetColor("_RendererColor", Color.white);
            material.enableInstancing = false;
            material.renderQueue = -1;
            EditorUtility.SetDirty(material);
            var materialSource = hierarchy.dependencies.Single(o => o.type == "Material");
            SetProvenance(materialPath, materialSource.outputs.typetree, materialSource.id, "confirmed-original-saved-properties");

            var root = BuildBackgroundNode(hierarchy.root, null, sprite, material);
            const string prefabPath = Destination + "/Background/HD4_CJ_1.prefab";
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
                if (!success) throw new InvalidDataException("Failed to save recovered background prefab.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            SetProvenance(prefabPath, hierarchyPath, hierarchy.root.id, "confirmed-hierarchy-and-dynamic-sprite-binding");
            CopySource(hierarchyPath, Destination + "/Data/HD4_CJ_1.hierarchy.json", hierarchy.root.id, "confirmed-source");
            report.backgroundPrefab = prefabPath;
            report.limitations.Add("HD4_CJ_1 dynamically binds scene_skin_game1 at100 PPU, center pivot, full texture rect (functions6058/7818/15169/7100). Source camera/render geometry still requires a matched original visual baseline.");
        }

        static GameObject BuildBackgroundNode(Node node, Transform parent, Sprite sprite, Material material)
        {
            var go = new GameObject(node.name);
            go.transform.SetParent(parent, false);
            go.layer = node.layer;
            go.SetActive(node.active);
            go.transform.localPosition = node.transform.m_LocalPosition;
            go.transform.localRotation = node.transform.m_LocalRotation;
            go.transform.localScale = node.transform.m_LocalScale;
            foreach (var component in node.components ?? Array.Empty<ComponentRecord>())
            {
                if (component.type == "Transform") continue;
                if (component.type != "SpriteRenderer")
                    throw new NotSupportedException("Unexpected background component: " + component.type);
                var data = component.data;
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sharedMaterial = material;
                renderer.sprite = sprite;
                renderer.enabled = data.m_Enabled;
                renderer.color = data.m_Color;
                renderer.flipX = data.m_FlipX;
                renderer.flipY = data.m_FlipY;
                renderer.sortingLayerID = data.m_SortingLayerID;
                renderer.sortingOrder = data.m_SortingOrder;
                renderer.drawMode = (SpriteDrawMode)data.m_DrawMode;
                renderer.size = data.m_Size;
                renderer.spriteSortPoint = (SpriteSortPoint)data.m_SpriteSortPoint;
                renderer.maskInteraction = (SpriteMaskInteraction)data.m_MaskInteraction;
                renderer.shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode)data.m_CastShadows;
                renderer.receiveShadows = data.m_ReceiveShadows != 0;
            }
            foreach (var child in node.children ?? Array.Empty<Node>()) BuildBackgroundNode(child, go.transform, sprite, material);
            return go;
        }

        static void PreserveSoldierData(Evidence evidence, string name)
        {
            string hierarchyPath = "generated/unity-assets/prefabs/" + name + ".hierarchy.json";
            var hierarchy = Read<Hierarchy>(hierarchyPath);
            CopySource(hierarchyPath, Destination + "/Data/" + name + ".hierarchy.json", hierarchy.root.id, "confirmed-source-only");
            foreach (var obj in hierarchy.dependencies)
            {
                string suffix = obj.type == "Mesh" ? obj.outputs.meshNative : obj.type == "Material" ? obj.outputs.typetree : null;
                if (string.IsNullOrEmpty(suffix)) continue;
                CopySource(suffix, Destination + "/Data/" + name + "/" + Path.GetFileName(suffix), obj.id, "confirmed-source-only");
            }
            report.soldierDataSets++;
        }

        static void ImportSprite(string source, string destination, Vector2 pivot, float ppu, Vector4 border,
            TextureSource sampling, string sourceId, string status)
        {
            if (ppu <= 0) throw new InvalidDataException("Invalid source sprite PPU: " + source);
            CopySource(source, destination, sourceId, status);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(destination) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Not a texture importer: " + destination);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spritePixelsPerUnit = ppu;
            settings.spriteBorder = border;
            importer.SetTextureSettings(settings);
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = sampling.m_ColorSpace == 1;
            importer.filterMode = (FilterMode)sampling.m_TextureSettings.m_FilterMode;
            importer.anisoLevel = sampling.m_TextureSettings.m_Aniso;
            importer.wrapModeU = (TextureWrapMode)sampling.m_TextureSettings.m_WrapU;
            importer.wrapModeV = (TextureWrapMode)sampling.m_TextureSettings.m_WrapV;
            importer.userData = ProvenanceJson(source, sourceId, status);
            importer.SaveAndReimport();
            var imported = AssetDatabase.LoadAssetAtPath<Sprite>(destination);
            if (imported == null || Mathf.Abs(imported.pixelsPerUnit - ppu) > .001f)
                throw new InvalidDataException("Sprite import validation failed: " + destination);
        }

        static T Read<T>(string relative) => JsonUtility.FromJson<T>(File.ReadAllText(SourcePath(relative)));

        static string SourcePath(string relative)
        {
            string full = Path.GetFullPath(Path.Combine(targetRoot, relative));
            if (!full.StartsWith(targetRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Source escapes authorized target: " + relative);
            if (!File.Exists(full)) throw new FileNotFoundException("Required recovered source is missing", full);
            return full;
        }

        static void CopySource(string source, string destination, string sourceId, string status)
        {
            string from = SourcePath(source);
            string to = Path.GetFullPath(Path.Combine(projectRoot, destination));
            string allowed = Path.GetFullPath(Path.Combine(projectRoot, Destination)) + Path.DirectorySeparatorChar;
            if (!to.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Invalid output path.");
            Directory.CreateDirectory(Path.GetDirectoryName(to));
            byte[] data = File.ReadAllBytes(from);
            if (!File.Exists(to) || !data.SequenceEqual(File.ReadAllBytes(to))) File.WriteAllBytes(to, data);
            report.assets.Add(new AssetResult { source = source, sourceIdentity = sourceId, sourceSha256 = Hash(data), assetPath = destination, status = status });
        }

        static void SetProvenance(string assetPath, string source, string identity, string status)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null) throw new InvalidDataException("Missing asset importer: " + assetPath);
            importer.userData = ProvenanceJson(source, identity, status);
            importer.SaveAndReimport();
        }

        static string ProvenanceJson(string source, string identity, string status) => JsonUtility.ToJson(new AssetResult
        {
            source = source, sourceIdentity = identity, sourceSha256 = Hash(File.ReadAllBytes(SourcePath(source))), status = status
        });

        static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || Path.GetFileName(name) != name || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidDataException("Invalid recovered asset name: " + name);
        }
        static void WriteReport()
        {
            string path = Path.Combine(workspaceRoot, "analysis/unity-asset-import-report.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        [Serializable] class Spec { public bool implementationReady; public Target target; }
        [Serializable] class Target { public string appId, version; }
        [Serializable] class Index { public TowerSprite[] towerSprites; }
        [Serializable] class TowerSprite { public string name, @object; public Vector2 pivot; public float pixelsToUnits; public Vector4 border; }
        [Serializable] class Evidence { public ObjectRecord[] objects; }
        [Serializable] class ObjectRecord { public string id, type, name, source; public Outputs outputs; }
        [Serializable] class Outputs { public string png, pngCanvas, font, textureRaw, typetree, meshNative; }
        [Serializable] class TextureSource { public int m_ColorSpace; public Sampling m_TextureSettings; }
        [Serializable] class Sampling { public int m_FilterMode, m_Aniso, m_WrapU, m_WrapV; }
        [Serializable] class Hierarchy { public Node root; public ObjectRecord[] dependencies; }
        [Serializable] class Node { public string id, name; public bool active; public int layer; public TransformData transform; public ComponentRecord[] components; public Node[] children; }
        [Serializable] class TransformData { public Vector3 m_LocalPosition, m_LocalScale; public Quaternion m_LocalRotation; }
        [Serializable] class ComponentRecord { public string type; public RendererData data; }
        [Serializable] class RendererData
        {
            public bool m_Enabled, m_FlipX, m_FlipY;
            public int m_SortingLayerID, m_SortingOrder, m_DrawMode, m_SpriteSortPoint, m_MaskInteraction, m_CastShadows, m_ReceiveShadows;
            public Color m_Color;
            public Vector2 m_Size;
        }
        [Serializable] class SceneSkins { public SceneSkin[] Datas; }
        [Serializable] class SceneSkin { public int id, prefabId; public string gameIconName; }
        [Serializable] class AssetResult { public string source, sourceIdentity, sourceSha256, assetPath, status; }
        [Serializable] class ImportReport
        {
            public string status, editorVersion, error, font, backgroundPrefab;
            public int towerSprites, soldierDataSets;
            public List<AssetResult> assets = new List<AssetResult>();
            public List<string> limitations = new List<string>();
        }
    }
}
