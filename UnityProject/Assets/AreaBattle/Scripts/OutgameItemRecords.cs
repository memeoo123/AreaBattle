using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameItemUserData
    {public int itemId;public long itemCount;public int holdTime;public OutgameItemUserData Clone()=>(OutgameItemUserData)MemberwiseClone();}
    [Serializable] public sealed class OutgameItemGetTypeBuyCount
    {public int key,value;}
    [Serializable] public sealed class OutgameProductBuyCount
    {public int key;public List<OutgameItemGetTypeBuyCount> buyCount=new List<OutgameItemGetTypeBuyCount>();}
    [Serializable] public sealed class OutgameProductUserData
    {
        public int m_uniqueId,productId,haveBuyTimes,buyCount;
        public List<OutgameProductBuyCount> HaveBuyCounts=new List<OutgameProductBuyCount>();
        public int lastResetTime;
        public long nextRefreshTimeStamp,consumeItemPrice;
        public long[] consumeItemPriceArray;
        public int UID {get{if(m_uniqueId==0)m_uniqueId=GetHashCode();return m_uniqueId;}}
        public OutgameProductUserData Clone()=>(OutgameProductUserData)MemberwiseClone();
    }
    // GlobalItemManager's source record. LocalData inventory uses separate Int32 rules.
    // Global dictionary rebuilding, timers and reward side effects are separate consumers.
    [Serializable] public sealed class OutgameItemManagerData
    {
        public long saveTimestamp;
        public int lastExitDayOfYear;
        public List<OutgameItemUserData> itemUserDatas=new List<OutgameItemUserData>();
        public List<OutgameProductUserData> productUserDatas=new List<OutgameProductUserData>();
        public static OutgameItemManagerData ReadOriginal(string text)
        {
            var data=JsonUtility.FromJson<OutgameItemManagerData>(text);
            if(data==null)
            {
                data=new OutgameItemManagerData();
                data.productUserDatas=new List<OutgameProductUserData>();
                data.itemUserDatas=new List<OutgameItemUserData>();
                data.saveTimestamp=-1;data.lastExitDayOfYear=-1;
            }
            return data;
        }
        public string SerializeRecord()=>JsonUtility.ToJson(this);
    }
}
