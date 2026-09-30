using System.Collections.Generic;
namespace AreaBattle
{
    // Bundle reference field16; dependency group2939.Reference/Release22973/22969.
    public sealed class OutgameBundleReference
    {
        public int RefCount {get;set;}
    }
    public sealed class OutgameBundleDependencies
    {
        readonly List<OutgameBundleReference> bundles;
        public OutgameBundleDependencies(List<OutgameBundleReference> bundles){this.bundles=bundles;}
        public void Reference(){foreach(var bundle in bundles)bundle.RefCount=unchecked(bundle.RefCount+1);}
        public void Release(){foreach(var bundle in bundles)bundle.RefCount=unchecked(bundle.RefCount-1);}
    }
}
