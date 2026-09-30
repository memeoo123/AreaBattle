using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameToolCount {public int id,count;}
    [Serializable] public sealed class OutgameLocalInventoryState
    {
        public int goldNum,diamondsNum,strengthsNum,ToolValue,collectNum,collectAdNum;
        public List<OutgameToolCount> toolCounts=new List<OutgameToolCount>();
    }
    // LocalDataManager f11482/f11484/f4958. Outer reward dispatch, event and save hooks belong to ToolControl.
    public sealed class OutgameLocalInventory
    {
        [Serializable] sealed class SkillRows {public SkillRow[] Datas;}
        [Serializable] sealed class SkillRow {public int gameItem;}
        readonly OutgameLocalInventoryState state;
        readonly Dictionary<int,OutgameToolCount> tools=new Dictionary<int,OutgameToolCount>();
        public OutgameLocalInventory(OutgameLocalInventoryState held,string allSkillJson)
        {
            state=held??throw new ArgumentNullException(nameof(held));
            foreach(var row in state.toolCounts)tools.Add(row.id,row);
            foreach(var skill in JsonUtility.FromJson<SkillRows>(allSkillJson).Datas)
            {
                if(tools.ContainsKey(skill.gameItem))continue;
                var row=new OutgameToolCount{id=skill.gameItem,count=2};
                tools.Add(row.id,row);state.toolCounts.Add(row);
            }
        }
        public int Count(int id)
        {
            switch(id)
            {
                case 1001:return state.goldNum;
                case 1002:return state.diamondsNum;
                case 1004:return state.strengthsNum;
                case 1005:return state.ToolValue;
                default:return tools.TryGetValue(id,out var row)?row.count:0;
            }
        }
        // Source i32.add wraps before checking the result's sign; no invented clamp or partial debit.
        public bool Change(int id,int delta)
        {
            int next=unchecked(Count(id)+delta);
            if(next<0)return false;
            switch(id)
            {
                case 1001:state.goldNum=next;break;
                case 1002:state.diamondsNum=next;break;
                case 1004:state.strengthsNum=next;break;
                case 1005:state.ToolValue=next;break;
                default:
                    if(!tools.TryGetValue(id,out var row))return false;
                    row.count=next;break;
            }
            return true;
        }
    }
}
