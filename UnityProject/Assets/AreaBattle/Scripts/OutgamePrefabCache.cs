using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // LoadPrefabControl fields 16/20/24, InitData f11495 and ClearModelEntityCache f4379.
    public sealed class OutgamePrefabCache
    {
        public readonly Dictionary<int,GameObject> Prefabs=new Dictionary<int,GameObject>();
        public readonly Dictionary<int,List<GameObject>> Entities=new Dictionary<int,List<GameObject>>();
        public readonly Dictionary<int,Transform> Roots=new Dictionary<int,Transform>();
        readonly Action<GameObject> destroy;
        readonly Func<string,OutgameLevelEditorConfig> loadEditorConfig;
        public OutgamePrefabCache(Func<string,OutgameLevelEditorConfig> loadEditorConfig,Action<GameObject> destroy=null)
        {
            this.loadEditorConfig=loadEditorConfig??throw new ArgumentNullException(nameof(loadEditorConfig));
            this.destroy=destroy??(value=>UnityEngine.Object.Destroy(value));
        }
        public void ClearModelEntityCache()
        {
            if(Roots.Count<1)return;
            var removed=new List<int>();
            foreach(int id in Roots.Keys)
            {
                if(unchecked((uint)(id-1000))>=6000u&&unchecked((uint)(id-9033))>=2u)continue;
                // Original does not null-check the Transform or its pool list.
                destroy(Roots[id].gameObject);
                removed.Add(id);
                if(Entities.ContainsKey(id)){Entities[id].Clear();Entities.Remove(id);}
            }
            foreach(int id in removed)Roots.Remove(id);
            removed.Clear();
        }
        public void InitData()
        {
            ClearModelEntityCache();Prefabs.Clear();Entities.Clear();Roots.Clear();
            loadEditorConfig("LevelEditor").IsEditor=false;
        }
    }
}
