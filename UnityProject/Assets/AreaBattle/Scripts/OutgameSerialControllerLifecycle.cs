using System;
namespace AreaBattle
{
    public interface IOutgameSerialControllerHost
    {
        void Log(string text);
        void ResetTimerReport();
        void SetInterVideoShowing(bool value);
        void SetIntersClose(string adType);
        void ShowIntersAfterIntersClose();
        void ReportInsertClose();
        void ShowIntersAfterVideoClose();
        void StopVideoShowLoad();
        int GetPlatListCount();
        void Load();
        void RestoreBanner();
        void CloseBanner();
        void ChangeTimerReport(int type);
        void ReportPlatformBack();
        void ReportRotaRequestAdSuccess();
        void VideoShowLoad();
        void CheckRequest();
        bool CanAutoShowInter4 {get;}
        void SetCanAutoShowInter4(bool value);
        void OnlyShowInter4(bool value);
        void VideoLoadSuccessEvent(bool value);
        float Realtime {get;}
    }
    // SerialController.onCloseAd/onShowFail/onVideoRewarded. Echelon preload differs.
    public sealed class OutgameSerialControllerLifecycle
    {
        readonly OutgameVideoController controller;
        readonly IOutgameSerialControllerHost host;
        public Action<bool> ShowFailCallback,RequestCallback;
        public OutgameSerialControllerLifecycle(OutgameVideoController controller,IOutgameSerialControllerHost host)
        {this.controller=controller??throw new ArgumentNullException(nameof(controller));this.host=host??throw new ArgumentNullException(nameof(host));}
        public OutgameAdListener CreateListener()
        {
            return new OutgameAdListener{
                Received=adapter=>OnReceiveAdSuccess((IOutgameAdShowCandidate)adapter),
                Failed=adapter=>OnReceiveAdFailed((IOutgameAdShowCandidate)adapter),
                Shown=adapter=>OnShowAd((IOutgameAdShowCandidate)adapter),
                Closed=adapter=>OnCloseAd(),Rewarded=OnVideoRewarded,
                ShowFailed=adapter=>OnShowFail()};
        }
        public void OnShowAd(IOutgameAdShowCandidate adapter)
        {
            var flow=controller.Flow;host.Log(flow.Prefix+"onShowAd");controller.ShowCallback?.Invoke(true);
            if(flow.AdType!="BANNER"&&flow.AdType!="NATIVE_SPLASH")
            {host.CloseBanner();host.ChangeTimerReport(1);}
            if(flow.AdType=="BANNER")host.ChangeTimerReport(2);
            if(flow.AdType.Contains("INTERSTITAL")&&!adapter.WaitCloseReportShow)
            {
                host.SetIntersClose(flow.AdType);host.ReportPlatformBack();
                if(!adapter.IsCacheRequest)host.ReportRotaRequestAdSuccess();
            }
            if(flow.AdType.Contains("INTERSTITAL")||flow.AdType=="VIDEO")host.SetInterVideoShowing(true);
            if(flow.AdType=="VIDEO")host.VideoShowLoad();
        }
        public void OnRealShowAd(IOutgameAdShowCandidate adapter)
        {
            bool inter=controller.Flow.AdType.Contains("INTERSTITAL");
            if(adapter!=null&&inter&&adapter.WaitCloseReportShow)host.ReportPlatformBack();
        }
        public void OnReceiveAdFailed(IOutgameAdShowCandidate adapter)
        {
            host.Log(controller.Flow.Prefix+"onReceiveAdFailed");RequestCallback?.Invoke(false);
            if(!adapter.IsCacheRequest){controller.Flow.ShowAd(true,false);return;}
            host.CheckRequest();
        }
        public void OnReceiveAdSuccess(IOutgameAdShowCandidate adapter,bool hasIdsInfo=true)
        {
            var flow=controller.Flow;host.Log(flow.Prefix+"onReceiveAdSuccess");
            if(adapter.IsCacheRequest)
            {
                if(!flow.Cache.Contains(adapter))flow.Cache.Add(adapter);
                RequestCallback?.Invoke(true);host.CheckRequest();
            }
            else if(flow.AdType.Contains("INTERSTITAL")&&hasIdsInfo)
                flow.Selection.LastShowPriority=adapter.Priority;
            if(flow.AdType=="INTERSTITAL4"&&host.CanAutoShowInter4&&adapter.IsCacheRequest)
            {host.SetCanAutoShowInter4(false);host.OnlyShowInter4(true);}
            if(flow.AdType=="VIDEO")
            {
                host.VideoLoadSuccessEvent(true);
                host.Log(flow.Prefix+"onReceiveAdSuccess videoShowTime:"+flow.VideoShowTime+" realTime "+host.Realtime);
                if(flow.VideoShowTime!=-1&&host.Realtime-flow.VideoShowTime<=3&&
                    (adapter.PlatformId==883||adapter.PlatformId==142))flow.ShowAd(false,false);
            }
        }
        public void OnVideoRewarded()
        {host.Log(controller.Flow.Prefix+"onVideoRewarded");controller.RewardCallback?.Invoke(false);}
        public bool OnShowFail()
        {
            host.Log(controller.Flow.Prefix+"onVideoShowFail");ShowFailCallback?.Invoke(true);
            if(controller.Flow.AdType!="BANNER"&&controller.IsReady())
            {
                host.SetInterVideoShowing(false);controller.Flow.ShowAd(false,true);return true;
            }
            OnCloseAd();return false;
        }
        public void OnCloseAd()
        {
            var flow=controller.Flow;
            host.Log(flow.Prefix+"onCloseAd");flow.CloseCallback?.Invoke(false);
            host.ResetTimerReport();
            if(flow.AdType.Contains("INTERSTITAL"))
            {
                host.SetInterVideoShowing(false);host.SetIntersClose(flow.AdType);
                if(!controller.SourceFlag20)host.ShowIntersAfterIntersClose();
                host.ReportInsertClose();
            }
            if(flow.AdType.Contains("VIDEO"))
            {
                host.SetInterVideoShowing(false);
                if(!controller.SourceFlag20)host.ShowIntersAfterVideoClose();
                host.StopVideoShowLoad();flow.CacheAmount=host.GetPlatListCount();
            }
            if(flow.AdType!="BANNER"&&flow.AdType!="NATIVE_SPLASH")
            {host.Load();host.RestoreBanner();}
        }
    }
}
