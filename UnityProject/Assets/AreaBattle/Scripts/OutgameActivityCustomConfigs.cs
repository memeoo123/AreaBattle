using System;
using System.Collections.Generic;
using AreaBattle.ActivityConfig;
namespace AreaBattle
{
    public interface IOutgameActivityBinaryConfigReader
    {void ReadTable<T>(Dictionary<object,T> rows)where T:class,IOutgameConfigRow;}
    public sealed class OutgameActivityCustomConfigServices
    {
        public Func<OutgameActivityConfigManager> Current;
        public Func<bool> UseBinary;
        public OutgameLegacyConfigRead Json;
        public IOutgameActivityBinaryConfigReader Binary;
        public void Read<T>(bool binary,Dictionary<object,T> rows)where T:class,IOutgameConfigRow
        {if(binary)Binary.ReadTable(rows);else Json.ReadTable(rows);}
    }
    public sealed class OutgameNoviceTaskConfigManager:OutgameActivityConfigCustomManager
    {
        readonly OutgameActivityCustomConfigServices services;
        public OutgameNoviceTaskConfigManager(OutgameActivityCustomConfigServices services){this.services=services;}
        public override void OnInit(Action complete)
        {
            bool binary=services.UseBinary();services.Read(binary,services.Current().NoviceTasks);services.Read(binary,services.Current().NoviceAcc);services.Read(binary,services.Current().NoviceActivities);
            services.Current().BuildNoviceTasks();services.Current().BuildNoviceAccRewards();
            foreach(var row in services.Current().NoviceAcc.Values)services.Current().NoviceAccList.Add(row);
            complete?.Invoke();
        }
        public override void OnDispose()
        {services.Current().NoviceTasks.Clear();services.Current().NoviceAcc.Clear();services.Current().NoviceTasksByActivity.Clear();services.Current().NoviceAccByActivity.Clear();services.Current().NoviceActivities.Clear();services.Current().NoviceAccList.Clear();}
    }
    public sealed class OutgameTaskConfigManager:OutgameActivityConfigCustomManager
    {
        readonly OutgameActivityCustomConfigServices services;
        public OutgameTaskConfigManager(OutgameActivityCustomConfigServices services){this.services=services;}
        public override void OnInit(Action complete)
        {
            bool binary=services.UseBinary();services.Read(binary,services.Current().Tasks);services.Read(binary,services.Current().Liveness);services.Read(binary,services.Current().TaskGroups);services.Read(binary,services.Current().TaskActivities);
            var owner=services.Current();owner.LivenessList=new List<PubdailyLivenessConfig>(services.Current().Liveness.Values);
            services.Current().BuildTaskGroups();services.Current().BuildLiveness();services.Current().BuildTasks();complete?.Invoke();
        }
        public override void OnDispose()
        {
            services.Current().TaskGroups.Clear();services.Current().Tasks.Clear();services.Current().Liveness.Clear();services.Current().LivenessList.Clear();
            services.Current().TaskGroupsByActivity.Clear();services.Current().TaskActivities.Clear();services.Current().LivenessByActivity.Clear();
            // Source35395 retains TasksByGroup, including lists that reference the released rows.
        }
    }
    public sealed class OutgameAchievementConfigManager:OutgameActivityConfigCustomManager
    {
        readonly OutgameActivityCustomConfigServices services;
        public OutgameAchievementConfigManager(OutgameActivityCustomConfigServices services){this.services=services;}
        public override void OnInit(Action complete)
        {
            // Source35603 always reads Unity JSON, independent of UseBinary.
            services.Json.ReadTable(services.Current().Achievements);services.Json.ReadTable(services.Current().AchievementAcc);
            foreach(var row in services.Current().Achievements.Values)
            {
                if(!services.Current().AchievementGroups.ContainsKey(row.type))
                {
                    services.Current().AchievementGroups.Add(row.type,new Dictionary<OutgameActivityConditionPriority,List<PubAchievementConfig>>());
                    var key=new OutgameActivityConditionPriority(row.contentType,row.content);
                    services.Current().AchievementGroups[row.type].Add(key,new List<PubAchievementConfig>());
                    services.Current().AchievementGroups[row.type][key].Add(row);
                }
                else
                {
                    var group=services.Current().AchievementGroups[row.type];var key=new OutgameActivityConditionPriority(row.contentType,row.content);
                    if(!group.ContainsKey(key))group.Add(key,new List<PubAchievementConfig>());group[key].Add(row);
                }
            }
            foreach(var group in services.Current().AchievementGroups)
                foreach(var pair in group.Value)pair.Value.Sort(ComparePriority);
            foreach(var row in services.Current().AchievementAcc.Values)services.Current().AchievementAccList.Add(row);
            complete?.Invoke();
        }
        public static int ComparePriority(PubAchievementConfig a,PubAchievementConfig b)=>a.priority<b.priority?-1:a.priority>b.priority?1:0;
        public override void OnDispose()
        {services.Current().Achievements.Clear();services.Current().AchievementAcc.Clear();services.Current().AchievementGroups.Clear();services.Current().AchievementAccList.Clear();}
    }
}
