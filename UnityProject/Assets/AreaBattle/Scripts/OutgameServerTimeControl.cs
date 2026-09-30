using System;
using System.Globalization;
namespace AreaBattle
{
    // ServerTimeModule31054..31067. SDK time/network remain explicit providers.
    public sealed class OutgameServerTimeControl:IOutgameLogicControl
    {
        readonly Func<long> serverMilliseconds;
        readonly Func<int> networkingState;
        readonly Func<DateTime> localNow;
        readonly Func<bool> isReleaseVersion;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly OutgameControllerRegistry registry;
        public bool ActiveUpdate {get;set;}
        public bool Initialized {get;private set;}
        public long Timestamp {get;private set;}
        public long DebugOffsetMilliseconds;
        public float Elapsed {get;private set;}
        public bool ConnectedNext {get;private set;}=true;
        public int PreviousDay {get;private set;}=-1;
        public int CurrentDay {get;private set;}=-1;
        public OutgameServerTimeControl(Func<long> serverMilliseconds,Func<int> networkingState,Func<DateTime> localNow,Func<bool> isReleaseVersion,Func<OutgameMessageDispatcher> messages,OutgameControllerRegistry registry)
        {this.serverMilliseconds=serverMilliseconds;this.networkingState=networkingState;this.localNow=localNow;this.isReleaseVersion=isReleaseVersion;this.messages=messages;this.registry=registry;}
        static long ToTimestamp(DateTime time)=>(long)(time-new DateTime(1970,1,1,8,0,0)).TotalMilliseconds;
        void Refresh()
        {
            // Source calls SDK getter twice on success; do not cache the first sample.
            Timestamp=serverMilliseconds()>=1?serverMilliseconds():ToTimestamp(localNow());
        }
        void OnPause(object[] args)
        {
            if(args!=null&&args.Length!=0&&!(bool)args[0])Refresh();
        }
        public void OnInit()
        {
            if(Initialized)return;
            Initialized=true;messages().AddListener("GamePause",OnPause);
            OnPause(new object[]{false});Refresh();
        }
        public void Updata(float deltaTime,float unscaledDeltaTime)
        {
            if(!Initialized)return;
            Elapsed+=unscaledDeltaTime;
            if(!(Elapsed>1))return;
            Elapsed-=1;Refresh();ConnectedNext=networkingState()==1;
            if(Timestamp==0)Timestamp=ToTimestamp(localNow());
            CurrentDay=GetTodayOfYear();
            if(CurrentDay==PreviousDay)return;
            if(PreviousDay!= -1)
            {
                messages().SendMessage("RefreshNetTime");messages().SendMessage("Time_NewDay");
            }
            PreviousDay=CurrentDay;
        }
        public void OnDispose()
        {
            ActiveUpdate=false;Initialized=false;
            messages().RemoveListener("GamePause",OnPause);registry.Clear(4034);
        }
        public long GetNowTimestampLong()=>isReleaseVersion()?Timestamp:unchecked(Timestamp+DebugOffsetMilliseconds);
        public int GetNowTimes()=>unchecked((int)(Timestamp/1000));
        public DateTime GetNowDateTime()=>OutgameLocalLimitedBags.ToSourceDateTime(Timestamp);
        public int GetTodayOfYear()=>GetNowDateTime().DayOfYear;
        public int GetYear()=>GetNowDateTime().Year;
        public int GetWeekOfYear()=>CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(GetNowDateTime(),CalendarWeekRule.FirstDay,DayOfWeek.Monday);
    }
}
