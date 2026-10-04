using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameMenuTabPage:IOutgameMenuItem {void DoClose();}
    public sealed class OutgameUiControlGlobals {public RectTransform UiRoot;public Camera UiCamera;}
    // UIControl32739/32709/32735 plus play-state handler32726 and display paths32704/32710.
    public sealed class OutgameUiControl:IOutgameLogicControl
    {
        readonly Func<OutgameUiModuleInitialization> module;
        readonly OutgameControllerRegistry registry;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly OutgameUiControlGlobals globals;
        readonly OutgameMenuItems items;
        readonly Func<IOutgameTopInfoPage> showTop;
        readonly Func<IOutgameMenuTabPage> showMenu,getMenu;
        readonly Action<int,int> switchMenuRoles;
        readonly Func<string,OutgameLoadingPage> createLoading;readonly Func<string,Transform> uiNode;
        public bool ActiveUpdate {get;set;}
        public int CurrentPage;
        public float UiWidth {get;private set;}=1080f;
        public IOutgameTopInfoPage TopInfo {get;private set;}
        Text goldText,diamondsText;
        // Source32736/32715: lazily retry Unity-null cached outlet; disposal retains these fields.
        public Text GoldText {get{if(goldText==null&&TopInfo!=null)goldText=((IOutgameCurrencyTopInfo)TopInfo).GoldText;return goldText;}}
        public Text DiamondsText {get{if(diamondsText==null&&TopInfo!=null)diamondsText=((IOutgameCurrencyTopInfo)TopInfo).DiamondsText;return diamondsText;}}
        public IOutgameMenuTabPage MenuTab {get;private set;}
        public OutgameLoadingPage LoadingPage; // Source field72; loading-page show/close owns this reference.
        public OutgameUiControl(Func<OutgameUiModuleInitialization> module,OutgameControllerRegistry registry,Func<OutgameMessageDispatcher> messages,OutgameUiControlGlobals globals,OutgameMenuItems items,Func<IOutgameTopInfoPage> showTop,Func<IOutgameMenuTabPage> showMenu,Func<IOutgameMenuTabPage> getMenu,Action<int,int> switchMenuRoles,Func<string,OutgameLoadingPage> createLoading=null,Func<string,Transform> uiNode=null)
        {this.module=module;this.registry=registry;this.messages=messages;this.globals=globals;this.items=items;this.showTop=showTop;this.showMenu=showMenu;this.getMenu=getMenu;this.switchMenuRoles=switchMenuRoles;this.createLoading=createLoading;this.uiNode=uiNode;}
        public void OnInit()
        {
            globals.UiRoot=module().UiRoot;globals.UiCamera=module().UiCamera;
            UiWidth=module().UiRoot.rect.width;
            // Source invokes ControlBase<UIControl>.I again here, not this.CurUIType.
            ((OutgameUiControl)registry.Resolve(4296)).CurrentPage=0;
            messages().AddListener("GamePlayState",OnGamePlayState);
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Original32709.
        public void OnDispose()
        {
            ActiveUpdate=false;registry.Clear(4296);TopInfo=null;
            messages().RemoveListener("GamePlayState",OnGamePlayState);
        }
        public OutgameLoadingPage OpenLoadingUI() //32697: every request creates and replaces held page.
        {LoadingPage=createLoading("FirstScreenRes/Proj_xqzdLoadingUI");LoadingPage.transform.SetParent(uiNode("UIStory"),false);return LoadingPage;}
        public void CloseLoadingUI() //32694: CLR reference check; clear after Close succeeds.
        {if(!ReferenceEquals(LoadingPage,null))LoadingPage.Close();LoadingPage=null;}
        public void ResetLoadingTime() //32703: direct managed-field write even for a destroyed native owner.
        {if(!ReferenceEquals(LoadingPage,null))LoadingPage.Elapsed=0;}
        public void ShowTopInfoUI()
        {
            if(TopInfo==null)TopInfo=showTop();
            if(!TopInfo.Visible)TopInfo.SetVisible(true);
            TopInfo.RendererPart(7,string.Empty);
        }
        public void ShowMenuTabUI()
        {
            if(MenuTab==null){MenuTab=showMenu();MenuTab.SetVisible(true);}
            else{MenuTab.SetVisible(true);items.OpenAllMenuItemUI();CurrentPage=3;}
            switchMenuRoles(0,CurrentPage);
        }
        void OnGamePlayState(object[] args)
        {
            switch((int)args[0])
            {
                case 2:ShowTopInfoUI();ShowMenuTabUI();return;
                case 3:var menu=getMenu();if(menu!=null)menu.DoClose();TopInfo?.SetVisible(false);return;
                case 8:TopInfo.RendererPart(6,OutgameUiLayerNames.Get(2));return;
                case 11:ShowTopInfoUI();return;
            }
        }
    }
}
