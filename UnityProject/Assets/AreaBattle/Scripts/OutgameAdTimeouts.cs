using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameAdTimeouts
    {
        readonly OutgameAdNotifyingAdapter adapter;
        readonly Func<IEnumerator,object> start;readonly Action<object> stop;
        readonly Action clearCache,reportTimeout;readonly Func<bool> audioInterrupted;
        readonly Action<bool> setAudioInterrupted;readonly Func<string> stateText;readonly Action<string> log;
        public object LoadRoutine {get;private set;}public object ShowRoutine {get;private set;}
        public OutgameAdTimeouts(OutgameAdNotifyingAdapter adapter,Func<IEnumerator,object> start,Action<object> stop,
            Action clearCache,Action reportTimeout,Func<bool> audioInterrupted,Action<bool> setAudioInterrupted,
            Func<string> stateText,Action<string> log)
        {this.adapter=adapter;this.start=start;this.stop=stop;this.clearCache=clearCache;this.reportTimeout=reportTimeout;
         this.audioInterrupted=audioInterrupted;this.setAudioInterrupted=setAudioInterrupted;this.stateText=stateText;this.log=log;}
        public static int GetRotaTimeout(float configured,string adType)
        {
            if(configured<5)return adType=="BANNER"?5:60;
            double value=configured;return Math.Abs(value)<2147483648d?(int)value:int.MinValue;
        }
        public static int GetShowTimeout(bool hasCustomParam,string videoAdTimeout,string prefix,Action<string> log)
        {
            if(!hasCustomParam)return 5;
            int timeout=5;if(!string.IsNullOrEmpty(videoAdTimeout))int.TryParse(videoAdTimeout,out timeout);
            log(prefix+"getShowTimeout "+timeout);return timeout;
        }
        public void Handle(Func<int> getRotaTimeout,Func<int> getShowTimeout,Action reportRequestAd,Action startRequestAd)
        {
            adapter.SourceState=1;
            if(adapter.IsCacheRequest)LoadRoutine=start(LoadTimeout(getRotaTimeout()));
            else ShowRoutine=start(ShowTimeout(getShowTimeout()));
            reportRequestAd();startRequestAd();
        }
        public void ShowAd(Func<int> getShowTimeout,Action startShowAd)
        {StopShowTimeout();ShowRoutine=start(ShowTimeout(getShowTimeout()));startShowAd();}
        public void StopLoadTimeout(){var handle=LoadRoutine;if(handle!=null){stop(handle);LoadRoutine=null;}}
        public void StopShowTimeout(){var handle=ShowRoutine;if(handle!=null){stop(handle);ShowRoutine=null;}}
        IEnumerator LoadTimeout(int seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if(adapter.SourceState!=1)yield break;
            log(adapter.LogPrefix+"loadTimeout end ");clearCache();adapter.NotifyRequestAdFail();
        }
        IEnumerator ShowTimeout(int seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if(audioInterrupted()&&adapter.AdType=="VIDEO")
            {
                log(adapter.LogPrefix+"showTimeout hasAudioInterrupt:"+audioInterrupted());
                setAudioInterrupted(false);adapter.NotifyShowAd(true,true);yield break;
            }
            log(adapter.LogPrefix+"showTimeout end "+stateText());clearCache();adapter.SourceState=5;
            adapter.Listener?.Closed?.Invoke(adapter);reportTimeout();
        }
    }
}
