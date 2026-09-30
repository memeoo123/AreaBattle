using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameSceneEffectAssets
    {
        static readonly Dictionary<string,string> Paths=new Dictionary<string,string>
        {
            {"effect/eff_idle_GuBao","eff_idle_GuBao"},
            {"effect/idle_eff_HuoShan","idle_eff_HuoShan"},
            {"effect/eff_idle_ZhaoZe","eff_idle_ZhaoZe"}
        };
        public static void Load(string path,Action<GameObject> complete)
        {
            if(!Paths.TryGetValue(path,out var name))throw new InvalidOperationException("Unrecovered original scene effect: "+path);
            var prefab=Resources.Load<GameObject>("Recovered/Outgame/SceneEffects/"+name);
            if(prefab==null)throw new InvalidOperationException("Original scene effect asset unavailable: "+path);
            var instance=UnityEngine.Object.Instantiate(prefab);instance.name=prefab.name;complete(instance);
        }
    }
}
