import json
from pathlib import Path
p=Path('UnityProject/Assets/AreaBattle/Scripts/BattleGuide.cs')
s=p.read_text(encoding='utf8')
if 'public static int InitialStage(int level)' in s:
    raise SystemExit('Guide extension is already present; runtime is maintained as C# source.')
r=json.loads(Path('analysis/targets/wxcf1394487200e48f/43/generated/guide-evidence.json').read_text(encoding='utf8'))
q=lambda x:json.dumps(x,ensure_ascii=False)
s=s.replace('// First-three tutorial controller.', '// All nine original main-table tutorial entries.')
s=s.replace('None, Connect, Cut }','None, Connect, Cut, SkillPulse, SkillDrag }')
s=s.replace('private readonly int levelId, smallLevelIndex;', 'private readonly int levelId, smallLevelIndex, commanderMode;')
s=s.replace('public string PersistentTip { get; private set; }','public string PersistentTip { get; private set; }\n        public int HighlightSkillIndex { get; private set; } = -1;')
s=s.replace('int smallLevelIndex = 0)', 'int smallLevelIndex = 0, int commanderMode = 1)')
s=s.replace('this.simulation = simulation;', 'if (commanderMode < 1 || commanderMode > 6) throw new ArgumentOutOfRangeException(nameof(commanderMode));\n            this.commanderMode = commanderMode;\n            this.simulation = simulation;')
s=s.replace('PersistentTip = null; Prompt = null; PromptVisible = false;', 'PersistentTip = null; Prompt = null; PromptVisible = false; HighlightSkillIndex = -1;')
s=s.replace('Stage = smallLevelIndex == 0 ? (levelId == 0 ? 1 : levelId == 1 ? 4 : levelId == 2 ? 5 : 0) : 0;', 'Stage = smallLevelIndex == 0 ? InitialStage(levelId) : 0;')
entry='''        public static int InitialStage(int level)
        {
            switch (level) { case 0: return 1; case 1: return 4; case 2: return 5;
                case 6: return 9; case 7: return 7; case 9: return 8;
                case 14: return 10; case 21: return 11; case 25: return 12; default: return 0; }
        }
        private static readonly Dictionary<string, string> OriginalText = new Dictionary<string, string>
        {
'''+''.join('            { '+q(k)+', '+q(v)+' },\n' for k,v in r['localization'].items() if k.startswith(('Commander.','GuideUI.tipAdd.','guide/4','guide/5','guide/11')))+'''        };
        private static string Text(string key) { return OriginalText[key]; }

'''
s=s.replace('        private void ShowPrompt()',entry+'        private void ShowPrompt()')
s=s.replace('                default: CloseGuide(); return;', '''                case 7: Prompt = new GuidePrompt(Text("guide/41"), Text("guide/42"), "guideUI_icon4"); break;
                case 8: Prompt = new GuidePrompt(Text("guide/51"), Text("guide/52"), "guideUI_icon5"); break;
                case 9: case 10: case 11:
                    int skill = Stage - 9 + commanderMode * 3 - 2;
                    Prompt = new GuidePrompt(Text("Commander.SkillName." + skill), Text("Commander.SkillDescribe." + skill), "guideUI_icon" + Stage + "_" + commanderMode); break;
                case 12: Prompt = new GuidePrompt(Text("guide/113"), Text("guide/114"), "guideUI_icon25"); break;
                default: CloseGuide(); return;''',1)
s=s.replace('case 3: case 4: CloseGuide(); return;', 'case 3: case 4: case 7: case 8: CloseGuide(); return;')
s=s.replace('case 6: PersistentTip = "全军出击"; break;', '''case 6: PersistentTip = "全军出击"; break;
                case 9: case 10: case 11:
                    HighlightSkillIndex = Stage - 9;
                    PersistentTip = Text(Stage == 9 ? "GuideUI.tipAdd.ice" : Stage == 10 ? "GuideUI.tipAdd.fire" : "GuideUI.tipAdd.lighting");
                    HandMode = Stage == 11 ? GuideHandMode.SkillDrag : GuideHandMode.SkillPulse;
                    if (Stage == 11)
                    {
                        foreach (var tower in simulation.Towers)
                            if (tower.Active && ((commanderMode == 1 && tower.Camp != BattleSimulation.PlayerCampID) || (commanderMode == 2 && tower.Camp == BattleSimulation.PlayerCampID)))
                            { HighlightTargetId = tower.Id; break; }
                        if (HighlightTargetId == 0) HandMode = GuideHandMode.None;
                    }
                    break;
                case 12: PersistentTip = Text("GuideUI.tipAdd.arrow"); break;''')
s=s.replace('            if (e.Kind == "restart")', '''            if (e.Kind == "skill-start" && e.Camp == BattleSimulation.PlayerCampID && Stage >= 9 && Stage <= 11 &&
                ((int)e.Value - 1) % 3 == Stage - 9)
            { CloseGuide(); return; }
            if (e.Kind == "restart")''')
s=s.replace('        private void CloseGuide()', '''        public void GetSkillHandPoints(Func<int, Vector3> skillButtonUiWorldPosition, Func<int, Vector3> towerUiWorldPosition, out Vector3 start, out Vector3 end)
        {
            if (HighlightSkillIndex < 0) { start = end = Vector3.zero; return; }
            start = skillButtonUiWorldPosition(HighlightSkillIndex);
            end = HandMode == GuideHandMode.SkillDrag ? towerUiWorldPosition(HighlightTargetId) - Vector3.up : start;
        }

        private void CloseGuide()''')
s=s.replace('Stage = 0; Prompt = null; PromptVisible = false; PersistentTip = null;', 'Stage = 0; Prompt = null; PromptVisible = false; PersistentTip = null; HighlightSkillIndex = -1;')
p.write_text(s,encoding='utf8')
