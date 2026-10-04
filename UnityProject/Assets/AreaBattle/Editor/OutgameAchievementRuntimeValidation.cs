using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
using GameItemConfig=AreaBattle.SharedItemConfig.GameItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameAchievementRuntimeValidation
    {
        public sealed class Fixture:IDisposable
        {
            readonly OutgameAchievementModelServices previous=OutgameAchievementModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime;
            internal readonly OutgameGlobalItemRewardsValidation.Fixture Items=new OutgameGlobalItemRewardsValidation.Fixture();
            public readonly OutgameLimitTimeTaskValidation.Reports Reports=new OutgameLimitTimeTaskValidation.Reports();
            public readonly List<string> Warnings=new List<string>();public readonly List<object[]> Logs=new List<object[]>();
            public readonly OutgameAchievementServices Services;
            public long Now=1234567890123;public long Lifetime;
            public OutgameAchievementActivity Activity=>Runtime.Control.GetActivity<OutgameAchievementActivity>(1601001);
            public OutgameAchievementManager Manager=>Activity.Manager;
            public OutgameAchievementOffStrategy Strategy=>Manager.Strategy;
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public Fixture(string path=null,bool native=false)
            {
                Runtime=new OutgameActivityControlValidation.Fixture(path,native,true);
                Items.Config.Instance.Items[1004]=new GameItemConfig{id=1004,type1=2,type2=2,item_game="g"};
                Runtime.Services.Items=()=>Items.Engine;Runtime.Activity.ItemFactoryError=Warnings.Add;
                Services=new OutgameAchievementServices{Items=()=>Items.Config.Instance,Rewards=()=>Items.Engine,Entities=Items.Services,Reports=()=>Reports,Warning=Warnings.Add,LogByColor=(c,a)=>Logs.Add(a)};
                new OutgameAchievementRuntime(Runtime.Runtime,Services,s=>{var stats=Runtime.Statistics;return new OutgameAchievementManager(new OutgameDataManagerStorage(()=>"CommonGameModuleAchievementMgr",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},a=>{},a=>{})),stats.StorageHost,k=>throw new Exception("unexpected download"),s);});
                Runtime.Statistics.Owner.ValueProviders[10000]=args=>Now;Runtime.Statistics.Owner.ValueProviders[10015]=args=>Lifetime;
                Runtime.Init();
            }
            public void Synthetic(params PubAchievementConfig[] rows)
            {
                Strategy.RemoveListeners();Strategy.MessageKeys.Clear();Config.Achievements.Clear();Config.AchievementGroups.Clear();
                var data=new OutgameAchievementData();
                foreach(var row in rows)
                {
                    Config.Achievements.Add(row.id,row);if(!Config.AchievementGroups.TryGetValue(row.type,out var groups)){groups=new Dictionary<OutgameActivityConditionPriority,List<PubAchievementConfig>>();Config.AchievementGroups.Add(row.type,groups);}
                    var key=new OutgameActivityConditionPriority(row.contentType,row.content);if(!groups.TryGetValue(key,out var group)){group=new List<PubAchievementConfig>();groups.Add(key,group);}group.Add(row);data.datas.Add(new OutgameAchievementItemData{id=row.id});
                }
                Strategy.Data=data;Manager.UpdateCallback(data);Strategy.AddListeners();Reports.Sent.Clear();Logs.Clear();Warnings.Clear();
            }
            public OutgameAchievementItemData Item(int id)=>Activity.FindAchievement(id);
            public void Event(int key,long amount,int? filter=null)=>Runtime.Statistics.Common.SendMessageGetKey(OutgameStatisticsMessageKey.Get(key),filter.HasValue?new object[]{amount,filter.Value}:new object[]{amount});
            public string Stored=>Runtime.Statistics.Strings.GetString(Runtime.Statistics.StorageHost.MineGameName+Manager.DataKey,"");
            public void Dispose(){try{Activity?.OnDispose();Runtime.Dispose();}finally{OutgameAchievementModels.Services=previous;}}
        }
        static PubAchievementConfig Row(int id,int key=71,int type=1,int content=0,long target=5)=>new PubAchievementConfig{id=id,type=type,priority=id,contentType=key,content=content,number=target,showCondition=new List<ListArrayInt>(),des=new Lang{key="achievement"},rewards=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,7}}}};
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Actual Achievement activity/manager/offline strategy, original127 configs, common statistics, item engine and account-aware data pool file storage. Explicit report/reward-delivery hosts. Source whole TaskPanelUI/controller/Main/platform and original audiovisual/Player remain pending; no new native run."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="achievement-runtime-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="achievement-runtime-"+id,result="fail",detail=e.ToString()});}};
            check("original127-owner-pool-fsm-and-three-live-cumulative-records",()=>{
                using(var f=new Fixture()){
                    Require(f.Manager.Data.datas.Count==127&&f.Activity.AchievementMap.Count==127&&f.Manager.SortList.Count==127,"all original records loaded");
                    Require(ReferenceEquals(f.Manager,f.Runtime.Statistics.Pool.Managers[4722])&&ReferenceEquals(f.Manager.Data,f.Strategy.Data)&&ReferenceEquals(f.Strategy.Activity,f.Activity)&&f.Activity.Fsm!=null,"actual pool/activity/strategy identity and FSM");
                    Require(f.Activity.GetAccItems().Select(a=>a.acc).SequenceEqual(new[]{30,60,90})&&f.Activity.GetAchievementsListByType(1).Any(a=>a.id==1),"original cumulative and progressive representative lists");
                    Require(f.Items.Engine.GetItem(f.Items.Config.Instance.Items[1004]) is OutgameAchievementPoint,"actual ActivityControl item factory resolution");
                    f.Activity.Ext.accPts=60;Require(f.Activity.GetAccItems()[1].CanComplete(),"cumulative model now resolves real owner");
                }
            });
            check("statistics-abs-filter-exact-unbox-and-progress-report-before-threshold",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2,72,2,9));f.Event(71,-2);Require(f.Item(1).progress==2&&f.Reports.Sent.Count==1,"absolute delta with report even below target");
                    f.Event(72,3);Require(f.Item(2).progress==0,"nonzero content requires matching filter");f.Event(72,3,9);Require(f.Item(2).progress==3,"exact boxed Int32 filter");
                    Throws<InvalidCastException>(()=>f.Strategy.StatisticsEvent(new object[]{1,71}));Throws<InvalidCastException>(()=>f.Strategy.StatisticsEvent(new object[]{1L,9L,72}));
                    f.Item(1).progress=0;f.Event(71,long.MinValue);Require(f.Item(1).progress==long.MinValue,"source unchecked abs minimum remains negative");
                }
            });
            check("live-progress-reordering-keeps-original-multiple-visit-behavior",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1,target:5),Row(2,target:5));f.Item(1).progress=0;f.Item(2).progress=5;f.Event(71,1);
                    Require(f.Item(1).progress==2&&f.Item(2).progress==5&&f.Logs.Count==2,"first change sorts Data so same first item is visited again and later item is skipped");
                }
            });
            check("refresh-statistics-recomputes-all-records-and-preserves-claimed-time",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2,72,2,9));f.Item(1).state=1;f.Item(1).time=77;f.Item(1).progress=99;
                    f.Runtime.Statistics.Owner.ValueProviders[71]=args=>{Require(args.Length==1&&args[0] is int&&(int)args[0]==0,"refresh passes content0 explicitly");return 8;};
                    f.Runtime.Statistics.Owner.ValueProviders[72]=args=>{Require((int)args[0]==9,"configured content argument");return 12;};
                    f.Runtime.Statistics.Common.SendMessage(OutgameAchievementStrategy.StatisticsRefresh);
                    Require(f.Item(1).progress==8&&f.Item(1).state==1&&f.Item(1).time==77&&f.Item(2).progress==12,"absolute refresh includes claimed records without resetting state/time");
                }
            });
            check("claim-real-economy-point-event-time-and-next-representative",()=>{
                using(var f=new Fixture()){
                    var a=Row(1);a.rewards.Add(new ListArrayInt{datas=new[]{1004,30}});f.Synthetic(a,Row(2,target:10));f.Item(1).progress=5;
                    f.Items.Host.Added=rows=>Require(f.Item(1).state==0,"source awards before claimed flag");f.Activity.GetAchievementReward(1);
                    Require(f.Item(1).state==1&&f.Item(1).time==f.Now&&f.Items.Global.GetItemCount(1001)==7&&f.Activity.Ext.accPts==30&&f.Items.Global.GetItemCount(1004)==30,"true award and cumulative event before claim/time");
                    Require(f.Activity.GetAchievementsListByType(1)[0].id==2,"advance source group representative");f.Manager.GetAchievementReward(1);Require(f.Items.Global.GetItemCount(1001)==7,"state rejects later repeat");
                }
            });
            check("award-failure-remains-unclaimed-with-mutated-economy",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1));f.Item(1).progress=5;f.Items.Host.Added=rows=>throw new InvalidOperationException("delivery");
                    Throws<InvalidOperationException>(()=>f.Activity.GetAchievementReward(1));Require(f.Item(1).state==0&&f.Item(1).time==0&&f.Items.Global.GetItemCount(1001)==7,"award delivery failure retains inventory but not later claimed fields");
                    f.Items.Host.Added=null;f.Activity.GetAchievementReward(1);Require(f.Item(1).state==1&&f.Items.Global.GetItemCount(1001)==14,"source permits retry after failed delivery");
                }
            });
            check("report-failure-prefix-and-missing-visible-representative",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2));f.Item(1).progress=5;f.Reports.Sending=(kind,report)=>{if(kind==OutgameLimitTaskReportKind.Reward)throw new InvalidOperationException("report");};
                    Throws<InvalidOperationException>(()=>f.Activity.GetAchievementReward(1));Require(f.Item(1).state==1&&f.Item(1).time==f.Now&&f.Activity.GetAchievementsListByType(1)[0].id==1,"report failure after claim before representative replacement");
                    f.Reports.Sending=null;f.Item(2).progress=5;Throws<ArgumentOutOfRangeException>(()=>f.Activity.GetAchievementReward(2));Require(f.Item(2).state==1&&f.Items.Global.GetItemCount(1001)==14,"nonrepresentative endpoint awards then fails source list[-1] replacement");
                }
            });
            check("point-stage-enumeration-order-no-ext-event-and-low-int32",()=>{
                using(var f=new Fixture()){
                    f.Config.AchievementAcc.Clear();f.Config.AchievementAcc[90]=new PubAchievementAccConfig{id=90};f.Config.AchievementAcc[30]=new PubAchievementAccConfig{id=30};int notices=0;f.Runtime.Statistics.Common.AddListener(OutgameAchievementStrategy.RefreshExt,args=>notices++);
                    f.Items.Engine.GetItem(f.Items.Config.Instance.Items[1004]).AddItem((1L<<32)+100);
                    var report=f.Reports.Sent.Last().report;Require(f.Activity.Ext.accPts==100&&report.ActivityName=="成就点阶段30"&&notices==0,"last qualifying dictionary entry not maximum; no point ext broadcast");
                }
            });
            check("compact-read-padding-legacy-repair-and-invalid-time-prefix",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2));f.Strategy.LoadData("{\"ext\":{\"accPts\":60,\"statePts\":2},\"ids\":[1,999],\"times\":[77]}");Require(f.Item(1).state==1&&f.Item(1).time==77&&f.Item(2).state==0&&!f.Activity.AchievementMap.ContainsKey(999)&&f.Activity.Ext.accPts==60,"compact repair removes unknown/adds missing with times padding");
                    f.Strategy.LoadData("{\"ext\":{\"accPts\":30},\"datas\":[{\"id\":2,\"state\":1,\"time\":123,\"progress\":99}]}");Require(f.Item(2).state==1&&f.Item(2).time==123&&f.Item(2).progress!=99&&f.Item(1).state==0,"legacy records preserved while progress refreshed");
                    f.Strategy.ReadStoredData("{\"ext\":{\"accPts\":4},\"ids\":[1,2],\"times\":[]}");Require(f.Strategy.Data.datas.Count==2&&f.Strategy.Data.datas.All(a=>a.time==0),"pad both missing timestamps");
                }
            });
            check("duplicate-last-wins-and-entire-type-claimed-fallback",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2),Row(3,72),Row(4,72));var data=f.Strategy.Data;foreach(var item in data.datas)item.state=1;var replacement=new OutgameAchievementItemData{id=2,state=1,time=99};data.datas.Add(replacement);f.Manager.UpdateCallback(data);
                    Require(ReferenceEquals(f.Item(2),replacement)&&f.Activity.AchievementMap.Count==4&&f.Activity.GetAchievementsListByType(1).Count==1&&f.Activity.GetAchievementsListByType(1)[0].id==2,"duplicate overwrite and fallback checks entire type list: second all-claimed group omitted");
                }
            });
            check("reset-clears-values-retains-current-representatives-and-listeners",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Row(1),Row(2));f.Item(1).progress=5;f.Activity.GetAchievementReward(1);var oldAcc=f.Activity.GetAccItems();var oldRow=oldAcc[0];f.Activity.Ext.accPts=30;f.Activity.Ext.statePts=1;
                    f.Activity.ResetProgress();Require(f.Item(1).state==0&&f.Item(1).time==0&&f.Item(1).progress==0&&f.Activity.Ext.accPts==0&&f.Activity.Ext.statePts==0,"source reset values");
                    Require(f.Activity.GetAchievementsListByType(1)[0].id==2&&ReferenceEquals(oldAcc,f.Activity.GetAccItems())&&!ReferenceEquals(oldRow,oldAcc[0]),"reset retains existing representatives but rebuilds cumulative rows in same list");
                    int keyCount=f.Strategy.MessageKeys.Count;f.Strategy.Dispose();f.Strategy.AddListeners();f.Event(71,1);Require(f.Item(1).progress==0&&f.Strategy.MessageKeys.Count==keyCount,"source dispose leaves MessageKeys populated so AddListeners short-circuits");
                }
            });
            check("pool-account-save-and-independent-original-config-restart",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleAchievement-"+Guid.NewGuid().ToString("N"));
                try{
                    using(var f=new Fixture(path)){f.Lifetime=30;f.Runtime.Statistics.Common.SendMessage(OutgameAchievementStrategy.StatisticsRefresh);f.Activity.GetAchievementReward(1);f.Activity.Ext.accPts=60;f.Activity.Ext.statePts=2;f.Runtime.Statistics.Pool.SaveData();var saved=JsonUtility.FromJson<OutgameAchievementSaveData>(f.Stored);Require(saved.ids.Contains(1)&&saved.times[saved.ids.IndexOf(1)]==f.Now&&!f.Stored.Contains("progress")&&!f.Stored.Contains("datas"),"real data pool saves compact claimed records under account key");}
                    using(var f=new Fixture(path)){Require(f.Item(1).state==1&&f.Item(1).time==f.Now&&f.Item(1).progress==0&&f.Activity.Ext.accPts==60&&f.Activity.GetAccItems()[1].State==1&&f.Activity.AchievementMap.Count==127,"independent file startup preserves claim/time/ext and recalculates current statistics");}
                }finally{if(Directory.Exists(path))Directory.Delete(path,true);}
            });
            check("empty-claim-save-restart-and-pool-save-disable",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleAchievementEmpty-"+Guid.NewGuid().ToString("N"));
                try{
                    using(var f=new Fixture(path)){
                        f.Activity.Ext.accPts=17;f.Runtime.Statistics.Pool.SetSaveDisabled(true);f.Runtime.Statistics.Pool.SaveData();Require(f.Stored=="","existing global disable gate prevents manager save");
                        f.Runtime.Statistics.Pool.SetSaveDisabled(false);f.Runtime.Statistics.Pool.SaveData();Require(JsonUtility.FromJson<OutgameAchievementSaveData>(f.Stored).ids.Count==0,"empty compact claim list persisted");
                    }
                    using(var f=new Fixture(path)){Require(f.Manager.Data.datas.Count==127&&f.Activity.Ext.accPts==17&&f.Manager.Data.datas.All(a=>a.state==0),"source empty-ids fallback reconstructs all runtime rows without losing ext");}
                }finally{if(Directory.Exists(path))Directory.Delete(path,true);}
            });
            return r;
        }
    }
}
