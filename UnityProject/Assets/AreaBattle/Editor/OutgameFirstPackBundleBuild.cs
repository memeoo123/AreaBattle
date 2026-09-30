using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
namespace AreaBattle.EditorTools
{
    public static class OutgameFirstPackBundleBuild
    {
        [Serializable] sealed class Manifest {public Row[] assets;}
        [Serializable] sealed class Row {public string assetPath,bundlePath,type,sha256;}
        [Serializable] sealed class Report {public bool passed;public string error,sha256;public long bytes;public int assetAddresses,configs,fonts,atlases,prefabs;public List<string> checks=new List<string>();}
        public static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/firstpack-native-bundles");
        public static void Build()
        {
            var report=new Report();AssetBundle bundle=null;
            try{
                var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/firstpack-bundle-build.json")));
                Require(manifest.assets.Length==91,"source firstpack explicit asset count");Directory.CreateDirectory(Folder);
                var build=new AssetBundleBuild{assetBundleName="firstpack.unity3d",assetNames=manifest.assets.Select(x=>x.assetPath).ToArray(),addressableNames=manifest.assets.Select(x=>x.bundlePath).ToArray()};
                var built=BuildPipeline.BuildAssetBundles(Folder,new[]{build},BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);Require(built!=null,"bundle build succeeds");
                string path=Path.Combine(Folder,"firstpack.unity3d");bundle=AssetBundle.LoadFromFile(path);Require(bundle!=null,"native firstpack loads");
                var names=new HashSet<string>(bundle.GetAllAssetNames());
                foreach(var row in manifest.assets){
                    Require(names.Contains(row.bundlePath),"source container address "+row.bundlePath);report.assetAddresses++;
                    if(row.type=="TextAsset"){
                        var asset=bundle.LoadAsset<TextAsset>(row.bundlePath);Require(asset!=null&&Hash(asset.bytes)==row.sha256,"bundled config byte identity "+row.bundlePath);report.configs++;
                    }else if(row.type=="Font"){
                        Require(bundle.LoadAsset<Font>(row.bundlePath)!=null,"bundled font "+row.bundlePath);report.fonts++;
                    }else if(row.type=="SpriteAtlas"){
                        var atlas=bundle.LoadAsset<SpriteAtlas>(row.bundlePath);Require(atlas!=null&&atlas.spriteCount==(atlas.name=="PublicUI"?50:13),"bundled atlas count");
                        var sprite=atlas.GetSprite(atlas.name=="PublicUI"?"Public_title":"Pulbic_add");Require(sprite!=null,"bundled atlas sprite lookup");UnityEngine.Object.DestroyImmediate(sprite);report.atlases++;
                    }else{
                        var asset=bundle.LoadAsset<GameObject>(row.bundlePath);Require(asset!=null,"bundled prefab "+row.bundlePath);report.prefabs++;
                    }
                }
                Require(bundle.LoadAsset<GameObject>("UIRoot")!=null&&bundle.LoadAsset<GameObject>("TipUI")!=null,"source basename resource instantiation lookup");
                var tip=bundle.LoadAsset<GameObject>("TipUI");var animation=tip.transform.Find("Content").GetComponent<Animation>();Require(animation!=null&&!animation.enabled&&animation.GetClipCount()==2,"native TipUI animation dependencies included");
                report.bytes=new FileInfo(path).Length;report.sha256=Hash(File.ReadAllBytes(path));report.checks.Add("All91 original addresses resolve in rebuilt native bundle");report.checks.Add("All85 bundled configurations preserve source bytes");report.checks.Add("Both fonts, both atlas sprite lookups and both UI prefabs resolve");report.checks.Add("UIRoot/TipUI basename loading and native animation dependencies preserved");report.passed=true;
            }catch(Exception ex){report.error=ex.ToString();Debug.LogException(ex);}
            finally{if(bundle!=null)bundle.Unload(true);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/firstpack-native-build-validation.json"),JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1);
        }
        static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
    }
}
