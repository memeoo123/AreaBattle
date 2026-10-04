using System;
using System.Collections.Generic;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameStatisticsItemData
    {public int eventId;public long count;}
    [Serializable] public sealed class OutgameGameStatisticsData
    {
        public int StatisticsEvent;
        public long count;
        public List<OutgameStatisticsItemData> items;
        public OutgameGameStatisticsData(int id,long count){StatisticsEvent=id;this.count=count;}
    }
    [Serializable] public sealed class OutgameStatisticsJsonData
    {
        public long firstStartTimeStamp,lastRefreshTimeStamp;
        public List<OutgameGameStatisticsData> datas=new List<OutgameGameStatisticsData>();
    }
    // Source35036/35037. The original cache is shared, signed64-keyed and never cleared by controller disposal.
    public static class OutgameStatisticsMessageKey
    {
        static readonly Dictionary<long,string> values=new Dictionary<long,string>();
        public static string Get(int id)
        {long key=id;if(!values.ContainsKey(key))values.Add(key,id.ToString());values.TryGetValue(key,out var value);return value;}
    }
    // Exact mutable-record portion of GameStatisticsManager35015..35022 and indexing loop35011.
    // Full manager/control/strategy lifecycle owns loading, dirty state and persistence; this class does not substitute them.
    public sealed class OutgameStatisticsRecords
    {
        readonly Func<OutgameCommonMessageDispatcher> messages;
        public Dictionary<int,OutgameGameStatisticsData> Datas=new Dictionary<int,OutgameGameStatisticsData>();
        public OutgameStatisticsRecords(Func<OutgameCommonMessageDispatcher> messages){this.messages=messages;}
        public void IndexRecords(OutgameStatisticsJsonData data)
        {for(int i=0;i<data.datas.Count;i++)Datas[data.datas[i].StatisticsEvent]=data.datas[i];}
        public OutgameGameStatisticsData GetGameStatisticsData(int id)
        {return Datas!=null&&Datas.TryGetValue(id,out var row)?row:null;}
        OutgameGameStatisticsData EnsureForAdd(int id)
        {
            var row=GetGameStatisticsData(id);if(row!=null)return row;
            row=new OutgameGameStatisticsData(id,0);
            if(Datas==null)Datas=new Dictionary<int,OutgameGameStatisticsData>();
            Datas.Add(id,row);return row;
        }
        void Notify(int id,params object[] args){string key=OutgameStatisticsMessageKey.Get(id);messages().SendMessageGetKey(key,args);}
        public void AddEventStatistics(int id,long value)
        {var row=EnsureForAdd(id);row.count=unchecked(value+row.count);Notify(id,value);}
        public void AddEventStatistics(int id,int itemId,long value)
        {
            var row=EnsureForAdd(id);
            if(row.items==null)row.items=new List<OutgameStatisticsItemData>();
            for(int i=0;i<row.items.Count;i++)if(row.items[i].eventId==itemId){row.items[i].count=unchecked(value+row.items[i].count);Notify(id,value,itemId);return;}
            row.items.Add(new OutgameStatisticsItemData{eventId=itemId,count=value});Notify(id,value,itemId);
        }
        public void SetEventStatistics(int id,long value)
        {
            var row=GetGameStatisticsData(id);
            if(value!=0&&row==null)row=EnsureForAdd(id);
            long delta;
            if(value!=0){delta=unchecked(value-row.count);row.count=value;}
            else {delta=row==null?0:unchecked(-row.count);if(Datas!=null)Datas.Remove(id);}
            Notify(id,delta);
        }
        public void SetEventStatistics(int id,int itemId,long value)
        {
            var row=GetGameStatisticsData(id);
            if(row==null)
            {
                row=new OutgameGameStatisticsData(id,0);Datas.Add(id,row); // Source does not repair a null dictionary on this overload.
                if(itemId!=0){if(row.items==null)row.items=new List<OutgameStatisticsItemData>();row.items.Add(new OutgameStatisticsItemData{eventId=itemId,count=value});}
                else row.count=value;
                Notify(id,value,itemId);return;
            }
            if(row.items!=null)
            {
                for(int i=0;i<row.items.Count;i++)if(row.items[i].eventId==itemId){long delta=unchecked(value-row.items[i].count);row.items[i].count=value;Notify(id,delta,itemId);return;}
            }
            else row.items=new List<OutgameStatisticsItemData>();
            row.items.Add(new OutgameStatisticsItemData{eventId=itemId,count=value});Notify(id,value,itemId);
        }
        public long GetEventStatistics(int id)=>GetGameStatisticsData(id)?.count??0;
        public long GetEventStatistics(int id,int itemId)
        {
            var row=GetGameStatisticsData(id);if(row==null)return 0;
            if(row.items==null)return row.count;
            for(int i=0;i<row.items.Count;i++)if(row.items[i].eventId==itemId)return row.items[i].count;
            return 0;
        }
        public void ResetEventStatistics(int id)
        {
            var row=GetGameStatisticsData(id);long delta=0;
            if(row!=null){Datas.Remove(id);delta=unchecked(-row.count);}
            Notify(id,delta);
        }
    }
}
