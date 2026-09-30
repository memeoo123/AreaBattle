using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace AreaBattle
{
    // One shared legacy manager state and its recovered bundle pipeline. Startup/manifest operation is separate.
    public sealed class OutgameLegacyBundleRuntime
    {
        public readonly Dictionary<string,OutgameLegacyBundleResult> Loaded=new Dictionary<string,OutgameLegacyBundleResult>();
        public readonly Dictionary<string,IOutgameLegacyDownload> Downloads=new Dictionary<string,IOutgameLegacyDownload>();
        public readonly Dictionary<string,string> Errors=new Dictionary<string,string>();
        public readonly Dictionary<string,string[]> Dependencies=new Dictionary<string,string[]>();
        public readonly Dictionary<string,int> DuplicateRequests=new Dictionary<string,int>();
        public readonly List<IOutgameLegacyManagerOperation> Operations=new List<IOutgameLegacyManagerOperation>();
        public readonly List<OutgameLegacyBundleResult> PendingUnload=new List<OutgameLegacyBundleResult>();
        public AssetBundleManifest Manifest;
        public string[] ActiveVariants=new string[0];
        public bool DisableUnload,DebugEnabled;
        public int UnloadInterval=OutgameLegacyResourceDefaults.InitialAssetUnloadInterval;
        public ulong Offset {get;private set;}
        public readonly OutgameLegacyBundleRegistry Registry;
        public readonly OutgameLegacyBundleAcquisition Acquisition;
        public readonly OutgameLegacyBundleUnload Unloader;
        public readonly OutgameLegacyDownloadUpdate DownloadUpdate;
        readonly OutgameLegacyBundleVariants variants;readonly OutgameLegacyBundleRequests requests;readonly Action<string> errorLog;
        public OutgameLegacyBundleRuntime(Func<IOutgameLegacyBundleServices> services,Action<string> log,Action<string> warning,Action<string> error,Action<Exception> exception,
            Func<float> clock=null,Func<float> delta=null,Func<string,UnityWebRequest> create=null,Action<UnityWebRequest> send=null,Func<string,string[]> dependencyQuery=null)
        {
            errorLog=error;
            Registry=new OutgameLegacyBundleRegistry(Loaded,Errors,Dependencies);
            var resolver=new OutgameLegacyBundleResolver(services,value=>Offset=value,warning);
            var reuse=new OutgameLegacyBundleReuse(Dependencies,PendingUnload,Registry.GetLoadedAssetBundle);
            variants=new OutgameLegacyBundleVariants(()=>Manifest==null?null:Manifest.GetAllAssetBundlesWithVariant(),()=>ActiveVariants,warning);
            Unloader=new OutgameLegacyBundleUnload(Loaded,Dependencies,Registry.GetLoadedAssetBundle,PendingUnload,()=>DisableUnload,()=>UnloadInterval,delta??(()=>Time.deltaTime),()=>DebugEnabled,log);
            OutgameLegacyBundleDependencies dependencies=null;
            Acquisition=new OutgameLegacyBundleAcquisition(Loaded,Downloads,DuplicateRequests,reuse,resolver.ResolveUrl,()=>Manifest!=null,name=>dependencies.Load(name),error,create,send);
            dependencies=new OutgameLegacyBundleDependencies(Dependencies,()=>Manifest!=null,dependencyQuery??(name=>Manifest.GetAllDependencies(name)),variants.Remap,Acquisition.LoadInternal,()=>DebugEnabled,log,error);
            requests=new OutgameLegacyBundleRequests(()=>DebugEnabled,log,variants.Remap,Acquisition.Load,
                name=>new OutgameLegacyBundleOperation(name,GetRequest,Registry.GetLoadedAssetBundle,error,exception),Operations);
            DownloadUpdate=new OutgameLegacyDownloadUpdate(Downloads,Loaded,Errors,clock??(()=>Time.realtimeSinceStartup),log,requests.UpdateOperations,Unloader.Update);
        }
        // GetAssetBundleWWW23864: lookup captures the current request or null.
        public UnityWebRequest GetRequest(string name)=>Downloads.TryGetValue(name,out var download)?((OutgameLegacyWebDownload)download).Request:null;
        public IOutgameLegacyBundleOperation LoadAsync(string name)=>requests.LoadAsync(name);
        // LoadManifest23835 does not reset the previous Manifest while requesting its replacement.
        public OutgameLegacyManifestOperation LoadManifest(string name)
        {
            if(Loaded.ContainsKey(name)){Loaded[name].Bundle.Unload(true);Loaded.Remove(name);}
            Acquisition.Load(name,true);
            var operation=new OutgameLegacyManifestOperation(name,Registry.GetLoadedAssetBundle,value=>Manifest=value,errorLog);
            Operations.Add(operation);return operation;
        }
        // AssetBundleManager.LoadAssetAsync23827: check, remap, retain/acquire, register operation.
        public OutgameLegacyAssetOperation LoadAssetAsync(string bundleName,string assetName,Type type,Func<bool> checkLegacy)
        {
            checkLegacy();var mapped=variants.Remap(bundleName);Acquisition.Load(mapped,false);
            var operation=new OutgameLegacyAssetOperation(mapped,assetName,type,Registry.GetLoadedAssetBundle,errorLog);
            Operations.Add(operation);return operation;
        }
        public void Update()=>DownloadUpdate.Update();
        public void Unload(string name)=>Unloader.Unload(name);
    }
}
