using System;
using System.Collections.Generic;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    // ItemConfigMgr4534. Explicit source-default ConfigRead Unity JSON route.
    // MemoryPack and the alternate JSON parser are separate, unbound acquisition routes.
    public sealed class OutgameItemConfigManager
    {
        public Dictionary<object,GameItemConfig> Items=new Dictionary<object,GameItemConfig>();
        public Dictionary<object,GamePackageConfig> Packages=new Dictionary<object,GamePackageConfig>();
        public Dictionary<object,GameRewardConfig> Rewards=new Dictionary<object,GameRewardConfig>();
        public Dictionary<object,GameProductConfig> Products=new Dictionary<object,GameProductConfig>();
        public Dictionary<int,Dictionary<int,GamePackageConfig>> PackageRewards;
        public Dictionary<int,Dictionary<int,GameRewardConfig>> RewardItems;
        public Dictionary<int,List<OutgameRewardRangeData>> RewardRanges=new Dictionary<int,List<OutgameRewardRangeData>>();
        public Dictionary<int,List<GameProductConfig>> GroupProducts=new Dictionary<int,List<GameProductConfig>>();
        readonly Func<OutgameItemConfigManager> current;readonly Action clear;readonly Action<object[]> error;
        public OutgameItemConfigManager(Func<OutgameItemConfigManager> current,Action clear,Action<object[]> error)
        {this.current=current;this.clear=clear;this.error=error;}
        public void InitLegacyUnityJson(OutgameLegacyConfigRead reader)
        {reader.ReadTable(Items);reader.ReadTable(Packages);reader.ReadTable(Rewards);reader.ReadTable(Products);BuildGroups();}
        public void BuildGroups()
        {
            PackageRewards=new Dictionary<int,Dictionary<int,GamePackageConfig>>();
            foreach(var pair in Packages){if(!PackageRewards.ContainsKey(pair.Value.packageId))PackageRewards[pair.Value.packageId]=new Dictionary<int,GamePackageConfig>();PackageRewards[pair.Value.packageId][pair.Value.packageRewardId]=pair.Value;}
            RewardItems=new Dictionary<int,Dictionary<int,GameRewardConfig>>();
            foreach(var pair in Rewards){if(!RewardItems.ContainsKey(pair.Value.rewardId))RewardItems[pair.Value.rewardId]=new Dictionary<int,GameRewardConfig>();RewardItems[pair.Value.rewardId][pair.Value.rewardItemId]=pair.Value;}
            foreach(var row in Products.Values){if(!GroupProducts.ContainsKey(row.groupID))GroupProducts.Add(row.groupID,new List<GameProductConfig>());GroupProducts[row.groupID].Add(row);}
        }
        public Dictionary<int,GamePackageConfig> GetPackageRewards(int id)=>PackageRewards.ContainsKey(id)?PackageRewards[id]:new Dictionary<int,GamePackageConfig>();
        public Dictionary<int,GameRewardConfig> GetRewardItems(int id)=>RewardItems.ContainsKey(id)?RewardItems[id]:new Dictionary<int,GameRewardConfig>();
        public GameItemConfig GetGameItemConfig(int id)
        {if(Items.TryGetValue(id,out var row))return row;error(new object[]{"道具表配置中不包含id",id});return null;}
        public GameProductConfig GetGameProductConfig(int id)
        {if(current().Products.TryGetValue(id,out var row))return row;error(new object[]{"商品表配置中不包含id",id});return null;}
        public List<OutgameRewardRangeData> GetRewardItemRanges(int id)
        {
            if(!RewardRanges.ContainsKey(id)){
                var ranges=new List<OutgameRewardRangeData>();
                foreach(var pair in GetRewardItems(id)){var range=new OutgameRewardRangeData();range.Config=pair.Value;range.Weight=range.Config.weight;ranges.Add(range);}
                RewardRanges[id]=ranges;
            }
            var cached=RewardRanges[id];var result=new List<OutgameRewardRangeData>();
            for(int i=0;i<cached.Count;i++)result.Add(cached[i]);return result;
        }
        public void Dispose(){RewardRanges.Clear();GroupProducts.Clear();clear();}
    }
    // RewardRangeData4550 inherits RandomObject3392.Weight at offset8.
    public sealed class OutgameRewardRangeData {public int Weight;public GameRewardConfig Config;}
    // get_Instance34554 publishes a new, empty manager; initialization is a separate source call.
    public sealed class OutgameItemConfigSlot
    {
        OutgameItemConfigManager instance;readonly Action<object[]> error;
        public OutgameItemConfigSlot(Action<object[]> error){this.error=error;}
        public bool HasInstance=>instance!=null;
        public OutgameItemConfigManager Instance=>instance??(instance=new OutgameItemConfigManager(()=>Instance,()=>instance=null,error));
    }
}
