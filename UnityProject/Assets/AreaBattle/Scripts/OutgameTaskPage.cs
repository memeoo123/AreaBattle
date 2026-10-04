using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameTaskPageServices
    {
        public OutgameTaskSubviews Subviews;
        public Func<OutgameTaskControl> Control;
        public Func<IOutgameTopInfoPage> TopInfo;
        public Action<int,int[]> PlayAudio;
        public Func<string,string> Language;
        public OutgameTaskRowServices Rows;
        public OutgameImportedUiOutlets RowOutlets;
        public OutgameLivenessPreviewServices Preview;
        public OutgameTaskLivenessServices Liveness;
    }
    // TaskPanelUI4416. BaseUI owns resource loading, visibility and asynchronous close.
    public sealed class OutgameTaskPage:OutgameUiPage,IOutgameOwnedUiPage,IOutgameDailyTaskPage,IOutgameAchievementTaskPage
    {
        sealed class PageLifetime:OutgameUiLifetime
        {
            public Action AfterBaseDispose;
            public PageLifetime(Action<GameObject> destroy):base(null,()=>{},destroy){}
            public override void Dispose(){base.Dispose();AfterBaseDispose();}
        }
        public const string SourceName="TaskPanelUI",SourceNamespace="Proj_hdzd.UI.MainMenu",SourcePath="MainMenu/TaskPanelUI";
        public const int Layer=3;
        readonly OutgameUiPageServices ui;readonly OutgameTaskPageServices services;readonly PageLifetime lifetime;
        readonly OutgameUiObjectInitialization initialization;readonly OutgameUiLoadRequest load;
        readonly OutgameUiOpenHost openHost;readonly OutgameUiCloseHost closeHost;readonly OutgameUiAsyncClose close;
        public object CurrentView;public Toggle SelectedToggle;
        public OutgameLivenessPreviewItem Preview {get;private set;}
        public OutgameTaskLivenessBinding Liveness {get;private set;}
        public Button CloseButton {get;private set;}public Image Mask {get;private set;}
        public Toggle TaskTab {get;private set;}public Toggle AchievementTab {get;private set;}
        public Slider Progress {get;private set;}public Text Countdown {get;private set;}public Text EmptyDaily {get;private set;}
        public Transform ProgressBackground {get;private set;}public Transform TaskItemContent {get;private set;}public Transform AchievementItemContent {get;private set;}
        public Transform TabBar {get;private set;}public Transform TabBar2 {get;private set;}
        public GameObject Template {get;private set;}public GameObject TaskContent {get;private set;}public GameObject AchievementContent {get;private set;}
        public GameObject EmptyAchievement {get;private set;}public GameObject GoldRoot {get;private set;}public GameObject PreviewRoot {get;private set;}
        public Transform Transform=>lifetime.Transform;public OutgameUiLifetime Lifetime=>lifetime;
        public OutgameUiResourceLists Resources {get;}public OutgameUiCanvas Canvas {get;}
        public bool ShowLoading,ShowTop=true,CanReportOpen=true;public string UiModel="None";
        public bool Cached {get=>openHost.Cached;set=>openHost.Cached=value;}
        public Action CloseAction;public Task Closing {get;private set;}
        public OutgameAssetHandle MainHandle=>closeHost.MainHandle;public object LegacyResource=>openHost.LegacyResource;
        public int OutletCount=>initialization.Objects.Count;
        public OutgameTaskPage(OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameTaskPageServices services):this(new PageLifetime(ui.DestroyObject),ui,outlets,services){}
        OutgameTaskPage(PageLifetime lifetime,OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameTaskPageServices services):base(()=>lifetime.GameObject,ui.Messages)
        {
            this.ui=ui;this.services=services;this.lifetime=lifetime;lifetime.AfterBaseDispose=DisposePage;
            Resources=new OutgameUiResourceLists(ui.UnloadUnusedAssets);Canvas=new OutgameUiCanvas(ui.MaximumWindowIndex,ui.WideWindowSorting);
            initialization=new OutgameUiObjectInitialization(lifetime,this,outlets.Read,InitializeComponent,()=>{},()=>{},Awake);
            openHost=new OutgameUiOpenHost(lifetime,this,initialization,Canvas,ui.Animation,()=>Loading(false),()=>{},OpenLater,null,ui.Log){Layer=Layer,OpenAnimation=0,OpenAnimationTime=0};
            closeHost=new OutgameUiCloseHost(lifetime,this,ui.Animation,Resources,SourcePath,ui.UsesNewResources,null,ui.UnloadUnusedAssets,ui.UnloadUnusedBundle,ui.EndOfFrame){CloseAnimation=0,CloseAnimationTime=0};
            close=new OutgameUiAsyncClose(closeHost,ui.Messages);
            load=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(openHost,ui.Messages),()=>Loading(true),ui.UsesNewResources,ui.Loader,()=>SourcePath,()=>OutgameUiLayerNames.Get(Layer),handle=>closeHost.MainHandle=handle);
        }
        void Loading(bool value){if(ShowLoading)ApplyObjectVisibility(ui.Loading(),value);}
        void InitializeComponent()
        {
            var n=initialization.Objects;
            CloseButton=n["CloseBtn"].GetComponent<Button>();Mask=n["Mask"].GetComponent<Image>();TaskTab=n["TaskTab"].GetComponent<Toggle>();AchievementTab=n["AchtTab"].GetComponent<Toggle>();
            ProgressBackground=n["ProgressBg"].transform;Progress=n["Prog"].GetComponent<Slider>();Countdown=n["Time"].GetComponent<Text>();
            TaskItemContent=n["TaskItemContent"].transform;Template=n["TaskItemItem"];EmptyDaily=n["TipText"].GetComponent<Text>();
            TaskContent=n["TaskContent"];AchievementContent=n["AchtContent"];AchievementItemContent=n["AchtItemContent"].transform;EmptyAchievement=n["AchtTipText"];
            TabBar=n["TabBar"].transform;TabBar2=n["TabBar2"].transform;GoldRoot=n["goldRoot"];PreviewRoot=n["LivenessPreviewItem"];
        }
        void Awake()
        {
            AchievementTab.onValueChanged.AddListener(value=>ToggleChanged(value,AchievementTab));TaskTab.onValueChanged.AddListener(value=>ToggleChanged(value,TaskTab));
            OutgameUiClick.Add(TaskTab.gameObject,PlayClickAudio);OutgameUiClick.Add(AchievementTab.gameObject,PlayClickAudio);
            CurrentView=services.Subviews.Daily.I;services.TopInfo().RendererPart(6,OutgameUiLayerNames.Get(Layer));
            Preview=new OutgameLivenessPreviewItem(PreviewRoot,services.Preview);
            // These callbacks belong to this page, while the subview owner is shared across pages.
            var s=services.Liveness;
            var bindingServices=new OutgameTaskLivenessServices{Daily=()=>services.Subviews.Daily.I,Config=s.Config,GoodsType=s.GoodsType,FormatNumber=s.FormatNumber,
                ShowEffect=s.ShowEffect,CloseEffect=s.CloseEffect,Effects=s.Effects,ShowAward=s.ShowAward,RefreshTopInfo=s.RefreshTopInfo,SetDailyTabReddot=SetDailyTabReddot,EndOfFrame=s.EndOfFrame,Messages=s.Messages};
            Liveness=new OutgameTaskLivenessBinding(ProgressBackground,Preview,bindingServices){Layer=Layer};Liveness.Refresh(true);Countdown.text=string.Empty;
        }
        public void OpenLater(){InitializeCurrent();OutgameUiClick.Add(CloseButton,CloseClicked,ui.Messages);OutgameUiClick.Add(Mask,CloseClicked);}
        public void PlayClickAudio()=>services.PlayAudio(1,new[]{2001});
        void CloseClicked(){PlayClickAudio();CloseSelf();}
        public void ToggleChanged(bool value,Toggle toggle)
        {
            if(value&&toggle==SelectedToggle)return;
            Liveness.CloseCurrentEffect();TaskContent.SetActive(toggle==TaskTab);AchievementContent.SetActive(toggle==AchievementTab);
            if(!value)return;SelectedToggle=toggle;
            if(toggle==AchievementTab)CurrentView=services.Subviews.Achievement.I;else if(toggle==TaskTab)CurrentView=services.Subviews.Daily.I;
            InitializeCurrent();
        }
        public void InitializeCurrent()
        {
            if(CurrentView is OutgameDailyTaskView daily){daily.RootUI=this;if(!daily.LateInited){daily.LateInited=true;daily.OnLateInit();}}
            else if(CurrentView is OutgameAchievementTaskView achievement){achievement.RootUI=this;if(!achievement.LateInited){achievement.LateInited=true;achievement.OnLateInit();}}
            else throw new NullReferenceException("ITaskSubUI");
            SetDailyTabReddot();SetAchTabReddot();
        }
        public OutgameTaskRow GetTaskItem()
        {
            var row=new OutgameTaskRow(services.Rows);Transform parent;
            if(ReferenceEquals(CurrentView,services.Subviews.Achievement.I))parent=AchievementItemContent;
            else if(ReferenceEquals(CurrentView,services.Subviews.Daily.I))parent=TaskItemContent;
            else return row;
            var root=UnityEngine.Object.Instantiate(Template,parent,false);root.transform.localScale=Vector3.one;root.SetActive(true);row.Instantiate(root,services.RowOutlets);return row;
        }
        public void SetTaskEmpty(bool empty)
        {if(ReferenceEquals(CurrentView,services.Subviews.Achievement.I))EmptyAchievement.SetActive(empty);else if(ReferenceEquals(CurrentView,services.Subviews.Daily.I))EmptyDaily.gameObject.SetActive(empty);}
        public void SetDailyProgress(float value)=>Progress.value=value;
        public void SetDailyCountdown(string value)=>Countdown.text=string.Format(services.Language("Task.Refresh"),value);
        public void RefreshPointDailyLivenessBar()=>Liveness.RefreshPointDailyLivenessBar();
        public void SetDailyTabReddot()=>TaskTab.transform.GetChild(2).gameObject.SetActive(services.Control().IsDailyCanComplete());
        public void SetAchTabReddot()=>AchievementTab.transform.GetChild(2).gameObject.SetActive(services.Control().IsAchiCanComplete());
        void DisposePage()
        {services.TopInfo().RendererPart(7,string.Empty);services.Subviews.Achievement.I.OnDestroy();services.Subviews.Daily.I.OnDestroy();services.Control().SetRedDot();}
        public void Refresh(){}
        public void Open(object[] args)=>load.Open(args);
        public void CloseSelf()=>OutgameUiCloseRegistry<OutgameUiPage>.CloseSelf(CloseAction,ui.CloseRegistry,()=>SourceName);
        public void CloseInModule(){Closing=close.CloseAsync();ObserveClose(Closing);}
        static async void ObserveClose(Task task)=>await task;
        public void CloseUINow()=>lifetime.CloseUINow();
    }
}
