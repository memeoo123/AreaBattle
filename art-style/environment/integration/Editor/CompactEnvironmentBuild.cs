using System;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class CompactEnvironmentBuild
    {
        public static void RunBatch()
        {
            try{
                const string root="Assets/AreaBattle/Resources/ArtStyles/Compact";
                var importer=(TextureImporter)AssetImporter.GetAtPath(root+"/battlefield-ground.png");
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                string path=root+"/WarmStoneWall.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("AreaBattle/WarmStoneWall"));AssetDatabase.CreateAsset(material,path);}
                material.SetColor("_Stone",new Color(.96f,.90f,.78f));material.SetColor("_Mortar",new Color(.56f,.49f,.37f));EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();AdvancementValidation.RunBatch();
            }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
