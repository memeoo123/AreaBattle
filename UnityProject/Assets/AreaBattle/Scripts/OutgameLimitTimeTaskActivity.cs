using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    public enum OutgameLimitTaskReportKind {Success,Reward,Complete}
    // Source activity_success/reward/complete fields assigned by the task activities.
    // The required host owns remaining fields, initialization and delivery.
    public sealed class OutgameLimitTaskReport
    {
        public string ParentName,ActivityName,DetailType,DetailId;
        public int?[] Values=new int?[3],OldValues=new int?[3];
    }
    public interface IOutgameLimitTaskReports
    {
        OutgameLimitTaskReport Create(OutgameLimitTaskReportKind kind,bool initialize,object argument);
        void Send(OutgameLimitTaskReportKind kind,OutgameLimitTaskReport report);
    }
    public sealed class OutgameLimitTimeTaskServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameActivityControl> Control;
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameGlobalItemRewards> Rewards;
        public OutgameItemEntityServices Entities;
        public Func<IOutgameLimitTaskReports> Reports;
        public Func<OutgameActivityConfigRow.Name,string> Localize;
        public Func<OriginalConfig.Lang,string> LocalizeBusiness;
        public Action<object[]> Log,Error;
        public Action<Color,object[]> LogByColor;
    }
    // Source LimitTimeTaskActivity4699. Manager and child ownership stay in the
    // actual common activity/data-pool graph; no second task save or reward engine.
    [OutgameActivityControlRegister(1301001)]
    public sealed class OutgameLimitTimeTaskActivity:OutgameFatherActivity<OutgameChildLimitTimeTaskActivity,OutgameLimitTimeTaskChildData>,IOutgameLimitTimeTaskActivity
    {
        readonly OutgameLimitTimeTaskServices tasks;
        OutgameNoviceTaskManager manager;
        public bool Dirty{get;set;}
        public Dictionary<int,OutgameChildLimitTimeTaskActivity> TaskViewActivities=>SelfViewActivities;
        public OutgameNoviceTaskManager Manager=>manager??(manager=tasks.Pool().GetModel<OutgameNoviceTaskManager>(4711,"NoviceTaskManager"));
        public OutgameLimitTimeTaskData ModuleData=>Manager.Data;
        public OutgameLimitTimeTaskActivity(OutgameActivityServices services,OutgameLimitTimeTaskServices tasks)
            :base(services,new OutgameActivityFamilyServices<OutgameChildLimitTimeTaskActivity>{Config=tasks.Config,Control=tasks.Control,CreateChild=()=>new OutgameChildLimitTimeTaskActivity(services,tasks)}){this.tasks=tasks;}
        public IOutgameLimitTimeTaskChild GetChild(int id)=>GetChildActivity<OutgameChildLimitTimeTaskActivity>(id);
        public override OutgameActivityFactory ActivityFactoryBase(int id)=>new OutgameLimitTimeTaskFactory(id,Services.ItemConfig,Services.ItemFactoryError,tasks.Entities,Services.Common);
        public override void OnInit()
        {
            base.OnInit();Manager.Activity=this;
            Services.Statistics.RegisteredValueFunc(10021,LimitTaskActDayComplete);
            ResetChildActivity(tasks.Config().NoviceActivities,Manager.LimitTimeTaskDic);
        }
        public void RefreshData()
        {
            for(int i=0;i<ModuleData.datas.Count;i++)
            {
                var child=GetChildActivity<OutgameChildLimitTimeTaskActivity>(ModuleData.datas[i].ext.activityId);
                child.ModuleData=ModuleData.datas[i];child.OnInit();
            }
        }
        public override void Update()
        {if(Dirty&&tasks.Pool().IsEnableSaveData){var current=Manager;if(current!=null)current.OnSave();Dirty=false;}}
        public long LimitTaskActDayComplete(object[] args)
        {
            if(args==null||args.Length==0)return 0;
            int value=(int)args[0];
            // Source35551 uses the owner's ActivityId here, not value / 100.
            var child=GetChildActivity<OutgameChildLimitTimeTaskActivity>(ActivityId);
            var list=child.GetDayTaskList(value%100);long count=0;
            for(int i=0;i<list.Count;i++)if(list[i].state==1)count=unchecked(count+1);
            return count;
        }
        public void OnSave(){for(int i=0;i<Children.Count;i++)((OutgameChildLimitTimeTaskActivity)Children[i]).SaveModule();}
        public override void Dispose(){base.Dispose();for(int i=0;i<Children.Count;i++)((OutgameChildLimitTimeTaskActivity)Children[i]).Dispose();}
    }
    // Source ChildLimitTimeTaskActivity4698, including observable failure prefixes.
    public sealed class OutgameChildLimitTimeTaskActivity:OutgameChildActivity<OutgameLimitTimeTaskChildData>,IOutgameLimitTimeTaskChild
    {
        readonly OutgameLimitTimeTaskServices tasks;
        OutgameLimitTimeTaskActivity parent;
        public SortedDictionary<int,List<OutgameLimitTaskItemData>> DayTasks;
        readonly List<OutgameNoviceAccRewardItemData> rawAccRewards=new List<OutgameNoviceAccRewardItemData>();
        public List<OutgameNoviceAccRewardItemData> AccRewards=new List<OutgameNoviceAccRewardItemData>();
        readonly DateTime epoch=new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
        readonly List<string> messageKeys=new List<string>();
        static readonly Color LogColor=new Color(1f,0.9215686321258545f,0.01568627543747425f,1f);
        const string RefreshList="CommonModule_RefreshNoviceTaskList",RefreshExt="CommonModule_NoviceExtRefresh",LivenessPrefix="CommonGameModule_LimitTimeTask_LivenessPointAdd";
        OutgameLimitTimeTaskChildData IOutgameLimitTimeTaskChild.ModuleData=>ModuleData;
        SortedDictionary<int,List<OutgameLimitTaskItemData>> IOutgameLimitTimeTaskChild.DayTasks=>DayTasks;
        public OutgameLimitTimeTaskActivity Parent=>parent??(parent=tasks.Control().GetActivity<OutgameLimitTimeTaskActivity>(1301001));
        public OutgameChildLimitTimeTaskActivity(OutgameActivityServices services,OutgameLimitTimeTaskServices tasks):base(services){this.tasks=tasks;}
        public void RefreshData(OutgameLimitTimeTaskChildData data){ModuleData=data;OnInit();}
        public override void OnInit()
        {
            if(DayTasks==null)DayTasks=new SortedDictionary<int,List<OutgameLimitTaskItemData>>();else DayTasks.Clear();
            AccRewards?.Clear();rawAccRewards?.Clear();var seen=new HashSet<int>();
            int i=0;OutgameActivityConfigManager config;
            while(true)
            {
                int count=ModuleData.datas.Count;config=tasks.Config();if(i>=count)break;
                if(config.NoviceTasks.TryGetValue(ModuleData.datas[i].id,out var row))
                {
                    if(!DayTasks.ContainsKey(row.day))DayTasks.Add(row.day,new List<OutgameLimitTaskItemData>());
                    seen.Add(ModuleData.datas[i].id);DayTasks[row.day].Add(ModuleData.datas[i]);
                }
                else tasks.Error(new object[]{"本地配置不包含任务id",ModuleData.datas[i].id});
                i++;
            }
            foreach(var row in config.GetNTTaskConfigDicByActivityId(ModuleData.ext.activityId).Values)
            {
                if(seen.Contains(row.id))continue;
                if(!DayTasks.ContainsKey(row.day))DayTasks.Add(row.day,new List<OutgameLimitTaskItemData>());
                var task=new OutgameLimitTaskItemData{id=row.id};task.ResetProgress();task.state=0;DayTasks[row.day].Add(task);
            }
            BuildAccRewards();RefreshDay();AccRewards.Sort((a,b)=>a.CompareTo(b));
            foreach(var ignored in DayTasks.Values)Services.Common().SendMessage(RefreshList,new object[]{ActivityId,ModuleData.ext.dayId});
            tasks.LogByColor(LogColor,new object[]{"新手任务初始化EXT数据","活动ID",ModuleData.ext.activityId,"dayId",ModuleData.ext.dayId,"dayFlag",ModuleData.ext.dayFlag,"accFlag",ModuleData.ext.accFlag});
            RemoveListeners();AddListeners();Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});
        }
        void BuildAccRewards()
        {
            int index=0;
            foreach(var row in tasks.Config().NoviceAccByActivity[ActivityId].Values)
            {
                var reward=new OutgameNoviceAccRewardItemData{id=row.id,state=(int)((ulong)ModuleData.ext.accFlag>>(index&63))&1};
                rawAccRewards.Add(reward);AccRewards.Add(reward);index++;
            }
        }
        public void SaveModule()
        {
            var seen=new HashSet<int>();for(int i=0;i<ModuleData.datas.Count;i++)seen.Add(ModuleData.datas[i].id);
            foreach(var list in DayTasks.Values)for(int i=0;i<list.Count;i++)
            {
                if(seen.Contains(list[i].id))continue;
                for(int j=0;j<list[i].conditions.Count;j++)if(list[i].conditions[j].value!=0||list[i].state!=0){ModuleData.datas.Add(list[i]);break;}
            }
        }
        public void AddListeners()
        {
            if(messageKeys.Count!=0)return;
            foreach(var list in DayTasks.Values)for(int i=0;i<list.Count;i++)for(int j=0;j<list[i].Config.conditionParams.Count;j++)
            {
                string key=OutgameStatisticsMessageKey.Get(list[i].Config.conditionParams[j].datas[0]);
                if(!messageKeys.Contains(key))messageKeys.Add(OutgameStatisticsMessageKey.Get(list[i].Config.conditionParams[j].datas[0]));
            }
            for(int i=0;i<messageKeys.Count;i++)
            {tasks.Log(new object[]{string.Format("活动{0}监听事件,{1}",ActivityId,messageKeys[i])});Services.Common().AddListener(messageKeys[i],StatisticsEvent);}
            Services.Common().AddListener(OutgameStatisticsMessageKey.Get(10000),TimeEvent);
            Services.Common().AddListener(LivenessPrefix+ActivityId.ToString(),LivenessEvent);
        }
        public void RemoveListeners()
        {
            foreach(var list in DayTasks.Values)for(int i=0;i<list.Count;i++)for(int j=0;j<list[i].conditions.Count;j++)
                Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(list[i].conditions[j].key),StatisticsEvent);
            Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(10000),TimeEvent);messageKeys.Clear();
            Services.Common().RemoveListener(LivenessPrefix+ActivityId.ToString(),LivenessEvent);
        }
        public override void Dispose()=>RemoveListeners();
        public void StatisticsEvent(object[] args)
        {
            try
            {
                if(args==null||args.Length<1)return;
                int key=Convert.ToInt32(args[args.Length-1]);var touched=new List<OutgameLimitTaskItemData>();bool changed=false;
                foreach(var list in DayTasks.Values)foreach(var task in list)
                {
                    var config=task.Config;if(config.allLifeStatistics==0&&Data.state!=3)continue;
                    if(args.Length==3)
                    {
                        int filter=Convert.ToInt32(args[1]);
                        for(int i=0;i<task.conditions.Count;i++)
                        {
                            if(task.conditions[i].key!=key||filter!=task.conditions[i].arg[0])continue;
                            long value=config.allLifeStatistics==0?unchecked(task.conditions[i].value+(long)args[0]):Services.Statistics.GameValue(task.conditions[i].key,new object[]{task.conditions[i].arg[0]});
                            touched.Add(task);ChangeTaskProgress(task.id,Math.Max(value,0L),key,new int[]{filter});changed=true;
                        }
                    }
                    else for(int i=0;i<task.conditions.Count;i++)
                    {
                        if(task.conditions[i].key!=key)continue;
                        long value=config.allLifeStatistics==0?unchecked(task.conditions[i].value+(long)args[0]):Services.Statistics.GameValue(task.conditions[i].key,Array.Empty<object>());
                        touched.Add(task);ChangeTaskProgress(task.id,Math.Max(value,0L),key,new int[0]);changed=true;
                    }
                }
                if(!changed)return;
                var days=new HashSet<int>();for(int i=0;i<touched.Count;i++)if(!days.Contains(touched[i].Config.day))days.Add(touched[i].Config.day);
                foreach(int day in days)SortDayAndNotify(day);
                Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});
            }
            catch(Exception ex){tasks.Error(new object[]{ex.Message+"\r\n"+ex.StackTrace});}
        }
        public void ChangeTaskProgress(int id,long value,int key,int[] args)
        {
            var task=FindTask(id);tasks.LogByColor(LogColor,new object[]{"进度变化","id",task.id,"进度",value});
            for(int i=0;i<task.conditions.Count;i++)
            {
                if(task.conditions[i].key!=key||task.conditions[i].arg.Length!=args.Length)continue;
                bool match=true;for(int j=0;j<task.conditions[i].arg.Length;j++)match&=task.conditions[i].arg[j].Equals(args[j]);
                if(!match)continue;
                long old=task.conditions[i].value;var condition=task.conditions[i];condition.value=value;task.conditions[i]=condition;
                // Original threshold uses the first configured row's length.
                if(old<task.Config.conditionParams[i].datas[task.Config.conditionParams[0].datas.Length-1]&&value!=old)
                {
                    var report=tasks.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);
                    report.ActivityName=tasks.Localize(Config.activityName);report.ParentName=tasks.Control().GetParentReportStr(Config);
                    report.DetailType="任务";report.DetailId=task.id.ToString();int slot=i<3?i:0;
                    report.OldValues[slot]=unchecked((int)old);report.Values[slot]=unchecked((int)value);tasks.Reports().Send(OutgameLimitTaskReportKind.Success,report);
                }
                Parent.Dirty=true;return;
            }
            Parent.Dirty=true;
        }
        public OutgameLimitTaskItemData FindTask(int id)
        {
            var config=tasks.Config().GetNoviceTaskConfig(id);
            if(DayTasks.ContainsKey(config.day))
            {
                for(int i=0;i<DayTasks[config.day].Count;i++)if(DayTasks[config.day][i].id==id)return DayTasks[config.day][i];
                tasks.Error(new object[]{string.Format("不包含id为{0}的任务",id)});return null;
            }
            tasks.Error(new object[]{string.Format("不包含day为{0}的任务",config.day)});return null;
        }
        public List<OutgameLimitTaskItemData> GetDayTaskList(int day)
        {if(DayTasks.TryGetValue(day,out var list))return list;tasks.Error(new object[]{string.Format("不包含day为{0}的任务",day)});return null;}
        public OutgameNoviceAccRewardItemData FindAccReward(int id)
        {for(int i=0;i<rawAccRewards.Count;i++)if(rawAccRewards[i].id==id)return rawAccRewards[i];return null;}
        public void SortDayAndNotify(int day)
        {
            if(!DayTasks.ContainsKey(day))return;DayTasks[day].Sort((a,b)=>a.CompareTo(b));
            Services.Common().SendMessage(RefreshList,new object[]{ActivityId,day});
        }
        public bool DayComplete(int day)
        {
            var list=GetDayTaskList(day);int i=0;
            while(true)
            {
                int count=list.Count;if(i>=count)return i>=count;
                bool shown=true;
                for(int j=0;j<list[i].ShowCondition.Count;j++)
                {
                    int key=list[i].ShowCondition[j].key;var args=list[i].ShowCondition[j].arg;
                    long value=Services.Statistics.GameValue(key,args);
                    if(value<list[i].ShowCondition[j].value){shown=false;break;}
                }
                if(shown&&list[i].state==0)return i>=count;i++;
            }
        }
        public bool ExitRewardWaitGet(int day)
        {var list=GetDayTaskList(day);for(int i=0;i<list.Count;i++)if(list[i].BtnState==0)return true;return false;}
        public int ExitToGetRewardDay()
        {
            int day=DayTasks.First().Key;
            while(true)
            {
                var ext=ModuleData.ext;if(ext.dayId<day)return ext.lastClickDayId;
                var list=GetDayTaskList(day);for(int i=0;i<list.Count;i++)if(list[i].BtnState==0)return day;
                day++;
            }
        }
        public void TimeEvent(object[] args)=>RefreshDay();
        public void RefreshDay()
        {
            long launch=Services.Statistics.GameValue(29000,new object[]{Data.id});if(launch==0)return;
            var launchDate=OutgameItemTimestamp.ToDateTime(launch);var now=OutgameItemTimestamp.ToDateTime(Services.Statistics.GameValue(10000,Array.Empty<object>()));
            int first=(launchDate-epoch).Days;double currentDays=(now-epoch).TotalDays;
            int current=Math.Abs(currentDays)<2147483648d?(int)currentDays:int.MinValue;int day=unchecked(current-first+1);
            if(DayTasks.Count==0){tasks.Error(new object[]{string.Format("限时X日任务子活动ID{0},配置的任务数量为0,请检查配置",ActivityId)});return;}
            int max=DayTasks.Last().Key;day=day>max?max:day;if(day==ModuleData.ext.dayId)return;
            ModuleData.ext.dayId=day;Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});Parent.Dirty=true;
        }
        public override void Launch()
        {
            var ext=ModuleData.ext;ext.dayId=1;ext.launchTime=Services.Statistics.GameValue(10000,Array.Empty<object>());
            Services.Statistics.SetEventCount(29000,Data.id,ModuleData.ext.launchTime);Parent.Dirty=true;
        }
        public override void ResetProgress()
        {
            var module=ModuleData;var ext=module.ext;ext.dayFlag=0;ext.dayId=1;module.datas.Clear();
            foreach(var list in DayTasks.Values)for(int i=0;i<list.Count;i++){list[i].ResetProgress();list[i].state=0;}
            AccRewards.Clear();rawAccRewards.Clear();BuildAccRewards();
            foreach(var pair in DayTasks)Services.Common().SendMessage(RefreshList,new object[]{ActivityId,pair.Key});
            AccRewards.Sort((a,b)=>a.CompareTo(b));Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});Parent.Dirty=true;
        }
        public void LivenessEvent(object[] args)
        {
            if(args==null||args.Length==0)return;int count=(int)args[0];
            var report=tasks.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);
            report.ActivityName=tasks.Localize(Config.activityName);report.ParentName=tasks.Control().GetParentReportStr(Config);report.DetailType="活跃度";
            report.OldValues[0]=ModuleData.ext.AccProgress;report.Values[0]=unchecked(count+ModuleData.ext.AccProgress);
            tasks.Reports().Send(OutgameLimitTaskReportKind.Success,report);Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});Parent.Dirty=true;
        }
        public void GetAccReward(int id)
        {
            var reward=FindAccReward(id);if(!reward.CanComplete())return;
            int position=1;foreach(var pair in tasks.Config().GetNTAccRewardsDicByActivityId(ModuleData.ext.activityId)){if(pair.Value.id==id)break;position++;}
            tasks.Rewards().AddRewardsByItemSelf(reward.RewardsData);reward.state=1;
            var ext=ModuleData.ext;ext.accFlag|=(long)(1<<(position-1));
            var config=tasks.Config().GetLimitTaskActivityConfig(ActivityId);
            var report=tasks.Reports().Create(OutgameLimitTaskReportKind.Reward,true,null);
            report.ActivityName=tasks.LocalizeBusiness(tasks.Config().NoviceActivities[Config.id].name);
            if(config.accType==0)report.DetailType="累计任务数量";else if(config.accType==1)report.DetailType="活跃度";
            report.ParentName=tasks.Control().GetParentReportStr(Config);report.Values[0]=reward.Config.accValue;
            tasks.Reports().Send(OutgameLimitTaskReportKind.Reward,report);
            Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});Parent.Dirty=true;
        }
        public void TaskComplete(int id)
        {
            var task=FindTask(id);if(task==null||!task.CanComplete)return;
            tasks.Rewards().AddRewardsByItemSelf(task.RewardsData);
            var expense=new List<OutgameItemReward>();
            for(int i=0;i<task.Config.expand.Count;i++)expense.Add(new OutgameItemReward{itemId=task.Config.expand[i].datas[0],itemCount=task.Config.expand[i].datas[1]});
            tasks.Rewards().ExpendReward(expense);task.state=1;
            var ext=ModuleData.ext;ext.dayFlag|=(long)(1<<(task.id-1));
            if(!(task.conditions.Count==1&&task.conditions[0].key==10021))Services.Statistics.AddEventCount(10021,unchecked(ActivityId*100+task.Config.day),1);
            var report=tasks.Reports().Create(OutgameLimitTaskReportKind.Reward,true,null);
            report.ActivityName=tasks.LocalizeBusiness(tasks.Config().NoviceActivities[Config.id].name);report.DetailType="任务";
            report.ParentName=tasks.Control().GetParentReportStr(Config);report.DetailId=task.id.ToString();
            for(int i=0;i<task.conditions.Count;i++)if(i<3)report.Values[i]=unchecked((int)task.conditions[i].value);
            tasks.Reports().Send(OutgameLimitTaskReportKind.Reward,report);
            if(DayComplete(task.Config.day))
            {
                report=tasks.Reports().Create(OutgameLimitTaskReportKind.Complete,true,null);
                report.ActivityName=tasks.LocalizeBusiness(tasks.Config().NoviceActivities[Config.id].name);report.ParentName=tasks.Control().GetParentReportStr(Config);
                report.DetailType="每日任务";report.DetailId=task.Config.day.ToString();tasks.Reports().Send(OutgameLimitTaskReportKind.Complete,report);
            }
            int day=DayTasks.First().Key;bool all=true;
            while(day<=DayTasks.Last().Key){if(!DayComplete(day)){all=false;break;}day++;}
            if(all)
            {
                report=tasks.Reports().Create(OutgameLimitTaskReportKind.Complete,true,null);
                report.ActivityName=tasks.LocalizeBusiness(tasks.Config().NoviceActivities[Config.id].name);report.ParentName=tasks.Control().GetParentReportStr(Config);report.DetailType="全部任务";
                tasks.Reports().Send(OutgameLimitTaskReportKind.Complete,report);
            }
            if(tasks.Config().GetLimitTaskActivityConfig(ActivityId).accType==0)
            {
                report=tasks.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);
                report.ActivityName=tasks.Localize(Config.activityName);report.ParentName=tasks.Control().GetParentReportStr(Config);report.DetailType="累计任务数量";
                report.OldValues[0]=unchecked(ModuleData.ext.AccProgress-1);report.Values[0]=ModuleData.ext.AccProgress;tasks.Reports().Send(OutgameLimitTaskReportKind.Success,report);
            }
            SortDayAndNotify(task.Config.day);Services.Common().SendMessage(RefreshExt,new object[]{ModuleData.ext.activityId});Parent.Dirty=true;
        }
    }
}
