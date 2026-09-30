using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // ResourcesModule29880-29892/29893/29905. Bundle acquisition belongs to the concrete loader.
    public sealed class OutgameLegacyResourceScheduler
    {
        readonly Func<bool> modern;readonly Action checkResourceRoute;readonly Action<string> warning;
        readonly Func<bool,OutgameLegacyResLoader> create;
        readonly List<OutgameLegacyResLoader> requestQueue=new List<OutgameLegacyResLoader>();
        readonly HashSet<OutgameLegacyResLoader> newLoaders=new HashSet<OutgameLegacyResLoader>();
        readonly HashSet<OutgameLegacyResLoader> currentLoaders=new HashSet<OutgameLegacyResLoader>();
        readonly Dictionary<string,OutgameLegacyResLoader> cache=new Dictionary<string,OutgameLegacyResLoader>();
        readonly Dictionary<string,OutgameLegacyPrefabResource> loaded=new Dictionary<string,OutgameLegacyPrefabResource>();
        public int RequestRemain {get;private set;}=100;
        public bool IsCurrentLoading {get;private set;}
        public int PendingCount=>newLoaders.Count;
        public int CurrentCount=>currentLoaders.Count;
        public int QueuedCount=>requestQueue.Count;
        public OutgameLegacyResourceScheduler(Func<bool> modern,Action checkResourceRoute,Func<bool,OutgameLegacyResLoader> create,Action<string> warning)
        {this.modern=modern;this.checkResourceRoute=checkResourceRoute;this.create=create;this.warning=warning;}
        public OutgameLegacyResLoader LoadPrefab(string path,Action<OutgameLegacyPrefabResource> complete,object[] arguments=null)
            =>LoadAsset(path+".prefab",complete,arguments);
        public OutgameLegacyResLoader LoadAsset(string path,Action<OutgameLegacyPrefabResource> complete,object[] arguments=null)
        {
            checkResourceRoute();
            return Load(path.ToLower()+".unity3d",complete,arguments);
        }
        public OutgameLegacyResLoader Load(string bundleName,Action<OutgameLegacyPrefabResource> complete,object[] arguments=null)
        {
            checkResourceRoute();
            var loader=CreateLoader(bundleName,true);
            loader.Completed+=complete;loader.Arguments=arguments;
            IsCurrentLoading=true;newLoaders.Add(loader);return loader;
        }
        public OutgameLegacyResLoader CreateLoader(string bundleName,bool asset)
        {
            if(cache.ContainsKey(bundleName))return cache[bundleName];
            var loader=create(asset);loader.Module=this;loader.BundleName=bundleName;cache[bundleName]=loader;return loader;
        }
        public void Update(float deltaTime,float unscaledDeltaTime)
        {if(modern()||!IsCurrentLoading)return;CheckNewLoaders();CheckQueue();}
        void CheckNewLoaders()
        {
            if(newLoaders.Count<1)return;
            var snapshot=new List<OutgameLegacyResLoader>(newLoaders);newLoaders.Clear();
            foreach(var loader in snapshot)currentLoaders.Add(loader);
            foreach(var loader in snapshot)loader.Start(false);
        }
        void CheckQueue()
        {
            while(RequestRemain>=1&&requestQueue.Count>0)
            {
                var loader=requestQueue[0];requestQueue.RemoveAt(0);
                if(!loader.IsComplete){loader.LoadBundle();RequestRemain--;}
            }
        }
        public void Enqueue(OutgameLegacyResLoader loader)
        {if(RequestRemain<=-1)RequestRemain=0;requestQueue.Add(loader);}
        public void LoadComplete(OutgameLegacyResLoader loader)
        {
            RequestRemain++;currentLoaders.Remove(loader);
            if(currentLoaders.Count==0&&newLoaders.Count==0)IsCurrentLoading=false;
        }
        // ResourcesModule29894/29895/29898. Lookup compares each stored resource name.
        public OutgameLegacyPrefabResource CreateResource(OutgameLegacyResLoader loader,OutgameLegacyPrefabResource resource,UnityEngine.AssetBundle bundle)
        {
            if(resource==null)resource=new OutgameLegacyPrefabResource(bundle);
            resource.BundleName=loader.BundleName.ToLower();resource.Bundle=bundle;
            loaded[resource.BundleName]=resource;return resource;
        }
        public OutgameLegacyPrefabResource GetBundleInfo(string name)
        {foreach(var pair in loaded)if(pair.Value.BundleName==name)return pair.Value;return null;}
        public void RemoveBundleInfo(OutgameLegacyPrefabResource resource)
        {
            checkResourceRoute();
            if(resource.IsUnloaded){warning("卸载资源失败,资源已经被释放了["+resource.BundleName+"]");return;}
            resource.Unload(true);loaded.Remove(resource.BundleName);
        }
        public void LoadError(OutgameLegacyResLoader loader)
        {warning("Cant load AB : "+loader.BundleName);LoadComplete(loader);}
    }
    // ResLoader29842-29848. No invented catch/finally or callback deduplication.
    public abstract class OutgameLegacyResLoader
    {
        static int nextId;
        public int Id {get;private set;}
        public int State {get;protected set;}
        public bool IsComplete=>(State&-2)==2;
        public object[] Arguments;
        public Action<OutgameLegacyPrefabResource> Completed;
        public Action<float> Progress;
        public OutgameLegacyPrefabResource Resource;
        public OutgameLegacyResourceScheduler Module;
        public string BundleName;
        protected OutgameLegacyResLoader(){Id=nextId++;}
        public virtual void Start(bool immediate){Id=nextId++;}
        public abstract void LoadBundle();
        public void FireEvent()
        {
            var callback=Completed;
            if(callback!=null){Completed=null;Progress=null;callback(Resource);}
        }
        public virtual void Complete(){FireEvent();Module.LoadComplete(this);}
        public virtual void Error(){FireEvent();Module.LoadError(this);}
    }
}
