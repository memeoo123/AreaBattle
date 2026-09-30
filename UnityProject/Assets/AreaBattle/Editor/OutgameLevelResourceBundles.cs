using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelResourceBundles
    {
        public static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/level-resource-native-bundles");
        public static void Build()
        {
            try{
                Directory.CreateDirectory(Folder);
                var builds=new AssetBundleBuild[2];
                for(int i=1;i<=2;i++)builds[i-1]=new AssetBundleBuild{assetBundleName="level-"+i,assetNames=new[]{"Assets/AreaBattle/Resources/Data/Levels/level_"+i+".json"},addressableNames=new[]{"Level_"+i}};
                if(!BuildPipeline.BuildAssetBundles(Folder,builds,BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64))throw new Exception("Level bundle build failed");
                RecoveredHudImporter.ImportLoadingBatch();
            }catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
    }
}
