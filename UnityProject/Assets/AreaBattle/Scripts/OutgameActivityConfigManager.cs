using System;
using System.Collections.Generic;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameActivitySettingConfig
    {
        public string dailyTaskResetDate;
        public int dailyTaskCountLimit,slotOrignalNumber,signOfCalendarLoop,signOfActiveOnlyLoop,signOfActiveLoginLoop,mailDuration,RandomStoreRefreshCondition;
        public string RandomStoreRefreshConditionValue;
        public int RandomStoreRefreshTimeLimit;public int[] RandomStoreAutoRefreshType;
        public int randomStoreShowNumber,randomStorePutBack;
    }
    public abstract class OutgameActivityConfigCustomManager
    {public abstract void OnInit(Action complete);public abstract void OnDispose();}
    public sealed class OutgameActivityConfigManagerServices
    {
        public Func<OutgameActivityConfigManager> Current;
        public Func<Type[]> AssemblyTypes;
        public Func<Type,OutgameActivityConfigCustomManager> CreateCustomManager;
        public Func<OutgameActivityConfigReader> CreateReader;
        public Action<object[]> Error;
        public Action<object[]> Warning;
    }
    // ActivityConfigMgr4626 common activity loading lifecycle; business indexes in partial class.
    public sealed partial class OutgameActivityConfigManager
    {
        public readonly OutgameActivityConfigManagerServices Services;
        public Dictionary<object,OutgameActivityConfigRow> Activities=new Dictionary<object,OutgameActivityConfigRow>();
        public OutgameActivitySettingConfig Settings;
        public OutgameActivityConfigReader Reader;
        public List<OutgameActivityConfigCustomManager> CustomManagers=new List<OutgameActivityConfigCustomManager>();
        public int Completed,Expected=2;
        public Action CompleteAction;
        public OutgameActivityConfigManager(OutgameActivityConfigManagerServices services){Services=services;}
        public OutgameActivityConfigRow GetActivityConfig(int id)
        {
            Activities.TryGetValue(id,out var config);
            if(config==null)Services.Error(new object[]{"配置中不包含活动id",id});
            return config;
        }
        public void ReadConfig(Action complete)
        {
            CompleteAction=complete;Reader=Services.CreateReader();InitializeCustomManagers();Reader.OnInit(CompleteCommonConfig);
        }
        public void InitializeCustomManagers()
        {
            var types=Services.AssemblyTypes();
            for(int i=0;i<types.Length;i++)
                if(types[i].IsSubclassOf(typeof(OutgameActivityConfigCustomManager)))CustomManagers.Add(Services.CreateCustomManager(types[i]));
            Expected=unchecked(Expected+CustomManagers.Count);
            for(int i=0;i<CustomManagers.Count;i++)CustomManagers[i].OnInit(CompleteCustomConfig);
        }
        public void CompleteCustomConfig(){Completed=unchecked(Completed+1);CheckComplete();}
        public void CheckComplete(){if(Completed==Expected)CompleteAction?.Invoke();}
        public void CompleteCommonConfig()
        {
            foreach(var config in Services.Current().Activities.Values)
            {
                if(config.closeType.Length==0&&config.open==1){config.closeType=new[]{-1};config.closeParams=new[]{"999999"};}
                if(config.overType.Length==0&&config.open==1){config.overType=new[]{-1};config.overParams=new[]{"999999"};}
            }
            Completed=unchecked(Completed+2);CheckComplete();
        }
        public void Dispose()
        {
            Activities?.Clear();Reader?.OnDispose();
            if(CustomManagers!=null)
            {
                for(int i=0;i<CustomManagers.Count;i++)CustomManagers[i]?.OnDispose();
                CustomManagers?.Clear();
            }
            Completed=0;Expected=2;
        }
    }
}
