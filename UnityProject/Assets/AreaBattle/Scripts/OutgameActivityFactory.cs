using System;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    public class OutgameActivityFactory
    {
        protected readonly Func<OutgameItemConfigManager> current;protected readonly Action<string> error;
        public GameItemConfig ItemConfig{get;private set;}
        public OutgameActivityFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error)
        {
            this.current=current;this.error=error;
            if(current().Items.ContainsKey(id))ItemConfig=current().Items[id];else error("无法创建当前道具 id :"+id);
        }
        public virtual IOutgameItemEntity Produce()
        {
            switch(ItemConfig.type1)
            {
                case 1:return new OutgameActivityCurrencyFactory(ItemConfig.id,current,error).Produce();
                case 2:return new OutgameActivityExpFactory(ItemConfig.id,current,error).Produce();
                case 9:return new OutgameActivityPackageFactory(ItemConfig.id,current,error).Produce();
                default:return null;
            }
        }
    }
    // Original35282/35284/35290 explicitly return null. Concrete activity-specific
    // factories may override these; this base does not invent reward entities.
    public sealed class OutgameActivityCurrencyFactory:OutgameActivityFactory
    {public OutgameActivityCurrencyFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error):base(id,current,error){}public override IOutgameItemEntity Produce()=>null;}
    public sealed class OutgameActivityExpFactory:OutgameActivityFactory
    {public OutgameActivityExpFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error):base(id,current,error){}public override IOutgameItemEntity Produce()=>null;}
    public sealed class OutgameActivityPackageFactory:OutgameActivityFactory
    {public OutgameActivityPackageFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error):base(id,current,error){}public override IOutgameItemEntity Produce()=>null;}
}
