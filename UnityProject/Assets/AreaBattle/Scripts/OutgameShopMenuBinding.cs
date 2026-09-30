using System;
namespace AreaBattle
{
    // Connect before MenuView initialization activates pages, after ShopSkinLists.Build has bound cards.
    public sealed class OutgameShopMenuBinding:IDisposable
    {
        readonly OutgameMenuView menu;
        readonly OutgameShopSkinLists cards;
        readonly Action<int> initializeFirstAdsItem;
        public OutgameShopMenuBinding(OutgameMenuView menu,OutgameShopSkinLists cards,Action<int> initializeFirstAdsItem)
        {
            this.menu=menu?menu:throw new ArgumentNullException(nameof(menu));
            this.cards=cards?cards:throw new ArgumentNullException(nameof(cards));
            this.initializeFirstAdsItem=initializeFirstAdsItem??throw new ArgumentNullException(nameof(initializeFirstAdsItem));
            menu.PageRefreshRequested+=Refresh;
        }
        void Refresh(OutgameMenuPage page){if(page==OutgameMenuPage.Skins)cards.RefreshSkinStatus(initializeFirstAdsItem);}
        public void Dispose(){menu.PageRefreshRequested-=Refresh;}
    }
}
