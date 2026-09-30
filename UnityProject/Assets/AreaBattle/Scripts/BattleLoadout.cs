using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public sealed class HeldSkillLevel { public int skillId,level; }
    [Serializable] public sealed class HeldSkillItem { public int itemId,count; }
    [Serializable] public sealed class BattleLoadoutProfile
    {
        public int schemaVersion,normalLevel,commanderId,genericStock;
        public List<HeldSkillLevel> skillLevels;
        public List<HeldSkillItem> skillItems;
    }
    // This is a local reconstruction profile. Item acquisition, purchases, ads and
    // original-account synchronization are deliberately outside this held-state contract.
    public sealed class BattleLoadout
    {
        readonly BattleLoadoutProfile profile;
        public string FilePath { get; private set; }
        public int NormalLevel=>profile.normalLevel;
        public int CommanderId=>profile.commanderId;
        public int GenericStock=>profile.genericStock;
        public bool CreatedFromFixture { get; private set; }
        public bool MigratedLegacyProfile { get; private set; }
        BattleLoadout(string path,BattleLoadoutProfile state) {FilePath=Path.GetFullPath(path);profile=state;}

        // The caller chooses these initial held values explicitly; none are inferred rewards.
        // A null first-run fixture creates an empty inventory, commander1 and level1 skills.
        public static BattleLoadoutProfile CreateFixture(int normalLevel=0,int commanderId=1,int defaultSkillLevel=1,int perSpecificStock=0,int genericStock=0)
        {
            var p=new BattleLoadoutProfile{schemaVersion=1,normalLevel=normalLevel,commanderId=commanderId,genericStock=genericStock,
                skillLevels=new List<HeldSkillLevel>(),skillItems=new List<HeldSkillItem>()};
            for(int id=1;id<=18;id++){p.skillLevels.Add(new HeldSkillLevel{skillId=id,level=defaultSkillLevel});p.skillItems.Add(new HeldSkillItem{itemId=2000+id,count=perSpecificStock});}
            Validate(p);return p;
        }
        public static BattleLoadout LoadOrCreate(string path,BattleLoadoutProfile firstRunFixture=null)
        {
            if(string.IsNullOrWhiteSpace(path))throw new ArgumentException("A local profile path is required.",nameof(path));
            string full=Path.GetFullPath(path);bool exists=File.Exists(full);
            BattleLoadoutProfile p;
            if(exists)
            {
                p=new BattleLoadoutProfile{normalLevel=-1};
                JsonUtility.FromJsonOverwrite(File.ReadAllText(full),p);
            }
            else p=JsonUtility.FromJson<BattleLoadoutProfile>(JsonUtility.ToJson(firstRunFixture??CreateFixture()));
            if(p==null)throw new InvalidDataException("The loadout profile is invalid.");
            bool legacy=exists&&p.schemaVersion==0;
            if(legacy)
            {
                // The previous format held normalLevel only. Preserve it, while absent
                // inventory means zero; do not copy stock from the first-run fixture.
                p=CreateFixture(p.normalLevel,1,1,0,0);
            }
            Validate(p);var result=new BattleLoadout(full,p){CreatedFromFixture=!exists,MigratedLegacyProfile=legacy};
            if(!exists||legacy)result.Save();return result;
        }
        static void Validate(BattleLoadoutProfile p)
        {
            if(p.schemaVersion!=1)throw new InvalidDataException("Unsupported loadout profile schema.");
            if(p.normalLevel<0||p.commanderId<1||p.commanderId>6||p.genericStock<0)throw new InvalidDataException("Invalid held loadout state.");
            if(p.skillLevels==null||p.skillItems==null)throw new InvalidDataException("Held skill levels and inventory must be explicit.");
            var levels=new HashSet<int>();
            foreach(var entry in p.skillLevels)
                if(entry==null||entry.skillId<1||entry.skillId>18||entry.level<1||entry.level>10||!levels.Add(entry.skillId))
                    throw new InvalidDataException("Each skill must have a unique recovered level in 1..10.");
            if(levels.Count!=18)throw new InvalidDataException("Specify the held level for all 18 skills.");
            var items=new HashSet<int>();
            foreach(var entry in p.skillItems)
                if(entry==null||entry.itemId<2001||entry.itemId>2018||entry.count<0||!items.Add(entry.itemId))
                    throw new InvalidDataException("Invalid or duplicated specific skill inventory.");
            // Missing specific-item rows are valid zero inventory, never a refill request.
        }
        public int SkillLevel(int skillId)
        {
            var row=profile.skillLevels.Find(x=>x.skillId==skillId);
            if(row==null)throw new ArgumentOutOfRangeException(nameof(skillId));return row.level;
        }
        public int ItemCount(int itemId)
        {
            if(itemId==1005)return profile.genericStock;
            var row=profile.skillItems.Find(x=>x.itemId==itemId);return row==null?0:row.count;
        }
        public int[] SelectedSkillLevels()
        {int start=(CommanderId-1)*3;return new[]{SkillLevel(start+1),SkillLevel(start+2),SkillLevel(start+3)};}
        public void SetCommander(int commanderId)
        {
            if(commanderId<1||commanderId>6)throw new ArgumentOutOfRangeException(nameof(commanderId));
            if(profile.commanderId==commanderId)return;profile.commanderId=commanderId;Save();
        }
        public void SetNormalLevel(int level)
        {if(level<0)throw new ArgumentOutOfRangeException(nameof(level));profile.normalLevel=level;Save();}
        public BattleSkillInput CreateSkillInput(BattleSimulation simulation,string rulesJson,int currentLevelIdentity)
        {
            var stocks=new Dictionary<int,int>();foreach(var item in profile.skillItems)stocks.Add(item.itemId,item.count);
            // A single active battle owns this input snapshot. Recreate it after changing
            // commander; currentLevelIdentity remains the source CurLevel, including specials.
            return new BattleSkillInput(simulation,rulesJson,CommanderId,currentLevelIdentity,SelectedSkillLevels(),stocks,GenericStock,OnConsumed);
        }
        void OnConsumed(SkillConsumption consumption)
        {
            if(consumption.ItemId==1005)profile.genericStock=consumption.RemainingGenericStock;
            else
            {
                var item=profile.skillItems.Find(x=>x.itemId==consumption.ItemId);
                if(item==null)throw new InvalidOperationException("Consumed an item absent from held inventory.");
                item.count=consumption.RemainingItemStock;
            }
            Save();
        }
        public void Save()
        {
            Validate(profile);string directory=Path.GetDirectoryName(FilePath);Directory.CreateDirectory(directory);
            string temporary=FilePath+"."+Guid.NewGuid().ToString("N")+".tmp";
            try
            {
                File.WriteAllText(temporary,JsonUtility.ToJson(profile,true),new UTF8Encoding(false));
                if(File.Exists(FilePath))File.Replace(temporary,FilePath,null);else File.Move(temporary,FilePath);
            }
            finally{if(File.Exists(temporary))File.Delete(temporary);}
        }
    }
}
