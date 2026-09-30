using System;
namespace AreaBattle
{
    public enum OutgameAdAdapterKind
    {WxBanner,WxGridBanner,WxCpsBanner,WxInters,CpsInters,ApiInters,WxVideo,WxNative}
    // ADModule.createAdAdapter. Construction is supplied for each recovered adapter kind.
    public static class OutgameAdAdapterFactory
    {
        public static T Create<T>(string zoneKey,int platformId,bool firstDayNoAd,
            Func<OutgameAdAdapterKind,T> construct) where T:class
        {
            if(firstDayNoAd)return null;
            int platform=platformId<10001?platformId:platformId/100;
            if(zoneKey=="BANNER")
            {
                if(platform==424)return construct(OutgameAdAdapterKind.WxCpsBanner);
                if(platform==484)return construct(OutgameAdAdapterKind.WxGridBanner);
                if(platform==883)return construct(OutgameAdAdapterKind.WxBanner);
            }
            if(zoneKey.Contains("INTERSTITAL"))
            {
                if(platform==424)return construct(OutgameAdAdapterKind.CpsInters);
                if(platform==425)return construct(OutgameAdAdapterKind.ApiInters);
                if(platform==883)return construct(OutgameAdAdapterKind.WxInters);
            }
            if(zoneKey=="VIDEO"&&platform==883)return construct(OutgameAdAdapterKind.WxVideo);
            if(platform==883&&(zoneKey=="NATIVE"||zoneKey=="NATIVE_BIG"||zoneKey=="NATIVE_START"||zoneKey=="NATIVE_OVER"||zoneKey=="NATIVE_SPLASH"))
                return construct(OutgameAdAdapterKind.WxNative);
            return null;
        }
    }
}
