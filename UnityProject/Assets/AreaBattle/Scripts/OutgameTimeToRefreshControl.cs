using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameRefreshServices
    {
        public Func<bool> IsReleaseVersion;
        public Func<long> ServerTime;
        public Func<DateTime> LocalNow;
        public Func<OutgameMessageDispatcher> Messages;
        public Func<Action> GetDisposableActions;
        public Action<Action> SetDisposableActions;
    }
    public sealed class OutgameRefreshGroup
    {
        public int Hour;
        public List<OutgameRefreshData> Records=new List<OutgameRefreshData>();
    }
    public sealed class OutgameRefreshData
    {
        public TimeSpan Interval;
        public DateTime LastRefreshTime;
        public Action<long> Refresh,Countdown;
        public Action<long,object[]> RefreshV2,CountdownV2;
        public Dictionary<object[],Action<long,object[]>> RefreshV2Dic=new Dictionary<object[],Action<long,object[]>>();
        public Dictionary<object[],Action<long,object[]>> CountdownV2Dic=new Dictionary<object[],Action<long,object[]>>();
    }
    // Source4589 public paths34784/85/87/88/93/99, DTO4586/87 and cleanup34803.
    public sealed class OutgameTimeToRefreshControl:MonoBehaviour
    {
        static OutgameTimeToRefreshControl instance;
        public static OutgameRefreshServices Services;
        public static long Timestamp;
        public static List<OutgameRefreshGroup> Groups=new List<OutgameRefreshGroup>();
        public float Elapsed;
        public DateTime BaseDate=new DateTime(1,1,1,0,0,0);
        public TimeSpan OneDay=new TimeSpan(1,0,0,0);
        public static OutgameTimeToRefreshControl Instance
        {
            get
            {
                if(instance==null)
                {
                    var root=GameObject.Find("TimeToRefreshControl");
                    if(root==null){root=new GameObject();root.name="TimeToRefreshControl";}
                    instance=root.GetComponent<OutgameTimeToRefreshControl>();
                    if(instance==null)instance=root.AddComponent<OutgameTimeToRefreshControl>();
                    Services.SetDisposableActions(Services.GetDisposableActions()+DisposeDelayed);
                    Services.Messages().AddListener("ExitGame",Clear);
                    DontDestroyOnLoad(root);
                }
                return instance;
            }
        }
        public static void BindStatistics(OutgameStatisticsOffNetServices services)
        {
            services.AddRefreshHandle=(hour,last,refresh,countdown,seconds)=>Instance.AddRefreshHandle(hour,last,refresh,countdown,seconds);
            services.RemoveRefreshHandle=(hour,refresh,countdown,args)=>Instance.RemoveRefreshHandle(hour,refresh,countdown,args);
        }
        public static void Clear(object[] args){Groups.Clear();}
        public static async void DisposeDelayed(){await OutgameUnityAwait.Await(new WaitForSeconds(.1f));Groups.Clear();}
        public void OnDestroy(){Groups.Clear();Services.SetDisposableActions(Services.GetDisposableActions()-DisposeDelayed);}
        public void AddRefreshHandle(int hour,long last,Action<long> refresh,Action<long> countdown,int seconds)
        {
            var interval=new TimeSpan((long)seconds*10000000L);
            for(int i=0;i<Groups.Count;i++)
            {
                var group=Groups[i];if(group.Hour!=hour)continue;
                for(int j=0;j<group.Records.Count;j++)
                {
                    var row=group.Records[j];
                    if(OutgameItemTimestamp.ToDateTime(last)!=row.LastRefreshTime||interval!=row.Interval)continue;
                    row.Refresh+=refresh;row.Countdown+=countdown;return;
                }
                group.Records.Add(new OutgameRefreshData{Interval=interval,LastRefreshTime=OutgameItemTimestamp.ToDateTime(last),Refresh=refresh,Countdown=countdown});return;
            }
            var added=new OutgameRefreshGroup{Hour=hour};
            added.Records.Add(new OutgameRefreshData{Interval=interval,LastRefreshTime=OutgameItemTimestamp.ToDateTime(last),Refresh=refresh,Countdown=countdown});Groups.Add(added);
        }
        public void RemoveRefreshHandle(int hour,Action<long> refresh,Action<long> countdown,object[] args)
        {
            for(int i=0;i<Groups.Count;i++)
            {
                var group=Groups[i];if(group.Hour!=hour)continue;
                for(int j=0;j<group.Records.Count;j++)
                {
                    var row=group.Records[j];if(row.Refresh==null){group.Records.RemoveAt(j);return;}
                    row.Refresh-=refresh;row.Countdown-=countdown;
                }
            }
        }
        static void AddV2(OutgameRefreshData row,Action<long,object[]> refresh,Action<long,object[]> countdown,object[] args,bool existing)
        {
            if(args==null){row.RefreshV2+=refresh;row.CountdownV2+=countdown;return;}
            if(row.RefreshV2Dic.ContainsKey(args))row.RefreshV2Dic[args]+=refresh;else row.RefreshV2Dic[args]=refresh;
            // Existing-row branch34788 combines refresh into an existing countdown slot.
            if(row.CountdownV2Dic.ContainsKey(args))row.CountdownV2Dic[args]+=existing?refresh:countdown;else row.CountdownV2Dic[args]=countdown;
        }
        public void AddRefreshHandleV2(int hour,long last,Action<long,object[]> refresh,Action<long,object[]> countdown,int seconds,object[] args)
        {
            var interval=new TimeSpan((long)seconds*10000000L);
            for(int i=0;i<Groups.Count;i++)
            {
                var group=Groups[i];if(group.Hour!=hour)continue;
                for(int j=0;j<group.Records.Count;j++)
                {
                    var row=group.Records[j];
                    if(OutgameItemTimestamp.ToDateTime(last)!=row.LastRefreshTime||interval!=row.Interval)continue;
                    AddV2(row,refresh,countdown,args,true);return;
                }
                var added=new OutgameRefreshData{Interval=interval,LastRefreshTime=OutgameItemTimestamp.ToDateTime(last)};
                AddV2(added,refresh,countdown,args,false);group.Records.Add(added);return;
            }
            var newGroup=new OutgameRefreshGroup{Hour=hour};
            var data=new OutgameRefreshData{Interval=interval,LastRefreshTime=OutgameItemTimestamp.ToDateTime(last)};
            AddV2(data,refresh,countdown,args,false);newGroup.Records.Add(data);Groups.Add(newGroup);
        }
        public void RemoveRefreshHandleV2(int hour,long last,Action<long,object[]> refresh,Action<long,object[]> countdown,int seconds,object[] args)
        {
            var interval=new TimeSpan((long)seconds*10000000L);
            for(int i=0;i<Groups.Count;i++)
            {
                var group=Groups[i];if(group.Hour!=hour)continue;
                for(int j=0;j<group.Records.Count;j++)
                {
                    var row=group.Records[j];
                    if(OutgameItemTimestamp.ToDateTime(last)!=row.LastRefreshTime||interval!=row.Interval)continue;
                    if(args==null){row.RefreshV2-=refresh;row.CountdownV2-=countdown;}
                    else{if(row.RefreshV2Dic.ContainsKey(args))row.RefreshV2Dic.Remove(args);if(row.CountdownV2Dic.ContainsKey(args))row.CountdownV2Dic.Remove(args);}
                    if(row.RefreshV2==null&&row.RefreshV2Dic.Count==0){group.Records.RemoveAt(j);return;}
                }
            }
        }
        public void Update(){Advance(Time.deltaTime);}
        static long Remaining(OutgameRefreshData row,DateTime now)=>(long)(row.LastRefreshTime+row.Interval-now).TotalMilliseconds;
        public void Advance(float delta)
        {
            Elapsed+=delta;if(!(Elapsed>=1))return;Elapsed-=1;
            if(Services.IsReleaseVersion()){Timestamp=Services.ServerTime();if(Timestamp<=0)Timestamp=OutgameItemTimestamp.FromDateTime(Services.LocalNow());}
            else Timestamp=OutgameItemTimestamp.FromDateTime(Services.LocalNow());
            var now=OutgameItemTimestamp.ToDateTime(Timestamp);int year=now.Year,month=now.Month,day=now.Day;
            for(int i=0;i<Groups.Count;i++)
            {
                for(int j=0;j<Groups[i].Records.Count;j++)
                {
                    var row=Groups[i].Records[j];long remaining;
                    if(now-row.LastRefreshTime>=row.Interval)
                    {
                        var boundary=BaseDate.AddYears(year-1).AddMonths(month-1).AddDays(day-1).AddHours(Groups[i].Hour);
                        if(now<boundary)boundary=BaseDate.AddYears(year-1).AddMonths(month-1).AddDays(day-2).AddHours(Groups[i].Hour);
                        row.LastRefreshTime=boundary;long stamp=OutgameItemTimestamp.FromDateTime(boundary);
                        row.Refresh?.Invoke(stamp);remaining=Remaining(row,now);
                        row.Countdown?.Invoke(remaining);row.RefreshV2?.Invoke(stamp,Array.Empty<object>());row.CountdownV2?.Invoke(remaining,Array.Empty<object>());
                        foreach(var pair in row.RefreshV2Dic)
                        {
                            pair.Value(stamp,pair.Key);
                            if(row.CountdownV2Dic.ContainsKey(pair.Key)&&row.CountdownV2Dic[pair.Key]!=null)row.CountdownV2Dic[pair.Key](remaining,pair.Key);
                        }
                    }
                    remaining=Remaining(row,now);row.Countdown?.Invoke(remaining);row.CountdownV2?.Invoke(remaining,Array.Empty<object>());
                    foreach(var pair in row.RefreshV2Dic)
                        if(row.CountdownV2Dic.ContainsKey(pair.Key)&&row.CountdownV2Dic[pair.Key]!=null)row.CountdownV2Dic[pair.Key](remaining,pair.Key);
                }
            }
        }
    }
}
