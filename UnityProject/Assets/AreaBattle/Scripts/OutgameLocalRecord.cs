using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameFirstChargeRecord {public bool hasCharge;public byte rewardState;public long firstChargeTime;}
    [Serializable] public sealed class OutgameJumpRecord {public int defeatNum,jumpNum,jumpDayOfYear;}
    [Serializable] public sealed class OutgameLimitedBagRecord {public bool isActive;public int id,ItemBagId;public long LimitedTime;}
    // All serialized LocalData fields from original metadata. dailyChallengeData is NonSerialized.
    [Serializable] public sealed class OutgameLocalRecord
    {
        public bool buyRemoveAd;
        public byte dealOldComm;
        public int LevelID,defeatState,goldNum,diamondsNum,strengthsNum,ToolValue,collectNum,collectAdNum;
        public OutgameFirstChargeRecord firstChargeData=new OutgameFirstChargeRecord();
        public OutgameJumpRecord jumpData=new OutgameJumpRecord();
        public List<OutgameToolCount> toolCounts=new List<OutgameToolCount>();
        public List<OutgameLimitedBagRecord> limitedBagInfos=new List<OutgameLimitedBagRecord>();
        public List<string> PurchasedJewelKey=new List<string>();
        public int season_pass=1,groupLevelIndex,SpeedUpPerDay;
        public static OutgameLocalRecord Read(string text)
        {
            var data=new OutgameLocalRecord();
            if(!string.IsNullOrEmpty(text)&&text.Trim()!="null")JsonUtility.FromJsonOverwrite(text.Replace("bank","collect"),data);
            return data;
        }
        // Existing progression and inventory views operate on this shared profile projection.
        public void CaptureProfile(OutgameProfile profile)
        {
            LevelID=profile.levelID;var inventory=profile.inventory;
            goldNum=inventory.goldNum;diamondsNum=inventory.diamondsNum;strengthsNum=inventory.strengthsNum;
            ToolValue=inventory.ToolValue;collectNum=inventory.collectNum;collectAdNum=inventory.collectAdNum;toolCounts=inventory.toolCounts;
        }
        public string ToOriginalJson()=>JsonUtility.ToJson(this);
    }
    public sealed class OutgameLocalLimitedBags
    {
        [Serializable] sealed class Rows {public Product[] Datas;}
        [Serializable] sealed class Product {public int id,isActive;}
        readonly Dictionary<int,Product> products=new Dictionary<int,Product>();
        readonly Func<DateTime> now;
        public Dictionary<int,OutgameLimitedBagRecord> Held {get;private set;}=new Dictionary<int,OutgameLimitedBagRecord>();
        public OutgameLocalLimitedBags(string productConfig,Func<DateTime> now)
        {this.now=now??throw new ArgumentNullException(nameof(now));foreach(var row in JsonUtility.FromJson<Rows>(productConfig).Datas)products.Add(row.id,row);}
        // TimeModule.defaultTime is explicitly 1970-01-01 08:00:00 (Unspecified), not UTC epoch.
        public static DateTime ToSourceDateTime(long milliseconds)=>new DateTime(1970,1,1,8,0,0).AddMilliseconds((double)milliseconds);
        public void Initialize(OutgameLocalRecord data)
        {
            Held=new Dictionary<int,OutgameLimitedBagRecord>();
            foreach(var bag in data.limitedBagInfos)
            {
                if(!products.TryGetValue(bag.ItemBagId,out var product)||product.isActive!=1)continue;
                var expiry=ToSourceDateTime(bag.LimitedTime);
                if(bag.LimitedTime==0||expiry.CompareTo(now())<1)continue;
                Held.Add(bag.id,bag);
            }
        }
        public void PrepareSave(OutgameLocalRecord data)
        {data.limitedBagInfos.Clear();foreach(var bag in Held.Values)data.limitedBagInfos.Add(bag);}
    }
}
