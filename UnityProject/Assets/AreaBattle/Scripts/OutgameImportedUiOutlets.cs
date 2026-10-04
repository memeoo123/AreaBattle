using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Local import adapter for evidence-exported IUIoutlet bindings. Paths are exact
    // source-object mappings from hud-import.json; no hierarchy name guessing.
    public sealed class OutgameImportedUiOutlets
    {
        [Serializable] sealed class Manifest {public Prefab[] prefabs;}
        [Serializable] sealed class Prefab {public string name;public Binding[] bindings;public Node[] nodes;}
        [Serializable] sealed class Node {public string path,name;}
        [Serializable] sealed class Binding {public string name,path,owner;}
        readonly Binding[] bindings;readonly Node[] nodes;readonly string owner;
        public OutgameImportedUiOutlets(string manifestJson,string prefabName,string owner="")
        {
            this.owner=owner;var manifest=JsonUtility.FromJson<Manifest>(manifestJson);
            foreach(var prefab in manifest.prefabs)if(prefab.name==prefabName){bindings=prefab.bindings;nodes=prefab.nodes;return;}
            throw new InvalidOperationException("Missing imported UI prefab: "+prefabName);
        }
        public IEnumerable<KeyValuePair<string,object>> Read(GameObject root)
        {
            // Import paths identify source objects even when sibling names repeat.
            // Resolve the preserved sibling order once, before callers add dynamic rows.
            Dictionary<string,Transform> resolved=null;
            if(nodes!=null)
            {
                resolved=new Dictionary<string,Transform>();var next=new Dictionary<string,int>();
                foreach(var source in nodes)
                {
                    Transform node;
                    if(string.IsNullOrEmpty(source.path))node=root.transform;
                    else
                    {
                        int slash=source.path.LastIndexOf('/');string parent=slash<0?"":source.path.Substring(0,slash);
                        next.TryGetValue(parent,out int index);node=resolved[parent].GetChild(index);next[parent]=index+1;
                        if(node.name!=source.name)throw new InvalidOperationException("Changed original UI sibling order: "+source.path);
                    }
                    resolved.Add(source.path,node);
                }
            }
            foreach(var binding in bindings)
            {
                if(binding.owner!=owner)continue;
                var node=resolved!=null?resolved[binding.path]:(string.IsNullOrEmpty(binding.path)?root.transform:root.transform.Find(binding.path));
                if(node==null)throw new InvalidOperationException("Missing original UI outlet path: "+binding.path);
                yield return new KeyValuePair<string,object>(binding.name,node.gameObject);
            }
        }
    }
}
