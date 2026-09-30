using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class RecoveredSpriteEffectImporter
    {
        [Serializable] sealed class Data {public Effect[] effects;}
        [Serializable] sealed class Effect {public string name;public int effectId;public Frame[] frames;public float stopTime;public bool loop;}
        [Serializable] sealed class Frame {public string path;public float time,ppu;public Vector2 pivot;public Vector4 border;public int filter;public bool sRGB;}
        const string Dest="Assets/AreaBattle/Resources/Recovered/Effects";
        public static void Import()
        {
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/presentation-prepared/sprite-effects-runtime.json")));
            Directory.CreateDirectory(Dest);AssetDatabase.Refresh();
            foreach(var e in data.effects)
            {
                if(e.effectId!=104&&e.effectId!=105)continue; //106 prefab transform not yet recovered.
                var sprites=new Sprite[e.frames.Length];var times=new float[e.frames.Length];
                for(int i=0;i<e.frames.Length;i++)
                {
                    var f=e.frames[i];string path=Dest+"/"+e.name+"-"+i+".png";
                    File.Copy(Path.Combine(BattleBuild.Target,f.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                    var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=f.pivot;settings.spritePixelsPerUnit=f.ppu;settings.spriteBorder=f.border;importer.SetTextureSettings(settings);
                    importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.sRGBTexture=f.sRGB;importer.filterMode=(FilterMode)f.filter;importer.alphaIsTransparency=true;importer.SaveAndReimport();
                    sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);times[i]=f.time;
                    if(sprites[i]==null)throw new InvalidDataException("Missing effect sprite "+path);
                }
                var obj=new GameObject(e.name);
                try{
                    obj.transform.localScale=Vector3.one*.30000001192092896f;
                    var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprites[0];renderer.color=Color.white;renderer.sortingOrder=0;
                    var player=obj.AddComponent<RecoveredSpriteEffect>();player.Frames=sprites;player.Times=times;player.ClipDuration=e.stopTime;player.Loop=e.loop;player.Lifetime=e.effectId==104?1:1.5f;
                    PrefabUtility.SaveAsPrefabAsset(obj,Dest+"/"+e.name+".prefab",out bool success);if(!success)throw new Exception("Failed recovered effect prefab");
                }finally{UnityEngine.Object.DestroyImmediate(obj);}
            }
            AssetDatabase.SaveAssets();
        }
    }
}
