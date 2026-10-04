using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Fixture=AreaBattle.EditorTools.OutgameLimitTaskPageValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTaskPagePlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskPageNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original UI bootstrap and composed CommonLimitTimeTaskUI, actual module load/close, day/task/accumulator/preview pointers, activity notifications without manual forwarding, automatic activity save and independent disk restart. Resource/report/localization/sprite/currency/skin/detail hosts remain explicit fixtures; production menu/Main/platform/Player/audiovisual acceptance pending.";
            public int requestFrame,openFrame,hideFrame,closeFrame;public List<string> checks=new List<string>();
        }
        static Fixture f;static OutgameLimitTaskPage page;static Report report;static GameObject oldRoot;static OutgameLimitTaskAccItem oldAcc;
        static int phase,frame;static double began;static bool stopped;static string path;
        static OutgameLimitTaskPagePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;frame=Time.frameCount;}
        static OutgameLimitTaskPageView Day(int index)=>(OutgameLimitTaskPageView)page.Days.List.GetItem(index).BaseItem;
        static void Click(GameObject root)=>OutgameLimitTaskUiItemsValidation.Click(root);
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                if(EventSystem.current==null)new GameObject("EventSystem",typeof(EventSystem));
                path=Path.Combine(Path.GetTempPath(),"AreaBattleOriginalTaskPage-"+Guid.NewGuid().ToString("N"));f=new Fixture(true,path);
                f.Messages.AddListener("OpenUI",args=>report.openFrame=Time.frameCount);
                f.Messages.AddListener("GF_VisibleUI",args=>{if(!(bool)args[1]){report.hideFrame=Time.frameCount;Check(page.Lifetime.IsDisposed&&page.GameObject&&f.Provider.RefCount==1,"Async close marks disposed and hides before page business cleanup/handle release");}});
                f.Messages.AddListener("CloseUI",args=>{report.closeFrame=Time.frameCount;Check(!oldRoot&&page.Lifetime.IsDisposed&&f.Provider.RefCount==0,"CloseUI follows root destruction and main resource release");});
                report.requestFrame=Time.frameCount;page=f.Open("native-argument");Check(page.GameObject==null&&page.MainHandle!=null&&f.Pages.ContainsKey(OutgameLimitTaskPage.SourceName),"Page registry and resource handle precede original two-frame load");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("LimitTask page phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    if(report.openFrame==0||page.AccItems.Count!=8)return;
                    Check(report.openFrame>=report.requestFrame+2&&page.GameObject.transform.parent==f.Layer&&page.DayData.Data.Count==7&&page.SelectedDay==1&&f.ReportHost.Sent.Count==1,"Native module load initializes original popup, seven-day list and report before completed open");
                    Click(Day(1).Normal.gameObject);Check(page.SelectedDay==2&&f.Child.ModuleData.ext.lastClickDayId==2&&page.TaskData.Data.TrueForAll(t=>t.Config.day==2),"Actual day pointer selects day2 and refreshes complete page without fixture forwarding");
                    Click(Day(0).Normal.gameObject);f.Data.Runtime.Statistics.Expansion.SetEventCount(10015,10);Phase(1);return;
                }
                if(phase==1)
                {
                    int index=page.TaskData.Data.FindIndex(t=>t.id==1301101);var task=(OutgameLimitTaskView)page.Tasks.List.GetItem(index).BaseItem;oldAcc=page.AccItems[1];Click(task.Claim.gameObject);
                    Check(f.Child.FindTask(1301101).state==1&&f.Data.Items.Global.GetItemCount(1301)==10&&page.AccNumber.text=="10"&&oldAcc.Lifetime.IsDisposed,"Actual task claim drives page's own activity listeners and accumulator rebuild");
                    var acc=page.AccItems[1];EventSystem.current.SetSelectedGameObject(acc.Lifetime.GameObject);Click(acc.Lifetime.GameObject);Check(page.Preview.Visible&&page.Preview.Items.Count==0,"Unready accumulator opens complete page preview before actual frame await");Phase(2);return;
                }
                if(phase==2)
                {
                    Check(page.Preview.Items.Count==2&&page.Preview.Items[0].Count.text=="50"&&page.Preview.Visible,"Original Node selection retains frame-built reward preview");
                    EventSystem.current.SetSelectedGameObject(null);foreach(var list in f.Child.DayTasks.Values)foreach(var task in list)task.state=1;
                    f.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301});oldAcc=page.AccItems[1];Click(oldAcc.Lifetime.GameObject);
                    Check(oldAcc.Data.state==1&&oldAcc.Lifetime.IsDisposed&&f.Data.Items.Global.GetItemCount(1001)==50&&page.AccItems[1].Got!=null,"Actual accumulator claim synchronously rebuilds its own page; source handler completes after reference disposal");
                    Click(page.AccItems[1].Lifetime.GameObject);f.Effects.Completion();Check(f.Data.Items.Global.GetItemCount(1001)==50,"Newly rebuilt claimed accumulator prevents duplicate reward");Phase(3);return;
                }
                if(phase==3)
                {
                    Check(!page.Preview.Visible&&!f.Data.Parent.Dirty&&f.Data.Stored.Length>0,"Native selection update hides preview and activity manager automatically saves both claims");
                    f.Now=f.Sevenday.GetActDate(false);f.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());Check(page.Countdown.text=="活动结束"&&f.Ended>0&&!page.Lifetime.IsDisposed,"Source end message leaves original page open");
                    oldRoot=page.GameObject;page.CloseAction=()=>Check(f.Pages.ContainsKey(OutgameLimitTaskPage.SourceName),"Close callback precedes registry removal");Click(page.CloseButton.gameObject);
                    Check(!f.Pages.ContainsKey(OutgameLimitTaskPage.SourceName)&&page.Closing!=null,"Original close button removes registered owner and starts BaseUI async close");Phase(4);return;
                }
                if(phase==4)
                {
                    if(!page.Closing.IsCompleted)return;page.Closing.GetAwaiter().GetResult();Check(report.closeFrame>report.hideFrame&&page.AccItems.Count==8&&page.DayData.Data.TrueForAll(d=>!d.Selected),"Frame-separated close retains source collections and clears day selection");
                    f.Dispose();f=new Fixture(true,path);page=f.Open();Phase(5);return;
                }
                if(page.GameObject==null||page.AccItems.Count!=8)return;
                Check(page.AccItems[1].Data.state==1&&f.Child.FindTask(1301101).state==1&&page.AccNumber.text==f.Child.ModuleData.ext.AccProgress.ToString(),"Independent file restart and complete page reopen restore task/accumulator claims and derived liveness");
                Click(page.AccItems[1].Lifetime.GameObject);Check(f.Data.Items.Host.Adds==0&&f.Data.Items.Global.GetItemCount(1001)==0,"Reopened claimed accumulator prevents new award with independently reset inventory host");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-page-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
