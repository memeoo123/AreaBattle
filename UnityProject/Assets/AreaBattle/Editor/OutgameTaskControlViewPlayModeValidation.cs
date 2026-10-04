using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTaskControlViewValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskControlViewPlayModeValidation
    {
        const string Pending="AreaBattle.TaskControlViewNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual task/Achievement controller/activity owners, original main entrance and achievement row, real EventSystem pointer and scaled .7s slide, real award and next-row projection, data-pool save and independent file restart. Full TaskSingleton/TaskPanel tabs/lifecycle/Main/platform and production effects/reports remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static OutgameTaskRow row;static Report report;static string path;static int phase,frame;static double began;static bool stopped;static float x;
        static OutgameTaskControlViewPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;}
        static void Click(GameObject target)=>ExecuteEvents.Execute(target,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Canvas(){var c=f.Rows.Root.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;f.Rows.Root.AddComponent<GraphicRaycaster>();}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                new GameObject("EventSystem",typeof(EventSystem));new GameObject("Camera",typeof(Camera)).transform.position=new Vector3(0,0,-10);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskControlNative-"+Guid.NewGuid().ToString("N"));f=new Fixture(true,path);Canvas();
                f.Level=9;f.Control.RefreshData(null);Check(!f.Entrance.Button.gameObject.activeSelf,"Original main task entrance hidden before level10");
                f.Level=10;f.Control.RefreshData(null);Check(f.Entrance.Button.gameObject.activeSelf,"Original main task entrance enabled at level10");
                f.Tasks.Runtime.Statistics.Owner.ValueProviders[10015]=args=>30;f.Tasks.Runtime.Statistics.Common.SendMessage(OutgameAchievementStrategy.StatisticsRefresh);f.Open();row=f.View.Rows[1];
                Check(row.Data.id==1&&row.Data.number==30&&row.Data.prog==30&&row.Claim.gameObject.activeInHierarchy,"Real original first achievement renders ready under achievement content");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("Task controller native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    // Let the original layout group finish its first rendered layout before
                    // measuring motion; initial layout changes position independently of timeScale.
                    Time.timeScale=0;x=row.Lifetime.Transform.localPosition.x;EventSystem.current.SetSelectedGameObject(row.Claim.gameObject);Click(row.Claim.gameObject);
                    Check(!row.Claim.enabled&&f.Achievement.FindAchievement(1).state==0&&f.Tasks.Items.Global.GetItemCount(1001)==0,"Native pointer disables claim before scaled slide and economy");Phase(1);return;
                }
                if(phase==1)
                {
                    Check(f.Achievement.FindAchievement(1).state==0&&row.Lifetime.Transform.localPosition.x==x,"Real Unity frames at timeScale0 preserve paused slide and unclaimed state");
                    int count=f.Rows.Trace.Count;Click(row.Claim.gameObject);Check(f.Rows.Trace.Count==count,"Disabled native Button ignores repeat pointer");Time.timeScale=1;Phase(2);return;
                }
                if(phase==2)
                {
                    if(f.Achievement.FindAchievement(1).state!=1)return;
                    Check(f.Tasks.Items.Global.GetItemCount(1001)==200&&f.Achievement.FindAchievement(1).time==f.Tasks.Now,"Native slide completion awards original200 gold and records actual achievement time");
                    Check(ReferenceEquals(row,f.View.Rows[1])&&row.Data.id==2&&row.Data.number==50&&row.Lifetime.GameObject.activeSelf&&row.Claim.enabled,"Same native type row advances to second original achievement after claim");
                    Check(f.Rows.Reported.Count==1&&f.Rows.Reported[0][0]=="Achievement"&&f.Rows.Reported[0][1]=="2","Source UI report reads current next-row ID after callback reentry");
                    f.Tasks.Runtime.Statistics.Pool.SaveData();f.Dispose();f=new Fixture(true,path);Canvas();f.Open();
                    Check(f.Achievement.FindAchievement(1).state==1&&f.View.Rows[1].Data.id==2&&f.Achievement.AchievementMap.Count==127,"Independent storage/activity/view startup restores claim and next visible original row");
                    f.Trace.Clear();f.View.OnDestroy();f.Achievement.RefreshSortList();Check(f.Trace.Count==1&&f.ClearSingleton==1,"Destroyed subview clears owner and unsubscribes from live manager refresh");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-control-view-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
