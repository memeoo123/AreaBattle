using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // BaseController.setAdz followed by SerialController.setAdz. Platform list is
    // the same mutable list read directly by showAd, including reentrant replacement.
    public sealed class OutgameSerialAdConfiguration
    {
        readonly OutgameAdShowFlow flow;
        readonly Action<string> log;
        readonly OutgameAdPlatformList<OutgameAdIdsInfo,IOutgameAdShowCandidate> platforms;
        public OutgameAdZoneConfig Zone {get;private set;}
        public OutgameSerialAdConfiguration(OutgameAdShowFlow flow,
            Func<string,int,IOutgameAdShowCandidate> create,
            Action<IOutgameAdShowCandidate,OutgameAdIdsInfo,OutgameAdZoneConfig> initialize,
            Func<IOutgameAdShowCandidate,bool> canRequest,Action<string> log)
        {
            this.flow=flow;this.log=log;
            platforms=new OutgameAdPlatformList<OutgameAdIdsInfo,IOutgameAdShowCandidate>(
                ()=>Zone.idsInfo,()=>Zone.zkey,(key,id)=>create(key,id.platformId),
                (adapter,id)=>initialize(adapter,id,Zone),canRequest,()=>flow.PlatList,list=>flow.PlatList=list);
        }
        public List<IOutgameAdShowCandidate> GetPlatforms()=>platforms.Get();
        public void SetAdz(OutgameAdZoneConfig zone)
        {
            flow.Prefix+=zone.zkey+"-Controller ";
            Zone=zone;flow.SourceConfig=zone;
            GetPlatforms();
            if(flow.CacheAmount>GetPlatforms().Count&&GetPlatforms().Count>=1)
            {
                flow.CacheAmount=GetPlatforms().Count;
                log(flow.Prefix+"adjust cacheAmount:"+flow.CacheAmount);
            }
            flow.Prefix="DAU- "+zone.zkey+"-SerialController ";
        }
    }
}
