using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // UserInfoData4230, including legacy fields retained by the source serializer.
    [Serializable] public sealed class OutgameUserInfoData
    {
        public string uname="";
        public int headportId=1,hdboxId=1,hasHeadport=3,hasHdBox=1;
        public long installTime,d3newPlayer;
        public List<int> headPort,headBox;
    }
    // UserInfoManager4229. Name generation remains the explicit ConfigHelper32592
    // dependency, including its random sequence; it is not replaced with a fixed name.
    public sealed class OutgameUserInfoManager:IOutgameDataManager
    {
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;
        readonly Action<string> download;readonly Func<long> timestamp;
        readonly Func<bool> isInstallVersion;readonly Func<int,bool,List<string>> randomNames;
        readonly Func<OutgameMessageDispatcher> messages;
        public OutgameUserInfoData Data;
        public readonly OutgameHeadBoxInventory HeadBoxes;
        public string DataKey=>"UserInfoManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameUserInfoManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,
            Action<string> download,Func<long> timestamp,Func<bool> isInstallVersion,
            Func<int,bool,List<string>> randomNames,Func<OutgameMessageDispatcher> messages)
        {
            this.storage=storage;this.host=host;this.download=download;this.timestamp=timestamp;
            this.isInstallVersion=isInstallVersion;this.randomNames=randomNames;this.messages=messages;
            HeadBoxes=new OutgameHeadBoxInventory(()=>Data.headBox,messages);
        }
        public string Name {get=>Data.uname;set=>Data.uname=value;}
        public int Icon
        {
            get=>Data.headportId;
            set {if(value!=Data.headportId)messages().SendMessage("HeadportChange",new object[]{value});Data.headportId=value;}
        }
        public int IconBox
        {
            get=>Data.hdboxId;
            set {if(value!=Data.hdboxId)messages().SendMessage("HeadBoxChange",new object[]{value});Data.hdboxId=value;}
        }
        public long InstallTime=>Data.installTime;
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){download(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {
            if(string.IsNullOrEmpty(text))CreateNewData();
            else {
                Data=JsonUtility.FromJson<OutgameUserInfoData>(text);
                if(Data==null)CreateNewData();
                // Original tests counts without null guards, and replaces BOTH lists
                // if either is empty; old unlocks are not merged into the defaults.
                if(Data.headPort.Count<1||Data.headBox.Count<=0){
                    Data.headPort=new List<int>{1,2,3,4};Data.headBox=new List<int>{1,2};
                }
            }
            if(string.IsNullOrEmpty(Data.uname)){
                var target=Data;target.uname=randomNames(1,false)[0];
            }
            CheckInstallVersion();
        }
        public void CreateNewData()
        {
            var fresh=new OutgameUserInfoData();fresh.installTime=timestamp();
            fresh.headPort=new List<int>{1,2,3,4};fresh.headBox=new List<int>{1,2};Data=fresh;
        }
        public void CheckInstallVersion()
        {if(isInstallVersion())return;var target=Data;if(target.installTime==0)target.installTime=timestamp();}
        public bool HaveHeadPort(int id)=>Data.headPort.Contains(id);
        public bool HaveHeadBox(int id)=>HeadBoxes.HaveHeadBox(id);
        public void AddHeadBox(int id)=>HeadBoxes.AddHeadBox(id);
        public void OnSave()=>storage.SaveLocalData(JsonUtility.ToJson(Data));
        public void OnRelease()=>Data=null;
    }
}
