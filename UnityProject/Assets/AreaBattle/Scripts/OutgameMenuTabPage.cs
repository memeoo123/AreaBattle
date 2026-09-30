using System;
using UnityEngine;
namespace AreaBattle
{
    // Native MenuTabUI visibility/closure adapter: original33482/33469.
    // Page-specific data refresh handlers are required from the recovered menu owner.
    public sealed class OutgameMenuTabPage:OutgameUiPage,IOutgameMenuTabPage,IOutgameMenuVisibilityHost
    {
        readonly OutgameMenuVisibility visibility;
        readonly Func<OutgameUiControl> control;
        readonly OutgameMenuItems items;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Action requestMain,checkStart,refreshCommander,refreshLevel,refreshSkins;
        public OutgameMenuTabPage(GameObject root,Func<OutgameUiControl> control,OutgameMenuItems items,Func<OutgameMessageDispatcher> messages,Action requestMain,Action checkStart,Action refreshCommander,Action refreshLevel,Action refreshSkins):base(root,messages)
        {
            this.control=control??throw new ArgumentNullException(nameof(control));
            this.items=items??throw new ArgumentNullException(nameof(items));
            this.messages=messages??throw new ArgumentNullException(nameof(messages));
            this.requestMain=requestMain??throw new ArgumentNullException(nameof(requestMain));
            this.checkStart=checkStart??throw new ArgumentNullException(nameof(checkStart));
            this.refreshCommander=refreshCommander??throw new ArgumentNullException(nameof(refreshCommander));
            this.refreshLevel=refreshLevel??throw new ArgumentNullException(nameof(refreshLevel));
            this.refreshSkins=refreshSkins??throw new ArgumentNullException(nameof(refreshSkins));
            visibility=new OutgameMenuVisibility(this);
        }
        public void DoClose()=>visibility.DoClose();
        protected override void VisibleImp(bool visible)=>visibility.VisibleImp(visible);
        void IOutgameMenuVisibilityHost.ApplyBaseVisibility(bool visible)=>base.VisibleImp(visible);
        void IOutgameMenuVisibilityHost.SetVisibility(bool visible)=>SetVisible(visible);
        void IOutgameMenuVisibilityHost.RequestMainPage()=>requestMain();
        void IOutgameMenuVisibilityHost.CloseAllMenuItems()=>items.CloseAllMenuItemUI();
        void IOutgameMenuVisibilityHost.OpenAllMenuItems()=>items.OpenAllMenuItemUI();
        int IOutgameMenuVisibilityHost.CurrentPage {set{control().CurrentPage=value;}}
        void IOutgameMenuVisibilityHost.CheckStartPanels()=>checkStart();
        void IOutgameMenuVisibilityHost.RefreshCommanderLockAndRedDots()=>refreshCommander();
        void IOutgameMenuVisibilityHost.RefreshSourceLevelGate()=>refreshLevel();
        void IOutgameMenuVisibilityHost.RefreshNewSkins()=>refreshSkins();
        void IOutgameMenuVisibilityHost.SendMenuTabDispose()=>messages().SendMessage("MenuTabDispose",null);
    }
}
