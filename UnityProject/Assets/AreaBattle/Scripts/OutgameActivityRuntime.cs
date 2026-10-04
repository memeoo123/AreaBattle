using System;
using UnityEngine;
namespace AreaBattle
{
    // Composes the recovered control/config/offline-data/FSM owners. Concrete activity
    // types/managers, reports, UI, item service and account storage are supplied by the app.
    public sealed class OutgameActivityRuntime
    {
        public readonly OutgameActivityConfigRuntime Config;
        public readonly OutgameActivityControlServices Services;
        public readonly OutgameActivityManagerServices ManagerServices;
        public readonly OutgameActivityServices ActivityServices;
        public OutgameActivityControl Control=>Services.Current();
        public OutgameActivityRuntime(OutgameActivityConfigRuntime config,OutgameActivityControlServices services,
            OutgameActivityServices activity,OutgameActivityManagerServices managers,OutgameFrameEntry frame,
            IOutgameDataStorageHost storageHost,OutgameSdkStringStorage strings,OutgameDataVersionState versions,Action<string> download)
        {
            Config=config;Services=services;ManagerServices=managers;ActivityServices=activity;
            Services.Current=Services.Current??(()=>OutgameActivityControl.Shared);
            Services.Config=()=>Config.Manager;Services.Activity=activity;
            Services.GetDisposableActions=()=>frame.DisposableActions;Services.SetDisposableActions=a=>frame.DisposableActions=a;
            Services.AddUpdate=OutgameUpdateManager.AddHandle;Services.RemoveUpdate=OutgameUpdateManager.RemoveHandle;
            Services.UnscaledDeltaTime=()=>Time.unscaledDeltaTime;
            Services.AddRefreshHandle=(hour,last,refresh,countdown,seconds)=>OutgameTimeToRefreshControl.Instance.AddRefreshHandle(hour,last,refresh,countdown,seconds);
            Services.RemoveRefreshHandle=(hour,refresh,countdown,args)=>OutgameTimeToRefreshControl.Instance.RemoveRefreshHandle(hour,refresh,countdown,args);
            Services.CreateManager=()=>new OutgameActivityManager(new OutgameDataManagerStorage(()=>"CommonGameModuleActivityManager",storageHost,strings,versions),storageHost,download,managers);
            managers.RegistrationCallback=()=>Control.RegisterCommonManagerDel;managers.Pool=services.Pool;
            managers.Configurations=()=>Config.Manager.Activities;managers.DataReady=()=>Control.DataReady();
            managers.OffNet.Statistics=services.Statistics;managers.OffNet.Configurations=managers.Configurations;managers.OffNet.SetDirty=b=>Control.SetDirty(b);
            activity.GetItemData=id=>Control.GetItemData(id);activity.GetConfig=id=>Config.Manager.GetActivityConfig(id);
            activity.Common=services.Common;activity.Statistics=services.Statistics;activity.SetDirty=b=>Control.SetDirty(b);
            activity.Launch=(a,id)=>Control.Launch(a,id);activity.Notice=(a,id)=>Control.Notice(a,id);activity.Over=(a,id)=>Control.Over(a,id);
            activity.Closed=id=>Control.Closed(id);activity.QueueNotice=e=>Control.QueueNotice(e);activity.QueueLaunch=e=>Control.QueueLaunch(e);
        }
        public void Init(OutgameActivityInitData data=null)
        {
            Control.Services=Services;
            OutgameActivityItemData.ConfigurationResolver=id=>Config.Manager.GetActivityConfig(id);
            Control.Init(data);
        }
    }
}
