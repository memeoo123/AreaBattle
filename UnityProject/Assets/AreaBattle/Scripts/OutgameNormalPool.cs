using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgamePoolAssetHandle
    {
        UnityEngine.Object MainObject {get;}
        GameObject Instantiate();
    }
    public interface IOutgamePoolResourceInfo
    {
        UnityEngine.Object MainObject {get;}
        string BundleName {get;}
        GameObject Instantiate(string intactName);
    }
    public interface IOutgameNormalPoolResources
    {
        bool UseNewResourceLoader {get;}
        void LoadOriginal(string path,Action<IOutgamePoolAssetHandle> complete);
        void LoadResource(string path,Action<IOutgamePoolResourceInfo> complete);
        void LoadSynchronousPrefab(string path,Action<GameObject> complete);
        string GetIntactBundleName(string assetName,string bundleName);
        void ReleaseUIForm(GameObject target);
        void Warning(string message);
    }
    public interface IOutgameNormalPoolLifetime
    {
        void DestroyRegisteredPoolIfManagerExists(string name);
        void DestroyRoot(GameObject root);
        void ReleaseHandle(IOutgamePoolAssetHandle handle);
        void UnloadUnusedAssets();
        void UnloadUnusedBundle(IOutgamePoolResourceInfo info);
    }
    // NormalPool.Spawn28685, helpers28681/28684 and callbacks28687..28690.
    // Source destruction delegates manager/resource operations to explicit lifetime provider.
    public sealed class OutgameNormalPool
    {
        const string InvalidResource="添加对象出错，ResourcesInfo为null 或者ResourcesInfo的对象不是GameObject";
        readonly OutgameObjectPool pool;readonly Transform root;readonly bool cacheResources;
        readonly IOutgameNormalPoolResources resources;
        readonly Dictionary<string,IOutgamePoolAssetHandle> handles=new Dictionary<string,IOutgamePoolAssetHandle>();
        readonly Dictionary<string,IOutgamePoolResourceInfo> infos=new Dictionary<string,IOutgamePoolResourceInfo>();
        public OutgameNormalPool(OutgameObjectPool pool,Transform root,bool cacheResources,IOutgameNormalPoolResources resources)
        {this.pool=pool;this.root=root;this.cacheResources=cacheResources;this.resources=resources;}
        public void DestroyObjectPool(IOutgameNormalPoolLifetime lifetime)
        {
            lifetime.DestroyRegisteredPoolIfManagerExists(pool.Name);
            if(root!=null)lifetime.DestroyRoot(root.gameObject);
            if(resources.UseNewResourceLoader){
                foreach(var handle in handles.Values)lifetime.ReleaseHandle(handle);
                lifetime.UnloadUnusedAssets();
            }else{
                foreach(var info in infos.Values)lifetime.UnloadUnusedBundle(info);
            }
            // Original28679 clears only the legacy ResourcesInfo dictionary.
            infos.Clear();
        }
        public void Unspawn(GameObject target)=>pool.Unspawn(target);
        void Register(string name,GameObject target,Action<GameObject> complete)
        {
            var item=new OutgamePooledPrefab(name,null,target,root,resources.ReleaseUIForm);
            pool.Register(item,true);complete?.Invoke(item.Target);
        }
        void FromHandle(string name,IOutgamePoolAssetHandle handle,Action<GameObject> complete)
        {
            if(handle==null||ReferenceEquals(handle.MainObject as GameObject,null)){
                resources.Warning(InvalidResource);complete?.Invoke(null);
                // Source continues after notifying; do not silently turn this into an early return.
            }
            Register(name,handle.Instantiate(),complete);
        }
        void FromInfo(string path,IOutgamePoolResourceInfo info,Action<GameObject> complete)
        {
            if(info==null||ReferenceEquals(info.MainObject as GameObject,null)){
                resources.Warning(InvalidResource);complete?.Invoke(null);
            }
            string name=resources.GetIntactBundleName(path+".prefab",info.BundleName);
            Register(name,info.Instantiate(name),complete);
        }
        public void Spawn(string path,Action<GameObject> complete)
        {
            var existing=pool.Spawn(path);
            if(existing!=null){complete?.Invoke(existing.Target);return;}
            if(cacheResources){
                if(resources.UseNewResourceLoader){
                    if(handles.TryGetValue(path,out var handle)){FromHandle(path,handle,complete);return;}
                    resources.LoadOriginal(path,loaded=>{
                        if(loaded==null)return;
                        if(!handles.ContainsKey(path))handles.Add(path,loaded);
                        FromHandle(path,loaded,complete);
                    });
                }else{
                    if(infos.TryGetValue(path,out var info)){FromInfo(path,info,complete);return;}
                    resources.LoadResource(path,loaded=>{
                        if(loaded==null)return;
                        if(!infos.ContainsKey(path))infos.Add(path,loaded);
                        FromInfo(path,loaded,complete);
                    });
                }
                return;
            }
            if(resources.UseNewResourceLoader){
                resources.LoadOriginal(path,handle=>{var target=handle.Instantiate();if(target!=null)Register(path,target,complete);});
            }else{
                resources.LoadSynchronousPrefab(path,target=>{if(target!=null)Register(path,target,complete);});
            }
        }
    }
}
