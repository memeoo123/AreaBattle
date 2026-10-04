using System;
namespace AreaBattle
{
    public sealed class OutgameSevendayActivityServices
    {
        public Func<OutgameActivityControl> Activities;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<OutgameSkinCatalog> Skins;
        public Func<int> CurrentLevel;
        public Func<OutgameCommanderControl> Commanders;
        public OutgameStatisticsExpansion Statistics;
    }
    // Source4502: lifecycle initialization/update are empty; scene completion calls
    // EnterGameInit separately. Cached dates and the child reference survive Dispose.
    public sealed class OutgameSevendayActivityControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;
        readonly OutgameSevendayActivityServices services;
        public OutgameChildLimitTimeTaskActivity Child;
        long startDate,endDate;
        public OutgameSevendayActivityControl(OutgameControllerRegistry registry,OutgameSevendayActivityServices services)
        {this.registry=registry;this.services=services;}
        public void OnInit(){}
        public void Updata(float deltaTime,float unscaledDeltaTime){}
        public void OnDispose()=>registry.Clear(4502);
        void ReadDates()
        {
            // Condition is a value-type array: source reads the low Int32 of the
            // first condition's Int64 target, then multiplies in Int32 before widening.
            int days=unchecked((int)Child.Data.OverCondition[0].value-1);
            var date=OutgameItemTimestamp.ToDateTime(Child.Data.LaunchTimeStamp);
            long start=OutgameItemTimestamp.FromDateTime(new DateTime(date.Year,date.Month,date.Day));
            startDate=start;endDate=unchecked(start+(long)unchecked(days*86400000));
        }
        public void EnterGameInit()
        {
            var parent=services.Activities().GetActivity<OutgameLimitTimeTaskActivity>(1301001);
            Child=parent==null?null:parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            if(Child==null)return;
            ReadDates();
            int key=services.Config().statisticEventConfig.HaveSkinNum;
            int count=services.Skins().UnlockedSoldierCount();services.Statistics.SetEventCount(key,count);
            int cleared=unchecked(services.CurrentLevel()-1);services.Statistics.SetEventCount(10015,cleared>0?cleared:0);
            key=services.Config().statisticEventConfig.CommanderUpgradeTotal;
            count=services.Commanders().GetCommanderTotalLv();services.Statistics.SetEventCount(key,count);
        }
        public bool IsInActivity()
        {
            if(Child!=null&&startDate==0)ReadDates();
            long now=services.Statistics.GameValue(10000,Array.Empty<object>());
            return now>startDate&&now<endDate;
        }
        public bool IsUnlock()=>Child!=null&&IsInActivity();
        // Both red queries run when unlocked; the source uses bitwise OR.
        public bool IsHaveAnyRed()=>IsUnlock()&&(IsAnyDayHaveRed()|IsAccHaveRed());
        public bool IsDayHaveRed(int day)=>Child.ExitRewardWaitGet(day);
        public bool IsAnyDayHaveRed()
        {
            int days=unchecked((int)Child.Data.OverCondition[0].value-1);
            for(int i=0;i<days;i++)if(IsDayHaveRed(i+1))return true;
            return false;
        }
        public bool IsAccHaveRed()
        {
            var rewards=Child.AccRewards;
            for(int i=0;i<rewards.Count;i++)if(Child.ModuleData.ext.AccProgress>=rewards[i].Config.accValue&&rewards[i].state==0)return true;
            return false;
        }
        public long GetActDate(bool start)=>start?startDate:endDate;
    }
}
