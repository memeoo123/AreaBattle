using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameConfigRow { object UniqueID {get;} }
    // ConfigRead26610/26614/26619/26626(type1)/26629. One explicit legacy-route state.
    public sealed class OutgameLegacyConfigRead
    {
        [Serializable] sealed class Rows<T> {public List<T> Datas;}
        readonly Func<string,TextAsset> load;readonly Action<string> error;
        public int ReadCount {get;private set;}
        public OutgameLegacyConfigRead(Func<string,TextAsset> load,Action<string> error)
        {this.load=load;this.error=error;}
        public void ReadTable<T>(Dictionary<object,T> destination) where T:class,IOutgameConfigRow
        {
            string name=typeof(T).Name;var asset=load(name);
            if(asset!=null){
                var rows=JsonUtility.FromJson<Rows<T>>(FromFirstBrace(asset.text)).Datas;
                if(rows!=null)for(int i=0;i<rows.Count;i++){
                    if(destination.ContainsKey(rows[i].UniqueID))error(string.Format("表[{0}]中有相同键({1})",name,rows[i].UniqueID));
                    else destination.Add(rows[i].UniqueID,rows[i]);
                }
            }else error("配置文件不存在"+name);
            ReadCount++;
        }
        public T ReadValue<T>() where T:class
        {
            string name=typeof(T).Name;var asset=load(name);
            if(asset!=null)return JsonUtility.FromJson<T>(FromFirstBrace(asset.text));
            error("配置文件不存在"+name);return default;
        }
        static string FromFirstBrace(string text)=>text.Substring(text.IndexOf("{",StringComparison.Ordinal));
    }
}
