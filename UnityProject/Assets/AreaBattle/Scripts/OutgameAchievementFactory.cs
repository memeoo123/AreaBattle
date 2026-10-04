using System;
namespace AreaBattle
{
    // Original AchievementFactory4628 and AchievementPoint4627, category2/2.
    public sealed class OutgameAchievementFactory:OutgameActivityFactory
    {
        readonly OutgameItemEntityServices entities;readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameAchievementFactory(int id,Func<OutgameItemConfigManager> config,Action<string> error,OutgameItemEntityServices entities,Func<OutgameCommonMessageDispatcher> common)
            :base(id,config,error){this.entities=entities;this.common=common;}
        public override IOutgameItemEntity Produce()
        {var config=ItemConfig;return config.type1==2&&config.type2==2?new OutgameAchievementPoint(config.id,entities,common):null;}
    }
    public sealed class OutgameAchievementPoint:OutgameActivityVirtualItemBase
    {
        public const string PointAdded="CommonModule_Achievement_PointAdd";
        readonly Func<OutgameCommonMessageDispatcher> common;
        public OutgameAchievementPoint(int id,OutgameItemEntityServices services,Func<OutgameCommonMessageDispatcher> common):base(id,services){this.common=common;}
        public override void Use(long count)=>base.Use(count);
        public override void AddItem(long count)
        {base.AddItem(count);common().SendMessage(PointAdded,new object[]{unchecked((int)count)});}
    }
}
