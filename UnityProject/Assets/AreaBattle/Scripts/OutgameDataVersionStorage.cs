using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
namespace AreaBattle
{
    // GameDataVersionMgr local-list format and active WebGL MineGameName+GameDataVer key.
    public sealed class OutgameDataVersionStorage
    {
        readonly OutgameDataVersionState state;readonly OutgameSdkStringStorage storage;readonly string key;
        public OutgameDataVersionStorage(OutgameDataVersionState state,OutgameSdkStringStorage storage,string mineGameName)
        {this.state=state??throw new ArgumentNullException(nameof(state));this.storage=storage??throw new ArgumentNullException(nameof(storage));key=mineGameName+"GameDataVer";}
        public void Load(Action initializeMissingManagerVersions)
        {
            if(initializeMissingManagerVersions==null)throw new ArgumentNullException(nameof(initializeMissingManagerVersions));
            string text=storage.GetString(key,"");state.Local.Clear();
            if(!string.IsNullOrEmpty(text))
                foreach(var row in Decode(text))state.Local.Add(row.dataKey,row);
            initializeMissingManagerVersions();
        }
        public void LoadManagers(IEnumerable<IOutgameVersionManager> managers)
        {
            if(managers==null)throw new ArgumentNullException(nameof(managers));
            Load(()=>state.InitializeManagerVersions(managers));
        }
        public void Save()
        {
            state.SaveObserved=true;
            storage.SetString(key,Encode(new List<OutgameDataVersion>(state.Local.Values)));
        }
        public static string Encode(List<OutgameDataVersion> rows)
        {
            using(var stream=new MemoryStream())
            {new DataContractJsonSerializer(typeof(List<OutgameDataVersion>)).WriteObject(stream,rows);return Encoding.UTF8.GetString(stream.ToArray());}
        }
        public static List<OutgameDataVersion> Decode(string json)
        {
            using(var stream=new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return (List<OutgameDataVersion>)new DataContractJsonSerializer(typeof(List<OutgameDataVersion>)).ReadObject(stream);
        }
    }
}
