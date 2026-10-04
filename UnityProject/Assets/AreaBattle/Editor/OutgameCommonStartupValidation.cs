using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameCommonStartupValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        public sealed class Fixture:IDisposable
        {
            readonly Func<int,OutgameActivityConfigRow> oldResolver=OutgameActivityItemData.ConfigurationResolver;
            readonly OutgameLimitTaskModelServices oldModels=OutgameLimitTaskModels.Services;
            readonly OutgameRefreshServices oldRefresh=OutgameTimeToRefreshControl.Services;
            public readonly OutgameStatisticsOffNetValidation.Host StorageHost=new OutgameStatisticsOffNetValidation.Host();
            public readonly OutgameFrameEntryPlayModeValidation.Platform Platform=new OutgameFrameEntryPlayModeValidation.Platform();
            public readonly List<string> Trace=new List<string>(),Downloads=new List<string>();
            public readonly List<object[]> Errors=new List<object[]>();
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameSdkStringStorage Strings;
            public readonly OutgameFrameEntry Frame=new OutgameFrameEntry(new OutgameFrameServices());
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly OutgameStatisticsControl StatisticsOwner=new OutgameStatisticsControl();
            public readonly OutgameStatisticsRuntime Statistics;
            public readonly OutgameActivityConfigRuntime Config;
            public readonly OutgameActivityControl Control=new OutgameActivityControl();
            public readonly OutgameActivityRuntime Activities;
            public readonly OutgameLegacyConfigManager Legacy;
            public readonly OutgameCommonPreLoadHost Host;
            public readonly OutgamePreLoadApplicationServices ApplicationServices;
            public readonly OutgameControllerRegistry Controllers=new OutgameControllerRegistry();
            public readonly OutgameSevendayActivityServices SevenServices;
            public readonly OutgameCommanderManager Commanders;
            public readonly OutgameSevendayValidation.EntryHost EntryHost=new OutgameSevendayValidation.EntryHost();
            public readonly OutgameStartupEntry Entry;
            public bool Flag64,Flag65;public int Flag65Reads;
            public readonly string PathName;
            public Fixture(string path=null)
            {
                PathName=path??Path.Combine(Path.GetTempPath(),"AreaBattleCommonStartup-"+Guid.NewGuid().ToString("N"));
                Strings=new OutgameSdkStringStorage(new OutgameFileStorageBackend(PathName,a=>a()),s=>Errors.Add(new object[]{s}));
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>Errors.Add(new object[]{s}));Pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>());
                var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{});
                Statistics=new OutgameStatisticsRuntime(()=>Pool,Frame,Platform,StorageHost,Strings,versions,Downloads.Add,()=>Messages,()=>false,()=>throw new Exception("unexpected HTTP"),a=>{},Errors.Add,s=>{},()=>StatisticsOwner);
                var json=new OutgameActivityConfigUnityJson(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>Errors.Add(new object[]{s}));
                Config=new OutgameActivityConfigRuntime(new OutgameActivityConfigReaderServices{OnlineParameter=k=>null,UseBinary=()=>false,ReadLocalActivities=json.ReadActivities,ReadLocalSettings=json.ReadSettings,Error=Errors.Add},
                    new OutgameActivityCustomConfigServices{UseBinary=()=>false,Json=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>Errors.Add(new object[]{s}))},Errors.Add);
                var items=new OutgameGlobalItemRewardsValidation.Fixture();var reports=new OutgameActivityControlValidation.Reports();var ui=new OutgameActivityControlValidation.Ui();
                var activity=new OutgameActivityServices{FsmManager=()=>new OutgameFsmManager(),Ui=()=>ui,Format=(s,a)=>string.Format(s,a),ItemFactoryError=s=>Errors.Add(new object[]{s})};
                var control=new OutgameActivityControlServices{Current=()=>Control,Pool=()=>Pool,Items=()=>items.Engine,AssemblyTypes=()=>Array.Empty<Type>(),CreateActivity=t=>throw new Exception("unbound activity"),
                    Messages=()=>Messages,Common=()=>OutgameCommonMessageDispatcher.Shared,Statistics=Statistics.Expansion,Reports=()=>reports,Localize=n=>n.key,Error=Errors.Add};
                var managers=new OutgameActivityManagerServices{AssemblyTypes=()=>Array.Empty<Type>(),CreateManager=t=>throw new Exception("unbound manager"),SourceTypeIndex=t=>throw new Exception("unbound type"),SourceAutoSyn=t=>false,Error=Errors.Add,
                    OffNet=new OutgameActivityOffNetServices{GetNowTimeInt=()=>1700000000,Warning=s=>{},Error=Errors.Add}};
                Activities=new OutgameActivityRuntime(Config,control,activity,managers,Frame,StorageHost,Strings,versions,Downloads.Add);
                var taskServices=new OutgameLimitTimeTaskServices{Rewards=()=>items.Engine,Entities=items.Services,Reports=()=>new OutgameLimitTimeTaskValidation.Reports(),Localize=n=>n.key,LocalizeBusiness=n=>n.key,Log=a=>{},LogByColor=(c,a)=>{},Error=Errors.Add};
                new OutgameLimitTimeTaskRuntime(Activities,taskServices,()=>new OutgameNoviceTaskManager(new OutgameDataManagerStorage(()=>"CommonGameModuleNoviceTaskManager",StorageHost,Strings,versions),StorageHost,Downloads.Add,
                    new OutgameNoviceTaskManagerServices{Config=()=>Config.Manager,Warning=s=>{},Error=Errors.Add}),()=>items.Config.Instance);
                Legacy=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>6,()=>0,()=>null,(p,c,a)=>{});
                Legacy.statisticEventConfig=JsonUtility.FromJson<StatisticEventConfig>(Resources.Load<TextAsset>("Recovered/FirstPack/Config/StatisticEventConfig").text);
                Commanders=new OutgameCommanderManager(new OutgameProfile(),Resources.Load<TextAsset>("Recovered/FirstPack/Config/CommanderConfig").text,new OutgameDataManagerStorage(()=>"CommanderManager",StorageHost,Strings,versions),StorageHost,Downloads.Add);
                Pool.AddModel(4028,Commanders,true);Controllers.Bind(4027,()=>new OutgameCommanderControl(()=>Pool,Controllers));Controllers.Resolve(4027).OnInit();
                SevenServices=new OutgameSevendayActivityServices{Activities=()=>Control,Config=()=>Legacy,Skins=()=>OutgameSkinCatalog.FromOriginal("",Resources.Load<TextAsset>("Recovered/FirstPack/Config/SkinConfig").text,Resources.Load<TextAsset>("Recovered/FirstPack/Config/SceneSkinConfig").text),CurrentLevel=()=>11,Commanders=()=>(OutgameCommanderControl)Controllers.Resolve(4027),Statistics=Statistics.Expansion};
                OutgameCoreControllerBindings.BindSevenday(Controllers,SevenServices);Entry=new OutgameStartupEntry(EntryHost,()=>Trace.Add("migrate"),Controllers);
                ApplicationServices=new OutgamePreLoadApplicationServices{Config=()=>Legacy,LogProcedure=Trace.Add,LogWarning=Trace.Add,InitializeUserNetModule=()=>Trace.Add("user-net"),Handle103VersionBug=()=>Trace.Add("repair"),ClearStateEvents=()=>Trace.Add("clear"),
                    LoginSourceFlag64=()=>Flag64,LoginSourceFlag65=()=>{Flag65Reads++;return Flag65;}};
                Host=new OutgameCommonPreLoadHost(ApplicationServices,Statistics,Activities);
            }
            public OutgameSevendayActivityControl Seven=>(OutgameSevendayActivityControl)Controllers.Resolve(4502);
            public OutgameChildLimitTimeTaskActivity Child=>Control.GetActivity<OutgameLimitTimeTaskActivity>(1301001).GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            public void Dispose()
            {
                try
                {
                    if(Control.Services!=null&&Control.Manager!=null)Control.Cleanup();
                    if(StatisticsOwner.Manager!=null){StatisticsOwner.Manager.OnRelease();StatisticsOwner.Dispose();}
                    OutgameUpdateManager.Instance.ProcessRemovals();
                    var refresh=GameObject.Find("TimeToRefreshControl");if(refresh!=null)UnityEngine.Object.DestroyImmediate(refresh);
                }
                finally{OutgameTimeToRefreshControl.Services=oldRefresh;OutgameActivityItemData.ConfigurationResolver=oldResolver;OutgameLimitTaskModels.Services=oldModels;}
            }
        }
        public sealed class Next:IOutgameFsmState<IOutgameProcedureManager>
        {
            public int Enters;public Action Entering;
            public void OnInit(OutgameFsm<IOutgameProcedureManager> f){}public void OnEnter(OutgameFsm<IOutgameProcedureManager> f){Enters++;Entering?.Invoke();}
            public void OnEnter(OutgameFsm<IOutgameProcedureManager> f,object[] args)=>throw new Exception("wrong overload");
            public void OnUpdate(OutgameFsm<IOutgameProcedureManager> f,float d,float u){}public void OnLeave(OutgameFsm<IOutgameProcedureManager> f,bool shutdown){}public void OnDestroy(OutgameFsm<IOutgameProcedureManager> f){}
        }
    }
}
