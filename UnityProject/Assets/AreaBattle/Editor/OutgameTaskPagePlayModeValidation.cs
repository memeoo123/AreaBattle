using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTaskPageValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskPagePlayModeValidation
    {
        const string Pending="AreaBattle.TaskPageNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;public long restartedFixtureGold;
            public string scope="Original main button, entire task page resource/open/close, daily/achievement Toggle pointer routing, actual in-session rewards and persisted task records, shared subview replacement, TopInfo restoration and independent restart. Currency engine uses existing in-memory reward fixture and is not an account currency persistence test; explicit acquisition/audio/localization/effect/report endpoints; production Main/account/platform and original audiovisual/Player acceptance remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static OutgameTaskEntryBinding entry;static Report report;static OutgameTaskPage oldPage;static OutgameDailyTaskView oldDaily;static string path;static int phase,frame;static double began;static bool stopped;
        static OutgameTaskPagePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;}
        static void Click(GameObject target)=>ExecuteEvents.Execute(target,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Compose()
        {
            f=new Fixture(true,path);var canvas=f.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=f.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);f.Root.AddComponent<GraphicRaycaster>();
            entry=new OutgameTaskEntryBinding(f.Data.Main.transform,new OutgameTaskEntryServices{Control=()=>f.Data.Control,Pages=()=>f.OpenRegistry,FindPage=()=>f.Pages.TryGetValue(OutgameTaskPage.SourceName,out var p)?p:null,Messages=f.Ui.Messages,PlayVoice=(kind,id)=>{if(kind!=1||id!=2001)throw new Exception("voice arguments");}});entry.Initialize();
        }
        static void Start()
        {
            stopped=false;report=new Report();began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                new GameObject("EventSystem",typeof(EventSystem));new GameObject("Camera",typeof(Camera)).transform.position=new Vector3(0,0,-10);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskPageNative-"+Guid.NewGuid().ToString("N"));Compose();
                var task=f.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);for(int i=0;i<task.conditions.Count;i++)task.conditions[i].value=task.Config.conditionParams[i].datas.Last();
                Click(entry.Button.gameObject);Check(f.Page!=null&&f.Provider.RefCount==1,"Native original main pointer publishes page and resource handle");Phase(0);EditorApplication.update+=Poll;
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
                    if(!f.Trace.Contains("open"))return;
                    Check(f.Page.OutletCount==18&&f.Subviews.Daily.I.Rows.Count==7&&f.Page.Transform.parent==f.Layer,"Real two-frame load and OpenLater populate seven source daily rows on UITip");
                    Check(f.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UITip","Actual TopInfo gold canvas follows task page layer");
                    Click(f.Page.AchievementTab.gameObject);Phase(1);return;
                }
                if(phase==1)
                {
                    Check(ReferenceEquals(f.Page.CurrentView,f.Subviews.Achievement.I)&&f.Subviews.Achievement.I.Rows.Count>0&&f.Page.AchievementContent.activeInHierarchy&&!f.Page.TaskContent.activeSelf,"Native Toggle pointer initializes and displays original achievement rows");
                    Click(f.Page.TaskTab.gameObject);Phase(2);return;
                }
                if(phase==2)
                {
                    Check(ReferenceEquals(f.Page.CurrentView,f.Subviews.Daily.I)&&f.Page.TaskContent.activeInHierarchy&&!f.Page.AchievementContent.activeSelf&&f.AudioCalls==2,"Native daily return retains original view and both toggle clicks play source audio");
                    var row=f.Subviews.Daily.I.Rows[1];Click(row.Claim.gameObject);Check(!row.Claim.enabled&&f.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==0,"Native task claim starts source slide before award");Phase(3);return;
                }
                if(phase==3)
                {
                    if(f.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state!=1)return;
                    Check(f.Data.Tasks.Items.Global.GetItemCount(1001)==50&&f.Data.Tasks.Child.ModuleData.ext.livenessValue==20&&!f.Subviews.Daily.I.Rows[1].Lifetime.GameObject.activeSelf&&Mathf.Abs(f.Page.Progress.value-.2f)<.001f,"Actual slide claim awards50 gold and updates page liveness/progress");
                    oldPage=f.Page;oldDaily=f.Subviews.Daily.I;Click(f.Page.CloseButton.gameObject);Phase(4);return;
                }
                if(phase==4)
                {
                    if(!f.Trace.Contains("close"))return;
                    Check(f.Pages.Count==0&&oldPage.Lifetime.IsDisposed&&oldPage.GameObject==null&&f.Provider.RefCount==0&&f.Subviews.Daily.Instance==null&&f.Subviews.Achievement.Instance==null,"Real asynchronous close removes registry, native object, resource handle and both singleton owners");
                    Check(f.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UIPopup","Close restores actual top info original layer");
                    f.Trace.Clear();Click(entry.Button.gameObject);Phase(5);return;
                }
                if(phase==5)
                {
                    if(!f.Trace.Contains("open"))return;
                    Check(!ReferenceEquals(oldPage,f.Page)&&!ReferenceEquals(oldDaily,f.Subviews.Daily.I)&&f.Subviews.Daily.I.Rows.Count==6&&!f.Subviews.Daily.I.Rows.ContainsKey(1),"Reopen creates fresh page/subviews and excludes actual claimed task");
                    f.Data.Tasks.Runtime.Statistics.Pool.SaveData();Click(f.Page.Mask.gameObject);Phase(6);return;
                }
                if(phase==6)
                {
                    if(f.Pages.Count!=0||f.Provider.RefCount!=0)return;
                    Check(f.Page.Lifetime.IsDisposed,"Native mask pointer completes page close");f.Dispose();Compose();Click(entry.Button.gameObject);Phase(7);return;
                }
                if(phase==7)
                {
                    if(!f.Trace.Contains("open"))return;
                    report.restartedFixtureGold=f.Data.Tasks.Items.Global.GetItemCount(1001);
                    Check(f.Data.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==1&&!f.Subviews.Daily.I.Rows.ContainsKey(1)&&f.Data.Tasks.Child.ModuleData.ext.livenessValue==20,"Independent task storage/activity/page restart restores actual claim and liveness");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-page-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
