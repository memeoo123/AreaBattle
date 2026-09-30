using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameCommanderView:MonoBehaviour
    {
        [Serializable] sealed class ArtManifest {public Art[] sprites;}
        [Serializable] sealed class Art {public string id,name;}
        [Serializable] sealed class ItemRows {public Item[] Datas;}
        [Serializable] sealed class Item {public int id;public string ItemIcon;}
        readonly Dictionary<int,string> itemIcons=new Dictionary<int,string>();
        readonly Dictionary<int,Transform> cards=new Dictionary<int,Transform>();
        readonly Dictionary<string,Sprite> icons=new Dictionary<string,Sprite>();
        OutgameProfile profile;OutgameCommanderProgression rules;OutgameCommanderActions actions;
        OutgameCommanderSkillCards skillCards;OutgameSkillDescription skillDescriptions;Func<int> realLevel;
        int selectedSkillSlot=-1;
        Func<int> currentLevel;Func<int,int> count;Func<string,string> language;
        const string Info="objCommander/objCommanderInfo/";
        public event Action<OutgameCommanderActionResult,int> ActionCompleted;
        public Transform Card(int id)=>cards[id];
        public void Bind(OutgameProfile held,OutgameCommanderProgression progression,OutgameCommanderActions controller,Func<int> level,Func<int,int> inventoryCount,Func<string,string> localize,int selectedId,Func<int> realLevel)
        {
            if(profile!=null)throw new InvalidOperationException("Commander view already bound");
            profile=held;this.realLevel=realLevel;rules=progression;actions=controller;currentLevel=level;count=inventoryCount;language=localize;
            var art=JsonUtility.FromJson<ArtManifest>(Resources.Load<TextAsset>("Recovered/Outgame/hud-import").text);
            foreach(var row in art.sprites)if(!icons.ContainsKey(row.name))icons.Add(row.name,Resources.Load<Sprite>("Recovered/Outgame/Sprites/"+row.id.Replace(':','_').Replace('/','_')));
            foreach(var item in JsonUtility.FromJson<ItemRows>(Resources.Load<TextAsset>("Data/Outgame/GameItemConfig").text).Datas)itemIcons.Add(item.id,item.ItemIcon);
            var template=transform.Find("objCommanderList/scvCommander/CommanderItem");var content=transform.Find("objCommanderList/scvCommander/Viewport/CommanderContent");
            template.gameObject.SetActive(false);
            foreach(var state in held.commanders)
            {
                int id=state.id;if(!rules.HasConfig(id))continue;var config=rules.Config(id);var card=Instantiate(template,content,false);card.name="CommanderItem_"+id;card.gameObject.SetActive(true);cards.Add(id,card);
                card.Find("imgIcon").GetComponent<Image>().sprite=icons[config.IconName];
                var button=card.gameObject.AddComponent<Button>();button.targetGraphic=card.Find("imgBg").GetComponent<Image>();
                button.onClick.AddListener(()=>{actions.Select(id);Refresh();});
            }
            var upgrade=transform.Find(Info+"objUpgrade/btnUpgrade");var upgradeButton=upgrade.GetComponent<Button>()??upgrade.gameObject.AddComponent<Button>();
            upgradeButton.onClick.AddListener(()=>{var result=actions.ClickUpgrade(currentLevel(),out int slot);if(result==OutgameCommanderActionResult.Success)CloseSkillDetail();Refresh();ActionCompleted?.Invoke(result,slot);});
            skillCards=gameObject.AddComponent<OutgameCommanderSkillCards>();skillCards.Bind(rules,realLevel,language,icons,itemIcons);
            skillDescriptions=new OutgameSkillDescription(Resources.Load<TextAsset>("Data/AllSkillConfig").text,Resources.Load<TextAsset>("Data/SkillConfig").text,language);
            skillCards.SkillSelected+=ShowSkillDetail;
            CloseSkillDetail();actions.Select(selectedId);Refresh();
        }
        public void ShowSkillDetail(int slot)
        {
            selectedSkillSlot=slot;Visible(Info+"objSkills/objSkillDetail",true);RefreshSkillDetail(slot);
        }
        void RefreshSkillDetail(int slot)
        {
            var state=profile.commanders.Find(c=>c.id==actions.SelectedId);int id=rules.SkillIds(state)[slot];
            string path=Info+"objSkills/objSkillDetail/";
            transform.Find(path+"textSkillName").GetComponent<Text>().text=skillDescriptions.Name(id);
            transform.Find(path+"textSkillDetail").GetComponent<Text>().text=skillDescriptions.Description(id,state.skillLevels[slot]);
            var tip=transform.Find(path+"textSkillTips");tip.gameObject.SetActive(realLevel()<skillDescriptions.UnlockLevel(id));
            tip.GetComponent<Text>().text=string.Format(language("CommanderUI.UnlockLevel"),skillDescriptions.UnlockLevel(id));
            for(int i=0;i<state.skillLevels.Length;i++)skillCards.Card(i).Find("imgSelected").gameObject.SetActive(i==slot);
            var backgrounds=transform.Find(path+"objDetailBg");for(int i=0;i<backgrounds.childCount;i++)backgrounds.GetChild(i).gameObject.SetActive(i==slot);
            var data=transform.Find(path+"objSkillData");data.gameObject.SetActive(true);
            data.Find("ImgData1/textTarget").GetComponent<Text>().text=skillDescriptions.Target(id);
            data.Find("ImgData2/textTouchType").GetComponent<Text>().text=skillDescriptions.Operating(id);
            string[] rows={"ImgData3/textDuration","ImgData6/textSkillNum","ImgData4/textFireNum","ImgData7/textSlowPercent","ImgData5/textEffectPercent"};
            var comparison=skillDescriptions.Comparison(id,state.skillLevels[slot]);
            for(int i=0;i<rows.Length;i++)
            {
                var text=data.Find(rows[i]).GetComponent<Text>();bool present=comparison.TryGetValue(i+1,out string value);
                text.transform.parent.gameObject.SetActive(present);if(present)text.text=value;
            }
        }
        public void CloseSkillDetail()=>Visible(Info+"objSkills/objSkillDetail",false);
        void OnDisable(){if(profile!=null)CloseSkillDetail();}
        void RenderPrice(int slot,int id,int amount,bool showOwnedForEntity)
        {
            string path=Info+"objUpgrade/goUpgrade/";
            var icon=transform.Find(path+"imgIcon"+slot).GetComponent<Image>();icon.sprite=icons[itemIcons[id]];
            icon.transform.localScale=Vector3.one*(showOwnedForEntity&&id>=8001?0.5f:1f);
            transform.Find(path+"textNum"+slot).GetComponent<Text>().text=showOwnedForEntity&&id>=8001?count(id)+"/"+amount:amount.ToString();
        }
        void Visible(string path,bool value)=>transform.Find(path).gameObject.SetActive(value);
        public void Refresh()
        {
            foreach(var state in profile.commanders)
            {
                if(!cards.TryGetValue(state.id,out var card))continue;bool unlocked=rules.IsUnlocked(state),selected=actions.SelectedId==state.id;
                card.Find("objLock").gameObject.SetActive(!unlocked);card.Find("objUnlock").gameObject.SetActive(unlocked);
                card.Find("objUnlock/imgUsed").gameObject.SetActive(profile.usedCommanderId==state.id);
                card.Find("objUnlock/textLv").GetComponent<Text>().text=language("CommanderUI.Level")+state.level;
                card.Find("imgSelected").gameObject.SetActive(selected);
                bool red=!selected&&!rules.IsMax(state)&&rules.IsLevelUnlockable(state,currentLevel())&&rules.Config(state.id).unlockType!=2&&(unlocked?rules.CanAffordUpgrade(state,count):rules.CanAffordUnlock(state,count));
                card.Find("imgRedDot").gameObject.SetActive(red);
            }
            var selectedState=profile.commanders.Find(c=>c.id==actions.SelectedId);if(selectedState==null)return;
            skillCards.Refresh(selectedState);
            bool owned=rules.IsUnlocked(selectedState),max=rules.IsMax(selectedState),purchaseOnly=!owned&&rules.Config(selectedState.id).unlockType==2;
            transform.Find(Info+"imgTitle/textCommanderName").GetComponent<Text>().text=language(rules.Config(selectedState.id).name);
            Visible(Info+"objUpgrade/btnUpgrade",!max&&!purchaseOnly);Visible(Info+"objUpgrade/btnNoUpgrade",max||purchaseOnly);
            bool redUpgrade=owned?rules.CanAffordUpgrade(selectedState,count)&&!max:rules.CanAffordUnlock(selectedState,count)&&rules.IsLevelUnlockable(selectedState,currentLevel());
            Visible(Info+"objUpgrade/btnUpgrade/goUpgradeReddot",redUpgrade&&!purchaseOnly);
            transform.Find(Info+"imgBg/textLv").GetComponent<Text>().text=language("CommanderUI.Level")+selectedState.level;
            Visible(Info+"objUpgrade/textUnlockName",!owned);Visible(Info+"objUpgrade/textUpgradeName",owned&&!max);Visible(Info+"objUpgrade/textMaxLevel",max);
            Visible(Info+"objUpgrade/goUpgrade",!max&&!purchaseOnly);
            var prices=rules.Config(selectedState.id).unlockPrice;
            if(!max&&!purchaseOnly)
            {
                RenderPrice(1,prices[0].datas[0],owned?rules.NextCost(selectedState):prices[0].datas[1],true);
                bool second=!owned&&prices.Length>1;Visible(Info+"objUpgrade/goUpgrade/imgIcon2",second);Visible(Info+"objUpgrade/goUpgrade/textNum2",second);
                if(second)RenderPrice(2,prices[1].datas[0],prices[1].datas[1],false);
            }
            if(selectedSkillSlot>=0)RefreshSkillDetail(selectedSkillSlot);
            // 3D model and acquisition descriptions remain separate source bindings.

        }
    }
}
