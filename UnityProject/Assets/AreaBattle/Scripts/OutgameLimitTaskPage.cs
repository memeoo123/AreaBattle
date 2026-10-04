using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameLimitTaskPageReports
    {
        OutgameActivityReport Create(bool initialize,object argument);
        void Send(OutgameActivityReport report);
    }
    public sealed class OutgameLimitTaskPageServices
    {
        public Func<OutgameActivityControl> Activities;
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameSevendayActivityControl> Sevenday;
        public OutgameStatisticsExpansion Statistics;
        public Func<OutgameCommonMessageDispatcher> Common=()=>OutgameCommonMessageDispatcher.Shared;
        public Func<OriginalConfig.Lang,string> Language;
        public Func<IOutgameLimitTaskPageReports> Reports;
        public Func<OutgamePrefabPoolControl> Pool;
        public Func<GameObject> TaskPrefab,DayPrefab;
        public OutgameLimitTaskItemServices Tasks;
        public OutgameLimitTaskPageItemServices Days;
        public OutgameLimitTaskAccItemServices Accumulator;
        public OutgameSevendayAccPreviewServices Preview;
    }
    // Source CommonLimitTimeTaskUI4310 with recovered BaseUI loading/close owners.
    public sealed class OutgameLimitTaskPage:OutgameUiPage,IOutgameOwnedUiPage,IOutgameLimitTaskAccPage
    {
        sealed class PageLifetime:OutgameUiLifetime
        {
            public Action AfterBaseDispose;
            public PageLifetime(Action<GameObject> destroy):base(null,()=>{},destroy){}
            public override void Dispose(){base.Dispose();AfterBaseDispose();}
        }
        public const string SourceName="CommonLimitTimeTaskUI",SourceNamespace="Proj_hdzd.UI.MainMenu",SourcePath="MainMenu/CommonLimitTimeTaskUI";
        const string RefreshList="CommonModule_RefreshNoviceTaskList",RefreshExt="CommonModule_NoviceExtRefresh";
        readonly OutgameUiPageServices ui;readonly OutgameLimitTaskPageServices services;readonly PageLifetime lifetime;
        readonly OutgameUiObjectInitialization initialization;readonly OutgameUiLoadRequest load;
        readonly OutgameUiOpenHost openHost;readonly OutgameUiCloseHost closeHost;readonly OutgameUiAsyncClose close;
        public readonly OutgameDynamicListProviderSelection<OutgameLimitTaskItemData> TaskData=new OutgameDynamicListProviderSelection<OutgameLimitTaskItemData>();
        public readonly OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem> DayData=new OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem>();
        public readonly Dictionary<int,OutgameLimitTaskAccItem> AccItems=new Dictionary<int,OutgameLimitTaskAccItem>();
        public Dictionary<int,string> ModeLabels;
        public OutgameChildLimitTimeTaskActivity Child;
        public ActivityConfig.PubNTActivityConfig Config;
        public int SelectedDay;
        public OutgameLimitTaskListBinding Tasks {get;private set;}
        public OutgameLimitTaskDayListBinding Days {get;private set;}
        public OutgameSevendayAccPreviewItem Preview {get;private set;}
        public Button CloseButton {get;private set;}
        public Text AccNumber {get;private set;}public Text Countdown {get;private set;}public Text Title {get;private set;}
        public Slider Progress {get;private set;}public GameObject AccLayout {get;private set;}public GameObject AccTemplate {get;private set;}
        public Transform Transform=>lifetime.Transform;
        public OutgameUiLifetime Lifetime=>lifetime;
        public OutgameUiResourceLists Resources {get;}
        public OutgameUiCanvas Canvas {get;}
        public bool ShowLoading,ShowTop=true,CanReportOpen=true;
        public string UiModel="None";
        public bool Cached {get=>openHost.Cached;set=>openHost.Cached=value;}
        public Action CloseAction;
        public Task Closing {get;private set;}
        public OutgameAssetHandle MainHandle=>closeHost.MainHandle;
        public object LegacyResource=>openHost.LegacyResource;
        public int OutletCount=>initialization.Objects.Count;
        public OutgameLimitTaskPage(OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameLimitTaskPageServices services)
            :this(new PageLifetime(ui.DestroyObject),ui,outlets,services){}
        OutgameLimitTaskPage(PageLifetime lifetime,OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameLimitTaskPageServices services)
            :base(()=>lifetime.GameObject,ui.Messages)
        {
            this.ui=ui;this.services=services;this.lifetime=lifetime;lifetime.AfterBaseDispose=DisposePage;
            Resources=new OutgameUiResourceLists(ui.UnloadUnusedAssets);Canvas=new OutgameUiCanvas(ui.MaximumWindowIndex,ui.WideWindowSorting);
            initialization=new OutgameUiObjectInitialization(lifetime,this,outlets.Read,InitializeComponent,()=>{},()=>{},Awake);
            openHost=new OutgameUiOpenHost(lifetime,this,initialization,Canvas,ui.Animation,()=>Loading(false),()=>{},()=>{},null,ui.Log){Layer=2,OpenAnimation=0,OpenAnimationTime=0};
            closeHost=new OutgameUiCloseHost(lifetime,this,ui.Animation,Resources,SourcePath,ui.UsesNewResources,null,ui.UnloadUnusedAssets,ui.UnloadUnusedBundle,ui.EndOfFrame){CloseAnimation=0,CloseAnimationTime=0};
            close=new OutgameUiAsyncClose(closeHost,ui.Messages);
            load=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(openHost,ui.Messages),()=>Loading(true),ui.UsesNewResources,ui.Loader,()=>SourcePath,()=>OutgameUiLayerNames.Get(2),handle=>closeHost.MainHandle=handle);
        }
        void Loading(bool visible){if(ShowLoading)ApplyObjectVisibility(ui.Loading(),visible);}
        void InitializeComponent()
        {
            var nodes=initialization.Objects;
            CloseButton=nodes["btnClose"].GetComponent<Button>();AccNumber=nodes["accNum"].GetComponent<Text>();AccLayout=nodes["AccLayout"];
            Progress=nodes["progressSlider"].GetComponent<Slider>();AccTemplate=nodes["LimitTimeTaskAccItem"];
            Tasks=new OutgameLimitTaskListBinding(GameObject,services.TaskPrefab(),services.Pool,services.Tasks,TaskData,false);
            Days=new OutgameLimitTaskDayListBinding(GameObject,services.DayPrefab(),services.Pool,services.Days,DayData,false);
            Countdown=nodes["txtRefreshCondition"].GetComponent<Text>();Title=nodes["title"].GetComponent<Text>();
        }
        void Awake()
        {
            Preview=new OutgameSevendayAccPreviewItem(Transform.Find("SevendayAccPreviewItem").gameObject,services.Preview);
            ModeLabels=new Dictionary<int,string>{{0,"CommonGameModule.CommonNoviceTaskUITemp.AccCompleteNum"},{1,"CommonGameModule.CommonNoviceTaskUITemp.Liveness"}};
            Child=services.Activities().GetActivity<OutgameLimitTimeTaskActivity>(1301001).GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            Config=services.Config().GetLimitTaskActivityConfig(1301);
            OutgameUiClick.Add(CloseButton,CloseSelf,ui.Messages);
            ui.Messages().AddListener(OutgameLimitTaskPageView.SelectPage,SelectPage);
            services.Common().AddListener(RefreshExt,RefreshAccItems);services.Common().AddListener(RefreshList,RefreshTasks);
            services.Common().AddListener(OutgameStatisticsMessageKey.Get(10000),RefreshCountdown);
            RefreshCountdown(Array.Empty<object>());InitializeLists();
            var report=services.Reports().Create(true,null);report.ActivityName="七日嘉年华";
            report.ParentName=services.Activities().GetParentReportStr(Child.Config);services.Reports().Send(report);
        }
        void InitializeLists()
        {
            Title.text=services.Language(Config.name);Tasks.InitializeRenderer();Days.InitializeRenderer();DayData.Data.Clear();
            SelectedDay=Child.ExitToGetRewardDay();var days=new List<int>(Child.DayTasks.Keys);
            for(int i=0;i<days.Count;i++)DayData.Data.Add(new OutgameLimitTaskPageItem(days[i],1301));
            DayData.Data.Sort((a,b)=>a.day.CompareTo(b.day));Days.List.UpdateList();DayData.SetSelect(unchecked(SelectedDay-1),true);
        }
        public void RefreshCountdown(object[] args)
        {
            if(services.Sevenday().IsInActivity())
            {
                long end=services.Sevenday().GetActDate(false);long remaining=unchecked(end-services.Statistics.GameValue(10000,Array.Empty<object>()));
                Countdown.text="剩余时间: "+FormatDuration(unchecked((int)(remaining/1000L)));return;
            }
            Countdown.text="活动结束";ui.Messages().SendMessage("SevendayClose");
        }
        // TimeUtility4288.ToDDHHMMSS32655 differs from TimeHelper: days omit minutes, Chinese suffixes.
        public static string FormatDuration(int seconds)
        {
            int days=seconds/86400,hours=(seconds-days*86400)/3600,minutes=(seconds-days*86400-hours*3600)/60;
            string h=hours.ToString().PadLeft(2,'0')+"时";
            return seconds>=86400?days.ToString()+"天"+h:h+minutes.ToString().PadLeft(2,'0')+"分"+(seconds%60).ToString().PadLeft(2,'0')+"秒";
        }
        public void SelectPage(object[] args)
        {
            if(args==null||args.Length<2)return;if((int)args[0]!=1301)return;
            SelectedDay=(int)args[1];Child.ModuleData.ext.lastClickDayId=SelectedDay;
            Tasks.RenderDay(Child,services.Statistics,SelectedDay);RefreshAccItems(Array.Empty<object>());
        }
        public void RefreshTasks(object[] args)
        {if(args==null||args.Length<1)return;if((int)args[0]!=1301)return;if(SelectedDay==(int)args[1])Tasks.RenderDay(Child,services.Statistics,SelectedDay);}
        public void RefreshAccProgress(object[] args)
        {
            if(args==null||args.Length!=2)return;if((int)args[0]!=SelectedDay)return;
            var ext=(OutgameLimitTimeTaskExt)args[1];var unused=ModeLabels[Config.accType];
            AccNumber.text=ext.AccProgress.ToString();int target=ext.TargetProgress;Progress.value=(float)ext.AccProgress/(float)target;
        }
        public void RefreshAccItems(object[] args)
        {
            foreach(var item in AccItems.Values){item.Dispose();services.Accumulator.Destroy(item.Lifetime.GameObject);}AccItems.Clear();
            var rewards=Child.AccRewards;
            for(int i=0;i<rewards.Count;i++)
            {
                var root=UnityEngine.Object.Instantiate(AccTemplate,AccLayout.transform,false);root.transform.localScale=Vector3.one;root.SetActive(true);
                var item=new OutgameLimitTaskAccItem(root,services.Accumulator);root.name="Node"+i.ToString();item.SetData(rewards[i],Child);AccItems.Add(rewards[i].id,item);
            }
            RefreshAccProgress(new object[]{SelectedDay,Child.ModuleData.ext});
        }
        public void PreviewAccReward(Transform origin,OutgameNoviceAccRewardItemData reward)
        {Preview.Lifetime.GameObject.SetActive(true);Preview.SetVisible(true);Preview.Lifetime.Transform.position=origin.position;Preview.SetData(reward.RewardsData);Preview.Refresh();}
        void DisposePage()
        {
            ui.Messages().RemoveListener(OutgameLimitTaskPageView.SelectPage,SelectPage);
            services.Common().RemoveListener(RefreshExt,RefreshAccItems);services.Common().RemoveListener(RefreshList,RefreshTasks);
            services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(10000),RefreshCountdown);DayData.KillSelect();
            foreach(var item in AccItems.Values)if(item!=null)item.Dispose();
        }
        public void Refresh(){}
        public void Open(object[] arguments)=>load.Open(arguments);
        public void CloseSelf()=>OutgameUiCloseRegistry<OutgameUiPage>.CloseSelf(CloseAction,ui.CloseRegistry,()=>SourceName);
        public void CloseInModule(){Closing=close.CloseAsync();ObserveClose(Closing);}
        static async void ObserveClose(Task task)=>await task;
        public void CloseUINow()=>lifetime.CloseUINow();
    }
}
