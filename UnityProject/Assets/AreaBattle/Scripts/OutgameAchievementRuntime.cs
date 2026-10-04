using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameAchievementServices
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameActivityControl> Control;
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameCommonMessageDispatcher> Common;
        public OutgameStatisticsExpansion Statistics;
        public Func<OutgameItemConfigManager> Items;
        public Func<OutgameGlobalItemRewards> Rewards;
        public OutgameItemEntityServices Entities;
        public Func<IOutgameLimitTaskReports> Reports;
        public Action<string> Warning;
        public Action<Color,object[]> LogByColor;
    }
    // Install before ActivityRuntime.Init. The existing pool owns account save scheduling.
    public sealed class OutgameAchievementRuntime
    {
        public readonly OutgameAchievementServices Services;
        public readonly OutgameAchievementModelServices Models;
        public OutgameAchievementRuntime(OutgameActivityRuntime activities,OutgameAchievementServices services,
            Func<OutgameAchievementServices,OutgameAchievementManager> createManager)
        {
            Services=services;services.Config=()=>activities.Config.Manager;services.Control=()=>activities.Control;
            services.Pool=activities.Services.Pool;services.Common=activities.Services.Common;services.Statistics=activities.Services.Statistics;
            activities.ActivityServices.ItemConfig=services.Items;
            var types=activities.Services.AssemblyTypes;var make=activities.Services.CreateActivity;
            activities.Services.AssemblyTypes=()=>types().Concat(new[]{typeof(OutgameAchievementActivity)}).Distinct().ToArray();
            activities.Services.CreateActivity=t=>t==typeof(OutgameAchievementActivity)?new OutgameAchievementActivity(activities.ActivityServices,services):make(t);
            var managers=activities.ManagerServices;var managerTypes=managers.AssemblyTypes;var makeManager=managers.CreateManager;
            var source=managers.SourceTypeIndex;var sync=managers.SourceAutoSyn;
            managers.AssemblyTypes=()=>managerTypes().Concat(new[]{typeof(OutgameAchievementManager)}).Distinct().ToArray();
            managers.CreateManager=t=>t==typeof(OutgameAchievementManager)?createManager(services):makeManager(t);
            managers.SourceTypeIndex=t=>t==typeof(OutgameAchievementManager)?4722:source(t);
            managers.SourceAutoSyn=t=>t==typeof(OutgameAchievementManager)||sync(t);
            Models=new OutgameAchievementModelServices{Config=services.Config,GetActivity=id=>services.Control().GetActivity<OutgameAchievementActivity>(id)};
            OutgameAchievementModels.Services=Models;
        }
    }
    // Original AchievementActivity4714.
    [OutgameActivityControlRegister(1601001)]
    public sealed class OutgameAchievementActivity:OutgameActivityBase,IOutgameAchievementActivity
    {
        readonly OutgameAchievementServices achievement;
        public OutgameAchievementManager Manager;
        List<OutgameAccAchievementItemData> accItems;
        public OutgameAchievementActivity(OutgameActivityServices services,OutgameAchievementServices achievement):base(services){this.achievement=achievement;}
        public OutgameAchievementExt Ext=>Manager.Data.ext;
        public Dictionary<int,OutgameAchievementItemData> AchievementMap=>Manager.AchievementMap;
        public Dictionary<int,List<OutgameAchievementItemData>> TypeLists=>Manager.TypeLists;
        public override void OnInit()=>Manager=achievement.Pool().GetModel<OutgameAchievementManager>(4722,"AchievementMgr");
        public override void ResetProgress()=>Manager.Strategy.ResetProgress();
        public override void Launch()=>Manager.Strategy.Launch();
        public override void Update(){}
        public override void Dispose(){accItems=null;Manager.OnRelease();}
        public override OutgameActivityFactory ActivityFactoryBase(int id)=>new OutgameAchievementFactory(id,Services.ItemConfig,Services.ItemFactoryError,achievement.Entities,Services.Common);
        public void RefreshSortList()=>Manager.RefreshSortList();
        public void RefreshTypeListSort(int type)=>Manager.RefreshTypeListSort(type);
        public List<OutgameAchievementItemData> GetAchievementsListByType(int type){Manager.TypeLists.TryGetValue(type,out var list);return list;}
        public OutgameAchievementItemData FindAchievement(int id)
        {if(Manager.AchievementMap.TryGetValue(id,out var item))return item;achievement.Warning(string.Format("不包含id为{0}的任务",id));return null;}
        public void GetAchievementReward(int id)=>Manager.Strategy.GetAchievementReward(id);
        public List<OutgameAccAchievementItemData> GetAccItems()
        {if(accItems==null){accItems=new List<OutgameAccAchievementItemData>();RefreshAccItems();}return accItems;}
        public void RefreshAccItems()
        {accItems.Clear();for(int i=0;i<achievement.Config().AchievementAccList.Count;i++)accItems.Add(new OutgameAccAchievementItemData{index=i,acc=achievement.Config().AchievementAccList[i].id});}
    }
    // Original AchievementMgr4722. Maps are rebuilt in source enumeration order.
    public sealed class OutgameAchievementManager:OutgameCommonModuleManager
    {
        readonly OutgameAchievementServices services;
        public OutgameAchievementData Data;
        public OutgameAchievementOffStrategy Strategy;
        public Dictionary<int,OutgameAchievementItemData> AchievementMap;
        public List<OutgameAchievementItemData> SortList=new List<OutgameAchievementItemData>();
        public Dictionary<int,List<OutgameAchievementItemData>> TypeLists=new Dictionary<int,List<OutgameAchievementItemData>>();
        public override int ActivityId=>1601001;
        public override string SourceClassName=>"AchievementMgr";
        public OutgameAchievementManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,OutgameAchievementServices services):base(storage,host,download){this.services=services;}
        public override void OnInit()=>Strategy=new OutgameAchievementOffStrategy(services);
        public override void InitStrategy(){var strategy=Strategy;if(strategy!=null)strategy.InitData(UpdateCallback,1601001);}
        public override void UpdateDataCallBack(string text)=>Strategy.LoadData(text);
        public override void OnSave()=>Strategy.OnSave();
        public override void OnRelease(){SortList.Clear();Strategy.Dispose();}
        public void ResetProgress()=>Strategy.ResetProgress();
        public void Launch()=>Strategy.Launch();
        public void GetAchievementReward(int id)=>Strategy.GetAchievementReward(id);
        public void UpdateCallback(OutgameAchievementData data)
        {
            Data=data;if(AchievementMap==null)AchievementMap=new Dictionary<int,OutgameAchievementItemData>();else AchievementMap.Clear();
            TypeLists.Clear();for(int i=0;i<data.datas.Count;i++)AchievementMap[data.datas[i].id]=data.datas[i];
            foreach(var type in services.Config().AchievementGroups)
            {
                if(!TypeLists.ContainsKey(type.Key))TypeLists.Add(type.Key,new List<OutgameAchievementItemData>());
                foreach(var group in type.Value)
                {
                    if(group.Value.Count==1){TypeLists[type.Key].Add(AchievementMap[group.Value[0].id]);continue;}
                    for(int i=0;i<group.Value.Count;i++)
                    {
                        if(!AchievementMap.ContainsKey(group.Value[i].id))continue;
                        if(i==group.Value.Count-1&&TypeLists[type.Key].Count==0){TypeLists[type.Key].Add(AchievementMap[group.Value[i].id]);continue;}
                        if(AchievementMap[group.Value[i].id].state==1)continue;
                        TypeLists[type.Key].Add(AchievementMap[group.Value[i].id]);break;
                    }
                }
            }
            RefreshSortList();foreach(var pair in TypeLists)RefreshTypeListSort(pair.Key);
            services.Common().SendMessage(OutgameAchievementStrategy.RefreshExt,new object[]{data.ext});
        }
        public void RefreshSortList()
        {
            SortList.Clear();bool complete=true;
            foreach(var pair in AchievementMap){SortList.Add(pair.Value);complete&=pair.Value.state==1;}
            if(complete){var report=services.Reports().Create(OutgameLimitTaskReportKind.Complete,true,null);report.ActivityName="成就";services.Reports().Send(OutgameLimitTaskReportKind.Complete,report);}
            SortList.Sort((a,b)=>a.CompareTo(b));AchievementMap.Clear();
            for(int i=0;i<SortList.Count;i++)AchievementMap.Add(SortList[i].id,SortList[i]);
            services.Common().SendMessage(OutgameAchievementStrategy.RefreshList,new object[]{SortList});
        }
        public void RefreshTypeListSort(int type)
        {
            if(!TypeLists.TryGetValue(type,out var list))return;
            list.Sort((a,b)=>a.CompareTo(b));services.Common().SendMessage(OutgameAchievementStrategy.RefreshTypeList,new object[]{type});
        }
    }
}
