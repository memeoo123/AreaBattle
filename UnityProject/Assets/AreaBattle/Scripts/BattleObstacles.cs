using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    // Exact source prefab collider hierarchy; visible cubes are a temporary geometry overlay.
    public static class BattleObstacles
    {
        [Serializable] public sealed class Document { public Prefab[] prefabs; }
        [Serializable] public sealed class Prefab { public int entityId; public string name; public Node[] nodes; }
        [Serializable] public sealed class Node
        { public string name; public bool active; public int layer,parent; public TransformData transform; public ComponentData[] components; }
        [Serializable] public sealed class TransformData
        { public Vector3 m_LocalPosition,m_LocalScale; public Quaternion m_LocalRotation; }
        [Serializable] public sealed class ComponentData { public string type; public BoxData data; }
        [Serializable] public sealed class BoxData { public bool m_IsTrigger,m_Enabled; public Vector3 m_Size,m_Center; }

        public static List<GameObject> Create(LevelLayout layout,Transform parent,Material debugMaterial=null)
        {
            var result=new List<GameObject>();
            if(layout.ObstacleInfoCfgs==null || layout.ObstacleInfoCfgs.Length==0)return result;
            var doc=JsonUtility.FromJson<Document>(BattleView.ReadText("Data/ObstacleGeometry"));
            foreach(var cfg in layout.ObstacleInfoCfgs)
            {
                var prefab=Array.Find(doc.prefabs,p=>p.entityId==cfg.EnityID);
                if(prefab==null)throw new InvalidOperationException("Unrecovered obstacle entity "+cfg.EnityID);
                var nodes=new List<GameObject>();
                foreach(var node in prefab.nodes)nodes.Add(CreateNode(node,node.parent<0?parent:nodes[node.parent].transform,debugMaterial));
                var root=nodes[0];
                // Source ObstacleInfoCfg.SetTransform f11209: all three IntVector3 fields /100.
                root.transform.position=cfg.pos.WorldPosition;
                root.transform.eulerAngles=cfg.angle.WorldPosition;
                root.transform.localScale=cfg.scale.WorldPosition;
                result.Add(root);
            }
            return result;
        }
        static GameObject CreateNode(Node node,Transform parent,Material debugMaterial)
        {
            var obj=new GameObject(node.name);obj.layer=node.layer;obj.transform.SetParent(parent,false);
            obj.transform.localPosition=node.transform.m_LocalPosition;
            obj.transform.localRotation=node.transform.m_LocalRotation;
            obj.transform.localScale=node.transform.m_LocalScale;
            foreach(var c in node.components??Array.Empty<ComponentData>())
            {
                if(c.type!="BoxCollider")throw new NotSupportedException("Obstacle collider "+c.type);
                var box=obj.AddComponent<BoxCollider>();box.size=c.data.m_Size;box.center=c.data.m_Center;
                box.isTrigger=c.data.m_IsTrigger;box.enabled=c.data.m_Enabled;
                if(debugMaterial!=null)
                {
                    var debug=GameObject.CreatePrimitive(PrimitiveType.Cube);debug.name="Temporary collider view";
                    debug.transform.SetParent(obj.transform,false);debug.transform.localPosition=box.center;debug.transform.localScale=box.size;
                    var extra=debug.GetComponent<Collider>();extra.enabled=false;
                    if(Application.isPlaying)UnityEngine.Object.Destroy(extra);else UnityEngine.Object.DestroyImmediate(extra);
                    debug.GetComponent<Renderer>().sharedMaterial=debugMaterial;
                }
            }
            obj.SetActive(node.active);return obj;
        }
    }
}
