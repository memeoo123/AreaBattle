using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameTaskServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameActivityControl> Control;
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameGlobalItemRewards> Rewards;
        public OutgameItemEntityServices Entities;
        public Func<IOutgameLimitTaskReports> Reports;
        public Func<OutgameActivityConfigRow.Name,string> Localize;
        public Action<object[]> Error,Warning;
        public Action<Color,object[]> LogByColor;
        public GameRandomSource Random=GameRandomSource.Shared;
    }
    // TaskActivity4685 derives directly from ActivityBase; its other-view getter is transient.
    [OutgameActivityControlRegister(1300001)]
    public sealed class OutgameTaskActivity:OutgameActivityBase,IOutgameTaskActivity
    {
        readonly OutgameTaskServices tasks;
        public OutgameTaskManager Manager;
        public Dictionary<int,OutgameChildTaskActivity> TaskViewActivities,OtherViewActivities;
        public Dictionary<int,OutgameChildTaskData> TaskViewDic=new Dictionary<int,OutgameChildTaskData>();
        public Dictionary<int,OutgameChildTaskData> OtherViewDic=>new Dictionary<int,OutgameChildTaskData>();
        public bool Dirty;
        public OutgameTaskActivity(OutgameActivityServices services,OutgameTaskServices tasks):base(services){this.tasks=tasks;}
        public override OutgameActivityFactory ActivityFactoryBase(int id)=>new OutgameTaskFactory(id,Services.ItemConfig,Services.ItemFactoryError,tasks.Entities,Services.Common);
        public override void Update(){if(Dirty&&tasks.Pool().IsEnableSaveData){var manager=Manager;if(manager!=null)manager.OnSave();Dirty=false;}}
        public override void OnInit()
        {
            Manager=tasks.Pool().GetModel<OutgameTaskManager>(4692,"TaskMgr");
            TaskViewActivities=new Dictionary<int,OutgameChildTaskActivity>();OtherViewActivities=new Dictionary<int,OutgameChildTaskActivity>();
            foreach(var row in tasks.Config().Activities.Values)
            {
                if(!tasks.Config().TaskActivities.ContainsKey(row.id))continue;
                bool attached=false;
                if(row.parentActivityID!=null&&row.parentActivityID.Length!=0)
                {
                    var child=new OutgameChildTaskActivity(Services,tasks);
                    for(int i=0;i<row.parentActivityID.Length;i++)
                    {
                        if(row.parentActivityID[i]!=1300001)continue;
                        Children.Add(child);ChildrenById.Add(row.id,child);TaskViewActivities.Add(row.id,child);tasks.Control().ChildActivities.Add(row.id,child);
                        if(Manager.ChildTaskDic.TryGetValue(row.id,out var data)){child.EarlyOnInit(data);child.Refresh(row.id);}
                        attached=true;break;
                    }
                }
                if(attached)continue;
                if(Manager.ChildTaskDic.TryGetValue(row.id,out var other))
                {
                    var child=new OutgameChildTaskActivity(Services,tasks);tasks.Control().ChildActivities.Add(row.id,child);OtherViewActivities.Add(row.id,child);
                    child.EarlyOnInit(other);child.Refresh(row.id);
                }
            }
            foreach(var child in TaskViewActivities.Values)if(child.Data.state==3)TaskViewDic[child.ModuleData.activityID]=child.ModuleData;
            SortTaskViewDic();
            foreach(var child in OtherViewActivities.Values)if(child.Data.state==3)OtherViewDic[child.ModuleData.activityID]=child.ModuleData;
            AddListener();
        }
        public void AddListener()=>Services.Common().AddListener("CommonActivity_Launch",ActivityLaunch);
        public void RemoveListener()=>Services.Common().RemoveListener("CommonActivity_Launch",ActivityLaunch);
        public void RefreshData(OutgameTaskData data)
        {
            for(int i=0;i<data.datas.Count;i++)
            {
                var config=tasks.Config().GetActivityConfig(data.datas[i].Config.id);
                if(config.parentActivityID!=null&&config.parentActivityID.Length!=0)
                    for(int j=0;j<config.parentActivityID.Length;j++)
                    {
                        var view=config.parentActivityID[j]==1300001?TaskViewDic:OtherViewDic;
                        view[data.datas[i].Config.id]=data.datas[i];
                    }
                var child=GetChildActivity<OutgameChildTaskActivity>(data.datas[i].activityID);
                if(child!=null)child.UpdateData(data.datas[i]);
            }
        }
        public void ActivityLaunch(object[] args)
        {
            if(args==null||args.Length==0)return;int id=(int)args[0];
            if(TaskViewActivities.TryGetValue(id,out var child)&&!TaskViewDic.ContainsKey(id))
            {TaskViewDic.Add(id,child.ModuleData);SortTaskViewDic();Services.Common().SendMessage("CommonModule_AddLaunchTaskViewActivity");}
            if(OtherViewActivities.TryGetValue(id,out var other)&&!OtherViewDic.ContainsKey(id))
            {OtherViewDic.Add(id,other.ModuleData);Services.Common().SendMessage("CommonModule_AddLaunchTaskOhterActivity",new object[]{other.ModuleData});}
        }
        public void SortTaskViewDic()
        {
            var ordered=TaskViewDic.OrderBy(pair=>pair.Value.Config.param);var replacement=new Dictionary<int,OutgameChildTaskData>();
            foreach(var pair in ordered)replacement.Add(pair.Key,pair.Value);TaskViewDic=replacement;
        }
        public void SaveData(){foreach(var child in TaskViewActivities.Values)child.SaveData();foreach(var child in OtherViewActivities.Values)child.SaveData();}
        public override void Dispose()=>RemoveListener();
    }
    // ChildTaskActivity4676. Reinitialization creates a new strategy without disposing the old one.
    public sealed class OutgameChildTaskActivity:OutgameActivityBase
    {
        public readonly OutgameTaskServices Tasks;
        public OutgameTaskNetStrategy Strategy;
        public OutgameChildTaskData ModuleData;
        public Dictionary<int,OutgameTaskGroupData> Groups=new Dictionary<int,OutgameTaskGroupData>();
        public Dictionary<long,OutgameTaskItemData> TasksByUid=new Dictionary<long,OutgameTaskItemData>();
        public OutgameChildTaskActivity(OutgameActivityServices services,OutgameTaskServices tasks):base(services){Tasks=tasks;}
        public void EarlyOnInit(OutgameChildTaskData data){ModuleData=data;}
        public override OutgameActivityFactory ActivityFactoryBase(int id)=>new OutgameTaskItemFactory(id,Services.ItemConfig,Services.ItemFactoryError,Tasks.Entities);
        public override void OnInit(){Reindex();Strategy=new OutgameChildTaskOffStrategy(Tasks);Strategy.InitStrategy(this);}
        void Reindex()
        {
            TasksByUid.Clear();Groups.Clear();
            for(int i=0;i<ModuleData.tasks.Count;i++)TasksByUid.Add(ModuleData.tasks[i].uid,ModuleData.tasks[i]);
            for(int i=0;i<ModuleData.ext.groupDatas.Count;i++)Groups.Add(ModuleData.ext.groupDatas[i].groupID,ModuleData.ext.groupDatas[i]);
        }
        public void UpdateData(OutgameChildTaskData data)
        {ModuleData=data;Reindex();Strategy.RemoveListeners();Strategy.AddListeners();Services.Common().SendMessage(OutgameTaskNetStrategy.RefreshList,new object[]{ModuleData.activityID,ModuleData.tasks});}
        public OutgameTaskItemData GetTaskItemData(long uid)
        {if(TasksByUid.TryGetValue(uid,out var task))return task;Tasks.Warning(new object[]{string.Format("不包含id为{0}的任务",uid)});return null;}
        public override void Update(){}
        public override void ResetProgress()=>Strategy.ResetProgress();
        public override void Launch()=>Strategy.Launch();
        public void GetLivenessReward(int id)=>Strategy.GetLivenessReward(id);
        public void TaskComplete(long uid)=>Strategy.TaskComplete(uid,true);
        public void SaveData()=>Strategy.SaveData();
        public override void Dispose()=>Strategy.Dispose();
    }
}
