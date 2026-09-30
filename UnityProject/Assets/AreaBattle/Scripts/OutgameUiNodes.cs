using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // UIModule._GetUINode27394. Missing layers use the module root and are cached.
    public sealed class OutgameUiNodes
    {
        readonly IDictionary<string,Transform> nodes;readonly Func<RectTransform> canvas;readonly Func<GameObject> root;readonly Action<string> error;
        public OutgameUiNodes(IDictionary<string,Transform> nodes,Func<RectTransform> canvas,Func<GameObject> root,Action<string> error)
        {this.nodes=nodes;this.canvas=canvas;this.root=root;this.error=error;}
        public Transform Get(string name)
        {
            if(!nodes.TryGetValue(name,out var node))
            {
                node=canvas().transform.Find(name);
                if(node==null){error("未找到UI节点:"+name);node=root().transform;}
                nodes.Add(name,node);
            }
            return node;
        }
    }
}
