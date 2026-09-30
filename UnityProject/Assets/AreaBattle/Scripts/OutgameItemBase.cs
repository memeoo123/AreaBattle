using System;
using System.Collections.Generic;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    // ItemBase4541, VirtualItemBase4492, VirtualItem_Gold4496/Diamond4494.
    public class OutgameItemBase:IOutgameItemEntity
    {
        public GameItemConfig ItemConfig;
        protected readonly OutgameItemEntityServices services;
        public OutgameItemBase(int id,OutgameItemEntityServices services){this.services=services;ItemConfig=services.Config().Items[id];}
        public virtual List<OutgameItemReward> ItemToReward(long count)
        {var rows=new List<OutgameItemReward>();if(ItemConfig!=null)rows.Add(new OutgameItemReward{itemId=ItemConfig.id,itemCount=count,rewardOrder=0});return rows;}
        public virtual void AddItem(long count){var global=services.Rewards();global.AddRewards(ItemToReward(count));}
        public virtual void AddItemOnlyModel(long count){var global=services.Rewards();global.AddRewardsModel(ItemToReward(count));}
        public virtual void Use(long count)
        {
            services.Messages().SendMessage("Item_Use",new object[]{ItemConfig.id,count});
            var report=services.Reports().CreateUse();report.Id=ItemConfig.id;report.Game=ItemConfig.item_game;report.Type1=ItemConfig.type1;report.Type2=ItemConfig.type2;
            services.Reports().SendUse(report);
        }
    }
    public class OutgameVirtualItemBase:OutgameItemBase
    {public OutgameVirtualItemBase(int id,OutgameItemEntityServices services):base(id,services){}public override void AddItem(long count)=>base.AddItem(count);public override void Use(long count)=>base.Use(count);}
    public sealed class OutgameGoldItem:OutgameVirtualItemBase {public OutgameGoldItem(int id,OutgameItemEntityServices services):base(id,services){}}
    public sealed class OutgameDiamondItem:OutgameVirtualItemBase {public OutgameDiamondItem(int id,OutgameItemEntityServices services):base(id,services){}}
    public sealed class OutgameItemEntityServices
    {
        public Func<OutgameItemConfigManager> Config;
        public Func<OutgameGlobalItemRewards> Rewards;
        public Func<OutgameMessageDispatcher> Messages;
        public Func<IOutgameItemReports> Reports;
        public OutgameVirtualItemServices Virtual;
        public OutgamePackageServices Packages;
        public IOutgameItemEntity Construct(int sourceType,int id)
        {switch(sourceType){case 4496:return new OutgameGoldItem(id,this);case 4494:return new OutgameDiamondItem(id,this);case 4493:return new OutgameCommanderItem(id,this);case 4495:return new OutgameGamePointsItem(id,this);case 4497:return new OutgameHeadBoxItem(id,this);case 4498:return new OutgameHeroPieceItem(id,this);case 4499:return new OutgameStrengthItem(id,this);case 4490:return new OutgameAutoPackageItem(id,this);case 4491:return new OutgameManualPackageItem(id,this);default:throw new ArgumentOutOfRangeException(nameof(sourceType));}}
    }
}
