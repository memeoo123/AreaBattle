using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameLocalization
    {
        [Serializable] sealed class Table {public Row[] Datas;}
        [Serializable] sealed class Row {public string id,zh_cn;}
        readonly Dictionary<string,string> values=new Dictionary<string,string>();
        public OutgameLocalization(string sourceJson){foreach(var row in JsonUtility.FromJson<Table>(sourceJson).Datas)values.Add(row.id,row.zh_cn);}
        public string Chinese(string key)=>(values.TryGetValue(key,out var value)?value:key)?.Replace("\\n","\n");
    }
}
