using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameControllerLifecycleValidation
    {
        sealed class Backend:IOutgameStorageBackend
        {public string Get(string key)=>null;public void Set(string k,string v,Action<string> f,Action c)=>c();public void Remove(string k,Action<string> f,Action c)=>c();public void Clear(Action<string> f,Action c)=>c();}
        sealed class StorageHost:IOutgameDataStorageHost
        {public int SourceLoginProgress=>0;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>false;public string MineGameName=>"controller-fixture";public bool HasToast=>false;public void Log(string s){}public void Error(string s){}public void Toast(string s){}public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;public void QueueUpload(string k,string s){throw new Exception("No online fixture");}}
        static OutgameLegacyConfigManager Config()
        {
            var state=new OutgameLegacyConfigReadState(s=>{throw new Exception(s);});
            var config=new OutgameLegacyConfigManager(state,null,null,null,null,null,null);
            var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),s=>{throw new Exception(s);});
            reader.ReadTable(config.dicChannel);reader.ReadTable(config.dicChannelProcedure);return config;
        }
        static string ChannelName(int n)=>n== -1?"NOAB":n>=0&&n<5?"channel"+(char)('A'+n):n.ToString();
        static void Require(bool value,string text){if(!value)throw new Exception(text);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("source-controller-channel-selection-real-config-and-old-account",()=>{
                var config=Config();var trace=new List<string>();var registry=new OutgameControllerRegistry();int channel=1;bool install=true;
                registry.Bind(3903,()=>new OutgameProcessControl(()=>config,()=>channel,()=>install,n=>{channel=n;trace.Add("set:"+n);},(color,s)=>{Require(color==Color.green,"source green logging");trace.Add(s);},trace.Add,ChannelName,registry));
                var control=(OutgameProcessControl)registry.Resolve(3903);control.SDK_StartLevelCount=12;control.SDK_CompleteLevelCount=7;control.Flag8=true;control.ActiveUpdate=true;control.OnInit();
                Require(control.IsBChannel&&control.ProcedureConfig.id==2&&control.SDK_StartLevelCount==0&&control.SDK_CompleteLevelCount==0&&!control.Flag8&&control.ActiveUpdate,"source field reset set");
                for(int platform=1;platform<=3;platform++)for(int ab= -1;ab<5;ab++)Require(control.GetChannelProcedureConfig(ab,platform).id==Math.Max(1,ab+1),"source six-way selection");
                Require(control.GetChannelProcedureConfig(5,999)==null&&control.GetChannelProcedureConfig(-2,999)==null,"unknown enum skips config lookup");
                Require(control.JudgeChannel(1)&&!control.JudgeChannel(0)&&!control.JudgeChannel(),"Judge compares procedure id with input+1");
                trace.Clear();install=false;control.OnInit();Require(channel== -1&&control.ProcedureConfig.id==1&&string.Join("|",trace)=="当前为老用户|set:-1|当前获取到的AB测试: channelB|当前渠道: RPAndroid|当前流程id: 1","old user reset retains original channel in log");
                control.OnDispose();Require(!registry.HasInstance(3903)&&control.ProcedureConfig.id==1&&control.ActiveUpdate,"dispose only flag8 and singleton");
            });
            test("source-controller-channel-failure-keeps-earlier-writes",()=>{
                var config=Config();var registry=new OutgameControllerRegistry();bool install=true;int channel=1;
                var control=new OutgameProcessControl(()=>config,()=>channel,()=>install,n=>channel=n,(c,s)=>{},s=>{},ChannelName,registry);control.OnInit();var held=control.ProcedureConfig;
                config.dicChannel.Clear();control.Flag8=true;control.SDK_StartLevelCount=9;bool failed=false;
                try{control.OnInit();}catch(KeyNotFoundException){failed=true;}
                Require(failed&&!control.Flag8&&control.SDK_StartLevelCount==0&&ReferenceEquals(held,control.ProcedureConfig),"failed rhs leaves previous config and reset fields");
                channel=55;control.OnInit();Require(control.ProcedureConfig==null&&!control.JudgeChannel((int[])null),"unknown channel stores null; Judge short circuits null params");
                bool nullFailed=false;try{_ = control.IsBChannel;}catch(NullReferenceException){nullFailed=true;}Require(nullFailed,"source IsBChannel dereferences missing config");
            });
            test("source-controller-prefab-pool-recycle-and-unregistered-destroy",()=>{
                var registry=new OutgameControllerRegistry();var warnings=new List<string>();GameObject prefab=new GameObject("source-prefab"),parent=new GameObject("parent"),root=null;var loose=new List<GameObject>();
                try{
                    registry.Bind(4561,()=>new OutgamePrefabPoolControl(registry,warnings.Add,warnings.Add,g=>{},g=>UnityEngine.Object.DestroyImmediate(g)));
                    var control=(OutgamePrefabPoolControl)registry.Resolve(4561);Require(ReferenceEquals(control,registry.Resolve(4561)),"singleton identity");control.OnInit();root=control.Root;
                    Require(root.name=="Pool"&&!root.activeSelf,"native root hidden");control.CreatePool(prefab,2);Require(prefab.activeSelf&&root.transform.childCount==2,"prewarm restores active prefab");
                    var first=control.Spawn(prefab,parent.transform,new Vector3(2,3,4),Vector3.one*2,Quaternion.Euler(0,20,0));loose.Add(first);
                    Require(first.activeSelf&&first.transform.parent==parent.transform&&first.transform.localPosition==new Vector3(2,3,4),"reuse applies native transform");control.Recycle(first);Require(!first.activeSelf&&first.transform.parent==root.transform,"recycle returns under inactive root");
                    var second=control.Spawn(prefab,parent.transform);loose.Add(second);Require(second!=first,"FIFO remaining prewarm precedes recycled item");control.Recycle(second);
                    var outsider=new GameObject("unregistered");loose.Add(outsider);var copy=control.Spawn(outsider,parent.transform);loose.Add(copy);control.Recycle(copy);Require(copy==null&&warnings.Count==1,"unregistered spawn destroys on recycle");
                    var old=control;control.OnDispose();root=null;Require(!registry.HasInstance(4561)&&control.Root==null,"clear singleton and destroy native root");Require(!ReferenceEquals(old,registry.Resolve(4561)),"new reference after disposal");
                }finally{foreach(var g in loose)if(g)UnityEngine.Object.DestroyImmediate(g);if(root)UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(parent);}
            });
            test("source-controller-prefab-pool-failed-prewarm-preserves-side-effects",()=>{
                var registry=new OutgameControllerRegistry();var prefab=new GameObject("failure-prefab");GameObject clone=null;
                try{
                    var control=new OutgamePrefabPoolControl(registry,s=>{},s=>{},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));bool failed=false;
                    try{control.CreatePool(prefab,1);}catch(NullReferenceException){failed=true;}
                    Require(failed&&!prefab.activeSelf,"root failure leaves prefab inactive");
                    // Original inserted the pool before instantiation; retry does not repair it.
                    control.CreatePool(prefab,1);Require(!prefab.activeSelf,"existing entry suppresses second prewarm");
                    foreach(var g in Resources.FindObjectsOfTypeAll<GameObject>())if(g.name=="failure-prefab(Clone)")clone=g;
                    Require(clone!=null,"instantiated child survives failure before parent operation");
                }finally{if(clone)UnityEngine.Object.DestroyImmediate(clone);UnityEngine.Object.DestroyImmediate(prefab);}
            });
            test("source-controller-commander-real-pool-reference-and-reinitialization",()=>{
                var warnings=new List<string>();var pool=new OutgameDataManagerPool(()=>{},warnings.Add,s=>{},s=>{});var registry=new OutgameControllerRegistry();
                registry.Bind(4027,()=>new OutgameCommanderControl(()=>pool,registry));var control=(OutgameCommanderControl)registry.Resolve(4027);control.OnInit();Require(control.Manager==null&&warnings[0]=="未找到[CommanderManager]数据管理器","missing pool warns without invented model");
                var host=new StorageHost();var storage=new OutgameDataManagerStorage(()=>"CommanderManager",host,new OutgameSdkStringStorage(new Backend(),s=>{}),new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{}));
                var manager=new OutgameCommanderManager(new OutgameProfile(),BattleView.ReadText("Data/Outgame/CommanderConfig"),storage,host,s=>{throw new Exception(s);});
                pool.OnInit(true,"test",new[]{new OutgameManagerRegistration(4028,"test",false,false,()=>manager)});control.OnInit();Require(ReferenceEquals(control.Manager,manager)&&manager.GetCommanderDatas().Count>0,"concrete source manager initialized and bound");
                control.OnDispose();Require(ReferenceEquals(control.Manager,manager)&&!registry.HasInstance(4027),"dispose retains manager field but resets singleton");
                var newer=registry.Resolve(4027);control.OnDispose();Require(!registry.HasInstance(4027)&&!ReferenceEquals(control,newer),"old instance disposal clears replacement slot as source static write");
                bool unknown=false;try{registry.Resolve(4034);}catch(KeyNotFoundException){unknown=true;}Require(unknown,"unimplemented controller has no empty fallback");
            });
            test("source-controller-local-data-events-purchases-and-disposal",()=>{
                var profile=new OutgameProfile();var host=new StorageHost();var warnings=new List<string>();
                var storage=new OutgameDataManagerStorage(()=>"LocalDataManager",host,new OutgameSdkStringStorage(new Backend(),warnings.Add),new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},warnings.Add));
                var manager=new OutgameLocalDataManager(profile,BattleView.ReadText("Data/AllSkillConfig"),"{\"Datas\":[]}",()=>new DateTime(2026,9,30),storage,host,s=>{},(m,s)=>{});
                var pool=new OutgameDataManagerPool(()=>{},warnings.Add,s=>{},s=>{});pool.OnInit(true,"test",new[]{new OutgameManagerRegistration(4119,"test",false,false,()=>manager)});
                var registry=new OutgameControllerRegistry();var messages=new OutgameMessageDispatcher();int today=200;long now=123456789;
                registry.Bind(4118,()=>new OutgameLocalDataControl(()=>pool,registry,()=>messages,()=>today,()=>now,null));
                var control=(OutgameLocalDataControl)registry.Resolve(4118);Require(control.GetCoinNum()==0&&!control.GetJewelPackFirstPurchaseStatus("x"),"null manager safe getters");control.OnInit();
                profile.inventory.goldNum=123;profile.inventory.diamondsNum=456;Require(control.GetCoinNum()==123&&control.GetJewelNum()==456,"shared live inventory");
                control.SetJewelPackFirstPurchaseStatus("");control.SetJewelPackFirstPurchaseStatus("x");control.SetJewelPackFirstPurchaseStatus("x");Require(control.GetJewelPackFirstPurchaseStatus("x")&&manager.Record.PurchasedJewelKey.Count==1,"nonempty purchase key once");
                manager.Record.jumpData.jumpNum=7;messages.SendMessage("GamePlayState",new object[]{3});Require(manager.Record.jumpData.jumpNum==0&&manager.Record.jumpData.jumpDayOfYear==200,"day change resets skip count");
                manager.Record.jumpData.jumpNum=9;messages.SendMessage("GamePlayState",new object[]{3});Require(manager.Record.jumpData.jumpNum==9,"same day retains skip count");
                messages.SendMessage("GamePlayState",new object[]{9});messages.SendMessage("GamePlayState",new object[]{9});Require(manager.Record.jumpData.defeatNum==2,"failure increments");messages.SendMessage("GamePlayState",new object[]{8});Require(manager.Record.jumpData.defeatNum==0,"win resets defeats");
                messages.SendMessage("IapSuccess");Require(manager.Record.firstChargeData.hasCharge&&manager.Record.firstChargeData.firstChargeTime==57600000,"first charge stores original UTC+8 date-start milliseconds");
                now+=86400000;messages.SendMessage("IapSuccess");Require(manager.Record.firstChargeData.firstChargeTime==144000000,"later purchase refreshes date without first-only guard");
                control.OnDispose();messages.SendMessage("GamePlayState",new object[]{9});Require(manager.Record.jumpData.defeatNum==0&&!registry.HasInstance(4118)&&ReferenceEquals(control.Manager,manager),"listeners removed and manager retained");
                var reload=OutgameLocalRecord.Read(manager.Record.ToOriginalJson());Require(reload.PurchasedJewelKey.Contains("x")&&reload.firstChargeData.firstChargeTime==144000000&&reload.jumpData.jumpNum==9,"changed fields serialize in original record");
            });
            test("source-controller-local-data-duplicate-init-and-partial-failure",()=>{
                var profile=new OutgameProfile();var host=new StorageHost();var storage=new OutgameDataManagerStorage(()=>"LocalDataManager",host,new OutgameSdkStringStorage(new Backend(),s=>{}),new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{}));
                var manager=new OutgameLocalDataManager(profile,BattleView.ReadText("Data/AllSkillConfig"),"{\"Datas\":[]}",()=>DateTime.Now,storage,host,s=>{},(m,s)=>{});
                var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});pool.OnInit(true,"t",new[]{new OutgameManagerRegistration(4119,"t",false,false,()=>manager)});
                var registry=new OutgameControllerRegistry();var messages=new OutgameMessageDispatcher();int calls=0;bool fail=false;
                var control=new OutgameLocalDataControl(()=>pool,registry,()=>messages,()=>++calls,()=>throw new InvalidOperationException("clock"),null);
                control.OnInit();control.OnInit();messages.SendMessage("GamePlayState",new object[]{9});Require(manager.Record.jumpData.defeatNum==2,"duplicate init duplicates listeners");control.OnDispose();messages.SendMessage("GamePlayState",new object[]{9});Require(manager.Record.jumpData.defeatNum==3,"dispose removes one occurrence");
                manager.Record.jumpData.jumpNum=12;messages.SendMessage("GamePlayState",new object[]{3});Require(calls==2&&manager.Record.jumpData.jumpDayOfYear==2&&manager.Record.jumpData.jumpNum==0,"day provider read twice");
                manager.Record.firstChargeData=null;try{messages.SendMessage("IapSuccess");}catch(InvalidOperationException){fail=true;}Require(fail&&manager.Record.firstChargeData.hasCharge&&manager.Record.firstChargeData.firstChargeTime==0,"failure preserves new record and early charge flag");
                control.OnDispose();
            });
            test("source-controller-server-time-cadence-day-change-and-debug-projection",()=>{
                var messages=new OutgameMessageDispatcher();var registry=new OutgameControllerRegistry();var trace=new List<string>();long stamp=123456789;int samples=0,network=1;bool release=false;
                registry.Bind(4034,()=>new OutgameServerTimeControl(()=>{samples++;return stamp;},()=>network,()=>new DateTime(2026,9,30),()=>release,()=>messages,registry));
                var control=(OutgameServerTimeControl)registry.Resolve(4034);control.Updata(100,100);Require(samples==0&&control.Elapsed==0,"before init no ticking");control.OnInit();control.OnInit();Require(samples==4&&control.Timestamp==stamp,"first init double refresh; repeat guarded");
                messages.AddListener("RefreshNetTime",a=>trace.Add("refresh"));messages.AddListener("Time_NewDay",a=>trace.Add("day"));
                control.Updata(100,1);Require(samples==4,"exact one second does not refresh");control.Updata(0,.125f);Require(samples==6&&trace.Count==0&&control.PreviousDay==2,"first observed day silent");
                stamp+=86400000;network=0;control.Updata(0,4);Require(samples==8&&trace.Count==2&&string.Join(",",trace)=="refresh,day"&&control.Elapsed==3.125f&&!control.ConnectedNext,"single subtraction/refresh despite long frame; ordered day messages");
                control.DebugOffsetMilliseconds=86400000;Require(control.GetNowTimestampLong()==stamp+86400000&&control.GetTodayOfYear()==3&&control.GetNowTimes()==stamp/1000,"debug offset affects only timestamp getter");release=true;Require(control.GetNowTimestampLong()==stamp,"release ignores offset");
                int held=samples;messages.SendMessage("GamePause",new object[]{true});messages.SendMessage("GamePause");messages.SendMessage("GamePause",Array.Empty<object>());Require(samples==held,"pause and missing args do nothing");messages.SendMessage("GamePause",new object[]{false});Require(samples==held+2,"focus regain refreshes");
                control.ActiveUpdate=true;control.OnDispose();float elapsed=control.Elapsed;long last=control.Timestamp;messages.SendMessage("GamePause",new object[]{false});control.Updata(0,10);Require(!registry.HasInstance(4034)&&!control.Initialized&&!control.ActiveUpdate&&control.Elapsed==elapsed&&control.Timestamp==last,"dispose detaches and preserves clock fields");
            });
            test("source-controller-server-time-partial-failure-and-local-fallback",()=>{
                var messages=new OutgameMessageDispatcher();var registry=new OutgameControllerRegistry();int calls=0;bool fail=true;var local=new DateTime(1970,1,3,8,0,0);
                var control=new OutgameServerTimeControl(()=>{calls++;if(fail)throw new InvalidOperationException("sdk");return 0;},()=>1,()=>local,()=>true,()=>messages,registry);
                bool failed=false;try{control.OnInit();}catch(InvalidOperationException){failed=true;}Require(failed&&control.Initialized&&calls==1,"init marks before first SDK failure");control.OnInit();Require(calls==1,"failed init is still guarded");
                fail=false;messages.SendMessage("GamePause",new object[]{false});Require(control.Timestamp==172800000,"source epoch uses 08:00 unspecified local fallback");control.Updata(0,1.25f);
                local=local.AddDays(1);messages.AddListener("RefreshNetTime",a=>throw new InvalidOperationException("listener"));failed=false;try{control.Updata(0,1);}catch(InvalidOperationException){failed=true;}
                Require(failed&&control.CurrentDay==4&&control.PreviousDay==3&&control.Elapsed==.25f,"day message failure retains prior day and already advanced time");
                control.OnDispose();
            });
            return report;
        }
    }
}
