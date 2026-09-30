using System;
using System.Collections.Generic;
using UnityEngine;
using LevelFixture=AreaBattle.EditorTools.OutgameLevelControlValidation.Fixture;
namespace AreaBattle.EditorTools
{
    public static class OutgamePlayerControlValidation
    {
        internal sealed class InputFixture:IOutgamePlayerInput
        {
            public bool Down,Held,Up;public Vector3 Position;public int Height=1000;public Action<float,float> Touch;public int Adds,Removes;public Action Adding,Removing;
            public bool MouseDown=>Down;public bool MouseHeld=>Held;public bool MouseUp=>Up;public Vector3 MousePosition=>Position;public int ScreenHeight=>Height;
            public void AddTouchBegin(Action<float,float> callback){Adding?.Invoke();Adds++;Touch+=callback;}public void RemoveTouchBegin(Action<float,float> callback){Removing?.Invoke();Removes++;Touch-=callback;}
        }
        sealed class Loads:IOutgameGameSceneResources
        {public void LoadAsset(int id,Action<GameObject> cb,object[] args=null){}public void LoadTexture(string n,Action<Texture2D> cb,object[] args=null){}public void LoadPrefab(string n,Action<GameObject> cb,object[] args=null){}}
        internal sealed class Fixture:IDisposable
        {
            public readonly LevelFixture Core=new LevelFixture();public readonly InputFixture Input=new InputFixture();
            public readonly GameObject Root;public readonly OutgameGameControl Game;public readonly OutgameUiControl Ui;public readonly OutgameSkinCatalog Skins;public readonly OutgamePlayerControl Player;
            public readonly List<object[]> Warnings=new List<object[]>();public int GameReads;public Action<object[]> Warn;
            public Fixture(bool initialize=true,IOutgamePlayerInput sharedInput=null)
            {
                Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));var scene=Root.GetComponentInChildren<OutgameGameSceneMono>(true);
                Core.Config.dicSceneSkin.Add(1,new OriginalConfig.SceneSkinConfig{id=1,idleIconName="native-home"});Game=new OutgameGameControl(()=>Core.Config,Core.Registry,new Loads(),()=>1,()=>0,id=>{},tag=>new[]{scene.gameObject},()=>1080,()=>1920);Game.OnInit();Game.SceneRoot=scene;Game.HomeCamera=scene.HomeCamera;
                Ui=new OutgameUiControl(null,Core.Registry,()=>Core.Messages,null,null,null,null,null,null);
                Skins=OutgameSkinCatalog.FromOriginal(null,BattleView.ReadText("Data/Outgame/SkinConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"));
                var paths=new Dictionary<int,string>{{1000,"Recovered/Outgame/Models/soldier_100"},{1002,"Recovered/Outgame/Models/soldier_102"},{2000,"Recovered/Outgame/Models/soldier_200"},{4000,"Recovered/Outgame/Models/soldier_400"},{9033,"Recovered/BossEmbedded/BBDyShadow"},{9034,"Recovered/BossEmbedded/QBDyShadow"}};
                foreach(var entry in paths)Core.Loader.Cache.Prefabs[entry.Key]=Resources.Load<GameObject>(entry.Value);
                var color=OutgameSoldierColor.FromRecovered();
                OutgameCoreControllerBindings.BindPlayer(Core.Registry,()=>{GameReads++;return Game;},()=>Core.Level,()=>Ui,()=>Skins,()=>Core.Messages,sharedInput??Input,()=>Core.Loader,()=>Core.Config,BattleView.ReadText("Data/Outgame/SkinConfig"),go=>color.Apply(go,Core.Level.PlayerCampId),Core.Calls.Add,a=>{Warnings.Add(a);Warn?.Invoke(a);});
                Player=(OutgamePlayerControl)Core.Registry.Resolve(4462);if(initialize)Player.InitLoadModel();
            }
            public void Dispose(){if(Root)UnityEngine.Object.DestroyImmediate(Root);Core.Dispose();}
        }
        static void Require(bool ok,string detail){if(!ok)throw new Exception(detail);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-player-native-models-and-shared-registry-owner",()=>{using(var f=new Fixture()){
                Require(f.Player.Rotation.SelectedType==3&&f.Player.Rotation.DragLimit==200&&f.Player.ModelScale==1&&f.Player.Roots.Count==3&&f.GameReads==3,"source defaults and three independent scene getters");for(int type=1;type<=3;type++){var go=f.Player.Models.Cached(type,f.Skins.UsedSkin(type));Require(go&&go.transform.parent==f.Player.Roots[type]&&go.GetComponent<OutgameBakedAnimator>().Current.name=="relax","source native original model/animation category"+type);}Require(f.Player.Animation.CachedCount==3&&ReferenceEquals(f.Core.Registry.Resolve(4462),f.Player),"same production binding owner and native animation cache");
            }});
            check("source-player-init-load-reentry-resets-model-map-before-duplicate-root-failure",()=>{using(var f=new Fixture()){
                var old=f.Player.Models.Cached(1,100);f.Player.Rotation.SelectedType=2;Throws<ArgumentException>(()=>f.Player.InitLoadModel());Require(old&&f.Player.Models.Cached(1,100)==null&&f.Player.Roots.Count==3&&f.Player.Rotation.SelectedType==2&&f.Player.Animation.CachedCount==3,"new model dictionary assigned before Roots.Add throws; no invented root clear/selection reset/native destroy");
            }});
            check("source-player-lifecycle-duplicate-listeners-and-one-at-a-time-removal",()=>{using(var f=new Fixture()){
                f.Player.OnInit();f.Player.OnInit();Require(f.Input.Adds==2,"OnInit has no guard");f.Core.Messages.SendMessage("RotateSoldier",new object[]{1});Require(f.Player.Rotation.SelectedType==1,"rotation event native roots");f.Player.OnDispose();Require(!f.Core.Registry.HasInstance(4462)&&f.Player.Animation.CachedCount==0&&f.Input.Removes==1&&f.Input.Touch!=null&&f.Player.Roots.Count==3&&f.Player.Models.Cached(1,100),"dispose clears registry/animation only, removes one subscription, retains model/root");f.Core.Messages.SendMessage("RotateSoldier",new object[]{2});Require(f.Player.Rotation.SelectedType==2,"one duplicate listener retained");f.Player.OnDispose();f.Core.Messages.SendMessage("RotateSoldier",new object[]{3});Require(f.Player.Rotation.SelectedType==2&&f.Input.Touch==null,"second removal clears remaining subscriptions");
            }});
            check("source-player-init-registration-failure-retains-prior-event-writes",()=>{using(var f=new Fixture()){
                f.Input.Adding=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Player.OnInit);f.Core.Messages.SendMessage("RotateSoldier",new object[]{1});Require(f.Player.Rotation.SelectedType==1,"message subscriptions survive failing touch registration");f.Input.Adding=null;f.Player.OnDispose();
            }});
            check("source-player-load-screen-initializes-scene-before-model-root-add-failure",()=>{using(var f=new Fixture()){
                f.Player.OnInit();f.Game.SceneRoot.Scene_game.gameObject.SetActive(false);Throws<ArgumentException>(()=>f.Core.Messages.SendMessage("LoadGameScreen"));Require(f.Game.SceneRoot.Scene_game.gameObject.activeSelf&&f.Player.Models.Cached(1,100)==null,"actual Game.InitScene effects precede duplicate-model initialization failure");f.Player.OnDispose();
            }});
            check("source-player-home-timers-strict-threshold-once-and-unscaled-ignored",()=>{using(var f=new Fixture(false)){
                f.Core.State.PlayState=6;f.Player.Updata(20,300);Require(f.Player.Timer1==0&&f.Warnings.Count==0,"not home no animation timers");f.Core.State.PlayState=2;f.Player.Timer1=11.7f;f.Player.Timer2=13.7f;f.Player.Timer3=14.7f;f.Player.Updata(0,999);Require(f.Warnings.Count==0,"strict greater than thresholds");f.Player.Updata(100,0);Require(f.Player.Timer1==0&&f.Player.Timer2==0&&f.Player.Timer3==0&&f.Warnings.Count==3,"once per type, reset to zero no remainder/catchup; missing model map caught");Require((int)f.Warnings[0][1]==1&&(int)f.Warnings[2][1]==3&&f.Warnings[0][2].ToString().Contains("NullReferenceException"),"source warning carries type and exception text");
            }});
            check("source-player-animation-warning-failure-preserves-all-timer-additions",()=>{using(var f=new Fixture(false)){
                f.Core.State.PlayState=2;f.Warn=a=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Player.Updata(20,1));Require(f.Player.Timer1==0&&f.Player.Timer2==20&&f.Player.Timer3==20,"all timers add first; first reset before refresh/warning failure prevents later reset");
            }});
            check("source-player-drag-top-half-gate-and-snap-back-at-threshold",()=>{using(var f=new Fixture()){
                f.Ui.CurrentPage=2;var r=f.Player.Rotation;f.Input.Down=f.Input.Held=true;f.Input.Position=new Vector3(0,500,0);f.Player.Updata(0,0);Require(!r.IsFingerMove,"screen integer half equal does not begin");f.Input.Position.y=501;f.Player.Updata(0,0);f.Input.Down=false;f.Input.Position.x=60;f.Player.Updata(0,0);Require(r.IsFingerMove&&Mathf.Abs(r.Amount-.3f)<.00001f,"drag uses fixed200 pixels");f.Input.Held=false;f.Input.Up=true;int chosen=0;f.Core.Messages.AddListener("ChooseSoldier",a=>chosen++);f.Player.Updata(0,0);Require(r.SelectedType==3&&!r.IsFingerMove&&chosen==0&&f.Player.Roots[3].localPosition==r.Middle&&f.Player.Roots[1].localPosition==r.Left,"equal .3 snaps back without ChooseSoldier");
            }});
            check("source-player-drag-positive-cap-and-chosen-event-before-finger-clear",()=>{using(var f=new Fixture()){
                f.Ui.CurrentPage=2;var r=f.Player.Rotation;f.Input.Down=f.Input.Held=true;f.Input.Position=new Vector3(10,600,0);f.Player.Updata(0,0);f.Input.Down=false;f.Input.Position.x=310;f.Player.Updata(0,0);Require(r.DragDistance==200&&r.Amount==1&&r.PositiveDirection,"positive clamp to200");f.Input.Held=false;f.Input.Up=true;f.Core.Messages.AddListener("ChooseSoldier",a=>{Require((int)a[0]==1&&r.SelectedType==1&&r.IsFingerMove,"selection and native placement precede event; finger clear follows");throw new InvalidOperationException();});Throws<InvalidOperationException>(()=>f.Player.Updata(0,0));Require(r.IsFingerMove&&f.Player.Roots[1].localPosition==r.Middle&&f.Player.Roots[3].localScale==r.RightScale,"event failure retains completed selection/placement and finger flag");
            }});
            check("source-player-drag-negative-interpolation-and-hidden-page-retention",()=>{using(var f=new Fixture()){
                f.Ui.CurrentPage=2;var r=f.Player.Rotation;f.Input.Down=f.Input.Held=true;f.Input.Position=new Vector3(0,700,0);f.Player.Updata(0,0);f.Input.Down=false;f.Input.Position.x=-100;f.Player.Updata(0,0);Require(!r.PositiveDirection&&r.Amount==.5f&&f.Player.Roots[3].localPosition==Vector3.Lerp(r.Middle,r.Left,.5f),"negative source interpolation");f.Ui.CurrentPage=1;f.Input.Held=false;f.Input.Up=true;f.Player.Updata(0,0);Require(r.IsFingerMove,"leaving page does not reset held drag");f.Ui.CurrentPage=2;f.Player.Updata(0,0);Require(r.SelectedType==2&&!r.IsFingerMove&&f.Player.Roots[2].localPosition==r.Middle,"return processes release selecting wrapped previous");
            }});
            check("source-player-rotation-null-root-failure-writes-positions-before-scales",()=>{using(var f=new Fixture()){
                var r=f.Player.Rotation;var oldScale=f.Player.Roots[3].localScale;f.Player.Roots.Remove(1);Throws<KeyNotFoundException>(()=>r.Select(1));Require(f.Player.Roots[3].localPosition==r.Right&&f.Player.Roots[3].localScale==oldScale&&r.SelectedType==3,"source first position written before next root lookup failure, no interleaved scale write");
            }});
            check("source-player-first-ads-reset-and-root-active-gates",()=>{using(var f=new Fixture()){
                f.Player.FirstAds1=1;f.Player.FirstAds2=2;f.Player.FirstAds3=3;f.Player.InitFirstAdsItem(2);Require(f.Player.FirstAds1==1&&f.Player.FirstAds2==0&&f.Player.FirstAds3==3,"one source slot");f.Player.InitFirstAdsItem(99);Require(f.Player.FirstAds1==1,"invalid selector ignored");f.Player.InitFirstAdsItem(0);Require(f.Player.FirstAds1==0&&f.Player.FirstAds3==0,"zero resets all");f.Player.SetSoldierActive(false);foreach(var t in f.Player.Roots.Values)Require(!t.gameObject.activeSelf,"each category hidden");f.Ui.CurrentPage=2;f.Input.Down=true;f.Input.Position.y=900;f.Player.Updata(0,0);Require(!f.Player.Rotation.IsFingerMove,"hidden selected root skips input");f.Player.SetSoldierActive(true);
            }});
            check("source-player-touch-gates-before-screenpoint-and-source-layer-map",()=>{using(var f=new Fixture()){
                Require(LayerMask.NameToLayer("Scenes11")==31,"original TagManager source index31 restored");f.Player.ScreenPoint=new Vector3(1,2,3);f.Core.State.PlayState=6;f.Ui.CurrentPage=3;f.Player.OnTouchBegin(4,5);Require(f.Player.ScreenPoint==new Vector3(1,2,3),"nonhome gate");f.Core.State.PlayState=2;f.Ui.CurrentPage=2;f.Player.OnTouchBegin(4,5);Require(f.Player.ScreenPoint.x==1,"nonscene-shop gate");f.Ui.CurrentPage=3;f.Player.OnTouchBegin(4,5);Require(f.Player.ScreenPoint==new Vector3(4,5,3),"source shared ray vector overwrites xy only");
            }});
            check("source-player-live-model-placement-separate-from-ray-and-native-dead-animator",()=>{using(var f=new Fixture()){
                f.Player.ScreenPoint=new Vector3(100,200,3);f.Player.SourceVector188=new Vector3(1,2,3);f.Player.ModelScale=2;f.Skins.SetUsedSkin(1,102);f.Player.ChangeSkinUse(1,102);var model=f.Player.Models.Cached(1,102);Require(model.transform.localPosition==new Vector3(1,2,3)&&model.transform.localScale==Vector3.one*2,"callback reads live placement188/scale184, not raypoint172");var animator=model.GetComponent<OutgameBakedAnimator>();UnityEngine.Object.DestroyImmediate(animator);int warnings=f.Warnings.Count;f.Player.RefreshAnimation(1);Require(f.Warnings.Count==warnings&&f.Core.Calls[f.Core.Calls.Count-1]==model.name+" 没有SpineAnimtor","destroyed cached native animator uses source Unity null diagnostic rather than CLR callback failure");
            }});
            return report;
        }
    }
}
