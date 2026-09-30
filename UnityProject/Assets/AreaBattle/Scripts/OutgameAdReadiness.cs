using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameAdReadinessCandidate
    {
        int SourceState {get;}
        bool CanShow();
        bool IsCacheRequest();
    }
    public static class OutgameAdReadiness
    {
        // BaseController.isReady: the second alternative is virtual slot8, not a state check.
        public static bool IsReady(IEnumerable<IOutgameAdReadinessCandidate> adapters,string prefix,Action<string> log)
        {
            foreach(var adapter in adapters)
                if((adapter.SourceState==2&&adapter.CanShow())||adapter.IsCacheRequest())
                {log(prefix+"isReady true ");return true;}
            log(prefix+"isReady false ");return false;
        }
    }
    // DAUAdsAdapter canShow/finish base behavior; concrete platform adapter provides cache clearing.
    public abstract class OutgameAdAdapterState:IOutgameAdReadinessCandidate
    {
        readonly Func<float> realtime,lastShowTime;
        readonly Action<string> log;
        public int SourceState {get;set;}
        public virtual string AdType {get;set;}
        public string LogPrefix;
        public float Interval;
        protected OutgameAdAdapterState(Func<float> realtime,Func<float> lastShowTime,Action<string> log)
        {this.realtime=realtime??throw new ArgumentNullException(nameof(realtime));this.lastShowTime=lastShowTime??throw new ArgumentNullException(nameof(lastShowTime));this.log=log??throw new ArgumentNullException(nameof(log));}
        public virtual bool IsCacheRequest=>true;
        bool IOutgameAdReadinessCandidate.IsCacheRequest()=>IsCacheRequest;
        public virtual bool IsGuarantee=>false;
        public virtual bool IsHighPriority=>false;
        public bool CanShow()
        {
            if(!IsGuarantee||AdType=="BANNER")return true;
            float now=realtime();
            log(LogPrefix+" canShow now:"+now+" lastShowTime:"+lastShowTime()+" interval:"+Interval);
            return !(Interval>now-lastShowTime());
        }
        protected abstract void OnFinishClearCache();
        public void Finish(){OnFinishClearCache();SourceState=0;}
    }
}
