using System;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // ConfigMgr30439: all eight online reads precede the first mutation.
    public static class OutgameConfigOnlineOverrides
    {
        public static void Apply(FuncSettingConfig config,Func<string,string> getOnlineConfigParams)
        {
            string hero=getOnlineConfigParams("Proj_hdzd_HeroSetting");
            string boss=getOnlineConfigParams("Proj_hdzd_BossLevel");
            string preUnlock=getOnlineConfigParams("Proj_hdzd_HeroPreUnlockLevel");
            string valentine=getOnlineConfigParams("Proj_hdzd_ValentineSetting");
            string afterTool=getOnlineConfigParams("DontShowInsertAfterSeeToolVideo");
            string afterVideo=getOnlineConfigParams("DontShowInsertIfSeeOverVideo");
            string everyLevel=getOnlineConfigParams("ShowInsertADPerXLevel");
            getOnlineConfigParams("NoRemoveAds");
            if(!string.IsNullOrEmpty(boss))config.BossLevel=int.Parse(boss);
            if(!string.IsNullOrEmpty(hero))config.HeroSetting=bool.Parse(hero);
            if(!string.IsNullOrEmpty(preUnlock))config.HeroPreUnlockLevel=int.Parse(preUnlock);
            if(!string.IsNullOrEmpty(valentine))config.ValentineSetting=int.Parse(valentine);
            if(!string.IsNullOrEmpty(afterTool))config.dontShowInsertAfterSeeToolVideo=int.Parse(afterTool);
            if(!string.IsNullOrEmpty(afterVideo))config.dontShowInsertIfSeeOverVideo=int.Parse(afterVideo);
            if(!string.IsNullOrEmpty(everyLevel))config.ShowInsertADPerXLevel=int.Parse(everyLevel);
        }
    }
}
