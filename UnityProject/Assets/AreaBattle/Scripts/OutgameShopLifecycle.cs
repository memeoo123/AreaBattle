using System;
namespace AreaBattle
{
    public interface IOutgameShopEventHost
    {
        void AddListener(string name,Action<object[]> callback);
        void RemoveListener(string name,Action<object[]> callback);
        void StopInitializationCoroutine(object coroutine);
        void EnsureSelectCardControl();
        void AddGoldVideoCallback(Action<bool> callback);
        void AddDiamondVideoCallback(Action<bool> callback);
    }
    // ShopUI.OpenLater33786 and Dispose33773. Do not map these to every Unity OnEnable/OnDisable.
    public sealed class OutgameShopLifecycle
    {
        readonly IOutgameShopEventHost host;
        public object InitializationCoroutine {get;set;}
        readonly Action<object[]> chooseSkin,chooseSoldier,refreshSkinState,toolChange;
        readonly Action<bool> goldVideo,diamondVideo;
        public OutgameShopLifecycle(IOutgameShopEventHost host,Action<object[]> chooseSkin,Action<object[]> chooseSoldier,
            Action<object[]> refreshSkinState,OutgameShopFeedback feedback)
        {
            this.host=host;this.chooseSkin=chooseSkin;this.chooseSoldier=chooseSoldier;this.refreshSkinState=refreshSkinState;
            toolChange=feedback.OnToolChange;goldVideo=feedback.GoldVideoCompleted;diamondVideo=feedback.DiamondVideoCompleted;
        }
        public void OpenLater()
        {
            host.AddListener("ChooseSkin",chooseSkin);
            host.AddListener("ChooseSoldier",chooseSoldier);
            host.AddListener("ValnetineStatueChanged",refreshSkinState);
            host.AddListener("ToolChange",toolChange);
            host.AddGoldVideoCallback(goldVideo);
            host.AddDiamondVideoCallback(diamondVideo);
        }
        public void Dispose()
        {
            host.RemoveListener("ChooseSkin",chooseSkin);
            host.RemoveListener("ChooseSoldier",chooseSoldier);
            host.RemoveListener("ValnetineStatueChanged",refreshSkinState);
            host.RemoveListener("ToolChange",toolChange);
            if(InitializationCoroutine!=null)host.StopInitializationCoroutine(InitializationCoroutine);
            host.EnsureSelectCardControl();
        }
    }
}
