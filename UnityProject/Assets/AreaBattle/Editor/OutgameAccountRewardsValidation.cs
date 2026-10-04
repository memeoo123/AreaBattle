using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameAccountRewardsValidation
    {
        sealed class LowestRandom:System.Random {public override int Next(int min,int max)=>min;}
        sealed class HighestRandom:System.Random {public override int Next(int min,int max)=>max-1;}
        static AssetBundle bundle;
        public static AssetBundle ConfigBundle
        {
            get
            {
                if(bundle)return bundle;
                string folder=Path.Combine(BattleBuild.Workspace,"analysis/account-rewards-config-bundles");Directory.CreateDirectory(folder);
                string[] names={"GameItemConfig","GamePackageConfig","GameRewardConfig","GameProductConfig"};
                var build=new AssetBundleBuild{assetBundleName="account-items",assetNames=names.Select(n=>"Assets/AreaBattle/Resources/Recovered/FirstPack/Config/"+n+".bytes").ToArray(),addressableNames=names.Select(n=>n+".bytes").ToArray()};
                // Native bundle loading is the production legacy reader path. Build from original
                // imported bytes so this fixture has no dependency on previous test artifacts.
                if(!BuildPipeline.BuildAssetBundles(folder,new[]{build},BuildAssetBundleOptions.ChunkBasedCompression,EditorUserBuildSettings.activeBuildTarget))throw new Exception("config bundle build");
                bundle=AssetBundle.LoadFromFile(Path.Combine(folder,"account-items"));if(!bundle)throw new Exception("config bundle load");return bundle;
            }
        }
        public static void PrepareConfig(){var asset=ConfigBundle;}
        public static void LoadPreparedConfig(){if(!bundle)bundle=AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b=>b.name=="account-items");if(!bundle)bundle=AssetBundle.LoadFromFile(Path.Combine(BattleBuild.Workspace,"analysis/account-rewards-config-bundles/account-items"));if(!bundle)throw new Exception("prepared bundle load");}
        public sealed class AccountHost:IOutgameDataStorageHost
        {
            readonly IOutgameDataStorageHost underlying;public int Progress=10;public bool Server;public readonly List<string> Downloads=new List<string>();
            public AccountHost(IOutgameDataStorageHost underlying){this.underlying=underlying;}
            public int SourceLoginProgress=>Progress;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>Server;public string MineGameName=>underlying.MineGameName;public bool HasToast=>false;
            public void Log(string s)=>underlying.Log(s);public void Error(string s)=>underlying.Error(s);public void Toast(string s)=>underlying.Toast(s);
            public string Compress(string k,string s)=>underlying.Compress(k,s);public string Decompress(string k,string s)=>underlying.Decompress(k,s);public void QueueUpload(string k,string s)=>throw new Exception("No simulated server upload success");
        }
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameTaskPageValidation.Fixture Page;
            public OutgameTaskActivityValidation.Fixture Tasks {get;private set;}
            public OutgameStatisticsOffNetValidation.Fixture Statistics=>Tasks.Runtime.Statistics;
            public OutgameAccountItemRuntime Account {get;private set;}
            public OutgameAccountItemServices Services {get;private set;}
            public readonly OutgameGlobalItemRewardsValidation.Reports Reports=new OutgameGlobalItemRewardsValidation.Reports();
            public readonly List<string> Trace=new List<string>(),Errors=new List<string>();public readonly List<object[]> ItemErrors=new List<object[]>();
            public AccountHost Host {get;private set;}
            public OutgameGlobalItemLifecycle Global=>Account.Items.Global.Instance.Lifecycle;
            public string PathName=>Statistics.PathName;
            public Action SavedRegistration;
            public Fixture(bool native=false,string path=null)
            {
                Page=new OutgameTaskPageValidation.Fixture(native,path,InitializeAccount);
                Account.Items.BindController(Page.Data.Registry,()=>Statistics.Pool,()=>Statistics.Messages,Errors.Add);Page.Data.Registry.Resolve(3875).OnInit();
                Page.Data.Registry.Bind(4256,()=>Account.Tool);Page.Data.Registry.Resolve(4256).OnInit();
            }
            void InitializeAccount(OutgameTaskActivityValidation.Fixture tasks)
            {
                Tasks=tasks;Host=new AccountHost(Statistics.StorageHost);
                var legacy=new OutgameLegacyConfigReadState(Errors.Add){ConfigResource=new OutgameLegacyPrefabResource(ConfigBundle)};
                Services=new OutgameAccountItemServices{Pool=()=>Statistics.Pool,Profile=new OutgameProfile(),Legacy=legacy,StorageHost=Host,Storage=Statistics.Strings,
                    AddUpdate=Statistics.Updates.Register,RemoveUpdate=Statistics.Updates.QueueRemove,Versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},Errors.Add),Now=()=>new DateTime(2026,10,3,12,0,0),Download=Host.Downloads.Add,
                    Skills=BattleView.ReadText("Data/AllSkillConfig"),Products=Resources.Load<TextAsset>("Recovered/FirstPack/Config/GameProductConfig").text,
                    GameItems=Resources.Load<TextAsset>("Recovered/FirstPack/Config/GameItemConfig").text,SceneSkins=BattleView.ReadText("Data/Outgame/SceneSkinConfig"),SoldierSkins=BattleView.ReadText("Data/Outgame/SkinConfig"),
                    RegisterSaveData=(m,s)=>{Trace.Add("local-loaded");SavedRegistration?.Invoke();},VirtualItems=new OutgameVirtualItemServices(),Messages=()=>Statistics.Messages,Reports=()=>Reports,PurchaseCost=()=>"PurchaseCost",Error=Errors.Add,Warning=ItemErrors.Add,ItemError=ItemErrors.Add,
                    Tools=new OutgameAccountToolEndpoints{RefreshTopInfo=()=>Trace.Add("top"),GoldSpent=(id,n)=>Statistics.Expansion.AddEventCount(Page.Data.Rows.Config.statisticEventConfig.UseCoinTotal,id,n),ToolChanged=id=>Trace.Add("tool:"+id),UnlockScene=id=>throw new Exception("unexpected skin"),UnlockSoldier=id=>throw new Exception("unexpected skin"),MissingItemEntity=id=>Trace.Add("missing:"+id),ReportGet=(id,c,n,b,s)=>Trace.Add("get:"+id),ReportCost=(id,c,n,b)=>Trace.Add("cost:"+id)}};
                Account=new OutgameAccountItemRuntime(Services);
                var points=new OutgameDicePoints(()=>Global,()=>Statistics.Messages);Services.VirtualItems.DicePoints=()=>points;
                // Add the original concrete managers to the SAME pool already owned by statistics
                // and activities; original source registration attributes are applied explicitly.
                Register(Account.LocalRegistration);Register(Account.Items.Registration);
                OutgameActivityRewardBinding.Bind(Tasks.Runtime.Runtime,Account.Items,Tasks.Binding,Tasks.AchievementBinding);
            }
            void Register(OutgameManagerRegistration r){var m=r.Create();m.ParticipatesInSync=r.AutoSyn;m.CompressData=r.CompressData;m.OnInit();Statistics.Pool.Managers[r.SourceTypeIndex]=m;}
            public void ReadyDaily(int id=1){var t=Tasks.Child.ModuleData.tasks.Single(r=>r.id==id);for(int i=0;i<t.conditions.Count;i++)t.conditions[i].value=t.Config.conditionParams[i].datas.Last();}
            public void ClaimDaily(int id=1){ReadyDaily(id);Tasks.Child.TaskComplete(Tasks.Child.ModuleData.tasks.Single(t=>t.id==id).uid);}
            public string Stored(string key)=>Statistics.Strings.GetString(Host.MineGameName+key,"");
            public OutgameTaskData SavedTasks=>JsonUtility.FromJson<OutgameTaskData>(OutgameActivityCodec.DecompressString(Stored("CommonGameModuleTaskMgr"),Errors.Add));
            public void Dispose()
            {
                Account.Items.Manager.Instance.OnRelease();Statistics.Updates.ProcessRemovals();Page.Dispose();
            }
        }
        static void Require(bool b,string why){if(!b)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Task/Achievement page and actual account ItemManager/LocalDataManager/Tool/data-pool/file-storage composition. Original config native bundle and live statistics owners. Account login state, reports/skins/presentation/download endpoints remain explicit fixture services; full production Main/platform/Player is not claimed."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="account-rewards-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="account-rewards-"+id,result="fail",detail=e.ToString()});}};
            check("same-pool-real-item-local-config-and-statistics-owners",()=>{
                using(var f=new Fixture()){
                    Require(ReferenceEquals(f.Statistics.Pool.Managers[4119],f.Account.Local)&&ReferenceEquals(f.Statistics.Pool.Managers[4500],f.Account.Items.Manager.Instance)&&ReferenceEquals(f.Global.RewardHost,f.Account.Items.Manager.Instance),"shared real manager graph");
                    Require(f.Account.Items.Config.Instance.Items.Count==79&&f.Account.Items.Config.Instance.Items[1003].paramInt==130001&&f.Account.Items.Config.Instance.Items[1004].type1==1,"actual original config, no synthetic achievement point override");
                    Require(ReferenceEquals(f.Tasks.Services.Entities,f.Account.Items.Entities)&&ReferenceEquals(f.Tasks.AchievementBinding.Services.Entities,f.Account.Items.Entities)&&f.Statistics.Expansion.GameValue(10020,new object[]{1001})==0,"both task families and statistics resolve actual account inventory");
                }
            });
            check("daily-claim-before-reward-and-shared-pool-save-order",()=>{
                using(var f=new Fixture()){
                    f.ClaimDaily();Require(f.Global.GetItemCount(1001)==50&&f.Account.Local.GoldNum==50&&f.Tasks.Child.ModuleData.ext.livenessValue==20,"real source fifty gold and twenty liveness");
                    Require(OutgameLocalRecord.Read(f.Stored("LocalDataManager")).goldNum==50&&OutgameItemManagerData.ReadOriginal(f.Stored("ItemManager")).itemUserDatas.Single(i=>i.itemId==1001).itemCount==50,"ToolChange saves both actual economic records through shared pool");
                    var before=f.SavedTasks.datas.Single(d=>d.activityID==130001);Require(before.tasks.Single(t=>t.id==1).state==1&&before.ext.livenessValue==20&&f.Tasks.Parent.Dirty,"daily state is set first and model rewards add liveness before manager delivery saves pool");
                    f.Tasks.Parent.Update();var after=f.SavedTasks.datas.Single(d=>d.activityID==130001);Require(after.tasks.Single(t=>t.id==1).state==1&&after.ext.livenessValue==20&&!f.Tasks.Parent.Dirty,"actual activity dirty update persists claim and virtual liveness afterwards");
                }
            });
            check("daily-currency-claim-and-liveness-independent-file-restart",()=>{
                string path;using(var f=new Fixture()){f.ClaimDaily();f.Tasks.Parent.Update();path=f.PathName;}
                using(var f=new Fixture(path:path)){Require(f.Global.GetItemCount(1001)==50&&f.Account.Local.GoldNum==50&&f.Account.Inventory.Count(1001)==50&&f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==1&&f.Tasks.Child.ModuleData.ext.livenessValue==20,"new owners restore both currency records and task state from same account directory");f.Page.Start();Require(!f.Page.Subviews.Daily.I.Rows.ContainsKey(1),"original page excludes persisted claimed task");}
            });
            check("achievement-reward-economy-save-order-and-final-account-save",()=>{
                string path;using(var f=new Fixture()){
                    var a=f.Page.Data.Achievement;a.FindAchievement(1).progress=30;a.GetAchievementReward(1);
                    Require(f.Global.GetItemCount(1001)==200&&f.Account.Local.GoldNum==200&&a.FindAchievement(1).state==1,"actual original achievement award reaches same account gold");
                    var early=JsonUtility.FromJson<OutgameAchievementSaveData>(f.Stored("CommonGameModuleAchievementMgr"));Require(early.ids.Count==0,"currency-triggered pool save precedes achievement claimed state");
                    f.Statistics.Pool.SaveData();path=f.PathName;
                }
                using(var f=new Fixture(path:path)){Require(f.Global.GetItemCount(1001)==200&&f.Account.Local.GoldNum==200&&f.Page.Data.Achievement.FindAchievement(1).state==1,"whole account save/restart restores original achievement and economic records");}
            });
            check("daily-liveness-award-shares-account-economy",()=>{
                using(var f=new Fixture()){
                    f.Tasks.Child.ModuleData.ext.livenessValue=30;f.Tasks.Child.Strategy.GetLivenessReward(1);Require(f.Global.GetItemCount(1001)==50&&f.Account.Local.GoldNum==50,"actual tier reward reaches shared manager and local money");f.Tasks.Parent.Update();Require(f.SavedTasks.datas.Single(d=>d.activityID==130001).ext.livenessAward!=0,"liveness claim bit saved after reward");
                }
            });
            check("login-gate-and-save-disabled-are-not-bypassed",()=>{
                using(var f=new Fixture()){
                    f.Host.Progress=9;f.ClaimDaily();Require(f.Global.GetItemCount(1001)==50&&f.Stored("ItemManager")==""&&f.Stored("LocalDataManager")=="","unsatisfied account gate does not fabricate persisted economics");
                }
                using(var f=new Fixture()){
                    f.Statistics.Pool.SetSaveDisabled(true);f.ClaimDaily();f.Tasks.Parent.Update();Require(f.Stored("ItemManager")==""&&f.Stored("LocalDataManager")==""&&f.Tasks.Parent.Dirty,"pool disabled blocks automatic economy and dirty task save");
                }
            });
            check("report-failure-retains-global-prefix-before-local-and-task",()=>{
                using(var f=new Fixture()){
                    f.Reports.Sending=()=>throw new InvalidOperationException("report");Throws<InvalidOperationException>(()=>f.ClaimDaily());Require(f.Global.GetItemCount(1001)==50&&f.Account.Local.GoldNum==0&&f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1).state==1&&f.Stored("ItemManager")=="","daily claim is marked before rewards; failed report retains global prefix and skips manager/local save");
                }
            });
            check("tool-item-entity-package-path-persists-both-real-currencies",()=>{
                string path;long gold,diamonds;using(var f=new Fixture()){
                    f.Account.Items.Entities.Packages.Random=new GameRandomSource(new LowestRandom());
                    Require(f.Account.Tool.ToolChange(20000,1,false,"",false),"source Tool type6 invokes actual package entity");gold=f.Global.GetItemCount(1001);diamonds=f.Global.GetItemCount(1002);
                    Require(gold>=10&&gold<=20&&diamonds>=5&&diamonds<=10&&f.Account.Local.GoldNum==gold&&f.Account.Local.DiamondsNum==diamonds&&f.Account.Items.Manager.Instance.PackageOpenRewardsTemp.Count==2,"original random package ranges and manager/local delivery");path=f.PathName;
                }
                using(var f=new Fixture(path:path)){Require(f.Global.GetItemCount(1001)==gold&&f.Global.GetItemCount(1002)==diamonds&&f.Account.Local.GoldNum==gold&&f.Account.Local.DiamondsNum==diamonds,"Tool type6 package and nested rewards persist in both manager records");}
            });
            check("package-virtual-points-and-piece-branches-use-real-inventory",()=>{
                string path;using(var f=new Fixture()){
                    f.Account.Items.Entities.Packages.Random=new GameRandomSource(new HighestRandom());int notices=0;f.Statistics.Messages.AddListener("Mxtz_DataChange",a=>{Require((int)a[0]==1&&(int)a[1]==10,"source points notice reads actual changed stock");notices++;});
                    f.Account.Tool.ToolChange(20000,1,false,"",false);Require(f.Global.GetItemCount(11001)==10&&f.Global.GetItemCount(8401)==3&&notices==1&&f.Account.Local.GoldNum==0,"nested source package8000 draws three8401 pieces while outer package grants ten points");path=f.PathName;
                }
                using(var f=new Fixture(path:path)){Require(f.Global.GetItemCount(11001)==10&&f.Global.GetItemCount(8401)==3&&f.Services.VirtualItems.DicePoints().GetPoint()==10,"independent file restart restores virtual points/pieces without inventing local currency");}
            });
            check("local-server-reload-rebinds-dispatcher-to-current-record",()=>{
                using(var f=new Fixture()){
                    var old=f.Account.Inventory;f.Account.Local.UpdateDataCallBack("{\"goldNum\":123}");f.Account.Tool.ToolChange(1001,7,false,"",false);
                    Require(old.Count(1001)==0&&f.Account.Local.GoldNum==130&&f.Account.Inventory.Count(1001)==130&&OutgameLocalRecord.Read(f.Stored("LocalDataManager")).goldNum==130,"new source LocalData projection used after callback; old inventory unchanged");
                }
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/account-rewards-validation.json"),JsonUtility.ToJson(r,true));return r;
        }
    }
}
