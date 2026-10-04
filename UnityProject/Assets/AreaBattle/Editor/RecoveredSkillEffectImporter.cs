using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class RecoveredSkillEffectImporter
    {
        static string Dest="Assets/AreaBattle/Resources/Recovered/SkillEffects";
        [Serializable] class Manifest {public Resource[] resources;public Prefab[] prefabs;public ShaderMap[] shaderMap;}
        [Serializable] class ShaderMap {public string sourceName,restoredName,path;}
        [Serializable] class Prefab {public string name,originalName,templatePath,sourceRoot;public SourceComponent[] components;}
        [Serializable] class SourceComponent {public string id,fileId,type,path;}
        [Serializable] class Settings {public int m_FilterMode,m_Aniso,m_WrapU,m_WrapV,m_WrapW;public float m_MipBias;}
        [Serializable] class Submesh {public int[] indices;}
        [Serializable] class MatrixRecord
        {
            public float m00,m01,m02,m03,m10,m11,m12,m13,m20,m21,m22,m23,m30,m31,m32,m33;
            public Matrix4x4 Value=>new Matrix4x4(new Vector4(m00,m10,m20,m30),new Vector4(m01,m11,m21,m31),new Vector4(m02,m12,m22,m32),new Vector4(m03,m13,m23,m33));
        }
        [Serializable] class BoneRecord
        {
            public int boneIndex0,boneIndex1,boneIndex2,boneIndex3;public float weight0,weight1,weight2,weight3;
            public BoneWeight Value=>new BoneWeight{boneIndex0=boneIndex0,boneIndex1=boneIndex1,boneIndex2=boneIndex2,boneIndex3=boneIndex3,weight0=weight0,weight1=weight1,weight2=weight2,weight3=weight3};
        }
        [Serializable] class TextureReference {public string name;public int resourceIndex;public Vector2 scale,offset;}
        [Serializable] class CurveKey {public float time,value,inTangent,outTangent;public bool step;public float[] coefficients;}
        [Serializable] class Curve {public string path,type,property;public CurveKey[] keys;}
        [Serializable] class Parameter {public string name;public int type,defaultInt;}
        [Serializable] class Condition {public string parameter;public int mode;public float threshold;}
        [Serializable] class Transition {public int destination,interruptionSource;public float duration,offset,exitTime;public bool hasExitTime,fixedDuration,orderedInterruption,canTransitionToSelf;public Condition[] conditions;}
        [Serializable] class State {public string name;public int clipIndex;public float speed,cycleOffset;public bool ikOnFeet,writeDefaults,mirror;public Transition[] transitions;}
        [Serializable] class Resource
        {
            public string id,type,name,path,sha256,sourceMesh,sourceSha256,templatePath,shaderName,restoredShaderName,fileId;
            public bool builtin,sRGB;public int index,mipCount,shaderIndex;public float ppu;public Vector2 pivot;public Vector4 border;public Settings settings;public TextureReference[] textures;
            public Vector3[] vertices,normals;public Vector2[] uv0,uv1;public Vector4[] tangents;public Color[] colors;public Submesh[] submeshes;public BoneRecord[] boneWeights;public MatrixRecord[] bindPoses;
            public Curve[] curves;public float duration,frameRate,layerWeight;public bool loopTime,ikPass;public string sourceTypetree,layerName;public int layerBlendingMode,defaultState;public Parameter[] parameters;public State[] states;
        }
        [Serializable] class Entry {public string name,prefab,sourceRoot;public int particleSystems,renderers,trails,animations,animators,skinnedRenderers;}
        [Serializable] class Report {public bool passed;public int prefabCount,resourceCount,packedCurves,curveSamples;public List<Entry> prefabs=new List<Entry>();public string scope="Native source components/curves/renderer fields; original Bullet code excluded. Custom shaders translated from retained original GLES. Current-engine mip generation is reported in source manifest.";}
        static string target,workspace;static Manifest manifest;static UnityEngine.Object[] imported;
        static int packedCurves,curveSamples;
        [MenuItem("AreaBattle/Import recovered skill effects")]
        public static void Import()
        {
            ImportManifest("prepared/native-import.json","unity-skill-effect-import-report.json");
            ImportManifest("prepared-embedded/native-import.json","unity-embedded-effect-import-report.json");
            ImportManifest("prepared-markers/native-import.json","unity-tower-marker-import-report.json");
        }
        public static void ImportTaskBoxEffect()=>ImportManifest("prepared/native-import.json","unity-task-box-effect-import-report.json","task-box-effect-20261003","Assets/AreaBattle/Resources/Recovered/TaskBoxEffect","task-box-effect-roundtrip");
        public static void ImportTopInfoEffects()=>ImportManifest("prepared/native-import.json","unity-top-info-effects-import-report.json","top-info-effects-20261003","Assets/AreaBattle/Resources/Recovered/TopInfoEffects","top-info-effects-roundtrip");
        static void ImportManifest(string manifestPath,string reportName,string snapshot="skill-effects-20260928",string destination="Assets/AreaBattle/Resources/Recovered/SkillEffects",string readbackDirectory="skill-effect-roundtrip")
        {
            Dest=destination;packedCurves=curveSamples=0;
            workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;
            target=Path.Combine(workspace,"analysis/targets/wxcf1394487200e48f/43");
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,"generated/resource-snapshots/"+snapshot+"/"+manifestPath)));
            imported=new UnityEngine.Object[manifest.resources.Length];
            foreach(string folder in new[]{Dest,Dest+"/Textures",Dest+"/Meshes",Dest+"/Materials",Dest+"/Animations",Dest+"/Shaders"})Directory.CreateDirectory(folder);
            foreach(var source in manifest.shaderMap)File.Copy(Path.Combine(target,source.path),Dest+"/Shaders/"+Path.GetFileName(source.path),true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(var r in manifest.resources)
            {
                if(r.builtin)continue;
                if(r.type=="Shader")
                {var shader=Shader.Find(r.restoredShaderName);if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidDataException("Original skill shader unavailable or invalid: "+r.shaderName);imported[r.index]=shader;}
                else if(r.type=="Texture2D"||r.type=="Sprite")ImportTexture(r);
                else if(r.type=="Mesh")ImportMesh(r);
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
            foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="AnimationClip"))
                if(r.curves!=null)ImportPackedClip(r);else ImportNativeResource(r,Dest+"/Animations/"+Safe(r.id)+".anim",typeof(AnimationClip));
            foreach(var r in manifest.resources.Where(x=>x.type=="AnimatorController"))ImportController(r);
            var report=new Report{resourceCount=manifest.resources.Length,prefabCount=manifest.prefabs.Length,packedCurves=packedCurves,curveSamples=curveSamples};
            foreach(var p in manifest.prefabs)
            {
                string path=Dest+"/"+p.originalName+".prefab";
                File.WriteAllText(path,ResolveTemplate(p.templatePath));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new InvalidDataException("Native skill prefab failed: "+p.originalName);
                var all=prefab.GetComponentsInChildren<Component>(true);if(all.Any(c=>c==null))throw new InvalidDataException("Missing component in skill prefab "+p.originalName);
                foreach(var source in p.components.Where(x=>x.type!="GameObject"))
                {
                    Type type=typeof(Transform).Assembly.GetType("UnityEngine."+source.type)??AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEngine."+source.type)).FirstOrDefault(t=>t!=null);
                    var t=string.IsNullOrEmpty(source.path)?prefab.transform:prefab.transform.Find(source.path);
                    if(t==null||type==null||t.GetComponent(type)==null)throw new InvalidDataException("Source component missing: "+p.originalName+"/"+source.path+" "+source.type);
                    string roundtrip=Path.Combine(workspace,"analysis/"+readbackDirectory,p.name);Directory.CreateDirectory(roundtrip);
                    File.WriteAllText(Path.Combine(roundtrip,source.fileId+".json"),EditorJsonUtility.ToJson(t.GetComponent(type),true));
                }
                int expected=p.components.Count(c=>c.type=="ParticleSystem");int actual=prefab.GetComponentsInChildren<ParticleSystem>(true).Length;
                if(expected!=actual)throw new InvalidDataException("Particle count changed for "+p.originalName);
                if(prefab.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)throw new InvalidDataException("Original custom executable component survived import");
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))foreach(var mat in renderer.sharedMaterials)if(mat!=null&&(mat.shader==null||mat.shader.name=="Hidden/InternalErrorShader"||ShaderUtil.ShaderHasError(mat.shader)))throw new InvalidDataException("Invalid material/shader in "+p.originalName);
                var meta=AssetImporter.GetAtPath(path);meta.userData=p.sourceRoot+" | source native component tree | original code excluded";meta.SaveAndReimport();
                report.prefabs.Add(new Entry{name=p.originalName,prefab=path,sourceRoot=p.sourceRoot,particleSystems=actual,renderers=prefab.GetComponentsInChildren<Renderer>(true).Length,trails=prefab.GetComponentsInChildren<TrailRenderer>(true).Length,animations=prefab.GetComponentsInChildren<Animation>(true).Length,animators=prefab.GetComponentsInChildren<Animator>(true).Length,skinnedRenderers=prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length});
            }
            AssetDatabase.SaveAssets();report.passed=true;File.WriteAllText(Path.Combine(workspace,"analysis/"+reportName),JsonUtility.ToJson(report,true));
        }
        static void ImportPackedClip(Resource r)
        {
            CheckHash(r.sourceTypetree,r.sourceSha256);string path=Dest+"/Animations/"+Safe(r.id)+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}else clip.ClearCurves();
            clip.name=r.name;clip.legacy=false;clip.frameRate=r.frameRate;
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.startTime=0;settings.stopTime=r.duration;settings.loopTime=r.loopTime;AnimationUtility.SetAnimationClipSettings(clip,settings);
            foreach(var source in r.curves)
            {
                Type type=source.type=="Transform"?typeof(Transform):source.type=="ParticleSystem"?typeof(ParticleSystem):source.type=="ParticleSystemRenderer"?typeof(ParticleSystemRenderer):null;
                if(type==null)throw new InvalidDataException("Unknown packed animation binding type "+source.type);
                var keys=source.keys.Select(k=>new Keyframe(k.time,k.value,k.inTangent,k.step?float.PositiveInfinity:k.outTangent)).ToArray();
                var curve=new AnimationCurve(keys);var binding=EditorCurveBinding.FloatCurve(source.path,type,source.property);AnimationUtility.SetEditorCurve(clip,binding,curve);
                var actual=AnimationUtility.GetEditorCurve(clip,binding);if(actual==null)throw new InvalidDataException("Packed clip binding missing "+source.property);
                for(int i=0;i<source.keys.Length-1;i++)for(int sample=0;sample<33;sample++)
                {
                    var k=source.keys[i];float sampleTime=k.time+(source.keys[i+1].time-k.time)*sample/33f;float dt=sampleTime-k.time;var c=k.coefficients;
                    float expected=k.step?k.value:((c[0]*dt+c[1])*dt+c[2])*dt+c[3];float observed=actual.Evaluate(sampleTime);
                    float magnitude=Mathf.Max(Mathf.Abs(expected),Mathf.Max(Mathf.Abs(k.value),Mathf.Abs(source.keys[i+1].value)));
                    if(Mathf.Abs(expected-observed)>Mathf.Max(.00002f,magnitude*.000002f))throw new InvalidDataException("Packed cubic roundtrip changed "+r.name+" "+source.path+" "+source.property+" at "+(k.time+dt)+" expected "+expected+" actual "+observed);
                    curveSamples++;
                }
                packedCurves++;
            }
            EditorUtility.SetDirty(clip);imported[r.index]=clip;
        }
        static void ImportController(Resource r)
        {
            string path=Dest+"/Animations/"+Safe(r.id)+".controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.layers=Array.Empty<AnimatorControllerLayer>();controller.parameters=Array.Empty<AnimatorControllerParameter>();
            foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(path))if(sub!=null&&sub!=controller)UnityEngine.Object.DestroyImmediate(sub,true);
            controller.name=r.name;
            foreach(var p in r.parameters)controller.AddParameter(new AnimatorControllerParameter{name=p.name,type=(AnimatorControllerParameterType)p.type,defaultInt=p.defaultInt});
            controller.AddLayer(r.layerName);var layers=controller.layers;var layer=layers[0];layer.defaultWeight=r.layerWeight;layer.blendingMode=(AnimatorLayerBlendingMode)r.layerBlendingMode;layer.iKPass=r.ikPass;controller.layers=layers;
            var machine=layer.stateMachine;var states=new AnimatorState[r.states.Length];
            for(int i=0;i<states.Length;i++)
            {
                var source=r.states[i];var state=machine.AddState(source.name);state.motion=(AnimationClip)imported[source.clipIndex];state.speed=source.speed;state.cycleOffset=source.cycleOffset;state.iKOnFeet=source.ikOnFeet;state.writeDefaultValues=source.writeDefaults;state.mirror=source.mirror;states[i]=state;
            }
            machine.defaultState=states[r.defaultState];
            for(int i=0;i<states.Length;i++)foreach(var source in r.states[i].transitions)
            {
                var transition=states[i].AddTransition(states[source.destination]);transition.duration=source.duration;transition.offset=source.offset;transition.exitTime=source.exitTime;transition.hasExitTime=source.hasExitTime;transition.hasFixedDuration=source.fixedDuration;transition.interruptionSource=(TransitionInterruptionSource)source.interruptionSource;transition.orderedInterruption=source.orderedInterruption;transition.canTransitionToSelf=source.canTransitionToSelf;
                foreach(var c in source.conditions)transition.AddCondition((AnimatorConditionMode)c.mode,c.threshold,c.parameter);
            }
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();imported[r.index]=controller;
        }
        static void ImportTexture(Resource r)
        {
            CheckHash(r.path,r.sha256);string path=Dest+"/Textures/"+Safe(r.id)+".png";File.Copy(Path.Combine(target,r.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
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
            if(r.boneWeights.Length>0)mesh.boneWeights=r.boneWeights.Select(b=>b.Value).ToArray();if(r.bindPoses.Length>0)mesh.bindposes=r.bindPoses.Select(b=>b.Value).ToArray();
            mesh.subMeshCount=r.submeshes.Length;for(int i=0;i<r.submeshes.Length;i++)mesh.SetTriangles(r.submeshes[i].indices,i,false);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);imported[r.index]=mesh;
        }
        static void ImportNativeResource(Resource r,string path,Type type)
        {File.WriteAllText(path,ResolveTemplate(r.templatePath));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);imported[r.index]=AssetDatabase.LoadAssetAtPath(path,type);if(imported[r.index]==null)throw new InvalidDataException("Native resource import failed "+r.name);}
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
                int referenceType=guid=="0000000000000000f000000000000000"||guid=="0000000000000000e000000000000000"?0:r.type=="Shader"||r.type=="Texture2D"||r.type=="Sprite"?3:2;
                string pattern="fileID: \\\"__FILEID_"+r.index+"__\\\"(?<a>\\s+)guid: \\\""+token+"\\\"(?<b>\\s+)type: [0-9]+";
                text=Regex.Replace(text,pattern,m=>"fileID: "+id.ToString(System.Globalization.CultureInfo.InvariantCulture)+m.Groups["a"].Value+"guid: \""+guid+"\""+m.Groups["b"].Value+"type: "+referenceType);
            }
            if(text.Contains("__GUID_")||text.Contains("__FILEID_"))throw new InvalidDataException("Unreplaced native asset pointer");return text;
        }
        static string Safe(string value)=>value.Replace(':','_').Replace('/','_');
        static void CheckHash(string path,string expected){using(var sha=SHA256.Create()){string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(target,path)))).Replace("-","").ToLowerInvariant();if(actual!=expected)throw new InvalidDataException("Skill resource hash changed: "+path);}}
    }
}
