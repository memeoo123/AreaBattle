using System;
using UnityEngine;
namespace AreaBattle
{
    // AssetResLoader29849-29854. The package coroutine/transport is supplied explicitly.
    public sealed class OutgameLegacyAssetLoader:OutgameLegacyResLoader
    {
        readonly Action<OutgameLegacyAssetLoader> loadBundle;readonly Action<string> unloadBundle;
        public AssetBundle PendingBundle;
        public bool HasError;
        public OutgameLegacyAssetLoader(Action<OutgameLegacyAssetLoader> loadBundle,Action<string> unloadBundle)
        {this.loadBundle=loadBundle;this.unloadBundle=unloadBundle;}
        public override void Start(bool immediate)
        {
            base.Start(false);
            if(HasError){State=2;Error();return;}
            switch(State)
            {
                case 0:State=1;if(!immediate){Module.Enqueue(this);return;}Complete();return;
                case 2:Error();return;
                case 3:Complete();return;
            }
        }
        public override void LoadBundle()=>loadBundle(this);
        public override void Complete()
        {
            if(Resource==null)
            {
                State=3;Resource=Module.CreateResource(this,null,PendingBundle);
                Resource.IsReady=true;Resource.OnUnloaded=Unloaded;PendingBundle=null;
            }
            Resource.Arguments=Arguments;base.Complete();
        }
        public override void Error(){base.Error();State=0;}
        void Unloaded(OutgameLegacyPrefabResource resource)
        {Resource=null;State=0;unloadBundle(BundleName);}
    }
}
