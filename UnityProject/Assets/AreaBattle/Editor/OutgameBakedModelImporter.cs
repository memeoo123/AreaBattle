using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameBakedModelImporter
    {
        [Serializable] sealed class Data {public Model[] models;}
        [Serializable] sealed class Model {public string name,autoPlay,source,sha256,sourcePrefab;public OutgameBakedAnimator.Clip[] clips;}
        public static void ImportBatch()
        {
            try{RecoveredSoldierImporter.ImportFrom(Path.Combine(BattleBuild.Target,"generated/outgame/model400-prepared"),"Assets/AreaBattle/Resources/Recovered/Outgame/ModelAssets",Path.Combine(BattleBuild.Workspace,"analysis/outgame-model400-import.json"));Import();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void Import()
        {
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/baked-model-clips.json")));
            string dest="Assets/AreaBattle/Resources/Recovered/Outgame/Models";Directory.CreateDirectory(dest);AssetDatabase.Refresh();
            foreach(var row in data.models)
            {
                var bytes=File.ReadAllBytes(Path.Combine(BattleBuild.Target,row.source));
                using(var sha=System.Security.Cryptography.SHA256.Create())
                    if(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()!=row.sha256)throw new InvalidDataException("Original model evidence changed");
                var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>(string.IsNullOrEmpty(row.sourcePrefab)?"Recovered/Soldiers/"+row.name:row.sourcePrefab));
                try
                {
                    go.name=row.name;UnityEngine.Object.DestroyImmediate(go.GetComponent<RecoveredSoldierVisual>());
                    var animator=go.AddComponent<OutgameBakedAnimator>();animator.Clips=row.clips;animator.AutoPlayAnimation=row.autoPlay;
                    PrefabUtility.SaveAsPrefabAsset(go,dest+"/"+row.name+".prefab",out bool ok);if(!ok)throw new InvalidDataException(row.name);
                }
                finally{UnityEngine.Object.DestroyImmediate(go);}
            }
            AssetDatabase.SaveAssets();
        }
    }
}
