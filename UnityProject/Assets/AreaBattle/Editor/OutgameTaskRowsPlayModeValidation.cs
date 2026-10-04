using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTaskRowsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskRowsPlayModeValidation
    {
        const string Pending="AreaBattle.TaskRowsNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original task panel hierarchy/row, actual Button pointer dispatch, scaled OutSine slide, concrete daily task/economy/liveness, native automatic save and independent restart. Effect/sprite/report endpoints are observed; whole-page tabs/lifecycle/preview/Main and original audiovisual/Player acceptance pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static string path;static OutgameTaskRow row;static GameObject doomed;static long uid;static int phase,frame;static double began;static bool stopped;
        static OutgameTaskRowsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;}
        static void Canvas()
        {var canvas=f.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;f.Root.AddComponent<GraphicRaycaster>();}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                new GameObject("EventSystem",typeof(EventSystem));new GameObject("Camera",typeof(Camera)).transform.position=new Vector3(0,0,-10);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskRowsNative-"+Guid.NewGuid().ToString("N"));f=new Fixture(true,path);Canvas();
                var task=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);uid=task.uid;f.Tasks.Event(task.Config.conditionParams[0].datas[0],1);f.Daily.RefreshRows();row=f.Daily.Rows[1];
                Check(row.Claim.gameObject.activeSelf&&row.Target.text=="1/1"&&task.CanComplete(),"Actual statistics event projects readiness into original daily task Button and UGUI text");
                Check(f.Daily.Rows.Count==8&&f.Page.GetComponentsInChildren<ScrollRect>(true).Length==2,"Original daily/achievement scroll geometry remains present alongside eight daily row clones");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Click()=>ExecuteEvents.Execute(row.Claim.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("Task rows native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    Time.timeScale=0;Click();Check(!row.Claim.enabled&&f.Motion.Count==1&&f.Tasks.Child.TasksByUid[uid].state==0,"Native pointer disables Button and schedules slide before economy claim");
                    Click();Check(f.Motion.Count==1,"Disabled native Button suppresses second pointer callback");Phase(1);return;
                }
                if(phase==1)
                {
                    Check(f.Tasks.Child.TasksByUid[uid].state==0&&f.Tasks.Items.Global.GetItemCount(1001)==0&&f.Motion.Count==1,"Scaled slide and claim remain pending while native timeScale is zero");
                    Time.timeScale=1;Phase(2);return;
                }
                if(phase==2)
                {
                    if(f.Tasks.Child.TasksByUid[uid].state==0)return;
                    Check(f.Tasks.Items.Global.GetItemCount(1001)==50&&f.Daily.Liveness==20,"Native slide completion applies actual task reward and liveness once");
                    Check(!row.Lifetime.GameObject.activeSelf&&row.Claim.enabled&&row.Lifetime.Transform.localPosition.x==1600&&f.Reported.Count==1,"Completion reaches original slide endpoint, re-enables/hides row and reports once");
                    Check(f.Fx.Kind==1&&f.Fx.Amount==50&&!f.Fx.Apply&&f.RedRefresh==1,"Currency host observes non-economic fly request and daily page refresh");f.Fx.Completion();Check(f.TopRefresh==1,"Observed currency completion refreshes top display after actual task callback");Phase(3);return;
                }
                if(phase==3)
                {
                    if(f.Tasks.Parent.Dirty)return;Check(f.Tasks.Stored.Length>0,"Native activity updates save the row-triggered task claim automatically");
                    f.Dispose();f=new Fixture(true,path);Canvas();Check(f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).uid==uid&&f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==1&&f.Daily.Liveness==20&&!f.Daily.Rows.ContainsKey(1),"Independent file owner recreates daily view without previously claimed row");
                    f.Daily.Claim(1,uid);Check(f.Tasks.Items.Host.Adds==0,"Restarted daily claim cannot duplicate inventory reward");
                    row=f.Daily.Rows[2];doomed=row.Lifetime.GameObject;var data=row.Data;var callback=row.ClaimedCallback;row.Dispose();
                    Check(doomed&&row.Lifetime.GameObject==null&&row.Lifetime.IsDisposed&&row.Data==data&&row.ClaimedCallback==callback,"Source disposal clears ownership immediately and retains task projection/callback until native destruction");Phase(4);return;
                }
                Check(!doomed,"Owned original row is destroyed on subsequent native frames");Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-rows-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
