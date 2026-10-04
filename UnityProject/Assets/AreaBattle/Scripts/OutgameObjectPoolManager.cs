using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameManagedObjectPool
    {
        void Update(float elapsedSeconds,float realElapsedSeconds);
        void Shutdown();
    }
    // Original3672/28712..28723; generic object type name is retained from original metadata.
    public sealed class OutgameObjectPoolManager:IOutgameFrameModule,IOutgameSingleSpawnPoolFactory
    {
        public const string PrefabObjectTypeName="g\u0093t\u0090\u00b6O";
        public readonly Dictionary<string,IOutgameManagedObjectPool> Pools=new Dictionary<string,IOutgameManagedObjectPool>();
        public int Priority=>70;
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public void Initialize(){IsInitialized=true;Initialized?.Invoke();}
        public void Initialize(object[] args)=>Initialize();
        public void Start(){}
        public void Update(float delta,float unscaled){foreach(var pair in Pools)pair.Value.Update(delta,unscaled);}
        public void Shutdown()
        {foreach(var pair in Pools)if(pair.Value!=null)pair.Value.Shutdown();Pools.Clear();IsInitialized=false;}
        // Text27496/27497: typeof(T).FullName, optionally "{0}.{1}" with the supplied name.
        public static string FullName(string sourceTypeFullName,string name)=>string.IsNullOrEmpty(name)?sourceTypeFullName:string.Format("{0}.{1}",sourceTypeFullName,name);
        public bool HasObjectPool(string sourceTypeFullName,string name)=>InternalHasObjectPool(FullName(sourceTypeFullName,name));
        public bool InternalHasObjectPool(string fullName)=>Pools.ContainsKey(fullName);
        public TPool CreateSingleSpawnObjectPool<TPool>(string sourceTypeFullName,string name,Func<string,bool,int,float,int,TPool> create)where TPool:class,IOutgameManagedObjectPool
        {return InternalCreateObjectPool(sourceTypeFullName,name,false,int.MaxValue,float.MaxValue,0,create);}
        public TPool InternalCreateObjectPool<TPool>(string sourceTypeFullName,string name,bool multi,int capacity,float expiry,int priority,Func<string,bool,int,float,int,TPool> create)where TPool:class,IOutgameManagedObjectPool
        {
            if(HasObjectPool(sourceTypeFullName,name))throw new OutgameFrameworkException(string.Format("Already exist object pool '{0}'.",FullName(sourceTypeFullName,name)));
            var pool=create(name,multi,capacity,expiry,priority);
            Pools.Add(FullName(sourceTypeFullName,name),pool);return pool;
        }
        public OutgameObjectPool CreateSingleSpawnObjectPool(string name)=>CreateSingleSpawnObjectPool(PrefabObjectTypeName,name,(n,m,c,e,p)=>new OutgameObjectPool(n,m,c,e,p));
        public bool HasObjectPool(string name)=>HasObjectPool(PrefabObjectTypeName,name);
        public bool DestroyObjectPool(string name)=>DestroyObjectPool(PrefabObjectTypeName,name);
        public bool DestroyObjectPool(string sourceTypeFullName,string name)=>InternalDestroyObjectPool(FullName(sourceTypeFullName,name));
        public bool InternalDestroyObjectPool(string fullName)
        {if(!Pools.TryGetValue(fullName,out var pool))return false;pool.Shutdown();return Pools.Remove(fullName);}
    }
    // NormalPool28679 GetModule then managed-null check and typed handle/resource cleanup.
    public sealed class OutgameNormalPoolLifetime:IOutgameNormalPoolLifetime
    {
        readonly Func<OutgameObjectPoolManager> manager;readonly Action<GameObject> destroy;
        readonly Action unloadUnused;readonly Action<IOutgamePoolResourceInfo> unloadBundle;
        public OutgameNormalPoolLifetime(Func<OutgameObjectPoolManager> manager,Action unloadUnused,Action<IOutgamePoolResourceInfo> unloadBundle,Action<GameObject> destroy=null)
        {this.manager=manager;this.unloadUnused=unloadUnused;this.unloadBundle=unloadBundle;this.destroy=destroy??(go=>UnityEngine.Object.Destroy(go));}
        public void DestroyRegisteredPoolIfManagerExists(string name){var current=manager();if(current!=null)current.DestroyObjectPool(name);}
        public void DestroyRoot(GameObject root)=>destroy(root);
        public void ReleaseHandle(IOutgamePoolAssetHandle handle)=>((OutgameAssetHandle)handle).Release();
        public void UnloadUnusedAssets()=>unloadUnused();
        public void UnloadUnusedBundle(IOutgamePoolResourceInfo info)=>unloadBundle(info);
        public static void Bind(OutgameEffectControlServices effects,OutgameFrameEntry frame,IOutgameNormalPoolResources resources,Action unloadUnused,Action<IOutgamePoolResourceInfo> unloadBundle,Action<GameObject> destroy=null)
        {
            effects.Resources=resources;effects.PoolManager=()=>frame.GetModule<OutgameObjectPoolManager>();
            effects.PoolLifetime=new OutgameNormalPoolLifetime(()=>frame.GetModule<OutgameObjectPoolManager>(),unloadUnused,unloadBundle,destroy);
        }
    }
}
