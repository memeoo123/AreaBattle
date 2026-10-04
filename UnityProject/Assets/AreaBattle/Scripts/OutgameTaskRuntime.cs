using System;
using System.Linq;
namespace AreaBattle
{
    // Install before ActivityRuntime.Init; concrete account, item and report hosts are required.
    public sealed class OutgameTaskRuntime
    {
        public readonly OutgameTaskServices Services;
        public readonly OutgameTaskModelServices Models;
        public readonly OutgameTaskManagerServices ManagerServices;
        public OutgameTaskRuntime(OutgameActivityRuntime activities,OutgameTaskServices tasks,
            Func<OutgameTaskManagerServices,OutgameTaskManager> createManager,Func<OutgameItemConfigManager> items,
            Func<string,object[],string> languageFormat,Action<string> warning,Action<object[]> log)
        {
            Services=tasks;tasks.Config=()=>activities.Config.Manager;tasks.Control=()=>activities.Control;tasks.Pool=activities.Services.Pool;
            activities.ActivityServices.ItemConfig=items;
            ManagerServices=new OutgameTaskManagerServices{Config=tasks.Config,Pool=tasks.Pool,GetActivity=id=>tasks.Control().GetActivity<OutgameTaskActivity>(id),Warning=warning,Error=tasks.Error,Log=log};
            var types=activities.Services.AssemblyTypes;var make=activities.Services.CreateActivity;
            activities.Services.AssemblyTypes=()=>types().Concat(new[]{typeof(OutgameTaskActivity)}).Distinct().ToArray();
            activities.Services.CreateActivity=t=>t==typeof(OutgameTaskActivity)?new OutgameTaskActivity(activities.ActivityServices,tasks):make(t);
            var managers=activities.ManagerServices;var managerTypes=managers.AssemblyTypes;var makeManager=managers.CreateManager;
            var source=managers.SourceTypeIndex;var sync=managers.SourceAutoSyn;
            managers.AssemblyTypes=()=>managerTypes().Concat(new[]{typeof(OutgameTaskManager)}).Distinct().ToArray();
            managers.CreateManager=t=>t==typeof(OutgameTaskManager)?createManager(ManagerServices):makeManager(t);
            managers.SourceTypeIndex=t=>t==typeof(OutgameTaskManager)?4692:source(t);
            managers.SourceAutoSyn=t=>t==typeof(OutgameTaskManager)||sync(t);
            Models=new OutgameTaskModelServices{Config=tasks.Config,LanguageFormat=languageFormat};OutgameTaskModels.Services=Models;
        }
    }
}
