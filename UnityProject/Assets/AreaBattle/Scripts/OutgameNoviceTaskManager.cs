using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    // Persisted fields and constructor defaults of source types4701..4704 and4707.
    // Task getters/reward behavior are restored separately from this storage schema.
    [Serializable] public sealed class OutgameLimitTimeTaskData
    {public List<OutgameLimitTimeTaskChildData> datas=new List<OutgameLimitTimeTaskChildData>();}
    [Serializable] public sealed class OutgameLimitTimeTaskChildData
    {
        public List<OutgameLimitTaskItemData> datas=new List<OutgameLimitTaskItemData>();
        public OutgameLimitTimeTaskExt ext=new OutgameLimitTimeTaskExt();
    }
    [Serializable] public sealed partial class OutgameLimitTimeTaskExt
    {public int activityId,dayId;public long dayFlag,accFlag,launchTime;public int lastClickDayId;}
    [Serializable] public sealed partial class OutgameLimitTaskItemData
    {public int id;public List<OutgameLimitTaskCondition> conditions=new List<OutgameLimitTaskCondition>();public int state;}
    [Serializable] public sealed class OutgameLimitTaskCondition
    {public int key;public long value;public int[] arg;}
    // Required concrete LimitTimeTaskActivity endpoints; no default reward or task behavior.
    public interface IOutgameNoviceTaskActivity
    {void RefreshData();void OnSave();}
    public sealed class OutgameNoviceTaskManagerServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Action<string> Warning;
        public Action<object[]> Error;
    }
    // Original NoviceTaskManager4711, CommonGameModule registration, autoSyn=true/compressData=false.
    public sealed class OutgameNoviceTaskManager:OutgameCommonModuleManager
    {
        public readonly OutgameNoviceTaskManagerServices Services;
        public OutgameLimitTimeTaskData Data;
        public IOutgameNoviceTaskActivity Activity;
        public Dictionary<int,OutgameLimitTimeTaskChildData> LimitTimeTaskDic=new Dictionary<int,OutgameLimitTimeTaskChildData>();
        public override int ActivityId=>1301001;
        public override string SourceClassName=>"NoviceTaskManager";
        public OutgameNoviceTaskManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,OutgameNoviceTaskManagerServices services)
            :base(storage,host,download){Services=services;}
        public override void OnInit()=>UpdateData(true);
        public override void InitStrategy(){}
        public override void OnRelease(){}
        OutgameLimitTimeTaskChildData NewChild(int id)
        {
            var child=new OutgameLimitTimeTaskChildData();child.ext.activityId=id;child.ext.dayId=1;
            var tasks=Services.Config().GetNTTaskConfigDicByActivityId(id);
            if(tasks.Count==0)Services.Error(new object[]{string.Format("PubnoviceTaskConfig表中不包含活动id为{0}的配置",id)});
            else child.ext.lastClickDayId=tasks.First().Value.day;
            return child;
        }
        public override void UpdateDataCallBack(string text)
        {
            try{Data=JsonUtility.FromJson<OutgameLimitTimeTaskData>(OutgameActivityCodec.DecompressString(text,Services.Warning));}
            catch(Exception){Data=JsonUtility.FromJson<OutgameLimitTimeTaskData>(text);}
            if(Data==null)
            {
                Data=new OutgameLimitTimeTaskData();
                foreach(var pair in Services.Config().NoviceTasksByActivity)
                {var child=NewChild(pair.Key);Data.datas.Add(child);}
            }
            int i=0;OutgameActivityConfigManager config;
            while(true)
            {
                int count=Data.datas.Count;config=Services.Config();
                if(i>=count)break;
                if(!config.NoviceActivities.ContainsKey(Data.datas[i].ext.activityId))
                {
                    Services.Error(new object[]{string.Format("限时X任务子活动表ID{0}不可删除，请策划对比上个版本的配置表",Data.datas[i].ext.activityId)});
                    Data.datas.RemoveAt(i);i--;
                }
                i++;
            }
            foreach(var pair in config.NoviceTasksByActivity)
            {
                if(Data.datas.Exists(child=>child.ext.activityId==pair.Key))continue;
                var child=NewChild(pair.Key);Data.datas.Add(child);
            }
            UpdateCallback();
        }
        public void UpdateCallback()
        {
            LimitTimeTaskDic.Clear();
            for(int i=0;i<Data.datas.Count;i++)LimitTimeTaskDic[Data.datas[i].ext.activityId]=Data.datas[i];
            var activity=Activity;if(activity!=null)activity.RefreshData();
        }
        public override void OnSave()
        {
            var activity=Activity;if(activity!=null)activity.OnSave();
            SaveLocalData(OutgameActivityCodec.CompressString(JsonUtility.ToJson(Data)));
        }
    }
}
