using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameEffectModuleValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameEffectModulePlayModeValidation
    {
        const string Pending="AreaBattle.EffectModuleNative";
        [Serializable] sealed class Config{public List<OutgameEffectData> Datas;}
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Recovered EffectModule/Base/UI/Fly/Line, actual Unity WaitUntil and scaled WaitForSeconds, original EffectConfig1016 and real OutgameAssetHandle/provider release, native tween and deferred destruction. Explicit asynchronous provider and fixture prefab; original1016 particles/prefab, production resource loading/EffectControl/Main and audiovisual/Player acceptance are not claimed.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static OutgameBaseEffect effect,late;static GameObject closed;static int phase,frame,baseReleases;static double began,phaseAt;static bool stopped;
        static OutgameEffectModulePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string detail){if(!value)throw new Exception(detail);report.checks.Add(detail);}
        static void Phase(int next){phase=next;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Start()
        {
            stopped=false;report=new Report();began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                f=new Fixture(true);var canvas=f.UiRoot.AddComponent<Canvas>();canvas.sortingOrder=12;
                f.Rows=JsonUtility.FromJson<Config>(Resources.Load<TextAsset>("Data/EffectConfig").text).Datas;
                Check(f.Rows.Single(r=>r.id==1016).res=="hdzd_eff_bxGlow"&&f.Rows.Single(r=>r.id==1016).duration==5,"Original task effect1016 resolves UI path and five-second duration");
                effect=f.Module.Get(f.Module.Show(1016,f.UiRoot.transform));effect.SetActive(false);
                Check(effect is OutgameUIEffect&&!effect.IsLoaded&&f.Module.Get(effect.UID)==effect&&f.Trace.Last()=="Effect/UI/hdzd_eff_bxGlow","Native Show publishes original effect while provider is pending");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>40)throw new TimeoutException("effect module phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0){f.CompleteLoad();Phase(1);return;}
                if(phase==1){
                    if(!effect.IsLoaded)return;
                    Check(!effect.GameObject.activeSelf&&effect.GameObject.transform.parent==f.UiRoot.transform&&effect.Order==13&&effect.GameObject.GetComponent<SpriteRenderer>().sortingOrder==13,"Actual WaitUntil resumes Play after parenting and inherited canvas order");
                    Time.timeScale=0;Phase(2);return;
                }
                if(phase==2){
                    if(EditorApplication.timeSinceStartup-phaseAt<.4)return;
                    Check(f.Module.Get(effect.UID)==effect&&!effect.IsDisposed&&f.Provider.Releases==0,"Original WaitForSeconds remains paused at timeScale zero");
                    effect.SetActive(true);Time.timeScale=3;Phase(3);return;
                }
                if(phase==3){
                    if(f.Module.Get(effect.UID)!=null)return;
                    Check(effect.IsDisposed&&!effect.GameObject&&f.Provider.Releases==1,"Five scaled seconds dispose native object, release real handle and unregister effect");
                    Time.timeScale=1;effect=f.Module.Get(f.Module.Show(1016,f.UiRoot.transform));f.CompleteLoad(1);Phase(4);return;
                }
                if(phase==4){
                    closed=effect.GameObject;f.Module.Close(effect.UID);
                    Check(effect.IsDisposed&&f.Module.Get(effect.UID)==null&&f.Provider.Releases==2,"Explicit native Close removes registration and releases handle immediately");Phase(5);return;
                }
                if(phase==5){
                    Check(!closed,"Native Object.Destroy completes on subsequent frame");
                    late=f.Module.Get(f.Module.Show(1016,f.UiRoot.transform));f.Module.Close(late.UID);Phase(6);return;
                }
                if(phase==6){f.CompleteLoad(2);f.LoadedObjects.Add(late.GameObject);Phase(7);return;}
                if(phase==7){
                    Check(late.IsDisposed&&late.IsLoaded&&late.GameObject&&late.GameObject.activeSelf&&f.Module.Get(late.UID)==null&&f.Provider.Releases==2,"Original early-close race retains late-load orphan and unreleased newly acquired handle");late.Dispose();
                    f.Module.Configs.Add(90001,new OutgameEffectData{id=90001,type=1,res="fixture-fly",duration=99});var runner=f.UiRoot.AddComponent<OutgameFlyTweenRunner>();f.Move=(t,p,d,c)=>runner.Move(t,p,d,runner.DefaultScaleEase,null,false,c);
                    effect=f.Module.Get(f.Module.ShowEffectWithId(90001,Vector3.zero,Vector3.zero,new Vector3(100,0,0),.2f,f.UiRoot.transform));baseReleases=f.Provider.Releases;f.CompleteLoad(3);Phase(8);return;
                }
                if(phase==8){
                    if(f.Module.Get(effect.UID)!=null)return;
                    Check(effect.IsDisposed&&!effect.GameObject&&f.Provider.Releases==baseReleases+1,"Recovered fly effect uses actual scaled native tween and releases on completion");
                    f.Module.Configs.Add(90002,new OutgameEffectData{id=90002,type=2,res="fixture-line",duration=.2});f.UiRoot.transform.position=new Vector3(10,20,0);
                    effect=f.Module.Get(f.Module.ShowEffectWithId(90002,Vector3.zero,Vector3.zero,new Vector3(266,20,0),0,f.UiRoot.transform));f.CompleteLoad(4);Phase(9);return;
                }
                if(phase==9){
                    if(effect.GameObject==null)throw new Exception("line finished before native geometry check");
                    Check(effect is OutgameLineEffect&&effect.GameObject.transform.parent==f.Module.LineEffectRoot&&Vector3.Distance(effect.GameObject.transform.position,new Vector3(10,20,0))<.01f&&Mathf.Abs(effect.GameObject.transform.localScale.y-1)<.001f&&Vector3.Dot(effect.GameObject.transform.up,Vector3.right)>.999f,"Native line uses parent world origin, UI-projected distance over256 and original direction");
                    baseReleases=f.Provider.Releases;f.Module.Close(effect.UID);Phase(10);return;
                }
                if(phase==10){
                    if(EditorApplication.timeSinceStartup-phaseAt<.4)return;
                    Check(f.Provider.Releases==baseReleases+1&&f.Warnings.Any(s=>s.StartsWith("Operation handle is released")),"Original line timer still disposes after explicit close; actual handle prevents second provider release and reports repeated release");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();
            try{f?.Dispose();}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            var path=Path.Combine(BattleBuild.Workspace,"analysis/effect-module-native-validation.json");File.WriteAllText(path,JsonUtility.ToJson(report,true));
            Debug.Log("AREABATTLE_EFFECT_MODULE_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
