using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using BigInteger=System.Numerics.BigInteger;
namespace AreaBattle
{
    // Live projection of MatchPlayerData4127 fields12/16/48. MatchManager owns this record.
    public interface IOutgameMatchPlayerInfo
    {
        string Nickname {get;set;}
        string AvatarUrl {get;set;}
        bool HasWechatInfo {get;set;}
    }
    public interface IOutgameAvatarSource {void GetAvatar(string url,Action<Sprite> complete);}
    public sealed class OutgameTopInfoServices
    {
        public Func<OutgameDataManagerPool> DataPool;
        public Func<OutgameUserInfoControl> UserInfo;
        public Func<BigInteger> RankScore;
        public OutgameBigNumberSymbols NumberSymbols;
        public Action<object[]> Warning;
        public Func<string,string> Language;
        public Action<Image,string,string,int> SetSprite;
        public Func<OutgameEffectModule> Effects;
        public Action<object[]> ShowUserInfo;
        public Func<IOutgameMatchPlayerInfo> MatchPlayer;
        public Func<string> MyNickname,MyAvatarUrl;
        public Func<IOutgameAvatarSource> Avatars;
        public Action SaveUserData;
        // Null selects the original native WaitUntil coroutine; controlled waits are validation hosts.
        public Func<WaitUntil,Task> WaitUntil;
    }
    public sealed partial class OutgameTopInfoPage:IOutgameOwnedUiPage
    {
        sealed class PageLifetime:OutgameUiLifetime
        {
            public Action DisposePage;
            public PageLifetime(Action<GameObject> destroy):base(null,()=>{},destroy){}
            // TopInfoUI33922 intentionally does not call BaseUI.Dispose.
            public override void Dispose()=>DisposePage();
        }
        public const string SourceName="TopInfoUI",SourceNamespace="Proj_hdzd.UI.MainMenu",SourcePath="MainMenu/TopInfoUI";
        readonly OutgameUiPageServices ui;readonly OutgameTopInfoServices services;readonly PageLifetime lifetime;
        readonly OutgameUiObjectInitialization initialization;readonly OutgameUiLoadRequest load;
        readonly OutgameUiOpenHost openHost;readonly OutgameUiCloseHost closeHost;readonly OutgameUiAsyncClose close;
        public OutgameUiReadyAwait Ready {get;}
        public OutgameUiLifetime Lifetime=>lifetime;
        public Transform Transform=>lifetime.Transform;
        public OutgameUiResourceLists Resources {get;}
        public OutgameUiCanvas Canvas {get;}
        public OutgameLocalDataManager LocalData {get;private set;}
        public int GoldEffectId {get;private set;}=-1;
        public int DiamondEffectId {get;private set;}=-1;
        public GameObject InfoRoot,GoldRoot,DiamondRoot,UserRoot,MatchUserRoot,StaminaRoot,RecoveryArea,NameRed;
        public Image GoldIcon,GoldClick,DiamondIcon,DiamondClick,HeadIcon,HeadBoxIcon,StaminaIcon,StaminaClick,WechatHead;
        public Text NameText,ScoreText,MatchNameText,MatchScoreText,StaminaText,RealStaminaText,RecoveryText;
        public bool ShowLoading,ShowTop=true,CanReportOpen=true;public string UiModel="None";
        public bool Cached {get=>openHost.Cached;set=>openHost.Cached=value;}
        public Action CloseAction;public Task Closing {get;private set;}public Task IconInitialization {get;private set;}
        public OutgameAssetHandle MainHandle=>closeHost.MainHandle;public object LegacyResource=>openHost.LegacyResource;
        public int OutletCount=>initialization.Objects.Count;
        public OutgameTopInfoPage(OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameTopInfoServices services):this(new PageLifetime(ui.DestroyObject),ui,outlets,services){}
        OutgameTopInfoPage(PageLifetime lifetime,OutgameUiPageServices ui,OutgameImportedUiOutlets outlets,OutgameTopInfoServices services):base(()=>lifetime.GameObject,ui.Messages)
        {
            this.ui=ui;this.services=services;this.lifetime=lifetime;lifetime.DisposePage=Dispose;
            Resources=new OutgameUiResourceLists(ui.UnloadUnusedAssets);Canvas=new OutgameUiCanvas(ui.MaximumWindowIndex,ui.WideWindowSorting);
            Ready=new OutgameUiReadyAwait(lifetime,this,ui.Messages,services.WaitUntil);
            initialization=new OutgameUiObjectInitialization(lifetime,this,outlets.Read,InitializeComponent,()=>{},()=>{},Awake);
            openHost=new OutgameUiOpenHost(lifetime,this,initialization,Canvas,ui.Animation,()=>Loading(false),Refresh,OpenParts,null,ui.Log){Layer=Layer,OpenAnimation=0,OpenAnimationTime=0};
            closeHost=new OutgameUiCloseHost(lifetime,this,ui.Animation,Resources,SourcePath,ui.UsesNewResources,null,ui.UnloadUnusedAssets,ui.UnloadUnusedBundle,ui.EndOfFrame){CloseAnimation=0,CloseAnimationTime=0};
            close=new OutgameUiAsyncClose(closeHost,ui.Messages);
            load=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(openHost,ui.Messages),()=>Loading(true),ui.UsesNewResources,ui.Loader,()=>SourcePath,()=>OutgameUiLayerNames.Get(Layer),h=>closeHost.MainHandle=h);
        }
        void Loading(bool value){if(ShowLoading)ApplyObjectVisibility(ui.Loading(),value);}
        void InitializeComponent()
        {
            var n=initialization.Objects;
            InfoRoot=n["objTopInfo"];GoldRoot=n["goldInfo"];GoldText=n["txt_goldNum"].GetComponent<Text>();GoldClick=n["imgGoldClick"].GetComponent<Image>();GoldIcon=n["imgIcon"].GetComponent<Image>();
            DiamondRoot=n["diamondInfo"];DiamondIcon=n["imgDiamondIcon"].GetComponent<Image>();DiamondsText=n["txt_DiamondNum"].GetComponent<Text>();DiamondClick=n["imgDiamondClick"].GetComponent<Image>();
            NameText=n["txtName"].GetComponent<Text>();HeadIcon=n["imgHeadIcon"].GetComponent<Image>();HeadBoxIcon=n["imgHBoxIcon"].GetComponent<Image>();ScoreText=n["txtSocre"].GetComponent<Text>();UserRoot=n["gouserInfo"];
            StaminaIcon=n["imgSpIcon"].GetComponent<Image>();StaminaClick=n["imgSpClick"].GetComponent<Image>();StaminaText=n["txtSpNum"].GetComponent<Text>();RealStaminaText=n["txtRealSpNum"].GetComponent<Text>();RecoveryArea=n["imgRecoveryTimeArea"];RecoveryText=n["txtRecoveryTime"].GetComponent<Text>();
            StaminaRoot=n["spInfo"];MatchUserRoot=n["gouserInfo_match"];MatchScoreText=n["txtSocre_match"].GetComponent<Text>();MatchNameText=n["txtName_match"].GetComponent<Text>();WechatHead=n["wxHead"].GetComponent<Image>();
            sourceParts=new Dictionary<int,GameObject>{{2,GoldRoot},{4,DiamondRoot},{5,StaminaRoot}};
        }
        void Awake()
        {
            ui.Messages().AddListener("LoadStartingUI",RefreshAll);ui.Messages().AddListener("Avatar_Refresh_OnAuth",AvatarAuthorized);
            LocalData=services.DataPool().GetModel<OutgameLocalDataManager>(4119,"LocalDataManager");
            OutgameUiClick.Add(UserRoot,OpenUserInfo);OutgameUiClick.Add(MatchUserRoot,OpenUserInfo);StaminaRoot.SetActive(false);
            NameRed=UserRoot.transform.Find("imgRed").gameObject;UserRoot.SetActive(true);MatchUserRoot.SetActive(false);
            RefreshInfo();StartIcons();RefreshProfile();
        }
        public void RefreshInfo()
        {
            if(GoldText!=null&&LocalData!=null){var gold=GoldText;gold.text=LocalData.GoldNum.ToString();DiamondsText.text=LocalData.DiamondsNum.ToString();}
        }
        public void RefreshProfile()
        {
            var name=NameText;name.text=services.UserInfo().Name;
            var red=NameRed;string currentName=services.UserInfo().Name;red.SetActive(string.Equals(currentName,services.Language("You")));
            var score=ScoreText;BigInteger value=services.RankScore();score.text=services.NumberSymbols.Format(value,services.Warning);
            var head=services.UserInfo().GetHeadportConfig(-1);if(head!=null)services.SetSprite(HeadIcon,head.icon,head.atlas,0);
            var box=services.UserInfo().GetHeadBoxConfig(-1);if(box!=null)services.SetSprite(HeadBoxIcon,box.icon,box.atlas,0);
        }
        public void RefreshAll(object[] args){RefreshInfo();RefreshProfile();}
        public void OpenUserInfo()=>services.ShowUserInfo(Array.Empty<object>());
        async void StartIcons(){IconInitialization=InitializeIconsAsync();await IconInitialization;}
        public async Task InitializeIconsAsync()
        {await Ready.Await();GoldEffectId=services.Effects().Show(1007,GoldIcon.transform);DiamondEffectId=services.Effects().Show(1019,DiamondIcon.transform);}
        public void AvatarAuthorized(object[] args)
        {
            if(args==null||args.Length==0)return;
            string first=args[0].ToString(),second=args[1].ToString();if(string.IsNullOrEmpty(first)||string.IsNullOrEmpty(second))return;
            services.MatchPlayer().HasWechatInfo=true;
            var player=services.MatchPlayer();player.Nickname=services.MyNickname();
            player=services.MatchPlayer();player.AvatarUrl=services.MyAvatarUrl();
            if(!string.IsNullOrEmpty(services.MatchPlayer().AvatarUrl)){
                var source=services.Avatars();string url=services.MatchPlayer().AvatarUrl;source.GetAvatar(url,AvatarLoaded);
            }
            var name=MatchNameText;name.text=services.MatchPlayer().Nickname;
            services.SaveUserData();services.DataPool().SaveData();
        }
        public void AvatarLoaded(Sprite sprite)=>WechatHead.sprite=sprite;
        public void Dispose()
        {
            ui.Messages().RemoveListener("LoadStartingUI",RefreshAll);ui.Messages().RemoveListener("Avatar_Refresh_OnAuth",AvatarAuthorized);
            services.Effects().Close(GoldEffectId);services.Effects().Close(DiamondEffectId);
        }
        public void Refresh(){} // Original33930 empty.
        public void Open(object[] args)=>load.Open(args);
        public void CloseSelf()=>OutgameUiCloseRegistry<OutgameUiPage>.CloseSelf(CloseAction,ui.CloseRegistry,()=>SourceName);
        public void CloseInModule(){Closing=close.CloseAsync();ObserveClose(Closing);}
        static async void ObserveClose(Task task)=>await task;
        public void CloseUINow()=>lifetime.CloseUINow();
    }
}
