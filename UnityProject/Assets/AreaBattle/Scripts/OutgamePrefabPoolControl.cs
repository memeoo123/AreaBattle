using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Type4561, methods34667..34676: original simple prefab pool, distinct from NormalPool.
    public sealed class OutgamePrefabPoolControl:IOutgameLogicControl
    {
        readonly Dictionary<GameObject,List<GameObject>> pools=new Dictionary<GameObject,List<GameObject>>();
        readonly Dictionary<GameObject,GameObject> spawned=new Dictionary<GameObject,GameObject>();
        readonly OutgameControllerRegistry registry;
        readonly Action<GameObject> persist,destroy;
        readonly Action<string> warning,error;
        public GameObject Root {get;private set;}
        public OutgamePrefabPoolControl(OutgameControllerRegistry registry,Action<string> warning,Action<string> error,Action<GameObject> persist=null,Action<GameObject> destroy=null)
        {this.registry=registry;this.warning=warning;this.error=error;this.persist=persist??(value=>UnityEngine.Object.DontDestroyOnLoad(value));this.destroy=destroy??(value=>UnityEngine.Object.Destroy(value));}
        public void OnInit(){Root=new GameObject("Pool");persist(Root);Root.SetActive(false);}
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Explicit interface34668, empty.
        public void OnDispose(){registry.Clear(4561);destroy(Root);}
        public void CreatePool(GameObject prefab,int count)
        {
            if(prefab==null)error("prefab 加载错误，请检查");
            if(prefab==null||pools.ContainsKey(prefab))return;
            var available=new List<GameObject>();pools.Add(prefab,available);
            if(count<1)return;
            bool active=prefab.activeSelf;prefab.SetActive(false);
            while(available.Count<count)
            {
                var item=UnityEngine.Object.Instantiate(prefab);item.transform.SetParent(Root.transform);available.Add(item);
            }
            prefab.SetActive(active); // No finally in source; earlier mutations survive failure.
        }
        public GameObject Spawn(GameObject prefab,Transform parent)=>Spawn(prefab,parent,Vector3.zero,Vector3.one,Quaternion.identity);
        public GameObject Spawn(GameObject prefab,Transform parent,Vector3 position,Vector3 scale,Quaternion rotation)
        {
            GameObject item=null;bool registered=pools.TryGetValue(prefab,out var available);
            if(registered)
                while(item==null&&available.Count>0){item=available[0];available.RemoveAt(0);}
            if(item==null)item=UnityEngine.Object.Instantiate(prefab);
            var transform=item.transform;transform.SetParent(parent);transform.localPosition=position;transform.localScale=scale;transform.localRotation=rotation;
            item.SetActive(true);
            if(registered)spawned.Add(item,prefab);
            return item;
        }
        public void Recycle(GameObject item)
        {
            if(!registry.HasInstance(4561)||Root==null||item==null)return;
            if(spawned.TryGetValue(item,out var prefab))
            {
                if(pools.ContainsKey(prefab))
                {
                    pools[prefab].Add(item);spawned.Remove(item);item.transform.SetParent(Root.transform,false);item.SetActive(false);
                }
                return;
            }
            warning("没回收物体，请检查代码"+item.name);destroy(item);
        }
    }
}
