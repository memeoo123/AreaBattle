using System;
namespace AreaBattle
{
    // Source MenuTabUI f5890/f11385: MoveCamera only after CheckUI succeeds.
    public sealed class OutgameMenuSceneBinding : IDisposable
    {
        readonly OutgameMenuView menu;
        readonly OutgameScenePresentation presentation;
        public OutgameMenuSceneBinding(OutgameMenuView menu,OutgameModelRoots roots)
        {
            this.menu=menu!=null?menu:throw new ArgumentNullException(nameof(menu));
            presentation=new OutgameScenePresentation(roots);
            menu.PageRequestAccepted+=Accepted;
        }
        void Accepted(OutgameMenuPage page)
        {
            if(page==OutgameMenuPage.Skins)presentation.MoveCamera(true,.5f);
            else if(page==OutgameMenuPage.Main)presentation.MoveCamera(false,0);
        }
        public void Dispose(){menu.PageRequestAccepted-=Accepted;}
    }
}
