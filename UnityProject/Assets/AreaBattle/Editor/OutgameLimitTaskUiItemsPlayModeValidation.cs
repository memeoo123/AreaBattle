using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameLimitTaskUiItemsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTaskUiItemsPlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskUiItemsNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original limited-task reward/progress objects in native PlayMode with Canvas, pointer callbacks, reentry and deferred Object.Destroy. Sprite and popup are explicit observed fixture endpoints; complete page, production entry and original audiovisual acceptance remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture fixture;static Report report;static GameObject rewardRoot,progressRoot;static int phase,frame;static double began;static bool stopped;
        static OutgameLimitTaskUiItemsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=EditorApplication.timeSinceStartup;
            try{
                var camera=new GameObject("ValidationCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);
                new GameObject("EventSystem",typeof(EventSystem));fixture=new Fixture(true);
                var canvas=fixture.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;fixture.Root.AddComponent<GraphicRaycaster>();
                fixture.Reward.SetData(new OutgameItemReward{itemId=1001,itemCount=1234567890123});
                fixture.Progress.SetData(new OutgameLimitTaskCondition{value=7},10);
                frame=Time.frameCount;EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>30)throw new TimeoutException("LimitTask items native phase "+phase);
                if(Time.frameCount<=frame+2)return;
                if(phase==0)
                {
                    Check(fixture.Reward.Count.text=="1234567890123"&&fixture.Progress.Value.text=="7 / 10"&&Mathf.Approximately(fixture.Progress.Progress.fillAmount,.7f),"Original UGUI text and progress state survive native frames");
                    int clicks=0;fixture.Progress.OnClick=item=>clicks++;OutgameLimitTaskUiItemsValidation.Click(fixture.ProgressRoot);
                    Check(clicks==1,"Native pointer handler reaches progress self callback");
                    fixture.Trace.Clear();fixture.Reward.OnClick=item=>{fixture.Trace.Add("callback");item.SetData(new OutgameItemReward{itemId=1002,itemCount=8});};
                    OutgameLimitTaskUiItemsValidation.Click(fixture.RewardRoot);
                    Check(string.Join(",",fixture.Trace)=="callback,sprite,popup:1002"&&fixture.Reward.Count.text=="8","Native reward pointer runs callback before live detail request");
                    rewardRoot=fixture.RewardRoot;progressRoot=fixture.ProgressRoot;var rewardRect=fixture.Reward.Lifetime.RectTransform;var progressRect=fixture.Progress.Lifetime.RectTransform;
                    fixture.Reward.Dispose();fixture.Progress.Dispose();
                    Check(fixture.Reward.Lifetime.GameObject==null&&fixture.Progress.Lifetime.GameObject==null&&fixture.Reward.OnClick==null&&fixture.Progress.OnClick==null&&ReferenceEquals(rewardRect,fixture.Reward.Lifetime.RectTransform)&&ReferenceEquals(progressRect,fixture.Progress.Lifetime.RectTransform),"Source disposal clears ownership and delegates while retaining rect references");
                    Check(rewardRoot&&progressRoot,"Object.Destroy leaves native objects alive until frame end");
                    frame=Time.frameCount;phase=1;return;
                }
                Check(!rewardRoot&&!progressRoot,"Next native frames destroy both owned original item objects");fixture.Dispose();Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-ui-items-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
