using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameWxAdError {public int Code;public string Message;}
    public interface IOutgameWxVideoLoadHandle
    {
        void OnLoad(Action callback);
        void OnError(Action<OutgameWxAdError> callback);
        void Load();
        void Destroy();
    }
    // WxVideoAdapter request/load/clear lifecycle; native bridge supplies the handle.
    public sealed class OutgameWxVideoLoading
    {
        readonly Func<string,bool,IOutgameWxVideoLoadHandle> create;
        readonly Func<int?> configuredIdCount;
        readonly Func<int> state;
        readonly Action success,fail;
        readonly Action<IEnumerator> startRoutine;
        readonly Action<string> log;
        public string Prefix,IdValues;
        public IOutgameWxVideoLoadHandle Native;
        public bool AutoloadSuccess;
        public OutgameWxVideoLoading(Func<string,bool,IOutgameWxVideoLoadHandle> create,
            Func<int?> configuredIdCount,Func<int> state,Action success,Action fail,
            Action<IEnumerator> startRoutine,Action<string> log)
        {this.create=create;this.configuredIdCount=configuredIdCount;this.state=state;
         this.success=success;this.fail=fail;this.startRoutine=startRoutine;this.log=log;}
        public void StartRequestAd()
        {
            bool multiton=true;
            if(configuredIdCount()==1){multiton=false;log(Prefix+" idCount:"+configuredIdCount()+" multiton:"+multiton);}
            log(Prefix+"startRequestAd "+IdValues);
            var native=Native;
            if(native!=null)
            {
                if(AutoloadSuccess)
                {
                    AutoloadSuccess=false;log(Prefix+" autoloadSuccess no need load");
                    startRoutine(DelaySuccess());return;
                }
            }
            else
            {
                log("CreateRewardedVideoAd,"+"adunit-"+IdValues);
                Native=create("adunit-"+IdValues,multiton);
                Native.OnLoad(OnLoad);
                Native.OnError(OnError);
                native=Native;
            }
            native.Load();
        }
        void OnLoad()
        {
            log(Prefix+" OnLoad");
            if(state()==5){log(Prefix+" autoloadSuccess");AutoloadSuccess=true;return;}
            success();
        }
        void OnError(OutgameWxAdError error)
        {log(Prefix+" error response "+error.Code+" "+error.Message);fail();}
        IEnumerator DelaySuccess(){yield return new WaitForSeconds(.1f);success();}
        public void ClearCache()
        {
            log(Prefix+"onFinishClearCache");
            var native=Native;
            if(native!=null){native.Destroy();Native=null;}
        }
    }
}
