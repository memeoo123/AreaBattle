using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // BaseController.getPlatList. A nonempty list is retained; an empty result retries.
    // Platform factory and adapter init/canRequest remain source-specific services.
    public sealed class OutgameAdPlatformList<TId,TAdapter> where TAdapter:class
    {
        readonly Func<IEnumerable<TId>> ids;
        readonly Func<string> zoneKey;
        readonly Func<string,TId,TAdapter> create;
        readonly Action<TAdapter,TId> initialize;
        readonly Func<TAdapter,bool> canRequest;
        readonly Func<List<TAdapter>> readItems;
        readonly Action<List<TAdapter>> writeItems;
        List<TAdapter> items=new List<TAdapter>();
        public List<TAdapter> Items
        {get=>readItems==null?items:readItems();set{if(writeItems==null)items=value;else writeItems(value);}}
        public OutgameAdPlatformList(Func<IEnumerable<TId>> ids,Func<string> zoneKey,
            Func<string,TId,TAdapter> create,Action<TAdapter,TId> initialize,Func<TAdapter,bool> canRequest,
            Func<List<TAdapter>> readItems=null,Action<List<TAdapter>> writeItems=null)
        {
            if((readItems==null)!=(writeItems==null))throw new ArgumentException("List accessors must be provided together");
            this.ids=ids;this.zoneKey=zoneKey;this.create=create;this.initialize=initialize;this.canRequest=canRequest;
            this.readItems=readItems;this.writeItems=writeItems;
        }
        public List<TAdapter> Get()
        {
            if(Items==null)Items=new List<TAdapter>();
            if(Items.Count<=0)
                foreach(var id in ids())
                {
                    string key=zoneKey();
                    if(!string.IsNullOrEmpty(key)&&key.Contains("INTERSTITAL"))key="INTERSTITAL";
                    var adapter=create(key,id);
                    if(adapter==null)continue;
                    initialize(adapter,id);
                    if(canRequest(adapter))Items.Add(adapter);
                }
            return Items;
        }
    }
}
