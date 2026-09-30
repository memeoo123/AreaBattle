using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AreaBattle.EditorTools
{
    public static class RecoveredObstacleImporter
    {
        const string Destination="Assets/AreaBattle/Resources/Recovered/Obstacles";
        [Serializable] class Manifest {public PrefabSource[] prefabs;public MeshSource[] meshes;public MaterialSource[] materials;public TextureSource[] textures;}
        [Serializable] class Source {public string id,name,sourceBundle;public SourceFile[] files;}
        [Serializable] class SourceFile {public string kind,path,sha256;}
        [Serializable] class MeshSource {public string id,name;public Source source;public Vector3[] vertices,normals;public Vector2[] uv0,uv1;public Vector4[] tangents;public Color[] colors;public bool hasNormals,hasColors,hasTangents;public Submesh[] submeshes;public BoundsSource bounds;}
        [Serializable] class BoundsSource {public Vector3 m_Center,m_Extent;}
        [Serializable] class Submesh {public int[] indices;}
        [Serializable] class TextureSource {public string id,name,path,sha256;public int width,height;public bool mipmap,srgb;public TextureSettings settings;public Source source;}
        [Serializable] class TextureSettings {public int m_FilterMode,m_Aniso,m_WrapU,m_WrapV,m_WrapW;public float m_MipBias;}
        [Serializable] class MaterialSource {public string id,name,shader,textureId;public Vector2 scale,offset;public int renderQueue;public Source source;}
        [Serializable] class PrefabSource {public string name,sourcePath,sourceSha256,sourceBundle,sourceBundleSha256;public int entityId;public Node[] nodes;}
        [Serializable] class Node {public string name,sourceId,meshId;public string[] materialIds;public int layer,parent;public bool active;public TransformSource transform;public RendererSource renderer;}
        [Serializable] class TransformSource {public Vector3 m_LocalPosition,m_LocalScale;public Quaternion m_LocalRotation;}
        [Serializable] class RendererSource {public bool m_Enabled;public int m_CastShadows,m_ReceiveShadows,m_LightProbeUsage,m_ReflectionProbeUsage,m_SortingLayerID,m_SortingOrder,m_MotionVectors;public uint m_RenderingLayerMask;}
        [Serializable] class ImportReport {public bool passed;public int prefabs,renderers,meshes,textures,colliders;public string scope="Original obstacle visuals only; existing collision owner untouched. Matched original-frame validation remains pending.";}
        static string Target=>Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43");
        static Manifest Read()=>JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Target,"generated/obstacle-visual-runtime.json")));
        static void Verify(string path,string expected)
        {using(var hash=SHA256.Create())if(BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(Path.Combine(Target,path)))).Replace("-","").ToLowerInvariant()!=expected)throw new InvalidDataException("Obstacle evidence changed: "+path);}
        static void Verify(Source source){foreach(var file in source.files)Verify(file.path,file.sha256);}
        static void Provenance(string path,Source source)
        {var importer=AssetImporter.GetAtPath(path);if(importer!=null){importer.userData=JsonUtility.ToJson(source);EditorUtility.SetDirty(importer);}}

        [MenuItem("AreaBattle/Import recovered obstacles")]
        public static void Import()
        {
            var input=Read();if(input.prefabs.Length!=15)throw new InvalidDataException("Expected all 15 used obstacle prefab types.");
            Directory.CreateDirectory(Destination);AssetDatabase.Refresh();
            var meshes=new Dictionary<string,Mesh>();var textures=new Dictionary<string,Texture2D>();var materials=new Dictionary<string,Material>();
            var report=new ImportReport();
            foreach(var data in input.meshes)
            {
                Verify(data.source);string path=Destination+"/"+data.name+".asset";
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(mesh==null){mesh=new Mesh{name=data.name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                mesh.vertices=data.vertices;mesh.uv=data.uv0;mesh.uv2=data.uv1;
                if(data.hasNormals)mesh.normals=data.normals;if(data.hasColors)mesh.colors=data.colors;if(data.hasTangents)mesh.tangents=data.tangents;
                mesh.subMeshCount=data.submeshes.Length;
                for(int i=0;i<data.submeshes.Length;i++)mesh.SetTriangles(data.submeshes[i].indices,i,false);
                mesh.bounds=new Bounds(data.bounds.m_Center,data.bounds.m_Extent*2);
                EditorUtility.SetDirty(mesh);Provenance(path,data.source);meshes.Add(data.id,mesh);report.meshes++;
            }
            foreach(var data in input.textures)
            {
                Verify(data.source);Verify(data.path,data.sha256);string path=Destination+"/"+data.name+".png";
                File.Copy(Path.Combine(Target,data.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
                importer.sRGBTexture=data.srgb;importer.mipmapEnabled=data.mipmap;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=false;
                importer.filterMode=(FilterMode)data.settings.m_FilterMode;importer.anisoLevel=data.settings.m_Aniso;importer.mipMapBias=data.settings.m_MipBias;
                importer.wrapModeU=(TextureWrapMode)data.settings.m_WrapU;importer.wrapModeV=(TextureWrapMode)data.settings.m_WrapV;importer.wrapModeW=(TextureWrapMode)data.settings.m_WrapW;
                importer.userData=JsonUtility.ToJson(data.source);importer.SaveAndReimport();textures.Add(data.id,AssetDatabase.LoadAssetAtPath<Texture2D>(path));report.textures++;
            }
            foreach(var data in input.materials)
            {
                Verify(data.source);var shader=Shader.Find(data.shader);if(shader==null)throw new InvalidDataException("Missing original built-in shader "+data.shader);
                string path=Destination+"/"+data.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
                material.SetTexture("_MainTex",textures[data.textureId]);material.SetTextureScale("_MainTex",data.scale);material.SetTextureOffset("_MainTex",data.offset);material.renderQueue=data.renderQueue;
                EditorUtility.SetDirty(material);Provenance(path,data.source);materials.Add(data.id,material);
            }
            foreach(var prefab in input.prefabs)
            {
                Verify(prefab.sourcePath,prefab.sourceSha256);Verify(prefab.sourceBundle,prefab.sourceBundleSha256);
                var nodes=new List<GameObject>();
                try
                {
                    foreach(var node in prefab.nodes)
                    {
                        var obj=new GameObject(node.name){layer=node.layer};nodes.Add(obj);
                        if(node.parent>=0)obj.transform.SetParent(nodes[node.parent].transform,false);
                        obj.transform.localPosition=node.transform.m_LocalPosition;obj.transform.localRotation=node.transform.m_LocalRotation;obj.transform.localScale=node.transform.m_LocalScale;
                        if(!string.IsNullOrEmpty(node.meshId))
                        {
                            obj.AddComponent<MeshFilter>().sharedMesh=meshes[node.meshId];var renderer=obj.AddComponent<MeshRenderer>();var r=node.renderer;
                            renderer.sharedMaterials=node.materialIds.Select(id=>materials[id]).ToArray();renderer.enabled=r.m_Enabled;
                            renderer.shadowCastingMode=(ShadowCastingMode)r.m_CastShadows;renderer.receiveShadows=r.m_ReceiveShadows!=0;
                            renderer.lightProbeUsage=(LightProbeUsage)r.m_LightProbeUsage;renderer.reflectionProbeUsage=(ReflectionProbeUsage)r.m_ReflectionProbeUsage;
                            renderer.motionVectorGenerationMode=(MotionVectorGenerationMode)r.m_MotionVectors;renderer.renderingLayerMask=r.m_RenderingLayerMask;
                            renderer.sortingLayerID=r.m_SortingLayerID;renderer.sortingOrder=r.m_SortingOrder;report.renderers++;
                        }
                        obj.SetActive(node.active);
                    }
                    string path=Destination+"/Entity_"+prefab.entityId+".prefab";
                    PrefabUtility.SaveAsPrefabAsset(nodes[0],path,out bool success);if(!success)throw new InvalidDataException("Failed obstacle prefab "+prefab.entityId);
                    report.colliders+=nodes[0].GetComponentsInChildren<Collider>(true).Length;report.prefabs++;
                    Provenance(path,new Source{id=prefab.nodes[0].sourceId,name=prefab.name,sourceBundle=prefab.sourceBundle,files=new[]{new SourceFile{kind="flat-original-prefab",path=prefab.sourcePath,sha256=prefab.sourceSha256},new SourceFile{kind="original-bundle",path=prefab.sourceBundle,sha256=prefab.sourceBundleSha256}}});
                }finally{if(nodes.Count>0)UnityEngine.Object.DestroyImmediate(nodes[0]);}
            }
            if(report.renderers!=125||report.colliders!=0)throw new InvalidDataException("Obstacle visual coverage mismatch");
            AssetDatabase.SaveAssets();report.passed=true;
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/obstacle-visual-import-report.json"),JsonUtility.ToJson(report,true));
        }

        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original obstacle visual dependencies and source geometry/transform fields checked; collision logic is reused unchanged; original matched visual capture pending."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            var input=Read();
            check("obstacle-original-mesh",()=>{
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Destination+"/HD3_wall.asset");var source=input.meshes.Single();
                Require(mesh!=null&&mesh.vertexCount==1178&&mesh.subMeshCount==1&&mesh.triangles.Length==2436,"source native mesh counts");
                Require(mesh.vertices.SequenceEqual(source.vertices)&&mesh.triangles.SequenceEqual(source.submeshes[0].indices),"native coordinates/winding");
                Require(mesh.bounds.center==source.bounds.m_Center&&mesh.bounds.extents==source.bounds.m_Extent,"original bounds");
            });
            check("obstacle-source-material",()=>{
                var material=AssetDatabase.LoadAssetAtPath<Material>(Destination+"/wall.mat");var texture=material.mainTexture;
                Require(material.shader.name=="Unlit/Texture"&&material.renderQueue==2000,"source shader/default opaque queue");
                Require(texture.width==256&&texture.height==256&&texture.filterMode==FilterMode.Bilinear&&texture.wrapMode==TextureWrapMode.Repeat,"source texture dimensions/sampling");
                Require(material.mainTextureScale==Vector2.one&&material.mainTextureOffset==Vector2.zero,"UV transform");
            });
            check("obstacle-all-prefab-hierarchies",()=>{
                int rendererCount=0;
                foreach(var source in input.prefabs)
                {
                    var prefab=Resources.Load<GameObject>("Recovered/Obstacles/Entity_"+source.entityId);Require(prefab!=null,"missing "+source.entityId);
                    var nodes=prefab.GetComponentsInChildren<Transform>(true);Require(nodes.Length==source.nodes.Length,"hierarchy size "+source.entityId);
                    for(int i=0;i<nodes.Length;i++)
                    {
                        var node=source.nodes[i];var tr=nodes[i];Require(tr.name==(i==0?"Entity_"+source.entityId:node.name)&&tr.localPosition==node.transform.m_LocalPosition&&tr.localRotation==node.transform.m_LocalRotation&&tr.localScale==node.transform.m_LocalScale,"source local transform "+source.entityId+"/"+i);
                        Require(tr.gameObject.activeSelf==node.active&&tr.gameObject.layer==node.layer,"source visibility/layer");
                    }
                    Require(prefab.GetComponentsInChildren<Collider>(true).Length==0,"visual added physics");rendererCount+=prefab.GetComponentsInChildren<MeshRenderer>(true).Length;
                }
                Require(rendererCount==125,"source125 renderers");
            });
            check("obstacle-attachment-keeps-collision",()=>{
                var parent=new GameObject("obstacle collision preservation check");
                try
                {
                    var layout=new LevelLayout{ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=81,pos=new IntVector3{x=125,y=10,z=250},angle=new IntVector3{y=4500},scale=new IntVector3{x=100,y=100,z=100}}}};
                    var roots=BattleObstacles.Create(layout,parent.transform);var root=roots[0];var boxes=root.GetComponentsInChildren<BoxCollider>(true);var before=boxes.Select(c=>c.transform.localToWorldMatrix).ToArray();
                    Require(boxes.Length==1,"fixture source collider");var visuals=RecoveredObstacleVisual.Attach(layout,roots);
                    Require(root.GetComponentsInChildren<Collider>(true).Length==boxes.Length&&boxes.Select(c=>c.transform.localToWorldMatrix).SequenceEqual(before),"collision geometry altered");
                    var visual=visuals.Single();Require(visual.transform.localPosition==Vector3.zero&&visual.transform.localRotation==Quaternion.identity&&visual.transform.localScale==Vector3.one,"root cfg transform applied twice");
                    var renderer=visual.GetComponentInChildren<MeshRenderer>();Require(renderer!=null,"original visible mesh attached");
                    Require(renderer.transform.localToWorldMatrix==boxes[0].transform.localToWorldMatrix,"original model/box node correspondence");
                }finally{UnityEngine.Object.DestroyImmediate(parent);}
            });
            return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
