using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public class OutgameProductPriceConfig
    {
        public virtual int[] buyTypeOrder {get;set;}
        public virtual int[] costItemPriceTypes {get;set;}
        public virtual int[] priceCalParam {get;set;}
        public int[][] costItemPriceParams;
        public virtual int getType {get;set;}
        public virtual int priceType {get;set;}
        public virtual int[] GetCostItemPriceParameters(int index)=>costItemPriceParams[index];
    }
    // Source ProductUserData.UpdateConsumeItemPrice34610. Random/power providers are
    // explicit dependencies so platform-specific sequence/rounding is not invented.
    public sealed class OutgameProductPrices
    {
        readonly Func<int,OutgameProductPriceConfig> config;
        readonly Func<int,int,int> randomInclusive;
        readonly Func<List<int>,int> randomElement;
        readonly Func<float,float,float> power;
        readonly Action<int,Exception> reportError;
        public OutgameProductPrices(Func<int,OutgameProductPriceConfig> config,Func<int,int,int> randomInclusive,Func<List<int>,int> randomElement,Func<float,float,float> power,Action<int,Exception> reportError)
        {this.config=config;this.randomInclusive=randomInclusive;this.randomElement=randomElement;this.power=power;this.reportError=reportError;}
        public OutgameProductPrices(Func<int,OutgameProductPriceConfig> config,Func<float,float,float> power,Action<int,Exception> reportError)
            :this(config,GameRandomSource.Shared.Inclusive,GameRandomSource.Shared.Element,power,reportError){}
        static int ItemCount(OutgameProductUserData row,int item)
        {
            OutgameProductBuyCount found=null;
            foreach(var entry in row.HaveBuyCounts)if(entry.key==1){found=entry;break;}
            if(found==null)found=new OutgameProductBuyCount{key=1};
            foreach(var entry in found.buyCount)if(entry.key==item)return entry.value;
            return 0;
        }
        static int TruncatePower(float value)=>Math.Abs((double)value)<2147483648.0?(int)value:int.MinValue;
        int Pick(int[] values){var candidates=new List<int>(values);candidates.RemoveAt(0);return randomElement(candidates);}
        public void Update(OutgameProductUserData row)
        {
            var initial=config(row.productId);var indexes=new List<int>();
            if(initial.buyTypeOrder==null)initial.buyTypeOrder=new[]{initial.getType};
            for(int i=0;i<config(row.productId).buyTypeOrder.Length;i++)
                if(config(row.productId).buyTypeOrder[i]==1)indexes.Add(i);
            if(row.consumeItemPriceArray==null)row.consumeItemPriceArray=new long[indexes.Count];
            for(int i=0;i<indexes.Count;i++)
            {
                // Source reads indexes[i] and discards it; compact i selects price parameters.
                try
                {
                    int type=config(row.productId).costItemPriceTypes[i];
                    switch(type)
                    {
                        case 0:row.consumeItemPriceArray[i]=config(row.productId).GetCostItemPriceParameters(i)[1];break;
                        case 1:row.consumeItemPriceArray[i]=randomInclusive(config(row.productId).GetCostItemPriceParameters(i)[1],config(row.productId).GetCostItemPriceParameters(i)[2]);break;
                        case 2:
                            int count=ItemCount(row,config(row.productId).GetCostItemPriceParameters(i)[0]);
                            int at=count%(config(row.productId).GetCostItemPriceParameters(i).Length-1)+1;
                            row.consumeItemPriceArray[i]=config(row.productId).GetCostItemPriceParameters(i)[at];break;
                        case 3:row.consumeItemPriceArray[i]=Pick(config(row.productId).GetCostItemPriceParameters(i));break;
                        case 4:
                            int basis=config(row.productId).GetCostItemPriceParameters(i)[1];
                            int bought=ItemCount(row,config(row.productId).GetCostItemPriceParameters(i)[0]);
                            row.consumeItemPriceArray[i]=unchecked(basis+config(row.productId).GetCostItemPriceParameters(i)[2]*bought);break;
                        case 5:
                            int start=config(row.productId).GetCostItemPriceParameters(i)[1];
                            int factor=config(row.productId).GetCostItemPriceParameters(i)[2];
                            int exponent=ItemCount(row,config(row.productId).GetCostItemPriceParameters(i)[0]);
                            row.consumeItemPriceArray[i]=unchecked(start*TruncatePower(power(factor,exponent)));break;
                    }
                }
                catch(Exception exception){reportError(row.productId,exception);}
            }
            if(initial.priceCalParam==null||initial.priceCalParam.Length==0){row.consumeItemPrice=0;return;}
            if(initial.getType!=1)return;
            try
            {
                var values=initial.priceCalParam;
                switch(initial.priceType)
                {
                    case 0:row.consumeItemPrice=values[1];break;
                    case 1:row.consumeItemPrice=randomInclusive(values[1],values[2]);break;
                    case 2:row.consumeItemPrice=values[row.haveBuyTimes%(values.Length-1)+1];break;
                    case 3:row.consumeItemPrice=Pick(values);break;
                    case 4:row.consumeItemPrice=unchecked(values[1]+row.haveBuyTimes*values[2]);break;
                    case 5:row.consumeItemPrice=unchecked(values[1]*TruncatePower(power(values[2],row.haveBuyTimes)));break;
                }
            }
            catch(Exception exception){reportError(row.productId,exception);}
        }
    }
}
