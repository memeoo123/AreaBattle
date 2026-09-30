using System.Collections.Generic;
namespace AreaBattle
{
    // AssetBundleManager.GetLoadedAssetBundle23812. Dictionaries are shared with manager acquisition.
    public sealed class OutgameLegacyBundleRegistry
    {
        readonly IDictionary<string,OutgameLegacyBundleResult> loaded;
        readonly IDictionary<string,string> errors;
        readonly IDictionary<string,string[]> dependencies;
        public OutgameLegacyBundleRegistry(IDictionary<string,OutgameLegacyBundleResult> loaded,
            IDictionary<string,string> errors,IDictionary<string,string[]> dependencies)
        {this.loaded=loaded;this.errors=errors;this.dependencies=dependencies;}
        public OutgameLegacyBundleResult GetLoadedAssetBundle(string name,out string error,out int dependencyMissing)
        {
            dependencyMissing=0;
            if(errors.TryGetValue(name,out error))return null;
            loaded.TryGetValue(name,out var result);
            if(result==null)return null;
            if(dependencies.TryGetValue(name,out var required))
            {
                foreach(var dependency in required)
                {
                    // Original rechecks the requested name, not the dependency's error key.
                    if(errors.TryGetValue(name,out error))break;
                    loaded.TryGetValue(dependency,out var item);
                    if(item==null){dependencyMissing=1;return null;}
                }
            }
            return result;
        }
    }
}
