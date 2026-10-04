using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameGuideBookPageValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameGuideBookRewardsValidation.Fixture Data;
            public readonly Dictionary<string,OutgameUiPage> Pages=new Dictionary<string,OutgameUiPage>();
            public readonly List<string> Trace=new List<string>();
            public readonly Queue<IEnumerator> Loads=new Queue<IEnumerator>(),Layouts=new Queue<IEnumerator>();
            public readonly OutgameAssetProvider Provider;
            public readonly OutgameUiPageServices Ui;
            public readonly OutgameUiOpenRegistry<OutgameUiPage> OpenRegistry;
            public readonly OutgameUiCloseRegistry<OutgameUiPage> CloseRegistry;
            public readonly OutgamePrefabPoolControl Pool;
            public readonly GameObject Root,Loading;public readonly Transform Layer;
            public readonly OutgameUiAnimation Runner;
            public readonly OutgameGuideBookItemServices ItemServices;
            public readonly OutgameGuideBookPopupServices PopupServices;
            public readonly bool Native;
            public GameObject LastLoaded;
            public OutgameGuideBookPage Page;
            public Fixture(bool native=false,string savePath=null)
            {
                Native=native;Data=new OutgameGuideBookRewardsValidation.Fixture(savePath);Data.Container.SetActive(false);
                if(Native)
                {
                    Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                    var module=new OutgameUiModuleInitialization(()=>true,(path,ready)=>ready((name,active)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.name=name;go.SetActive(active);return go;}),
                        ()=>throw new Exception("Unexpected legacy root"),OutgameUiDisplaySettings.Shared,Debug.Log,s=>{throw new Exception(s);});
                    module.Initialize();Layer=module.UiRoot.Find("UIPopup");
                }
                else{Root=new GameObject("guide-page-module",typeof(RectTransform));Layer=new GameObject("UIPopup",typeof(RectTransform)).transform;Layer.SetParent(Root.transform,false);}
                Loading=new GameObject("module-loading");Loading.transform.SetParent(Root.transform,false);Runner=Root.AddComponent<OutgameUiAnimation>();
                var registry=new OutgameControllerRegistry();Pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));registry.Bind(4561,()=>Pool);registry.Resolve(4561);Pool.OnInit();
                Provider=new OutgameAssetProvider(()=>false,s=>Trace.Add("warning:"+s)){Status=4,AssetObject=Resources.Load<GameObject>("Recovered/GuideBook/GuideBookUI")};
                var loader=new OutgameUiModernLoader((path,ready)=>{Require(path=="UI/MainMenu/GuideBookUI","source UI resource path");Trace.Add("load");var handle=Provider.CreateHandle(path,()=>false);ready(handle);return handle;},name=>{Require(name=="UIPopup","source layer2 name");return Layer;},
                    routine=>{if(Native)Runner.StartCoroutine(routine);else Loads.Enqueue(routine);},(p,l,cb)=>throw new Exception("Unexpected legacy load"));
                CloseRegistry=new OutgameUiCloseRegistry<OutgameUiPage>(Pages,p=>((IOutgameOwnedUiPage)p).CloseInModule());
                Ui=new OutgameUiPageServices{Loader=()=>loader,UsesNewResources=()=>true,Animation=Runner,CloseRegistry=()=>CloseRegistry,Messages=()=>Data.Messages,
                    MaximumWindowIndex=()=>throw new Exception("Popup layer should not request window index"),WideWindowSorting=()=>throw new Exception("Popup layer should not request window sort"),Loading=()=>Loading,
                    UnloadUnusedAssets=()=>Trace.Add("unload:"+Provider.RefCount),UnloadUnusedBundle=p=>throw new Exception("Unexpected legacy unload"),Log=s=>Trace.Add("log:"+s),DestroyObject=g=>{if(Native)UnityEngine.Object.Destroy(g);else UnityEngine.Object.DestroyImmediate(g);}};
                var language=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));var sprites=new OutgameGuideBookSprites();
                ItemServices=new OutgameGuideBookItemServices{Control=()=>Data.Control,Config=()=>Data.Config,Page=()=>Pages.TryGetValue("GuideBookUI",out var p)?(IOutgameGuideBookItemsPage)p:null,
                    LangValue=l=>language.Chinese(l.key),Language=language.Chinese,LanguageFormat=(k,a)=>string.Format(language.Chinese(k),a),Toast=s=>Trace.Add("toast:"+s),Voice=id=>Trace.Add("voice:"+id),Messages=()=>Data.Messages,
                    StartCoroutine=r=>{if(Native)Runner.StartCoroutine(r);else Layouts.Enqueue(r);}};
                PopupServices=new OutgameGuideBookPopupServices{Config=ItemServices.Config,Control=ItemServices.Control,CurrentCommanderId=()=>1,Language=language.Chinese,
                    SetSprite=(image,key,atlas,size)=>image.sprite=sprites.Find(key),Voice=ItemServices.Voice,Messages=ItemServices.Messages,ClosePage=()=>throw new Exception("Page must own close")};
                var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/GuideBook/hud-import").text,"GuideBookUI");
                OpenRegistry=new OutgameUiOpenRegistry<OutgameUiPage>(Pages,name=>{Require(name=="Proj_hdzd.UI.MainMenu.GuideBookUI","source page type");return new OutgameUiType<OutgameUiPage>("GuideBookUI",()=>{
                    Page=new OutgameGuideBookPage(Ui,outlets,ItemServices,PopupServices,()=>Data.Tools,()=>Data.Effects,()=>Pool,()=>Resources.Load<GameObject>("Recovered/GuideBook/GuideBookItem")){Cached=!Native};return Page;});},
                    (p,args)=>((IOutgameOwnedUiPage)p).Open(args),s=>{throw new Exception(s);});
                Data.Messages.AddListener("GF_VisibleUI",args=>{
                    // Edit mode does not deliver Unity Awake. Replay component initialization before the page's Awake;
                    // native validation uses actual Unity callbacks without this branch.
                    if(!Native&&(bool)args[1]){Page.Browse.GuideTab.SendMessage("Awake");Page.Browse.TipTab.SendMessage("Awake");Page.Browse.Tabs.SendMessage("Awake");}
                    Trace.Add("visible:"+args[1]);});Data.Messages.AddListener("OpenUI",args=>Trace.Add("open"));Data.Messages.AddListener("CloseUI",args=>Trace.Add("close"));
            }
            public OutgameGuideBookPage Open(params object[] args)=>(OutgameGuideBookPage)OpenRegistry.Open("Proj_hdzd.UI.MainMenu","GuideBookUI",args);
            public void CompleteLoad(){var routine=Loads.Dequeue();Require(routine.MoveNext()&&routine.MoveNext(),"source two-frame delay");Require(!routine.MoveNext(),"source completion follows two frames");LastLoaded=Page.GameObject;}
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Root);Pool.OnDispose();Data.Dispose();}
        }
        public static void Require(bool ok,string reason){if(!ok)throw new Exception(reason);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Composed GuideBook page with original prefab/config/data/rewards and recovered BaseUI/UIModule load contracts. Edit cases manually advance loader frames in cached mode. Native open/animation/async close is separate; local asset acquisition and account/SkillControl/audio/effects endpoints are explicit fixture services, not full Main production completion."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("guide-page-registry-load-and-source-awake-order",()=>{
                using(var f=new Fixture()){
                    var args=new object[]{"saved-reference"};var page=f.Open(args);Require(page.GameObject==null&&page.Lifetime.Arguments==args&&page.MainHandle!=null&&f.Provider.RefCount==1,"register/arguments/handle before delayed completion");
                    Require(f.Open("ignored")==page&&page.Lifetime.Arguments==args&&f.Loads.Count==1,"existing page prevents duplicate load/argument replacement");
                    f.CompleteLoad();Require(page.OutletCount==24&&page.GameObject.activeSelf&&page.GameObject.transform.parent==f.Layer,"original outlets and native parent");
                    Require(page.GameObject.GetComponent<Canvas>()==null&&page.Canvas.WindowIndex==0,"popup layer does not add independent window canvas");
                    Require(page.Data.Data.Count==f.Data.Config.dicGuidebook.Count&&page.Browse.Tips.Count==f.Data.Config.dicGuideTips.Count,"both original dictionaries populated");
                    Require(page.Browse.Tabs.Current==page.Browse.GuideTab&&page.Browse.GuideTab.IsSelect&&!page.Browse.TipTab.IsSelect,"source Awake selects guide tab");
                    Require(f.Trace.IndexOf("visible:True")<f.Trace.IndexOf("voice:2001")&&!f.Trace.Contains("open"),"base visibility precedes Awake; cached mode suppresses OpenUI");
                }
            });
            check("guide-page-actual-provider-popup-reward-and-tip-page-lookup",()=>{
                using(var f=new Fixture()){
                    var page=f.Open();f.CompleteLoad();OutgameDynamicListValidation.Tick(page.List.List);
                    var item=((OutgameGuideBookDynamicItem)page.List.List.GetItem(0).BaseItem).Item;item.UnlockButton.onClick.Invoke();
                    Require(page.Rewards.SelectedBook==page.Data.Data[0].Config&&page.Popup.Popup.activeSelf,"source singleton lookup reaches page owner");
                    page.Rewards.BookRewardButton.GetComponent<Button>().onClick.Invoke();Require(f.Data.Manager.ContainsGuide(1)&&!item.UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"owner wires real reward back to data provider");
                    page.Browse.Tabs.SetSelect(page.Browse.TipTab);Require(page.Browse.TipTab.IsSelect&&!page.List.List.gameObject.activeInHierarchy,"source selected tip page hides guide scroll");
                }
            });
            check("guide-page-immediate-close-base-disposal-and-data-order",()=>{
                using(var f=new Fixture()){
                    var page=f.Open("arg");f.CompleteLoad();OutgameDynamicListValidation.Tick(page.List.List);var rect=page.Lifetime.RectTransform;
                    page.CloseUINow();Require(page.Lifetime.IsDisposed&&page.GameObject==null&&page.Lifetime.ObjectList==null&&page.Lifetime.Arguments==null,"base fields cleared on immediate close");
                    Require(ReferenceEquals(page.Lifetime.RectTransform,rect)&&page.Data.Data.Count==0&&page.Browse.Tips.Count==0,"source retained rect and guide/tip cleanup");
                    Require(f.Provider.RefCount==1&&!f.Trace.Contains("close")&&f.Pages.ContainsKey("GuideBookUI"),"immediate close does not invent async handle release/event/registry removal");
                }
            });
            check("guide-page-loading-flag-and-disposed-late-completion",()=>{
                using(var f=new Fixture()){
                    var page=f.Open();Require(f.Loading.activeSelf,"defaultfalse loading flag leaves module loading untouched");
                    page.ShowLoading=true;page.Lifetime.MarkDisposed();f.CompleteLoad();Require(page.GameObject==null&&page.Lifetime.IsDisposed&&f.Provider.RefCount==1,"late root destroyed, handle not silently released");
                }
            });
            return r;
        }
        public static void Validate()
        {
            var r=Run();var c=OutgameDynamicCenterValidation.Run();r.checks.AddRange(c.checks);r.passed&=c.passed;
            System.IO.File.WriteAllText(System.IO.Path.Combine(BattleBuild.Workspace,"analysis/guide-page-validation.json"),JsonUtility.ToJson(r,true));Debug.Log("AREABATTLE_GUIDE_PAGE_"+(r.passed?"PASS":"FAIL")+" cases="+r.checks.Count);EditorApplication.Exit(r.passed?0:1);
        }
    }
}
