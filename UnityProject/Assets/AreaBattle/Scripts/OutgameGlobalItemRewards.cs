using System;
using System.Collections.Generic;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    public interface IOutgameGlobalItemRewardHost
    {
        void Update();
        IOutgameItemEntity GetItem(GameItemConfig config);
        void AddRewards(List<OutgameItemReward> rewards);
        void ExpendReward(List<OutgameItemReward> rewards);
        DateTime GetNowDateTime();
    }
    // Only source-owned report fields are assigned here. The report host owns creation
    // and platform delivery, including any other fields populated by ReportManager.
    public sealed class OutgameItemChangeReport
    {public string Game;public int? Id,Type1,Type2,Amount,Balance;}
    public sealed class OutgameItemUseReport {public string Game;public int? Id,Type1,Type2;}
    public interface IOutgameItemReports
    {
        OutgameItemChangeReport CreateChange(bool gain);
        void SendChange(bool gain,OutgameItemChangeReport report);
        OutgameItemUseReport CreateUse();
        void SendUse(OutgameItemUseReport report);
    }
    // GlobalItemManager4542 reward methods and helpers34589/34581.
    public sealed class OutgameGlobalItemRewards
    {
        readonly OutgameGlobalItemLifecycle global;readonly Func<OutgameItemConfigManager> config;
        readonly Func<OutgameMessageDispatcher> messages;readonly Func<IOutgameItemReports> reports;readonly Action<string> error;
        public Func<GameItemConfig,IOutgameItemEntity> GetCommonItem;
        public Action<List<OutgameItemReward>,int> AddRewardsDel,AddRewardsModelDel;
        public Action<List<OutgameItemReward>> AddRewardsByItemSelfDel,ExpendRewardsDel;
        public Action<int,long,int> ReportDel;
        public OutgameGlobalItemRewards(OutgameGlobalItemLifecycle global,Func<OutgameItemConfigManager> config,Func<OutgameMessageDispatcher> messages,Func<IOutgameItemReports> reports,Action<string> error)
        {this.global=global;this.config=config;this.messages=messages;this.reports=reports;this.error=error;}
        public void AddRewards(List<OutgameItemReward> rows)
        {var custom=AddRewardsDel;if(custom!=null){custom(rows,0);return;}for(int i=0;i<rows.Count;i++)Change(rows[i].itemId,rows[i].itemCount);global.RewardHost.AddRewards(rows);}
        public void AddRewardsModel(List<OutgameItemReward> rows)
        {var custom=AddRewardsModelDel;if(custom!=null){custom(rows,0);return;}for(int i=0;i<rows.Count;i++)Change(rows[i].itemId,rows[i].itemCount);}
        public void ExpendReward(List<OutgameItemReward> rows)
        {var custom=ExpendRewardsDel;if(custom!=null){custom(rows);return;}for(int i=0;i<rows.Count;i++)Change(rows[i].itemId,unchecked(-rows[i].itemCount));global.RewardHost.ExpendReward(rows);}
        public IOutgameItemEntity GetItem(GameItemConfig row)
        {if(row==null)return null;var common=GetCommonItem;if(common!=null){var item=common(row);if(item!=null)return item;}return global.RewardHost.GetItem(row);}
        public void AddRewardsByItemSelf(List<OutgameItemReward> rows)
        {
            var custom=AddRewardsByItemSelfDel;if(custom!=null){custom(rows);return;}
            for(int i=0;i<rows.Count;i++){var item=GetItem(config().GetGameItemConfig(rows[i].itemId));if(item!=null)item.AddItemOnlyModel(rows[i].itemCount);}
            global.RewardHost.AddRewards(rows);
        }
        public void Change(int id,long delta)
        {
            var items=global.Indexes.Items;
            if(items.ContainsKey(id)){
                var row=items[id];row.itemCount=unchecked(row.itemCount+delta);
                messages().SendMessage("CommonGameModule_StatisticsEventSet",new object[]{global.StatisticsEventId,items[id].itemCount,id});
            }else{
                var row=new OutgameItemUserData{itemId=id,itemCount=delta};
                messages().SendMessage("CommonGameModule_StatisticsEventSet",new object[]{global.StatisticsEventId,row.itemCount,id});
                row.holdTime=0;items.Add(id,row);
            }
            // Source12037 / metadata34596 copies live items(field36) into item snapshots(field44).
            global.Indexes.SnapshotItem(id);
            messages().SendMessage("ItemUI_RefreshUserItem",new object[]{id});
            Report(id,delta,0);global.Indexes.IsDirty=true;
        }
        public void Report(int id,long delta,int reason)
        {
            messages().SendMessage("Item_ItemChange",new object[]{id,delta});
            var custom=ReportDel;if(custom!=null){custom(id,delta,reason);return;}
            var row=config().Items[id];
            if(row==null){error(string.Format("道具上报错误, 未找到道具id {0} , 请检查配置",id));return;}
            bool gain=delta>=1;var report=reports().CreateChange(gain);
            report.Id=row.id;report.Game=row.item_game;report.Type1=row.type1;report.Type2=row.type2;
            int value=gain?(delta>int.MaxValue?0:unchecked((int)delta)):(delta<int.MinValue?0:unchecked((int)delta));
            int sign=value>>31;report.Amount=unchecked((value+sign)^sign);
            long balance=global.Indexes.Items[id].itemCount;
            report.Balance=balance>int.MaxValue||balance<int.MinValue?0:(int)balance;
            reports().SendChange(gain,report);
        }
    }
}
