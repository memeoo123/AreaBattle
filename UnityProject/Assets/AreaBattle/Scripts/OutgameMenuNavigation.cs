using System;
namespace AreaBattle
{
    public enum OutgameMenuPage { None=0, Skins=2, Main=3, Commander=4 }
    // Runtime UIControl routes, not the legacy MenuTabConfig uiName/isActive list.
    public sealed class OutgameMenuNavigation
    {
        public const float TransitionSeconds=0.3f;
        public OutgameMenuPage Current { get; private set; }
        public bool CanSwitch { get; private set; }
        public event Action<OutgameMenuPage> FirstPageShown;
        // Incoming page starts at direction*width; outgoing page moves to -direction*width.
        public event Action<OutgameMenuPage,OutgameMenuPage,int,float> TransitionStarted;
        public event Action<OutgameMenuPage> TransitionFinished;
        public static string SourceUI(OutgameMenuPage page)
        {
            switch(page){case OutgameMenuPage.Skins:return "ShopUI";case OutgameMenuPage.Main:return "Proj_xqzdStartUI";case OutgameMenuPage.Commander:return "CommanderUI";default:throw new ArgumentOutOfRangeException(nameof(page));}
        }
        public bool CheckUI(OutgameMenuPage page)
        {
            SourceUI(page);
            if(Current!=OutgameMenuPage.None&&(!CanSwitch||Current==page))return false;
            if(Current==OutgameMenuPage.None)
            {
                Current=page;CanSwitch=true;FirstPageShown?.Invoke(page);return true;
            }
            var previous=Current;CanSwitch=false;Current=page;
            TransitionStarted?.Invoke(previous,page,(int)page<(int)previous?-1:1,TransitionSeconds);
            return true;
        }
        // Called by the completed tween, after the incoming page is detached from the outgoing parent.
        public void CompleteTransition()
        {
            if(CanSwitch||Current==OutgameMenuPage.None)return;
            CanSwitch=true;TransitionFinished?.Invoke(Current);
        }
        public bool ReturnToMain()=>CheckUI(OutgameMenuPage.Main);
        // MenuTabUI uses commander1.unlockLevel for the lock overlay (not the first owned commander).
        public static bool CommanderLocked(int currentLevel,OutgameCommanderProgression rules)=>currentLevel<rules.Config(1).unlockLevel;
    }
}
