using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameCommanderSkillCards:MonoBehaviour
    {
        [Serializable] sealed class Table{public Row[] Datas;}
        [Serializable] sealed class Row{public int id,unLockLevel;}
        readonly Dictionary<int,int> unlockLevels=new Dictionary<int,int>();
        readonly List<Transform> cards=new List<Transform>();
        OutgameCommanderProgression rules;Func<int> realLevel;Func<string,string> language;
        Dictionary<string,Sprite> icons;Dictionary<int,string> itemIcons;Transform template,content;int commanderId;
        public event Action<int> SkillSelected;
        public Transform Card(int index)=>cards[index];
        public void Bind(OutgameCommanderProgression progression,Func<int> sourceRealLevel,Func<string,string> localize,Dictionary<string,Sprite> sprites,Dictionary<int,string> sourceItemIcons)
        {
            rules=progression;realLevel=sourceRealLevel??throw new ArgumentNullException(nameof(sourceRealLevel));language=localize;icons=sprites;itemIcons=sourceItemIcons;
            foreach(var row in JsonUtility.FromJson<Table>(Resources.Load<TextAsset>("Data/AllSkillConfig").text).Datas)unlockLevels.Add(row.id,row.unLockLevel);
            template=transform.Find("objCommander/objCommanderInfo/objSkills/scvSkillList/CommanderSkillItem");
            content=transform.Find("objCommander/objCommanderInfo/objSkills/scvSkillList/Viewport/objSkillContent");template.gameObject.SetActive(false);
        }
        public void Refresh(OutgameCommanderState state)
        {
            int[] skills=rules.SkillIds(state);
            if(state.id!=commanderId)
            {
                foreach(var old in cards){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}cards.Clear();commanderId=state.id;
                for(int i=0;i<skills.Length;i++)
                {
                    int slot=i;var card=Instantiate(template,content,false);card.name="CommanderSkillItem_"+skills[i];card.gameObject.SetActive(true);cards.Add(card);
                    card.Find("imgSkillIcon").GetComponent<Image>().sprite=icons[itemIcons[skills[i]+2000]];
                    card.Find("imgSelected").gameObject.SetActive(false);
                    var button=card.gameObject.AddComponent<Button>();button.targetGraphic=card.Find("Image").GetComponent<Image>();button.onClick.AddListener(()=>SkillSelected?.Invoke(slot));
                }
            }
            for(int i=0;i<skills.Length;i++)
            {
                var card=cards[i];int gate=unlockLevels.TryGetValue(skills[i],out int value)?value:0;bool unlocked=realLevel()>=gate;
                card.Find("imgLock").gameObject.SetActive(!unlocked);
                card.Find("imgLock/textLockLv").GetComponent<Text>().text=string.Format(language("CommanderUI.UnlockSkillLevel"),gate);
                card.Find("imgTextBg").gameObject.SetActive(unlocked);
                card.Find("imgTextBg/textSkillLv").GetComponent<Text>().text=language("CommanderUI.Level")+state.skillLevels[i];
                card.Find("imgUpgradeTip").gameObject.SetActive(state.level>0&&!rules.IsMax(state)&&i==(state.level-1)%3);
            }
        }
    }
}
