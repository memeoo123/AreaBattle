using System;
namespace AreaBattle
{
    public sealed class OutgamePreLoadApplicationServices
    {
        public Func<OutgameLegacyConfigManager> Config;
        public Action<string> LogProcedure,LogWarning;
        public Action InitializeUserNetModule,Handle103VersionBug,ClearStateEvents;
        public Func<bool> LoginSourceFlag64,LoginSourceFlag65;
    }
    // Concrete pre-load endpoints share the recovered statistics/activity owners.
    // Account flags, network initialization and legacy repair must come from the app.
    public sealed class OutgameCommonPreLoadHost:IOutgamePreLoadHost
    {
        readonly OutgamePreLoadApplicationServices application;
        readonly OutgameStatisticsRuntime statistics;
        readonly OutgameActivityRuntime activities;
        public OutgameCommonPreLoadHost(OutgamePreLoadApplicationServices application,OutgameStatisticsRuntime statistics,OutgameActivityRuntime activities)
        {this.application=application;this.statistics=statistics;this.activities=activities;}
        public int ArenaRankEvent=>application.Config().statisticEventConfig.ArenaRank;
        public bool LoginSourceFlag64=>application.LoginSourceFlag64();
        public bool LoginSourceFlag65=>application.LoginSourceFlag65();
        public void LogProcedure(string message)=>application.LogProcedure(message);
        public void LogWarning(string message)=>application.LogWarning(message);
        public void InitializeUserNetModule()=>application.InitializeUserNetModule();
        public void RegisterStatisticValue(int key,Func<object[],long> provider)=>statistics.Expansion.RegisteredValueFunc(key,provider);
        public long EventCount(int key)=>statistics.Expansion.EventCount(key);
        public void InitializeStatistics(Action complete)=>statistics.Initialize(complete);
        public void InitializeActivityWithNullData()=>activities.Init(null);
        public void Handle103VersionBug()=>application.Handle103VersionBug();
        public void ClearStateEvents()=>application.ClearStateEvents();
    }
}
