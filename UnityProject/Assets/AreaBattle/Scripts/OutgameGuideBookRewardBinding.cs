using System;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // Reward-button portion of GuideBookUI; full popup/list/tab ownership is separate.
    public sealed class OutgameGuideBookRewardBinding:MonoBehaviour,IOutgameGuideBookRewardPage
    {
        Func<OutgameGuideBookControl> control;Action<int> voice,refreshItem;
        public GuidebookConfig SelectedBook {get;set;}
        public int SelectedIndex {get;set;}
        public Transform Root=>transform;
        public Transform BookRewardButton=>transform.Find("guidePop/mainbg/btnBox");
        public OutgameGuideBookRewards Rewards {get;private set;}
        public void Bind(Func<OutgameGuideBookControl> control,Func<OutgameToolControl> tools,
            Func<IOutgameShopCurrencyEffects> effects,Action<int> voice,Action<int> refreshItem,Func<OutgameMessageDispatcher> messages=null)
        {
            this.control=control;this.voice=voice;this.refreshItem=refreshItem;
            Rewards=new OutgameGuideBookRewards(this,control,tools,effects);
            OutgameUiClick.Add(BookRewardButton.GetComponent<Button>(),Rewards.ClaimBook,messages??(()=>OutgameMessageDispatcher.Shared));
        }
        public void PlayVoice(int id)=>voice(id);
        public void RefreshBookItem(int index)=>refreshItem(index);
        public void SetBookRewardVisible(bool visible)=>BookRewardButton.gameObject.SetActive(visible);
        public void RefreshRedDots()
        {
            bool book=control().HaveAnyUnGetGuideReward();
            transform.Find("tabBtnGroup/tabBtnG/btnBookG0/imgRedG0").gameObject.SetActive(book);
            transform.Find("tabBtnGroup/tabBtnG/btnBookG1/imgRedG1").gameObject.SetActive(book);
            bool tip=control().HaveAnyUnGetTipReward();
            transform.Find("tabBtnGroup/tabBtnT/btnBookT0/imgRedT0").gameObject.SetActive(tip);
            transform.Find("tabBtnGroup/tabBtnT/btnBookT1/imgRedT1").gameObject.SetActive(tip);
        }
    }
}
