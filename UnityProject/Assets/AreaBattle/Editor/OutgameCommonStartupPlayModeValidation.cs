using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameCommonStartupPlayModeValidation
    {
        const string Pending="AreaBattle.CommonStartupNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual ProcedurePreLoad native WaitUntil, StatisticsRuntime and ActivityRuntime with original49 tasks, pending server callback, two explicit account flags, FSM transition, delayed scene-completion endpoint, SevenDay seeding/red and native automatic save. Account/network/UI/scene/report hosts remain explicit fixtures; full Main and original task pages/Player are not claimed.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameCommonStartupValidation.Fixture fixture;
        static OutgameProcedureManager procedures;static OutgameProcedurePreLoad preload;static OutgameCommonStartupValidation.Next next;
        static int phase,atFrame,arena;static double started,phaseAt;static bool stopped;
        static OutgameCommonStartupPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string why){if(!value)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int value){phase=value;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Start()
        {
            report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try
            {
                fixture=new OutgameCommonStartupValidation.Fixture();fixture.StorageHost.Server=true;
                arena=fixture.Legacy.statisticEventConfig.ArenaRank;
                procedures=new OutgameProcedureManager(()=>new OutgameFsmManager());next=new OutgameCommonStartupValidation.Next{Entering=fixture.Entry.Enter};
                preload=new OutgameProcedurePreLoad(fixture.Host,typeof(OutgameCommonStartupValidation.Next));procedures.Register(preload,next);procedures.StartProcedure<OutgameProcedurePreLoad>();
                Check(fixture.Downloads.Count==1&&fixture.Statistics.Manager.Data==null&&fixture.Control.Manager==null,"Preload requests statistics data and leaves activities uninitialized while source callback is pending");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("common startup phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.3)return;
                if(phase==0)
                {
                    Check(next.Enters==0&&fixture.Control.Manager==null,"Native frames cannot bypass pending statistics response");
                    fixture.StorageHost.Server=false;fixture.Statistics.Manager.UpdateDataCallBack("");
                    Check(fixture.Control.InitData!=null&&fixture.Config.Manager.NoviceTasks.Count==49&&fixture.Child.DayTasks.Count==7,"Real statistics callback initializes actual activity/task graph through Init(null)");
                    Phase(1);return;
                }
                if(phase==1)
                {
                    Check(next.Enters==0&&fixture.Flag65Reads==0&&procedures.ProcedureFsm.CurrentState==preload,"Native WaitUntil preserves preload and short-circuits second login flag while first false");
                    fixture.Flag64=true;Phase(2);return;
                }
                if(phase==2)
                {
                    Check(next.Enters==0&&fixture.Flag65Reads>0,"First ready flag alone cannot advance startup");fixture.Flag65=true;Phase(3);return;
                }
                if(phase==3)
                {
                    if(next.Enters==0)return;
                    Check(next.Enters==1&&fixture.Trace.Contains("repair")&&fixture.EntryHost.Loaded!=null&&fixture.Seven.Child==null,"Both flags resume actual FSM and startup; seven-day binding still waits for scene callback");
                    fixture.EntryHost.Loaded();
                    Check(ReferenceEquals(fixture.Seven.Child,fixture.Child)&&fixture.Child.FindTask(1301101).conditions[0].value==10,"Delayed scene completion seeds original seven-day lifetime task from actual statistic10015");
                    Check(fixture.Statistics.Expansion.EventCount(fixture.Legacy.statisticEventConfig.HaveSkinNum)==3,"Original81-row skin catalog seeds three initially unlocked soldier skins");
                    Check(fixture.EntryHost.Trace.Contains("state:1")&&fixture.EntryHost.Trace[fixture.EntryHost.Trace.Count-1]=="interactive","Play state, loading close and interactive endpoint follow seven-day initialization");
                    fixture.Legacy.statisticEventConfig.ArenaRank=9876;fixture.Statistics.Expansion.SetEventCount(9876,77);
                    Check(fixture.Statistics.Expansion.GameValue(arena,new object[]{"ignored"})==77,"Original arena registration key retains provider that rereads current config event");
                    Phase(4);return;
                }
                var parent=fixture.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);
                if(parent.Dirty)return;
                string saved=fixture.Strings.GetString(fixture.StorageHost.MineGameName+parent.Manager.DataKey,"");
                Check(saved.Length>0&&fixture.Child.ModuleData.datas.Exists(t=>t.id==1301101&&t.conditions[0].value==10),"Subsequent native common controller updates save newly seeded task progress to actual compressed file");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            try{fixture?.Dispose();fixture=null;}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/sevenday-startup-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
