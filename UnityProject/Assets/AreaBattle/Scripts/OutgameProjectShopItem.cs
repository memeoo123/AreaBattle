using System;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameProjectShopReward {public int[] datas;}
    [Serializable] public sealed class OutgameProjectProduct
    {
        public int id,isActive,storeType,NoAds,getType;
        public string icon,price;
        public OutgameProjectShopReward[] itemConfig;
    }
    [Serializable] public sealed class OutgameProjectProductTable
    {public OutgameProjectProduct[] Datas;public static OutgameProjectProductTable Read(string json)=>JsonUtility.FromJson<OutgameProjectProductTable>(json);}
    public interface IOutgameProjectShopItemView
    {
        void SetActive(bool active);
        void SetIcon(string icon,string atlas);
        void SetPriceText(string text);
        void SetQuantityText(string text);
        void PlayVoice(int group,int id);
        void ReportToolGet(int item,int category,int amount,int balance,string reason);
        void Refresh();
    }
    // Proj_hdzd.ShopItem33336/33333/33337; separate from ItemModule price machinery.
    public sealed class OutgameProjectShopItem
    {
        readonly OutgameToolDispatcher tools;readonly OutgameLocalInventory inventory;
        readonly IOutgameProjectShopItemView view;readonly Func<string> coinCostReason;
        public OutgameProjectProduct Data {get;private set;}
        public int ItemId {get;private set;}
        public int Amount {get;private set;}
        public int Price {get;private set;}
        public OutgameProjectShopItem(OutgameToolDispatcher tools,OutgameLocalInventory inventory,IOutgameProjectShopItemView view,Func<string> coinCostReason)
        {this.tools=tools;this.inventory=inventory;this.view=view;this.coinCostReason=coinCostReason;}
        public void SetData(OutgameProjectProduct data)
        {
            Data=data;ItemId=data.itemConfig[0].datas[0];Amount=data.itemConfig[0].datas[1];
            view.SetActive(true);view.SetIcon(data.icon,"ShopUI");view.SetPriceText(data.price);
            Price=int.Parse(data.price);view.SetQuantityText("x"+Amount);
        }
        public void Click()
        {
            if(tools.Change(1002,unchecked(-Price),true,"",true))
            {
                int item=ItemId;
                tools.Change(item,Amount,true,"",true);
                view.PlayVoice(1,2017);
                view.ReportToolGet(item,1,Amount,inventory.Count(item),coinCostReason());
                view.Refresh();
            }
        }
    }
}
