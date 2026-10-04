using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
namespace AreaBattle
{
    // The activity runtime supplies the actual ActivityControl owner for source activity1601001.
    public interface IOutgameAchievementActivity {OutgameAchievementExt Ext{get;}}
    public sealed class OutgameAchievementModelServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<int,IOutgameAchievementActivity> GetActivity;
    }
    public static class OutgameAchievementModels {public static OutgameAchievementModelServices Services;}
    // Original4715..4719. SaveData is the compact claimed-record schema; Data is runtime/legacy.
    [Serializable] public sealed class OutgameAchievementSaveData
    {
        public OutgameAchievementExt ext=new OutgameAchievementExt();
        public List<int> ids=new List<int>();
        public List<long> times=new List<long>();
    }
    [Serializable] public sealed class OutgameAchievementData
    {
        public OutgameAchievementExt ext=new OutgameAchievementExt();
        public List<OutgameAchievementItemData> datas=new List<OutgameAchievementItemData>();
    }
    [Serializable] public sealed class OutgameAchievementExt {public long statePts;public int accPts;}
    [Serializable] public sealed class OutgameAchievementItemData:IOutgameSelectData,IComparable<OutgameAchievementItemData>
    {
        public int id;public long progress;public int state;public long time;
        PubAchievementConfig config;List<OutgameActivityCondition> showConditions;
        public bool Selected{get;set;}
        public PubAchievementConfig Config=>config??(config=OutgameAchievementModels.Services.Config().GetAchievementConfig(id));
        public bool CanComplete(){var row=Config;return progress>=row.number&&state==0;}
        // Source35632 creates a fresh reward list on every call, unlike ordinary-task tiers.
        public List<OutgameItemReward> GetRewards()
        {
            var result=new List<OutgameItemReward>();var row=Config;
            for(int i=0;i<row.rewards.Count;i++)
            {int item=row.rewards[i].datas[0],count=row.rewards[i].datas[1];result.Add(new OutgameItemReward{itemId=item,itemCount=count});}
            return result;
        }
        public List<OutgameActivityCondition> ShowCondition
        {
            get
            {
                if(showConditions==null)
                {
                    showConditions=new List<OutgameActivityCondition>();var row=Config;
                    for(int i=0;i<Config.showCondition.Count;i++)
                    {
                        var condition=new OutgameActivityCondition();condition.arg=new object[row.showCondition[i].datas.Length-2];
                        condition.key=row.showCondition[i].datas[0];condition.value=row.showCondition[i].datas[row.showCondition[i].datas.Length-1];
                        Array.Copy(row.showCondition[i].datas,1,condition.arg,0,row.showCondition[i].datas.Length-2);showConditions.Add(condition);
                    }
                }
                return showConditions;
            }
        }
        public int CompareTo(OutgameAchievementItemData other)
        {
            bool ready=CanComplete(),otherReady=other.CanComplete();
            if(state==0)
            {if(ready){if(!(other.state==0&&otherReady))return -1;}else if(other.state!=0||otherReady)return other.state!=0?-1:1;}
            else if(other.state==0)return 1;
            var a=Config;var b=other.Config;return a.id<b.id?-1:a.id>b.id?1:0;
        }
    }
    [Serializable] public sealed class OutgameAccAchievementItemData:IComparable<OutgameAccAchievementItemData>
    {
        public int index,acc;PubAchievementAccConfig config;
        public PubAchievementAccConfig Config=>config??(config=OutgameAchievementModels.Services.Config().GetAchievementAccConfig(acc));
        public int State=>(int)((ulong)OutgameAchievementModels.Services.GetActivity(1601001).Ext.statePts>>(index&63))&1;
        public bool CanComplete()=>OutgameAchievementModels.Services.GetActivity(1601001).Ext.accPts>=acc&&State==0;
        public int CompareTo(OutgameAccAchievementItemData other)
        {
            bool ready=CanComplete(),otherReady=other.CanComplete();int state=State,otherState=other.State;
            if(state==0)
            {if(ready){if(!(otherState==0&&otherReady))return -1;}else if(otherState!=0||otherReady)return otherState!=0?-1:1;}
            else if(otherState==0)return 1;
            var a=Config;var b=other.Config;return a.id<b.id?-1:a.id>b.id?1:0;
        }
    }
}
