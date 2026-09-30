using System;
namespace AreaBattle
{
    // ServerTimeModule31055/31057/31061/31063/31064/31065/31066/31067.
    public sealed class OutgameServerClock
    {
        readonly Func<long> sdkTime;readonly Func<int> network;readonly Func<DateTime> localNow;
        readonly Func<bool> release;readonly Func<OutgameMessageDispatcher> messages;readonly Action clearOwner;
        bool initialized;long timestamp;float elapsed;int previousDay=-1,currentDay=-1;
        public bool ActiveUpdate {get;set;}
        public bool ConnectedNext {get;private set;}=true;
        public long DebugOffset;
        public OutgameServerClock(Func<long> sdkServerTime,Func<int> networkState,Func<bool> releaseVersion,
            Func<DateTime> localNow=null,Func<OutgameMessageDispatcher> messages=null,Action clearOwner=null)
        {sdkTime=sdkServerTime;network=networkState;release=releaseVersion;this.localNow=localNow??(()=>DateTime.Now);this.messages=messages??(()=>OutgameMessageDispatcher.Shared);this.clearOwner=clearOwner;}
        public void OnInit()
        {
            if(initialized)return;initialized=true;messages().AddListener("GamePause",OnPause);
            OnPause(new object[]{false});Refresh();
        }
        void OnPause(object[] args){if(args!=null&&args.Length!=0&&!(bool)args[0])Refresh();}
        void Refresh(){timestamp=sdkTime()>=1?sdkTime():OutgameItemTimestamp.FromDateTime(localNow());}
        public long GetNowTimestampLong(){long value=timestamp;if(!release())value=unchecked(value+DebugOffset);return value;}
        public int GetNowTimes()=>unchecked((int)(timestamp/1000));
        public DateTime GetNowDateTime()=>OutgameItemTimestamp.ToDateTime(timestamp);
        public int GetTodayOfYear()=>GetNowDateTime().DayOfYear;
        public int GetYear()=>GetNowDateTime().Year;
        public void Updata(float scaledElapsed,float realElapsed)
        {
            if(!initialized)return;elapsed+=realElapsed;if(!(elapsed>1f))return;elapsed-=1f;
            Refresh();ConnectedNext=network()==1;
            if(timestamp==0)timestamp=OutgameItemTimestamp.FromDateTime(localNow());
            currentDay=GetTodayOfYear();
            if(currentDay==previousDay)return;
            if(previousDay!=-1){messages().SendMessage("RefreshNetTime");messages().SendMessage("Time_NewDay");}
            previousDay=currentDay;
        }
        public void OnDispose(){ActiveUpdate=false;initialized=false;messages().RemoveListener("GamePause",OnPause);clearOwner?.Invoke();}
    }
}
