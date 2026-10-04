using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTimeTaskPlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskNative";
        [Serializable] sealed class ItemRows{public List<SharedItemConfig.GameItemConfig> Datas;}
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Concrete registered task activity, original49 task/item configs, native UpdateManager saving, actual common statistics and liveness reward routes, diagnostic Button pointer claim, fresh file-backed task restart and clock day refresh. Button is a test endpoint, original task page/SevenDay/Main/account/report delivery/Player remain pending. Inventory persistence is covered separately, not by this host.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameLimitTimeTaskValidation.Fixture fixture;static GameObject events;
        static string path;static int phase,atFrame;static double started,phaseAt;static bool stopped;static long now;
        static OutgameChildLimitTimeTaskActivity Child=>fixture.Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
        static OutgameLimitTaskItemData Task=>Child.FindTask(1301101);
        static OutgameLimitTimeTaskPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string why){if(!value)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int value){phase=value;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Create()
        {
            fixture=new OutgameLimitTimeTaskValidation.Fixture(path,true);
            var rows=JsonUtility.FromJson<ItemRows>(Resources.Load<TextAsset>("Recovered/FirstPack/Config/GameItemConfig").text);
            foreach(var row in rows.Datas)fixture.Items.Config.Instance.Items[row.id]=row;
        }
        static void Start()
        {
            report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try
            {
                events=new GameObject("limit-task-native-events",typeof(EventSystem));path=Path.Combine(Path.GetTempPath(),"AreaBattleLimitTaskNative-"+Guid.NewGuid().ToString("N"));Create();
                Check(fixture.Config.NoviceTasks.Count==49&&Child.DayTasks.Count==7&&Task.state==0,"Native original49-task graph initializes seven days through real common activity registration");
                fixture.Runtime.Statistics.Expansion.SetEventCount(10015,10);
                Check(Task.conditions[0].value==10&&Task.BtnState==0&&fixture.Parent.Dirty,"Real statistics set and common event update lifetime progress and eligibility");
                fixture.Runtime.Button.gameObject.SetActive(true);fixture.Runtime.Button.onClick.AddListener(()=>Child.TaskComplete(1301101));
                ExecuteEvents.Execute(fixture.Runtime.Button.gameObject,new PointerEventData(events.GetComponent<EventSystem>()){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
                Check(Task.state==1&&fixture.Items.Global.GetItemCount(1301)==10&&Child.ModuleData.ext.AccProgress==10,"Native pointer endpoint claims original reward through actual activity factory and item engine");
                Check(fixture.ReportHost.Sent.Exists(x=>x.report.DetailType=="活跃度"&&x.report.Values[0]==10),"Liveness event reports10 before task claim state is published");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("limit task native phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.3)return;
                if(phase==0)
                {
                    Check(!fixture.Parent.Dirty&&fixture.Stored.Length>0,"Native Unity updates invoke parent save and clear dirty without manual Update call");
                    fixture.Dispose();fixture=null;Create();
                    Check(Task.state==1&&Task.conditions[0].value==10&&Child.ModuleData.ext.AccProgress==10,"Fresh activity/pool/manager restores claimed task and derived progress from compressed file");
                    Child.TaskComplete(1301101);Check(fixture.Items.Global.GetItemCount(1301)==0&&fixture.Items.Host.Adds==0,"Restored claimed task prevents another award after restart");
                    now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,23,59,0));fixture.Runtime.Statistics.Owner.ValueProviders[10000]=a=>now;
                    Child.Launch();now+=120000;fixture.Runtime.Statistics.Expansion.SetEventCount(10000,now);
                    Check(Child.ModuleData.ext.dayId==2&&fixture.Parent.Dirty,"Actual clock event unlocks next calendar day and schedules save");Phase(1);return;
                }
                Check(!fixture.Parent.Dirty&&fixture.Stored.Length>0,"Next native frames persist changed unlock day");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            try{fixture?.Dispose();fixture=null;if(events!=null)UnityEngine.Object.DestroyImmediate(events);}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-activities-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
