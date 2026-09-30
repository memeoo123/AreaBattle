using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace AreaBattle
{
    // DataManagerBase.get_DataKey (slot 4) and source byte at offset 9.
    public interface IOutgameVersionManager
    {
        string DataKey {get;}
        bool ParticipatesInSync {get;}
    }
    [Serializable,DataContract] public sealed class OutgameDataVersion
    {
        [DataMember] public string dataKey;
        [DataMember] public int versionNumber=-1;
        [OnDeserializing] void InitializeDefaults(StreamingContext context){versionNumber=-1;}
    }
    [Serializable] public sealed class OutgameReportDataVersion {public string dataKey;public int localVersionNumber,serverVersionNumber;}
    // GameDataVersionMgr f12179/f6056/f12175 and CloseSynData wasmcode1 f27835.
    // Unresolved field identities retain source offsets rather than invented semantics.
    public sealed class OutgameDataVersionState
    {
        public readonly Dictionary<string,OutgameDataVersion> Local=new Dictionary<string,OutgameDataVersion>();
        public readonly Dictionary<string,OutgameDataVersion> Server=new Dictionary<string,OutgameDataVersion>();
        public readonly List<OutgameReportDataVersion> Pending20=new List<OutgameReportDataVersion>(),Pending24=new List<OutgameReportDataVersion>();
        public bool SourceFlag16,SourceFlag18,ForceUpload,Closed,SaveObserved;
        readonly Action persist,reportSuccess,sendSyncOver;
        readonly Action<int> setLocalSyncFlag;
        readonly Action<string> error;
        public OutgameDataVersionState(Action persist,Action reportSuccess,Action sendSyncOver,Action<int> setLocalSyncFlag,Action<string> error)
        {
            this.persist=persist??throw new ArgumentNullException(nameof(persist));this.reportSuccess=reportSuccess??throw new ArgumentNullException(nameof(reportSuccess));
            this.sendSyncOver=sendSyncOver??throw new ArgumentNullException(nameof(sendSyncOver));this.setLocalSyncFlag=setLocalSyncFlag??throw new ArgumentNullException(nameof(setLocalSyncFlag));
            this.error=error??throw new ArgumentNullException(nameof(error));
        }
        // GameDataVersionMgr f12180: add missing rows, prune stale keys, then save once.
        public void InitializeManagerVersions(IEnumerable<IOutgameVersionManager> managers)
        {
            if(managers==null)throw new ArgumentNullException(nameof(managers));
            foreach(var manager in managers)
            {
                if(!manager.ParticipatesInSync||Local.ContainsKey(manager.DataKey))continue;
                string key=manager.DataKey;
                var row=new OutgameDataVersion{dataKey=manager.DataKey};
                Local.Add(key,row);
            }
            foreach(string key in new List<string>(Local.Keys))
            {
                bool found=false;
                foreach(var manager in managers)
                    if(manager.ParticipatesInSync&&string.Equals(manager.DataKey,key)) {found=true;break;}
                if(!found)Local.Remove(key);
            }
            persist();
        }
        public void AddGameVersion(string key)
        {
            if(string.IsNullOrEmpty(key)){error("GameData:AddGameVer时，DataKey为空");return;}
            if(!Local.TryGetValue(key,out var version)){version=new OutgameDataVersion{dataKey=key};Local.Add(key,version);}
            version.versionNumber=unchecked(version.versionNumber+1);persist();
        }
        public void CheckCompletion()
        {
            if(Pending20.Count!=0||Pending24.Count!=0)return;
            if(ForceUpload)setLocalSyncFlag(1);
            Complete();
        }
        public void Complete()
        {
            reportSuccess();SourceFlag16=false;persist();sendSyncOver();
        }
        public void Close()
        {
            SourceFlag16=false;Closed=true;Pending20.Clear();Pending24.Clear();ForceUpload=false;SourceFlag18=false;
        }
    }
}
