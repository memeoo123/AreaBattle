using System;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    // Resolve the current shared ItemConfigMgr4534, not the project product table.
    public sealed class OutgameProductConfigProvider
    {
        readonly Func<OutgameItemConfigManager> config;
        public OutgameProductConfigProvider(Func<OutgameItemConfigManager> config){this.config=config;}
        public OutgameProductRefreshConfig Refresh(int id)
        {
            var row=config().GetGameProductConfig(id);
            return row==null?null:new OutgameProductRefreshConfig {
                id=row.id,buyLimit=row.buyLimit,buyLimitParam=row.buyLimitParam,refreshPeriod=row.refreshPeriod
            };
        }
        public OutgameProductPriceConfig Price(int id)
        {
            var row=config().GetGameProductConfig(id);
            return row==null?null:new PriceRow(row);
        }
        sealed class PriceRow:OutgameProductPriceConfig
        {
            readonly GameProductConfig row;
            public PriceRow(GameProductConfig row){this.row=row;}
            public override int[] buyTypeOrder {get=>row.buyTypeOrder;set=>row.buyTypeOrder=value;}
            public override int[] costItemPriceTypes {get=>row.costItemPriceTypes;set=>row.costItemPriceTypes=value;}
            public override int[] priceCalParam {get=>row.priceCalParam;set=>row.priceCalParam=value;}
            public override int getType {get=>row.getType;set=>row.getType=value;}
            public override int priceType {get=>row.priceType;set=>row.priceType=value;}
            // Eager flattening moves null-entry errors outside the original price catch.
            public override int[] GetCostItemPriceParameters(int index)=>row.costItemPriceParams[index].datas;
        }
    }
}
