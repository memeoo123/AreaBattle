using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.SharedItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityControlValidation
    {
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public class Page:OutgameUiPage{public Page():base((GameObject)null){}}
        sealed class OtherPage:Page{}
        public sealed class Ui:IOutgameActivityUiHost
        {
            public Action OpenAction;public readonly List<string> Opened=new List<string>();public OutgameUiPage Result=new Page();
            public OutgameUiPage Open(string ns,string name,object[] args){Opened.Add(name);OpenAction?.Invoke();return Result;}
        }
        public sealed class Reports:IOutgameActivityReports
        {
            public readonly List<string> Trace=new List<string>();public OutgameActivityReport Last;
            public Action<OutgameActivityReportKind> Creating,Sending;
            public OutgameActivityReport Create(OutgameActivityReportKind kind,bool initialize,object argument)
            {Require(initialize&&argument==null,"source report initialization arguments");Trace.Add("create:"+kind);Creating?.Invoke(kind);return Last=new OutgameActivityReport();}
            public void Send(OutgameActivityReportKind kind,OutgameActivityReport report)
            {Require(ReferenceEquals(Last,report),"same mutable report passed through event boundary");Trace.Add("send:"+kind);Sending?.Invoke(kind);}
        }
        public class Probe:OutgameActivityBase
        {
            public Action OnRefresh,OnRelease,OnTick,OnLaunch,OnWarm,OnOver;public int Refreshes;
            public Probe(OutgameActivityServices services):base(services){}
            public override void Refresh(int id){ActivityId=id;Refreshes++;OnRefresh?.Invoke();}
            public override void Dispose()=>OnRelease?.Invoke();public override void Update()=>OnTick?.Invoke();
            public override void Launch()=>OnLaunch?.Invoke();public override void Warm()=>OnWarm?.Invoke();public override void Over()=>OnOver?.Invoke();
        }
        [OutgameActivityControlRegister(7)]sealed class Parent:Probe{public Parent(OutgameActivityServices s):base(s){}}
        [OutgameActivityControlRegister(8)]sealed class Child:Probe{public Child(OutgameActivityServices s):base(s){}}
        [OutgameActivityControlRegister(99)]sealed class Missing:Probe{public Missing(OutgameActivityServices s):base(s){}}
        sealed class Entity:IOutgameItemEntity{public void AddItem(long count){}public void AddItemOnlyModel(long count){}}
        sealed class Factory:OutgameActivityFactory
        {readonly Func<IOutgameItemEntity> produce;public Factory(OutgameItemConfigManager config,Func<IOutgameItemEntity> produce):base(1,()=>config,s=>{}){this.produce=produce;}public override IOutgameItemEntity Produce()=>produce();}
        sealed class ItemProbe:OutgameActivityBase
        {public Func<int,OutgameActivityFactory> Make;public ItemProbe(OutgameActivityServices s):base(s){}public override OutgameActivityFactory ActivityFactoryBase(int id)=>Make(id);}
        public sealed class Fixture:IDisposable
        {
            readonly Func<int,OutgameActivityConfigRow> previous;
            public readonly OutgameStatisticsOffNetValidation.Fixture Statistics;
            public readonly OutgameActivityControl Control=new OutgameActivityControl();
            public readonly OutgameActivityConfigRuntime Config;
            public readonly OutgameActivityRuntime Runtime;
            public readonly OutgameActivityControlServices Services;
            public readonly OutgameActivityServices Activity;
            public readonly OutgameFrameEntry Frame=new OutgameFrameEntry(new OutgameFrameServices());
            public readonly OutgameFsmManager Fsm=new OutgameFsmManager();
            public readonly OutgameGlobalItemRewards Items;
            public readonly Reports ReportHost=new Reports();public readonly Ui UiHost=new Ui();
            public readonly List<object[]> Errors=new List<object[]>();
            public readonly List<string> Trace=new List<string>();
            public string Online;public float Delta;public int DailyAdds,DailyRemoves;public Action<long> Daily;
            public readonly GameObject Root;public readonly Button Button;public readonly Text Text;
            public OutgameActivityConfigRow Row=>Config.Manager.GetActivityConfig(7);
            public OutgameActivityItemData Data=>Control.GetItemData(7);
            public OutgameActivityBase Activity7=>Control.GetActivity<OutgameActivityBase>(7);
            public Fixture(string path=null,bool native=false,bool originalConfig=false)
            {
                previous=OutgameActivityItemData.ConfigurationResolver;
                Statistics=new OutgameStatisticsOffNetValidation.Fixture(path,native);Statistics.Init();
                var json=new OutgameActivityConfigUnityJson(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>Errors.Add(new object[]{s}));
                Online=originalConfig?null:"{\"Datas\":["+JsonUtility.ToJson(NewRow(7))+"]}";
                var reader=new OutgameActivityConfigReaderServices{OnlineParameter=k=>Online,UseBinary=()=>false,ReadLocalActivities=json.ReadActivities,ReadLocalSettings=json.ReadSettings,Error=Errors.Add};
                Config=new OutgameActivityConfigRuntime(reader,new OutgameActivityCustomConfigServices{UseBinary=()=>false,Json=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>Errors.Add(new object[]{s}))},Errors.Add);
                Items=new OutgameGlobalItemRewards(null,()=>null,()=>Statistics.Messages,()=>null,s=>{});
                Activity=new OutgameActivityServices{FsmManager=()=>Fsm,Ui=()=>UiHost,Format=(s,a)=>string.Format(s,a)};
                Services=new OutgameActivityControlServices{Current=()=>Control,Pool=()=>Statistics.Pool,Items=()=>Items,
                    AssemblyTypes=()=>Array.Empty<Type>(),CreateActivity=t=>t==typeof(Parent)?new Parent(Activity):t==typeof(Child)?new Child(Activity):throw new Exception("unexpected activity factory"),
                    Messages=()=>Statistics.Messages,Common=()=>Statistics.Common,Statistics=Statistics.Expansion,Reports=()=>ReportHost,Localize=n=>n.key,Error=Errors.Add};
                var managers=new OutgameActivityManagerServices{AssemblyTypes=()=>Array.Empty<Type>(),CreateManager=t=>throw new Exception("unexpected manager"),SourceTypeIndex=t=>throw new Exception("unexpected manager type"),SourceAutoSyn=t=>false,Error=Errors.Add,
                    OffNet=new OutgameActivityOffNetServices{GetNowTimeInt=()=>1700000000,Warning=s=>{},Error=Errors.Add}};
                Runtime=new OutgameActivityRuntime(Config,Services,Activity,managers,Frame,Statistics.StorageHost,Statistics.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}),s=>throw new Exception("unexpected download"));
                // Deterministic integration uses the real UpdateManager with a controlled clock
                // and captures scheduler registration; native validation also runs its Unity Update.
                Services.AddUpdate=Statistics.Updates.Register;Services.RemoveUpdate=Statistics.Updates.QueueRemove;
                Services.UnscaledDeltaTime=()=>native?Time.unscaledDeltaTime:Delta;
                Services.AddRefreshHandle=(hour,last,refresh,countdown,seconds)=>{Require(hour==0&&countdown==null&&seconds==86400,"source daily add");DailyAdds++;Daily=refresh;Trace.Add("daily");};
                Services.RemoveRefreshHandle=(hour,refresh,countdown,args)=>{Require(hour==0&&countdown==null&&ReferenceEquals(args,Array.Empty<object>()),"source daily remove");DailyRemoves++;if(Daily==refresh)Daily=null;};
                Root=new GameObject("activity-control-widgets",typeof(RectTransform));Button=new GameObject("button",typeof(RectTransform),typeof(Button)).GetComponent<Button>();Button.transform.SetParent(Root.transform,false);
                Text=new GameObject("text",typeof(RectTransform),typeof(Text)).GetComponent<Text>();Text.transform.SetParent(Root.transform,false);
                if(!originalConfig){Control.WidgetBindings[7]=new List<(Button,Text)>{(Button,Text)};Control.BindUi(7,typeof(Page),typeof(OtherPage));}
                Control.Services=Services;
            }
            public void Init()=>Runtime.Init();
            public string Stored=>Statistics.Strings.GetString(Statistics.StorageHost.MineGameName+Control.Manager.DataKey,"");
            public void Set(int id,long value)=>Statistics.Expansion.SetEventCount(id,value);
            public void Dispose()
            {
                try{Control.Cleanup();}finally{OutgameActivityItemData.ConfigurationResolver=previous;Statistics.Dispose();UnityEngine.Object.DestroyImmediate(Root);}
            }
        }
        public static OutgameActivityConfigRow NewRow(int id)=>new OutgameActivityConfigRow{id=id,open=1,uniqueId="one",parentActivityID=Array.Empty<int>(),activityName=new OutgameActivityConfigRow.Name{key="activity-"+id},
            noticeType=new[]{701},noticeParams=new[]{"1"},launchType=new[]{702},launchParams=new[]{"1"},overType=new[]{703},overParams=new[]{"1"},closeType=new[]{704},closeParams=new[]{"1"},noticeDes=new OutgameActivityConfigRow.Name{key="Need {0}"}};
        static OutgameActivityPopEvent Pop(int id,int priority=0,Type type=null,int automatic=0)=>new OutgameActivityPopEvent{Config=new OutgameActivityConfigRow{id=id,popPriority=priority,noticeAutoPop=automatic},UiType=type??typeof(Page),Args=Array.Empty<object>()};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="ActivityControl/config/offline data/FSM/update/item hooks and source popup/report/statistics ownership. Real JSON resources and isolated compressed file restart. UI/report concrete delivery endpoints are explicit fixtures. Concrete activity gameplay/Main/platform/Player remain pending."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-control-real-original-config-and-offline-owner-graph",()=>{
                using(var f=new Fixture(originalConfig:true))
                {
                    f.Init();Require(f.Config.Manager.Completed==5&&f.Control.Data.datas.Count==9&&f.Control.ItemDatas.Count==9,"full original config roster publishes offline records");
                    Require(f.Control.ActivityList.Count==f.Control.Activities.Count&&f.Control.Activities.Count>0&&f.DailyAdds==1,"root activity roster and daily registered after data");
                    foreach(var activity in f.Control.ActivityList)Require(activity.Data!=null&&activity.Config!=null&&activity.Fsm!=null,"source generic root FSMs initialized");
                    Require(ReferenceEquals(f.Statistics.Pool.Managers[4658],f.Control.Manager)&&f.Control.Manager.ParticipatesInSync&&f.Items.GetCommonItem!=null,"actual pool registration and common item hook");
                }
            });
            check("activity-control-real-state-cycle-report-pop-and-file-restart",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleControl-"+Guid.NewGuid().ToString("N"));long launch;
                using(var f=new Fixture(path))
                {
                    f.Init();f.Set(701,1);Require(f.Data.state==2&&f.Data.WarmTimeStamp==f.Statistics.ServerTime&&f.Control.NoticeQueue.Count==1&&f.ReportHost.Trace[0]=="create:Warmup","notice state uses actual control bookkeeping and report endpoint");
                    f.Button.onClick.Invoke();Require(f.UiHost.Opened.Count==1,"actual bound UGUI event calls source UI host");
                    f.Row.noticeAutoPop=1;f.UiHost.OpenAction=()=>f.Statistics.Messages.SendMessage("OpenUI",new object[]{f.UiHost.Result});f.Control.Update();
                    Require(f.Data.noticePop&&f.Control.NoticeQueue.Count==0&&ReferenceEquals(f.Control.NoticeUi,f.UiHost.Result),"automatic popup marked before open and dequeued after return");
                    f.Set(702,1);launch=f.Data.LaunchTimeStamp;Require(f.Data.state==3&&launch==f.Statistics.ServerTime&&f.Data.WarmTimeStamp==0&&f.Control.LaunchQueue.Count==1,"launch bookkeeping, queue and reset");
                    f.Control.Update();Require(!f.Control.IsDirty&&f.Stored.Length>0&&!f.Stored.StartsWith("{"),"actual compressed save clears dirty after IO");
                }
                using(var f=new Fixture(path))
                {
                    f.Init();Require(f.Data.state==3&&f.Data.LaunchTimeStamp==launch&&f.Data.noticePop,"new owner loads saved activity flags/timestamp");
                    f.Set(703,1);Require(f.Data.state==4&&f.Data.LaunchTimeStamp==0&&f.Data.WarmTimeStamp==0&&f.ReportHost.Trace.Contains("send:Complete"),"over bookkeeping through original statistics transitions");
                    f.Set(704,1);Require(f.Data.state==1&&!f.Data.noticePop&&!f.Data.launchPop,"over-close flags reset");
                }
            });
            check("activity-control-reflection-parent-child-and-pending-override",()=>{
                using(var f=new Fixture())
                {
                    var parent=NewRow(7);var child=NewRow(8);child.parentActivityID=new[]{7,7,10};var unregistered=NewRow(9);unregistered.parentActivityID=new[]{7};
                    f.Config.Manager.Activities[7]=parent;f.Config.Manager.Activities[8]=child;f.Config.Manager.Activities[9]=unregistered;f.Config.Manager.Activities[10]=NewRow(10);
                    f.Services.AssemblyTypes=()=>new[]{typeof(string),typeof(OutgameActivityBase),typeof(Parent),typeof(Child),typeof(Missing)};
                    var replacement=new Probe(f.Activity);f.Control.PendingActivities[10]=replacement;f.Control.RegisterActivities();
                    var p=f.Control.GetActivity<Parent>(7);var c=f.Control.GetActivity<Child>(8);
                    Require(p.Children.Count==2&&ReferenceEquals(p.Children[0],c)&&ReferenceEquals(p.ChildrenById[8],c)&&p.ActivityId==7&&c.ActivityId==8,"shared child attached to all configured parents, duplicate list but overwritten child map");
                    Require(f.Control.Activities.Count==2&&f.Control.ChildActivities.Count==1&&f.Control.GetActivity<OutgameActivityBase>(9)==null,"no generic fallback for unregistered child configs");
                    Require(ReferenceEquals(f.Control.Activities[10],replacement)&&replacement.Children.Count==0&&f.Control.PendingActivities.Count==0&&f.Errors.Count==2,"pending root overrides after child attachment; missing registered id diagnosed in both passes");
                    f.Control.Activities[8]=null;Require(ReferenceEquals(f.Control.GetActivity<Child>(8),c),"found-null root falls through child map");Throws<InvalidCastException>(()=>f.Control.GetActivity<Parent>(8));
                    f.Control.GetActivity<Probe>(900);Require(f.Errors.Count==2,"int missing is silent");f.Control.GetActivityBySourceId<Probe>(900);Require(f.Errors.Count==3,"source enum overload logs missing");
                }
            });
            check("activity-control-data-ready-last-row-wins-and-exact-base-child-refresh",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Activity7.OnDispose();var p=new Probe(f.Activity);var child=new Probe(f.Activity){ActivityId=8};p.Children.Add(child);p.OnRefresh=()=>child.Refresh(8);
                    f.Control.Activities[7]=p;var last=new OutgameActivityItemData{id=7};f.Control.Data.datas.Add(last);p.OnRefresh=()=>{Require(ReferenceEquals(f.Control.GetItemData(7),last),"all row indexing finishes before any refresh");child.Refresh(8);};
                    f.Control.UiBindings[7]=(typeof(Page),typeof(OtherPage));p.OnRefresh+=()=>{p.Data=last;p.Config=f.Row;};p.ButtonClickAction=()=>{};
                    f.Control.DataReady();Require(p.Refreshes==2&&child.Refreshes==2&&p.Buttons.Count==1&&p.Descriptions.Count==1&&p.NoticePopUi==typeof(Page)&&f.DailyAdds==1,"duplicate records refresh twice; derived roots do not enter exact-base extra child loop; widgets dedup");
                    f.Control.Data.datas.Add(null);Throws<NullReferenceException>(f.Control.DataReady);Require(f.DailyAdds==1&&ReferenceEquals(f.Control.ItemDatas[7],last),"null row fails indexing with previous prefix, no duplicate daily registration");f.Control.Data.datas.RemoveAt(2);
                    child.Refreshes=0;var exactBase=new OutgameActivityBase(f.Activity);exactBase.Children.Add(child);f.Control.Activities[7]=exactBase;
                    f.Control.DataReady();Require(child.Refreshes==4,"exact ActivityBase roots refresh children once internally and once explicitly for each record");exactBase.OnDispose();
                }
            });
            check("activity-control-popup-priority-exact-type-and-adjacent-close-skip",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var low=Pop(7,int.MinValue);var high=Pop(7,int.MaxValue);f.Control.QueueNotice(low);f.Control.QueueNotice(high);Require(ReferenceEquals(f.Control.NoticeQueue[0],high),"descending signed priority without overflow");
                    f.Control.QueueLaunch(low);f.Control.QueueLaunch(high);Require(ReferenceEquals(f.Control.LaunchQueue[0],high),"launch uses same descending comparator");
                    f.Control.OpenUiEvent(new object[]{new OtherPage()});Require(!f.Data.noticePop&&!f.Data.launchPop,"UI type comparison exact, not assignable");
                    f.Control.OpenUiEvent(new object[]{new Page()});Require(f.Data.noticePop&&f.Data.launchPop&&f.Control.NoticeQueue.Count==2,"open marks both queues without removing");
                    f.Control.NoticeUi=new Page();f.Control.LaunchUi=new Page();f.Control.CloseUiEvent(new object[]{new Page()});
                    Require(f.Control.NoticeQueue.Count==1&&f.Control.LaunchQueue.Count==1&&ReferenceEquals(f.Control.NoticeQueue[0],low)&&f.Control.NoticeUi==null&&f.Control.LaunchUi==null,"source forward Remove skips adjacent matching rows");
                }
            });
            check("activity-control-popup-failure-prefix-current-owner-and-retained-ui",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Set(701,1);f.Row.noticeAutoPop=1;f.Control.Manager.Dirty=false;
                    var other=new OutgameActivityControl{Manager=f.Services.CreateManager()};f.Services.Current=()=>other;
                    f.UiHost.OpenAction=()=>throw new InvalidOperationException("UI open failed");Throws<InvalidOperationException>(f.Control.ProcessNoticeQueue);
                    Require(f.Data.noticePop&&other.IsDirty&&!f.Control.IsDirty&&f.Control.NoticeQueue.Count==1&&f.Control.NoticeUi==null,"receiver row mutated, singleton owner dirty, failure retains queue");
                    f.Services.Current=()=>f.Control;f.UiHost.OpenAction=null;f.Control.ProcessNoticeQueue();f.Control.CloseUiEvent(new object[]{f.UiHost.Result});
                    Require(ReferenceEquals(f.Control.NoticeUi,f.UiHost.Result),"already removed automatic entry cannot clear NoticeUi on CloseUI");
                    f.Control.QueueNotice(Pop(7,automatic:1));int calls=f.UiHost.Opened.Count;f.Control.ProcessNoticeQueue();Require(f.UiHost.Opened.Count==calls,"retained UI blocks subsequent automatic notice");
                }
            });
            check("activity-control-bind-ui-launch-and-null-widget-semantics",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Set(702,1);f.Data.launchPop=true;f.Control.LaunchQueue.Clear();f.Control.BindUi(7,typeof(Page),typeof(OtherPage));
                    Require(f.Control.LaunchQueue.Count==1,"bind launch UI on state3 queues regardless saved launchPop");
                    f.Control.BindWidget(f.Activity7,null,null);f.Control.BindWidget(f.Activity7,null,null);Require(f.Activity7.Buttons.Count==2&&f.Activity7.Descriptions.Count==2,"null widgets retained once by Contains");
                    f.Control.BindUi(99,typeof(Page),null);f.Control.BindUi(99,null,typeof(Page));Require(f.Control.UiBindings[99].notice==null&&f.Control.UiBindings[99].launch==typeof(Page),"deferred binding overwrites tuple");
                    f.Activity7.Buttons.Remove(null);f.Activity7.Descriptions.Remove(null);
                }
            });
            check("activity-control-statistics-live-row-after-clock-and-signed-minutes",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Activity7.OnDispose();var row=NewRow(8);row.launchType=new[]{10000};row.launchParams=new[]{"20261003120000"};f.Config.Manager.Activities[8]=row;
                    var old=new OutgameActivityItemData{id=7,state=2,WarmTimeStamp=1};var next=new OutgameActivityItemData{id=8,state=2,WarmTimeStamp=180001};
                    f.Control.Manager.Data=new OutgameActivityData{datas=new List<OutgameActivityItemData>{old}};int calls=0;long target=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0));
                    f.Statistics.Owner.ValueProviders[10000]=a=>{calls++;f.Control.Data.datas[0]=next;return calls==1?60000:target+119999;};
                    f.Control.RefreshStatistics();Require(calls==2&&f.Statistics.Expansion.EventCount(10600,8)==-2&&f.Statistics.Expansion.EventCount(10700,8)==-1,"clock callback replacement re-read for warm timestamp/id; negative minute truncation and no clamp");
                    next.state=3;calls=0;f.Control.RefreshStatistics();Require(calls==0,"launched state excluded from minute counters");
                }
            });
            check("activity-control-daily-live-row-overflow-and-no-dirty",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Activity7.OnDispose();f.Control.Manager.Dirty=false;var old=new OutgameActivityItemData{id=7,state=4,LaunchTimeStamp=1};var next=new OutgameActivityItemData{id=8,state=1,LaunchTimeStamp=172800001};
                    f.Control.Manager.Data=new OutgameActivityData{datas=new List<OutgameActivityItemData>{null,old}};
                    f.Statistics.Owner.ValueProviders[10000]=a=>{Require(f.Control.Data.lastRefreshTimeStamp==99,"refresh timestamp stored before clock");f.Control.Data.datas[1]=next;return 1;};
                    f.Control.DailyRefresh(99);Require(f.Statistics.Expansion.EventCount(20000,8)==-2&&!f.Control.IsDirty,"daily ignores state, uses reread launch and id, does not dirty activity manager");
                    f.Statistics.Owner.ValueProviders[10000]=a=>long.MaxValue;long expected=unchecked((int)(unchecked((long.MaxValue-next.LaunchTimeStamp)*10000L)/864000000000L));
                    f.Control.DailyRefresh(100);Require(f.Statistics.Expansion.EventCount(20000,8)==expected,"unchecked tick overflow and signed day cast");
                }
            });
            check("activity-control-update-single-minute-subtraction-and-save-failure",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var probe=new Probe(f.Activity);f.Control.ActivityList.Add(probe);int ticks=0;probe.OnTick=()=>ticks++;
                    f.Delta=121;f.Control.Update();Require(ticks==1&&f.Control.Elapsed==61&&!f.Control.IsDirty,"one refresh interval removed even for 121 second frame");
                    f.Delta=0;f.Control.Update();Require(ticks==2&&f.Control.Elapsed==1,"remaining interval consumed next update");
                    f.Data.launchPop=!f.Data.launchPop;f.Control.SetDirty(true);f.Statistics.Writing=()=>{Require(f.Control.IsDirty,"activity dirty survives until storage returns");throw new InvalidOperationException("write");};
                    Throws<InvalidOperationException>(f.Control.Update);Require(f.Control.IsDirty,"failed save keeps dirty");f.Statistics.Writing=null;
                    f.Statistics.Pool.SetSaveDisabled(true);f.Control.Update();Require(f.Control.IsDirty,"pool blocks save");f.Statistics.Pool.SetSaveDisabled(false);f.Control.Update();Require(!f.Control.IsDirty,"enabled save clears dirty after success");
                }
            });
            check("activity-control-report-order-captured-data-and-mutable-owner",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var p=new Probe(f.Activity){ActivityId=7,Data=f.Data,Config=f.Row};var held=p.Data;var replacement=new OutgameActivityItemData{id=8,WarmTimeStamp=3};
                    p.OnLaunch=()=>{f.Trace.Add("virtual");held.WarmTimeStamp=5;};f.Statistics.Owner.ValueProviders[10000]=a=>{f.Trace.Add("clock");p.Data=replacement;return 55;};
                    f.ReportHost.Creating=k=>{Require(held.LaunchTimeStamp==55&&held.WarmTimeStamp==5&&replacement.WarmTimeStamp==0,"launch captures old data for timestamp, rereads for warm reset");f.Trace.Add("create");};
                    f.Statistics.Common.AddListener("CommonModule_Activity_Lunch",a=>{Require((int)a[0]==7&&f.ReportHost.Last.ActivityName=="activity-7","source message typo and payload after report fields");f.Trace.Add("message");f.ReportHost.Last.ParentName="callback";});
                    f.ReportHost.Sending=k=>{f.Trace.Add("send");Require(f.ReportHost.Last.ParentName=="callback","report remains mutable across message callback");};
                    f.Control.Launch(p,999);Require(string.Join(",",f.Trace)=="daily,virtual,clock,create,message,send","virtual hook, clock, report initialization, event, report delivery order");
                    f.ReportHost.Creating=null;f.ReportHost.Sending=null;p.OnOver=()=>{p.Data=replacement;replacement.LaunchTimeStamp=9;replacement.WarmTimeStamp=8;};f.Control.Over(p,999);
                    Require(replacement.LaunchTimeStamp==0&&replacement.WarmTimeStamp==0&&f.ReportHost.Trace[f.ReportHost.Trace.Count-1]=="send:Complete","over clears both fields on captured data and sends complete report");
                }
            });
            check("activity-control-parent-report-cache-missing-trailing-separator",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var row=NewRow(8);row.parentActivityID=new[]{7,99};Require(f.Control.GetParentReportStr(row)=="activity-7_"&&f.Errors.Count==1,"separator based on original array position, missing final parent keeps underscore");
                    f.Row.activityName.key="changed";Require(f.Control.GetParentReportStr(row)=="activity-7_"&&f.Errors.Count==1,"nonempty parent cache retained");
                    row.parentName="";row.parentActivityID=new[]{99};f.Control.GetParentReportStr(row);f.Control.GetParentReportStr(row);Require(f.Errors.Count==3,"empty computed cache recomputes and logs missing again");
                }
            });
            check("activity-control-widget-area-sort-rebind-and-null-failure-prefix",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var otherObject=new GameObject("other",typeof(RectTransform),typeof(Button));otherObject.transform.SetParent(f.Root.transform,false);var button=otherObject.GetComponent<Button>();
                    var row=NewRow(8);row.btnPriority=int.MinValue;f.Row.btnPriority=int.MaxValue;f.Config.Manager.Activities[8]=row;
                    var other=new Probe(f.Activity){Config=row,Data=new OutgameActivityItemData{id=8}};other.Buttons.Add(button);int clicks=0,stale=0;other.ButtonClickAction=()=>clicks++;button.onClick.AddListener(()=>stale++);f.Control.ChildActivities[8]=other;
                    f.Control.ArrangeWidgets();button.onClick.Invoke();Require(button.transform.GetSiblingIndex()==0&&f.Button.transform.GetSiblingIndex()==1&&clicks==1&&stale==0,"roots and children share area grouping, signed priority, actual sibling order and listener replacement");
                    other.Buttons.Add(null);Throws<NullReferenceException>(f.Control.ArrangeWidgets);Require(button.transform.GetSiblingIndex()==0,"null widget fails after earlier rebind; no source null guard");other.Buttons.Remove(null);
                }
            });
            check("activity-control-cleanup-retained-fields-hook-and-failure-order",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var manager=f.Control.Manager;var list=f.Control.ActivityList;f.Control.QueueNotice(Pop(7));f.Control.PendingActivities[8]=new Probe(f.Activity);f.Control.RegisterCommonManagerDel=()=>{};f.Control.Elapsed=12;
                    f.Control.Cleanup();Require(f.Control.Activities.Count==0&&f.Control.ChildActivities.Count==0&&f.Control.ItemDatas.Count==0&&f.Control.RegisterCommonManagerDel==null,"cleared activity maps and delegate");
                    Require(f.Control.PendingActivities.Count==1&&f.Control.NoticeQueue.Count==1&&ReferenceEquals(f.Control.Manager,manager)&&ReferenceEquals(f.Control.ActivityList,list)&&f.Control.Elapsed==12&&f.Control.WidgetBindings.Count==1,"source cleanup retains pending/queue/manager/list/elapsed/widgets");
                    Require(f.Control.RegisterDaily&&f.Daily==null&&f.Frame.DisposableActions==null&&f.Items.GetCommonItem!=null,"daily flag restored, frame removed, global item hook intentionally retained");
                    f.Control.ActivityList=new List<OutgameActivityBase>{new Probe(f.Activity){OnRelease=()=>throw new InvalidOperationException("dispose")}};f.Control.ItemDatas[7]=new OutgameActivityItemData();int removals=f.DailyRemoves;
                    Throws<InvalidOperationException>(f.Control.Cleanup);Require(f.Control.ItemDatas.Count==1&&f.DailyRemoves==removals,"activity dispose failure aborts later clears/listener removal");f.Control.ActivityList.Clear();
                }
            });
            check("activity-control-item-hook-parent-first-immediate-children-and-failure",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Control.Activities.Clear();var config=new OutgameItemConfigManager(null,null,null);config.Items[1]=new GameItemConfig{id=1};var entity=new Entity();
                    var parent=new ItemProbe(f.Activity);var child=new ItemProbe(f.Activity);var later=new ItemProbe(f.Activity);var calls=new List<string>();
                    parent.Make=id=>new Factory(config,()=>{calls.Add("parent:"+id);return null;});child.Make=id=>new Factory(config,()=>{calls.Add("child:"+id);return entity;});later.Make=id=>throw new Exception("should not reach later root");
                    parent.Children.Add(child);f.Control.Activities[7]=parent;f.Control.Activities[8]=later;
                    Require(ReferenceEquals(f.Items.GetItem(new GameItemConfig{id=123}),entity)&&string.Join(",",calls)=="parent:123,child:123","actual GlobalItemRewards hook checks parent then immediate children and stops at first nonnull entity");
                    parent.Make=id=>null;Throws<NullReferenceException>(()=>f.Control.GetCommonItem(new GameItemConfig{id=123}));Require(calls.Count==2,"null factory throws rather than bypassing to child/fallback");
                }
            });
            check("activity-control-reinit-hook-dedup-and-registration-failure-prefix",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var manager=f.Control.Manager;f.Items.GetCommonItem+=row=>null;f.Control.Init(null);
                    Require(f.Items.GetCommonItem.GetInvocationList().Length==2&&f.Control.InitData!=null&&f.DailyAdds==2,"Init removes then appends own hook and replaces init data; scheduler re-registers");
                    Require(!ReferenceEquals(manager,f.Control.Manager)&&ReferenceEquals(f.Statistics.Pool.Managers[4658],manager),"source creates fresh control manager even if pool duplicate retains old manager");
                    f.Control.Cleanup();f.Config.Manager.Activities[7]=NewRow(7);f.Services.AssemblyTypes=()=>new[]{typeof(Parent),typeof(Parent)};
                    Throws<ArgumentException>(f.Control.RegisterActivities);Require(f.Control.Activities.Count==1&&f.Control.ChildActivities.Count==0,"duplicate root Add throws with prefix before child pass");
                }
            });
            return report;
        }
    }
}
