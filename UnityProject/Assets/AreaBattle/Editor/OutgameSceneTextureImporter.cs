using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameSceneTextureImporter
    {
        [Serializable] sealed class Data {public Row[] textures;}
        [Serializable] sealed class Row {public string name,path,sha256;public bool sRGB,mipmaps;public int filter,wrap,width,height;}
        public static void ImportBatch()
        {
            try
            {
                var data=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/scene-texture-import.json")));
                string dest="Assets/AreaBattle/Resources/Recovered/Outgame/SceneTextures";Directory.CreateDirectory(dest);
                foreach(var row in data.textures)
                {
                    byte[] bytes=File.ReadAllBytes(Path.Combine(BattleBuild.Target,row.path));
                    using(var sha=System.Security.Cryptography.SHA256.Create())if(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()!=row.sha256)throw new InvalidDataException(row.name);
                    string path=dest+"/"+row.name+".png";File.WriteAllBytes(path,bytes);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.sRGBTexture=row.sRGB;importer.mipmapEnabled=row.mipmaps;importer.filterMode=(FilterMode)row.filter;importer.wrapMode=(TextureWrapMode)row.wrap;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;importer.SaveAndReimport();
                    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture.width!=row.width||texture.height!=row.height)throw new InvalidDataException("Original scene texture resized: "+row.name);
                }
                AssetDatabase.SaveAssets();EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
