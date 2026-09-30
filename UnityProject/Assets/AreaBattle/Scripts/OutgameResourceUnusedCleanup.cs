using System;
namespace AreaBattle
{
    // Resource system2927.UnloadUnusedAssets22872; internal cleanup pass22864 remains supplied by the resource owner.
    public sealed class OutgameResourceUnusedCleanup
    {
        readonly Func<bool> sourceFlag24;readonly Action cleanupPass;readonly Action<string> warning;
        public OutgameResourceUnusedCleanup(Func<bool> sourceFlag24,Action cleanupPass,Action<string> warning)
        {this.sourceFlag24=sourceFlag24;this.cleanupPass=cleanupPass;this.warning=warning;}
        public void UnloadUnusedAssets()
        {
            if(!sourceFlag24()){warning("Can not unload unused assets when processing resource loading !");return;}
            for(int i=0;i<10;i++)cleanupPass();
        }
    }
    // YOResourcesModule27207 -> ResourcePackage23609; no coalescing/delay is present at these layers.
    public sealed class OutgameResourcePackageUnload
    {
        readonly Func<bool> hasPackage;readonly Action updateResourceSystem,unloadResourceSystem;
        public OutgameResourcePackageUnload(Func<bool> hasPackage,Action updateResourceSystem,Action unloadResourceSystem)
        {this.hasPackage=hasPackage;this.updateResourceSystem=updateResourceSystem;this.unloadResourceSystem=unloadResourceSystem;}
        public void UnloadUnusedAssets(){if(!hasPackage())return;updateResourceSystem();unloadResourceSystem();}
    }
}
