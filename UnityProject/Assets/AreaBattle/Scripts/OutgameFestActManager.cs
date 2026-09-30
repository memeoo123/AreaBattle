using System;
using UnityEngine;
namespace AreaBattle
{
    // FestActManager4505. Award dispatch may save before this manager advances its reward index.
    public sealed class OutgameFestActManager:IOutgameDataManager
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;public int[] itemId;}
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;readonly Action<string> download;
        readonly string config;readonly OutgameServerClock clock;readonly OutgameLevelProgression levels;
        readonly Func<int,int,bool,string,bool,bool> grant;readonly Func<string> activityName;readonly Action<string,string,string,string> report;
        public OutgameFestActivityData Data {get;private set;}
        public int SignRewardCount {get;set;}public int LimitSkinCount {get;set;}
        public string DataKey=>"FestActManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameFestActManager(string config,OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,
            OutgameServerClock clock,OutgameLevelProgression levels,Func<int,int,bool,string,bool,bool> grant,Func<string> activityName,Action<string,string,string,string> report)
        {this.config=config;this.storage=storage;this.host=host;this.download=download;this.clock=clock;this.levels=levels;this.grant=grant;this.activityName=activityName;this.report=report;}
        public void OnInit()
        {
            UpdateData(true);var rows=JsonUtility.FromJson<Rows>(config).Datas;
            SignRewardCount=Array.Find(rows,x=>x.id==1).itemId.Length;LimitSkinCount=Array.Find(rows,x=>x.id==2).itemId.Length;
        }
        public void UpdateData(bool allowServer)
        {if(allowServer&&host.IsUseServer&&ParticipatesInSync){download(DataKey);return;}UpdateDataCallBack(storage.ReadLocalData());}
        public void UpdateDataCallBack(string json){Data=OutgameFestActivityData.Read(json);}
        public void OnSave(){storage.SaveLocalData(Data.ToOriginalJson());}
        public void OnRelease(){}
        public void CheckInit(long notice,long end)=>Data.CheckInit(notice,end,clock.GetNowTimestampLong);
        public bool CanGetTodayReward()=>Data.rewardId>=1&&Data.rewardId<=SignRewardCount&&clock.GetNowTimestampLong()>Data.nextGetAwardTime;
        public void GetTodayReward(int id,int amount)
        {
            grant(id,amount,true,"",true);
            var held=Data;long now=clock.GetNowTimestampLong();var date=OutgameItemTimestamp.ToDateTime(now);
            held.nextGetAwardTime=unchecked(now-(date.Hour*3600000+date.Minute*60000+date.Second*1000+date.Millisecond)+86400000);
            report(activityName(),levels.CurrentLevel.ToString(),Data.rewardId.ToString(),null);
            Data.rewardId=unchecked(Data.rewardId+1);
        }
        public void CompleteSpecialLevel1(){Data.limetSkinStatus|=1;}
        public void CompleteSpecialLevel2(){Data.limetSkinStatus|=2;}
    }
}
