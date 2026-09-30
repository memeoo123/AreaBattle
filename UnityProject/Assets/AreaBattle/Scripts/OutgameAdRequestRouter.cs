using System;
namespace AreaBattle
{
    public interface IOutgameAdController
    {
        bool IsReady();
        bool SourceFlag20 {get;set;}
        void Close();
        void Show(Action<bool> shown,Action<bool> closed);
    }
    // NewAdsManager.showVideo f7513, fixNoClose f7518, isVideoReady f5866.
    public sealed class OutgameAdRequestRouter
    {
        readonly Func<string> channel;
        readonly Action<string> log;
        readonly string logPrefix;
        public IOutgameAdController VideoController,InterstitialController;
        public bool InterVideoShowing,SourceFlag64;
        public OutgameAdRequestRouter(Func<string> channel,string logPrefix,Action<string> log)
        {this.channel=channel??throw new ArgumentNullException(nameof(channel));this.logPrefix=logPrefix;this.log=log??throw new ArgumentNullException(nameof(log));}
        public bool IsVideoReady(){return VideoController!=null&&VideoController.IsReady();}
        public void FixNoClose()
        {
            if((channel()=="xiaomi"||channel()=="weixin")&&InterVideoShowing)
            {
                log(logPrefix+" fixNoClose");
                if(VideoController!=null)VideoController.Close();
                if(InterstitialController!=null)InterstitialController.Close();
            }
        }
        public void ShowVideo(Action<bool> shown,Action<bool> closed,bool sourceOption=false)
        {
            log(logPrefix+" showVideo ");FixNoClose();
            if((channel()=="alipay"||channel()=="zijie")&&InterVideoShowing)
            {log(logPrefix+" fix inter no close");if(InterstitialController!=null)InterstitialController.Close();}
            if(InterVideoShowing){log(logPrefix+" showVideo InterVideoShow return");closed?.Invoke(false);return;}
            if(VideoController==null){log(logPrefix+" showVideo videoController == null");shown?.Invoke(false);return;}
            VideoController.SourceFlag20=sourceOption;VideoController.Show(shown,closed);
        }
    }
}
