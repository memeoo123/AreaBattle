using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fixture=AreaBattle.EditorTools.OutgamePlayerControlValidation.Fixture;
using Source=AreaBattle.EditorTools.OutgameInputManagerValidation.Source;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameInputManagerPlayModeValidation
    {
        const string Pending="AreaBattle.InputManagerNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Native MonoBehaviour Update, singleton/GameObject/scene persistence and default PlayerInput subscriptions. Device reads use an explicit scripted provider; no claim of OS/device touch injection, SDK, full Main entry or original matched replay.";public List<string> checks=new List<string>();}
        static Report report;static bool stopped;static double began;static int phase,beginCount,endCount,duplicateCount,baseline;static Fixture fixture;static OutgameInputManager manager;static Source source;static List<string> events;static Queue<OutgameTouch[]> frames;
        static OutgameInputManagerPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string text){if(!yes)throw new Exception(text);report.checks.Add(text);}
        static void Enqueue(TouchPhase phase,float x,float y)=>frames.Enqueue(new[]{new OutgameTouch(new Vector2(x,y),phase)});
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;phase=beginCount=endCount=duplicateCount=0;events=new List<string>();frames=new Queue<OutgameTouch[]>();
            try{
                var go=new GameObject("InputManager");var existing=go.AddComponent<OutgameInputManager>();manager=OutgameInputManager.Instance;
                Check(ReferenceEquals(existing,manager)&&ReferenceEquals(manager,OutgameInputManager.Instance)&&go.GetComponents<OutgameInputManager>().Length==1,"Source singleton finds existing named native component and reuses one owner");
                Check(manager.Source is OutgameUnityInputSource,"Production singleton defaults to actual Unity input provider");
                source=new Source{platform=RuntimePlatform.WebGLPlayer};source.ReadTouches=()=>source.touches=frames.Count>0?frames.Dequeue():Array.Empty<OutgameTouch>();manager.Source=source;
                manager.TouchBegin+=(x,y)=>{beginCount++;events.Add("begin");};manager.TouchHold+=(x,y)=>events.Add("hold");manager.TouchEnd+=(x,y)=>{endCount++;events.Add("end");};manager.Click+=(x,y)=>events.Add("click");manager.MulTouchEnd+=()=>events.Add("multi-end");
                Action<float,float> duplicate=(x,y)=>duplicateCount++;OutgameInputManager.AddTouchBeginListener(duplicate);OutgameInputManager.AddTouchBeginListener(duplicate);OutgameInputManager.RemoveTouchBeginListener(duplicate);
                fixture=new Fixture(true,new OutgamePlayerInput());fixture.Core.State.PlayState=2;fixture.Ui.CurrentPage=3;fixture.Player.OnInit();fixture.Player.ScreenPoint=new Vector3(0,0,3);
                Enqueue(TouchPhase.Began,12,34);Enqueue(TouchPhase.Stationary,12,34);Enqueue(TouchPhase.Ended,12,34);EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-began>25)throw new TimeoutException("Native input update timed out");
                if(phase==0){if(!events.Contains("multi-end"))return;
                    Check(string.Join(",",events)=="begin,hold,end,click,multi-end"&&source.mouseReads==0,"Native Unity Update dispatches scripted WebGL touch frames with exact source order");
                    Check(duplicateCount==1,"Static listener wrappers preserve duplicates and remove one subscription");
                    Check(fixture.Player.ScreenPoint==new Vector3(12,34,3),"Actual Player OnInit/default PlayerInput receives shared manager touch and runs native home-camera raycast");
                    fixture.Player.OnDispose();fixture.Player.ScreenPoint=new Vector3(90,91,3);baseline=source.touchReads;Enqueue(TouchPhase.Began,7,8);phase=1;return;
                }
                if(phase==1){if(source.touchReads<=baseline||frames.Count!=0)return;
                    Check(beginCount==2&&fixture.Player.ScreenPoint==new Vector3(90,91,3),"Player disposal removes production touch callback while independent manager listeners keep receiving frames");
                    manager.HeldKeys=new List<KeyCode>{KeyCode.A};manager.LastScroll=9;manager.Shutdown();Check(manager.MulTouchLen==1&&manager.PressPoint==new Vector3(7,8,0)&&manager.HeldKeys.Count==1&&manager.LastScroll==9,"Shutdown retains touch/key/scroll state and native singleton owner");
                    baseline=source.touchReads;Enqueue(TouchPhase.Began,20,30);phase=2;return;
                }
                if(phase==2){if(source.touchReads<=baseline||frames.Count!=0)return;
                    Check(beginCount==2&&manager.MouseX==20&&ReferenceEquals(manager,OutgameInputManager.Instance),"Native Update continues after Shutdown with all prior subscriptions cleared");
                    var scene=SceneManager.CreateScene("input-persistence-probe");SceneManager.SetActiveScene(scene);Check(manager&&manager.gameObject.scene.name=="DontDestroyOnLoad"&&ReferenceEquals(manager,OutgameInputManager.Instance),"Singleton resides in Unity persistent scene across active-scene switch");
                    UnityEngine.Object.DestroyImmediate(manager.gameObject);manager=OutgameInputManager.Instance;Check(manager&&manager.gameObject.name=="InputManager"&&manager.Source is OutgameUnityInputSource&&manager.HeldKeys==null,"Destroyed native singleton recreates named owner with fresh source defaults");
                    Finish(null);
                }
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/input-manager-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
