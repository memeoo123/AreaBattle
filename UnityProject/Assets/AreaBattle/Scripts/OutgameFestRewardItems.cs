using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // FestActUI34024. Image setter is the existing UIExtension asset-loading boundary.
    public sealed class OutgameFestRewardItems
    {
        [Serializable] sealed class Rewards {public Reward[] Datas;}
        [Serializable] sealed class Reward {public int id;public int[] itemId,itemCount;}
        [Serializable] sealed class Items {public Item[] Datas;}
        [Serializable] sealed class Item {public int id;public string ItemIcon;}
        readonly Transform gifts,skins;readonly OutgameFestActManager manager;
        readonly Func<string> rewards,items;readonly Func<string,string> language;
        readonly Action<Image,string,string> setSprite;readonly Func<int,(string icon,string atlas)> awardImage;
        public OutgameFestRewardItems(Transform page,OutgameFestActManager manager,Func<string> rewards,Func<string> items,
            Func<string,string> language,Action<Image,string,string> setSprite,Func<int,(string icon,string atlas)> awardImage)
        {
            gifts=page.Find("go_Main/Area1/Main/ItemGifts");skins=page.Find("go_Main/Area2/Main/SkinGifts");
            this.manager=manager;this.rewards=rewards;this.items=items;this.language=language;this.setSprite=setSprite;this.awardImage=awardImage;
        }
        public void Refresh()
        {
            var row=Array.Find(JsonUtility.FromJson<Rewards>(rewards()).Datas,x=>x.id==1);
            for(int i=0;i<row.itemId.Length;i++)
            {
                var gift=gifts.GetChild(i);var item=Array.Find(JsonUtility.FromJson<Items>(items()).Datas,x=>x.id==row.itemId[i]);
                setSprite(gift.GetChild(1).GetComponent<Image>(),item.ItemIcon,"PublicIcon");
                gift.GetChild(2).GetComponent<Text>().text=string.Format(language("ValentineUI.DayIndex"),i+1);
                gift.GetChild(3).GetChild(0).GetComponent<Text>().text=string.Format("X{0}",row.itemCount[i]);
                gift.GetComponent<Image>().enabled=manager.Data.rewardId==i+1;
                gift.GetChild(5).gameObject.SetActive(manager.Data.rewardId>i+1);
                gift.GetChild(4).gameObject.SetActive(manager.Data.rewardId>i+1);
            }
            var first=awardImage(4);setSprite(skins.GetChild(0).GetChild(0).GetComponent<Image>(),first.icon,first.atlas);
            var second=awardImage(5);setSprite(skins.GetChild(1).GetChild(0).GetComponent<Image>(),second.icon,second.atlas);
        }
    }
}
