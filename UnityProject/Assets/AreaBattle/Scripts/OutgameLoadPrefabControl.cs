using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgamePrefabPreloadHost
    {
        IOutgameLevelLayout CurrentLevel {get;}
        int GetSkinEntityByType(int shipId);
        int GetSkinEntityByRandomType(int shipId,int campId);
        void ResetLoadingProgress(); // UIControl.I loading page float field44 = 0, when page exists.
    }
    // LoadPrefabControl4117 source loading callbacks and shared prefab/entity/root caches.
    // Level/player/loading-UI dependencies must be supplied by the production composition.
    public sealed class OutgameLoadPrefabControl:IOutgameLogicControl
    {
        readonly Func<OutgameLegacyConfigManager> config;readonly OutgameControllerRegistry registry;readonly OutgameResLoadHelper resources;readonly Action<string> error;
        public readonly OutgamePrefabCache Cache;readonly IOutgamePrefabPreloadHost preload;
        public static bool CanLoadNext;
        public static int PreparedCount;
        public static readonly List<int> PreloadIds=new List<int>();
        public bool IsEditor;public GameObject EntityRoot;
        // Source static offset12. Neither success nor controller disposal resets this dictionary.
        public static readonly Dictionary<int,int> LoadFailures=new Dictionary<int,int>();
        public OutgameLoadPrefabControl(Func<OutgameLegacyConfigManager> config,OutgameControllerRegistry registry,OutgameResLoadHelper resources,OutgamePrefabCache cache,Action<string> error,IOutgamePrefabPreloadHost preload=null)
        {this.config=config;this.registry=registry;this.resources=resources;Cache=cache;this.error=error;this.preload=preload;}
        public void OnInit(){} // Source31553.
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Source31551.
        public void OnDispose()=>registry.Clear(4117); // Source31546; caches/instances retained.
        public void InitData()=>Cache.InitData();
        public void ClearModelEntityCache()=>Cache.ClearModelEntityCache();
        public bool Contains(int id)=>Cache.Prefabs.ContainsKey(id);
        public void PrepareStart(Action complete) //31543/31544/31579: shared cursor, not callback-local.
        {CanLoadNext=false;PreparedCount=0;BuildPreloadList();PrepareNext(complete);}
        public void PreLoad() //31545/31565/31568: leaves shared prepare cursor unchanged.
        {CanLoadNext=true;BuildPreloadList();PreloadNext(0);}
        void BuildPreloadList() //31555, original LevelInfoCfg maps to recovered LevelLayout.
        {
            PreloadIds.Clear();PreloadIds.AddRange(new[]{1,10,11,12,13,14,15,101,9033,9034});
            if(preload.CurrentLevel.Stars!=null&&preload.CurrentLevel.Stars.Length>0)
            {
                var stars=preload.CurrentLevel.Stars;
                for(int i=0;i<stars.Length;i++)
                {
                    var star=stars[i];int shipId=star.ShipID;
                    AddPreloadId(preload.GetSkinEntityByType(shipId));
                    AddPreloadId(preload.GetSkinEntityByRandomType(shipId,star.CampID));
                    if(star.isBoss)AddPreloadId(821);
                }
            }
            if(preload.CurrentLevel.Obstacles!=null&&preload.CurrentLevel.Obstacles.Length>0)
            {
                var obstacles=preload.CurrentLevel.Obstacles;
                for(int i=0;i<obstacles.Length;i++)AddPreloadId(obstacles[i].EnityID);
            }
        }
        static void AddPreloadId(int id){if(id>0&&!PreloadIds.Contains(id))PreloadIds.Add(id);}
        void PrepareNext(Action complete)
        {
            if(PreloadIds.Count>PreparedCount)
            {
                PrepareEntity(PreloadIds[PreparedCount],()=>{PreparedCount++;preload.ResetLoadingProgress();PrepareNext(complete);});return;
            }
            complete?.Invoke();
        }
        void PreloadNext(int index)
        {if(CanLoadNext&&index<PreloadIds.Count)PrepareEntity(PreloadIds[index],()=>PreloadNext(index+1));}
        string EntityAssetName(int id)
        {
            if(config().dicEntityModel.TryGetValue(id,out var row))return row.AssetName;
            error(string.Format("EntityModelConfig 不存在 Id={0} 的数据",id));return id.ToString();
        }
        public Transform GetEntityRoot(int id) //31564: world position and default SetParent(true).
        {
            if(!EntityRoot){EntityRoot=new GameObject("EntityRoot");EntityRoot.transform.position=Vector3.zero;}
            if(!Cache.Roots.TryGetValue(id,out var root))
            {
                root=new GameObject(EntityAssetName(id)).transform;root.SetParent(EntityRoot.transform);
                root.position=Vector3.zero;Cache.Roots.Add(id,root);
            }
            return root; // A present destroyed/null root is deliberately returned unchanged.
        }
        GameObject CreateEntity(int id,GameObject prefab) //31547/31560.
        {
            var instance=UnityEngine.Object.Instantiate(prefab);
            if(Cache.Entities.ContainsKey(id))Cache.Entities[id].Add(instance);
            else Cache.Entities[id]=new List<GameObject>{instance};
            var root=GetEntityRoot(id);instance.transform.SetParent(root);
            instance.name=EntityAssetName(id)+"_"+root.childCount.ToString(CultureInfo.InvariantCulture);
            return instance;
        }
        bool TryAcquireEntity(int id,out GameObject entity) //31550.
        {
            entity=null;
            if(Cache.Entities.TryGetValue(id,out var list))
                for(int i=0;i<list.Count;i++)
                {
                    var candidate=list[i];
                    if(candidate){if(!candidate.activeSelf){candidate.SetActive(true);entity=candidate;return true;}}
                    else Cache.Entities[id].Remove(candidate); // Source increments i even after removal.
                }
            return false;
        }
        public void GetEntity(int id,Action<GameObject,object[]> complete,object[] args=null) //31552/31575.
        {
            if(TryAcquireEntity(id,out var entity)){complete?.Invoke(entity,args);return;}
            if(Cache.Prefabs.TryGetValue(id,out var prefab)&&prefab)
            {entity=CreateEntity(id,prefab);entity.SetActive(true);complete?.Invoke(entity,args);return;}
            LoadEntityTemplate(id,(template,arguments)=>{entity=CreateEntity(id,template);entity.SetActive(true);complete?.Invoke(entity,arguments);},args);
        }
        public GameObject GetEntityNow(int id) //31559: schedules missing template, always returns null this call.
        {
            if(TryAcquireEntity(id,out var entity))return entity;
            if(Cache.Prefabs.TryGetValue(id,out var prefab))
            {
                if(prefab){entity=CreateEntity(id,prefab);entity.SetActive(true);return entity;}
                error(string.Format("id = {0} 的实体缓存失效，需重新缓存",id));
            }
            else error(string.Format("未找到id = {0} 的实体缓存，请先缓存再调用",id));
            LoadEntityTemplate(id,null,Array.Empty<object>());return null;
        }
        public void PrepareEntity(int id,Action complete) //31561/31572/31573: template only, no instance.
        {
            if(Cache.Prefabs.ContainsKey(id))
            {
                if(Cache.Prefabs[id]){complete?.Invoke();return;}
                error(string.Format("{0} 缓存失效，重新加载",id));
            }
            LoadEntityTemplate(id,(prefab,args)=>complete?.Invoke(),Array.Empty<object>());
        }
        void LoadEntityTemplate(int id,Action<GameObject,object[]> complete,object[] args) //31562/31577.
        {
            if(!config().dicEntityModel.TryGetValue(id,out var row)){error(string.Format("资源表中不包含id = {0} 的配置",id));return;}
            string path=OutgameResourcePaths.GetPath(row.AssetName,row.PathEnum);
            resources.LoadAsset(path,resource=>{
                var prefab=resource.LoadAsset<GameObject>(OutgameResourcePaths.ToFileName(path));
                if(prefab)
                {
                    if(!Cache.Prefabs.ContainsKey(id))Cache.Prefabs.Add(id,prefab);
                    // Source TryGetValue writes a local zero on success, not the dictionary.
                    if(LoadFailures.TryGetValue(id,out int failures))failures=0;
                    complete?.Invoke(prefab,args);return;
                }
                if(!LoadFailures.ContainsKey(id))LoadFailures.Add(id,0);
                int count=LoadFailures[id];
                if(count<=3)
                {
                    // WASM31577 calls set_Item with the same count; there is no increment.
                    LoadFailures[id]=count;LoadEntityTemplate(id,complete,args);return;
                }
                error(string.Format("资源id = {0} 加载失败",id));
            },args);
        }
        public void LoadAsset(int id,Action<GameObject,object[]> complete,object[] args=null)
        {
            if(Cache.Prefabs.TryGetValue(id,out var cached)&&cached){var instance=UnityEngine.Object.Instantiate(cached);instance.name=cached.name;complete?.Invoke(instance,args);return;}
            if(!config().dicEntityModel.TryGetValue(id,out var row)){error(string.Format("资源表中不包含id = {0} 的配置",id));return;}
            string path=OutgameResourcePaths.GetPath(row.AssetName,row.PathEnum);
            resources.LoadAsset(path,resource=>{
                var prefab=resource.LoadAsset<GameObject>(OutgameResourcePaths.ToFileName(path));
                if(Cache.Prefabs.ContainsKey(id))Cache.Prefabs[id]=prefab;else Cache.Prefabs.Add(id,prefab);
                var instance=UnityEngine.Object.Instantiate(prefab);instance.name=row.AssetName;complete?.Invoke(instance,args);
            },args);
        }
        public void LoadPrefab(string path,Action<GameObject,object[]> complete,object[] args=null)
        {resources.LoadPrefab(path,resource=>{var instance=UnityEngine.Object.Instantiate(resource.LoadAsset<GameObject>(OutgameResourcePaths.ToFileName(path)));complete?.Invoke(instance,args);},args);}
        public void LoadTexture(string name,Action<Texture2D,object[]> complete,object[] args=null)
        {
            if(!config().SceneResources.TryGetValue(name,out int type)){error("加载图片出错: "+name);return;}
            string path=OutgameResourcePaths.GetPath(name,type);
            resources.LoadAsset(path,resource=>{var texture=resource.LoadAsset<Texture2D>(OutgameResourcePaths.ToFileName(path));complete?.Invoke(texture,args);},args);
        }
    }
    public sealed class OutgameGameSceneResources:IOutgameGameSceneResources
    {
        readonly Func<OutgameLoadPrefabControl> control;
        public OutgameGameSceneResources(Func<OutgameLoadPrefabControl> control){this.control=control;}
        public void LoadAsset(int id,Action<GameObject> loaded,object[] arguments=null)=>control().LoadAsset(id,(value,args)=>loaded(value),arguments);
        public void LoadTexture(string name,Action<Texture2D> loaded,object[] arguments=null)=>control().LoadTexture(name,(value,args)=>loaded(value),arguments);
        public void LoadPrefab(string name,Action<GameObject> loaded,object[] arguments=null)=>control().LoadPrefab(name,(value,args)=>loaded(value),arguments);
    }
}
