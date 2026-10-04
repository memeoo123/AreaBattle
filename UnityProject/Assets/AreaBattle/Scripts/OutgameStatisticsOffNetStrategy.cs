using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameStatisticsOffNetServices
    {
        public Func<OutgameMessageDispatcher> Messages;
        public Func<long> ServerTime;
        public Func<bool> HasHttpHelper,IsReleaseVersion;
        public Action RequestServerTime;
        public Func<DateTime> LocalNow;
        public Func<float> RealtimeSinceStartup,DeltaTime;
        public Func<OutgameUpdateManager> Updates;
        public Action<int,long,Action<long>,Action<long>,int> AddRefreshHandle;
        public Action<int,Action<long>,Action<long>,object[]> RemoveRefreshHandle;
        public Action<string> Warning;
        public Action<object[]> Error;
    }
    // Concrete source4623. Account/server/http and daily-refresh services are required from their actual owners.
    public sealed class OutgameStatisticsOffNetStrategy:OutgameStatisticsStrategy
    {
        readonly OutgameStatisticsOffNetServices services;
        public OutgameStatisticsJsonData Data;
        public Coroutine RefreshCoroutine;
        public long ClockAnchor,RealtimeAnchor;
        public float SecondElapsed,OnlineElapsed,DirtyElapsed;
        public OutgameStatisticsOffNetStrategy(Func<OutgameDataManagerPool> pool,Func<OutgameCommonMessageDispatcher> common,
            Func<OutgameStatisticsControl> control,OutgameStatisticsExpansion expansion,OutgameStatisticsOffNetServices services)
            :base(pool,common,control,expansion){this.services=services;}
        public override void InitData(Action<OutgameStatisticsJsonData> update)
        {
            base.InitData(update);
            Manager.UpdateData(true);
            services.Messages().AddListener("GF_ShowInterst",InterstitialShown);
            services.Messages().AddListener("GF_AdsPlayCallBack",AdsComplete);
            services.Messages().AddListener("GF_ReceiveServerTime",ReceiveServerTime);
            services.Messages().AddListener("Item_ItemChange",ItemChanged);
            services.Messages().AddListener("ItemUI_ProductReset",ProductReset);
            services.Messages().AddListener("ItemUI_ProductReward",ProductReward);
        }
        public override void LoadData(string text)
        {
            try{Data=JsonUtility.FromJson<OutgameStatisticsJsonData>(OutgameStatisticsCodec.DecompressString(text,services.Warning));}
            catch{Data=JsonUtility.FromJson<OutgameStatisticsJsonData>(text);}
            if(Data==null)
            {
                Data=new OutgameStatisticsJsonData();
                long now=services.ServerTime();
                OutgameStatisticsJsonData held;
                if(now>=1){Expansion.SetEventCount(10000,now);held=Data;}
                else{held=Data;now=OutgameItemTimestamp.FromDateTime(services.LocalNow());}
                held.firstStartTimeStamp=now;Control().IsDirty=true;
            }
            RefreshClock();
            UpdateDataAction(Data);
            if(RefreshCoroutine==null)RefreshCoroutine=services.Updates().StartCoroutine(WaitForRefresh());
        }
        public IEnumerator WaitForRefresh()
        {
            yield return new WaitUntil(()=>Pool().IsEnableSaveData);
            services.AddRefreshHandle(0,Data.lastRefreshTimeStamp,RefreshDay,null,86400);
            Expansion.AddEventCount(10902,1);
        }
        // Source converts the float multiplication to signed64, using MinValue for non-finite/out-of-range values.
        static long Milliseconds(float seconds)
        {double value=seconds*1000f;return Math.Abs(value)<9223372036854775808d?(long)value:long.MinValue;}
        public void RefreshClock()
        {
            ClockAnchor=services.ServerTime();
            if(ClockAnchor==0)ClockAnchor=OutgameItemTimestamp.FromDateTime(services.LocalNow());
            RealtimeAnchor=Milliseconds(services.RealtimeSinceStartup());
            if(services.HasHttpHelper())services.RequestServerTime();
        }
        public long NowTimestamp()
        {long anchor=ClockAnchor;float seconds=services.RealtimeSinceStartup();long baseline=RealtimeAnchor;return unchecked(anchor+unchecked(Milliseconds(seconds)-baseline));}
        public void ReceiveServerTime(object[] args)
        {ClockAnchor=(long)args[0];RealtimeAnchor=Milliseconds(services.RealtimeSinceStartup());}
        public override long Value10000(object[] args)=>services.IsReleaseVersion()?NowTimestamp():OutgameItemTimestamp.FromDateTime(services.LocalNow());
        public override void UpdateDate(object[] args)
        {
            if(ClockAnchor==0)RefreshClock();
            SecondElapsed+=services.DeltaTime();OnlineElapsed+=services.DeltaTime();DirtyElapsed+=services.DeltaTime();
            if(SecondElapsed>=1f)
            {
                SecondElapsed-=1f;
                Expansion.SetEventCount(10000,NowTimestamp());
                if(DirtyElapsed>30f){DirtyElapsed-=30f;Control().IsDirty=true;}
                var now=OutgameItemTimestamp.ToDateTime(Expansion.GameValue(10000,Array.Empty<object>()));
                int hour=now.Hour,week=(int)now.DayOfWeek,day=now.Day;
                if(Expansion.GameValue(10500,Array.Empty<object>())!=hour)SendCalendarChange(10500);
                if(Expansion.GameValue(10501,Array.Empty<object>())!=week)SendCalendarChange(10501);
                if(Expansion.GameValue(10502,Array.Empty<object>())!=day)SendCalendarChange(10502);
            }
            if(OnlineElapsed>=30f)
            {OnlineElapsed-=30f;Expansion.AddEventCount(10800,30);RefreshClock();}
        }
        void SendCalendarChange(int id)
        {string key=OutgameStatisticsMessageKey.Get(id);Common().SendMessageGetKey(key,Array.Empty<object>());}
        public void RefreshDay(long timestamp)
        {
            Data.lastRefreshTimeStamp=timestamp;Control().IsDirty=true;
            Expansion.AddEventCount(10901,1);
            Expansion.ResetEventCount(10004);Expansion.ResetEventCount(10005);Expansion.ResetEventCount(10009);
            Expansion.ResetEventCount(10010);Expansion.ResetEventCount(10016);Expansion.ResetEventCount(10801);
            long now=Expansion.GameValue(10000,Array.Empty<object>());long first=Data.firstStartTimeStamp;
            long ticks=unchecked(unchecked(now-first)*10000L);
            Expansion.SetEventCount(10900,unchecked((int)(ticks/864000000000L)));
        }
        public override void OnSave(OutgameStatisticsJsonData data,Dictionary<int,OutgameGameStatisticsData> records)
        {
            if(records==null||data==null)return;
            Control().IsDirty=false;
            data.datas=new List<OutgameGameStatisticsData>();
            foreach(var row in records.Values)
            {
                if(row.items!=null&&row.items.Count==0)row.items=null;
                if(row.count==0&&row.items==null)continue;
                data.datas.Add(row);
            }
            Manager.SaveData(OutgameStatisticsCodec.CompressString(JsonUtility.ToJson(data)));
        }
        public void InterstitialShown(object[] args){Expansion.AddEventCount(10001,1);Expansion.AddEventCount(10004,1);}
        public void AdsComplete(object[] args){if((bool)args[0]){Expansion.AddEventCount(10002,1);Expansion.AddEventCount(10005,1);}}
        public void ItemChanged(object[] args)
        {
            if(args==null||args.Length<2)return;
            int id=(int)args[0];long count=(long)args[1];
            if(count<=-1)Expansion.AddEventCount(10007,id,unchecked(-count));
            else Expansion.AddEventCount(10008,id,count);
        }
        public void ProductReset(object[] args)
        {
            if(args==null||args.Length<1||args.Length>2)return;
            int id=(int)args[args.Length==2?1:0];Expansion.SetEventCount(10019,id,0);
        }
        public void ProductReward(object[] args)
        {
            if(args==null||args.Length<1)return;
            int id=(int)args[0];
            if(args.Length>=2)
            {
                int count=(int)args[1];
                if(args.Length>=3)
                {
                    int type=(int)args[2];Expansion.AddEventCount(10017,type,(long)count);Expansion.AddEventCount(10018,id,(long)count);
                    if(args.Length==3)Expansion.SetEventCount(10019,id,0);
                    if(args.Length<4)return;
                    id=(int)args[3];
                }
            }
            Expansion.SetEventCount(10019,id,0);
        }
        public override long Value10015(object[] args)=>Control().GetEventCount(10015);
        public override long Value10011(object[] args)=>Control().GetEventCount(10011);
        public override long Value10901(object[] args)=>Control().GetEventCount(10901);
        public override long Value10900(object[] args)=>Control().GetEventCount(10900);
        public override long Value10800(object[] args)=>Control().GetEventCount(10800);
        public override long Value10003(object[] args)=>Control().GetEventCount(10003);
        public override long Value10002(object[] args)=>Control().GetEventCount(10002);
        public override long Value10001(object[] args)=>Control().GetEventCount(10001);
        public override long Value10902(object[] args)=>Control().GetEventCount(10902);
        public override long Value10008(object[] args)
        {if(args==null){services.Error(new object[]{"����Ϊ��"});return 0;}int id=(int)args[0];return Control().GetEventCount(10008,id);}
        public override long Value10007(object[] args)
        {if(args==null){services.Error(new object[]{"��������Ϊ��"});return 0;}int id=(int)args[0];return Control().GetEventCount(10007,id);}
        public override long Value20000(object[] args)=>Control().GetEventCount(20000);
        protected override void OnDispose()
        {
            services.Messages().RemoveListener("GF_ShowInterst",InterstitialShown);
            services.Messages().RemoveListener("GF_AdsPlayCallBack",AdsComplete);
            services.Messages().RemoveListener("Item_ItemChange",ItemChanged);
            services.Messages().RemoveListener("ItemUI_ProductReset",ProductReset);
            services.Messages().RemoveListener("ItemUI_ProductReward",ProductReward);
            services.Messages().RemoveListener("GF_ReceiveServerTime",ReceiveServerTime);
            services.RemoveRefreshHandle(0,RefreshDay,null,Array.Empty<object>());
            if(RefreshCoroutine!=null){services.Updates().StopCoroutine(RefreshCoroutine);RefreshCoroutine=null;}
        }
    }
}
