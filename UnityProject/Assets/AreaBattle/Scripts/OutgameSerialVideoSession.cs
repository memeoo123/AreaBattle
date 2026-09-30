using System;
using System.Collections;
using System.Collections.Generic;
namespace AreaBattle
{
    // Source VIDEO composition. Platform runtime/reporting and manager publication
    // are supplied by the application; lifecycle continuation is owned here.
    public sealed class OutgameSerialVideoSession
    {
        readonly IOutgameAdShowEnvironment environment;
        readonly IOutgameSerialControllerHost host;
        readonly Action printAllState;
        public readonly OutgameVideoController Controller;
        public readonly OutgameSerialAdConfiguration Configuration;
        public readonly OutgameSerialControllerLifecycle Lifecycle;
        public readonly OutgameSerialAdPreload Preload;
        public readonly OutgameAdRequestCompletion Completion;
        public OutgameSerialVideoSession(IOutgameAdShowEnvironment environment,IOutgameSerialControllerHost host,
            OutgameAdModuleEntry module,Func<IOutgameWxVideoRuntime> createRuntime,
            Func<IEnumerator,object> start,Action<object> stop,Action printAllState)
        {
            this.environment=environment;this.host=host;this.printAllState=printAllState;
            Controller=new OutgameVideoController(environment,GetVideos,()=>host.Realtime,host.SetInterVideoShowing);
            Configuration=new OutgameSerialAdConfiguration(Controller.Flow,
                (key,id)=>module.CreateAdAdapter<IOutgameAdShowCandidate>(key,id,kind=>
                {
                    if(kind!=OutgameAdAdapterKind.WxVideo)throw new NotSupportedException("Non-video adapter requires its own controller composition");
                    return new OutgameWxVideoAdapter(createRuntime());
                }),
                (adapter,id,zone)=>((OutgameWxVideoAdapter)adapter).Initialize(id,zone),
                adapter=>((OutgameWxVideoAdapter)adapter).CanRequest(),host.Log);
            Lifecycle=new OutgameSerialControllerLifecycle(Controller,new LifecycleHost(this));
            Preload=new OutgameSerialAdPreload(Controller.Flow,()=>Configuration.GetPlatforms().Count,start,stop,Load)
                {Listener=Lifecycle.CreateListener()};
            Completion=new OutgameAdRequestCompletion(Controller.Flow,()=>Preload.GetNowCacheNum(GetPreloadCandidates()),
                host.ReportRotaRequestAdSuccess,environment.ReportRotaRequestAdFail,host.VideoLoadSuccessEvent,Load,start,stop,host.Log);
        }
        IEnumerable<IOutgameVideoCandidate> GetVideos()
        {foreach(var adapter in Configuration.GetPlatforms())yield return (IOutgameVideoCandidate)adapter;}
        IEnumerable<IOutgameAdPreloadCandidate> GetPreloadCandidates()
        {foreach(var adapter in Configuration.GetPlatforms())yield return (IOutgameAdPreloadCandidate)adapter;}
        public void Initialize(OutgameAdZoneConfig zone,int bidding,Func<bool> requestDelayEnabled)
        {Controller.InitializeSerial(c=>Configuration.SetAdz(zone),bidding,requestDelayEnabled,Load);}
        public void Load()=>Preload.Load(GetPreloadCandidates,environment.ReportRotaRequestAd,host.Log);
        public void CheckRequest()=>Preload.CheckRequest(printAllState,Completion,host.Log);
        sealed class LifecycleHost:IOutgameSerialControllerHost
        {
            readonly OutgameSerialVideoSession owner;
            IOutgameSerialControllerHost Host=>owner.host;
            public LifecycleHost(OutgameSerialVideoSession owner){this.owner=owner;}
            public void Load()=>owner.Load();
            public void CheckRequest()=>owner.CheckRequest();
            public void VideoShowLoad()=>owner.Preload.VideoShowLoad();
            public void StopVideoShowLoad()=>owner.Preload.StopVideoShowLoad();
            public int GetPlatListCount()=>owner.Configuration.GetPlatforms().Count;
            public void Log(string text)=>Host.Log(text);
            public void ResetTimerReport()=>Host.ResetTimerReport();
            public void SetInterVideoShowing(bool value)=>Host.SetInterVideoShowing(value);
            public void SetIntersClose(string type)=>Host.SetIntersClose(type);
            public void ShowIntersAfterIntersClose()=>Host.ShowIntersAfterIntersClose();
            public void ReportInsertClose()=>Host.ReportInsertClose();
            public void ShowIntersAfterVideoClose()=>Host.ShowIntersAfterVideoClose();
            public void RestoreBanner()=>Host.RestoreBanner();
            public void CloseBanner()=>Host.CloseBanner();
            public void ChangeTimerReport(int type)=>Host.ChangeTimerReport(type);
            public void ReportPlatformBack()=>Host.ReportPlatformBack();
            public void ReportRotaRequestAdSuccess()=>Host.ReportRotaRequestAdSuccess();
            public bool CanAutoShowInter4=>Host.CanAutoShowInter4;
            public void SetCanAutoShowInter4(bool value)=>Host.SetCanAutoShowInter4(value);
            public void OnlyShowInter4(bool value)=>Host.OnlyShowInter4(value);
            public void VideoLoadSuccessEvent(bool value)=>Host.VideoLoadSuccessEvent(value);
            public float Realtime=>Host.Realtime;
        }
    }
}
