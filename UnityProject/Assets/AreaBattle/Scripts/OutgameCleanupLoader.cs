using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Loader2938.TryDestroyAllProviders22963 / CanDestroy22962. Native bundle Destroy remains supplied by its concrete loader.
    public sealed class OutgameCleanupLoader:IOutgameCleanupLoader
    {
        public int Status {get;set;}public string RegistryKey {get;}
        public readonly OutgameBundleReference Reference;public readonly List<OutgameAssetProvider> Providers;
        readonly int[] dependents;readonly Func<int,bool> dependentReleased;readonly Action<List<OutgameAssetProvider>> unregister;readonly Action destroy;
        public OutgameCleanupLoader(string key,OutgameBundleReference reference,List<OutgameAssetProvider> providers,int[] dependents,
            Func<int,bool> dependentReleased,Action<List<OutgameAssetProvider>> unregister,Action destroy)
        {RegistryKey=key;Reference=reference;Providers=providers;this.dependents=dependents;this.dependentReleased=dependentReleased;this.unregister=unregister;this.destroy=destroy;}
        bool IsDone=>Status==1||Status==2;
        public void TryDestroyAllProviders()
        {
            if(!IsDone)return;
            foreach(var provider in Providers)if(!provider.CanDestroy)return;
            if(Providers.Count<Reference.RefCount)return;
            foreach(var provider in Providers)provider.Destroy();
            unregister(Providers);Providers.Clear();
        }
        public bool CanDestroy()
        {
            if(!IsDone||Reference.RefCount>0)return false;
            foreach(int id in dependents)if(!dependentReleased(id))return false;
            return true;
        }
        public void Destroy()=>destroy();
    }
}
