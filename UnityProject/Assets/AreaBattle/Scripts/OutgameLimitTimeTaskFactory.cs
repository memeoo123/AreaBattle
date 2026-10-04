using System;
namespace AreaBattle
{
    // Source ActivityVirtualItemBase4653 delegates directly to ItemBase4541.
    public class OutgameActivityVirtualItemBase:OutgameItemBase
    {
        public OutgameActivityVirtualItemBase(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count)=>base.AddItem(count);
        public override void Use(long count)=>base.Use(count);
    }
    // LimitTimeTaskFactory4708 only produces type1=2/type2=4 liveness points.
    public sealed class OutgameLimitTimeTaskFactory:OutgameActivityFactory
    {
        readonly OutgameItemEntityServices entities;readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameLimitTimeTaskFactory(int id,Func<OutgameItemConfigManager> config,Action<string> error,OutgameItemEntityServices entities,Func<OutgameCommonMessageDispatcher> common)
            :base(id,config,error){this.entities=entities;this.common=common;}
        public override IOutgameItemEntity Produce()
        {
            var config=ItemConfig;
            return config.type1==2&&config.type2==4?new OutgameLimitTimeTaskLivenessPoint(config.id,entities,common):null;
        }
    }
    public sealed class OutgameLimitTimeTaskLivenessPoint:OutgameActivityVirtualItemBase
    {
        readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameLimitTimeTaskLivenessPoint(int id,OutgameItemEntityServices services,Func<OutgameCommonMessageDispatcher> common):base(id,services){this.common=common;}
        public override void AddItem(long count)=>base.AddItem(count);
        public override void Use(long count)=>base.Use(count);
        public override void AddItemOnlyModel(long count)
        {
            base.AddItemOnlyModel(count);
            string key="CommonGameModule_LimitTimeTask_LivenessPointAdd"+ItemConfig.paramInt.ToString();
            var args=new object[]{unchecked((int)count)};common().SendMessage(key,args);
        }
    }
}
