using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // GlobalItemManager initialization34586/34587 and snapshot updates34583/34596.
    public sealed class OutgameGlobalItemIndexes
    {
        public readonly Dictionary<int,OutgameItemUserData> Items=new Dictionary<int,OutgameItemUserData>();
        public readonly Dictionary<int,OutgameProductUserData> Products=new Dictionary<int,OutgameProductUserData>();
        // Source ctor34570 creates only the live dictionaries. Init creates snapshots.
        public Dictionary<int,OutgameItemUserData> ItemSnapshot {get;private set;}
        public Dictionary<int,OutgameProductUserData> ProductSnapshot {get;private set;}
        public bool IsDirty;
        public void InitializeProducts(OutgameItemManagerData data,Func<int,bool> configContains)
        {
            Products.Clear();ProductSnapshot=new Dictionary<int,OutgameProductUserData>();
            foreach(var row in data.productUserDatas)
            {
                if(!configContains(row.productId))continue;
                Products.Add(row.UID,row);SnapshotProduct(row.UID);
            }
        }
        public void InitializeItems(OutgameItemManagerData data,Func<int,bool> configContains)
        {
            Items.Clear();ItemSnapshot=new Dictionary<int,OutgameItemUserData>();
            foreach(var row in data.itemUserDatas)
            {
                if(!configContains(row.itemId))continue;
                Items.Add(row.itemId,row);SnapshotItem(row.itemId);
            }
        }
        public void SnapshotItem(int id)
        {
            if(Items.ContainsKey(id))
            {
                if(ItemSnapshot.ContainsKey(id))
                {ItemSnapshot[id].holdTime=Items[id].holdTime;ItemSnapshot[id].itemCount=Items[id].itemCount;}
                else ItemSnapshot.Add(id,Items[id].Clone());
            }
            IsDirty=true;
        }
        public void SnapshotProduct(int uid)
        {
            if(Products.ContainsKey(uid))
            {
                if(ProductSnapshot.ContainsKey(uid))
                {
                    if(Products.ContainsKey(uid))
                    {
                        ProductSnapshot[uid].buyCount=Products[uid].buyCount;
                        // Original stores haveBuyTimes back onto the live record, not its snapshot.
                        Products[uid].haveBuyTimes=Products[uid].haveBuyTimes;
                        ProductSnapshot[uid].HaveBuyCounts=Products[uid].HaveBuyCounts;
                        ProductSnapshot[uid].lastResetTime=Products[uid].lastResetTime;
                        ProductSnapshot[uid].consumeItemPriceArray=Products[uid].consumeItemPriceArray;
                    }
                    else Products.Remove(uid);
                }
                else ProductSnapshot.Add(uid,Products[uid].Clone());
            }
            IsDirty=true;
        }
    }
}
