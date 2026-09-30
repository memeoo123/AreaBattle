using System;
using System.Collections.Generic;
using UnityEngine.Networking;
namespace AreaBattle
{
    // AssetBundleManager.LoadAssetBundle23824 and LoadAssetBundleInternal23836.
    public sealed class OutgameLegacyBundleAcquisition
    {
        readonly IDictionary<string,OutgameLegacyBundleResult> loaded;readonly IDictionary<string,IOutgameLegacyDownload> downloading;
        readonly IDictionary<string,int> duplicateRequests;readonly OutgameLegacyBundleReuse reuse;
        readonly Func<string,string> resolveUrl;readonly Func<bool> hasManifest;readonly Action<string> loadDependencies,error;
        readonly Func<string,UnityWebRequest> create;readonly Action<UnityWebRequest> send;
        public OutgameLegacyBundleAcquisition(IDictionary<string,OutgameLegacyBundleResult> loaded,IDictionary<string,IOutgameLegacyDownload> downloading,
            IDictionary<string,int> duplicateRequests,OutgameLegacyBundleReuse reuse,Func<string,string> resolveUrl,Func<bool> hasManifest,
            Action<string> loadDependencies,Action<string> error,Func<string,UnityWebRequest> create=null,Action<UnityWebRequest> send=null)
        {this.loaded=loaded;this.downloading=downloading;this.duplicateRequests=duplicateRequests;this.reuse=reuse;this.resolveUrl=resolveUrl;this.hasManifest=hasManifest;this.loadDependencies=loadDependencies;this.error=error;
            this.create=create??(url=>UnityWebRequestAssetBundle.GetAssetBundle(url));this.send=send??(request=>request.SendWebRequest());}
        public void Load(string name,bool manifest)
        {
            if(!manifest&&!hasManifest()){error("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");return;}
            if(!LoadInternal(name,true)&&!manifest)loadDependencies(name);
        }
        // The unused second source parameter is omitted; the third controls duplicate-request accounting.
        public bool LoadInternal(string name,bool suppressDuplicateReference)
        {
            loaded.TryGetValue(name,out var item);
            if(item!=null){reuse.Retain(item);return true;}
            if(downloading.ContainsKey(name))
            {
                if(!suppressDuplicateReference)
                {if(duplicateRequests.ContainsKey(name))duplicateRequests[name]++;else duplicateRequests.Add(name,1);}
                return true;
            }
            var url=resolveUrl(name);
            if(!string.IsNullOrEmpty(url))
            {
                var request=create(url);downloading.Add(name,new OutgameLegacyWebDownload(request));send(request);
            }
            return false;
        }
    }
}
