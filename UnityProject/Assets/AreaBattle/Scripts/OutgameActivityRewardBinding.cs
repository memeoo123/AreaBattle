namespace AreaBattle
{
    // Install before activities.Init so the original initialization registers GetCommonItem.
    // All activity families share the account's actual item configuration, entity services,
    // reward engine, inventory and ItemManager. Their own factories still handle virtual rewards.
    public static class OutgameActivityRewardBinding
    {
        public static void Bind(OutgameActivityRuntime activities,OutgameItemRuntime items,
            OutgameTaskRuntime tasks,OutgameAchievementRuntime achievements,OutgameLimitTimeTaskServices limited=null)
        {
            activities.Services.Items=()=>items.Global.Instance.Rewards;
            activities.ActivityServices.ItemConfig=()=>items.Config.Instance;
            if(tasks!=null){tasks.Services.Rewards=()=>items.Global.Instance.Rewards;tasks.Services.Entities=items.Entities;}
            if(achievements!=null){achievements.Services.Items=()=>items.Config.Instance;achievements.Services.Rewards=()=>items.Global.Instance.Rewards;achievements.Services.Entities=items.Entities;}
            if(limited!=null){limited.Rewards=()=>items.Global.Instance.Rewards;limited.Entities=items.Entities;}
        }
    }
}
