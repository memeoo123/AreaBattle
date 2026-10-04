using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    // Source4727 and concrete NetStratrgyBase<Data,Activity,Manager> instantiation3386.
    public abstract class OutgameAchievementStrategy
    {
        public const string RefreshExt="CommonModule_Achievement_RefreshExt",RefreshList="CommonModule_Achievement_RefreshList",RefreshTypeList="CommonModule_Achievement_RefreshTypeList",StatisticsRefresh="CommonModule_StatisticsData_Refresh";
        protected readonly OutgameAchievementServices Services;
        public OutgameAchievementManager Manager;
        public OutgameAchievementData Data;
        public int ActivityId;
        public Action<OutgameAchievementData> UpdateDataAction;
        OutgameAchievementActivity activity;
        public List<string> MessageKeys=new List<string>();
        public OutgameAchievementActivity Activity=>activity??(activity=Services.Control().GetActivity<OutgameAchievementActivity>(ActivityId));
        protected OutgameAchievementStrategy(OutgameAchievementServices services){Services=services;}
        public void InitData(Action<OutgameAchievementData> callback,int id)
        {ActivityId=id;UpdateDataAction=callback;Manager=Services.Pool().GetModel<OutgameAchievementManager>(4722,"AchievementMgr");if(Services.Config().Activities.ContainsKey(id))OnInit();}
        public virtual void OnInit(){}
        public abstract void LoadData(string text);
        public virtual void ResetProgress(){}
        public virtual void Launch(){}
        public abstract void GetAchievementReward(int id);
        public abstract void ChangeProgress(int id,long value);
        public abstract void OnSave();
        public virtual void Dispose()=>RemoveListeners();
        public void AddListeners()
        {
            if(MessageKeys.Count!=0)return;
            for(int i=0;i<Data.datas.Count;i++)
            {
                var config=Services.Config().GetAchievementConfig(Data.datas[i].id);if(config==null)continue;
                if(!MessageKeys.Contains(OutgameStatisticsMessageKey.Get(config.contentType)))MessageKeys.Add(OutgameStatisticsMessageKey.Get(config.contentType));
            }
            for(int i=0;i<MessageKeys.Count;i++)Services.Common().AddListener(MessageKeys[i],StatisticsEvent);
        }
        public void RemoveListeners()
        {
            for(int i=0;i<Data.datas.Count;i++)
            {var config=Services.Config().GetAchievementConfig(Data.datas[i].id);if(config!=null)Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(config.contentType),StatisticsEvent);}
        }
        public void StatisticsEvent(object[] args)
        {
            if(args==null||args.Length<1)return;int key=Convert.ToInt32(args[args.Length-1]);long delta=(long)args[0];int filter=args.Length>=3?(int)args[1]:-1;
            var types=new List<int>();bool changed=false;
            // ChangeProgress replaces/sorts Data.datas; source intentionally keeps a live index loop.
            for(int i=0;i<Data.datas.Count;i++)
            {
                var config=Data.datas[i].Config;if(config.contentType!=key||(config.content!=0&&config.content!=filter))continue;
                if(!types.Contains(config.type))types.Add(config.type);
                long value=unchecked(Data.datas[i].progress+(delta<0?unchecked(-delta):delta));
                ChangeProgress(Data.datas[i].id,value);changed=true;
            }
            if(!changed)return;Manager.RefreshSortList();
            for(int i=0;i<types.Count;i++)Activity.RefreshTypeListSort(types[i]);
        }
    }
    // Original AchievementOffStrategy4725. No per-frame save or invented claim transaction.
    public sealed class OutgameAchievementOffStrategy:OutgameAchievementStrategy
    {
        static readonly Color ProgressColor=new Color(1f,0.9215686321258545f,0.01568627543747425f,1f);
        public OutgameAchievementOffStrategy(OutgameAchievementServices services):base(services){}
        public override void OnInit()
        {
            Manager.UpdateData(true);
            Services.Common().RemoveListener(StatisticsRefresh,RefreshTaskProgress);Services.Common().AddListener(StatisticsRefresh,RefreshTaskProgress);
            Services.Common().RemoveListener(OutgameAchievementPoint.PointAdded,PointAdded);Services.Common().AddListener(OutgameAchievementPoint.PointAdded,PointAdded);
        }
        public override void Dispose()
        {RemoveListeners();Services.Common().RemoveListener(StatisticsRefresh,RefreshTaskProgress);Services.Common().RemoveListener(OutgameAchievementPoint.PointAdded,PointAdded);}
        public void ReadStoredData(string text)
        {
            var saved=JsonUtility.FromJson<OutgameAchievementSaveData>(text);
            if(saved!=null&&saved.ids!=null&&saved.ids.Count>0)
            {
                Data=new OutgameAchievementData();Data.ext=saved.ext;
                if(saved.ids.Count>saved.times.Count)for(int i=saved.times.Count;i<saved.ids.Count;i++)saved.times.Add(0);
                for(int i=0;i<saved.ids.Count;i++)Data.datas.Add(new OutgameAchievementItemData{id=saved.ids[i],state=1,progress=0,time=saved.times[i]});
            }
            else Data=JsonUtility.FromJson<OutgameAchievementData>(text);
        }
        public override void LoadData(string text)
        {
            ReadStoredData(text);
            if(Data==null)
            {
                Data=new OutgameAchievementData();
                foreach(var config in Services.Config().Achievements.Values)Data.datas.Add(new OutgameAchievementItemData{id=config.id,state=0,progress=0});
            }
            else
            {
                for(int i=0;i<Data.datas.Count;i++)if(!Services.Config().Achievements.ContainsKey(Data.datas[i].id))Data.datas.RemoveAt(i--);
                foreach(var config in Services.Config().Achievements.Values.ToList())
                    if(!Data.datas.Exists(item=>item.id==config.id))Data.datas.Add(new OutgameAchievementItemData{id=config.id,state=0,progress=0});
            }
            AddListeners();RefreshTaskProgress(null);
        }
        public void RefreshTaskProgress(object[] args)
        {
            foreach(var item in Data.datas){var config=item.Config;item.progress=Services.Statistics.GameValue(config.contentType,new object[]{config.content});}
            UpdateDataAction?.Invoke(Data);
        }
        public string SerializeClaimed()
        {
            var saved=new OutgameAchievementSaveData{ext=Data.ext,ids=new List<int>(),times=new List<long>()};
            for(int i=Data.datas.Count-1;i>=0;i--)if(Data.datas[i].state==1){saved.ids.Add(Data.datas[i].id);saved.times.Add(Data.datas[i].time);}
            return JsonUtility.ToJson(saved);
        }
        public override void OnSave()
        {var data=Data;data.datas=new List<OutgameAchievementItemData>(Activity.AchievementMap.Values);var manager=Manager;string text=SerializeClaimed();manager.SaveLocalData(text);}
        public override void ResetProgress()
        {
            Data.ext.statePts=0;Data.ext.accPts=0;
            for(int i=0;i<Data.datas.Count;i++){Data.datas[i].progress=0;Data.datas[i].time=0;Data.datas[i].state=0;}
            Activity.GetAccItems();Activity.RefreshAccItems();
            foreach(var pair in Activity.TypeLists)Activity.RefreshTypeListSort(pair.Key);
            Services.Common().SendMessage(RefreshExt,new object[]{Data.ext});
        }
        public void PointAdded(object[] args)
        {
            if(args==null||args.Length<1)return;int delta=(int)args[0];var ext=Data.ext;int old=ext.accPts;ext.accPts=unchecked(old+delta);int value=Data.ext.accPts,stage=0;
            foreach(var row in Services.Config().AchievementAcc.Values)if(row.id<=value)stage=row.id;
            var report=Services.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);report.ParentName="成就";report.ActivityName=string.Format("成就点阶段{0}",stage);report.OldValues[0]=old;report.Values[0]=value;Services.Reports().Send(OutgameLimitTaskReportKind.Success,report);
        }
        public override void ChangeProgress(int id,long value)
        {
            var item=Activity.FindAchievement(id);long old=item.progress;item.progress=value;
            Services.LogByColor(ProgressColor,new object[]{"成就进度变化","id",item.id,"进度",value});
            if(old<item.Config.number&&value!=old)
            {
                var report=Services.Reports().Create(OutgameLimitTaskReportKind.Success,true,null);report.ParentName="成就";report.ActivityName=string.Format("成就{0}",item.id);report.OldValues[0]=unchecked((int)old);report.Values[0]=unchecked((int)value);Services.Reports().Send(OutgameLimitTaskReportKind.Success,report);
            }
            var data=Data;data.datas=new List<OutgameAchievementItemData>(Activity.AchievementMap.Values);Data.datas.Sort((a,b)=>a.CompareTo(b));
        }
        public override void GetAchievementReward(int id)
        {
            if(!Activity.AchievementMap.TryGetValue(id,out var item))return;var config=item.Config;if(!item.CanComplete())return;
            var rewards=item.GetRewards();
            for(int i=0;i<rewards.Count;i++)
            {var itemConfig=Services.Items().GetGameItemConfig(rewards[i].itemId);var entity=Services.Rewards().GetItem(itemConfig);if(entity!=null)entity.AddItem(rewards[i].itemCount);}
            item.state=1;item.time=Services.Statistics.GameValue(10000,Array.Empty<object>());
            var report=Services.Reports().Create(OutgameLimitTaskReportKind.Reward,true,null);report.ParentName="成就";report.ActivityName=string.Format("成就{0}",item.id);report.Values[0]=Data.ext.accPts;Services.Reports().Send(OutgameLimitTaskReportKind.Reward,report);
            var group=Services.Config().AchievementGroups[config.type][new OutgameActivityConditionPriority(config.contentType,config.content)];int index=group.IndexOf(config);if(index!=group.Count-1)index++;
            var list=Activity.GetAchievementsListByType(config.type);int slot=list.IndexOf(item);var next=group[index];item=Activity.FindAchievement(next.id);list[slot]=item;
            Activity.RefreshSortList();Activity.RefreshTypeListSort(item.Config.type);Services.Common().SendMessage(RefreshExt,new object[]{Data.ext});
        }
    }
}
