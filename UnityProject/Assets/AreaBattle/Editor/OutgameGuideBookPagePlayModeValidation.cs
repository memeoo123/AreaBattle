using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameGuideBookPageValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameGuideBookPagePlayModeValidation
    {
        const string Pending="AreaBattle.GuideBookPageNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;public string centeringDiagnostic;
            public string scope="Original scene canvas/UIRoot plus composed modern two-frame load, GuideBook Awake/browse/reward, native opening/closing and handle/pool lifetime; scaled frame-end centering and overlapping routines. Resource acquisition is a local recovered-prefab provider; account/commander/localization/audio/effects remain fixture endpoints, full Main/Player/original visual match pending.";
            public List<string> checks=new List<string>();public int requestFrame,openFrame,hideFrame,closeFrame;
        }
        static Report report;static Fixture f;static OutgameDynamicListValidation.Fixture center;
        static OutgameGuideBookPage page;static GameObject oldRoot;static OutgameAssetHandle oldHandle;static Camera camera;static RenderTexture texture;
        static int phase,frame;static double started,phaseAt;static bool stopped;static HashSet<GameObject> rows;
        static OutgameGuideBookPagePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);report.checks.Add(why);}
        static void Click(Component value)=>Check(ExecuteEvents.Execute(value.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler),"Pointer delivered: "+value.name);
        static OutgameGuideBookItem Row(int index)=>((OutgameGuideBookDynamicItem)page.List.List.GetItem(index).BaseItem).Item;
        static void SetPhase(int value){phase=value;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static bool Centering=>typeof(OutgameDynamicList).GetField("positionCoroutine",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(center.List)!=null;
        static Vector2 CenterPosition=>((RectTransform)center.List.transform).anchoredPosition;
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;
            try{
                if(EventSystem.current==null)new GameObject("EventSystem",typeof(EventSystem));
                f=new Fixture(true);camera=f.Root.GetComponent<Canvas>().worldCamera;texture=new RenderTexture(750,1334,24);texture.Create();camera.targetTexture=texture;
                f.Data.Messages.AddListener("OpenUI",a=>{report.openFrame=Time.frameCount;Check(ReferenceEquals(a[0],page)&&page.GameObject!=null&&!page.Lifetime.IsDisposed,"OpenUI identifies initialized live page after animation");});
                f.Data.Messages.AddListener("GF_VisibleUI",a=>{if((bool)a[1])return;report.hideFrame=Time.frameCount;Check(page.Lifetime.IsDisposed&&page.GameObject!=null&&page.Data.Data.Count>0&&f.Provider.RefCount==3,"Async hide precedes business disposal and handle release");});
                f.Data.Messages.AddListener("CloseUI",a=>{report.closeFrame=Time.frameCount;Check(oldRoot==null&&page.Data.Data.Count==0&&page.Browse.Tips.Count==0&&f.Provider.RefCount==0,"CloseUI follows native destruction, guide/tip cleanup and all resource releases");});
                report.requestFrame=Time.frameCount;page=f.Open("native-argument");Check(page.GameObject==null&&page.MainHandle!=null&&f.Pages.ContainsKey("GuideBookUI"),"Registry/handle exist before two-frame resource callback");SetPhase(0);EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Capture(string name)
        {
            camera.Render();var previous=RenderTexture.active;RenderTexture.active=texture;var image=new Texture2D(750,1334,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,750,1334),0,0);image.Apply();File.WriteAllBytes(Path.Combine(BattleBuild.Workspace,"analysis/guide-page-native-"+name+".png"),image.EncodeToPNG());UnityEngine.Object.Destroy(image);RenderTexture.active=previous;
        }
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-started>50)throw new TimeoutException("Guide page phase "+phase);
                if(Time.frameCount<=frame+2||EditorApplication.timeSinceStartup-phaseAt<.2)return;
                if(phase==0){
                    if(report.openFrame==0)return;
                    Check(report.openFrame>=report.requestFrame+2&&page.OutletCount==24&&page.List.List.ShowMinIdx==0,"Real delayed loading reaches original outlets and native list render");
                    Check(page.GameObject.transform.parent==f.Layer&&f.Layer.name=="UIPopup"&&f.Root.scene.name=="DontDestroyOnLoad","Recovered UI bootstrap and layer own the page");
                    Check(page.Browse.Tips.Count==f.Data.Config.dicGuideTips.Count&&page.Browse.Tabs.Current==page.Browse.GuideTab,"Native Awake builds tips and selects guide tab");Capture("open");
                    center=new OutgameDynamicListValidation.Fixture();var centerRect=(RectTransform)center.List.transform;centerRect.anchorMin=centerRect.anchorMax=centerRect.pivot=new Vector2(0,1);centerRect.anchoredPosition=Vector2.zero;center.Init(40);center.Root.SetActive(true);center.List.Scroll.StopMovement();center.List.CenteredWithIndex(20,.6f);Time.timeScale=0;SetPhase(1);return;
                }
                if(phase==1){Check(CenterPosition==Vector2.zero,"Frame-end positioning remains at start while scaled time is paused");Time.timeScale=1;SetPhase(2);return;}
                if(phase==2){Check(CenterPosition.y>0&&CenterPosition.y<237.5f,"Real frame-end routine interpolates with scaled deltaTime");SetPhase(3);return;}
                if(phase==3){
                    if(Centering)return;report.centeringDiagnostic="position="+CenterPosition+", velocity="+center.List.Scroll.velocity+", time="+Time.time+", delta="+Time.deltaTime+", coroutine="+Centering;Check(Mathf.Abs(CenterPosition.y-237.5f)<.001f,"Single centering reaches exact source destination");
                    center.List.CenteredWithIndex(10,.2f);center.List.CenteredWithIndex(30,1f);SetPhase(4);return;
                }
                if(phase==4){
                    if(Centering||EditorApplication.timeSinceStartup-phaseAt<.6)return;Check(Mathf.Abs(CenterPosition.y-112.5f)<.001f,"Older completion stops latest stored coroutine and keeps older destination");
                    Click(Row(0).UnlockButton);Click(page.Rewards.BookRewardButton.GetComponent<Button>());Check(f.Data.Manager.ContainsGuide(1)&&!Row(0).UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"Composed page persists reward and refreshes actual row");
                    page.Popup.ClosePopup();Click(page.Browse.TipTab.GetComponent<Button>());Check(page.Browse.TipTab.IsSelect&&page.GameObject.transform.Find("tipSV").gameObject.activeSelf,"Native tab pointer uses initialized group handlers");Capture("tips");
                    page.Browse.Tabs.SetSelect(page.Browse.GuideTab);rows=new HashSet<GameObject>();foreach(var slot in page.List.List.Renderers)if(slot.Root!=null)rows.Add(slot.Root);
                    oldRoot=page.GameObject;oldHandle=page.MainHandle;
                    page.Resources.Dynamic=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{oldRoot,f.Provider.CreateHandle("dynamic",()=>false)}};
                    page.Resources.Custom=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{oldRoot,f.Provider.CreateHandle("custom",()=>false)}};
                    page.CloseAction=()=>Check(f.Pages.ContainsKey("GuideBookUI"),"CloseAction runs before registry removal");Click(oldRoot.transform.Find("btnClose").GetComponent<Button>());
                    Check(!f.Pages.ContainsKey("GuideBookUI")&&page.Closing!=null,"Close button removes module page and starts source async close");SetPhase(5);return;
                }
                if(phase==5){
                    if(!page.Closing.IsCompleted)return;page.Closing.GetAwaiter().GetResult();Check(report.closeFrame>report.hideFrame&&page.Resources.Dynamic==null&&page.Resources.Custom==null,"Second frame-end separates disposal and dynamic/custom release");
                    Check(string.Join("|",f.Trace.FindAll(x=>x.StartsWith("unload:")))=="unload:2|unload:1|unload:0","Main, dynamic and custom handles released in source order");
                    bool survived=true;foreach(var root in rows)survived&=root!=null&&root.transform.parent==f.Pool.Root.transform;Check(survived,"List objects survive actual async page destruction in source pool");
                    var previous=page;report.openFrame=0;page=f.Open();Check(page!=previous&&page.MainHandle!=oldHandle,"Registry reopens with fresh page and main handle");SetPhase(6);return;
                }
                if(phase==6){
                    if(report.openFrame==0)return;Check(rows.Contains(page.List.List.GetItem(0).Root)&&!Row(0).UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"New owner reuses pooled row and retained claimed state");Capture("reopened");
                    using(var reload=new OutgameGuideBookRewardsValidation.Fixture(f.Data.Path))Check(reload.Manager.ContainsGuide(1),"Independent data manager restart confirms persisted guide claim");Finish(null);
                }
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-page-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
