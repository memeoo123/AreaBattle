using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Source4683. Event arguments are boxed object[]; exact Int32/Int64 distinctions survive.
    public abstract class OutgameTaskNetStrategy
    {
        public const string RefreshList="CommonModule_RefreshTaskList",RefreshExt="CommonModule_ExtRefresh",LivenessPrefix="CommonGameModule_Task_LivenessPointAdd";
        protected readonly OutgameTaskServices Tasks;
        public OutgameChildTaskActivity Activity;
        public readonly List<string> MessageKeys=new List<string>();
        public OutgameChildTaskData Data=>Activity.ModuleData;
        protected OutgameActivityServices Services=>Activity.Services;
        protected OutgameTaskNetStrategy(OutgameTaskServices tasks){Tasks=tasks;}
        public void InitStrategy(OutgameChildTaskActivity activity){Activity=activity;OnInit();AddListeners();}
        public virtual void OnInit(){}
        public virtual void ResetProgress(){}
        public virtual void TimeEvent(object[] args){}
        public virtual void Launch(){}
        public abstract void ChangeTaskProgress(long uid,long value,int key,object[] args);
        public abstract void TaskComplete(long uid,bool notify);
        public abstract void GetLivenessReward(int id);
        public abstract void SaveData();
        public virtual void Dispose()=>RemoveListeners();
        public void AddListeners()
        {
            Services.Common().AddListener(OutgameStatisticsMessageKey.Get(10000),TimeEvent);
            if(MessageKeys.Count!=0)return;
            for(int i=0;i<Data.tasks.Count;i++)
            {
                var config=Tasks.Config().GetTaskConfig(Data.tasks[i].id);if(config==null)continue;
                for(int j=0;j<config.conditionParams.Count;j++)
                {string key=OutgameStatisticsMessageKey.Get(config.conditionParams[j].datas[0]);if(!MessageKeys.Contains(key))MessageKeys.Add(OutgameStatisticsMessageKey.Get(config.conditionParams[j].datas[0]));}
            }
            for(int i=0;i<MessageKeys.Count;i++)Services.Common().AddListener(MessageKeys[i],StatisticsEvent);
        }
        public void RemoveListeners()
        {
            Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(10000),TimeEvent);
            for(int i=0;i<Data.tasks.Count;i++)for(int j=0;j<Data.tasks[i].conditions.Count;j++)
                Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(Data.tasks[i].conditions[j].key),StatisticsEvent);
            MessageKeys.Clear();
        }
        public void StatisticsEvent(object[] args)
        {
            try
            {
                if(args==null||args.Length<1)return;int key=Convert.ToInt32(args[args.Length-1]);
                var touched=new List<OutgameTaskItemData>();bool changed=false;
                foreach(var task in Data.tasks)
                {
                    var config=task.Config;if(config.allLifeStatistics==0&&Activity.Data.state!=3)continue;
                    if(task.receiveState==0)continue;
                    if(args.Length==3)
                    {
                        int filter=Convert.ToInt32(args[1]);
                        for(int i=0;i<task.conditions.Count;i++)
                        {
                            if(task.conditions[i].key!=key||filter!=task.conditions[i].arg[0])continue;
                            long value=config.allLifeStatistics==0?unchecked(task.conditions[i].value+(long)args[0]):Services.Statistics.GameValue(task.conditions[i].key,new object[]{task.conditions[i].arg[0]});
                            touched.Add(task);ChangeTaskProgress(task.uid,Math.Max(value,0L),key,new object[]{filter});changed=true;
                        }
                    }
                    else for(int i=0;i<task.conditions.Count;i++)
                    {
                        if(task.conditions[i].key!=key)continue;
                        long value=config.allLifeStatistics==0?unchecked(task.conditions[i].value+(long)args[0]):Services.Statistics.GameValue(task.conditions[i].key,Array.Empty<object>());
                        touched.Add(task);ChangeTaskProgress(task.uid,Math.Max(value,0L),key,Array.Empty<object>());changed=true;
                    }
                }
                if(!changed)return;
                Data.tasks.Sort((a,b)=>a.CompareTo(b));Services.Common().SendMessage(RefreshList,new object[]{Data.activityID,Data.tasks});
            }
            catch(Exception exception){Tasks.Error(new object[]{exception.Message,exception.StackTrace});}
        }
    }
}
