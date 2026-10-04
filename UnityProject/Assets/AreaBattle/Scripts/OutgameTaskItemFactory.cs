using System;
namespace AreaBattle
{
    // ChildTaskActivity35398 allocates TaskItemFactory4669 (usage3939868), not TaskFactory4673.
    public sealed class OutgameTaskItemFactory:OutgameActivityFactory
    {
        readonly OutgameItemEntityServices entities;
        public OutgameTaskItemFactory(int id,Func<OutgameItemConfigManager> config,Action<string> error,OutgameItemEntityServices entities)
            :base(id,config,error){this.entities=entities;}
        public override IOutgameItemEntity Produce()
        {var config=ItemConfig;return config.type1==10&&config.type2==4?new OutgameTaskIapRefreshItem(config.id,entities):null;}
    }
    // Source4670: regular Add invokes base award then virtual Use. No implicit task refresh/debit.
    public class OutgameTaskIapRefreshItem:OutgameActivityVirtualItemBase
    {
        public OutgameTaskIapRefreshItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count){base.AddItem(count);Use(count);}
        public override void Use(long count)=>base.Use(count);
    }
}
