using System.Collections.Generic;
namespace AreaBattle
{
    // AssetBundleManager23863, called by the loaded-resource branch of23836.
    public sealed class OutgameLegacyBundleReuse
    {
        readonly IDictionary<string,string[]> dependencies;readonly IList<OutgameLegacyBundleResult> pending;
        readonly OutgameLegacyBundleLookup lookup;
        public OutgameLegacyBundleReuse(IDictionary<string,string[]> dependencies,IList<OutgameLegacyBundleResult> pending,OutgameLegacyBundleLookup lookup)
        {this.dependencies=dependencies;this.pending=pending;this.lookup=lookup;}
        public void Retain(OutgameLegacyBundleResult item)
        {
            RetainOne(item);
            if(dependencies.TryGetValue(item.BundleName,out var names))
            {
                foreach(var name in names)
                {
                    var dependency=lookup(name,out var error,out var missing);
                    if(dependency!=null)RetainOne(dependency);
                }
            }
        }
        void RetainOne(OutgameLegacyBundleResult item)
        {
            item.ReferenceCount++;item.UnloadRemaining=2147483648f;
            if(pending.Contains(item))pending.Remove(item);
        }
    }
}
