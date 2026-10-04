using System;
namespace AreaBattle
{
    // Source34584 ->34595 ->34574 ->34610 on the same GlobalItemManager.
    // GlobalItemSlot still publishes this lifecycle before reading configuration.
    public sealed class OutgameProductServices
    {
        public readonly OutgameGlobalItemLifecycle Lifecycle;
        public readonly OutgameProductPrices Prices;
        public readonly OutgameProductResets Resets;
        public OutgameProductServices(IOutgameGlobalItemLifecycleHost host,Func<OutgameItemConfigManager> config,
            Func<DateTime> localNow,Func<float> deltaTime,Func<float,float,float> power,
            Func<OutgameMessageDispatcher> messages,Action<object[]> warning,Action<object[]> error)
        {
            var provider=new OutgameProductConfigProvider(config);
            Prices=new OutgameProductPrices(provider.Price,power,(id,ex)=>error(new object[]{
                string.Format("商品表的gettype或priceCalParam字段配置有误，id{0}\r\n{1}\r\n{2}",id,ex.Message,ex.StackTrace)}));
            Lifecycle=new OutgameGlobalItemLifecycle(host,id=>config().Products.ContainsKey(id),
                id=>config().Items.ContainsKey(id),localNow,deltaTime,provider.Refresh,
                id=>Resets.Reset(id),()=>Lifecycle.RewardHost.Update(),error);
            Resets=new OutgameProductResets(Lifecycle.Indexes,provider.Refresh,Prices.Update,
                (name,args)=>messages().SendMessage(name,args),warning);
        }
    }
}
