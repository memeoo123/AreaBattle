using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameVideoCandidate:IOutgameAdShowCandidate
    {
        void Finish();
    }
    // Shared SerialVideoController/EchelonVideoController show and close semantics.
    // Their differing request initialization and native adapter event wiring remain external.
    public sealed class OutgameVideoController:IOutgameAdController
    {
        readonly Func<IEnumerable<IOutgameVideoCandidate>> getPlatList;
        readonly Func<float> realtime;
        readonly Action<bool> setInterVideoShowing;
        readonly IOutgameAdShowEnvironment environment;
        public readonly OutgameAdShowFlow Flow;
        public bool SourceFlag20 {get;set;}
        public bool IsPlayerEnd {get;private set;}
        public Action<bool> ShowCallback,RewardCallback;
        public OutgameVideoController(IOutgameAdShowEnvironment environment,
            Func<IEnumerable<IOutgameVideoCandidate>> getPlatList,Func<float> realtime,
            Action<bool> setInterVideoShowing)
        {
            this.environment=environment??throw new ArgumentNullException(nameof(environment));
            this.getPlatList=getPlatList??throw new ArgumentNullException(nameof(getPlatList));
            this.realtime=realtime??throw new ArgumentNullException(nameof(realtime));
            this.setInterVideoShowing=setInterVideoShowing??throw new ArgumentNullException(nameof(setInterVideoShowing));
            Flow=new OutgameAdShowFlow(environment,()=>this.getPlatList()){AdType="VIDEO"};
        }
        // NewAdsManager.initAd VIDEO branch: serial controller, cache=1, setAdz,
        // bidding, then immediate load only when global request delay is disabled.
        // Caller publishes the newly created controller before invoking this sequence.
        public void InitializeSerial(Action<OutgameVideoController> setAdz,int biddingEnable,
            Func<bool> requestDelayEnabled,Action load)
        {
            Flow.CacheAmount=1;
            setAdz(this);
            Flow.BiddingEnable=biddingEnable;
            if(!requestDelayEnabled())load();
        }
        public bool IsReady()
        {
            foreach(var adapter in getPlatList())
                if((adapter.SourceState==2&&adapter.CanShow())||adapter.IsCacheRequest)
                {environment.Log(Flow.Prefix+"isReady true ");return true;}
            environment.Log(Flow.Prefix+"isReady false ");return false;
        }
        public void Show(Action<bool> shown,Action<bool> closed)
        {
            float now=realtime();
            IsPlayerEnd=false;Flow.VideoShowTime=now;
            ShowCallback=shown;
            RewardCallback=ignored=>IsPlayerEnd=true;
            Flow.CloseCallback=ignored=>closed?.Invoke(IsPlayerEnd);
            Flow.ShowAd(false,false);
        }
        public void Close()
        {
            environment.Log(Flow.Prefix+"close");
            foreach(var adapter in getPlatList())
                if(adapter.SourceState==4)
                {
                    adapter.Finish();
                    setInterVideoShowing(false);
                    Flow.CloseCallback?.Invoke(false);
                }
        }
    }
}
