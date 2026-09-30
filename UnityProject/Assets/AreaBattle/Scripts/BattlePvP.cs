// Source: generated/pvp-evidence.json. Arena matchmaking and model inference are not simulated.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public sealed class AgentSkillUseConfig
    {
        public int id, total, rate_skill1, rate_skill2, rate_skill3;
        public float DelayTime, space_skill1, space_skill2, space_skill3;
    }
    public sealed class AgentSkillSlot
    {
        public int Slot, SkillId, Level;
        public bool Active;
        public float Delay, Timer, Space, Rate, Used, Total;
    }
    public partial class BattleSimulation
    {
        public bool PvPEnabled { get; private set; }
        public int PvPEnemyCamp { get; private set; }
        public int PvPCommanderId { get; private set; }
        public readonly List<AgentSkillSlot> PvPSlots = new List<AgentSkillSlot>();
        // RandomHelper chance is Next(0,100), whereas its ranged helper includes max.
        public Func<int> PvPChanceSample;
        public Func<int, int> PvPTargetIndex;
        public Func<int, int, int> PvPInclusiveInteger;
        // Skill2's camera projection must be supplied by a camera-owning host.
        public Func<Vector3> PvPFireballOrigin;
        private AgentSkillUseConfig pvpUseConfig;

        public static int SelectPvPEnemyCamp(IEnumerable<CampInfoCfg> camps)
        {
            int camp = 0;
            foreach (var row in camps) if (row.CampID >= 2 && row.CampID <= 4) camp = row.CampID;
            return camp;
        }
        // Commander and its three saved skill levels are explicit fixture inputs.
        // This avoids inventing an Arena opponent record or a model's line actions.
        public void ConfigurePvP(int commanderId, int[] skillLevels, AgentSkillUseConfig useConfig, int enemyCamp = 0)
        {
            if (commanderId < 1 || commanderId > 6) throw new ArgumentOutOfRangeException("commanderId");
            if (skillLevels == null || skillLevels.Length != 3 || useConfig == null) throw new ArgumentException("Three skill levels and AgentSkillUseConfig are required.");
            PvPEnemyCamp = enemyCamp == 0 ? SelectPvPEnemyCamp(initialCamps) : enemyCamp;
            if (PvPEnemyCamp < 2 || PvPEnemyCamp > 4) throw new ArgumentException("A recovered enemy camp in 2..4 is required.");
            PvPCommanderId = commanderId; pvpUseConfig = useConfig; PvPEnabled = true;
            while (PvPSlots.Count < 3) PvPSlots.Add(new AgentSkillSlot());
            for (int i = 0; i < 3; i++)
            {
                var slot = PvPSlots[i]; slot.Slot = i + 1; slot.SkillId = (commanderId - 1) * 3 + i + 1;
                slot.Level = skillLevels[i];
            }
            RestartPvpAgents();
        }
        public void DisablePvP()
        {
            PvPEnabled = false;
            foreach (var slot in PvPSlots) slot.Active = false;
        }
        private void RestartPvpAgents()
        {
            if (!PvPEnabled || pvpUseConfig == null) return;
            foreach (var slot in PvPSlots)
            {
                slot.Active = true; slot.Delay = pvpUseConfig.DelayTime; slot.Timer = 0;
                slot.Total = pvpUseConfig.total;
                slot.Rate = slot.Slot == 1 ? pvpUseConfig.rate_skill1 : slot.Slot == 2 ? pvpUseConfig.rate_skill2 : pvpUseConfig.rate_skill3;
                slot.Space = slot.Slot == 1 ? pvpUseConfig.space_skill1 : slot.Slot == 2 ? pvpUseConfig.space_skill2 : pvpUseConfig.space_skill3;
                // AgentSkillUse.Init never resets usedCount; Clear only clears active.
            }
        }
        private bool TryEvaluatePvpOutcome()
        {
            if (!PvPEnabled) return false;
            if (State != BattlePhase.Running) return true;
            int enemyScore = GetCampScore(PvPEnemyCamp), playerScore = GetCampScore(PlayerCampID);
            if (enemyScore <= 0) SetPhase(BattlePhase.Victory);
            else if (playerScore <= 0) SetPhase(BattlePhase.Defeat);
            return true;
        }
        public int GetCampScore(int camp)
        {
            int total = 0;
            foreach (var tower in Towers) if (tower.Active && tower.Camp == camp) total += (int)tower.Score;
            return total;
        }
        private void TickPvpAgents(float rawDelta)
        {
            if (!PvPEnabled || State != BattlePhase.Running) return;
            float dt = rawDelta * Configs.GameTimeScale;
            foreach (var slot in PvPSlots)
            {
                if (!slot.Active) continue;
                if (slot.Delay > 0f) { slot.Delay -= dt; continue; }
                slot.Timer -= dt;
                if (slot.Timer >= 0f || slot.Used > slot.Total) continue;
                slot.Timer += slot.Space;
                int sample = PvPChanceSample == null ? random.Next(0, 100) : PvPChanceSample();
                if (sample < 0 || sample >= 100) throw new InvalidOperationException("PvP chance sample must be in [0,100).");
                if (sample >= (int)slot.Rate) continue;
                if (!IsSkillActive(slot.SkillId, PvPEnemyCamp)) CastAgentSkill(slot.SkillId, slot.Level);
                slot.Used += 1f;
            }
        }
        private TowerState PickPvPTower(Predicate<TowerState> predicate)
        {
            var candidates = Towers.FindAll(predicate);
            if (candidates.Count == 0) return null;
            int index = PvPTargetIndex == null ? random.Next(candidates.Count) : PvPTargetIndex(candidates.Count);
            if (index < 0 || index >= candidates.Count) throw new InvalidOperationException("PvP target index outside candidate list.");
            return candidates[index];
        }
        private int PvPRanged(int minimum, int maximum)
        {
            return PvPInclusiveInteger == null ? random.Next(minimum, maximum + 1) : PvPInclusiveInteger(minimum, maximum);
        }
        public bool CastAgentSkill(int skillId, int level)
        {
            if (!PvPEnabled || IsSkillActive(skillId, PvPEnemyCamp)) return false;
            TowerState target = null; Vector3? point = null, origin = null;
            if (skillId == 3 || skillId == 12 || skillId == 18)
                target = PickPvPTower(t => t.Active && t.Camp == PlayerCampID);
            else if (skillId == 9) target = PickPvPTower(t => t.Active && t.Camp == PvPEnemyCamp);
            else if (skillId == 15) target = PickPvPTower(t => t.Active);
            if ((skillId == 3 || skillId == 9 || skillId == 12 || skillId == 15 || skillId == 18) && target == null)
                return RejectSkill("The original agent target list is empty; no fallback target is invented.");
            if (skillId == 18) point = target.Position + new Vector3(PvPRanged(-20, 20) / 100f, 0f, PvPRanged(-20, 20) / 100f);
            if (skillId == 2 && PvPFireballOrigin != null) origin = PvPFireballOrigin();
            return CastSkill(skillId, level, PvPEnemyCamp, target == null ? 0 : target.Id, point, origin, true);
        }
    }

    // DailyChallengeData consumes persisted ordered pools. Pool fill/shuffle provenance
    // remains unknown; callers must supply recovered data rather than create a shuffle.
    public static class DailyChallengeLayoutMap
    {
        public static void Advance(IDictionary<int, List<int>> pools, IDictionary<int, int> today)
        {
            foreach (var pair in pools)
            {
                if (!today.ContainsKey(pair.Key) || pair.Value.Count == 0)
                    throw new InvalidOperationException("Recovered daily pool/mapping is incomplete.");
                int last = pair.Value.Count - 1; today[pair.Key] = pair.Value[last]; pair.Value.RemoveAt(last);
            }
        }
        public static int Resolve(IDictionary<int, int> today, int currentChallenge, int selectedChallenge, bool unlockAll)
        { return today[unlockAll ? selectedChallenge : currentChallenge]; }
    }
}
