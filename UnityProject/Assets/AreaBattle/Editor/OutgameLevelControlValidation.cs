using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelControlValidation
    {
        internal class Tower:IOutgameLevelTower
        {
            public bool Enabled=true;public int Camp;public float Amount;public Action<int,StarInfoCfg> OnInit;public Action OnClear;public Action<float,float> Tick;
            public GameObject Entity {get;set;}public Vector3 SourcePosition68 {get;set;}
            public bool Active=>Enabled;public int CampId=>Camp;public float Score=>Amount;
            public void Clear(){OnClear?.Invoke();Enabled=false;}
            public void Init(int id,StarInfoCfg cfg){OnInit?.Invoke(id,cfg);}public void Updata(float d,float u){Tick?.Invoke(d,u);}
        }
        sealed class Effects:IOutgameLevelEffects{public void SetStatistic(int id,long count){}public void SaveLocalData(){}}
        internal sealed class Fixture:IDisposable
        {
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();public readonly OutgameLevelRuntimeState State=new OutgameLevelRuntimeState();public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();public readonly List<string> Calls=new List<string>();
            public readonly OutgameLegacyConfigManager Config;public readonly OutgameLevelResourceState Resources;public readonly OutgameLoadPrefabControl Loader;public readonly OutgameLevelControl Level;public readonly OutgameLevelConfigAsset Asset;public bool Pvp;public int Player=1,CampCount=6,ConfigReads;
            public Fixture()
            {
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Calls.Add),new OutgameConfigGlobalValues{GameTimeScale=2},null,null,null,null,null);Config.dicLevel[0]=new OriginalConfig.LevelConfig();Config.dicLevel[1]=new OriginalConfig.LevelConfig();
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(l=>{},n=>{}),Calls.Add);var helper=new OutgameResLoadHelper(()=>false,()=>scheduler,Calls.Add);
                Resources=new OutgameLevelResourceState(()=>Config,new OutgameLevelProgression(new OutgameProfile{levelID=1},new Effects()),helper,()=>Loader,()=>0,b=>{},Calls.Add);
                Loader=new OutgameLoadPrefabControl(()=>Config,Registry,helper,new OutgamePrefabCache(n=>UnityEngine.Resources.Load<OutgameLevelEditorConfig>(n)),Calls.Add);
                Asset=ScriptableObject.CreateInstance<OutgameLevelConfigAsset>();Asset.StarInfoCfgs=new StarInfoCfg[0];Asset.ObstacleInfoCfgs=new ObstacleInfoCfg[0];Resources.Loaded=Tuple.Create(1,0,Asset);
                Level=new OutgameLevelControl(Registry,State,Resources,()=>Player,()=>CampCount,()=>{ConfigReads++;return Config;},()=>Pvp,()=>Messages,(s,b)=>Calls.Add("state:"+s+":"+b),()=>Loader,Calls.Add);Level.OnInit();Messages.AddListener("CampChange",a=>Calls.Add("camp"));
            }
            public Tower Add(int camp,float score,bool active=true){var t=new Tower{Camp=camp,Amount=score,Enabled=active};Level.Towers.Add(t);return t;}
            public void Dispose(){if(Loader.EntityRoot)UnityEngine.Object.DestroyImmediate(Loader.EntityRoot);UnityEngine.Object.DestroyImmediate(Asset);}
        }
        sealed class PlayHost:IOutgamePlayStateHost
        {
            public void PauseGame(bool b){}public void AudioState(int s){}public void WayLineState(int s){}public void AiState(int s){}public void SkillState(int s){}public void PublishGamePlayState(int s){}public bool PvpActive=>false;public bool DiceActive=>false;public void SetCameraSize(){}public void DispatchStateBranch(int s){}
        }
        sealed class Lines:IOutgameLevelWayLines
        {public readonly List<string> Calls;public Lines(List<string> calls){Calls=calls;}public void Clear()=>Calls.Add("clear");public void InitPrePrefabLoad()=>Calls.Add("prefabs");public void InitStarLine()=>Calls.Add("lines");}
        sealed class PlayPage:IOutgamePreparedPlayPage {public Action Refreshed;public void Refresh()=>Refreshed();}
        sealed class PlayUi:IOutgamePreparedPlayUi {public IOutgamePreparedPlayPage Page;public readonly List<string> Calls;public PlayUi(List<string> calls){Calls=calls;}public IOutgamePreparedPlayPage GetPlayPage(){Calls.Add("get");return Page;}public void ShowPlayPage()=>Calls.Add("show");}
        static void Require(bool yes,string message){if(!yes)throw new Exception(message);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-level-lifecycle-shared-dispatch-state-retains-owned-data",()=>{using(var f=new Fixture()){
                Require(f.Level.PlayerCampId==1&&f.Resources.MaxLevel==1,"Global.PlayCampID then configcount-1");f.Player=3;f.Level.OnInit();Require(f.Level.PlayerCampId==3,"reinit reads live global");var dispatcher=new OutgamePlayStateDispatcher(new PlayHost(),f.State);dispatcher.SetPlaySate(6);f.Registry.Bind(4107,()=>f.Level);Require(ReferenceEquals(f.Registry.Resolve(4107),f.Level),"actual registry instance");f.Add(1,5);f.Level.OnDispose();Require(dispatcher.State==0&&!f.Registry.HasInstance(4107)&&f.Level.Towers.Count==1&&f.Resources.CurrentLevel==f.Asset,"dispose clears registry/state with no resource or tower cleanup");
            }});
            check("source-level-tick-live-scale-unscaled-time-and-once-per-frame-refresh",()=>{using(var f=new Fixture()){
                var times=new List<float>();f.Add(1,2).Tick=(d,u)=>{times.Add(d);times.Add(u);f.Config.Globals.GameTimeScale=3;};f.Add(2,3).Tick=(d,u)=>{times.Add(d);times.Add(u);};f.Add(3,4,false).Tick=(d,u)=>throw new Exception("inactive");
                f.Level.Updata(2,.7f);Require(times.Count==0,"nonrunning state gate");f.State.PlayState=6;f.Level.TotalTowerCount=3;f.Level.NeedsCampRefresh=true;f.Level.CampRefreshRemaining=.5f;f.Level.Updata(2,.7f);
                Require(string.Join(",",times)=="4,0.7,6,0.7"&&f.ConfigReads==4,"scale read for each active tower then each missing camp color");Require(f.Level.CampRefreshRemaining==-.5f&&!f.Level.NeedsCampRefresh&&f.Calls.Count==1,"refresh once adds1, no catchup loop despite negative remainder");
            }});
            check("source-level-tick-foreach-mutation-and-refresh-failure-order",()=>{using(var f=new Fixture()){
                f.State.PlayState=6;f.Add(1,1).Tick=(d,u)=>f.Add(2,1);f.Level.NeedsCampRefresh=true;f.Level.CampRefreshRemaining=2;Throws<InvalidOperationException>(()=>f.Level.Updata(1,1));Require(f.Level.CampRefreshRemaining==2,"enumerator mutation aborts before timer");f.Level.Towers.Clear();f.Add(1,1);f.Level.TotalTowerCount=2;f.Level.CampRefreshRemaining=.25f;f.Messages.AddListener("CampChange",a=>throw new InvalidOperationException("listener"));Throws<InvalidOperationException>(()=>f.Level.Updata(1,1));Require(f.Level.CampRefreshRemaining==-.75f&&!f.Level.NeedsCampRefresh,"callback failure retains subtracted timer and cleared refresh flag, before add1");
            }});
            check("source-camp-refresh-active-history-orders-neutral-total-and-colors",()=>{using(var f=new Fixture()){
                f.State.PlayState=2;var previous=new OutgameCampInfo();previous.Init(7,4,Color.red);previous.TowerCount=9;previous.Score=99;f.Level.Camps.Add(previous);var inactive=new OutgameCampInfo{Index=100,CampId=9,TowerCount=7,Score=8};f.Level.Camps.Add(inactive);
                f.Config.LineColors[2]=Color.blue;f.Add(1,2.9f);f.Add(2,3.9f);f.Add(0,10.8f);f.Add(2,-1.2f);f.Add(3,999,false);f.Level.NeedsCampRefresh=true;f.Level.RefreshCampInfo();
                Require(previous.IsActive&&previous.TowerCount==0&&previous.Score==0&&ReferenceEquals(inactive,f.Level.GetCampInfoByCampId(1)),"active histories persist, first inactive slot reused");Require(f.Level.GetCampInfoByCampId(1).Index==0&&f.Level.GetCampInfoByCampId(2).Index==8&&f.Level.GetCampInfoByCampId(0).Index==6,"player0, nextmax8, neutralGlobalCampCount");Require(f.Level.GetCampInfoByCampId(2).TowerCount==2&&f.Level.GetCampInfoByCampId(2).Score==2&&f.Level.TotalScore==4,"individual truncation, neutral excluded from total");Require(f.Level.GetCampInfoByCampId(2).Color==Color.blue&&f.Level.GetCampInfoByCampId(0).Color==Color.green&&string.Join(",",f.Calls)=="camp","config color fallback and home-state result bypass");
            }});
            check("source-camp-missing-player-is-not-defeat-and-retained-empty-player-is",()=>{using(var f=new Fixture()){
                f.State.PlayState=6;f.Level.TotalTowerCount=3;f.Add(2,2);f.Level.RefreshCampInfo();Require(string.Join(",",f.Calls)=="camp","no player record => no result");var mine=new OutgameCampInfo();mine.Init(0,1,Color.green);f.Level.Camps.Add(mine);f.Calls.Clear();f.Level.RefreshCampInfo();Require(string.Join(",",f.Calls)=="camp,state:9:False","existing nowempty player => defeat");
            }});
            check("source-camp-result-after-event-live-state-and-pvp-gates",()=>{using(var f=new Fixture()){
                f.Add(1,2);f.Level.TotalTowerCount=1;f.State.PlayState=6;f.Pvp=true;f.Level.RefreshCampInfo();Require(f.Calls.Count==1,"pvp bypass");f.Pvp=false;f.Calls.Clear();f.Messages.AddListener("CampChange",a=>f.State.PlayState=2);f.Level.RefreshCampInfo();Require(f.Calls.Count==1,"state read after camp event, not captured before");
            }});
            check("source-camp-boss-result-truncation-overflow-and-no-active-gate",()=>{using(var f=new Fixture()){
                f.State.PlayState=6;f.Level.TotalTowerCount=9;f.Add(1,float.NaN);f.Add(0,1);f.Level.CurrentBoss=new Tower{Enabled=false,Amount=.9f};f.Level.RefreshCampInfo();Require(f.Level.TotalScore==int.MinValue&&string.Join(",",f.Calls)=="camp,state:8:False","WASM nonfinite int conversion and inactive boss score truncation");f.Calls.Clear();f.Level.CurrentBoss=null;f.Level.TotalTowerCount=1;f.Level.RefreshCampInfo();Require(f.Calls[1]=="state:8:False","all towers owned win path before boss");
            }});
            check("source-camp-clear-preserves-color-and-lookup-skips-inactive",()=>{using(var f=new Fixture()){
                var camp=f.Level.AcquireCampInfo();Require(!camp.IsActive&&camp.CampId==0&&camp.Index==0,"new object defaultfields without implicit Init");camp.Init(2,3,Color.red);camp.Score=99;camp.TowerCount=2;camp.Clear();Require(!camp.IsActive&&camp.Index==-1&&camp.CampId==-1&&camp.Score==0&&camp.TowerCount==0&&camp.Color==Color.red,"Clear source exact fields");Require(f.Level.GetCampInfoByCampId(-1)==null&&ReferenceEquals(f.Level.AcquireCampInfo(),camp),"inactive ignored bylookup reused bypool");
            }});
            check("source-level-tower-init-live-total-boss-last-and-partial-failure",()=>{using(var f=new Fixture()){
                f.Asset.StarInfoCfgs=new[]{new StarInfoCfg{ShipID=2},new StarInfoCfg{isBoss=true},new StarInfoCfg{isBoss=true}};var seen=new List<string>();var boss1=new Tower{OnInit=(id,cfg)=>seen.Add("boss:"+id)};var boss2=new Tower{OnInit=(id,cfg)=>seen.Add("boss:"+id)};int bosses=0;
                f.Level.InitializeTowers(type=>new Tower{OnInit=(id,cfg)=>seen.Add(type+":"+id)},()=>++bosses==1?boss1:boss2);Require(string.Join(",",seen)=="2:1,boss:2,boss:3"&&f.Level.CurrentBoss==boss2&&f.Level.HasBoss&&f.Level.TotalTowerCount==3,"one-based ids and last boss retained");f.Level.HasBoss=false;f.Level.InitializeTowers(type=>new Tower{OnInit=(id,cfg)=>f.Level.TotalTowerCount=1},()=>throw new Exception("must not reach"));Require(!f.Level.HasBoss,"source live total loop observes mutation");
                f.Asset.StarInfoCfgs=new[]{new StarInfoCfg{isBoss=true}};var failing=new Tower{OnInit=(id,cfg)=>throw new InvalidOperationException()};Throws<InvalidOperationException>(()=>f.Level.InitializeTowers(null,()=>failing));Require(f.Level.CurrentBoss==failing&&!f.Level.HasBoss,"boss reference stored beforeInit; flag only after success");
            }});
            check("source-level-obstacle-native-prefab-transform-and-append",()=>{using(var f=new Fixture()){
                f.Loader.Cache.Prefabs[81]=UnityEngine.Resources.Load<GameObject>("Recovered/Obstacles/Entity_81");var cfg=new ObstacleInfoCfg{EnityID=81,pos=new IntVector3{x=125,z=-50},angle=new IntVector3{y=9000},scale=new IntVector3{x=200,y=100,z=50}};f.Asset.ObstacleInfoCfgs=new[]{cfg};f.Level.InitializeObstacles();var go=f.Level.ObstacleObjects[0];Require(go&&go.GetComponentInChildren<MeshFilter>().sharedMesh!=null&&go.transform.position==new Vector3(1.25f,0,-.5f)&&go.transform.localScale==new Vector3(2,1,.5f)&&Mathf.Abs(go.transform.eulerAngles.y-90)<.001f,"original native obstacle geometry with /100 position/angle/local scale");f.Level.InitializeObstacles();Require(f.Level.ObstacleObjects.Count==2&&f.Level.ObstaclesInitialized&&f.Calls.Contains("Play: 障碍物初始完毕"),"append retained objects, no implicit clear");
            }});
            check("source-level-obstacle-empty-null-and-failure-flags",()=>{using(var f=new Fixture()){
                f.Asset.ObstacleInfoCfgs=null;f.Level.InitializeObstacles();Require(f.Level.ObstaclesInitialized&&f.Calls.Count==0,"null array no completionlog");f.Asset.ObstacleInfoCfgs=new ObstacleInfoCfg[]{null};Throws<NullReferenceException>(()=>f.Level.InitializeObstacles());Require(!f.Level.ObstaclesInitialized,"partial initialization keepsfalse");
            }});
            check("source-level-line-start-coroutine-two-scaled-waits-and-live-services",()=>{using(var f=new Fixture()){
                int resolves=0;var calls=new List<string>();var lines=new Lines(calls);var prepared=new OutgameLevelPreparedStart(f.Level,null,null,null,null,null,null,null,()=>{resolves++;return lines;},()=>calls.Add("ai"));var routine=prepared.InitializeLines();Require(calls.Count==0,"iterator is lazy");Require(routine.MoveNext()&&routine.Current is WaitForSeconds&&string.Join(",",calls)=="clear","first scaled wait after clear");var first=routine.Current;Require(routine.MoveNext()&&routine.Current is WaitForSeconds&&!ReferenceEquals(first,routine.Current)&&string.Join(",",calls)=="clear,prefabs,lines"&&resolves==3,"second wait uses independently resolved line services");Require(!routine.MoveNext()&&string.Join(",",calls)=="clear,prefabs,lines,ai","AI initializes only after second wait");
            }});
            check("source-level-post-prepare-order-real-scene-and-ui-refresh",()=>{using(var f=new Fixture()){
                var root=UnityEngine.Object.Instantiate(UnityEngine.Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));
                try{
                    f.State.PlayState=2;f.Asset.StarInfoCfgs=new[]{new StarInfoCfg{ShipID=2}};
                    var scene=root.GetComponentInChildren<OutgameGameSceneMono>(true);var game=new OutgameGameControl(()=>f.Config,f.Registry,null,null,null,null,width:()=>1080,height:()=>1920);game.SceneRoot=scene;game.GameCamera=scene.GameCamera;game.HomeCamera=scene.HomeCamera;game.CommanderCamera=scene.CommanderCamera;
                    var ui=new PlayUi(f.Calls);ui.Page=new PlayPage{Refreshed=()=>f.Calls.Add("refresh")};IEnumerator routine=null;
                    var prepared=new OutgameLevelPreparedStart(f.Level,type=>{var tower=f.Add(1,5);tower.OnInit=(id,cfg)=>f.Calls.Add("init:"+type+":"+id);return tower;},()=>throw new Exception("boss"),()=>{f.Calls.Add("game");return game;},()=>{f.Calls.Add("ui");return ui;},(state,opt)=>f.Calls.Add("set:"+state+":"+opt),()=>{f.Calls.Add("level");return 1;},value=>{f.Calls.Add("coroutine");routine=value;},()=>new Lines(f.Calls),()=>f.Calls.Add("ai"));
                    prepared.Complete(false);Require(string.Join(",",f.Calls)=="init:2:1,game,ui,get,ui,get,refresh,camp,set:4:True,game,level,coroutine","source callback exact order, second UI getter and currentLevel side effect");Require(scene.Scene_game.gameObject.activeSelf&&!scene.Scene_home.gameObject.activeSelf&&scene.GameCamera.gameObject.activeSelf&&!scene.HomeCamera.gameObject.activeSelf&&Mathf.Abs(game.GameCamera.orthographicSize-2.1f)<.001f&&routine!=null,"real source scene cameras/background switched and resized before starting coroutine");
                    f.Calls.Clear();ui.Page=null;prepared.Complete(true);Require(f.Calls.Contains("show")&&!f.Calls.Contains("refresh")&&!f.Calls.Contains("set:4:True"),"absent PlayUI shown; noSetBefore skips only state4");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            }});
            check("source-level-post-prepare-failure-stops-before-scene-side-effects",()=>{using(var f=new Fixture()){
                f.Asset.StarInfoCfgs=null;int sceneReads=0;var prepared=new OutgameLevelPreparedStart(f.Level,null,null,()=>{sceneReads++;return null;},null,null,null,null,null,null);Throws<NullReferenceException>(()=>prepared.Complete(false));Require(sceneReads==0,"tower initialization failure stops before obstacle/scene/UI/state/coroutine");
            }});
            check("source-level-line-iterator-dispose-current-reset-and-failure-state",()=>{using(var f=new Fixture()){
                var calls=new List<string>();var lines=new Lines(calls);var prepared=new OutgameLevelPreparedStart(f.Level,null,null,null,null,null,null,null,()=>lines,()=>throw new InvalidOperationException("ai"));var routine=prepared.InitializeLines();Require(routine.Current==null,"new iterator current is null");Throws<NotSupportedException>(()=>routine.Reset());Require(routine.MoveNext(),"first yield");((IDisposable)routine).Dispose();Require(routine.MoveNext(),"source emptyDispose does not cancel later MoveNext");var current=routine.Current;Throws<InvalidOperationException>(()=>routine.MoveNext());Require(!routine.MoveNext()&&ReferenceEquals(current,routine.Current),"failure sets terminal state but retains last yielded object");
            }});
            return report;
        }
    }
}
