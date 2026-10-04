using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Fixture=AreaBattle.EditorTools.OutgameSevendayEntryValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameSevendayEntryPlayModeValidation
    {
        const string Pending="AreaBattle.SevendayEntryNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original main-page seven-day entry on recovered UI bootstrap, actual control/message/page registry, native entry/task/accumulator/close pointers, red refresh after activity and effect completion, automatic save and independent restart. Claimed prerequisites seeded. Full main-page lifecycle/Main/account/resource/report/audio/effect hosts and Player/original audiovisual acceptance remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static OutgameLimitTaskPage page;static GameObject firstRoot;static string path;static int phase,frame;static double began;static bool stopped;
        static OutgameSevendayEntryPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;frame=Time.frameCount;}
        static void Click(GameObject root)=>OutgameLimitTaskUiItemsValidation.Click(root);
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                if(EventSystem.current==null)new GameObject("EventSystem",typeof(EventSystem));path=Path.Combine(Path.GetTempPath(),"AreaBattleSevenDayEntry-"+Guid.NewGuid().ToString("N"));f=new Fixture(true,path);
                f.ClaimAllPrerequisites();var task=f.Page.Child.FindTask(1301101);task.state=0;f.Page.Data.Runtime.Statistics.Expansion.SetEventCount(10015,10);f.Entry.Initialize();
                Check(f.Entry.Button.gameObject.activeInHierarchy&&f.Entry.RedDot.activeSelf,"Original main seven-day button and red use actual unlocked interval/ready task");
                f.Trace.Clear();Click(f.Entry.Button.gameObject);page=f.Page.Page;
                Check(string.Join(",",f.Trace)=="voice:1:2001,page-owner,button"&&page!=null&&page.GameObject==null,"Native original entry pointer plays source sound then opens typed page before final button notification");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("Seven-day entry phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    if(page.GameObject==null||page.AccItems.Count!=8)return;
                    Check(page.GameObject.transform.parent==f.Page.Layer&&f.Page.Pages.Count==1&&page.DayData.Data.Count==7,"Entry's actual registry finishes original popup load and full seven-day composition");
                    Click(f.Entry.Button.gameObject);Check(ReferenceEquals(page,f.Page.Page)&&f.Page.Provider.RefCount==1,"Repeated native entry pointer reuses registered page and handle");
                    int index=page.TaskData.Data.FindIndex(t=>t.id==1301101);var view=(OutgameLimitTaskView)page.Tasks.List.GetItem(index).BaseItem;Click(view.Claim.gameObject);
                    Check(f.Page.Child.FindTask(1301101).state==1&&f.Page.Data.Items.Global.GetItemCount(1301)==10&&!f.Entry.RedDot.activeSelf,"Actual task completion message clears original main red after final outstanding task claim");
                    f.Page.Child.AccRewards[0].state=0;f.Page.Messages.SendMessage("SevendayUnlock");f.Page.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301});
                    Check(f.Entry.RedDot.activeSelf,"Original unlock event queries actual eligible accumulator red");
                    Click(page.AccItems[1].Lifetime.GameObject);Check(f.Page.Data.Items.Global.GetItemCount(1001)==50&&f.Entry.RedDot.activeSelf,"Actual accumulator award precedes entry red refresh while currency completion remains pending");
                    f.Page.Effects.Completion();Check(!f.Entry.RedDot.activeSelf,"Source currency completion event refreshes actual main red after claim");Phase(1);return;
                }
                if(phase==1)
                {
                    Check(!f.Page.Data.Parent.Dirty&&f.Page.Data.Stored.Length>0,"Real update frames automatically save claimed task and accumulator");firstRoot=page.GameObject;Click(page.CloseButton.gameObject);
                    Check(!f.Page.Pages.ContainsKey(OutgameLimitTaskPage.SourceName)&&f.Entry.Button.gameObject.activeInHierarchy,"Closing popup removes page owner while original main entry remains usable");Phase(2);return;
                }
                if(phase==2)
                {
                    if(!page.Closing.IsCompleted)return;page.Closing.GetAwaiter().GetResult();Check(!firstRoot&&f.Page.Provider.RefCount==0,"Native popup close destroys root and releases original main handle");
                    f.Dispose();f=new Fixture(true,path);f.Entry.Initialize();Click(f.Entry.Button.gameObject);page=f.Page.Page;Phase(3);return;
                }
                if(page.GameObject==null||page.AccItems.Count!=8)return;
                Check(page.AccItems[1].Data.state==1&&f.Page.Child.FindTask(1301101).state==1,"Fresh main entry and actual popup reopen persisted task/accumulator claims from independent disk owner");
                Click(page.AccItems[1].Lifetime.GameObject);Check(f.Page.Data.Items.Host.Adds==0,"Restored claimed accumulator does not grant again through reopened page");
                f.Page.Now=f.Page.Sevenday.GetActDate(false);f.Page.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());
                Check(!f.Entry.Button.gameObject.activeSelf&&page.Countdown.text=="活动结束"&&!page.Lifetime.IsDisposed,"Page end countdown message hides original main entry while source popup remains open");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/sevenday-entry-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
