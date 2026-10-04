using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameGuideBookPopupValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameDynamicListPlayModeValidation
    {
        const string Pending="AreaBattle.DynamicListNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Real Unity LateUpdate, original GuideBook rows/popup, ScrollRect pointer drag, saved reward refresh and native destruction/pool reuse on a fresh page. Test canvas/camera and fixture account/audio/effects services; full production BaseUI/Main and original visual match not claimed.";
            public List<string> checks=new List<string>();
        }
        static Report report;static Fixture f;static OutgameGuideBookListBinding list;static OutgamePrefabPoolControl pool;
        static Camera camera;static RenderTexture target;static GraphicRaycaster raycaster;static HashSet<GameObject> prior;
        static int phase,frame,slots;static double started,phaseAt;static bool stopped;static string savePath;
        static OutgameDynamicListPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);report.checks.Add(why);}
        static void Click(Component target)=>Check(ExecuteEvents.Execute(target.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler),"Pointer delivered: "+target.name);
        static OutgameGuideBookItem Row(int index)=>((OutgameGuideBookDynamicItem)list.List.GetItem(index).BaseItem).Item;
        static void Start()
        {
            report=new Report();stopped=false;phase=0;started=EditorApplication.timeSinceStartup;
            try{
                new GameObject("EventSystem",typeof(EventSystem));camera=new GameObject("ListCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.12f,.12f,1);
                target=new RenderTexture(750,1334,24);target.Create();camera.targetTexture=target;
                var registry=new OutgameControllerRegistry();pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);});registry.Bind(4561,()=>pool);registry.Resolve(4561);pool.OnInit();
                Open(null);slots=list.List.SlotCount;SetPhase(0);EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Open(string path)
        {
            f=new Fixture(path);savePath=f.Rewards.Path;
            var root=f.Rewards.Container.transform.Find("Canvas");var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
            var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);raycaster=root.gameObject.AddComponent<GraphicRaycaster>();
            var layer=(RectTransform)root.Find("Normal");layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.sizeDelta=Vector2.zero;layer.anchoredPosition=Vector2.zero;
            Canvas.ForceUpdateCanvases();
            list=new OutgameGuideBookListBinding(f.Popup.transform,Resources.Load<GameObject>("Recovered/GuideBook/GuideBookItem"),()=>pool,f.ItemServices);
            f.Rewards.RefreshItem=list.Data.UpdateItemData;f.Popup.Popup.SetActive(false);f.Browse.GuideTab.SetSelect(true);
        }
        static void SetPhase(int value){phase=value;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Capture(string name)
        {
            camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(750,1334,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,750,1334),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(BattleBuild.Workspace,"analysis/dynamic-list-native-"+name+".png"),texture.EncodeToPNG());UnityEngine.Object.Destroy(texture);RenderTexture.active=previous;
        }
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("Dynamic list phase "+phase);
                if(Time.frameCount<=frame+2||EditorApplication.timeSinceStartup-phaseAt<.2)return;
                if(phase==0){
                    Check(list.List.ShowMinIdx==0&&!list.List.IsDirty&&Row(0).Index==0,"Native LateUpdate binds original visible rows");
                    Check(list.List.transform.childCount==Math.Min(slots,list.Data.Data.Count)&&list.List.ShowMaxIdx<list.Data.Data.Count-1,"Native objects respect slot capacity and only viewport rows are bound");
                    prior=new HashSet<GameObject>();foreach(var slot in list.List.Renderers)if(slot.Root!=null)prior.Add(slot.Root);
                    Capture("top");
                    var scroll=list.List.Scroll;var drag=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=new Vector2(375,500),pressPosition=new Vector2(375,500),pointerPressRaycast=new RaycastResult{module=raycaster}};
                    ExecuteEvents.Execute(scroll.gameObject,drag,ExecuteEvents.initializePotentialDrag);ExecuteEvents.Execute(scroll.gameObject,drag,ExecuteEvents.beginDragHandler);
                    drag.position=new Vector2(375,1000);drag.delta=new Vector2(0,500);ExecuteEvents.Execute(scroll.gameObject,drag,ExecuteEvents.dragHandler);ExecuteEvents.Execute(scroll.gameObject,drag,ExecuteEvents.endDragHandler);scroll.StopMovement();SetPhase(1);return;
                }
                if(phase==1){
                    int index=list.List.ShowMinIdx;Check(index>0&&list.List.SlotCount==slots&&prior.Contains(list.List.GetItem(index).Root),"Native ScrollRect drag advances rows using existing pooled roots");
                    Click(Row(index).UnlockButton);Check(f.Rewards.Binding.SelectedIndex==index&&f.Rewards.Binding.SelectedBook==list.Data.Data[index].Config,"Recycled row pointer opens newly bound book");
                    Click(f.Popup.transform.Find("guidePop/mainbg/OKBtn").GetComponent<Button>());Capture("scrolled");list.List.Scroll.verticalNormalizedPosition=1;list.List.Scroll.StopMovement();SetPhase(2);return;
                }
                if(phase==2){
                    Check(list.List.ShowMinIdx==0,"Native normalized scroll returns to first row");Click(Row(0).UnlockButton);Click(f.Rewards.Button);
                    Check(f.Rewards.Manager.ContainsGuide(1)&&!Row(0).UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"Native reward callback refreshes visible row through data provider");
                    UnityEngine.Object.Destroy(f.Rewards.Container);SetPhase(3);return;
                }
                if(phase==3){
                    Check(pool.Root.transform.childCount==prior.Count,"Native OnDestroy recycles list objects before page hierarchy disappears");
                    bool survived=true;foreach(var root in prior)survived&=root!=null&&root.transform.parent==pool.Root.transform&&!root.activeSelf;Check(survived,"All pooled rows survive native page destruction");
                    Open(savePath);SetPhase(4);return;
                }
                if(phase==4){
                    Check(prior.Contains(list.List.GetItem(0).Root)&&f.Rewards.Manager.ContainsGuide(1)&&!Row(0).UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"Fresh page reuses source pool and reads persisted claim");
                    f.Trace.Clear();Click(Row(0).UnlockButton);Check(f.Trace.FindAll(x=>x=="voice:2001").Count==1&&f.Rewards.Binding.SelectedIndex==0,"Reopened pooled button invokes one current page callback");
                    Capture("reopened");Finish(null);
                }
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/dynamic-list-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
