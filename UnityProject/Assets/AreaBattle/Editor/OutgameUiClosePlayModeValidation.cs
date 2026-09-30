using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameUiClosePlayModeValidation
    {
        const string Pending="AreaBattle.UiClosePlayModeValidation";
        [Serializable] sealed class Report
        {
            public bool passed;public string scope="Real Play Mode animation and end-of-frame close on original activity prefab; isolated resource provider, no production account or native bundle loading";
            public List<string> checks=new List<string>();public string error;public int hideFrame,disposeFrame,closeFrame;public bool batchMode;
        }
        static Report report;static GameObject root;static OutgameFestUiLifetime lifetime;static OutgameUiPage page;
        static OutgameAssetProvider provider;static Task pending;static float began;static bool resumed;static List<int> counts;
        static OutgameUiClosePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var gameView=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));gameView.Show();gameView.Focus();EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report{batchMode=Application.isBatchMode};counts=new List<int>();began=Time.realtimeSinceStartup;resumed=false;
            try{
                Time.timeScale=0;var animation=new GameObject("UI animation owner").AddComponent<OutgameUiAnimation>();
                new GameObject("Validation camera").AddComponent<Camera>();
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));root.SetActive(true);
                var events=new OutgameMessageDispatcher();lifetime=new OutgameFestUiLifetime(root,()=>{},()=>events,obj=>UnityEngine.Object.Destroy(obj));
                page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);
                provider=new OutgameAssetProvider(()=>false,Debug.LogWarning){Status=4};var main=provider.CreateHandle("main",()=>false);
                Action unload=()=>counts.Add(provider.RefCount);
                var resources=new OutgameUiResourceLists(unload){Dynamic=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,provider.CreateHandle("dynamic",()=>false)}},Custom=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,provider.CreateHandle("custom",()=>false)}}};
                events.AddListener("GF_VisibleUI",args=>{report.hideFrame=Time.frameCount;Check(lifetime.IsDisposed&&!page.Visible&&root!=null,"Hide occurs with disposed flag while native object still exists");});
                events.AddListener("CheckUISortAfterStartUIShow",args=>{report.disposeFrame=Time.frameCount;Check(page.GameObject==null&&root!=null&&provider.RefCount==3,"Dispose clears references before deferred native destruction and resource releases");});
                events.AddListener("CloseUI",args=>{report.closeFrame=Time.frameCount;Check(ReferenceEquals(args[0],page)&&root==null&&provider.RefCount==0,"Close notification follows actual native destruction and all handle releases");});
                var host=new OutgameUiCloseHost(lifetime,page,animation,resources,"FestActUI/ValentineUI",()=>true,()=>Task.CompletedTask,unload,path=>throw new Exception("Unexpected legacy path")){CloseAnimation=2,CloseAnimationTime=.2f,MainHandle=main};
                pending=new OutgameUiAsyncClose(host,()=>events).CloseAsync();EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            try{
                float elapsed=Time.realtimeSinceStartup-began;
                if(!resumed&&elapsed>=.35f){Check(!pending.IsCompleted&&!lifetime.IsDisposed&&provider.RefCount==3&&root!=null,"Scaled animation wait retains page and resources while game time is paused");resumed=true;Time.timeScale=1;}
                if(pending.IsCompleted){pending.GetAwaiter().GetResult();Check(resumed&&report.disposeFrame==report.hideFrame&&report.closeFrame>report.disposeFrame,"Second end-of-frame occurs after hide/dispose frame");Check(string.Join(",",counts)=="2,1,0","Real close releases main, dynamic and custom handles in order");Finish(null);return;}
                if(elapsed>15)throw new TimeoutException("Real end-of-frame close did not complete; no synthetic frame substitution used.");
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-ui-close-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
