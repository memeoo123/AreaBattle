using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace AreaBattle
{
    // UIVideoBtn completion/listener subset74412/74416/74427-74429.
    // Advertising, click gating and report transport belong to the original host.
    public sealed class OutgameVideoButtonCompletion:MonoBehaviour
    {
        UnityEvent<bool> click,completed;
        Func<int> showType;
        Action<int> startDelayReport;
        Action reportSuccess,reportFailure;
        bool destroyed;
        public int ReportState {get;set;}
        public void Bind(Func<int> showType,Action<int> startDelayReport,Action reportSuccess,Action reportFailure)
        {this.showType=showType;this.startDelayReport=startDelayReport;this.reportSuccess=reportSuccess;this.reportFailure=reportFailure;}
        public void AddVideoBtnEvent(UnityAction<bool> callback)
        {(click??(click=new UnityEvent<bool>())).AddListener(callback);}
        public void AddVideoPlayCallBack(UnityAction<bool> callback)
        {(completed??(completed=new UnityEvent<bool>())).AddListener(callback);}
        public void InvokeClick(bool videoReady)=>click?.Invoke(videoReady);
        public void RemoveVideoBtnListeners()
        {completed?.RemoveAllListeners();click?.RemoveAllListeners();}
        public void VideoCallBack(string callbackName,bool success)
        {
            if(showType()==2)
            {
                ReportState=0;
                if(!destroyed&&gameObject.activeSelf)startDelayReport(1);
            }
            if(success)reportSuccess();else reportFailure();
            completed?.Invoke(success);
        }
        // DelayReport74423 / state machine74434: scaled WaitForSeconds, then report and SDK readiness gate.
        public static IEnumerator DelayReport(int seconds,Action createReport,Func<bool> isSdkVideoOpen,Action readyReport)
        {
            yield return new WaitForSeconds(seconds);
            createReport();
            if(isSdkVideoOpen())readyReport();
        }
        void OnDestroy(){destroyed=true;click=null;completed=null;}
    }
}
