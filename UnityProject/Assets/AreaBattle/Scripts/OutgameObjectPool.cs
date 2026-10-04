using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ObjectPool<T>28732..28746 and normal-pool-rgctx.json. Concrete prefab specialization.
    public sealed class OutgameObjectPool:IOutgameManagedObjectPool
    {
        readonly LinkedList<OutgamePoolEntry> objects=new LinkedList<OutgamePoolEntry>();
        readonly bool allowMultiSpawn;
        int capacity;float expireTime,autoReleaseTime;
        public string Name {get;}
        public int Count=>objects.Count;
        public float AutoReleaseInterval {get;set;}
        public int Priority {get;set;}
        public int Capacity {get=>capacity;set{if(value<0)throw new InvalidOperationException("Capacity is invalid.");if(capacity!=value){capacity=value;Release();}}}
        public float ExpireTime {get=>expireTime;set{if(value<0)throw new InvalidOperationException("ExpireTime is invalid.");if(expireTime!=value){expireTime=value;Release();}}}
        public OutgameObjectPool(string name,bool allowMultiSpawn,int capacity,float expireTime,int priority)
        {
            Name=name??string.Empty;this.allowMultiSpawn=allowMultiSpawn;AutoReleaseInterval=expireTime;
            Capacity=capacity;ExpireTime=expireTime;autoReleaseTime=0;Priority=priority;
        }
        public void Register(OutgamePooledPrefab item,bool spawned)
        {
            if(item==null)throw new InvalidOperationException("Object is invalid.");
            objects.AddLast(new OutgamePoolEntry(item,spawned));Release();
        }
        public OutgamePooledPrefab Spawn(string name)
        {
            foreach(var entry in objects)
                if(entry.Name==name&&(allowMultiSpawn||!entry.IsInUse))return entry.Spawn();
            return null;
        }
        public void Unspawn(GameObject target)
        {
            if(ReferenceEquals(target,null))throw new InvalidOperationException("Target is invalid.");
            foreach(var entry in objects)
                if(ReferenceEquals(entry.Peek().Target,target)){entry.Unspawn();Release();return;}
            throw new InvalidOperationException(string.Format("Can not find target in object pool '{0}'.",Name));
        }
        public LinkedList<OutgamePooledPrefab> GetCanReleaseObjects()
        {
            var result=new LinkedList<OutgamePooledPrefab>();
            foreach(var entry in objects)if(!entry.IsInUse&&!entry.Locked)result.AddLast(entry.Peek());
            return result;
        }
        public static LinkedList<OutgamePooledPrefab> DefaultReleaseObjectFilterCallback(LinkedList<OutgamePooledPrefab> candidates,int count,DateTime cutoff)
        {
            var result=new LinkedList<OutgamePooledPrefab>();
            if(cutoff>DateTime.MinValue){
                for(var node=candidates.First;node!=null;){var next=node.Next;if(node.Value.LastUseTime<=cutoff){result.AddLast(node.Value);candidates.Remove(node);}node=next;}
                count=unchecked(count-result.Count);
            }
            // Source selection sort swaps node values; preserve its tie behavior.
            for(var node=candidates.First;node!=null&&count>0;node=node.Next,count--){
                for(var other=node.Next;other!=null;other=other.Next){
                    var a=node.Value;var b=other.Value;
                    if(a.Priority>b.Priority||(a.Priority==b.Priority&&a.LastUseTime>b.LastUseTime)){node.Value=b;other.Value=a;}
                }
                result.AddLast(node.Value);
            }
            return result;
        }
        public void Release()=>Release(unchecked(objects.Count-capacity),DefaultReleaseObjectFilterCallback);
        public void Release(int count,Func<LinkedList<OutgamePooledPrefab>,int,DateTime,LinkedList<OutgamePooledPrefab>> filter)
        {
            if(filter==null)throw new InvalidOperationException("Release object filter callback is invalid.");
            autoReleaseTime=0;count=Math.Max(count,0);
            var cutoff=DateTime.MinValue;if(expireTime<float.MaxValue)cutoff=DateTime.Now.AddSeconds(-expireTime);
            var selected=filter(GetCanReleaseObjects(),count,cutoff);
            if(selected==null||selected.Count==0)return;
            foreach(var item in selected){
                if(item==null)throw new InvalidOperationException("Can not release null object.");
                bool found=false;
                foreach(var entry in objects){
                    if(!ReferenceEquals(entry.Peek(),item))continue;
                    objects.Remove(entry);entry.Release();found=true;break;
                }
                if(!found)throw new InvalidOperationException("Can not release object which is not found.");
            }
        }
        public void Update(float elapsedSeconds,float realElapsedSeconds)
        {
            autoReleaseTime+=realElapsedSeconds;
            if(!(autoReleaseTime<AutoReleaseInterval))Release();
        }
        public void Shutdown()
        {
            for(var node=objects.First;node!=null;){var next=node.Next;objects.Remove(node);node.Value.Release();node=next;}
        }
    }
}
