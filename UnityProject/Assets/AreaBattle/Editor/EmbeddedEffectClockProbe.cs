using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class EmbeddedEffectClockProbe
    {
        [Serializable] public sealed class Sample {public string step,prefab,state;public float normalizedTime,alpha;public Vector3 scale,euler;public int loopingParticles;}
        [Serializable] public sealed class Result {public bool passed;public List<Sample> samples=new List<Sample>();public List<string> errors=new List<string>();}
        public static void Run()
        {
            var result=new Result();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            foreach(string name in new[]{"Hdzd_Effect_icicle","Hdzd_Effect_Wy_speedUp","Hdzd_Effect_Wy_speedDown"})
            {
                var source=Resources.Load<GameObject>("Recovered/SkillEffects/"+name);var obj=UnityEngine.Object.Instantiate(source);obj.SetActive(false);
                var animator=obj.GetComponentInChildren<Animator>(true);var visual=RecoveredEffectVisual.Attach(obj);bool ice=name=="Hdzd_Effect_icicle";
                if(animator==null)throw new InvalidDataException("Embedded source Animator missing "+name);
                Transform target=ice?obj.transform.Find("root/bing"):animator.transform.GetChild(0);
                Action<string> sample=step=>
                {
                    var st=obj.activeInHierarchy?animator.GetCurrentAnimatorStateInfo(0):default(AnimatorStateInfo);float alpha=0;var renderer=target.GetComponent<Renderer>();if(renderer!=null){var pb=new MaterialPropertyBlock();renderer.GetPropertyBlock(pb);alpha=pb.GetFloat("_Alpha");}
                    int loops=0;foreach(var ps in obj.GetComponentsInChildren<ParticleSystem>(true))if(ps.main.loop)loops++;
                    result.samples.Add(new Sample{prefab=name,step=step,scale=target.localScale,euler=target.localEulerAngles,normalizedTime=st.normalizedTime,state=st.fullPathHash.ToString(),alpha=alpha,loopingParticles=loops});
                };
                sample("inactive");obj.SetActive(true);if(ice)animator.SetInteger("skill",0);visual.Step(0);sample("activate0");
                if(animator.enabled)result.errors.Add(name+" automatic Animator tick was not disabled");
                for(int i=0;i<5;i++)visual.Step(.1f);sample("tick.5");
                var tick=result.samples[result.samples.Count-1];
                if(Mathf.Abs(tick.normalizedTime-(ice?1:.125f))>.00001f)result.errors.Add(name+" manual Animator time mismatch");
                if(ice&&(tick.scale!=Vector3.one||Mathf.Abs(tick.alpha-1)>.00001f||tick.loopingParticles!=8))result.errors.Add(name+" original grow curves not applied");
                if(ice)animator.SetInteger("skill",1);
                for(int i=0;i<20;i++)visual.Step(.1f);sample("melt2.0");
                var melt=result.samples[result.samples.Count-1];
                if(ice&&(melt.state!=unchecked((int)3286457299).ToString()||Mathf.Abs(melt.alpha)>.00001f||melt.loopingParticles!=2))result.errors.Add(name+" original disappear curves not applied");
                var before=animator.GetCurrentAnimatorStateInfo(0).normalizedTime;visual.Step(0);sample("pause0");
                if(before!=animator.GetCurrentAnimatorStateInfo(0).normalizedTime)result.errors.Add(name+" zero delta advanced Animator");
                obj.SetActive(false);visual.Step(.1f);sample("deactivate");obj.SetActive(true);if(ice)animator.SetInteger("skill",0);visual.Step(0);sample("reactivate0");
                if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime!=0)result.errors.Add(name+" reactivation did not reset source state clock");
                for(int i=0;i<5;i++)visual.Step(.1f);sample("reactivate.5");
                if(ice&&target.localScale!=Vector3.one)result.errors.Add(name+" grow did not reach original scale1");
                UnityEngine.Object.DestroyImmediate(obj);
            }
            result.passed=result.errors.Count==0;string workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;File.WriteAllText(Path.Combine(workspace,"analysis/embedded-effect-clock-probe.json"),JsonUtility.ToJson(result,true));
            if(!result.passed)throw new InvalidDataException(string.Join("; ",result.errors));Debug.Log("EMBEDDED_EFFECT_CLOCK_PROBE_PASS");
        }
    }
}
