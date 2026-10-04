using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameSevendayEntryServices
    {
        public Func<OutgameSevendayActivityControl> Control;
        public Func<OutgameUiOpenRegistry<OutgameUiPage>> Pages;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Action<int,int> PlayVoice;
    }
    // Proj_xqzdStartUI4399 seven-day portion of Awake/Dispose and complete methods
    // 33722(init),33704(click),33695(red),33690(visibility). Full main page owns lifecycle.
    public sealed class OutgameSevendayEntryBinding:IDisposable
    {
        readonly OutgameSevendayEntryServices services;
        public Button Button;
        public GameObject RedDot;
        public OutgameSevendayEntryBinding(Transform main,OutgameSevendayEntryServices services)
        {
            this.services=services;Button=main.Find("RigthBar/btn_sevenDay").GetComponent<Button>();
            RedDot=Button.transform.Find("imgReddot").gameObject;
        }
        public void Initialize()
        {
            bool unlocked=services.Control().IsUnlock();
            if(unlocked)
            {
                OutgameUiClick.Add(Button,Click,services.Messages);
                services.Messages().AddListener("SevendayUnlock",RefreshRed);
                services.Messages().AddListener("SevendayFinishTask",RefreshRed);
                services.Messages().AddListener("SevendayGetAccReward",RefreshRed);
                services.Messages().AddListener("SevendayClose",RefreshVisibility);
                RefreshRed(Array.Empty<object>());
            }
            Button.gameObject.SetActive(unlocked);
        }
        void Click()
        {
            services.PlayVoice(1,2001);
            services.Pages().Open(OutgameLimitTaskPage.SourceNamespace,OutgameLimitTaskPage.SourceName,Array.Empty<object>());
        }
        public void RefreshRed(object[] args)
        {
            var dot=RedDot;
            // Source checks the managed field, not Unity Object.op_Equality.
            if(!ReferenceEquals(dot,null))dot.SetActive(services.Control().IsHaveAnyRed());
        }
        public void RefreshVisibility(object[] args)
        {bool unlocked=services.Control().IsUnlock();Button.gameObject.SetActive(unlocked);}
        public void Dispose()
        {
            // Original Dispose33692 tests CURRENT unlock, not a remembered registration flag.
            if(!services.Control().IsUnlock())return;
            services.Messages().RemoveListener("SevendayUnlock",RefreshRed);
            services.Messages().RemoveListener("SevendayFinishTask",RefreshRed);
            services.Messages().RemoveListener("SevendayGetAccReward",RefreshRed);
            services.Messages().RemoveListener("SevendayClose",RefreshVisibility);
        }
    }
}
