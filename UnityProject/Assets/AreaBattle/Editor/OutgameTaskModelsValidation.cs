using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskModelsValidation
    {
        sealed class Fixture:IDisposable
        {
            readonly OutgameTaskModelServices previous=OutgameTaskModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime=new OutgameActivityControlValidation.Fixture(originalConfig:true);
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public readonly List<object[]> Formats=new List<object[]>();public Action Formatting;
            public Fixture(){Runtime.Init();OutgameTaskModels.Services=new OutgameTaskModelServices{Config=()=>Config,LanguageFormat=(key,args)=>{Formats.Add(args);Formatting?.Invoke();return key+":"+string.Join(",",args);}};}
            public OutgameTaskItemData Task(int id,int receive=1,int state=0,long progress=0)
            {Config.Tasks[id]=new PubTaskConfig{id=id,conditionParams=new List<ListArrayInt>{new ListArrayInt{datas=new[]{880,5}}},des=new Lang{key="test"}};var task=new OutgameTaskItemData{id=id,receiveState=receive,state=state};task.ResetProgress();task.conditions[0].value=progress;return task;}
            public void Dispose(){OutgameTaskModels.Services=previous;Runtime.Dispose();}
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original ordinary-task storage/models and liveness factory over real config/item engine. Temporary file JSON roundtrip validates storage schema, not TaskMgr/strategy automatic persistence. Task/Achievement activities/managers/UI/Main/Player remain pending; no new native run."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="task-model-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="task-model-"+id,result="fail",detail=e.ToString()});}};
            check("original-eight-tasks-three-liveness-tiers-and-target-description",()=>{
                using(var f=new Fixture()){
                    Require(f.Config.Tasks.Count==8&&f.Config.TaskActivities.Count==1&&f.Config.Liveness.Count==3,"actual original tables");
                    foreach(var config in f.Config.Tasks.Values){var task=new OutgameTaskItemData{id=config.id};task.ResetProgress();Require(task.receiveState==1&&task.conditions.Count==config.conditionParams.Count&&!task.Selected,"source constructor/default progress");task.GetDes();Require(f.Formats[f.Formats.Count-1].SequenceEqual(config.conditionParams.Select(p=>(object)p.datas[p.datas.Length-1])),"description uses target values boxed Int32");}
                    var ext=new OutgameTaskExt{activityID=130001};var tiers=ext.GetLivenessItems();Require(tiers.Count==3&&tiers[0].livenessValue==30&&tiers[0].Rewards[0].itemId==1001&&tiers[0].Rewards[0].itemCount==50,"original daily liveness first tier");
                }
            });
            check("reset-appends-copied-conditions-and-preserves-source-failure-prefix",()=>{
                using(var f=new Fixture()){
                    var task=f.Task(91,0,1,33);task.Selected=true;var first=task.conditions[0];task.Config.conditionParams[0].datas=new[]{881,7,8,99};task.ResetProgress();
                    Require(task.conditions.Count==2&&ReferenceEquals(first,task.conditions[0])&&first.value==33&&task.conditions[1].key==881&&task.conditions[1].arg.SequenceEqual(new[]{7,8})&&task.state==1&&task.Selected,"append without clearing existing conditions/state/selection");
                    task.Config.conditionParams[0].datas[1]=44;Require(task.conditions[1].arg[0]==7,"copied condition args");task.Config.conditionParams.Add(new ListArrayInt{datas=new[]{882}});Throws<OverflowException>(task.ResetProgress);Require(task.conditions.Count==3&&task.conditions[2].arg[0]==44,"malformed second row leaves appended first row");
                }
            });
            check("eligibility-exact-state-zero-without-receive-expiry-or-progress-gates",()=>{
                using(var f=new Fixture()){
                    var task=f.Task(92,0,0,5);task.expireTimeStamp=-1;task.progress=long.MinValue;Require(task.CanComplete(),"recorded conditions sufficient even unreceived/expired/independent progress");
                    task.state=2;Require(!task.CanComplete(),"only state0 eligible");task.state=0;task.conditions[0].value=4;Require(!task.CanComplete(),"Int64 threshold comparison");task.conditions.Clear();Require(task.CanComplete(),"empty recorded conditions vacuously complete despite configured targets");
                    task.conditions.Add(new OutgameTaskCondition{value=5});task.conditions.Add(new OutgameTaskCondition{value=5});Throws<ArgumentOutOfRangeException>(()=>task.CanComplete());
                }
            });
            check("source-sort-receive-status-priority-id-and-noncanonical-state",()=>{
                using(var f=new Fixture()){
                    var ready=f.Task(99,1,0,5);var unreceived=f.Task(98,0);var received=f.Task(97);var claimed=f.Task(96,1,1,5);
                    var list=new List<OutgameTaskItemData>{claimed,received,unreceived,ready};list.Sort();Require(list.SequenceEqual(new[]{ready,unreceived,received,claimed}),"source ready then unreceived then received then claimed priority");
                    var otherReady=f.Task(100,0,0,5);Require(ready.CompareTo(otherReady)<0&&otherReady.CompareTo(ready)>0,"equal eligibility sorts by cached config id");
                    var abnormal=f.Task(101,1,2,5);Require(abnormal.CompareTo(abnormal)==1&&abnormal.CompareTo(claimed)>0&&claimed.CompareTo(abnormal)==1,"source noncanonical state branch is not normalized into total ordering");
                    ready.Config.id=200;Require(ready.CompareTo(otherReady)>0,"tie breaker uses config id instead of task id");
                }
            });
            check("config-cache-null-retry-and-description-captured-targets",()=>{
                using(var f=new Fixture()){
                    var task=f.Task(93);var cached=task.Config;task.id=987;f.Config.Tasks[93]=new PubTaskConfig{id=999};Require(ReferenceEquals(task.Config,cached),"cached config survives id/table change");
                    var missing=new OutgameTaskItemData{id=555};Require(missing.Config==null,"missing config is null");f.Config.Tasks[555]=cached;Require(ReferenceEquals(missing.Config,cached),"null cache retries");
                    f.Formatting=()=>cached.conditionParams[0].datas[1]=88;Require(task.GetDes()=="test:5"&&task.Config.conditionParams[0].datas[1]==88,"description snapshots target arguments before formatting callback");
                }
            });
            check("condition-key-equality-ignores-progress-and-preserves-null-distinction",()=>{
                var a=new OutgameTaskCondition{key=1,value=long.MaxValue};var b=new OutgameTaskCondition{key=1,value=-1};Require(a.KeyEqual(b),"both null args equal despite value");b.arg=Array.Empty<int>();Require(!a.KeyEqual(b),"null differs from empty");a.arg=Array.Empty<int>();Require(a.KeyEqual(b),"empty args equal");a.arg=new[]{2,3};b.arg=new[]{3,2};Require(!a.KeyEqual(b),"argument order matters");b.arg=new[]{2,3};Require(a.KeyEqual(b),"matching key and arguments");b.key=2;Require(!a.KeyEqual(b),"key matters");Throws<NullReferenceException>(()=>a.KeyEqual(null));
            });
            check("refresh-counters-replace-lists-with-zeroes-and-retain-unrelated-fields",()=>{
                using(var f=new Fixture()){
                    f.Config.TaskActivities[777]=new PubTaskActivityConfig{id=777,extraRefreshTimes=new[]{4,8}};f.Config.TaskGroups[778]=new PubTaskGroupConfig{Id=778,extraRefreshTimes=new[]{5,9,12}};
                    var child=new OutgameChildTaskData{activityID=777};var old=child.ext.extraRefreshNumList;old.Add(9);child.ext.extraRefreshNum=7;child.ResetExtraRefreshTimes();
                    Require(!ReferenceEquals(old,child.ext.extraRefreshNumList)&&child.ext.extraRefreshNumList.SequenceEqual(new[]{0,0})&&old[0]==9&&child.ext.extraRefreshNum==7,"replace counter vector, preserve total and old references");
                    var group=new OutgameTaskGroupData{groupID=778,freeRefreshTimes=6};Require(group.extraRefreshTimes==null,"group ctor does not allocate extra list");group.ResetExtraRefreshTimes();Require(group.extraRefreshTimes.SequenceEqual(new[]{0,0,0})&&group.freeRefreshTimes==6,"group shape from cached config");
                    child.Config.extraRefreshTimes=null;Throws<NullReferenceException>(child.ResetExtraRefreshTimes);Require(child.ext.extraRefreshNumList.Count==0,"replacement published before missing-array failure");
                }
            });
            check("liveness-cache-explicit-refresh-dictionary-order-and-int32-bit-wrap",()=>{
                using(var f=new Fixture()){
                    var ext=new OutgameTaskExt{activityID=888,livenessAward=unchecked((int)0x80000001)};Throws<NullReferenceException>(ext.RefreshLivenessItems);
                    var rows=new Dictionary<int,PubdailyLivenessConfig>();for(int i=0;i<34;i++){int id=300-i;rows.Add(id,new PubdailyLivenessConfig{id=id,livenessValue=i*10,rewards=new List<ListArrayInt>()});}f.Config.LivenessByActivity[888]=rows;
                    var list=ext.GetLivenessItems();Require(list.Count==34&&list[0].id==300&&list[0].state==1&&list[31].state==1&&list[32].state==1&&list[33].state==0,"dictionary enumeration order, unsigned shift, low five-bit count");
                    ext.livenessAward=0;Require(ReferenceEquals(list,ext.GetLivenessItems())&&list[0].state==1,"cached state stays until explicit refresh");var old=list[0];ext.RefreshLivenessItems();Require(ReferenceEquals(list,ext.GetLivenessItems())&&list[0].state==0&&!ReferenceEquals(old,list[0]),"refresh clears same list and creates new records");
                    f.Config.LivenessByActivity.Remove(888);ext.RefreshLivenessItems();Require(list.Count==0,"missing config group leaves cleared list after warning");
                }
            });
            check("reward-cache-publishes-before-malformed-row-and-keeps-negative-count",()=>{
                using(var f=new Fixture()){
                    var config=new PubdailyLivenessConfig{id=777,rewards=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,-7}},new ListArrayInt{datas=new[]{1002}}}};f.Config.Liveness[777]=config;
                    var item=new OutgameTaskLivenessItemData{id=777};Throws<IndexOutOfRangeException>(()=>{var ignored=item.Rewards;});Require(item.Rewards.Count==1&&item.Rewards[0].itemCount==-7,"partial rewards cache retained after malformed second entry");config.rewards[0].datas[1]=8;Require(item.Rewards[0].itemCount==-7,"cached count independent of original row");
                }
            });
            check("source-shallow-clone-and-independent-file-schema-roundtrip",()=>{
                using(var f=new Fixture()){
                    var task=f.Task(94);task.uid=long.MaxValue;task.expireTimeStamp=long.MinValue;task.Selected=true;task.conditions[0].arg=new[]{17};
                    var data=new OutgameTaskData();var child=new OutgameChildTaskData{activityID=130001};child.tasks.Add(task);child.ext.activityID=130001;child.ext.livenessAward=int.MinValue;child.ext.GetLivenessItems();data.datas.Add(child);
                    var clone=(OutgameTaskData)data.Clone();Require(!ReferenceEquals(clone,data)&&ReferenceEquals(clone.datas,data.datas)&&ReferenceEquals(clone.datas[0].tasks[0],task),"source MemberwiseClone shares entire nested graph");
                    string json=JsonUtility.ToJson(data);Require(!json.Contains("Selected")&&!json.Contains("m_config")&&!json.Contains("livenessItemDatas"),"nonpublic caches/selected property not stored");
                    string path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskSchema-"+Guid.NewGuid().ToString("N")+".json");try{File.WriteAllText(path,json);var restored=JsonUtility.FromJson<OutgameTaskData>(File.ReadAllText(path));Require(restored.datas[0].tasks[0].uid==long.MaxValue&&restored.datas[0].tasks[0].expireTimeStamp==long.MinValue&&restored.datas[0].tasks[0].conditions[0].arg[0]==17&&!restored.datas[0].tasks[0].Selected&&restored.datas[0].ext.livenessAward==int.MinValue,"independent file read retains exact public signed values and clears selection");}finally{File.Delete(path);}
                }
            });
            check("actual-task-liveness-factory-model-event-order-and-low-int32",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();f.Config.Instance.Items[1001].type1=2;f.Config.Instance.Items[1001].type2=3;f.Config.Instance.Items[1001].paramInt=130001;
                var factory=new OutgameTaskFactory(1001,()=>f.Config.Instance,f.Errors.Add,f.Services,()=>common);var item=factory.Produce();Require(item is OutgameTaskLivenessPoint,"source category2/3 factory");long amount=(1L<<32)+7;int calls=0;
                common.AddListener("CommonGameModule_Task_LivenessPointAdd130001",args=>{Require(args[0] is int&&(int)args[0]==7&&f.Global.GetItemCount(1001)==amount&&f.Reports.Changes.Count==1,"actual long award and report precede lowInt32 activity event");calls++;});item.AddItemOnlyModel(amount);Require(calls==1&&f.Host.Adds==0,"model award event without regular host delivery");
                f.Config.Instance.Items[1001].type2=4;Require(factory.Produce()==null,"limited-task category rejected by ordinary factory");
            });
            check("liveness-event-failure-keeps-model-award-and-regular-add-omits-event",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();f.Config.Instance.Items[1001].type1=2;f.Config.Instance.Items[1001].type2=3;f.Config.Instance.Items[1001].paramInt=11;
                var item=new OutgameTaskLivenessPoint(1001,f.Services,()=>common);common.AddListener("CommonGameModule_Task_LivenessPointAdd11",args=>throw new InvalidOperationException("activity event"));Throws<InvalidOperationException>(()=>item.AddItemOnlyModel(9));Require(f.Global.GetItemCount(1001)==9&&f.Reports.Changes.Count==1,"model and report retained before activity callback failure");
                item.AddItem(4);Require(f.Global.GetItemCount(1001)==13&&f.Host.Adds==1,"regular add delegates base without activity-only event");item.Use(2);Require(f.Global.GetItemCount(1001)==13&&f.Reports.Uses.Count==1,"source Use reports but does not debit by itself");
            });
            return r;
        }
    }
}
