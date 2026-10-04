using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameUserInfoValidation
    {
        sealed class StorageHost:IOutgameDataStorageHost
        {
            public bool Server;public int SourceLoginProgress=>10; // Explicit isolated test account.
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>Server;
            public string MineGameName=>"Proj_hdzd";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){throw new Exception(s);}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;
            public void QueueUpload(string k,string s){throw new Exception("Unexpected test upload");}
        }
        sealed class Fixture
        {
            public readonly StorageHost Host=new StorageHost();
            public readonly OutgameFileStorageBackend Backend;
            public readonly OutgameUserInfoManager Manager;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameLegacyConfigManager Config;
            public readonly List<string> Trace=new List<string>();
            public Func<long> Clock=()=>1234567890123L;
            public bool Install;public int Downloads,Names;
            public Fixture(string path=null)
            {
                Backend=new OutgameFileStorageBackend(path??Path.Combine(Path.GetTempPath(),"AreaBattleUserInfo-"+Guid.NewGuid().ToString("N")),a=>{Trace.Add("write");a();});
                var storage=new OutgameDataManagerStorage(()=>"UserInfoManager",Host,
                    new OutgameSdkStringStorage(Backend,s=>{throw new Exception(s);}),new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{}));
                Manager=new OutgameUserInfoManager(storage,Host,s=>{Require(s=="UserInfoManager","download key");Downloads++;},
                    ()=>Clock(),()=>Install,(count,mode)=>{Require(count==1&&!mode,"original RandAIInfo arguments");Names++;return new List<string>{"fixture-name"};},()=>Messages);
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});
                reader.ReadTable(Config.dicHeadport);reader.ReadTable(Config.dicHeadBox);
                OutgameCoreControllerBindings.BindUserInfo(Registry,()=>Pool,()=>Config,s=>Trace.Add("role:"+s));
            }
            public void Initialize()=>Pool.OnInit(true,"Proj_hdzd",new[]{new OutgameManagerRegistration(4229,"Proj_hdzd",true,false,()=>Manager)});
            public OutgameUserInfoControl Control=>(OutgameUserInfoControl)Registry.Resolve(4228);
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Throws<T>(Action action) where T:Exception
        {try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameUserInfoData Held()=>new OutgameUserInfoData {uname="held",headPort=new List<int>{7},headBox=new List<int>{8},installTime=9};
        public static BattleBuild.Report Run()
        {
            var result=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();result.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){result.passed=false;result.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("user-info-original-defaults-real-file-save-and-independent-reload",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleUserInfo-"+Guid.NewGuid().ToString("N"));
                var f=new Fixture(path);f.Initialize();var d=f.Manager.Data;
                Require(d.uname=="fixture-name"&&d.headportId==1&&d.hdboxId==1&&d.hasHeadport==3&&d.hasHdBox==1,"source constructor values and explicit name provider");
                Require(string.Join(",",d.headPort)=="1,2,3,4"&&string.Join(",",d.headBox)=="1,2"&&d.installTime==1234567890123L&&f.Names==1,"original fresh lists and time");
                f.Pool.SaveData();var next=new Fixture(path);next.Initialize();
                Require(next.Manager.Name==d.uname&&next.Manager.InstallTime==d.installTime&&next.Names==0,"real disk reload preserves name and Int64 timestamp");
                next.Manager.OnRelease();Require(next.Manager.Data==null,"source release clears data");
            });
            check("user-info-source-list-repair-native-null-json-and-whitespace-name",()=>{
                var f=new Fixture();var d=Held();d.headBox.Clear();d.uname=" ";f.Manager.UpdateDataCallBack(JsonUtility.ToJson(d));
                Require(string.Join(",",f.Manager.Data.headPort)=="1,2,3,4"&&string.Join(",",f.Manager.Data.headBox)=="1,2"&&f.Names==0,"either empty list replaces both; whitespace is not empty");
                f.Manager.UpdateDataCallBack("{\"uname\":\"held-null-json\",\"headPort\":null,\"headBox\":[8]}");
                Require(f.Manager.Data.uname=="held-null-json"&&string.Join(",",f.Manager.Data.headPort)=="1,2,3,4"&&string.Join(",",f.Manager.Data.headBox)=="1,2","native JsonUtility converts list null to empty before original repair");
                f.Manager.Data.headPort=null;Throws<NullReferenceException>(()=>f.Manager.HaveHeadPort(1));
            });
            check("user-info-icon-message-precedes-store-and-rereads-reentrant-record",()=>{
                var f=new Fixture();f.Manager.Data=Held();var old=f.Manager.Data;var replacement=Held();int calls=0;
                f.Messages.AddListener("HeadportChange",args=>{Require(f.Manager.Icon==1&&(int)args[0]==4,"listener observes old selection and requested id");calls++;f.Manager.Data=replacement;});
                f.Manager.Icon=4;Require(calls==1&&old.headportId==1&&replacement.headportId==4,"setter writes current record after callback");
                f.Manager.Icon=4;Require(calls==1,"same value skips message");
                f.Messages.AddListener("HeadBoxChange",args=>{throw new InvalidOperationException();});
                Throws<InvalidOperationException>(()=>f.Manager.IconBox=3);Require(f.Manager.IconBox==1,"failed listener prevents selection write");
            });
            check("user-info-install-time-gates-and-create-failure-retain-source-state",()=>{
                var f=new Fixture();f.Manager.Data=Held();f.Manager.Data.installTime=0;f.Install=true;int reads=0;
                f.Clock=()=>{reads++;return 42;};f.Manager.CheckInstallVersion();Require(reads==0&&f.Manager.InstallTime==0,"install-version early return");
                f.Install=false;f.Manager.CheckInstallVersion();f.Manager.CheckInstallVersion();Require(reads==1&&f.Manager.InstallTime==42,"fill only exact zero");
                var old=f.Manager.Data;f.Clock=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Manager.CreateNewData);
                Require(ReferenceEquals(old,f.Manager.Data),"publish fresh data only after timestamp and lists");
            });
            check("user-info-controller-config-gating-persistence-and-report-order",()=>{
                var f=new Fixture();f.Initialize();var control=f.Control;control.OnInit();
                Require(ReferenceEquals(control.Manager,f.Manager)&&control.GetHeadportConfig()!=null&&control.GetHeadBoxConfig()!=null,"actual registry/pool/default config binding");
                f.Config.dicHeadport[999]=new HeadportConfig{id=999};f.Trace.Clear();
                f.Messages.AddListener("HeadportChange",args=>{Require(f.Manager.Icon==1,"notify before selection");f.Trace.Add("icon");});
                control.ApplyHeadport(999);Require(!control.IsHeadPortUnlock(999)&&control.Icon==999&&string.Join(",",f.Trace)=="icon,write","source accepts configured locked id and saves after selection");
                f.Trace.Clear();control.ApplyHeadbox(-100);Require(f.Trace.Count==0,"unknown config does not save");
                control.ApplyName("renamed");Require(string.Join(",",f.Trace)=="write,role:renamed","name persists before external report");
                Require(OutgameUserInfoDataFrom(f.Backend).uname=="renamed","actual stored name");
                control.OnDispose();Require(!f.Registry.HasInstance(4228)&&ReferenceEquals(control.Manager,f.Manager),"dispose clears singleton while retaining field");
            });
            check("user-info-real-avatar-item-shares-manager-record-and-unlock-order",()=>{
                var f=new Fixture();f.Initialize();f.Control.OnInit();
                var configs=new OutgameItemConfigSlot(a=>{});configs.Instance.InitLegacyUnityJson(new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{}));
                AreaBattle.SharedItemConfig.GameItemConfig config=null;
                foreach(var row in configs.Instance.Items.Values)if(row.type1==1&&row.type2==10){config=row;break;}
                Require(config!=null,"original head-box item exists");f.Manager.Data.headBox.Clear();int messages=0;
                f.Messages.AddListener("UserInfo_HeadBoxUnlock",args=>{Require(f.Manager.HaveHeadBox((int)args[0]),"append before notification");messages++;});
                var services=new OutgameItemEntityServices {Config=()=>configs.Instance,Virtual=new OutgameVirtualItemServices{HeadBoxes=()=>f.Manager.HeadBoxes}};
                var item=new OutgameItemFactory(config.id,()=>configs.Instance,s=>{},services.Construct).Produce();
                item.AddItemOnlyModel(0);item.AddItem(100);f.Control.UnlockHeadBox(config.paramInt);
                Require(messages==1&&f.Manager.Data.headBox.Count==1,"entity/control share original deduplicated unlock irrespective of amount");
                f.Pool.SaveData();Require(OutgameUserInfoDataFrom(f.Backend).headBox[0]==config.paramInt,"real manager save includes item unlock");
                f.Manager.AddHeadBox(config.paramInt);Require(messages==2&&f.Manager.Data.headBox.Count==2,"manager direct Add preserves source duplicates");
            });
            check("user-info-server-startup-waits-for-callback",()=>{
                var f=new Fixture();f.Host.Server=true;f.Initialize();Require(f.Downloads==1&&f.Manager.Data==null&&f.Names==0,"download request does not fabricate data");
                f.Manager.UpdateDataCallBack(JsonUtility.ToJson(Held()));f.Control.OnInit();
                Require(f.Control.Name=="held"&&f.Names==0,"explicit callback unblocks actual controller");
            });
            return result;
        }
        static OutgameUserInfoData OutgameUserInfoDataFrom(OutgameFileStorageBackend backend)
            =>JsonUtility.FromJson<OutgameUserInfoData>(backend.Get("Proj_hdzdUserInfoManager"));
        public static void Validate()
        {
            var report=Run();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/user-info-validation.json"),JsonUtility.ToJson(report,true));
            Debug.Log("USER_INFO_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);
            if(Application.isBatchMode)EditorApplication.Exit(report.passed?0:1);
        }
    }
}
