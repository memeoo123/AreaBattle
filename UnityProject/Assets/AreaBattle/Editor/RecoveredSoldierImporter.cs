using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class RecoveredSoldierImporter
    {
        [Serializable] public sealed class Data {public Soldier[] soldiers;public Camp[] campColors;}
        [Serializable] public sealed class Camp {public int camp;public float[] rgba;}
        [Serializable] public sealed class Soldier
        {
            public string name,meshSourceId,animTexturePath,animTextureSha256,mainTexturePath;
            public int soldierType,animWidth,animHeight,animFilter,animWrap,mainFilter,mainWrap,sortingOrder,renderQueue;
            public float[] animAdd,animMul;
            public float runStart,runFrames,runSeconds,deadStart,deadFrames,deadSeconds,scale;
            public bool animLinear,mainSRGB;
        }
        [Serializable] public sealed class MeshData {public NativeMesh[] meshes;}
        [Serializable] public sealed class Submesh {public int[] indices;}
        [Serializable] public sealed class NativeMesh
        {public string name;public Vector3[] vertices,normals;public Vector2[] uv0,uv1;public Color[] colors;public Vector4[] tangents;public Submesh[] submeshes;public bool hasColors,hasNormals,hasTangents;}
        [Serializable] public sealed class ImportRecord {public string name,sourceMesh,animationSha256,prefab;public int vertices,indices;}
        [Serializable] public sealed class Report {public bool passed;public ImportRecord[] soldiers;public string scope="Three original default ordinary soldiers, native mesh and packed GPU run animation. Special units, death effects and live-original visual matching remain pending.";}
        const string Dest="Assets/AreaBattle/Resources/Recovered/Soldiers";
        static Vector4 Vector(float[] a)=>new Vector4(a[0],a[1],a[2],a[3]);
        public static void Import()
        {ImportFrom(Path.Combine(BattleBuild.Target,"generated/presentation-prepared"),Dest,Path.Combine(BattleBuild.Workspace,"analysis/soldier-import-report.json"));}
        public static void ImportFrom(string prep,string destination,string reportPath)
        {
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(prep,"runtime-data.json")));
            var meshes=JsonUtility.FromJson<MeshData>(File.ReadAllText(Path.Combine(prep,"soldier-meshes-runtime.json")));
            Directory.CreateDirectory(destination);AssetDatabase.Refresh();
            var shader=Shader.Find("AreaBattle/Recovered SkeletonMeshBaker");
            if(shader==null)throw new InvalidDataException("Recovered animation shader not imported");
            var report=new Report{soldiers=new ImportRecord[data.soldiers.Length]};
            for(int index=0;index<data.soldiers.Length;index++)
            {
                var s=data.soldiers[index];var source=meshes.meshes.Single(m=>m.name==s.name);
                string stem=destination+"/"+s.name;
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(stem+".asset");
                if(mesh==null){mesh=new Mesh{name=s.name};AssetDatabase.CreateAsset(mesh,stem+".asset");}else mesh.Clear();
                mesh.vertices=source.vertices;mesh.uv=source.uv0;mesh.uv2=source.uv1;
                if(source.hasNormals)mesh.normals=source.normals;
                if(source.hasColors)mesh.colors=source.colors;
                if(source.hasTangents)mesh.tangents=source.tangents;
                mesh.subMeshCount=source.submeshes.Length;int count=0;
                for(int sub=0;sub<source.submeshes.Length;sub++){mesh.SetTriangles(source.submeshes[sub].indices,sub,false);count+=source.submeshes[sub].indices.Length;}
                // Animated vertices can lie outside the static mesh; bounds span the original quantizer.
                var add=Vector(s.animAdd);var mul=Vector(s.animMul);
                mesh.bounds=new Bounds(new Vector3(add.x+mul.x/2,add.y+mul.y/2,add.z),new Vector3(mul.x,mul.y,.02f));
                EditorUtility.SetDirty(mesh);
                byte[] raw=File.ReadAllBytes(Path.Combine(BattleBuild.Target,s.animTexturePath));
                string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(raw)).Replace("-","").ToLowerInvariant();
                if(hash!=s.animTextureSha256||raw.Length!=s.animWidth*s.animHeight*4)throw new InvalidDataException("Packed animation source mismatch: "+s.name);
                string animPath=stem+"-anim.asset";
                var anim=AssetDatabase.LoadAssetAtPath<Texture2D>(animPath);
                if(anim==null){anim=new Texture2D(s.animWidth,s.animHeight,TextureFormat.RGBA32,false,s.animLinear);AssetDatabase.CreateAsset(anim,animPath);}
                anim.LoadRawTextureData(raw);anim.filterMode=(FilterMode)s.animFilter;anim.wrapMode=(TextureWrapMode)s.animWrap;anim.Apply(false,false);EditorUtility.SetDirty(anim);
                string mainPath=stem+"-atlas.png";File.Copy(Path.Combine(BattleBuild.Target,s.mainTexturePath),mainPath,true);
                AssetDatabase.ImportAsset(mainPath,ImportAssetOptions.ForceSynchronousImport);
                var ti=(TextureImporter)AssetImporter.GetAtPath(mainPath);ti.textureType=TextureImporterType.Default;
                ti.npotScale=TextureImporterNPOTScale.None;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;
                ti.sRGBTexture=s.mainSRGB;ti.filterMode=(FilterMode)s.mainFilter;ti.wrapMode=(TextureWrapMode)s.mainWrap;
                ti.alphaIsTransparency=false;ti.maxTextureSize=4096;ti.SaveAndReimport();
                var material=AssetDatabase.LoadAssetAtPath<Material>(stem+".mat");
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,stem+".mat");}
                material.shader=shader;material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(mainPath);
                material.SetTexture("_AnimTex",anim);material.SetVector("_AnimAdd",add);material.SetVector("_AnimMul",mul);material.SetFloat("_AnimLoop",1);material.renderQueue=s.renderQueue;EditorUtility.SetDirty(material);
                var go=new GameObject(s.name);
                try{
                    go.transform.localScale=Vector3.one*s.scale;go.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingLayerName="Default";renderer.sortingOrder=s.sortingOrder;
                    renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                    var runtime=go.AddComponent<RecoveredSoldierVisual>();runtime.RunStart=s.runStart;runtime.RunFrames=s.runFrames;runtime.RunSeconds=s.runSeconds;runtime.SourceScale=s.scale;
                    runtime.DeadStart=s.deadStart;runtime.DeadFrames=s.deadFrames;runtime.DeadSeconds=s.deadSeconds;
                    runtime.CampColors=new Color[7];foreach(var c in data.campColors)runtime.CampColors[c.camp]=new Color(c.rgba[0],c.rgba[1],c.rgba[2],c.rgba[3]);
                    PrefabUtility.SaveAsPrefabAsset(go,stem+".prefab",out bool ok);if(!ok)throw new InvalidDataException("Failed prefab: "+s.name);
                }finally{UnityEngine.Object.DestroyImmediate(go);}
                report.soldiers[index]=new ImportRecord{name=s.name,sourceMesh=s.meshSourceId,animationSha256=hash,prefab=stem+".prefab",vertices=mesh.vertexCount,indices=count};
            }
            AssetDatabase.SaveAssets();report.passed=true;
            File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));
        }
    }
}
