using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.ActivityConfig;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskManagerValidation
    {
        sealed class Activity:IOutgameTaskActivity
        {
            public int Refreshes,Saves;public OutgameTaskData Last;public Action Saving;
            public void RefreshData(OutgameTaskData data){Last=data;Refreshes++;}
            public void SaveData(){Saves++;Saving?.Invoke();}
        }
        sealed class Fixture:IDisposable
        {
            readonly OutgameTaskModelServices previous=OutgameTaskModels.Services;
            public readonly OutgameActivityControlValidation.Fixture Runtime;
            public readonly OutgameTaskManager Manager;public readonly OutgameTaskManagerServices Services;
            public readonly List<object[]> Errors=new List<object[]>(),Logs=new List<object[]>();
            public Activity Current=new Activity();public int Downloads,Lookups;
            public OutgameActivityConfigManager Config=>Runtime.Config.Manager;
            public Fixture(string path=null)
            {
                Runtime=new OutgameActivityControlValidation.Fixture(path,originalConfig:true);Runtime.Init();
                OutgameTaskModels.Services=new OutgameTaskModelServices{Config=()=>Config,LanguageFormat=(key,args)=>key};
                Services=new OutgameTaskManagerServices{Config=()=>Config,Pool=()=>Runtime.Statistics.Pool,GetActivity=id=>{Require(id==1300001,"source activity id");Lookups++;return Current;},Warning=s=>{},Error=Errors.Add,Log=Logs.Add};
                var stats=Runtime.Statistics;
                var storage=new OutgameDataManagerStorage(()=>"CommonGameModuleTaskMgr",stats.StorageHost,stats.Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
                Manager=new OutgameTaskManager(storage,stats.StorageHost,k=>{Require(k=="CommonGameModuleTaskMgr","source download key");Downloads++;},Services);
                stats.Pool.AddModel(4692,Manager,true);
            }
            public void Only(params int[] ids)
            {
                Config.TaskActivities.Clear();Config.TaskGroupsByActivity.Clear();Config.TaskGroups.Clear();Config.Tasks.Clear();
                foreach(int id in ids){Config.TaskActivities[id]=new PubTaskActivityConfig{id=id,extraRefreshTimes=new[]{4,8}};Config.TaskGroupsByActivity[id]=new List<PubTaskGroupConfig>();}
            }
            public OutgameTaskItemData Task(int id,long progress=0)
            {
                Config.Tasks[id]=new PubTaskConfig{id=id,des=new Lang{key="task"},conditionParams=new List<ListArrayInt>{new ListArrayInt{datas=new[]{810,7,9}},new ListArrayInt{datas=new[]{811,20}}}};
                return new OutgameTaskItemData{id=id,progress=progress};
            }
            public string Stored=>Runtime.Statistics.Strings.GetString(Runtime.Statistics.StorageHost.MineGameName+Manager.DataKey,"");
            public void Dispose(){OutgameTaskModels.Services=previous;Runtime.Dispose();}
        }
        static OutgameChildTaskData Child(int id,params OutgameTaskItemData[] tasks)=>new OutgameChildTaskData{activityID=id,tasks=new List<OutgameTaskItemData>(tasks)};
        static string Json(params OutgameChildTaskData[] children)=>JsonUtility.ToJson(new OutgameTaskData{datas=new List<OutgameChildTaskData>(children)});
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original TaskMgr/TaskOffStrategy with actual pool/config/storage, explicit activity callback fixture. Concrete task activity, automatic daily refresh/claims/UI and production Main remain pending. No fresh native/Player run."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="task-manager-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-manager-"+id,result="fail",detail=e.ToString()});}};
            check("pool-init-deferred-config-gate-and-original-data",()=>{
                using(var f=new Fixture()){
                    Require(f.Manager.Data==null&&f.Manager.Strategy!=null&&f.Downloads==0,"OnInit constructs strategy only");
                    f.Config.Activities.Remove(1300001);f.Manager.InitStrategy();Require(f.Manager.Data==null&&ReferenceEquals(f.Manager.Strategy.Manager,f.Manager),"pool binding precedes absent parent config gate");
                    f.Config.Activities[1300001]=new OutgameActivityConfigRow{id=1300001};f.Manager.InitStrategy();
                    Require(f.Manager.Data.datas.Count==1&&f.Manager.ChildTaskDic.ContainsKey(130001)&&f.Current.Refreshes==1&&f.Manager.Data.datas[0].tasks.Count==0,"actual original config creates empty module, child activity later populates tasks");
                    Require(f.Stored==""&&f.Manager.ActivityId==1300001&&f.Manager.ParticipatesInSync&&!f.Manager.CompressData,"no implicit save, original flags and identity");
                }
            });
            check("server-deferred-callback-and-disabled-sync-local",()=>{
                using(var f=new Fixture()){
                    f.Only(11);f.Runtime.Statistics.StorageHost.Server=true;f.Manager.InitStrategy();Require(f.Downloads==1&&f.Manager.Data==null,"await genuine server callback");
                    f.Manager.UpdateDataCallBack(Json(Child(11)));Require(f.Manager.ChildTaskDic[11].ext.extraRefreshNum==-1&&f.Current.Refreshes==1,"callback repairs and publishes");
                    f.Manager.ParticipatesInSync=false;f.Manager.InitStrategy();Require(f.Downloads==1&&f.Manager.Data.datas[0].ext.extraRefreshNum==0,"fresh local branch skips existing-data migration");
                }
            });
            check("legacy-progress-group-migration-and-condition-reconciliation",()=>{
                using(var f=new Fixture()){
                    f.Only(11);f.Config.TaskGroups[77]=new PubTaskGroupConfig{Id=77,extraRefreshTimes=new[]{2,4}};
                    var old=f.Task(1,-7);old.conditions.Add(new OutgameTaskCondition{key=999,value=123});var existing=f.Task(2);existing.uid=long.MaxValue;existing.conditions.Add(new OutgameTaskCondition{key=810,arg=new[]{7},value=88});
                    var child=Child(11,old,existing,new OutgameTaskItemData{id=999});child.ext.activityID=999;child.ext.groupDatas.Add(new OutgameTaskGroupData{groupID=999});child.ext.groupDatas.Add(new OutgameTaskGroupData{groupID=77,freeRefreshTimes=3,extraRefreshTimes=new List<int>{8}});
                    f.Manager.InitStrategy();f.Manager.UpdateDataCallBack(Json(Child(98),Child(99),child));var repaired=f.Manager.Data.datas[0];
                    Require(f.Manager.Data.datas.Count==1&&repaired.ext.activityID==11&&repaired.ext.extraRefreshNum==-1&&repaired.ext.groupDatas.Count==1&&repaired.ext.groupDatas[0].extraRefreshTimes.SequenceEqual(new[]{0,0})&&repaired.ext.groupDatas[0].freeRefreshTimes==3,"orphan deletion, ext correction and one-time group migration");
                    Require(repaired.tasks.Count==2&&repaired.tasks[0].progress==0&&repaired.tasks[0].conditions.Count==2&&repaired.tasks[0].conditions[1].value==-7&&repaired.tasks[0].conditions[1].arg.SequenceEqual(new[]{7}),"legacy nonzero signed progress appended only for first configured condition");
                    Require(repaired.tasks[0].uid==repaired.tasks[0].GetHashCode()&&repaired.tasks[1].uid==long.MaxValue&&repaired.tasks[1].conditions.Count==2&&repaired.tasks[1].conditions[0].value==88&&f.Errors.Count==1&&f.Logs.Count==1,"uid repair, existing condition progress retained and only missing condition logged");
                    string saved=JsonUtility.ToJson(f.Manager.Data);f.Manager.UpdateDataCallBack(saved);Require(f.Manager.Data.datas[0].tasks[0].conditions.Count==3&&f.Logs.Count==2,"second load fills legacy path second condition instead of retroactively doing so on first load");
                }
            });
            check("new-child-reset-publication-and-sentinel-preservation",()=>{
                using(var f=new Fixture()){
                    f.Only(11);f.Config.TaskActivities[11].extraRefreshTimes=null;Throws<NullReferenceException>(()=>f.Manager.Strategy.LoadData(""));Require(f.Manager.Strategy.Data.datas.Count==1&&f.Manager.Data==null,"fresh child published before reset failure");
                    Throws<NullReferenceException>(()=>f.Manager.Strategy.LoadData(Json()));Require(f.Manager.Strategy.Data.datas.Count==0,"missing existing-data child reset before append");
                    f.Config.TaskActivities[11].extraRefreshTimes=Array.Empty<int>();var child=Child(11);child.ext.extraRefreshNum=-1;child.ext.groupDatas.Add(new OutgameTaskGroupData{groupID=999,extraRefreshTimes=new List<int>{8}});
                    f.Manager.InitStrategy();f.Manager.UpdateDataCallBack(Json(child));Require(f.Manager.Data.datas[0].ext.groupDatas[0].extraRefreshTimes[0]==8,"sentinel skips validation/reset even for removed group");
                }
            });
            check("duplicate-index-and-malformed-fallback-prefix",()=>{
                using(var f=new Fixture()){
                    f.Only(11);f.Manager.InitStrategy();int refreshes=f.Current.Refreshes;Throws<ArgumentException>(()=>f.Manager.UpdateDataCallBack(Json(Child(11),Child(11))));
                    Require(f.Manager.Data.datas.Count==2&&f.Manager.ChildTaskDic.Count==1&&ReferenceEquals(f.Manager.ChildTaskDic[11],f.Manager.Data.datas[0])&&f.Current.Refreshes==refreshes,"publish input then Add rejects duplicate after first index, no activity refresh");
                    var held=f.Manager.Strategy.Data;Throws<ArgumentException>(()=>f.Manager.UpdateDataCallBack("not-json"));Require(ReferenceEquals(held,f.Manager.Strategy.Data),"second parse failure leaves old strategy data");
                    f.Services.Warning=s=>throw new InvalidOperationException("warning");f.Manager.UpdateDataCallBack(Json(Child(11)));Require(f.Current.Refreshes==refreshes+1,"warning exception enters raw JSON fallback");
                }
            });
            check("log-failure-keeps-appended-condition-before-uid-and-callback",()=>{
                using(var f=new Fixture()){
                    f.Only(11);var task=f.Task(1);f.Manager.InitStrategy();var held=f.Manager.Data;f.Services.Log=a=>throw new InvalidOperationException("log");
                    Throws<InvalidOperationException>(()=>f.Manager.UpdateDataCallBack(Json(Child(11,task))));var migrated=f.Manager.Strategy.Data.datas[0].tasks[0];
                    Require(migrated.conditions.Count==1&&migrated.uid==0&&ReferenceEquals(f.Manager.Data,held),"append precedes logging failure; UID and manager callback not reached");
                }
            });
            check("save-snapshot-before-activity-cached-owner-and-release",()=>{
                using(var f=new Fixture()){
                    f.Only(11);f.Manager.InitStrategy();var first=f.Current;first.Saving=()=>f.Manager.Data.datas[0].ext.livenessValue++;
                    f.Manager.OnSave();var saved=JsonUtility.FromJson<OutgameTaskData>(OutgameActivityCodec.DecompressString(f.Stored,s=>{}));Require(saved.datas[0].ext.livenessValue==0&&f.Manager.Data.datas[0].ext.livenessValue==1,"serialized clone captured before activity SaveData mutation");
                    f.Current=new Activity();f.Manager.OnSave();Require(first.Saves==2&&f.Current.Saves==0,"save activity caches first nonnull lookup");
                    string stored=f.Stored;first.Saving=()=>throw new InvalidOperationException("save");Throws<InvalidOperationException>(f.Manager.OnSave);Require(f.Stored==stored,"activity save failure prevents manager storage");
                    var strategy=f.Manager.Strategy;f.Manager.OnRelease();Require(ReferenceEquals(strategy,f.Manager.Strategy)&&f.Manager.Data!=null,"source release is empty and retains references");
                }
            });
            check("missing-save-owner-retries-and-file-restart",()=>{
                string path;using(var f=new Fixture()){
                    f.Only(11);f.Manager.InitStrategy();f.Current=null;Throws<NullReferenceException>(f.Manager.OnSave);Require(f.Stored=="","missing activity cannot silently save");f.Current=new Activity();
                    var task=f.Task(1);task.uid=long.MaxValue;task.state=1;task.expireTimeStamp=long.MinValue;task.ResetProgress();task.conditions[0].value=long.MaxValue;f.Manager.Data.datas[0].tasks.Add(task);f.Manager.Data.datas[0].ext.livenessAward=int.MinValue;
                    f.Manager.OnSave();path=f.Runtime.Statistics.PathName;Require(f.Stored.Length>0&&!f.Stored.StartsWith("{"),"actual compressed manager storage");
                }
                using(var f=new Fixture(path)){
                    f.Only(11);f.Task(1);f.Manager.InitStrategy();var task=f.Manager.ChildTaskDic[11].tasks[0];Require(task.uid==long.MaxValue&&task.expireTimeStamp==long.MinValue&&task.state==1&&task.conditions[0].value==long.MaxValue&&f.Manager.Data.datas[0].ext.livenessAward==int.MinValue,"fresh independent storage/backend manager retains signed fields and progress");
                }
            });
            return report;
        }
    }
}
