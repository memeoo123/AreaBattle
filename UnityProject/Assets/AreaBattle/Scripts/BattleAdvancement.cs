using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public enum TowerSpecialization { None, Single, Split, Relay, Arrow }
    public sealed class AdvancementStats
    {
        public int Speed, Spawn, Regen, LineLimit;
        public void Add(AdvancementStats b)
        { Speed+=b.Speed; Spawn+=b.Spawn; Regen+=b.Regen; if(b.LineLimit>0)LineLimit=b.LineLimit; }
    }
    public sealed class AdvancementOption
    {
        public string Id, Name, Description;
        public TowerSpecialization Route;
        public int Doctrine = -1, ArtFocus;
        public AdvancementStats Bonus = new AdvancementStats();
    }
    public partial class TowerState
    {
        public TowerSpecialization Specialization, RememberedSpecialization;
        public int AdvancementEarned, Doctrine = -1, ArtFocus;
        public readonly List<string> AdvancementHistory = new List<string>();
        public AdvancementStats AdvancementBonuses = new AdvancementStats();
        public int AdvancementSpent => AdvancementHistory.Count;
        public string AdvancementName = "";
    }
    public partial class SoldierState
    {
        public TowerSpecialization VisualRoute;
        public int VisualDoctrine = -1, VisualTier, VisualFocus;
    }
    public partial class BattleSimulation
    {
        public bool BasicTowerExperiment { get; private set; }
        public const int AdvancementStep = 10, MaximumAdvancements = 6;
        public static string BaseTowerName(int shipId) => shipId==2?"防御塔":shipId==3?"进攻塔":shipId==4?"箭塔":"普通塔";
        public static string ConnectionName(TowerSpecialization route) => route==TowerSpecialization.Single?"单线":route==TowerSpecialization.Split?"分流":route==TowerSpecialization.Relay?"中继":route==TowerSpecialization.Arrow?"箭塔":"未进阶";
        private int ConnectionCapacity(TowerState t)
        {
            if(t.Specialization==TowerSpecialization.Arrow)return 0;
            if(t.Specialization==TowerSpecialization.None)return BasicTowerExperiment?1:GetDispatchLineNum(t.Grade);
            if(BasicTowerExperiment)return t.Specialization==TowerSpecialization.Split?(t.AdvancementSpent>=2?3:2):1;
            if(t.Specialization==TowerSpecialization.Single)return 1;
            if(t.AdvancementBonuses.LineLimit>0)return t.AdvancementBonuses.LineLimit;
            return t.Specialization==TowerSpecialization.Split?3:2;
        }
        private float ConnectionSpawnTime(TowerState t)
        {
            if(t.Specialization==TowerSpecialization.None)return GetSpawnTime(t.Grade,t.OutgoingCount);
            if(BasicTowerExperiment){
                float total=t.Specialization==TowerSpecialization.Single?1.2f:t.Specialization==TowerSpecialization.Split?1f:.5f;
                return GetSpawnTime(t.Grade,1)/(total+t.AdvancementBonuses.Spawn/100f);
            }
            // Existing single-line interval is the reference; no new resource or burst meter.
            float rate=t.Specialization==TowerSpecialization.Single?1.35f:t.Specialization==TowerSpecialization.Split?.70f:.50f;
            return GetSpawnTime(t.Grade,1)/(rate*(1f+t.AdvancementBonuses.Spawn/100f));
        }
        public bool CanAdvance(int towerId)
        {
            var t=Tower(towerId);
            return State==BattlePhase.Running&&t!=null&&t.Active&&t.Camp==PlayerCampID&&!t.IsBoss&&
                t.ShipID>=1&&t.ShipID<=3&&(!BasicTowerExperiment||(t.AdvancementSpent==0&&t.Score>=AdvancementStep))&&t.AdvancementSpent<t.AdvancementEarned&&t.AdvancementSpent<MaximumAdvancements;
        }
        public AdvancementOption[] AdvancementOptions(int towerId)
        {var t=Tower(towerId);return t==null||t.AdvancementSpent>=MaximumAdvancements?Array.Empty<AdvancementOption>():BasicTowerExperiment?ExperimentOptions(t):AdvancementCatalog.Options(t);}
        public bool AdvanceTower(int towerId,TowerSpecialization route)
        {
            var t=Tower(towerId);if(t==null||t.Specialization!=TowerSpecialization.None)return false;
            foreach(var option in AdvancementOptions(towerId))if(option.Route==route)return ChooseAdvancement(towerId,option.Id);
            return false;
        }
        public bool ChooseAdvancement(int towerId,string optionId)
        {
            if(!CanAdvance(towerId))return false;
            var t=Tower(towerId);var option=Array.Find(AdvancementOptions(towerId),x=>x.Id==optionId);if(option==null)return false;
            ApplyAdvancementChoice(t,option);return true;
        }
        private void ApplyAdvancementChoice(TowerState t,AdvancementOption option)
        {
            int towerId=t.Id;
            if(t.Specialization==TowerSpecialization.None)t.Specialization=option.Route;
            if(BasicTowerExperiment)t.RememberedSpecialization=t.Specialization;
            if(t.Specialization==TowerSpecialization.Arrow)t.ArrowAccumulator=0;
            if(option.Doctrine>=0)t.Doctrine=option.Doctrine;
            t.ArtFocus=option.ArtFocus;t.AdvancementHistory.Add(option.Id);t.AdvancementName=option.Name;t.AdvancementBonuses.Add(option.Bonus);
            var outgoing=Outgoing(towerId);
            for(int i=ConnectionCapacity(t);i<outgoing.Count;i++){var line=outgoing[i];SetDirection(line,line.Direction&~(line.SmallTowerId==towerId?1:2));}
            RefreshTower(t,true);
            Emit(new BattleEvent{Kind="advance",TowerId=t.Id,Camp=t.Camp,Value=t.AdvancementSpent});
        }
        private void AdvanceEnemyTowers()
        {
            if(!BasicTowerExperiment||!AIEnabled||State!=BattlePhase.Running)return;
            foreach(var t in Towers){
                if(!t.Active||t.Camp==0||t.Camp==PlayerCampID||t.IsBoss||t.ShipID<1||t.ShipID>3||t.Specialization!=TowerSpecialization.None||t.Score<AdvancementStep)continue;
                int targets=0;bool underAttack=false;
                foreach(var line in adjacency[t.Id]){
                    var other=Tower(line.Other(t.Id));
                    if(other==null||!other.Active||other.Camp==t.Camp)continue;
                    targets++;
                    if(line.IsFrom(other.Id))underAttack=true;
                }
                // Multiple expansion targets favor splitting; pressure or a single route favors concentrated output.
                int slot=underAttack&&FindArrowSoldier(t)!=null?2:targets>=2&&!underAttack?1:0;
                t.AdvancementEarned=Math.Max(t.AdvancementEarned,Math.Min(MaximumAdvancements,(int)t.Score/AdvancementStep));
                ApplyAdvancementChoice(t,ExperimentOptions(t)[slot]);
            }
        }
        static AdvancementOption[] ExperimentOptions(TowerState t)
        {
            if(t.AdvancementSpent>0)return Array.Empty<AdvancementOption>();
            return new[]{
                new AdvancementOption{Id="root_single",Name="单线塔",Route=TowerSpecialization.Single,Description="1条进攻线 · 总出兵效率120%"},
                new AdvancementOption{Id="root_split",Name="分流塔",Route=TowerSpecialization.Split,Description="2条进攻线 · 每路出兵效率100%，增加线路不减速"},
                new AdvancementOption{Id="root_arrow",Name="箭塔",Route=TowerSpecialization.Arrow,Description="自动射击敌兵及敌塔；不能出兵占领"}
            };
        }
        private void ResetEvolution(TowerState t,bool forgetRoute=true)
        {
            if(forgetRoute)t.RememberedSpecialization=TowerSpecialization.None;
            else if(t.Specialization!=TowerSpecialization.None)t.RememberedSpecialization=t.Specialization;
            t.Specialization=TowerSpecialization.None;t.AdvancementHistory.Clear();t.AdvancementEarned=0;
            t.AdvancementBonuses=new AdvancementStats();t.Doctrine=-1;t.ArtFocus=0;t.AdvancementName="";
            t.ForwardingHistory.Clear();t.ArrowAccumulator=0;
        }
        private void DowngradeEvolution(TowerState t)
        {
            if(!BasicTowerExperiment)return;
            int tier=Mathf.Clamp((int)t.Score/AdvancementStep,0,MaximumAdvancements);
            // An owned arrow tower retains its base weapon until ownership changes.
            if(t.Specialization==TowerSpecialization.Arrow)tier=Mathf.Max(1,tier);
            int previous=t.AdvancementSpent;
            if(tier==0)ResetEvolution(t,false);
            else if(t.Specialization==TowerSpecialization.None&&t.RememberedSpecialization!=TowerSpecialization.None){
                t.Specialization=t.RememberedSpecialization;
                t.AdvancementHistory.Add("root_"+t.Specialization.ToString().ToLowerInvariant());
                t.AdvancementName=t.Specialization==TowerSpecialization.Arrow?"箭塔":ConnectionName(t.Specialization)+"塔";
                Emit(new BattleEvent{Kind="evolution-restored",TowerId=t.Id,Camp=t.Camp,Value=1});
            }
            else if(previous>tier){
                t.AdvancementHistory.RemoveRange(tier,previous-tier);
                t.AdvancementBonuses=new AdvancementStats();
                if(t.Specialization==TowerSpecialization.Single)t.AdvancementBonuses.Spawn=(tier-1)*5;
                else if(t.Specialization==TowerSpecialization.Split)t.AdvancementBonuses.Spawn=(tier-1)*5;
                else if(t.Specialization==TowerSpecialization.Relay)t.AdvancementBonuses.Speed=(tier-1)*25;
            }
            t.AdvancementEarned=t.Specialization!=TowerSpecialization.None||t.Camp==PlayerCampID?tier:0;
            if(previous>t.AdvancementSpent){
                var outgoing=Outgoing(t.Id);
                for(int i=ConnectionCapacity(t);i<outgoing.Count;i++){
                    var line=outgoing[i];SetDirection(line,line.Direction&~(line.SmallTowerId==t.Id?1:2));
                }
                Emit(new BattleEvent{Kind="evolution-down",TowerId=t.Id,Camp=t.Camp,Value=t.AdvancementSpent});
            }
        }
        void ApplyAutomaticEvolution(TowerState t)
        {
            if(!BasicTowerExperiment||t.Specialization==TowerSpecialization.None)return;
            while(t.AdvancementSpent<t.AdvancementEarned){
                int tier=t.AdvancementSpent+1;
                t.AdvancementHistory.Add("auto_"+tier);
                if(t.Specialization==TowerSpecialization.Single)t.AdvancementBonuses.Spawn+=5;
                else if(t.Specialization==TowerSpecialization.Split)t.AdvancementBonuses.Spawn+=5;
                else if(t.Specialization==TowerSpecialization.Relay)t.AdvancementBonuses.Speed+=25;
                Emit(new BattleEvent{Kind="evolution",TowerId=t.Id,Camp=t.Camp,Value=tier});
            }
        }
        public static string EvolutionBenefit(TowerState t)
        {
            if(t.Specialization==TowerSpecialization.Arrow)return "射速与攻击范围提升";
            if(t.Specialization==TowerSpecialization.Single)return "出兵效率提升";
            if(t.Specialization==TowerSpecialization.Split)return t.AdvancementSpent==2?"新增第3条进攻线":"各路出兵效率提升";
            return "援军转发速度提升";
        }
        private static void ApplySpecialization(TowerState tower,SoldierState soldier)
        {
            // Keep original soldier HP, combat, occupy and reinforce values for every base tower.
            soldier.Speed*=1f+tower.AdvancementBonuses.Speed/100f;
            soldier.VisualRoute=tower.Specialization;soldier.VisualDoctrine=tower.Doctrine;soldier.VisualTier=tower.AdvancementSpent;soldier.VisualFocus=tower.ArtFocus;
        }
        private static void ApplyRelaySpeed(TowerState tower,SoldierState soldier)
        {if(tower.Specialization==TowerSpecialization.Relay)soldier.Speed=Mathf.Max(soldier.Speed,1f+tower.AdvancementBonuses.Speed/100f);}
    }
}
