using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Spine.Unity;

namespace AreaBattle.EditorTools
{
    public static class RecoveredBossImporter
    {
        static string Dest="Assets/AreaBattle/Resources/Recovered/Bosses";
        [Serializable] class Manifest {public Resource[] resources;public Prefab[] prefabs;public ShaderMap[] shaderMap;}
        [Serializable] class ShaderMap {public string sourceName,restoredName,path;}
        [Serializable] class Prefab {public string name,originalName,templatePath,sourceRoot;public SourceComponent[] components;}
        [Serializable] class SourceComponent {public string id,fileId,type,path;}
        [Serializable] class Settings {public int m_FilterMode,m_Aniso,m_WrapU,m_WrapV,m_WrapW;public float m_MipBias;}
        [Serializable] class Submesh {public int[] indices;}
        [Serializable] class TextureReference {public string name;public int resourceIndex;public Vector2 scale,offset;}
        [Serializable] class Resource
        {
            public string id,type,name,path,sha256,sourceMesh,sourceSha256,templatePath,shaderName,restoredShaderName,fileId,scriptPath,scriptClass;
            public bool builtin,sRGB;public int index,mipCount,shaderIndex;public float ppu;public Vector2 pivot;public Vector4 border;public Settings settings;public TextureReference[] textures;
            public Vector3[] vertices,normals;public Vector2[] uv0,uv1;public Vector4[] tangents;public Color[] colors;public Submesh[] submeshes;public BoneWeight[] boneWeights;public Matrix4x4[] bindPoses;
        }
        [Serializable] class Entry {public string name,prefab,sourceRoot;public int particleSystems,renderers,trails,animations,skinnedRenderers;}
        [Serializable] class Report {public bool passed;public int prefabCount,resourceCount;public List<Entry> prefabs=new List<Entry>();public string scope="Manifest-listed original prefabs, native source components and Spine 4.1.16 skeleton/atlas data. Pinned official Spine 4.1 runtime; original game executable components excluded. Frame comparison remains pending.";}
        static string target,workspace;static Manifest manifest;static UnityEngine.Object[] imported;
        [MenuItem("AreaBattle/Import recovered Boss Spine models")]
        public static void Import()
        {
            ImportFrom("generated/resource-snapshots/boss-entities-20260928/prepared/native-import.json","Assets/AreaBattle/Resources/Recovered/Bosses","unity-boss-import-report.json");
        }
        public static void ImportOutgameScenesBatch()
        {
            try { ImportFrom("generated/resource-snapshots/outgame-scenes-20260929/prepared/native-import.json","Assets/AreaBattle/Resources/Recovered/Outgame/SceneEffects","outgame-scene-effects-import-report.json"); EditorApplication.Exit(0); }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        static void ImportFrom(string manifestPath,string destination,string reportName)
        {
            Dest=destination;
            workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;
            target=Path.Combine(workspace,"analysis/targets/wxcf1394487200e48f/43");
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,manifestPath)));
            imported=new UnityEngine.Object[manifest.resources.Length];
            foreach(string folder in new[]{Dest,Dest+"/Textures",Dest+"/Meshes",Dest+"/Materials",Dest+"/Animations",Dest+"/Shaders",Dest+"/Spine"})Directory.CreateDirectory(folder);
            foreach(var source in manifest.shaderMap)File.Copy(Path.Combine(target,source.path),Dest+"/Shaders/"+Path.GetFileName(source.path),true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(var r in manifest.resources)
            {
                if(r.builtin)continue;
                if(r.type=="Shader")
                {var shader=Shader.Find(r.restoredShaderName);if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidDataException("Original skill shader unavailable or invalid: "+r.shaderName);imported[r.index]=shader;}
                else if(r.type=="Texture2D"||r.type=="Sprite")ImportTexture(r);
                else if(r.type=="Mesh")ImportMesh(r);
                else if(r.type=="MonoScript") { imported[r.index]=AssetDatabase.LoadAssetAtPath<MonoScript>(r.scriptPath);if(imported[r.index]==null)throw new InvalidDataException("Missing official Spine script "+r.scriptPath); }
                else if(r.type=="TextAsset") {CheckHash(r.path,r.sha256);string path=Dest+"/Spine/"+Safe(r.id)+".txt";File.Copy(Path.Combine(target,r.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);imported[r.index]=AssetDatabase.LoadAssetAtPath<TextAsset>(path);}
            }
            AssetDatabase.SaveAssets();
            foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="Material"))ImportNativeResource(r,Dest+"/Materials/"+Safe(r.id)+".mat",typeof(Material));
            foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="Material"))
            {
                var material=(Material)imported[r.index];var expected=manifest.resources[r.shaderIndex];
                if(material.shader==null||material.shader.name=="Hidden/InternalErrorShader"||ShaderUtil.ShaderHasError(material.shader))throw new InvalidDataException("Invalid native material shader: "+r.name);
                if(!expected.builtin && material.shader.name!=expected.restoredShaderName)throw new InvalidDataException("Native shader binding mismatch: "+r.name+" expected "+expected.restoredShaderName+" actual "+material.shader.name);
                if(expected.builtin && (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material.shader,out string shaderGuid,out long shaderId)||shaderId.ToString()!=expected.fileId))throw new InvalidDataException("Builtin shader identity changed: "+r.name);
                foreach(var texture in r.textures.Where(t=>t.resourceIndex>=0))
                    if(material.HasProperty(texture.name) && !manifest.resources[texture.resourceIndex].builtin && material.GetTexture(texture.name)!=imported[texture.resourceIndex])throw new InvalidDataException("Native material texture binding mismatch: "+r.name+" "+texture.name);
            }
            foreach(string script in new[]{"SpineAtlasAsset","SkeletonDataAsset"})
                foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="MonoBehaviour"&&x.scriptClass==script))
                    ImportNativeResource(r,Dest+"/Spine/"+Safe(r.id)+".asset",script=="SpineAtlasAsset"?typeof(SpineAtlasAsset):typeof(SkeletonDataAsset));
            foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="AnimationClip"))ImportNativeResource(r,Dest+"/Animations/"+Safe(r.id)+".anim",typeof(AnimationClip));
            var report=new Report{resourceCount=manifest.resources.Length,prefabCount=manifest.prefabs.Length};
            foreach(var p in manifest.prefabs)
            {
                string path=Dest+"/"+p.originalName+".prefab";
                File.WriteAllText(path,ResolveTemplate(p.templatePath));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new InvalidDataException("Native skill prefab failed: "+p.originalName);
                var all=prefab.GetComponentsInChildren<Component>(true);if(all.Any(c=>c==null))throw new InvalidDataException("Missing component in skill prefab "+p.originalName);
                foreach(var source in p.components.Where(x=>x.type!="GameObject"))
                {
                    Type type=source.type=="MonoBehaviour"?typeof(SkeletonAnimation):typeof(Transform).Assembly.GetType("UnityEngine."+source.type)??AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEngine."+source.type)).FirstOrDefault(t=>t!=null);
                    var t=string.IsNullOrEmpty(source.path)?prefab.transform:prefab.transform.Find(source.path);
                    if(t==null||type==null||t.GetComponent(type)==null)throw new InvalidDataException("Source component missing: "+p.originalName+"/"+source.path+" "+source.type);
                    string roundtrip=Path.Combine(workspace,"analysis/boss-roundtrip",p.name);Directory.CreateDirectory(roundtrip);
                    File.WriteAllText(Path.Combine(roundtrip,source.fileId+".json"),EditorJsonUtility.ToJson(t.GetComponent(type),true));
                }
                int expected=p.components.Count(c=>c.type=="ParticleSystem");int actual=prefab.GetComponentsInChildren<ParticleSystem>(true).Length;
                if(expected!=actual)throw new InvalidDataException("Particle count changed for "+p.originalName);
                if(prefab.GetComponentsInChildren<MonoBehaviour>(true).Any(x=>!(x is SkeletonAnimation)))throw new InvalidDataException("Original custom executable component survived import");
                foreach(var skeleton in prefab.GetComponentsInChildren<SkeletonAnimation>(true)) {
                    foreach(var atlas in skeleton.skeletonDataAsset.atlasAssets)foreach(var page in atlas.GetAtlas().Pages)if(page.rendererObject==null)throw new InvalidDataException("Spine atlas page has no original material "+page.name);var data=skeleton.skeletonDataAsset.GetSkeletonData(false);
                    if(data==null||data.Version!="4.1.16"||data.FindAnimation(skeleton.AnimationName)==null)throw new InvalidDataException("Original Spine data unavailable "+p.originalName);
                }
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))foreach(var mat in renderer.sharedMaterials)if(mat!=null&&(mat.shader==null||mat.shader.name=="Hidden/InternalErrorShader"||ShaderUtil.ShaderHasError(mat.shader)))throw new InvalidDataException("Invalid material/shader in "+p.originalName);
                var meta=AssetImporter.GetAtPath(path);meta.userData=p.sourceRoot+" | source native component tree | original game code excluded; official Spine runtime bound";meta.SaveAndReimport();
                report.prefabs.Add(new Entry{name=p.originalName,prefab=path,sourceRoot=p.sourceRoot,particleSystems=actual,renderers=prefab.GetComponentsInChildren<Renderer>(true).Length,trails=prefab.GetComponentsInChildren<TrailRenderer>(true).Length,animations=prefab.GetComponentsInChildren<Animation>(true).Length,skinnedRenderers=prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length});
            }
            AssetDatabase.SaveAssets();report.passed=true;File.WriteAllText(Path.Combine(workspace,"analysis",reportName),JsonUtility.ToJson(report,true));
        }
        static void ImportTexture(Resource r)
        {
            CheckHash(r.path,r.sha256);string folder=Dest+"/Textures/"+Safe(r.id);Directory.CreateDirectory(folder);string path=folder+"/"+r.name+".png";File.Copy(Path.Combine(target,r.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;importer.alphaIsTransparency=false;
            if(r.type=="Sprite")
            {importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=r.ppu;importer.spriteBorder=r.border;var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=r.pivot;importer.SetTextureSettings(settings);importer.mipmapEnabled=false;importer.sRGBTexture=true;}
            else
            {importer.textureType=TextureImporterType.Default;importer.sRGBTexture=r.sRGB;importer.mipmapEnabled=r.mipCount>1;importer.filterMode=(FilterMode)r.settings.m_FilterMode;importer.anisoLevel=r.settings.m_Aniso;importer.mipMapBias=r.settings.m_MipBias;importer.wrapModeU=(TextureWrapMode)r.settings.m_WrapU;importer.wrapModeV=(TextureWrapMode)r.settings.m_WrapV;importer.wrapModeW=(TextureWrapMode)r.settings.m_WrapW;}
            importer.userData=r.id+" | source SHA256 "+r.sha256;importer.SaveAndReimport();imported[r.index]=AssetDatabase.LoadAssetAtPath(path,r.type=="Sprite"?typeof(Sprite):typeof(Texture2D));
        }
        static void ImportMesh(Resource r)
        {
            CheckHash(r.sourceMesh,r.sourceSha256);string path=Dest+"/Meshes/"+Safe(r.id)+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh{name=r.name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.indexFormat=r.vertices.Length>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16;mesh.vertices=r.vertices;
            if(r.normals.Length>0)mesh.normals=r.normals;if(r.uv0.Length>0)mesh.uv=r.uv0;if(r.uv1.Length>0)mesh.uv2=r.uv1;if(r.colors.Length>0)mesh.colors=r.colors;if(r.tangents.Length>0)mesh.tangents=r.tangents;
            if(r.boneWeights.Length>0)mesh.boneWeights=r.boneWeights;if(r.bindPoses.Length>0)mesh.bindposes=r.bindPoses;
            mesh.subMeshCount=r.submeshes.Length;for(int i=0;i<r.submeshes.Length;i++)mesh.SetTriangles(r.submeshes[i].indices,i,false);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);imported[r.index]=mesh;
        }
        static void ImportNativeResource(Resource r,string path,Type type)
        {
            File.WriteAllText(path,ResolveTemplate(r.templatePath));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);imported[r.index]=AssetDatabase.LoadAssetAtPath(path,type);if(imported[r.index]==null)throw new InvalidDataException("Native resource import failed "+r.name);
            if(typeof(ScriptableObject).IsAssignableFrom(type)){
                var native=imported[r.index];var persistent=ScriptableObject.CreateInstance(type);EditorUtility.CopySerialized(native,persistent);
                AssetDatabase.CreateAsset(persistent,path);AssetDatabase.SaveAssets();imported[r.index]=persistent;
            }
        }
        static string ResolveTemplate(string source)
        {
            string text=File.ReadAllText(Path.Combine(target,source));
            foreach(var r in manifest.resources)
            {
                string token="__GUID_"+r.index+"__";if(!text.Contains(token))continue;
                if(imported[r.index]==null||!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(imported[r.index],out string guid,out long id))throw new InvalidDataException("Unresolved native skill dependency "+r.id);
                // Shader.Find can resolve a bundled original shader to Unity's built-in
                // resource. Its actual GUID determines native pointer type, not the
                // original object's serialized-file location.
                int referenceType=guid=="0000000000000000f000000000000000"||guid=="0000000000000000e000000000000000"?0:r.type=="Shader"||r.type=="Texture2D"||r.type=="Sprite"||r.type=="MonoScript"||r.type=="TextAsset"?3:2;
                string pattern="fileID: \\\"__FILEID_"+r.index+"__\\\"(?<a>\\s+)guid: \\\""+token+"\\\"(?<b>\\s+)type: [0-9]+";
                text=Regex.Replace(text,pattern,m=>"fileID: "+id.ToString(System.Globalization.CultureInfo.InvariantCulture)+m.Groups["a"].Value+"guid: \""+guid+"\""+m.Groups["b"].Value+"type: "+referenceType);
            }
            if(text.Contains("__GUID_")||text.Contains("__FILEID_"))throw new InvalidDataException("Unreplaced native asset pointer");return text;
        }
        static string Safe(string value)=>value.Replace(':','_').Replace('/','_');
        static void CheckHash(string path,string expected){using(var sha=SHA256.Create()){string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(target,path)))).Replace("-","").ToLowerInvariant();if(actual!=expected)throw new InvalidDataException("Skill resource hash changed: "+path);}}
    }
}

