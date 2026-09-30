using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameToolEffects
    {
        void RefreshTopInfo();
        void GoldSpent(int itemId,long amount);
        void ToolChanged(int itemId);
        void UnlockScene(int id);
        void UnlockSoldier(int id);
        bool ApplyItemEntity(int id,long delta);
        void MissingItemEntity(int id);
        void ReportGet(int id,int category,int amount,int balance,string reason);
        void ReportCost(int id,int category,int amount,int balance);
        void Save();
    }
    // ItemHelper.GetGoodsType f2585 and ToolControl.ToolChange f1799.
    public sealed class OutgameToolDispatcher
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;}
        readonly HashSet<int> items=new HashSet<int>(),scenes=new HashSet<int>(),soldiers=new HashSet<int>();
        readonly OutgameLocalInventory inventory;
        readonly IOutgameToolEffects effects;
        static void ReadIds(string json,HashSet<int> target){foreach(var r in JsonUtility.FromJson<Rows>(json).Datas)target.Add(r.id);}
        public OutgameToolDispatcher(OutgameLocalInventory inventory,IOutgameToolEffects effects,string gameItems,string sceneSkins,string soldierSkins)
        {
            this.inventory=inventory??throw new ArgumentNullException(nameof(inventory));this.effects=effects??throw new ArgumentNullException(nameof(effects));
            ReadIds(gameItems,items);ReadIds(sceneSkins,scenes);ReadIds(soldierSkins,soldiers);
        }
        public int GoodsType(int id)
        {
            if(id==1001)return 1;if(id==1002)return 2;if(id==1005)return 7;
            if(id>=2001&&id<=2999)return 3;
            if(id>=100&&id<=399)return 4;
            if(id>0&&id<100)return 5;
            return items.Contains(id)?6:0;
        }
        public bool Change(int id,int delta,bool refreshTop=true,string reason="",bool notifyAndReport=true)
        {
            bool accepted=true;int kind=GoodsType(id);
            switch(kind)
            {
                case 1:
                    accepted=inventory.Change(id,delta);
                    if(refreshTop)effects.RefreshTopInfo();
                    if(delta<0&&accepted)effects.GoldSpent(id,unchecked(-delta));
                    break;
                case 2:accepted=inventory.Change(id,delta);if(refreshTop)effects.RefreshTopInfo();break;
                case 3:accepted=inventory.Change(id,delta);effects.ToolChanged(id);break;
                case 4:if(soldiers.Contains(id))effects.UnlockSoldier(id);break;
                case 5:
                    if(!scenes.Contains(id))throw new KeyNotFoundException("Original scene skin configuration missing: "+id);
                    effects.UnlockScene(id);break;
                case 6:if(!effects.ApplyItemEntity(id,delta))effects.MissingItemEntity(id);break;
                case 7:accepted=inventory.Change(1005,delta);break;
            }
            if(kind!=0&&notifyAndReport)
            {
                effects.ToolChanged(id);
                int category=id==1001||id==1002?1:3;
                if(delta>0)effects.ReportGet(id,category,delta,inventory.Count(id),reason);
                else if(delta<0&&accepted)effects.ReportCost(id,category,unchecked(-delta),inventory.Count(id));
            }
            effects.Save();return accepted;
        }
    }
}
