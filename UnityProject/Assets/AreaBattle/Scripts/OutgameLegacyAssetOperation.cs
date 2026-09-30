using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    // AssetBundleLoadAssetOperationFull23789/23791/23792 and generic23788.
    public sealed class OutgameLegacyAssetOperation:IEnumerator,IOutgameLegacyManagerOperation
    {
        readonly string bundleName,assetName;readonly Type type;readonly OutgameLegacyBundleLookup lookup;
        readonly Func<AssetBundle,string,Type,IOutgameLegacyAssetRequest> load;readonly Action<string> errorLog;
        IOutgameLegacyAssetRequest request;string error;
        public OutgameLegacyAssetOperation(string bundleName,string assetName,Type type,OutgameLegacyBundleLookup lookup,Action<string> errorLog,
            Func<AssetBundle,string,Type,IOutgameLegacyAssetRequest> load=null)
        {this.bundleName=bundleName;this.assetName=assetName;this.type=type;this.lookup=lookup;this.errorLog=errorLog;
            this.load=load??((bundle,name,t)=>new OutgameLegacyNativeAssetRequest(bundle.LoadAssetAsync(name,t)));}
        public bool Update()
        {
            if(request!=null)return false;
            var bundle=lookup(bundleName,out error,out var missing);
            if(bundle==null)return true;
            request=load(bundle.Bundle,assetName,type);return false;
        }
        public bool IsDone(){if(request!=null)return request.IsDone;if(error==null)return false;errorLog(error);return true;}
        public float Progress()=>request==null?0:request.Progress;
        public T GetAsset<T>()where T:UnityEngine.Object=>request!=null&&request.IsDone?request.Asset as T:null;
        public object Current=>null;public bool MoveNext()=>!IsDone();public void Reset(){}
    }
}
