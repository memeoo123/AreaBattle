using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskPageValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameTaskControlViewValidation.Fixture Data;
            public readonly Dictionary<string,OutgameUiPage> Pages=new Dictionary<string,OutgameUiPage>();
            public readonly List<string> Trace=new List<string>();public readonly Queue<IEnumerator> Loads=new Queue<IEnumerator>();
            public readonly OutgameUiPageServices Ui;public readonly OutgameTaskPageServices Services;
            public readonly OutgameUiOpenRegistry<OutgameUiPage> OpenRegistry;public readonly OutgameUiCloseRegistry<OutgameUiPage> CloseRegistry;
            public readonly OutgameAssetProvider Provider;public readonly GameObject Root,TopRoot;public readonly Transform Layer;
            public readonly OutgameTopInfoPage Top;public readonly OutgameUiAnimation Runner;public readonly bool Native;
            public readonly OutgameTaskSubviews Subviews;public OutgameTaskPage Page;public int AudioCalls,ClosedEffects;
            public Action Audio,CloseEffect;public readonly OutgameDailyTaskViewServices DailyServices;
            public Fixture(bool native=false,string path=null,Action<OutgameTaskActivityValidation.Fixture> configureBeforeInit=null)
            {
                Native=native;Data=new OutgameTaskControlViewValidation.Fixture(native,path,configureBeforeInit);Data.Rows.Page.SetActive(false);
                Root=Data.Rows.Root;Layer=new GameObject("UITip",typeof(RectTransform)).transform;Layer.SetParent(Root.transform,false);
                Runner=Root.AddComponent<OutgameUiAnimation>();
                TopRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"),Root.transform,false);TopRoot.SetActive(true);Top=OutgameTopInfoPage.FromOriginal(TopRoot,()=>Data.Tasks.Runtime.Statistics.Messages);Top.OpenParts();
                DailyServices=new OutgameDailyTaskViewServices{DailyActivity=()=>Data.Tasks.Child,Config=()=>Data.Tasks.Config,Common=()=>Data.Tasks.Runtime.Statistics.Common,GameValue=Data.Services.Statistics.GameValue,DailyActivityId=()=>130001,FormatHours=s=>"seconds:"+s};
                Subviews=new OutgameTaskSubviews(DailyServices,new OutgameAchievementTaskViewServices{Activity=()=>Data.Achievement,Common=()=>Data.Tasks.Runtime.Statistics.Common,Statistics=Data.Services.Statistics,Warning=Data.Warnings.Add});
                Provider=new OutgameAssetProvider(()=>false,s=>Trace.Add(s)){Status=4,AssetObject=Resources.Load<GameObject>("Recovered/TaskPanel/TaskPanelUI")};
                var loader=new OutgameUiModernLoader((p,ready)=>{Require(p=="UI/MainMenu/TaskPanelUI","original path");var handle=Provider.CreateHandle(p,()=>false);ready(handle);return handle;},name=>{Require(name=="UITip","source layer3");return Layer;},routine=>{if(native)Runner.StartCoroutine(routine);else Loads.Enqueue(routine);},(p,l,cb)=>throw new Exception("legacy load"));
                CloseRegistry=new OutgameUiCloseRegistry<OutgameUiPage>(Pages,p=>((IOutgameOwnedUiPage)p).CloseInModule());
                Ui=new OutgameUiPageServices{Loader=()=>loader,UsesNewResources=()=>true,Animation=Runner,CloseRegistry=()=>CloseRegistry,Messages=()=>Data.Tasks.Runtime.Statistics.Messages,
                    MaximumWindowIndex=()=>throw new Exception("tip index"),WideWindowSorting=()=>false,Loading=()=>Root,UnloadUnusedAssets=()=>Trace.Add("unload"),UnloadUnusedBundle=s=>Trace.Add(s),Log=Trace.Add,
                    DestroyObject=native?(Action<GameObject>)(go=>UnityEngine.Object.Destroy(go)):(go=>{if(Page!=null&&ReferenceEquals(go,Page.GameObject))return;UnityEngine.Object.DestroyImmediate(go);})};
                string manifest=File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-ui-import.json"));
                Services=new OutgameTaskPageServices{Subviews=Subviews,Control=()=>Data.Control,TopInfo=()=>Top,PlayAudio=(kind,ids)=>{Require(kind==1&&ids.SequenceEqual(new[]{2001}),"source audio payload");AudioCalls++;Audio?.Invoke();},Language=key=>{Require(key=="Task.Refresh","source localization key");return "refresh {0}";},
                    Rows=Data.Rows.Services,RowOutlets=new OutgameImportedUiOutlets(manifest,"TaskItemItem"),
                    Preview=new OutgameLivenessPreviewServices{SetSprite=(im,k,a,n)=>{},Language=l=>l.key,PopItemInfo=(rect,id)=>{},Destroy=Ui.DestroyObject,AddUpdate=a=>0,RemoveUpdate=a=>{}},
                    Liveness=new OutgameTaskLivenessServices{Config=()=>Data.Rows.Config,GoodsType=Data.Rows.Services.GoodsType,FormatNumber=(n,b)=>n.ToString(),ShowEffect=(id,tr)=>17,CloseEffect=id=>{ClosedEffects++;CloseEffect?.Invoke();},Effects=()=>Data.Rows.Fx,ShowAward=(id,n)=>{},RefreshTopInfo=()=>{},Messages=Ui.Messages}};
                if(!native){Services.Preview.EndOfFrame=i=>Task.CompletedTask;Services.Liveness.EndOfFrame=i=>Task.CompletedTask;}
                var outlets=new OutgameImportedUiOutlets(manifest,"TaskPanelUI");
                OpenRegistry=new OutgameUiOpenRegistry<OutgameUiPage>(Pages,name=>{Require(name==OutgameTaskPage.SourceNamespace+"."+OutgameTaskPage.SourceName,"source type");return new OutgameUiType<OutgameUiPage>(OutgameTaskPage.SourceName,()=>Page=new OutgameTaskPage(Ui,outlets,Services){Cached=!native});},(page,args)=>((IOutgameOwnedUiPage)page).Open(args),s=>{throw new Exception(s);});
                Ui.Messages().AddListener("OpenUI",a=>Trace.Add("open"));Ui.Messages().AddListener("CloseUI",a=>Trace.Add("close"));
            }
            public OutgameTaskPage Open(params object[] args)=>(OutgameTaskPage)OpenRegistry.Open(OutgameTaskPage.SourceNamespace,OutgameTaskPage.SourceName,args);
            public void CompleteLoad(){var load=Loads.Dequeue();Require(load.MoveNext()&&load.MoveNext()&&!load.MoveNext(),"two-frame loader");}
            // Cached mode deliberately omits OpenLater. Invoke that source step explicitly in edit-mode.
            public void Start(){Open();CompleteLoad();Page.OpenLater();}
            public void Dispose(){if(Page!=null&&!Page.Lifetime.IsDisposed&&Page.GameObject)Page.CloseUINow();Data.Dispose();}
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Complete task-page and shared subview lifecycle over actual Task/Achievement/TopInfo owners and original resources. Edit-mode load/OpenLater steps controlled; native runner separate. Audio/localization/sprite/effect/report acquisition hosts explicit fixture endpoints. Full production Main/account/platform/Player acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="task-page-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="task-page-"+id,result="fail",detail=e.ToString()});}};
            check("singleton-publication-reentry-failure-and-unconditional-clear",()=>{
                OutgameTaskSingleton<object> owner=null;object published=null;int count=0;
                owner=new OutgameTaskSingleton<object>(()=>new object(),o=>{count++;published=owner.I;throw new InvalidOperationException("init");});
                Throws<InvalidOperationException>(()=>{var value=owner.I;});Require(ReferenceEquals(published,owner.I)&&count==1,"published instance survives failed init and reentrant getter does not recurse");owner.Clear();Require(owner.Instance==null,"unconditional singleton clear");
            });
            check("deferred-original-outlets-layer-and-real-subview-owner",()=>{
                using(var f=new Fixture()){
                    var args=new object[]{3};var p=f.Open(args);Require(p.GameObject==null&&f.Provider.RefCount==1&&p.Lifetime.Arguments==args,"handle and args published before frame load");Require(f.Open()==p&&f.Loads.Count==1,"registry retains one page");f.CompleteLoad();
                    Require(p.OutletCount==18&&p.Transform.parent==f.Layer&&p.GameObject.GetComponent<Canvas>()==null,"original eighteen outlets and layer3");Require(f.Subviews.Daily.Instance!=null&&!f.Subviews.Daily.I.LateInited&&f.Subviews.Achievement.Instance==null,"Awake creates only daily view, cached mode omits late init");
                    Require(p.Countdown.text==""&&!p.Preview.Lifetime.GameObject.activeSelf&&p.Liveness.Layer==3,"preview/tier initialization and cleared countdown");p.OpenLater();
                    Require(f.Subviews.Daily.I.LateInited&&f.Subviews.Daily.I.Rows.Count==7&&ReferenceEquals(f.Subviews.Daily.I.RootUI,p),"source provider900001 filters task8 and binds real rows");Require(f.Subviews.Achievement.Instance!=null&&!f.Subviews.Achievement.I.LateInited,"row singleton comparison creates unvisited achievement view");
                    Require(f.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UITip","real top part promoted into task layer");
                }
            });
            check("toggle-late-init-once-false-event-and-effect-order",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;p.ToggleChanged(true,p.AchievementTab);var view=f.Subviews.Achievement.I;Require(view.LateInited&&view.Rows.Count>0&&ReferenceEquals(view.RootUI,p)&&p.AchievementContent.activeSelf&&!p.TaskContent.activeSelf,"achievement switch late initializes actual manager and original rows");
                    var row=view.Rows[1];p.Liveness.EffectHandle=17;p.ToggleChanged(true,p.AchievementTab);Require(p.Liveness.EffectHandle==17,"same selected true event returns before cleanup");p.ToggleChanged(false,p.TaskTab);
                    Require(p.Liveness.EffectHandle==0&&f.ClosedEffects==1&&ReferenceEquals(p.CurrentView,view)&&p.SelectedToggle==p.AchievementTab&&p.TaskContent.activeSelf,"false toggle changes content and effect but retains selected/current");
                    p.ToggleChanged(true,p.TaskTab);p.ToggleChanged(true,p.AchievementTab);Require(ReferenceEquals(row,view.Rows[1]),"second visit keeps source late init rows");
                }
            });
            check("effect-failure-stops-tab-state-before-content",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;bool before=p.TaskContent.activeSelf;p.Liveness.EffectHandle=17;f.CloseEffect=()=>throw new InvalidOperationException("effect");Throws<InvalidOperationException>(()=>p.ToggleChanged(true,p.AchievementTab));Require(p.Liveness.EffectHandle==17&&p.TaskContent.activeSelf==before&&p.SelectedToggle==null,"source effect failure retains handle and tab state");f.CloseEffect=null;
                }
            });
            check("late-init-flag-is-written-before-render-failure",()=>{
                using(var f=new Fixture()){
                    f.Open();f.CompleteLoad();f.Data.Rows.SpriteCallback=()=>throw new InvalidOperationException("sprite");Throws<InvalidOperationException>(f.Page.InitializeCurrent);var daily=f.Subviews.Daily.I;Require(daily.LateInited&&daily.Rows.Count==1,"partial row publication and late flag retained");f.Data.Rows.SpriteCallback=null;f.Page.InitializeCurrent();Require(daily.Rows.Count==1,"retry does not invent late initialization");
                }
            });
            check("countdown-empty-routing-unbound-row-and-pointer-audio",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;f.Data.Tasks.Runtime.Statistics.Common.SendMessage("CommonModule_ResetTimeRefresh",new object[]{130001,61000L});Require(p.Countdown.text=="refresh seconds:61","actual countdown listener reaches source page formatter");
                    p.SetTaskEmpty(true);Require(p.EmptyDaily.gameObject.activeSelf,"daily placeholder routing");p.ToggleChanged(true,p.AchievementTab);p.SetTaskEmpty(true);Require(p.EmptyAchievement.activeSelf,"achievement placeholder routing");
                    OutgameUiPointerClick.Get(p.TaskTab.gameObject).OnPointerClick(null);Require(f.AudioCalls==1,"generic toggle pointer plays audio");p.CurrentView=new object();var row=p.GetTaskItem();Require(row!=null&&row.Lifetime.GameObject==null,"unknown current returns source uninstantiated row");
                }
            });
            check("close-audio-before-hook-and-button-message",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;int messages=0;f.Ui.Messages().AddListener("GF_UIButtonClick",a=>messages++);p.CloseAction=()=>throw new InvalidOperationException("close");Throws<InvalidOperationException>(()=>p.CloseButton.onClick.Invoke());Require(f.AudioCalls==1&&messages==0&&!p.Lifetime.IsDisposed,"audio precedes failed close callback and suppresses global click");
                    f.Audio=()=>throw new InvalidOperationException("audio");bool called=false;p.CloseAction=()=>called=true;Throws<InvalidOperationException>(()=>OutgameUiPointerClick.Get(p.Mask.gameObject).OnPointerClick(null));Require(!called,"mask audio failure stops close");
                }
            });
            check("dispose-restores-top-clears-shared-owners-and-reopen",()=>{
                using(var f=new Fixture()){
                    f.Start();var p=f.Page;var daily=f.Subviews.Daily.I;var achievement=f.Subviews.Achievement.I;var preview=p.Preview;p.Liveness.EffectHandle=17;p.CloseUINow();
                    Require(p.Lifetime.IsDisposed&&f.Subviews.Daily.Instance==null&&f.Subviews.Achievement.Instance==null&&f.ClosedEffects==0&&!preview.Lifetime.IsDisposed,"source disposal clears singleton owners but does not invent preview/effect cleanup");
                    Require(daily.Rows.Count==7&&ReferenceEquals(daily.RootUI,p)&&f.TopRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingLayerName=="UIPopup","retains old view fields and restores TopInfo layer");
                    f.Data.Achievement.RefreshSortList();f.Pages.Clear();f.Start();Require(!ReferenceEquals(daily,f.Subviews.Daily.I)&&!ReferenceEquals(achievement,f.Subviews.Achievement.I)&&f.Subviews.Daily.I.Rows.Count==7,"new page creates new subviews; old refresh listener removed");
                }
            });
            check("main-entry-original-button-open-show-and-audio-failure",()=>{
                using(var f=new Fixture()){
                    var binding=new OutgameTaskEntryBinding(f.Data.Main.transform,new OutgameTaskEntryServices{Control=()=>f.Data.Control,Pages=()=>f.OpenRegistry,FindPage=()=>f.Pages.TryGetValue(OutgameTaskPage.SourceName,out var page)?page:null,Messages=f.Ui.Messages,PlayVoice=(kind,id)=>{Require(kind==1&&id==2001,"source entry voice");f.Audio?.Invoke();f.AudioCalls++;}});binding.Initialize();
                    f.Audio=()=>throw new InvalidOperationException("voice");Throws<InvalidOperationException>(()=>binding.Button.onClick.Invoke());Require(f.Pages.Count==0,"audio failure precedes UI lookup and opening");f.Audio=null;binding.Button.onClick.Invoke();Require(f.Pages.Count==1&&f.Loads.Count==1,"original main button opens actual page registry");f.CompleteLoad();f.Page.OpenLater();var page=f.Page;page.SetVisible(false);binding.Button.onClick.Invoke();Require(page.Visible&&page.Transform.localScale==Vector3.one&&f.Loads.Count==0,"existing hidden page is shown without reloading or reinitializing");binding.Button.onClick.Invoke();Require(f.Page==page&&f.Loads.Count==0,"already visible page reused");
                }
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-page-validation.json"),JsonUtility.ToJson(r,true));return r;
        }
    }
}
