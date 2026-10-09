using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle
{
    public sealed partial class BattleHud
    {
        readonly Dictionary<int, Button> advancementBadges = new Dictionary<int, Button>();
        RectTransform advancementPanel;
        Text advancementTitle;
        readonly Button[] advancementChoices = new Button[3];
        int advancingTower;

        Button AdvancementButton(Transform parent, string name, Vector2 size, Vector2 position, string caption, Font font, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.layer = 5;
            var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            if (parent == advancementPanel) rt.anchorMin = rt.anchorMax = new Vector2(.5f, 1);
            rt.sizeDelta = size; rt.anchoredPosition = position;
            go.GetComponent<Image>().color = new Color(.10f, .23f, .34f, .97f);
            var button = go.GetComponent<Button>(); button.onClick.AddListener(action);
            var colors = button.colors; colors.disabledColor = Color.white; button.colors = colors;
            var textGo = new GameObject("Caption", typeof(RectTransform), typeof(Text)); textGo.layer = 5;
            var tr = (RectTransform)textGo.transform; tr.SetParent(rt, false);
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = new Vector2(8, 2); tr.offsetMax = new Vector2(-8, -2);
            var text = textGo.GetComponent<Text>(); text.font = font; text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; text.text = caption;
            return button;
        }
        void SynchronizeAdvancement()
        {
            var sim = view.Simulation;
            bool available = sim.State == BattlePhase.Running && (view.Guide == null || view.Guide.Stage == 0);
            foreach (var t in sim.Towers)
            {
                if (!labels.TryGetValue(t.Id, out var label)) continue;
                if (!advancementBadges.TryGetValue(t.Id, out var badge))
                {
                    int id = t.Id;
                    badge = AdvancementButton(towerLayer, "AdvanceTower_" + id, (view.BasicTowerExperiment ? new Vector2(114, 28) : new Vector2(170, 42)), Vector2.zero, "", label.Score.font,
                        () => { if (sim.CanAdvance(id)) OpenAdvancement(id, label.Score.font); });
                    if(view.BasicTowerExperiment){
                        badge.GetComponentInChildren<Text>().fontSize=22;
                        badge.GetComponent<Image>().color=new Color(.10f,.23f,.34f,.72f);
                    }
                    advancementBadges.Add(id, badge);
                }
                bool show = available && label.Root.gameObject.activeSelf && (view.BasicTowerExperiment || sim.CanAdvance(t.Id) || t.Specialization != TowerSpecialization.None);
                badge.gameObject.SetActive(show);
                ((RectTransform)badge.transform).anchoredPosition = label.Root.anchoredPosition + new Vector2(0, view.BasicTowerExperiment ? 59 : 130);
                badge.interactable = sim.CanAdvance(t.Id);
                string identity = t.Specialization == TowerSpecialization.None ? (view.BasicTowerExperiment ? "基础塔" : "进阶") :
                    RouteName(t.Specialization) + (t.Doctrine < 0 ? "" : "·" + AdvancementCatalog.DoctrineName(t.Doctrine));
                badge.GetComponentInChildren<Text>().text = identity + (sim.CanAdvance(t.Id) ? " ↑" : "");
            }
            if(view.BasicTowerExperiment)SynchronizeEvolutionModal();
            if (advancementPanel != null)
            {
                if (!available || !sim.CanAdvance(advancingTower)) advancingTower = 0;
                advancementPanel.gameObject.SetActive(advancingTower != 0);
                if (advancingTower != 0)
                {
                    var t = sim.Tower(advancingTower);
                    advancementTitle.text = (view.BasicTowerExperiment ? (t.Specialization == TowerSpecialization.None ? "基础塔" : "进阶塔") : BattleSimulation.BaseTowerName(t.ShipID)) + " · " + ((t.AdvancementSpent + 1) * BattleSimulation.AdvancementStep) + " 点进阶\n" +
                        (t.Specialization == TowerSpecialization.None ? "保留原兵种能力，选择连接方式" : RouteName(t.Specialization) + " / " + (t.Doctrine < 0 ? "选择子路线" : AdvancementCatalog.DoctrineName(t.Doctrine) + " → 下一阶段"));
                    var options = sim.AdvancementOptions(t.Id);
                    for (int i = 0; i < advancementChoices.Length; i++)
                    {
                        advancementChoices[i].gameObject.SetActive(i < options.Length);
                        if (i < options.Length) advancementChoices[i].GetComponentInChildren<Text>().text = options[i].Name + "\n" + options[i].Description;
                    }
                }
            }
        }
        void OpenAdvancement(int id, Font font)
        {
            if(view.BasicTowerExperiment){SynchronizeEvolutionModal();return;}
            advancingTower = id;
            if (advancementPanel == null)
            {
                var go = new GameObject("Tower advancement", typeof(RectTransform), typeof(Image)); go.layer = 5;
                advancementPanel = (RectTransform)go.transform; advancementPanel.SetParent(Canvas.transform, false);
                advancementPanel.anchorMin = advancementPanel.anchorMax = new Vector2(.5f, 1);
                advancementPanel.pivot = new Vector2(.5f, 1); advancementPanel.anchoredPosition = new Vector2(0, -190);
                advancementPanel.sizeDelta = new Vector2(800, 450); go.GetComponent<Image>().color = new Color(.04f, .08f, .14f, .98f);
                var title = AdvancementButton(advancementPanel, "Title", new Vector2(770, 64), new Vector2(0, -35), "", font, () => {});
                title.interactable = false; advancementTitle = title.GetComponentInChildren<Text>(); advancementTitle.fontSize = 20;
                for (int i = 0; i < 3; i++)
                {
                    int slot = i;
                    advancementChoices[i] = AdvancementButton(advancementPanel, "Choice_" + i, new Vector2(760, 88), new Vector2(0, -120 - i * 96), "", font,
                        () => {
                            var tower = view.Simulation.Tower(advancingTower);
                            if (tower != null)
                            {
                                var options = view.Simulation.AdvancementOptions(advancingTower);
                                if (slot < options.Length) view.Simulation.ChooseAdvancement(advancingTower, options[slot].Id);
                            }
                            advancingTower = 0; advancementPanel.gameObject.SetActive(false);
                        });
                }
                AdvancementButton(advancementPanel, "Close", new Vector2(760, 48), new Vector2(0, -410), "稍后选择 · 战斗继续进行", font,
                    () => { advancingTower = 0; advancementPanel.gameObject.SetActive(false); });
            }
            advancementPanel.gameObject.SetActive(true); advancementPanel.SetAsLastSibling(); SynchronizeAdvancement();
        }
        void LayoutExperimentTower(TowerLabel label,TowerState tower)
        {
            // One compact panel, fixed to the actual sprite roof. Numeric grade changes must not move it independently.
            if(label.Root.Find("InfoBackground")==null)
            {
                var bg=new GameObject("InfoBackground",typeof(RectTransform),typeof(Image));bg.layer=5;
                var rt=(RectTransform)bg.transform;rt.SetParent(label.Root,false);rt.SetAsFirstSibling();
                rt.sizeDelta=new Vector2(68,40);rt.anchoredPosition=new Vector2(0,29);
                bg.GetComponent<Image>().color=new Color(.06f,.12f,.18f,.60f);bg.GetComponent<Image>().raycastTarget=false;
                var stem=new GameObject("TowerAnchor",typeof(RectTransform),typeof(Image));stem.layer=5;
                var st=(RectTransform)stem.transform;st.SetParent(label.Root,false);st.sizeDelta=new Vector2(5,12);st.anchoredPosition=new Vector2(0,-20);
                stem.GetComponent<Image>().color=new Color(.9f,.94f,1f,.95f);stem.GetComponent<Image>().raycastTarget=false;
                label.Score.rectTransform.anchoredPosition=new Vector2(0,29);label.Score.rectTransform.sizeDelta=new Vector2(142,40);
                label.Score.alignment=TextAnchor.MiddleCenter;label.Score.fontSize=34;label.Score.color=Color.white;label.Score.raycastTarget=false;
                foreach(string family in new[]{"normal","defense","attack"})
                {
                    var group=label.Root.Find(family);if(group==null)continue;
                    var layout=group.GetComponent<HorizontalLayoutGroup>();if(layout!=null)layout.enabled=false;
                    ((RectTransform)group).anchoredPosition=Vector2.zero;
                }
            }
            int slots=Mathf.Clamp(Mathf.Max(tower.MaxLines,tower.OutgoingCount),1,3);
            for(int i=0;i<label.Slots.Length;i++)if(label.Slots[i]!=null)
            {
                var rt=label.Slots[i].rectTransform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);
                rt.anchoredPosition=new Vector2((i-(slots-1)*.5f)*25,0);rt.sizeDelta=new Vector2(18,18);
                label.Slots[i].raycastTarget=false;
            }
            var art=view.TowerPresentationTransform(tower.Id).Find("Tower Art").GetComponent<SpriteRenderer>();
            var roof=art.sprite==null?tower.Position+Vector3.up*.35f:art.transform.TransformPoint(new Vector3(0,art.sprite.bounds.max.y,0));
            var screen=view.BattleCamera.WorldToScreenPoint(roof);
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(towerLayer,screen,UICamera,out var point))
                // Recovered sprites include transparent space above the visible roof.
                label.Root.anchoredPosition=point+new Vector2(0,tower.IsArrow?-16:-63);
        }
        static string RouteName(TowerSpecialization route) => BattleSimulation.ConnectionName(route);
        public bool BlocksAdvancementPointer(Vector2 screen)
        {
            if (advancementPanel != null && advancementPanel.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(advancementPanel, screen, UICamera)) return true;
            foreach (var b in advancementBadges.Values)
                if (b != null && b.gameObject.activeInHierarchy && b.interactable && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)b.transform, screen, UICamera)) return true;
            return false;
        }
        void ClearAdvancement()
        {
            ClearEvolutionModal();
            foreach (var badge in advancementBadges.Values) if (badge != null) DestroyHudObject(badge.gameObject);
            advancementBadges.Clear(); advancingTower = 0;
            if (advancementPanel != null) DestroyHudObject(advancementPanel.gameObject);
            advancementPanel = null;
        }
    }
}
