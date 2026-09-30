using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // FestActUI source methods34029/34028/34032; bindings in Awake34026.
    public sealed class OutgameFestDailyRewardBinding
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;public int[] itemId,itemCount;}
        readonly OutgameFestActManager manager;readonly Func<string> config;readonly Action refreshState,refreshItems;
        public OutgameFestDailyRewardBinding(Transform page,OutgameFestActManager manager,Func<string> config,Action refreshState,Action refreshItems)
        {
            this.manager=manager;this.config=config;this.refreshState=refreshState;this.refreshItems=refreshItems;
            page.Find("go_Main/Area1/Main/Claim").GetComponent<Button>().onClick.AddListener(ClaimNormal);
            page.Find("go_Main/Area1/Main/ClaimDouble").GetComponent<OutgameVideoButton>().AddVideoPlayCallBack(ClaimVideo);
        }
        void ClaimNormal(){if(manager.CanGetTodayReward())Claim(1);}
        void ClaimVideo(bool success){if(success&&manager.CanGetTodayReward())Claim(2);}
        void Claim(int multiplier)
        {
            var row=Array.Find(JsonUtility.FromJson<Rows>(config()).Datas,x=>x.id==1);
            int id=row.itemId[manager.Data.rewardId-1];int amount=unchecked(multiplier*row.itemCount[manager.Data.rewardId-1]);
            manager.GetTodayReward(id,amount);refreshState();refreshItems();
        }
    }
    // FestActUI34031. The first-row mask/claim references in the completed branch are preserved.
    public sealed class OutgameFestRewardVisibility
    {
        readonly Transform page,gifts;readonly OutgameFestActManager manager;readonly Func<string,string> language;
        public OutgameFestRewardVisibility(Transform page,OutgameFestActManager manager,Func<string,string> language)
        {this.page=page;this.manager=manager;this.language=language;gifts=page.Find("go_Main/Area2/Main/SkinGifts");}
        public void Refresh()
        {
            page.Find("go_Main/Area1/Main/Claim").gameObject.SetActive(manager.CanGetTodayReward());
            page.Find("go_Main/Area1/Main/ClaimDouble").gameObject.SetActive(manager.CanGetTodayReward());
            page.Find("go_Main/Area1/Main/Claimed").gameObject.SetActive(!manager.CanGetTodayReward());
            for(int i=0;i<manager.LimitSkinCount;i++)
            {
                var row=gifts.GetChild(i);
                if(((uint)manager.Data.limetSkinStatus>>i&1)!=0)
                {
                    row.GetChild(2).gameObject.SetActive(true);
                    gifts.GetChild(0).GetChild(3).gameObject.SetActive(false);
                    gifts.GetChild(0).GetChild(4).gameObject.SetActive(true);
                    gifts.GetChild(0).GetChild(5).gameObject.SetActive(true);
                }
                else row.GetChild(3).GetChild(0).GetComponent<Text>().text=language("ValentineUI.Play");
            }
        }
    }
}
