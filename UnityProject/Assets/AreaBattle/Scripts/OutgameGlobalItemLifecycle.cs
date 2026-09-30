using System;
namespace AreaBattle
{
    public interface IOutgameGlobalItemLifecycleHost
    {
        int AddUpdate(Action update);
        void RemoveUpdate(int id);
        void DisposeItemConfiguration();
        void ClearGlobalInstance();
        void SendStatisticsRegistration(string message,int eventId,Func<object[],long> query);
    }
    // Original GlobalItemManager.Init/SynServerTime/OnRelease and statistics query.
    // The source Update/reward engine must supply update; this is not a substitute timer.
    public sealed class OutgameGlobalItemLifecycle
    {
        public const string StatisticsRegistration="CommonGameModule_StatisticsEventResiger";
        readonly IOutgameGlobalItemLifecycleHost host;
        readonly Action update;
        readonly Func<int,bool> hasProduct,hasItem;
        public readonly OutgameGlobalItemIndexes Indexes;
        public readonly OutgameGlobalItemClock Clock;
        public readonly OutgameGlobalItemPersistence Persistence;
        public int? UpdateHandle {get;private set;}
        public int StatisticsEventId=10020;
        public OutgameItemManagerData Data {get=>Persistence.Data;set=>Persistence.Data=value;}
        public OutgameGlobalItemLifecycle(IOutgameGlobalItemLifecycleHost host,Action update,Func<int,bool> hasProduct,Func<int,bool> hasItem,Func<DateTime> localNow)
        {
            this.host=host;this.update=update;this.hasProduct=hasProduct;this.hasItem=hasItem;
            Indexes=new OutgameGlobalItemIndexes();Clock=new OutgameGlobalItemClock(localNow);
            Persistence=new OutgameGlobalItemPersistence(Indexes,Clock);
        }
        public OutgameProductUpdates ProductUpdates {get;private set;}
        public OutgameGlobalItemLifecycle(IOutgameGlobalItemLifecycleHost host,Func<int,bool> hasProduct,Func<int,bool> hasItem,Func<DateTime> localNow,Func<float> deltaTime,Func<int,OutgameProductRefreshConfig> config,Action<int> reset,Action hostUpdate,Action<object[]> error)
            :this(host,null,hasProduct,hasItem,localNow)
        {
            ProductUpdates=new OutgameProductUpdates(this,config,reset,hostUpdate,error);
            update=()=>ProductUpdates.Update(deltaTime());
        }
        public IOutgameGlobalItemRewardHost RewardHost;
        public void InitializeWithHost(string text,IOutgameGlobalItemRewardHost rewardHost)
        {RewardHost=rewardHost;Initialize(text,rewardHost==null?null:(Func<DateTime>)rewardHost.GetNowDateTime);}
        public void Initialize(string text,Func<DateTime> hostNow)
        {
            Clock.HostNow=hostNow;
            Data=OutgameItemManagerData.ReadOriginal(text);
            SynchronizeServerTime(OutgameItemTimestamp.FromDateTime(Clock.GetNow()));
            Indexes.InitializeProducts(Data,hasProduct);
            Indexes.InitializeItems(Data,hasItem);
            Indexes.IsDirty=true;
            host.SendStatisticsRegistration(StatisticsRegistration,StatisticsEventId,QueryStatistics);
        }
        public void SynchronizeServerTime(long timestamp)
        {
            Data.saveTimestamp=timestamp;
            if(!UpdateHandle.HasValue)UpdateHandle=host.AddUpdate(update);
        }
        public long GetItemCount(int id)=>Indexes.Items.ContainsKey(id)?Indexes.Items[id].itemCount:0L;
        public long QueryStatistics(object[] arguments)
        {
            if(arguments==null||arguments.Length==0)return 0;
            return GetItemCount((int)arguments[0]);
        }
        public string Save()=>Persistence.Save();
        public void Release()
        {
            if(UpdateHandle.HasValue){host.RemoveUpdate(UpdateHandle.Value);UpdateHandle=null;}
            host.DisposeItemConfiguration();
            host.ClearGlobalInstance();
        }
    }
}
