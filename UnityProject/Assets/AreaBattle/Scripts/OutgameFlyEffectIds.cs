using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Original EffectControl nested uX...4054; one registry shared across effect controllers.
    public sealed class OutgameFlyEffectIds
    {
        public static readonly OutgameFlyEffectIds Shared=new OutgameFlyEffectIds();
        readonly Dictionary<int,int> subCounts=new Dictionary<int,int>();int next=1;
        public int NextId()
        {
            lock(this){int id=next;next=unchecked(next+1);subCounts.Add(id,0);return id;}
        }
        public string NextSubId(int id)
        {int sub=subCounts[id];subCounts[id]=unchecked(sub+1);return string.Format("fly_{0}_{1}",id,sub);}
        public int SubIdCount(int id)=>subCounts.TryGetValue(id,out int count)?count:-1;
        public void RemoveSubIds(int id)=>subCounts.Remove(id);
    }
}
