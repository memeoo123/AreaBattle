using System;
namespace AreaBattle
{
    public sealed class OutgameWxVideoCloseResult {public bool IsEnded;}
    public interface IOutgameWxVideoHandle:IOutgameWxVideoLoadHandle
    {
        void OnClose(Action<OutgameWxVideoCloseResult> callback);
        void OffClose(Action<OutgameWxVideoCloseResult> callback);
        void Show(Action<OutgameWxAdError> success,Action<OutgameWxAdError> fail);
    }
    public interface IOutgameWxVideoPresentationHost
    {
        bool MiniGameCommonPlugin {get;}
        int PlayCount {get;set;}
        int SuccessCount {get;set;}
        void ReportRewardShowEvent();
        void RewardAdShow();
        void RequestRewardPrediction(int total,int successes);
        string StateText {get;}
        void Log(string text);
    }
    // WxVideoAdapter startShowAd/CloseCallback and native show callbacks.
    // Prediction response processing remains the reporting host's responsibility.
    public sealed class OutgameWxVideoPresentation
    {
        readonly OutgameWxVideoLoading loading;
        readonly OutgameAdNotifyingAdapter adapter;
        readonly IOutgameWxVideoPresentationHost host;
        public bool IsClosed {get;private set;}
        public OutgameWxVideoPresentation(OutgameWxVideoLoading loading,OutgameAdNotifyingAdapter adapter,IOutgameWxVideoPresentationHost host)
        {this.loading=loading;this.adapter=adapter;this.host=host;}
        public void StartShowAd()
        {
            host.Log(adapter.LogPrefix+"startShowAd");
            var native=(IOutgameWxVideoHandle)loading.Native;
            if(native==null)
            {
                host.Log(adapter.LogPrefix+"VideoAd is null");adapter.NotifyShowAdError(0,"VideoAd is null");return;
            }
            IsClosed=false;native.OnClose(CloseCallback);
            ((IOutgameWxVideoHandle)loading.Native).Show(ShowSuccess,ShowFail);
        }
        void ShowSuccess(OutgameWxAdError ignored)
        {
            host.Log(adapter.LogPrefix+"ShowVideoSuccess "+host.StateText+" isClosed:"+IsClosed);
            if(!IsClosed)adapter.NotifyShowAd(true,!host.MiniGameCommonPlugin);
            host.RewardAdShow();
        }
        void ShowFail(OutgameWxAdError error)
        {
            if(error==null||string.IsNullOrEmpty(error.Message))
            {host.Log(adapter.LogPrefix+"ShowVideoFail errMsg null");return;}
            host.Log(adapter.LogPrefix+"ShowVideoFail "+error.Code+" "+error.Message);
            adapter.NotifyShowAdError(error.Code,error.Message);
        }
        public void CloseCallback(OutgameWxVideoCloseResult result)
        {
            host.Log(adapter.LogPrefix+"CloseCallback getAdState:"+host.StateText);
            IsClosed=true;
            var native=(IOutgameWxVideoHandle)loading.Native;
            if(native!=null)native.OffClose(CloseCallback);
            loading.AutoloadSuccess=false;
            if(result!=null)
            {
                if(result.IsEnded){adapter.NotifyVideoRewarded();host.SuccessCount=unchecked(host.SuccessCount+1);}
                else if(host.MiniGameCommonPlugin)host.ReportRewardShowEvent();
                host.PlayCount=unchecked(host.PlayCount+1);
                if(result.IsEnded)host.RequestRewardPrediction(host.PlayCount,host.SuccessCount);
            }
            else host.PlayCount=unchecked(host.PlayCount+1);
            adapter.NotifyCloseAd();
        }
    }
}
