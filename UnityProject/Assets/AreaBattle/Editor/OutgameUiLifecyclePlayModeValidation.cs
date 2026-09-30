using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameUiLifecyclePlayModeValidation
    {
        const string Pending="AreaBattle.UiLifecyclePlayModeValidation";
        [Serializable] sealed class Report
        {
            public bool passed;public string scope="Real original bootstrap Awake, UIRoot Start and open/init/outlet/canvas/animation/close lifecycle; platform height injected, page business hooks/provider isolated, no production account/native bundles";
            public List<string> checks=new List<string>();public string error;public int openFrame,hideFrame,disposeFrame,closeFrame;public bool batchMode;public int bootstrapFrame,bangsFrame;public float bangsPixels;
        }
        static Report report;static GameObject root,canvasRoot;static RectTransform moduleRoot;static bool bangsChecked;static OutgameFestUiLifetime lifetime;static OutgameUiPage page;
        static OutgameAssetProvider provider;static Task pending;static float began;static bool resumed;static List<int> counts;static List<string> order;
        static OutgameUiLifecyclePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
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
            report=new Report{batchMode=Application.isBatchMode};counts=new List<int>();order=new List<string>();pending=null;began=Time.realtimeSinceStartup;resumed=false;bangsChecked=false;
            try{
                Time.timeScale=0;var animation=new GameObject("UI animation owner").AddComponent<OutgameUiAnimation>();
                canvasRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));report.bootstrapFrame=Time.frameCount;
                Check(OutgameUiDisplaySettings.Shared.IsPortrait&&!OutgameUiDisplaySettings.Shared.HeightControlsWidthFixedWidth&&canvasRoot.GetComponent<Canvas>().worldCamera==canvasRoot.transform.Find("UICamera").GetComponent<Camera>(),"Original bootstrap Awake publishes source display settings and binds original camera");
                OutgameAdaptiveBangs.SetBangsPixel(100);
                var module=new OutgameUiModuleInitialization(()=>true,(path,loaded)=>loaded((name,active)=>{var instance=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));instance.name=name;instance.SetActive(active);return instance;}),()=>throw new Exception("Unexpected legacy root load"),OutgameUiDisplaySettings.Shared,Debug.Log,message=>throw new Exception(message));
                module.Initialize();moduleRoot=module.UiRoot;
                Check(module.IsInitialized&&module.CanvasRoot==canvasRoot&&canvasRoot.scene.name=="DontDestroyOnLoad"&&module.UiCamera==canvasRoot.GetComponent<Canvas>().worldCamera,"Native module finds tagged canvas/camera and persists root before initialization completes");
                Check(moduleRoot.GetComponent<OutgameAdaptiveBangs>().ModuleCanvas==canvasRoot&&moduleRoot.offsetMax.y==0f,"Root callback assigns module canvas before native bangs Start");
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"),moduleRoot.Find("UIWindow"),false);root.SetActive(false);
                var events=new OutgameMessageDispatcher();lifetime=new OutgameFestUiLifetime(null,()=>{},()=>events,obj=>UnityEngine.Object.Destroy(obj));
                page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);
                provider=new OutgameAssetProvider(()=>false,Debug.LogWarning){Status=4};var main=provider.CreateHandle("main",()=>false);
                Action unload=()=>counts.Add(provider.RefCount);
                var resources=new OutgameUiResourceLists(unload){Dynamic=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,provider.CreateHandle("dynamic",()=>false)}},Custom=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,provider.CreateHandle("custom",()=>false)}}};
                events.AddListener("GF_VisibleUI",args=>{if((bool)args[1]){order.Add("visible");return;}report.hideFrame=Time.frameCount;Check(lifetime.IsDisposed&&!page.Visible&&root!=null,"Hide occurs with disposed flag while native object still exists");});
                events.AddListener("CheckUISortAfterStartUIShow",args=>{report.disposeFrame=Time.frameCount;Check(page.GameObject==null&&root!=null&&provider.RefCount==3,"Dispose clears references before deferred native destruction and resource releases");});
                events.AddListener("CloseUI",args=>{report.closeFrame=Time.frameCount;Check(ReferenceEquals(args[0],page)&&root==null&&provider.RefCount==0,"Close notification follows actual native destruction and all handle releases");});
                var host=new OutgameUiCloseHost(lifetime,page,animation,resources,"FestActUI/ValentineUI",()=>true,()=>Task.CompletedTask,unload,path=>throw new Exception("Unexpected legacy path")){CloseAnimation=2,CloseAnimationTime=.2f,MainHandle=main};
                var close=new OutgameUiAsyncClose(host,()=>events);
                var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FestActivity/hud-import").text,"ValentineUI");
                OutgameUiObjectInitialization init=null;
                init=new OutgameUiObjectInitialization(lifetime,page,outlets.Read,()=>{Check(init.Objects.Count==13,"Real page binds all thirteen original root outlets");order.Add("components");},()=>order.Add("init"),()=>order.Add("skin"),()=>order.Add("awake"));
                events.AddListener("OpenUI",args=>{report.openFrame=Time.frameCount;Check(ReferenceEquals(args[0],page)&&!lifetime.IsDisposed&&root!=null&&root.GetComponent<Canvas>().sortingOrder==10,"Real OpenUI follows live initialized sorted page");Check(string.Join(",",order)=="components,init,visible,skin,awake,loading,refresh,later","Source initialization and OpenLater run in order across actual animation coroutine");pending=close.CloseAsync();});
                var openHost=new OutgameUiOpenHost(lifetime,page,init,new OutgameUiCanvas(()=>0,()=>false),animation,()=>order.Add("loading"),()=>order.Add("refresh"),()=>order.Add("later"),()=>throw new Exception("Unexpected custom animation"),Debug.Log){Layer=1,OpenAnimation=1,OpenAnimationTime=.2f};
                new OutgameUiOpenLifecycle(openHost,()=>events).LoadedModern(root);EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            try{
                float elapsed=Time.realtimeSinceStartup-began;
                if(!bangsChecked&&Time.frameCount>report.bootstrapFrame+2)
                {
                    report.bangsFrame=Time.frameCount;report.bangsPixels=OutgameUiDisplaySettings.Shared.BangsPixel;
                    float expected=Mathf.Ceil(100f*canvasRoot.GetComponent<RectTransform>().sizeDelta.y/Screen.height);
                    Check(report.bangsPixels==expected&&expected>0&&moduleRoot.offsetMax.y==-expected&&moduleRoot.offsetMin.y==0f,"Native Start applies source pixel conversion after bootstrap/root setup even while timeScale is zero");bangsChecked=true;
                }
                if(!resumed&&elapsed>=.35f){Check(pending==null&&report.openFrame==0&&!lifetime.IsDisposed&&provider.RefCount==3&&root!=null,"Opening animation waits while paused and retains initialized page/resources");resumed=true;Time.timeScale=1;}
                if(pending!=null&&pending.IsCompleted){pending.GetAwaiter().GetResult();Check(bangsChecked&&resumed&&report.openFrame<report.hideFrame&&report.disposeFrame==report.hideFrame&&report.closeFrame>report.disposeFrame,"Second end-of-frame occurs after hide/dispose frame");Check(string.Join(",",counts)=="2,1,0","Real close releases main, dynamic and custom handles in order");Finish(null);return;}
                if(elapsed>15)throw new TimeoutException("Real end-of-frame close did not complete; no synthetic frame substitution used.");
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-ui-lifecycle-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
