using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskControlViewValidation
    {
        public sealed class Fixture:IDisposable,IOutgameAchievementTaskPage
        {
            public readonly OutgameTaskRowsValidation.Fixture Rows;
            public OutgameTaskActivityValidation.Fixture Tasks=>Rows.Tasks;
            public OutgameAchievementActivity Achievement=>Tasks.Runtime.Control.GetActivity<OutgameAchievementActivity>(1601001);
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly OutgameTaskControl Control;public readonly OutgameTaskControlServices Services;
            public readonly OutgameAchievementTaskView View;public readonly GameObject Main;
            public readonly OutgameTaskEntrance Entrance;public readonly List<OutgameTaskRow> Created=new List<OutgameTaskRow>();
            public readonly List<object[]> Warnings=new List<object[]>();public readonly List<string> Trace=new List<string>();
            readonly Dictionary<string,GameObject> nodes=new Dictionary<string,GameObject>();readonly string manifest;readonly bool native;bool opened;
            public int Level=10;public Func<int> LevelRead;public int ClearSingleton;public bool Empty;
            public Fixture(bool native=false,string path=null,Action<OutgameTaskActivityValidation.Fixture> configureBeforeInit=null)
            {
                this.native=native;Rows=new OutgameTaskRowsValidation.Fixture(native,path,true,configureBeforeInit);
                Main=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/Proj_xqzdStartUI"),Rows.Root.transform,false);Main.SetActive(true);
                Entrance=new OutgameTaskEntrance{Root=Main,Button=Main.transform.Find("LeftBar/taskBtn").GetComponent<Button>()};
                Services=new OutgameTaskControlServices{Activities=()=>Tasks.Runtime.Control,Config=()=>Rows.Config,CurrentLevel=()=>LevelRead!=null?LevelRead():Level,DailyActivityId=()=>130001,Messages=()=>Tasks.Runtime.Statistics.Messages,Statistics=Tasks.Runtime.Statistics.Expansion,Entrance=()=>Entrance};
                OutgameCoreControllerBindings.BindTasks(Registry,Services);Control=(OutgameTaskControl)Registry.Resolve(4507);Control.OnInit();
                manifest=File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-ui-import.json"));
                foreach(var pair in new OutgameImportedUiOutlets(manifest,"TaskPanelUI").Read(Rows.Page))nodes.Add(pair.Key,(GameObject)pair.Value);
                nodes["TaskContent"].SetActive(false);nodes["AchtContent"].SetActive(true);
                View=new OutgameAchievementTaskView(new OutgameAchievementTaskViewServices{Activity=()=>Achievement,Common=()=>Tasks.Runtime.Statistics.Common,Statistics=Services.Statistics,ClearSingleton=()=>{ClearSingleton++;Trace.Add("clear");},Warning=Warnings.Add}){RootUI=this};
            }
            public void Open(){View.OnInit();opened=true;View.OnLateInit();}
            public void Synthetic(params PubAchievementConfig[] configs)
            {
                var strategy=Achievement.Manager.Strategy;strategy.RemoveListeners();strategy.MessageKeys.Clear();Tasks.Config.Achievements.Clear();Tasks.Config.AchievementGroups.Clear();var data=new OutgameAchievementData();
                foreach(var config in configs){Tasks.Config.Achievements[config.id]=config;if(!Tasks.Config.AchievementGroups.TryGetValue(config.type,out var groups)){groups=new Dictionary<OutgameActivityConditionPriority,List<PubAchievementConfig>>();Tasks.Config.AchievementGroups[config.type]=groups;}var key=new OutgameActivityConditionPriority(config.contentType,config.content);if(!groups.TryGetValue(key,out var group)){group=new List<PubAchievementConfig>();groups[key]=group;}group.Add(config);data.datas.Add(new OutgameAchievementItemData{id=config.id});}
                strategy.Data=data;Achievement.Manager.UpdateCallback(data);strategy.AddListeners();
            }
            public OutgameTaskRow GetTaskItem()
            {var root=UnityEngine.Object.Instantiate(nodes["TaskItemItem"],nodes["AchtItemContent"].transform,false);root.SetActive(true);var row=new OutgameTaskRow(root,new OutgameImportedUiOutlets(manifest,"TaskItemItem"),Rows.Services);Created.Add(row);return row;}
            public void SetAchTabReddot(){Trace.Add("red");nodes["AchtTab"].transform.GetChild(2).gameObject.SetActive(Control.IsAchiCanComplete());}
            public void SetTaskEmpty(bool empty){Empty=empty;nodes["AchtTipText"].SetActive(empty);Trace.Add("empty:"+empty);}
            public void Dispose(){Control.OnDispose();if(opened)View.OnDestroy();Rows.Dispose();}
        }
        public static PubAchievementConfig Config(int id,int type=1,int key=71,long target=5)=>new PubAchievementConfig{id=id,type=type,priority=id,contentType=key,number=target,showCondition=new List<ListArrayInt>(),des=new Lang{key="achievement"},rewards=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,7}}}};
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Concrete controller4507 and AchiTaskSubUI3986 using simultaneous ordinary-task/Achievement activities, original main entrance/task page/rows and actual statistics/item storage. Full TaskSingleton/TaskPanel lifecycle/tabs/Main startup and platform/effect/report hosts remain pending. Native validation separate."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="task-control-view-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-control-view-"+id,result="fail",detail=e.ToString()});}};
            check("actual-resolver-owner-cache-and-statistic900001",()=>{
                using(var f=new Fixture()){
                    Require(ReferenceEquals(f.Registry.Resolve(4507),f.Control)&&ReferenceEquals(f.Control.Daily,f.Tasks.Child)&&ReferenceEquals(f.Control.Achievement,f.Achievement),"actual registered controller resolves both installed activities");
                    Require(f.Services.Statistics.GameValue(900001)==0&&f.Rows.Daily.Filter(f.Tasks.Child.ModuleData.tasks).Count==8,"controller registers source zero provider; old fixture view endpoint remains explicit");
                    f.Rows.DailyServices.GameValue=f.Services.Statistics.GameValue;Require(f.Rows.Daily.Filter(f.Tasks.Child.ModuleData.tasks).Count==7,"actual provider hides task8");
                    f.Control.OnDispose();Require(f.Registry.HasInstance(4507)&&ReferenceEquals(f.Control.Daily,f.Tasks.Child)&&f.Services.Statistics.GameValue(900001)==0,"dispose clears owner caches but preserves controller slot/provider");
                }
            });
            check("ads-success-warwin-real-statistics-and-unsubscribe",()=>{
                using(var f=new Fixture()){
                    var stats=f.Services.Statistics;int ad=f.Rows.Config.statisticEventConfig.DailyWatchAds,win=f.Rows.Config.statisticEventConfig.DailyCompleteLv;
                    long a=stats.EventCount(ad),w=stats.EventCount(win);var msg=f.Tasks.Runtime.Statistics.Messages;
                    msg.SendMessage("GF_AdsPlayCallBack",new object[]{false});Require(stats.EventCount(ad)==a,"false ads callback no award statistic");msg.SendMessage("GF_AdsPlayCallBack",new object[]{true});msg.SendMessage("WarWin");
                    Require(stats.EventCount(ad)==a+1&&stats.EventCount(win)==w+1&&f.Tasks.Child.ModuleData.tasks.Any(t=>t.conditions.Any(c=>c.key==ad&&c.value==1)),"controller messages update real daily records");
                    f.Control.OnDispose();msg.SendMessage("WarWin");Require(stats.EventCount(win)==w+1,"source controller listener removed");Throws<InvalidCastException>(()=>f.Control.AdsComplete(new object[]{1}));
                }
            });
            check("level9-reset-level10-entrance-and-refresh-not-auto-subscribed",()=>{
                using(var f=new Fixture()){
                    foreach(var t in f.Tasks.Child.ModuleData.tasks)foreach(var c in t.conditions)c.value=8;
                    f.Level=9;f.Control.RefreshData(null);Require(f.Tasks.Child.ModuleData.tasks.All(t=>t.conditions.All(c=>c.value==0))&&!f.Entrance.Button.gameObject.activeSelf,"preunlock clears condition values and hides actual source entrance");
                    f.Level=10;f.Control.RefreshData(null);Require(f.Entrance.Button.gameObject.activeSelf,"source level10 unlock");
                    f.Level=9;f.Tasks.Runtime.Statistics.Messages.SendMessage("LoadStartingUI");Require(f.Entrance.Button.gameObject.activeSelf,"source OnInit does not subscribe RefreshData despite removing it on Dispose");
                    int calls=0;f.LevelRead=()=>++calls==1?9:10;f.Control.RefreshData(null);Require(calls==2&&f.Entrance.Button.gameObject.activeSelf,"source reads level again after reset");
                }
            });
            check("daily-red-includes-filtered-unreceived-and-liveness",()=>{
                using(var f=new Fixture()){
                    foreach(var t in f.Tasks.Child.ModuleData.tasks){t.state=0;foreach(var c in t.conditions)c.value=0;}var hidden=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==8);hidden.receiveState=0;foreach(var c in hidden.conditions)c.value=long.MaxValue;
                    Require(f.Control.IsDailyCanComplete(),"daily red ignores UI task8 filter and receive state");hidden.state=1;Require(!f.Control.IsDailyCanComplete(),"unready ordinary tasks and empty liveness no red");
                    f.Tasks.Child.ModuleData.ext.livenessValue=30;Require(f.Control.IsDailyCanComplete(),"first unclaimed liveness threshold contributes red");f.Tasks.Child.ModuleData.ext.livenessAward=7;f.Tasks.Child.ModuleData.ext.RefreshLivenessItems();Require(!f.Control.IsDailyCanComplete(),"claimed liveness bits excluded");
                }
            });
            check("rank-red-rewrites-positive-progress-and-skips-that-record",()=>{
                using(var f=new Fixture()){
                    int rank=f.Rows.Config.statisticEventConfig.ArenaRank;f.Synthetic(Config(1,key:rank,target:-10));var item=f.Achievement.FindAchievement(1);item.progress=3;f.Services.Statistics.SetEventCount(rank,2);item.progress=3;
                    Require(!f.Control.IsAchiCanComplete()&&item.progress==2,"positive rank record refreshed using raw EventCount but skipped for current completion scan");
                    item.progress=0;Require(f.Control.IsAchiCanComplete(),"zero progress falls through source eligibility even negative rank target");
                }
            });
            check("red-short-circuit-and-source-entrance-node-selection",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1,target:0));f.Services.DailyActivityId=()=>throw new Exception("must short circuit daily");f.Control.SetRedDot();Require(f.Entrance.Button.transform.GetChild(0).gameObject.activeSelf,"achievement ready short-circuits daily owner");
                    f.Entrance.SetNode(0,false);Require(!f.Entrance.Button.gameObject.activeSelf,"node0 visibility");f.Entrance.SetNode(1,false);Require(!f.Entrance.Button.transform.GetChild(0).gameObject.activeSelf,"node1 red");f.Entrance.SetNode(99,true);Require(!f.Entrance.Button.gameObject.activeSelf,"other node index no-op");
                }
            });
            check("original-achievement-rows-bind-real-config-and-parent",()=>{
                using(var f=new Fixture()){
                    f.Open();Require(f.View.Rows.Count>0&&f.Created.Count==f.View.Rows.Count&&f.Trace[0]=="red","source refresh updates red before projection");
                    var row=f.View.Rows[1];Require(row.Type==OutgameTaskType.Achievement&&row.Data.id==1&&row.Data.number==30&&ReferenceEquals(row.Data.rewards,f.Tasks.Config.Achievements[1].rewards)&&row.ActionCache&&!row.IsClaim,"real first achievement projected into original shared row");
                    Require(row.Lifetime.Transform.parent.name=="AchtItemContent"&&row.Title.font&&row.Icon.sprite,"original achievement content/outlets/assets");
                }
            });
            check("show-filter-equality-empty-args-and-reserved-type",()=>{
                using(var f=new Fixture()){
                    var a=Config(1);a.showCondition.Add(new ListArrayInt{datas=new[]{777,9,2}});var b=Config(2);f.Synthetic(a,b,Config(3,type:2));
                    f.Services.Statistics.RegisteredValueFunc(777,args=>{Require(args.Length==0,"source ignores condition middle args");return 3;});
                    var list=f.View.Filter(f.Achievement.Manager.Data.datas);Require(list.Count==1&&list[0].id==3,"not greater-equal; invisible first type still reserves type and suppresses second");
                    f.Achievement.FindAchievement(1).state=1;list=f.View.Filter(f.Achievement.Manager.Data.datas);Require(list.Count==2&&list[0].id==2,"state1 skips without reserving type");
                }
            });
            check("projection-int32-truncation-shared-rewards-and-retained-uid",()=>{
                using(var f=new Fixture()){
                    var config=Config(1,target:(1L<<32)+12);config.content=7;f.Synthetic(config);f.Open();var item=f.Achievement.FindAchievement(1);var row=f.View.Rows[1];row.Data.Uid=99;item.progress=(1L<<32)+8;
                    OutgameAchievementTaskView.ApplyData(row,item);Require(row.Data.number==12&&row.Data.prog==8&&row.Data.ContentArgument==7&&row.Data.Uid==99&&row.Data.liveness==0&&ReferenceEquals(row.Data.dis,config.des),"source store32 on Int64 values and no UID assignment");
                }
            });
            check("refresh-existing-only-reorders-and-missing-type-warns",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1),Config(2,type:2));f.Open();var row=f.View.Rows[1];f.Achievement.FindAchievement(1).progress=4;f.View.RefreshRows(new object[]{f.Achievement.Manager.SortList});Require(row.Data.prog==0,"existing refresh does not rewrite snapshots");
                    f.View.Rows.Remove(2);f.View.RefreshRows(new object[]{f.Achievement.Manager.SortList});Require(f.Created.Count==2&&f.Warnings.Count==1&&(int)f.Warnings[0][0]==2,"nonempty row dictionary prevents creation, missing reorder logs type");
                    f.View.RefreshRows(new object[]{new List<OutgameAchievementItemData>()});Require(f.Empty,"empty filter shows placeholder");f.View.RefreshRows(new object[]{f.Achievement.Manager.SortList});Require(f.Empty,"nonempty refresh does not explicitly hide old placeholder");
                }
            });
            check("actual-row-slide-claim-reuses-next-type-row-then-removes",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1),Config(2,target:10));f.Open();var row=f.View.Rows[1];f.Achievement.FindAchievement(1).progress=5;OutgameAchievementTaskView.ApplyData(row,f.Achievement.FindAchievement(1));row.Refresh();row.Claim.onClick.Invoke();
                    Require(f.Achievement.FindAchievement(1).state==0&&!row.Claim.enabled,"actual source row waits for slide");f.Rows.Motion.Advance(.71f);
                    Require(f.Achievement.FindAchievement(1).state==1&&row.Data.id==2&&ReferenceEquals(row,f.View.Rows[1])&&row.Lifetime.GameObject.activeSelf&&f.Tasks.Items.Global.GetItemCount(1001)==7,"actual claim advances existing type row and economy");
                    f.Achievement.FindAchievement(2).progress=10;OutgameAchievementTaskView.ApplyData(row,f.Achievement.FindAchievement(2));row.Refresh();row.Claim.onClick.Invoke();f.Rows.Motion.Advance(.71f);
                    Require(!f.View.Rows.ContainsKey(1)&&!row.ActionCache&&!row.Lifetime.GameObject.activeSelf&&f.Tasks.Items.Global.GetItemCount(1001)==14,"last claim removes type mapping and hides row without disposal");
                }
            });
            check("destroy-clears-owner-before-unsubscribe-and-retains-view-fields",()=>{
                using(var f=new Fixture()){
                    f.Synthetic(Config(1));f.Open();var row=f.View.Rows[1];f.Trace.Clear();f.View.OnDestroy();Require(f.ClearSingleton==1&&f.Trace.SequenceEqual(new[]{"clear"}),"clear singleton before listener removal");
                    f.Achievement.RefreshSortList();Require(f.Trace.Count==1&&ReferenceEquals(f.View.Rows[1],row)&&ReferenceEquals(f.View.RootUI,f),"destroy unsubscribes while retaining fields and row objects");
                }
            });
            return report;
        }
    }
}
