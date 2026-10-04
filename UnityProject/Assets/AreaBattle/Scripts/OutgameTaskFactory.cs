using System;
namespace AreaBattle
{
    // TaskFactory4673 and TaskLivenessPoint4674, ordinary task category2/3.
    public sealed class OutgameTaskFactory:OutgameActivityFactory
    {
        readonly OutgameItemEntityServices entities;readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameTaskFactory(int id,Func<OutgameItemConfigManager> config,Action<string> error,OutgameItemEntityServices entities,Func<OutgameCommonMessageDispatcher> common)
            :base(id,config,error){this.entities=entities;this.common=common;}
        public override IOutgameItemEntity Produce()
        {var config=ItemConfig;return config.type1==2&&config.type2==3?new OutgameTaskLivenessPoint(config.id,entities,common):null;}
    }
    public sealed class OutgameTaskLivenessPoint:OutgameActivityVirtualItemBase
    {
        readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameTaskLivenessPoint(int id,OutgameItemEntityServices services,Func<OutgameCommonMessageDispatcher> common):base(id,services){this.common=common;}
        public override void AddItem(long count)=>base.AddItem(count);
        public override void Use(long count)=>base.Use(count);
        public override void AddItemOnlyModel(long count)
        {
            base.AddItemOnlyModel(count);
            string key="CommonGameModule_Task_LivenessPointAdd"+ItemConfig.paramInt.ToString();
            common().SendMessage(key,new object[]{unchecked((int)count)});
        }
    }
}
