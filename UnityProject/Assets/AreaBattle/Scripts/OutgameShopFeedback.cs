using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameShopCurrencyEffects
    {
        void FlyMoney(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue);
        void FlyDiamonds(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue);
    }
    // ShopUI.Refresh33778, OnToolChange33788 and video completions33801/33779.
    public sealed class OutgameShopFeedback:MonoBehaviour
    {
        OutgameLocalInventory inventory;Func<string,string> language;IOutgameShopCurrencyEffects effects;
        public void Bind(OutgameLocalInventory inventory,Func<string,string> language,IOutgameShopCurrencyEffects effects)
        {this.inventory=inventory;this.language=language;this.effects=effects;}
        public void Refresh()
        {
            transform.Find("bottom/skinGroup_Shop/Viewport/Content/Shop_Diamond/shopInfo/toolValueNum/txt_toolValue").GetComponent<Text>().text=
                language("ShopUI.toolvalue1")+inventory.Count(1005).ToString();
        }
        public void OnToolChange(object[] args)=>Refresh();
        public void GoldVideoCompleted(bool success)
        {if(success)effects.FlyMoney(50,transform.Find("effectRoot"),transform.Find("btn_addGold").position,true,null,true);}
        public void DiamondVideoCompleted(bool success)
        {if(success)effects.FlyDiamonds(20,transform.Find("effectRoot"),transform.Find("btn_addDiamond").position,true,null,true);}
    }
}
