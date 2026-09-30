using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // UIObject27441/27442: dictionary<Object,AssetOperationHandle>, not a list of asset paths.
    public sealed class OutgameUiResourceLists
    {
        public Dictionary<UnityEngine.Object,OutgameAssetHandle> Dynamic {get;set;}
        public Dictionary<UnityEngine.Object,OutgameAssetHandle> Custom {get;set;}
        readonly Action unloadUnused;
        public OutgameUiResourceLists(Action unloadUnused){this.unloadUnused=unloadUnused;}
        public void ReleaseDynamic()
        {
            if(Dynamic==null)return;
            foreach(var pair in Dynamic)pair.Value?.Release();
            unloadUnused();Dynamic=null;
        }
        public void ReleaseCustom()
        {
            if(Custom==null)return;
            foreach(var pair in Custom)pair.Value?.Release();
            unloadUnused();Custom=null;
        }
    }
}
