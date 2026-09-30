using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Resource system2927 unregister22860: list removal precedes key lookup and dictionary removal.
    public sealed class OutgameProviderRegistry
    {
        readonly IList<OutgameAssetProvider> providers;readonly IDictionary<string,OutgameAssetProvider> index;
        readonly Func<OutgameAssetProvider,string> key;
        public OutgameProviderRegistry(IList<OutgameAssetProvider> providers,IDictionary<string,OutgameAssetProvider> index,Func<OutgameAssetProvider,string> key)
        {this.providers=providers;this.index=index;this.key=key;}
        public void Unregister(IEnumerable<OutgameAssetProvider> removed)
        {foreach(var provider in removed){providers.Remove(provider);index.Remove(key(provider));}}
    }
}
