using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
using UnityEngine;
namespace AreaBattle
{
    // Source4681; task generation, calendar refresh, claims and callback order.
    public sealed class OutgameChildTaskOffStrategy:OutgameTaskNetStrategy
    {
        public OutgameTaskActivity Parent;
        static readonly Color ProgressColor=new Color(1f,0.9215686321258545f,0.01568627543747425f,1f);
        public OutgameChildTaskOffStrategy(OutgameTaskServices tasks):base(tasks){}
        long Now()=>Services.Statistics.GameValue(10000,Array.Empty<object>());
        public override void OnInit()
        {
            Services.Common().AddListener(LivenessPrefix+Data.activityID,LivenessEvent);
            Data.tasks.Sort((a,b)=>a.CompareTo(b));Parent=Tasks.Control().GetActivity<OutgameTaskActivity>(1300001);TimeEvent(null);
        }
        public override void Dispose(){base.Dispose();Services.Common().RemoveListener(LivenessPrefix+Data.activityID,LivenessEvent);}
        public override void SaveData(){var ext=Data.ext;ext.groupDatas=new List<OutgameTaskGroupData>(Activity.Groups.Values);}
        public override void ResetProgress()
        {
            switch(Data.Config.refreshPeriod)
            {
                case 1:{var ext=Data.ext;long now=Now();ext.nextRefreshTimeStamp=unchecked(now+(long)unchecked(Data.Config.refreshParams[0]*1000));break;}
                case 2:SetNextDaily();break;
                case 3:SetNextWeekly();break;
            }
            ResetAll();
        }
        public override void TimeEvent(object[] args)
        {
            switch(Data.Config.refreshPeriod)
            {
                case 1:RefreshInterval();break;
                case 2:if(Now()>Data.ext.nextRefreshTimeStamp){SetNextDaily();ResetAll();}break;
                case 3:if(Now()>Data.ext.nextRefreshTimeStamp){SetNextWeekly();ResetAll();}break;
            }
            RemoveExpired();
            var values=new object[2];values[0]=Data.activityID;long next=Data.ext.nextRefreshTimeStamp;values[1]=unchecked(next-Now());
            Services.Common().SendMessage("CommonModule_ResetTimeRefresh",values);
        }
        public void SetNextDaily()
        {
            var now=OutgameItemTimestamp.ToDateTime(Now());int hour=now.Hour;
            if(Data.Config.refreshParams.Length==0){Tasks.Error(new object[]{"刷新参数为空",Data.activityID});return;}
            int i=0;while(i<Data.Config.refreshParams.Length&&hour>=Data.Config.refreshParams[i])i++;
            var ext=Data.ext;
            var date=new DateTime(now.Year,now.Month,now.Day);
            ext.nextRefreshTimeStamp=OutgameItemTimestamp.FromDateTime(i==Data.Config.refreshParams.Length?date.AddDays(1).AddHours(Data.Config.refreshParams[0]):date.AddHours(Data.Config.refreshParams[i]));
            Parent.Dirty=true;Services.Common().SendMessage("CommonModule_TaskSingle_DailyRest");
        }
        public void SetNextWeekly()
        {
            var now=OutgameItemTimestamp.ToDateTime(Now());int day=(int)now.DayOfWeek;
            if(Data.Config.refreshParams.Length==0){Tasks.Error(new object[]{"刷新参数为空",Data.activityID});return;}
            int i=0;while(i<Data.Config.refreshParams.Length&&day>=Data.Config.refreshParams[i])i++;
            int days=i==Data.Config.refreshParams.Length?unchecked(Data.Config.refreshParams[0]+7-day):unchecked(Data.Config.refreshParams[i]-day);
            var ext=Data.ext;ext.nextRefreshTimeStamp=OutgameItemTimestamp.FromDateTime(new DateTime(now.Year,now.Month,now.Day).AddDays(days));
            Parent.Dirty=true;Services.Common().SendMessage("CommonModule_TaskSingle_WeeklyRest");
        }
        public void RefreshInterval()
        {
            if(Data.Config.refreshParams==null||Data.Config.refreshParams.Length==0){Tasks.Error(new object[]{"配置中刷新参数refreshParams为空",Data.activityID});return;}
            var ext=Data.ext;
            if(ext.nextRefreshTimeStamp==0)
            {long now=Now();ext.nextRefreshTimeStamp=unchecked(now+(long)unchecked(Data.Config.refreshParams[0]*1000));ResetAll();return;}
            int milliseconds=unchecked(Data.Config.refreshParams[0]*1000);
            long elapsed=unchecked(Now()-Data.ext.nextRefreshTimeStamp);long current=Now();ext=Data.ext;long next=ext.nextRefreshTimeStamp;
            if(current<next)return;
            ext.nextRefreshTimeStamp=unchecked(next+(long)milliseconds*(elapsed/milliseconds+1));ResetAll();
        }
        public void ResetAll()
        {
            Data.ext.videoTimes=0;Data.ext.freeRefreshNum=0;Data.ResetExtraRefreshTimes();Data.ext.TRefreshTimes=0;
            ResetTasks(true);Parent.Dirty=true;
        }
        public void RemoveExpired()
        {
            for(int i=0;i<Data.tasks.Count;i++)
            {
                if(Data.tasks[i].expireTimeStamp==0||Now()<=Data.tasks[i].expireTimeStamp)continue;
                Activity.TasksByUid.Remove(Data.tasks[i].uid);Data.tasks.RemoveAt(i--);Parent.Dirty=true;
                Services.Common().SendMessage(RefreshList,new object[]{Data.activityID,Data.tasks});
            }
        }
        public override void ChangeTaskProgress(long uid,long value,int key,object[] args)
        {
            var task=Activity.GetTaskItemData(uid);Tasks.LogByColor(ProgressColor,new object[]{"进度变化","id",task.id,"进度",value});
            for(int i=0;i<task.conditions.Count;i++)
            {
                if(task.conditions[i].key!=key||task.conditions[i].arg.Length!=args.Length)continue;
                bool match=true;for(int j=0;j<task.conditions[i].arg.Length;j++)match&=task.conditions[i].arg[j].Equals(args[j]);
                if(!match)continue;
                long old=task.conditions[i].value;var condition=task.conditions[i];condition.value=value;task.conditions[i]=condition;
                var targetRow=task.Config.conditionParams[i].datas;
                if(old<targetRow[task.Config.conditionParams[0].datas.Length-1]&&old!=value)
                {
                    var report=Tasks.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);
                    report.ActivityName=Tasks.Localize(Activity.Config.activityName);report.DetailType=task.id.ToString();
                    int index=i<3?i:0;report.OldValues[index]=unchecked((int)old);report.Values[index]=unchecked((int)value);
                    Tasks.Reports().Send(OutgameLimitTaskReportKind.Success,report);
                }
                Parent.Dirty=true;return;
            }
        }
        public void LivenessEvent(object[] args)
        {
            if(args==null||args.Length==0)return;int delta=(int)args[0];var ext=Data.ext;int old=ext.livenessValue;ext.livenessValue=unchecked(old+delta);
            var report=Tasks.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);
            report.ActivityName=Tasks.Localize(Activity.Config.activityName);report.DetailType="活跃度";report.OldValues[0]=old;report.Values[0]=Data.ext.livenessValue;
            Tasks.Reports().Send(OutgameLimitTaskReportKind.Success,report);
            Services.Common().SendMessage(RefreshExt,new object[]{Data.activityID,Data.ext});Parent.Dirty=true;
        }
        public override void TaskComplete(long uid,bool notify)
        {
            var task=Activity.GetTaskItemData(uid);if(task==null||!task.CanComplete())return;
            var config=Tasks.Config().GetTaskConfig(task.id);if(config==null)return;
            task.state=1;var rewards=new List<OutgameItemReward>();
            for(int i=0;i<config.rewards.Count;i++)rewards.Add(new OutgameItemReward{itemId=config.rewards[i].datas[0],itemCount=config.rewards[i].datas[1]});
            Tasks.Rewards().AddRewardsByItemSelf(rewards);
            var costs=new List<OutgameItemReward>();
            for(int i=0;i<config.expandItems.Count;i++)costs.Add(new OutgameItemReward{itemId=config.expandItems[i].datas[0],itemCount=config.expandItems[i].datas[1]});
            Tasks.Rewards().ExpendReward(costs);
            if(notify){Data.tasks.Sort((a,b)=>a.CompareTo(b));Services.Common().SendMessage(RefreshList,new object[]{Data.activityID,Data.tasks});}
            Parent.Dirty=true;
            var report=Tasks.Reports().Create(OutgameLimitTaskReportKind.Reward,true,null);
            report.ActivityName=Tasks.Localize(Activity.Config.activityName);report.DetailType=task.id.ToString();report.Values[0]=unchecked((int)task.progress);
            Tasks.Reports().Send(OutgameLimitTaskReportKind.Reward,report);
            foreach(var row in Activity.TasksByUid.Values)if(row.state==0)return;
            report=Tasks.Reports().Create(OutgameLimitTaskReportKind.Complete,true,null);report.ActivityName=Tasks.Localize(Activity.Config.activityName);
            Tasks.Reports().Send(OutgameLimitTaskReportKind.Complete,report);
        }
        public override void GetLivenessReward(int id)
        {
            var config=Tasks.Config().GetLivenessConfig(id);if(config==null)return;
            var rewards=new List<OutgameItemReward>();
            for(int i=0;i<config.rewards.Count;i++)rewards.Add(new OutgameItemReward{itemId=config.rewards[i].datas[0],itemCount=config.rewards[i].datas[1]});
            Tasks.Rewards().AddRewardsByItemSelf(rewards);
            var items=Data.ext.GetLivenessItems();
            for(int i=0;i<items.Count;i++)if(items[i].livenessValue==config.livenessValue){Data.ext.livenessAward|=1<<i;items[i].state=1;break;}
            var report=Tasks.Reports().Create(OutgameLimitTaskReportKind.Reward,true,null);
            report.ActivityName=Tasks.Localize(Activity.Config.activityName);report.DetailType="活跃度";report.Values[0]=id;Tasks.Reports().Send(OutgameLimitTaskReportKind.Reward,report);
            Services.Common().SendMessage(RefreshExt,new object[]{Data.activityID,Data.ext});Parent.Dirty=true;
        }
        sealed class Weight {public int Value;public PubTaskConfig Config;}
        Weight Select(List<Weight> rows)
        {
            int total=0;foreach(var row in rows)total=unchecked(total+row.Value);
            int roll=Tasks.Random.Managed.Next(0,total),cumulative=0;
            for(int i=0;i<rows.Count;i++){cumulative=unchecked(cumulative+rows[i].Value);if(roll<cumulative)return rows[i];}
            return rows[0];
        }
        public void ResetTasks(bool all)
        {
            var removed=new Dictionary<long,OutgameTaskItemData>();
            for(int i=0;i<Data.tasks.Count;i++)
            {
                if(all)
                {if(Data.tasks[i].CanComplete()&&Data.tasks[i].Config.autoGet==0)TaskComplete(Data.tasks[i].uid,false);}
                else if(Data.tasks[i].state==1)
                {if(Data.tasks[i].Config.autoRefreshComplete!=0)continue;}
                else if(Data.tasks[i].CanComplete())
                {if(Data.tasks[i].Config.autoRefresh!=0)continue;if(Data.tasks[i].Config.autoGet==0)TaskComplete(Data.tasks[i].uid,false);}
                if(!removed.ContainsKey(Data.tasks[i].uid))removed.Add(Data.tasks[i].uid,Data.tasks[i]);
            }
            foreach(var task in removed.Values)
            {
                Services.Common().SendMessage("CommonGameModule_Task_EntityRemove",new object[]{task.uid});Data.tasks.Remove(task);
                if(Activity.TasksByUid.ContainsKey(task.uid))Activity.TasksByUid.Remove(task.uid);
            }
            if(Tasks.Config().TaskGroupsByActivity.TryGetValue(Data.activityID,out var groups))
            {
                int remaining=Data.tasks.Count;
                for(int g=0;g<groups.Count-remaining;g++)
                {
                    if(Tasks.Random.Inclusive(0,10000)>groups[g].taskGroupRandom)continue;
                    int groupId=groups[g].taskGroupId;var candidates=new List<Weight>();
                    if(!Tasks.Config().TasksByGroup.TryGetValue(groupId,out var configs))continue;
                    for(int k=0;k<configs.Count;k++)
                    {
                        int index=k;if(Data.tasks.Count!=0&&Data.tasks.Exists(task=>task.id==configs[index].id))continue;
                        var weight=new Weight{Config=configs[k],Value=configs[k].weight};bool eligible=true;
                        for(int j=0;j<configs[k].showType.Count;j++)
                        {
                            var args=new object[configs[k].showType[j].datas.Length-2];int key=configs[k].showType[j].datas[0];
                            int target=configs[k].showType[j].datas[configs[k].showType[j].datas.Length-1];
                            Array.Copy(configs[k].showType[j].datas,1,args,0,configs[k].showType[j].datas.Length-2);
                            if(Services.Statistics.GameValue(key,args)<target){eligible=false;break;}
                        }
                        if(eligible)candidates.Add(weight);
                    }
                    if(groups[g].taskGroupCount>candidates.Count){Tasks.Error(new object[]{string.Format("{0}任务集生效次数大于任务集配置内的任务数量",Data.activityID)});continue;}
                    var choices=new List<Weight>(candidates);candidates.Clear();
                    for(int k=0;k<groups[g].taskGroupCount;k++){var selected=Select(choices);candidates.Add(selected);choices.Remove(selected);}
                    for(int k=0;k<candidates.Count;k++)
                    {
                        var task=new OutgameTaskItemData{activityID=Data.activityID,groupID=groups[g].Id,id=candidates[k].Config.id};
                        task.expireTimeStamp=candidates[0].Config.Expire!=0?unchecked(Now()+(long)unchecked(candidates[0].Config.Expire*60000)):0;
                        if(candidates[0].Config.receiveMode==1)task.receiveState=0;
                        if(Activity.TasksByUid.ContainsKey(task.uid))continue;
                        task.uid=task.GetHashCode();task.ResetProgress();Activity.TasksByUid.Add(task.uid,task);
                        OutgameTaskGroupData group;
                        if(!Activity.Groups.ContainsKey(groups[g].Id))
                        {group=new OutgameTaskGroupData{groupID=groups[g].Id};Activity.Groups.Add(groups[g].Id,group);Data.ext.groupDatas.Add(group);}
                        else{group=Activity.Groups[groups[g].Id];group.freeRefreshTimes=0;}
                        group.ResetExtraRefreshTimes();Data.tasks.Add(task);
                    }
                }
            }
            if(all||Data.Config.extraRefreshOffOn==1)
            {
                if(Data.ext.GetLivenessItems()!=null)
                {
                    var items=Data.ext.GetLivenessItems();
                    for(int i=0;i<items.Count;i++)if(Data.ext.livenessValue>=items[i].livenessValue&&items[i].state==0)GetLivenessReward(items[i].id);
                }
                Data.ext.livenessAward=0;Data.ext.livenessValue=0;Data.ext.videoTimes=0;
                if(Data.ext.GetLivenessItems()!=null)Data.ext.RefreshLivenessItems();
            }
            var data=Data;data.tasks=new List<OutgameTaskItemData>(Activity.TasksByUid.Values);Data.tasks.Sort((a,b)=>a.CompareTo(b));
            RemoveListeners();AddListeners();
            for(int i=0;i<Data.tasks.Count;i++)Services.Common().SendMessage("CommonGameModule_Task_EntityAdd",new object[]{Data.tasks[i].uid});
            Services.Common().SendMessage(RefreshExt,new object[]{Data.activityID,Data.ext});Services.Common().SendMessage(RefreshList,new object[]{Data.activityID,Data.tasks});Parent.Dirty=true;
        }
    }
}
