using System;
namespace AreaBattle
{
    public sealed class OutgameLocalDataManager:IOutgameDataManager
    {
        readonly OutgameProfile profile;
        readonly string skills;
        readonly OutgameDataManagerStorage storage;
        readonly IOutgameDataStorageHost host;
        readonly Action<string> requestDownload;
        readonly Action<OutgameLocalDataManager,string> registerSaveData;
        public OutgameLocalRecord Record {get;private set;}
        public OutgameLocalLimitedBags LimitedBags {get;}
        public string DataKey=>"LocalDataManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameLocalDataManager(OutgameProfile profile,string skills,string products,Func<DateTime> now,OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> requestDownload,Action<OutgameLocalDataManager,string> registerSaveData)
        {this.profile=profile??throw new ArgumentNullException(nameof(profile));this.skills=skills;this.storage=storage??throw new ArgumentNullException(nameof(storage));this.host=host??throw new ArgumentNullException(nameof(host));this.requestDownload=requestDownload??throw new ArgumentNullException(nameof(requestDownload));this.registerSaveData=registerSaveData??throw new ArgumentNullException(nameof(registerSaveData));LimitedBags=new OutgameLocalLimitedBags(products,now);}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){requestDownload(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {
            Record=OutgameLocalRecord.Read(text);
            LimitedBags.Initialize(Record);
            OutgameOriginalLocalData.Apply(profile,text,skills);
            Record.CaptureProfile(profile);
            registerSaveData(this,string.IsNullOrEmpty(text)?text:text.Replace("bank","collect"));
        }
        // The profile owns the live currency projection used by existing inventory actions.
        public int GoldNum=>profile.inventory.goldNum;
        public int DiamondsNum=>profile.inventory.diamondsNum;
        public bool HasPurchasedJewelKey(string key)=>!string.IsNullOrEmpty(key)&&Record.PurchasedJewelKey!=null&&Record.PurchasedJewelKey.Contains(key);
        public void SetPurchasedJewelKey(string key)
        {
            if(!string.IsNullOrEmpty(key)&&Record.PurchasedJewelKey!=null&&!Record.PurchasedJewelKey.Contains(key))Record.PurchasedJewelKey.Add(key);
        }
        // LocalDataManager31624 -> FirstChargeData31636 -> TimeExtend32528.
        public void SuccessFirstCharge(Func<long> nowTimestamp)
        {
            if(Record.firstChargeData==null)Record.firstChargeData=new OutgameFirstChargeRecord();
            var charge=Record.firstChargeData;charge.hasCharge=true;
            long now=nowTimestamp();var date=OutgameLocalLimitedBags.ToSourceDateTime(now);
            charge.firstChargeTime=unchecked(now-(date.Hour*3600000+date.Minute*60000+date.Second*1000+date.Millisecond));
        }
        public void OnSave()
        {
            LimitedBags.PrepareSave(Record);Record.CaptureProfile(profile);storage.SaveLocalData(Record.ToOriginalJson());
        }
    }
}
