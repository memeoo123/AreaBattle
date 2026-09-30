using System;
using UnityEngine;
namespace AreaBattle
{
    // LevelControl resource/config portion. Full controller tick, towers and state host are separate.
    public sealed class OutgameLevelResourceState
    {
        readonly Func<OutgameLegacyConfigManager> config;readonly OutgameLevelProgression progression;
        readonly OutgameResLoadHelper resources;readonly Func<OutgameLoadPrefabControl> loader;
        readonly Func<int> smallLevel;readonly Action<bool> initializePreparedGame;
        readonly GameRandomSource random;
        public readonly OutgameLevelConfigAccess ConfigAccess;
        public Tuple<int,int,OutgameLevelConfigAsset> Loaded;
        public int MaxLevel;
        public bool IsGuideLevel;
        public int[] EnemyNormal=new int[3],EnemyDefense=new int[3],EnemyAttack=new int[3];
        public OutgameLevelConfigAsset CurrentLevel=>Loaded==null?null:Loaded.Item3;
        public OutgameLevelResourceState(Func<OutgameLegacyConfigManager> config,OutgameLevelProgression progression,OutgameResLoadHelper resources,Func<OutgameLoadPrefabControl> loader,Func<int> smallLevel,Action<bool> initializePreparedGame,Action<string> log,GameRandomSource random=null)
        {
            this.config=config;this.progression=progression;this.resources=resources;this.loader=loader;this.smallLevel=smallLevel;this.initializePreparedGame=initializePreparedGame;this.random=random??GameRandomSource.Shared;
            ConfigAccess=new OutgameLevelConfigAccess(config,()=>MaxLevel,smallLevel,log);
        }
        public void InitializeMaximum()=>MaxLevel=config().dicLevel.Count-1; //31530, called by controller OnInit.
        public void InitEnemySkin() //31471: independent inclusive random calls in index/category order.
        {
            int count=config().dicSkin.Count/3;
            for(int i=0;i<3;i++){EnemyNormal[i]=random.Inclusive(0,count-1);EnemyDefense[i]=random.Inclusive(0,count-1);EnemyAttack[i]=random.Inclusive(0,count-1);}
        }
        void Resolve(int requested,out int identity,out string path)
        {
            identity=requested;int scene=requested;
            if(progression.SpecialState==0){var row=ConfigAccess.GetLevelConfig(requested);scene=row.SceneId;identity=row.id;}
            path=OutgameResourcePaths.GetPath("Level_"+scene,4);
        }
        public void LoadHomeLevel(int requested) //31489/31533: no ordinary cache-hit shortcut in source.
        {
            Resolve(requested,out int identity,out string path);
            // Source checks Item1!=identity OR Item1==identity and therefore always loads for a stable tuple.
            resources.LoadAsset(path,resource=>{
                string json=resource.LoadAsset<TextAsset>(OutgameResourcePaths.ToFileName(path)).text;
                Loaded=new Tuple<int,int,OutgameLevelConfigAsset>(resource.GetArg<int>(1),resource.GetArg<int>(2),ScriptableObject.CreateInstance<OutgameLevelConfigAsset>());
                JsonUtility.FromJsonOverwrite(json,Loaded.Item3);loader().PreLoad();
            },new object[]{path,identity,smallLevel()});
        }
        public void LoadLevel(int requested) //31505: same redundant cache predicate as home route.
        {
            Resolve(requested,out int identity,out string path);
            resources.LoadAsset(path,OnLevelConfigLoaded,new object[]{path,identity,smallLevel()});
        }
        public void OnLevelConfigLoaded(OutgameLegacyPrefabResource resource) //31529: use mutable resource args.
        {
            string json=resource.LoadAsset<TextAsset>(OutgameResourcePaths.ToFileName(resource.GetArg<string>(0))).text;
            Loaded=new Tuple<int,int,OutgameLevelConfigAsset>(resource.GetArg<int>(1),resource.GetArg<int>(2),ScriptableObject.CreateInstance<OutgameLevelConfigAsset>());
            JsonUtility.FromJsonOverwrite(json,Loaded.Item3);
            InitGameByLevelConfig(resource.GetArg<int>(1),resource.GetArg<bool>(3));
        }
        public void InitGameByLevelConfig(int identity,bool noSetBefore)
        {
            ConfigAccess.SetLevelSpeed(identity);IsGuideLevel=ConfigAccess.IsGuideLevel(progression.CurrentLevel);
            loader().PrepareStart(()=>initializePreparedGame(noSetBefore));
        }
    }
    // Concrete preload adapter: shared level tuple, equipped skin catalog and actual loading page.
    public sealed class OutgamePrefabPreloadServices:IOutgamePrefabPreloadHost
    {
        readonly Func<OutgameLevelResourceState> level;readonly Func<OutgamePlayerSkinEntities> player;readonly Func<OutgameLoadingPage> loading;
        public OutgamePrefabPreloadServices(Func<OutgameLevelResourceState> level,Func<OutgamePlayerSkinEntities> player,Func<OutgameLoadingPage> loading)
        {this.level=level;this.player=player;this.loading=loading;}
        public IOutgameLevelLayout CurrentLevel=>level().CurrentLevel;
        public int GetSkinEntityByType(int id)=>player().GetSkinEntityByType(id);
        public int GetSkinEntityByRandomType(int id,int camp)=>player().GetSkinEntityByRandomType(id,camp);
        public void ResetLoadingProgress(){var page=loading();if(!ReferenceEquals(page,null))page.ResetTime();}
    }
}
