using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
namespace AreaBattle
{
    public sealed class OutgameTaskModelServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<string,object[],string> LanguageFormat;
    }
    public static class OutgameTaskModels {public static OutgameTaskModelServices Services;}
    // Source4686.Clone35473 uses the same Object.MemberwiseClone native wrapper as item data.
    [Serializable] public sealed class OutgameTaskData:ICloneable
    {
        public List<OutgameChildTaskData> datas=new List<OutgameChildTaskData>();
        public object Clone()=>MemberwiseClone();
    }
    [Serializable] public sealed class OutgameChildTaskData
    {
        public int activityID;
        public List<OutgameTaskItemData> tasks=new List<OutgameTaskItemData>();
        public OutgameTaskExt ext=new OutgameTaskExt();
        PubTaskActivityConfig m_config;int m_ArriveVideoTimes=-1;
        public PubTaskActivityConfig Config=>m_config??(m_config=OutgameTaskModels.Services.Config().GetTaskActivityConfig(activityID));
        public void ResetExtraRefreshTimes()
        {
            ext.extraRefreshNumList=new List<int>();
            for(int i=0;i<Config.extraRefreshTimes.Length;i++)ext.extraRefreshNumList.Add(0);
        }
    }
    [Serializable] public sealed class OutgameTaskExt
    {
        public int activityID,livenessAward,livenessValue,freeRefreshNum,extraRefreshNum;
        public List<int> extraRefreshNumList=new List<int>();
        public int videoTimes;public long nextRefreshTimeStamp;public int TRefreshTimes;
        public List<OutgameTaskGroupData> groupDatas=new List<OutgameTaskGroupData>();
        internal List<OutgameTaskLivenessItemData> livenessItemDatas;
        public List<OutgameTaskLivenessItemData> GetLivenessItems()
        {
            if(livenessItemDatas==null){livenessItemDatas=new List<OutgameTaskLivenessItemData>();RefreshLivenessItems();}
            return livenessItemDatas;
        }
        public void RefreshLivenessItems()
        {
            livenessItemDatas.Clear();var rows=OutgameTaskModels.Services.Config().GetLivenessDicByActivityId(activityID);
            if(rows==null)return;int i=0;
            foreach(var row in rows.Values)
            {var item=new OutgameTaskLivenessItemData{id=row.id,livenessValue=row.livenessValue,state=(int)(((uint)livenessAward>>(i&31))&1)};livenessItemDatas.Add(item);i++;}
        }
    }
    [Serializable] public sealed class OutgameTaskGroupData
    {
        public int groupID,freeRefreshTimes;public List<int> extraRefreshTimes;
        PubTaskGroupConfig m_config;int m_ArriveVideoTimes=-1;
        public PubTaskGroupConfig Config=>m_config??(m_config=OutgameTaskModels.Services.Config().GetTaskGroupConfig(groupID));
        public void ResetExtraRefreshTimes()
        {extraRefreshTimes=new List<int>();for(int i=0;i<Config.extraRefreshTimes.Length;i++)extraRefreshTimes.Add(0);}
    }
    [Serializable] public sealed class OutgameTaskCondition
    {
        public int key;public long value;public int[] arg;
        public bool KeyEqual(OutgameTaskCondition other)
        {
            if(key!=other.key)return false;
            var left=arg;var right=other.arg;if(left==null||right==null)return left==null&&right==null;
            if(left.Length!=right.Length)return false;int i=0;
            for(;i<arg.Length;i++)if(arg[i]!=other.arg[i])break;
            return i>=arg.Length;
        }
    }
    [Serializable] public sealed class OutgameTaskItemData:IOutgameSelectData,IComparable<OutgameTaskItemData>
    {
        public long uid;public int id,groupID,activityID;public long progress,expireTimeStamp;
        public int receiveState=1;public List<OutgameTaskCondition> conditions=new List<OutgameTaskCondition>();public int state;
        PubTaskConfig m_config;int m_ArriveVideoTimes=-1;
        public bool Selected {get;set;}
        public PubTaskConfig Config=>m_config??(m_config=OutgameTaskModels.Services.Config().GetTaskConfig(id));
        // Source35484 APPENDS; unlike the limited-task model it does not clear old conditions.
        public void ResetProgress()
        {
            for(int i=0;i<Config.conditionParams.Count;i++)
            {
                var condition=new OutgameTaskCondition();condition.arg=new int[Config.conditionParams[i].datas.Length-2];
                condition.key=Config.conditionParams[i].datas[0];
                Array.Copy(Config.conditionParams[i].datas,1,condition.arg,0,Config.conditionParams[i].datas.Length-2);conditions.Add(condition);
            }
        }
        public bool CanComplete()
        {
            var config=Config;
            for(int i=0;i<conditions.Count;i++)if(conditions[i].value<config.conditionParams[i].datas[config.conditionParams[i].datas.Length-1])return false;
            return state==0;
        }
        public string GetDes()
        {
            var config=Config;var values=new int[config.conditionParams.Count];
            for(int i=0;i<config.conditionParams.Count;i++)values[i]=config.conditionParams[i].datas[config.conditionParams[i].datas.Length-1];
            var args=new object[values.Length];for(int i=0;i<values.Length;i++)args[i]=values[i];
            return OutgameTaskModels.Services.LanguageFormat(config.des.key,args);
        }
        public int CompareTo(OutgameTaskItemData other)
        {
            bool complete=CanComplete(),otherComplete=other.CanComplete();
            if(state==0)
            {
                if(complete){if(!(other.state==0&&otherComplete))return -1;}
                else if(receiveState==0)
                {
                    if(other.receiveState==1){if(other.state!=0)return -1;return otherComplete?1:-1;}
                }
                else
                {
                    if(other.receiveState==0)return 1;
                    if(other.state!=0||otherComplete)return other.state!=0?-1:1;
                }
            }
            else if(other.state!=1)return 1;
            var config=Config;var otherConfig=other.Config;
            return config.id<otherConfig.id?-1:config.id>otherConfig.id?1:0;
        }
    }
    [Serializable] public sealed class OutgameTaskLivenessItemData
    {
        public int id,livenessValue,state;
        PubdailyLivenessConfig config;List<OutgameItemReward> rewards;
        public PubdailyLivenessConfig Config=>config??(config=OutgameTaskModels.Services.Config().GetLivenessConfig(id));
        public List<OutgameItemReward> Rewards
        {
            get
            {
                if(rewards==null)
                {
                    rewards=new List<OutgameItemReward>();
                    for(int i=0;i<Config.rewards.Count;i++)
                    {int item=Config.rewards[i].datas[0];int count=Config.rewards[i].datas[1];rewards.Add(new OutgameItemReward{itemId=item,itemCount=count});}
                }
                return rewards;
            }
        }
    }
}
