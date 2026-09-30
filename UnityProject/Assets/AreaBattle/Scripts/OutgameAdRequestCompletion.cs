using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    // BaseController.checkRequest/reload/stopReload and <loadDelay>d__39.
    public sealed class OutgameAdRequestCompletion
    {
        readonly OutgameAdShowFlow flow;
        readonly Func<int> cacheCount;
        readonly Action success,fail,load;
        readonly Action<bool> videoLoadEvent;
        readonly Action<string> log;
        readonly Func<IEnumerator,object> start;
        readonly Action<object> stop;
        public object ReloadRoutine {get;private set;}
        public OutgameAdRequestCompletion(OutgameAdShowFlow flow,Func<int> cacheCount,
            Action success,Action fail,Action<bool> videoLoadEvent,Action load,
            Func<IEnumerator,object> start,Action<object> stop,Action<string> log)
        {this.flow=flow;this.cacheCount=cacheCount;this.success=success;this.fail=fail;
         this.videoLoadEvent=videoLoadEvent;this.load=load;this.start=start;this.stop=stop;this.log=log;}
        public int GetNowCacheNum()=>cacheCount();
        public void CheckRequest()
        {
            int count=cacheCount();bool showing=flow.IsShowing();
            log(flow.Prefix+"checkRequest cacheAmount:"+flow.CacheAmount+" hasShow:"+showing+" nowCacheNum:"+count);
            if((showing?unchecked(count+1):count)>=flow.CacheAmount)
            {if(flow.AdType!="BANNER")success();return;}
            bool banner=flow.AdType=="BANNER";
            if(count>=1){if(!banner)success();}
            else
            {
                if(!banner)fail();
                if(flow.AdType=="VIDEO")videoLoadEvent(false);
            }
            Reload();
        }
        public static int GetReloadDelay(float configured)
        {
            double value=configured;int seconds=Math.Abs(value)<2147483648d?(int)value:int.MinValue;
            return seconds>15?seconds:15;
        }
        public void Reload()
        {
            int seconds=GetReloadDelay(flow.AdType=="BANNER"?flow.SourceConfig.banRefreshTime:flow.SourceConfig.reqInterTime);
            log(flow.Prefix+"reload delay:"+seconds);StopReload();ReloadRoutine=start(LoadDelay(seconds));
        }
        public void StopReload(){var routine=ReloadRoutine;if(routine!=null){stop(routine);ReloadRoutine=null;}}
        IEnumerator LoadDelay(int seconds){yield return new WaitForSecondsRealtime(seconds);load();}
    }
}
