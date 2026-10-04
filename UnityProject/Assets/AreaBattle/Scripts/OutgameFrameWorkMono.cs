using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace AreaBattle
{
    public enum OutgamePauseSource{AppFocus=0,AppPause=1,ActiveTriggerPause=2}
    [Serializable] public sealed class OutgameDomainConfig
    {public string id,GN,BN,D,TD,RD;}
    [Serializable] public sealed class OutgameDomainData
    {
        public List<OutgameDomainConfig> Datas=new List<OutgameDomainConfig>();
        // Original public26530: last matching GN/BN wins; only types2/3 assign a value.
        public string GetValue(string gameName,int type,string business)
        {
            string value=string.Empty;
            for(int i=0;i<Datas.Count;i++)
                if(string.Equals(gameName,Datas[i].GN)&&string.Equals(business,Datas[i].BN))
                {if(type==2)value=Datas[i].TD;else if(type==3)value=Datas[i].RD;}
            return value;
        }
    }
    public sealed class OutgameFrameMonoServices
    {
        public Func<OutgameMessageDispatcher> Messages;
        public Action<object[]> Log,ReleaseLog;
        public Func<bool> IsReleaseVersion;
        public Action CheckSdkLogSupport;
        public Func<int> GetNetworkSample;
        public Action<int> SetNetworkSample;
        public Action SavePreferences;
        public Func<string,string> OnlineConfig;
        public Func<int,int,int> RandomInclusive=GameRandomSource.Shared.Inclusive;
    }
    // Source3442 normal Init/focus/pause paths, singleton26512 and sampler26500.
    // Uncalled obfuscated alternatives are not substituted for these bodies.
    public sealed class OutgameFrameWorkMono:MonoBehaviour
    {
        static OutgameFrameWorkMono instance;
        public static OutgameFrameMonoServices Services;
        public static int MainThreadId;
        public int PauseState;
        public OutgamePauseSource PauseSource;
        public bool IsInitialized,NetworkSamplingEnabled=true,HasFocus=true;
        public OutgameDomainData DomainData=new OutgameDomainData();
        public static OutgameFrameWorkMono Instance
        {
            get
            {
                if(instance==null)
                {
                    var root=GameObject.Find("GameFrameWorkMono");
                    if(root==null){root=new GameObject("GameFrameWorkMono");DontDestroyOnLoad(root);}
                    // Source always AddComponent, including an existing root; no GetComponent reuse.
                    instance=root.AddComponent<OutgameFrameWorkMono>();
                }
                return instance;
            }
        }
        public static void Init()
        {
            Services.ReleaseLog(new object[]{"初始化GameFrameWorkMono"});Instance.IsInitialized=true;
            MainThreadId=Thread.CurrentThread.ManagedThreadId;
            if(Services.IsReleaseVersion())Services.CheckSdkLogSupport();
        }
        public void ConfigureNetworkSampling()
        {
            int sample=Services.GetNetworkSample();
            if(sample==-1){sample=Services.RandomInclusive(1,100);Services.SetNetworkSample(sample);Services.SavePreferences();}
            Services.Log(new object[]{"网络随机上报1:"+sample});
            string text=Services.OnlineConfig("net_status_game_statistic_filter");int threshold=20;
            if(!string.IsNullOrEmpty(text))int.TryParse(text,out threshold);
            Services.Log(new object[]{"网络随机上报2:"+threshold});
            if(threshold<=sample)NetworkSamplingEnabled=false;
        }
        public void OnApplicationFocus(bool focus)
        {if(!IsInitialized)return;SetFocus(focus);SetPause(!focus,OutgamePauseSource.AppFocus);}
        public void OnApplicationPause(bool pause)
        {if(IsInitialized)SetPause(pause,OutgamePauseSource.AppPause);}
        public void ActivePause(bool pause)
        {if(IsInitialized)SetPause(pause,OutgamePauseSource.ActiveTriggerPause);}
        public void SetFocus(bool focus)
        {if(HasFocus==focus)return;HasFocus=focus;Services.Messages().SendMessage("GF_GameFocus",new object[]{HasFocus});}
        public void PublishPause()
        {Services.Messages().SendMessage("GF_NewGamePause",new object[]{PauseState==1,PauseSource});}
        public void SetPause(bool pause,OutgamePauseSource source)
        {
            int held=PauseState;
            if(pause)
            {
                if(held==0){PauseSource=source;PauseState=1;PublishPause();return;}
                if((int)PauseSource<(int)source)PauseSource=source;
                return;
            }
            if(held!=1)return;var previous=PauseSource;if((int)previous>(int)source)return;
            PauseState=0;
            string message=source!=previous
                ?string.Format("GamePause：被更高优先级来源解除暂停  来源[{0}]  当前状态来源[{1}]",source,PauseSource)
                :string.Format("GamePause：游戏继续  来源[{0}]",source);
            Services.Log(new object[]{message});PublishPause();
            // Source does not replace PauseSource on resume; reentrant logging can alter the payload.
        }
    }
}
