using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameMenuItem {void SetVisible(bool visible);}
    // UIControl.OpenAllMenuItemUI state machine32741. Shared WaitForEndOfFrame from UIControl ctor.
    public sealed class OutgameMenuItems
    {
        public IOutgameMenuItem Main,Shop,Commander,LegacyPage24,LegacyPage40,ItemInfo;
        readonly Func<string,bool,IOutgameMenuItem> showPage;
        readonly Func<IOutgameMenuItem> showItemInfo;
        readonly Func<WaitForEndOfFrame,Task> wait;
        readonly WaitForEndOfFrame frame=new WaitForEndOfFrame();
        public OutgameMenuItems(Func<string,bool,IOutgameMenuItem> showPage,Func<IOutgameMenuItem> showItemInfo,Func<WaitForEndOfFrame,Task> wait=null)
        {this.showPage=showPage;this.showItemInfo=showItemInfo;this.wait=wait??WaitFrame;}
        static async Task WaitFrame(WaitForEndOfFrame instruction){await OutgameUnityAwait.Await(instruction);}
        public async void OpenAllMenuItemUI(){await OpenAsync();}
        public async Task OpenAsync()
        {
            if(Main==null){Main=showPage("Proj_xqzdStartUI",false);await wait(frame);}else Main.SetVisible(true);
            if(Shop==null){Shop=showPage("ShopUI",false);await wait(frame);}else Shop.SetVisible(true);
            if(Commander==null){Commander=showPage("CommanderUI",false);await wait(frame);}else Commander.SetVisible(true);
            if(ItemInfo==null)ItemInfo=showItemInfo();
        }
        public void CloseAllMenuItemUI()
        {Main?.SetVisible(false);LegacyPage24?.SetVisible(false);Shop?.SetVisible(false);Commander?.SetVisible(false);LegacyPage40?.SetVisible(false);}
    }
}
