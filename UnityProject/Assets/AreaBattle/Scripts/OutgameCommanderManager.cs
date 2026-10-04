using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Original CommanderManager OnInit/UpdateDataCallBack/InitData/OnSave.
    public sealed class OutgameCommanderManager:IOutgameDataManager
    {
        [Serializable] sealed class ConfigRows {public OutgameCommanderConfig[] Datas;}
        readonly OutgameProfile profile;
        readonly string configs;
        readonly OutgameDataManagerStorage storage;
        readonly IOutgameDataStorageHost host;
        readonly Action<string> requestDownload;
        Dictionary<int,OutgameCommanderState> held;
        public string DataKey=>"CommanderManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameCommanderManager(OutgameProfile profile,string configs,OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> requestDownload)
        {this.profile=profile??throw new ArgumentNullException(nameof(profile));this.configs=configs;this.storage=storage??throw new ArgumentNullException(nameof(storage));this.host=host??throw new ArgumentNullException(nameof(host));this.requestDownload=requestDownload??throw new ArgumentNullException(nameof(requestDownload));}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){requestDownload(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {
            OutgameOriginalCommanders.ReadCurrent(profile,text,configs);
            RebuildIndex();
        }
        public void ApplyLegacy(string text)
        {
            if(OutgameOriginalCommanders.ApplyLegacy(profile,text,configs))RebuildIndex();
        }
        void RebuildIndex()
        {
            var valid=new HashSet<int>();foreach(var config in JsonUtility.FromJson<ConfigRows>(configs).Datas)valid.Add(config.id);
            held=new Dictionary<int,OutgameCommanderState>();
            foreach(var commander in profile.commanders)if(valid.Contains(commander.id))held.Add(commander.id,commander);
        }
        public void OnSave()
        {
            // Source rebuilds serialized list from the live dictionary before SaveLocalData.
            profile.commanders=new List<OutgameCommanderState>(held.Values);
            storage.SaveLocalData(OutgameOriginalCommanders.WriteCurrent(profile.usedCommanderId,profile.commanders));
        }
        // Source31013 TryGetValue returns null on miss;31015 unconditionally sets level1.
        public OutgameCommanderState GetCommanderData(int id)=>held.TryGetValue(id,out var state)?state:null;
        public void UnlockCommander(int id)=>GetCommanderData(id).level=1;
        public ICollection<OutgameCommanderState> GetCommanderDatas()=>held.Values;
        // Original31020 sums every live indexed level with unchecked Int32 arithmetic.
        public int GetCommanderTotalLv()
        {int total=0;foreach(var pair in held)total=unchecked(total+pair.Value.level);return total;}
        public void OnRelease(){}
    }
}
