using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
namespace AreaBattle
{
    public interface IOutgameLimitTimeTaskChild
    {
        OutgameLimitTimeTaskChildData ModuleData{get;}
        SortedDictionary<int,List<OutgameLimitTaskItemData>> DayTasks{get;}
    }
    public interface IOutgameLimitTimeTaskActivity:IOutgameNoviceTaskActivity
    {IOutgameLimitTimeTaskChild GetChild(int id);}
    public sealed class OutgameLimitTaskModelServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<int,IOutgameLimitTimeTaskActivity> GetActivity;
        public Func<OutgameItemConfigManager> Items;
        public OutgameStatisticsExpansion Statistics;
        public Action<object[]> Error;
    }
    // Source models resolve application-domain config/control/statistics singletons.
    // The concrete activity runtime must bind these owners; no fallback data or rewards.
    public static class OutgameLimitTaskModels
    {
        public static OutgameLimitTaskModelServices Services;
        internal static int ClaimedCount(SortedDictionary<int,List<OutgameLimitTaskItemData>> days)
        {
            int count=0;
            foreach(var tasks in days.Values)for(int i=0;i<tasks.Count;i++)if(tasks[i].state==1)count=unchecked(count+1);
            return count;
        }
    }
    public sealed partial class OutgameLimitTaskItemData:IComparable<OutgameLimitTaskItemData>,IOutgameSelectData
    {
        PubnoviceTaskConfig cachedConfig;
        List<OutgameActivityCondition> showConditions;
        List<OutgameItemReward> rewards;
        public bool Selected{get;set;}
        public int ActivityId
        {
            get
            {
                foreach(var pair in OutgameLimitTaskModels.Services.Config().NoviceTasksByActivity)
                    if(pair.Value.ContainsKey(id))return pair.Key;
                OutgameLimitTaskModels.Services.Error(new object[]{string.Format("未找到该任务对应的活动id，请检查任务id:{0}",id)});return 0;
            }
        }
        public IOutgameLimitTimeTaskChild ChildActivity=>OutgameLimitTaskModels.Services.GetActivity(1301001).GetChild(ActivityId);
        public PubnoviceTaskConfig Config
        {get{if(cachedConfig==null)cachedConfig=OutgameLimitTaskModels.Services.Config().GetNoviceTaskConfig(id);return cachedConfig;}}
        public List<OutgameActivityCondition> ShowCondition
        {
            get
            {
                if(showConditions==null)
                {
                    showConditions=new List<OutgameActivityCondition>();
                    for(int i=0;i<Config.showType.Count;i++)
                    {
                        var condition=new OutgameActivityCondition();condition.arg=new object[Config.showType[i].datas.Length-2];
                        condition.key=Config.showType[i].datas[0];condition.value=Config.showType[i].datas[Config.showType[i].datas.Length-1];
                        Array.Copy(Config.showType[i].datas,1,condition.arg,0,Config.showType[i].datas.Length-2);showConditions.Add(condition);
                    }
                }
                return showConditions;
            }
        }
        public void ResetProgress()
        {
            conditions.Clear();
            for(int i=0;i<Config.conditionParams.Count;i++)
            {
                var condition=new OutgameLimitTaskCondition();condition.arg=new int[Config.conditionParams[i].datas.Length-2];
                condition.key=Config.conditionParams[i].datas[0];
                Array.Copy(Config.conditionParams[i].datas,1,condition.arg,0,Config.conditionParams[i].datas.Length-2);conditions.Add(condition);
            }
        }
        public bool CanComplete
        {
            get
            {
                for(int i=0;i<conditions.Count;i++)
                    if(conditions[i].value<Config.conditionParams[i].datas[Config.conditionParams[i].datas.Length-1])return false;
                for(int i=0;i<ShowCondition.Count;i++)
                {
                    int key=ShowCondition[i].key;var args=ShowCondition[i].arg;
                    long value=OutgameLimitTaskModels.Services.Statistics.GameValue(key,args);
                    if(value<ShowCondition[i].value)return false;
                }
                if(ChildActivity.ModuleData.ext.dayId<Config.day)return false;
                return state==0;
            }
        }
        public int BtnState=>state==1?2:(CanComplete?0:1);
        public int CompareTo(OutgameLimitTaskItemData other)
        {
            bool ownComplete=CanComplete,otherComplete=other.CanComplete;int otherState=other.state;
            if(state==0)
            {
                if(ownComplete){if(!(otherState==0&&otherComplete))return -1;}
                else if(otherState!=0||otherComplete)return otherState!=0?-1:1;
            }
            else if(otherState==0)return 1;
            var ownConfig=Config;int otherId=other.Config.id;return ownConfig.id.CompareTo(otherId);
        }
        public List<OutgameItemReward> RewardsData
        {
            get
            {
                if(rewards==null)
                {
                    rewards=new List<OutgameItemReward>();rewards=new List<OutgameItemReward>();var config=Config;
                    for(int i=0;i<config.rewards.Count;i++)
                    {int item=config.rewards[i].datas[0],count=config.rewards[i].datas[1];rewards.Add(new OutgameItemReward{itemId=item,itemCount=count,rewardOrder=0});}
                }
                return rewards;
            }
        }
    }
    // Original NoviceAccRewardItemData4705. State1 is claimed; eligibility ignores other state values.
    [Serializable] public sealed class OutgameNoviceAccRewardItemData:IComparable<OutgameNoviceAccRewardItemData>
    {
        public int id,state;
        PubnoviceAccRewardConfig cachedConfig;List<OutgameItemReward> rewards;
        public int ActivityId
        {
            get
            {
                foreach(var pair in OutgameLimitTaskModels.Services.Config().NoviceAccByActivity)
                    if(pair.Value.ContainsKey(id))return pair.Key;
                OutgameLimitTaskModels.Services.Error(new object[]{string.Format("限时X日任务活动未找到累计奖励id:{0}对应的子活动ID",id)});return 0;
            }
        }
        public IOutgameLimitTimeTaskChild ChildActivity=>OutgameLimitTaskModels.Services.GetActivity(1301001).GetChild(ActivityId);
        public PubNTActivityConfig LimitTaskActivityConfig=>OutgameLimitTaskModels.Services.Config().GetLimitTaskActivityConfig(ActivityId);
        public PubnoviceAccRewardConfig Config
        {get{if(cachedConfig==null)cachedConfig=OutgameLimitTaskModels.Services.Config().GetNoviceAccRwardConfig(id);return cachedConfig;}}
        public int Progress
        {
            get
            {
                if(LimitTaskActivityConfig.accType==0)return OutgameLimitTaskModels.ClaimedCount(ChildActivity.DayTasks);
                if(LimitTaskActivityConfig.accType!=1)return 0;
                long total=0;
                foreach(var tasks in ChildActivity.DayTasks.Values)
                    for(int i=0;i<tasks.Count;i++)if(tasks[i].state==1)
                        for(int j=0;j<tasks[i].RewardsData.Count;j++)
                        {
                            var items=OutgameLimitTaskModels.Services.Items();var item=items.GetGameItemConfig(tasks[i].RewardsData[j].itemId);
                            if(item.type1==2&&item.type2==4)total=unchecked(total+tasks[i].RewardsData[j].itemCount);
                        }
                return unchecked((int)total);
            }
        }
        public bool CanComplete()=>state!=1&&Progress>=Config.accValue;
        public int CompareTo(OutgameNoviceAccRewardItemData other)
        {var ownConfig=Config;int otherValue=other.Config.accValue;return ownConfig.accValue.CompareTo(otherValue);}
        public List<OutgameItemReward> RewardsData
        {
            get
            {
                if(rewards==null)
                {
                    rewards=new List<OutgameItemReward>();
                    for(int i=0;i<Config.rewards.Count;i++)
                    {int item=Config.rewards[i].datas[0],count=Config.rewards[i].datas[1];rewards.Add(new OutgameItemReward{itemId=item,itemCount=count,rewardOrder=0});}
                }
                return rewards;
            }
        }
    }
    public sealed partial class OutgameLimitTimeTaskExt
    {
        public IOutgameLimitTimeTaskChild ChildActivity=>OutgameLimitTaskModels.Services.GetActivity(1301001).GetChild(activityId);
        public int AccProgress
        {
            get
            {
                var config=OutgameLimitTaskModels.Services.Config().GetLimitTaskActivityConfig(activityId);
                if(config.accType==0)return OutgameLimitTaskModels.ClaimedCount(ChildActivity.DayTasks);
                if(config.accType!=1)
                {OutgameLimitTaskModels.Services.Error(new object[]{string.Format("id为{0}的PubNTActivityConfig配置表accType字段填写错误，请检查",config.id)});return 0;}
                long total=0;
                foreach(var tasks in ChildActivity.DayTasks.Values)
                    for(int i=0;i<tasks.Count;i++)if(tasks[i].state==1)
                        for(int j=0;j<tasks[i].RewardsData.Count;j++)
                        {
                            var items=OutgameLimitTaskModels.Services.Items();var item=items.GetGameItemConfig(tasks[i].RewardsData[j].itemId);
                            if(item.type1==2&&item.type2==4&&item.paramInt==activityId)total=unchecked(total+tasks[i].RewardsData[j].itemCount);
                        }
                return unchecked((int)total);
            }
        }
        public int TargetProgress
        {
            get
            {
                OutgameLimitTaskModels.Services.Config().GetLimitTaskActivityConfig(activityId);
                if(OutgameLimitTaskModels.Services.Config().NoviceAccByActivity.ContainsKey(activityId))
                {
                    int target=0;
                    foreach(var config in OutgameLimitTaskModels.Services.Config().NoviceAccByActivity[activityId].Values)
                        if(target<config.accValue)target=config.accValue;
                    return target;
                }
                OutgameLimitTaskModels.Services.Error(new object[]{string.Format("限时X日任务累计奖励表不包含活动id:{0}",activityId)});return 999;
            }
        }
    }
    public sealed class OutgameLimitTaskPageItem:IOutgameSelectData
    {
        public int activityId,day;public bool Selected{get;set;}
        public OutgameLimitTaskPageItem(int day,int activityId){this.activityId=activityId;this.day=day;}
    }
}
