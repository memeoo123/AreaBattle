using System;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    public interface IOutgameItemEntity {void AddItem(long count);void AddItemOnlyModel(long count);}
    // FactoryBase4486, VirtualItem_Factory4488 and PackageItem_Factory4487.
    // Entity constructors must be supplied explicitly; no generic inventory fallback.
    public class OutgameItemFactory
    {
        protected readonly Func<OutgameItemConfigManager> current;
        protected readonly Action<string> error;
        protected readonly Func<int,int,IOutgameItemEntity> construct;
        public GameItemConfig ItemConfig {get;private set;}
        public OutgameItemFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error,Func<int,int,IOutgameItemEntity> construct)
        {
            this.current=current;this.error=error;this.construct=construct;
            if(current().Items.ContainsKey(id))ItemConfig=current().Items[id];
            else error("无法创建当前道具 id :"+id);
        }
        public virtual IOutgameItemEntity Produce()
        {
            if(ItemConfig.type1==1)return new OutgameVirtualItemFactory(ItemConfig.id,current,error,construct).Produce();
            if(ItemConfig.type1==9)return new OutgamePackageItemFactory(ItemConfig.id,current,error,construct).Produce();
            return null;
        }
    }
    public sealed class OutgameVirtualItemFactory:OutgameItemFactory
    {
        public OutgameVirtualItemFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error,Func<int,int,IOutgameItemEntity> construct):base(id,current,error,construct){}
        public override IOutgameItemEntity Produce()
        {
            int sourceType;
            switch(ItemConfig.type2){case 1:sourceType=4496;break;case 2:sourceType=4494;break;case 4:sourceType=4499;break;case 5:sourceType=4493;break;case 8:sourceType=4498;break;case 10:sourceType=4497;break;case 11:sourceType=4495;break;default:return null;}
            return construct(sourceType,ItemConfig.id);
        }
    }
    public sealed class OutgamePackageItemFactory:OutgameItemFactory
    {
        public OutgamePackageItemFactory(int id,Func<OutgameItemConfigManager> current,Action<string> error,Func<int,int,IOutgameItemEntity> construct):base(id,current,error,construct){}
        public override IOutgameItemEntity Produce()
        {switch(ItemConfig.type2){case 1:return construct(4490,ItemConfig.id);case 2:return construct(4491,ItemConfig.id);default:return null;}}
    }
}
