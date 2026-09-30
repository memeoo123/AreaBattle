using System;
namespace AreaBattle
{
    // One original GlobalItemManager4542 split into lifecycle and reward implementations.
    public sealed class OutgameGlobalItemInstance
    {
        public readonly OutgameGlobalItemLifecycle Lifecycle;
        public readonly OutgameGlobalItemRewards Rewards;
        internal OutgameGlobalItemInstance(OutgameGlobalItemLifecycle lifecycle,OutgameGlobalItemRewards rewards)
        {Lifecycle=lifecycle;Rewards=rewards;}
    }
    // Source34590: construct, publish, then initialize the current config, finally reread
    // the static slot. Config failure retains the published object, without an init retry.
    public sealed class OutgameGlobalItemSlot
    {
        OutgameGlobalItemInstance instance;
        readonly Func<IOutgameGlobalItemLifecycleHost,OutgameGlobalItemLifecycle> construct;
        readonly Func<OutgameItemConfigManager> config;
        readonly Func<OutgameLegacyConfigRead> reader;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<IOutgameItemReports> reports;
        readonly Action<string> error;
        readonly LifecycleHost host;
        public OutgameGlobalItemSlot(Func<IOutgameGlobalItemLifecycleHost,OutgameGlobalItemLifecycle> construct,
            Func<OutgameItemConfigManager> config,Func<OutgameLegacyConfigRead> reader,
            Func<OutgameMessageDispatcher> messages,Func<IOutgameItemReports> reports,Action<string> error,
            Func<Action,int> addUpdate,Action<int> removeUpdate)
        {
            this.construct=construct;this.config=config;this.reader=reader;this.messages=messages;this.reports=reports;this.error=error;
            host=new LifecycleHost(this,addUpdate,removeUpdate);
        }
        public bool HasInstance=>instance!=null;
        public OutgameGlobalItemInstance Instance
        {
            get
            {
                if(instance==null)
                {
                    var lifecycle=construct(host);
                    var rewards=new OutgameGlobalItemRewards(lifecycle,config,messages,reports,error);
                    instance=new OutgameGlobalItemInstance(lifecycle,rewards);
                    config().InitLegacyUnityJson(reader());
                }
                return instance;
            }
        }
        // Factories and entities always resolve current source slots, including after release.
        // Virtual/UI/report and package-manager services are supplied by their real owners.
        public void BindEntities(OutgameItemEntityServices services)
        {services.Config=config;services.Rewards=()=>Instance.Rewards;services.Messages=messages;services.Reports=reports;}
        public OutgameItemFactory CreateFactory(int id,OutgameItemEntityServices services)
        {return new OutgameItemFactory(id,config,error,services.Construct);}
        sealed class LifecycleHost:IOutgameGlobalItemLifecycleHost
        {
            readonly OutgameGlobalItemSlot slot;readonly Func<Action,int> add;readonly Action<int> remove;
            public LifecycleHost(OutgameGlobalItemSlot slot,Func<Action,int> add,Action<int> remove)
            {this.slot=slot;this.add=add;this.remove=remove;}
            public int AddUpdate(Action update)=>add(update);
            public void RemoveUpdate(int id)=>remove(id);
            public void DisposeItemConfiguration()=>slot.config().Dispose();
            public void ClearGlobalInstance()=>slot.instance=null;
            public void SendStatisticsRegistration(string message,int eventId,Func<object[],long> query)
            {slot.messages().SendMessage(message,new object[]{eventId,query});}
        }
    }
}
