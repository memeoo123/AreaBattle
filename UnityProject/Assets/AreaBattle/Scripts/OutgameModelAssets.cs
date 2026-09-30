using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // EntityModelConfig + source PlayerControl category3 prefabId+1000 mapping.
    // Only evidence-backed imported assets are registered; absent skins fail explicitly.
    public sealed class OutgameModelAssets
    {
        static readonly Dictionary<int,string> Paths=new Dictionary<int,string>
        {
            {1000,"Recovered/Outgame/Models/soldier_100"},{1002,"Recovered/Outgame/Models/soldier_102"},
            {2000,"Recovered/Outgame/Models/soldier_200"},{4000,"Recovered/Outgame/Models/soldier_400"},
            {9033,"Recovered/BossEmbedded/BBDyShadow"},{9034,"Recovered/BossEmbedded/QBDyShadow"}
        };
        public void Load(int id,Action<GameObject> ready)
        {
            if(!Paths.TryGetValue(id,out string path))throw new InvalidOperationException("Original outgame model asset not imported: "+id);
            var prefab=Resources.Load<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Missing original model resource: "+path);
            var instance=UnityEngine.Object.Instantiate(prefab);instance.name=prefab.name;ready(instance);
        }
    }
}
