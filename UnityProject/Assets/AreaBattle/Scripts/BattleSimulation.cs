// Ordinary-battle kernel recovered from wxcf1394487200e48f/43.
// Sources: combat-evidence.json, controls-evidence.json and flow-evidence.json.
// Presentation, physics adjacency, tutorial, commander and boss behavior live outside this kernel.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public struct IntVector3
    {
        public int x, y, z;
        public Vector3 WorldPosition { get { return new Vector3(x / 100f, y / 100f, z / 100f); } }
    }
    [Serializable] public class CampInfoCfg { public int CampID, EnityID, AIGrade, BossActionId; }
    [Serializable] public class StarInfoCfg
    {
        public IntVector3 pos, angle, scale;
        public int CampID, StartScore, ShipID, bossSkillId;
        public bool isBoss;
        // Preserved legacy JSON fields; the current ordinary runtime does not consume these.
        public int DispathchID, HeroId;
        public float CollisionRadius;
        public Vector3 CollosionSize, CollosionCenter;
    }
    [Serializable] public class ObstacleInfoCfg
    {
        public IntVector3 pos, angle, scale;
        public int ObstacleID, EntityID, EnityID, id;
    }
    [Serializable] public class LevelLayout:IOutgameLevelLayout
    {
        public StarInfoCfg[] Stars=>StarInfoCfgs;
        public ObstacleInfoCfg[] Obstacles=>ObstacleInfoCfgs;
        public string DisplayName;
        public bool BasicTowerExperiment;
        public CampInfoCfg[] CampInfoCfgs;
        public StarInfoCfg[] StarInfoCfgs;
        public ObstacleInfoCfg[] ObstacleInfoCfgs;
    }
    [Serializable] public class ShipConfig
    {
        public int id, EntityID, shipType, hp, attack, occupy, reinforce, voyage;
        public float param1, param2;
    }
    [Serializable] public class AIConfig
    {
        public int id, AIType, DelayTime, ActionNum;
        public int[] ActionTime, Protect, Defend, Reinforce, Occupy;
    }
    [Serializable] public class DispatchConfig
    {
        public int id, scoreLimit, maxLine, addSpace, swanpSpaceOne, swanpSpaceTwo, swanpSpaceThree;
    }
    [Serializable] public class BattleConfigData
    {
        public ShipConfig[] Ships;
        public AIConfig[] AIs;
        public DispatchConfig[] Dispatch;
        public float GameTimeScale = 1f, ShipTimeScale = 0.8f;
    }
    public enum BattlePhase { Running = 6, Pause = 7, Victory = 8, Defeat = 9 }
    public sealed class BattleEvent
    {
        public string Kind;
        public int TowerId, LineId, SoldierId, ArrowId, Camp, PreviousCamp, SourceCamp, AudioId, EffectId;
        public float Value, Amount;
        public BattlePhase Phase;
    }
    public partial class TowerState
    {
        public int Id, Camp, Grade, ShipID, OutgoingCount, MaxLines;
        public float Score, MaxScore, SpawnTime, RegenInterval, RegenAccumulator;
        public Vector3 Position;
        public float CollisionRadius = 0.1f;
        public bool Active = true, AutoAddScore = true;
        public int Mode; // Normal=0, Tree=1, Rain=3, Unlinkable=4, Undead=5, Comm04_2=6.
        public readonly List<int> ForwardingHistory = new List<int>();
        public bool CanAddLine { get { return Active && ShipID != 4 && OutgoingCount < MaxLines; } }
    }
    public partial class LineState
    {
        public int Id, SmallTowerId, LargeTowerId, Direction;
        public float Length, SmallSpawnTimer, LargeSpawnTimer;
        public bool Active = true;
        public readonly List<SoldierState> SmallSoldiers = new List<SoldierState>();
        public readonly List<SoldierState> LargeSoldiers = new List<SoldierState>();
        public bool IsFrom(int towerId)
        {
            return towerId == SmallTowerId ? (Direction & 1) != 0 :
                towerId == LargeTowerId && (Direction & 2) != 0;
        }
        public int Other(int towerId) { return towerId == SmallTowerId ? LargeTowerId : towerId == LargeTowerId ? SmallTowerId : 0; }
    }
    public partial class SoldierState
    {
        public int Id, Camp, HP, Attack, Occupy, Reinforce, Voyage, ShipID, ShipType;
        public int OriginTowerId, SourceTowerId, TargetTowerId, LineId;
        public Vector3 Position, LegStart, LegEnd;
        public float Speed = 1f;
        public bool Active = true;
        public bool PlayDeathAnimation;
    }

    public partial class BattleSimulation
    {
        public const int PlayerCampID = 1;
        public readonly List<TowerState> Towers = new List<TowerState>();
        public readonly List<LineState> Lines = new List<LineState>();
        public readonly List<SoldierState> Soldiers = new List<SoldierState>();
        public BattlePhase State { get; private set; }
        public float Elapsed { get; private set; }
        public bool AIEnabled = true;
        public event Action<BattleEvent> Event;
        public readonly BattleConfigData Configs;
        // Extension inputs default to the verified unmodified ordinary loadout.
        public Func<TowerState, float> SpawnMultiplier = delegate { return 1f; };
        public Func<TowerState, float> RegenMultiplier = delegate { return 1f; };
        public Func<SoldierState, float> SpeedMultiplier = delegate { return 1f; };
        // AIGoToAction shuffles with Unity Random, independently of RandomHelper.
        // A replay can supply recorded indexes without consuming either live stream.
        public Func<int, int> AIShuffleIndex;

        private readonly Dictionary<int, ShipConfig> ships = new Dictionary<int, ShipConfig>();
        private readonly Dictionary<int, AIConfig> aiConfigs = new Dictionary<int, AIConfig>();
        private readonly Dictionary<int, DispatchConfig> dispatch = new Dictionary<int, DispatchConfig>();
        private readonly Dictionary<long, LineState> pairs = new Dictionary<long, LineState>();
        private readonly Dictionary<int, List<LineState>> adjacency = new Dictionary<int, List<LineState>>();
        private readonly List<StarInfoCfg> initialTowers = new List<StarInfoCfg>();
        private readonly List<CampInfoCfg> initialCamps = new List<CampInfoCfg>();
        private readonly List<AIClock> aiClocks = new List<AIClock>();
        private readonly Func<int, int, bool> topology;
        private readonly System.Random random;
        private int nextSoldierId;
        private float outcomeTimer;
        private bool outcomeDirty;
        private class AIClock { public int Camp; public AIConfig Config; public float Delay, Timer; }
        partial void BeforeBattleTick(float dt);
        partial void AfterBattleTick(float dt);
        partial void OnSoldierCreated(SoldierState soldier);
        partial void OnBattleRestarted();

        public BattleSimulation(LevelLayout layout, BattleConfigData configs, int seed, Func<int, int, bool> canConnect, System.Random randomSource = null)
        {
            if (layout == null || layout.StarInfoCfgs == null) throw new ArgumentException("A recovered layout is required.", "layout");
            if (configs == null) throw new ArgumentNullException("configs");
            if (canConnect == null) throw new ArgumentNullException("canConnect", "Supply the collider-verified pair topology.");
            Configs = configs; topology = canConnect; BasicTowerExperiment = layout.BasicTowerExperiment;
            // The original RandomHelper owns one static System.Random. A seed here
            // creates an explicit isolated test stream; live views supply the shared stream.
            random = randomSource ?? new System.Random(seed);
            foreach (var row in configs.Ships ?? Array.Empty<ShipConfig>()) ships.Add(row.id, row);
            foreach (var row in configs.AIs ?? Array.Empty<AIConfig>()) aiConfigs.Add(row.id, row);
            foreach (var row in configs.Dispatch ?? Array.Empty<DispatchConfig>()) dispatch.Add(row.id, row);
            for (int id = 1; id <= 3; id++) if (!dispatch.ContainsKey(id)) throw new ArgumentException("Missing ordinary DispatchConfig " + id);
            foreach (var cfg in layout.StarInfoCfgs)
            {
                if (!cfg.isBoss && cfg.ShipID != 4 && !ships.ContainsKey(cfg.ShipID))
                    throw new NotSupportedException("This kernel slice requires ordinary towers with recovered soldier config: ShipID=" + cfg.ShipID);
                initialTowers.Add(new StarInfoCfg { pos = cfg.pos, CampID = cfg.CampID, StartScore = cfg.StartScore, ShipID = cfg.ShipID,
                    isBoss = cfg.isBoss, bossSkillId = cfg.bossSkillId });
            }
            foreach (var cfg in layout.CampInfoCfgs ?? Array.Empty<CampInfoCfg>())
                initialCamps.Add(new CampInfoCfg { CampID = cfg.CampID, AIGrade = cfg.AIGrade, EnityID = cfg.EnityID, BossActionId = cfg.BossActionId });
            Restart();
        }

        public TowerState Tower(int id) { return id > 0 && id <= Towers.Count ? Towers[id - 1] : null; }
        public LineState Line(int id) { return id > 0 && id <= Lines.Count ? Lines[id - 1] : null; }
        public LineState FindLine(int a, int b) { LineState line; pairs.TryGetValue(PairKey(a, b), out line); return line; }
        // GuideControl retains and may clear this actual sorted adjacency list.
        public List<LineState> GetPotentialLines(int towerId) { return adjacency[towerId]; }
        private static long PairKey(int a, int b) { return ((long)Math.Min(a, b) << 32) | (uint)Math.Max(a, b); }
        private DispatchConfig GradeConfig(int grade) { return dispatch[grade == 0 ? 1 : grade == 1 ? 2 : 3]; }
        public float GetSpawnTime(int grade, int lines)
        {
            var c = GradeConfig(grade);
            return (float)(lines == 1 ? c.swanpSpaceOne : lines == 2 ? c.swanpSpaceTwo : c.swanpSpaceThree) / 1000f;
        }
        public int GetDispatchLineNum(int grade) { return GradeConfig(grade).maxLine; }
        public float GetDispatchAddScoreTime(int grade) { return (float)GradeConfig(grade).addSpace / 1000f; }
        public int GetDispatchScoreNum(int grade) { return GradeConfig(grade).scoreLimit; }

        public void Restart()
        {
            // The original tower pools reuse entities on retry. Keep their identity for
            // retained arrow callbacks; ArrowTower.Clear does not reset its clock.
            var reusedTowers = Towers.ToArray();
            foreach (var soldier in Soldiers) soldier.Active = false;
            Towers.Clear(); Lines.Clear(); Soldiers.Clear(); pairs.Clear(); adjacency.Clear(); aiClocks.Clear();
            nextSoldierId = 0; Elapsed = 0; outcomeTimer = 0; outcomeDirty = false;
            evolutionReaction=.6f; evolutionPlanning=4.5f;
            for (int i = 0; i < initialTowers.Count; i++)
            {
                var c = initialTowers[i];
                var t = i < reusedTowers.Length ? reusedTowers[i] : new TowerState();
                t.Id = i + 1; t.Camp = c.CampID; t.Score = c.StartScore; t.ShipID = c.ShipID;
                t.Position = c.pos.WorldPosition; t.MaxScore = GetDispatchScoreNum(2);
                t.Active = true; t.AutoAddScore = true; t.Mode = 0; t.Grade = 0;
                t.OutgoingCount = 0; t.RegenAccumulator = 0; t.CollisionRadius = .1f;
                t.Specialization = TowerSpecialization.None; t.RememberedSpecialization = TowerSpecialization.None; t.AdvancementEarned = 0; t.Doctrine = -1; t.ArtFocus = 0; t.AdvancementHistory.Clear(); t.AdvancementBonuses = new AdvancementStats(); t.AdvancementName = "";
                t.ForwardingHistory.Clear();
                Towers.Add(t); adjacency.Add(t.Id, new List<LineState>()); RefreshTower(t, false);
            }
            for (int i = 0; i < Towers.Count; i++) for (int j = i + 1; j < Towers.Count; j++)
            {
                if (!topology(Towers[i].Id, Towers[j].Id)) continue;
                var l = new LineState { Id = Lines.Count + 1, SmallTowerId = Towers[i].Id, LargeTowerId = Towers[j].Id,
                    Length = Vector3.Distance(Towers[i].Position, Towers[j].Position) };
                Lines.Add(l); pairs.Add(PairKey(l.SmallTowerId, l.LargeTowerId), l);
                adjacency[l.SmallTowerId].Add(l); adjacency[l.LargeTowerId].Add(l);
            }
            foreach (var list in adjacency.Values) list.Sort((a, b) => a.Length.CompareTo(b.Length));
            foreach (var c in initialCamps)
            {
                if (c.CampID == 0 || c.CampID == PlayerCampID) continue;
                AIConfig a; if (!aiConfigs.TryGetValue(c.AIGrade, out a)) continue;
                aiClocks.Add(new AIClock { Camp = c.CampID, Config = a, Delay = a.DelayTime / 1000f });
            }
            State = BattlePhase.Running;
            OnBattleRestarted();
            Emit(new BattleEvent { Kind = "restart", Phase = State });
            RestartPvpAgents();
            EvaluateOutcome();
        }
        public void StartBattle() { if (State == BattlePhase.Pause) Pause(false); }
        public void Pause(bool paused)
        {
            if (State != BattlePhase.Running && State != BattlePhase.Pause) return;
            SetPhase(paused ? BattlePhase.Pause : BattlePhase.Running);
        }
        private void SetPhase(BattlePhase phase)
        {
            if (State == phase) return;
            State = phase; Emit(new BattleEvent { Kind = "phase", Phase = phase });
        }
        private void Emit(BattleEvent e) { var callback = Event; if (callback != null) callback(e); }

        public bool Connect(int sourceId, int targetId, bool ai = false)
        {
            if (State != BattlePhase.Running) return false;
            var s = Tower(sourceId); var t = Tower(targetId);
            if (s == null || t == null || sourceId == targetId || !s.Active || !t.Active || !s.CanAddLine) return false;
            if (!ai && s.Camp != PlayerCampID) return false;
            if (ai && (s.ShipID == 4 || s.Mode == 4)) return false;
            return AddDirection(FindLine(sourceId, targetId), s, t);
        }
        private bool AddDirection(LineState l, TowerState source, TowerState target)
        {
            if (l == null || !l.Active || l.Direction == 3) return false;
            int direction = source.Id < target.Id ? 1 : 2;
            if (l.Direction == direction) return false;
            if (l.Direction != 0 && source.Camp != target.Camp) direction = 3;
            SetDirection(l, direction);
            Emit(new BattleEvent { Kind = "connect", TowerId = source.Id, LineId = l.Id, Camp = source.Camp, AudioId = 2007 });
            return true;
        }
        // WayLine.AddActiveLine (f14740) is distinct from SetLineState. AI actions
        // 3/4 check source capacity before enumerating targets, then use this path.
        // In particular, they must not reverse an existing same-camp direction.
        public bool TryActivateLine(int sourceId, int targetId)
        {
            if (State != BattlePhase.Running) return false;
            var source = Tower(sourceId); var target = Tower(targetId);
            if (source == null || target == null || sourceId == targetId) return false;
            return ActivateDirection(FindLine(sourceId, targetId), source, target);
        }
        private bool ActivateDirection(LineState line, TowerState source, TowerState target)
        {
            if (line == null || !line.Active || source.Mode == 4 || line.Other(source.Id) != target.Id) return false;
            if (line.Direction != 0 && source.Camp == target.Camp) return false;
            return AddDirection(line, source, target);
        }
        private void SetDirection(LineState line, int direction)
        {
            line.Direction = direction;
            // Direction changes preserve both spawn clocks and existing in-flight soldiers.
            RefreshOutgoing(Tower(line.SmallTowerId)); RefreshOutgoing(Tower(line.LargeTowerId));
        }
        public bool CutPlayerLine(int lineId)
        {
            if (State != BattlePhase.Running) return false;
            var l = Line(lineId); if (l == null) return false;
            int result = l.Direction;
            if (Tower(l.SmallTowerId).Camp == PlayerCampID) result &= ~1;
            if (Tower(l.LargeTowerId).Camp == PlayerCampID) result &= ~2;
            if (result == l.Direction) return false;
            SetDirection(l, result); Emit(new BattleEvent { Kind = "cut", LineId = l.Id, AudioId = 2008 }); return true;
        }
        public void RemoveOutgoing(int towerId)
        {
            foreach (var l in Lines)
                if (l.IsFrom(towerId)) SetDirection(l, l.Direction & ~(l.SmallTowerId == towerId ? 1 : 2));
        }
        private void RefreshOutgoing(TowerState tower)
        {
            int count = 0; foreach (var l in Lines) if (l.Active && l.IsFrom(tower.Id)) count++;
            tower.OutgoingCount = count; tower.SpawnTime = ConnectionSpawnTime(tower);
        }
        private void RefreshTower(TowerState tower, bool notify)
        {
            int old = tower.Grade, score = (int)tower.Score;
            tower.Grade = score <= GetDispatchScoreNum(0) ? 0 : score <= GetDispatchScoreNum(1) ? 1 : 2;
            DowngradeEvolution(tower);
            tower.MaxLines = ConnectionCapacity(tower);
            if (!tower.IsBoss && tower.ShipID >= 1 && tower.ShipID <= 3 && tower.Score >= 10 && (!BasicTowerExperiment || tower.Specialization != TowerSpecialization.None || tower.Camp == PlayerCampID)) tower.AdvancementEarned = System.Math.Max(tower.AdvancementEarned, System.Math.Min(MaximumAdvancements, (int)tower.Score / AdvancementStep));
            ApplyAutomaticEvolution(tower);
            tower.MaxLines = ConnectionCapacity(tower);
            if(BasicTowerExperiment&&tower.OutgoingCount>tower.MaxLines){
                var extra=Outgoing(tower.Id);
                for(int i=tower.MaxLines;i<extra.Count;i++){var line=extra[i];SetDirection(line,line.Direction&~(line.SmallTowerId==tower.Id?1:2));}
            }
            tower.RegenInterval = GetDispatchAddScoreTime(tower.Grade);
            tower.SpawnTime = ConnectionSpawnTime(tower);
            if (notify && old != tower.Grade)
                Emit(new BattleEvent { Kind = "grade", TowerId = tower.Id, Value = tower.Grade,
                    EffectId = tower.Grade > old ? 105 : 0, AudioId = tower.Grade > old ? 2014 : 0 });
        }
        public void ChangeScore(int towerId, int sourceCamp, float delta, bool preventCapture = false)
        {
            ChangeScoreReference(Tower(towerId), sourceCamp, delta, preventCapture);
        }
        private void ChangeScoreReference(TowerState t, int sourceCamp, float delta, bool preventCapture, bool ignoreActive = false)
        {
            if (t == null || (!ignoreActive && !t.Active) || delta == 0f) return;
            int previousCamp = t.Camp;
            t.Score += delta;
            bool captured = t.Score <= 0f && !preventCapture && !t.IsBoss;
            if (captured)
            {
                t.Score = -t.Score; int previous = t.Camp;
                RemoveOutgoing(t.Id); t.Camp = sourceCamp;
                // A capture crosses zero even when excess damage leaves a positive remainder.
                if (BasicTowerExperiment && previous != sourceCamp) ResetEvolution(t);
                if (previous != sourceCamp) t.Mode = 0;
                Emit(new BattleEvent { Kind = "capture", TowerId = t.Id, PreviousCamp = previous, Camp = sourceCamp,
                    AudioId = sourceCamp == PlayerCampID ? 2005 : previous == PlayerCampID ? 2006 : 0 });
            }
            else t.Score = Mathf.Clamp(t.Score, 0f, t.MaxScore);
            RefreshTower(t, true); outcomeDirty = true;
            Emit(new BattleEvent { Kind = "score", TowerId = t.Id, Camp = t.Camp, PreviousCamp = previousCamp,
                SourceCamp = sourceCamp, Amount = delta, Value = t.Score });
            if (captured) EvaluateOutcome();
        }
        public void EvaluateOutcome()
        {
            if (TryEvaluatePvpOutcome()) return;
            int mine = 0; foreach (var t in Towers) if (t.Active && t.Camp == PlayerCampID) mine++;
            if (mine == 0) SetPhase(BattlePhase.Defeat);
            else if (mine >= initialTowers.Count) SetPhase(BattlePhase.Victory);
            else
            {
                // RefreshCampInfo f4384: only the last created curBoss is retained;
                // score is truncated toward zero, and no Boss.Active check is made.
                TowerState currentBoss = null;
                foreach (var t in Towers) if (t.IsBoss) currentBoss = t;
                if (currentBoss != null && (int)currentBoss.Score <= 0) SetPhase(BattlePhase.Victory);
            }
        }

        public void Tick(float dt)
        {
            if (dt < 0 || float.IsNaN(dt) || float.IsInfinity(dt)) throw new ArgumentOutOfRangeException("dt");
            // DOMove uses Unity scaled time and outlives a result transition.
            // Advance existing projectiles before issuing this frame's new attacks.
            if (State != BattlePhase.Pause) TickArrowProjectiles(dt);
            if (State != BattlePhase.Running) return;
            AdvanceEnemyTowers();
            TickPvpAgents(dt);
            TickEnemySkillTimers(dt);
            TickBosses(dt);
            Elapsed += dt;
            // MineGameLogicModule registration order: AI -> LevelControl -> WayLineControl.
            TickEvolutionAI(dt);
            if (AIEnabled) foreach (var a in aiClocks)
            {
                if(BasicTowerExperiment&&!Towers.Exists(t=>t.IsBoss&&t.Camp==a.Camp))continue;
                if (a.Delay > 0f) { a.Delay -= dt; continue; }
                a.Timer -= dt;
                if (a.Timer < 0f) { a.Timer += GetAIActionTime(a.Config); RunAI(a.Camp, a.Config); }
            }
            float scaledDelta = dt * Configs.GameTimeScale;
            foreach (var t in Towers)
            {
                if (!t.Active) continue;
                float towerDelta = 0f;
                if (t.Mode != 1 && t.Camp != 0 && t.OutgoingCount == 0 && t.AutoAddScore)
                {
                    t.RegenAccumulator += scaledDelta;
                    if (t.RegenAccumulator >= t.RegenInterval)
                    {
                        t.RegenAccumulator -= t.RegenInterval;
                        towerDelta += RegenMultiplier(t) * (1f + t.AdvancementBonuses.Regen / 100f);
                    }
                }
                // Tower.Update combines regeneration and rain into one ChangeScore.
                towerDelta += SkillTowerRainDelta(t, scaledDelta);
                if (towerDelta != 0f) ChangeScore(t.Id, t.Camp, towerDelta, true);
                if (t.IsArrow) TickArrowTower(t, scaledDelta);
                // Boss.Update occupies its original place in list_tower, after base
                // Tower.Update. Earlier towers see newly applied Rain next frame.
                TickBossTower(t, scaledDelta);
            }
            AdvanceEnemyTowers();
            if (outcomeDirty)
            {
                outcomeTimer -= dt;
                if (outcomeTimer <= 0f) { outcomeTimer += 1f; outcomeDirty = false; EvaluateOutcome(); }
            }
            if (State != BattlePhase.Running) return;
            // Registration order: LevelControl -> SkillControl -> WayLineControl.
            BeforeBattleTick(dt);
            foreach (var line in Lines) if (line.Active) TickLine(line, scaledDelta);
            AfterBattleTick(dt);
        }
        private void TickLine(LineState l, float dt)
        {
            var small = Tower(l.SmallTowerId); var large = Tower(l.LargeTowerId);
            TickSpawn(l, small, large, 1, dt); TickSpawn(l, large, small, 2, dt);
            float length = dt * Configs.ShipTimeScale;
            MoveList(l.SmallSoldiers, length); MoveList(l.LargeSoldiers, length);
            ArriveList(l.SmallSoldiers, l.Id); ArriveList(l.LargeSoldiers, l.Id);
            float collisionDistance = Mathf.Max(length, 0.1f);
            foreach (var a in l.SmallSoldiers.ToArray())
            {
                if (!a.Active || a.LineId != l.Id) continue;
                foreach (var b in l.LargeSoldiers.ToArray())
                {
                    if (!b.Active || b.LineId != l.Id || a.Camp == b.Camp) continue;
                    if (Vector3.Distance(a.Position, b.Position) > collisionDistance) continue;
                    ResolveSoldierContact(a, b); break;
                }
            }
        }
        private void TickSpawn(LineState l, TowerState source, TowerState target, int bit, float dt)
        {
            if ((l.Direction & bit) == 0 || source.Mode == 1) return;
            float timer = (bit == 1 ? l.SmallSpawnTimer : l.LargeSpawnTimer) + dt;
            if (timer > source.SpawnTime / SpawnMultiplier(source)) { timer = 0f; SpawnSoldier(source.Id, target.Id); }
            if (bit == 1) l.SmallSpawnTimer = timer; else l.LargeSpawnTimer = timer;
        }
        public SoldierState SpawnSoldier(int sourceId, int targetId)
        {
            var t = Tower(sourceId); var line = FindLine(sourceId, targetId);
            if (t == null || t.IsArrow || line == null || !line.IsFrom(sourceId)) return null;
            var c = ships[t.ShipID];
            var s = new SoldierState { Id = ++nextSoldierId, Camp = t.Camp, HP = c.hp, Attack = c.attack,
                Occupy = c.occupy, Reinforce = c.reinforce, Voyage = c.voyage, ShipID = c.id,
                ShipType = c.shipType, OriginTowerId = sourceId };
            Soldiers.Add(s); AddSoldierToLine(s, line, sourceId);
            OnSoldierCreated(s);
            ApplySpecialization(t, s);
            Emit(new BattleEvent { Kind = "spawn", SoldierId = s.Id, TowerId = sourceId, LineId = line.Id, Camp = s.Camp });
            return s;
        }
        private void AddSoldierToLine(SoldierState s, LineState l, int sourceId)
        {
            if (!s.Active || !l.IsFrom(sourceId)) { ClearSoldier(s); return; }
            Detach(s); s.LineId = l.Id; s.SourceTowerId = sourceId; s.TargetTowerId = l.Other(sourceId);
            s.LegStart = Tower(sourceId).Position; s.LegEnd = Tower(s.TargetTowerId).Position; s.Position = s.LegStart;
            (sourceId == l.SmallTowerId ? l.SmallSoldiers : l.LargeSoldiers).Add(s);
        }
        private void Detach(SoldierState s)
        {
            var old = Line(s.LineId);
            if (old != null) { old.SmallSoldiers.Remove(s); old.LargeSoldiers.Remove(s); }
            s.LineId = 0;
        }
        private void ClearSoldier(SoldierState s, bool deathAnimation = false)
        {
            if (s == null || !s.Active) return;
            s.PlayDeathAnimation = deathAnimation;
            s.Active = false; Detach(s); Emit(new BattleEvent { Kind = "soldier-clear", SoldierId = s.Id, Camp = s.Camp });
        }
        private void MoveList(List<SoldierState> list, float length)
        {
            foreach (var s in list)
                if (s.Active) s.Position += (s.LegEnd - s.LegStart).normalized * (length * s.Speed * SpeedMultiplier(s));
        }
        private void ArriveList(List<SoldierState> list, int lineId)
        {
            foreach (var s in list.ToArray())
            {
                if (!s.Active || s.LineId != lineId) continue;
                var target = Tower(s.TargetTowerId);
                if (Vector3.Distance(s.Position, target.Position) <= target.CollisionRadius ||
                    Vector3.Distance(s.Position, s.LegStart) >= Vector3.Distance(s.LegStart, s.LegEnd))
                {
                    ArriveSoldier(s, target);
                    if (s.Camp == PlayerCampID || target.Camp == PlayerCampID)
                        Emit(new BattleEvent { Kind = "arrival", SoldierId = s.Id, TowerId = target.Id,
                            LineId = lineId, Camp = s.Camp, AudioId = 2012 });
                }
            }
        }
        public void ArriveSoldier(SoldierState soldier, TowerState target)
        {
            if (soldier == null || target == null || !soldier.Active) return;
            Detach(soldier);
            if (soldier.Camp != target.Camp)
            {
                ChangeScore(target.Id, soldier.Camp, -soldier.Occupy); ClearSoldier(soldier);
            }
            else if ((int)target.Score < target.MaxScore &&
                !(target.Specialization == TowerSpecialization.Relay && Outgoing(target.Id).Count > 0))
            {
                ChangeScore(target.Id, soldier.Camp, soldier.Reinforce); ClearSoldier(soldier);
            }
            else
            {
                soldier.Voyage--;
                if (soldier.OriginTowerId == target.Id || soldier.Voyage <= 0) { ClearSoldier(soldier); return; }
                var outgoing = Outgoing(target.Id);
                LineState selected = outgoing.Find(l => !target.ForwardingHistory.Contains(l.Id));
                if (selected == null) { target.ForwardingHistory.Clear(); if (outgoing.Count > 0) selected = outgoing[0]; }
                if (selected == null) { ClearSoldier(soldier); return; }
                ApplyRelaySpeed(target, soldier);
                target.ForwardingHistory.Add(selected.Id); AddSoldierToLine(soldier, selected, target.Id);
                Emit(new BattleEvent { Kind = "forward", SoldierId = soldier.Id, TowerId = target.Id, LineId = selected.Id });
            }
        }
        public void ResolveSoldierContact(SoldierState a, SoldierState b)
        {
            if (a == null || b == null || !a.Active || !b.Active || a.Camp == b.Camp) return;
            a.HP -= b.Attack; b.HP -= a.Attack;
            Emit(new BattleEvent { Kind = "contact", SoldierId = a.Id, LineId = a.LineId,
                AudioId = (a.HP <= 0 || b.HP <= 0) && (a.Camp == PlayerCampID || b.Camp == PlayerCampID) ? 2013 : 0 });
            if (a.HP <= 0) ClearSoldier(a, true); if (b.HP <= 0) ClearSoldier(b, true);
        }
        private List<LineState> Outgoing(int towerId) { return Lines.FindAll(l => l.Active && l.IsFrom(towerId)); }
        private List<TowerState> Incoming(int towerId)
        {
            var result = new List<TowerState>();
            foreach (var l in Lines)
            {
                int other = l.Other(towerId);
                if (l.Active && other != 0 && l.IsFrom(other)) result.Add(Tower(other));
            }
            return result;
        }

        private float GetAIActionTime(AIConfig a)
        {
            return a.ActionTime == null || a.ActionTime.Length == 0 ? float.MaxValue : a.ActionTime[random.Next(a.ActionTime.Length)] / 1000f;
        }
        public int GetAIAction(AIConfig cfg, TowerState tower)
        {
            int score = (int)tower.Score;
            if (cfg.AIType == 2 || cfg.AIType == 3)
            {
                int q = (int)tower.MaxScore / 5;
                return score < q ? 1 : score < q * 2 ? 2 : score < q * 3 ? 4 : 3;
            }
            if (cfg.AIType != 0 && cfg.AIType != 1) return 0;
            var thresholds = new List<int>(); var actions = new List<int>(); int sum = 0;
            AddWeight(cfg.Protect, 1, score, tower.MaxScore, thresholds, actions, ref sum);
            AddWeight(cfg.Defend, 2, score, tower.MaxScore, thresholds, actions, ref sum);
            AddWeight(cfg.Reinforce, 4, score, tower.MaxScore, thresholds, actions, ref sum);
            AddWeight(cfg.Occupy, 3, score, tower.MaxScore, thresholds, actions, ref sum);
            int choice = random.Next(0, sum + 1);
            for (int i = 0; i < thresholds.Count; i++) if (thresholds[i] > choice) return actions[i];
            return 0; // The original inclusive terminal sample also yields no action.
        }
        private static void AddWeight(int[] pair, int action, int score, float maxScore, List<int> thresholds, List<int> actions, ref int sum)
        {
            if (pair == null || pair.Length < 2 || score < pair[0] * (maxScore / 100f)) return;
            sum += pair[1]; thresholds.Add(sum); actions.Add(action);
        }
        public void RunAI(int camp, AIConfig cfg)
        {
            if (State != BattlePhase.Running || cfg == null || cfg.ActionNum <= 0) return;
            if(BasicTowerExperiment&&!Towers.Exists(t=>t.IsBoss&&t.Camp==camp)){PlanEvolutionAI(camp);return;}
            var sources = Towers.FindAll(t => t.Active && t.Camp == camp && t.ShipID != 4);
            for (int i = 0; i < sources.Count; i++)
            {
                int j = AIShuffleIndex == null ? UnityEngine.Random.Range(0, sources.Count) : AIShuffleIndex(sources.Count);
                if (j < 0 || j >= sources.Count) throw new InvalidOperationException("AI shuffle index outside source list.");
                var temp = sources[i]; sources[i] = sources[j]; sources[j] = temp;
            }
            int budget = random.Next(1, cfg.ActionNum + 1);
            foreach (var source in sources)
            {
                if (budget <= 0) return;
                int action = GetAIAction(cfg, source); if (action == 0) continue;
                if (action == 1)
                {
                    RemoveOutgoing(source.Id); var incoming = Incoming(source.Id);
                    if (incoming.Count == 0) return;
                    foreach (var target in incoming)
                    {
                        if (!source.CanAddLine) break;
                        if (target.Camp != source.Camp) Connect(source.Id, target.Id, true);
                    }
                    budget--; continue;
                }
                if (!source.CanAddLine || adjacency[source.Id].Count == 0) continue;
                var existing = Outgoing(source.Id);
                if (action == 2)
                {
                    foreach (var target in Incoming(source.Id))
                    {
                        if (target.Camp == source.Camp || ExcludePlayer(cfg, target) || existing.Exists(l => l.Other(source.Id) == target.Id)) continue;
                        Connect(source.Id, target.Id, true); budget--; // Each attempt consumes budget, even rejection.
                    }
                    continue;
                }
                if (action != 3 && action != 4) continue;
                bool friendly = action == 4;
                if (TryAITarget(source, cfg, existing, friendly)) { budget--; return; }
                if ((cfg.AIType == 2 || cfg.AIType == 3) && TryAITarget(source, cfg, existing, !friendly)) budget--;
                // A fallback success continues with the next shuffled source, unlike primary success.
            }
        }
        private static bool ExcludePlayer(AIConfig cfg, TowerState t)
        {
            return (cfg.AIType == 0 || cfg.AIType == 2) && t.Camp == PlayerCampID;
        }
        private bool TryAITarget(TowerState source, AIConfig cfg, List<LineState> existing, bool friendly)
        {
            foreach (var l in adjacency[source.Id])
            {
                var target = Tower(l.Other(source.Id));
                if (target == null || existing.Contains(l) || (target.Camp == source.Camp) != friendly) continue;
                if (!friendly && ExcludePlayer(cfg, target)) continue;
                if (ActivateDirection(l, source, target)) return true;
            }
            return false;
        }
    }
}
