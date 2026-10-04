using System;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameAvatarAssetImporter
    {
        public static void Run()
        {
            const string texturePath="Assets/AreaBattle/Resources/EnterGameUI/defaultIconTexture.png";
            AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;
            importer.sRGBTexture=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.alphaIsTransparency=false;
            importer.filterMode=FilterMode.Bilinear;importer.wrapModeU=TextureWrapMode.Clamp;importer.wrapModeV=TextureWrapMode.Clamp;importer.wrapModeW=TextureWrapMode.Repeat;importer.anisoLevel=1;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=128;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(!texture)throw new Exception("imported Texture2D missing; shape="+importer.textureShape);
            var sprite=Sprite.Create(texture,new Rect(0,0,128,128),new Vector2(.5f,.5f),80,1,SpriteMeshType.FullRect);sprite.name="defaultIcon";
            const string path="Assets/AreaBattle/Resources/EnterGameUI/defaultIcon.asset";
            var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(old){EditorUtility.CopySerialized(sprite,old);UnityEngine.Object.DestroyImmediate(sprite);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(sprite,path);
            AssetDatabase.SaveAssets();
            // Runtime OverrideGeometry/OverridePhysicsShape are not serialized by CreateAsset.
            // Restore original native data directly in both Unity6 render-data copies.
            string yaml=File.ReadAllText(path);
            yaml=Regex.Replace(yaml,@"indexCount: \d+","indexCount: 15");
            yaml=Regex.Replace(yaml,@"vertexCount: \d+","vertexCount: 7");
            yaml=Regex.Replace(yaml,@"m_VertexCount: \d+","m_VertexCount: 7");
            yaml=Regex.Replace(yaml,@"m_IndexBuffer: [0-9a-f]+","m_IndexBuffer: 060005000100040001000500030001000400000001000300020001000000");
            yaml=Regex.Replace(yaml,@"m_DataSize: \d+","m_DataSize: 152");
            yaml=Regex.Replace(yaml,@"_typelessdata: [0-9a-f]+","_typelessdata: cdccec3ecdcc2c3f000000006666063fcdcc4cbf000000006666063f000080bd000000006666a63e6666463f000000009a9959be6666463f00000000cdccecbecdcc2c3f00000000cdccecbecdcc4cbf000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            yaml=Regex.Replace(yaml,@"textureRect:\s+serializedVersion: 2\s+x: [^\n]+\s+y: [^\n]+\s+width: [^\n]+\s+height: [^\n]+","textureRect:\n      serializedVersion: 2\n      x: 27.076120376586914\n      y: 0\n      width: 78.84776306152344\n      height: 125.97325134277344");
            yaml=Regex.Replace(yaml,@"textureRectOffset: [^\n]+","textureRectOffset: {x: 27.076120376586914, y: 0}");
            yaml=Regex.Replace(yaml,@"settingsRaw: \d+","settingsRaw: 64");
            yaml=Regex.Replace(yaml,@"  m_PhysicsShape:[\s\S]*?(?=  m_Bones:)","  m_PhysicsShape:\n  - - {x: 0.11250000447034836, y: -0.7750000357627869}\n    - {x: 0.10000000149011612, y: -0.7125000357627869}\n    - {x: 0.13750000298023224, y: -0.800000011920929}\n    - {x: 0.48750001192092896, y: -0.800000011920929}\n    - {x: 0.48750001192092896, y: -0.7750000357627869}\n    - {x: 0.4124999940395355, y: -0.42500001192092896}\n    - {x: 0.32500001788139343, y: -0.16250000894069672}\n    - {x: 0.32500001788139343, y: 0.08749999850988388}\n    - {x: 0.4000000059604645, y: 0.20000000298023224}\n    - {x: 0.4000000059604645, y: 0.4625000059604645}\n    - {x: 0.3499999940395355, y: 0.612500011920929}\n    - {x: 0.13750000298023224, y: 0.762499988079071}\n    - {x: -0.125, y: 0.762499988079071}\n    - {x: -0.30000001192092896, y: 0.6875}\n    - {x: -0.42500001192092896, y: 0.4625000059604645}\n    - {x: -0.42500001192092896, y: 0.375}\n    - {x: -0.4124999940395355, y: 0.20000000298023224}\n    - {x: -0.3499999940395355, y: -0.17499999701976776}\n    - {x: -0.21250000596046448, y: -0.3125}\n    - {x: -0.42500001192092896, y: -0.762499988079071}\n    - {x: -0.42500001192092896, y: -0.800000011920929}\n    - {x: -0.22500000894069672, y: -0.800000011920929}\n    - {x: -0.15000000596046448, y: -0.737500011920929}\n    - {x: -0.10000000149011612, y: -0.800000011920929}\n    - {x: 0.11250000447034836, y: -0.800000011920929}\n");
            File.WriteAllText(path,yaml);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);

            var loaded=Resources.Load<Sprite>("EnterGameUI/defaultIcon");
            if(!loaded||loaded.vertices.Length!=7||loaded.triangles.Length!=15||loaded.pixelsPerUnit!=80||loaded.texture.width!=128||loaded.GetPhysicsShapeCount()!=1)throw new Exception("original avatar geometry/texture import: loaded="+loaded+" vertices="+(loaded?loaded.vertices.Length:-1)+" indices="+(loaded?loaded.triangles.Length:-1)+" PPU="+(loaded?loaded.pixelsPerUnit:-1)+" width="+(loaded&&loaded.texture?loaded.texture.width:-1)+" physics="+(loaded?loaded.GetPhysicsShapeCount():-1));
            Debug.Log("AREABATTLE_AVATAR_IMPORT_PASS vertices=7 triangles=15 ppu=80");
        }
    }
}
