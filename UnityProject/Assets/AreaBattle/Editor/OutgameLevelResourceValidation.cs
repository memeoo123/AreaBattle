using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelResourceValidation
    {
        sealed class Effects:IOutgameLevelEffects {public void SetStatistic(int id,long count){}public void SaveLocalData(){}}
        sealed class SequenceRandom:System.Random
        {public int NextValue;public readonly List<string> Calls=new List<string>();public override int Next(int min,int max){Calls.Add(min+":"+max);return NextValue++;}}
        sealed class Fixture:IDisposable
        {
            readonly bool oldNext=OutgameLoadPrefabControl.CanLoadNext;readonly int oldCount=OutgameLoadPrefabControl.PreparedCount;readonly int[] oldIds=OutgameLoadPrefabControl.PreloadIds.ToArray();readonly OutgameLoadingPage oldPage=OutgameLoadingPage.Instance;
            public readonly List<string> Errors=new List<string>();public readonly List<OutgameLegacyAssetLoader> Pending=new List<OutgameLegacyAssetLoader>();public readonly List<OutgameLevelConfigAsset> Held=new List<OutgameLevelConfigAsset>();
            public readonly AssetBundle[] Bundles=new AssetBundle[2];public readonly OutgameLegacyConfigManager Config;public readonly OutgameLevelProgression Progression;public readonly OutgameLegacyResourceScheduler Scheduler;public readonly OutgameLevelResourceState Level;public readonly OutgameSkinCatalog Skins;public readonly OutgamePlayerSkinEntities Player;public readonly OutgameGameControl Game;public readonly OutgameLoadPrefabControl Loader;public readonly OutgameLoadingPage Page;public readonly OutgamePrefabPreloadServices Preload;public readonly SequenceRandom Random=new SequenceRandom();public readonly GameObject Template;public int Small,Prepared;public bool LastNoSetBefore;
            public Fixture()
            {
                for(int i=0;i<2;i++){Bundles[i]=AssetBundle.LoadFromFile(Path.Combine(OutgameLevelResourceBundles.Folder,"level-"+(i+1)));if(!Bundles[i])throw new Exception("Build original level fixture bundles first");}
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Errors.Add),new OutgameConfigGlobalValues{BaseGameTimeScale=2,BaseLineRendererMoveSpeed=3},null,null,null,null,null);
                var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),Errors.Add);reader.ReadTable(Config.dicLevel);reader.ReadTable(Config.dicSkin);reader.ReadTable(Config.dicEntityModel);
                Progression=new OutgameLevelProgression(new OutgameProfile{levelID=1},new Effects());
                Scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(l=>Pending.Add(l),n=>{}),Errors.Add);
                var helper=new OutgameResLoadHelper(()=>false,()=>Scheduler,Errors.Add);var registry=new OutgameControllerRegistry();
                Skins=OutgameSkinCatalog.FromOriginal(null,Resources.Load<TextAsset>("Recovered/FirstPack/Config/SkinConfig").text,Resources.Load<TextAsset>("Recovered/FirstPack/Config/SceneSkinConfig").text);
                Game=new OutgameGameControl(()=>Config,registry,null,null,null,null);
                Level=new OutgameLevelResourceState(()=>Config,Progression,helper,()=>Loader,()=>Small,noSet=>{Prepared++;LastNoSetBefore=noSet;},Errors.Add,new GameRandomSource(Random));Level.InitializeMaximum();
                Player=new OutgamePlayerSkinEntities(()=>Skins,()=>Config,()=>Level,()=>Game);
                Page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Loading/Proj_xqzdLoadingUI")).GetComponent<OutgameLoadingPage>();
                Preload=new OutgamePrefabPreloadServices(()=>Level,()=>Player,()=>Page);
                Loader=new OutgameLoadPrefabControl(()=>Config,registry,helper,new OutgamePrefabCache(name=>Resources.Load<OutgameLevelEditorConfig>(name)),Errors.Add,Preload);
                // Explicit test-only warm templates: these cases validate config/queue/callback semantics, not entity asset coverage.
                Template=new GameObject("test-only-warm-template");foreach(var pair in Config.dicEntityModel)Loader.Cache.Prefabs[pair.Value.id]=Template;
            }
            public void Pump()=>Scheduler.Update(0,0);
            public void Complete(int index,int bundle=0){Pending[index].PendingBundle=Bundles[bundle];Pending[index].Complete();if(Level.CurrentLevel)Held.Add(Level.CurrentLevel);}
            public void Dispose(){foreach(var asset in Held)if(asset)UnityEngine.Object.DestroyImmediate(asset);if(Level.CurrentLevel&&!Held.Contains(Level.CurrentLevel))UnityEngine.Object.DestroyImmediate(Level.CurrentLevel);if(Page)UnityEngine.Object.DestroyImmediate(Page.gameObject);UnityEngine.Object.DestroyImmediate(Template);foreach(var bundle in Bundles)if(bundle)bundle.Unload(true);OutgameLoadingPage.Instance=oldPage;OutgameLoadPrefabControl.CanLoadNext=oldNext;OutgameLoadPrefabControl.PreparedCount=oldCount;OutgameLoadPrefabControl.PreloadIds.Clear();OutgameLoadPrefabControl.PreloadIds.AddRange(oldIds);}
        }
        static void Require(bool yes,string message){if(!yes)throw new Exception(message);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-level-config-interleaved-loop-and-unchecked-input",()=>{using(var f=new Fixture()){
                Require(f.Level.MaxLevel==560,"source config count minus one");int[] input={-1,0,560,561,562,822,823,1084,1085},expected={-1,0,560,37,39,559,38,560,37};for(int i=0;i<input.Length;i++)Require(f.Level.ConfigAccess.GetLevelRealId(input[i])==expected[i],"mapped level "+input[i]);f.Level.MaxLevel=36;Throws<DivideByZeroException>(()=>f.Level.ConfigAccess.GetLevelRealId(37));
            }});
            check("source-level-config-owner-field-missing-null-and-speed",()=>{using(var f=new Fixture()){
                Require(f.Level.ConfigAccess.GetLevelConfig(561)==f.Config.dicLevel[37]&&f.Config.CurrentLevelConfig.id==37,"mapped row stored on config owner");Require(f.Level.ConfigAccess.GetLevelConfig(-1)==null&&f.Errors.Count==1&&f.Config.CurrentLevelConfig==null,"miss overwrites previous row before diagnostic");f.Config.dicLevel[-2]=null;Require(f.Level.ConfigAccess.GetLevelConfig(-2)==null&&f.Errors.Count==1,"present null is not a missing-key diagnostic");f.Config.Globals.GameTimeScale=99;f.Config.Globals.LineRendererMoveSpeed=88;f.Level.ConfigAccess.SetLevelSpeed(1);Require(f.Config.Globals.GameTimeScale==2&&f.Config.Globals.LineRendererMoveSpeed==3&&f.Config.CurrentLevelConfig.id==1,"speed restored from base, independent of guide array");
            }});
            check("source-guide-raw-level-upper-only-index-clamp",()=>{using(var f=new Fixture()){
                Require(f.Level.ConfigAccess.IsGuideLevel(1),"first sublevel guide");f.Small=900;Require(!f.Level.ConfigAccess.IsGuideLevel(1),"upper index clamps to final nonguide");f.Small=-1;Throws<IndexOutOfRangeException>(()=>f.Level.ConfigAccess.IsGuideLevel(1));Require(!f.Level.ConfigAccess.IsGuideLevel(561),"guide query does not wrap levels");f.Config.dicLevel[1].LevelType=new int[0];f.Small=0;Throws<IndexOutOfRangeException>(()=>f.Level.ConfigAccess.IsGuideLevel(1));
            }});
            check("source-enemy-skin-nine-random-calls-interleaved-categories",()=>{using(var f=new Fixture()){
                f.Level.InitEnemySkin();Require(string.Join(",",f.Level.EnemyNormal)=="0,3,6"&&string.Join(",",f.Level.EnemyDefense)=="1,4,7"&&string.Join(",",f.Level.EnemyAttack)=="2,5,8","source index then category call order");Require(f.Random.Calls.Count==9&&f.Random.Calls.TrueForAll(x=>x=="0:27"),"81 rows divided by3, managed exclusive upper27");
            }});
            check("source-player-used-skin-fallback-animation-and-missing-type",()=>{using(var f=new Fixture()){
                Require(f.Player.GetSkinEntityByType(1)==1000&&f.Player.GetSkinEntityByType(2)==2000&&f.Player.GetSkinEntityByType(3)==3000,"source equipped defaults");f.Skins.SetUsedSkin(1,102);Require(f.Player.GetSkinEntityByType(1)==1002,"prefab from selected row");f.Skins.SetUsedSkin(1,99999);Require(f.Player.GetSkinEntityByType(1)==1000,"missing selected row uses category fallback");f.Game.UseAnimationIns=true;Require(f.Player.GetSkinEntityByType(1)==4000,"animation offset applied after fallback");Throws<KeyNotFoundException>(()=>f.Player.GetSkinEntityByType(99));
            }});
            check("source-player-enemy-camp-clamp-row-zero-and-animation",()=>{using(var f=new Fixture()){
                f.Level.EnemyNormal=new[]{0,1,2};Require(f.Player.GetSkinEntityByRandomType(1,-9)==1000&&f.Player.GetSkinEntityByRandomType(1,3)==1001&&f.Player.GetSkinEntityByRandomType(1,99)==1002,"camp clamped to2..4");f.Level.EnemyAttack[0]=999;Require(f.Player.GetSkinEntityByRandomType(3,2)==3000,"missing random row category fallback");f.Config.dicSkin[0]=new SkinConfig{prefabId=77};Require(f.Player.GetSkinEntityByRandomType(8,2)==77,"nonstandard category still queries row0");f.Game.UseAnimationIns=true;Require(f.Player.GetSkinEntityByRandomType(8,2)==3077,"animation applies to row0 too");
            }});
            check("source-level-home-native-json-scriptableobject-and-preload-adapter",()=>{using(var f=new Fixture()){
                f.Small=2;f.Level.LoadHomeLevel(1);Require(f.Level.CurrentLevel==null,"no synchronous data fabrication");f.Pump();Require(f.Pending[0].BundleName=="data/levelcfg/level_1.json.unity3d","source filename and lowercased bundle route");f.Complete(0);Require(f.Level.Loaded.Item1==1&&f.Level.Loaded.Item2==2&&f.Level.CurrentLevel.StarInfoCfgs.Length==4&&f.Level.CurrentLevel.CampInfoCfgs.Length==3,"native original TextAsset parsed into original ScriptableObject field layout");Require(ReferenceEquals(f.Preload.CurrentLevel,f.Level.CurrentLevel)&&OutgameLoadPrefabControl.CanLoadNext&&f.Prepared==0,"same owned level drives home preload, no game-init callback");
            }});
            check("source-level-home-repeat-load-replaces-tuple-even-same-id",()=>{using(var f=new Fixture()){
                f.Level.LoadHomeLevel(1);f.Pump();f.Complete(0);var old=f.Level.CurrentLevel;f.Small=1;f.Level.LoadHomeLevel(1);Require(f.Level.CurrentLevel==old,"cached resource still dispatched next scheduler update");f.Pump();Require(f.Level.CurrentLevel!=old&&old&&f.Level.Loaded.Item2==1&&f.Pending.Count==1,"same-id tuple does not short-circuit, old ScriptableObject not destroyed");
            }});
            check("source-level-coalesced-resource-arguments-last-writer",()=>{using(var f=new Fixture()){
                f.Level.LoadHomeLevel(1);f.Config.dicLevel[2].SceneId=1;f.Small=2;f.Level.LoadHomeLevel(2);f.Pump();Require(f.Pending.Count==1,"same original bundle is coalesced");f.Complete(0);Require(f.Level.Loaded.Item1==2&&f.Level.Loaded.Item2==2,"completion tuple uses mutable resource arguments, not requested identity closure");
            }});
            check("source-level-prepare-flag-guide-speed-and-loading-time-reset",()=>{using(var f=new Fixture()){
                f.Page.Elapsed=4.9f;f.Level.LoadLevel(1);f.Pump();f.Complete(0);Require(f.Prepared==1&&!f.LastNoSetBefore&&f.Level.IsGuideLevel&&f.Config.Globals.GameTimeScale==2,"source prepare completion receives missing bool argument defaultfalse");Require(f.Page.Elapsed==0&&!OutgameLoadPrefabControl.CanLoadNext&&OutgameLoadPrefabControl.PreparedCount==OutgameLoadPrefabControl.PreloadIds.Count,"warm preparation resets concrete loading page and finishes shared cursor");
                var resource=new OutgameLegacyPrefabResource(f.Bundles[1]){Arguments=new object[]{"Data/LevelCfg/Level_2.Json",2,7,true}};f.Level.OnLevelConfigLoaded(resource);Require(f.Prepared==2&&f.LastNoSetBefore&&f.Level.CurrentLevel.Stars.Length==3&&f.Level.Loaded.Item2==7,"explicit argument3 and resource filename drive direct completion");
            }});
            check("source-level-special-mode-resource-identity-bypasses-config-map",()=>{using(var f=new Fixture()){
                f.Progression.SpecialState=2;f.Config.dicLevel[2].SceneId=1;f.Level.LoadHomeLevel(2);f.Pump();Require(f.Pending[0].BundleName=="data/levelcfg/level_2.json.unity3d","special source request bypasses normal SceneId mapping");f.Complete(0,1);Require(f.Level.Loaded.Item1==2&&f.Level.CurrentLevel.Stars.Length==3,"special tuple uses requested identity");
            }});
            check("source-level-resource-failure-order-and-preload-page-null",()=>{using(var f=new Fixture()){
                f.Level.LoadHomeLevel(1);f.Pump();f.Complete(0);var old=f.Level.Loaded;
                Throws<NullReferenceException>(()=>f.Level.OnLevelConfigLoaded(new OutgameLegacyPrefabResource(f.Bundles[0]){Arguments=new object[]{"missing.Json",1,0}}));Require(ReferenceEquals(old,f.Level.Loaded),"asset text failure precedes replacement");
                Throws<InvalidCastException>(()=>f.Level.OnLevelConfigLoaded(new OutgameLegacyPrefabResource(f.Bundles[0]){Arguments=new object[]{"Level_1.Json","wrong",0}}));Require(ReferenceEquals(old,f.Level.Loaded),"id cast failure precedes new tuple assignment");new OutgamePrefabPreloadServices(()=>f.Level,()=>f.Player,()=>null).ResetLoadingProgress();f.Page.Elapsed=4;f.Preload.ResetLoadingProgress();Require(f.Page.Elapsed==0,"absent UI allowed and live UI reset");
            }});
            return report;
        }
    }
}
