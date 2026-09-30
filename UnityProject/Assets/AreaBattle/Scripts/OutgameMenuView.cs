using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Original prefab binding. Dynamic page controllers attach through PageShown.
    public sealed class OutgameMenuView : MonoBehaviour
    {
        public OutgameMenuNavigation Navigation { get; private set; }
        public event Action<OutgameMenuPage> PageShown;
        public event Action<OutgameMenuPage> PageRequestAccepted;
        public event Action CommanderLockedClicked;
        public event Action<OutgameMenuPage> PageRefreshRequested;
        public event Action SelectedCommanderRefreshRequested;
        public RectTransform Menu { get; private set; }
        public bool CommanderLocked { get; private set; }
        readonly Dictionary<OutgameMenuPage,RectTransform> pages=new Dictionary<OutgameMenuPage,RectTransform>();
        readonly Dictionary<RectTransform,OutgameUiPage> visibility=new Dictionary<RectTransform,OutgameUiPage>();
        RectTransform outgoing,incoming;Transform pageParent;
        float elapsed,width;int direction;
        // Provisional presentation curve: source tween duration/direction are proven, default easing is not.
        public AnimationCurve TransitionCurve=AnimationCurve.Linear(0,0,1,1);
        public void Initialize(RectTransform parent,int currentLevel,OutgameCommanderProgression rules,Action<OutgameMenuPage,RectTransform> bindPage=null)
        {
            if(Navigation!=null)throw new InvalidOperationException("Menu already initialized");
            pageParent=parent;width=parent.rect.width;
            foreach(var p in new[]{OutgameMenuPage.Skins,OutgameMenuPage.Main,OutgameMenuPage.Commander})
            {
                var prefab=Resources.Load<GameObject>("Recovered/Outgame/"+OutgameMenuNavigation.SourceUI(p));
                if(!prefab)throw new InvalidOperationException("Missing original page "+p);
                var obj=Instantiate(prefab,parent,false);obj.name=prefab.name;pages.Add(p,(RectTransform)obj.transform);obj.SetActive(false);
            }
            Menu=(RectTransform)Instantiate(Resources.Load<GameObject>("Recovered/Outgame/MenuTabUI"),parent,false).transform;
            Menu.name="MenuTabUI";Menu.gameObject.SetActive(true);
            Navigation=new OutgameMenuNavigation();
            Navigation.FirstPageShown+=p=>{visibility[pages[p]].SetVisible(true);RefreshTabs();PageShown?.Invoke(p);};
            Navigation.TransitionStarted+=BeginTransition;
            Bind("objSkinUnSelected",OutgameMenuPage.Skins);Bind("objMainUnSelected",OutgameMenuPage.Main);Bind("objOtherUnSelected",OutgameMenuPage.Commander);
            SetTab("objShopUnSelected",false);SetTab("objShopSelected",false);SetTab("objItemUnSelected",false);SetTab("objItemSelected",false);
            SetCommanderLevel(currentLevel,rules);
            foreach(var pair in pages)
            {
                bindPage?.Invoke(pair.Key,pair.Value);
                var page=pair.Key;
                var item=new OutgameMenuPageVisibility(pair.Value.gameObject,page,()=>PageRefreshRequested?.Invoke(page),()=>SelectedCommanderRefreshRequested?.Invoke());visibility.Add(pair.Value,item);item.SetVisible(false);
            }
            Navigation.ReturnToMain();
        }
        Transform Tab(string name)=>Menu.Find("objBottomTab/TabContent/"+name)??throw new InvalidOperationException("Original tab missing: "+name);
        void SetTab(string name,bool visible)=>Tab(name).gameObject.SetActive(visible);
        void Bind(string name,OutgameMenuPage page)
        {
            var target=Tab(name);var button=target.GetComponent<Button>()??target.gameObject.AddComponent<Button>();
            button.onClick.AddListener(()=>RequestPage(page));
        }
        public void SetCommanderLevel(int level,OutgameCommanderProgression rules)
        {
            CommanderLocked=OutgameMenuNavigation.CommanderLocked(level,rules);
            var overlay=Tab("objOtherUnSelected").Find("imgCommanderLock");overlay.gameObject.SetActive(CommanderLocked);
            var button=overlay.GetComponent<Button>()??overlay.gameObject.AddComponent<Button>();button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>CommanderLockedClicked?.Invoke());
        }
        public bool RequestPage(OutgameMenuPage page)
        {
            if(page==OutgameMenuPage.Commander&&CommanderLocked){CommanderLockedClicked?.Invoke();return false;}
            bool accepted=Navigation.CheckUI(page);
            if(accepted)PageRequestAccepted?.Invoke(page);
            return accepted;
        }
        public RectTransform Page(OutgameMenuPage page)=>pages[page];
        void RefreshTabs()
        {
            foreach(var pair in new[]{(OutgameMenuPage.Skins,"Skin"),(OutgameMenuPage.Main,"Main"),(OutgameMenuPage.Commander,"Other")})
            {bool selected=Navigation.Current==pair.Item1;SetTab("obj"+pair.Item2+"Selected",selected);SetTab("obj"+pair.Item2+"UnSelected",!selected);}
        }
        void BeginTransition(OutgameMenuPage oldPage,OutgameMenuPage newPage,int sign,float duration)
        {
            outgoing=pages[oldPage];incoming=pages[newPage];direction=sign;elapsed=0;
            outgoing.anchoredPosition=Vector2.zero;incoming.SetParent(outgoing,false);incoming.anchoredPosition=new Vector2(direction*width,0);visibility[incoming].SetVisible(true);
            RefreshTabs();
        }
        void Update(){if(outgoing)AdvanceTransition(Time.deltaTime);}
        public void AdvanceTransition(float delta)
        {
            if(!outgoing)return;
            elapsed+=Mathf.Max(0,delta);float normalized=Mathf.Clamp01(elapsed/OutgameMenuNavigation.TransitionSeconds);
            outgoing.anchoredPosition=new Vector2(-direction*width*TransitionCurve.Evaluate(normalized),0);
            if(normalized<1)return;
            incoming.SetParent(pageParent,false);incoming.anchoredPosition=Vector2.zero;
            visibility[outgoing].SetVisible(false);outgoing.anchoredPosition=Vector2.zero;outgoing=null;incoming=null;
            Menu.SetAsLastSibling();Navigation.CompleteTransition();PageShown?.Invoke(Navigation.Current);
        }
    }
}
