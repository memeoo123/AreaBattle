using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameAdCacheCandidate
    {
        int Priority { get; }
        int PlatformId { get; }
        int GetRealPrice();
        bool IsGuarantee { get; }
    }
    // BaseController.showAd cache branch (54851). Request queues and platform fallback
    // remain the caller's responsibility. The default overload uses the source price policy.
    public sealed class OutgameAdCacheSelection
    {
        public int LastShowPriority = -1;
        public static int ComparePriority(IOutgameAdCacheCandidate x,IOutgameAdCacheCandidate y)
        {
            if(x==null||y==null)return 0;
            int a=x.Priority,b=y.Priority;
            return a<b?-1:a>b?1:0;
        }
        public static int ComparePrice(IOutgameAdCacheCandidate x,IOutgameAdCacheCandidate y)
        {
            if(x==null||y==null)return 0;
            int a=x.GetRealPrice(),b=y.GetRealPrice();
            return a<b?-1:a>b?1:0;
        }
        public static bool AllPlatHasPrice(List<IOutgameAdCacheCandidate> cache,string prefix,Action<string> log)
        {
            string text="";
            foreach(var adapter in cache)
            {
                text=text+adapter.PlatformId+" "+adapter.GetRealPrice()+" ";
                if(adapter.GetRealPrice()==0){log(prefix+"show allListStr:"+text);return false;}
            }
            log(prefix+"show allListStr:"+text);return true;
        }
        public IOutgameAdCacheCandidate Select(List<IOutgameAdCacheCandidate> cache,
            IOutgameAdCacheCandidate platformFallback,bool preferPlatform,int biddingEnable,
            int cacheAmount,string prefix,Action<string> log)
        {
            return Select(cache,platformFallback,preferPlatform,biddingEnable,cacheAmount,
                list=>AllPlatHasPrice(list,prefix,log),prefix,log);
        }
        public IOutgameAdCacheCandidate Select(List<IOutgameAdCacheCandidate> cache,
            IOutgameAdCacheCandidate platformFallback,bool preferPlatform,int biddingEnable,
            int cacheAmount,Func<List<IOutgameAdCacheCandidate>,bool> allPlatHasPrice,
            string prefix,Action<string> log)
        {
            IOutgameAdCacheCandidate selected=null;
            if(cache.Count>=1&&!(platformFallback!=null&&preferPlatform))
            {
                if(biddingEnable==1&&cache.Count==cacheAmount&&allPlatHasPrice(cache))
                {
                    cache.Sort(ComparePrice);
                    selected=cache[cache.Count-1];
                    log(prefix+"show RealPrice:"+selected.GetRealPrice());
                }
                else
                {
                    cache.Sort(ComparePriority);
                    if(LastShowPriority==-1&&!cache[0].IsGuarantee)
                    {
                        selected=cache[0];LastShowPriority=selected.Priority;
                    }
                    else
                    {
                        for(int i=0;i<cache.Count;i++)
                            if(LastShowPriority<cache[i].Priority&&!cache[i].IsGuarantee)
                            {selected=cache[i];LastShowPriority=selected.Priority;break;}
                        if(selected==null&&!cache[0].IsGuarantee)
                        {selected=cache[0];LastShowPriority=selected.Priority;}
                    }
                }
            }
            return selected??platformFallback;
        }
    }
}
