using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
using GameItemConfig=AreaBattle.SharedItemConfig.GameItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskModelsValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action run)where T:Exception{try{run();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Child:IOutgameLimitTimeTaskChild
        {
            public OutgameLimitTimeTaskChildData ModuleData{get;}=new OutgameLimitTimeTaskChildData();
            public SortedDictionary<int,List<OutgameLimitTaskItemData>> DayTasks{get;}=new SortedDictionary<int,List<OutgameLimitTaskItemData>>();
        }
        sealed class Parent:IOutgameLimitTimeTaskActivity
        {
            public readonly Dictionary<int,IOutgameLimitTimeTaskChild> Children=new Dictionary<int,IOutgameLimitTimeTaskChild>();
            public IOutgameLimitTimeTaskChild GetChild(int id){Children.TryGetValue(id,out var child);return child;}
            public void OnSave(){}public void RefreshData(){}
        }
        sealed class Fixture:IDisposable
        {
            readonly OutgameLimitTaskModelServices previous=OutgameLimitTaskModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime=new OutgameActivityControlValidation.Fixture(originalConfig:true);
            public readonly OutgameGlobalItemRewardsValidation.Fixture Items=new OutgameGlobalItemRewardsValidation.Fixture();
            public readonly List<object[]> Errors=new List<object[]>();public readonly Parent Parent=new Parent();
            public readonly Child Child=new Child();public readonly OutgameLimitTaskModelServices Services;
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public Fixture()
            {
                Runtime.Init();Child.ModuleData.ext.activityId=11;Child.ModuleData.ext.dayId=7;Parent.Children[11]=Child;
                Services=new OutgameLimitTaskModelServices{Config=()=>Config,GetActivity=id=>{Require(id==1301001,"source root activity ID");return Parent;},Items=()=>Items.Config.Instance,Statistics=Runtime.Statistics.Expansion,Error=Errors.Add};
                OutgameLimitTaskModels.Services=Services;
            }
            public OutgameLimitTaskItemData Task(int id,int day=1)
            {
                var config=new PubnoviceTaskConfig{id=id,activityId=11,day=day,conditionParams=new List<ListArrayInt>(),showType=new List<ListArrayInt>(),rewards=new List<ListArrayInt>()};
                Config.NoviceTasks[id]=config;if(!Config.NoviceTasksByActivity.ContainsKey(11))Config.NoviceTasksByActivity[11]=new Dictionary<int,PubnoviceTaskConfig>();Config.NoviceTasksByActivity[11][id]=config;
                return new OutgameLimitTaskItemData{id=id};
            }
            public OutgameNoviceAccRewardItemData Acc(int id=100,int mode=0)
            {
                Config.NoviceActivities[11]=new PubNTActivityConfig{id=11,accType=mode};
                var config=new PubnoviceAccRewardConfig{id=id,activityId=11,accValue=1,rewards=new List<ListArrayInt>()};
                Config.NoviceAcc[id]=config;if(!Config.NoviceAccByActivity.ContainsKey(11))Config.NoviceAccByActivity[11]=new Dictionary<int,PubnoviceAccRewardConfig>();Config.NoviceAccByActivity[11][id]=config;
                return new OutgameNoviceAccRewardItemData{id=id};
            }
            public void Dispose(){OutgameLimitTaskModels.Services=previous;Runtime.Dispose();}
        }
        static ListArrayInt ArrayRow(params int[] values)=>new ListArrayInt{datas=values};
        static OutgameLimitTimeTaskFactory Factory(OutgameGlobalItemRewardsValidation.Fixture f,OutgameCommonMessageDispatcher common,int id=1001)=>new OutgameLimitTimeTaskFactory(id,()=>f.Config.Instance,f.Errors.Add,f.Services,()=>common);
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original limit-task models, cumulative progress and liveness item factory. Real49-row config/statistics/item engine; explicit required child activity graph fixture. Actual child initialization, progress listeners, reward claims/pages/SevenDay/Main remain pending. No native/Player run."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("limit-task-model-original49-config-condition-and-reward-shapes",()=>{
                using(var f=new Fixture())
                {
                    Require(f.Config.NoviceTasks.Count==49,"original resource task table");
                    foreach(var config in f.Config.NoviceTasks.Values)
                    {
                        var task=new OutgameLimitTaskItemData{id=config.id};task.ResetProgress();
                        Require(task.ActivityId==config.activityId&&task.conditions.Count==config.conditionParams.Count&&task.ShowCondition.Count==config.showType.Count&&task.RewardsData.Count==config.rewards.Count,"all original rows decode without invented defaults");
                        for(int i=0;i<task.conditions.Count;i++)Require(task.conditions[i].value==0&&task.conditions[i].key==config.conditionParams[i].datas[0],"recorded progress starts zero");
                    }
                }
            });
            check("limit-task-model-config-cache-and-group-owner-independence",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1);var cached=task.Config;cached.activityId=900;f.Config.NoviceTasks[1]=new PubnoviceTaskConfig{id=90};
                    Require(ReferenceEquals(cached,task.Config)&&task.ActivityId==11,"cached row retained; owner from group keys not row activityId");
                    var missing=new OutgameLimitTaskItemData{id=987654};Require(missing.Config==null,"missing source config returns null");f.Config.NoviceTasks[987654]=cached;Require(ReferenceEquals(missing.Config,cached),"null cache retries");
                    f.Config.NoviceTasksByActivity.Clear();Require(task.ActivityId==0&&f.Errors.Count==1,"uncached activity mapping diagnoses absent membership");
                }
            });
            check("limit-task-model-show-cache-boxing-and-failure-prefix",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1);task.Config.showType.Add(ArrayRow(71,-5,6,int.MaxValue));task.Config.showType.Add(ArrayRow(72,0));
                    var rows=task.ShowCondition;Require(rows[0].key==71&&rows[0].value==int.MaxValue&&rows[0].arg[0] is int&&rows[0].arg.SequenceEqual(new object[]{-5,6})&&rows[1].arg.Length==0,"threshold excluded; args boxed Int32, condition value Int64");
                    task.Config.showType[0].datas[1]=88;Require((int)task.ShowCondition[0].arg[0]==-5&&ReferenceEquals(rows,task.ShowCondition),"independent cached argument array");
                    var bad=f.Task(2);bad.Config.showType.Add(ArrayRow(71,1));bad.Config.showType.Add(ArrayRow(72));Throws<OverflowException>(()=>{var ignored=bad.ShowCondition;});
                    Require(bad.ShowCondition.Count==1,"published cache retains earlier row and prevents reconstruction after malformed second row");
                }
            });
            check("limit-task-model-reset-keeps-claim-state-and-partial-progress",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1);task.state=1;task.Selected=true;task.Config.showType.Add(ArrayRow(71,1));var show=task.ShowCondition;
                    task.Config.conditionParams.Add(ArrayRow(9,4,7,99));task.ResetProgress();task.conditions[0].value=123;task.Config.conditionParams.Add(ArrayRow(10));Throws<OverflowException>(task.ResetProgress);
                    Require(task.conditions.Count==1&&task.conditions[0].value==0&&task.conditions[0].arg.SequenceEqual(new[]{4,7})&&task.state==1&&task.Selected&&ReferenceEquals(show,task.ShowCondition),"clear and rebuild prefix only; no state/cache reset");
                }
            });
            check("limit-task-model-eligibility-record-show-day-state-matrix",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1,3);task.Config.conditionParams.Add(ArrayRow(8,2));task.Config.showType.Add(ArrayRow(71,4));task.ResetProgress();long shown=0;int queries=0;
                    f.Runtime.Statistics.Owner.ValueProviders[71]=a=>{queries++;return shown;};
                    for(int state=0;state<3;state++)for(int recorded=1;recorded<=2;recorded++)for(int current=3;current<=4;current++)for(int day=2;day<=3;day++)
                    {
                        task.state=state;task.conditions[0].value=recorded;shown=current;f.Child.ModuleData.ext.dayId=day;queries=0;
                        bool expected=state==0&&recorded==2&&current==4&&day==3;
                        Require(task.CanComplete==expected&&queries==(recorded==2?1:0),"recorded short-circuit; state checked after statistics/day");
                        Require(task.BtnState==(state==1?2:expected?0:1),"ready0/pending1/claimed2");
                    }
                    task.state=1;task.conditions=null;Require(task.BtnState==2,"claimed button bypasses eligibility access");
                }
            });
            check("limit-task-model-live-statistics-callback-threshold-and-day",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1,2);task.Config.showType.Add(ArrayRow(71,1));f.Child.ModuleData.ext.dayId=1;
                    f.Runtime.Statistics.Owner.ValueProviders[71]=a=>{var row=task.ShowCondition[0];row.value=4;task.ShowCondition[0]=row;f.Child.ModuleData.ext.dayId=2;return 3;};
                    Require(!task.CanComplete,"target reread after GameValue callback");
                    f.Runtime.Statistics.Owner.ValueProviders[71]=a=>4;Require(task.CanComplete,"latest target/day permit completion");
                    task.conditions.Add(new OutgameLimitTaskCondition{value=5});Throws<ArgumentOutOfRangeException>(()=>{var ignored=task.CanComplete;});
                }
            });
            check("limit-task-model-task-order-priority-and-signed-config-id",()=>{
                using(var f=new Fixture())
                {
                    var ready=f.Task(1);var waiting=f.Task(2,9);var claimed=f.Task(3);claimed.state=1;var otherClaim=f.Task(4);otherClaim.state=2;
                    var rows=new List<OutgameLimitTaskItemData>{otherClaim,claimed,waiting,ready};rows.Sort();Require(rows.SequenceEqual(new[]{ready,waiting,claimed,otherClaim}),"claimable first, pending next, nonzero states last with config ID tie-break");
                    ready.Config.id=int.MaxValue;waiting.Config.day=1;waiting.Config.id=int.MinValue;Require(ready.CompareTo(waiting)==1&&waiting.CompareTo(ready)==-1,"signed comparison no subtraction overflow");
                    Throws<NullReferenceException>(()=>ready.CompareTo(null));
                }
            });
            check("limit-task-model-reward-cache-failure-and-json-runtime-exclusion",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1);task.Config.rewards.Add(ArrayRow(1001,-7));task.Config.rewards.Add(ArrayRow(1002));Throws<IndexOutOfRangeException>(()=>{var ignored=task.RewardsData;});
                    Require(task.RewardsData.Count==1&&task.RewardsData[0].itemCount==-7&&task.RewardsData[0].rewardOrder==0,"partial reward cache retained; signed amount preserved");
                    task.Selected=true;task.ResetProgress();var ignoredShow=task.ShowCondition;string json=JsonUtility.ToJson(task);Require(json=="{\"id\":1,\"conditions\":[],\"state\":0}","only source public save fields serialized, no runtime services/caches/selected");
                    var restart=JsonUtility.FromJson<OutgameLimitTaskItemData>(json);task.Config.rewards.RemoveAt(1);Require(restart.CanComplete&&!restart.Selected&&restart.RewardsData[0].itemCount==-7,"deserialized model uses bound owners and rebuilt runtime caches");
                }
            });
            check("limit-task-model-cumulative-count-claimed-only-and-state-gate",()=>{
                using(var f=new Fixture())
                {
                    var reward=f.Acc();var a=f.Task(1);a.state=1;var b=f.Task(2);b.state=2;var c=f.Task(3);
                    f.Child.DayTasks[1]=new List<OutgameLimitTaskItemData>{a,b,c};f.Child.DayTasks[9]=new List<OutgameLimitTaskItemData>{a};
                    Require(reward.Progress==2&&f.Child.ModuleData.ext.AccProgress==2,"all day groups counted; duplicates count twice; only exactly state1");
                    reward.state=2;reward.Config.accValue=2;Require(reward.CanComplete(),"acc reward only state1 blocks claim, not arbitrary nonzero state");
                    reward.state=1;f.Services.Config=()=>throw new InvalidOperationException();Require(!reward.CanComplete(),"already claimed accumulator short-circuits all lookups");
                }
            });
            check("limit-task-model-cumulative-item-ownership-difference",()=>{
                using(var f=new Fixture())
                {
                    var reward=f.Acc(mode:1);var task=f.Task(1);task.state=1;task.Config.rewards.AddRange(new[]{ArrayRow(101,7),ArrayRow(102,5),ArrayRow(103,99)});
                    f.Items.Config.Instance.Items[101]=new GameItemConfig{id=101,type1=2,type2=4,paramInt=11};f.Items.Config.Instance.Items[102]=new GameItemConfig{id=102,type1=2,type2=4,paramInt=12};f.Items.Config.Instance.Items[103]=new GameItemConfig{id=103,type1=1,type2=4,paramInt=11};
                    f.Child.DayTasks[1]=new List<OutgameLimitTaskItemData>{task};
                    Require(reward.Progress==12&&f.Child.ModuleData.ext.AccProgress==7,"reward progress includes all matching item types; Ext additionally filters item.paramInt to activityId");
                    f.Items.Global.Indexes.Items[101]=new OutgameItemUserData{itemId=101,itemCount=999};Require(reward.Progress==12,"progress derives from claimed task rewards, not inventory balance");
                    task.RewardsData[0].itemCount=long.MaxValue;task.RewardsData[1].itemCount=2;Require(reward.Progress==1&&f.Child.ModuleData.ext.AccProgress==-1,"unchecked Int64 accumulation then Int32 truncation");
                }
            });
            check("limit-task-model-cumulative-invalid-mode-and-config-reread",()=>{
                using(var f=new Fixture())
                {
                    var reward=f.Acc(mode:8);Require(reward.Progress==0&&f.Errors.Count==0,"invalid reward mode silently returns zero");
                    Require(f.Child.ModuleData.ext.AccProgress==0&&f.Errors.Count==1,"Ext invalid mode diagnoses configured row ID");
                    var task=f.Task(1);task.state=1;f.Child.DayTasks[1]=new List<OutgameLimitTaskItemData>{task};int calls=0;
                    f.Services.Config=()=>{if(++calls==3)f.Config.NoviceActivities[11].accType=1;return f.Config;};
                    Require(reward.Progress==0&&calls>=4,"activity config resolved again for second mode comparison");
                }
            });
            check("limit-task-model-target-progress-max-empty-missing-and-owner",()=>{
                using(var f=new Fixture())
                {
                    var reward=f.Acc();reward.Config.accValue=-3;var second=f.Acc(101);second.Config.accValue=9;var ext=f.Child.ModuleData.ext;
                    Require(ext.TargetProgress==9,"max configured threshold with initial zero");f.Config.NoviceAccByActivity[11].Clear();Require(ext.TargetProgress==0,"present empty group returns zero");f.Config.NoviceAccByActivity.Remove(11);
                    Require(ext.TargetProgress==999&&f.Errors.Count==1,"missing group reports and returns source sentinel999");
                    var next=new OutgameActivityConfigManager(null);next.NoviceAccByActivity[11]=new Dictionary<int,PubnoviceAccRewardConfig>{{1,new PubnoviceAccRewardConfig{accValue=19}}};int reads=0;
                    f.Services.Config=()=>++reads==1?f.Config:next;Require(ext.TargetProgress==19&&reads==3,"initial activity lookup retained; membership and values resolve current owners independently");
                }
            });
            check("limit-task-model-child-owner-capture-and-missing-membership",()=>{
                using(var f=new Fixture())
                {
                    var task=f.Task(1);var alternate=new Parent();f.Services.GetActivity=id=>{f.Services.GetActivity=k=>alternate;return f.Parent;};
                    Require(ReferenceEquals(task.ChildActivity,f.Child),"root captured before activity membership query");
                    f.Config.NoviceTasksByActivity.Clear();Require(task.ChildActivity==null&&f.Errors.Count==1,"missing membership diagnosed then source ID0 child lookup; no synthetic child");
                    var reward=new OutgameNoviceAccRewardItemData{id=999999};Require(reward.ActivityId==0&&f.Errors.Count==2,"separate cumulative membership diagnostic");
                }
            });
            check("limit-task-model-acc-order-reward-cache-and-page-selection",()=>{
                using(var f=new Fixture())
                {
                    var a=f.Acc(1);var b=f.Acc(2);a.Config.accValue=int.MaxValue;b.Config.accValue=int.MinValue;Require(a.CompareTo(b)==1&&b.CompareTo(a)==-1,"acc threshold signed ordering");
                    a.Config.rewards.Add(ArrayRow(1001,-5));var rows=a.RewardsData;a.Config.rewards.Clear();Require(ReferenceEquals(rows,a.RewardsData)&&rows[0].itemCount==-5,"acc reward cache preserves negative source count");
                    var fresh=f.Acc(3);f.Services.Config=()=>{a.Config.accValue=-10;return f.Config;};Require(a.CompareTo(fresh)==-1,"own threshold read after other config resolution may change cached row");
                    var page=new OutgameLimitTaskPageItem(7,11);page.Selected=true;Require(page.day==7&&page.activityId==11&&page.Selected,"source constructor day then activity, independent selected property");
                }
            });
            check("limit-task-factory-type-routing-and-constructor-current-config",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();
                Require(Factory(f,common).Produce()==null,"non-liveness returns null");f.Config.Instance.Items[1001].type1=2;f.Config.Instance.Items[1001].type2=4;
                var factory=Factory(f,common);var next=new GameItemConfig{id=1001,type1=1,paramInt=9};f.Config.Instance.Items[1001]=next;
                var item=(OutgameLimitTimeTaskLivenessPoint)factory.Produce();Require(ReferenceEquals(item.ItemConfig,next),"factory routing uses captured row, item constructor rereads current config");
                var absent=Factory(f,common,98765);Throws<NullReferenceException>(()=>absent.Produce());Require(f.Errors.Count==1,"absent config diagnosed by base then source dereference failure");
            });
            check("limit-task-factory-real-model-event-truncation-regular-add-and-use",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();f.Config.Instance.Items[1001].type1=2;f.Config.Instance.Items[1001].type2=4;f.Config.Instance.Items[1001].paramInt=11;
                int received=0;long count=(long)int.MaxValue+2;common.AddListener("CommonGameModule_LimitTimeTask_LivenessPointAdd11",a=>{Require(a.Length==1&&a[0] is int&&f.Global.GetItemCount(1001)==count&&f.Reports.Changes.Count==1,"full model/reports committed before common event with boxed Int32");received=(int)a[0];});
                var item=(OutgameLimitTimeTaskLivenessPoint)Factory(f,common).Produce();item.AddItemOnlyModel(count);Require(received==int.MinValue+1&&f.Host.Adds==0,"model event truncates amount, no reward UI host");
                item.AddItem(3);Require(f.Global.GetItemCount(1001)==count+3&&f.Host.Adds==1,"regular add follows base reward route without common model event");
                item.Use(99);Require(f.Global.GetItemCount(1001)==count+3&&f.Reports.Uses.Count==1,"source Use reports but does not debit");
            });
            check("limit-task-factory-event-live-config-and-failure-prefix",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();var item=new OutgameLimitTimeTaskLivenessPoint(1001,f.Services,()=>common);
                f.Messages.AddListener("Item_ItemChange",a=>item.ItemConfig=new GameItemConfig{id=1001,paramInt=12});int calls=0;
                common.AddListener("CommonGameModule_LimitTimeTask_LivenessPointAdd12",a=>{calls++;throw new InvalidOperationException("listener");});
                Throws<InvalidOperationException>(()=>item.AddItemOnlyModel(4));Require(f.Global.GetItemCount(1001)==4&&calls==1&&f.Global.Indexes.IsDirty,"listener fails after model save-dirty/report and reads updated item config");
                f.Engine.AddRewardsModelDel=(rows,reason)=>throw new InvalidOperationException("model endpoint");Throws<InvalidOperationException>(()=>item.AddItemOnlyModel(2));Require(calls==1&&f.Global.GetItemCount(1001)==4,"base failure prevents common event");
            });
            return report;
        }
    }
}
