using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameLegacyAssetRequest
    {
        bool IsDone {get;} float Progress {get;} UnityEngine.Object Asset {get;}
    }
    public sealed class OutgameLegacyNativeAssetRequest:IOutgameLegacyAssetRequest
    {
        readonly AssetBundleRequest request;
        public OutgameLegacyNativeAssetRequest(AssetBundleRequest request){this.request=request;}
        public bool IsDone=>request.isDone;
        public float Progress=>request.progress;
        public UnityEngine.Object Asset=>request.asset;
    }
    // AssetBundleLoadManifestOperation23794 with base23789/23791/23792.
    public sealed class OutgameLegacyManifestOperation:IEnumerator,IOutgameLegacyManagerOperation
    {
        readonly string name;readonly OutgameLegacyBundleLookup lookup;
        readonly Func<AssetBundle,IOutgameLegacyAssetRequest> load;
        readonly Action<AssetBundleManifest> publish;readonly Action<string> errorLog;
        IOutgameLegacyAssetRequest request;string error;
        public OutgameLegacyManifestOperation(string name,OutgameLegacyBundleLookup lookup,Action<AssetBundleManifest> publish,Action<string> errorLog,
            Func<AssetBundle,IOutgameLegacyAssetRequest> load=null)
        {this.name=name;this.lookup=lookup;this.publish=publish;this.errorLog=errorLog;
            this.load=load??(bundle=>new OutgameLegacyNativeAssetRequest(bundle.LoadAssetAsync("AssetBundleManifest",typeof(AssetBundleManifest))));}
        public bool Update()
        {
            // Base.Update is called even when the derived operation has already finished.
            if(request==null)
            {
                var bundle=lookup(name,out error,out var missing);
                if(bundle!=null)request=load(bundle.Bundle);
            }
            if(request!=null&&request.IsDone){publish(GetAsset());return false;}
            return true;
        }
        // Shared generic23788 checks completion again before the safe reference-type cast.
        public AssetBundleManifest GetAsset()=>request!=null&&request.IsDone?request.Asset as AssetBundleManifest:null;
        public bool IsDone()
        {
            if(request!=null)return request.IsDone;
            if(error==null)return false;
            errorLog(error);return true;
        }
        public float Progress()=>request==null?0f:request.Progress;
        public object Current=>null;
        public bool MoveNext()=>!IsDone();
        public void Reset(){}
    }
}
