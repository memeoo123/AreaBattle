using System;
namespace AreaBattle
{
    // Controller4504 OnLateInit34397 and statistics callback34396 only.
    public sealed class OutgameFestActivityInitialization
    {
        readonly Func<bool> enabled;readonly OutgameActivityConfig config;readonly OutgameFestActManager manager;
        readonly OutgameFestActivityState state;readonly OutgameActivityCountdown countdown;readonly Action refreshIcons;
        readonly Action<int,Action> subscribe,unsubscribe;
        public bool ActiveUpdate {get;private set;}
        public OutgameFestActivityInitialization(Func<bool> enabled,OutgameActivityConfig config,OutgameFestActManager manager,
            OutgameFestActivityState state,OutgameActivityCountdown countdown,Action refreshIcons,Action<int,Action> subscribe,Action<int,Action> unsubscribe)
        {this.enabled=enabled;this.config=config;this.manager=manager;this.state=state;this.countdown=countdown;this.refreshIcons=refreshIcons;this.subscribe=subscribe;this.unsubscribe=unsubscribe;}
        public void OnLateInit()
        {
            if(!enabled())return;
            config.Apply(103002,state,countdown);manager.CheckInit(state.NoticeTime,state.EndTimeStamp);
            state.Refresh();refreshIcons();
            if(state.Status==5)return;
            ActiveUpdate=true;subscribe(10000,OnStatisticChanged);
        }
        public void OnStatisticChanged()
        {
            switch(state.Status)
            {
                case 2:case 3:case 4:refreshIcons();break;
                case 5:refreshIcons();unsubscribe(10000,OnStatisticChanged);break;
            }
            state.Refresh();
        }
        // Only this initializer's statistic subscription; full controller disposal also owns gameplay subscriptions.
        public void RemoveStatisticListener()=>unsubscribe(10000,OnStatisticChanged);
    }
}
