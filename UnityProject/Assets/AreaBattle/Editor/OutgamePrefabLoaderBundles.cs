using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgamePrefabLoaderBundles
    {
        public static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/prefab-loader-native-bundles");
        public static readonly string[] Names={"scene-prefab","home-texture","game-texture","effect-prefab"};
        public static readonly string[] Keys={"model/scene/gamescene/hd4_cj_1.prefab.unity3d","textures/skinscene/scene_skin_idle1.png.unity3d","textures/skinscene/scene_skin_game1.png.unity3d","prefabs/effect/eff_idle_gubao.prefab.unity3d"};
        public static void Build()
        {
            try{
                Directory.CreateDirectory(Folder);
                var paths=new[]{"Assets/AreaBattle/Resources/Recovered/Background/HD4_CJ_1.prefab","Assets/AreaBattle/Resources/Recovered/Outgame/SceneTextures/scene_skin_idle1.png","Assets/AreaBattle/Resources/Recovered/Background/scene_skin_game1.png","Assets/AreaBattle/Resources/Recovered/Outgame/SceneEffects/eff_idle_GuBao.prefab"};
                var builds=paths.Select((path,i)=>new AssetBundleBuild{assetBundleName=Names[i],assetNames=new[]{path}}).ToArray();
                if(BuildPipeline.BuildAssetBundles(Folder,builds,BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64)==null)throw new Exception("Native source-asset bundle build failed");
                File.WriteAllText(Path.Combine(Folder,"source-assets.json"),JsonUtility.ToJson(new Manifest{paths=paths,keys=Keys,bundles=Names},true));EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
        [Serializable] sealed class Manifest{public string[] paths,keys,bundles;}
    }
}
