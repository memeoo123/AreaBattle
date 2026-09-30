using System;
using System.Collections;
namespace AreaBattle
{
    public interface IOutgameRewardVideoEnvironment
    {
        string ClickLockParameter {get;}
        float Realtime {get;}
        object StartRoutine(IEnumerator routine);
        void StopRoutine(object routine);
        void Log(string message);
        void Warn(string message);
        void ReportNoAds(string label);
        void Send(string name,params object[] args);
    }
    // Connect ADModule to recovered NewAdsManager routing; no synthetic ad-success fallback.
    public sealed class OutgameRewardVideoControllerHost:IOutgameRewardVideoHost
    {
        readonly OutgameAdRequestRouter router;
        readonly IOutgameRewardVideoEnvironment environment;
        public OutgameRewardVideoControllerHost(OutgameAdRequestRouter router,IOutgameRewardVideoEnvironment environment)
        {this.router=router??throw new ArgumentNullException(nameof(router));this.environment=environment??throw new ArgumentNullException(nameof(environment));}
        public string ClickLockParameter=>environment.ClickLockParameter;
        public float Realtime=>environment.Realtime;
        public bool NativeVideoReady=>router.IsVideoReady();
        public void ResetNativeSourceFlag64(){router.SourceFlag64=false;}
        public void ShowNative(Action<bool> shown,Action<bool> closed){router.ShowVideo(shown,closed,false);}
        public object StartRoutine(IEnumerator routine)=>environment.StartRoutine(routine);
        public void StopRoutine(object routine){environment.StopRoutine(routine);}
        public void Log(string message){environment.Log(message);}
        public void Warn(string message){environment.Warn(message);}
        public void ReportNoAds(string label){environment.ReportNoAds(label);}
        public void Send(string name,params object[] args){environment.Send(name,args);}
    }
}
