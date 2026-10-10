using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class CompactTowerArtBuild
    {
        public static void RunBatch()
        {
            try
            {
                const string root="Assets/AreaBattle/Resources/ArtStyles/Compact";
                foreach(string name in new[]{"compact-towers","basic-plain"})
                {
                    string path=root+"/"+name+".png";
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType=TextureImporterType.Default;importer.isReadable=true;
                    importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
                    importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;
                    importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
                    importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
                }
                string matPath=root+"/TowerCamp.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if(material==null){material=new Material(Shader.Find("AreaBattle/CompactTowerCamp"));AssetDatabase.CreateAsset(material,matPath);}
                AssetDatabase.SaveAssets();
                AdvancementValidation.RunBatch();
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
