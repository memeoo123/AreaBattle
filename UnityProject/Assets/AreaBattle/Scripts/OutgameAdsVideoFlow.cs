using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // AdsManager24562/state machine24626 and24567-24569. SDK transport remains explicit.
    public sealed class OutgameAdsVideoFlow
    {
        readonly Func<float> unscaledTime;
        readonly Func<WaitForSecondsRealtime,Task> wait;
        readonly Action<int,int> showVideoStatic;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Action<string> reportVideo,releaseLog;
        readonly Action<bool> setSdkVideoPlaying;
        readonly Func<string,Dictionary<string,object>> parseData;
        readonly WaitForSecondsRealtime wait2=new WaitForSecondsRealtime(.2f);
        Action<string,bool> videoPlayAction;
        public bool IsOpenAdsReport {get;set;}
        public float ShowVideoInterval {get;private set;}
        public OutgameAdsVideoFlow(Func<float> unscaledTime,Func<WaitForSecondsRealtime,Task> wait,Action<int,int> showVideoStatic,Func<OutgameMessageDispatcher> messages,Action<string> reportVideo,Action<string> releaseLog,Action<bool> setSdkVideoPlaying,Func<string,Dictionary<string,object>> parseData)
        {this.unscaledTime=unscaledTime;this.wait=wait;this.showVideoStatic=showVideoStatic;this.messages=messages;this.reportVideo=reportVideo;this.releaseLog=releaseLog;this.setSdkVideoPlaying=setSdkVideoPlaying;this.parseData=parseData;}
        public async void ShowVide(int flag,Action<string,bool> callback,int videoId)
        {await ShowVideoAsync(flag,callback,videoId);}
        public async Task ShowVideoAsync(int flag,Action<string,bool> callback,int videoId)
        {
            videoPlayAction=null;videoPlayAction+=callback;
            if(unscaledTime()<ShowVideoInterval)return;
            ShowVideoInterval=unscaledTime()+4f;
            await wait(wait2);
            messages().SendMessage("GF_ShowAdsVideo");
            showVideoStatic(flag,videoId);
        }
        public void AfterVideo(string data)
        {
            setSdkVideoPlaying(false);
            ParseAfterVideo(parseData(data));
        }
        public void AfterVideoFailed(string flag)
        {releaseLog("Ads:视频播放奖励发放失败：videoFlag["+flag+"] ");}
        public void ParseAfterVideo(Dictionary<string,object> data)
        {
            string flag=data.ContainsKey("videoFlag")?data["videoFlag"].ToString():"";
            string result=data.ContainsKey("result")?data["result"].ToString():"";
            releaseLog("Ads:解析视频回调：videoFlag["+flag+"] result["+result+"] ");
            bool success=result=="0";
            if(success&&IsOpenAdsReport)reportVideo(flag);
            if(!success)ShowVideoInterval=unscaledTime();
            messages().SendMessage("GF_AdsPlayCallBack",new object[]{success});
            videoPlayAction?.Invoke(flag,success);
        }
    }
}
