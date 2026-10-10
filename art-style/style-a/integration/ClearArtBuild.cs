using System;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class ClearArtBuild
    {
        public static void RunBatch()
        {
            try{
                const string root="Assets/AreaBattle/Resources/ArtStyles/ClearA/";
                foreach(string name in new[]{"towers","towers-upright-v7","arrow-upright-v6","ground","soldier-run"}){
                    var importer=(TextureImporter)AssetImporter.GetAtPath(root+name+".png");
                    importer.textureType=TextureImporterType.Default;importer.isReadable=name!="ground";
                    importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=name!="ground";
                    importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.maxTextureSize=4096;
                    importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
                }
                var wall=Resources.Load<Material>("ArtStyles/Compact/WarmStoneWall");
                wall.SetColor("_Stone",new Color(.90f,.85f,.72f));wall.SetColor("_Mortar",new Color(.40f,.36f,.29f));EditorUtility.SetDirty(wall);
                AssetDatabase.SaveAssets();AdvancementValidation.RunBatch();
            }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}




