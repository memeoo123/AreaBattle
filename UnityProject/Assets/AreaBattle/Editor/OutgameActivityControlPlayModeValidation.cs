using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameActivityControlPlayModeValidation
    {
        const string Pending="AreaBattle.ActivityControlNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual ActivityControl/config/offline manager/FSM, native UpdateManager, Button pointer dispatch, popup lifecycle messages, compressed file save/restart and cleanup. Original page assets/report delivery/account/Main/concrete activity gameplay/Player remain pending; scheduler registration uses an explicit captured endpoint.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameActivityControlValidation.Fixture fixture;static GameObject events;
        static string path;static long launch;static int phase,atFrame;static double started,phaseAt;static bool stopped;
        static OutgameActivityControlPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Click(){ExecuteEvents.Execute(fixture.Button.gameObject,new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
        static void Start()
        {
            report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try
            {
                events=new GameObject("activity-control-pointer-events",typeof(EventSystem));path=Path.Combine(Path.GetTempPath(),"AreaBattleControlNative-"+Guid.NewGuid().ToString("N"));
                fixture=new OutgameActivityControlValidation.Fixture(path,true);fixture.Init();
                Check(fixture.Config.Manager.Completed==5&&fixture.Data.state==1&&fixture.Control.ActivityList.Count==1&&!fixture.Button.gameObject.activeInHierarchy,"Native initialization loads full custom config roster, publishes offline activity and hides closed button");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>30)throw new TimeoutException("activity control phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.3)return;
                if(phase==0)
                {
                    Check(!fixture.Control.IsDirty&&fixture.Stored.Length>0,"Actual Unity UpdateManager saves initial dirty activity without manual control.Update");
                    fixture.Set(701,1);Check(fixture.Data.state==2&&fixture.Data.WarmTimeStamp>0&&fixture.Control.NoticeQueue.Count==1&&fixture.Text.text=="Need 1","Actual statistics event invokes control notice report/timestamp and FSM widget updates");
                    Click();Check(fixture.UiHost.Opened.Count==1,"Native pointer-click dispatch opens bound notice UI through recovered activity callback");
                    fixture.Row.noticeAutoPop=1;fixture.UiHost.OpenAction=()=>fixture.Statistics.Messages.SendMessage("OpenUI",new object[]{fixture.UiHost.Result});
                    Phase(1);return;
                }
                if(phase==1)
                {
                    Check(fixture.Control.NoticeQueue.Count==0&&fixture.Control.NoticeUi==fixture.UiHost.Result&&fixture.Data.noticePop&&!fixture.Control.IsDirty,"Native control Update automatically opens queued popup, handles reentrant OpenUI, dequeues and saves flag");
                    fixture.Statistics.Messages.SendMessage("CloseUI",new object[]{fixture.UiHost.Result});
                    Check(fixture.Control.NoticeUi==fixture.UiHost.Result,"Original automatic popup close retains UI reference after queue was removed");
                    fixture.Set(702,1);launch=fixture.Data.LaunchTimeStamp;
                    Check(fixture.Data.state==3&&launch>0&&fixture.Data.WarmTimeStamp==0&&fixture.Control.LaunchQueue.Count==1&&!fixture.Text.gameObject.activeInHierarchy,"Native launch transition resets warm timestamp, queues launch and hides description");
                    Click();Check(fixture.UiHost.Opened.Count==3,"Native launch pointer listener replaced notice listener");Phase(2);return;
                }
                if(phase==2)
                {
                    Check(!fixture.Control.IsDirty&&fixture.Stored.Length>0,"Launch data saved by subsequent native update frames");
                    var old=fixture.Activity7.Fsm;fixture.Dispose();fixture=null;Check(old.IsDestroyed,"Controller cleanup destroys recovered activity FSM and removes its frame subscription");
                    fixture=new OutgameActivityControlValidation.Fixture(path,true);fixture.Init();
                    Check(fixture.Data.state==3&&fixture.Data.LaunchTimeStamp==launch&&fixture.Data.noticePop,"Independent owner graph restores compressed activity state, timestamp and popup flag");Phase(3);return;
                }
                if(phase==3)
                {
                    fixture.Set(703,1);Check(fixture.Data.state==4&&fixture.Data.LaunchTimeStamp==0&&fixture.ReportHost.Trace.Contains("send:Complete"),"Native over transition dispatches completion report endpoint and clears timestamps");
                    fixture.Set(704,1);Check(fixture.Data.state==1&&!fixture.Data.noticePop&&!fixture.Button.gameObject.activeInHierarchy,"Native close transition resets popup flags and hides button");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            try{fixture?.Dispose();fixture=null;if(events!=null)UnityEngine.Object.DestroyImmediate(events);}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/activity-control-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
