using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTimeTaskValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action run)where T:Exception{try{run();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public sealed class Reports:IOutgameLimitTaskReports
        {
            public readonly List<(OutgameLimitTaskReportKind kind,OutgameLimitTaskReport report)> Sent=new List<(OutgameLimitTaskReportKind,OutgameLimitTaskReport)>();
            public Action<OutgameLimitTaskReportKind,OutgameLimitTaskReport> Sending;
            public OutgameLimitTaskReport Create(OutgameLimitTaskReportKind kind,bool initialize,object arg)
            {Require(initialize&&arg==null,"source report initialization");return new OutgameLimitTaskReport();}
            public void Send(OutgameLimitTaskReportKind kind,OutgameLimitTaskReport report){Sending?.Invoke(kind,report);Sent.Add((kind,report));}
        }
        public sealed class Fixture:IDisposable
        {
            readonly OutgameLimitTaskModelServices previous=OutgameLimitTaskModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime;
            internal readonly OutgameGlobalItemRewardsValidation.Fixture Items=new OutgameGlobalItemRewardsValidation.Fixture();
            public readonly Reports ReportHost=new Reports();
            public readonly List<object[]> Errors=new List<object[]>();public readonly List<string> Trace=new List<string>();
            public readonly OutgameLimitTimeTaskServices Services;
            public readonly OutgameLimitTimeTaskRuntime Binding;
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public OutgameLimitTimeTaskActivity Parent=>Runtime.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);
            public OutgameNoviceTaskManager Manager=>Parent.Manager;
            public OutgameChildLimitTimeTaskActivity Child=>Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(11);
            public Fixture(string path=null,bool native=false)
            {
                Runtime=new OutgameActivityControlValidation.Fixture(path,native,true);
                Runtime.Services.Items=()=>Items.Engine;Runtime.Activity.ItemFactoryError=s=>Errors.Add(new object[]{s});
                Services=new OutgameLimitTimeTaskServices{Rewards=()=>Items.Engine,Entities=Items.Services,Reports=()=>ReportHost,Localize=n=>n.key,LocalizeBusiness=n=>n.key,
                    Log=a=>Trace.Add("listen:"+a[0]),LogByColor=(c,a)=>Trace.Add("color:"+a[0]),Error=Errors.Add};
                Binding=new OutgameLimitTimeTaskRuntime(Runtime.Runtime,Services,()=>{
                    var stats=Runtime.Statistics;
                    return new OutgameNoviceTaskManager(new OutgameDataManagerStorage(()=>"CommonGameModuleNoviceTaskManager",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{})),stats.StorageHost,
                        k=>throw new Exception("unexpected download"),new OutgameNoviceTaskManagerServices{Config=()=>Config,Warning=s=>{},Error=Errors.Add});
                },()=>Items.Config.Instance);
                Runtime.Init();
            }
            public void Synthetic(params PubnoviceTaskConfig[] tasks)
            {
                var manager=Manager;Parent.OnDispose();Runtime.Control.ChildActivities.Clear();
                Config.NoviceTasks.Clear();Config.NoviceTasksByActivity.Clear();Config.NoviceActivities.Clear();Config.NoviceAcc.Clear();Config.NoviceAccByActivity.Clear();
                Config.NoviceActivities[11]=new PubNTActivityConfig{id=11,accType=0,name=new Lang{key="business"}};
                Config.NoviceTasksByActivity[11]=tasks.ToDictionary(t=>t.id);foreach(var task in tasks)Config.NoviceTasks[task.id]=task;
                Config.NoviceAccByActivity[11]=new Dictionary<int,PubnoviceAccRewardConfig>();
                var row=OutgameActivityControlValidation.NewRow(11);row.parentActivityID=new[]{1301001};Config.Activities[11]=row;
                Runtime.Control.ItemDatas[11]=new OutgameActivityItemData{id=11,state=3};Runtime.Set(702,1);
                manager.Activity=null;manager.Data=new OutgameLimitTimeTaskData();var data=new OutgameLimitTimeTaskChildData();data.ext.activityId=11;data.ext.dayId=1;data.ext.lastClickDayId=1;manager.Data.datas.Add(data);manager.UpdateCallback();
                var parent=new OutgameLimitTimeTaskActivity(Runtime.Activity,Services){ActivityId=1301001,Data=Runtime.Control.GetItemData(1301001),Config=Config.GetActivityConfig(1301001)};
                Runtime.Control.Activities[1301001]=parent;parent.OnInit();Trace.Clear();ReportHost.Sent.Clear();Errors.Clear();parent.Dirty=false;
            }
            public void Acc(int id,int target=1)
            {
                var row=new PubnoviceAccRewardConfig{id=id,activityId=11,accValue=target,rewards=new List<ListArrayInt>{Row(1002,3)}};
                Config.NoviceAcc[id]=row;Config.NoviceAccByActivity[11][id]=row;
            }
            public void Event(int key,long amount,int? filter=null)
            {Runtime.Statistics.Common.SendMessageGetKey(OutgameStatisticsMessageKey.Get(key),filter.HasValue?new object[]{amount,filter.Value}:new object[]{amount});}
            public string Stored=>Runtime.Statistics.Strings.GetString(Runtime.Statistics.StorageHost.MineGameName+Manager.DataKey,"");
            public void Dispose(){try{Parent?.OnDispose();Runtime.Dispose();}finally{OutgameLimitTaskModels.Services=previous;}}
        }
        static ListArrayInt Row(params int[] values)=>new ListArrayInt{datas=values};
        static PubnoviceTaskConfig Task(int id,int day=1,int key=71,int target=2)=>new PubnoviceTaskConfig{id=id,activityId=11,day=day,
            conditionParams=new List<ListArrayInt>{Row(key,target)},showType=new List<ListArrayInt>(),expand=new List<ListArrayInt>(),rewards=new List<ListArrayInt>{Row(1001,5)}};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Concrete limited-task activities, original49 task configuration, real common dispatch/statistics/pool/storage/reward engine; explicit report/UI/account hosts. SevenDay/pages/Main and platform delivery pending. Native/Player acceptance separate."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id="limit-task-activity-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="limit-task-activity-"+id,result="fail",detail=e.ToString()});}};
            check("original49-runtime-registration-and-model-ownership",()=>{
                using(var f=new Fixture())
                {
                    Require(f.Config.NoviceTasks.Count==49&&ReferenceEquals(f.Manager.Activity,f.Parent),"real manager attached to concrete registered activity");
                    Require(ReferenceEquals(f.Runtime.Statistics.Pool.Managers[4711],f.Manager)&&f.Manager.ParticipatesInSync,"pool source4711 registration");
                    int count=0;foreach(var child in f.Parent.TaskViewActivities.Values)
                    {Require(ReferenceEquals(child.Parent,f.Parent)&&ReferenceEquals(f.Binding.Models.GetActivity(1301001).GetChild(child.ActivityId),child)&&child.Fsm!=null,"one actual owner graph and base FSM");foreach(var list in child.DayTasks.Values)count+=list.Count;}
                    Require(count==49&&f.Manager.Data.datas.All(d=>d.datas.Count==0),"all configured tasks initialized in runtime without populating saved records");
                    Require(f.Runtime.Statistics.Owner.ValueProviders.ContainsKey(10021),"completion provider registered");
                }
            });
            check("saved-object-identity-orphans-new-tasks-and-repeated-init",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2,2));var saved=new OutgameLimitTaskItemData{id=1,state=1};saved.conditions.Add(new OutgameLimitTaskCondition{key=71,value=7,arg=new int[0]});
                    var orphan=new OutgameLimitTaskItemData{id=999};f.Child.ModuleData.datas.Add(saved);f.Child.ModuleData.datas.Add(orphan);
                    var days=f.Child.DayTasks;f.Child.OnInit();var first=f.Child.FindTask(2);f.Child.OnInit();
                    Require(ReferenceEquals(days,f.Child.DayTasks)&&ReferenceEquals(saved,f.Child.FindTask(1))&&!ReferenceEquals(first,f.Child.FindTask(2)),"dictionary retained, persisted records reused, unsaved entries recreated");
                    Require(f.Child.ModuleData.datas.Count==2&&f.Errors.Count==2&&f.Child.FindTask(2).conditions[0].value==0,"orphan retained and diagnosed per initialization");
                    f.Event(71,1);Require(f.Child.FindTask(2).conditions[0].value==1,"repeated init removes current recorded listeners before readding once");
                }
            });
            check("save-filter-zero-conditions-and-duplicate-runtime-ids",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2),Task(3));var one=f.Child.FindTask(1);var two=f.Child.FindTask(2);var three=f.Child.FindTask(3);
                    one.conditions[0].value=1;two.state=1;two.conditions.Clear();three.state=2;f.Child.DayTasks[1].Add(one);
                    f.Child.SaveModule();Require(f.Child.ModuleData.datas.SequenceEqual(new[]{one,three,one}),"only nonzero progress/state inside condition loop; seen set not updated during append");
                    f.Child.SaveModule();Require(f.Child.ModuleData.datas.Count==3,"preexisting IDs suppress later appends");
                }
            });
            check("dispatcher-progress-clamp-filters-and-report-threshold",()=>{
                using(var f=new Fixture())
                {
                    var a=Task(1);var b=Task(2);b.conditionParams[0]=Row(71,9,2);f.Synthetic(a,b);
                    f.Event(71,1);Require(f.Child.FindTask(1).conditions[0].value==1&&f.Child.FindTask(2).conditions[0].value==0&&f.Parent.Dirty,"no-arg event only updates matching argument shape");
                    f.Event(71,3);f.Event(71,10);Require(f.Child.FindTask(1).conditions[0].value==14&&f.ReportHost.Sent.Count==2,"progress continues after threshold, reporting stops once old reaches target");
                    f.Event(71,-99);Require(f.Child.FindTask(1).conditions[0].value==0,"event delta clamps to zero");
                    f.Child.DayTasks[1].Remove(f.Child.FindTask(1));f.Event(71,1,8);f.Event(71,1,9);
                    Require(f.Child.FindTask(2).conditions[0].value==1,"filtered event matches first recorded argument");
                }
            });
            check("event-all-life-inactive-unbox-and-caught-prefix",()=>{
                using(var f=new Fixture())
                {
                    var a=Task(1);var b=Task(2);b.allLifeStatistics=1;f.Synthetic(a,b);f.Child.Data.state=1;f.Runtime.Statistics.Owner.ValueProviders[71]=args=>8;
                    f.Event(71,1);Require(f.Child.FindTask(1).conditions[0].value==0&&f.Child.FindTask(2).conditions[0].value==8,"inactive local skipped, lifetime reads absolute statistic");
                    f.Child.Data.state=3;f.Child.StatisticsEvent(new object[]{1,71});Require(f.Errors.Count==1&&f.Child.FindTask(1).conditions[0].value==0,"Int32 amount cannot unbox as Int64 and is caught");
                    f.Child.DayTasks[1].Remove(f.Child.FindTask(2));f.ReportHost.Sending=(kind,r)=>throw new InvalidOperationException("report-failure");f.Parent.Dirty=false;f.Event(71,1);
                    Require(f.Errors.Count==2&&f.Child.FindTask(1).conditions[0].value==1&&!f.Parent.Dirty,"record changed before failed report; dirty not reached");
                }
            });
            check("listener-removal-recorded-versus-configured-keys",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1));var task=f.Child.FindTask(1);task.conditions[0].key=72;f.Child.RemoveListeners();task.conditions[0].key=71;
                    f.Event(71,1);Require(task.conditions[0].value==1,"source removal uses saved key72, leaving configured listener71");
                    f.Child.AddListeners();f.Event(71,1);Require(task.conditions[0].value==3,"readding after key-list clear appends another callback");
                }
            });
            check("progress-no-match-dirty-and-fourth-slot-report",()=>{
                using(var f=new Fixture())
                {
                    var cfg=Task(1);cfg.conditionParams=new List<ListArrayInt>{Row(70,10),Row(71,10),Row(72,10),Row(73,10)};f.Synthetic(cfg);
                    f.Child.ChangeTaskProgress(1,4,99,new int[0]);Require(f.Parent.Dirty&&f.ReportHost.Sent.Count==0,"missing condition still dirties parent");
                    f.Child.ChangeTaskProgress(1,4,73,new int[0]);var reportRow=f.ReportHost.Sent.Single().report;
                    Require(reportRow.Values[0]==4&&reportRow.OldValues[0]==0&&reportRow.Values[1]==null&&reportRow.DetailId=="1","condition index3 falls back to first report slot");
                }
            });
            check("day-launch-utc-boundaries-upper-only-clamp",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2,7));long now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,23,59,0));f.Runtime.Statistics.Owner.ValueProviders[10000]=a=>now;
                    f.Child.Launch();Require(f.Child.ModuleData.ext.dayId==1&&f.Child.ModuleData.ext.launchTime==now&&f.Runtime.Statistics.Expansion.EventCount(29000,11)==now,"launch writes day/time and keyed launch statistic");
                    now+=120000;f.Child.RefreshDay();Require(f.Child.ModuleData.ext.dayId==2,"calendar day boundary rather than24h duration");
                    now+=20*86400000L;f.Child.RefreshDay();Require(f.Child.ModuleData.ext.dayId==7,"maximum configured day cap");
                    now-=24*86400000L;f.Child.RefreshDay();Require(f.Child.ModuleData.ext.dayId==-2,"source retains negative day after clock rollback");
                }
            });
            check("day-complete-hidden-and-nonzero-state-navigation",()=>{
                using(var f=new Fixture())
                {
                    var cfg=Task(1);cfg.showType.Add(Row(88,1));f.Synthetic(cfg,Task(2,2));f.Runtime.Statistics.Owner.ValueProviders[88]=a=>0;
                    Require(f.Child.DayComplete(1)&&!f.Child.DayComplete(2),"hidden pending task skipped; visible pending task blocks");
                    f.Runtime.Statistics.Owner.ValueProviders[88]=a=>1;f.Child.FindTask(1).state=2;Require(f.Child.DayComplete(1),"any nonzero visible state is complete");
                    f.Child.FindTask(2).conditions[0].value=2;f.Child.ModuleData.ext.dayId=2;Require(f.Child.ExitRewardWaitGet(2)&&f.Child.ExitToGetRewardDay()==2,"first unlocked claimable day");
                    f.Child.FindTask(2).state=1;f.Child.ModuleData.ext.lastClickDayId=8;Require(f.Child.ExitToGetRewardDay()==8,"no available reward returns recorded page");
                }
            });
            check("real-reward-cost-claim-reports-and-repeat-guard",()=>{
                using(var f=new Fixture())
                {
                    var cfg=Task(32);cfg.expand.Add(Row(1002,2));f.Synthetic(cfg);var task=f.Child.FindTask(32);task.conditions[0].value=2;
                    f.Child.TaskComplete(32);
                    Require(f.Items.Global.GetItemCount(1001)==5&&f.Items.Global.GetItemCount(1002)==-2&&task.state==1,"actual rewards and costs before claimed state, no invented insufficient-funds guard");
                    Require(f.Child.ModuleData.ext.dayFlag==(long)int.MinValue&&f.Runtime.Statistics.Expansion.EventCount(10021,1101)==1,"source signed32-bit bitmask extended to Int64 and completion statistic");
                    Require(f.ReportHost.Sent.Select(x=>x.report.DetailType).SequenceEqual(new[]{"任务","每日任务","全部任务","累计任务数量"})&&f.Parent.Dirty,"reward/day/all/cumulative report order");
                    Require(f.ReportHost.Sent[0].report.ActivityName=="business"&&f.ReportHost.Sent[3].report.ActivityName=="activity-11","business and common localization remain distinct");
                    f.Child.TaskComplete(32);Require(f.Items.Host.Adds==1&&f.ReportHost.Sent.Count==4,"repeat claim is ineligible");
                }
            });
            check("claim-cost-and-report-failure-prefixes",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1));var task=f.Child.FindTask(1);task.conditions[0].value=2;f.Items.Engine.ExpendRewardsDel=rows=>throw new InvalidOperationException("cost");
                    Throws<InvalidOperationException>(()=>f.Child.TaskComplete(1));Require(f.Items.Global.GetItemCount(1001)==5&&task.state==0&&f.Child.ModuleData.ext.dayFlag==0,"award survives failed cost before claim flags");
                    f.Items.Engine.ExpendRewardsDel=null;f.ReportHost.Sending=(kind,r)=>throw new InvalidOperationException("report");
                    Throws<InvalidOperationException>(()=>f.Child.TaskComplete(1));Require(f.Items.Global.GetItemCount(1001)==10&&task.state==1&&f.Child.ModuleData.ext.dayFlag==1&&!f.Parent.Dirty,"report failure preserves claimed prefix before final dirty");
                }
            });
            check("self-count-condition-suppresses-completion-statistic",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1,key:10021));f.Child.FindTask(1).conditions[0].value=2;f.Child.TaskComplete(1);
                    Require(f.Runtime.Statistics.Expansion.EventCount(10021,1101)==0,"single10021 task does not increment itself");
                }
            });
            check("acc-position-mask-reset-and-sorted-view",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1));for(int i=1;i<=33;i++)f.Acc(100+i,34-i);f.Child.OnInit();f.Child.FindTask(1).state=1;
                    var reward=f.Child.FindAccReward(133);Require(ReferenceEquals(reward,f.Child.AccRewards[0]),"sorted view shares objects with config insertion-order list");f.Child.GetAccReward(133);
                    Require(reward.state==1&&f.Child.ModuleData.ext.accFlag==1&&f.Items.Global.GetItemCount(1002)==3,"claim33 aliases bit0 via int32 shift");
                    f.Child.ModuleData.ext.launchTime=77;f.Child.ModuleData.ext.lastClickDayId=4;f.Child.ResetProgress();
                    Require(f.Child.FindAccReward(101).state==1&&f.Child.FindAccReward(133).state==0&&f.Child.ModuleData.ext.accFlag==1,"reset retains flag then decodes64 positions, exposing original32/64 asymmetry");
                    Require(f.Child.ModuleData.ext.launchTime==77&&f.Child.ModuleData.ext.lastClickDayId==4&&f.Child.FindTask(1).state==0&&f.Child.ModuleData.datas.Count==0,"reset clears tasks but retains launch/click fields");
                }
            });
            check("liveness-factory-event-before-claim-and-progress-derived",()=>{
                using(var f=new Fixture())
                {
                    var cfg=Task(1);cfg.rewards.Clear();cfg.rewards.Add(Row(900,7));f.Items.Config.Instance.Items[900]=new SharedItemConfig.GameItemConfig{id=900,type1=2,type2=4,paramInt=11,item_game="g"};
                    f.Synthetic(cfg);f.Config.NoviceActivities[11].accType=1;var task=f.Child.FindTask(1);task.conditions[0].value=2;
                    f.Child.TaskComplete(1);var first=f.ReportHost.Sent[0];
                    Require(first.kind==OutgameLimitTaskReportKind.Success&&first.report.DetailType=="活跃度"&&first.report.OldValues[0]==0&&first.report.Values[0]==7,"actual common item factory emits before task claimed");
                    Require(f.Child.ModuleData.ext.AccProgress==7&&f.Items.Global.GetItemCount(900)==7,"derived progress and inventory both from actual path");
                    Throws<InvalidCastException>(()=>f.Child.LivenessEvent(new object[]{7L}));
                }
            });
            check("parent-provider-original-owner-id-and-save-gate",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1));Throws<NullReferenceException>(()=>f.Parent.LimitTaskActDayComplete(new object[]{1101}));
                    f.Parent.ChildrenById[1301001]=f.Child;f.Child.FindTask(1).state=1;Require(f.Parent.LimitTaskActDayComplete(new object[]{9901})==1,"source uses owner ActivityId and argument modulo100 only");
                    Throws<InvalidCastException>(()=>f.Parent.LimitTaskActDayComplete(new object[]{1101L}));
                    f.Parent.Dirty=true;f.Runtime.Statistics.Pool.SourceReadyFlag=false;f.Parent.Update();Require(f.Parent.Dirty&&f.Stored=="","disabled save preserves dirty");
                    f.Runtime.Statistics.Pool.SourceReadyFlag=true;f.Parent.Update();Require(!f.Parent.Dirty&&f.Stored.Length>0,"enabled parent update performs manager save and clears dirty");
                }
            });
            check("acc-sign-extension-and-report-failure-after-award",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1));for(int i=1;i<=40;i++)f.Acc(100+i,0);f.Child.OnInit();f.Parent.Dirty=false;
                    f.ReportHost.Sending=(kind,r)=>throw new InvalidOperationException("report");
                    Throws<InvalidOperationException>(()=>f.Child.GetAccReward(132));
                    Require(f.Child.FindAccReward(132).state==1&&f.Child.ModuleData.ext.accFlag==(long)int.MinValue&&f.Items.Global.GetItemCount(1002)==3&&!f.Parent.Dirty,"reward/state/sign-extended flag precede report failure and dirty");
                    f.ReportHost.Sending=null;f.Child.OnInit();Require(f.Child.FindAccReward(131).state==0&&f.Child.FindAccReward(132).state==1&&f.Child.FindAccReward(140).state==1,"negative flag decodes upper64-bit positions as claimed");
                }
            });
            check("empty-task-conditions-duplicate-clock-listeners",()=>{
                using(var f=new Fixture())
                {
                    var cfg=Task(1);cfg.conditionParams.Clear();f.Synthetic(cfg);int calls=0;f.Runtime.Statistics.Owner.ValueProviders[29000]=a=>{calls++;return 0;};
                    f.Child.AddListeners();f.Event(10000,1);Require(calls==2,"empty key list allows repeated entire listener registration");
                    f.Child.RemoveListeners();calls=0;f.Event(10000,1);Require(calls==1,"one removal retains one duplicate clock delegate");
                }
            });
            check("init-notifications-current-day-reset-notifications-group-day",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2,3));var days=new List<int>();f.Runtime.Statistics.Common.AddListener("CommonModule_RefreshNoviceTaskList",a=>days.Add((int)a[1]));
                    f.Child.ModuleData.ext.dayId=2;f.Child.OnInit();Require(days.SequenceEqual(new[]{2,2}),"initialization sends selected day once per group without task sorting");
                    days.Clear();f.Child.ResetProgress();Require(days.SequenceEqual(new[]{1,3}),"reset sends each sorted group key");
                }
            });
            check("day-refresh-notification-failure-retains-day-before-dirty",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2,7));long start=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3));
                    f.Runtime.Statistics.Owner.ValueProviders[29000]=a=>start;f.Runtime.Statistics.Owner.ValueProviders[10000]=a=>start+86400000;
                    f.Runtime.Statistics.Common.AddListener("CommonModule_NoviceExtRefresh",a=>throw new InvalidOperationException("ui"));
                    Throws<InvalidOperationException>(f.Child.RefreshDay);Require(f.Child.ModuleData.ext.dayId==2&&!f.Parent.Dirty,"new day published before notification failure, final dirty skipped");
                }
            });
            check("missing-day-and-empty-navigation-failure-boundaries",()=>{
                using(var f=new Fixture())
                {
                    f.Synthetic(Task(1),Task(2,3));f.Child.FindTask(1).state=1;f.Child.ModuleData.ext.dayId=3;
                    Throws<NullReferenceException>(()=>f.Child.ExitToGetRewardDay());Require(f.Errors.Count==1,"source integer-day scan encounters missing day2 rather than skipping gap");
                    f.Child.DayTasks.Clear();Throws<InvalidOperationException>(()=>f.Child.ExitToGetRewardDay());
                }
            });
            check("actual-file-restart-progress-and-claim-state",()=>{
                string path;int activity,id;using(var f=new Fixture())
                {
                    path=f.Runtime.Statistics.PathName;var child=f.Parent.TaskViewActivities.Values.First();activity=child.ActivityId;var task=child.DayTasks.Values.First()[0];id=task.id;
                    task.conditions[0].value=17;task.state=1;child.ModuleData.ext.accFlag=5;child.ModuleData.ext.lastClickDayId=6;f.Parent.Dirty=true;f.Parent.Update();Require(f.Stored.Length>0,"real compressed account storage");
                }
                using(var f=new Fixture(path))
                {
                    var child=f.Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(activity);var task=child.FindTask(id);
                    Require(task.conditions[0].value==17&&task.state==1&&child.ModuleData.ext.accFlag==5&&child.ModuleData.ext.lastClickDayId==6,"fresh runtime/pool/manager reconstructs saved actual activity state");
                }
            });
            return report;
        }
    }
}
