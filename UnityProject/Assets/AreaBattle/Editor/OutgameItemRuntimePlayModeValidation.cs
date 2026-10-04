using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad]
    public static class OutgameItemRuntimePlayModeValidation
    {
        const string Pending="AreaBattle.ItemRuntimeValidation";
        const string StoragePath="AreaBattle.ItemRuntimeValidation.Storage";
        static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/product-services-native-bundles");
        [Serializable] sealed class Report
        {public bool passed;public string error,unityVersion,platform;public List<string> checks=new List<string>();
         public string limitations="Native item/product/profile service groups only. Login/report endpoints are explicit test fixtures; no complete Main/menu, payment, SDK delivery or Player claim.";}
        // Explicit test account state. Production composition requires the real host.
        sealed class StorageHost:IOutgameDataStorageHost
        {
            public int Progress;public bool Server;public int SourceLoginProgress=>Progress;
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;
            public bool IsUseServer=>Server;public string MineGameName=>"Proj_hdzd";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){throw new Exception(s);}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;
            public void QueueUpload(string k,string s){throw new InvalidOperationException("Unexpected test upload");}
        }
        sealed class Fixture
        {
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameItemRuntime Runtime;
            public readonly OutgameUserInfoRuntime UserInfo;
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly StorageHost Host=new StorageHost();
            public readonly OutgameFileStorageBackend Backend;
            public readonly List<object[]> PriceErrors=new List<object[]>();
            public readonly List<string> Events=new List<string>();
            public int Downloads;
            public Fixture(AssetBundle bundle,bool server=false)
            {
                Host.Server=server;
                Backend=new OutgameFileStorageBackend(SessionState.GetString(StoragePath,""),a=>a());
                var legacy=new OutgameLegacyConfigReadState(s=>{throw new Exception(s);}) {
                    ConfigResource=new OutgameLegacyPrefabResource(bundle)};
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});
                var reports=new OutgameGlobalItemRewardsValidation.Reports();
                var storage=new OutgameSdkStringStorage(Backend,s=>{throw new Exception(s);});
                var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{});
                var virtualItems=new OutgameVirtualItemServices();
                Config=new OutgameLegacyConfigManager(legacy,new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                legacy.Reader.ReadTable(Config.dicAIName);legacy.Reader.ReadTable(Config.dicCountryConfig);
                legacy.Reader.ReadTable(Config.dicHeadport);legacy.Reader.ReadTable(Config.dicHeadBox);
                UserInfo=new OutgameUserInfoRuntime(()=>Pool,()=>Config,Host,storage,versions,s=>Downloads++,
                    ()=>123456789,()=>false,()=>Messages,s=>{},a=>{throw new Exception("Unexpected random error");});
                UserInfo.BindController(Registry,virtualItems);
                Runtime=new OutgameItemRuntime(()=>Pool,legacy,Host,storage,
                    versions,s=>Downloads++,
                    ()=>throw new InvalidOperationException("Model-only test must not use Tool"),id=>6,
                    virtualItems,()=>Messages,()=>reports,s=>{throw new Exception(s);},a=>{},PriceErrors.Add);
                Messages.AddListener("ItemUI_RefreshStore",a=>Events.Add("refresh:"+a[0]));
                Messages.AddListener("ItemUI_ProductReset",a=>Events.Add("reset:"+a[0]+":"+a[1]));
            }
            public void Initialize()
            {
                Pool.OnInit(true,"Proj_hdzd",new[]{UserInfo.Registration,Runtime.Registration});
                // Server data arrives before the source controller startup phase.
                if(!Host.Server)Registry.Resolve(4228).OnInit();
                Runtime.BindController(Registry,()=>Pool,()=>Messages,s=>{});
                Registry.Resolve(3875).OnInit();
            }
        }
        static Report report;static AssetBundle bundle;static Fixture first,restarted,server;
        static int phase;static double started,pauseStarted;static float pausedTime;static bool done;
        static OutgameUpdateManager updates;
        static OutgameItemRuntimePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch validation only");
            Directory.CreateDirectory(Folder);
            var names=new[]{"GameItemConfig","GamePackageConfig","GameRewardConfig","GameProductConfig","AINameConfig","CountryConfigConfig","HeadportConfig","HeadBoxConfig"};
            var paths=Array.ConvertAll(names,n=>"Assets/AreaBattle/Resources/Recovered/FirstPack/Config/"+n+".bytes");
            var build=new AssetBundleBuild{assetBundleName="item-config",assetNames=paths,addressableNames=Array.ConvertAll(names,n=>n+".bytes")};
            var manifest=BuildPipeline.BuildAssetBundles(Folder,new[]{build},BuildAssetBundleOptions.ChunkBasedCompression,EditorUserBuildSettings.activeBuildTarget);
            if(!manifest)throw new Exception("Native config bundle build failed");
            SessionState.SetString(StoragePath,Path.Combine(Path.GetTempPath(),"AreaBattleItemValidation-"+Guid.NewGuid().ToString("N")));
            SessionState.SetBool(Pending,true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string text)
        {if(!condition)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            done=false;phase=0;started=EditorApplication.timeSinceStartup;
            report=new Report{unityVersion=Application.unityVersion,platform=Application.platform.ToString()};
            try{
                bundle=AssetBundle.LoadFromFile(Path.Combine(Folder,"item-config"));
                Check(bundle&&bundle.LoadAsset<TextAsset>("GameProductConfig")!=null,"Native bundle and original basename config lookup");
                first=new Fixture(bundle);
                var seed=new OutgameItemManagerData{lastExitDayOfYear=DateTime.Now.DayOfYear==1?366:DateTime.Now.DayOfYear-1};
                seed.itemUserDatas.Add(new OutgameItemUserData{itemId=1001,itemCount=3});
                seed.productUserDatas.Add(new OutgameProductUserData{m_uniqueId=77,productId=100101,haveBuyTimes=2,buyCount=0});
                first.Backend.Set("Proj_hdzdItemManager",seed.SerializeRecord(),s=>{throw new Exception(s);},()=>{});
                first.Initialize();updates=OutgameUpdateManager.Instance;
                var user=(OutgameUserInfoManager)first.Pool.Managers[4229];bool originalName=false;
                for(int id=1;id<98;id++)originalName|=first.Config.dicAIName[id].zh_cn==user.Name;
                Check(originalName&&user.InstallTime==123456789,"Native resource tables generate original profile name and retain supplied timestamp");
                var runtime=first.Runtime;var manager=runtime.Manager.Instance;
                Check(ReferenceEquals(manager,first.Pool.Managers[4500])&&ReferenceEquals(manager,runtime.Global.Instance.Lifecycle.RewardHost),"Pool registration, manager slot and global host share production owner");
                Check(runtime.Config.Instance.Items.Count==79&&runtime.Config.Instance.Products.Count==16,"Shared config reads original four tables through native legacy resource");
                var control=(OutgameItemModuleControl)first.Registry.Resolve(3875);
                Check(ReferenceEquals(control.ItemMgr,manager)&&ReferenceEquals(control.Items,runtime.Config.Instance.Items),"Item controller uses registered manager and shared config");
                Check(updates.HandleList.Count==1&&runtime.Global.Instance.Lifecycle.ProductUpdates!=null,"Production runtime registers real product scheduler with native UpdateManager");
                updates.IsPause=true;pausedTime=runtime.Global.Instance.Lifecycle.ProductUpdates.AccumulatedTime;pauseStarted=EditorApplication.timeSinceStartup;
                EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(done)return;
            try{
                if(EditorApplication.timeSinceStartup-started>30)throw new TimeoutException("Item runtime validation phase "+phase);
                if(phase==0){
                    if(EditorApplication.timeSinceStartup-pauseStarted<.15)return;
                    Check(first.Runtime.Global.Instance.Lifecycle.ProductUpdates.AccumulatedTime==pausedTime&&first.Events.Count==0,"Native update pause prevents product scheduling");
                    updates.IsPause=false;phase=1;return;
                }
                if(phase==1){
                    if(first.Events.Count==0)return;
                    var global=first.Runtime.Global.Instance.Lifecycle;
                    Check(string.Join(",",first.Events)=="refresh:77,reset:77:100101"&&global.Indexes.Products[77].buyCount==2,"Actual Unity frames run daily reset and ordered UI notifications");
                    Check(first.PriceErrors.Count==1&&global.Indexes.Products[77].consumeItemPrice==0,"Missing original shared price parameters retain source caught-error behavior");
                    first.Pool.SaveData();
                    Check(OutgameItemManagerData.ReadOriginal(first.Backend.Get("Proj_hdzdItemManager")).productUserDatas[0].buyCount==0,"Production persistence still obeys unsatisfied login gate");
                    Check(string.IsNullOrEmpty(first.Backend.Get("Proj_hdzdUserInfoManager")),"Profile persistence also waits for actual account login gate");
                    first.Host.Progress=10; // Explicit fixture transition, never a runtime login implementation.
                    first.Runtime.Manager.Instance.GetItem(first.Runtime.Config.Instance.Items[1001]).AddItemOnlyModel(9);
                    int unlocks=0;first.Messages.AddListener("UserInfo_HeadBoxUnlock",a=>unlocks++);
                    var user=(OutgameUserInfoManager)first.Pool.Managers[4229];
                    var avatar=first.Runtime.Manager.Instance.GetItem(first.Runtime.Config.Instance.Items[10004]);
                    avatar.AddItemOnlyModel(0);avatar.AddItem(100);
                    Check(user.HaveHeadBox(4)&&unlocks==1,"Native item factory shares profile owner and unlocks original avatar exactly once");
                    first.Pool.SaveData();
                    var savedUser=JsonUtility.FromJson<OutgameUserInfoData>(first.Backend.Get("Proj_hdzdUserInfoManager"));
                    Check(savedUser.uname==user.Name&&savedUser.headBox.Contains(4),"Pool writes generated profile and virtual reward to actual isolated file storage");
                    var held=OutgameItemManagerData.ReadOriginal(first.Backend.Get("Proj_hdzdItemManager"));
                    Check(held.itemUserDatas[0].itemCount==12&&held.productUserDatas[0].buyCount==2,"Actual file backend saves model reward and native product reset");
                    first.Runtime.Manager.Instance.OnRelease();
                    Check(!first.Runtime.Global.HasInstance&&!first.Runtime.Config.HasInstance&&updates.RemoveIds.Count==1,"Release clears slots and queues source native handle removal");
                    phase=2;return;
                }
                if(phase==2){
                    if(updates.IndexDict.Count!=0)return;
                    Check(updates.HandleList.Count==0,"Following native frame removes released product callback");
                    restarted=new Fixture(bundle);restarted.Initialize();
                    Check(restarted.Runtime.Manager.Instance.GetItemNum(1001)==12&&restarted.Runtime.Global.Instance.Lifecycle.Indexes.Products[77].buyCount==2,"Independent service group restores actual disk record");
                    var previous=(OutgameUserInfoManager)first.Pool.Managers[4229];var restored=(OutgameUserInfoManager)restarted.Pool.Managers[4229];
                    Check(restored.Name==previous.Name&&restored.HaveHeadBox(4)&&restored.InstallTime==previous.InstallTime,"Independent native profile owner restores name, reward and install time");
                    server=new Fixture(bundle,true);server.Initialize();
                    Check(server.Downloads==2&&!server.Runtime.Global.HasInstance&&updates.HandleList.Count==1,"Both registered server models request data without inventing completion or scheduling");
                    var pending=(OutgameUserInfoManager)server.Pool.Managers[4229];
                    Check(pending.Data==null,"Server-backed profile remains absent until explicit download callback");
                    pending.UpdateDataCallBack(server.Backend.Get("Proj_hdzdUserInfoManager"));server.Registry.Resolve(4228).OnInit();
                    Check(pending.Name==restored.Name&&ReferenceEquals(((OutgameUserInfoControl)server.Registry.Resolve(4228)).Manager,pending),"Explicit server profile callback precedes real controller initialization");
                    server.Runtime.Manager.Instance.UpdateDataCallBack(server.Backend.Get("Proj_hdzdItemManager"));
                    Check(server.Runtime.Manager.Instance.GetItemNum(1001)==12&&updates.HandleList.Count==2,"Explicit download callback initializes source owner and frame registration");
                    restarted.Runtime.Manager.Instance.OnRelease();server.Runtime.Manager.Instance.OnRelease();phase=3;return;
                }
                if(phase==3){
                    if(updates.IndexDict.Count!=0)return;
                    Check(updates.HandleList.Count==0&&updates.RemoveIds.Count==0,"Both independent owners release their native callbacks");
                    Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(done)return;done=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);
            report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/item-runtime-native-validation.json"),JsonUtility.ToJson(report,true));
            if(bundle)bundle.Unload(true);
            if(error!=null)Debug.LogException(error);
            EditorApplication.Exit(error==null?0:1);
        }
    }
}
