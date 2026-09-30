using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgamePrice { public int[] datas; }
    [Serializable] public sealed class OutgameCommanderConfig
    {
        public int id,maxLevel,unlockType,unlockLevel;
        public string name,IconName;
        public OutgamePrice[] unlockPrice;
        public int[] skills;
    }
    [Serializable] public sealed class OutgameCommanderUpgrade { public int id,upgradeCost; }
    [Serializable] public sealed class OutgameCommanderState
    {
        public int id,level;
        public bool isNew;
        public int[] skillIds,skillLevels;
    }
    // Source contract: generated/outgame/OUTGAME_RESTORE_SPEC.json, commander-core.
    // Explicit configuration and held state: UI/channel selection and profile defaults belong to callers.
    public sealed class OutgameCommanderProgression
    {
        [Serializable] sealed class Commanders { public OutgameCommanderConfig[] Datas; }
        [Serializable] sealed class Upgrades { public OutgameCommanderUpgrade[] Datas; }
        readonly Dictionary<int,OutgameCommanderConfig> configs=new Dictionary<int,OutgameCommanderConfig>();
        readonly Dictionary<int,int> costs=new Dictionary<int,int>();
        public OutgameCommanderProgression(string commanderJson,string upgradeJson)
        {
            foreach(var row in JsonUtility.FromJson<Commanders>(commanderJson).Datas)configs.Add(row.id,row);
            foreach(var row in JsonUtility.FromJson<Upgrades>(upgradeJson).Datas)costs.Add(row.id,row.upgradeCost);
        }
        public OutgameCommanderConfig Config(int id)=>configs[id];
        public bool HasConfig(int id)=>configs.ContainsKey(id);
        public int[] SkillIds(OutgameCommanderState state)=>state.skillIds??Config(state.id).skills;
        public bool IsUnlocked(OutgameCommanderState state)=>state.level>0;
        public bool IsMax(OutgameCommanderState state)=>state.level>=Config(state.id).maxLevel;
        public bool IsLevelUnlockable(OutgameCommanderState state,int currentLevel)=>currentLevel>=Config(state.id).unlockLevel;
        public int NextCost(OutgameCommanderState state)=>IsUnlocked(state)&&!IsMax(state)?costs[state.level+1]:Config(state.id).unlockPrice[0].datas[1];
        public bool CanAffordUnlock(OutgameCommanderState state,Func<int,int> count)
        {
            foreach(var price in Config(state.id).unlockPrice)if(count(price.datas[0])<price.datas[1])return false;
            return true;
        }
        // The source query itself does not enforce the max/level gate. TryUpgrade does enforce max.
        public bool CanAffordUpgrade(OutgameCommanderState state,Func<int,int> count)=>count(Config(state.id).unlockPrice[0].datas[0])>=NextCost(state);
        public bool TryUpgrade(OutgameCommanderState state,Func<int,int> count,Action<int,int> changeItem,out int changedSkillSlot)
        {
            changedSkillSlot=-1;
            if(state==null||!configs.TryGetValue(state.id,out var config))return false;
            if(IsUnlocked(state))
            {
                if(IsMax(state)||!CanAffordUpgrade(state,count))return false;
                if(state.skillLevels==null||state.skillLevels.Length!=SkillIds(state).Length||state.skillLevels.Length==0)
                    throw new ArgumentException("Held skill IDs and levels must have matching nonempty arrays.");
                int cost=NextCost(state);
                changeItem(config.unlockPrice[0].datas[0],-cost);
                state.level++;
                changedSkillSlot=(state.level-2)%state.skillLevels.Length;
                state.skillLevels[changedSkillSlot]++;
            }
            else
            {
                if(!CanAffordUnlock(state,count))return false;
                foreach(var price in config.unlockPrice)changeItem(price.datas[0],-price.datas[1]);
                state.level=1;
            }
            return true;
        }
    }
}
