using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameUiLegacyPlayModeValidation
    {
        const string Pending="AreaBattle.UiLegacyPlayModeValidation";
        [Serializable] sealed class Report
        {
            public bool passed,batchMode;public int loadFrame,completeFrame;public string error;
            public string scope="Native legacy root/page instantiation and two-frame completion through cached page initialization; imported local asset adapter, no original bundle transport/account/business composition";
            public List<string> checks=new List<string>();
        }
        static Report report;static float began;static GameObject canvas;static bool completed;
        static OutgameUiLegacyPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report{batchMode=Application.isBatchMode};began=Time.realtimeSinceStartup;completed=false;
            try{
                Time.timeScale=0;var runner=new GameObject("legacy coroutine owner").AddComponent<OutgameUiAnimation>();
                canvas=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                var rootResource=new OutgameLegacyPrefabResource(null,name=>Resources.Load<GameObject>("Recovered/UiRoot/"+name));
                var pageResource=new OutgameLegacyPrefabResource(null,name=>Resources.Load<GameObject>("Recovered/FestActivity/"+name));
                Action<string,Action<OutgameLegacyPrefabResource>> acquire=(path,loaded)=>{
                    Check(path=="UI/UIRoot"||path=="UI/FestActUI/ValentineUI","Resource acquisition receives original UI path: "+path);
                    loaded(path=="UI/UIRoot"?rootResource:pageResource);
                };
                var resources=new OutgameUiLegacyRootResources(acquire,(a,b)=>Check(a==0&&b==0,"Legacy resource pump receives zero deltas during UI initialization"));
                var module=new OutgameUiModuleInitialization(()=>false,(path,cb)=>throw new Exception("modern route"),()=>resources,OutgameUiDisplaySettings.Shared,Debug.Log,message=>throw new Exception(message));
                module.Initialize();
                Check(module.IsInitialized&&module.UiRoot.name=="UIRoot"&&module.UiRoot.gameObject.activeSelf&&canvas.scene.name=="DontDestroyOnLoad","Legacy root is active under persistent native canvas");
                var nodes=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>module.UiRoot,()=>canvas,message=>throw new Exception(message));
                var lifetime=new OutgameUiLifetime(null,()=>{});var events=new OutgameMessageDispatcher();var page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);
                var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FestActivity/hud-import").text,"ValentineUI");
                var init=new OutgameUiObjectInitialization(lifetime,page,outlets.Read,()=>{},()=>{},()=>{},()=>{});
                var host=new OutgameUiOpenHost(lifetime,page,init,new OutgameUiCanvas(()=>0,()=>false),runner,()=>{},()=>{},()=>throw new Exception("cached OpenLater"),()=>throw new Exception("cached custom animation"),Debug.Log){Layer=1,Cached=true};
                var open=new OutgameUiOpenLifecycle(host,()=>events);
                report.loadFrame=Time.frameCount;
                var loader=new OutgameUiLegacyLoader(acquire,nodes.Get,routine=>runner.StartCoroutine(Guard(routine)),Debug.Log);
                loader.Load("FestActUI/ValentineUI","UIWindow",(go,res)=>{
                    report.completeFrame=Time.frameCount;
                    Check(Time.timeScale==0&&report.completeFrame>=report.loadFrame+2,"Two native frame yields complete while game time is paused");
                    Check(!go.activeSelf&&ReferenceEquals(res,pageResource)&&go.transform.parent==module.UiRoot.Find("UIWindow"),"Hidden page callback preserves resource identity and original layer");
                    open.LoadedLegacy(go,res);
                    Check(init.IsInitialized&&init.Objects.Count==13&&page.Visible&&go.activeSelf&&go.GetComponent<Canvas>().sortingOrder==10,"Concrete legacy open binds thirteen original outlets, activates and sorts the page");
                    completed=true;
                });
                Check(!completed&&!init.IsInitialized&&!module.UiRoot.Find("UIWindow").GetChild(0).gameObject.activeSelf,"Native scheduler leaves page hidden before two-frame completion");
                EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static IEnumerator Guard(IEnumerator routine)
        {
            while(true){object value;try{if(!routine.MoveNext())yield break;value=routine.Current;}catch(Exception ex){Finish(ex);yield break;}yield return value;}
        }
        static void Poll(){if(completed)Finish(null);else if(Time.realtimeSinceStartup-began>15)Finish(new TimeoutException("Legacy UI native callback did not complete"));}
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-ui-legacy-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
