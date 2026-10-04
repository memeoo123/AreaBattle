using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Spine.Unity;
using Fixture=AreaBattle.EditorTools.OutgameGuideBookPopupValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameGuideBookPopupPlayModeValidation
    {
        const string Pending="AreaBattle.GuideBookPopupNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Original popup text/sprites and own Spine4.1.16 subtree, actual item/popup pointer dispatch, animation frames, hide/reopen and real reward persistence. Camera/canvas are an isolated presentation harness; fixture voice/SkillControl/close and local atlas adapter do not prove full Main/async services or original visual match.";public List<string> checks=new List<string>();}
        static Report report;static Fixture f;static SkeletonAnimation spine;static Camera camera;static RenderTexture target;
        static bool stopped;static int phase,phaseFrame;static double began,phaseAt;static float cursor;static int coins;
        static OutgameGuideBookPopupPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Click(Component component){Check(ExecuteEvents.Execute(component.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler),"Native pointer delivered: "+component.name);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=phaseAt=EditorApplication.timeSinceStartup;
            try{
                new GameObject("EventSystem",typeof(EventSystem));f=new Fixture();camera=new GameObject("PopupCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=667;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.12f,.12f,1);
                target=new RenderTexture(750,1334,24);target.Create();camera.targetTexture=target;
                var root=f.Rewards.Container.transform.Find("Canvas");var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(750,1334);root.gameObject.AddComponent<GraphicRaycaster>();
                var layer=(RectTransform)root.Find("Normal");layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.sizeDelta=Vector2.zero;layer.anchoredPosition=Vector2.zero;
                var item=f.Item(1);Click(item.UnlockButton);spine=f.Popup.Demonstration.GetComponent<SkeletonAnimation>();
                Check(spine&&spine.valid&&spine.Skeleton.Data.Version=="4.1.16"&&spine.AnimationState.GetCurrent(0).Animation.Name=="YD","Own original Spine data initializes and selects YD");
                cursor=spine.AnimationState.GetCurrent(0).TrackTime;Canvas.ForceUpdateCanvases();Capture("open");phaseFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Capture(string name)
        {
            camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(750,1334,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,750,1334),0,0);image.Apply();
            string path=Path.Combine(BattleBuild.Workspace,"analysis/guide-book-popup-native-"+name+".png");File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);RenderTexture.active=old;
        }
        static void Poll()
        {
            if(stopped)return;try{
                double now=EditorApplication.timeSinceStartup;if(now-began>30)throw new TimeoutException("Popup phase "+phase);if(now-phaseAt<.3||Time.frameCount<=phaseFrame+2)return;
                if(phase==0){Check(spine.AnimationState.GetCurrent(0).TrackTime>cursor&&spine.GetComponent<MeshFilter>().sharedMesh.vertexCount>0,"Native frames advance animation and generate mesh: from="+cursor+", to="+spine.AnimationState.GetCurrent(0).TrackTime+", vertices="+spine.GetComponent<MeshFilter>().sharedMesh.vertexCount);Capture("animated");Click(f.Popup.transform.Find("guidePop/mainbg/OKBtn").GetComponent<Button>());Check(!f.Popup.Popup.activeSelf&&f.Rewards.Binding.SelectedBook==null,"OK pointer closes original popup and clears selection");cursor=spine.AnimationState.GetCurrent(0).TrackTime;phase=1;phaseAt=now;phaseFrame=Time.frameCount;return;}
                if(phase==1){Check(spine.AnimationState.GetCurrent(0).TrackTime==cursor,"Hidden popup stops native animation updates");f.Popup.OnItemClick(0,f.Rewards.Config.dicGuidebook[1]);phase=2;phaseAt=now;phaseFrame=Time.frameCount;return;}
                if(phase==2){Check(spine.AnimationState.GetCurrent(0).TrackTime>cursor,"Reopened source component resumes native animation");coins=f.Rewards.Inventory.Count(1001);Click(f.Rewards.Button);Check(f.Rewards.Inventory.Count(1001)==coins+50&&f.Rewards.Manager.ContainsGuide(1)&&!f.Rewards.Button.gameObject.activeSelf,"Native popup reward applies real inventory/claim and hides button");using(var reload=new OutgameGuideBookRewardsValidation.Fixture(f.Rewards.Path))Check(reload.Manager.ContainsGuide(1)&&reload.Inventory.Count(1001)==coins+50,"Independent restart retains popup reward");f.Popup.OnItemClick(6,f.Rewards.Config.dicGuidebook[7]);Check(!spine.gameObject.activeSelf&&f.Popup.Pitch.activeSelf&&f.Popup.Content.text=="","Defense page replaces demonstration with source pitch");Capture("defense");f.Popup.OnItemClick(5,f.Rewards.Config.dicGuidebook[6]);Check(!spine.gameObject.activeSelf&&!f.Popup.Pitch.activeSelf&&f.Popup.Icon.sprite==f.Sprites.Find("guideUI_icon9_1"),"Skill page uses actual commander picture");Capture("skill");Click(f.Popup.transform.Find("guidePop/imgPopMask").GetComponent<Image>());Check(!f.Popup.Popup.activeSelf&&f.Rewards.Binding.SelectedBook==null,"Native image mask dispatch closes popup");Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-book-popup-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
