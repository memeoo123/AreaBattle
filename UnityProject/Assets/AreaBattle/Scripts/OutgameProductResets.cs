using System;
namespace AreaBattle
{
    // GlobalItemManager.UpdateProductBuyLimit34595 and price/dirty helper34574.
    public sealed class OutgameProductResets
    {
        readonly OutgameGlobalItemIndexes indexes;
        readonly Func<int,OutgameProductRefreshConfig> config;
        readonly Action<OutgameProductUserData> updatePrice;
        readonly Action<string,object[]> send;
        readonly Action<object[]> warning;
        public OutgameProductResets(OutgameGlobalItemIndexes indexes,Func<int,OutgameProductRefreshConfig> config,Action<OutgameProductUserData> updatePrice,Action<string,object[]> send,Action<object[]> warning)
        {this.indexes=indexes;this.config=config;this.updatePrice=updatePrice;this.send=send;this.warning=warning;}
        void RefreshPrice(int uid){updatePrice(indexes.Products[uid]);indexes.IsDirty=true;}
        public void Reset(int uid)
        {
            if(!indexes.Products.ContainsKey(uid)){warning(new object[]{"配置为空",uid});return;}
            var row=indexes.Products[uid];
            switch(config(row.productId).buyLimit)
            {
                case 0:
                    row.haveBuyTimes=0;row.HaveBuyCounts.Clear();RefreshPrice(uid);break;
                case 1:
                    int count=config(row.productId).buyLimitParam;
                    row.haveBuyTimes=0;row.buyCount=count;row.HaveBuyCounts.Clear();RefreshPrice(uid);
                    indexes.SnapshotProduct(config(row.productId).id);break;
                case 2:return;
                case 3:case 4:case 5:
                    row.haveBuyTimes=0;row.HaveBuyCounts.Clear();RefreshPrice(uid);
                    row.buyCount=config(row.productId).buyLimitParam;
                    indexes.SnapshotProduct(config(row.productId).id);break;
            }
            send("ItemUI_RefreshStore",new object[]{uid});
            send("ItemUI_ProductReset",new object[]{uid,row.productId});
        }
    }
}
