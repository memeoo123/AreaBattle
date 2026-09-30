using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameRewardVideoHost
    {
        string ClickLockParameter {get;}
        float Realtime {get;}
        bool NativeVideoReady {get;}
        void ResetNativeSourceFlag64();
        void ShowNative(Action<bool> shown,Action<bool> closed);
        object StartRoutine(IEnumerator routine);
        void StopRoutine(object routine);
        void Log(string message);
        void Warn(string message);
        void ReportNoAds(string label);
        void Send(string name,params object[] args);
    }
    // ADModule.ShowVideo and source callbacks. Native delivery remains an explicit provider.
    public sealed class OutgameRewardVideo
    {
        const string Prefix="DAU-ADModule ";
        readonly IOutgameRewardVideoHost host;
        Action<bool> callback;
        object autoClose;
        public bool Active; // WXModuleBase source field8, managed zero default.
        public bool Ready=true,FirstDayNoAd;
        public bool Showing {get;private set;}
        public float LastClick {get;private set;}=-1f;
        public OutgameRewardVideo(IOutgameRewardVideoHost host){this.host=host??throw new ArgumentNullException(nameof(host));}
        int FilterType(){string value=host.ClickLockParameter;return string.IsNullOrEmpty(value)||value=="1"?1:0;}
        public void Show(Action<bool> completed)
        {
            callback=null;callback+=completed;
            if(FilterType()==0&&host.Realtime-LastClick<1f){host.Log(Prefix+" ShowVideo isDoubleClick");Fail(false);return;}
            if(FilterType()==1&&Showing){host.Log(Prefix+" ShowVideo videoShowing");Fail(false);return;}
            float now=host.Realtime;Showing=true;LastClick=now;
            if(FirstDayNoAd){host.Log(Prefix+" ShowVideo firstDayNoAd");Fail(true);return;}
            if(!Ready){host.Log(Prefix+" ShowVideo Failed no video ready");Fail(true);return;}
            if(!host.NativeVideoReady){host.Log(Prefix+" ShowVideo no video ready");CancelAutoClose();autoClose=host.StartRoutine(DelayClose());}
            host.ResetNativeSourceFlag64();host.Log(Prefix+" ShowVideo");
            host.ShowNative(Shown,Closed);host.Send("VideoAdShow");
        }
        void Fail(bool resetShowing){callback?.Invoke(false);callback=null;if(resetShowing)Showing=false;}
        IEnumerator DelayClose()
        {
            yield return new WaitForSeconds(10f);
            host.Log(Prefix+" ShowVideo no video ready auto close");Fail(true);
        }
        void CancelAutoClose(){if(autoClose!=null){host.StopRoutine(autoClose);autoClose=null;}}
        void Shown(bool success)
        {CancelAutoClose();if(success){host.Send("VideoAdOnShowSuccess");return;}Showing=false;}
        void Closed(bool ended)
        {
            CancelAutoClose();Showing=false;host.Log(Prefix+" VideoCloseEvent isEnded:"+ended);
            callback?.Invoke(ended);callback=null;host.Send("VideoAdOnClose",ended);
        }
        public void OnRewardADCallBack(string result)
        {
            if(callback==null){host.Log("");return;}
            if(result=="1"){callback?.Invoke(true);callback=null;return;}
            if(result=="0"){callback?.Invoke(false);callback=null;return;}
            if(result=="-1"){host.ReportNoAds("unknow");host.Warn("OnRewardADCallBack:-1");}
        }
    }
}
