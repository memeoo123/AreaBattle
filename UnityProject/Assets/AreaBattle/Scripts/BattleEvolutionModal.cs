using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed partial class BattleHud
    {
        RectTransform evolutionOverlay, evolutionSheet, evolutionHighlight;
        Text evolutionHeading, evolutionContext, evolutionHint;
        Button evolutionConfirm;
        readonly Button[] evolutionCards=new Button[3];
        readonly Dictionary<int,int> evolutionSeen=new Dictionary<int,int>();
        readonly Dictionary<int,float> evolutionNoticeUntil=new Dictionary<int,float>();
        int evolutionTower, evolutionSelection=-1;
        public bool EvolutionOpen=>evolutionTower!=0;
        static readonly Color EvolutionInk=new Color(.13f,.20f,.26f);
        static readonly Color[] EvolutionColors={new Color(.22f,.52f,.74f),new Color(.18f,.56f,.43f),new Color(.60f,.40f,.72f)};
        RectTransform EvolutionBox(Transform parent,string name,Vector2 size,Vector2 pos,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.layer=5;
            var rt=(RectTransform)go.transform;rt.SetParent(parent,false);rt.sizeDelta=size;rt.anchoredPosition=pos;
            go.GetComponent<Image>().color=color;return rt;
        }
        Text EvolutionText(Transform parent,string name,Vector2 size,Vector2 pos,string caption,int fontSize,Font font,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.layer=5;
            var rt=(RectTransform)go.transform;rt.SetParent(parent,false);rt.sizeDelta=size;rt.anchoredPosition=pos;
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=fontSize;text.alignment=align;text.color=EvolutionInk;
            text.raycastTarget=false;text.text=caption;return text;
        }
        // Code-native connection diagrams over the actual in-game tower sprite.
        void EvolutionArrow(Transform parent,float x,float y,Color color)
        {
            EvolutionBox(parent,"Shaft",new Vector2(7,33),new Vector2(x,y),color);
            var l=EvolutionBox(parent,"ArrowLeft",new Vector2(7,20),new Vector2(x-6,y+13),color);l.localRotation=Quaternion.Euler(0,0,-40);
            var r=EvolutionBox(parent,"ArrowRight",new Vector2(7,20),new Vector2(x+6,y+13),color);r.localRotation=Quaternion.Euler(0,0,40);
        }
        void BuildEvolutionModal(Font font)
        {
            evolutionOverlay=EvolutionBox(Canvas.transform,"Evolution modal",Vector2.zero,Vector2.zero,new Color(.025f,.06f,.10f,.12f));
            evolutionOverlay.anchorMin=Vector2.zero;evolutionOverlay.anchorMax=Vector2.one;evolutionOverlay.offsetMin=evolutionOverlay.offsetMax=Vector2.zero;
            evolutionHighlight=EvolutionBox(evolutionOverlay,"Selected tower",new Vector2(190,230),Vector2.zero,new Color(1,.82f,.32f,.08f));
            evolutionHighlight.GetComponent<Image>().raycastTarget=false;
            foreach(string side in new[]{"Top","Bottom","Left","Right"}){
                var line=EvolutionBox(evolutionHighlight,side,Vector2.zero,Vector2.zero,new Color(1,.73f,.13f));
                bool horizontal=side=="Top"||side=="Bottom";
                line.anchorMin=horizontal?new Vector2(0,side=="Top"?1:0):new Vector2(side=="Right"?1:0,0);
                line.anchorMax=horizontal?new Vector2(1,side=="Top"?1:0):new Vector2(side=="Right"?1:0,1);
                line.sizeDelta=horizontal?new Vector2(0,5):new Vector2(5,0);line.GetComponent<Image>().raycastTarget=false;
            }
            var tag=EvolutionBox(evolutionHighlight,"Tag",new Vector2(190,38),Vector2.zero,new Color(1,.73f,.13f));
            tag.anchorMin=tag.anchorMax=new Vector2(.5f,1);tag.anchoredPosition=new Vector2(0,23);
            EvolutionText(tag,"Label",new Vector2(190,36),Vector2.zero,"正在进阶",25,font);
            evolutionSheet=EvolutionBox(evolutionOverlay,"Sheet",new Vector2(1000,450),new Vector2(0,245),new Color(.96f,.95f,.90f,.99f));
            evolutionSheet.anchorMin=evolutionSheet.anchorMax=new Vector2(.5f,0);
            var shadow=evolutionSheet.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.30f);shadow.effectDistance=new Vector2(4,5);
            EvolutionBox(evolutionSheet,"Top stripe",new Vector2(1000,5),new Vector2(0,223),new Color(.94f,.70f,.24f));
            evolutionHeading=EvolutionText(evolutionSheet,"Heading",new Vector2(225,48),new Vector2(-360,184),"10级进阶",34,font,TextAnchor.MiddleLeft);
            evolutionContext=EvolutionText(evolutionSheet,"Context",new Vector2(700,42),new Vector2(112,184),"",25,font,TextAnchor.MiddleRight);
            string[] names={"单线突破","分流进攻","箭塔防守"};
            string[] descriptions={"1路 · 出兵120%\n集中攻一个目标","2路 · 每路出兵100%\n增加线路不减速","自动射击 · 不能出兵\n压制敌兵和敌塔"};
            for(int i=0;i<3;i++){
                int slot=i;
                var card=EvolutionBox(evolutionSheet,"Route_"+i,new Vector2(304,166),new Vector2(-320+i*320,61),Color.white);
                var button=card.gameObject.AddComponent<Button>();button.targetGraphic=card.GetComponent<Image>();button.onClick.AddListener(()=>SelectEvolution(slot));evolutionCards[i]=button;
                var edge=card.gameObject.AddComponent<Outline>();edge.effectColor=EvolutionColors[i];edge.effectDistance=Vector2.zero;
                EvolutionBox(card,"Stripe",new Vector2(304,5),new Vector2(0,81),EvolutionColors[i]);
                var symbol=EvolutionBox(card,"Route diagram",new Vector2(100,60),new Vector2(-111,36),Color.clear);symbol.localScale=Vector3.one*.6f;
                if(i==0)EvolutionArrow(symbol,0,0,EvolutionColors[i]);
                if(i==1){EvolutionBox(symbol,"Branch",new Vector2(64,7),new Vector2(0,-16),EvolutionColors[i]);EvolutionArrow(symbol,-29,1,EvolutionColors[i]);EvolutionArrow(symbol,29,1,EvolutionColors[i]);}
                if(i==2){EvolutionArrow(symbol,0,0,EvolutionColors[i]);EvolutionBox(symbol,"Bow",new Vector2(65,7),new Vector2(0,-7),EvolutionColors[i]);}
                EvolutionText(card,"Name",new Vector2(230,46),new Vector2(28,39),names[i],31,font);
                EvolutionText(card,"Benefits",new Vector2(290,82),new Vector2(0,-30),descriptions[i],27,font).lineSpacing=1.3f;
            }
            evolutionHint=EvolutionText(evolutionSheet,"Selection hint",new Vector2(950,52),new Vector2(0,-69),"选择高亮塔的发展路线 · 战斗已暂停",25,font);
            var confirm=EvolutionBox(evolutionSheet,"Confirm",new Vector2(944,72),new Vector2(0,-151),EvolutionInk);
            evolutionConfirm=confirm.gameObject.AddComponent<Button>();evolutionConfirm.targetGraphic=confirm.GetComponent<Image>();evolutionConfirm.onClick.AddListener(ConfirmEvolution);
            var label=EvolutionText(confirm,"Caption",new Vector2(920,68),Vector2.zero,"请选择路线",31,font);label.color=Color.white;
        }
        void SelectEvolution(int slot)
        {
            if(!EvolutionOpen||slot<0||slot>=3)return;
            evolutionSelection=slot;
            for(int i=0;i<3;i++){
                evolutionCards[i].GetComponent<Outline>().effectDistance=i==slot?new Vector2(4,-4):Vector2.zero;
                evolutionCards[i].GetComponent<Image>().color=i==slot?Color.Lerp(Color.white,EvolutionColors[i],.13f):Color.white;
            }
            evolutionConfirm.interactable=true;evolutionConfirm.GetComponent<Image>().color=EvolutionColors[slot];
            evolutionConfirm.GetComponentInChildren<Text>().text="确认进阶为"+view.Simulation.AdvancementOptions(evolutionTower)[slot].Name;
            evolutionHint.text=new[]{"每10级出兵效率增加5个百分点 · 始终1路","20级解锁第3路 · 每10级每路增加5个百分点","射速与射程逐级提高 · 低于10仍保留箭塔"}[slot];
        }
        void ConfirmEvolution()
        {
            if(evolutionSelection<0||!EvolutionOpen)return;
            var options=view.Simulation.AdvancementOptions(evolutionTower);
            if(evolutionSelection>=options.Length||!view.Simulation.ChooseAdvancement(evolutionTower,options[evolutionSelection].Id))return;
            evolutionTower=0;evolutionSelection=-1;view.CancelInputForEvolution();
            SynchronizeEvolutionModal();view.RefreshPresentation();
        }
        void SynchronizeEvolutionModal()
        {
            var sim=view.Simulation;
            bool allowed=sim.State==BattlePhase.Running&&(view.Guide==null||view.Guide.Stage==0);
            int next=0,count=0;
            if(allowed)foreach(var t in sim.Towers)if(sim.CanAdvance(t.Id)){if(next==0)next=t.Id;count++;}
            if(next==0){
                evolutionTower=0;
                if(evolutionOverlay!=null)evolutionOverlay.gameObject.SetActive(false);
                Time.timeScale=sim.State==BattlePhase.Pause?0:1;
            }else{
                if(next!=evolutionTower||evolutionOverlay==null){
                    evolutionTower=next;evolutionSelection=-1;
                    if(evolutionOverlay==null)BuildEvolutionModal(labels[next].Score.font);
                    view.CancelInputForEvolution();
                    evolutionConfirm.interactable=false;evolutionConfirm.GetComponent<Image>().color=new Color(.48f,.52f,.53f);
                    evolutionConfirm.GetComponentInChildren<Text>().text="请选择路线";
                    evolutionHint.text="选择高亮塔的发展路线 · 战斗已暂停";
                    foreach(var card in evolutionCards){card.GetComponent<Outline>().effectDistance=Vector2.zero;card.GetComponent<Image>().color=Color.white;}
                }
                evolutionOverlay.gameObject.SetActive(true);evolutionOverlay.SetAsLastSibling();Time.timeScale=0;
                int index=sim.Towers.FindIndex(t=>t.Id==next);
                string[] rows={"上方","中部","下方"},cols={"左侧","中央","右侧"};
                float minX=float.MaxValue,maxX=float.MinValue,minZ=float.MaxValue,maxZ=float.MinValue;
                foreach(var tower in sim.Towers){minX=Mathf.Min(minX,tower.Position.x);maxX=Mathf.Max(maxX,tower.Position.x);minZ=Mathf.Min(minZ,tower.Position.z);maxZ=Mathf.Max(maxZ,tower.Position.z);}
                var position=sim.Tower(next).Position;
                int col=maxX-minX<.01f?1:Mathf.Clamp((int)(Mathf.InverseLerp(minX,maxX,position.x)*3),0,2);
                int row=maxZ-minZ<.01f?1:2-Mathf.Clamp((int)(Mathf.InverseLerp(minZ,maxZ,position.z)*3),0,2);
                evolutionContext.text=rows[row]+cols[col]+"的塔"+(count>1?" · 另有"+(count-1)+"座待进阶":" · 战斗已暂停");
                var screen=RectTransformUtility.WorldToScreenPoint(UICamera,labels[next].Root.position);
                var ground=view.BattleCamera.WorldToScreenPoint(sim.Tower(next).Position);
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(evolutionOverlay,screen,UICamera,out var local)&&
                   RectTransformUtility.ScreenPointToLocalPointInRectangle(evolutionOverlay,ground,UICamera,out var foot)){
                    float top=local.y+88,bottom=foot.y-24;
                    evolutionHighlight.anchoredPosition=new Vector2(local.x,(top+bottom)*.5f);
                    evolutionHighlight.sizeDelta=new Vector2(190,Mathf.Max(180,top-bottom));
                    // Keep the chosen tower visible even when a short viewport puts it behind the bottom sheet.
                    float sheetTop=-evolutionOverlay.rect.height*.5f+470;
                    bool moveToTop=bottom<sheetTop+12;
                    evolutionSheet.anchorMin=evolutionSheet.anchorMax=new Vector2(.5f,moveToTop?1:0);
                    evolutionSheet.anchoredPosition=new Vector2(0,moveToTop?-245:245);
                }
            }
            if(!battleHudClosed&&skillPanel!=null)skillPanel.gameObject.SetActive(!EvolutionOpen);
            foreach(var t in sim.Towers){
                if(evolutionSeen.TryGetValue(t.Id,out int previous)&&t.AdvancementSpent>previous&&t.AdvancementSpent>=2)evolutionNoticeUntil[t.Id]=Time.unscaledTime+3;
                if(evolutionSeen.TryGetValue(t.Id,out int oldTier)&&t.AdvancementSpent<oldTier)evolutionNoticeUntil.Remove(t.Id);
                evolutionSeen[t.Id]=t.AdvancementSpent;
                if(!labels.TryGetValue(t.Id,out var label))continue;
                var notice=label.Root.Find("Evolution notice");
                if(notice==null){var text=EvolutionText(label.Root,"Evolution notice",new Vector2(340,48),new Vector2(0,112),"",24,label.Score.font);text.color=new Color(1,.88f,.35f);var o=text.gameObject.AddComponent<Outline>();o.effectColor=EvolutionInk;notice=text.transform;}
                notice.gameObject.SetActive(t.Camp==BattleSimulation.PlayerCampID&&evolutionNoticeUntil.TryGetValue(t.Id,out float until)&&Time.unscaledTime<until);
                notice.GetComponent<Text>().text=BattleSimulation.EvolutionBenefit(t);
            }
        }
        void ClearEvolutionModal()
        {
            if(EvolutionOpen)Time.timeScale=view!=null&&view.Simulation!=null&&view.Simulation.State==BattlePhase.Pause?0:1;
            evolutionTower=0;evolutionSelection=-1;evolutionSeen.Clear();evolutionNoticeUntil.Clear();
            if(evolutionOverlay!=null)DestroyHudObject(evolutionOverlay.gameObject);
            evolutionOverlay=null;
            if(!battleHudClosed&&skillPanel!=null)skillPanel.gameObject.SetActive(true);
        }
    }
}
