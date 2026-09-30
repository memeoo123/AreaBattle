using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class EffectLifecycleAudit
    {
        [Serializable] sealed class Sample {public string label;public int state;public float normalizedTime;public List<string> nodes=new List<string>();}
        [Serializable] sealed class Report {public List<Sample> samples=new List<Sample>();}
        static Sample Inspect(GameObject root,string label)
        {
            var a=root.GetComponentInChildren<Animator>(true);var state=a.GetCurrentAnimatorStateInfo(0);
            var s=new Sample{label=label,state=state.fullPathHash,normalizedTime=state.normalizedTime};
            foreach(var t in root.GetComponentsInChildren<Transform>(true))s.nodes.Add(t.name+" active="+t.gameObject.activeSelf+" scale="+t.localScale.ToString("F5"));
            return s;
        }
        public static void Run()
        {
            var objects=new List<GameObject>();
            try
            {
                var prefab=Resources.Load<GameObject>("Recovered/SkillEffects/Hdzd_Effect_icicle");
                var obj=UnityEngine.Object.Instantiate(prefab);objects.Add(obj);obj.SetActive(true);
                var clock=RecoveredEffectVisual.Attach(obj);var animator=obj.GetComponentInChildren<Animator>(true);
                var report=new Report();animator.SetInteger("skill",0);
                for(int i=0;i<18;i++)clock.Step(1f/60);
                report.samples.Add(Inspect(obj,"first-cast-0.3s"));
                animator.SetInteger("skill",1);for(int i=0;i<120;i++)clock.Step(1f/60);
                report.samples.Add(Inspect(obj,"after-melt-request-2s"));
                obj.SetActive(false);clock.Step(0);obj.SetActive(true);animator.SetInteger("skill",0);
                for(int i=0;i<18;i++)clock.Step(1f/60);
                report.samples.Add(Inspect(obj,"reuse-after-disable-0.3s"));
                var fresh=UnityEngine.Object.Instantiate(prefab);objects.Add(fresh);fresh.SetActive(true);
                var freshClock=RecoveredEffectVisual.Attach(fresh);fresh.GetComponentInChildren<Animator>(true).SetInteger("skill",0);
                for(int i=0;i<18;i++)freshClock.Step(1f/60);
                report.samples.Add(Inspect(fresh,"fresh-control-0.3s"));
                File.WriteAllText(Path.Combine(BattleBuild.Target,"generated/video-20260928/effect-lifecycle-audit.json"),JsonUtility.ToJson(report,true));
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{foreach(var obj in objects)if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
        }
    }
}

