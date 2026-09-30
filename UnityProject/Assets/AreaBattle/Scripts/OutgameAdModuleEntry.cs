using System;
namespace AreaBattle
{
    // WXFunctionManger cached ADModule lookup and CheckFunctionActive.
    public sealed class OutgameAdModuleEntry
    {
        readonly Func<OutgameRewardVideo> getModule;
        readonly Action<string> warn;
        OutgameRewardVideo module;
        public OutgameAdModuleEntry(Func<OutgameRewardVideo> getModule,Action<string> warn)
        {this.getModule=getModule??throw new ArgumentNullException(nameof(getModule));this.warn=warn??throw new ArgumentNullException(nameof(warn));}
        OutgameRewardVideo Module(){if(module==null)module=getModule();return module;}
        bool CheckActive(OutgameRewardVideo value)
        {if(value.Active)return true;warn("功能未开启请不要使用!");return value.Active;}
        // Factory entry has no CheckFunctionActive gate in the source manager.
        public T CreateAdAdapter<T>(string zoneKey,int platformId,Func<OutgameAdAdapterKind,T> construct) where T:class
        {return OutgameAdAdapterFactory.Create(zoneKey,platformId,Module().FirstDayNoAd,construct);}
        public void ShowRewardAd(Action<bool> callback)
        {var value=Module();if(CheckActive(value))module.Show(callback);}
        public bool IsRewardAdReady()
        {var value=Module();if(CheckActive(value))return module.Ready;warn("WXFunctionManger AdsIsReady false");return false;}
    }
}
