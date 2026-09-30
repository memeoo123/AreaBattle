using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // VideoBtnManager3843: lazy table, first duplicate wins, reset only invalidates initialized flag.
    public sealed class OutgameVideoButtonManager
    {
        readonly Func<bool> hasBundle,readWithoutBundle;
        readonly Func<List<OutgameVideoButtonData>> readConfig;
        readonly Action<string> log,error,releaseLog;
        Dictionary<int,OutgameVideoButtonData> rows=new Dictionary<int,OutgameVideoButtonData>();
        public bool Initialized {get;private set;}
        public OutgameVideoButtonManager(Func<bool> hasBundle,Func<bool> readWithoutBundle,Func<List<OutgameVideoButtonData>> readConfig,Action<string> log,Action<string> error,Action<string> releaseLog)
        {this.hasBundle=hasBundle;this.readWithoutBundle=readWithoutBundle;this.readConfig=readConfig;this.log=log;this.error=error;this.releaseLog=releaseLog;}
        public void Reset()=>Initialized=false;
        void EnsureLoaded()
        {
            if(Initialized)return;
            if(!hasBundle()&&!readWithoutBundle()){error("videoBtn:配置文件AB资源为null");return;}
            log("videoBtn:读取视频按钮数据");
            rows=new Dictionary<int,OutgameVideoButtonData>();
            var data=readConfig();
            if(data!=null)foreach(var item in data)
            {
                if(rows.ContainsKey(item.id)){error(string.Format("表[视频表]中有相同键({0})",item.id));continue;}
                rows.Add(item.id,item);
            }
            Initialized=true;
        }
        public OutgameVideoButtonData GetData(int id)
        {
            EnsureLoaded();
            if(!rows.TryGetValue(id,out var value))releaseLog(string.Format("未找到视频按钮数据===[{0}]",id));
            return value;
        }
        [Serializable] sealed class Table {public List<OutgameVideoButtonData> Datas;}
        // ConfigRead26619 / JsonUtility.FromJson<Table<T>>. Preserve source field-name matching.
        public static List<OutgameVideoButtonData> ParseJson(string text)
        {return JsonUtility.FromJson<Table>(text.Substring(text.IndexOf("{",StringComparison.CurrentCulture))).Datas;}
        public static List<OutgameVideoButtonData> ReadRecoveredJson()
        {return ParseJson(Resources.Load<TextAsset>("Recovered/OutgameConfig/VideoBtnConfig").text);}
    }
}
