using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameAdPreloadCandidate
    {
        int SourceState {get;}
        bool IsCacheRequest {get;}
        OutgameAdListener Listener {get;set;}
        void Handle();
    }
    public sealed class OutgameSerialAdPreload
    {
        readonly OutgameAdShowFlow flow;
        readonly Func<int> getPlatListCount;
        readonly Func<IEnumerator,object> start;
        readonly Action<object> stop;
        readonly Action load;
        public List<IOutgameAdPreloadCandidate> RequestList=new List<IOutgameAdPreloadCandidate>();
        public OutgameAdListener Listener;
        public object VideoShowRoutine {get;private set;}
        public OutgameSerialAdPreload(OutgameAdShowFlow flow,Func<int> getPlatListCount,
            Func<IEnumerator,object> start,Action<object> stop,Action load)
        {this.flow=flow;this.getPlatListCount=getPlatListCount;this.start=start;this.stop=stop;this.load=load;}
        public void VideoShowLoad()
        {
            if(getPlatListCount()<2)return;
            flow.CacheAmount=getPlatListCount()-1;StopVideoShowLoad();
            VideoShowRoutine=start(VideoShowDelayLoad());
        }
        IEnumerator VideoShowDelayLoad(){yield return new WaitForSeconds(2);load();}
        public void StopVideoShowLoad()
        {
            var routine=VideoShowRoutine;
            if(routine!=null){stop(routine);VideoShowRoutine=null;}
        }
        public bool IsRequestEnd()=>RequestList==null||RequestList.Count==0;
        public int GetNowCacheNum(IEnumerable<IOutgameAdPreloadCandidate> adapters)
        {
            int count=0;foreach(var adapter in adapters)if(adapter.SourceState==2)count++;
            return count;
        }
        public void InitRequestList(IEnumerable<IOutgameAdPreloadCandidate> adapters)
        {
            RequestList.Clear();
            foreach(var adapter in adapters)
            {
                if(!adapter.IsCacheRequest)continue;
                if(adapter.SourceState!=0&&adapter.SourceState!=3&&adapter.SourceState!=5)continue;
                RequestList.Add(adapter);
                if(flow.CacheAmount==1&&flow.AdType!="BANNER")break;
            }
        }
        public void RequestAdapters(Func<IEnumerable<IOutgameAdPreloadCandidate>> getPlatList,Action reportRotaRequestAd)
        {
            foreach(var adapter in getPlatList())
                if(adapter.Listener==null&&!adapter.IsCacheRequest)adapter.Listener=Listener;
            InitRequestList(getPlatList());
            if(RequestList.Count==0)return;
            if(flow.AdType!="BANNER")reportRotaRequestAd();
            SerialLoad();
        }
        public void Load(Func<IEnumerable<IOutgameAdPreloadCandidate>> getPlatList,Action reportRotaRequestAd,Action<string> log)
        {log(flow.Prefix+"load ");RequestAdapters(getPlatList,reportRotaRequestAd);}
        public void SerialLoad()
        {
            foreach(var adapter in RequestList)
            {
                if(adapter.Listener==null)adapter.Listener=Listener;
                if(adapter.SourceState==0||adapter.SourceState==3||adapter.SourceState==5)
                {RequestList.Remove(adapter);adapter.Handle();return;}
            }
        }
        public void CheckRequest(Action printAllState,OutgameAdRequestCompletion completion,Action<string> log)
        {CheckRequest(printAllState,IsRequestEnd,completion.GetNowCacheNum,completion.CheckRequest,log);}
        public void CheckRequest(Action printAllState,Func<bool> isRequestEnd,Func<int> getNowCacheNum,Action baseCheckRequest,Action<string> log)
        {
            printAllState();
            if(isRequestEnd()||getNowCacheNum()>=flow.CacheAmount){baseCheckRequest();return;}
            log(flow.Prefix+"loadNext ");SerialLoad();
        }
    }
}
