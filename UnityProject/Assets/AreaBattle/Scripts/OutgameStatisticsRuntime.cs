using System;
using UnityEngine;
namespace AreaBattle
{
    // Concrete CommonGameModule statistics graph. The app provides actual account/storage/SDK/HTTP
    // dependencies; clocks, UpdateManager, refresh singleton and frame disposal share their owners.
    public sealed class OutgameStatisticsRuntime
    {
        readonly Func<OutgameStatisticsControl> control;
        readonly OutgameStatisticsControlServices controlServices;
        public readonly OutgameStatisticsExpansion Expansion;
        public readonly OutgameStatisticsOffNetServices OffNet;
        public OutgameStatisticsControl Control=>control();
        public OutgameStatisticsManager Manager=>Control.Manager;
        public OutgameStatisticsRuntime(Func<OutgameDataManagerPool> pool,OutgameFrameEntry frame,IOutgameControllerPlatform platform,
            IOutgameDataStorageHost storageHost,OutgameSdkStringStorage storage,OutgameDataVersionState versions,Action<string> download,
            Func<OutgameMessageDispatcher> messages,Func<bool> hasHttpHelper,Action requestServerTime,
            Action<object[]> log,Action<object[]> error,Action<string> warning,Func<OutgameStatisticsControl> control=null)
        {
            this.control=control??(()=>OutgameStatisticsControl.Shared);
            Expansion=new OutgameStatisticsExpansion(this.control,error);
            OffNet=new OutgameStatisticsOffNetServices{Messages=messages,ServerTime=platform.GetServerTimeByServerTimeZone,
                HasHttpHelper=hasHttpHelper,RequestServerTime=requestServerTime,IsReleaseVersion=()=>platform.IsReleaseVersion,
                LocalNow=()=>platform.LocalNow,RealtimeSinceStartup=()=>Time.realtimeSinceStartup,DeltaTime=()=>Time.deltaTime,
                Updates=()=>OutgameUpdateManager.Instance,Warning=warning,Error=error};
            OutgameTimeToRefreshControl.Services=new OutgameRefreshServices{IsReleaseVersion=()=>platform.IsReleaseVersion,
                ServerTime=platform.GetServerTimeByServerTimeZone,LocalNow=()=>platform.LocalNow,Messages=messages,
                GetDisposableActions=()=>frame.DisposableActions,SetDisposableActions=a=>frame.DisposableActions=a};
            OutgameTimeToRefreshControl.BindStatistics(OffNet);
            controlServices=new OutgameStatisticsControlServices{Pool=pool,AddUpdate=OutgameUpdateManager.AddHandle,
                RemoveUpdate=OutgameUpdateManager.RemoveHandle,GetDisposableActions=()=>frame.DisposableActions,SetDisposableActions=a=>frame.DisposableActions=a,
                ClearCommonMessages=OutgameCommonMessageDispatcher.ClearEvent,
                CreateManager=()=>new OutgameStatisticsManager(
                    new OutgameDataManagerStorage(()=>"CommonGameModuleGameStatisticsManager",storageHost,storage,versions),storageHost,download,
                    ()=>new OutgameStatisticsOffNetStrategy(pool,()=>OutgameCommonMessageDispatcher.Shared,this.control,Expansion,OffNet),
                    messages,()=>OutgameCommonMessageDispatcher.Shared,this.control,Expansion,log)};
        }
        public void Initialize(Action complete)=>Control.OnInit(controlServices,complete);
    }
}
