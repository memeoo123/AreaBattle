using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
namespace AreaBattle
{
    // ConditionPriority4625 is a value key; the original constructor zeroes its private first field.
    public struct OutgameActivityConditionPriority
    {
        int reserved;public int contentType,content;
        public OutgameActivityConditionPriority(int type,int value){reserved=0;contentType=type;content=value;}
    }
    public sealed partial class OutgameActivityConfigManager
    {
        public Dictionary<object,PubAchievementConfig> Achievements=new Dictionary<object,PubAchievementConfig>();
        public Dictionary<object,PubAchievementAccConfig> AchievementAcc=new Dictionary<object,PubAchievementAccConfig>();
        public Dictionary<int,Dictionary<OutgameActivityConditionPriority,List<PubAchievementConfig>>> AchievementGroups=new Dictionary<int,Dictionary<OutgameActivityConditionPriority,List<PubAchievementConfig>>>();
        public List<PubAchievementAccConfig> AchievementAccList=new List<PubAchievementAccConfig>();
        public Dictionary<object,PubnoviceTaskConfig> NoviceTasks=new Dictionary<object,PubnoviceTaskConfig>();
        public List<PubnoviceAccRewardConfig> NoviceAccList=new List<PubnoviceAccRewardConfig>();
        public Dictionary<object,PubnoviceAccRewardConfig> NoviceAcc=new Dictionary<object,PubnoviceAccRewardConfig>();
        public Dictionary<object,PubNTActivityConfig> NoviceActivities=new Dictionary<object,PubNTActivityConfig>();
        public Dictionary<int,Dictionary<int,PubnoviceTaskConfig>> NoviceTasksByActivity=new Dictionary<int,Dictionary<int,PubnoviceTaskConfig>>();
        public Dictionary<int,Dictionary<int,PubnoviceAccRewardConfig>> NoviceAccByActivity=new Dictionary<int,Dictionary<int,PubnoviceAccRewardConfig>>();
        public Dictionary<object,PubTaskGroupConfig> TaskGroups=new Dictionary<object,PubTaskGroupConfig>();
        public Dictionary<object,PubTaskConfig> Tasks=new Dictionary<object,PubTaskConfig>();
        public Dictionary<object,PubdailyLivenessConfig> Liveness=new Dictionary<object,PubdailyLivenessConfig>();
        public Dictionary<object,PubTaskActivityConfig> TaskActivities=new Dictionary<object,PubTaskActivityConfig>();
        public Dictionary<int,List<PubTaskConfig>> TasksByGroup=new Dictionary<int,List<PubTaskConfig>>();
        public Dictionary<int,List<PubTaskGroupConfig>> TaskGroupsByActivity=new Dictionary<int,List<PubTaskGroupConfig>>();
        public List<PubdailyLivenessConfig> LivenessList;
        public Dictionary<int,Dictionary<int,PubdailyLivenessConfig>> LivenessByActivity=new Dictionary<int,Dictionary<int,PubdailyLivenessConfig>>();
        public PubAchievementConfig GetAchievementConfig(int id)=>GetRow(Achievements,id,"成就表表不包含ID:");
        public PubAchievementAccConfig GetAchievementAccConfig(int id)=>GetRow(AchievementAcc,id,"活跃度表不包含ID:");
        public PubTaskGroupConfig GetTaskGroupConfig(int id){TaskGroups.TryGetValue(id,out var row);return row;}
        public PubTaskConfig GetTaskConfig(int id){Tasks.TryGetValue(id,out var row);return row;}
        public PubdailyLivenessConfig GetLivenessConfig(int id)=>GetRow(Liveness,id,"活跃度表不包含ID:");
        public PubTaskActivityConfig GetTaskActivityConfig(int id)=>GetRow(TaskActivities,id,"活跃度表不包含ID:");
        public PubnoviceTaskConfig GetNoviceTaskConfig(int id)=>GetRow(NoviceTasks,id,"限时X日任务配置表不包含id",true);
        public PubnoviceAccRewardConfig GetNoviceAccRwardConfig(int id)=>GetRow(NoviceAcc,id,"限时X日任务累计奖励配置表不包含id",true);
        public PubNTActivityConfig GetLimitTaskActivityConfig(int id)=>GetRow(NoviceActivities,id,"限时X日任务子活动配置表不包含id");
        T GetRow<T>(Dictionary<object,T> rows,int id,string text,bool logNull=false)where T:class
        {bool found=rows.TryGetValue(id,out var row);if(logNull?row==null:!found)Services.Error(new object[]{text,id});return row;}
        public Dictionary<int,PubnoviceTaskConfig> GetNTTaskConfigDicByActivityId(int id)=>GetNoviceGroup(NoviceTasksByActivity,id);
        public Dictionary<int,PubnoviceAccRewardConfig> GetNTAccRewardsDicByActivityId(int id)=>GetNoviceGroup(NoviceAccByActivity,id);
        Dictionary<int,T> GetNoviceGroup<T>(Dictionary<int,Dictionary<int,T>> groups,int id)
        {if(groups.TryGetValue(id,out var rows))return rows;Services.Error(new object[]{string.Format("不存在子活动id:{0}",id)});return null;}
        public Dictionary<int,PubdailyLivenessConfig> GetLivenessDicByActivityId(int id)
        {if(!LivenessByActivity.TryGetValue(id,out var rows))Services.Warning(new object[]{string.Format("活跃度配置表中不包含活动id为{0}的配置",id)});return rows;}
        public void BuildTaskGroups()
        {
            foreach(var row in TaskGroups.Values)
            {
                if(!TaskGroupsByActivity.ContainsKey(row.activityId))TaskGroupsByActivity[row.activityId]=new List<PubTaskGroupConfig>();
                TaskGroupsByActivity[row.activityId].Add(row);
            }
        }
        public void BuildTasks()
        {
            foreach(var row in Tasks.Values)
            {
                if(!TasksByGroup.ContainsKey(row.taskGroupId))TasksByGroup[row.taskGroupId]=new List<PubTaskConfig>();
                TasksByGroup[row.taskGroupId].Add(row);
            }
        }
        public void BuildLiveness()
        {
            foreach(var row in Liveness.Values)
            {
                if(!LivenessByActivity.ContainsKey(row.activityID))LivenessByActivity[row.activityID]=new Dictionary<int,PubdailyLivenessConfig>();
                LivenessByActivity[row.activityID].Add(row.id,row);
            }
        }
        public void BuildNoviceTasks()
        {
            foreach(var row in NoviceTasks.Values)
            {
                if(!NoviceTasksByActivity.ContainsKey(row.activityId))NoviceTasksByActivity[row.activityId]=new Dictionary<int,PubnoviceTaskConfig>();
                NoviceTasksByActivity[row.activityId].Add(row.id,row);
            }
            foreach(var row in NoviceActivities.Values)
                if(!NoviceTasksByActivity.ContainsKey(row.id))NoviceTasksByActivity[row.id]=new Dictionary<int,PubnoviceTaskConfig>();
        }
        public void BuildNoviceAccRewards()
        {
            foreach(var row in NoviceAcc.Values)
            {
                if(!NoviceAccByActivity.ContainsKey(row.activityId))NoviceAccByActivity[row.activityId]=new Dictionary<int,PubnoviceAccRewardConfig>();
                NoviceAccByActivity[row.activityId].Add(row.id,row);
            }
        }
    }
}
