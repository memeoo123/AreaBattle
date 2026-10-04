using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameTaskActivityValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskActivityPlayModeValidation
    {
        const string Pending="AreaBattle.TaskActivityNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Concrete ordinary and seven-day activity graph, actual statistics dispatch/item engine, native UpdateManager frames, readiness-gated automatic task save, independent disk restart and daily boundary. Claims invoke restored activity methods; task UI/pointers, production Main/account/effect/report hosts and Player/original audiovisual acceptance remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static string path;static long uid,boundary;static int phase,frame;static double began;static bool stopped;
        static OutgameTaskActivityPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int value){phase=value;frame=Time.frameCount;}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                path=Path.Combine(Path.GetTempPath(),"AreaBattleTasksNative-"+Guid.NewGuid().ToString("N"));f=new Fixture(path,true,true);
                Check(f.Child.ModuleData.tasks.Count==8&&f.Child.Fsm!=null&&f.Parent.ChildrenById[1301001] is OutgameLimitTimeTaskActivity,"Original eight daily tasks and actual seven-day parent share the recovered root/FSM graph");
                f.Items.Config.Instance.Items[7001]=new AreaBattle.SharedItemConfig.GameItemConfig{id=7001,type1=10,type2=4,paramInt=130001,item_game="g"};
                var item=f.Items.Engine.GetItem(f.Items.Config.Instance.Items[7001]);item.AddItem(1);
                Check(item is OutgameTaskIapRefreshItem&&f.Items.Global.GetItemCount(7001)==1&&f.Items.Reports.Uses.Count==1,"Actual child task item factory awards type10/4 item then invokes source use/report");
                f.Runtime.Statistics.Pool.SourceReadyFlag=false;var task=f.Child.ModuleData.tasks.Single(t=>t.id==1);uid=task.uid;f.Event(task.Config.conditionParams[0].datas[0],1);
                Check(task.CanComplete()&&f.Parent.Dirty,"Real common statistics event updates original daily record and marks parent dirty");
                f.Child.TaskComplete(uid);Check(task.state==1&&f.Items.Global.GetItemCount(1001)==50&&f.Child.ModuleData.ext.livenessValue==20,"Restored activity claim applies actual item model and liveness factory event");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("Task native phase "+phase);
                if(Time.frameCount<=frame+4)return;
                if(phase==0)
                {
                    Check(f.Parent.Dirty&&f.Stored=="","Native frames retain dirty data while actual pool readiness is false");
                    f.Runtime.Statistics.Pool.SourceReadyFlag=true;Phase(1);return;
                }
                if(phase==1)
                {
                    if(f.Parent.Dirty)return;
                    Check(f.Stored.Length>0,"Native UpdateManager/ActivityControl root update automatically writes task storage");
                    var limited=f.Runtime.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);var child=limited.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
                    f.Runtime.Statistics.Expansion.SetEventCount(10015,10);child.TaskComplete(1301101);Check(child.FindTask(1301101).state==1&&limited.Dirty,"Seven-day task claim remains functional in same root graph");Phase(2);return;
                }
                if(phase==2)
                {
                    var limited=f.Runtime.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);if(limited.Dirty)return;
                    Check(!limited.Dirty,"Native parent traversal also automatically saves limited-task child manager");
                    f.Dispose();f=new Fixture(path,true,true);var task=f.Child.ModuleData.tasks.Single(t=>t.id==1);
                    Check(task.uid==uid&&task.state==1&&task.conditions[0].value==1&&f.Child.ModuleData.ext.livenessValue==20,"Independent actual file owner restores daily task UID/progress/claim and liveness");
                    limited=f.Runtime.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);
                    Check(limited.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301).FindTask(1301101).state==1,"Independent limited-task manager restores seven-day claim alongside daily data");
                    f.Child.TaskComplete(uid);Check(f.Items.Host.Adds==0,"Restored claimed daily task cannot award again");
                    boundary=f.Child.ModuleData.ext.nextRefreshTimeStamp;f.Now=boundary;f.Runtime.Statistics.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());
                    Check(f.Child.ModuleData.tasks.Any(t=>t.uid==uid)&&f.Child.ModuleData.ext.nextRefreshTimeStamp==boundary,"Daily refresh retains tasks at exact boundary");
                    f.Now++;f.Runtime.Statistics.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());
                    Check(f.Child.ModuleData.tasks.Count==8&&f.Child.ModuleData.tasks.All(t=>t.state==0)&&f.Child.ModuleData.ext.livenessValue==0&&f.Child.ModuleData.ext.nextRefreshTimeStamp>boundary,"Next millisecond regenerates original eight tasks and resets liveness");Phase(3);return;
                }
                if(f.Parent.Dirty)return;
                Check(f.Stored.Length>0&&!f.Parent.Dirty,"Native frames save refreshed task generation after day transition");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-activity-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
