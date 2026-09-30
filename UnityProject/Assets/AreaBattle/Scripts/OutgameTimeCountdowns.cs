using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Static clock fields and methods from TimeModule3534. Original local epoch is08:00, not UTC conversion.
    public static class OutgameTimeClock
    {
        public static DateTime Now,StartTime,DefaultTime=new DateTime(1970,1,1,8,0,0);
        public static bool IsInit,IsFocus=true;public static double TimeDifference;
        public static double GetTime(DateTime value)
        {
            double ms=(value-StartTime).Ticks*.0001;
            return Math.Max(-922337203685477d,Math.Min(922337203685477d,ms))+TimeDifference;
        }
        public static double GetNowTime(Func<DateTime> localNow){Now=localNow();return GetTime(Now);}
        public static int GetNowTimeInt(Func<DateTime> localNow){Now=localNow();return ToInt((Now-StartTime).Ticks*1e-7);}
        public static long GetNowTimeLong(Func<DateTime> localNow){Now=localNow();double seconds=(Now-StartTime).Ticks*1e-7;return Math.Abs(seconds)<9223372036854775808d?(long)seconds:long.MinValue;}
        internal static int ToInt(double value)=>Math.Abs(value)<2147483648d?(int)value:int.MinValue;
    }
    // Source3537 Call: accumulate float, invoke at most once, reset only after successful callback.
    public sealed class OutgameTimeEvery
    {
        public Action<int,int> Callback;public int Interval;public float Accumulated;
        public OutgameTimeEvery(Action<int,int> callback,int interval){Callback=callback;Accumulated=0;Interval=interval;}
        public void Call(int id,int remaining,float elapsed){Accumulated+=elapsed;if(Interval<=Accumulated){Callback(id,remaining);Accumulated=0;}}
    }
    // Source countdown portion27270..27284; full module extends this with the shared TimeId and lifecycle.
    public class OutgameTimeCountdowns
    {
        protected readonly Func<DateTime> localNow;
        public Dictionary<int,Action<int>> CompleteHandles;
        public Dictionary<int,double> CompleteTimes;
        public Dictionary<int,OutgameTimeEvery> EverySecondHandles,EveryMillisecondHandles,EveryMinuteHandles;
        public List<int> RemoveList;
        public double LastTime,LastMillisecond,LastMinute,UnityRealTime;public int TimeId;
        public OutgameTimeCountdowns(Func<DateTime> localNow=null){this.localNow=localNow??(()=>DateTime.Now);}
        public virtual void Initialize(){InitializeCountdownMaps();ResetTimeFields();}
        protected void InitializeCountdownMaps()
        {
            OutgameTimeClock.StartTime=OutgameTimeClock.DefaultTime;
            CompleteHandles=new Dictionary<int,Action<int>>();EverySecondHandles=new Dictionary<int,OutgameTimeEvery>();
            EveryMinuteHandles=new Dictionary<int,OutgameTimeEvery>();EveryMillisecondHandles=new Dictionary<int,OutgameTimeEvery>();CompleteTimes=new Dictionary<int,double>();
        }
        protected void ResetTimeFields(){LastMinute=0;LastTime=0;TimeId=0;} // Retains LastMillisecond/UnityRealTime.
        public double GetTime(DateTime value)=>OutgameTimeClock.GetTime(value);
        public double GetNowTime()=>OutgameTimeClock.GetNowTime(localNow);
        public int GetNowTimeInt()=>OutgameTimeClock.GetNowTimeInt(localNow);
        public long GetNowTimeLong()=>OutgameTimeClock.GetNowTimeLong(localNow);
        public int GetTimeId()=>TimeId=unchecked(TimeId+1);
        public int SetCountDownByMillisecond(int duration,Action<int> complete,Action<int,int> every)=>AddCoundDown2(duration,complete,every);
        public int AddCoundDown2(int duration,Action<int> complete,Action<int,int> every)
        {
            if(complete==null)return -1;
            int id=GetTimeId();CompleteHandles.Add(id,complete);double deadline=GetNowTime()+duration;CompleteTimes.Add(id,deadline);
            if(every!=null)EveryMillisecondHandles.Add(id,new OutgameTimeEvery(every,1));return id;
        }
        public void RemoveTime(int id)=>Remove(id);
        public void Remove(int id)
        {
            if(CompleteHandles.ContainsKey(id))CompleteHandles.Remove(id);
            if(CompleteTimes.ContainsKey(id))CompleteTimes.Remove(id);
            if(EverySecondHandles.ContainsKey(id))EverySecondHandles.Remove(id);
            if(EveryMinuteHandles.ContainsKey(id))EveryMinuteHandles.Remove(id);
            if(EveryMillisecondHandles.ContainsKey(id))EveryMillisecondHandles.Remove(id);
            _=CompleteHandles.Count;_=EverySecondHandles.Count;_=EveryMinuteHandles.Count;
        }
        public void ClearCountdowns(){ClearCountdownMaps();ResetTimeFields();}
        protected void ClearCountdownMaps()
        {
            CompleteHandles.Clear();EverySecondHandles.Clear();EveryMinuteHandles.Clear();EveryMillisecondHandles.Clear();CompleteTimes.Clear();
        }
        public void CheckTime(DateTime date,float realtime){UnityRealTime=realtime;Check(date);}
        public void Check(DateTime date)
        {
            double now=GetTime(date);CheckComplete(now);CheckSecond(now);CheckMillisecond(now);CheckMinute(now);
        }
        public void CheckComplete(double now)
        {
            if(CompleteHandles.Count<1)return;
            RemoveList=new List<int>();var ids=new List<int>();var handles=new List<Action<int>>();
            foreach(var pair in CompleteHandles){ids.Add(pair.Key);handles.Add(pair.Value);}
            for(int i=0;i<handles.Count;i++){
                if(handles[i]!=null&&CompleteTimes.ContainsKey(ids[i])&&CompleteTimes[ids[i]]<=now){handles[i](ids[i]);RemoveList.Add(ids[i]);}
            }
            for(int i=0;i<RemoveList.Count;i++)Remove(RemoveList[i]);
        }
        void CallEvery(Dictionary<int,OutgameTimeEvery> dictionary,double now,Func<double> last,int unit)
        {
            var ids=new List<int>();var handles=new List<OutgameTimeEvery>();
            foreach(var pair in dictionary){ids.Add(pair.Key);handles.Add(pair.Value);}
            for(int i=0;i<handles.Count;i++)if(handles[i]!=null){
                int remaining=0;if(CompleteTimes.ContainsKey(ids[i]))remaining=OutgameTimeClock.ToInt(CompleteTimes[ids[i]]-now)/unit;
                handles[i].Call(ids[i],remaining,(float)(now-last())/unit);
            }
        }
        public void CheckSecond(double now)
        {
            if(now-LastTime>=1000){if(EverySecondHandles.Count>=1)CallEvery(EverySecondHandles,now,()=>LastTime,1000);LastTime=now;}
        }
        public void CheckMillisecond(double now)
        {
            if(now>LastMillisecond){if(EveryMillisecondHandles.Count>=1)CallEvery(EveryMillisecondHandles,now,()=>LastMillisecond,1);LastMillisecond=now;}
        }
        public void CheckMinute(double now)
        {
            if(!(now-LastMinute>=60000)||EveryMinuteHandles.Count<1)return;
            CallEvery(EveryMinuteHandles,now,()=>LastMinute,60000);LastMinute=now;
        }
        public void BindAudio(OutgameAudioActionServices services)
        {services.SetCountDownByMillisecond=SetCountDownByMillisecond;services.RemoveTime=RemoveTime;}
    }
}
