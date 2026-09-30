using System;
namespace AreaBattle
{
    // MineGameMain30607/30603. Subsequent30617 composition is supplied explicitly.
    public sealed class OutgameMainConfigStartup
    {
        readonly Action<string> log;readonly Action gameStart,afterConfig;
        readonly Action<string,Action<OutgameLegacyPrefabResource>,object[]> loadAsset;
        readonly Func<OutgameLegacyConfigManager> configManager;
        public bool ConfigLoaded {get;private set;}
        public OutgameMainConfigStartup(Action<string> log,Action gameStart,
            Action<string,Action<OutgameLegacyPrefabResource>,object[]> loadAsset,
            Func<OutgameLegacyConfigManager> configManager,Action afterConfig)
        {this.log=log;this.gameStart=gameStart;this.loadAsset=loadAsset;this.configManager=configManager;this.afterConfig=afterConfig;}
        public void LoadingShown()
        {
            log("LoadingUIShow");gameStart();loadAsset("data/config",Loaded,Array.Empty<object>());
        }
        public void Loaded(OutgameLegacyPrefabResource resource)
        {
            log("ConfigLoadOver");configManager().SetConfigABRes(resource);configManager().Initialize();
            ConfigLoaded=true;afterConfig();
        }
    }
}
