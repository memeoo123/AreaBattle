using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.ActivityConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityBusinessValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Binary:IOutgameActivityBinaryConfigReader
        {public Action<Type,object> Read;public void ReadTable<T>(Dictionary<object,T> rows)where T:class,IOutgameConfigRow=>Read(typeof(T),rows);}
        sealed class Fixture
        {
            public readonly List<object[]> Errors=new List<object[]>(),Warnings=new List<object[]>();public readonly List<string> Reads=new List<string>();
            public readonly OutgameActivityConfigRuntime Runtime;public OutgameActivityConfigManager Manager=>Runtime.Manager;
            public readonly OutgameLegacyConfigRead Json;public readonly OutgameActivityConfigUnityJson CommonJson;
            public readonly OutgameActivityCustomConfigServices Custom;public readonly OutgameActivityConfigReaderServices Reader;
            public bool UseBinary;public Func<string,TextAsset> Load;public int Completed;
            public Fixture()
            {
                Load=name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name);
                Json=new OutgameLegacyConfigRead(name=>{Reads.Add(name);return Load(name);},s=>Errors.Add(new object[]{s}));
                CommonJson=new OutgameActivityConfigUnityJson(name=>{Reads.Add(name);return Load(name);},s=>Errors.Add(new object[]{s}));
                Custom=new OutgameActivityCustomConfigServices{UseBinary=()=>UseBinary,Json=Json};
                Reader=new OutgameActivityConfigReaderServices{OnlineParameter=key=>null,UseBinary=()=>UseBinary,ReadLocalActivities=CommonJson.ReadActivities,ReadLocalSettings=CommonJson.ReadSettings,Error=Errors.Add};
                Runtime=new OutgameActivityConfigRuntime(Reader,Custom,Warnings.Add);
            }
            public void Read()=>Runtime.Read(()=>Completed++);
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Complete source common/novice/task/achievement config roster and actual original JSON resources, schema-backed queries/groups/release. Main/ActivityControl/concrete activities/UI and platform/binary implementation remain pending; no new native or Player."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-business-original-all-configs-and-production-roster",()=>{
                var f=new Fixture();f.Read();var m=f.Manager;
                Require(m.CustomManagers.Count==3&&m.Expected==5&&m.Completed==5&&f.Completed==1,"all three original config managers complete before common readiness");
                Require(string.Join(",",f.Reads)=="PubnoviceTaskConfig,PubnoviceAccRewardConfig,PubNTActivityConfig,PubTaskConfig,PubdailyLivenessConfig,PubTaskGroupConfig,PubTaskActivityConfig,PubAchievementConfig,PubAchievementAccConfig,PubActivityConfig,PubActivitySettingConfig","source metadata roster and exact per-loader order");
                Require(m.NoviceTasks.Count==49&&m.NoviceAcc.Count==8&&m.NoviceActivities.Count==1&&m.Tasks.Count==8&&m.TaskGroups.Count==1&&m.TaskActivities.Count==1&&m.Liveness.Count==3&&m.Achievements.Count==127&&m.AchievementAcc.Count==3&&m.Activities.Count==9,"all original table row counts");
                Require(m.GetNTTaskConfigDicByActivityId(1301).Count==49&&m.GetNTAccRewardsDicByActivityId(1301).Count==8&&m.GetLivenessDicByActivityId(130001).Count==3&&m.TasksByGroup[10001].Count==8&&m.TaskGroupsByActivity[130001].Count==1,"original group graph");
                Require(m.AchievementGroups[1][new OutgameActivityConditionPriority(10015,0)][0].priority==1&&m.GetAchievementConfig(1).number==30&&m.GetAchievementConfig(1).rewards[0].datas[0]==1001,"metadata long/nested reward schema and source condition key");
                Require(f.Json.ReadCount==9&&f.CommonJson.ReadCount==1&&f.Errors.Count==0,"successful table reads and diagnostics");
            });
            check("activity-business-query-null-vs-missing-diagnostics",()=>{
                var f=new Fixture();var m=f.Manager;m.Achievements[1]=null;m.AchievementAcc[1]=null;m.Liveness[1]=null;m.TaskActivities[1]=null;m.NoviceActivities[1]=null;m.NoviceTasks[1]=null;m.NoviceAcc[1]=null;
                m.GetAchievementConfig(1);m.GetAchievementAccConfig(1);m.GetLivenessConfig(1);m.GetTaskActivityConfig(1);m.GetLimitTaskActivityConfig(1);Require(f.Errors.Count==0,"found-null entries silent on existence-based getters");
                m.GetNoviceTaskConfig(1);m.GetNoviceAccRwardConfig(1);Require(f.Errors.Count==2,"novice row getters log found-null");
                m.GetTaskConfig(99);m.GetTaskGroupConfig(99);Require(f.Errors.Count==2,"task/group misses silent");m.GetAchievementConfig(99);m.GetAchievementAccConfig(99);m.GetTaskActivityConfig(99);
                Require(Equals(f.Errors[2][0],"成就表表不包含ID:")&&Equals(f.Errors[3][0],"活跃度表不包含ID:")&&Equals(f.Errors[4][1],99),"original diagnostic wording including copied achievement-acc message");
                m.NoviceTasksByActivity[7]=null;m.NoviceAccByActivity[7]=null;m.LivenessByActivity[7]=null;int count=f.Errors.Count;
                Require(m.GetNTTaskConfigDicByActivityId(7)==null&&m.GetNTAccRewardsDicByActivityId(7)==null&&m.GetLivenessDicByActivityId(7)==null&&f.Errors.Count==count&&f.Warnings.Count==0,"found-null groups silent");
                m.GetNTTaskConfigDicByActivityId(8);m.GetNTAccRewardsDicByActivityId(8);m.GetLivenessDicByActivityId(8);Require(f.Errors.Count==count+2&&f.Warnings.Count==1&&Equals(f.Warnings[0][0],"活跃度配置表中不包含活动id为8的配置"),"group misses return null and preserve warning/error distinction");
            });
            check("activity-business-groups-live-aliases-duplicates-and-empty-novice",()=>{
                var m=new Fixture().Manager;var task=new PubTaskConfig{id=1,taskGroupId=3};var group=new PubTaskGroupConfig{Id=1,activityId=4};m.Tasks[90]=task;m.TaskGroups[91]=group;m.BuildTasks();m.BuildTaskGroups();var held=m.TasksByGroup[3];m.BuildTasks();m.BuildTaskGroups();
                Require(ReferenceEquals(held,m.TasksByGroup[3])&&held.Count==2&&ReferenceEquals(held[0],task)&&m.TaskGroupsByActivity[4].Count==2,"task/list grouping appends shared rows on rebuild");
                m.NoviceActivities[99]=new PubNTActivityConfig{id=8};m.BuildNoviceTasks();Require(m.NoviceTasksByActivity[8].Count==0&&!m.NoviceAccByActivity.ContainsKey(8),"novice activities get empty task groups only");
                m.Liveness[1]=new PubdailyLivenessConfig{id=5,activityID=4};m.Liveness[2]=new PubdailyLivenessConfig{id=5,activityID=4};Throws<ArgumentException>(m.BuildLiveness);Require(m.LivenessByActivity[4].Count==1,"duplicate row ID throws in inner dictionary, retains earlier row");
                m.NoviceTasks[1]=new PubnoviceTaskConfig{id=1,activityId=8};m.NoviceTasks[2]=null;m.NoviceActivities[100]=new PubNTActivityConfig{id=9};Throws<NullReferenceException>(m.BuildNoviceTasks);Require(m.NoviceTasksByActivity[8].Count==1&&!m.NoviceTasksByActivity.ContainsKey(9),"task failure prevents second empty-group pass");
            });
            check("activity-business-achievement-condition-value-key-and-priority-order",()=>{
                var f=new Fixture();f.Load=name=>new TextAsset("{\"Datas\":[]}");var m=f.Manager;
                var high=new PubAchievementConfig{id=1,type=2,contentType=3,content=4,priority=int.MaxValue};var low=new PubAchievementConfig{id=2,type=2,contentType=3,content=4,priority=int.MinValue};
                m.Achievements[1]=high;m.Achievements[2]=low;m.Achievements[3]=new PubAchievementConfig{id=3,type=2,contentType=3,content=5,priority=9};m.AchievementAcc[4]=new PubAchievementAccConfig{id=4};
                var loader=new OutgameAchievementConfigManager(f.Custom);int done=0;loader.OnInit(()=>done++);var rows=m.AchievementGroups[2][new OutgameActivityConditionPriority(3,4)];
                Require(rows.Count==2&&ReferenceEquals(rows[0],low)&&ReferenceEquals(rows[1],high)&&m.AchievementGroups[2].Count==2&&done==1,"value-type condition keys group content type/value; signed priorities compare without subtraction overflow");
                loader.OnInit(()=>done++);Require(rows.Count==4&&m.AchievementAccList.Count==2&&done==2,"repeat appends entries before sorting; accumulator list also appends");
                m.Achievements[5]=null;Throws<NullReferenceException>(()=>loader.OnInit(()=>done++));Require(done==2&&rows.Count==6&&m.AchievementAccList.Count==2,"failed grouping leaves append prefix, skips accumulator and completion");
            });
            check("activity-business-loader-captures-binary-once-and-achievement-always-json",()=>{
                var f=new Fixture{UseBinary=true};var types=new List<Type>();f.Custom.Binary=new Binary{Read=(t,rows)=>{types.Add(t);f.UseBinary=false;}};
                new OutgameNoviceTaskConfigManager(f.Custom).OnInit(null);Require(types.Count==3&&f.Reads.Count==0,"novice reads all three with captured binary choice despite flag mutation");
                f.UseBinary=true;types.Clear();new OutgameTaskConfigManager(f.Custom).OnInit(null);Require(types.Count==4&&f.Reads.Count==0&&f.Manager.LivenessList.Count==0,"task captures mode for all four tables and publishes list");
                f.Custom.UseBinary=()=>throw new Exception("achievement must not query binary flag");new OutgameAchievementConfigManager(f.Custom).OnInit(null);Require(f.Reads.Count==2&&f.Manager.Achievements.Count==127,"achievement ignores binary mode and uses original JSON unconditionally");
            });
            check("activity-business-loader-current-owner-between-read-calls",()=>{
                var a=new Fixture();var b=new Fixture();OutgameActivityConfigManager current=a.Manager;a.Custom.Current=()=>current;int reads=0;
                a.Custom.Json=new OutgameLegacyConfigRead(name=>{reads++;if(reads==1)current=b.Manager;return Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name);},s=>{});
                new OutgameNoviceTaskConfigManager(a.Custom).OnInit(null);
                Require(a.Manager.NoviceTasks.Count==49&&a.Manager.NoviceAcc.Count==0&&b.Manager.NoviceTasks.Count==0&&b.Manager.NoviceAcc.Count==8&&b.Manager.NoviceTasksByActivity[1301].Count==0&&b.Manager.NoviceAccList.Count==8,"first destination captured before read; subsequent tables/groups/list use current owner");
            });
            check("activity-business-complete-runtime-release-and-reread-stale-task-index",()=>{
                var f=new Fixture();f.Read();var m=f.Manager;var tasks=m.TasksByGroup[10001];var first=tasks[0];var liveness=m.LivenessList;var settings=m.Settings;
                f.Runtime.Dispose();Require(m.Activities.Count==0&&m.CustomManagers.Count==0&&m.Achievements.Count==0&&m.NoviceTasks.Count==0&&m.TaskGroups.Count==0&&m.LivenessByActivity.Count==0&&liveness.Count==0&&tasks.Count==8&&ReferenceEquals(settings,m.Settings),"release clears source tables/lists except retained task group index/settings");
                f.Read();Require(f.Completed==2&&m.Completed==5&&m.TasksByGroup[10001].Count==16&&ReferenceEquals(first,m.TasksByGroup[10001][0])&&!ReferenceEquals(first,m.TasksByGroup[10001][8])&&!ReferenceEquals(liveness,m.LivenessList),"reread after disposal retains stale task rows and appends newly parsed rows; liveness list replaced");
            });
            check("activity-business-initialization-failure-prefix-and-repeat-without-dispose",()=>{
                var f=new Fixture();f.Read();int count=f.Errors.Count;var held=f.Manager.NoviceAccList;Throws<ArgumentException>(f.Read);
                Require(f.Manager.CustomManagers.Count==6&&f.Manager.Expected==11&&f.Manager.Completed==5&&f.Completed==1&&f.Errors.Count-count==58&&held.Count==8,"second Read appends three loaders, adds full list count, then duplicate novice grouping aborts before readiness");
                var g=new Fixture();g.Load=name=>name=="PubdailyLivenessConfig"?throw new InvalidOperationException("load"):Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name);Throws<InvalidOperationException>(g.Read);
                Require(g.Manager.NoviceTasks.Count==49&&g.Manager.NoviceAccList.Count==8&&g.Manager.Tasks.Count==8&&g.Manager.LivenessList==null&&g.Manager.Completed==1&&g.Manager.Activities.Count==0,"novice completes, first task table remains; later failure prevents list/common reads");
            });
            check("activity-business-dispose-failure-order-and-current-target",()=>{
                var a=new Fixture();var b=new Fixture();a.Manager.Tasks[1]=new PubTaskConfig();b.Manager.Tasks[1]=new PubTaskConfig();a.Custom.Current=()=>b.Manager;
                Throws<NullReferenceException>(()=>new OutgameTaskConfigManager(a.Custom).OnDispose());Require(a.Manager.Tasks.Count==1&&b.Manager.Tasks.Count==0,"dispose queries current owner and throws on never-initialized liveness list after clearing first three tables");
                var f=new Fixture();f.Read();f.Manager.NoviceAcc=null;Throws<NullReferenceException>(f.Runtime.Dispose);Require(f.Manager.Activities.Count==0&&f.Manager.NoviceTasks.Count==0&&f.Manager.NoviceTasksByActivity.Count==1&&f.Manager.Tasks.Count==8&&f.Manager.Completed==5&&f.Manager.CustomManagers.Count==3,"custom dispose failure stops later clears/managers/counter reset");
            });
            check("activity-business-source-schema-signed64-and-shared-references",()=>{
                var f=new Fixture();f.Load=name=>new TextAsset(name=="PubAchievementConfig"?"{\"Datas\":[{\"id\":1,\"type\":1,\"contentType\":7,\"content\":9,\"number\":9223372036854775807,\"showCondition\":[{\"datas\":[1,2]}],\"rewards\":[{\"datas\":[3,4]}]}]}":"{\"Datas\":[]}");
                new OutgameAchievementConfigManager(f.Custom).OnInit(null);var row=f.Manager.GetAchievementConfig(1);Require(row.number==long.MaxValue&&row.showCondition[0].datas[1]==2,"source Int64 and nested list values retained");
                row.rewards[0].datas[1]=99;Require(f.Manager.AchievementGroups[1][new OutgameActivityConditionPriority(7,9)][0].rewards[0].datas[1]==99,"group/query share actual config rows and nested reward references");
                Require(typeof(PubAchievementAccConfig).GetField("rewards")==null,"original shared achievement accumulator metadata only has id; project JSON reward fields are not invented into schema");
            });
            return report;
        }
    }
}
