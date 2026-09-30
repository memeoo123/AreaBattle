using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class GuideValidation
    {
        [Serializable] public sealed class Result { public string id, result, detail; }
        [Serializable] public sealed class Report
        {
            public string evidence = "generated/guide-evidence.json", unityVersion;
            public bool passed = true;
            public List<Result> cases = new List<Result>();
            public string limitations = "Production BattleGuide and BattleSimulation tested against recovered layouts with explicit test topology. Native touch, original guide prefab appearance and live original-game timing are not accepted by these cases.";
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Near(float actual, float expected, string name) { Require(Mathf.Abs(actual - expected) < 0.00001f, name + ": " + actual + " != " + expected); }
        private static BattleSimulation World(int id, Func<int, int, bool> topology = null)
        {
            var layout = JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_" + id));
            return new BattleSimulation(layout, BattleView.ReadConfig(), 4305, topology ?? ((a, b) => true)) { AIEnabled = false };
        }
        private static void Acknowledge(BattleGuide guide) { guide.Tick(0, 1); Require(guide.Confirm(), "one-second prompt acknowledgement"); }
        private static void CaptureTarget(BattleSimulation world, BattleGuide guide)
        {
            var target = world.Tower(guide.HighlightTargetId);
            world.ChangeScore(target.Id, BattleSimulation.PlayerCampID, -target.Score);
        }
        private static void BeginCut(BattleSimulation world, BattleGuide guide)
        {
            Acknowledge(guide);
            Require(guide.TryConnect(guide.HighlightSourceId, guide.HighlightTargetId), "connect tutorial pair");
            CaptureTarget(world, guide); guide.Tick(BattleGuide.TransitionDelay, 0);
            Require(guide.Stage == 2 && guide.HandMode == GuideHandMode.Cut, "stage2 after capture and scaled delay");
        }

        [MenuItem("AreaBattle/Validate All Guides")]
        public static void ValidateMenu() { if (!Run().passed) throw new InvalidOperationException("Guide validation failed"); }
        public static BattleBuild.Report Run()
        {
            var report = new Report { unityVersion = Application.unityVersion };
            Action<string, Action> check = (id, action) =>
            {
                try { action(); report.cases.Add(new Result { id = id, result = "pass" }); }
                catch (Exception ex) { report.passed = false; report.cases.Add(new Result { id = id, result = "fail", detail = ex.ToString() }); }
            };
            check("popup-countdown-pauses-battle", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0))
                {
                    Require(g.Stage == 1 && g.Prompt.Title == "操作" && w.State == BattlePhase.Pause, "initial prompt");
                    Require(!g.Confirm() && !g.CanDraw, "must not acknowledge immediately");
                    g.Tick(10, .5f); Require(!g.CanConfirm, "countdown uses unscaled input");
                    g.Tick(0, .5f); Require(g.Confirm() && w.State == BattlePhase.Running, "confirm resumes");
                }
            });
            check("stage1-original-target-and-visibility", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0))
                {
                    Acknowledge(g); Require(g.HighlightSourceId == 4 && g.HighlightTargetId == 1, "first player and greatest worldZ");
                    Require(g.VisibleTowerIds.Count == 2 && g.IsTowerVisible(4) && g.IsTowerVisible(1), "neighbor visibility");
                    Require(w.Towers.TrueForAll(t => t.Active), "hiding must preserve simulation Active");
                    Require(w.Lines.TrueForAll(l => l.Direction == 0), "hand does not auto-connect");
                    Require(!g.TryConnect(4, 2), "hidden target cannot receive touch");
                }
            });
            check("connection-waits-for-capture", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0))
                {
                    Acknowledge(g); Require(g.TryConnect(4, 1), "source drag");
                    Require(g.Stage == 1 && !g.CanDraw && g.HandMode == GuideHandMode.None, "connection locks input only");
                    g.Tick(1, 1); Require(g.Stage == 1, "no capture means no next stage");
                    CaptureTarget(w, g); g.Tick(0, 1); Require(g.Stage == 1, "transition requires scaled time");
                    g.Tick(.69f, 0); Require(g.Stage == 1, "before0.7");
                    g.Tick(.02f, 0); Require(g.Stage == 2 && !g.PromptVisible && g.CanDraw, "automatic empty-title cut stage");
                }
            });
            check("cut-restores-neighbors-and-exits-through-stage3", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0))
                {
                    BeginCut(w, g); var borrowed = w.GetPotentialLines(4);
                    Require(g.TryCut(w.FindLine(4, 1).Id), "cut line");
                    Require(g.VisibleTowerIds.Count == 4 && !g.CanDraw && g.Stage == 2, "restore before delay");
                    g.Tick(BattleGuide.TransitionDelay, 0); Require(g.Stage == 3 && g.PromptVisible && w.State == BattlePhase.Pause, "victory-objective prompt");
                    Acknowledge(g); Require(g.Stage == 0 && w.State == BattlePhase.Running && g.CanDraw, "close to free battle");
                    Require(borrowed.Count == 0, "original ClearState clears borrowed adjacency list");
                    Require(w.Lines.Count == 6 && w.Towers.TrueForAll(t => t.Active), "clear does not remove line entities or towers");
                }
            });
            check("stage2-hand-uses-ui-plane-forward-axis", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0))
                {
                    BeginCut(w, g); Vector3 start, end;
                    g.GetHandPoints(id => new Vector3(id, 2, 0), out start, out end);
                    Near(start.x, 2.5f, "startX"); Near(start.y, -.5f, "startY");
                    Near(end.x, 2.5f, "endX"); Near(end.y, 2.5f, "endY");
                }
            });
            check("level1-stage4-has-no-forced-action", () =>
            {
                var w = World(1); using (var g = new BattleGuide(w, 1))
                {
                    Require(g.Stage == 4 && g.Prompt.Title == "阵营", "level1 prompt"); Acknowledge(g);
                    Require(g.Stage == 0 && g.VisibleTowerIds.Count == w.Towers.Count && g.CanDraw, "free battle after OK");
                    Require(w.Lines.TrueForAll(l => l.Direction == 0), "no auto-line");
                }
            });
            check("stage5-source-target-and-growth-gate", () =>
            {
                var w = World(2); using (var g = new BattleGuide(w, 2))
                {
                    Acknowledge(g); Require(g.HighlightSourceId == 2 && g.HighlightTargetId == 1, "65-point donor and nearest55-point receiver");
                    Require(!g.IsTowerVisible(3) && w.Tower(3).Active, "enemy hidden but active");
                    Require(w.Towers.TrueForAll(t => !t.AutoAddScore), "disable all active tower regen");
                    Require(!g.IsSourceAllowed(1) && g.IsSourceAllowed(2), "stage5 permits only max-score source");
                    Require(w.Lines.TrueForAll(l => l.Direction == 0), "no automatic donor connection");
                }
            });
            check("stage5-poll-scaled-delay-stage6-tip", () =>
            {
                var w = World(2); using (var g = new BattleGuide(w, 2))
                {
                    Acknowledge(g); Require(g.TryConnect(2, 1), "connect donor");
                    Require(!g.CanDraw, "wait for receiver full");
                    w.ChangeScore(1, 1, 10); g.Tick(10, 0); Require(!g.HasPendingTransition, "poll does not use scaled time");
                    g.Tick(0, .99f); Require(!g.HasPendingTransition, "before poll boundary");
                    g.Tick(0, .01f); Require(g.HasPendingTransition && g.IsTowerVisible(3), "poll max restores neighbors");
                    g.Tick(0, 1); Require(g.Stage == 5, "unscaled time cannot finish scaled wait");
                    g.Tick(BattleGuide.TransitionDelay, 0); Require(g.Stage == 6 && g.PromptVisible, "stage6 popup");
                    Acknowledge(g); Require(g.Stage == 6 && g.PersistentTip == "全军出击" && g.CanDraw, "retain stage6 and tip");
                    Require(w.Towers.TrueForAll(t => !t.AutoAddScore), "do not restore natural growth");
                }
            });
            check("stage5-unscaled-large-frame-skipped", () =>
            {
                var w = World(2); using (var g = new BattleGuide(w, 2))
                {
                    Acknowledge(g); w.ChangeScore(1, 1, 10); g.Tick(0, 2);
                    Require(!g.HasPendingTransition, "original unscaled delta greater than1 is ignored");
                    g.Tick(0, 1); Require(g.HasPendingTransition, "normal unscaled frame polls");
                }
            });
            check("stage5-score-tie-selects-last-player", () =>
            {
                var w = World(2); w.ChangeScore(1, 1, 10);
                using (var g = new BattleGuide(w, 2)) { Acknowledge(g); Require(g.HighlightSourceId == 2, "last tie wins"); }
            });
            check("restart-restores-real-first-stage", () =>
            {
                var w = World(2); using (var g = new BattleGuide(w, 2))
                {
                    Acknowledge(g); g.TryConnect(2, 1); w.Restart();
                    Require(g.Stage == 5 && g.PromptVisible && w.State == BattlePhase.Pause, "restart enters tutorial");
                    Require(g.VisibleTowerIds.Count == w.Towers.Count && w.Towers.TrueForAll(t => t.AutoAddScore), "fresh visibility and growth until OK");
                    Require(w.Lines.TrueForAll(l => l.Direction == 0), "fresh directions");
                }
            });
            check("later-small-level-has-no-first-branch-guide", () =>
            {
                var w = World(0); using (var g = new BattleGuide(w, 0, 1))
                    Require(g.Stage == 0 && !g.PromptVisible && g.CanDraw, "LevelType second entry0");
            });
            check("level2-production-arrivals-complete-guide", () =>
            {
                var w = World(2); using (var g = new BattleGuide(w, 2))
                {
                    Acknowledge(g); Require(g.TryConnect(2, 1), "donor line");
                    for (int frame = 0; frame < 2400 && g.Stage == 5; frame++) { g.Tick(1f / 60, 1f / 60); w.Tick(1f / 60); }
                    Require(g.Stage == 6, "production/arrival and poll reach stage6");
                    Near(w.Tower(1).Score, 65, "receiver full through actual soldiers");
                }
            });
            check("all-nine-original-guide-entry-mappings", () =>
            {
                int[] levels = { 0, 1, 2, 6, 7, 9, 14, 21, 25 };
                int[] stages = { 1, 4, 5, 9, 7, 8, 10, 11, 12 };
                for (int i = 0; i < levels.Length; i++)
                {
                    var w = World(levels[i] == 25 ? 501 : levels[i]);
                    using (var g = new BattleGuide(w, levels[i]))
                        Require(g.Stage == stages[i] && g.PromptVisible && w.State == BattlePhase.Pause, "entry " + levels[i]);
                    using (var g = new BattleGuide(World(0), levels[i], 1)) Require(g.Stage == 0, "only first small-level branch");
                }
                Require(BattleGuide.InitialStage(3) == 0 && BattleGuide.InitialStage(26) == 0, "ordinary entries");
            });
            check("defense-attack-dismiss-arrow-remains", () =>
            {
                foreach (int level in new[] { 7, 9, 25 })
                {
                    var w = World(level == 25 ? 501 : level);
                    using (var g = new BattleGuide(w, level))
                    {
                        Acknowledge(g);
                        Require(g.Stage == (level == 25 ? 12 : 0) && !g.PromptVisible && g.CanDraw, "post OK " + level);
                        Require(level != 25 || g.PersistentTip.Contains("敌人无处遁形"), "original arrow tip");
                        Require(w.Lines.TrueForAll(l => l.Direction == 0), "no auto connection");
                    }
                }
            });
            check("skill-tutorial-production-casts-close-matching-slot", () =>
            {
                int[] levels = { 6, 14, 21 };
                for (int slot = 0; slot < 3; slot++)
                {
                    var w = World(levels[slot]); BattleView.ConfigureSkills(w);
                    using (var g = new BattleGuide(w, levels[slot]))
                    {
                        Acknowledge(g); Require(g.Stage == slot + 9 && g.HighlightSkillIndex == slot && g.CanDraw, "skill guidance running");
                        Require(w.CastSkill(slot == 0 ? 4 : 1, 1, 2), "nonplayer production cast");
                        Require(g.Stage == slot + 9, "nonplayer cast does not exit player tutorial");
                        int wrongId = slot == 0 ? 5 : 4;
                        Require(w.CastSkill(wrongId, 1), "wrong slot production cast");
                        Require(g.Stage == slot + 9, "wrong slot does not exit");
                        int target = slot == 2 ? w.Towers.Find(t => t.Active && t.Camp != 1).Id : 0;
                        Require(w.CastSkill(slot + 1, 1, 1, target, null, Vector3.zero), "matching production cast");
                        Require(g.Stage == 0 && g.HighlightSkillIndex == -1 && !g.PromptVisible && g.CanDraw, "matching original event closes immediately");
                    }
                }
            });
            check("commander-specific-prompts-and-drag-target", () =>
            {
                for (int mode = 1; mode <= 6; mode++)
                {
                    var w = World(21);
                    using (var g = new BattleGuide(w, 21, 0, mode))
                    {
                        Require(g.Prompt.Picture == "guideUI_icon11_" + mode, "original mode picture");
                        Require(!g.Prompt.Title.Contains("{0}"), "formatted localized title");
                        Acknowledge(g);
                        if (mode <= 2)
                        {
                            var expected = w.Towers.Find(t => t.Active && (mode == 1 ? t.Camp != 1 : t.Camp == 1));
                            Require(g.HighlightTargetId == expected.Id && g.HandMode == GuideHandMode.SkillDrag, "first matching target by commander mode");
                            Vector3 a, b; g.GetSkillHandPoints(i => new Vector3(i, 7, 8), i => w.Tower(i).Position, out a, out b);
                            Require(a == new Vector3(2, 7, 8) && b == expected.Position - Vector3.up, "original hand endpoint conversion");
                        }
                        else Require(g.HighlightTargetId == 0 && g.HandMode == GuideHandMode.None, "original GetEndPosInSkill only modes1/2");
                    }
                }
            });
            string workspace = Directory.GetParent(Application.dataPath).Parent.FullName;
            Directory.CreateDirectory(Path.Combine(workspace, "analysis"));
            File.WriteAllText(Path.Combine(workspace, "analysis/unity-guide-validation.json"), JsonUtility.ToJson(report, true));
            var combined = new BattleBuild.Report { passed = report.passed, unityVersion = report.unityVersion, limitations = report.limitations };
            foreach (var result in report.cases) combined.checks.Add(new BattleBuild.Check { id = result.id, result = result.result, detail = result.detail });
            Debug.Log("AREABATTLE_GUIDE_VALIDATION_" + (report.passed ? "PASS " : "FAIL ") + report.cases.Count);
            return combined;
        }
    }
}
