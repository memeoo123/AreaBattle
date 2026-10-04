using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameGuideBookBrowseValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameGuideBookBrowsePlayModeValidation
    {
        const string Pending="AreaBattle.GuideBookBrowseNative";
        [Serializable] sealed class Report
        {public bool passed;public string error;public string scope="Imported GuideBookUI, actual TabButton Awake/Start, native Button/Image pointer dispatch and source UpdateManager WaitForEndOfFrame resizing, real tool/claim storage and BaseItem destruction. Voice/localization/toast/effects are explicit fixture endpoints; full popup/DynamicList/Spine/Main and visual/audio acceptance remain pending.";public List<string> checks=new List<string>();}
        static Fixture fixture;static Report report;static int phase,frame;static double began;static bool stopped;
        static OutgameTipBookItem first,second;static GameObject firstRoot,secondRoot;static int balance;
        static OutgameGuideBookBrowsePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string detail){if(!value)throw new Exception(detail);report.checks.Add(detail);}
        static void Click(Component target)
        {bool delivered=ExecuteEvents.Execute(target.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);Check(delivered,"Native pointer event delivered to "+target.name);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=EditorApplication.timeSinceStartup;
            try{
                var camera=new GameObject("GuideBookValidationCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);
                new GameObject("EventSystem",typeof(EventSystem));fixture=new Fixture(true);
                var canvasRoot=fixture.Rewards.Container.transform.Find("Canvas").gameObject;
                var canvas=canvasRoot.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvasRoot.AddComponent<GraphicRaycaster>();
                fixture.Populate();first=fixture.Binding.Tips[0];second=fixture.Binding.Tips[1];
                Check(fixture.Binding.Tabs.Buttons.Length==2&&fixture.Binding.GuideTab.IsSelect,"Actual Awake discovers two original tab components before page selection");
                Check(OutgameUpdateManager.Instance&&fixture.Coroutines.Count==0,"Tip layout coroutines belong to native shared UpdateManager");
                frame=Time.frameCount;EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>30)throw new TimeoutException("GuideBook native phase "+phase);
                if(Time.frameCount<=frame+2)return;
                if(phase==0)
                {
                    Check(Mathf.Approximately(first.Lifetime.RectTransform.rect.height,first.InitialHeight),"Initial collapsed height survives native frame scheduling");
                    Click(fixture.Binding.TipTab.GetComponent<Button>());
                    Check(fixture.Binding.TipTab.IsSelect&&!fixture.Binding.GuideTab.IsSelect,"Native Start attaches group click delegates");
                    float previous=first.Lifetime.RectTransform.rect.height;Click(first.UnlockButton);
                    Check(first.IsSelected&&fixture.Binding.SelectedTip==first&&Mathf.Approximately(first.Lifetime.RectTransform.rect.height,previous),"Pointer expansion changes selection immediately and leaves height until frame end");
                    frame=Time.frameCount;phase=1;return;
                }
                if(phase==1)
                {
                    Check(Mathf.Approximately(first.Lifetime.RectTransform.rect.height,first.Description.rect.height)&&!Mathf.Approximately(first.Lifetime.RectTransform.rect.height,first.InitialHeight),"Actual WaitForEndOfFrame applies expanded description height: root="+first.Lifetime.RectTransform.rect.height+", description="+first.Description.rect.height+", initial="+first.InitialHeight);
                    Click(second.UnlockButton);Check(!first.IsSelected&&second.IsSelected&&fixture.Binding.SelectedTip==second,"Second native item collapses first selection");frame=Time.frameCount;phase=2;return;
                }
                if(phase==2)
                {
                    Check(Mathf.Approximately(first.Lifetime.RectTransform.rect.height,first.InitialHeight)&&Mathf.Approximately(second.Lifetime.RectTransform.rect.height,second.Description.rect.height),"Native parent layout follows both delayed height changes");
                    balance=fixture.Rewards.Inventory.Count(second.Config.reward[0]);Click(second.RewardButton);
                    Check(fixture.Rewards.Inventory.Count(second.Config.reward[0])==balance+second.Config.reward[1]&&fixture.Rewards.Manager.ContainsTip(second.Config.id),"Native reward pointer reaches real economic and claim records");
                    Check(second.IsSelected&&!second.RewardButton.transform.parent.gameObject.activeSelf,"Claim keeps expansion and hides reward row before delayed rebuild");frame=Time.frameCount;phase=3;return;
                }
                if(phase==3)
                {
                    Check(Mathf.Approximately(second.Lifetime.RectTransform.rect.height,second.Description.rect.height),"Post-claim native coroutine resizes after reward row removal");
                    using(var restarted=new OutgameGuideBookRewardsValidation.Fixture(fixture.Rewards.Path))Check(restarted.Manager.ContainsTip(second.Config.id)&&restarted.Inventory.Count(second.Config.reward[0])==balance+second.Config.reward[1],"Independent disk restart restores native claim and currency");
                    fixture.Trace.Clear();Click(fixture.Binding.TipTab.GetComponent<Button>());Check(fixture.Trace.Count==0&&second.IsSelected,"Clicking selected tab retains expansion without repeat voice");
                    Click(fixture.Binding.GuideTab.GetComponent<Button>());Check(fixture.Binding.SelectedTip==null&&!second.IsSelected&&!fixture.Binding.transform.Find("tipSV").gameObject.activeSelf,"Leaving tip tab clears item selection and hides original scroll view");frame=Time.frameCount;phase=4;return;
                }
                if(phase==4)
                {
                    Check(Mathf.Approximately(second.Lifetime.RectTransform.rect.height,second.InitialHeight),"Shared coroutine completes collapse while tip scroll view is inactive");
                    firstRoot=first.Lifetime.GameObject;secondRoot=second.Lifetime.GameObject;first.OnClick=i=>throw new Exception("unused source delegate");
                    fixture.Binding.DisposeItemsAndTabs();Check(fixture.Binding.Tips.Count==0&&first.Lifetime.IsDisposed&&first.Lifetime.GameObject==null&&first.Lifetime.RectTransform&&first.OnClick==null,"Source BaseItem disposal clears references and callback while retaining rect field until native destroy");frame=Time.frameCount;phase=5;return;
                }
                if(phase==5)
                {Check(!firstRoot&&!secondRoot,"Native frame destroys both owned item objects");UnityEngine.Object.Destroy(fixture.Rewards.Container);UnityEngine.Object.Destroy(OutgameUpdateManager.Instance.gameObject);Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex)
        {if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-book-browse-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
