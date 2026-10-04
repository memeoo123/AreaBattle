using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameSevendayValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action run)where T:Exception{try{run();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTimeTaskValidation.Fixture Tasks=new OutgameLimitTimeTaskValidation.Fixture();
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameSevendayActivityServices Services;
            public readonly OutgameCommanderManager Commanders;
            public readonly OutgameProfile Profile=new OutgameProfile();
            public OutgameSkinCatalog Skins;
            public int Level=11;
            public OutgameSevendayActivityControl Control=>(OutgameSevendayActivityControl)Registry.Resolve(4502);
            public OutgameChildLimitTimeTaskActivity Child=>Tasks.Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            public Fixture()
            {
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>6,()=>0,()=>null,(p,c,a)=>{});
                Config.statisticEventConfig=JsonUtility.FromJson<StatisticEventConfig>(Resources.Load<TextAsset>("Recovered/FirstPack/Config/StatisticEventConfig").text);
                Skins=OutgameSkinCatalog.FromOriginal("","{\"Datas\":[]}","{\"Datas\":[]}");
                var stats=Tasks.Runtime.Statistics;
                Commanders=new OutgameCommanderManager(Profile,Resources.Load<TextAsset>("Recovered/FirstPack/Config/CommanderConfig").text,
                    new OutgameDataManagerStorage(()=>"CommanderManager",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{})),stats.StorageHost,s=>throw new Exception("unexpected download"));
                stats.Pool.AddModel(4028,Commanders,true);Registry.Bind(4027,()=>new OutgameCommanderControl(()=>stats.Pool,Registry));Registry.Resolve(4027).OnInit();
                Services=new OutgameSevendayActivityServices{Activities=()=>Tasks.Runtime.Control,Config=()=>Config,Skins=()=>Skins,CurrentLevel=()=>Level,Commanders=()=>(OutgameCommanderControl)Registry.Resolve(4027),Statistics=stats.Expansion};
                OutgameCoreControllerBindings.BindSevenday(Registry,Services);
            }
            public void Clock(long value)=>Tasks.Runtime.Statistics.Owner.ValueProviders[10000]=a=>value;
            public void Dispose()=>Tasks.Dispose();
        }
        public sealed class EntryHost:IOutgameStartupEntryHost
        {
            public readonly List<string> Trace=new List<string>();public Action Loaded,Ranking;public bool EnterGame{get;set;}public bool RedDotSourceFlag8{get;set;}public int RedDotSourceValue16{get;set;}
            public void ShowMenu()=>Trace.Add("menu");public void SetMenuVisible(bool value)=>Trace.Add("visible:"+value);public void ShowCommonReward()=>Trace.Add("reward");
            public void InitializePrefabs()=>Trace.Add("prefabs");public void LoadScene(string name,Action complete,bool option){Require(name=="GamePlay"&&!option,"source scene arguments");Trace.Add("load");Loaded=complete;}
            public void LateInitializeModule()=>Trace.Add("late");public void SendLoadGameScreen()=>Trace.Add("screen");public int CurrentLevel=>11;
            public void ReportActivityEnter(string activity,string level)=>Trace.Add("enter:"+activity);
            public void InitializeLevelRank(){Trace.Add("rank");Ranking?.Invoke();}
            public void InitializeSevenDayActivity()=>throw new Exception("concrete registry route must replace host endpoint");
            public void SetPlayState(int state)=>Trace.Add("state:"+state);public void CloseLoading()=>Trace.Add("close");public void ReportGameInteractive(string message)=>Trace.Add("interactive");
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original SevendayActivityControl lifecycle, live limited-task graph, dates/red checks, scene-completion binding and concrete commander sum. Full Main/account/pages/platform and Player remain pending."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id="sevenday-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="sevenday-"+id,result="fail",detail=e.ToString()});}};
            check("registry-lifecycle-empty-init-and-retained-dispose",()=>{
                using(var f=new Fixture())
                {
                    var c=f.Control;c.OnInit();c.Updata(float.NaN,float.PositiveInfinity);Require(c.Child==null&&c.GetActDate(true)==0,"lifecycle alone does not bind activity or seed statistics");
                    c.EnterGameInit();long start=c.GetActDate(true);c.OnDispose();Require(!f.Registry.HasInstance(4502)&&ReferenceEquals(c.Child,f.Child)&&c.GetActDate(true)==start,"dispose clears singleton only");
                    var next=f.Control;Require(!ReferenceEquals(c,next)&&next.Child==null,"resolve allocates fresh controller");c.OnDispose();Require(!f.Registry.HasInstance(4502),"old instance disposal also clears newer singleton");
                }
            });
            check("scene-completion-seeds-real-task-statistics",()=>{
                using(var f=new Fixture())
                {
                    foreach(var state in f.Commanders.GetCommanderDatas())state.level=2;
                    var host=new EntryHost();var c=f.Control;var entry=new OutgameStartupEntry(host,()=>host.Trace.Add("migrate"),f.Registry);entry.Enter();
                    Require(c.Child==null&&host.EnterGame&&host.RedDotSourceFlag8,"seven-day binding waits for actual scene callback");
                    host.Ranking=()=>Require(c.Child==null,"rank initialized before seven-day binding");host.Loaded();
                    Require(ReferenceEquals(c.Child,f.Child)&&f.Child.FindTask(1301101).conditions[0].value==10,"scene completion binds actual child and seeds original cleared-level lifetime task");
                    Require(f.Tasks.Runtime.Statistics.Expansion.EventCount(f.Config.statisticEventConfig.CommanderUpgradeTotal)==f.Commanders.GetCommanderTotalLv(),"real commander manager sum seeded through controller");
                    Require(host.Trace.IndexOf("rank")<host.Trace.IndexOf("state:1")&&host.Trace[host.Trace.Count-1]=="interactive","play state/loading/interactive remain after seven-day initialization");
                }
            });
            check("strict-time-endpoints-caching-and-forced-enter-refresh",()=>{
                using(var f=new Fixture())
                {
                    f.Child.Data.LaunchTimeStamp=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,15,30,0));f.Control.EnterGameInit();
                    long start=f.Control.GetActDate(true),end=f.Control.GetActDate(false);
                    Require(start==OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3))&&end==start+7*86400000L,"midnight dates derive first over-condition target8 minus1");
                    f.Clock(start);Require(!f.Control.IsUnlock(),"start equality excluded");f.Clock(start+1);Require(f.Control.IsUnlock(),"strict interior included");f.Clock(end);Require(!f.Control.IsUnlock(),"end equality excluded");
                    f.Child.Data.LaunchTimeStamp+=86400000;f.Control.IsInActivity();Require(f.Control.GetActDate(true)==start,"nonnull cached start stays stale until explicit reentry");f.Control.EnterGameInit();Require(f.Control.GetActDate(true)==start+86400000,"entry recomputes dates");
                }
            });
            check("time-int32-truncation-multiplication-and-null-child-cache",()=>{
                using(var f=new Fixture())
                {
                    f.Child.Data.OverCondition[0].value=(1L<<32)+31;f.Child.Data.LaunchTimeStamp=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3));f.Control.EnterGameInit();
                    long start=f.Control.GetActDate(true);Require(f.Control.GetActDate(false)==start+unchecked(30*86400000),"target lowInt32 and multiplication overflow retained");
                    f.Child.Data.OverCondition[0].value=8;f.Control.EnterGameInit();f.Control.Child=null;f.Clock(start+1);
                    Require(f.Control.IsInActivity()&&!f.Control.IsUnlock(),"direct interval query retains cached dates after child removed; unlock gates null child");
                }
            });
            check("missing-parent-or-child-skips-seeding-but-retains-dates",()=>{
                using(var f=new Fixture())
                {
                    f.Control.EnterGameInit();long start=f.Control.GetActDate(true);f.Tasks.Runtime.Control.Activities.Remove(1301001);f.Tasks.Runtime.Control.ChildActivities.Remove(1301001);
                    f.Services.Skins=()=>throw new Exception("must skip skin access");f.Control.EnterGameInit();Require(f.Control.Child==null&&f.Control.GetActDate(true)==start,"missing parent assigns null then returns without clearing dates");
                }
            });
            check("seed-callback-order-live-config-and-failure-prefix",()=>{
                using(var f=new Fixture())
                {
                    int skinKey=f.Config.statisticEventConfig.HaveSkinNum;var trace=new List<string>();
                    f.Tasks.Runtime.Statistics.Common.AddListener(OutgameStatisticsMessageKey.Get(skinKey),a=>{trace.Add("skin");f.Level=16;f.Config.statisticEventConfig.CommanderUpgradeTotal=987;});
                    f.Tasks.Runtime.Statistics.Common.AddListener(OutgameStatisticsMessageKey.Get(10015),a=>trace.Add("level"));
                    f.Tasks.Runtime.Statistics.Common.AddListener(OutgameStatisticsMessageKey.Get(987),a=>trace.Add("commander"));
                    f.Control.EnterGameInit();Require(string.Join(",",trace)=="skin,level,commander"&&f.Child.FindTask(1301101).conditions[0].value==15,"callbacks change later source reads");
                    f.Services.Skins=()=>throw new InvalidOperationException("skin failure");long before=f.Control.GetActDate(true);f.Child.Data.LaunchTimeStamp+=86400000;
                    Throws<InvalidOperationException>(f.Control.EnterGameInit);Require(f.Control.GetActDate(true)==before+86400000&&trace.Count==3,"date publication precedes first seed failure");
                }
            });
            check("completed-level-wrap-and-commander-signed-sum",()=>{
                using(var f=new Fixture())
                {
                    foreach(var state in f.Commanders.GetCommanderDatas())state.level=0;
                    var iterator=f.Commanders.GetCommanderDatas().GetEnumerator();iterator.MoveNext();iterator.Current.level=int.MaxValue;iterator.MoveNext();iterator.Current.level=2;iterator.Dispose();
                    Require(f.Commanders.GetCommanderTotalLv()==int.MinValue+1,"signed unchecked sum across actual held dictionary");
                    f.Level=int.MinValue;f.Control.EnterGameInit();Require(f.Tasks.Runtime.Statistics.Expansion.EventCount(10015)==int.MaxValue,"level-minus1 wraps before nonnegative clamp");
                    f.Level=0;f.Control.EnterGameInit();Require(f.Tasks.Runtime.Statistics.Expansion.EventCount(10015)==0,"ordinary nonpositive cleared count clamps to0");
                }
            });
            check("red-query-both-paths-and-exact-unclaimed-state",()=>{
                using(var f=new Fixture())
                {
                    f.Control.EnterGameInit();f.Clock(f.Control.GetActDate(true)+1);Require(f.Control.IsDayHaveRed(1),"seeded day1 task is claimable");
                    var acc=f.Child.AccRewards[0];acc.Config.accValue=0;acc.state=2;Require(!f.Control.IsAccHaveRed(),"red only state0, unlike accumulator CanComplete allowing state2");
                    acc.state=0;Require(f.Control.IsAccHaveRed(),"target satisfied and state0 red");
                    f.Child.AccRewards=null;Throws<NullReferenceException>(()=>f.Control.IsHaveAnyRed());Require(f.Control.IsDayHaveRed(1),"acc query is evaluated despite already true day red");
                    f.Clock(f.Control.GetActDate(false));Require(!f.Control.IsHaveAnyRed(),"locked date short-circuits both red paths");
                    f.Child.AccRewards=new List<OutgameNoviceAccRewardItemData>();
                }
            });
            check("red-day-bound-uses-condition-not-map-maximum",()=>{
                using(var f=new Fixture())
                {
                    f.Control.EnterGameInit();f.Child.Data.OverCondition[0].value=1;Require(!f.Control.IsAnyDayHaveRed(),"zero configured duration skips all days even with claimable task");
                    f.Child.Data.OverCondition[0].value=2;Require(f.Control.IsAnyDayHaveRed(),"first day visited");
                    foreach(var list in f.Child.DayTasks.Values)foreach(var task in list)task.state=1;
                    f.Child.Data.OverCondition[0].value=9;Throws<NullReferenceException>(()=>f.Control.IsAnyDayHaveRed());
                }
            });
            return report;
        }
    }
}
