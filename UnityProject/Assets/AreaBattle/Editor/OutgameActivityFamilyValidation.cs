using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityFamilyValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action run)where T:Exception{try{run();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Child:OutgameChildActivity<string>
        {
            public Action<Child,int> Refreshing;public int Refreshes;
            public Child():base(null){}
            public override void Refresh(int id){Refreshes++;ActivityId=id;Refreshing?.Invoke(this,id);}
        }
        sealed class Father:OutgameFatherActivity<Child,string>
        {
            public Action Sorting;public Father(OutgameActivityFamilyServices<Child> f):base(null,f){ActivityId=7;}
            public override void SortDataDic()=>Sorting?.Invoke();
        }
        sealed class FamilyFixture
        {
            public readonly OutgameActivityConfigManager Config=new OutgameActivityConfigManager(null);
            public readonly OutgameActivityControl Control=new OutgameActivityControl();
            public readonly Dictionary<object,object> Configs=new Dictionary<object,object>();
            public readonly Dictionary<int,string> Data=new Dictionary<int,string>();
            public readonly List<Child> Made=new List<Child>();
            public readonly Father Parent;public readonly OutgameActivityFamilyServices<Child> Services;
            public FamilyFixture()
            {
                Services=new OutgameActivityFamilyServices<Child>{Config=()=>Config,Control=()=>Control,CreateChild=()=>{
                    var c=new Child{Refreshing=(child,id)=>{Require(child.ModuleData==Data[id],"module assigned before Refresh");child.Data=new OutgameActivityItemData{id=id,state=3};}};Made.Add(c);return c;}};
                Parent=new Father(Services);
            }
            public void Add(int id,int[] parents,bool data=true){Config.Activities[id]=new OutgameActivityConfigRow{id=id,parentActivityID=parents};Configs[id]=null;if(data)Data[id]="module-"+id;}
            public void Reset()=>Parent.ResetChildActivity(Configs,Data);
        }
        sealed class Activity:IOutgameNoviceTaskActivity
        {public Action Refreshing,Saving;public int Refreshes;public void RefreshData(){Refreshes++;Refreshing?.Invoke();}public void OnSave()=>Saving?.Invoke();}
        sealed class ManagerFixture:IDisposable
        {
            public readonly OutgameActivityControlValidation.Fixture Runtime;
            public readonly OutgameNoviceTaskManager Manager;public readonly OutgameNoviceTaskManagerServices Services;
            public readonly List<string> Warnings=new List<string>();public readonly List<object[]> Errors=new List<object[]>();public int Downloads;
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public ManagerFixture(string path=null,bool original=false)
            {
                Runtime=new OutgameActivityControlValidation.Fixture(path,originalConfig:original);Runtime.Init();
                Services=new OutgameNoviceTaskManagerServices{Config=()=>Config,Warning=Warnings.Add,Error=Errors.Add};
                var stats=Runtime.Statistics;
                var storage=new OutgameDataManagerStorage(()=>"CommonGameModuleNoviceTaskManager",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
                Manager=new OutgameNoviceTaskManager(storage,stats.StorageHost,k=>{Require(k=="CommonGameModuleNoviceTaskManager","original download key");Downloads++;},Services);
            }
            public void Only(params int[] ids)
            {
                Config.NoviceActivities.Clear();Config.NoviceTasksByActivity.Clear();
                foreach(int id in ids){Config.NoviceActivities[id]=new PubNTActivityConfig{id=id};Config.NoviceTasksByActivity[id]=new Dictionary<int,PubnoviceTaskConfig>{{id,new PubnoviceTaskConfig{id=id,activityId=id,day=5}}};}
            }
            public string Stored=>Runtime.Statistics.Strings.GetString(Runtime.Statistics.StorageHost.MineGameName+Manager.DataKey,"");
            public void Dispose()=>Runtime.Dispose();
        }
        static OutgameLimitTimeTaskChildData Row(int id,int day=1)=>new OutgameLimitTimeTaskChildData{ext=new OutgameLimitTimeTaskExt{activityId=id,dayId=day}};
        static string Json(params OutgameLimitTimeTaskChildData[] rows)=>JsonUtility.ToJson(new OutgameLimitTimeTaskData{datas=new List<OutgameLimitTimeTaskChildData>(rows)});
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Shared father/child activity behavior and NoviceTaskManager source storage/reconciliation with real config, pool, files and base FSM. Concrete LimitTimeTaskActivity endpoints use explicit fixtures; task rewards/pages/SevenDay/Main still pending. No fresh native or Player claim."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-family-parent-match-and-discarded-other-construction",()=>{
                var f=new FamilyFixture();f.Add(11,new[]{7,7});f.Add(12,new[]{8});f.Add(13,null);f.Add(14,Array.Empty<int>(),false);f.Add(15,new[]{8},false);
                f.Config.Activities[16]=new OutgameActivityConfigRow{id=16};f.Reset();
                Require(f.Made.Count==5&&f.Parent.Children.Count==1&&f.Control.ChildActivities.Count==3,"first matching parent only, unmarked config skipped, nonmatching parents construct even without data");
                Require(f.Made[1].Refreshes==0&&f.Made[2].ActivityId==12&&f.Parent.OtherViewActivities.Count==2,"nonmatching instance discarded before independent other-view construction");
                Require(f.Parent.SelfViewDatas[11]=="module-11"&&f.Parent.OtherViewDatas.Count==2,"only initialized state3 modules enter views");
            });
            check("activity-family-view-data-retention-and-sort-boundary",()=>{
                var f=new FamilyFixture();f.Add(11,new[]{7});f.Add(12,null);f.Parent.SelfViewDatas[99]="old-self";f.Parent.OtherViewDatas[98]="old-other";
                f.Parent.Sorting=()=>{Require(f.Parent.SelfViewDatas.ContainsKey(11)&&!f.Parent.OtherViewDatas.ContainsKey(12),"sort after self data but before other data");f.Parent.OtherViewActivities[12].Data.state=4;};
                var old=f.Parent.SelfViewActivities;f.Reset();
                Require(!ReferenceEquals(old,f.Parent.SelfViewActivities)&&f.Parent.SelfViewDatas.Count==2&&f.Parent.OtherViewDatas.Count==1,"activity maps replaced, old data maps retained, state read after sort");
            });
            check("activity-family-repeated-reset-add-failure-prefix",()=>{
                var f=new FamilyFixture();f.Add(11,new[]{7});f.Reset();var original=f.Parent.Children[0];
                Throws<ArgumentException>(f.Reset);
                Require(f.Parent.Children.Count==2&&ReferenceEquals(f.Parent.ChildrenById[11],original)&&f.Parent.SelfViewActivities.Count==0&&f.Parent.SelfViewDatas.Count==1,"new child appended before old parent map rejects duplicate; new view maps already published");
            });
            check("activity-family-global-collision-and-missing-module-prefix",()=>{
                var f=new FamilyFixture();f.Add(11,new[]{7});f.Control.ChildActivities[11]=new Child();Throws<ArgumentException>(f.Reset);
                Require(f.Parent.ChildrenById.Count==1&&f.Parent.SelfViewActivities.Count==1&&f.Made[0].Refreshes==0,"parent maps precede global Add, module/refresh not reached");
                var missing=new FamilyFixture();missing.Add(11,new[]{7},false);Throws<NullReferenceException>(missing.Reset);
                Require(missing.Control.ChildActivities.Count==1&&missing.Parent.Children.Count==1&&missing.Made[0].Refreshes==0,"missing saved module retains attached child then fails when filtering absent activity Data");
            });
            check("activity-family-null-factory-and-sort-failure-persistence",()=>{
                var f=new FamilyFixture();f.Add(11,null);f.Services.CreateChild=()=>null;Throws<NullReferenceException>(f.Reset);
                Require(f.Control.ChildActivities.ContainsKey(11)&&f.Parent.OtherViewActivities.ContainsKey(11),"null registered before EarlyOnInit dereference");
                var other=new FamilyFixture();other.Add(11,new[]{7});other.Add(12,null);other.Parent.Sorting=()=>throw new InvalidOperationException("sort");Throws<InvalidOperationException>(other.Reset);
                Require(other.Parent.SelfViewDatas.Count==1&&other.Parent.OtherViewDatas.Count==0&&other.Control.ChildActivities.Count==2,"sort failure retains registration/self filtering but precedes other data filtering");
            });
            check("activity-family-real-child-refresh-fsm-and-owner-registration",()=>{
                using(var f=new OutgameActivityControlValidation.Fixture())
                {
                    f.Init();var cfg=OutgameActivityControlValidation.NewRow(11);cfg.parentActivityID=new[]{7};f.Config.Manager.Activities[11]=cfg;
                    f.Control.ItemDatas[11]=new OutgameActivityItemData{id=11,state=3};f.Set(702,1);
                    var parent=new OutgameFatherActivity<OutgameChildActivity<string>,string>(f.Activity,new OutgameActivityFamilyServices<OutgameChildActivity<string>>{Config=()=>f.Config.Manager,Control=()=>f.Control,CreateChild=()=>new OutgameChildActivity<string>(f.Activity)}){ActivityId=7};
                    parent.ResetChildActivity(new Dictionary<object,object>{{11,null}},new Dictionary<int,string>{{11,"live"}});
                    var child=parent.GetChildActivity<OutgameChildActivity<string>>(11);
                    Require(child.Fsm!=null&&child.Data.state==3&&child.ModuleData=="live"&&ReferenceEquals(f.Control.ChildActivities[11],child)&&parent.SelfViewDatas[11]=="live","generic family uses actual base initialization/FSM and shared owner index");
                    parent.OnDispose();Require(parent.Children.Count==0&&parent.ChildrenById.Count==0&&parent.SelfViewActivities.Count==1,"base disposal clears child ownership but family Dispose leaves view dictionaries");
                }
            });
            check("activity-family-novice-original-config-pool-and-new-data",()=>{
                using(var f=new ManagerFixture(original:true))
                {
                    f.Runtime.Statistics.Pool.AddModel(4711,f.Manager,true);
                    Require(f.Config.NoviceTasks.Count==49&&f.Manager.Data.datas.Count==f.Config.NoviceTasksByActivity.Count&&f.Manager.LimitTimeTaskDic.Count==f.Manager.Data.datas.Count,"original49 task configs and actual pool-triggered OnInit");
                    foreach(var child in f.Manager.Data.datas)Require(child.ext.dayId==1&&child.ext.lastClickDayId==f.Config.NoviceTasksByActivity[child.ext.activityId].First().Value.day&&child.datas.Count==0,"source creates only module/ext; concrete child later populates tasks");
                    Require(f.Stored==""&&f.Manager.ActivityId==1301001&&f.Manager.ParticipatesInSync&&!f.Manager.CompressData,"initial load does not save; exact ID/registration flags");
                }
            });
            check("activity-family-novice-compressed-file-restart-all-persisted-fields",()=>{
                string path;using(var f=new ManagerFixture())
                {
                    f.Only(11);f.Manager.OnInit();path=f.Runtime.Statistics.PathName;var child=f.Manager.Data.datas[0];
                    child.ext.dayId=7;child.ext.dayFlag=long.MinValue;child.ext.accFlag=long.MaxValue;child.ext.launchTime=-33;child.ext.lastClickDayId=4;
                    child.datas.Add(new OutgameLimitTaskItemData{id=17,state=2,conditions=new List<OutgameLimitTaskCondition>{new OutgameLimitTaskCondition{key=3,value=long.MaxValue,arg=new[]{-1,4}}}});
                    f.Manager.OnSave();Require(f.Stored.Length>0&&!f.Stored.StartsWith("{"),"source gzip inside noncompressed manager storage");
                }
                using(var f=new ManagerFixture(path))
                {
                    f.Only(11);f.Manager.OnInit();var child=f.Manager.Data.datas[0];var task=child.datas[0];
                    Require(child.ext.dayId==7&&child.ext.dayFlag==long.MinValue&&child.ext.accFlag==long.MaxValue&&child.ext.launchTime==-33&&child.ext.lastClickDayId==4,"all original ext fields preserved after fresh owner/backend");
                    Require(task.id==17&&task.state==2&&task.conditions[0].key==3&&task.conditions[0].value==long.MaxValue&&task.conditions[0].arg.SequenceEqual(new[]{-1,4}),"task state and condition payload remain exact");
                }
            });
            check("activity-family-novice-orphans-duplicates-and-first-config-day",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11,12);f.Config.NoviceTasksByActivity[12].Add(1,new PubnoviceTaskConfig{day=1});
                    var activity=new Activity();f.Manager.Activity=activity;f.Manager.UpdateDataCallBack(Json(Row(99),Row(98),Row(11,3),Row(11,6)));
                    Require(f.Errors.Count==2&&f.Manager.Data.datas.Count==3&&f.Manager.Data.datas[0].ext.dayId==3&&f.Manager.Data.datas[1].ext.dayId==6,"adjacent orphans both removed, existing duplicates preserved");
                    Require(f.Manager.LimitTimeTaskDic[11].ext.dayId==6&&f.Manager.LimitTimeTaskDic[12].ext.lastClickDayId==5&&activity.Refreshes==1,"last duplicate indexes, insertion-first config day not minimum, one refresh after complete indexing");
                }
            });
            check("activity-family-novice-empty-config-and-raw-parse-fallback",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11);f.Config.NoviceTasksByActivity[11].Clear();f.Manager.OnInit();Require(f.Errors.Count==1&&f.Manager.Data.datas[0].ext.lastClickDayId==0,"empty task group still creates child after diagnostic");
                    var held=f.Manager.Data;Throws<ArgumentException>(()=>f.Manager.UpdateDataCallBack("not-json"));Require(ReferenceEquals(held,f.Manager.Data),"uncaught second parse failure preserves prior data");
                    f.Services.Warning=s=>throw new InvalidOperationException("warning callback");f.Manager.UpdateDataCallBack(Json(Row(11,9)));
                    Require(f.Manager.Data.datas[0].ext.dayId==9,"decompression warning failure is caught by manager then raw JSON parsed");
                }
            });
            check("activity-family-novice-server-deferred-and-sync-disabled-local",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11);f.Runtime.Statistics.StorageHost.Server=true;f.Manager.OnInit();Require(f.Downloads==1&&f.Manager.Data==null,"no fabricated server completion");
                    f.Manager.UpdateDataCallBack(Json(Row(11,6)));Require(f.Manager.LimitTimeTaskDic[11].ext.dayId==6,"actual callback publishes/indexes remote data");
                    f.Manager.ParticipatesInSync=false;f.Manager.OnInit();Require(f.Downloads==1&&f.Manager.Data.datas[0].ext.dayId==1,"disabled sync takes local empty storage path");
                }
            });
            check("activity-family-novice-reconciliation-and-index-failure-prefix",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11);f.Manager.OnInit();var indexed=f.Manager.LimitTimeTaskDic[11];f.Services.Error=a=>throw new InvalidOperationException("diagnostic sink");
                    Throws<InvalidOperationException>(()=>f.Manager.UpdateDataCallBack(Json(Row(99),Row(11,8))));
                    Require(f.Manager.Data.datas[0].ext.activityId==99&&ReferenceEquals(f.Manager.LimitTimeTaskDic[11],indexed),"parsed data published before reconciliation failure; prior index unchanged");
                    f.Manager.Data=new OutgameLimitTimeTaskData{datas=new List<OutgameLimitTimeTaskChildData>{Row(12),null}};
                    Throws<NullReferenceException>(f.Manager.UpdateCallback);Require(f.Manager.LimitTimeTaskDic.Count==1&&f.Manager.LimitTimeTaskDic.ContainsKey(12),"index clear and prefix survive malformed later row");
                }
            });
            check("activity-family-novice-current-config-terminal-read-and-live-data",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11);int reads=0;var second=new OutgameActivityConfigManager(new OutgameActivityConfigManagerServices{Error=f.Errors.Add});
                    second.NoviceTasksByActivity[12]=new Dictionary<int,PubnoviceTaskConfig>{{12,new PubnoviceTaskConfig{day=4}}};
                    f.Services.Config=()=>++reads==1?f.Config:second;
                    f.Manager.UpdateDataCallBack(Json(Row(11)));Require(reads==3&&f.Manager.LimitTimeTaskDic.ContainsKey(12)&&f.Manager.Data.datas[1].ext.lastClickDayId==4,"terminal removal-loop lookup supplies add-group enumeration; task getter resolves current again");
                    f.Services.Config=()=>f.Config;f.Config.NoviceTasksByActivity[11].Clear();f.Manager.Data=null;
                    var replacement=new OutgameLimitTimeTaskData();f.Services.Error=a=>f.Manager.Data=replacement;f.Manager.UpdateDataCallBack("");
                    Require(ReferenceEquals(replacement,f.Manager.Data)&&replacement.datas.Count==1,"diagnostic can replace published data before new child appended");
                }
            });
            check("activity-family-novice-refresh-save-callback-order-and-release",()=>{
                using(var f=new ManagerFixture())
                {
                    f.Only(11);var activity=new Activity();f.Manager.Activity=activity;
                    activity.Refreshing=()=>{Require(f.Manager.LimitTimeTaskDic.ContainsKey(11),"index ready before concrete refresh");throw new InvalidOperationException("refresh");};
                    Throws<InvalidOperationException>(f.Manager.OnInit);Require(f.Manager.Data!=null&&f.Manager.LimitTimeTaskDic.Count==1,"refresh failure leaves loaded/indexed data");
                    var replacement=new OutgameLimitTimeTaskData{datas=new List<OutgameLimitTimeTaskChildData>{Row(11,8)}};activity.Saving=()=>f.Manager.Data=replacement;f.Manager.OnSave();
                    Require(JsonUtility.FromJson<OutgameLimitTimeTaskData>(OutgameActivityCodec.DecompressString(f.Stored,s=>{})).datas[0].ext.dayId==8,"save serializes live data after concrete activity OnSave");
                    string old=f.Stored;activity.Saving=()=>throw new InvalidOperationException("save");Throws<InvalidOperationException>(f.Manager.OnSave);Require(f.Stored==old,"failed activity save prevents storage write");
                    f.Manager.OnRelease();f.Manager.InitStrategy();Require(ReferenceEquals(f.Manager.Activity,activity)&&ReferenceEquals(f.Manager.Data,replacement)&&f.Manager.LimitTimeTaskDic.Count==1,"source release and strategy are empty");
                }
            });
            return report;
        }
    }
}
