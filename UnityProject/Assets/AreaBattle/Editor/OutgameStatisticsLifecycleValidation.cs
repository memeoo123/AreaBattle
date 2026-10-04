using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameStatisticsLifecycleValidation
    {
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Host:IOutgameDataStorageHost
        {
            public bool Server;
            public int SourceLoginProgress=>10;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;
            public bool IsUseServer=>Server;public string MineGameName=>"statistics-test";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){}public void Toast(string s){}
            public string Compress(string k,string s)=>throw new Exception("unexpected storage compression");
            public string Decompress(string k,string s)=>throw new Exception("unexpected storage decompression");
            public void QueueUpload(string k,string s)=>throw new Exception("unexpected upload");
        }
        // Explicit strategy test endpoint; this is never registered as a production/offline strategy.
        sealed class Probe:OutgameStatisticsStrategy
        {
            public Action Init,Save,Cleanup,Tick;public Action<string> Load;
            public object[] LastUpdateArgs;public OutgameStatisticsJsonData SavedData;
            public Dictionary<int,OutgameGameStatisticsData> SavedRecords;
            public int Saves,Updates,Disposes;
            public Probe(Fixture f):base(()=>f.Pool,()=>f.Common,()=>f.Owner,f.Expansion){}
            public override void InitData(Action<OutgameStatisticsJsonData> update){base.InitData(update);Init?.Invoke();}
            public void Publish(OutgameStatisticsJsonData data)=>UpdateDataAction(data);
            public void Request(bool server)=>UpdateManagerData(server);
            public override void LoadData(string text)=>Load?.Invoke(text);
            public override void UpdateDate(object[] args){LastUpdateArgs=args;Updates++;Tick?.Invoke();}
            protected override void OnDispose(){Disposes++;Cleanup?.Invoke();}
            public override void OnSave(OutgameStatisticsJsonData data,Dictionary<int,OutgameGameStatisticsData> records)
            {Saves++;SavedData=data;SavedRecords=records;Save?.Invoke();}
            public override long Value10000(object[] a)=>0;
            public override long Value10015(object[] a)=>10015;
            public override long Value10011(object[] a)=>10011;
            public override long Value10901(object[] a)=>10901;
            public override long Value10900(object[] a)=>10900;
            public override long Value10800(object[] a)=>10800;
            public override long Value10003(object[] a)=>10003;
            public override long Value10002(object[] a)=>10002;
            public override long Value10001(object[] a)=>10001;
            public override long Value10902(object[] a)=>10902;
            public override long Value10008(object[] a)=>10008;
            public override long Value10007(object[] a)=>10007;
            public override long Value20000(object[] a)=>20000;
        }
        sealed class Fixture:IDisposable
        {
            public readonly Host StorageHost=new Host();
            public readonly List<string> Trace=new List<string>(),Errors=new List<string>();
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public OutgameCommonMessageDispatcher Common=new OutgameCommonMessageDispatcher();
            public OutgameStatisticsControl Owner=new OutgameStatisticsControl();
            public readonly OutgameStatisticsExpansion Expansion;
            public readonly OutgameFileStorageBackend Backend;
            public readonly OutgameUpdateManager Updates;
            public readonly OutgameStatisticsControlServices Services;
            public OutgameStatisticsManager Manager;public Probe Strategy;
            public Action Disposables,Creating;public int Downloads;
            public Fixture()
            {
                Backend=new OutgameFileStorageBackend(Path.Combine(Path.GetTempPath(),"AreaBattleStatistics-"+Guid.NewGuid().ToString("N")),a=>a());
                Pool=new OutgameDataManagerPool(()=>{},s=>Trace.Add(s),s=>{},s=>Errors.Add(s));
                Pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>());
                Expansion=new OutgameStatisticsExpansion(()=>Owner,a=>Errors.Add((string)a[0]));
                Updates=new GameObject("statistics-validation-updates").AddComponent<OutgameUpdateManager>();
                Services=new OutgameStatisticsControlServices{Pool=()=>Pool,CreateManager=Create,
                    AddUpdate=a=>{Trace.Add("add-update");return Updates.Register(a);},RemoveUpdate=a=>{Trace.Add("remove-update");Updates.QueueRemove(a);},
                    GetDisposableActions=()=>Disposables,SetDisposableActions=a=>{Trace.Add("set-dispose");Disposables=a;},
                    ClearCommonMessages=()=>{Trace.Add("clear-common");Common=new OutgameCommonMessageDispatcher();}};
            }
            public OutgameStatisticsManager Create()
            {
                var storage=new OutgameDataManagerStorage(()=>"CommonGameModuleGameStatisticsManager",StorageHost,
                    new OutgameSdkStringStorage(Backend,s=>Errors.Add(s)),new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
                return Manager=new OutgameStatisticsManager(storage,StorageHost,s=>{Require(s==Manager.DataKey,"source download key");Downloads++;},
                    ()=>{Creating?.Invoke();return Strategy=new Probe(this);},()=>Messages,()=>Common,()=>Owner,Expansion,a=>Trace.Add((string)a[0]));
            }
            public void Init(Action complete=null)=>Owner.OnInit(Services,complete);
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Updates.gameObject);}
        }
        sealed class ChangingText
        {
            public int Calls;public override string ToString()=>++Calls==1?"not-a-number":"14";
        }
        sealed class ThrowingText{public override string ToString()=>throw new InvalidOperationException("text failure");}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Source statistics manager/control/expansion/abstract strategy and real pool/UpdateManager adapter. Probe is an explicit test strategy. Concrete offnet codec/clock/day refresh, production CommonGameModule/Main and Player are still pending; manual Update invocation is not a native PlayMode claim."};
            Action<string,Action> check=(id,body)=>{var save=OutgameStatisticsManager.SaveDataDel;try{OutgameStatisticsManager.SaveDataDel=null;body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}finally{OutgameStatisticsManager.SaveDataDel=save;}};
            check("statistics-lifecycle-pool-manual-add-publishes-before-init-and-retains-duplicate",()=>{
                using(var f=new Fixture())
                {
                    Require(f.Owner.SourceFlag20&&f.Owner.InitOver==null&&f.Owner.ValueProviders==null,"source constructor fields");
                    f.Creating=()=>Require(ReferenceEquals(f.Pool.Managers[4617],f.Manager)&&f.Manager.ParticipatesInSync,"manual insert and autoSyn before OnInit");
                    f.Init();var first=f.Manager;Require(ReferenceEquals(first,f.Owner.Manager)&&first.Strategy==f.Strategy&&first.IsInit&&first.Data==null,"OnInit creates strategy, does not load or claim completion");
                    var duplicate=f.Create();duplicate.ParticipatesInSync=false;var returned=f.Pool.AddModel(4617,duplicate,true);
                    Require(ReferenceEquals(duplicate,returned)&&ReferenceEquals(first,f.Pool.Managers[4617])&&duplicate.Strategy==null&&!duplicate.ParticipatesInSync&&f.Errors.Count==1,"duplicate returns supplied object without init/attribute rewrite");
                }
            });
            check("statistics-lifecycle-registration-game-filter-and-add-failure-partial-state",()=>{
                using(var f=new Fixture())
                {
                    int made=0;f.Pool.OnInit(true,"Proj_hdzd",new[]{new OutgameManagerRegistration(4617,"CommonGameModule",true,false,()=>{made++;return f.Create();})});Require(made==0,"original registration belongs to CommonGameModule");
                    f.Creating=()=>throw new InvalidOperationException("strategy construction");Throws<InvalidOperationException>(()=>f.Init());
                    Require(ReferenceEquals(f.Owner.Manager,f.Pool.Managers[4617])&&f.Owner.ValueProviders!=null&&f.Updates.HandleList.Count==0,"failed init retains pool publication, stops before updates");
                }
            });
            check("statistics-lifecycle-refresh-registers-nineteen-captured-providers-first-wins",()=>{
                using(var f=new Fixture())
                {
                    int completed=0;f.Init(()=>{Require(!f.Manager.IsInit&&f.Owner.ValueProviders.Count==19,"publish initialized flag before completion");completed++;});
                    f.Expansion.RegisteredValueFunc(10015,a=>777);var first=f.Strategy;first.Publish(new OutgameStatisticsJsonData());
                    Require(completed==1&&f.Expansion.GameValue(10015)==777&&f.Expansion.GameValue(10011)==10011,"preexisting custom value wins");
                    string keys=string.Join(",",f.Owner.ValueProviders.Keys);Require(keys=="10015,10000,10011,10901,10900,10800,10003,10002,10001,10902,10008,10007,20000,10500,10501,10502,10600,10700,10013","original registration order");
                    f.Manager.Strategy=new Probe(f);f.Manager.RefreshData(new OutgameStatisticsJsonData());Require(completed==1&&ReferenceEquals(f.Owner.ValueProviders[10011].Target,first),"refresh does not replace captured provider or repeat completion");
                    f.Manager.IsInit=true;f.Owner.InitOver=()=>throw new InvalidOperationException("completion");Throws<InvalidOperationException>(()=>f.Manager.RefreshData(new OutgameStatisticsJsonData()));Require(!f.Manager.IsInit,"failed completion still consumes first-init flag");
                }
            });
            check("statistics-lifecycle-refresh-null-index-and-provider-failure-order",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Manager.SetEventStatistics(7,9L);Throws<NullReferenceException>(()=>f.Manager.RefreshData(null));
                    Require(f.Manager.Data==null&&f.Manager.IsInit&&f.Manager.GetEventStatistics(7)==9&&f.Trace.Contains("游戏统计数据为null"),"null logs after Data assignment then fails before provider/callback");
                    f.Manager.Strategy=null;var data=new OutgameStatisticsJsonData();data.datas.Add(new OutgameGameStatisticsData(8,10));Throws<NullReferenceException>(()=>f.Manager.RefreshData(data));
                    Require(ReferenceEquals(data,f.Manager.Data)&&f.Manager.GetEventStatistics(8)==10&&f.Manager.IsInit,"virtual delegate null target fails after indexing");
                }
            });
            check("statistics-lifecycle-message-register-set-parse-and-provider-gates",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Messages.SendMessage(OutgameStatisticsManager.RegistrationMessage,new object[]{91,(Func<object[],long>)(a=>9)});
                    f.Messages.SendMessage(OutgameStatisticsManager.SetMessage,new object[]{91,new ThrowingText()});Require(f.Manager.GetGameStatisticsData(91)==null,"provider gate precedes value formatting");
                    f.Messages.SendMessage(OutgameStatisticsManager.SetMessage,new object[]{92,"8",3});Require(f.Manager.GetEventStatistics(92,3)==8&&f.Owner.IsDirty,"message reaches shared actual records");
                    var change=new ChangingText();f.Messages.SendMessage(OutgameStatisticsManager.SetMessage,new object[]{93,change});Require(change.Calls==2&&f.Manager.GetEventStatistics(93)==14,"int fallback re-evaluates ToString after long parse failure");
                    f.Messages.SendMessage(OutgameStatisticsManager.SetMessage,new object[]{94,"4",1,2});Require(f.Manager.GetGameStatisticsData(94)==null,"only arities2/3 write");
                    Throws<InvalidCastException>(()=>f.Messages.SendMessage(OutgameStatisticsManager.RegistrationMessage,new object[]{95,new object()}));Require(!f.Owner.ValueProviders.ContainsKey(95),"source cast helper throws before registration");
                    f.Messages.SendMessage(OutgameStatisticsManager.RegistrationMessage,new object[]{95,null});Require(f.Owner.ValueProviders.ContainsKey(95)&&f.Owner.ValueProviders[95]==null,"explicit null provider is registered");
                    Throws<NullReferenceException>(()=>f.Manager.SetFromMessage(new object[]{96,null}));Throws<InvalidCastException>(()=>f.Manager.SetFromMessage(new object[]{96L,"1"}));
                }
            });
            check("statistics-lifecycle-expansion-dirty-order-owner-reentry-and-clock-exception",()=>{
                using(var f=new Fixture())
                {
                    f.Init();bool during=true;f.Common.AddListener("81",a=>during=f.Owner.IsDirty);f.Expansion.AddEventCount(81,2);Require(!during&&f.Owner.IsDirty,"notification precedes dirty write");
                    f.Owner.IsDirty=false;f.Common.AddListener("82",a=>throw new InvalidOperationException());Throws<InvalidOperationException>(()=>f.Expansion.SetEventCount(82,3));Require(!f.Owner.IsDirty&&f.Manager.GetEventStatistics(82)==3,"failed notify commits count but does not mark dirty");
                    f.Expansion.SetEventCount(10000,6);Require(!f.Owner.IsDirty,"only parent clock set skips dirty");f.Expansion.SetEventCount(10000,1,4);Require(f.Owner.IsDirty,"child clock set marks dirty");
                    var old=f.Owner;var replacement=new OutgameStatisticsControl();f.Common.AddListener("83",a=>f.Owner=replacement);f.Expansion.AddEventCount(83,9);Require(replacement.IsDirty&&f.Manager.GetEventStatistics(83)==9,"post-notify resolver marks current owner");f.Owner=old;
                }
            });
            check("statistics-lifecycle-game-value-provider-catch-and-record-fallback",()=>{
                using(var f=new Fixture())
                {
                    Require(f.Expansion.GameValue(1)==0&&f.Errors[0]=="GameStatisticsControl未初始化","uninitialized provider map gate");f.Init();f.Manager.SetEventStatistics(1,7L);f.Manager.SetEventStatistics(1,0,3L);f.Manager.AddEventStatistics(1,2,9L);
                    Require(f.Expansion.GameValue(1)==7&&f.Expansion.GameValue(1,new object[]{0})==7&&f.Expansion.GameValue(1,"2")==9,"zero GameValue argument asks parent, not child0");
                    Require(f.Expansion.GameValue(1,new object[]{null})==0&&f.Expansion.GameValue(1,1,2)==0,"invalid and unsupported arguments log/return0");
                    object[] seen=null;f.Expansion.RegisteredValueFunc(3,a=>{seen=a;return 12;});var args=new object[]{new object(),1};Require(f.Expansion.GameValue(3,args)==12&&ReferenceEquals(args,seen),"provider gets unmodified arguments before arity parsing");
                    f.Expansion.RegisteredValueFunc(4,a=>throw new InvalidOperationException("provider detail"));Require(f.Expansion.GameValue(4)==0&&f.Errors[f.Errors.Count-1]=="GameValue注册函数执行异常: provider detail","only provider execution is caught and Message is logged");
                    Throws<InvalidOperationException>(()=>f.Expansion.GameValue(1,new ThrowingText()));
                    f.Expansion.RegisteredValueFunc(5,null);Require(f.Expansion.GameValue(5)==0,"registered-null invocation goes through provider catch");
                }
            });
            check("statistics-lifecycle-update-pool-gate-and-save-callback-reread",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Owner.IsDirty=true;f.Pool.SourceReadyFlag=false;f.Updates.Update();Require(f.Strategy.Updates==1&&f.Strategy.Saves==0&&ReferenceEquals(f.Strategy.LastUpdateArgs,Array.Empty<object>()),"strategy update precedes pool save gate");
                    f.Pool.SourceReadyFlag=true;var replacement=new OutgameStatisticsJsonData();var dict=new Dictionary<int,OutgameGameStatisticsData>();
                    OutgameStatisticsManager.SaveDataDel=(d,r)=>{f.Manager.Data=replacement;f.Manager.Datas=dict;};f.Updates.Update();Require(f.Strategy.Saves==1&&ReferenceEquals(replacement,f.Strategy.SavedData)&&ReferenceEquals(dict,f.Strategy.SavedRecords)&&f.Owner.IsDirty,"strategy rereads data after static callback; controller does not clear dirty");
                    OutgameStatisticsManager.SaveDataDel=(d,r)=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Updates.Update());Require(f.Strategy.Saves==1,"callback failure prevents strategy save");
                    f.Strategy.Tick=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Updates.Update());Require(f.Strategy.Saves==1,"update failure prevents save gate");
                }
            });
            check("statistics-lifecycle-load-before-common-refresh-and-real-storage-path",()=>{
                using(var f=new Fixture())
                {
                    f.Init();var trace=new List<string>();f.Strategy.Load=s=>trace.Add("load:"+s);f.Common.AddListener(OutgameStatisticsManager.RefreshMessage,a=>{Require(a==null,"refresh has null args");trace.Add("refresh");});
                    f.Manager.SaveData("opaque-source-payload");f.Strategy.Request(false);Require(string.Join("|",trace)=="load:opaque-source-payload|refresh","real storage SaveData/UpdateData forwards exact text, not a fabricated statistics codec");
                    f.StorageHost.Server=true;f.Strategy.Request(true);Require(f.Downloads==1&&trace.Count==2,"server request waits for actual callback");
                    f.Strategy.Load=s=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Manager.UpdateDataCallBack("failed"));Require(trace.Count==2,"failed load does not emit refresh");
                }
            });
            check("statistics-lifecycle-base-calendar-activity-and-null-argument-asymmetry",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Expansion.RegisteredValueFunc(10000,a=>OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,21,0,0)));f.Strategy.Publish(new OutgameStatisticsJsonData());
                    Require(f.Expansion.GameValue(10500)==21&&f.Expansion.GameValue(10501)==6&&f.Expansion.GameValue(10502)==3,"calendar providers use original timestamp epoch");
                    f.Common.SendMessage("CommonModule_Activity_Lunch",new object[]{123});Require(f.Manager.GetEventStatistics(20000,123)==1&&f.Owner.IsDirty,"source launch callback writes child activity count");
                    Require(f.Strategy.Value10600(null)==0&&f.Strategy.Value10700(Array.Empty<object>())==0,"different source input guards");Throws<NullReferenceException>(()=>f.Strategy.Value10700(null));
                    int errors=f.Errors.Count;Require(f.Expansion.GameValue(10700,(object[])null)==0&&f.Errors.Count==errors+1,"provider exception is contained by GameValue");
                    f.Strategy.Dispose();f.Common.SendMessage("CommonModule_Activity_Lunch",new object[]{123});Require(f.Manager.GetEventStatistics(20000,123)==1&&f.Strategy.Disposes==1,"remove activity listener before subclass cleanup");
                }
            });
            check("statistics-lifecycle-control-dispose-defers-update-removal-and-retains-manager",()=>{
                using(var f=new Fixture())
                {
                    int prior=0;f.Disposables=()=>prior++;f.Init(()=>{});f.Owner.IsDirty=true;var map=f.Owner.ValueProviders;var common=f.Common;f.Trace.Clear();f.Disposables();
                    Require(prior==1&&f.Owner.ValueProviders==null&&f.Owner.InitOver==null&&ReferenceEquals(f.Owner.Manager,f.Manager)&&f.Owner.IsDirty,"source dispose retains manager/flags, clears map/callback");
                    Require(!ReferenceEquals(common,f.Common)&&string.Join(",",f.Trace)=="remove-update,set-dispose,clear-common"&&f.Updates.HandleList.Count==1,"framework dispose action unlinks itself and queues update removal");
                    f.Updates.Update();Require(f.Updates.HandleList.Count==0&&f.Strategy.Updates==0,"actual UpdateManager processes queued delegate removal");
                }
            });
            check("statistics-lifecycle-manager-release-removes-listeners-before-cleanup-failure",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Strategy.Publish(new OutgameStatisticsJsonData());f.Manager.SetEventStatistics(9,8L);var data=f.Manager.Data;f.Strategy.Cleanup=()=>throw new InvalidOperationException();
                    Throws<InvalidOperationException>(f.Manager.OnRelease);Require(ReferenceEquals(data,f.Manager.Data)&&!f.Manager.IsInit&&f.Manager.GetEventStatistics(9)==8,"cleanup failure preserves later data clear and init flag");
                    f.Messages.SendMessage(OutgameStatisticsManager.SetMessage,new object[]{10,3});Require(f.Manager.GetGameStatisticsData(10)==null,"normal listeners removed before failing cleanup");
                    f.Strategy.Cleanup=null;f.Manager.OnRelease();Require(f.Manager.Data==null&&f.Manager.IsInit&&f.Manager.Datas.Count==0&&ReferenceEquals(f.Strategy,f.Manager.Strategy),"successful release retains strategy while clearing indexed data");
                }
            });
            check("statistics-lifecycle-existing-manager-completes-before-update-registration",()=>{
                using(var f=new Fixture())
                {
                    var manager=f.Create();f.Pool.AddModel(4617,manager,true);f.Strategy.Init=()=>f.Strategy.Publish(new OutgameStatisticsJsonData());
                    int complete=0;f.Init(()=>{complete++;Require(f.Updates.HandleList.Count==0&&f.Disposables==null,"source completion runs inside InitStrategy before frame/dispose registration");});
                    Require(complete==1&&ReferenceEquals(f.Owner.Manager,manager)&&f.Updates.HandleList.Count==1,"existing model is reused and receives completion before scheduling");
                    f.Updates.QueueRemove(f.Owner.Update);f.Updates.ProcessRemovals();f.Disposables=null;manager.IsInit=true;
                    Throws<InvalidOperationException>(()=>f.Init(()=>throw new InvalidOperationException("preload completion")));
                    Require(!manager.IsInit&&f.Updates.HandleList.Count==0&&f.Disposables==null,"failed synchronous completion stops later controller scheduling");
                }
            });
            return report;
        }
    }
}
