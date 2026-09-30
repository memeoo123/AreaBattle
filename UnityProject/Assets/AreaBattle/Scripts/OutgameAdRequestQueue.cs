using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameAdRequestCandidate
    {
        bool IsCacheRequest {get;}
        bool IsHighPriority {get;}
        void Handle();
    }
    // Request portion of BaseController.showAd (54851), before platform/cache selection.
    public sealed class OutgameAdRequestQueue
    {
        public List<IOutgameAdRequestCandidate> Pending=new List<IOutgameAdRequestCandidate>();
        readonly Func<IEnumerable<IOutgameAdRequestCandidate>> getPlatList;
        readonly Func<string> adType;
        readonly Action reportRotaRequestAd;
        public OutgameAdRequestQueue(Func<IEnumerable<IOutgameAdRequestCandidate>> getPlatList,
            Func<string> adType,Action reportRotaRequestAd)
        {
            this.getPlatList=getPlatList??throw new ArgumentNullException(nameof(getPlatList));
            this.adType=adType??throw new ArgumentNullException(nameof(adType));
            this.reportRotaRequestAd=reportRotaRequestAd??throw new ArgumentNullException(nameof(reportRotaRequestAd));
        }
        public bool TryRequest(bool isFromRealRequest)
        {
            if(!isFromRealRequest)
            {
                Pending.Clear();
                foreach(var adapter in getPlatList())
                    if(!adapter.IsCacheRequest&&!Pending.Contains(adapter)&&adapter.IsHighPriority)
                        Pending.Add(adapter);
                foreach(var adapter in getPlatList())
                    if(!adapter.IsCacheRequest&&!Pending.Contains(adapter)&&!adapter.IsHighPriority)
                        Pending.Add(adapter);
                if(Pending.Count>=1&&adType().Contains("INTERSTITAL"))reportRotaRequestAd();
            }
            if(Pending.Count<1)return false;
            var first=Pending[0];
            first.Handle();
            Pending.Remove(first);
            return true;
        }
    }
}
