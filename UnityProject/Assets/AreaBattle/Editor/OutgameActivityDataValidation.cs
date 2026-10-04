using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityDataValidation
    {
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Host:IOutgameDataStorageHost
        {
            public bool Server;public int SourceLoginProgress=>10;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;
            public bool IsUseServer=>Server;public string MineGameName=>"activity-data-test";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){}public void Toast(string s){}
            public string Compress(string k,string s)=>throw new Exception("unexpected storage compression");
            public string Decompress(string k,string s)=>throw new Exception("unexpected storage decompression");
            public void QueueUpload(string k,string s){}
        }
        sealed class Child:OutgameCommonModuleManager
        {
            public int Id=7;public Action InitAction,StrategyAction;public Child(OutgameDataManagerStorage s,IOutgameDataStorageHost h):base(s,h,k=>{}){}
            public override int ActivityId=>Id;public override string SourceClassName=>"Child";
            public override void OnInit()=>InitAction?.Invoke();public override void InitStrategy()=>StrategyAction?.Invoke();
            public override void OnSave(){}public override void OnRelease(){}public override void UpdateDataCallBack(string text){}
        }
        sealed class Fixture
        {
            public readonly string DirectoryName;public readonly Host StorageHost=new Host();
            public readonly List<string> Trace=new List<string>(),Warnings=new List<string>();public readonly List<object[]> Errors=new List<object[]>();
            public Dictionary<object,OutgameActivityConfigRow> Configs=new Dictionary<object,OutgameActivityConfigRow>();
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameStatisticsControl Statistics=new OutgameStatisticsControl{ValueProviders=new Dictionary<int,Func<object[],long>>()};
            public readonly OutgameActivityManagerServices Services;public readonly OutgameActivityManager Manager;
            public readonly OutgameSdkStringStorage Strings;public Action Registration,Ready,Writing;public int DirtyWrites,Downloads,ReadyCount;
            public Fixture(string path=null)
            {
                DirectoryName=path??Path.Combine(Path.GetTempPath(),"AreaBattleActivity-"+Guid.NewGuid().ToString("N"));
                Strings=new OutgameSdkStringStorage(new OutgameFileStorageBackend(DirectoryName,a=>{Writing?.Invoke();a();}),s=>Trace.Add("storage-error:"+s));
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>Trace.Add("duplicate"));Pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>());
                Services=new OutgameActivityManagerServices{RegistrationCallback=()=>Registration,AssemblyTypes=()=>Array.Empty<Type>(),CreateManager=t=>throw new Exception("missing source factory"),
                    SourceTypeIndex=t=>t==typeof(OutgameActivityManager)?4658:99,SourceAutoSyn=t=>t==typeof(OutgameActivityManager),Configurations=()=>Configs,Pool=()=>Pool,
                    DataReady=()=>{ReadyCount++;Trace.Add("ready");Ready?.Invoke();},Error=Errors.Add,
                    OffNet=new OutgameActivityOffNetServices{Statistics=new OutgameStatisticsExpansion(()=>Statistics,Errors.Add),Configurations=()=>Configs,
                        GetNowTimeInt=()=>{Trace.Add("clock");return 1700000000;},SetDirty=b=>{DirtyWrites++;Manager.Dirty=b;Trace.Add("dirty");},Warning=Warnings.Add,Error=Errors.Add}};
                Manager=new OutgameActivityManager(NewStorage("ActivityManager"),StorageHost,k=>{Downloads++;Trace.Add("download");},Services);
                Pool.AddModel(4658,Manager,true);
                foreach(int id in new[]{1,2,3,10000})Statistics.ValueProviders[id]=a=>{Require(ReferenceEquals(a,Array.Empty<object>()),"condition params use original empty array");return 0;};
            }
            public OutgameDataManagerStorage NewStorage(string name)=>new OutgameDataManagerStorage(()=>"CommonGameModule"+name,StorageHost,Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
            public string Stored=>Strings.GetString(StorageHost.MineGameName+Manager.DataKey,"");
            public OutgameActivityOffNetStrategy Strategy=>(OutgameActivityOffNetStrategy)Manager.Strategy;
        }
        static OutgameActivityConfigRow Row(int id)=>new OutgameActivityConfigRow{id=id,uniqueId="config-"+id,
            launchType=new[]{1},launchParams=new[]{"1"},noticeType=new[]{2},noticeParams=new[]{"1"},overType=new[]{3},overParams=new[]{"1"}};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Original activity manager/offline data/codec and condition rules with real pool and isolated file restart. ActivityControl/config loading/reflected production factories/SevenDay/Main are required pending hosts. No new native PlayMode or Player claim."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-data-codec-utf8-multibuffer-and-raw-fallback",()=>{
                var warnings=new List<string>();string raw="活动"+new string('x',4097);Require(OutgameActivityCodec.DecompressString(OutgameActivityCodec.CompressString(raw),warnings.Add)==raw,"UTF8 across 1024-byte buffers");
                Require(ReferenceEquals(raw,OutgameActivityCodec.DecompressString(raw,warnings.Add))&&warnings.Count==1,"bad gzip returns original reference and warns");
                Require(OutgameActivityCodec.CompressString(null)==""&&OutgameActivityCodec.DecompressString("",warnings.Add)=="","empty guards");
                Throws<ArgumentNullException>(()=>OutgameActivityCodec.Decompress(null));
                Throws<InvalidOperationException>(()=>OutgameActivityCodec.DecompressString(raw,s=>throw new InvalidOperationException("warning failed")));
            });
            check("activity-data-condition-date-number-invalid-and-short-circuit",()=>{
                var f=new Fixture();var strategy=new OutgameActivityStrategy(f.Services.OffNet);int calls=0;
                f.Statistics.ValueProviders[1]=a=>{calls++;return -1;};f.Statistics.ValueProviders[10000]=a=>{calls++;return OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0));};
                Require(strategy.ConditionsMet(new[]{1,10000,1},new[]{"bad","bad","2147483648"})&&calls==0,"invalid dates and out-of-range int ignored without statistics query");
                Require(strategy.ConditionsMet(new[]{1,10000},new[]{" -1 ","20261003120000"})&&calls==2,"inclusive equality and invariant exact date");
                Require(!strategy.ConditionsMet(new[]{10000,1},new[]{"20261003120001","-1"})&&calls==3,"future timestamp stops later query");
                Require(!strategy.ConditionsMet(new[]{1},new[]{"0"}),"signed numeric lower bound");
                Require(strategy.ConditionsMet(Array.Empty<int>(),null),"empty types do not read parameters");
                Throws<NullReferenceException>(()=>strategy.ConditionsMet(null,null));Throws<IndexOutOfRangeException>(()=>strategy.ConditionsMet(new[]{1},Array.Empty<string>()));
            });
            check("activity-data-first-load-constructor-config-order-and-publish",()=>{
                var f=new Fixture();f.Configs.Add(88,Row(8));f.Configs.Add(77,Row(7));f.Registration=()=>{Require(f.Manager.Data!=null&&f.Manager.Dirty,"data assigned and dirty before child managers");f.Trace.Add("register");};
                Require(f.Manager.Strategy==null,"pool OnInit is empty");f.Manager.Init();
                Require(f.Manager.Data.firstLoginDay==1700000000&&f.Manager.Data.lastRefreshTimeStamp==0,"original int clock field; refresh stamp unchanged");
                var records=f.Manager.Data.datas;Require(records.Count==2&&records[0].id==8&&records[1].id==7&&records[0].state==1&&records[0].uniqueId=="config-8","values order and copied ID/uniqueId, dictionary keys not substituted");
                Require(string.Join(",",f.Trace)=="clock,dirty,register,ready"&&f.Manager.IsInitStrategy,"dirty before publish and ready after registration");
            });
            check("activity-data-real-compressed-file-and-independent-restart",()=>{
                var f=new Fixture();f.Configs.Add(7,Row(7));f.Manager.Init();var record=f.Manager.Data.datas[0];record.noticePop=true;record.launchPop=true;record.LaunchTimeStamp=long.MaxValue;record.WarmTimeStamp=long.MinValue;record.uniqueId="存档原标识";record.state=2;
                f.Manager.Data.lastRefreshTimeStamp=9223372036854770000L;f.Pool.SaveData();Require(f.Stored.Length>0&&!f.Stored.StartsWith("{")&&f.Manager.Dirty,"gzip storage and no dirty clearing in OnSave");
                var next=new Fixture(f.DirectoryName);next.Configs.Add(7,Row(7));next.Manager.Init();var loaded=next.Manager.Data.datas[0];
                Require(loaded.noticePop&&loaded.launchPop&&loaded.LaunchTimeStamp==long.MaxValue&&loaded.WarmTimeStamp==long.MinValue&&loaded.uniqueId=="存档原标识"&&loaded.state==2,"all persisted item fields survive fresh backend");
                Require(next.Manager.Data.firstLoginDay==1700000000&&next.Manager.Data.lastRefreshTimeStamp==9223372036854770000L&&!next.Manager.Dirty&&next.DirtyWrites==0,"existing load retains clocks without needless dirty");
            });
            check("activity-data-server-deferred-raw-and-corrupt-json-recovery",()=>{
                var f=new Fixture();f.StorageHost.Server=true;f.Manager.Init();Require(f.Downloads==1&&f.Manager.Data==null&&f.ReadyCount==0,"pending server cannot publish synthetic data");
                f.Manager.UpdateDataCallBack("{\"firstLoginDay\":12,\"lastRefreshTimeStamp\":34,\"datas\":[]}");Require(f.Manager.Data.firstLoginDay==12&&f.Warnings.Count==1&&f.ReadyCount==1,"legacy raw JSON accepted");
                f.Manager.UpdateDataCallBack("not-json");Require(f.Errors.Count==1&&f.Errors[0].Length==2&&f.Errors[0][0] is string&&f.Manager.Data.firstLoginDay==1700000000&&f.ReadyCount==2,"second parse error logs message/stack and constructs fresh data");
                f.Services.OffNet.Error=a=>throw new InvalidOperationException("error sink failed");var held=f.Manager.Data;
                Throws<InvalidOperationException>(()=>f.Manager.UpdateDataCallBack("not-json"));Require(ReferenceEquals(held,f.Manager.Data)&&f.ReadyCount==2,"error sink failure propagates before replacement");
            });
            check("activity-data-state-reconciliation-all-launch-notice-over-combinations",()=>{
                foreach(int state in new[]{0,1,2,3,4,5})for(int mask=0;mask<8;mask++)
                {
                    var f=new Fixture();f.Configs.Add(7,Row(7));f.Manager.Init();bool launch=(mask&1)!=0,notice=(mask&2)!=0,over=(mask&4)!=0;
                    f.Statistics.ValueProviders[1]=a=>launch?1:0;f.Statistics.ValueProviders[2]=a=>notice?1:0;f.Statistics.ValueProviders[3]=a=>over?1:0;
                    var data=new OutgameActivityData();data.datas.Add(new OutgameActivityItemData{id=7,state=state,noticePop=true,uniqueId="old"});f.DirtyWrites=0;f.Manager.Dirty=false;
                    f.Manager.UpdateDataCallBack(OutgameActivityCodec.CompressString(JsonUtility.ToJson(data)));
                    bool reset=(state==4&&(launch||notice)&&!over)||(state==3&&!launch&&notice);
                    var item=f.Manager.Data.datas[0];Require(item.state==(reset?1:state)&&f.DirtyWrites==(reset?1:0)&&item.uniqueId=="old"&&item.noticePop,"state="+state+" mask="+mask+" keeps all non-state fields");
                }
            });
            check("activity-data-duplicate-first-match-orphans-and-missing-config-record",()=>{
                var f=new Fixture();f.Configs.Add(7,Row(7));f.Configs.Add(8,Row(8));f.Manager.Init();f.Statistics.ValueProviders[1]=a=>1;
                var data=new OutgameActivityData();data.datas.Add(new OutgameActivityItemData{id=7,state=4,uniqueId="old"});data.datas.Add(new OutgameActivityItemData{id=7,state=4,uniqueId="duplicate"});data.datas.Add(new OutgameActivityItemData{id=99,state=4});
                f.DirtyWrites=0;f.Manager.UpdateDataCallBack(JsonUtility.ToJson(data));var rows=f.Manager.Data.datas;
                Require(rows.Count==4&&rows[0].state==1&&rows[1].state==4&&rows[2].id==99&&rows[3].id==8&&rows[3].uniqueId=="config-8"&&f.DirtyWrites==2,"only first matching duplicate reset; orphan kept; missing appended");
            });
            check("activity-data-condition-recheck-reentry-and-load-failure-prefix",()=>{
                var f=new Fixture();f.Configs.Add(7,Row(7));f.Manager.Init();int calls=0;f.Statistics.ValueProviders[1]=a=>++calls==1?0:1;f.Statistics.ValueProviders[2]=a=>1;
                var data=new OutgameActivityData();data.datas.Add(new OutgameActivityItemData{id=7,state=3});f.DirtyWrites=0;f.Manager.UpdateDataCallBack(JsonUtility.ToJson(data));
                Require(calls==2&&f.Manager.Data.datas[0].state==3&&f.DirtyWrites==0,"launch condition queried again for state3 and latest result respected");
                var held=f.Manager.Data;int ready=f.ReadyCount;f.Configs[7].launchParams=null;
                Throws<NullReferenceException>(()=>f.Manager.UpdateDataCallBack(JsonUtility.ToJson(data)));Require(ReferenceEquals(held,f.Manager.Data)&&f.ReadyCount==ready,"reconciliation failure cannot publish parsed local object");
                f.Configs[7]=Row(7);f.Services.OffNet.SetDirty=b=>throw new InvalidOperationException("dirty sink");
                Throws<InvalidOperationException>(()=>f.Manager.UpdateDataCallBack(""));Require(ReferenceEquals(held,f.Manager.Data),"initial dirty failure precedes callback too");
            });
            check("activity-data-manager-registration-order-duplicate-and-config-reread",()=>{
                var f=new Fixture();var children=new List<Child>();f.Configs.Add(7,Row(7));f.Services.AssemblyTypes=()=>new[]{typeof(string),typeof(OutgameCommonModuleManager),typeof(Child),typeof(Child)};
                f.Services.CreateManager=t=>{f.Trace.Add("create");var c=new Child(f.NewStorage("Child"),f.StorageHost);c.InitAction=()=>{Require(ReferenceEquals(f.Pool.Managers[99],c),"pool publishes before OnInit");f.Trace.Add("init");};c.StrategyAction=()=>f.Trace.Add("strategy");children.Add(c);return c;};
                int reads=0;f.Services.Configurations=()=>{reads++;return f.Configs;};f.Manager.ManagerInit();
                Require(string.Join(",",f.Trace)=="create,init,strategy,create,duplicate,strategy"&&reads==4&&f.Manager.IsInitStrategy,"create before config, double config lookup, duplicate still initializes supplied strategy");
                Require(ReferenceEquals(f.Pool.Managers[99],children[0])&&!children[0].ParticipatesInSync&&children[1].ParticipatesInSync,"manual registration only sets published instance autoSyn");
                f.Trace.Clear();f.Manager.ManagerInit();Require(f.Trace.Count==0,"completed registration is one-shot");
            });
            check("activity-data-manager-null-config-construction-failure-and-retry",()=>{
                var f=new Fixture();int creates=0;f.Configs=null;f.Services.AssemblyTypes=()=>new[]{typeof(Child)};
                f.Services.CreateManager=t=>{creates++;throw new InvalidOperationException("constructor");};Throws<InvalidOperationException>(f.Manager.ManagerInit);
                Require(creates==1&&!f.Manager.IsInitStrategy,"constructor called before absent-config guard; exception leaves retry open");
                f.Services.CreateManager=t=>{creates++;return new Child(f.NewStorage("Child"),f.StorageHost);};f.Manager.ManagerInit();Require(creates==2&&f.Manager.IsInitStrategy&&f.Pool.Managers.Count==1,"absent config skips registration after constructing");
                f.Manager.OnRelease();f.Configs=new Dictionary<object,OutgameActivityConfigRow>{{7,Row(7)}};
                f.Services.CreateManager=t=>new Child(f.NewStorage("Child"),f.StorageHost){InitAction=()=>throw new InvalidOperationException("init")};
                Throws<InvalidOperationException>(f.Manager.ManagerInit);Require(!f.Manager.IsInitStrategy&&f.Pool.Managers.ContainsKey(99),"failed OnInit retains pool entry and permits retry");
            });
            check("activity-data-manager-callback-reread-reentry-null-and-release",()=>{
                var f=new Fixture();int reads=0,nested=0;Action second=()=>{nested++;if(nested==1)f.Manager.ManagerInit();};
                f.Services.RegistrationCallback=()=>{reads++;return reads==1?()=>throw new Exception("stale callback"):second;};f.Manager.ManagerInit();
                Require(reads==4&&nested==2&&f.Manager.IsInitStrategy,"delegate reread and no pre-registration flag before reentry");
                f.Manager.Init();var strategy=f.Manager.Strategy;var data=f.Manager.Data;f.Manager.Dirty=true;f.Manager.OnRelease();
                Require(!f.Manager.IsInitStrategy&&ReferenceEquals(strategy,f.Manager.Strategy)&&ReferenceEquals(data,f.Manager.Data)&&f.Manager.Dirty,"release only resets registration gate");
                f.Manager.RefreshData(null);Require(f.Manager.Data==null&&f.Errors.Count==1&&(string)f.Errors[0][0]=="活动数据为null"&&!f.Manager.IsInitStrategy,"null assigned before error and returns before child registration");
            });
            check("activity-data-manager-save-live-data-dirty-and-failure",()=>{
                var f=new Fixture();f.Manager.OnSave();f.Manager.Init();var original=f.Manager.Strategy;f.Manager.Data.datas.Add(new OutgameActivityItemData{id=9});
                f.Writing=()=>{Require(f.Manager.Dirty,"save does not clear dirty before IO");throw new InvalidOperationException("write scheduling failed");};
                Throws<InvalidOperationException>(f.Manager.OnSave);Require(f.Manager.Dirty&&ReferenceEquals(original,f.Manager.Strategy),"save failure retains source state");
                f.Services.DataReady=()=>throw new InvalidOperationException("owner callback");var next=new OutgameActivityData();Throws<InvalidOperationException>(()=>f.Manager.RefreshData(next));
                Require(ReferenceEquals(f.Manager.Data,next)&&f.Manager.IsInitStrategy,"owner callback failure keeps assigned data and completed registration");
            });
            return report;
        }
    }
}
