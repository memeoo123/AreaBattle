using System;
namespace AreaBattle
{
    // XYXADControl55276/55278 and closure55287: platform bool -> framework raw callback.
    public sealed class OutgameXyxVideoBridge
    {
        readonly Func<bool> isReady;
        readonly Action<Action<bool>> show;
        readonly Action<string> warning,afterVideo;
        public OutgameXyxVideoBridge(Func<bool> isReady,Action<Action<bool>> show,Action<string> warning,Action<string> afterVideo)
        {this.isReady=isReady;this.show=show;this.warning=warning;this.afterVideo=afterVideo;}
        // Bridge_WX_XYXFunction64920/64921 -> WXFunctionManger AdsIsReady/ShowVideo.
        public OutgameXyxVideoBridge(OutgameAdModuleEntry wxEntry,Action<string> warning,Action<string> afterVideo)
            :this(wxEntry.IsRewardAdReady,wxEntry.ShowRewardAd,warning,afterVideo){}
        public bool IsVideoReadyStatic()=>isReady();
        public void ShowVideoStatic(int videoFlag)
        {
            warning("ShowVideoStatic:"+videoFlag);
            show(success=>afterVideo(string.Format("{{\"videoFlag\":\"{0}\",\"result\":\"{1}\"}}",videoFlag,success?0:1)));
        }
    }
}
