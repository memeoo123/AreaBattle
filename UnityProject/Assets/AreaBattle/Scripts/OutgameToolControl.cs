using System;
using System.Collections.Generic;
using ListArrayInt=AreaBattle.OriginalConfig.ListArrayInt;
namespace AreaBattle
{
    // ToolControl4256 source façade. ToolChange32533 remains the recovered dispatcher;
    // current local inventory/item module/global purchase reason are resolved on each call.
    public sealed class OutgameToolControl:IOutgameLogicControl
    {
        readonly Func<OutgameToolDispatcher> dispatcher;
        readonly Func<OutgameLocalInventory> local;
        readonly Func<int,long> itemModuleCount;
        readonly Func<string> purchaseCost;
        public OutgameToolControl(Func<OutgameToolDispatcher> dispatcher,Func<OutgameLocalInventory> local,Func<int,long> itemModuleCount,Func<string> purchaseCost)
        {this.dispatcher=dispatcher;this.local=local;this.itemModuleCount=itemModuleCount;this.purchaseCost=purchaseCost;}
        public void OnInit(){} //32537 nop/end.
        public void Updata(float deltaTime,float unscaledDeltaTime){} //32534 nop/end.
        public void OnDispose(){} //32540 nop/end: source deliberately retains singleton.
        public bool ToolChange(int id,int delta,bool refreshTop,string reason,bool notifyAndReport)=>dispatcher().Change(id,delta,refreshTop,reason,notifyAndReport);
        public bool ToolChange(int id,int delta,bool refreshTop=true,string reason="")=>ToolChange(id,delta,refreshTop,reason,true);
        public void GetReward(List<ListArrayInt> rewards,bool refreshTop)
        {
            for(int i=0;i<rewards.Count;i++)
            {var row=rewards[i].datas;int id=row[0],amount=row[1];ToolChange(id,amount,refreshTop,purchaseCost(),true);}
        }
        public void GetMultiReward(List<ListArrayInt> rewards,int multiplier,bool refreshTop)
        {
            for(int i=0;i<rewards.Count;i++)
            {var row=rewards[i].datas;int id=row[0],amount=unchecked(multiplier*row[1]);ToolChange(id,amount,refreshTop,purchaseCost(),true);}
        }
        public int GetItemCountById(int id)=>local().Count(id);
        public int GetItemNum(int id)=>id>=8001?unchecked((int)itemModuleCount(id)):local().Count(id);
        public bool IsEnough(int id,int count)=>GetItemNum(id)>=count;
    }
}
