using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameWxVideoRuntime:IOutgameAdNotificationHost,IOutgameWxVideoPresentationHost
    {
        IOutgameWxVideoHandle CreateVideo(string id,bool multiton);
        int? ConfiguredIdCount {get;}
        float Realtime {get;}
        float LastShowTime {get;}
        DateTime LocalNow {get;}
        int ReadInt(string key,int fallback);
        object StartRoutine(IEnumerator routine);
        void StopRoutine(object handle);
        bool AudioInterrupted {get;set;}
        void ReportRequestAd();
        void ReportTimeout();
    }
    // One source adapter identity is shared by controller cache, preload and native callbacks.
    // Runtime supplies actual platform services; this class never synthesizes native success.
    public sealed class OutgameWxVideoAdapter:OutgameAdNotifyingAdapter,IOutgameVideoCandidate,IOutgameAdPreloadCandidate
    {
        readonly IOutgameWxVideoRuntime runtime;
        public readonly OutgameWxVideoLoading Loading;
        public readonly OutgameWxVideoPresentation Presentation;
        public readonly OutgameAdTimeouts Timeouts;
        public readonly OutgameAdPriceCache PriceCache;
        public OutgameAdIdsInfo IdsInfo {get;private set;}
        public OutgameAdZoneConfig Zone {get;private set;}
        bool initialized;
        int priority;
        public int Priority {get=>initialized?IdsInfo.priority:priority;set{if(initialized)IdsInfo.priority=value;else priority=value;}}
        public int PlatformId=>initialized?IdsInfo.platformId:883;
        public int NativePlatformId=>883;
        public override string AdType {get=>initialized?Zone.zkey:base.AdType;set=>base.AdType=value;}
        public bool WaitCloseReportShow=>false;
        public float RotaTimeout;
        public bool HasCustomParam;
        public string VideoAdTimeout;
        public string IdValues;
        OutgameAdListener IOutgameAdPreloadCandidate.Listener {get=>Listener;set=>Listener=value;}
        public OutgameWxVideoAdapter(IOutgameWxVideoRuntime runtime):this(runtime,new NotificationProxy(runtime)){}
        OutgameWxVideoAdapter(IOutgameWxVideoRuntime runtime,NotificationProxy proxy)
            :base(proxy,()=>runtime.Realtime,()=>runtime.LastShowTime,runtime.Log)
        {
            this.runtime=runtime;proxy.Owner=this;AdType="VIDEO";LogPrefix="DAU-WxVideo ";Interval=20;BannerErrorReportEnable=1;
            PriceCache=new OutgameAdPriceCache(runtime.ReadInt,()=>runtime.LocalNow){PlatformId=PlatformId};
            Loading=new OutgameWxVideoLoading(runtime.CreateVideo,()=>runtime.ConfiguredIdCount,()=>SourceState,
                NotifyRequestAdSuccess,NotifyRequestAdFail,r=>runtime.StartRoutine(r),runtime.Log);
            Presentation=new OutgameWxVideoPresentation(Loading,this,runtime);
            Timeouts=new OutgameAdTimeouts(this,runtime.StartRoutine,runtime.StopRoutine,Loading.ClearCache,
                runtime.ReportTimeout,()=>runtime.AudioInterrupted,x=>runtime.AudioInterrupted=x,()=>runtime.StateText,runtime.Log);
        }
        public bool CanRequest()=>true;
        public void Initialize(OutgameAdIdsInfo ids,OutgameAdZoneConfig zone)
        {
            IdsInfo=ids;Zone=zone;initialized=true;
            LogPrefix+=IdsInfo.platformId+" ";
            if(Zone==null||string.IsNullOrEmpty(Zone.customParam))return;
            string raw=Zone.customParam;
            if(!raw.Contains("ad_bidding_cache_nextday_clear_enable")&&
                !(raw.Contains("banner_error_report_enable")&&Zone.zkey=="BANNER"))return;
            var custom=JsonUtility.FromJson<OutgameAdCustomParam>(raw);
            if(custom==null)return;
            if(!string.IsNullOrEmpty(custom.ad_bidding_cache_nextday_clear_enable))
                int.TryParse(custom.ad_bidding_cache_nextday_clear_enable,out PriceCache.ClearNextDayEnable);
            if(!string.IsNullOrEmpty(custom.banner_error_report_enable))
                int.TryParse(custom.banner_error_report_enable,out BannerErrorReportEnable);
            runtime.Log(LogPrefix+"ad_bidding_cache_nextday_clear_enable:"+PriceCache.ClearNextDayEnable+
                " banner_error_report_enable:"+BannerErrorReportEnable);
        }
        public int GetRealPrice()
        {
            if(initialized&&PriceCache.RealPrice==0){PriceCache.ZoneKey=Zone.zkey;PriceCache.PlatformId=IdsInfo.platformId;}
            return PriceCache.GetRealPrice();
        }
        int GetShowTimeout()
        {
            if(!initialized)return OutgameAdTimeouts.GetShowTimeout(HasCustomParam,VideoAdTimeout,LogPrefix,runtime.Log);
            var custom=Zone==null?null:JsonUtility.FromJson<OutgameAdCustomParam>(Zone.customParam);
            return OutgameAdTimeouts.GetShowTimeout(custom!=null,custom?.video_ad_timeout,LogPrefix,runtime.Log);
        }
        public void Handle()
        {
            Loading.Prefix=LogPrefix;Loading.IdValues=initialized?IdsInfo.idVals:IdValues;
            Timeouts.Handle(()=>OutgameAdTimeouts.GetRotaTimeout(initialized?Zone.rotaTimeout:RotaTimeout,AdType),GetShowTimeout,runtime.ReportRequestAd,Loading.StartRequestAd);
        }
        public void ShowAd()=>Timeouts.ShowAd(GetShowTimeout,Presentation.StartShowAd);
        protected override void OnFinishClearCache()=>Loading.ClearCache();
        sealed class NotificationProxy:IOutgameAdNotificationHost
        {
            readonly IOutgameAdNotificationHost host;
            public OutgameWxVideoAdapter Owner;
            public NotificationProxy(IOutgameAdNotificationHost host){this.host=host??throw new ArgumentNullException(nameof(host));}
            public void StopLoadTimeout()=>Owner.Timeouts.StopLoadTimeout();
            public void StopShowTimeout()=>Owner.Timeouts.StopShowTimeout();
            public void StartShow()=>Owner.ShowAd();
            public int GetPlatformId()=>Owner.PlatformId;
            public bool InterVideoShowing=>host.InterVideoShowing;
            public double GetPrice()=>host.GetPrice();
            public void ReportRequest()=>host.ReportRequest();
            public void ReportShow(bool reportValue)=>host.ReportShow(reportValue);
            public void ReportNewAdShow()=>host.ReportNewAdShow();
            public void ReportVideoComplete()=>host.ReportVideoComplete();
            public void HandleAdsLevel(string key)=>host.HandleAdsLevel(key);
            public void ReportClick()=>host.ReportClick();
            public void ReportNewClick(bool repeated)=>host.ReportNewClick(repeated);
            public void ReportNewShowError(int code,string message)=>host.ReportNewShowError(code,message);
            public void ReportShowFail(int code,string message)=>host.ReportShowFail(code,message);
        }
    }
}
