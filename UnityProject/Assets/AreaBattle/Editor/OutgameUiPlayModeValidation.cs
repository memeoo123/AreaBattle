using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameUiPlayModeValidation
    {
        const string Pending="AreaBattle.UiPlayModeValidation";
        [Serializable] sealed class Report {public bool passed;public string error;public string scope="Native UIControl/TopInfo/MenuTab with original prefabs and default WaitForEndOfFrame menu loading. Menu data/role/account callbacks are explicit fixtures; no complete Main or Player claim.";public List<string> checks=new List<string>();}
        static Report report;static int phase,frame;static double began;static OutgameUiControl control;static OutgameControllerRegistry registry;static OutgameMessageDispatcher messages;static OutgameMenuItems items;static OutgameTopInfoPage top;static OutgameMenuTabPage menu;static GameObject topRoot,menuRoot;static List<string> loaded;
        static OutgameUiPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        public static void RunRendered()
        {
            if(Application.isBatchMode)throw new InvalidOperationException("Rendered native validation requires GameView");
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));gameView.Show();gameView.Focus();EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string name){if(!condition)throw new Exception(name);report.checks.Add(name);}
        static void Start()
        {
            report=new Report();began=EditorApplication.timeSinceStartup;phase=0;loaded=new List<string>();
            try{
                registry=new OutgameControllerRegistry();messages=new OutgameMessageDispatcher();
                var canvas=new GameObject("canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));((RectTransform)canvas.transform).sizeDelta=new Vector2(1080,1920);var camera=new GameObject("UICamera",typeof(Camera));camera.transform.SetParent(canvas.transform);
                var root=new GameObject("UIRoot",typeof(RectTransform));
                var module=new OutgameUiModuleInitialization(()=>true,(path,callback)=>callback((name,active)=>{root.SetActive(active);return root;}),null,new OutgameUiDisplaySettings(),Debug.Log,Debug.LogError,tag=>canvas,g=>{});module.Initialize();
                topRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"),root.transform,false);top=OutgameTopInfoPage.FromOriginal(topRoot,()=>messages);top.OpenParts();
                menuRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/MenuTabUI"),root.transform,false);
                items=new OutgameMenuItems((name,visible)=>{loaded.Add(name);return new OutgameUiPage(UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/"+name),root.transform,false),()=>messages);},()=>{loaded.Add("ItemInfoUI");return new OutgameUiPage(new GameObject("item-info-fixture"),()=>messages);});
                menu=new OutgameMenuTabPage(menuRoot,()=>control,items,()=>messages,()=>control.CurrentPage=3,()=>{},()=>{},()=>{},()=>{});
                OutgameCoreControllerBindings.BindUi(registry,()=>module,()=>messages,new OutgameUiControlGlobals(),items,()=>top,()=>menu,()=>menu,(a,b)=>{});control=(OutgameUiControl)registry.Resolve(4296);control.OnInit();
                Time.timeScale=0;messages.SendMessage("GamePlayState",new object[]{2});
                Check(loaded.Count==1&&loaded[0]=="Proj_xqzdStartUI"&&top.Visible&&menu.Visible,"Home entry starts main immediately before native end-of-frame waits");
                frame=Time.frameCount;EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            try{
                if(EditorApplication.timeSinceStartup-began>25)throw new TimeoutException("Native UI end-of-frame sequence timed out");
                if(Time.frameCount<=frame)return;
                if(phase==0){
                    if(items.ItemInfo==null)return;
                    Check(string.Join(",",loaded)=="Proj_xqzdStartUI,ShopUI,CommanderUI,ItemInfoUI"&&Time.frameCount>frame,"Native end-of-frame loads preserve original page order while timeScale is zero");
                    messages.SendMessage("GamePlayState",new object[]{3});
                    Check(!menu.Visible&&!top.Visible&&control.CurrentPage==0&&menuRoot.activeSelf&&menuRoot.transform.localScale==Vector3.zero,"Battle state closes native menu and top, preserving active root");
                    messages.SendMessage("GamePlayState",new object[]{8});
                    var gold=topRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>();
                    Check(top.Visible&&gold.sortingOrder==9&&gold.sortingLayerName=="UIPopup","Result state activates original header parts on popup layer");
                    frame=Time.frameCount;phase=1;return;
                }
                messages.SendMessage("GamePlayState",new object[]{2});
                Check(menu.Visible&&top.Visible&&control.CurrentPage==3&&loaded.Count==4&&topRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingOrder==0,"Return to home reuses native pages and restores top ordering");
                control.OnDispose();messages.SendMessage("GamePlayState",new object[]{3});
                Check(menu.Visible&&control.TopInfo==null&&!registry.HasInstance(4296),"Disposed UI controller removes state listener and clears registry");Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/ui-native-playmode-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
