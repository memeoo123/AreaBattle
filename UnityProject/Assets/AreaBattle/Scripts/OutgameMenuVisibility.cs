namespace AreaBattle
{
    public interface IOutgameMenuVisibilityHost
    {
        bool Visible {get;}
        void ApplyBaseVisibility(bool visible);
        void SetVisibility(bool visible);
        void RequestMainPage();void CloseAllMenuItems();void OpenAllMenuItems();
        int CurrentPage {set;}
        void CheckStartPanels();void RefreshCommanderLockAndRedDots();void RefreshSourceLevelGate();void RefreshNewSkins();
        void SendMenuTabDispose();
    }
    // MenuTabUI.DoClose33469 / VisibleImp33482. SetVisibility must dispatch the virtual VisibleImp path.
    public sealed class OutgameMenuVisibility
    {
        readonly IOutgameMenuVisibilityHost host;
        public OutgameMenuVisibility(IOutgameMenuVisibilityHost host){this.host=host;}
        public void DoClose()
        {
            if(!host.Visible)return;
            host.RequestMainPage();host.CloseAllMenuItems();host.CurrentPage=0;host.SetVisibility(false);
        }
        public void VisibleImp(bool visible)
        {
            host.ApplyBaseVisibility(visible);
            if(!visible){host.SendMenuTabDispose();return;}
            host.OpenAllMenuItems();host.RequestMainPage();host.CheckStartPanels();
            host.RefreshCommanderLockAndRedDots();host.RefreshSourceLevelGate();host.RefreshNewSkins();
        }
    }
}
