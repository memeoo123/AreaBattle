using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskPanelAnimationImport
    {
        [Serializable] sealed class Manifest {public Source[] animations;public Clip[] clips;}
        [Serializable] sealed class Source {public string path,sourceId,defaultClipId;public string[] clipIds;public bool enabled,playAutomatically,animatePhysics;public int wrapMode,cullingType;}
        [Serializable] sealed class Clip {public string sourceId,name,sourceJson,sourceSha256;public bool legacy,compressed;public int compressedRotationCount,wrapMode;public float sampleRate;public Curve[] curves;}
        [Serializable] sealed class Curve {public string path,component,property;public int preInfinity,postInfinity;public Key[] keys;}
        [Serializable] sealed class Key {public float time,value,inTangent,outTangent,inWeight,outWeight;public int weightedMode;public string inTangentSpecial,outTangentSpecial,inWeightSpecial,outWeightSpecial;}
        [Serializable] sealed class Report {public bool passed;public int clips,curves,keys,components;public List<string> unboundSourcePaths=new List<string>();public string error;public string limitation="Original two legacy clips and six native Animation components restored. Original box clip contains hdzd_eff_bxGlow binding absent from source task-panel hierarchy; retained as unbound source track. ParticleSystems and EffectModule1016 still require production restoration; no full audiovisual-match claim.";}
        const string Prefab="Assets/AreaBattle/Resources/Recovered/TaskPanel/TaskPanelUI.prefab";
        public static void Run(){try{Attach();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        public static void Attach()
        {
            var report=new Report();GameObject root=null;
            try
            {
                string target=Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43");var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,"generated/outgame/task-panel-animations.json")));
                string folder="Assets/AreaBattle/Resources/Recovered/TaskPanel/Animations";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var clips=new Dictionary<string,AnimationClip>();
                foreach(var data in manifest.clips)
                {
                    var bytes=File.ReadAllBytes(Path.Combine(target,data.sourceJson));using(var sha=System.Security.Cryptography.SHA256.Create())Require(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()==data.sourceSha256,"source clip hash");
                    Require(data.legacy&&!data.compressed&&data.compressedRotationCount==0,"supported original legacy encoding");
                    var clip=new AnimationClip{name=data.name,legacy=true,frameRate=data.sampleRate,wrapMode=(WrapMode)data.wrapMode};
                    foreach(var curve in data.curves)
                    {
                        var keys=curve.keys.Select(k=>new Keyframe(k.time,k.value,Special(k.inTangent,k.inTangentSpecial),Special(k.outTangent,k.outTangentSpecial),Special(k.inWeight,k.inWeightSpecial),Special(k.outWeight,k.outWeightSpecial)){weightedMode=(WeightedMode)k.weightedMode}).ToArray();
                        var binding=EditorCurveBinding.FloatCurve(curve.path,Type(curve.component),curve.property);
                        AnimationUtility.SetEditorCurve(clip,binding,new AnimationCurve(keys){preWrapMode=RecoveredUiAnimationImporter.InfinityMode(curve.preInfinity),postWrapMode=RecoveredUiAnimationImporter.InfinityMode(curve.postInfinity)});
                        var actual=AnimationUtility.GetEditorCurve(clip,binding);Require(actual.length==keys.Length,"original key count");for(int i=0;i<keys.Length;i++)Require(actual.keys[i].Equals(keys[i]),"original key/tangent/weight preserved");report.curves++;report.keys+=keys.Length;
                    }
                    string path=folder+"/"+clip.name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if(existing){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;EditorUtility.SetDirty(clip);}else AssetDatabase.CreateAsset(clip,path);
                    clips.Add(data.sourceId,clip);report.clips++;
                }
                root=PrefabUtility.LoadPrefabContents(Prefab);
                foreach(var source in manifest.animations)
                {
                    var node=root.transform.Find(source.path);Require(node,"original animation owner");var animation=node.GetComponent<Animation>();if(!animation)animation=node.gameObject.AddComponent<Animation>();
                    foreach(var id in source.clipIds)
                    {
                        var clip=clips[id];animation.RemoveClip(clip.name);animation.AddClip(clip,clip.name);
                        foreach(var curve in manifest.clips.Single(c=>c.sourceId==id).curves)
                            if(!string.IsNullOrEmpty(curve.path)&&node.Find(curve.path)==null)report.unboundSourcePaths.Add(source.path+" | "+curve.path+" | "+curve.property);
                    }
                    animation.clip=clips[source.defaultClipId];animation.enabled=source.enabled;animation.playAutomatically=source.playAutomatically;animation.animatePhysics=source.animatePhysics;animation.wrapMode=(WrapMode)source.wrapMode;animation.cullingType=(AnimationCullingType)source.cullingType;report.components++;
                }
                PrefabUtility.SaveAsPrefabAsset(root,Prefab);AssetDatabase.SaveAssets();Require(report.clips==2&&report.components==6&&report.curves==18&&report.keys==119,"complete original task animation inventory");report.passed=true;
            }
            catch(Exception e){report.error=e.ToString();throw;}
            finally{if(root)PrefabUtility.UnloadPrefabContents(root);File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-panel-animation-import-report.json"),JsonUtility.ToJson(report,true));}
        }
        static Type Type(string component){switch(component){case "Transform":return typeof(Transform);case "RectTransform":return typeof(RectTransform);case "GameObject":return typeof(GameObject);default:throw new InvalidDataException(component);}}
        static float Special(float value,string special)=>string.IsNullOrEmpty(special)?value:special=="Infinity"?float.PositiveInfinity:special=="-Infinity"?float.NegativeInfinity:float.NaN;
        static void Require(bool value,string why){if(!value)throw new InvalidDataException(why);}
    }
}
