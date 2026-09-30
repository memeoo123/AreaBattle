using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace AreaBattle
{
    // Reconstruction envelope, not the original platform save wire format.
    [Serializable] public sealed class OutgameProfile
    {
        public int schemaVersion=1;
        public string sourceLocalDataJson; // Preserved verbatim for remaining original subsystem migrations.
        public OutgameSkinState skins; // Initialized by the original skin manager loading path.
        public OutgameLocalInventoryState inventory=new OutgameLocalInventoryState();
        public int usedCommanderId=1;
        public int levelID; // Original LocalData scalar; outer first-account initialization is separate.
        public List<OutgameCommanderState> commanders=new List<OutgameCommanderState>();
    }
    public static class OutgameProfileInitialization
    {
        [Serializable] sealed class Rows { public OutgameCommanderConfig[] Datas; }
        // CommanderManager.InitData: held records survive; only missing config IDs get defaults.
        // This does not perform guide rewards, remote login or old-platform save migration.
        public static void Initialize(OutgameProfile profile,string commandersJson,string skillsJson)
        {
            if(profile==null||profile.schemaVersion!=1||profile.inventory==null||profile.commanders==null)
                throw new InvalidDataException("Unsupported or incomplete outgame profile.");
            InitializeCommanders(profile,commandersJson);
            new OutgameLocalInventory(profile.inventory,skillsJson);
        }
        public static void InitializeCommanders(OutgameProfile profile,string commandersJson)
        {
            var configs=JsonUtility.FromJson<Rows>(commandersJson).Datas;
            var valid=new HashSet<int>();foreach(var c in configs)valid.Add(c.id);
            var held=new Dictionary<int,OutgameCommanderState>();
            foreach(var c in profile.commanders)if(valid.Contains(c.id))held.Add(c.id,c);
            foreach(var config in configs)
            {
                if(held.ContainsKey(config.id))continue;
                var state=new OutgameCommanderState{id=config.id,level=0,skillIds=(int[])config.skills.Clone(),skillLevels=new int[config.skills.Length]};
                for(int i=0;i<state.skillLevels.Length;i++)state.skillLevels[i]=1;
                profile.commanders.Add(state);held.Add(state.id,state);
            }
        }
    }
    // An explicit path keeps reconstruction saves separate from the user's existing battle profile.
    // Write a complete temporary file before replacing the committed generation.
    public sealed class OutgameProfileStore
    {
        readonly string path;
        public OutgameProfileStore(string path){this.path=Path.GetFullPath(path);}
        public OutgameProfile Load()
        {
            if(!File.Exists(path))return new OutgameProfile();
            var profile=JsonUtility.FromJson<OutgameProfile>(File.ReadAllText(path,Encoding.UTF8));
            Validate(profile);return profile;
        }
        static void Validate(OutgameProfile profile)
        {
            if(profile==null||profile.schemaVersion!=1||profile.inventory==null||profile.commanders==null)
                throw new InvalidDataException("Invalid outgame save; existing file was preserved.");
        }
        public void Save(OutgameProfile profile)
        {
            Validate(profile);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary=path+".pending";
            byte[] bytes=new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(profile,true));
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None))
            {stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            if(File.Exists(path))File.Replace(temporary,path,path+".backup");
            else File.Move(temporary,path);
        }
    }
}
