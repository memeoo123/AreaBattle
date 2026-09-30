using System;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameProductRefreshConfig
    {public int id,buyLimit,buyLimitParam;public int[] refreshPeriod;}
    // GlobalItemManager.Update and source helpers34582/34585/34569.
    public sealed class OutgameProductUpdates
    {
        readonly OutgameGlobalItemLifecycle manager;
        readonly Func<int,OutgameProductRefreshConfig> config;
        readonly Action<int> reset;
        readonly Action hostUpdate;
        readonly Action<object[]> error;
        public float AccumulatedTime {get;private set;}
        public OutgameProductUpdates(OutgameGlobalItemLifecycle manager,Func<int,OutgameProductRefreshConfig> config,Action<int> reset,Action hostUpdate,Action<object[]> error)
        {this.manager=manager;this.config=config;this.reset=reset;this.hostUpdate=hostUpdate;this.error=error;}
        long NowTimestamp()=>OutgameItemTimestamp.FromDateTime(manager.Clock.GetNow());
        public void Update(float deltaTime)
        {
            AccumulatedTime+=deltaTime;
            if(AccumulatedTime>1f)
            {
                AccumulatedTime-=1f;
                int lastDay=manager.Data.lastExitDayOfYear;
                bool sameDay=manager.Clock.GetNow().DayOfYear==lastDay;
                if(!sameDay)manager.Data.lastExitDayOfYear=manager.Clock.GetNow().DayOfYear;
                foreach(var row in manager.Indexes.Products.Values)
                {
                    var entry=config(row.productId);if(entry==null)continue;
                    int limit=entry.buyLimit;
                    if(!sameDay&&limit==1)reset(row.UID);
                    switch(limit)
                    {
                        case 3:UpdateInterval(row);break;
                        case 4:UpdateHours(row);break;
                        case 5:UpdateWeekdays(row);break;
                    }
                }
            }
            hostUpdate();
        }
        public void UpdateInterval(OutgameProductUserData row)
        {
            var entry=config(row.productId);if(entry==null)return;
            if(entry.refreshPeriod[0]==-1)return;
            if(entry.refreshPeriod==null||entry.refreshPeriod.Length==0){error(new object[]{"配置中刷新参数refreshParams为空",entry.id});return;}
            if(row.nextRefreshTimeStamp==0)
            {
                row.nextRefreshTimeStamp=unchecked(NowTimestamp()+(long)unchecked(entry.refreshPeriod[0]*1000));reset(row.UID);return;
            }
            int period=unchecked(entry.refreshPeriod[0]*1000);
            long elapsed=unchecked(NowTimestamp()-row.nextRefreshTimeStamp);
            long now=NowTimestamp(),next=row.nextRefreshTimeStamp;
            if(now<=next)return;
            row.nextRefreshTimeStamp=unchecked(next+period*(elapsed/period+1));reset(row.UID);
        }
        public void UpdateHours(OutgameProductUserData row)
        {
            var entry=config(row.productId);if(entry==null)return;
            if(entry.refreshPeriod[0]==-1)return;
            DateTime now=manager.Clock.GetNow();
            if(OutgameItemTimestamp.FromDateTime(now)<=row.nextRefreshTimeStamp)return;
            if(entry.refreshPeriod.Length==0){error(new object[]{"刷新参数为空",entry.id});return;}
            if(OutgameItemTimestamp.FromDateTime(now)<=row.nextRefreshTimeStamp)return;
            int i=0;for(;i<entry.refreshPeriod.Length;i++)if(now.Hour<entry.refreshPeriod[i])break;
            DateTime next=i<entry.refreshPeriod.Length
                ?new DateTime(now.Year,now.Month,now.Day).AddHours(entry.refreshPeriod[i])
                :new DateTime(now.Year,now.Month,now.Day+1).AddHours(entry.refreshPeriod[0]);
            row.nextRefreshTimeStamp=OutgameItemTimestamp.FromDateTime(next);reset(row.UID);
        }
        public void UpdateWeekdays(OutgameProductUserData row)
        {
            var entry=config(row.productId);if(entry==null)return;
            if(entry.refreshPeriod[0]==-1)return;
            if(NowTimestamp()<=row.nextRefreshTimeStamp)return;
            DateTime now=manager.Clock.GetNow();int day=(int)now.DayOfWeek;
            if(entry.refreshPeriod.Length==0){error(new object[]{"刷新参数为空,商品配置Id:"+row.productId});return;}
            int i=0;for(;i<entry.refreshPeriod.Length;i++)if(day<entry.refreshPeriod[i])break;
            int days=i<entry.refreshPeriod.Length?entry.refreshPeriod[i]-day:entry.refreshPeriod[0]+7-day;
            row.nextRefreshTimeStamp=OutgameItemTimestamp.FromDateTime(new DateTime(now.Year,now.Month,now.Day).AddDays(days));reset(row.UID);
        }
    }
}
