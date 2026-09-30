using System;
namespace AreaBattle
{
    // DAUAdsAdapter.getRealPrice; storage is Utils_PlayerPrefs.GetInt, not account UserData.
    public sealed class OutgameAdPriceCache
    {
        readonly Func<string,int,int> readInt;
        readonly Func<DateTime> localNow;
        public string ZoneKey;
        public int PlatformId,ClearNextDayEnable,RealPrice;
        public OutgameAdPriceCache(Func<string,int,int> readInt,Func<DateTime> localNow)
        {this.readInt=readInt??throw new ArgumentNullException(nameof(readInt));this.localNow=localNow??throw new ArgumentNullException(nameof(localNow));}
        public int GetRealPrice()
        {
            if(RealPrice==0)
            {
                string key;
                if(ClearNextDayEnable==1)
                {
                    DateTime now=localNow();
                    key="realPrice"+ZoneKey+PlatformId.ToString()+now.ToString("yyyy-MM-dd");
                }
                else key="realPrice"+ZoneKey+PlatformId.ToString();
                RealPrice=readInt(key,0);
            }
            return RealPrice;
        }
    }
}
