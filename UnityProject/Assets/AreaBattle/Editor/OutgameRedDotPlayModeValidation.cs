using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public sealed class OutgameRedDotNativeDriver:MonoBehaviour
    {
        public OutgameLogicModule Logic;
        public bool Running;
        public float UnscaledElapsed;
        void Update(){if(Running){UnscaledElapsed+=Time.unscaledDeltaTime;Logic.Update(Time.deltaTime,Time.unscaledDeltaTime);}}
    }
    [InitializeOnLoad] public static class OutgameRedDotPlayModeValidation
    {
        const string Pending="AreaBattle.RedDotNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual Unity Start/Update/OnDestroy, shared registry/LogicModule and original MenuTabUI component subtree; paused scaled time with unscaled refresh, inactive/disabled checks, real destruction and reopen. Test listener observes event order; original Tower_CheckReddot target is absent. Full Main/activity predicates/Player/visual acceptance remain pending.";
            public List<string> checks=new List<string>();public int startFrame,destroyFrame,reopenFrame;public float unscaledAtFirstRefresh;
        }
        static Report report;static OutgameRedDotValidation.Fixture f;static OutgameRedDotControl control;
        static OutgameRedDotNativeDriver driver;static OutgameRedDotItem item;static GameObject menu;
        static int phase,frame,calls,beforeHide;static double started,phaseAt;static float scaledAtStart;static bool stopped;
        static List<string> lifetime;
        static OutgameRedDotPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);report.checks.Add(why);}
        static void SetPhase(int next){phase=next;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static OutgameLogicModule Module(OutgameRedDotControl target)
        {
            var logic=new OutgameLogicModule(b=>lifetime.Add("auto:"+b),()=>lifetime.Add("data-init"),()=>lifetime.Add("data-release"),s=>{},s=>{});
            logic.Initialize();logic.RegisterLogicCtr(target,false);logic.InitCtrl(true);target.InitRedDot(OutgameRedDotDetectType.PerSecond);return logic;
        }
        static void Open()
        {
            menu=new GameObject("recovered-menu-owner",typeof(RectTransform));menu.SetActive(false);var rect=(RectTransform)menu.transform;rect.sizeDelta=new Vector2(1080,1920);
            var view=menu.AddComponent<OutgameMenuView>();var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
            view.Initialize(rect,5,rules,redDots:()=> (OutgameRedDotControl)f.Registry.Resolve(4453));item=view.Menu.GetComponentInChildren<OutgameRedDotItem>(true);
            item.gameObject.SetActive(true);item.DotPrefab.SetActive(true);menu.SetActive(true);
        }
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;calls=0;lifetime=new List<string>();
            try{
                f=new OutgameRedDotValidation.Fixture(false);control=f.Control;
                driver=new GameObject("native-red-dot-logic-driver").AddComponent<OutgameRedDotNativeDriver>();driver.Logic=Module(control);
                Time.timeScale=0;scaledAtStart=Time.time;Open();Check(control.Items.Count==0,"Imported component binding leaves registration to actual Start");SetPhase(0);EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-started>40)throw new TimeoutException("Red dot phase "+phase);
                if(Time.frameCount<=frame+2||EditorApplication.timeSinceStartup-phaseAt<.1)return;
                if(phase==0){
                    Check(control.Items.Count==1&&ReferenceEquals(control.Items[0],item),"Actual Start registers the original menu component once");report.startFrame=Time.frameCount;
                    Check(item.DotPrefab.name=="imgCommanderDot3"&&item.DotPrefab.activeSelf,"Original target exists before first timed refresh");
                    driver.Running=true;SetPhase(1);return;
                }
                if(phase==1){
                    if(driver.UnscaledElapsed<1.15f)return;
                    Check(!item.DotPrefab.activeSelf&&!item.IsShowRedDot,"Original null persistent target leaves timed red dot hidden");
                    Check(Time.timeScale==0&&Time.time==scaledAtStart&&driver.UnscaledElapsed>=1,"Native LogicModule refreshes through unscaled time while gameplay is paused");
                    item.CheckActionBool.AddListener(x=>{if(calls==0){Check(!x.IsShowRedDot&&!x.DotPrefab.activeSelf,"Check resets flag and invokes listener before Show");report.unscaledAtFirstRefresh=driver.UnscaledElapsed;}calls++;x.IsShowRedDot=true;});
                    SetPhase(2);return;
                }
                if(phase==2){
                    if(calls==0)return;Check(item.DotPrefab.activeSelf&&item.IsShowRedDot,"Actual frame update displays completed event result");
                    Check(report.unscaledAtFirstRefresh>=2,"Per-second timing retains first missing-target event cycle");
                    control.InitRedDot(OutgameRedDotDetectType.Update);beforeHide=calls;item.enabled=false;menu.SetActive(false);SetPhase(3);return;
                }
                if(phase==3){
                    Check(calls>beforeHide&&control.Items.Count==1,"Inactive hierarchy and disabled item remain registered and checked");
                    Check(item.DotPrefab.activeSelf&&!item.DotPrefab.activeInHierarchy,"Show changes target activeSelf without activating its hidden ancestors");
                    driver.Running=false;UnityEngine.Object.Destroy(menu);Check(control.Items.Count==1&&item!=null,"Destroy request precedes native OnDestroy registration removal");SetPhase(4);return;
                }
                if(phase==4){
                    if(menu!=null)return;report.destroyFrame=Time.frameCount;
                    Check(item==null&&control.Items.Count==0,"Native destruction removes the actual component from the controller");
                    Check(f.Logs.Count==2&&f.Logs[0].StartsWith("有红点注册: ")&&f.Logs[1].StartsWith("有红点退出: "),"Registration and destruction log exactly once");
                    driver.Logic.Shutdown();Check(!f.Registry.HasInstance(4453)&&lifetime[lifetime.Count-1]=="data-release","Real module shutdown releases data and clears registry slot");
                    var old=control;control=(OutgameRedDotControl)f.Registry.Resolve(4453);driver.Logic=Module(control);Check(!ReferenceEquals(old,control),"Reopen obtains a fresh source singleton after shutdown");
                    Open();SetPhase(5);return;
                }
                if(phase==5){
                    report.reopenFrame=Time.frameCount;Check(control.Items.Count==1&&ReferenceEquals(control.Items[0],item)&&item.CheckActionBool.GetPersistentEventCount()==1&&item.CheckActionBool.GetPersistentTarget(0)==null&&item.CheckActionBool.GetPersistentMethodName(0)=="Tower_CheckReddot","Reopened component registers once without inventing missing original callback");
                    control.InitRedDot(OutgameRedDotDetectType.Update);driver.Running=true;SetPhase(6);return;
                }
                if(phase==6){Check(!item.DotPrefab.activeSelf&&control.Items.Count==1,"Reopened native page is independently refreshed");Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);if(driver)driver.Running=false;Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/red-dot-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
