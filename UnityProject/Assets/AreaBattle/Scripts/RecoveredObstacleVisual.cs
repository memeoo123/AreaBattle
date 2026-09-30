using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    // Visual-only attachment: the existing BattleObstacles hierarchy remains the physics owner.
    public static class RecoveredObstacleVisual
    {
        public static GameObject Attach(GameObject collisionRoot,int entityId)
        {
            if(collisionRoot==null)throw new ArgumentNullException(nameof(collisionRoot));
            var prefab=Resources.Load<GameObject>("Recovered/Obstacles/Entity_"+entityId);
            if(prefab==null)throw new InvalidOperationException("Missing recovered obstacle visual "+entityId);
            if(prefab.GetComponentsInChildren<Collider>(true).Length!=0)
                throw new InvalidOperationException("Obstacle visual prefab must not contain physics colliders.");
            var visual=UnityEngine.Object.Instantiate(prefab,collisionRoot.transform,false);
            visual.name="Recovered visual "+entityId;
            // Original SetTransform replaces the prefab root's transform with cfg/100.
            // The collision root already owns that transform, so do not apply it twice.
            visual.transform.localPosition=Vector3.zero;
            visual.transform.localRotation=Quaternion.identity;
            visual.transform.localScale=Vector3.one;
            return visual;
        }

        public static List<GameObject> Attach(LevelLayout layout,IReadOnlyList<GameObject> collisionRoots)
        {
            int count=layout.ObstacleInfoCfgs?.Length??0;
            if(collisionRoots==null||collisionRoots.Count!=count)
                throw new ArgumentException("Collision roots must follow serialized obstacle order.");
            var result=new List<GameObject>(count);
            for(int i=0;i<count;i++)result.Add(Attach(collisionRoots[i],layout.ObstacleInfoCfgs[i].EnityID));
            return result;
        }
    }
}
