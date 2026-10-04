using System;
using System.Globalization;
namespace AreaBattle
{
    // Original value type Condition4643; args contain boxed Int32 values, target is Int64.
    public struct OutgameActivityCondition
    {
        public int key;
        public long value;
        public object[] arg;
    }
    public sealed partial class OutgameActivityItemData
    {
        // Original lookup is through the application-domain ActivityConfigMgr singleton.
        // The production config owner must bind this; there is no fallback catalog.
        public static Func<int,OutgameActivityConfigRow> ConfigurationResolver;
        OutgameActivityConfigRow cachedConfig;
        OutgameActivityCondition[] noticeCondition,launchCondition,overCondition,closeCondition;
        public OutgameActivityConfigRow Config
        {get{if(cachedConfig==null)cachedConfig=ConfigurationResolver(id);return cachedConfig;}}
        public OutgameActivityCondition[] NoticeCondition=>ReadConditions(ref noticeCondition,0);
        public OutgameActivityCondition[] LaunchCondition=>ReadConditions(ref launchCondition,1);
        public OutgameActivityCondition[] OverCondition=>ReadConditions(ref overCondition,2);
        public OutgameActivityCondition[] CloseCondition=>ReadConditions(ref closeCondition,3);
        static int[] Types(OutgameActivityConfigRow config,int kind)
        {switch(kind){case 0:return config.noticeType;case 1:return config.launchType;case 2:return config.overType;default:return config.closeType;}}
        static string[] Parameters(OutgameActivityConfigRow config,int kind)
        {switch(kind){case 0:return config.noticeParams;case 1:return config.launchParams;case 2:return config.overParams;default:return config.closeParams;}}
        OutgameActivityCondition[] ReadConditions(ref OutgameActivityCondition[] cache,int kind)
        {
            if(cache!=null)return cache;
            cache=new OutgameActivityCondition[Parameters(Config,kind).Length];
            for(int i=0;i<Parameters(Config,kind).Length;i++)
            {
                cache[i].key=Types(Config,kind)[i];
                int type=Types(Config,kind)[i];string text=Parameters(Config,kind)[i];
                if(type==10000)
                {
                    cache[i].value=DateTime.TryParseExact(text,"yyyyMMddHHmmss",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)
                        ?OutgameItemTimestamp.FromDateTime(date):0;
                    continue;
                }
                string[] parts=text.Split('_');
                if(parts.Length<1)continue;
                cache[i].arg=new object[parts.Length-1];
                for(int j=0;j<parts.Length-1;j++){int.TryParse(parts[j],out int argument);cache[i].arg[j]=argument;}
                long.TryParse(parts[parts.Length-1],out cache[i].value);
            }
            return cache;
        }
    }
    public static class OutgameActivityConditions
    {
        // ActivityStateBase35369: config gate first, supplied type IDs (not cached keys), live target after provider.
        public static bool Met(OutgameActivityConfigRow config,int[] types,OutgameActivityCondition[] conditions,OutgameStatisticsExpansion statistics)
        {
            if(config.open==0)return false;
            for(int i=0;i<types.Length;i++)
            {
                long actual=statistics.GameValue(types[i],conditions[i].arg);
                if(actual<conditions[i].value)return false;
            }
            return true;
        }
    }
}
