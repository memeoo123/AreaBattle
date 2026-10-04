using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.SharedItemConfig;
namespace AreaBattle
{
    public sealed class OutgameActivityInitData {}
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OutgameActivityControlRegister:Attribute
    {public int ActivityId{get;}public OutgameActivityControlRegister(int activityId){ActivityId=activityId;}}
    public enum OutgameActivityReportKind {Warmup,Unlock,Complete}
    // Only these two fields are assigned by ActivityControl. The report host owns the
    // original DTO's other fields, defaults, serialization and delivery.
    public class OutgameActivityReport
    {public string ParentName,ActivityName;}
    public interface IOutgameActivityReports
    {
        OutgameActivityReport Create(OutgameActivityReportKind kind,bool initialize,object argument);
        void Send(OutgameActivityReportKind kind,OutgameActivityReport report);
    }
    public sealed class OutgameActivityControlServices
    {
        public Func<OutgameActivityControl> Current;
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameActivityManager> CreateManager;
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameGlobalItemRewards> Items;
        public Func<Type[]> AssemblyTypes;
        public Func<Type,OutgameActivityBase> CreateActivity;
        public OutgameActivityServices Activity;
        public Func<OutgameMessageDispatcher> Messages;
        public Func<OutgameCommonMessageDispatcher> Common;
        public OutgameStatisticsExpansion Statistics;
        public Func<IOutgameActivityReports> Reports;
        public Func<OutgameActivityConfigRow.Name,string> Localize;
        public Action<object[]> Error;
        public Func<Action> GetDisposableActions;
        public Action<Action> SetDisposableActions;
        public Func<Action,int> AddUpdate;
        public Action<Action> RemoveUpdate;
        public Func<float> UnscaledDeltaTime;
        public Action<int,long,Action<long>,Action<long>,int> AddRefreshHandle;
        public Action<int,Action<long>,Action<long>,object[]> RemoveRefreshHandle;
    }
    // CommonGameModule ActivityControl4637 is independent of the 38 LogicModule controllers.
    public sealed class OutgameActivityControl
    {
        static OutgameActivityControl shared;
        public static OutgameActivityControl Shared=>shared??(shared=new OutgameActivityControl());
        public OutgameActivityControlServices Services;
        public OutgameActivityManager Manager;
        public Type[] Types;
        public Dictionary<int,OutgameActivityBase> Activities=new Dictionary<int,OutgameActivityBase>();
        public Dictionary<int,OutgameActivityBase> PendingActivities=new Dictionary<int,OutgameActivityBase>();
        public Dictionary<int,OutgameActivityBase> ChildActivities=new Dictionary<int,OutgameActivityBase>();
        public Dictionary<int,OutgameActivityItemData> ItemDatas=new Dictionary<int,OutgameActivityItemData>();
        public List<OutgameActivityPopEvent> NoticeQueue=new List<OutgameActivityPopEvent>(),LaunchQueue=new List<OutgameActivityPopEvent>();
        public OutgameUiPage NoticeUi,LaunchUi;
        public List<OutgameActivityBase> ActivityList;
        public Dictionary<int,List<(Button button,Text text)>> WidgetBindings=new Dictionary<int,List<(Button,Text)>>();
        public Dictionary<int,(Type notice,Type launch)> UiBindings=new Dictionary<int,(Type,Type)>();
        public Action RegisterCommonManagerDel;
        public OutgameActivityInitData InitData;
        public Action UpdateHandle;
        public float Elapsed;
        public bool RegisterDaily=true;
        public List<OutgameActivityPopEvent> SourceQueue80=new List<OutgameActivityPopEvent>();
        public OutgameActivityControl(OutgameActivityControlServices services=null){Services=services;}
        public OutgameActivityData Data=>Manager.Data;
        public bool IsDirty=>Manager!=null&&Manager.Dirty;
        public void SetDirty(bool value){if(Manager!=null)Manager.Dirty=value;}
        public OutgameActivityItemData GetItemData(int id)
        {if(Data==null||Data.datas==null)return null;ItemDatas.TryGetValue(id,out var row);return row;}
        public T GetActivity<T>(int id)where T:OutgameActivityBase
        {Activities.TryGetValue(id,out var activity);if(activity==null)ChildActivities.TryGetValue(id,out activity);return (T)activity;}
        // Original enum overload35236 logs a miss; the int overload35231 is silent.
        public T GetActivityBySourceId<T>(int id)where T:OutgameActivityBase
        {var activity=GetActivity<T>(id);if(activity==null)Services.Error(new object[]{"活动映射中不包含id：",id});return activity;}
        public void Init(OutgameActivityInitData data)
        {
            InitData=data??new OutgameActivityInitData();Cleanup();
            Services.Items().GetCommonItem-=GetCommonItem;
            Services.Items().GetCommonItem+=GetCommonItem;
            Services.SetDisposableActions(Services.GetDisposableActions()+Dispose);
            Manager=Services.CreateManager();Services.Pool().AddModel(4658,Manager,true);
            ReadConfig(ConfigReady);
        }
        public void ReadConfig(Action complete)=>Services.Config().ReadConfig(complete);
        public void ConfigReady()
        {AddListeners();RegisterActivities();ItemDatas.Clear();ActivityList=new List<OutgameActivityBase>(Activities.Values);Manager.Init();}
        public void Cleanup()
        {
            if(ActivityList!=null)for(int i=0;i<ActivityList.Count;i++)ActivityList[i]?.OnDispose();
            ItemDatas?.Clear();Activities.Clear();ChildActivities.Clear();RegisterCommonManagerDel=null;
            RemoveListeners();Services.Config().Dispose();Services.SetDisposableActions(Services.GetDisposableActions()-Dispose);
        }
        public void Dispose(){Cleanup();shared=null;}
        public void AddListeners()
        {
            Services.Messages().AddListener("OpenUI",OpenUiEvent);Services.Messages().AddListener("CloseUI",CloseUiEvent);
            Services.Messages().AddListener("GF_GameFocus",Focus);UpdateHandle=Update;Services.AddUpdate(UpdateHandle);
        }
        public void RemoveListeners()
        {
            Services.RemoveUpdate(UpdateHandle);Services.Messages().RemoveListener("OpenUI",OpenUiEvent);
            Services.Messages().RemoveListener("CloseUI",CloseUiEvent);Services.Messages().RemoveListener("GF_GameFocus",Focus);
            RegisterDaily=true;Services.RemoveRefreshHandle(0,DailyRefresh,null,Array.Empty<object>());
        }
        public void Focus(object[] args){if(args==null||args.Length==0)return;_ = args[0];}
        public void RegisterActivities()
        {
            Types=Services.AssemblyTypes();var registrations=new List<OutgameActivityControlRegister>();
            foreach(var type in Types)
            {
                if(!type.IsSubclassOf(typeof(OutgameActivityBase)))continue;
                var attribute=(OutgameActivityControlRegister)Attribute.GetCustomAttribute(type,typeof(OutgameActivityControlRegister));
                if(attribute==null)continue;registrations.Add(attribute);
                if(Services.Config().Activities.TryGetValue(attribute.ActivityId,out var config))
                {
                    if(config.parentActivityID!=null&&config.parentActivityID.Length!=0)continue;
                    var activity=Services.CreateActivity(type);activity.ActivityId=attribute.ActivityId;Activities.Add(attribute.ActivityId,activity);
                }
                else Services.Error(new object[]{"配置中不包含活动id",attribute.ActivityId});
            }
            foreach(var pair in Services.Config().Activities)
            {
                if(Activities.ContainsKey(pair.Value.id))continue;
                if(pair.Value.parentActivityID!=null&&pair.Value.parentActivityID.Length!=0)continue;
                var activity=new OutgameActivityBase(Services.Activity);activity.ActivityId=pair.Value.id;Activities.Add(pair.Value.id,activity);
            }
            foreach(var type in Types)
            {
                if(!type.IsSubclassOf(typeof(OutgameActivityBase)))continue;
                var attribute=(OutgameActivityControlRegister)Attribute.GetCustomAttribute(type,typeof(OutgameActivityControlRegister));
                if(attribute==null)continue;registrations.Add(attribute);
                if(Services.Config().Activities.TryGetValue(attribute.ActivityId,out var config))
                {
                    if(config.parentActivityID==null||config.parentActivityID.Length==0)continue;
                    var activity=Services.CreateActivity(type);ChildActivities[attribute.ActivityId]=activity;activity.ActivityId=attribute.ActivityId;
                    for(int i=0;i<config.parentActivityID.Length;i++)if(Activities.TryGetValue(config.parentActivityID[i],out var parent))
                    {parent.Children.Add(activity);parent.ChildrenById[attribute.ActivityId]=activity;}
                }
                else Services.Error(new object[]{"配置中不包含活动id",attribute.ActivityId});
            }
            foreach(var pair in PendingActivities)Activities[pair.Key]=pair.Value;
            PendingActivities.Clear();
        }
        public void DataReady()
        {
            if(RegisterDaily){Services.AddRefreshHandle(0,Data.lastRefreshTimeStamp,DailyRefresh,null,86400);RegisterDaily=false;}
            for(int i=0;i<Data.datas.Count;i++)ItemDatas[Data.datas[i].id]=Data.datas[i];
            for(int i=0;i<Data.datas.Count;i++)
            {
                if(!Activities.ContainsKey(Data.datas[i].id))continue;
                var activity=GetActivity<OutgameActivityBase>(Data.datas[i].id);
                WidgetBindings.TryGetValue(Data.datas[i].id,out var widgets);
                if(widgets!=null)for(int j=0;j<widgets.Count;j++)BindWidget(activity,widgets[j].button,widgets[j].text);
                if(UiBindings.TryGetValue(Data.datas[i].id,out var ui)){activity.NoticePopUi=ui.notice;activity.LaunchPopUi=ui.launch;}
                activity.Refresh(Data.datas[i].id);
                if(!activity.GetType().Equals(typeof(OutgameActivityBase)))continue;
                for(int j=0;j<activity.Children.Count;j++)activity.Children[j].Refresh(activity.Children[j].ActivityId);
            }
            ArrangeWidgets();
        }
        public void BindWidget(OutgameActivityBase activity,Button button,Text text)
        {if(!activity.Buttons.Contains(button))activity.Buttons.Add(button);if(!activity.Descriptions.Contains(text))activity.Descriptions.Add(text);}
        public void BindUi(int id,Type notice,Type launch)
        {
            var activity=GetActivity<OutgameActivityBase>(id);
            if(activity!=null){activity.NoticePopUi=notice;activity.LaunchPopUi=launch;if(activity.Data!=null&&activity.Data.state==3)activity.UnlockPop();return;}
            UiBindings[id]=(notice,launch);
        }
        public void ArrangeWidgets()
        {
            var groups=new Dictionary<int,List<OutgameActivityBase>>();
            foreach(var config in Services.Config().Activities.Values)
            {
                var activity=GetActivity<OutgameActivityBase>(config.id);if(activity==null||activity.Data==null)continue;
                if(!groups.ContainsKey(config.areaID))groups.Add(config.areaID,new List<OutgameActivityBase>());
                groups[config.areaID].Add(activity);
            }
            foreach(var pair in groups)
            {
                var rows=pair.Value;rows.Sort((a,b)=>a.Config.btnPriority<b.Config.btnPriority?-1:a.Config.btnPriority>b.Config.btnPriority?1:0);
                for(int i=0;i<rows.Count;i++)
                {
                    var activity=rows[i];for(int j=0;j<activity.Buttons.Count;j++)
                    {activity.Buttons[j].transform.SetSiblingIndex(i);activity.Buttons[j].onClick.RemoveAllListeners();activity.Buttons[j].onClick.AddListener(activity.ButtonClickAction);}
                }
            }
        }
        public IOutgameItemEntity GetCommonItem(GameItemConfig config)
        {
            foreach(var activity in Activities.Values)
            {
                var item=activity.ActivityFactoryBase(config.id).Produce();if(item!=null)return item;
                for(int i=0;i<activity.Children.Count;i++){item=activity.Children[i].ActivityFactoryBase(config.id).Produce();if(item!=null)return item;}
            }
            return null;
        }
        static int ComparePop(OutgameActivityPopEvent a,OutgameActivityPopEvent b)=>b.Config.popPriority<a.Config.popPriority?-1:b.Config.popPriority>a.Config.popPriority?1:0;
        public void QueueNotice(OutgameActivityPopEvent item){NoticeQueue.Add(item);NoticeQueue.Sort(ComparePop);}
        public void QueueLaunch(OutgameActivityPopEvent item){LaunchQueue.Add(item);LaunchQueue.Sort(ComparePop);}
        public void ProcessNoticeQueue()
        {
            if(NoticeUi!=null)return;
            for(int i=0;i<NoticeQueue.Count;i++)
            {
                var config=NoticeQueue[i].Config;if(config.noticeAutoPop!=1)continue;
                var activity=GetActivity<OutgameActivityBase>(config.id);activity.Data.noticePop=true;Services.Current().SetDirty(true);
                NoticeUi=activity.OpenBindNoticeUi(NoticeQueue[i]);NoticeQueue.RemoveAt(i);break;
            }
        }
        public void OpenUiEvent(object[] args)
        {
            var page=(OutgameUiPage)args[0];
            for(int i=0;i<NoticeQueue.Count;i++)
            {var item=NoticeQueue[i];if(item.UiType.Equals(page.GetType())){GetActivity<OutgameActivityBase>(item.Config.id).Data.noticePop=true;Services.Current().SetDirty(true);}}
            for(int i=0;i<LaunchQueue.Count;i++)
            {var item=LaunchQueue[i];if(item.UiType.Equals(page.GetType())){GetActivity<OutgameActivityBase>(item.Config.id).Data.launchPop=true;Services.Current().SetDirty(true);}}
        }
        public void CloseUiEvent(object[] args)
        {
            var page=(OutgameUiPage)args[0];
            for(int i=0;i<NoticeQueue.Count;i++){var item=NoticeQueue[i];if(item.UiType.Equals(page.GetType())){NoticeQueue.Remove(item);NoticeUi=null;}}
            for(int i=0;i<LaunchQueue.Count;i++){var item=LaunchQueue[i];if(item.UiType.Equals(page.GetType())){LaunchQueue.Remove(item);LaunchUi=null;}}
        }
        public void Update()
        {
            if(ActivityList!=null)for(int i=0;i<ActivityList.Count;i++)ActivityList[i].BaseUpdate();ProcessNoticeQueue();
            Elapsed+=Services.UnscaledDeltaTime();if(Elapsed>=60){Elapsed-=60;RefreshStatistics();}
            if(IsDirty&&Services.Pool().IsEnableSaveData){Manager?.OnSave();SetDirty(false);}
        }
        public void RefreshStatistics()
        {
            if(Data==null)return;
            for(int i=0;i<Data.datas.Count;i++)
            {
                if(Data.datas[i]==null)continue;
                if(Data.datas[i].state==2&&Data.datas[i].WarmTimeStamp!=0)
                {long now=Services.Statistics.GameValue(10000,Array.Empty<object>());long elapsed=unchecked(now-Data.datas[i].WarmTimeStamp);Services.Statistics.SetEventCount(10600,Data.datas[i].id,elapsed/60000);}
                if(Data.datas[i].state!=1&&Data.datas[i].state!=2)continue;
                if(Data.datas[i].Config==null||Data.datas[i].Config.launchType.Length!=1||Data.datas[i].Config.launchType[0]!=10000)continue;
                long target=Data.datas[i].LaunchCondition[0].value;long timestamp=Services.Statistics.GameValue(10000,Array.Empty<object>());
                Services.Statistics.SetEventCount(10700,Data.datas[i].id,unchecked(target-timestamp)/60000);
            }
        }
        public void DailyRefresh(long timestamp)
        {
            if(Data==null)return;Data.lastRefreshTimeStamp=timestamp;
            for(int i=0;i<Data.datas.Count;i++)
            {
                if(Data.datas[i]==null||Data.datas[i].LaunchTimeStamp==0)continue;
                long now=Services.Statistics.GameValue(10000,Array.Empty<object>());long ticks=unchecked((now-Data.datas[i].LaunchTimeStamp)*10000L);
                Services.Statistics.SetEventCount(20000,Data.datas[i].id,unchecked((int)(ticks/864000000000L)));
            }
        }
        public string GetParentReportStr(OutgameActivityConfigRow config)
        {
            if(string.IsNullOrEmpty(config.parentName))
            {
                var result=new StringBuilder();for(int i=0;i<config.parentActivityID.Length;i++)
                {var parent=Services.Config().GetActivityConfig(config.parentActivityID[i]);if(parent==null)continue;result.Append(Services.Localize(parent.activityName));if(i!=config.parentActivityID.Length-1)result.Append("_");}
                config.parentName=result.ToString();
            }
            return config.parentName;
        }
        public void Launch(OutgameActivityBase activity,int ignored)
        {
            activity.Launch();var data=activity.Data;data.LaunchTimeStamp=Services.Statistics.GameValue(10000,Array.Empty<object>());activity.Data.WarmTimeStamp=0;
            Services.Statistics.SetEventCount(10600,activity.ActivityId,0);Services.Statistics.SetEventCount(10700,activity.ActivityId,0);
            var report=Services.Reports().Create(OutgameActivityReportKind.Unlock,true,null);report.ActivityName=Services.Localize(activity.Config.activityName);report.ParentName=GetParentReportStr(activity.Config);
            Services.Common().SendMessage("CommonModule_Activity_Lunch",new object[]{activity.ActivityId});Services.Reports().Send(OutgameActivityReportKind.Unlock,report);
        }
        public void Notice(OutgameActivityBase activity,int ignored)
        {
            activity.Warm();var data=activity.Data;data.WarmTimeStamp=Services.Statistics.GameValue(10000,Array.Empty<object>());activity.Data.LaunchTimeStamp=0;
            var report=Services.Reports().Create(OutgameActivityReportKind.Warmup,true,null);report.ActivityName=Services.Localize(activity.Config.activityName);report.ParentName=GetParentReportStr(activity.Config);
            Services.Common().SendMessage("CommonModule_Activity_WarmOnceOnLT",new object[]{activity.ActivityId});Services.Reports().Send(OutgameActivityReportKind.Warmup,report);
        }
        public void Over(OutgameActivityBase activity,int ignored)
        {
            activity.Over();var data=activity.Data;data.LaunchTimeStamp=0;data.WarmTimeStamp=0;
            Services.Statistics.SetEventCount(10600,activity.ActivityId,0);Services.Statistics.SetEventCount(10700,activity.ActivityId,0);
            var report=Services.Reports().Create(OutgameActivityReportKind.Complete,true,null);report.ActivityName=Services.Localize(activity.Config.activityName);report.ParentName=GetParentReportStr(activity.Config);
            Services.Reports().Send(OutgameActivityReportKind.Complete,report);
        }
        public void Closed(int id)=>Services.Common().SendMessage("CommonActivity_Close",new object[]{id});
    }
}
