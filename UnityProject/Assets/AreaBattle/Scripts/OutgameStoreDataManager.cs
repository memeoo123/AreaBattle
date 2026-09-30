using System;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameStoreSettings
    {
        public int storeAdDiamondCount,storeAdGoldCount;
        public int[] storeAdSendDiamond;
        // Original constructors read global GameModule.Config.settingConfig.
        // Startup supplies the current source configuration before model construction.
        public static Func<OutgameStoreSettings> Resolve;
    }
    [Serializable] public sealed class OutgameStoreCoin
    {
        public int count=1;
        public void Reset()=>count=OutgameStoreSettings.Resolve().storeAdGoldCount;
    }
    [Serializable] public sealed class OutgameStoreDiamond
    {
        public int MaxCount {get;set;}
        [NonSerialized] int[] numArray=new[]{20,20,20};
        public int count;
        public long lastRewardTime;
        public OutgameStoreDiamond(){MaxCount=OutgameStoreSettings.Resolve().storeAdDiamondCount;}
        public bool CanFree=>count>0;
        public int CurrentReward=>numArray[count-1];
        public void Reset(){numArray=OutgameStoreSettings.Resolve().storeAdSendDiamond;count=MaxCount;}
    }
    [Serializable] public sealed class OutgameStoreRecord
    {
        public OutgameStoreCoin coinData=new OutgameStoreCoin();
        public OutgameStoreDiamond diamondData=new OutgameStoreDiamond();
    }
    // Original StoreDataManager source type4092. Empty and decoded-null paths retain
    // the original create/decode/create sequence, without repairs of held records.
    public sealed class OutgameStoreDataManager:IOutgameDataManager
    {
        readonly OutgameDataManagerStorage storage;
        readonly IOutgameDataStorageHost host;
        readonly Action<string> requestDownload;
        public static OutgameStoreDataManager Instance {get;private set;}
        public OutgameStoreRecord Data {get;private set;}
        public string DataKey=>"StoreDataManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameStoreDataManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> requestDownload)
        {this.storage=storage;this.host=host;this.requestDownload=requestDownload;}
        public void OnInit(){Instance=this;UpdateData(true);}
        public void OnRelease(){Instance=null;}
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){requestDownload(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {
            if(string.IsNullOrEmpty(text))CreateNewData();
            Data=JsonUtility.FromJson<OutgameStoreRecord>(text);
            if(Data==null)CreateNewData();
        }
        public void CreateNewData()
        {Data=new OutgameStoreRecord();Data.diamondData.Reset();Data.coinData.Reset();}
        public void OnSave()=>storage.SaveLocalData(JsonUtility.ToJson(Data));
    }
}
