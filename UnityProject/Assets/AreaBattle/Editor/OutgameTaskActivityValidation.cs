using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
using GameItemConfig=AreaBattle.SharedItemConfig.GameItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskActivityValidation
    {
        public sealed class Fixture:IDisposable
        {
            readonly OutgameTaskModelServices previous=OutgameTaskModels.Services;
            readonly OutgameLimitTaskModelServices previousLimited=OutgameLimitTaskModels.Services;
            readonly OutgameAchievementModelServices previousAchievement=OutgameAchievementModels.Services;
            readonly bool withAchievement;
            public readonly OutgameActivityControlValidation.Fixture Runtime;
            internal readonly OutgameGlobalItemRewardsValidation.Fixture Items=new OutgameGlobalItemRewardsValidation.Fixture();
            public readonly OutgameLimitTimeTaskValidation.Reports Reports=new OutgameLimitTimeTaskValidation.Reports();
            public readonly List<object[]> Errors=new List<object[]>(),Warnings=new List<object[]>();
            public readonly List<string> Trace=new List<string>();public readonly OutgameTaskServices Services;
            public readonly OutgameTaskRuntime Binding;
            public readonly OutgameAchievementRuntime AchievementBinding;
            public long Now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0));
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public OutgameTaskActivity Parent=>Runtime.Control.GetActivity<OutgameTaskActivity>(1300001);
            public OutgameTaskManager Manager=>Parent.Manager;
            public OutgameChildTaskActivity Child=>Parent.GetChildActivity<OutgameChildTaskActivity>(130001);
            public OutgameChildTaskOffStrategy Strategy=>(OutgameChildTaskOffStrategy)Child.Strategy;
            public Fixture(string path=null,bool native=false,bool limited=false,bool achievement=false,Action<Fixture> configureBeforeInit=null)
            {
                Runtime=new OutgameActivityControlValidation.Fixture(path,native,true);
                Items.Config.Instance.Items[1003]=new GameItemConfig{id=1003,type1=2,type2=3,paramInt=130001,item_game="g"};
                Runtime.Services.Items=()=>Items.Engine;Runtime.Activity.ItemFactoryError=s=>Errors.Add(new object[]{s});
                Services=new OutgameTaskServices{Rewards=()=>Items.Engine,Entities=Items.Services,Reports=()=>Reports,Localize=n=>n.key,Error=Errors.Add,Warning=Warnings.Add,LogByColor=(c,a)=>Trace.Add("progress:"+a[2]),Random=new GameRandomSource(new System.Random(912))};
                Binding=new OutgameTaskRuntime(Runtime.Runtime,Services,managerServices=>{
                    var stats=Runtime.Statistics;return new OutgameTaskManager(new OutgameDataManagerStorage(()=>"CommonGameModuleTaskMgr",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{})),stats.StorageHost,k=>throw new Exception("unexpected download"),managerServices);
                },()=>Items.Config.Instance,(key,args)=>key,s=>{},a=>{});
                withAchievement=achievement;
                if(achievement)
                {
                    Items.Config.Instance.Items[1004]=new GameItemConfig{id=1004,type1=2,type2=2,item_game="g"};
                    AchievementBinding=new OutgameAchievementRuntime(Runtime.Runtime,new OutgameAchievementServices{Items=()=>Items.Config.Instance,Rewards=()=>Items.Engine,Entities=Items.Services,Reports=()=>Reports,Warning=s=>Warnings.Add(new object[]{s}),LogByColor=(c,a)=>Trace.Add("achievement:"+a[2])},s=>{
                        var stats=Runtime.Statistics;return new OutgameAchievementManager(new OutgameDataManagerStorage(()=>"CommonGameModuleAchievementMgr",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},a=>{},a=>{})),stats.StorageHost,k=>throw new Exception("unexpected download"),s);
                    });
                }
                if(limited)
                {
                    Items.Config.Instance.Items[1301]=new GameItemConfig{id=1301,type1=2,type2=4,paramInt=1301,item_game="g"};
                    new OutgameLimitTimeTaskRuntime(Runtime.Runtime,new OutgameLimitTimeTaskServices{Rewards=()=>Items.Engine,Entities=Items.Services,Reports=()=>Reports,Localize=n=>n.key,LocalizeBusiness=n=>n.key,Log=a=>{},LogByColor=(c,a)=>{},Error=Errors.Add},()=>{
                        var stats=Runtime.Statistics;return new OutgameNoviceTaskManager(new OutgameDataManagerStorage(()=>"CommonGameModuleNoviceTaskManager",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{})),stats.StorageHost,k=>throw new Exception("unexpected download"),new OutgameNoviceTaskManagerServices{Config=()=>Config,Warning=s=>{},Error=Errors.Add});
                    },()=>Items.Config.Instance);
                }
                Runtime.Statistics.Owner.ValueProviders[10000]=args=>Now;
                configureBeforeInit?.Invoke(this);
                Runtime.Init();
            }
            public void Synthetic(params PubTaskConfig[] configs)
            {
                Child.Strategy.Dispose();Config.Tasks.Clear();Config.TasksByGroup.Clear();Config.TaskGroupsByActivity[130001].Clear();
                foreach(var config in configs)Config.Tasks[config.id]=config;
                var data=new OutgameChildTaskData{activityID=130001};data.ext.activityID=130001;data.ext.nextRefreshTimeStamp=long.MaxValue;
                foreach(var config in configs){var task=new OutgameTaskItemData{id=config.id,uid=config.id,activityID=130001};task.ResetProgress();data.tasks.Add(task);}
                Manager.Data.datas=new List<OutgameChildTaskData>{data};Manager.ChildTaskDic[130001]=data;Child.ModuleData=data;Child.Data.state=3;Child.OnInit();
                Parent.Dirty=false;Errors.Clear();Warnings.Clear();Reports.Sent.Clear();Trace.Clear();
            }
            public OutgameTaskItemData Task(int id)=>Child.TasksByUid[id];
            public void Event(int key,long value,int? filter=null)=>Runtime.Statistics.Common.SendMessageGetKey(OutgameStatisticsMessageKey.Get(key),filter.HasValue?new object[]{value,filter.Value}:new object[]{value});
            public string Stored=>Runtime.Statistics.Strings.GetString(Runtime.Statistics.StorageHost.MineGameName+Manager.DataKey,"");
            public void Dispose(){try{Parent?.OnDispose();if(withAchievement)Runtime.Control.GetActivity<OutgameAchievementActivity>(1601001)?.OnDispose();Runtime.Dispose();}finally{OutgameTaskModels.Services=previous;OutgameLimitTaskModels.Services=previousLimited;OutgameAchievementModels.Services=previousAchievement;}}
        }
        static ListArrayInt Row(params int[] values)=>new ListArrayInt{datas=values};
        static PubTaskConfig Config(int id,int key=71,int target=2)=>new PubTaskConfig{id=id,taskGroupId=10001,weight=1,des=new Lang{key="task"},conditionParams=new List<ListArrayInt>{Row(key,target)},showType=new List<ListArrayInt>(),rewards=new List<ListArrayInt>{Row(1001,5),Row(1003,20)},expandItems=new List<ListArrayInt>()};
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Concrete ordinary task manager/activity/strategy/factory over original config, common events, actual item engine and file storage. Explicit account/report/UI hosts; main-page task UI and production Main/platform remain pending. Native/Player validation separate."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="task-activity-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-activity-"+id,result="fail",detail=e.ToString()});}};
            check("original-eight-tasks-real-owner-fsm-factory-and-daily-initialization",()=>{
                using(var f=new Fixture()){
                    Require(f.Config.Tasks.Count==8&&f.Child.ModuleData.tasks.Count==8&&f.Child.TasksByUid.Count==8&&f.Child.Groups.Count==1,"original eight weighted unique tasks and one group generated");
                    Require(ReferenceEquals(f.Strategy.Parent,f.Parent)&&ReferenceEquals(f.Manager.Data.datas[0],f.Child.ModuleData)&&ReferenceEquals(f.Runtime.Control.ChildActivities[130001],f.Child)&&f.Child.Fsm!=null,"single actual owner graph and FSM");
                    Require(f.Child.ModuleData.ext.nextRefreshTimeStamp==OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,4))&&f.Parent.Dirty,"original daily24 configuration and dirty initialization");
                    Require(f.Items.Engine.GetItem(f.Items.Config.Instance.Items[1003]) is OutgameTaskLivenessPoint,"actual activity item factory hook");
                }
            });
            check("statistics-shape-state-receive-filter-and-report-failure",()=>{
                using(var f=new Fixture()){
                    var a=Config(1);var b=Config(2);b.conditionParams[0]=Row(71,9,2);f.Synthetic(a,b);
                    f.Event(71,1);Require(f.Task(1).conditions[0].value==1&&f.Task(2).conditions[0].value==0,"unfiltered event only changes empty-arg record");
                    f.Task(1).receiveState=0;f.Event(71,1);Require(f.Task(1).conditions[0].value==1,"unreceived task skips statistics");
                    f.Event(71,1,9);Require(f.Task(2).conditions[0].value==1,"boxed Int32 filter matches recorded parameter");
                    f.Task(2).conditions[0].value=0;f.Reports.Sending=(kind,r)=>throw new InvalidOperationException("report");f.Parent.Dirty=false;f.Event(71,1,9);
                    Require(f.Task(2).conditions[0].value==1&&!f.Parent.Dirty&&f.Errors.Count==1,"caught report failure preserves progress before dirty/notification");
                }
            });
            check("lifetime-inactive-clamp-unbox-and-condition-target-index",()=>{
                using(var f=new Fixture()){
                    var a=Config(1);var b=Config(2);b.allLifeStatistics=1;f.Synthetic(a,b);f.Child.Data.state=1;f.Runtime.Statistics.Owner.ValueProviders[71]=args=>8;
                    f.Event(71,1);Require(f.Task(1).conditions[0].value==0&&f.Task(2).conditions[0].value==8,"inactive local skip/lifetime absolute count");
                    f.Child.Data.state=3;f.Strategy.StatisticsEvent(new object[]{1,71});Require(f.Errors.Count==1,"wrong boxed Int32 amount caught");
                    f.Event(71,-99);Require(f.Task(1).conditions[0].value==0,"negative clamp");
                    var c=Config(3);c.conditionParams=new List<ListArrayInt>{Row(70,9,10),Row(72,10)};f.Synthetic(c);
                    f.Event(72,1);Require(f.Task(3).conditions[1].value==1&&f.Errors.Count==1&&!f.Parent.Dirty,"source uses first row length to index current target: assignment precedes malformed-shape failure");
                }
            });
            check("claim-real-economy-liveness-cost-order-and-repeat",()=>{
                using(var f=new Fixture()){
                    var c=Config(1);c.expandItems.Add(Row(1002,3));f.Synthetic(c);f.Event(71,2);f.Parent.Dirty=false;
                    var order=new List<string>();f.Items.Host.Added=rows=>{Require(f.Task(1).state==1,"claimed state set before award");order.Add("reward");};
                    f.Runtime.Statistics.Common.AddListener(OutgameTaskNetStrategy.RefreshList,args=>{Require(f.Items.Global.GetItemCount(1002)==-3,"cost applied before list refresh");order.Add("list");});
                    f.Child.TaskComplete(1);Require(f.Task(1).state==1&&f.Items.Global.GetItemCount(1001)==5&&f.Items.Global.GetItemCount(1003)==20&&f.Child.ModuleData.ext.livenessValue==20&&f.Parent.Dirty,"real item factory awards liveness and money once");
                    Require(string.Join(",",order)=="reward,list"&&f.Reports.Sent.Any(r=>r.kind==OutgameLimitTaskReportKind.Complete),"reward/cost/list ordering and all-complete report");
                    f.Child.TaskComplete(1);Require(f.Items.Global.GetItemCount(1001)==5,"claimed state rejects repeat");
                }
            });
            check("reward-failure-claimed-prefix-and-unguarded-liveness-claim",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1));f.Event(71,2);f.Parent.Dirty=false;f.Items.Host.Added=rows=>throw new InvalidOperationException("effects");
                    Throws<InvalidOperationException>(()=>f.Child.TaskComplete(1));Require(f.Task(1).state==1&&f.Items.Global.GetItemCount(1001)==5&&f.Items.Host.Costs==0,"reward-host failure retains claimed state/economy, skips later costs");
                    f.Items.Host.Added=null;long before=f.Items.Global.GetItemCount(1001);f.Child.GetLivenessReward(1);f.Child.GetLivenessReward(1);
                    Require(f.Items.Global.GetItemCount(1001)==before+100&&f.Child.ModuleData.ext.livenessAward==1,"source liveness endpoint itself has no threshold/repeat guard");
                }
            });
            check("daily-strict-boundary-auto-claim-and-reset",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1));f.Event(71,2);long boundary=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,4));f.Child.ModuleData.ext.nextRefreshTimeStamp=boundary;
                    f.Child.ModuleData.ext.videoTimes=8;f.Child.ModuleData.ext.TRefreshTimes=4;f.Child.ModuleData.ext.freeRefreshNum=3;
                    f.Now=boundary;f.Strategy.TimeEvent(null);Require(f.Child.ModuleData.tasks.Count==1&&f.Task(1).state==0,"daily equality does not reset");
                    f.Now++;f.Strategy.TimeEvent(null);Require(f.Child.ModuleData.tasks.Count==0&&f.Items.Global.GetItemCount(1001)==5&&f.Child.ModuleData.ext.livenessValue==0&&f.Child.ModuleData.ext.videoTimes==0&&f.Child.ModuleData.ext.freeRefreshNum==0&&f.Child.ModuleData.ext.TRefreshTimes==0,"strictly later auto-claims ready task then resets counters/removes task");
                    Require(f.Child.ModuleData.ext.nextRefreshTimeStamp==OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,5)),"next original24 boundary");
                }
            });
            check("interval-boundary-overflow-and-expiry-per-removal",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1),Config(2));var config=f.Child.ModuleData.Config;config.refreshPeriod=1;config.refreshParams=new[]{10};f.Child.ModuleData.ext.nextRefreshTimeStamp=f.Now;
                    f.Strategy.TimeEvent(null);Require(f.Child.ModuleData.ext.nextRefreshTimeStamp==f.Now+10000&&f.Child.ModuleData.tasks.Count==0,"interval equality refreshes unlike daily strict comparison");
                    f.Synthetic(Config(1),Config(2));config.refreshPeriod=0;f.Task(1).expireTimeStamp=f.Now;f.Task(2).expireTimeStamp=f.Now;int notices=0;f.Runtime.Statistics.Common.AddListener(OutgameTaskNetStrategy.RefreshList,a=>notices++);
                    f.Strategy.TimeEvent(null);Require(notices==0,"expiry equality retained");f.Now++;f.Strategy.TimeEvent(null);Require(notices==2&&f.Child.TasksByUid.Count==0,"adjacent expired records both removed with separate notifications");
                    config.refreshPeriod=1;config.refreshParams=new[]{0};f.Child.ModuleData.ext.nextRefreshTimeStamp=f.Now;Throws<DivideByZeroException>(f.Strategy.RefreshInterval);
                }
            });
            check("group-selection-first-picked-expiry-and-receive-mode",()=>{
                using(var f=new Fixture()){
                    f.Synthetic();var a=Config(1);var b=Config(2);a.Expire=2;a.receiveMode=1;b.Expire=5;b.receiveMode=0;
                    f.Config.Tasks[1]=a;f.Config.Tasks[2]=b;f.Config.TasksByGroup[10001]=new List<PubTaskConfig>{a,b};var group=new PubTaskGroupConfig{Id=77,activityId=130001,taskGroupId=10001,taskGroupCount=2,taskGroupRandom=10000,extraRefreshTimes=new[]{2}};
                    f.Config.TaskGroups[77]=group;f.Config.TaskGroupsByActivity[130001].Add(group);f.Strategy.ResetTasks(true);
                    var rows=f.Child.ModuleData.tasks;Require(rows.Count==2&&rows[0].expireTimeStamp==rows[1].expireTimeStamp&&rows[0].receiveState==rows[1].receiveState,"all selected rows inherit expiry and receiveMode from first selected config");
                    Require(rows.All(t=>t.uid!=0&&t.groupID==77)&&f.Child.Groups[77].extraRefreshTimes.SequenceEqual(new[]{0}),"source generated identity and group reset");
                }
            });
            check("update-data-rebinds-existing-strategy-and-transient-other-view",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1));var strategy=f.Child.Strategy;var data=new OutgameChildTaskData{activityID=130001};data.ext.activityID=130001;data.ext.nextRefreshTimeStamp=long.MaxValue;
                    var task=new OutgameTaskItemData{id=1,uid=99};task.ResetProgress();data.tasks.Add(task);f.Child.UpdateData(data);f.Event(71,1);
                    Require(ReferenceEquals(strategy,f.Child.Strategy)&&f.Child.TasksByUid[99].conditions[0].value==1,"update replaces module/index and listener binding without creating strategy");
                    var view=f.Parent.OtherViewDic;view[1]=data;Require(f.Parent.OtherViewDic.Count==0&&!ReferenceEquals(view,f.Parent.OtherViewDic),"source getter creates new dictionary every time");
                    f.Parent.OtherViewActivities[88]=f.Child;int count=0;f.Runtime.Statistics.Common.AddListener("CommonModule_AddLaunchTaskOhterActivity",a=>count++);f.Parent.ActivityLaunch(new object[]{88});f.Parent.ActivityLaunch(new object[]{88});Require(count==2,"transient other-view existence never suppresses repeat launch event");
                }
            });
            check("automatic-dirty-save-real-file-restart",()=>{
                string path;long uid;using(var f=new Fixture()){
                    var task=f.Child.ModuleData.tasks.Single(t=>t.id==1);uid=task.uid;f.Event(task.Config.conditionParams[0].datas[0],1);f.Child.TaskComplete(uid);
                    f.Parent.Update();Require(!f.Parent.Dirty&&f.Stored.Length>0,"activity update invokes actual manager storage and clears dirty");path=f.Runtime.Statistics.PathName;
                }
                using(var f=new Fixture(path)){
                    var task=f.Child.ModuleData.tasks.Single(t=>t.id==1);Require(task.uid==uid&&task.state==1&&task.conditions[0].value==1&&f.Child.ModuleData.ext.livenessValue==20,"fresh task runtime restores identity/claimed progress/liveness");
                }
            });
            check("ordinary-and-seven-day-share-real-parent-and-factory-graph",()=>{
                using(var f=new Fixture(limited:true)){
                    var limited=f.Runtime.Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001);var child=limited.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
                    Require(ReferenceEquals(f.Parent.ChildrenById[1301001],limited)&&f.Parent.Children.Count==2&&child.DayTasks.Values.Sum(rows=>rows.Count)==49,"actual ordinary parent preserves registered seven-day parent alongside daily child");
                    Require(f.Items.Engine.GetItem(f.Items.Config.Instance.Items[1003]) is OutgameTaskLivenessPoint&&f.Items.Engine.GetItem(f.Items.Config.Instance.Items[1301]) is OutgameLimitTimeTaskLivenessPoint,"both original category factories coexist");
                    var task=f.Child.ModuleData.tasks.Single(t=>t.id==1);f.Event(task.Config.conditionParams[0].datas[0],1);f.Child.TaskComplete(task.uid);
                    f.Runtime.Statistics.Expansion.SetEventCount(10015,10);child.TaskComplete(1301101);f.Parent.BaseUpdate();
                    Require(task.state==1&&child.FindTask(1301101).state==1&&f.Child.ModuleData.ext.livenessValue==20&&!f.Parent.Dirty&&!limited.Dirty,"both task families progress/claim and save through shared root update");
                }
            });
            check("child-type10-4-factory-base-award-before-use-without-refresh",()=>{
                using(var f=new Fixture()){
                    f.Items.Config.Instance.Items[7001]=new GameItemConfig{id=7001,type1=10,type2=4,paramInt=130001,item_game="g"};
                    Require(f.Parent.ActivityFactoryBase(7001).Produce()==null&&f.Child.ActivityFactoryBase(1003).Produce()==null,"parent and child factory categories remain distinct");
                    var item=f.Items.Engine.GetItem(f.Items.Config.Instance.Items[7001]);Require(item is OutgameTaskIapRefreshItem,"global lookup reaches concrete child factory");
                    var tasks=f.Child.ModuleData.tasks;int count=f.Items.Reports.Uses.Count;
                    item.AddItem(2);Require(f.Items.Global.GetItemCount(7001)==2&&f.Items.Reports.Uses.Count==count+1&&ReferenceEquals(tasks,f.Child.ModuleData.tasks),"regular Add applies inventory then use report without implicit refresh or debit");
                    item.AddItemOnlyModel(3);Require(f.Items.Global.GetItemCount(7001)==5&&f.Items.Reports.Uses.Count==count+1,"model-only add does not call use");
                    f.Items.Host.Added=rows=>throw new InvalidOperationException("award");Throws<InvalidOperationException>(()=>item.AddItem(1));Require(f.Items.Global.GetItemCount(7001)==6&&f.Items.Reports.Uses.Count==count+1,"failed base award prevents virtual use after retaining economic prefix");
                }
            });
            return report;
        }
    }
}
