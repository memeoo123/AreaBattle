// All nine original main-table tutorial entries. Evidence: generated/guide-evidence.json.
// This layer changes presentation visibility separately from simulation TowerState.Active.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    public sealed class GuidePrompt
    {
        public readonly string Title, Description, Button, Picture;
        public GuidePrompt(string title, string description, string picture)
        { Title = title; Description = description; Button = "知道了"; Picture = picture; }
    }

    public enum GuideHandMode { None, Connect, Cut, SkillPulse, SkillDrag }

    public sealed class BattleGuide : IDisposable
    {
        public const float TransitionDelay = 0.699999988079071f;
        private readonly BattleSimulation simulation;
        private readonly int levelId, smallLevelIndex, commanderMode;
        private readonly HashSet<int> visible = new HashSet<int>();
        private readonly List<int> affected = new List<int>();
        private readonly List<float> delays = new List<float>();
        private List<LineState> borrowedAdjacency;
        private bool drawEnabled = true, disposed, polling;
        private float promptTimer, pollTimer, skillHandRemaining;

        public int Stage { get; private set; }
        public GuidePrompt Prompt { get; private set; }
        public bool PromptVisible { get; private set; }
        public float ConfirmSecondsRemaining { get { return Mathf.Max(0, promptTimer); } }
        public bool CanConfirm { get { return PromptVisible && promptTimer <= 0; } }
        public bool CanDraw { get { return !disposed && drawEnabled && !PromptVisible && simulation.State == BattlePhase.Running; } }
        public int HighlightSourceId { get; private set; }
        public int HighlightTargetId { get; private set; }
        public GuideHandMode HandMode { get; private set; }
        public string PersistentTip { get; private set; }
        public int HighlightSkillIndex { get; private set; } = -1;
        public IReadOnlyCollection<int> VisibleTowerIds { get { return visible; } }
        public bool HasPendingTransition { get { return delays.Count > 0; } }
        public event Action Changed;
        public event Action<int> Confirmed;

        public BattleGuide(BattleSimulation simulation, int levelId, int smallLevelIndex = 0, int commanderMode = 1)
        {
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            if (commanderMode < 1 || commanderMode > 6) throw new ArgumentOutOfRangeException(nameof(commanderMode));
            this.commanderMode = commanderMode;
            this.simulation = simulation; this.levelId = levelId; this.smallLevelIndex = smallLevelIndex;
            simulation.Event += OnBattleEvent;
            Begin();
        }

        private void Notify() { var callback = Changed; if (callback != null) callback(); }
        private void Begin()
        {
            // Restart replaces the simulation's adjacency dictionary; release old reference only.
            borrowedAdjacency = null; affected.Clear(); delays.Clear(); visible.Clear();
            foreach (var t in simulation.Towers) visible.Add(t.Id);
            drawEnabled = true; polling = false; pollTimer = 0; promptTimer = 0; skillHandRemaining = 0;
            HighlightSourceId = HighlightTargetId = 0; HandMode = GuideHandMode.None;
            PersistentTip = null; Prompt = null; PromptVisible = false; HighlightSkillIndex = -1;
            Stage = smallLevelIndex == 0 ? InitialStage(levelId) : 0;
            if (Stage != 0) ShowPrompt(); else Notify();
        }

        public static int InitialStage(int level)
        {
            switch (level) { case 0: return 1; case 1: return 4; case 2: return 5;
                case 6: return 9; case 7: return 7; case 9: return 8;
                case 14: return 10; case 21: return 11; case 25: return 12; default: return 0; }
        }
        private static readonly Dictionary<string, string> OriginalText = new Dictionary<string, string>
        {
            { "Commander.SkillDescribe.1", "使用冰冻使敌方塔无法派出小兵或者升级。" },
            { "Commander.SkillDescribe.10", "使用箭雨，可以对敌方小兵和塔造成伤害。" },
            { "Commander.SkillDescribe.11", "使用迅捷，可以增加所有己方小兵移动速度。" },
            { "Commander.SkillDescribe.12", "使用瞄准，可以指定一个敌方塔，对该塔造成持续伤害。" },
            { "Commander.SkillDescribe.13", "使用鼓舞，可以增加所有己方小兵的攻击力。" },
            { "Commander.SkillDescribe.14", "使用募兵，可以招募一定数量的小兵对敌方塔造成伤害。" },
            { "Commander.SkillDescribe.15", "使用降临，可以指定一个塔，给己方塔血量加成或者给敌方塔造成伤害。" },
            { "Commander.SkillDescribe.16", "使用虚弱，可以使敌方小兵攻击力和对塔的补血量变为0。" },
            { "Commander.SkillDescribe.17", "使用恢复，可以使所有己方塔持续回血。" },
            { "Commander.SkillDescribe.18", "使用毒药，可以对范围内的敌方塔和小兵造成持续伤害。" },
            { "Commander.SkillDescribe.2", "使用火焰，可以对敌方塔随机攻击，或支援己方塔。" },
            { "Commander.SkillDescribe.3", "使用闪电，可以指定一个敌方塔进行攻击，对该塔造成大量伤害。" },
            { "Commander.SkillDescribe.4", "使用加速，可以使所有己方塔的小兵派遣速度增加。" },
            { "Commander.SkillDescribe.5", "使用减速，可以使所有敌方塔的小兵派遣速度降低。" },
            { "Commander.SkillDescribe.6", "使用发展，可以指定一个己方塔，快速增加该塔的数值。" },
            { "Commander.SkillDescribe.7", "使用夜魔侵袭，可以召唤夜魔蝙蝠切断敌方所有塔的出兵线路，并使敌方塔无法出兵。" },
            { "Commander.SkillDescribe.8", "使用血族契约，可以使己方小兵处于无敌状态，不会死亡。" },
            { "Commander.SkillDescribe.9", "使用吸血，可以指定一个己方塔，从所有敌方塔上吸血到自身。" },
            { "Commander.SkillName.1", "冰冻" },
            { "Commander.SkillName.10", "箭雨" },
            { "Commander.SkillName.11", "迅捷" },
            { "Commander.SkillName.12", "瞄准" },
            { "Commander.SkillName.13", "鼓舞" },
            { "Commander.SkillName.14", "募兵" },
            { "Commander.SkillName.15", "降临" },
            { "Commander.SkillName.16", "虚弱" },
            { "Commander.SkillName.17", "恢复" },
            { "Commander.SkillName.18", "毒药" },
            { "Commander.SkillName.1B", "缠绕" },
            { "Commander.SkillName.2", "火焰" },
            { "Commander.SkillName.3", "闪电" },
            { "Commander.SkillName.4", "加速" },
            { "Commander.SkillName.5", "减速" },
            { "Commander.SkillName.6", "发展" },
            { "Commander.SkillName.7", "夜魔侵袭" },
            { "Commander.SkillName.8", "血族契约" },
            { "Commander.SkillName.9", "吸血" },
            { "GuideUI.tipAdd.arrow", "<color=#FF2A01>攻防一体</color>，无死角攻击\n优先占领，敌人无处遁形" },
            { "GuideUI.tipAdd.attack", "双倍<color=#FF2A00>进攻</color>\n优先占领抢夺进攻先机" },
            { "GuideUI.tipAdd.defanse", "双倍<color=#FF2A00>增援</color>\n优先占领获得更快增援优势" },
            { "GuideUI.tipAdd.fire", "火焰覆盖，<color=#FF2A01>剿灭敌人，治愈友军</color>" },
            { "GuideUI.tipAdd.ice", "冰冻敌人，使其<color=#FF2A00>无法进攻</color>" },
            { "GuideUI.tipAdd.lighting", "召唤闪电，击溃敌人" },
            { "GuideUI.tipAdd.max", "全军出击" },
            { "guide/11", "操作" },
            { "guide/113", "弓箭塔" },
            { "guide/114", "弓箭塔会自动攻击处于攻击范围内的敌方塔或敌方小兵，不可连线也不会派出小兵。" },
            { "guide/41", "盾兵城堡" },
            { "guide/42", "盾兵城堡支援友方城堡时，效果是普通城堡的两倍。" },
            { "guide/51", "骑兵城堡" },
            { "guide/52", "骑兵城堡进攻敌方城堡时，效果是普通城堡的两倍。" },
        };
        private static string Text(string key) { return OriginalText[key]; }

        private void ShowPrompt()
        {
            HandMode = GuideHandMode.None; PersistentTip = null;
            switch (Stage)
            {
                case 1: Prompt = new GuidePrompt("操作", "通过拖动操作，在你的城堡和其他城堡之间建立连接；通过滑动来断开你所建立的连接。", "guideUI_icon1"); break;
                case 2: Prompt = null; PromptVisible = false; ConfirmInternal(); return;
                case 3: Prompt = new GuidePrompt("胜利目标", "占领所有城堡获得胜利。", "guideUI_icon3"); break;
                case 4: Prompt = new GuidePrompt("阵营", "每种颜色代表不同的城堡阵营，消灭其他阵营获得胜利。", "guideUI_icon6"); break;
                case 5: Prompt = new GuidePrompt("城堡进化", "城堡积攒足够分数时会进化到下一阶段，能同时发出更多条兵线且速度更快。", "guideUI_icon7"); break;
                case 6: Prompt = new GuidePrompt("城堡进化", "当城堡到达最高分时，<color=#000000>会将所有接收的士兵派发出去。</color>", "guideUI_icon8"); break;
                case 7: Prompt = new GuidePrompt(Text("guide/41"), Text("guide/42"), "guideUI_icon4"); break;
                case 8: Prompt = new GuidePrompt(Text("guide/51"), Text("guide/52"), "guideUI_icon5"); break;
                case 9: case 10: case 11:
                    int skill = Stage - 9 + commanderMode * 3 - 2;
                    Prompt = new GuidePrompt(Text("Commander.SkillName." + skill), Text("Commander.SkillDescribe." + skill), "guideUI_icon" + Stage + "_" + commanderMode); break;
                case 12: Prompt = new GuidePrompt(Text("guide/113"), Text("guide/114"), "guideUI_icon25"); break;
                default: CloseGuide(); return;
            }
            promptTimer = 1f; PromptVisible = true;
            simulation.Pause(true); Notify();
        }

        public bool Confirm()
        {
            if (disposed || !CanConfirm) return false;
            int confirmedStage=Stage;ConfirmInternal();Confirmed?.Invoke(confirmedStage);return true;
        }

        private void ConfirmInternal()
        {
            PromptVisible = false; promptTimer = 0; simulation.Pause(false);
            switch (Stage)
            {
                case 1: SelectConnectPair(); HandMode = GuideHandMode.Connect; break;
                case 2: HandMode = GuideHandMode.Cut; break;
                case 3: case 4: case 7: case 8: CloseGuide(); return;
                case 5:
                    foreach (var t in simulation.Towers) if (t.Active) t.AutoAddScore = false;
                    SelectUpgradePair(); HandMode = GuideHandMode.Connect;
                    polling = true; pollTimer = 0; break;
                case 6: PersistentTip = "全军出击"; break;
                case 9: case 10: case 11:
                    HighlightSkillIndex = Stage - 9;
                    PersistentTip = Text(Stage == 9 ? "GuideUI.tipAdd.ice" : Stage == 10 ? "GuideUI.tipAdd.fire" : "GuideUI.tipAdd.lighting");
                    HandMode = Stage == 11 ? GuideHandMode.SkillDrag : GuideHandMode.SkillPulse;
                    skillHandRemaining = Stage == 11 ? 30f : 15f;
                    if (Stage == 11)
                    {
                        foreach (var tower in simulation.Towers)
                            if (tower.Active && ((commanderMode == 1 && tower.Camp != BattleSimulation.PlayerCampID) || (commanderMode == 2 && tower.Camp == BattleSimulation.PlayerCampID)))
                            { HighlightTargetId = tower.Id; break; }
                        if (HighlightTargetId == 0) HandMode = GuideHandMode.None;
                    }
                    break;
                case 12: PersistentTip = Text("GuideUI.tipAdd.arrow"); break;
            }
            Notify();
        }

        private TowerState FirstPlayerTower()
        {
            foreach (var t in simulation.Towers) if (t.Active && t.Camp == BattleSimulation.PlayerCampID) return t;
            throw new InvalidOperationException("Recovered guide requires an active player tower.");
        }
        private void CollectNeighbors(TowerState tower)
        {
            borrowedAdjacency = simulation.GetPotentialLines(tower.Id);
            if (borrowedAdjacency == null) throw new InvalidOperationException("Guide adjacency is unavailable.");
            foreach (var line in borrowedAdjacency) affected.Add(line.Other(tower.Id));
        }
        private void SelectConnectPair()
        {
            var source = FirstPlayerTower(); HighlightSourceId = source.Id;
            CollectNeighbors(source);
            float maxZ = -100f; TowerState target = null;
            foreach (int id in affected)
            {
                var t = simulation.Tower(id);
                if (t.Position.z > maxZ) { maxZ = t.Position.z; target = t; }
            }
            if (target == null) throw new InvalidOperationException("Recovered stage1 has no eligible target above world Z=-100.");
            HighlightTargetId = target.Id;
            foreach (int id in affected) SetVisible(id, id == target.Id);
        }
        private void SelectUpgradePair()
        {
            var source = FirstPlayerTower();
            foreach (var t in simulation.Towers)
                if (t.Active && t.Camp == BattleSimulation.PlayerCampID && t != source && (int)t.Score >= (int)source.Score) source = t;
            HighlightSourceId = source.Id; CollectNeighbors(source);
            if (affected.Count == 0) throw new InvalidOperationException("Recovered stage5 source has no potential neighbor.");
            var target = simulation.Tower(affected[0]); HighlightTargetId = target.Id;
            CollectNeighbors(target);
            foreach (int id in affected) SetVisible(id, id == source.Id || id == target.Id);
        }
        private void SetVisible(int id, bool value) { if (value) visible.Add(id); else visible.Remove(id); }
        public bool IsTowerVisible(int id) { return visible.Contains(id); }
        public bool IsSourceAllowed(int towerId)
        {
            if (!CanDraw || !IsTowerVisible(towerId)) return false;
            var t = simulation.Tower(towerId);
            return t != null && t.Active && t.Camp == BattleSimulation.PlayerCampID && t.CanAddLine &&
                !(levelId == 2 && Stage == 5 && (int)t.Score < t.MaxScore);
        }
        public bool TryConnect(int sourceId, int targetId)
        {
            return IsSourceAllowed(sourceId) && IsTowerVisible(targetId) && simulation.Connect(sourceId, targetId);
        }
        public bool TryCut(int lineId) { return CanDraw && simulation.CutPlayerLine(lineId); }

        private void OnBattleEvent(BattleEvent e)
        {
            if (disposed) return;
            if (e.Kind == "skill-start" && e.Camp == BattleSimulation.PlayerCampID && Stage >= 9 && Stage <= 11 &&
                ((int)e.Value - 1) % 3 == Stage - 9)
            { CloseGuide(); return; }
            if (e.Kind == "restart") { Begin(); return; }
            if (e.Kind == "connect" && e.Camp == BattleSimulation.PlayerCampID && (Stage == 1 || Stage == 5))
            { drawEnabled = false; HandMode = GuideHandMode.None; Notify(); }
            else if (e.Kind == "capture" && e.Camp == BattleSimulation.PlayerCampID && Stage == 1)
                delays.Add(TransitionDelay);
            else if (e.Kind == "cut" && Stage == 2)
            {
                drawEnabled = false; HandMode = GuideHandMode.None; RestoreAffected();
                delays.Add(TransitionDelay); Notify();
            }
        }
        private void RestoreAffected() { foreach (int id in affected) visible.Add(id); }

        // Call before Simulation.Tick. WaitForSeconds uses scaled time; popup/poll timers use unscaled time.
        // This method does not advance combat. Unity pause state and Unity timeScale are separate inputs.
        public void Tick(float scaledDelta, float unscaledDelta)
        {
            if (scaledDelta < 0 || unscaledDelta < 0 || float.IsNaN(scaledDelta) || float.IsNaN(unscaledDelta) ||
                float.IsInfinity(scaledDelta) || float.IsInfinity(unscaledDelta)) throw new ArgumentOutOfRangeException("delta");
            if (disposed) return;
            if (skillHandRemaining > 0)
            {
                skillHandRemaining = Mathf.Max(0, skillHandRemaining - scaledDelta);
                if (skillHandRemaining == 0) { HandMode = GuideHandMode.None; Notify(); }
            }
            if (PromptVisible && promptTimer > 0) { promptTimer = Mathf.Max(0, promptTimer - unscaledDelta); Notify(); }
            // Take the old delay count: a timer scheduled below starts on a later Tick.
            for (int i = delays.Count - 1; i >= 0; i--)
            {
                delays[i] -= scaledDelta;
                if (delays[i] > 0) continue;
                delays.RemoveAt(i); drawEnabled = true;
                Stage = Stage == 1 ? 2 : Stage == 2 ? 3 : Stage == 5 ? 6 : 0;
                ShowPrompt();
            }
            if (polling && unscaledDelta <= 1f)
            {
                pollTimer += unscaledDelta;
                if (pollTimer >= 1f)
                {
                    pollTimer = 0;
                    var target = simulation.Tower(HighlightTargetId);
                    if (target != null && (int)target.Score >= target.MaxScore)
                    {
                        RestoreAffected(); polling = false; delays.Add(TransitionDelay); Notify();
                    }
                }
            }
        }

        // The original returns TowerUI positions minus Vector3.up; caller supplies those UI world points.
        public void GetHandPoints(Func<int, Vector3> towerUiWorldPosition, out Vector3 start, out Vector3 end)
        {
            if (towerUiWorldPosition == null) throw new ArgumentNullException(nameof(towerUiWorldPosition));
            start = towerUiWorldPosition(HighlightSourceId) - Vector3.up;
            end = towerUiWorldPosition(HighlightTargetId) - Vector3.up;
            if (HandMode == GuideHandMode.Cut)
            {
                var midpoint = (start + end) * .5f; var rotation = Quaternion.AngleAxis(-90, Vector3.forward);
                start = midpoint + rotation * (start - midpoint); end = midpoint + rotation * (end - midpoint);
            }
        }
        public void GetSkillHandPoints(Func<int, Vector3> skillButtonUiWorldPosition, Func<int, Vector3> towerUiWorldPosition, out Vector3 start, out Vector3 end)
        {
            if (HighlightSkillIndex < 0) { start = end = Vector3.zero; return; }
            start = skillButtonUiWorldPosition(HighlightSkillIndex);
            end = HandMode == GuideHandMode.SkillDrag ? towerUiWorldPosition(HighlightTargetId) - Vector3.up : start;
        }

        private void CloseGuide()
        {
            // GuideControl owns a borrowed GetStarAllLine List reference; ClearState clears that same list.
            if (borrowedAdjacency != null) borrowedAdjacency.Clear();
            borrowedAdjacency = null; affected.Clear(); polling = false; skillHandRemaining = 0;
            Stage = 0; Prompt = null; PromptVisible = false; PersistentTip = null; HighlightSkillIndex = -1;
            HighlightSourceId = HighlightTargetId = 0; HandMode = GuideHandMode.None; Notify();
        }
        public void Dispose()
        {
            if (disposed) return;
            simulation.Event -= OnBattleEvent; delays.Clear(); CloseGuide(); Confirmed=null;disposed = true;
        }
    }
}
