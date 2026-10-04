using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.SharedItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityOwnerValidation
    {
        static void Require(bool value,string text){if(!value)throw new Exception(text);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Custom:OutgameActivityConfigCustomManager
        {public Action<Action> Init;public Action Release;public override void OnInit(Action complete)=>Init(complete);public override void OnDispose()=>Release?.Invoke();}
        sealed class Reader:OutgameActivityConfigReader
        {public Action<Action> Init;public Action Release;public override void OnInit(Action complete){base.OnInit(complete);Init(complete);}public override void OnDispose()=>Release?.Invoke();}
        sealed class Fixture
        {
            public readonly List<object[]> Errors=new List<object[]>();public readonly List<string> Trace=new List<string>();
            public readonly OutgameActivityConfigManager Manager;public readonly OutgameActivityConfigManagerServices Services;
            public readonly OutgameActivityConfigReaderServices ReaderServices;public readonly OutgameActivityConfigUnityJson Json;
            public int Completed;public string Online;public bool Binary;
            public Fixture()
            {
                Services=new OutgameActivityConfigManagerServices{Current=()=>Manager,AssemblyTypes=()=>Array.Empty<Type>(),CreateCustomManager=t=>throw new Exception("unexpected custom manager"),Error=Errors.Add};
                Manager=new OutgameActivityConfigManager(Services);
                Json=new OutgameActivityConfigUnityJson(name=>{Trace.Add(name);return Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name);},s=>Errors.Add(new object[]{s}));
                ReaderServices=new OutgameActivityConfigReaderServices{Current=()=>Manager,OnlineParameter=key=>{Require(key=="common_ActivityConfig","source online key");return Online;},UseBinary=()=>Binary,
                    ReadLocalActivities=Json.ReadActivities,ReadLocalSettings=Json.ReadSettings,ReadBinaryActivities=d=>throw new Exception("binary host absent"),ReadBinarySettings=()=>throw new Exception("binary settings host absent"),Error=Errors.Add};
                Services.CreateReader=()=>new OutgameActivityLocalConfigReader(ReaderServices);
            }
            public void Read()=>Manager.ReadConfig(()=>Completed++);
        }
        static OutgameActivityConfigRow Row(int id=7,int open=1)=>new OutgameActivityConfigRow{id=id,open=open,uniqueId="one",closeType=Array.Empty<int>(),overType=Array.Empty<int>(),closeParams=new[]{"old"},overParams=new[]{"old"}};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Common ActivityConfigMgr lifecycle/reader, original local resources and online JSON, ActivityBase factory/child lookup. Business-specific config tables/groups and ActivityControl/Main remain pending. Required binary acquisition endpoints are tested explicitly, not implemented MemoryPack or platform success. No new native/Player."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-owner-original-resource-read-and-normalized-conditions",()=>{
                var f=new Fixture();f.Read();Require(f.Manager.Activities.Count==9&&f.Completed==1&&f.Manager.Completed==2&&f.Manager.Expected==2,"real source nine activities complete");
                Require(string.Join(",",f.Trace)=="PubActivityConfig,PubActivitySettingConfig"&&f.Json.ReadCount==1&&f.Errors.Count==0,"local table then settings, table read counter only");
                Require(f.Manager.GetActivityConfig(130001).closeType[0]==-1&&f.Manager.GetActivityConfig(130001).overParams[0]=="999999"&&f.Manager.GetActivityConfig(1301001).closeType.Length==0,"enabled empty conditions normalized; disabled remain empty");
                Require(f.Manager.Settings.dailyTaskResetDate=="00:00"&&f.Manager.Settings.dailyTaskCountLimit==8&&f.Manager.Settings.RandomStoreAutoRefreshType==null,"original setting fields and absent defaults");
            });
            check("activity-owner-online-override-duplicates-and-malformed-prefix",()=>{
                var f=new Fixture{Online="prefix {\"Datas\":[{\"id\":7,\"open\":1,\"closeType\":[],\"overType\":[]},{\"id\":7,\"open\":0},{\"id\":8,\"open\":0,\"closeType\":[],\"overType\":[]}]}"};
                f.Read();Require(f.Manager.Activities.Count==2&&f.Manager.GetActivityConfig(7).open==1&&f.Errors.Count==1&&Equals(f.Errors[0][0],"表[PubActivityConfig]中有相同键(7)")&&f.Trace.Count==1&&f.Completed==1,"online only overrides activities, keeps first duplicate and still reads settings");
                f.Online="bad-json";Throws<ArgumentOutOfRangeException>(f.Read);Require(f.Manager.Completed==2&&f.Completed==1&&f.Manager.Activities.Count==2,"parse failure preserves rows/counters and suppresses completion");
                OutgameActivityLocalConfigReader.ReadOnline(f.Manager.Activities,"{}",a=>{});Require(f.Manager.Activities.Count==2,"null Datas leaves destination intact");
            });
            check("activity-owner-custom-async-completion-and-repeated-callback",()=>{
                var f=new Fixture();Action done=null;int inits=0;f.Services.AssemblyTypes=()=>new[]{typeof(string),typeof(OutgameActivityConfigCustomManager),typeof(Custom)};
                f.Services.CreateCustomManager=t=>new Custom{Init=callback=>{inits++;done=callback;}};f.Read();
                Require(inits==1&&f.Manager.Expected==3&&f.Manager.Completed==2&&f.Completed==0,"only subclasses created; common load can finish while custom pending");
                done();Require(f.Completed==1&&f.Manager.Completed==3,"last custom completes");done();f.Manager.CheckComplete();Require(f.Completed==1&&f.Manager.Completed==4,"over-completion no equality and no implicit dedup/reset");
                f.Manager.Completed=3;f.Manager.CheckComplete();f.Manager.CheckComplete();Require(f.Completed==3,"source completion check can invoke callback repeatedly at equality");
            });
            check("activity-owner-reentrant-callback-and-reread-keeps-lifecycle-state",()=>{
                var f=new Fixture();f.Read();var row=f.Manager.Activities[130001];var reader=f.Manager.Reader;f.Read();
                Require(f.Manager.Completed==4&&f.Completed==1&&f.Errors.Count==9&&ReferenceEquals(row,f.Manager.Activities[130001])&&!ReferenceEquals(reader,f.Manager.Reader),"reread creates reader, retains rows/counters and logs duplicates");
                var g=new Fixture();g.Services.CreateReader=()=>new Reader{Init=complete=>complete()};g.Manager.CompleteAction=()=>{};
                int calls=0;g.Manager.ReadConfig(()=>{calls++;g.Manager.Completed=0;});Require(calls==1&&g.Manager.Completed==0,"caller may mutate counters synchronously");
            });
            check("activity-owner-normalization-live-current-owner-and-failure-prefix",()=>{
                var f=new Fixture();var other=new Fixture();var first=Row();var invalid=Row(8);invalid.overType=null;
                other.Manager.Activities.Add(7,first);other.Manager.Activities.Add(8,invalid);f.Services.Current=()=>other.Manager;
                Throws<NullReferenceException>(f.Manager.CompleteCommonConfig);Require(first.closeType[0]==-1&&first.overType[0]==-1&&invalid.closeType[0]==-1&&f.Manager.Completed==0,"singleton-current rows normalized in order; null array failure leaves prefix without completion");
                invalid.overType=Array.Empty<int>();f.Manager.CompleteCommonConfig();Require(f.Manager.Completed==2&&other.Manager.Completed==0,"receiver owns completion counter even when current config owner differs");
                first.open=2;first.closeType=Array.Empty<int>();first.overType=Array.Empty<int>();f.Manager.CompleteCommonConfig();Require(first.closeType.Length==0&&first.overType.Length==0,"normalization requires exactly open1");
            });
            check("activity-owner-reader-host-selection-and-local-callback-capture",()=>{
                var f=new Fixture{Binary=true};int tables=0,settings=0;var value=new OutgameActivitySettingConfig{mailDuration=3};
                f.ReaderServices.ReadBinaryActivities=d=>{tables++;d.Add(7,Row());f.Binary=false;};f.ReaderServices.ReadLocalSettings=()=>{settings++;return value;};f.Read();
                Require(tables==1&&settings==1&&ReferenceEquals(value,f.Manager.Settings)&&f.Completed==1,"binary flag is reread for settings after table host returns");
                var reader=new OutgameActivityLocalConfigReader(f.ReaderServices);int callback=0;f.Online="{}";f.ReaderServices.ReadLocalSettings=()=>{reader.CompleteAction=()=>throw new Exception("replacement");return value;};reader.OnInit(()=>callback++);
                Require(callback==1,"reader invokes captured method argument, not replacement stored callback");
                f.ReaderServices.ReadLocalSettings=()=>throw new InvalidOperationException("settings");Throws<InvalidOperationException>(()=>reader.OnInit(()=>callback++));Require(callback==1,"settings failure aborts callback");
            });
            check("activity-owner-dispose-retains-fields-and-failure-stops-counter-reset",()=>{
                var f=new Fixture();f.Read();var held=f.Manager.Reader;var setting=f.Manager.Settings;var complete=f.Manager.CompleteAction;var trace=new List<string>();
                f.Manager.Reader=new Reader{Release=()=>{Require(f.Manager.Activities.Count==0,"rows clear first");trace.Add("reader");}};
                f.Manager.CustomManagers.Add(new Custom{Release=()=>trace.Add("custom")});f.Manager.CustomManagers.Add(null);f.Manager.Dispose();
                Require(string.Join(",",trace)=="reader,custom"&&f.Manager.CustomManagers.Count==0&&f.Manager.Completed==0&&f.Manager.Expected==2&&ReferenceEquals(setting,f.Manager.Settings)&&ReferenceEquals(complete,f.Manager.CompleteAction)&&f.Manager.Reader!=null,"release only clears source selected fields");
                f.Manager.Completed=8;f.Manager.Expected=9;f.Manager.Reader=new Reader{Release=()=>throw new InvalidOperationException("release")};Throws<InvalidOperationException>(f.Manager.Dispose);Require(f.Manager.Completed==8&&f.Manager.Expected==9,"release failure prevents counter reset");
                Require(f.Manager.GetActivityConfig(99)==null&&Equals(f.Errors[0][0],"配置中不包含活动id")&&Equals(f.Errors[0][1],99),"missing activity diagnostic payload");
            });
            check("activity-owner-factory-original-null-products-and-reread",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});var manager=slot.Instance;var errors=new List<string>();int lookups=0;
                foreach(int type in new[]{1,2,9,4})
                {
                    manager.Items[7]=new GameItemConfig{id=7,type1=type};lookups=0;var factory=new OutgameActivityFactory(7,()=>{lookups++;return manager;},errors.Add);
                    Require(factory.Produce()==null&&lookups==(type==4?2:4),"source branch creates subfactory and rereads config; source products return null");
                }
                var missing=new OutgameActivityFactory(8,()=>manager,errors.Add);Require(errors.Count==1&&errors[0]=="无法创建当前道具 id :8","missing logs source diagnostic");Throws<NullReferenceException>(()=>missing.Produce());
                manager.Items[7]=new GameItemConfig{id=7,type1=1};var held=new OutgameActivityFactory(7,()=>manager,errors.Add);manager.Items.Remove(7);Require(held.Produce()==null&&errors.Count==2,"subfactory logs missing but its concrete Produce still returns null");
            });
            check("activity-owner-base-child-query-cast-and-factory-dispatch",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});slot.Instance.Items[7]=new GameItemConfig{id=7,type1=9};var services=new OutgameActivityServices{ItemConfig=()=>slot.Instance,ItemFactoryError=s=>{}};
                var owner=new OutgameActivityBase(services);var child=new OutgameActivityBase(services);owner.ChildrenById[7]=child;owner.ChildrenById[8]=null;
                Require(ReferenceEquals(owner.GetChildActivity<OutgameActivityBase>(7),child)&&owner.GetChildActivity<OutgameActivityBase>(8)==null&&owner.GetChildActivity<OutgameActivityBase>(9)==null,"source dictionary lookup and null/missing default");
                Throws<InvalidCastException>(()=>owner.GetChildActivity<OutgameActivityStatesValidation.Probe>(7));Require(owner.ActivityFactoryBase(7).Produce()==null,"base activity factory reaches actual config-based factory");
            });
            check("activity-owner-config-offline-file-state-machine-save-restart",()=>{
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"AreaBattleActivityOwner-"+Guid.NewGuid().ToString("N"));long warm=0;
                for(int pass=0;pass<2;pass++)using(var f=new OutgameActivityStatesValidation.Fixture())
                {
                    var configs=new Fixture{Online="{\"Datas\":[{\"id\":7,\"uniqueId\":\"one\",\"open\":1,\"noticeType\":[701],\"noticeParams\":[\"1\"],\"launchType\":[702],\"launchParams\":[\"1\"],\"overType\":[],\"overParams\":[],\"closeType\":[],\"closeParams\":[],\"noticeDes\":{\"key\":\"Need {0}\"}}]}"};
                    var strings=new OutgameSdkStringStorage(new OutgameFileStorageBackend(path,a=>a()),s=>{});
                    var storage=new OutgameDataManagerStorage(()=>"CommonGameModuleActivityManager",f.Statistics.StorageHost,strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
                    OutgameActivityManager manager=null;
                    var services=new OutgameActivityManagerServices{RegistrationCallback=()=>null,AssemblyTypes=()=>Array.Empty<Type>(),Configurations=()=>configs.Manager.Activities,Pool=()=>f.Statistics.Pool,Error=a=>{},
                        DataReady=()=>{f.Config=configs.Manager.GetActivityConfig(7);f.Data=manager.Data.datas.Find(r=>r.id==7);f.Activity.Refresh(7);},
                        OffNet=new OutgameActivityOffNetServices{Statistics=f.Statistics.Expansion,Configurations=()=>configs.Manager.Activities,GetNowTimeInt=()=>1700000000,SetDirty=b=>manager.Dirty=b,Warning=s=>{},Error=a=>{}}};
                    manager=new OutgameActivityManager(storage,f.Statistics.StorageHost,s=>throw new Exception("unexpected download"),services);
                    f.Services.GetConfig=configs.Manager.GetActivityConfig;f.Services.GetItemData=id=>manager.Data.datas.Find(r=>r.id==id);f.Services.SetDirty=b=>manager.Dirty=b;
                    f.Statistics.Pool.AddModel(4658,manager,true);configs.Manager.ReadConfig(manager.Init);
                    Require(ReferenceEquals(f.Activity.Config,configs.Manager.Activities[7])&&ReferenceEquals(f.Activity.Data,manager.Data.datas[0])&&manager.IsInitStrategy,"loaded configuration and actual data manager feed the same activity instance");
                    if(pass==0)
                    {
                        Require(f.Activity.Fsm.CurrentState is OutgameActivityCloseState,"first graph starts closed");f.Set(701,1);warm=f.Data.WarmTimeStamp;
                        Require(f.Activity.Fsm.CurrentState is OutgameActivityNoticeState&&warm>0&&f.Text.text=="Need 1","actual statistics reaches configured notice and widgets");manager.OnSave();
                        Require(strings.GetString(f.Statistics.StorageHost.MineGameName+manager.DataKey,"").Length>0,"actual compressed storage written");
                    }
                    else Require(f.Activity.Fsm.CurrentState is OutgameActivityNoticeState&&f.Data.WarmTimeStamp==warm&&f.Text.text=="Need 1","independent owner/config/manager graph restores saved notice state and warm timestamp");
                    manager.OnRelease();configs.Manager.Dispose();
                }
            });
            return report;
        }
    }
}
