using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AreaBattle.EditorTools
{
    public static class OutgameTipUiAnimationImport
    {
        [Serializable] sealed class Manifest {public bool enabled,playAutomatically,animatePhysics;public int wrapMode,cullingType;public string defaultClipId;public Source[] clips;}
        [Serializable] sealed class Source {public string sourceId,sourceJson,sha256;}
        [Serializable] sealed class ClipData {public string m_Name;public bool m_Legacy,m_Compressed;public float m_SampleRate;public int m_WrapMode;public ScaleCurve[] m_ScaleCurves;}
        [Serializable] sealed class ScaleCurve {public string path;public Curve curve;}
        [Serializable] sealed class Curve {public Key[] m_Curve;public int m_PreInfinity,m_PostInfinity;}
        [Serializable] sealed class Key {public float time;public Vector3 value,inSlope,outSlope,inWeight,outWeight;public int weightedMode;}
        [Serializable] sealed class Report {public bool passed;public int clips,curves,keys;public string error;}
        const string PrefabPath="Assets/AreaBattle/Resources/Recovered/FirstPack/TipUI/TipUI.prefab";
        public static void Run(){try{Attach();EditorApplication.Exit(0);}catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}}
        public static void Attach()
        {
            var report=new Report();GameObject root=null;
            try{
                var target=Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43");
                var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,"generated/outgame/tip-ui-animation-import.json")));
                string folder="Assets/AreaBattle/Resources/Recovered/FirstPack/TipUI/Animations";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
                var clips=new Dictionary<string,AnimationClip>();
                foreach(var source in manifest.clips){
                    var bytes=File.ReadAllBytes(Path.Combine(target,source.sourceJson));using(var sha=System.Security.Cryptography.SHA256.Create())Require(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()==source.sha256,"source clip hash");
                    var data=JsonUtility.FromJson<ClipData>(System.Text.Encoding.UTF8.GetString(bytes));Require(data.m_Legacy&&!data.m_Compressed,"legacy source clip");
                    var clip=new AnimationClip{name=data.m_Name,legacy=true,frameRate=data.m_SampleRate,wrapMode=(WrapMode)data.m_WrapMode};
                    foreach(var scale in data.m_ScaleCurves)for(int axis=0;axis<3;axis++){
                        var keys=new Keyframe[scale.curve.m_Curve.Length];for(int i=0;i<keys.Length;i++){var key=scale.curve.m_Curve[i];keys[i]=new Keyframe(key.time,key.value[axis],key.inSlope[axis],key.outSlope[axis],key.inWeight[axis],key.outWeight[axis]){weightedMode=(WeightedMode)key.weightedMode};}
                        var curve=new AnimationCurve(keys){preWrapMode=RecoveredUiAnimationImporter.InfinityMode(scale.curve.m_PreInfinity),postWrapMode=RecoveredUiAnimationImporter.InfinityMode(scale.curve.m_PostInfinity)};
                        var binding=EditorCurveBinding.FloatCurve(scale.path,typeof(Transform),"m_LocalScale."+"xyz"[axis]);AnimationUtility.SetEditorCurve(clip,binding,curve);
                        var actual=AnimationUtility.GetEditorCurve(clip,binding);Require(actual.length==keys.Length,"key count");
                        for(int i=0;i<keys.Length;i++)Require(actual.keys[i].Equals(keys[i]),"complete keyframe value/tangent/weight roundtrip");report.curves++;report.keys+=keys.Length;
                    }
                    var path=folder+"/"+data.m_Name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if(existing!=null){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;EditorUtility.SetDirty(clip);}else AssetDatabase.CreateAsset(clip,path);
                    clips.Add(source.sourceId,clip);report.clips++;
                }
                root=PrefabUtility.LoadPrefabContents(PrefabPath);var content=root.transform.Find("Content").gameObject;
                var animation=content.GetComponent<Animation>();if(animation==null)animation=content.AddComponent<Animation>();
                foreach(var clip in clips.Values){animation.RemoveClip(clip.name);animation.AddClip(clip,clip.name);}
                animation.clip=clips[manifest.defaultClipId];animation.enabled=manifest.enabled;animation.playAutomatically=manifest.playAutomatically;animation.wrapMode=(WrapMode)manifest.wrapMode;animation.animatePhysics=manifest.animatePhysics;animation.cullingType=(AnimationCullingType)manifest.cullingType;
                PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);AssetDatabase.SaveAssets();
                var saved=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).transform.Find("Content").GetComponent<Animation>();Require(saved!=null&&!saved.enabled&&saved.playAutomatically&&saved.GetClipCount()==2&&saved.clip==clips[manifest.defaultClipId],"native component references and original disabled state saved");report.passed=true;
            }catch(Exception ex){report.error=ex.ToString();throw;}
            finally{if(root!=null)PrefabUtility.UnloadPrefabContents(root);File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/tip-ui-animation-validation.json"),JsonUtility.ToJson(report,true));}
        }
        static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
    }
}
