using System;
using System.Collections.Generic;
using System.Linq;
namespace AreaBattle
{
    public interface IOutgameDailyTaskPage
    {
        OutgameTaskRow GetTaskItem();
        void SetDailyProgress(float value);
        void SetTaskEmpty(bool empty);
        void RefreshPointDailyLivenessBar();
        void SetDailyCountdown(string value);
    }
    public sealed class OutgameDailyTaskViewServices
    {
        public Func<OutgameChildTaskActivity> DailyActivity;
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameCommonMessageDispatcher> Common=()=>OutgameCommonMessageDispatcher.Shared;
        public Func<int,object[],long> GameValue;
        public Func<int> DailyActivityId;
        public Func<int,string> FormatHours;
        public Action ClearSingleton;
    }
    // DailyTaskSubUI3988. Singleton creation and TaskPanelUI lifecycle own initialization.
    public sealed class OutgameDailyTaskView
    {
        readonly OutgameDailyTaskViewServices services;
        public IOutgameDailyTaskPage RootUI;
        public bool LateInited;
        public float MaximumLiveness;
        public OutgameChildTaskActivity Child;
        public readonly Dictionary<int,OutgameTaskRow> Rows=new Dictionary<int,OutgameTaskRow>();
        public OutgameDailyTaskView(OutgameDailyTaskViewServices services){this.services=services;}
        public void OnInit()
        {
            Child=services.DailyActivity();services.Common().AddListener("CommonModule_ResetTimeRefresh",RefreshCountdown);
            MaximumLiveness=services.Config().GetLivenessDicByActivityId(Child.Config.id).Max(row=>row.Value.livenessValue);
        }
        public void OnLateInit(){RootUI.SetDailyProgress((float)Child.ModuleData.ext.livenessValue/MaximumLiveness);RefreshRows();}
        public List<OutgameTaskItemData> Filter(List<OutgameTaskItemData> tasks)
        {
            var result=new List<OutgameTaskItemData>();
            foreach(var task in tasks)
            {long value=services.GameValue(900001,Array.Empty<object>());if(value==0&&task.id==8)continue;result.Add(task);}
            return result;
        }
        public void RefreshRows()
        {
            var tasks=Filter(Child.ModuleData.tasks);bool empty=true;
            for(int i=0;i<tasks.Count;i++)
            {
                var task=tasks[i];var stateTask=tasks[i];
                if(stateTask.state==1)
                {if(Rows.TryGetValue(task.id,out var claimed)){claimed.ActionCache=false;claimed.Lifetime.GameObject.SetActive(false);}continue;}
                if(!Rows.TryGetValue(task.id,out var row))
                {row=RootUI.GetTaskItem();row.SetData(new OutgameTaskRowData(),OutgameTaskType.DailyTask,Claim);Rows.Add(task.id,row);}
                row.Data.rewards=task.Config.rewards;row.Data.dis=task.Config.des;row.Data.liveness=task.Config.liveness;
                row.Data.number=task.Config.conditionParams[0].datas[1];row.Data.ContentArgument=0;
                row.Data.contentType=task.Config.conditionParams[0].datas[0];
                // Source i64.store32 truncates the first condition value to the display's Int32.
                if(task.conditions.Count>=1)row.Data.prog=unchecked((int)task.conditions[0].value);
                empty=false;row.Data.id=task.Config.id;row.Data.Uid=task.uid;row.Refresh();
            }
            RootUI.SetTaskEmpty(empty);
        }
        public void Claim(int id,long uid)
        {
            Child.Strategy.TaskComplete(uid,true);
            if(Rows.TryGetValue(id,out var row))
            {row.ActionCache=false;row.Lifetime.GameObject.SetActive(false);RootUI.SetDailyProgress((float)Child.ModuleData.ext.livenessValue/MaximumLiveness);RootUI.RefreshPointDailyLivenessBar();}
        }
        public void ClaimLiveness(int id)=>Child.Strategy.GetLivenessReward(id);
        public int Liveness=>Child.ModuleData.ext.livenessValue;
        public List<OutgameTaskLivenessItemData> LivenessItems=>Child.ModuleData.ext.GetLivenessItems();
        public void RefreshCountdown(object[] args)
        {
            if(RootUI==null)return;int id=(int)args[0];long remaining=(long)args[1];
            if(id!=services.DailyActivityId())return;RootUI.SetDailyCountdown(services.FormatHours(unchecked((int)(remaining/1000L))));
        }
        public void OnDestroy(){services.ClearSingleton();services.Common().RemoveListener("CommonModule_ResetTimeRefresh",RefreshCountdown);}
    }
}
