using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameActivityStatesPlayModeValidation
    {
        const string Pending="AreaBattle.ActivityStatesNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual recovered activity FSM/statistics/common-message path, Unity Button pointer handler, Text and native Update clock. ActivityControl/pop queues/UI opening remain explicit fixture hosts. No original activity page, rendered visual acceptance, platform or Player claim.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameActivityStatesValidation.Fixture fixture;static GameObject events;
        static int phase,atFrame;static double started,phaseAt;static string initialText;static bool stopped;
        static OutgameActivityStatesPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Click(){ExecuteEvents.Execute(fixture.Button.gameObject,new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try
            {
                events=new GameObject("activity-pointer-events",typeof(EventSystem));fixture=new OutgameActivityStatesValidation.Fixture(true);fixture.Start();
                Check(fixture.Activity.Fsm.CurrentState is OutgameActivityCloseState&&!fixture.Button.gameObject.activeInHierarchy,"Native close state hides actual button hierarchy");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>30)throw new TimeoutException("activity state phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.3)return;
                if(phase==0)
                {
                    fixture.Set(701,1);Check(fixture.Activity.Fsm.CurrentState is OutgameActivityNoticeState&&fixture.Button.gameObject.activeInHierarchy&&fixture.Text.text=="Need 1"&&fixture.Data.WarmTimeStamp>0,"Actual statistics message enters notice and updates Text, visibility and warm timestamp");
                    Click();Check(fixture.UiHost.Calls==1&&fixture.UiHost.LastName==nameof(OutgameActivityStatesValidation.Fixture),"Unity pointer-click dispatch invokes rebound notice UI host");
                    fixture.Set(702,1);Click();Check(fixture.Activity.Fsm.CurrentState is OutgameActivityLaunchState&&fixture.UiHost.Calls==2&&fixture.UiHost.LastName==nameof(OutgameActivityStatesValidation.Probe)&&!fixture.Text.gameObject.activeInHierarchy,"Launch statistics rebinds actual pointer listener and hides description");
                    fixture.Set(703,1);Check(fixture.Activity.Fsm.CurrentState is OutgameActivityOverState&&!fixture.Button.gameObject.activeInHierarchy&&fixture.Data.launchPop,"Over statistics hides button and persists source flag");
                    fixture.Set(704,1);Check(fixture.Activity.Fsm.CurrentState is OutgameActivityCloseState&&fixture.Activity.Resets==1&&!fixture.Data.launchPop&&!fixture.Data.noticePop,"Close statistics resets progress and popup flags through complete native cycle");
                    var old=fixture.Activity.Fsm;fixture.Dispose();fixture=null;Check(old.IsDestroyed,"Native owner release destroys actual recovered FSM");
                    fixture=new OutgameActivityStatesValidation.Fixture(true);fixture.Config.noticeType=new[]{10000};fixture.Config.noticeParams=new[]{"20261003120000"};fixture.Statistics.Strategy.ReceiveServerTime(new object[]{OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0))-3661000L});fixture.Start(2);initialText=fixture.Text.text;
                    Check(fixture.Activity.Fsm.CurrentState is OutgameActivityNoticeState&&initialText.StartsWith("Need "),"Recreated owner starts saved notice state with original countdown formatting");Phase(1);return;
                }
                if(phase==1)
                {
                    // Native statistics Update sends event10000 once its scaled second elapses.
                    if(fixture.Text.text==initialText)return;
                    Check(((OutgameActivityNoticeState)fixture.Activity.Fsm.CurrentState).DescriptionArgs!=null&&fixture.Text.text!=initialText,"Actual Unity Update advances statistics clock and refreshes notice Text without manual date callback");
                    fixture.Set(702,1);initialText=fixture.Text.text;Phase(2);return;
                }
                if(phase==2)
                {
                    if(EditorApplication.timeSinceStartup-phaseAt<1.5)return;
                    Check(fixture.Activity.Fsm.CurrentState is OutgameActivityLaunchState&&fixture.Text.text==initialText&&!fixture.Text.gameObject.activeSelf,"Leaving notice unregisters native clock text refresh across subsequent frames");
                    Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            try{fixture?.Dispose();fixture=null;if(events!=null)UnityEngine.Object.DestroyImmediate(events);}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/activity-states-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
