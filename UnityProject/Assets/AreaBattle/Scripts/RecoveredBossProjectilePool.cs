using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Boss.f31308 clones its own inactive fire child; f42662 enqueues it on hit.
    // Reuse intentionally preserves rotation. Boss.Clear destroys bulletRoot children.
    public sealed class RecoveredBossProjectilePool : MonoBehaviour
    {
        readonly Queue<GameObject> available=new Queue<GameObject>();
        readonly List<GameObject> owned=new List<GameObject>();
        Transform source;
        public int CreatedCount=>owned.Count;
        public int AvailableCount=>available.Count;
        public static RecoveredBossProjectilePool For(Transform boss)
        {
            var root=boss.Find("bulletRoot");
            if(root==null)throw new InvalidOperationException("Original Boss bulletRoot missing");
            var pool=root.GetComponent<RecoveredBossProjectilePool>()??root.gameObject.AddComponent<RecoveredBossProjectilePool>();
            pool.source=boss.Find("Hdzd_Effect_Wy_Fire");
            if(pool.source==null)throw new InvalidOperationException("Original Boss fire template missing");
            return pool;
        }
        public GameObject Acquire(Vector3 start)
        {
            GameObject obj=null;
            while(available.Count>0&&obj==null)obj=available.Dequeue();
            if(obj==null)
            {
                obj=Instantiate(source.gameObject);obj.name="Boss action2 / "+source.name;
                obj.transform.eulerAngles=new Vector3(0,90,0);owned.Add(obj);
                RecoveredEffectVisual.Attach(obj);
            }
            obj.SetActive(true);obj.transform.position=start;obj.transform.SetParent(transform,true);
            return obj;
        }
        public void Release(GameObject obj)
        {
            if(obj==null)return;
            obj.SetActive(false);obj.GetComponent<RecoveredEffectVisual>().Step(0);
            available.Enqueue(obj);
        }
        public void ClearSourcePool()
        {
            foreach(var obj in owned)if(obj!=null){obj.SetActive(false);if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
            owned.Clear();available.Clear();
        }
    }
}
