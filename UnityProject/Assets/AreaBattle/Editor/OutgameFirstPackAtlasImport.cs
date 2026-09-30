using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
namespace AreaBattle.EditorTools
{
    public static class OutgameFirstPackAtlasImport
    {
        [Serializable] sealed class Manifest {public AtlasRow[] atlases;}
        [Serializable] sealed class AtlasRow {public string name,assetPath;public SpriteRow[] sprites;}
        [Serializable] sealed class SpriteRow {public string name,assetPath,sha256;public Vector2 pivot;public Vector4 border;public float pixelsPerUnit,width,height;}
        [Serializable] sealed class Report {public bool passed;public string error;public int atlases,sprites;public List<string> checks=new List<string>();}
        public static void Run()
        {
            var report=new Report();
            try{
                var path=Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/firstpack-atlas-import.json");
                var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(path));AssetDatabase.Refresh();var built=new List<SpriteAtlas>();
                foreach(var row in manifest.atlases){
                    var packables=new List<UnityEngine.Object>();
                    foreach(var item in row.sprites){
                        var importer=(TextureImporter)AssetImporter.GetAtPath(item.assetPath);
                        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                        importer.spritePixelsPerUnit=item.pixelsPerUnit;importer.spritePivot=item.pivot;importer.spriteBorder=item.border;
                        importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;
                        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
                        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(item.assetPath);Require(sprite!=null&&sprite.name==item.name,"sprite name "+item.name);
                        Require(Mathf.Abs(sprite.rect.width-item.width)<.0001f&&Mathf.Abs(sprite.rect.height-item.height)<.0001f&&sprite.border==item.border&&Mathf.Approximately(sprite.pixelsPerUnit,item.pixelsPerUnit),"sprite geometry "+item.name);
                        Require(Vector2.Distance(sprite.pivot,new Vector2(item.pivot.x*item.width,item.pivot.y*item.height))<.001f,"sprite pivot "+item.name);
                        packables.Add(sprite);
                    }
                    var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(row.assetPath);
                    if(atlas==null){atlas=new SpriteAtlas();AssetDatabase.CreateAsset(atlas,row.assetPath);}
                    atlas.name=row.name;atlas.Remove(atlas.GetPackables());atlas.Add(packables.ToArray());
                    atlas.SetPackingSettings(new SpriteAtlasPackingSettings{enableRotation=false,enableTightPacking=false,padding=4});
                    atlas.SetTextureSettings(new SpriteAtlasTextureSettings{readable=true,generateMipMaps=false,sRGB=true,filterMode=FilterMode.Bilinear});
                    atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name="DefaultTexturePlatform",maxTextureSize=4096,format=TextureImporterFormat.RGBA32,textureCompression=TextureImporterCompression.Uncompressed,compressionQuality=100});
                    atlas.SetIncludeInBuild(true);EditorUtility.SetDirty(atlas);built.Add(atlas);
                }
                AssetDatabase.SaveAssets();SpriteAtlasUtility.PackAtlases(built.ToArray(),BuildTarget.StandaloneWindows64);
                for(int i=0;i<manifest.atlases.Length;i++){
                    var row=manifest.atlases[i];var atlas=built[i];Require(atlas.spriteCount==row.sprites.Length,"atlas sprite count "+row.name);
                    foreach(var item in row.sprites){var sprite=atlas.GetSprite(item.name);Require(sprite!=null,"atlas lookup "+item.name);UnityEngine.Object.DestroyImmediate(sprite);report.sprites++;}
                    report.atlases++;report.checks.Add(row.name+": all source sprite names, dimensions, pivot, border and PPU verified; native atlas lookup complete");
                }
                report.passed=true;
            }catch(Exception ex){report.error=ex.ToString();Debug.LogException(ex);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/firstpack-atlas-import-validation.json"),JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1);
        }
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    }
}
