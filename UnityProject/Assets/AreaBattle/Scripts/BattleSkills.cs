// Static source: generated/combat-evidence.json and combat-skill-coverage.json, target 43.
// No invented TowerBuff source. Gameplay duration, coroutine waits and projectile updates
// are separate clocks; the host must supply Unity scaled delta to both async APIs.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public sealed class SkillParameters
    { public int id; public float duration, data1, data2, data3; }
    [Serializable] public sealed class BossParameters
    {
        public int id, DelayTime, skill1_Hit, skill2_Hit;
        public int[] ActionTime, Skill1, Skill2;
        public float skill1_duration, skill1_first, skill1_second, skill2_duration, skill2_first, skill2_second;
    }
    public partial class TowerState
    { public bool IsBoss; public int BossSkillId; }
    public sealed class SkillRuntime
    {
        public int Id, Camp, Level, TargetId;
        public SkillParameters Parameters;
        public bool Active;
        public float Elapsed, DamageTimer, KillTimer;
        public Vector3 Point, ProjectileOrigin;
        internal readonly List<int> VisualHandles = new List<int>();
        internal int AudioHandleId;
        // Skill12 stores two current effect handles, not a list of every past shot.
        internal int AimVisualHandle, ShotVisualHandle;
    }
    public sealed class SkillProjectile
    {
        public int VisualId { get; internal set; }
        public int SkillId, Camp, TargetTowerId, SourceTowerId;
        public Vector3 Start, End, ControlPoint;
        public float Duration, Elapsed, Arc;
        public bool Active = true;
        internal bool RepeatAtEnd;
        internal bool IsBulletCurve;
        internal bool VisualArrived;
        internal Action Callback;
        public Vector3 Position
        {
            get
            {
                float t = Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);
                if (!IsBulletCurve) return Vector3.Lerp(Start, End, t);
                float oneMinusT = 1f - t;
                return oneMinusT * oneMinusT * Start + 2f * oneMinusT * t * ControlPoint + t * t * End;
            }
        }
    }
    public sealed class RecruitedSoldier
    {
        public int VisualId { get; internal set; }
        public int Camp, TargetId; public Vector3 Position, Direction; public bool Active = true;
    }

    // Value snapshots only: subscribers cannot mutate a mechanism entity through this event.
    // EffectId resolves through the recovered EffectConfig; direct Path bypasses its lifetime.
    public sealed class SkillVisualEvent
    {
        public string Kind, Path, Parent, Orientation, AudioRoute;
        public int SkillId, Camp, EffectId, TowerId, SourceTowerId, SoldierId, VisualId, EntityId, SkinType, AudioId, VoiceMode, HandleId;
        public Vector3 Position, Start, End, ControlPoint, LocalScale, Euler;
        public float Duration, RotationSpeed, ParticleSpeed;
        public bool OverrideScale, OverrideEuler, LookAt, AutoDisable, RepeatAtEnd, PositionIsLocal;
    }

    public partial class BattleSimulation
    {
        public readonly List<SkillRuntime> SkillStates = new List<SkillRuntime>();
        public readonly List<SkillProjectile> SkillProjectiles = new List<SkillProjectile>();
        public readonly List<RecruitedSoldier> RecruitedSoldiers = new List<RecruitedSoldier>();
        public event Action<SkillVisualEvent> SkillVisual;
        private int nextSkillVisualId;
        private readonly List<SkillWait> skillVisualWaits = new List<SkillWait>();
        // Isolated presentation clock: no gameplay random calls, waits or score changes.
        private void EmitSkillVisual(SkillVisualEvent value)
        {
            var listeners = SkillVisual;
            if (listeners == null) return;
            foreach (Action<SkillVisualEvent> listener in listeners.GetInvocationList())
                try { listener(value); } catch (Exception exception) { System.Diagnostics.Trace.TraceError(exception.ToString()); }
        }
        private int ManagedSkillEffect(int skill, int camp, int effect, Vector3 position, int tower = 0, string parent = "world")
        {
            int handle = ++nextSkillVisualId;
            float duration = effect == 421 ? 1.6f : effect == 612 ? 10f : 5f;
            EmitSkillVisual(new SkillVisualEvent { Kind="effect-show", SkillId=skill, Camp=camp, EffectId=effect,
                VisualId=handle, TowerId=tower, Parent=parent, Position=position, PositionIsLocal=parent=="tower"||parent=="soldier", Duration=duration, OverrideEuler=true });
            return handle;
        }
        private void CloseSkillEffect(int skill, int camp, int handle)
        { EmitSkillVisual(new SkillVisualEvent { Kind="effect-close", SkillId=skill, Camp=camp, VisualId=handle }); }
        private void AttachedSkillEffect(int skill, SoldierState soldier, int effect, bool show = true)
        { EmitSkillVisual(new SkillVisualEvent { Kind=show?"soldier-effect-add":"soldier-effect-remove", SkillId=skill,
            Camp=soldier.Camp, SoldierId=soldier.Id, EffectId=effect, Parent="soldier", Position=Vector3.zero, PositionIsLocal=true, OverrideEuler=true }); }
        private void WaitSkillVisual(float delay, Action callback, bool survivesBattleRestart=false)
        { skillVisualWaits.Add(new SkillWait { Remaining=delay, Callback=callback, SurvivesBattleRestart=survivesBattleRestart }); }
        private void SkillAudio(int skill,int camp,int id,bool playerVoice=true,int handle=0)
        { EmitSkillVisual(new SkillVisualEvent { Kind="audio-play",SkillId=skill,Camp=camp,AudioId=id,VoiceMode=1,
            HandleId=handle,AudioRoute=playerVoice?"player-voice":"ui-audio" }); }
        private void StopSkillAudio(SkillRuntime run)
        {
            if(run.AudioHandleId==0)return;
            EmitSkillVisual(new SkillVisualEvent { Kind="audio-stop",SkillId=run.Id,Camp=run.Camp,AudioId=2041,
                VoiceMode=1,HandleId=run.AudioHandleId,AudioRoute="ui-audio" });
            run.AudioHandleId=0;
        }
        public bool BossAIEnabled = true;
        // The source uses UnityEngine.Random.value here, separate from RandomHelper.
        public Func<float> DrainArcRandom = () => UnityEngine.Random.value;
        public string LastSkillRejection { get; private set; }
        private readonly Dictionary<int, SkillParameters> skillParameters = new Dictionary<int, SkillParameters>();
        private readonly Dictionary<int, BossParameters> bossParameters = new Dictionary<int, BossParameters>();
        private readonly List<SkillWait> skillWaits = new List<SkillWait>();
        private readonly List<BossClock> bossClocks = new List<BossClock>();
        public bool TryGetBossRain(int towerId, out Vector3 center, out bool raining)
        {
            var clock = bossClocks.Find(x => x.Tower.Id == towerId);
            center = clock == null ? Vector3.zero : clock.RainCenter;
            raining = clock != null && clock.Raining;
            return clock != null;
        }
        private readonly Dictionary<int, float> rainDamageTimers = new Dictionary<int, float>();
        private Func<TowerState, float> upstreamSpawn;
        private Func<SoldierState, float> upstreamSpeed;
        private sealed class SkillWait
        {
            public float Remaining; public Action Callback;
            public bool SurvivesBattleRestart;
        }
        private sealed class BossClock
        {
            public TowerState Tower; public BossParameters Config;
            public float Delay, Countdown, RainElapsed, RainScan;
            public int ActionGeneration; public bool Raining; public Vector3 RainCenter;
        }

        public void ConfigureSkills(SkillParameters[] skillsConfig, BossParameters[] bossesConfig)
        {
            skillParameters.Clear(); bossParameters.Clear();
            foreach (var row in skillsConfig ?? Array.Empty<SkillParameters>()) skillParameters.Add(row.id, row);
            foreach (var row in bossesConfig ?? Array.Empty<BossParameters>()) bossParameters.Add(row.id, row);
            if (upstreamSpawn == null)
            {
                upstreamSpawn = SpawnMultiplier; upstreamSpeed = SpeedMultiplier;
                SpawnMultiplier = SkillSpawnMultiplier; SpeedMultiplier = SkillSpeedMultiplier;
            }
            InitializeBossClocks();
        }
        partial void OnBattleRestarted()
        {
            foreach(var run in SkillStates)
            {
                StopSkillAudio(run);
                if(run.Id==2){run.Active=false;run.Elapsed=0;}
                if(run.Id!=12)continue;
                // Type4191.Reset clears target/timer and closes fields52/56; the
                // AsyncVoid await retains this same object and is not cancelled.
                run.Active=false;run.Elapsed=0;run.TargetId=0;run.DamageTimer=run.KillTimer=0;
                if(run.AimVisualHandle!=0)CloseSkillEffect(12,run.Camp,run.AimVisualHandle);
                if(run.ShotVisualHandle!=0)CloseSkillEffect(12,run.Camp,run.ShotVisualHandle);
            }
            skillVisualWaits.RemoveAll(wait=>!wait.SurvivesBattleRestart);
            EmitSkillVisual(new SkillVisualEvent { Kind="battle-reset" });
            // SkillControl.f10350 reuses the player's Commander dictionary entry;
            // Commander.Reset iterates, but does not replace, its skill objects.
            // Enemy commander reuse on PvP initialization is a separate lifecycle.
            SkillStates.RemoveAll(run=>(run.Id!=12&&run.Id!=2)||run.Camp!=PlayerCampID);
            // Type4170.Reset stops the launch coroutine only. Already emitted
            // Bullet callbacks survive the ordinary Again10 reset path.
            SkillProjectiles.RemoveAll(projectile=>projectile.SkillId!=2||!projectile.Active);
            RecruitedSoldiers.Clear();
            // Skill15 serial waits run on persistent AsyncCoroutineRunner. They retain
            // their remaining delay and inspect the original Tower when resumed.
            skillWaits.RemoveAll(wait => !wait.SurvivesBattleRestart);
            rainDamageTimers.Clear(); LastSkillRejection = null;
            for (int i = 0; i < Towers.Count; i++)
            {
                var cfg = initialTowers[i]; var tower = Towers[i];
                tower.IsBoss = cfg.isBoss; tower.BossSkillId = cfg.bossSkillId;
                if (!tower.IsBoss) continue;
                tower.MaxScore = (int)cfg.StartScore; tower.AutoAddScore = false; tower.CollisionRadius = 0.15f;
                int bossShip = tower.Camp >= 5 && tower.Camp <= 9 ? tower.Camp + 6 : cfg.ShipID;
                if (!ships.ContainsKey(bossShip)) throw new ArgumentException("Missing Boss SoldierConfig " + bossShip);
                tower.ShipID = bossShip;
            }
            InitializeBossClocks();
        }
        private void InitializeBossClocks()
        {
            bossClocks.Clear();
            foreach (var tower in Towers)
            {
                BossParameters cfg;
                if (tower.IsBoss && bossParameters.TryGetValue(tower.BossSkillId, out cfg))
                    bossClocks.Add(new BossClock { Tower = tower, Config = cfg, Delay = cfg.DelayTime / 1000f });
            }
        }
        public bool IsSkillActive(int id, int camp = PlayerCampID)
        { var skill = FindSkill(id, camp); return skill != null && skill.Active; }
        public SkillRuntime FindSkill(int id, int camp = PlayerCampID)
        { return SkillStates.Find(s => s.Id == id && s.Camp == camp); }
        private bool RejectSkill(string reason) { LastSkillRejection = reason; return false; }

        // This API performs mechanics only. Item price/unlock/cast input belongs to the
        // recovered UI controller. Skill2 requires its actual projected launch origin.
        public bool CastSkill(int id, int level, int camp = PlayerCampID, int targetTowerId = 0,
            Vector3? groundPoint = null, Vector3? projectileOrigin = null, bool agentCast = false)
        {
            LastSkillRejection = null;
            if (State != BattlePhase.Running) return RejectSkill("Battle is not Running.");
            SkillParameters cfg;
            if (id < 1 || id > 18 || !skillParameters.TryGetValue(id * 100 + level, out cfg))
                return RejectSkill("Missing recovered SkillConfig.");
            var target = Tower(targetTowerId);
            bool needsTower = id == 3 || id == 6 || id == 9 || id == 12 || id == 15;
            if (needsTower && !(agentCast && id == 6) && (target == null || !target.Active)) return RejectSkill("An active selected tower is required.");
            if ((id == 6 || id == 9) && target != null && target.Camp != camp) return RejectSkill("This skill requires a friendly tower.");
            if ((id == 3 || id == 12) && target.Camp == camp) return RejectSkill("This skill requires a different-camp tower.");
            if (id == 2 && !projectileOrigin.HasValue) return RejectSkill("Skill2 requires the projected original launch position; no guessed origin is supplied.");
            if (id == 18 && (!groundPoint.HasValue || (!agentCast && (Mathf.Abs(groundPoint.Value.x) >= 2.5f || groundPoint.Value.z <= 0.5f || groundPoint.Value.z >= 5f))))
                return RejectSkill("Poison requires a ground point inside |x|<2.5 and 0.5<z<5.");
            var run = FindSkill(id, camp);
            if (run == null) { run = new SkillRuntime { Id = id, Camp = camp }; SkillStates.Add(run); }
            run.Level = level; run.Parameters = cfg; run.TargetId = targetTowerId;
            run.Point = groundPoint ?? (target == null ? Vector3.zero : target.Position);
            run.ProjectileOrigin = projectileOrigin ?? Vector3.zero;
            if (!run.Active) { run.Elapsed = 0f; run.Active = true; }
            Emit(new BattleEvent { Kind = "skill-start", Value = id, Camp = camp, TowerId = targetTowerId });
            switch (id)
            {
                case 1:
                    foreach (var t in Towers) if (EnemyTower(t, camp))
                    {
                        t.Mode = 1;
                        EmitSkillVisual(new SkillVisualEvent { Kind="tower-ice-begin", SkillId=1, Camp=camp,
                            TowerId=t.Id, Parent="tower", Position=Vector3.zero, PositionIsLocal=true });
                    }
                    SkillAudio(1,camp,2016);
                    foreach (var s in Soldiers.ToArray()) if (s.Active && s.Camp != camp) ClearSoldier(s);
                    break;
                case 2: StartFireballs(run); break;
                case 3:
                    SkillAudio(3,camp,4401);
                    ManagedSkillEffect(3,camp,413,target.Position,target.Id);
                    ChangeScore(target.Id, PlayerCampID, -(int)cfg.data1, true); break;
                // Fresh enemy skill6 never assigns target44 in the original Execute.
                case 4: SkillAudio(4,camp,2042,false);break;
                case 5: SkillAudio(5,camp,2043,false);break;
                case 6: if (target != null) { ChangeScore(target.Id, PlayerCampID, (int)cfg.data1, true);SkillAudio(6,camp,4501); } run.TargetId = 0; break;
                case 7: StartBats(run); SkillAudio(7,camp,2027);break;
                case 8:
                    foreach (var s in Soldiers) if (s.Active && s.Camp == camp) { s.HP = 10000; AttachedSkillEffect(8,s,418); }
                    foreach (var t in Towers) if (t.Active && t.Camp == PlayerCampID) t.Mode = 5;
                    SkillAudio(8,camp,2028);
                    break;
                case 9: StartDrain(run);SkillAudio(9,camp,2029,false); break;
                case 10: SkillAudio(10,camp,2030);StartArrowRain(run); break;
                case 11:
                    SkillAudio(11,camp,2031);
                    foreach (var s in Soldiers) if (s.Active && s.Camp == camp) AttachedSkillEffect(11,s,420);
                    foreach (var t in Towers) if (t.Active && t.Camp == PlayerCampID) t.Mode = 6;
                    break;
                case 12:
                    run.AimVisualHandle=ManagedSkillEffect(12,camp,417,target.Position+Vector3.up*0.7f,target.Id);
                    WaitSkillVisual(0.8f,()=>{
                        // MoveNext@512ee2 loads current skill.target44 after await.
                        // It does not capture the initial tower or test Active/camp.
                        var currentTarget=Tower(run.TargetId);
                        if(currentTarget==null)
                        {
                            // The source WASM dereferences null without a guard. Its
                            // original-runtime fault/low-memory result is unverified;
                            // expose that gap rather than invent an effect at zero.
                            EmitSkillVisual(new SkillVisualEvent {Kind="async-target-unresolved",SkillId=12,Camp=camp});
                            return;
                        }
                        run.ShotVisualHandle=ManagedSkillEffect(12,camp,419,currentTarget.Position,currentTarget.Id);
                        SkillAudio(12,camp,2032);SkillAudio(12,camp,2033);
                    },true);
                    break;
                case 13: SkillAudio(13,camp,2034);break;
                case 14: SkillAudio(14,camp,2035);StartRecruits(run); break;
                case 15: StartSerialScore(run, target); run.TargetId = 0; break;
                case 16: SkillAudio(16,camp,2038);break;
                case 17: StartRecovery(run);SkillAudio(17,camp,2039); break;
                case 18: StartPoisonVisual(run); break;
            }
            EmitSkillVisual(new SkillVisualEvent { Kind="skill-start", SkillId=id, Camp=camp, TowerId=targetTowerId });
            return true;
        }
        private static bool EnemyTower(TowerState t, int camp) { return t.Active && t.Camp != 0 && t.Camp != camp; }
        private TowerState RandomTower(Predicate<TowerState> predicate)
        {
            var candidates = Towers.FindAll(predicate);
            if (candidates.Count == 0) { Emit(new BattleEvent { Kind = "skill-empty-candidates" }); return null; }
            return candidates[random.Next(candidates.Count)];
        }
        private void WaitScaled(float seconds, Action callback, bool survivesBattleRestart = false)
        { skillWaits.Add(new SkillWait { Remaining = Mathf.Max(0f, seconds), Callback = callback,
            SurvivesBattleRestart = survivesBattleRestart }); }
        public void TickSkillCoroutines(float unityScaledDelta)
        {
            ValidateSkillDelta(unityScaledDelta);
            // Snapshot before gameplay callbacks: newly scheduled visuals are first advanced next call.
            var visuals=skillVisualWaits.ToArray();
            // Result does not stop Unity WaitForSeconds. Pause freezes scaled time.
            foreach (var wait in skillWaits.ToArray())
            {
                wait.Remaining -= unityScaledDelta;
                if (wait.Remaining > 0f) continue;
                skillWaits.Remove(wait); wait.Callback();
            }
            foreach(var wait in visuals)
            {
                wait.Remaining-=unityScaledDelta;
                if(wait.Remaining>0f)continue;
                skillVisualWaits.Remove(wait);wait.Callback();
            }
        }
        public void TickSkillProjectiles(float unityScaledDelta)
        {
            ValidateSkillDelta(unityScaledDelta);
            // Bullet.Update uses UnityEngine.Time::get_deltaTime(), not DOTween.
            foreach (var p in SkillProjectiles.ToArray())
            {
                if (!p.Active) continue;
                p.Elapsed += unityScaledDelta;
                if (p.IsBulletCurve ? (p.Position - p.End).sqrMagnitude >= 1e-10f : p.Elapsed < p.Duration) continue;
                bool firstArrival=!p.VisualArrived;p.VisualArrived=true;
                if (!p.RepeatAtEnd) p.Active = false;
                if(firstArrival) EmitSkillVisual(new SkillVisualEvent { Kind="projectile-arrival", SkillId=p.SkillId,
                    Camp=p.Camp, TowerId=p.TargetTowerId, VisualId=p.VisualId, Position=p.End, RepeatAtEnd=p.RepeatAtEnd });
                // Boss callback pools its bullet before audio/score/effect. Ordinary
                // skill projectile callbacks retain their existing close order.
                if(!p.Active&&p.SkillId==102)EmitSkillVisual(new SkillVisualEvent { Kind="projectile-hide",SkillId=102,Camp=p.Camp,VisualId=p.VisualId,SourceTowerId=p.SourceTowerId });
                p.Callback();
                if(!p.Active&&p.SkillId!=102) EmitSkillVisual(new SkillVisualEvent { Kind="projectile-hide", SkillId=p.SkillId, Camp=p.Camp, VisualId=p.VisualId });
            }
            SkillProjectiles.RemoveAll(p => !p.Active);
        }
        private static void ValidateSkillDelta(float dt)
        { if (dt < 0f || float.IsNaN(dt) || float.IsInfinity(dt)) throw new ArgumentOutOfRangeException("dt"); }
        private SkillProjectile Projectile(int skill, int camp, TowerState target, Vector3 start, Vector3 end, float duration, float arc, Action callback, bool repeat = false, int sourceTowerId=0)
        {
            var p = new SkillProjectile { SkillId = skill, Camp = camp, TargetTowerId = target == null ? 0 : target.Id,
                Start = start, End = end, Duration = duration, Arc = arc, Callback = callback, RepeatAtEnd = repeat,
                IsBulletCurve = skill != 10,SourceTowerId=sourceTowerId };
            p.ControlPoint = BulletControlPoint(start, end, arc, arc == 0f && p.IsBulletCurve ? (random.Next(1, 101) & 1) != 0 : true);
            p.VisualId=++nextSkillVisualId;
            SkillProjectiles.Add(p);
            EmitProjectileSpawn(p);
            return p;
        }
        private void EmitProjectileSpawn(SkillProjectile p)
        {
            if(p.SkillId==102)
            {
                EmitSkillVisual(new SkillVisualEvent { Kind="projectile-spawn",SkillId=102,Camp=p.Camp,VisualId=p.VisualId,
                    TowerId=p.TargetTowerId,SourceTowerId=p.SourceTowerId,Path="Boss/Hdzd_Effect_Wy_Fire",Parent="boss-bullet-root",
                    Position=p.Start,Start=p.Start,End=p.End,ControlPoint=p.ControlPoint,Duration=p.Duration,
                    RotationSpeed=50,LookAt=false,Euler=new Vector3(0,90,0) });
                SkillAudio(102,p.Camp,3121);return;
            }
            if(p.SkillId!=2&&p.SkillId!=7&&p.SkillId!=9&&p.SkillId!=10)return;
            string path=p.SkillId==2?"effect/scene/Hdzd_Effect_Wy_Fire":p.SkillId==7?"effect/scene/hdzd_eff_ZHG04_01":
                p.SkillId==9?"effect/scene/hdzd_eff_ZHG04_03":"effect/scene/hdzd_eff_ZHG03_01";
            EmitSkillVisual(new SkillVisualEvent { Kind="projectile-spawn", SkillId=p.SkillId, Camp=p.Camp, VisualId=p.VisualId,
                TowerId=p.TargetTowerId, Path=path, Parent=p.SkillId==7?"world":"skill", Position=p.Start, Start=p.Start, End=p.End, ControlPoint=p.ControlPoint,
                Duration=p.Duration, LocalScale=p.SkillId==10?Vector3.one*4:Vector3.one, OverrideScale=p.SkillId!=7,
                RotationSpeed=p.SkillId==2?180:0, LookAt=p.SkillId!=10&&p.Arc!=0, AutoDisable=p.SkillId==9, RepeatAtEnd=p.RepeatAtEnd });
            if(p.SkillId==2)SkillAudio(2,p.Camp,3121);
        }
        // Skill10's source DOMoveY uses default OutQuad; its mechanical arrival time remains unchanged.
        public Vector3 SkillProjectileVisualPosition(SkillProjectile p)
        {
            if(p.SkillId!=10)return p.Position;
            float t=p.Duration<=0?1:Mathf.Clamp01(p.Elapsed/p.Duration);
            return Vector3.Lerp(p.Start,p.End,1-(1-t)*(1-t));
        }
        public static Vector3 BulletControlPoint(Vector3 start, Vector3 end, float arc, bool positiveSide)
        {
            Vector3 midpoint = (start + end) * 0.5f;
            if (arc != 0f) return midpoint + Vector3.up * arc;
            Vector3 difference = start - end;
            Vector3 normal = Vector3.Cross(difference, Vector3.ProjectOnPlane(difference, Vector3.up)).normalized * 0.5f;
            return positiveSide ? midpoint + normal : midpoint - normal;
        }
        partial void BeforeBattleTick(float dt)
        { TickSkillTimers(dt, true); }
        private void TickEnemySkillTimers(float dt)
        { TickSkillTimers(dt, false); }
        private void TickSkillTimers(float dt, bool playerCommander)
        {
            foreach (var run in SkillStates.ToArray())
            {
                if (!run.Active || (run.Camp == PlayerCampID) != playerCommander) continue;
                run.Elapsed += dt;
                if (run.Id == 12)
                {
                    run.DamageTimer += dt;
                    if (run.DamageTimer >= 1f)
                    { run.DamageTimer -= 1f; ChangeScore(run.TargetId, run.Camp, -(int)run.Parameters.data1, true); }
                }
                else if (run.Id == 18) TickPoison(run, dt);
                if (run.Elapsed >= run.Parameters.duration) EndSkill(run);
            }
        }
        partial void AfterBattleTick(float dt)
        {
            float step = dt * Configs.GameTimeScale * Configs.ShipTimeScale * 0.5f;
            foreach (var s in RecruitedSoldiers)
            {
                var t = Tower(s.TargetId);
                if (!s.Active || t == null || !t.Active) continue;
                s.Position += s.Direction * step;
                if ((s.Position - t.Position).sqrMagnitude >= 0.01f) continue;
                EmitSkillVisual(new SkillVisualEvent { Kind="recruit-hide", SkillId=14, Camp=s.Camp, VisualId=s.VisualId, TowerId=t.Id, Position=s.Position });
                s.Active = false; ChangeScore(t.Id, s.Camp, t.Camp == s.Camp ? 1 : -1, false);
            }
            RecruitedSoldiers.RemoveAll(s => !s.Active);
        }
        partial void OnSoldierCreated(SoldierState soldier)
        {
            var source = Tower(soldier.OriginTowerId);
            if (source != null && source.Mode == 5) { soldier.HP = 100000; AttachedSkillEffect(8,soldier,418); }
            if (source != null && source.Mode == 6) AttachedSkillEffect(11,soldier,420);
            foreach (var run in SkillStates)
            {
                if (!run.Active) continue;
                if (run.Id == 13 && soldier.Camp == run.Camp)
                { soldier.Occupy *= (int)(1f + run.Parameters.data1 / 100f); soldier.Attack += 99; AttachedSkillEffect(13,soldier,511); }
                if (run.Id == 16 && soldier.Camp != run.Camp) { soldier.Attack = 0; soldier.Reinforce = 0; AttachedSkillEffect(16,soldier,611); }
            }
        }
        private float SkillSpawnMultiplier(TowerState tower)
        {
            float rate = upstreamSpawn(tower);
            if (tower.Mode == 3)
                foreach (var b in bossClocks) if (b.Tower.Camp == 6) { rate *= b.Config.skill1_first; break; }
            foreach (var run in SkillStates)
            {
                // WayLine f6775 consults SkillControl.currentCommander only.
                if (!run.Active || run.Camp != PlayerCampID) continue;
                if ((run.Id == 4 && run.Camp == tower.Camp) || (run.Id == 5 && run.Camp != tower.Camp)) rate *= run.Parameters.data1;
                if (run.Id == 11 && run.Camp == tower.Camp) rate *= run.Parameters.data2;
            }
            return rate;
        }
        private float SkillSpeedMultiplier(SoldierState soldier)
        {
            float rate = upstreamSpeed(soldier);
            foreach (var run in SkillStates) if (run.Active && run.Camp == PlayerCampID && run.Id == 11 && run.Camp == soldier.Camp) rate *= run.Parameters.data1;
            return rate;
        }
        private void EndSkill(SkillRuntime run)
        {
            run.Active = false; run.Elapsed = 0f;
            if (run.Id == 1)
            {
                int changed=0;
                foreach (var t in Towers) if (t.Active && t.Camp != run.Camp && t.Mode == 1)
                {
                    t.Mode = 0; changed++;
                    EmitSkillVisual(new SkillVisualEvent { Kind="tower-ice-melt", SkillId=1, Camp=run.Camp,
                        TowerId=t.Id, Parent="tower", Position=Vector3.zero, PositionIsLocal=true });
                }
                if(changed>0)SkillAudio(1,run.Camp,2018);
            }
            if (run.Id == 7)
            {
                foreach (var t in Towers) if (t.Active && t.Mode == 4) t.Mode = 0;
                foreach (var p in SkillProjectiles) if (p.SkillId == 7 && p.Camp == run.Camp)
                { p.Active = false; EmitSkillVisual(new SkillVisualEvent { Kind="projectile-hide", SkillId=7, Camp=run.Camp, VisualId=p.VisualId }); }
            }
            if (run.Id == 8)
            {
                foreach (var s in Soldiers) if (s.Active && s.Camp == run.Camp) { s.HP = ships[s.ShipID].hp; AttachedSkillEffect(8,s,418,false); }
                foreach (var t in Towers) if (t.Active && t.Camp == PlayerCampID) t.Mode = 0;
            }
            if (run.Id == 11) foreach (var t in Towers) { if (t.Active && t.Camp == PlayerCampID) t.Mode = 0; }
            if (run.Id == 9 || run.Id == 12) run.TargetId = 0;
            if (run.Id == 12 || run.Id == 18) { run.DamageTimer = 0f; run.KillTimer = 0f; }
            if (run.Id == 17) { foreach(int handle in run.VisualHandles)CloseSkillEffect(17,run.Camp,handle);run.VisualHandles.Clear(); }
            if (run.Id == 18)
            { EmitSkillVisual(new SkillVisualEvent { Kind="poison-ground-hide", SkillId=18, Camp=run.Camp, EffectId=631 });StopSkillAudio(run); }
            EmitSkillVisual(new SkillVisualEvent { Kind="skill-end", SkillId=run.Id, Camp=run.Camp });
            Emit(new BattleEvent { Kind = "skill-end", Value = run.Id, Camp = run.Camp });
        }
        private void StartFireballs(SkillRuntime run)
        {
            int emitted = 0; float count = run.Parameters.data1;
            Action launch = null;
            launch = () =>
            {
                if (emitted >= count) return;
                if (!run.Active) { WaitScaled(0f, launch); return; }
                var t = RandomTower(x => x.Active);
                if (t != null)
                {
                    Vector3 end = t.Position + Vector3.up * 0.2f;
                    float duration = Vector3.Distance(run.ProjectileOrigin, t.Position) / 10f + 0.3f;
                    Projectile(2, run.Camp, t, run.ProjectileOrigin, end, duration, 1.5f,
                        () => {
                            bool friendly=t.Camp==run.Camp;
                            ChangeScore(t.Id, t.Camp, friendly ? (int)run.Parameters.data3 : -(int)run.Parameters.data2, true);
                            ManagedSkillEffect(2,run.Camp,friendly?404:408,t.Position+(friendly?Vector3.zero:Vector3.up*0.2f),t.Id);
                            SkillAudio(2,run.Camp,friendly?2021:3122);
                        });
                }
                emitted++; if (emitted < count) WaitScaled(run.Parameters.duration / count, launch);
            };
            launch();
        }
        private void StartBats(SkillRuntime run)
        {
            foreach (var tower in Towers)
            {
                if (!EnemyTower(tower, run.Camp)) continue;
                var t = tower; SkillProjectile bat = null;
                bat = Projectile(7, run.Camp, t, run.ProjectileOrigin, t.Position, 1.5f, 0f, () =>
                {
                    if (State != BattlePhase.Running) return;
                    if (t.Mode == 4)
                    { if (t.Camp == run.Camp) { t.Mode = 0; bat.Active = false; } return; }
                    t.Mode = 4; RemoveOutgoing(t.Id);
                }, true);
            }
        }
        private void StartDrain(SkillRuntime run)
        {
            var destination = Tower(run.TargetId);
            run.VisualHandles.Add(ManagedSkillEffect(9,run.Camp,421,Vector3.zero,destination.Id,"tower"));
            foreach (var t in Towers)
            {
                if (!EnemyTower(t, run.Camp)) continue;
                ChangeScore(t.Id, t.Camp, -(int)run.Parameters.data1, true);
                Projectile(9, run.Camp, destination, t.Position, destination.Position, 0.7f, DrainArcRandom(), () =>
                {
                    var currentTarget = Tower(run.TargetId);
                    if (State == BattlePhase.Running && currentTarget != null && currentTarget.Camp == run.Camp)
                        ChangeScore(currentTarget.Id, currentTarget.Camp, (int)run.Parameters.data1, false);
                });
            }
        }
        private void StartArrowRain(SkillRuntime run)
        {
            int damagePass = 0; Action damage = null;
            damage = () =>
            {
                if (damagePass > run.Parameters.duration) return;
                if (!run.Active) { WaitScaled(0f, damage); return; }
                foreach (var t in Towers) if (t.Active && t.Camp != run.Camp) ChangeScore(t.Id, run.Camp, -(int)run.Parameters.data2, true);
                damagePass++; WaitScaled(1f, damage);
            };
            WaitScaled(0.3f, damage);
            int second = 0, pair = 0; Action arrows = null;
            arrows = () =>
            {
                if (second > run.Parameters.duration) return;
                if (!run.Active) { WaitScaled(0f, arrows); return; }
                for (int n = 0; n < 2; n++)
                {
                    var end = new Vector3(random.Next(-20, 21) / 20f, 0f, random.Next(-40, 41) / 15f + 3f);
                    Projectile(10, run.Camp, null, end + Vector3.up * 10f, end, 1f, 0f, () =>
                    {
                        var hit = Soldiers.Find(s => s.Active && s.Camp != run.Camp && (s.Position - end).sqrMagnitude <= 0.15f * 0.15f);
                        if (hit != null) ClearSoldier(hit, true);
                    });
                }
                pair++; if (pair >= run.Parameters.data1 * 0.5f) { pair = 0; second++; }
                WaitScaled(2f / run.Parameters.data1, arrows);
            };
            arrows();
        }
        private void StartRecruits(SkillRuntime run)
        {
            int remaining = (int)run.Parameters.data1;
            float gap = run.Parameters.duration / remaining; Action recruit = null;
            recruit = () =>
            {
                if (remaining-- <= 0) return;
                var enemies = Towers.FindAll(t => EnemyTower(t, run.Camp));
                var pool = enemies.Count > 0 ? enemies : Towers.FindAll(t => t.Active && t.Camp == 0);
                if (pool.Count > 0)
                {
                    var t = pool[random.Next(pool.Count)];
                    float angle = random.Next(0, 101) * 0.01f * Mathf.PI * 2f;
                    float radius = random.Next(0, 101) * 0.01f * (0.5f - 0.3f) + 0.3f;
                    var pos = t.Position + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    var summoned=new RecruitedSoldier { VisualId=++nextSkillVisualId, Camp = run.Camp, TargetId = t.Id, Position = pos, Direction = (t.Position - pos).normalized };
                    RecruitedSoldiers.Add(summoned);
                    EmitSkillVisual(new SkillVisualEvent { Kind="recruit-spawn", SkillId=14, Camp=run.Camp, VisualId=summoned.VisualId,
                        TowerId=t.Id, EntityId=1000, SkinType=1, Parent="world", Position=pos, End=t.Position, LocalScale=Vector3.one*0.05f, OverrideScale=true, Orientation="camera-tilt-flip" });
                    ManagedSkillEffect(14,run.Camp,521,pos,t.Id);
                }
                if (remaining > 0) WaitScaled(gap, recruit);
            };
            recruit();
        }
        private void StartSerialScore(SkillRuntime run, TowerState target)
        {
            SkillAudio(15,run.Camp,target.Camp==run.Camp?2036:2037);
            ManagedSkillEffect(15,run.Camp,target.Camp==run.Camp?532:531,target.Position,target.Id);
            int count = Math.Abs((int)run.Parameters.data1), originalCamp = target.Camp, sourceCamp = run.Camp;
            float sign = originalCamp == sourceCamp ? 1f : -1f;
            float gap = run.Parameters.duration / run.Parameters.data1;
            Action apply = null;
            apply = () =>
            {
                if (count <= 0 || !target.Active || target.Camp != originalCamp) return;
                count--;
                // The original async state machine holds a Tower reference, not an ID.
                // A replacement object with the same ID must not receive old ticks.
                ChangeScoreReference(target, sourceCamp, sign, true);
                if (count > 0) WaitScaled(gap, apply, true);
            };
            apply();
        }
        private void StartRecovery(SkillRuntime run)
        {
            var snapshot = Towers.FindAll(t => t.Active && t.Camp == run.Camp);
            var handles=new Dictionary<TowerState,int>();
            foreach(var tower in snapshot)
            { int handle=ManagedSkillEffect(17,run.Camp,612,tower.Position,tower.Id);handles[tower]=handle;run.VisualHandles.Add(handle); }
            float duration = run.Parameters.duration, amount = run.Parameters.data1;
            int pass = 0; Action recover = null;
            recover = () =>
            {
                if (pass >= duration) return;
                foreach (var t in snapshot) if (t.Camp == run.Camp) ChangeScore(t.Id, t.Camp, amount, false);
                foreach(var t in snapshot) if(t.Camp!=run.Camp && handles.ContainsKey(t))
                { int handle=handles[t];CloseSkillEffect(17,run.Camp,handle);run.VisualHandles.Remove(handle);handles.Remove(t); }
                pass++; if (pass < duration) WaitScaled(1f, recover);
            };
            recover();
        }
        private void StartPoisonVisual(SkillRuntime run)
        {
            int handle=++nextSkillVisualId;Vector3 origin=run.ProjectileOrigin,center=run.Point;
            EmitSkillVisual(new SkillVisualEvent { Kind="poison-ground-prepare", SkillId=18, Camp=run.Camp, EffectId=631,
                Parent="world", Position=center, LocalScale=Vector3.one, OverrideScale=true, ParticleSpeed=7f/(run.Parameters.duration-1f) });
            EmitSkillVisual(new SkillVisualEvent { Kind="projectile-spawn", SkillId=18, Camp=run.Camp, VisualId=handle,
                Path="effect/scene/hdzd_eff_wyys04", Parent="skill", Position=origin, Start=origin, End=center,
                ControlPoint=BulletControlPoint(origin,center,1.5f,true),Duration=1f,LocalScale=Vector3.one*150f,
                OverrideScale=true,RotationSpeed=180,LookAt=true });
            SkillAudio(18,run.Camp,3121);
            WaitSkillVisual(1f,()=>{
                EmitSkillVisual(new SkillVisualEvent { Kind="projectile-arrival", SkillId=18,Camp=run.Camp,VisualId=handle,Position=center });
                SkillAudio(18,run.Camp,2040);
                run.AudioHandleId=++nextSkillVisualId;SkillAudio(18,run.Camp,2041,false,run.AudioHandleId);
                EmitSkillVisual(new SkillVisualEvent { Kind="poison-ground-show", SkillId=18,Camp=run.Camp,EffectId=631,Position=center });
                EmitSkillVisual(new SkillVisualEvent { Kind="projectile-hide", SkillId=18,Camp=run.Camp,VisualId=handle });
            });
        }
        private void TickPoison(SkillRuntime run, float dt)
        {
            run.DamageTimer += dt; run.KillTimer += dt;
            if (run.DamageTimer >= 1f)
            {
                run.DamageTimer -= 1f;
                foreach (var t in Towers) if (t.Active && t.Camp != run.Camp && (t.Position - run.Point).sqrMagnitude <= 0.49f)
                    ChangeScore(t.Id, run.Camp, -(int)run.Parameters.data1, true);
            }
            if (run.Elapsed > 1f && run.KillTimer > 0.2f)
            {
                run.KillTimer = 0f;
                foreach (var s in Soldiers.ToArray()) if (s.Active && s.Camp != run.Camp && (s.Position - run.Point).sqrMagnitude <= 0.49f) ClearSoldier(s, true);
            }
        }
        private void TickBosses(float dt)
        {
            foreach (var b in bossClocks)
            {
                if (!b.Tower.Active) continue;
                if (BossAIEnabled)
                {
                    if (b.Delay > 0f) b.Delay -= dt;
                    else
                    {
                        b.Countdown -= dt;
                        if (b.Countdown < 0f)
                        {
                            b.Countdown += b.Config.ActionTime == null || b.Config.ActionTime.Length == 0 ? float.MaxValue : b.Config.ActionTime[random.Next(b.Config.ActionTime.Length)] / 1000f;
                            int slot = GetBossActionSlot(b.Config, (int)b.Tower.Score, b.Tower.MaxScore);
                            int action = slot == 0 ? 0 : b.Tower.Camp == 5 ? slot : b.Tower.Camp == 7 ? slot + 2 : b.Tower.Camp == 6 ? slot + 4 : b.Tower.Camp == 8 ? slot + 6 : slot + 8;
                            ExecuteBossAction(b.Tower.Id, action);
                        }
                    }
                }
            }
        }
        private void TickBossTower(TowerState tower, float dt)
        {
            var b = bossClocks.Find(x => x.Tower == tower);
            if (b != null && b.Raining)
                {
                    b.RainScan += dt;
                    if (b.RainScan > 0.5f)
                    {
                        b.RainScan -= 0.5f;
                        foreach (var t in Towers) if (t.Active && (t.Camp == PlayerCampID || t.Camp == 0) && Vector3.Distance(t.Position, b.RainCenter) < b.Config.skill1_second) t.Mode = 3;
                    }
                    b.RainElapsed += dt;
                    if (b.RainElapsed > b.Config.skill1_duration)
                    {
                        b.Raining = false; b.RainElapsed = b.RainScan = 0f;
                        foreach (var t in Towers) if (t.Camp == PlayerCampID || t.Camp == 0) t.Mode = 0;
                    }
            }
        }
        private float SkillTowerRainDelta(TowerState t, float dt)
        {
                if (!t.Active || t.Mode != 3) return 0f;
                float delta = 0f, timer; rainDamageTimers.TryGetValue(t.Id, out timer); timer += dt;
                if (timer >= 1f)
                {
                    timer -= 1f;
                    foreach (var b in bossClocks) if (b.Tower.Camp == 6) { delta = -b.Config.skill1_Hit; break; }
                }
                rainDamageTimers[t.Id] = timer;
                return delta;
        }
        public int GetBossActionSlot(BossParameters cfg, int score, float maxScore)
        {
            var thresholds = new List<int>(); var actions = new List<int>(); int total = 0;
            AddWeight(cfg.Skill1, 1, score, maxScore, thresholds, actions, ref total);
            AddWeight(cfg.Skill2, 2, score, maxScore, thresholds, actions, ref total);
            int sample = random.Next(0, total + 1);
            for (int i = 0; i < thresholds.Count; i++) if (thresholds[i] > sample) return actions[i];
            return 0;
        }
        public bool ExecuteBossAction(int towerId, int action)
        {
            var b = bossClocks.Find(x => x.Tower.Id == towerId);
            if (b == null || !b.Tower.Active) return false;
            int generation = ++b.ActionGeneration;
            Emit(new BattleEvent { Kind = "boss-action", TowerId = towerId, Value = action, Camp = b.Tower.Camp });
            if (action == 1 || action == 6)
            {
                WaitScaled(1f, () =>
                {
                    if (generation != b.ActionGeneration) return;
                    var t = RandomTower(x => x.Active && x.Camp == PlayerCampID);
                    if (t != null) ChangeScore(t.Id, t.Camp, -(action == 1 ? b.Config.skill1_Hit : b.Config.skill2_Hit), true);
                });
            }
            else if (action == 5)
            {
                var t = RandomTower(x => x.Active && x.Camp == PlayerCampID);
                if (t != null) { b.RainCenter = t.Position; b.Raining = true; b.RainElapsed = b.RainScan = 0f; }
            }
            else if (action == 2)
            {
                int i = 0; Action fire = null;
                fire = () =>
                {
                    if (generation != b.ActionGeneration || i >= b.Config.skill2_first) return;
                    var t = RandomTower(x => x.Active && !x.IsBoss);
                    if (t != null) Projectile(102, b.Tower.Camp, t, b.Tower.Position, t.Position,
                        Vector3.Distance(b.Tower.Position, t.Position) / 2f, 0.5f,
                        () => {
                            SkillAudio(102,b.Tower.Camp,3122);
                            bool friendly=t.Camp==5;
                            ChangeScore(t.Id,t.Camp,friendly?b.Config.skill2_Hit:-b.Config.skill2_Hit,true);
                            ManagedSkillEffect(102,b.Tower.Camp,friendly?404:408,t.Position+(friendly?Vector3.zero:Vector3.up*.2f),t.Id);
                            SkillAudio(102,b.Tower.Camp,friendly?2021:3122);
                        },sourceTowerId:b.Tower.Id);
                    i++; if (i < b.Config.skill2_first) WaitScaled(b.Config.skill2_duration / b.Config.skill2_first, fire);
                };
                WaitScaled(1f, fire);
            }
            return true; // 0/3/4/7..10 have only the confirmed coroutine-stop side effect.
        }
    }
}
