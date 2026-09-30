using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle
{
    // Original GuideUI presentation only. All tutorial decisions remain in BattleGuide.
    public sealed class RecoveredGuidePresentation : MonoBehaviour
    {
        Transform hand, main, backdrop;
        Image icon;
        Text tip;
        Transform pitch;
        SkeletonAnimation demonstration;
        BattleGuide boundGuide;
        GuideHandMode mode;
        int stage = -1, source, target, slot;
        bool initialized, prompt, moving;
        Vector3 from, to, initialHandScale;
        double elapsed;
        float duration;
        int loops;
        public bool HandVisible => hand != null && hand.gameObject.activeSelf;
        public Vector3 HandPosition => hand.position;
        public Vector3 HandScale => hand.localScale;
        public SkeletonAnimation Demonstration => demonstration;

        public void Initialize()
        {
            if (initialized) return;
            hand = Require("guideHand"); main = Require("mainbg"); backdrop = Require("imageBg");
            icon = Require("mainbg/guideIcon").GetComponent<Image>();
            tip = Require("mainbg/textTipAdd").GetComponent<Text>();
            pitch = Require("mainbg/guideContent/Pitch");
            demonstration = Require("mainbg/guideIcon/YD_0").GetComponent<SkeletonAnimation>();
            if (icon == null || demonstration == null) throw new InvalidOperationException("Original GuideUI components missing");
            initialHandScale = hand.localScale;
            demonstration.Initialize(false); demonstration.enabled = false;
            demonstration.Update(0); demonstration.LateUpdate();
            demonstration.gameObject.SetActive(false); hand.gameObject.SetActive(false);
            initialized = true;
        }
        Transform Require(string path) => transform.Find(path) ?? throw new InvalidOperationException("Missing GuideUI node " + path);

        public void Synchronize(BattleGuide guide, Func<int, Vector3> towerCanvasWorldPosition, Func<int, Vector3> skillItemWorldPosition)
        {
            Initialize();
            if (!ReferenceEquals(guide, boundGuide))
            {
                boundGuide = guide; stage = -1; mode = GuideHandMode.None; moving = false;
                demonstration.gameObject.SetActive(false); icon.enabled = true;
                hand.localScale = initialHandScale;
            }
            bool visible = guide != null && guide.Stage > 0;
            bool showPrompt = visible && guide.PromptVisible;
            main.gameObject.SetActive(showPrompt); backdrop.gameObject.SetActive(showPrompt);
            SynchronizePopupText(showPrompt ? guide.Stage : 0);
            if (!visible) { moving = false; hand.gameObject.SetActive(false); stage = 0; return; }
            // ShowUI only enables YD in stage 1; later popups on the same UI object
            // retain that state. Hiding mainbg hides the demonstration with it.
            if (showPrompt && (!prompt || stage != guide.Stage) && guide.Stage == 1)
            { demonstration.gameObject.SetActive(true); icon.enabled = false; }
            var nextMode = showPrompt ? GuideHandMode.None : guide.HandMode;
            if (stage != guide.Stage || mode != nextMode || source != guide.HighlightSourceId || target != guide.HighlightTargetId || slot != guide.HighlightSkillIndex)
            {
                stage = guide.Stage; mode = nextMode; source = guide.HighlightSourceId; target = guide.HighlightTargetId; slot = guide.HighlightSkillIndex;
                moving = false; hand.gameObject.SetActive(false);
                if (mode == GuideHandMode.Connect || mode == GuideHandMode.Cut)
                {
                    var a = towerCanvasWorldPosition(source) - Vector3.up;
                    var b = towerCanvasWorldPosition(target) - Vector3.up;
                    if (mode == GuideHandMode.Cut)
                    { var midpoint = (a + b) * .5f; var rotation = Quaternion.AngleAxis(-90, Vector3.forward); a = midpoint + rotation * (a - midpoint); b = midpoint + rotation * (b - midpoint); }
                    StartHand(mode, a, b);
                }
                else if (mode == GuideHandMode.SkillPulse)
                    StartHand(mode, skillItemWorldPosition(slot), Vector3.one * 1.2f);
                else if (mode == GuideHandMode.SkillDrag)
                    StartHand(mode, skillItemWorldPosition(slot), towerCanvasWorldPosition(target) - Vector3.up);
            }
            prompt = showPrompt;
        }

        void SynchronizePopupText(int promptStage)
        {
            // TextGuideLogic subscribes to ShowUI/OnOK, not to an after-confirm
            // persistent overlay. Preserve original rich text and TextAnchor.
            string value = null;
            switch(promptStage)
            {
                case 6: value="全军出击"; break;
                case 7: value="双倍<color=#FF2A00>增援</color>\n优先占领获得更快增援优势"; break;
                case 8: value="双倍<color=#FF2A00>进攻</color>\n优先占领抢夺进攻先机"; break;
                case 9: value="冰冻敌人，使其<color=#FF2A00>无法进攻</color>"; break;
                case 10: value="火焰覆盖，<color=#FF2A01>剿灭敌人，治愈友军</color>"; break;
                case 11: value="召唤闪电，击溃敌人"; break;
                case 12: value="<color=#FF2A01>攻防一体</color>，无死角攻击\n优先占领，敌人无处遁形"; break;
            }
            tip.text=value??string.Empty;tip.gameObject.SetActive(value!=null);
            tip.alignment=promptStage==7||promptStage==8?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter;
            bool showPitch=promptStage==7||promptStage==8;pitch.gameObject.SetActive(showPitch);
            if(showPitch)
            {
                // GuideConfig.param split ';', child[i].GetChild(2).Text.
                string[] values=promptStage==7?new[]{"2","2","1","2"}:new[]{"2","2","2","1"};
                if(pitch.childCount!=values.Length)throw new InvalidOperationException("Source GuideUI Pitch topology changed");
                for(int i=0;i<values.Length;i++)pitch.GetChild(i).GetChild(2).GetComponent<Text>().text=values[i];
            }
        }

        // Shared by the production controller and deterministic presentation probe.
        public void StartHand(GuideHandMode requestedMode, Vector3 startWorld, Vector3 end)
        {
            Initialize(); mode = requestedMode; hand.position = startWorld;
            from = mode == GuideHandMode.SkillPulse ? hand.localScale : startWorld; to = end;
            duration = mode == GuideHandMode.SkillPulse ? 1f : 2f;
            loops = mode == GuideHandMode.Connect || mode == GuideHandMode.Cut ? int.MaxValue : 15;
            elapsed = 0; moving = mode != GuideHandMode.None; hand.gameObject.SetActive(moving);
        }
        public void Advance(float scaledDelta)
        {
            if (scaledDelta < 0 || float.IsNaN(scaledDelta) || float.IsInfinity(scaledDelta)) throw new ArgumentOutOfRangeException(nameof(scaledDelta));
            Initialize();
            if (demonstration.gameObject.activeInHierarchy)
            { demonstration.Update(scaledDelta); demonstration.LateUpdate(); }
            if (!moving || scaledDelta == 0) return;
            elapsed += scaledDelta;
            bool complete = elapsed >= (double)duration * loops;
            double remainder = elapsed % duration;
            // DOTween renders the completed endpoint on exact loop boundaries.
            float t = complete || remainder == 0 ? 1 : (float)(remainder / duration);
            float eased = t * (2 - t); // Source DOTweenSettings defaultEaseType=6 OutQuad.
            var value = Vector3.LerpUnclamped(from, to, eased);
            if (mode == GuideHandMode.SkillPulse) hand.localScale = value; else hand.position = value;
            if (complete) { moving = false; hand.gameObject.SetActive(false); }
        }
    }
}
