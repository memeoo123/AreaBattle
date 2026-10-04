using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskPageValidation
    {
        [Serializable] sealed class ItemRows {public List<SharedItemConfig.GameItemConfig> Datas;}
        public sealed class Reports:IOutgameLimitTaskPageReports
        {
            public readonly List<OutgameActivityReport> Sent=new List<OutgameActivityReport>();public Action Creating,Sending;
            public OutgameActivityReport Create(bool initialize,object argument){Require(initialize&&argument==null,"source report construction");Creating?.Invoke();return new OutgameActivityReport();}
            public void Send(OutgameActivityReport report){Sent.Add(report);Sending?.Invoke();}
        }
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTimeTaskValidation.Fixture Data;
            public readonly Dictionary<string,OutgameUiPage> Pages=new Dictionary<string,OutgameUiPage>();
            public readonly List<string> Trace=new List<string>();public readonly Queue<IEnumerator> Loads=new Queue<IEnumerator>();
            public readonly OutgameAssetProvider Provider;public readonly OutgameUiPageServices Ui;public readonly OutgameLimitTaskPageServices Services;
            public readonly OutgameUiOpenRegistry<OutgameUiPage> OpenRegistry;public readonly OutgameUiCloseRegistry<OutgameUiPage> CloseRegistry;
            public readonly OutgamePrefabPoolControl Pool;public readonly GameObject Root;public readonly Transform Layer;public readonly OutgameUiAnimation Runner;
            public readonly OutgameLegacyConfigManager Config;public readonly Reports ReportHost=new Reports();
            public readonly OutgameLimitTaskAccValidation.Effects Effects=new OutgameLimitTaskAccValidation.Effects();
            public readonly OutgameSevendayActivityControl Sevenday;public readonly bool Native;public OutgameLimitTaskPage Page;
            public Action SpriteCallback;public long Now;public int Ended;
            public OutgameChildLimitTimeTaskActivity Child=>Data.Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            public OutgameMessageDispatcher Messages=>Data.Runtime.Statistics.Messages;
            public OutgameCommonMessageDispatcher Common=>Data.Runtime.Statistics.Common;
            public Fixture(bool native=false,string path=null)
            {
                Native=native;Data=new OutgameLimitTimeTaskValidation.Fixture(path,native);Child.ModuleData.ext.dayId=2;
                foreach(var row in JsonUtility.FromJson<ItemRows>(Resources.Load<TextAsset>("Recovered/FirstPack/Config/GameItemConfig").text).Datas)Data.Items.Config.Instance.Items[row.id]=row;
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);}).ReadTable(Config.dicGameItem);
                var registry=new OutgameControllerRegistry();Pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));registry.Bind(4561,()=>Pool);registry.Resolve(4561);Pool.OnInit();
                Sevenday=new OutgameSevendayActivityControl(registry,new OutgameSevendayActivityServices{Statistics=Data.Runtime.Statistics.Expansion}){Child=Child};
                Child.Data.LaunchTimeStamp=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3));Now=Child.Data.LaunchTimeStamp+1000;Data.Runtime.Statistics.Owner.ValueProviders[10000]=a=>Now;
                if(Native)
                {
                    Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                    var module=new OutgameUiModuleInitialization(()=>true,(p,ready)=>ready((name,active)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.name=name;go.SetActive(active);return go;}),()=>throw new Exception("legacy root"),OutgameUiDisplaySettings.Shared,Debug.Log,s=>{throw new Exception(s);});
                    module.Initialize();Layer=module.UiRoot.Find("UIPopup");
                }
                else{Root=new GameObject("task-page-module",typeof(RectTransform));Layer=new GameObject("UIPopup",typeof(RectTransform)).transform;Layer.SetParent(Root.transform,false);}
                Runner=Root.AddComponent<OutgameUiAnimation>();Provider=new OutgameAssetProvider(()=>false,s=>Trace.Add("warning:"+s)){Status=4,AssetObject=Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskUI")};
                var loader=new OutgameUiModernLoader((p,ready)=>{Require(p=="UI/MainMenu/CommonLimitTimeTaskUI","original resource path");Trace.Add("load");var handle=Provider.CreateHandle(p,()=>false);ready(handle);return handle;},name=>{Require(name=="UIPopup","source popup layer");return Layer;},routine=>{if(Native)Runner.StartCoroutine(routine);else Loads.Enqueue(routine);},(p,l,cb)=>throw new Exception("legacy load"));
                CloseRegistry=new OutgameUiCloseRegistry<OutgameUiPage>(Pages,p=>((IOutgameOwnedUiPage)p).CloseInModule());
                Ui=new OutgameUiPageServices{Loader=()=>loader,UsesNewResources=()=>true,Animation=Runner,CloseRegistry=()=>CloseRegistry,Messages=()=>Messages,
                    MaximumWindowIndex=()=>throw new Exception("popup window index"),WideWindowSorting=()=>throw new Exception("popup sorting"),Loading=()=>Root,
                    UnloadUnusedAssets=()=>Trace.Add("unload:"+Provider.RefCount),UnloadUnusedBundle=p=>throw new Exception("legacy unload"),Log=s=>Trace.Add("log:"+s),
                    DestroyObject=native?(Action<GameObject>)(g=>UnityEngine.Object.Destroy(g)):(g=>{if(Page!=null&&ReferenceEquals(g,Page.GameObject))return;UnityEngine.Object.DestroyImmediate(g);})};
                var inventory=new OutgameLocalInventory(new OutgameProfile().inventory,BattleView.ReadText("Data/AllSkillConfig"));
                var dispatcher=new OutgameToolDispatcher(inventory,Effects,BattleView.ReadText("Data/Outgame/GameItemConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/SkinConfig"));
                var tools=new OutgameToolControl(()=>dispatcher,()=>inventory,id=>0,()=>"fixture-purchase");
                var reward=new OutgameLimitTaskRewardItemServices{Config=()=>Config,SetSprite=(image,key,atlas,size)=>{Trace.Add("sprite");SpriteCallback?.Invoke();},PopItemInfo=(rect,id)=>Trace.Add("detail:"+id)};
                Services=new OutgameLimitTaskPageServices{Activities=()=>Data.Runtime.Control,Config=()=>Data.Config,Sevenday=()=>Sevenday,Statistics=Data.Runtime.Statistics.Expansion,Common=()=>Common,Language=lang=>lang.key,Reports=()=>ReportHost,Pool=()=>Pool,
                    TaskPrefab=()=>Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskItem"),DayPrefab=()=>Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskPageItem"),
                    Tasks=new OutgameLimitTaskItemServices{Activities=()=>Data.Runtime.Control,Messages=()=>Messages,LanguageFormat=(key,args)=>key,Rewards=reward,Destroy=Ui.DestroyObject},
                    Days=new OutgameLimitTaskPageItemServices{Activities=()=>Data.Runtime.Control,Messages=()=>Messages,Common=()=>Common,LanguageFormat=(key,args)=>key},
                    Accumulator=new OutgameLimitTaskAccItemServices{ActivityConfig=()=>Data.Config,Config=()=>Config,Page=()=>Pages.TryGetValue(OutgameLimitTaskPage.SourceName,out var p)?(IOutgameLimitTaskAccPage)p:null,
                        Effects=()=>Effects,Tools=()=>tools,GoodsType=dispatcher.GoodsType,CoinCost=()=>"fixture-CoinCost",ActivityReason=()=>"fixture-Activity",ShowSkinReward=args=>Trace.Add("skin:"+args[0]),SetSprite=reward.SetSprite,Messages=()=>Messages,Destroy=Ui.DestroyObject},
                    Preview=new OutgameSevendayAccPreviewServices{Rewards=reward,Destroy=Ui.DestroyObject}};
                if(!Native){Services.Preview.AddUpdate=a=>0;Services.Preview.RemoveUpdate=a=>{};Services.Preview.EndOfFrame=instruction=>Task.CompletedTask;}
                var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/LimitTask/hud-import").text,"CommonLimitTimeTaskUI");
                OpenRegistry=new OutgameUiOpenRegistry<OutgameUiPage>(Pages,name=>{Require(name==OutgameLimitTaskPage.SourceNamespace+"."+OutgameLimitTaskPage.SourceName,"source page type");return new OutgameUiType<OutgameUiPage>(OutgameLimitTaskPage.SourceName,()=>Page=new OutgameLimitTaskPage(Ui,outlets,Services){Cached=!Native});},(p,args)=>((IOutgameOwnedUiPage)p).Open(args),s=>{throw new Exception(s);});
                Messages.AddListener("GF_VisibleUI",args=>Trace.Add("visible:"+args[1]));Messages.AddListener("OpenUI",args=>Trace.Add("open"));Messages.AddListener("CloseUI",args=>Trace.Add("close"));Messages.AddListener("SevendayClose",args=>Ended++);
            }
            public OutgameLimitTaskPage Open(params object[] args)=>(OutgameLimitTaskPage)OpenRegistry.Open(OutgameLimitTaskPage.SourceNamespace,OutgameLimitTaskPage.SourceName,args);
            public void CompleteLoad(){var routine=Loads.Dequeue();Require(routine.MoveNext()&&routine.MoveNext(),"original two-frame load delay");Require(!routine.MoveNext(),"load completion after two frames");}
            public void Tick(){OutgameDynamicListValidation.Tick(Page.Days.List);OutgameDynamicListValidation.Tick(Page.Tasks.List);}
            public void Start(){Open();CompleteLoad();Tick();}
            public OutgameLimitTaskView TaskView(int id){int i=Page.TaskData.Data.FindIndex(t=>t.id==id);Page.Tasks.List.CenteredWithIndex(i);OutgameDynamicListValidation.Tick(Page.Tasks.List);return (OutgameLimitTaskView)Page.Tasks.List.GetItem(i).BaseItem;}
            public void Dispose(){if(Page!=null&&!Page.Lifetime.IsDisposed&&Page.GameObject)Page.CloseUINow();UnityEngine.Object.DestroyImmediate(Root);Pool.OnDispose();Data.Dispose();}
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Composed original limited-task page with actual registry, recovered BaseUI loading/close, activity and UI components. Edit-mode frames are controlled; native page lifecycle separate. Report/localization/sprite/currency/skin/detail and resource acquisition hosts explicit fixtures; production menu/Main/platform/Player/audiovisual acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="limit-task-page-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="limit-task-page-"+id,result="fail",detail=e.ToString()});}};
            check("registry-load-original-outlets-and-day-selection-populates-page",()=>{
                using(var f=new Fixture()){
                    var args=new object[]{"held"};var page=f.Open(args);Require(page.GameObject==null&&page.Lifetime.Arguments==args&&f.Provider.RefCount==1,"published owner and handle before load");
                    Require(f.Open("ignored")==page&&page.Lifetime.Arguments==args&&f.Loads.Count==1,"existing registry page avoids duplicate load");f.CompleteLoad();
                    Require(page.GameObject.transform.parent==f.Layer&&page.Title.text==page.Config.name.key&&page.DayData.Data.Count==7&&page.GameObject.GetComponent<Canvas>()==null,"original title/seven days/popup layer");
                    Require(f.ReportHost.Sent.Count==1&&f.ReportHost.Sent[0].ActivityName=="七日嘉年华"&&f.Trace.Contains("visible:True")&&!f.Trace.Contains("open"),"source entry report and cached lifecycle");f.Tick();
                    Require(page.SelectedDay==1&&page.AccItems.Count==8&&page.TaskData.Data.Count==f.Child.GetDayTaskList(1).Count&&page.AccNumber.text=="0","selected renderer message populates actual task and accumulator components");
                }
            });
            check("day-pointer-writes-last-click-without-dirty-and-rebuilds",()=>{
                using(var f=new Fixture()){
                    f.Start();f.Data.Parent.Dirty=false;var old=f.Page.AccItems[1];var day=(OutgameLimitTaskPageView)f.Page.Days.List.GetItem(1).BaseItem;day.Normal.onClick.Invoke();
                    Require(f.Page.SelectedDay==2&&f.Child.ModuleData.ext.lastClickDayId==2&&!f.Data.Parent.Dirty&&old.Lifetime.IsDisposed,"actual day button updates source lastClick and rebuilds without inventing dirty save");
                    Require(f.Page.TaskData.Data.TrueForAll(t=>t.Config.day==2)&&f.Page.AccItems[1].Lifetime.GameObject.name=="Node0","correct day task list and original accumulator names");
                }
            });
            check("actual-task-claim-auto-refreshes-accumulator-through-page-listeners",()=>{
                using(var f=new Fixture()){
                    f.Start();f.Data.Runtime.Statistics.Expansion.SetEventCount(10015,10);var task=f.TaskView(1301101);var old=f.Page.AccItems[1];task.Claim.onClick.Invoke();f.Tick();
                    Require(f.Child.FindTask(1301101).state==1&&f.Data.Items.Global.GetItemCount(1301)==10&&old.Lifetime.IsDisposed&&f.Page.AccNumber.text=="10","actual activity notification reaches page, rebuilds accumulator and displays real liveness");
                    Require(f.TaskView(1301101).Claimed.gameObject.activeSelf,"page automatically renders claimed task without test forwarding");
                }
            });
            check("countdown-source-format-and-end-message-does-not-close",()=>{
                Require(OutgameLimitTaskPage.FormatDuration(90061)=="1天01时"&&OutgameLimitTaskPage.FormatDuration(3661)=="01时01分01秒"&&OutgameLimitTaskPage.FormatDuration(-1)=="00时00分-1秒","source day cutoff, suffixes and negative remainder");
                using(var f=new Fixture()){
                    f.Start();Require(f.Page.Countdown.text=="剩余时间: 6天23时","actual cached seven-day end and Chinese formatter");f.Now=f.Sevenday.GetActDate(false);
                    f.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());f.Common.SendMessage(OutgameStatisticsMessageKey.Get(10000),Array.Empty<object>());
                    Require(f.Ended==2&&f.Page.Countdown.text=="活动结束"&&!f.Page.Lifetime.IsDisposed&&f.Pages.ContainsKey(OutgameLimitTaskPage.SourceName),"each ended refresh notifies without auto-close");
                }
            });
            check("message-argument-guards-and-source-invalid-mode-lookup",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;p.SelectPage(null);p.SelectPage(new object[]{1301});p.RefreshTasks(new object[]{1302});p.RefreshAccProgress(new object[]{1});
                    Throws<IndexOutOfRangeException>(()=>p.RefreshTasks(new object[]{1301}));Throws<InvalidCastException>(()=>p.SelectPage(new object[]{1301,"1"}));
                    p.Config.accType=9;Throws<KeyNotFoundException>(()=>p.RefreshAccProgress(new object[]{p.SelectedDay,f.Child.ModuleData.ext}));p.Config.accType=1;
                    Require(p.AccNumber.text=="0","unused mode dictionary lookup still throws before UI writes");
                }
            });
            check("extension-refresh-ignores-payload-and-preview-uses-original-position",()=>{
                using(var f=new Fixture()){
                    f.Start();var old=f.Page.AccItems[1];f.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{9999});Require(old.Lifetime.IsDisposed,"page rebuilds even unrelated extension payload");
                    var item=f.Page.AccItems[1];OutgameUiPointerClick.Get(item.Lifetime.GameObject).OnPointerClick(null);
                    Require(f.Page.Preview.Visible&&f.Page.Preview.Lifetime.Transform.position==item.BoxLocked.transform.position&&ReferenceEquals(f.Page.Preview.Data,item.Data.RewardsData)&&f.Page.Preview.Items.Count==2,"actual page preview binds source reward list, origin and refresh");
                }
            });
            check("accumulator-render-failure-keeps-untracked-clone-and-cleared-prefix",()=>{
                using(var f=new Fixture()){
                    f.Start();var old=f.Page.AccItems[1];f.SpriteCallback=()=>throw new InvalidOperationException("icon failure");Throws<InvalidOperationException>(()=>f.Page.RefreshAccItems(null));
                    Require(old.Lifetime.IsDisposed&&f.Page.AccItems.Count==7&&f.Page.AccLayout.transform.childCount==8,"final skin sprite throws after seven published rows and eighth native clone before dictionary add");
                }
            });
            check("source-disposal-removes-page-listeners-retains-provider-dictionary-preview",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;var preview=p.Preview;var row=p.AccItems[1];p.CloseUINow();
                    Require(p.Lifetime.IsDisposed&&p.GameObject==null&&p.DayData.Data.Count==7&&p.TaskData.Data.Count>0&&p.AccItems.Count==8&&row.Lifetime.IsDisposed&&!preview.Lifetime.IsDisposed,"base clear then source item disposal, collections and preview object retained");
                    Require(p.DayData.Data.TrueForAll(d=>!d.Selected)&&f.Provider.RefCount==1&&f.Pages.ContainsKey(OutgameLimitTaskPage.SourceName),"KillSelect clears data; immediate close does not release handle/remove registry");
                    f.Messages.SendMessage(OutgameLimitTaskPageView.SelectPage,new object[]{1301,2});f.Common.SendMessage("CommonModule_NoviceExtRefresh",Array.Empty<object>());
                    Require(p.SelectedDay==1,"removed page listeners no longer mutate disposed owner");
                }
            });
            check("report-failure-retains-initialized-prefix-and-close-hook-failure-stops-button-message",()=>{
                using(var f=new Fixture()){
                    f.ReportHost.Sending=()=>throw new InvalidOperationException("report failure");f.Open();Throws<InvalidOperationException>(f.CompleteLoad);
                    Require(f.Page.GameObject&&f.Page.DayData.Data.Count==7&&f.Provider.RefCount==1,"report failure follows native initialization and data population");
                    int clicked=0;f.Messages.AddListener("GF_UIButtonClick",args=>clicked++);f.Page.CloseAction=()=>throw new InvalidOperationException("close hook");Throws<InvalidOperationException>(()=>f.Page.CloseButton.onClick.Invoke());Require(clicked==0&&!f.Page.Lifetime.IsDisposed,"close hook exception prevents subsequent button message/disposal");
                }
            });
            return r;
        }
    }
}
