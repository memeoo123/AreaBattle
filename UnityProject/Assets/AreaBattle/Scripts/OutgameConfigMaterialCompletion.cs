using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameConfigMaterialCompletion
    {
        // UtilitHelper.ToFileName: slash only, then first dot; not platform Path APIs.
        public static string ToFileName(string path)=>path.Substring(path.LastIndexOf("/")+1).Split('.')[0];
        // ConfigMgr30407: retain even a null material; overwrite an existing camp entry.
        public static void Loaded(OutgameLegacyPrefabResource resource,Dictionary<int,Material> materials)
        {
            int camp=resource.GetArg<int>(0);
            var material=resource.LoadAsset<Material>(ToFileName(resource.GetArg<string>(1)));
            materials[camp]=material;
        }
    }
}
