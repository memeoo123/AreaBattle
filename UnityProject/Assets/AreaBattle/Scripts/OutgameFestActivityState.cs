using System;
using UnityEngine;
namespace AreaBattle
{
    // Original SummerData4506 serialized names; FestActManager.CheckInit34420.
    [Serializable] public sealed class OutgameFestActivityData
    {
        public bool isShowPanel=true;
        public long nextGetAwardTime;
        public int rewardId=1,limetSkinStatus,status=1;
        public long openActTime=-1;
        public static OutgameFestActivityData Read(string json)=>string.IsNullOrEmpty(json)?new OutgameFestActivityData():JsonUtility.FromJson<OutgameFestActivityData>(json)??new OutgameFestActivityData();
        public string ToOriginalJson()=>JsonUtility.ToJson(this);
        public void CheckInit(long notice,long end,Func<long> now)
        {
            if(notice<=openActTime&&end>=openActTime)return;
            status=1;rewardId=1;limetSkinStatus=0;nextGetAwardTime=0;isShowPanel=true;openActTime=now();
        }
    }
    // Controller4504 source34395/34402. Configuration and manager storage retain their own lifecycle.
    public sealed class OutgameFestActivityState
    {
        readonly Func<OutgameFestActivityData> data;readonly Func<bool> enabled;
        readonly OutgameServerClock clock;readonly OutgameLevelProgression levels;
        readonly Action<string,string,string,string,string> report;
        public long NoticeTime,StartTimeStamp,EndTimeStamp;
        public int UnlockLevel;public string ActivityName;
        public OutgameFestActivityState(Func<OutgameFestActivityData> data,Func<bool> enabled,OutgameServerClock clock,OutgameLevelProgression levels,Action<string,string,string,string,string> report)
        {this.data=data;this.enabled=enabled;this.clock=clock;this.levels=levels;this.report=report;}
        public int Status=>enabled()?data().status:5;
        void Set(int value){data().status=value;}
        void Report(string kind)=>report(kind,ActivityName,levels.CurrentLevel.ToString(),null,null);
        public void Refresh()
        {
            if(clock.ConnectedNext)
            {
                long now=clock.GetNowTimestampLong();
                if(now<NoticeTime){Set(1);return;}
                if(EndTimeStamp>now)
                {
                    if(StartTimeStamp>now){if(Status<=1)Report("warmup");Set(2);}
                    if(UnlockLevel>=levels.CurrentLevel)return;
                    if(StartTimeStamp<=now){if(Status<=3)Report("unlock");Set(4);return;}
                    Set(3);return;
                }
                if(Status!=5)Report("complete");Set(5);return;
            }
            if(Status>=3&&Status<=4)Set(6);
        }
    }
}
