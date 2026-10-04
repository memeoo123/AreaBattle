// Source metadata schemas; preserve original JSON field names and declared types.
using System;
using System.Collections.Generic;
using AreaBattle.OriginalConfig;
namespace AreaBattle.ActivityConfig
{
    // Original type 4562
    [Serializable] public sealed class PubAchievementAccConfig:IOutgameConfigRow
    {
        public int id;
        public object UniqueID=>id;
    }
    // Original type 4563
    [Serializable] public sealed class PubAchievementConfig:IOutgameConfigRow
    {
        public int id;
        public int type;
        public int priority;
        public List<ListArrayInt> showCondition;
        public int contentType;
        public int content;
        public Lang des;
        public long number;
        public List<ListArrayInt> rewards;
        public object UniqueID=>id;
    }
    // Original type 4573
    [Serializable] public sealed class PubnoviceAccRewardConfig:IOutgameConfigRow
    {
        public int id;
        public int activityId;
        public int accValue;
        public Lang des;
        public List<ListArrayInt> rewards;
        public object UniqueID=>id;
    }
    // Original type 4575
    [Serializable] public sealed class PubnoviceTaskConfig:IOutgameConfigRow
    {
        public int id;
        public int activityId;
        public int day;
        public int permission;
        public List<ListArrayInt> showType;
        public List<ListArrayInt> conditionParams;
        public Lang des;
        public List<ListArrayInt> expand;
        public List<ListArrayInt> rewards;
        public int showCompleted;
        public int allLifeStatistics;
        public object UniqueID=>id;
    }
    // Original type 4577
    [Serializable] public sealed class PubNTActivityConfig:IOutgameConfigRow
    {
        public int id;
        public string icon;
        public Lang name;
        public int accType;
        public object UniqueID=>id;
    }
    // Original type 4579
    [Serializable] public sealed class PubTaskActivityConfig:IOutgameConfigRow
    {
        public int id;
        public string icon;
        public Lang name;
        public int type;
        public int param;
        public int refreshPeriod;
        public int[] refreshParams;
        public int freeRefreshTimes;
        public int[] extraCondition;
        public string[] extraParams;
        public int[] extraRefreshTimes;
        public int extraRefreshOffOn;
        public object UniqueID=>id;
    }
    // Original type 4581
    [Serializable] public sealed class PubTaskConfig:IOutgameConfigRow
    {
        public int id;
        public int taskGroupId;
        public List<ListArrayInt> showType;
        public int weight;
        public int taskTypeId;
        public List<ListArrayInt> conditionParams;
        public Lang des;
        public List<ListArrayInt> rewards;
        public int liveness;
        public int showCompleted;
        public List<ListArrayInt> expandItems;
        public int autoRefresh;
        public int autoGet;
        public int autoRefreshComplete;
        public int Expire;
        public int receiveMode;
        public int allLifeStatistics;
        public object UniqueID=>id;
    }
    // Original type 4583
    [Serializable] public sealed class PubTaskGroupConfig:IOutgameConfigRow
    {
        public int Id;
        public int activityId;
        public int taskGroupId;
        public int taskGroupCount;
        public int taskGroupRandom;
        public int freeRefreshTimes;
        public int[] extraCondition;
        public string[] extraParams;
        public int[] extraRefreshTimes;
        public int allLifeStatistics;
        public object UniqueID=>Id;
    }
    // Original type 4672
    [Serializable] public sealed class PubdailyLivenessConfig:IOutgameConfigRow
    {
        public int id;
        public int livenessValue;
        public int activityID;
        public List<ListArrayInt> rewards;
        public object UniqueID=>id;
    }
}
