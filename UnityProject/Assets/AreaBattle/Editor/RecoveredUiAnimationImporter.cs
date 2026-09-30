using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle.EditorTools
{
    public static class RecoveredUiAnimationImporter
    {
        [Serializable] class Manifest {public SourceAnimation[] animations;public Clip[] clips;}
        [Serializable] class SourceAnimation {public string panel,path,sourceId,defaultClipId;public string[] clipIds;public bool playAutomatically,animatePhysics;public int wrapMode,cullingType;}
        [Serializable] class Clip {public string sourceId,name,sourceJson,sourceSha256;public bool legacy,compressed;public float sampleRate,duration;public int wrapMode,compressedRotationCount;public Curve[] curves;}
        [Serializable] class Curve {public string path,component,property;public int preInfinity,postInfinity;public Key[] keys;}
        [Serializable] class Key {public float time,value,inTangent,outTangent,inWeight,outWeight;public int weightedMode;public string inTangentSpecial,outTangentSpecial,inWeightSpecial,outWeightSpecial;}
        [Serializable] class Report {public bool passed;public int animationComponents,clips,retainedCurves,verifiedKeyframes,productionSamplerChecks;public List<string> skippedPaths=new List<string>();public string clock="BattleHud.AdvancePresentation(scaledDelta); no implicit Update clock, no secondary-clip chaining";}
        static Manifest manifest;
        static string target,workspace;
        static Report report;
        static readonly Dictionary<int,WrapMode> infinityModes=new Dictionary<int,WrapMode>();
        const string Destination="Assets/AreaBattle/Resources/Recovered/Hud/Animations";
        public static void Begin(string sourceRoot,string workspaceRoot)
        {
            target=sourceRoot;workspace=workspaceRoot;
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,"generated/presentation-prepared/ui-result-animations.json")));
            report=new Report();infinityModes.Clear();Directory.CreateDirectory(Destination);AssetDatabase.Refresh();
        }
        public static void AddToPrefab(string panel,GameObject root)
        {
            foreach(var source in manifest.animations.Where(x=>x.panel==panel))
            {
                var node=Find(root.transform,source.path);if(node==null){report.skippedPaths.Add(panel+" component node "+source.path);continue;}
                var clips=new Dictionary<string,AnimationClip>();
                foreach(string id in source.clipIds)
                {
                    var data=manifest.clips.Single(x=>x.sourceId==id);VerifySource(data);
                    if(!data.legacy || data.compressed || data.compressedRotationCount!=0)throw new InvalidDataException("Unsupported source UI clip encoding: "+data.name);
                    var clip=new AnimationClip{name=data.name,legacy=true,frameRate=data.sampleRate,wrapMode=(WrapMode)data.wrapMode};
                    var retained=new List<Curve>();
                    foreach(var curve in data.curves)
                    {
                        var destination=Find(node,curve.path);
                        if(destination==null){report.skippedPaths.Add(panel+" / "+source.path+" / "+data.name+" : "+curve.path+"."+curve.property);continue;}
                        Type type=ComponentType(curve.component);
                        if(type!=typeof(GameObject) && destination.GetComponent(type)==null)throw new InvalidDataException("Retained UI curve has missing component: "+curve.path+" "+curve.component);
                        var keys=curve.keys.Select(k=>new Keyframe(k.time,k.value,Special(k.inTangent,k.inTangentSpecial),Special(k.outTangent,k.outTangentSpecial),Special(k.inWeight,k.inWeightSpecial),Special(k.outWeight,k.outWeightSpecial)){weightedMode=(WeightedMode)k.weightedMode}).ToArray();
                        var native=new AnimationCurve(keys){preWrapMode=InfinityMode(curve.preInfinity),postWrapMode=InfinityMode(curve.postInfinity)};
                        clip.SetCurve(curve.path,type,curve.property,native);retained.Add(curve);report.retainedCurves++;
                    }
                    string path=Destination+"/"+panel+"_"+source.sourceId.Replace(':','_')+"_"+data.sourceId.Replace(':','_')+".anim";
                    var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if(existing!=null){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;EditorUtility.SetDirty(clip);}else AssetDatabase.CreateAsset(clip,path);
                    foreach(var curve in retained)
                    {
                        var native=AnimationUtility.GetEditorCurve(clip,EditorCurveBinding.FloatCurve(curve.path,ComponentType(curve.component),curve.property));
                        if(native==null || native.length!=curve.keys.Length)throw new InvalidDataException("UI animation curve roundtrip failed: "+curve.path+"."+curve.property);
                        for(int i=0;i<native.length;i++){var actual=native.keys[i];var expected=curve.keys[i];if(actual.time!=expected.time||actual.value!=expected.value||actual.inTangent!=Special(expected.inTangent,expected.inTangentSpecial)||actual.outTangent!=Special(expected.outTangent,expected.outTangentSpecial))throw new InvalidDataException("UI animation keyframe changed during import: "+data.name);report.verifiedKeyframes++;}
                    }
                    var importer=AssetImporter.GetAtPath(path);importer.userData=data.sourceId+" | source SHA256 "+data.sourceSha256;clips.Add(id,clip);report.clips++;
                }
                var player=node.gameObject.AddComponent<RecoveredUiAnimation>();var original=manifest.clips.Single(x=>x.sourceId==source.defaultClipId);
                player.DefaultClip=clips[source.defaultClipId];player.SourceId=source.sourceId;player.SourceDuration=original.duration;player.SourcePlayAutomatically=source.playAutomatically;player.SourceComponentWrapMode=(WrapMode)source.wrapMode;player.SourceAnimatePhysics=source.animatePhysics;player.SourceCullingType=source.cullingType;report.animationComponents++;
            }
        }
        public static void Finish()
        {
            AssetDatabase.SaveAssets();
            foreach(string panel in manifest.animations.Select(x=>x.panel).Distinct())
            {
                var instance=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AreaBattle/Resources/Recovered/Hud/"+panel+".prefab"));
                try
                {
                    instance.SetActive(true);var player=instance.GetComponent<RecoveredUiAnimation>();player.ResetPlayback();player.Advance(.1f);
                    var data=manifest.clips.Single(c=>c.sourceId==manifest.animations.Single(a=>a.panel==panel&&a.path=="").defaultClipId);
                    var curve=data.curves.First(c=>c.component!="GameObject"&&Find(instance.transform,c.path)!=null);
                    var node=Find(instance.transform,curve.path);var component=node.GetComponent(ComponentType(curve.component));
                    var property=new SerializedObject(component).FindProperty(curve.property);
                    var native=AnimationUtility.GetEditorCurve(player.DefaultClip,EditorCurveBinding.FloatCurve(curve.path,ComponentType(curve.component),curve.property));
                    if(property==null||Mathf.Abs(property.floatValue-native.Evaluate(.1f))>.00001f||Mathf.Abs(player.SampleTime-.1f)>.00001f)throw new InvalidDataException("Production UI animation sampler failed: "+panel);
                    player.Advance(0);if(Mathf.Abs(player.SampleTime-.1f)>.00001f)throw new InvalidDataException("Zero scaled delta advanced UI animation");report.productionSamplerChecks+=2;
                }
                finally{UnityEngine.Object.DestroyImmediate(instance);}
            }
            report.passed=true;File.WriteAllText(Path.Combine(workspace,"analysis/unity-ui-animation-import-report.json"),JsonUtility.ToJson(report,true));
        }
        static Transform Find(Transform root,string path)=>string.IsNullOrEmpty(path)?root:root.Find(path);
        static float Special(float value,string special)=>string.IsNullOrEmpty(special)?value:special=="Infinity"?float.PositiveInfinity:special=="-Infinity"?float.NegativeInfinity:float.NaN;
        static Type ComponentType(string name)
        {switch(name){case "GameObject":return typeof(GameObject);case "Transform":return typeof(Transform);case "RectTransform":return typeof(RectTransform);case "Image":return typeof(Image);case "Text":return typeof(Text);default:throw new InvalidDataException("Unsupported original curve component "+name);}}
        internal static WrapMode InfinityMode(int original)
        {
            if(infinityModes.TryGetValue(original,out var mode))return mode;
            // Serialized curve infinity values are not the public WrapMode enum. Ask this
            // installed Unity serializer for the exact mapping instead of casting them.
            var probe=new AnimationClip{legacy=true};
            try
            {
                foreach(var candidate in new[]{WrapMode.ClampForever,WrapMode.Once,WrapMode.Loop,WrapMode.PingPong,WrapMode.Default})
                {
                    probe.SetCurve("",typeof(Transform),"m_LocalPosition.x",new AnimationCurve(new Keyframe(0,0),new Keyframe(1,1)){preWrapMode=candidate,postWrapMode=candidate});
                    string serialized=EditorJsonUtility.ToJson(probe);
                    if(Regex.IsMatch(serialized,"\"m_PreInfinity\"\\s*:\\s*"+original+"(?:,|})")){infinityModes.Add(original,candidate);return candidate;}
                }
                throw new InvalidDataException("Cannot verify original curve infinity mode "+original+" with installed Unity serializer");
            }
            finally{UnityEngine.Object.DestroyImmediate(probe);}
        }
        static void VerifySource(Clip clip)
        {using(var sha=SHA256.Create()){string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(target,clip.sourceJson)))).Replace("-","").ToLowerInvariant();if(actual!=clip.sourceSha256)throw new InvalidDataException("UI animation source hash mismatch: "+clip.name);}}
    }
}
