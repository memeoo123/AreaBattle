// ArrowTower static rules: generated/arrow-evidence.json (wxcf1394487200e48f/43).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    public partial class TowerState
    {
        public float ArrowAccumulator;
        public int ArrowGrade=>Specialization==TowerSpecialization.Arrow?1:Grade;
        public float ArrowRange=>Specialization==TowerSpecialization.Arrow?1f+Mathf.Max(0,AdvancementSpent-1)*.1f:BattleSimulation.GetArrowRange(Grade);
        public float ArrowRate=>BattleSimulation.GetArrowRate(ArrowGrade)/(Specialization==TowerSpecialization.Arrow?1f+Mathf.Max(0,AdvancementSpent-1)*.2f:1f);
        public bool IsArrow { get { return ShipID == 4 || Specialization == TowerSpecialization.Arrow; } }
    }
    public sealed class ArrowState
    {
        public int Id, SourceTowerId, TargetTowerId, TargetSoldierId, LaunchCamp;
        public Vector3 Start, End, Position;
        public float Elapsed;
        public const float Duration = .2f;
        public bool Active = true;
        internal TowerState Source, TargetTower;
        internal SoldierState TargetSoldier;
    }
    public partial class BattleSimulation
    {
        // Recovered SettingConfig values; no ordinary ShipConfig exists for ShipID4.
        private static readonly float[] ArrowRates = { 1.8f, 1.3f, .8f };
        private static readonly float[] ArrowRanges = { .7f, 1f, 1.4f };
        private static readonly float[] ArrowHeights = { .2f, .24f, .28f };
        public readonly List<ArrowState> Arrows = new List<ArrowState>();
        // Injecting a sample makes list-choice tests independent of Unity's RNG state.
        public Func<int, int> ArrowTargetIndex;
        private int nextArrowId;
        public static float GetArrowRate(int grade) { return ArrowRates[grade]; }
        public static float GetArrowRange(int grade) { return ArrowRanges[grade]; }
        public static float GetArrowRegionScale(int grade) { return ArrowRanges[grade] * .44f; }
        public static bool ArrowSoldierInRange(Vector3 origin, Vector3 target, float range)
        {
            float d2 = (target - origin).sqrMagnitude;
            return range == 0f || (d2 <= range * range && d2 >= .2f * .2f);
        }
        public static bool ArrowTowerInRange(Vector3 origin, Vector3 target, float range)
        {
            float d2 = (target - origin).sqrMagnitude;
            // Original GetTowerInRange computes sqrt(min), unlike the soldier path.
            return d2 <= range * range && d2 >= Mathf.Sqrt(.2f);
        }
        public SoldierState FindArrowSoldier(TowerState source)
        {
            float range = source.ArrowRange;
            foreach (var line in Lines)
            {
                if (!line.Active || line.Direction == 0) continue;
                // Origin tower's CURRENT camp filters the entire list, even if an
                // older soldier's camp differs after its origin was captured.
                if (Tower(line.LargeTowerId).Camp != source.Camp)
                    foreach (var s in line.LargeSoldiers)
                        if (ArrowSoldierInRange(source.Position, s.Position, range)) return s;
                if (Tower(line.SmallTowerId).Camp != source.Camp)
                    foreach (var s in line.SmallSoldiers)
                        if (ArrowSoldierInRange(source.Position, s.Position, range)) return s;
            }
            return null;
        }
        public TowerState FindArrowTower(TowerState source)
        {
            var candidates = new List<TowerState>();
            foreach (var t in Towers)
                if (t.Active && t.Camp != source.Camp && ArrowTowerInRange(source.Position, t.Position, source.ArrowRange))
                    candidates.Add(t);
            if (candidates.Count == 0) return null;
            int index = ArrowTargetIndex == null ? random.Next(candidates.Count) : ArrowTargetIndex(candidates.Count);
            if (index < 0 || index >= candidates.Count) throw new InvalidOperationException("ArrowTargetIndex must return [0,count).");
            return candidates[index];
        }
        private void TickArrowTower(TowerState tower, float dt)
        {
            tower.ArrowAccumulator += dt;
            if (tower.ArrowAccumulator <= tower.ArrowRate) return;
            tower.ArrowAccumulator = 0f;
            FireArrow(tower.Id);
        }
        // Represents the recovered prefab-spawn callback when its asset is ready.
        // Async resource latency is a presentation input, not an invented fire delay.
        public ArrowState FireArrow(int sourceId)
        {
            var source = Tower(sourceId);
            if (source == null || !source.Active || !source.IsArrow || State != BattlePhase.Running) return null;
            var soldier = FindArrowSoldier(source);
            var target = soldier == null ? FindArrowTower(source) : null;
            if (soldier == null && target == null) return null;
            var a = new ArrowState { Id = ++nextArrowId, Source = source, SourceTowerId = source.Id,
                LaunchCamp = source.Camp, TargetSoldier = soldier, TargetTower = target,
                TargetSoldierId = soldier == null ? 0 : soldier.Id, TargetTowerId = target == null ? 0 : target.Id,
                Start = source.Position + Vector3.up * ArrowHeights[source.ArrowGrade] };
            a.End = soldier != null ? soldier.Position + (soldier.LegEnd - soldier.LegStart).normalized * .2f : target.Position + Vector3.up * .1f;
            a.Position = a.Start; Arrows.Add(a);
            Emit(new BattleEvent { Kind = "arrow-fire", ArrowId = a.Id, TowerId = source.Id, SoldierId = a.TargetSoldierId, Camp = source.Camp });
            return a;
        }
        private void TickArrowProjectiles(float dt)
        {
            foreach (var a in Arrows.ToArray())
            {
                if (!a.Active) continue;
                a.Elapsed += dt;
                float t = Mathf.Clamp01(a.Elapsed / ArrowState.Duration);
                // Serialized DOTween defaultEaseType6 is OutQuad.
                a.Position = Vector3.LerpUnclamped(a.Start, a.End, t * (2f - t));
                if (a.Elapsed < ArrowState.Duration) continue;
                a.Active = false;
                if (a.TargetSoldier != null) ClearSoldier(a.TargetSoldier, true);
                else if (a.TargetTower != null) ChangeScoreReference(a.TargetTower, a.Source.Camp, -1f, true, true);
                Emit(new BattleEvent { Kind = "arrow-hit", ArrowId = a.Id, TowerId = a.TargetTowerId,
                    SoldierId = a.TargetSoldierId, Camp = a.Source.Camp, Value = -1 });
            }
        }
    }
}
