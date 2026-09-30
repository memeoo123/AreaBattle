using System;
namespace AreaBattle
{
    // Original Object<T>, metadata28724..28731; shared bodies resolved from registration.
    public sealed class OutgamePoolEntry
    {
        readonly OutgamePooledPrefab item;
        int spawnCount;
        public string Name=>item.Name;
        public bool Locked=>item.Locked;
        public bool IsInUse=>spawnCount>0;
        public int SpawnCount=>spawnCount;
        public OutgamePoolEntry(OutgamePooledPrefab item,bool spawned)
        {
            if(item==null)throw new InvalidOperationException("Object is invalid.");
            this.item=item;spawnCount=spawned?1:0;
            if(spawned)item.OnSpawn();
        }
        public OutgamePooledPrefab Peek()=>item;
        public OutgamePooledPrefab Spawn()
        {
            spawnCount=unchecked(spawnCount+1);
            item.LastUseTime=DateTime.Now;
            item.OnSpawn();
            return item;
        }
        public void Unspawn()
        {
            item.OnUnspawn();
            item.LastUseTime=DateTime.Now;
            spawnCount=unchecked(spawnCount-1);
        }
        public void Release()=>item.Release();
    }
}
