using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameOwnedUiPage {void Open(object[] arguments);void CloseInModule();}
    // Module-owned services, shared with the recovered BaseUI loading and close state machines.
    public sealed class OutgameUiPageServices
    {
        public Func<IOutgameUiLoader> Loader;
        public Func<bool> UsesNewResources;
        public OutgameUiAnimation Animation;
        public Func<OutgameUiCloseRegistry<OutgameUiPage>> CloseRegistry;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Func<int> MaximumWindowIndex;
        public Func<bool> WideWindowSorting;
        public Func<GameObject> Loading;
        public Action UnloadUnusedAssets;
        public Action<string> UnloadUnusedBundle;
        public Action<string> Log;
        public Func<WaitForEndOfFrame,Task> EndOfFrame;
        public Action<GameObject> DestroyObject;
    }
    // Source4328 plus recovered BaseUI27320..27347. Actual resource/account/platform services are supplied by the owning module.
    public sealed class OutgameGuideBookPage:OutgameUiPage,IOutgameOwnedUiPage,IOutgameGuideBookItemsPage
    {
        sealed class PageLifetime:OutgameUiLifetime
        {
            public Action AfterBaseDispose;
            public PageLifetime(Action<GameObject> destroy):base(null,()=>{},destroy){}
            public override void Dispose(){base.Dispose();AfterBaseDispose();}
        }
        public const string SourceName="GuideBookUI",SourceNamespace="Proj_hdzd.UI.MainMenu",SourcePath="MainMenu/GuideBookUI";
        public readonly OutgameDynamicListProvider<OutgameGuideBookRow> Data=new OutgameDynamicListProvider<OutgameGuideBookRow>();
        readonly PageLifetime lifetime;readonly OutgameUiPageServices ui;
        readonly OutgameGuideBookItemServices itemServices;readonly OutgameGuideBookPopupServices popupServices;
        readonly Func<OutgameToolControl> tools;readonly Func<IOutgameShopCurrencyEffects> effects;
        readonly Func<OutgamePrefabPoolControl> pool;readonly Func<GameObject> rowPrefab;
        readonly OutgameUiObjectInitialization initialization;readonly OutgameUiLoadRequest load;
        readonly OutgameUiOpenHost openHost;readonly OutgameUiCloseHost closeHost;readonly OutgameUiAsyncClose close;
        public OutgameUiLifetime Lifetime=>lifetime;
        public OutgameUiResourceLists Resources {get;}
        public OutgameGuideBookListBinding List {get;private set;}
        public OutgameGuideBookBrowseBinding Browse {get;private set;}
        public OutgameGuideBookRewardBinding Rewards {get;private set;}
        public OutgameGuideBookPopupBinding Popup {get;private set;}
        public OutgameUiCanvas Canvas {get;}
        public bool ShowLoading; // BaseUI field56 defaults false; GuideBook constructor does not override it.
        public bool ShowTop=true,CanReportOpen=true;
        public string UiModel="None";
        public bool Cached {get=>openHost.Cached;set=>openHost.Cached=value;}
        public Action CloseAction;
        public Task Closing {get;private set;}
        public OutgameAssetHandle MainHandle=>closeHost.MainHandle;
        public object LegacyResource=>openHost.LegacyResource;
        public int OutletCount=>initialization.Objects.Count;
        public OutgameGuideBookPage(OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,
            OutgameGuideBookItemServices itemServices,OutgameGuideBookPopupServices popupServices,
            Func<OutgameToolControl> tools,Func<IOutgameShopCurrencyEffects> effects,Func<OutgamePrefabPoolControl> pool,Func<GameObject> rowPrefab)
            :this(new PageLifetime(ui.DestroyObject),ui,outlets,itemServices,popupServices,tools,effects,pool,rowPrefab){}
        OutgameGuideBookPage(PageLifetime lifetime,OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,
            OutgameGuideBookItemServices itemServices,OutgameGuideBookPopupServices popupServices,
            Func<OutgameToolControl> tools,Func<IOutgameShopCurrencyEffects> effects,Func<OutgamePrefabPoolControl> pool,Func<GameObject> rowPrefab)
            :base(()=>lifetime.GameObject,ui.Messages)
        {
            this.lifetime=lifetime;this.ui=ui;this.itemServices=itemServices;this.popupServices=popupServices;
            this.tools=tools;this.effects=effects;this.pool=pool;this.rowPrefab=rowPrefab;
            lifetime.AfterBaseDispose=()=>{Data.Data.Clear();Browse.DisposeItemsAndTabs();};
            Resources=new OutgameUiResourceLists(ui.UnloadUnusedAssets);
            Canvas=new OutgameUiCanvas(ui.MaximumWindowIndex,ui.WideWindowSorting);
            initialization=new OutgameUiObjectInitialization(lifetime,this,outlets.Read,InitializeComponent,()=>{},()=>{},Awake);
            // Source33037: popup layer2, fade-in1, close animation0; durations remain base defaults0.
            openHost=new OutgameUiOpenHost(lifetime,this,initialization,Canvas,ui.Animation,()=>Loading(false),()=>{},()=>{},null,ui.Log)
                {Layer=2,OpenAnimation=1,OpenAnimationTime=0};
            closeHost=new OutgameUiCloseHost(lifetime,this,ui.Animation,Resources,SourcePath,ui.UsesNewResources,null,
                ui.UnloadUnusedAssets,ui.UnloadUnusedBundle,ui.EndOfFrame){CloseAnimation=0,CloseAnimationTime=0};
            close=new OutgameUiAsyncClose(closeHost,ui.Messages);
            load=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(openHost,ui.Messages),()=>Loading(true),ui.UsesNewResources,ui.Loader,
                ()=>SourcePath,()=>OutgameUiLayerNames.Get(2),handle=>closeHost.MainHandle=handle);
        }
        void Loading(bool visible){if(ShowLoading)ApplyObjectVisibility(ui.Loading(),visible);}
        void InitializeComponent()
        {
            // Imported source outlet map is attached by UIObject before these concrete binding owners are constructed.
            Rewards=GameObject.AddComponent<OutgameGuideBookRewardBinding>();Popup=GameObject.AddComponent<OutgameGuideBookPopupBinding>();
            Browse=GameObject.AddComponent<OutgameGuideBookBrowseBinding>();Browse.PrepareComponents();
            List=new OutgameGuideBookListBinding(GameObject.transform,rowPrefab(),pool,itemServices,Data,false);
        }
        void Awake()
        {
            List.InitializeRenderer();
            // Source33045 registers close/mask/OK, then reward, then tab selection handlers, before populating data.
            Popup.Bind(Rewards,new OutgameGuideBookPopupServices{Config=popupServices.Config,Control=popupServices.Control,
                CurrentCommanderId=popupServices.CurrentCommanderId,Language=popupServices.Language,SetSprite=popupServices.SetSprite,
                Voice=popupServices.Voice,ClosePage=CloseSelf,Messages=popupServices.Messages});
            Rewards.Bind(itemServices.Control,tools,effects,itemServices.Voice,Data.UpdateItemData,itemServices.Messages);
            Browse.BindHandlers(itemServices,Rewards,Popup.OnItemClick);
            List.Populate();Browse.PopulateTips();Browse.Tabs.SetSelect(Browse.GuideTab);Rewards.RefreshRedDots();
        }
        public void Open(object[] arguments)=>load.Open(arguments);
        public void CloseSelf()=>OutgameUiCloseRegistry<OutgameUiPage>.CloseSelf(CloseAction,ui.CloseRegistry,()=>SourceName);
        public void CloseInModule(){Closing=close.CloseAsync();ObserveClose(Closing);}
        static async void ObserveClose(Task task)=>await task;
        public void CloseUINow()=>lifetime.CloseUINow();
        public void OnItemClick(int index,OriginalConfig.GuidebookConfig config)=>Browse.OnItemClick(index,config);
        public void OnTipItemClick(OutgameTipBookItem item)=>Browse.OnTipItemClick(item);
        public void GetTipReward(OriginalConfig.GuideTipsConfig config,Vector3 position)=>Browse.GetTipReward(config,position);
    }
}
