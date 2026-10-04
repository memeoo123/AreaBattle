using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Fixture=AreaBattle.EditorTools.OutgameLimitTaskRowsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTaskRowsPlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskRowsNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual original CommonLimitTimeTaskUI task/day columns, source task-row rendering, native original claim button, concrete task activity/liveness award, source list refresh and native automatic task save/restart. Sprite/localization/detail hosts are observed test endpoints; full page lifecycle/accumulator/preview/menu/Main and original audiovisual acceptance pending. Inventory persistence is a separate host contract.";
            public List<string> checks=new List<string>();
        }
        static Fixture fixture;static Report report;static GameObject oldReward,oldProgress;static int phase,frame;static double began;static bool stopped;static string path;
        static OutgameLimitTaskRowsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;frame=Time.frameCount;}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                var camera=new GameObject("ValidationCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);
                new GameObject("EventSystem",typeof(EventSystem));path=Path.Combine(Path.GetTempPath(),"AreaBattleOriginalTaskRows-"+Guid.NewGuid().ToString("N"));
                fixture=new Fixture(true,path);fixture.AutoRefresh=true;
                Check(fixture.Days.Tasks.Config.NoviceTasks.Count==49&&fixture.Child.DayTasks.Count==7,"Original49 tasks and seven day groups feed both original page columns");
                fixture.Days.Tasks.Runtime.Statistics.Expansion.SetEventCount(10015,10);Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>45)throw new TimeoutException("LimitTask row native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    int index=fixture.Binding.Data.Data.FindIndex(t=>t.id==1301101);var view=fixture.At(index);
                    Check(view.Claim.gameObject.activeInHierarchy&&view.ProgressItems[0].Value.text=="10 / 10"&&view.RewardItems.Count==fixture.Task.RewardsData.Count,"Native list refresh displays original ready task and its progress/reward children");
                    oldReward=view.RewardItems[0].Lifetime.GameObject;oldProgress=view.ProgressItems[0].Lifetime.GameObject;fixture.Trace.Clear();
                    OutgameLimitTaskUiItemsValidation.Click(view.Claim.gameObject);
                    Check(fixture.Task.state==1&&fixture.Days.Tasks.Items.Global.GetItemCount(1301)==10&&fixture.Child.ModuleData.ext.AccProgress==10,"Original native claim pointer reaches actual activity and liveness reward model");
                    Check(fixture.Trace.Contains("finished:1:1301101")&&fixture.Binding.List.IsDirty,"Activity list notification precedes finish message and schedules source list rendering");
                    OutgameLimitTaskUiItemsValidation.Click(view.Claim.gameObject);
                    Check(fixture.Days.Tasks.Items.Global.GetItemCount(1301)==10&&fixture.Trace.FindAll(s=>s.StartsWith("finished:")).Count==2,"Same-frame duplicate pointer emits source finish again without duplicate economic reward");
                    Phase(1);return;
                }
                if(phase==1)
                {
                    Check(!oldReward&&!oldProgress,"Reused task row rebuild destroys previous native reward and progress objects at frame end");
                    Check(!fixture.Days.Tasks.Parent.Dirty&&fixture.Days.Tasks.Stored.Length>0,"Actual UpdateManager frames automatically save claimed task through parent manager");
                    fixture.Dispose();fixture=new Fixture(true,path);fixture.AutoRefresh=true;
                    Check(fixture.Task.state==1&&fixture.Task.conditions[0].value==10&&fixture.Child.ModuleData.ext.AccProgress==10,"Fresh original activity and manager restore claimed task/progress from actual disk");
                    int index=fixture.Binding.Data.Data.FindIndex(t=>t.id==1301101);fixture.Binding.List.CenteredWithIndex(index);Phase(2);return;
                }
                int restoredIndex=fixture.Binding.Data.Data.FindIndex(t=>t.id==1301101);var restored=fixture.At(restoredIndex);
                Check(restored.Claimed.gameObject.activeInHierarchy&&!restored.Claim.gameObject.activeSelf,"Reopened original task row renders claimed state after native centering");
                restored.Claim.onClick.Invoke();Check(fixture.Days.Tasks.Items.Global.GetItemCount(1301)==0&&fixture.Days.Tasks.Items.Host.Adds==0,"Restored task prevents another award through the row's actual handler");
                Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);
            try{fixture?.Dispose();fixture=null;}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-rows-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
