using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    [Serializable] public sealed class BattleLevelRow { public int id,SceneId; public int[] LevelType; }
    public sealed class BattleLevelCatalog
    {
        [Serializable] sealed class Table { public BattleLevelRow[] Datas; }
        readonly Dictionary<int,BattleLevelRow> rows=new Dictionary<int,BattleLevelRow>();
        public int MaximumLevel { get; private set; }
        public BattleLevelCatalog(string recoveredJson)
        {
            foreach(var row in JsonUtility.FromJson<Table>(recoveredJson).Datas)rows.Add(row.id,row);
            MaximumLevel=rows.Count-1;
        }
        // ConfigMgr.GetLevelRealID f0x5e032e: alternate parity after the source table ends.
        public int ResolveIdentity(int requested)
        {
            if(requested<0)throw new ArgumentOutOfRangeException(nameof(requested));
            if(requested<=MaximumLevel)return requested;
            int n=MaximumLevel-36, r=(requested-MaximumLevel)%n;
            if(r==0)r=n;
            int h=(n+1)/2;
            return r<=h?2*r+35:2*(r-h)+36;
        }
        public BattleLevelRow Resolve(int requested)
        {
            var id=ResolveIdentity(requested);
            if(!rows.TryGetValue(id,out var row))throw new InvalidOperationException("Unrecovered normal level "+id);
            return row;
        }
    }
}
