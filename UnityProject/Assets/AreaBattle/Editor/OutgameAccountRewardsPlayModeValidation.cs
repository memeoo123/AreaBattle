using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameAccountRewardsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAccountRewardsPlayModeValidation
    {
        const string Pending="AreaBattle.AccountRewardsNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;public long restartedGlobalGold,restartedLocalGold;
            public string scope="Original main button, entire task page resource/open/close, daily/achievement Toggle pointer routing, actual shared account currency and persisted task/achievement records, shared subview replacement, TopInfo restoration and independent restart. Actual ItemManager/LocalDataManager/file storage and activity reward binding; explicit account login/acquisition/audio/localization/effect/report endpoints; production Main/account/platform and original audiovisual/Player acceptance remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static OutgameTaskEntryBinding entry;static Report report;static OutgameTaskPage oldPage;static OutgameDailyTaskView oldDaily;static string path;static int phase,frame;static double began;static bool stopped;
        static OutgameAccountRewardsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){OutgameAccountRewardsValidation.PrepareConfig();SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;}
        static void Click(GameObject target)=>ExecuteEvents.Execute(target,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Compose()
        {
            f=new Fixture(true,path);var canvas=f.Page.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=f.Page.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);f.Page.Root.AddComponent<GraphicRaycaster>();
            entry=new OutgameTaskEntryBinding(f.Page.Data.Main.transform,new OutgameTaskEntryServices{Control=()=>f.Page.Data.Control,Pages=()=>f.Page.OpenRegistry,FindPage=()=>f.Page.Pages.TryGetValue(OutgameTaskPage.SourceName,out var p)?p:null,Messages=f.Page.Ui.Messages,PlayVoice=(kind,id)=>{if(kind!=1||id!=2001)throw new Exception("voice arguments");}});entry.Initialize();
            f.Statistics.Owner.ValueProviders[10015]=args=>30;f.Statistics.Common.SendMessage(OutgameAchievementStrategy.StatisticsRefresh);
        }
        static void Start()
        {
            stopped=false;report=new Report();began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                new GameObject("EventSystem",typeof(EventSystem));new GameObject("Camera",typeof(Camera)).transform.position=new Vector3(0,0,-10);
                OutgameAccountRewardsValidation.LoadPreparedConfig();
                path=Path.Combine(Path.GetTempPath(),"AreaBattleAccountRewardsNative-"+Guid.NewGuid().ToString("N"));Compose();
                var task=f.Page.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);for(int i=0;i<task.conditions.Count;i++)task.conditions[i].value=task.Config.conditionParams[i].datas.Last();
                Click(entry.Button.gameObject);Check(f.Page.Page!=null&&f.Page.Provider.RefCount==1,"Native original main pointer publishes page and resource handle");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("task page native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    if(!f.Page.Trace.Contains("open"))return;
                    Check(f.Page.Page.OutletCount==18&&f.Page.Subviews.Daily.I.Rows.Count==7&&f.Page.Page.Transform.parent==f.Page.Layer,"Real two-frame load and OpenLater populate seven source daily rows on UITip");
                    Check(f.Page.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UITip","Actual TopInfo gold canvas follows task page layer");
                    Click(f.Page.Page.AchievementTab.gameObject);Phase(1);return;
                }
                if(phase==1)
                {
                    Check(ReferenceEquals(f.Page.Page.CurrentView,f.Page.Subviews.Achievement.I)&&f.Page.Subviews.Achievement.I.Rows.Count>0&&f.Page.Page.AchievementContent.activeInHierarchy&&!f.Page.Page.TaskContent.activeSelf,"Native Toggle pointer initializes and displays original achievement rows");
                    Click(f.Page.Subviews.Achievement.I.Rows[1].Claim.gameObject);Phase(11);return;
                }
                if(phase==11)
                {
                    if(f.Page.Data.Achievement.FindAchievement(1).state!=1)return;
                    Check(f.Global.GetItemCount(1001)==200&&f.Account.Local.GoldNum==200&&f.Page.Subviews.Achievement.I.Rows[1].Data.id==2,"Native achievement slide grants200 gold to both real account records and advances original row");
                    Click(f.Page.Page.TaskTab.gameObject);Phase(2);return;
                }
                if(phase==2)
                {
                    Check(ReferenceEquals(f.Page.Page.CurrentView,f.Page.Subviews.Daily.I)&&f.Page.Page.TaskContent.activeInHierarchy&&!f.Page.Page.AchievementContent.activeSelf&&f.Page.AudioCalls==2,"Native daily return retains original view and both toggle clicks play source audio");
                    var row=f.Page.Subviews.Daily.I.Rows[1];Click(row.Claim.gameObject);Check(!row.Claim.enabled&&f.Page.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==0,"Native task claim starts source slide before award");Phase(3);return;
                }
                if(phase==3)
                {
                    if(f.Page.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state!=1)return;
                    Check(f.Global.GetItemCount(1001)==250&&f.Account.Local.GoldNum==250&&f.Page.Data.Tasks.Child.ModuleData.ext.livenessValue==20&&!f.Page.Subviews.Daily.I.Rows[1].Lifetime.GameObject.activeSelf&&Mathf.Abs(f.Page.Page.Progress.value-.2f)<.001f,"Native daily slide adds50 gold to both real account records and updates page liveness/progress");
                    oldPage=f.Page.Page;oldDaily=f.Page.Subviews.Daily.I;Click(f.Page.Page.CloseButton.gameObject);Phase(4);return;
                }
                if(phase==4)
                {
                    if(!f.Page.Trace.Contains("close"))return;
                    Check(f.Page.Pages.Count==0&&oldPage.Lifetime.IsDisposed&&oldPage.GameObject==null&&f.Page.Provider.RefCount==0&&f.Page.Subviews.Daily.Instance==null&&f.Page.Subviews.Achievement.Instance==null,"Real asynchronous close removes registry, native object, resource handle and both singleton owners");
                    Check(f.Page.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UIPopup","Close restores actual top info original layer");
                    f.Page.Trace.Clear();Click(entry.Button.gameObject);Phase(5);return;
                }
                if(phase==5)
                {
                    if(!f.Page.Trace.Contains("open"))return;
                    Check(!ReferenceEquals(oldPage,f.Page.Page)&&!ReferenceEquals(oldDaily,f.Page.Subviews.Daily.I)&&f.Page.Subviews.Daily.I.Rows.Count==6&&!f.Page.Subviews.Daily.I.Rows.ContainsKey(1),"Reopen creates fresh page/subviews and excludes actual claimed task");
                    f.Page.Data.Tasks.Runtime.Statistics.Pool.SaveData();Click(f.Page.Page.Mask.gameObject);Phase(6);return;
                }
                if(phase==6)
                {
                    if(f.Page.Pages.Count!=0||f.Page.Provider.RefCount!=0)return;
                    Check(f.Page.Page.Lifetime.IsDisposed,"Native mask pointer completes page close");f.Dispose();Compose();Click(entry.Button.gameObject);Phase(7);return;
                }
                if(phase==7)
                {
                    if(!f.Page.Trace.Contains("open"))return;
                    report.restartedGlobalGold=f.Global.GetItemCount(1001);report.restartedLocalGold=f.Account.Local.GoldNum;
                    Check(f.Page.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==1&&!f.Page.Subviews.Daily.I.Rows.ContainsKey(1)&&f.Page.Data.Tasks.Child.ModuleData.ext.livenessValue==20&&f.Global.GetItemCount(1001)==250&&f.Account.Local.GoldNum==250&&f.Page.Data.Achievement.FindAchievement(1).state==1,"Independent account/activity/page restart restores250 gold in both records, daily/achievement claims and liveness");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/account-rewards-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
