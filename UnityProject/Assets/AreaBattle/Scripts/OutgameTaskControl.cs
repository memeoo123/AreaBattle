using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameTaskControlServices
    {
        public Func<OutgameActivityControl> Activities;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<int> CurrentLevel,DailyActivityId;
        public Func<OutgameMessageDispatcher> Messages;
        public OutgameStatisticsExpansion Statistics;
        public Func<OutgameTaskEntrance> Entrance;
    }
    // Proj_xqzdStartUI33723:0 changes button visibility;1 changes its first child red dot.
    public sealed class OutgameTaskEntrance
    {
        public GameObject Root;
        public Button Button;
        public void SetNode(int index,bool value)
        {
            switch(index)
            {
                case 0:Button.gameObject.SetActive(value);break;
                case 1:(ReferenceEquals(Button,null)?null:Button.transform.GetChild(0).gameObject).SetActive(value);break;
            }
        }
    }
    // Original obfuscated controller4507. Dispose clears cached activities, not the singleton.
    public sealed class OutgameTaskControl:IOutgameLogicControl
    {
        readonly OutgameTaskControlServices services;
        OutgameAchievementActivity achievement;OutgameChildTaskActivity daily;
        public OutgameTaskControl(OutgameTaskControlServices services){this.services=services;}
        public OutgameAchievementActivity Achievement=>achievement??(achievement=services.Activities().GetActivity<OutgameAchievementActivity>(1601001));
        public OutgameChildTaskActivity Daily=>daily??(daily=services.Activities().GetActivity<OutgameTaskActivity>(1300001).TaskViewActivities[services.DailyActivityId()]);
        public void OnInit()
        {
            services.Messages().AddListener("GF_AdsPlayCallBack",AdsComplete);services.Messages().AddListener("WarWin",WarWin);
            services.Statistics.RegisteredValueFunc(900001,LockedTaskValue);
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){}
        public void InitHczzqEvent(){}
        public long LockedTaskValue(object[] args)=>0;
        public void AdsComplete(object[] args){if((bool)args[0])services.Statistics.AddEventCount(services.Config().statisticEventConfig.DailyWatchAds,1);}
        public void WarWin(object[] args)=>services.Statistics.AddEventCount(services.Config().statisticEventConfig.DailyCompleteLv,1);
        public void RefreshData(object[] args)
        {
            bool unlocked=services.CurrentLevel()>9;
            if(!unlocked)foreach(var task in Daily.ModuleData.tasks)for(int i=0;i<task.conditions.Count;i++)task.conditions[i].value=0;
            if(services.CurrentLevel()>=10){SetRedDot();unlocked=true;}
            var entrance=services.Entrance();if(entrance!=null&&entrance.Root)entrance.SetNode(0,unlocked);
        }
        public bool IsDailyCanComplete()
        {
            foreach(var task in Daily.ModuleData.tasks)if(task.CanComplete())return true;
            var ext=Daily.ModuleData.ext;foreach(var item in ext.GetLivenessItems())if(item.state==0&&item.livenessValue<=ext.livenessValue)return true;
            return false;
        }
        public bool IsAchiCanComplete()
        {
            foreach(var pair in Achievement.AchievementMap)
            {
                if(pair.Value.Config.contentType==services.Config().statisticEventConfig.ArenaRank&&pair.Value.progress>0)
                {pair.Value.progress=services.Statistics.EventCount(services.Config().statisticEventConfig.ArenaRank);continue;}
                if(pair.Value.CanComplete())return true;
            }
            return false;
        }
        public void SetRedDot(){bool red=IsAchiCanComplete()||IsDailyCanComplete();services.Entrance().SetNode(1,red);}
        public void OnDispose()
        {
            services.Messages().RemoveListener("GF_AdsPlayCallBack",AdsComplete);services.Messages().RemoveListener("LoadStartingUI",RefreshData);services.Messages().RemoveListener("WarWin",WarWin);
            achievement=null;daily=null;
        }
    }
}
