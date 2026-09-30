using System;
using UnityEngine;
namespace AreaBattle
{
    // NormalPool object type3665: ctor28676 and virtual lifecycle slots4/5/6.
    public sealed class OutgamePooledPrefab
    {
        public string Name {get;}
        public GameObject Target {get;}
        public object ResourceInfo {get;}
        public bool Locked {get;} = false;
        public int Priority {get;} = 0;
        public DateTime LastUseTime {get;internal set;}
        readonly Transform poolRoot;readonly Action<GameObject> release;
        Transform previousParent;
        public OutgamePooledPrefab(string name,object resourceInfo,GameObject target,Transform poolRoot,Action<GameObject> release)
        {
            // ObjectBase28692 tests the managed reference, not Unity destroyed-object equality.
            if(ReferenceEquals(target,null))throw new InvalidOperationException(string.Format("Target '{0}' is invalid.",name));
            Name=name??string.Empty;Target=target;LastUseTime=DateTime.Now;
            ResourceInfo=resourceInfo;this.poolRoot=poolRoot;this.release=release;
        }
        public void OnSpawn()
        {
            if(Target!=null){if(previousParent!=null)Target.transform.SetParent(previousParent,false);Target.SetActive(true);}
        }
        public void OnUnspawn()
        {
            if(Target!=null){previousParent=Target.transform.parent;Target.SetActive(false);Target.transform.SetParent(poolRoot,false);}
        }
        public void Release()=>release(Target);
    }
}
