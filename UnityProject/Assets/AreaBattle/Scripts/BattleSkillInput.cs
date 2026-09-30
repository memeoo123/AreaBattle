using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public sealed class SkillInputConfig
    { public int id,gameItem,unLockLevel,castType,price,diamondPrice,useType,targetType; }
    public sealed class SkillConsumption
    {
        public int Slot,SkillId,SkillLevel,ItemId,RemainingItemStock,RemainingGenericStock;
    }
    // In-level item stock is injected by the local loadout. It never accesses the original account.
    public sealed class BattleSkillInput
    {
        [Serializable] sealed class Table { public SkillInputConfig[] Datas; }
        readonly BattleSimulation simulation;
        readonly Dictionary<int,SkillInputConfig> rules=new Dictionary<int,SkillInputConfig>();
        readonly Dictionary<int,int> stock=new Dictionary<int,int>();
        readonly int[] skillLevels;
        readonly Action<SkillConsumption> onConsumed;
        public readonly int CommanderMode,NormalLevel,SkillLevel;
        public int GenericStock { get; private set; }
        public string Rejection { get; private set; }
        public BattleSkillInput(BattleSimulation simulation,string recoveredRules,int mode,int normalLevel,
            int skillLevel,IDictionary<int,int> itemStock,int genericStock=0)
            : this(simulation,recoveredRules,mode,normalLevel,new[]{skillLevel,skillLevel,skillLevel},itemStock,genericStock,null)
        { }
        public BattleSkillInput(BattleSimulation simulation,string recoveredRules,int mode,int normalLevel,
            int[] slotSkillLevels,IDictionary<int,int> itemStock,int genericStock=0,Action<SkillConsumption> onConsumed=null)
        {
            if(mode<1 || mode>6)throw new ArgumentOutOfRangeException(nameof(mode));
            if(simulation==null)throw new ArgumentNullException(nameof(simulation));
            if(slotSkillLevels==null || slotSkillLevels.Length!=3)throw new ArgumentException("Three independent skill levels are required.",nameof(slotSkillLevels));
            skillLevels=(int[])slotSkillLevels.Clone();
            foreach(int level in skillLevels)if(level<1 || level>10)throw new ArgumentOutOfRangeException(nameof(slotSkillLevels),"Recovered SkillConfig levels are 1..10.");
            if(genericStock<0)throw new ArgumentOutOfRangeException(nameof(genericStock));
            this.simulation=simulation;this.onConsumed=onConsumed;CommanderMode=mode;NormalLevel=normalLevel;SkillLevel=skillLevels[0];GenericStock=genericStock;
            foreach(var row in JsonUtility.FromJson<Table>(recoveredRules).Datas)rules.Add(row.id,row);
            foreach(var pair in itemStock){if(pair.Value<0)throw new ArgumentOutOfRangeException(nameof(itemStock));stock.Add(pair.Key,pair.Value);}
        }
        public int SkillId(int slot){if(slot<0||slot>2)throw new ArgumentOutOfRangeException(nameof(slot));return (CommanderMode-1)*3+slot+1;}
        public int SkillLevelAt(int slot){SkillId(slot);return skillLevels[slot];}
        public SkillInputConfig Rule(int slot)=>rules[SkillId(slot)];
        public int Count(int slot)=>stock.TryGetValue(Rule(slot).gameItem,out int count)?count:0;
        public bool Unlocked(int slot)=>NormalLevel>=Rule(slot).unLockLevel;
        public bool InUse(int slot)=>simulation.IsSkillActive(SkillId(slot));
        public bool TryUse(int slot,int targetId=0,Vector3? groundPoint=null,Vector3? projectileOrigin=null)
        {
            Rejection=null;var rule=Rule(slot);
            if(!Unlocked(slot)){Rejection="第 "+rule.unLockLevel+" 关解锁";return false;}
            if(InUse(slot)){Rejection="技能正在生效";return false;}
            if(Count(slot)<=0 && GenericStock<=0){Rejection="道具数量不足";return false;}
            if(!simulation.CastSkill(SkillId(slot),SkillLevelAt(slot),BattleSimulation.PlayerCampID,targetId,groundPoint,projectileOrigin))
            {Rejection=simulation.LastSkillRejection;return false;}
            bool specific=Count(slot)>0;
            if(specific)stock[rule.gameItem]--;else GenericStock--;
            onConsumed?.Invoke(new SkillConsumption{Slot=slot,SkillId=rule.id,SkillLevel=SkillLevelAt(slot),ItemId=specific?rule.gameItem:1005,
                RemainingItemStock=Count(slot),RemainingGenericStock=GenericStock});
            return true;
        }
    }
}
