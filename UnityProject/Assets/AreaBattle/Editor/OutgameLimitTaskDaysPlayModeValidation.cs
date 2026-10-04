using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Fixture=AreaBattle.EditorTools.OutgameLimitTaskDaysValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTaskDaysPlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskDaysNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual imported CommonLimitTimeTaskUI day scroll and original day-item prefabs, native DynamicList LateUpdate, pointer selection, concrete limited-task statistics/red notifications and disposal. Full task/accumulator/preview/page lifecycle/menu/Main/Player and audiovisual acceptance pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture fixture;static Report report;static GameObject firstRoot;static OutgameLimitTaskPageView first;static int phase,frame;static double began;static bool stopped;
        static OutgameLimitTaskDaysPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
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
                frame=Time.frameCount;EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>30)throw new TimeoutException("LimitTask day-list native phase "+phase);
                if(Time.frameCount<=frame+2)return;
                if(phase==0)
                {
                    first=fixture.At(0);var second=fixture.At(1);var third=fixture.At(2);
                    Check(first.NormalName.text=="1天"&&second.NormalName.text=="2天"&&third.Locked.gameObject.activeInHierarchy&&fixture.Binding.List.Scroll.content==fixture.Binding.List.transform,"Native LateUpdate renders original day rows in correct same-name scroll view");
                    OutgameLimitTaskUiItemsValidation.Click(first.Normal.gameObject);OutgameLimitTaskUiItemsValidation.Click(second.Normal.gameObject);
                    Check(fixture.SelectedDays.Count==2&&fixture.SelectedDays[0]==1&&fixture.SelectedDays[1]==2&&second.Selected.gameObject.activeInHierarchy&&first.Normal.gameObject.activeInHierarchy,"Original native button pointers dispatch page selection and deselect previous day");
                    Check(third.Locked.GetComponent<UnityEngine.UI.Button>()==null&&!third.Normal.gameObject.activeInHierarchy,"Locked original day has no active normal button");
                    foreach(var tasks in fixture.Child.DayTasks.Values)foreach(var entry in tasks)entry.state=1;
                    fixture.Child.FindTask(1301101).state=0;fixture.Tasks.Runtime.Statistics.Expansion.SetEventCount(10015,10);
                    Check(first.Red.activeSelf&&fixture.Child.FindTask(1301101).conditions[0].value==10,"Actual statistic10015 updates original task and subscribed day red point");
                    fixture.SelectedDays.Clear();fixture.Binding.Data.UpdateList();frame=Time.frameCount;phase=1;return;
                }
                if(phase==1)
                {
                    Check(fixture.SelectedDays.Count==1&&fixture.SelectedDays[0]==2&&fixture.At(1).Selected.gameObject.activeInHierarchy,"Native refresh renders data then replays selected page callback");
                    fixture.Child.FindTask(1301101).state=1;fixture.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301});
                    Check(!first.Red.activeSelf,"Actual common activity refresh clears completed day's red point");
                    firstRoot=first.Lifetime.GameObject;fixture.Binding.Data.KillSelect();first.Dispose();
                    Check(firstRoot&&first.Lifetime.GameObject==null&&first.Lifetime.IsDisposed,"Dynamic item disposal clears ownership while preserving native pooled row");
                    int before=fixture.SelectedDays.Count;first.Normal.onClick.Invoke();Check(fixture.SelectedDays.Count==before,"Disposed original button removes selection handler");
                    frame=Time.frameCount;phase=2;return;
                }
                Check(firstRoot,"Non-recycling source row survives disposal across native frames until page owner destroys it");fixture.Dispose();Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-days-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
