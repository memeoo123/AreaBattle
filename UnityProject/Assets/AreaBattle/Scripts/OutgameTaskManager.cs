using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Required TaskActivity4685 endpoints; the concrete activity supplies its own business logic.
    public interface IOutgameTaskActivity
    {void RefreshData(OutgameTaskData data);void SaveData();}
    public sealed class OutgameTaskManagerServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameDataManagerPool> Pool;
        public Func<int,IOutgameTaskActivity> GetActivity;
        public Action<string> Warning;
        public Action<object[]> Error,Log;
    }
    // Original TaskMgr4692, manually registered by ActivityManager under CommonGameModule.
    public sealed class OutgameTaskManager:OutgameCommonModuleManager
    {
        public readonly OutgameTaskManagerServices Services;
        public OutgameTaskData Data;
        public OutgameTaskOffStrategy Strategy;
        public Dictionary<int,OutgameChildTaskData> ChildTaskDic=new Dictionary<int,OutgameChildTaskData>();
        public override int ActivityId=>1300001;
        public override string SourceClassName=>"TaskMgr";
        public OutgameTaskManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,OutgameTaskManagerServices services)
            :base(storage,host,download){Services=services;}
        public override void OnInit()=>Strategy=new OutgameTaskOffStrategy(Services);
        public override void InitStrategy(){var strategy=Strategy;if(strategy!=null)strategy.InitData(UpdateCallback,1300001);}
        public override void UpdateDataCallBack(string text)=>Strategy.LoadData(text);
        public void UpdateCallback(OutgameTaskData data)
        {
            Data=data;ChildTaskDic.Clear();
            for(int i=0;i<data.datas.Count;i++)ChildTaskDic.Add(data.datas[i].activityID,data.datas[i]);
            var activity=Services.GetActivity(1300001);if(activity!=null)activity.RefreshData(data);
        }
        public override void OnRelease(){var strategy=Strategy;if(strategy!=null)strategy.Dispose();}
        public override void OnSave(){var strategy=Strategy;if(strategy!=null)strategy.OnSave();}
    }
    // Source TaskOffStrategy4696 and the concretely resolved NetStratrgyBase`3 methods34690..34697.
    public sealed class OutgameTaskOffStrategy
    {
        readonly OutgameTaskManagerServices services;
        public OutgameTaskManager Manager;
        public OutgameTaskData Data;
        public int ActivityId;
        public Action<OutgameTaskData> UpdateDataAction;
        IOutgameTaskActivity activity;
        public OutgameTaskOffStrategy(OutgameTaskManagerServices services){this.services=services;}
        public IOutgameTaskActivity Activity=>activity??(activity=services.GetActivity(ActivityId));
        public void InitData(Action<OutgameTaskData> callback,int id)
        {
            ActivityId=id;UpdateDataAction=callback;
            Manager=services.Pool().GetModel<OutgameTaskManager>(4692,"TaskMgr");
            if(services.Config().Activities.ContainsKey(id))UpdateManagerData(true);
        }
        public void UpdateManagerData(bool allowServer)=>Manager.UpdateData(allowServer);
        public void Dispose(){}
        public void OnSave()
        {
            string text=OutgameActivityCodec.CompressString(JsonUtility.ToJson(JsonUtility.FromJson<OutgameTaskData>(JsonUtility.ToJson(Data))));
            Activity.SaveData();Manager.SaveLocalData(text);
        }
        public void LoadData(string text)
        {
            try{Data=JsonUtility.FromJson<OutgameTaskData>(OutgameActivityCodec.DecompressString(text,services.Warning));}
            catch(Exception){Data=JsonUtility.FromJson<OutgameTaskData>(text);}
            if(Data==null)
            {
                Data=new OutgameTaskData();
                foreach(var row in services.Config().TaskActivities.Values)
                {
                    var child=new OutgameChildTaskData{activityID=row.id};child.ext.activityID=row.id;
                    Data.datas.Add(child);child.ResetExtraRefreshTimes();
                }
            }
            else RepairExisting();
            foreach(var pair in services.Config().TaskGroupsByActivity)
            {
                if(Data.datas.Exists(child=>child.activityID==pair.Key))continue;
                var child=new OutgameChildTaskData{activityID=pair.Key};child.ext.activityID=pair.Key;
                child.ResetExtraRefreshTimes();Data.datas.Add(child);
            }
            UpdateDataAction?.Invoke(Data);
        }
        void RepairExisting()
        {
            for(int i=0;i<Data.datas.Count;i++)
            {
                Data.datas[i].ext.activityID=Data.datas[i].activityID;
                if(!services.Config().TaskGroupsByActivity.ContainsKey(Data.datas[i].activityID))
                {Data.datas.RemoveAt(i--);continue;}
                if(Data.datas[i].ext.extraRefreshNum!=-1)
                {
                    for(int j=0;j<Data.datas[i].ext.groupDatas.Count;j++)
                    {
                        if(!services.Config().TaskGroups.ContainsKey(Data.datas[i].ext.groupDatas[j].groupID))
                        {Data.datas[i].ext.groupDatas.RemoveAt(j--);continue;}
                        Data.datas[i].ext.groupDatas[j].ResetExtraRefreshTimes();
                    }
                    Data.datas[i].ext.extraRefreshNum=-1;
                }
                for(int j=0;j<Data.datas[i].tasks.Count;j++)
                {
                    if(!services.Config().Tasks.ContainsKey(Data.datas[i].tasks[j].id))
                    {
                        services.Error(new object[]{string.Format("版本升级，任务表id:{0}被删除了，请检查！！！",Data.datas[i].tasks[j].id)});
                        Data.datas[i].tasks.RemoveAt(j--);continue;
                    }
                    if(Data.datas[i].tasks[j].progress!=0)
                    {
                        var condition=new OutgameTaskCondition();
                        condition.arg=new int[Data.datas[i].tasks[j].Config.conditionParams[0].datas.Length-2];
                        condition.key=Data.datas[i].tasks[j].Config.conditionParams[0].datas[0];
                        condition.value=Data.datas[i].tasks[j].progress;
                        Array.Copy(Data.datas[i].tasks[j].Config.conditionParams[0].datas,1,condition.arg,0,Data.datas[i].tasks[j].Config.conditionParams[0].datas.Length-2);
                        Data.datas[i].tasks[j].conditions.Add(condition);Data.datas[i].tasks[j].progress=0;
                    }
                    else
                    {
                        for(int k=0;k<Data.datas[i].tasks[j].Config.conditionParams.Count;k++)
                        {
                            var condition=new OutgameTaskCondition();
                            condition.arg=new int[Data.datas[i].tasks[j].Config.conditionParams[k].datas.Length-2];
                            condition.key=Data.datas[i].tasks[j].Config.conditionParams[k].datas[0];
                            Array.Copy(Data.datas[i].tasks[j].Config.conditionParams[k].datas,1,condition.arg,0,Data.datas[i].tasks[j].Config.conditionParams[k].datas.Length-2);
                            if(Data.datas[i].tasks[j].conditions.Exists(row=>row.KeyEqual(condition)))continue;
                            Data.datas[i].tasks[j].conditions.Add(condition);
                            services.Log(new object[]{"添加任务条件:"+Data.datas[i].tasks[j].id,Data.datas[i].tasks[j].GetDes(),condition.key});
                        }
                    }
                    if(Data.datas[i].tasks[j].uid==0)Data.datas[i].tasks[j].uid=Data.datas[i].tasks[j].GetHashCode();
                }
            }
        }
    }
}
