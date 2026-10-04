using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public abstract class OutgameActivityConfigReader
    {
        public Action CompleteAction;
        public virtual void OnInit(Action complete){CompleteAction=complete;}
        public abstract void OnDispose();
    }
    public sealed class OutgameActivityConfigReaderServices
    {
        public Func<OutgameActivityConfigManager> Current;
        public Func<string,string> OnlineParameter;
        public Func<bool> UseBinary;
        public Action<Dictionary<object,OutgameActivityConfigRow>> ReadLocalActivities,ReadBinaryActivities;
        public Func<OutgameActivitySettingConfig> ReadLocalSettings,ReadBinarySettings;
        public Action<object[]> Error;
    }
    // pLdP4633 and shared generic35194: online string overrides only activity rows.
    public sealed class OutgameActivityLocalConfigReader:OutgameActivityConfigReader
    {
        readonly OutgameActivityConfigReaderServices services;
        [Serializable] sealed class Rows{public List<OutgameActivityConfigRow> Datas;}
        public OutgameActivityLocalConfigReader(OutgameActivityConfigReaderServices services){this.services=services;}
        public override void OnInit(Action complete)
        {
            CompleteAction=complete;
            string online=services.OnlineParameter("common_ActivityConfig");
            if(string.IsNullOrEmpty(online))
            {
                bool binary=services.UseBinary();var dictionary=services.Current().Activities;
                if(binary)services.ReadBinaryActivities(dictionary);else services.ReadLocalActivities(dictionary);
            }
            else ReadOnline(services.Current().Activities,online,services.Error);
            bool binarySettings=services.UseBinary();var owner=services.Current();
            owner.Settings=binarySettings?services.ReadBinarySettings():services.ReadLocalSettings();
            complete?.Invoke();
        }
        public override void OnDispose(){}
        public static void ReadOnline(Dictionary<object,OutgameActivityConfigRow> destination,string text,Action<object[]> error)
        {
            var rows=JsonUtility.FromJson<Rows>(text.Substring(text.IndexOf("{",StringComparison.Ordinal))).Datas;
            if(rows==null)return;
            for(int i=0;i<rows.Count;i++)
            {
                if(destination.ContainsKey(rows[i].id))error(new object[]{string.Format("表[{0}]中有相同键({1})","PubActivityConfig",rows[i].id)});
                else destination.Add(rows[i].id,rows[i]);
            }
        }
    }
    // Explicit ConfigRead26610/26614 Unity JSON adapter for the recovered source names.
    public sealed class OutgameActivityConfigUnityJson
    {
        readonly Func<string,TextAsset> load;readonly Action<string> error;
        public int ReadCount{get;private set;}
        public OutgameActivityConfigUnityJson(Func<string,TextAsset> load,Action<string> error){this.load=load;this.error=error;}
        public void ReadActivities(Dictionary<object,OutgameActivityConfigRow> destination)
        {
            const string name="PubActivityConfig";var asset=load(name);
            if(asset!=null)OutgameActivityLocalConfigReader.ReadOnline(destination,asset.text,args=>error((string)args[0]));
            else error("配置文件不存在"+name);
            ReadCount++;
        }
        public OutgameActivitySettingConfig ReadSettings()
        {
            const string name="PubActivitySettingConfig";var asset=load(name);
            if(asset!=null)return JsonUtility.FromJson<OutgameActivitySettingConfig>(asset.text.Substring(asset.text.IndexOf("{",StringComparison.Ordinal)));
            error("配置文件不存在"+name);return null;
        }
    }
}
