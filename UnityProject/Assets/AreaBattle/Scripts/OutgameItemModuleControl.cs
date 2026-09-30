using System;
using System.Collections.Generic;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    // ItemModuleControl3875: shared item configuration and manager, separate from local currency.
    public sealed class OutgameItemModuleControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;readonly Func<OutgameDataManagerPool> pool;
        readonly Func<OutgameItemConfigManager> config;readonly Func<OutgameMessageDispatcher> messages;readonly Action<string> warning;
        OutgameItemManager manager;
        public Dictionary<int,GameItemConfig> Props=new Dictionary<int,GameItemConfig>();
        public Dictionary<int,GameItemConfig> Skills=new Dictionary<int,GameItemConfig>();
        public Dictionary<int,GameItemConfig> Heroes=new Dictionary<int,GameItemConfig>();
        public Dictionary<object,GameItemConfig> Items;
        public OutgameItemModuleControl(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,Func<OutgameItemConfigManager> config,Func<OutgameMessageDispatcher> messages,Action<string> warning)
        {this.registry=registry;this.pool=pool;this.config=config;this.messages=messages;this.warning=warning;}
        public OutgameItemManager ItemMgr=>manager??(manager=pool().GetModel<OutgameItemManager>(4500,"ItemManager"));
        public void OnInit()=>InitMgr();
        public void InitMgr()
        {
            Items=config().Items;
            foreach(var row in config().Items.Values){switch(row.type1){case 4:Props.Add(row.id,row);break;case 5:Skills.Add(row.id,row);break;case 6:Heroes[row.id]=row;break;}}
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} //30184 source empty.
        public void OnDispose()=>registry.Clear(3875);
        public GameItemConfig GetItemConfig(int id)=>config().GetGameItemConfig(id);
        public long GetItemNum(int id)=>ItemMgr.GetItemNum(id);
        public bool ChangeItemNum(int id,int delta)
        {
            long next=unchecked(GetItemNum(id)+(long)delta);
            if(next<0){warning(string.Format("道具 {0} 数量不足!!!",id));return false;}
            ItemMgr.GetItem(GetItemConfig(id)).AddItem(delta);
            messages().SendMessage("Item_Change",new object[]{id});
            pool().SaveData();return next>=0;
        }
    }
}
