using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameTaskEntryServices
    {
        public Func<OutgameTaskControl> Control;
        public Func<OutgameUiOpenRegistry<OutgameUiPage>> Pages;
        public Func<OutgameUiPage> FindPage;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Action<int,int> PlayVoice;
    }
    // StartUI33679 task portion and closure33726; GameDefine.ShowUI32617 with visible=true.
    public sealed class OutgameTaskEntryBinding
    {
        readonly OutgameTaskEntryServices services;
        public readonly Button Button;
        public OutgameTaskEntryBinding(Transform main,OutgameTaskEntryServices services)
        {this.services=services;Button=main.Find("LeftBar/taskBtn").GetComponent<Button>();}
        public void Initialize(){OutgameUiClick.Add(Button,Click,services.Messages);services.Control().SetRedDot();}
        void Click()
        {
            services.PlayVoice(1,2001);var page=services.FindPage();
            if(page==null){services.Pages().Open(OutgameTaskPage.SourceNamespace,OutgameTaskPage.SourceName,Array.Empty<object>());return;}
            if(!page.Visible)page.SetVisible(true);
        }
    }
}
