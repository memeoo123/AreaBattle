using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameControllerPlatform
    {
        int GetAppChannelId();
        void SetAppChannelId(int channel);
        bool IsInstallVersion {get;}
        long GetServerTimeByServerTimeZone();
        int GetNetworkingState();
        DateTime LocalNow {get;}
        bool IsReleaseVersion {get;}
    }
    // The first recovered controller service group. Remaining roster entries must be
    // explicitly bound by their own recovered services; this does not complete Main startup.
    public static class OutgameCoreControllerBindings
    {
        public static void Bind(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,IOutgameControllerPlatform platform,Func<OutgameMessageDispatcher> messages,OutgameLegacyCommanderRepair repair,Action<Color,string> log,Action<string> warning,Action<string> error)
        {
            if(platform==null)throw new ArgumentNullException(nameof(platform));
            registry.Bind(4025,()=>new OutgameBuffControl(registry,messages));
            registry.Bind(4561,()=>new OutgamePrefabPoolControl(registry,warning,error));
            registry.Bind(4034,()=>new OutgameServerTimeControl(platform.GetServerTimeByServerTimeZone,platform.GetNetworkingState,()=>platform.LocalNow,()=>platform.IsReleaseVersion,messages,registry));
            registry.Bind(4027,()=>new OutgameCommanderControl(pool,registry));
            registry.Bind(3903,()=>new OutgameProcessControl(config,platform.GetAppChannelId,()=>platform.IsInstallVersion,platform.SetAppChannelId,log,warning,ChannelName,registry));
            registry.Bind(4118,()=>new OutgameLocalDataControl(pool,registry,messages,()=>((OutgameServerTimeControl)registry.Resolve(4034)).GetTodayOfYear(),()=>((OutgameServerTimeControl)registry.Resolve(4034)).GetNowTimestampLong(),repair));
        }
        public static void BindUi(OutgameControllerRegistry registry,Func<OutgameUiModuleInitialization> module,Func<OutgameMessageDispatcher> messages,OutgameUiControlGlobals globals,OutgameMenuItems items,Func<IOutgameTopInfoPage> showTop,Func<IOutgameMenuTabPage> showMenu,Func<IOutgameMenuTabPage> getMenu,Action<int,int> switchMenuRoles,Func<string,OutgameLoadingPage> createLoading=null,Func<string,Transform> uiNode=null)
        {
            registry.Bind(4296,()=>new OutgameUiControl(module,registry,messages,globals,items,showTop,showMenu,getMenu,switchMenuRoles,createLoading,uiNode));
        }
        public static void BindLevel(OutgameControllerRegistry registry,OutgameLevelRuntimeState state,OutgameLevelResourceState resources,Func<int> globalPlayerCamp,Func<int> globalCampCount,Func<OutgameLegacyConfigManager> config,Func<bool> pvpActive,Func<OutgameMessageDispatcher> messages,Action<int,bool> setPlayState,Func<OutgameLoadPrefabControl> loader,Action<string> log)
        {registry.Bind(4107,()=>new OutgameLevelControl(registry,state,resources,globalPlayerCamp,globalCampCount,config,pvpActive,messages,setPlayState,loader,log));}
        public static void BindRedDots(OutgameControllerRegistry registry,Action<Color,object[]> log)
        {registry.Bind(4453,()=>new OutgameRedDotControl(registry,log));}
        public static void BindBattle(OutgameControllerRegistry registry,Func<OutgameLegacyConfigManager> config)
        {registry.Bind(4060,()=>new OutgameBattleControl(config,registry));}
        public static void BindGame(OutgameControllerRegistry registry,Func<OutgameLegacyConfigManager> config,IOutgameGameSceneResources resources,Func<int> usedScene,Func<float> heightAdjustment,Action<int> notifyShop)
        {registry.Bind(4064,()=>new OutgameGameControl(config,registry,resources,usedScene,heightAdjustment,notifyShop));}
        public static void BindPrefabLoader(OutgameControllerRegistry registry,Func<OutgameLegacyConfigManager> config,OutgameResLoadHelper resources,Func<OutgamePrefabCache> cache,Action<string> error,IOutgamePrefabPreloadHost preload=null)
        {registry.Bind(4117,()=>new OutgameLoadPrefabControl(config,registry,resources,cache(),error,preload));}
        public static void BindPlayer(OutgameControllerRegistry registry,Func<OutgameGameControl> game,Func<OutgameLevelControl> level,Func<OutgameUiControl> ui,Func<OutgameSkinCatalog> skins,Func<OutgameMessageDispatcher> messages,IOutgamePlayerInput input,Func<OutgameLoadPrefabControl> loader,Func<OutgameLegacyConfigManager> config,string soldierConfig,Action<GameObject> color,Action<string> error,Action<object[]> warning)
        {registry.Bind(4462,()=>new OutgamePlayerControl(registry,game,level,ui,skins,messages,input,loader,config,soldierConfig,color,error,warning));}
        public static void BindAudio(OutgameControllerRegistry registry,Func<OutgameAudioSettings> settings,Func<OutgameUiAudioManager> uiAudio)
        {registry.Bind(3904,()=>new OutgameAudioControl(registry,settings,uiAudio));}
        public static void BindTools(OutgameControllerRegistry registry,Func<OutgameToolDispatcher> dispatcher,Func<OutgameLocalInventory> local,Func<int,long> itemModuleCount,Func<string> purchaseCost)
        {registry.Bind(4256,()=>new OutgameToolControl(dispatcher,local,itemModuleCount,purchaseCost));}
        public static void BindItems(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,Func<OutgameItemConfigManager> config,Func<OutgameMessageDispatcher> messages,Action<string> warning)
        {registry.Bind(3875,()=>new OutgameItemModuleControl(registry,pool,config,messages,warning));}
        public static void BindRank(OutgameControllerRegistry registry,OutgameRankScoreServices score,OutgameRankHomeServices home)
        {
            home.Registry=registry;home.HasCurrent=()=>registry.HasInstance(4134);
            home.UserInfo=()=>((OutgameUserInfoControl)registry.Resolve(4228));score.Home=home;
            score.Current=()=>((OutgameRankControl)registry.Resolve(4134));registry.Bind(4134,()=>new OutgameRankControl(score));
        }
        public static void BindUserInfo(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,Action<string> reportCreateRole)
        {registry.Bind(4228,()=>new OutgameUserInfoControl(registry,pool,config,reportCreateRole));}
        public static void BindGuideBook(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,Func<int> currentLevel,Func<OutgameMessageDispatcher> messages)
        {registry.Bind(4076,()=>new OutgameGuideBookControl(pool,config,currentLevel,messages));}
        public static void BindSevenday(OutgameControllerRegistry registry,OutgameSevendayActivityServices services)
        {registry.Bind(4502,()=>new OutgameSevendayActivityControl(registry,services));}
        public static void BindTasks(OutgameControllerRegistry registry,OutgameTaskControlServices services)
        {registry.Bind(4507,()=>new OutgameTaskControl(services));}
        public static void BindEffects(OutgameControllerRegistry registry,OutgameEffectControlServices services)
        {registry.Bind(4058,()=>new OutgameEffectControl(registry,services));}
        static string ChannelName(int value)=>value== -1?"NOAB":value>=0&&value<5?"channel"+(char)('A'+value):value.ToString();
    }
}
