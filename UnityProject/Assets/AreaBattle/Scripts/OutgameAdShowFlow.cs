using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameAdShowCandidate:IOutgameAdCacheCandidate,IOutgameAdRequestCandidate
    {
        int SourceState {get;}
        bool CanShow();
        bool WaitCloseReportShow {get;}
        void ShowAd();
    }
    public interface IOutgameAdShowEnvironment
    {
        string Channel {get;}
        bool InterVideoShowing {get;}
        void Log(string text);
        void ReportIntersRequest();
        void ReportRotaRequestAd();
        void ReportRotaRequestAdFail();
    }
    // BaseController.showAd/isShowing/removeCacheList. Adapter events and native SDK
    // initialization are separate lifecycle responsibilities, not synthesized here.
    public sealed class OutgameAdShowFlow
    {
        readonly IOutgameAdShowEnvironment environment;
        readonly Func<IEnumerable<IOutgameAdShowCandidate>> getPlatList;
        public readonly OutgameAdRequestQueue Requests;
        public readonly OutgameAdCacheSelection Selection=new OutgameAdCacheSelection();
        public List<IOutgameAdShowCandidate> PlatList=new List<IOutgameAdShowCandidate>();
        public List<IOutgameAdCacheCandidate> Cache=new List<IOutgameAdCacheCandidate>();
        string adType;
        public OutgameAdZoneConfig SourceConfig;
        public string AdType {get=>SourceConfig==null?adType:SourceConfig.zkey;set=>adType=value;}
        public string Prefix="DAU- ";
        public int BiddingEnable;public int CacheAmount=1;
        public float VideoShowTime=-1;
        public Action<bool> CloseCallback;
        public OutgameAdShowFlow(IOutgameAdShowEnvironment environment,
            Func<IEnumerable<IOutgameAdShowCandidate>> getPlatList=null)
        {
            this.environment=environment??throw new ArgumentNullException(nameof(environment));
            this.getPlatList=getPlatList??(()=>PlatList);
            Requests=new OutgameAdRequestQueue(()=>this.getPlatList(),()=>AdType,environment.ReportRotaRequestAd);
        }
        public bool IsShowing()
        {
            foreach(var adapter in getPlatList())if(adapter.SourceState==4)return true;
            return false;
        }
        public void ShowAd(bool isFromRealRequest=false,bool preferPlatform=false)
        {
            if(!preferPlatform&&!isFromRealRequest&&AdType.Contains("INTERSTITAL"))
                environment.ReportIntersRequest();
            if(IsShowing())
            {
                environment.Log(Prefix+"show fail, already showing");
                if(AdType=="VIDEO")CloseCallback?.Invoke(false);
                return;
            }
            if(environment.InterVideoShowing)
            {
                environment.Log(Prefix+"inter or video showing return");
                if(AdType=="VIDEO")CloseCallback?.Invoke(false);
                return;
            }
            environment.Log(Prefix+"show cacheCount:"+Cache.Count+" isFromRealRequest:"+isFromRealRequest+" realReqCount:"+Requests.Pending.Count);
            if(Requests.TryRequest(isFromRealRequest))return;
            IOutgameAdShowCandidate fallback=null;
            if(environment.Channel=="weixin")
                foreach(var adapter in PlatList)
                    if(adapter.IsGuarantee&&adapter.SourceState==2&&adapter.CanShow())
                    {fallback=adapter;break;}
            if(isFromRealRequest&&fallback==null&&Requests.Pending.Count==0)
            {
                if(AdType=="INTERSTITAL4")CloseCallback?.Invoke(true);
                environment.ReportRotaRequestAdFail();
            }
            var selected=(IOutgameAdShowCandidate)Selection.Select(Cache,fallback,preferPlatform,
                BiddingEnable,CacheAmount,Prefix,environment.Log);
            if(selected==null||selected.SourceState!=2)return;
            if(AdType=="VIDEO")VideoShowTime=-1;
            selected.ShowAd();
            if(Cache.Contains(selected))Cache.Remove(selected);
        }
    }
}
