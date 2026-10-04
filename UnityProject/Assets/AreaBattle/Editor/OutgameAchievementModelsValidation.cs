using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameAchievementModelsValidation
    {
        sealed class Owner:IOutgameAchievementActivity {public OutgameAchievementExt Value=new OutgameAchievementExt();public OutgameAchievementExt Ext=>Value;}
        sealed class Fixture:IDisposable
        {
            readonly OutgameAchievementModelServices previous=OutgameAchievementModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime=new OutgameActivityControlValidation.Fixture(originalConfig:true);
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public readonly Owner Activity=new Owner();public int OwnerReads;
            public Fixture(){Runtime.Init();OutgameAchievementModels.Services=new OutgameAchievementModelServices{Config=()=>Config,GetActivity=id=>{Require(id==1601001,"source owner id");OwnerReads++;return Activity;}};}
            public OutgameAchievementItemData Item(int id,long target=5)
            {Config.Achievements[id]=new PubAchievementConfig{id=id,number=target,showCondition=new List<ListArrayInt>(),rewards=new List<ListArrayInt>{Row(1001,7)}};return new OutgameAchievementItemData{id=id};}
            public void Dispose(){OutgameAchievementModels.Services=previous;Runtime.Dispose();}
        }
        static ListArrayInt Row(params int[] values)=>new ListArrayInt{datas=values};
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original Achievement storage/models/factory over127 original config rows,3 cumulative rows and actual item engine. Required activity-owner endpoint is explicit fixture; concrete Achievement manager/strategy/runtime/UI and automatic persistence remain pending. Schema file roundtrip is not automatic save/restart. No fresh native/Player run."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="achievement-model-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="achievement-model-"+id,result="fail",detail=e.ToString()});}};
            check("original127-configs-three-cumulative-records-and-signed-targets",()=>{
                using(var f=new Fixture()){
                    Require(f.Config.Achievements.Count==127&&f.Config.AchievementAcc.Count==3&&f.Config.AchievementAccList.Select(r=>r.id).SequenceEqual(new[]{30,60,90}),"original full tables");
                    foreach(var row in f.Config.Achievements.Values){var item=new OutgameAchievementItemData{id=row.id,progress=row.number};Require(item.CanComplete()&&!item.Selected&&item.GetRewards().Count==row.rewards.Count&&item.ShowCondition.Count==row.showCondition.Count,"every original row binds source threshold/rewards/show schema");item.state=1;Require(!item.CanComplete(),"claimed state excluded");}
                    var first=new OutgameAchievementItemData{id=1,progress=29};Require(!first.CanComplete()&&first.Config.number==30&&first.GetRewards()[0].itemCount==200,"original first30-level achievement200-gold reward");
                }
            });
            check("eligibility-state-zero-int64-and-independent-show-time-selection",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(901,long.MaxValue);item.progress=long.MaxValue-1;Require(!item.CanComplete(),"full Int64 comparison");item.progress=long.MaxValue;item.time=long.MinValue;item.Selected=true;item.Config.showCondition=null;Require(item.CanComplete(),"show/time/selection not gates");
                    foreach(int state in new[]{1,2,-1,int.MinValue}){item.state=state;Require(!item.CanComplete(),"exact zero state");}item.state=0;item.Config.number=long.MinValue;item.progress=long.MinValue;Require(item.CanComplete(),"signed negative target retained");
                    var reentrant=f.Item(912);OutgameAchievementModels.Services.Config=()=>{reentrant.progress=5;return f.Config;};Require(reentrant.CanComplete(),"source resolves configuration before reading mutable progress");
                }
            });
            check("sorting-ready-incomplete-claimed-and-cached-config-id",()=>{
                using(var f=new Fixture()){
                    var ready=f.Item(904);ready.progress=5;var pending=f.Item(902);var claimed=f.Item(901);claimed.state=1;var second=f.Item(903);second.progress=6;
                    var list=new List<OutgameAchievementItemData>{claimed,pending,ready,second};list.Sort();Require(list.SequenceEqual(new[]{second,ready,pending,claimed}),"ready first then incomplete then claimed; ties by config id");
                    ready.Config.id=900;Require(ready.CompareTo(second)<0,"cached config id tie breaker");claimed.state=2;Require(claimed.CompareTo(claimed)==0&&claimed.CompareTo(pending)>0,"all nonzero states use claimed branch without ordinary-task anomaly");
                    Throws<NullReferenceException>(()=>ready.CompareTo(null));
                }
            });
            check("first-nonnull-config-cache-and-null-retry",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(905);var saved=item.Config;item.id=906;f.Config.Achievements[905]=new PubAchievementConfig{id=999};Require(ReferenceEquals(item.Config,saved),"published config retained after id/table change");
                    var absent=new OutgameAchievementItemData{id=907};Require(absent.Config==null,"missing stays null");f.Config.Achievements[907]=saved;Require(ReferenceEquals(absent.Config,saved),"null cache retries current manager");
                    var acc=new OutgameAccAchievementItemData{index=1,acc=999};Require(acc.Config==null,"cumulative lookup is acc rather than index");var row=new PubAchievementAccConfig{id=999};f.Config.AchievementAcc[999]=row;Require(ReferenceEquals(acc.Config,row),"cumulative null retry");acc.acc=30;Require(ReferenceEquals(acc.Config,row),"cumulative first nonnull retained");
                }
            });
            check("fresh-rewards-signed-quantity-and-retry-after-malformed-row",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(908);item.Config.rewards=new List<ListArrayInt>{Row(1001,-7),Row(1002,int.MinValue)};var first=item.GetRewards();var second=item.GetRewards();Require(!ReferenceEquals(first,second)&&!ReferenceEquals(first[0],second[0])&&second[0].itemCount==-7&&second[1].itemCount==int.MinValue&&second[0].rewardOrder==0,"fresh list and objects, signed counts, order0");
                    first[0].itemCount=44;item.Config.rewards[0].datas[1]=9;Require(item.GetRewards()[0].itemCount==9,"no rewards cache");item.Config.rewards.Add(Row(1001));Throws<IndexOutOfRangeException>(()=>item.GetRewards());item.Config.rewards[2]=Row(1001,11);Require(item.GetRewards().Count==3&&item.GetRewards()[2].itemCount==11,"failed call does not publish partial cache");
                }
            });
            check("show-condition-boxed-middle-copy-and-partial-cache-on-failure",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(909);item.Config.showCondition=new List<ListArrayInt>{Row(71,8,9,-12),Row(72)};Throws<OverflowException>(()=>{var ignored=item.ShowCondition;});var list=item.ShowCondition;
                    Require(list.Count==1&&list[0].key==71&&list[0].value==-12&&list[0].arg.SequenceEqual(new object[]{8,9})&&list[0].arg.All(a=>a is int),"prefix survives invalid negative argument length; middle values boxed Int32");item.Config.showCondition[0].datas[1]=99;item.Config.showCondition[1]=Row(72,3);Require(ReferenceEquals(list,item.ShowCondition)&&list.Count==1&&(int)list[0].arg[0]==8,"published cache does not retry or alias config array");
                    var empty=f.Item(910);empty.Config.showCondition=null;Throws<NullReferenceException>(()=>{var ignored=empty.ShowCondition;});Require(empty.ShowCondition.Count==0,"empty cache published before null configuration failure");
                }
            });
            check("cumulative-live-owner-int64-bit-wrap-and-threshold-short-circuit",()=>{
                using(var f=new Fixture()){
                    f.Activity.Value.statePts=unchecked((long)0x8000000000000001UL);f.Activity.Value.accPts=90;
                    foreach(var pair in new[]{(0,1),(1,0),(63,1),(64,1),(65,0),(-1,1)}){var row=new OutgameAccAchievementItemData{index=pair.Item1,acc=60};Require(row.State==pair.Item2,"unsigned Int64 bit state wraps low six bits");}
                    var item=new OutgameAccAchievementItemData{index=1,acc=90};f.OwnerReads=0;Require(item.CanComplete()&&f.OwnerReads==2,"eligible reads owner for threshold then state");f.Activity.Value.accPts=89;f.OwnerReads=0;Require(!item.CanComplete()&&f.OwnerReads==1,"insufficient short-circuits state lookup");f.Activity.Value=new OutgameAchievementExt{accPts=90,statePts=2};Require(!item.CanComplete()&&item.State==1,"owner replacement read live without snapshot");
                }
            });
            check("cumulative-sorting-readiness-state-and-config-id",()=>{
                using(var f=new Fixture()){
                    f.Activity.Value.accPts=60;f.Activity.Value.statePts=1;
                    var claimed=new OutgameAccAchievementItemData{index=0,acc=30};var ready=new OutgameAccAchievementItemData{index=1,acc=60};var pending=new OutgameAccAchievementItemData{index=2,acc=90};
                    var list=new List<OutgameAccAchievementItemData>{claimed,pending,ready};list.Sort();Require(list.SequenceEqual(new[]{ready,pending,claimed}),"cumulative source ordering");
                    f.Activity.Value.statePts=0;f.Activity.Value.accPts=90;Require(claimed.CompareTo(ready)<0,"ready ties by configured id");claimed.Config.id=80;Require(claimed.CompareTo(ready)>0,"cached config id versus threshold");
                }
            });
            check("independent-file-schema-roundtrip-and-private-cache-exclusion",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(911);item.progress=long.MaxValue;item.time=long.MinValue;item.state=2;item.Selected=true;var conditions=item.ShowCondition;
                    var data=new OutgameAchievementData();data.datas.Add(item);data.ext.statePts=long.MinValue;data.ext.accPts=int.MinValue;
                    var compact=new OutgameAchievementSaveData();compact.ext=data.ext;compact.ids.Add(911);compact.times.Add(long.MinValue);
                    string path=Path.Combine(Path.GetTempPath(),"AreaBattleAchievementSchema-"+Guid.NewGuid().ToString("N"));try{
                        string json=JsonUtility.ToJson(data);Require(!json.Contains("Selected")&&!json.Contains("showConditions")&&!json.Contains("config"),"private cache/selection not serialized");File.WriteAllText(path,json);var restored=JsonUtility.FromJson<OutgameAchievementData>(File.ReadAllText(path));Require(restored.datas[0].progress==long.MaxValue&&restored.datas[0].time==long.MinValue&&restored.datas[0].state==2&&!restored.datas[0].Selected&&restored.ext.statePts==long.MinValue&&restored.ext.accPts==int.MinValue,"signed runtime fields survive independent file read");
                        File.WriteAllText(path,JsonUtility.ToJson(compact));var saved=JsonUtility.FromJson<OutgameAchievementSaveData>(File.ReadAllText(path));Require(saved.ids.SequenceEqual(new[]{911})&&saved.times[0]==long.MinValue&&saved.ext.accPts==int.MinValue,"compact schema exact public fields");
                    }finally{File.Delete(path);}
                    var a=new OutgameAchievementSaveData();var b=new OutgameAchievementData();Require(a.ids.Count==0&&a.times.Count==0&&b.datas.Count==0&&!ReferenceEquals(a.ext,b.ext),"fresh constructor containers");
                }
            });
            check("factory-exact-category-captured-config-and-actual-item-economy",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();var config=f.Config.Instance.Items[1001];config.type1=2;config.type2=2;
                var factory=new OutgameAchievementFactory(1001,()=>f.Config.Instance,f.Errors.Add,f.Services,()=>common);var item=factory.Produce();Require(item is OutgameAchievementPoint,"allocation metadata4627 despite shared constructor annotation");long count=(1L<<32)+7;int events=0;
                common.AddListener(OutgameAchievementPoint.PointAdded,args=>{Require(args.Length==1&&args[0] is int&&(int)args[0]==7&&f.Global.GetItemCount(1001)==count&&f.Host.Adds==1&&f.Reports.Changes.Count==1,"model, report, delivery before boxed lowInt32 common event");events++;});item.AddItem(count);Require(events==1,"regular add emits source event");
                config.type2=3;Require(factory.Produce()==null,"ordinary-task category rejected");config.type2=2;config.type1=1;Require(factory.Produce()==null,"nonactivity primary category rejected");
            });
            check("factory-model-only-and-use-omit-point-event",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();var item=new OutgameAchievementPoint(1001,f.Services,()=>common);int events=0;common.AddListener(OutgameAchievementPoint.PointAdded,a=>events++);
                item.AddItemOnlyModel(13);item.Use(2);Require(events==0&&f.Global.GetItemCount(1001)==13&&f.Host.Adds==0&&f.Reports.Changes.Count==1&&f.Reports.Uses.Count==1,"source model-only inherited path omits point event; Use reports without debit");
            });
            check("factory-host-and-event-failure-retain-source-prefix",()=>{
                var f=new OutgameGlobalItemRewardsValidation.Fixture();var common=new OutgameCommonMessageDispatcher();var item=new OutgameAchievementPoint(1001,f.Services,()=>common);int events=0;common.AddListener(OutgameAchievementPoint.PointAdded,a=>{events++;throw new InvalidOperationException("point handler");});
                f.Host.Added=rows=>throw new InvalidOperationException("reward delivery");Throws<InvalidOperationException>(()=>item.AddItem(3));Require(f.Global.GetItemCount(1001)==3&&f.Host.Adds==1&&events==0,"delivery failure precedes point event after model/report");f.Host.Added=null;Throws<InvalidOperationException>(()=>item.AddItem(4));Require(f.Global.GetItemCount(1001)==7&&f.Host.Adds==2&&events==1,"point event failure keeps completed award/delivery");
            });
            return report;
        }
    }
}
