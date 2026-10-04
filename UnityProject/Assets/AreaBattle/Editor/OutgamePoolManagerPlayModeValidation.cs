using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgamePoolManagerPlayModeValidation
    {
        const string Pending="AreaBattle.PoolManagerNative";
        [Serializable] sealed class Report {public bool passed;public string error,capture;public int frames,loadedIcons,returnedIcons;public List<string> checks=new List<string>();public string scope="Original ObjectPoolManager70 and LogicModule12 through actual Frame dispatch, registered EffectControl4058, UpdateManager coroutines/tweens, original TaskPanel/TopInfo/effect1016/currency assets and shared account/file rewards. Resource acquisition and audio/report/platform remain explicit local hosts; full production Main/all business/Player/original audiovisual equivalence pending.";}
        static Report report;static OutgameTaskBoxEffectValidation.Fixture task;static OutgameEffectControlValidation.Fixture fx;static Camera camera;
        static readonly List<GameObject> icons=new List<GameObject>();static readonly Dictionary<GameObject,Vector3> origins=new Dictionary<GameObject,Vector3>();static readonly List<int> sequencePositions=new List<int>();
        static int phase,frame,startFrame,updateId,completions,sequenceDone;static float phaseAt,pausedAt;static double started;static bool stopped;static string path;static Text gold,diamonds;
        static OutgamePoolManagerPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){OutgameTaskBoxEffectValidation.Prepare();SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"),false,"Game").Show();EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange c){if(c==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool b,string s){if(!b)throw new Exception(s);report.checks.Add(s);}
        static void Click(GameObject go){ExecuteEvents.Execute(go,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
        static void Phase(int p){phase=p;frame=Time.frameCount;phaseAt=Time.realtimeSinceStartup;}
        static void RefreshTop(){gold.text=task.Account.Account.Inventory.Count(1001).ToString();diamonds.text=task.Account.Account.Inventory.Count(1002).ToString();}
        static void Start()
        {
            stopped=false;report=new Report();started=EditorApplication.timeSinceStartup;startFrame=Time.frameCount;Time.timeScale=1;icons.Clear();origins.Clear();sequencePositions.Clear();completions=sequenceDone=0;
            try{
                OutgameTaskBoxEffectValidation.LoadPrepared();new GameObject("EventSystem",typeof(EventSystem));camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.farClipPlane=3000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;
                path=Path.Combine(Path.GetTempPath(),"AreaBattleEffectControl-"+Guid.NewGuid().ToString("N"));task=new OutgameTaskBoxEffectValidation.Fixture(true,path);fx=new OutgameEffectControlValidation.Fixture(true,true);fx.TopRoot.SetActive(false);
                var canvas=task.Account.Page.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;var scaler=task.Account.Page.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;task.Account.Page.Root.AddComponent<GraphicRaycaster>();Canvas.ForceUpdateCanvases();
                var source=new OutgameUiRootInitialization(task.Account.Page.Root,()=>true,()=>false,()=>{});source.Loaded((n,a)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.SetActive(a);return go;});
                task.Account.Page.Layer.SetParent(source.UiRoot.Find("UITip"),false);var layer=(RectTransform)task.Account.Page.Layer;layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;task.Services.UiRoot=()=>source.UiRoot;
                gold=task.Account.Page.TopRoot.transform.Find("objTopInfo/goldInfo/txt_goldNum").GetComponent<Text>();diamonds=task.Account.Page.TopRoot.transform.Find("objTopInfo/diamondInfo/txt_DiamondNum").GetComponent<Text>();RefreshTop();
                fx.Services.Tools=()=>task.Account.Account.Dispatcher;fx.Services.InventoryCount=id=>task.Account.Account.Inventory.Count(id);
                var ui=new OutgameUiControl(()=>null,fx.Registry,task.Account.Page.Ui.Messages,new OutgameUiControlGlobals(),null,()=>task.Account.Page.Top,()=>null,()=>null,(a,b)=>{});ui.ShowTopInfoUI();fx.Services.BindUi(()=>ui,source.UiRoot.Find);
                var nativeMove=fx.Services.Move;fx.Services.Move=(tr,p,d,e,id,u,complete)=>{if(!origins.ContainsKey(tr.gameObject)){icons.Add(tr.gameObject);origins.Add(tr.gameObject,tr.position);}nativeMove(tr,p,d,e,id,u,complete);};
                task.Account.Page.Services.Liveness.Effects=()=>fx.Control;task.Account.Page.Services.Liveness.RefreshTopInfo=RefreshTop;task.Account.Services.Tools.RefreshTopInfo=RefreshTop;
                updateId=OutgameUpdateManager.AddHandle(()=>fx.Frame.Update(Time.deltaTime,Time.unscaledDeltaTime));
                var entry=new OutgameTaskEntryBinding(task.Account.Page.Data.Main.transform,new OutgameTaskEntryServices{Control=()=>task.Account.Page.Data.Control,Pages=()=>task.Account.Page.OpenRegistry,FindPage=()=>task.Account.Page.Pages.TryGetValue(OutgameTaskPage.SourceName,out var page)?page:null,Messages=task.Account.Page.Ui.Messages,PlayVoice=(k,id)=>{}});entry.Initialize();task.Account.Tasks.Child.ModuleData.ext.livenessValue=100;Click(entry.Button.gameObject);Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>50)throw new TimeoutException("effect control phase "+phase);
                report.frames=Time.frameCount-startFrame;if(Time.frameCount<=frame+3)return;
                float elapsed=Time.realtimeSinceStartup-phaseAt;
                if(phase==0){if(!task.Account.Page.Trace.Contains("open"))return;Check(fx.Frame.Modules.First.Value==fx.Manager&&fx.Frame.Modules.Last.Value==fx.Logic&&fx.Manager.IsInitialized&&fx.Logic.ControllerCount==1&&ReferenceEquals(fx.Registry.Resolve(4058),fx.Control)&&fx.Control.Pool.Name=="moneyPool","Real Frame priority70/12, ObjectPoolManager and LogicModule own original moneyPool controller");Click(task.Box().gameObject);Phase(1);return;}
                if(phase==1){if(elapsed<.45f||task.Account.Global.GetItemCount(1001)!=50)return;
                    Check(task.Effect!=null&&task.Effect.IsLoaded&&fx.Control.ActiveEffectCount==1,"Native task pointer runs actual box effect and currency controller together");
                    Check(task.Account.Account.Local.GoldNum==50&&task.Account.Tasks.Child.ModuleData.ext.livenessAward==1,"Source task claim reaches both economic records and first tier bit");
                    Check(icons.Count==5&&icons.All(go=>go&&go.name=="goldItem")&&icons.Any(go=>go.transform.position!=origins[go]),"Five original currency prefabs move in native rendered frames");
                    report.capture=Path.Combine(BattleBuild.Workspace,"analysis/captures/pool-manager-task-native.png");Directory.CreateDirectory(Path.GetDirectoryName(report.capture));ScreenCapture.CaptureScreenshot(report.capture);Phase(2);return;
                }
                if(phase==2){if(elapsed<1.7f||fx.Control.ActiveEffectCount!=0)return;
                    Check(gold.text=="50"&&icons.All(go=>!go.activeSelf)&&fx.Control.PendingCleanupCount==0,"Task fly completion refreshes original TopInfo and returns all icons through source pool");
                    Check(task.Account.Global.GetItemCount(1001)==50&&task.Account.Account.Local.GoldNum==50,"Presentation does not duplicate task reward");
                    Time.timeScale=0;pausedAt=Time.time;fx.Control.FlyMoney(7,null,task.Box().position,true,()=>completions++,true);fx.Control.FlyDiamonds(3,null,task.Box(1).position,true,()=>completions++,true);
                    Check(task.Account.Account.Local.GoldNum==57&&task.Account.Account.Inventory.Count(1002)==3&&fx.Control.ActiveEffectCount==2,"Direct economic fly calls save before concurrent animations complete");Phase(3);return;
                }
                if(phase==3){if(elapsed<2||fx.Control.ActiveEffectCount!=0)return;
                    Check(completions==2&&gold.text=="57"&&diamonds.text=="3"&&Time.time==pausedAt,"Actual realtime coroutines and tweens complete while scaled time is paused");
                    Check(fx.Host.Loaded==10&&icons.Count==10&&fx.Control.Pool.Pool.Count==10&&icons.All(go=>!go.activeSelf),"Original single-spawn pool reuses gold icons and adds only five diamond prefabs");
                    fx.Control.PlaySequenceEffect(new[]{1001,1002},new[]{7,3},()=>sequenceDone++,i=>{sequencePositions.Add(i);return task.Box(i).position;});Phase(4);return;
                }
                if(phase==4){if(elapsed<3.2f||fx.Control.ActiveEffectCount!=0)return;
                    Check(sequenceDone==1&&sequencePositions.SequenceEqual(new[]{0,1}),"Native arrival callback advances gold then diamond sequence and completes exactly once");
                    Check(task.Account.Global.GetItemCount(1001)==50&&task.Account.Account.Local.GoldNum==57&&task.Account.Account.Inventory.Count(1002)==3,"Sequence retains source split: ItemManager50, direct ToolChange LocalData57 and diamonds3");
                    report.loadedIcons=fx.Host.Loaded;report.returnedIcons=icons.Count(go=>!go.activeSelf);task.Account.Tasks.Parent.Update();task.Account.Statistics.Pool.SaveData();
                    Check(fx.Manager.Pools.Count==1&&fx.Host.Pools.Count==0,"All icon ownership lives in restored manager instead of local host registry");Phase(5);return;
                }
                if(phase==5){if(elapsed<6||fx.Control.Pool.Pool.Count!=0||icons.Any(go=>go))return;Check(fx.Control.Pool.Pool.Count==0&&icons.All(go=>!go)&&Time.time==pausedAt,"Frame-driven unscaled pool expiry releases original icons while scaled game time stays paused");
                    OutgameUpdateManager.Instance.RemoveNow(updateId);fx.Frame.Shutdown();Check(!fx.Registry.HasInstance(4058)&&fx.Manager.Pools.Count==0&&!fx.Manager.IsInitialized&&fx.Frame.Modules.Count==0&&fx.Host.Unloads==1,"Reverse Frame shutdown disposes Logic effects before ObjectPoolManager final clear");Phase(6);return;
                }
                if(phase==6){Check(icons.All(go=>!go),"Native expired pool objects stay destroyed on the subsequent Unity frame");fx.Dispose();fx=null;task.Dispose();task=null;
                    using(var again=new OutgameAccountRewardsValidation.Fixture(true,path)){Check(again.Global.GetItemCount(1001)==50&&again.Account.Local.GoldNum==57&&again.Account.Inventory.Count(1002)==3&&again.Tasks.Child.ModuleData.ext.livenessAward==1,"Independent account restart restores source ItemManager50/LocalData57, diamonds3 and task claim bit");}
                    Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception e)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=e==null;report.error=e?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/pool-manager-native-validation.json"),JsonUtility.ToJson(report,true));
            if(fx!=null){OutgameUpdateManager.Instance.RemoveNow(updateId);fx.Dispose();}task?.Dispose();
            if(e!=null)Debug.LogException(e);Debug.Log("AREABATTLE_POOL_MANAGER_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
