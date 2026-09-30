using System;
namespace AreaBattle
{
    public interface IOutgameAdNotificationHost
    {
        void StopLoadTimeout();
        void StopShowTimeout();
        void ReportRequest();
        bool InterVideoShowing {get;}
        double GetPrice();
        void ReportShow(bool reportValue);
        void ReportNewAdShow();
        void ReportVideoComplete();
        void HandleAdsLevel(string key);
        void StartShow();
        int GetPlatformId();
        void ReportClick();
        void ReportNewClick(bool repeated);
        void ReportNewShowError(int code,string message);
        void ReportShowFail(int code,string message);
    }
    public sealed class OutgameAdListener
    {
        public Action<OutgameAdNotifyingAdapter> Received,Failed,Shown,Closed;
        public Action Rewarded;
        public Func<OutgameAdNotifyingAdapter,bool> ShowFailed;
    }
    // DAUAdsAdapter notifications share the actual adapter state and retain source reentrancy.
    public abstract class OutgameAdNotifyingAdapter:OutgameAdAdapterState
    {
        readonly IOutgameAdNotificationHost host;
        readonly Action<string> log;
        public OutgameAdListener Listener;
        public bool Clicked;
        public int BannerErrorReportEnable;
        protected OutgameAdNotifyingAdapter(IOutgameAdNotificationHost host,Func<float> realtime,
            Func<float> lastShowTime,Action<string> log):base(realtime,lastShowTime,log)
        {this.host=host??throw new ArgumentNullException(nameof(host));this.log=log;}
        public void NotifyRequestAdFail()
        {
            if(SourceState==3)return;
            log(LogPrefix+"notifyRequestAdFail ");SourceState=3;
            host.StopLoadTimeout();host.StopShowTimeout();Listener?.Failed?.Invoke(this);
        }
        public void NotifyRequestAdSuccess()
        {
            if(SourceState==2)return;
            log(LogPrefix+"notifyRequestAdSuccess");SourceState=2;
            host.StopLoadTimeout();host.ReportRequest();Listener?.Received?.Invoke(this);
            if(IsCacheRequest)return;
            if(host.InterVideoShowing)
            {
                log(LogPrefix+" has inter or video ad showing stop intershow");
                OnFinishClearCache();SourceState=3;Listener?.Failed?.Invoke(this);return;
            }
            host.StartShow();
        }
        public void NotifyShowAd(bool report,bool reportValue)
        {
            if(SourceState!=2)return;
            Clicked=false;log(LogPrefix+"notifyShowAd price:"+host.GetPrice());SourceState=4;
            host.StopShowTimeout();if(report)host.ReportShow(reportValue);
            if(AdType!="BANNER")host.ReportNewAdShow();Listener?.Shown?.Invoke(this);
        }
        public void NotifyCloseAd()
        {
            if(SourceState==5)return;
            log(LogPrefix+"notifyCloseAd");SourceState=5;
            host.StopShowTimeout();Listener?.Closed?.Invoke(this);
        }
        public void NotifyClickAd()
        {
            log(LogPrefix+"notifyClickAd");
            if(Clicked){host.ReportNewClick(true);return;}
            Clicked=true;host.ReportClick();host.ReportNewClick(false);
            if(AdType.Contains("INTERSTITAL"))host.HandleAdsLevel("inters_click_level");
        }
        void ReportShowError(int code,string message)
        {
            log(LogPrefix+"NewReportDBTAdShowError errCode:"+code+" errMsg:"+message);
            host.ReportNewShowError(code,message);
        }
        public void NotifyShowAdError(int code,string message)
        {
            if(SourceState==5)return;
            log(LogPrefix+"notifyShowAdError");SourceState=5;host.StopShowTimeout();
            if(AdType!=null&&AdType.Contains("INTERSTITAL")&&
                ((message!=null&&message.Contains("距离小程序插屏广告或者激励视频广告上次播放时间间隔不足，不允许展示插屏广告"))||host.GetPlatformId()==458||host.GetPlatformId()==459))
            {
                ReportShowError(code,message);host.ReportShowFail(code,message);
                Listener?.Closed?.Invoke(this);return;
            }
            if(AdType!=null&&AdType.Contains("VIDEO")&&message!=null&&
                (message.Contains("can't invoke show() while other video-ad is showed")||
                 message.Contains("setDeviceOrientation:fail: requestDeviceOrientation fail")||host.GetPlatformId()==458))
            {
                ReportShowError(code,message);host.ReportShowFail(code,message);
                Listener?.Closed?.Invoke(this);return;
            }
            if(AdType!="BANNER"||BannerErrorReportEnable==1)
            {
                ReportShowError(code,message);
                var listener=Listener;
                if(listener==null)return;
                if(listener.ShowFailed?.Invoke(this)==true)return;
                host.ReportShowFail(code,message);return;
            }
            if(AdType=="BANNER")Listener?.ShowFailed?.Invoke(this);
            else Listener?.Closed?.Invoke(this);
        }
        public void NotifyVideoRewarded()
        {
            log(LogPrefix+"notifyVideoRewarded");host.ReportVideoComplete();Listener?.Rewarded?.Invoke();
            if(AdType=="VIDEO")host.HandleAdsLevel("video_click_level");
        }
    }
}
