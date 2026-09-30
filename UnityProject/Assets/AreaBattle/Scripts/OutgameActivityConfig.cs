using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameActivityConfigRow
    {
        [Serializable] public sealed class Name {public string key;}
        public int id,open;public Name activityName;
        public int[] launchType;public string[] launchParams,noticeParams,overParams;
    }
    public struct OutgameActivityWindow
    {public long Start,End,Notice;public int UnlockLevel;}
    // ConfigHelper.ReadActivityConfig32590/ TryParseStringToDateInt32593-style exact format.
    // The catalog here is recovered local data; network config selection remains ActivityConfigMgr's job.
    public sealed class OutgameActivityConfig
    {
        [Serializable] sealed class Rows {public OutgameActivityConfigRow[] Datas;}
        readonly Dictionary<int,OutgameActivityConfigRow> rows=new Dictionary<int,OutgameActivityConfigRow>();
        readonly Action<string> error;
        public OutgameActivityConfig(string json,Action<string> error)
        {this.error=error;foreach(var row in JsonUtility.FromJson<Rows>(json).Datas)rows.Add(row.id,row);}
        public OutgameActivityConfigRow Get(int id)=>rows.TryGetValue(id,out var row)?row:null;
        bool Parse(string text,out long timestamp)
        {
            timestamp=0;
            if(!DateTime.TryParseExact(text,"yyyyMMddHHmmss",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)){error("活动时间转换错误！");return false;}
            timestamp=OutgameItemTimestamp.FromDateTime(date);return true;
        }
        public OutgameActivityWindow Read(int id)
        {
            var result=new OutgameActivityWindow();var row=Get(id);if(row==null)return result;
            for(int i=0;i<row.launchType.Length;i++)
            {
                if(row.launchType[i]==10000){if(Parse(row.launchParams[i],out long stamp))result.Start=stamp;}
                else if(row.launchType[i]==10015)result.UnlockLevel=int.Parse(row.launchParams[i]);
            }
            if(row.overParams.Length!=0&&Parse(row.overParams[0],out long end))result.End=end;
            if(row.noticeParams.Length!=0&&Parse(row.noticeParams[0],out long notice))result.Notice=notice;
            return result;
        }
        public void Apply(int id,OutgameFestActivityState state,OutgameActivityCountdown countdown)
        {
            var row=Get(id)??throw new InvalidOperationException("Missing activity "+id);var window=Read(id);
            state.ActivityName=row.activityName.key;state.StartTimeStamp=window.Start;state.EndTimeStamp=window.End;state.NoticeTime=window.Notice;state.UnlockLevel=window.UnlockLevel;
            countdown.StartTimeStamp=window.Start;countdown.EndTimeStamp=window.End;
        }
    }
}
