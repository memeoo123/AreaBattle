using System;
using System.Linq;
namespace AreaBattle
{
    // Application composition for the recovered parent, child, manager and item model.
    // Call before ActivityRuntime.Init; storage/account/report delivery remain supplied
    // by the application. Existing activity and manager registrations are preserved.
    public sealed class OutgameLimitTimeTaskRuntime
    {
        public readonly OutgameActivityRuntime Activities;
        public readonly OutgameLimitTimeTaskServices Services;
        public readonly OutgameLimitTaskModelServices Models;
        public OutgameLimitTimeTaskRuntime(OutgameActivityRuntime activities,OutgameLimitTimeTaskServices services,
            Func<OutgameNoviceTaskManager> createManager,Func<OutgameItemConfigManager> items)
        {
            Activities=activities;Services=services;
            services.Config=()=>activities.Config.Manager;services.Control=()=>activities.Control;services.Pool=activities.Services.Pool;
            activities.ActivityServices.ItemConfig=items;
            var activityTypes=activities.Services.AssemblyTypes;var createActivity=activities.Services.CreateActivity;
            activities.Services.AssemblyTypes=()=>activityTypes().Concat(new[]{typeof(OutgameLimitTimeTaskActivity)}).Distinct().ToArray();
            activities.Services.CreateActivity=t=>t==typeof(OutgameLimitTimeTaskActivity)?new OutgameLimitTimeTaskActivity(activities.ActivityServices,services):createActivity(t);
            var managers=activities.ManagerServices;var managerTypes=managers.AssemblyTypes;var make=managers.CreateManager;
            var sourceType=managers.SourceTypeIndex;var autoSyn=managers.SourceAutoSyn;
            managers.AssemblyTypes=()=>managerTypes().Concat(new[]{typeof(OutgameNoviceTaskManager)}).Distinct().ToArray();
            managers.CreateManager=t=>t==typeof(OutgameNoviceTaskManager)?createManager():make(t);
            managers.SourceTypeIndex=t=>t==typeof(OutgameNoviceTaskManager)?4711:sourceType(t);
            managers.SourceAutoSyn=t=>t==typeof(OutgameNoviceTaskManager)||autoSyn(t);
            Models=new OutgameLimitTaskModelServices{Config=services.Config,GetActivity=id=>services.Control().GetActivity<OutgameLimitTimeTaskActivity>(id),
                Items=items,Statistics=activities.Services.Statistics,Error=services.Error};
            OutgameLimitTaskModels.Services=Models;
        }
    }
}
