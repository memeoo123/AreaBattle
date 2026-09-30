using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ShopUI coroutine33808 goods section: only storeType11, original enumeration order.
    // Page initialization/coroutine pacing belongs to the surrounding ShopUI lifecycle.
    public sealed class OutgameShopProductList:MonoBehaviour
    {
        readonly List<OutgameProjectShopItemView> cards=new List<OutgameProjectShopItemView>();
        public IReadOnlyList<OutgameProjectShopItemView> Cards=>cards;
        public void Build(IEnumerable<OutgameProjectProduct> products,Action<OutgameProjectShopItemView,OutgameProjectProduct> bind)
        {
            if(bind==null)throw new ArgumentNullException(nameof(bind));
            foreach(var card in cards){card.gameObject.SetActive(false);if(Application.isPlaying)Destroy(card.gameObject);else DestroyImmediate(card.gameObject);}cards.Clear();
            var template=transform.Find("bottom/ShopItem");template.gameObject.SetActive(false);
            var parent=transform.Find("bottom/skinGroup_Shop/Viewport/Content/Shop_Diamond/Content_Shop");
            foreach(var data in products)
            {
                if(data.storeType!=11)continue;
                var node=Instantiate(template,parent,false);node.name="ShopItem_"+data.id;
                var view=node.gameObject.AddComponent<OutgameProjectShopItemView>();
                bind(view,data);node.gameObject.SetActive(true);cards.Add(view);
            }
        }
    }
}
