using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgamePlayerControlValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgamePlayerControlPlayModeValidation
    {
        const string Pending="AreaBattle.PlayerControlNative",ControllerPath="Assets/AreaBattle/Resources/Validation/PlayerTouchProbe.controller";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Actual restored Player controller registered in LogicModule, original model roots/meshes/baked animations, native physics and Animator trigger. Mouse/touch input is an explicit fixture; touch collider/AnimatorController are synthetic probes. Full Main/platform/account and matched original UI replay remain open.";public List<string> checks=new List<string>();}
        static Report report;static bool stopped;static double began;static int phase,chosen;static Fixture fixture;static OutgameLogicModule module;static GameObject probe;static Animator animator;static OutgameBakedAnimator[] baked;
        static OutgamePlayerControlPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");
            Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));AssetDatabase.Refresh();var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if(!controller){controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);controller.AddParameter("Trigger",AnimatorControllerParameterType.Trigger);var machine=controller.layers[0].stateMachine;var idle=machine.AddState("Idle");var triggered=machine.AddState("Triggered");machine.defaultState=idle;var transition=idle.AddTransition(triggered);transition.hasExitTime=false;transition.duration=0;transition.AddCondition(AnimatorConditionMode.If,0,"Trigger");EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();}
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string text){if(!yes)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;chosen=0;began=EditorApplication.timeSinceStartup;
            try{
                Time.timeScale=1;fixture=new Fixture(false);module=new OutgameLogicModule(b=>{},()=>{},()=>{},Debug.LogWarning,Debug.Log);module.Initialize();module.RegisterLogicCtr(fixture.Player,true);fixture.Core.Messages.SendMessage("LoadGameScreen");
                Check(module.ControllerCount==1&&fixture.Input.Adds==1&&fixture.Player.Roots.Count==3&&fixture.Game.SceneRoot.Scene_game.gameObject.activeSelf,"LogicModule registration and LoadGameScreen initialize actual source scene and player roots");
                baked=new OutgameBakedAnimator[3];for(int i=0;i<3;i++){var model=fixture.Player.Models.Cached(i+1,fixture.Skins.UsedSkin(i+1));baked[i]=model.GetComponent<OutgameBakedAnimator>();if(!model.GetComponent<MeshFilter>().sharedMesh||baked[i].Current.name!="relax")throw new Exception("Native original model or relax missing");}
                Check(fixture.Player.Animation.CachedCount==3,"Three original baked model animations initialized through actual pooled loader and source Player callbacks");
                fixture.Ui.CurrentPage=2;fixture.Core.State.PlayState=2;fixture.Core.Messages.AddListener("ChooseSoldier",a=>chosen=(int)a[0]);fixture.Input.Position=new Vector3(20,800,0);fixture.Input.Down=fixture.Input.Held=true;module.Update(0,0);fixture.Input.Down=false;fixture.Input.Position.x=140;module.Update(0,0);
                var rotation=fixture.Player.Rotation;Check(rotation.Amount==.6f&&fixture.Player.Roots[3].localPosition==Vector3.Lerp(rotation.Middle,rotation.Right,.6f),"LogicModule mouse drag interpolates the original native category transforms");fixture.Input.Held=false;fixture.Input.Up=true;module.Update(0,0);fixture.Input.Up=false;
                Check(chosen==1&&rotation.SelectedType==1&&!rotation.IsFingerMove&&fixture.Player.Roots[1].localPosition==rotation.Middle,"Drag release snaps native roots and dispatches ChooseSoldier with source wrapped category");
                probe=new GameObject("explicit-physics-animator-probe",typeof(BoxCollider),typeof(Animator));probe.layer=LayerMask.NameToLayer("Scenes11");probe.transform.position=fixture.Game.HomeCamera.transform.position+fixture.Game.HomeCamera.transform.forward*2;probe.transform.localScale=Vector3.one*.1f;animator=probe.GetComponent<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.runtimeAnimatorController=Resources.Load<RuntimeAnimatorController>("Validation/PlayerTouchProbe");animator.Rebind();animator.Update(0);Physics.SyncTransforms();
                fixture.Ui.CurrentPage=3;Vector3 point=fixture.Game.HomeCamera.WorldToScreenPoint(probe.transform.position);fixture.Input.Touch(point.x,point.y);
                Check(fixture.Player.LastHit.collider==probe.GetComponent<BoxCollider>()&&fixture.Player.ScreenPoint.x==point.x&&fixture.Player.ScreenPoint.y==point.y,"Registered touch callback raycasts through actual home camera using original Scenes11 layer31");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>25)throw new TimeoutException("Native player animation/trigger timed out");
                if(phase==0)
                {
                    if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Triggered"))return;
                    Check(true,"Source OnTouchBegin activates native Animator Trigger after physics hit");phase=1;
                }
                if(phase==1)
                {
                    foreach(var value in baked)if(value.Current==null||value.Current.name!="idle"||!value.Looping)return;
                    Check(true,"Original baked relax animations complete on native frames and source callbacks select looping idle");
                    fixture.Player.Timer1=11.7f;fixture.Player.Timer2=0;fixture.Player.Timer3=0;module.Update(.01f,.5f);
                    Check(baked[0].Current.name=="relax"&&!baked[0].Looping&&fixture.Player.Timer1==0&&baked[1].Current.name=="idle","Home timer refresh runs through LogicModule and restarts only the matching equipped native animation");
                    module.Shutdown();Check(module.ControllerCount==0&&!fixture.Core.Registry.HasInstance(4462)&&fixture.Input.Touch==null&&fixture.Player.Animation.CachedCount==0&&fixture.Player.Models.Cached(1,100),"LogicModule shutdown unregisters source listener/cache while retaining native model objects");phase=2;return;
                }
                if(phase==2)
                {
                    if(baked[0].Current==null||baked[0].Current.name!="idle")return;
                    Check(baked[0].Looping&&fixture.Player.Animation.CachedCount==0,"Retained baked completion after controller disposal uses source captured animator fallback without repopulating cache");Finish(null);
                }
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/player-control-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
