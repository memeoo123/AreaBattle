using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameCleanupLoader
    {
        void TryDestroyAllProviders();bool CanDestroy();string RegistryKey {get;}void Destroy();
    }
    // Resource system2927 method22864: two reverse scans, then destroy/list removal/dictionary removal.
    public sealed class OutgameResourceCleanupPass
    {
        readonly IList<IOutgameCleanupLoader> loaders;readonly IDictionary<string,IOutgameCleanupLoader> index;
        public OutgameResourceCleanupPass(IList<IOutgameCleanupLoader> loaders,IDictionary<string,IOutgameCleanupLoader> index)
        {this.loaders=loaders;this.index=index;}
        public void Run()
        {
            for(int i=loaders.Count-1;i>=0;i--)loaders[i].TryDestroyAllProviders();
            for(int i=loaders.Count-1;i>=0;i--)
            {
                var loader=loaders[i];if(!loader.CanDestroy())continue;
                string key=loader.RegistryKey;loader.Destroy();loaders.RemoveAt(i);index.Remove(key);
            }
        }
    }
}
