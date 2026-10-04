using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameGuideBookData
    {
        public List<int> GuideRewards,TipRewards; // Original4077 ctor leaves both null.
    }
    // GuideBookDataManager4078. Claims are recorded here; UI owns granting items.
    public sealed class OutgameGuideBookManager:IOutgameDataManager
    {
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;
        readonly Action<string> download;
        public OutgameGuideBookData BookData;
        public string DataKey=>"GuideBookDataManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameGuideBookManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download)
        {this.storage=storage;this.host=host;this.download=download;}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){download(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {
            if(string.IsNullOrEmpty(text)){BookData=CreateNewData();return;}
            BookData=JsonUtility.FromJson<OutgameGuideBookData>(text);
            if(BookData==null)BookData=CreateNewData();
        }
        public OutgameGuideBookData CreateNewData()=>new OutgameGuideBookData{GuideRewards=new List<int>(),TipRewards=new List<int>()};
        public bool ContainsGuide(int id)=>BookData.GuideRewards.Contains(id);
        public bool ContainsTip(int id)=>BookData.TipRewards.Contains(id);
        public void GetGuideReward(int id)
        {if(!BookData.GuideRewards.Contains(id)){BookData.GuideRewards.Add(id);OnSave();}}
        public void GetTipReward(int id)
        {if(!BookData.TipRewards.Contains(id)){BookData.TipRewards.Add(id);OnSave();}}
        public void OnSave()=>storage.SaveLocalData(JsonUtility.ToJson(BookData));
        public void OnRelease(){} //31345 empty; source retains the record.
    }
}
