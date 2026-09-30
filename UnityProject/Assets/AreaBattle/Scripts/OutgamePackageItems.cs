using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public sealed class OutgamePackageServices
    {
        public Func<OutgameItemManager> Manager;
        public Func<Random> NewChanceRandom=()=>new Random();
        public GameRandomSource Random=GameRandomSource.Shared;
        public Action<string> Error;
        // RandomHelper.GetRandomWeight26203 and accumulator26209.
        public OutgameRewardRangeData Select(List<OutgameRewardRangeData> rows)
        {
            if(rows==null){Error("权重随机集合不能为空");return null;}
            int total=0;foreach(var row in rows)total=unchecked(total+row.Weight);
            int roll=Random.Managed.Next(0,total),cumulative=0;
            for(int i=0;i<rows.Count;i++){cumulative=unchecked(cumulative+rows[i].Weight);if(roll<cumulative)return rows[i];}
            return rows[0];
        }
    }
    // PackageItemBase4489. Int32 outer counter is compared to the original Int64 count.
    public class OutgamePackageItemBase:OutgameItemBase
    {
        public OutgamePackageItemBase(int id,OutgameItemEntityServices services):base(id,services){}
        protected List<OutgameItemReward> Draw(long count,bool expandNested)
        {
            var result=new List<OutgameItemReward>();
            for(int i=0;(long)i<count;i=unchecked(i+1)){
                foreach(var pair in services.Config().GetPackageRewards(ItemConfig.paramInt)){
                    int chance=services.Packages.NewChanceRandom().Next(0,10000);
                    if(chance>=pair.Value.rewardRandom)continue;
                    var ranges=services.Config().GetRewardItemRanges(pair.Value.rewardId);
                    if(ranges.Count<1)continue;
                    for(int j=0;j<pair.Value.rewardItemCount;j=unchecked(j+1)){
                        var selected=services.Packages.Select(ranges);ranges.Remove(selected);
                        if(expandNested){
                            var config=services.Config().Items[selected.Config.itemId];
                            if(config.type1==9){
                                var item=services.Rewards().GetItem(config);
                                int quantity=services.Packages.Random.Inclusive(selected.Config.itemCountMin,selected.Config.itemCountMax);
                                if(item!=null)item.AddItem(quantity);
                                continue;
                            }
                        }
                        var reward=new OutgameItemReward{rewardOrder=0,itemId=selected.Config.itemId};
                        reward.itemCount=services.Packages.Random.Inclusive(selected.Config.itemCountMin,selected.Config.itemCountMax);
                        reward.rewardOrder=selected.Config.rewardOrder;result.Add(reward);
                    }
                }
            }
            return result;
        }
        public override List<OutgameItemReward> ItemToReward(long count)=>Draw(count,false);
        public override void AddItem(long count)=>base.AddItem(count);
        public override void AddItemOnlyModel(long count)=>base.AddItemOnlyModel(count);
        public override void Use(long count)
        {
            base.Use(count);var rewards=ItemToReward(count);
            for(int i=0;i<rewards.Count;i++)services.Packages.Manager().PackageOpenRewardsTemp.AddLast(rewards[i]);
            services.Rewards().AddRewardsByItemSelf(rewards);
        }
    }
    public class OutgameAutoPackageItem:OutgamePackageItemBase
    {
        public OutgameAutoPackageItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override List<OutgameItemReward> ItemToReward(long count)=>Draw(count,true);
        public override void AddItem(long count)=>Use(count);
        public override void AddItemOnlyModel(long count)=>Use(count);
    }
    public sealed class OutgameManualPackageItem:OutgamePackageItemBase
    {public OutgameManualPackageItem(int id,OutgameItemEntityServices services):base(id,services){}}
}
