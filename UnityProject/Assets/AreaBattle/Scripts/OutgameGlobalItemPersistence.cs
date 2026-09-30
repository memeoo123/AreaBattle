using System;
namespace AreaBattle
{
    // TimeModule..cctor initializes an unspecified 1970-01-01 08:00:00.
    // TimeHelper subtracts DateTime ticks directly, without timezone conversion.
    public static class OutgameItemTimestamp
    {
        public static readonly DateTime Epoch=new DateTime(1970,1,1,8,0,0);
        public static long FromDateTime(DateTime value)=>(long)(value-Epoch).TotalMilliseconds;
        public static DateTime ToDateTime(long value)=>Epoch.AddMilliseconds(value);
    }
    // GlobalItemManager.GetNowDateTime: a present host is invoked twice, then reread.
    public sealed class OutgameGlobalItemClock
    {
        readonly Func<DateTime> localNow;
        public Func<DateTime> HostNow;
        public OutgameGlobalItemClock(Func<DateTime> localNow){this.localNow=localNow;}
        public DateTime GetNow()
        {if(HostNow!=null){HostNow();return HostNow();}return localNow();}
    }
    // GlobalItemManager.OnSave rebuilds original lists from live dictionaries before
    // reading time. Snapshots and dirty state are not altered by this method.
    public sealed class OutgameGlobalItemPersistence
    {
        readonly OutgameGlobalItemIndexes indexes;
        readonly OutgameGlobalItemClock clock;
        readonly Func<DateTime,long> toTimestamp;
        public OutgameItemManagerData Data;
        public OutgameGlobalItemPersistence(OutgameGlobalItemIndexes indexes,OutgameGlobalItemClock clock):this(indexes,clock,OutgameItemTimestamp.FromDateTime){}
        public OutgameGlobalItemPersistence(OutgameGlobalItemIndexes indexes,OutgameGlobalItemClock clock,Func<DateTime,long> toTimestamp)
        {this.indexes=indexes;this.clock=clock;this.toTimestamp=toTimestamp;}
        public string Save()
        {
            Data.productUserDatas.Clear();
            foreach(var pair in indexes.Products)Data.productUserDatas.Add(pair.Value);
            Data.itemUserDatas.Clear();
            foreach(var pair in indexes.Items)Data.itemUserDatas.Add(pair.Value);
            Data.saveTimestamp=toTimestamp(clock.GetNow());
            return Data.SerializeRecord();
        }
    }
}
