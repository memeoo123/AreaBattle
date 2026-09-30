using System;
using System.Collections.Generic;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameItemReward {public int itemId;public long itemCount;public int rewardOrder;}
    // ItemManager4500 delegates persistence to the original GlobalItemManager record.
    public sealed class OutgameItemManager:IOutgameDataManager,IOutgameGlobalItemRewardHost
    {
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;
        readonly Action<string> requestDownload;readonly Func<OutgameGlobalItemLifecycle> global;
        readonly Func<int,OutgameItemFactory> factory;readonly Func<OutgameToolControl> tool;
        readonly Func<int,int> goodsType;readonly Func<DateTime> now;
        public LinkedList<OutgameItemReward> PackageOpenRewardsTemp=new LinkedList<OutgameItemReward>();
        public string DataKey=>"ItemManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameItemManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> requestDownload,Func<OutgameGlobalItemLifecycle> global,Func<int,OutgameItemFactory> factory,Func<OutgameToolControl> tool,Func<int,int> goodsType,Func<DateTime> now)
        {this.storage=storage;this.host=host;this.requestDownload=requestDownload;this.global=global;this.factory=factory;this.tool=tool;this.goodsType=goodsType;this.now=now;}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {if(allowServer&&host.IsUseServer&&ParticipatesInSync){requestDownload(DataKey);return;}UpdateDataCallBack(storage.ReadLocalData());}
        public void UpdateDataCallBack(string text)=>global().InitializeWithHost(text,this);
        public void OnRelease()=>global().Release();
        public void OnSave()=>storage.SaveLocalData(global().Save());
        public void Update(){} //34338 source empty.
        public long GetItemNum(int id)=>global().GetItemCount(id);
        public IOutgameItemEntity GetItem(GameItemConfig config)=>factory(config.id).Produce();
        public DateTime GetNowDateTime()=>now(); //34343 DateTime.Now, not server clock.
        public void AddRewards(List<OutgameItemReward> rewards)
        {
            for(int i=0;i<rewards.Count;i++)if(goodsType(rewards[i].itemId)!=6)
                tool().ToolChange(rewards[i].itemId,unchecked((int)rewards[i].itemCount),false,"",false);
        }
        public void ExpendReward(List<OutgameItemReward> rewards){} //34341 source empty.
        public List<OutgameItemReward> GetPackageOpenReward()
        {var result=new List<OutgameItemReward>();result.AddRange(PackageOpenRewardsTemp);PackageOpenRewardsTemp.Clear();return result;}
    }
    //34333: resolve lazily through DataManagerPool; OnInit/OnRelease do not reset this slot.
    public sealed class OutgameItemManagerSlot
    {
        readonly Func<OutgameDataManagerPool> pool;OutgameItemManager instance;
        public OutgameItemManagerSlot(Func<OutgameDataManagerPool> pool){this.pool=pool;}
        public OutgameItemManager Instance=>instance??(instance=pool().GetModel<OutgameItemManager>(4500,"ItemManager"));
    }
}
