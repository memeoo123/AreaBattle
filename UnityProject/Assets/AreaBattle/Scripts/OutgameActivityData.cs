using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // ActivityData4641 / persisted fields of ActivityItemData4642. Runtime condition caches are separate.
    [Serializable] public sealed class OutgameActivityData
    {
        public int firstLoginDay;
        public long lastRefreshTimeStamp;
        public List<OutgameActivityItemData> datas=new List<OutgameActivityItemData>();
    }
    [Serializable] public sealed partial class OutgameActivityItemData
    {
        public int id,state=1;
        public bool noticePop,launchPop;
        public string uniqueId;
        public long LaunchTimeStamp,WarmTimeStamp;
    }
}
