using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // LocalDataControl31593. Mutates existing records; source does not save here.
    public sealed class OutgameLegacyCommanderRepair
    {
        readonly Func<bool> isInstallVersion;
        readonly Func<OutgameLocalRecord> local;
        readonly Func<int,OutgameCommanderState> commander;
        readonly Func<IEnumerable<OutgameCommanderState>> commanders;
        readonly Func<int> upgradeEvent;
        readonly Action<int,int,long> setEventCount;
        readonly Func<int,object[],long> gameValue;
        readonly Action<string> warning;
        public OutgameLegacyCommanderRepair(Func<bool> isInstallVersion,Func<OutgameLocalRecord> local,Func<int,OutgameCommanderState> commander,Func<IEnumerable<OutgameCommanderState>> commanders,Func<int> upgradeEvent,Action<int,int,long> setEventCount,Func<int,object[],long> gameValue,Action<string> warning)
        {this.isInstallVersion=isInstallVersion;this.local=local;this.commander=commander;this.commanders=commanders;this.upgradeEvent=upgradeEvent;this.setEventCount=setEventCount;this.gameValue=gameValue;this.warning=warning;}
        public void Handle103VersionBug()
        {
            if(isInstallVersion())return;
            var record=local();if(record.dealOldComm==1)return;
            record.dealOldComm=1;warning("处理老用户数据 Handle1_03VersionBug");
            var firstCharge=local().firstChargeData;
            if(firstCharge.hasCharge&&(firstCharge.rewardState&1)!=0)
            {
                var held=commander(3);
                if(held.level<=0){held.level=1;setEventCount(upgradeEvent(),3,1);}
            }
            int eventKey=upgradeEvent();
            foreach(var held in commanders())
            {
                long previous=gameValue(eventKey,new object[]{held.id});
                if(previous<=held.level)continue;
                held.level=previous<0?0:(int)Math.Min((float)previous,28f);
                for(int i=0;i<held.level;i++)held.skillLevels[i%3]=1;
                for(int i=0;i<held.level-1;i++)held.skillLevels[i%3]++;
            }
        }
    }
}
