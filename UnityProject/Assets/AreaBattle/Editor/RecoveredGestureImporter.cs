using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class RecoveredGestureImporter
    {
        const string Destination="Assets/AreaBattle/Resources/Recovered/Gestures";
        static string Target=>Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43");
        [Serializable] class Manifest {public PrefabSource[] prefabs;public MaterialSource[] materials;public TextureSource[] textures;public MeshSource[] meshes;public External[] externalAssets;}
        [Serializable] class PrefabSource {public string name,template,sha256;}
        [Serializable] class External {public string id,placeholder,name,type;}
        [Serializable] class TextureSource {public string id,name,path,sha256;public int width,height;public bool mipmap,srgb;public TextureSettings settings;}
        [Serializable] class TextureSettings {public int m_FilterMode,m_Aniso,m_WrapU,m_WrapV,m_WrapW;public float m_MipBias;}
        [Serializable] class FloatValue {public string name;public float value;}
        [Serializable] class ColorValue {public string name;public Color value;}
        [Serializable] class MaterialSource {public string id,name,textureId,shader,source,sha256;public int renderQueue,lightmapFlags;public bool instancing,doubleSidedGI;public Vector2 scale,offset;public FloatValue[] floats;public ColorValue[] colors;}
        [Serializable] class MeshSource {public string id,name,source,sha256;public Vector3[] vertices,normals;public Vector2[] uv0,uv1;public Vector4[] tangents;public Color[] colors;public Submesh[] submeshes;public BoundsSource bounds;}
        [Serializable] class Submesh {public int[] indices;}
        [Serializable] class BoundsSource {public Vector3 m_Center,m_Extent;}
        [Serializable] class DragTrace {public int layout=5,source,target;public Vector3 point;public bool previewValid,committed;public string[] inputs;public List<string> events=new List<string>();}
        static Manifest Read()=>JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Target,"generated/gesture-visual-runtime.json")));
        static string Verified(string path,string expected)
        {
            string full=Path.Combine(Target,path);
            using(var hash=SHA256.Create())if(BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(full))).Replace("-","").ToLowerInvariant()!=expected)
                throw new InvalidDataException("Gesture source fingerprint changed: "+path);
            return full;
        }
        [MenuItem("AreaBattle/Import recovered arrows and gestures")]
        public static void Import()
        {
            ImportManifest("generated/gesture-visual-runtime.json",Destination);
            ImportManifest("generated/skill-target-runtime.json","Assets/AreaBattle/Resources/Recovered/SkillTargets");
        }
        static void ImportManifest(string manifestPath,string Destination)
        {
            var input=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Target,manifestPath)));Directory.CreateDirectory(Destination);var paths=new Dictionary<string,string>();var textures=new Dictionary<string,Texture2D>();
            foreach(var item in input.textures)
            {
                string path=Destination+"/"+item.name+".png";File.Copy(Verified(item.path,item.sha256),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
                importer.sRGBTexture=item.srgb;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=false;
                importer.mipmapEnabled=item.mipmap;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.filterMode=(FilterMode)item.settings.m_FilterMode;importer.anisoLevel=item.settings.m_Aniso;importer.mipMapBias=item.settings.m_MipBias;
                importer.wrapModeU=(TextureWrapMode)item.settings.m_WrapU;importer.wrapModeV=(TextureWrapMode)item.settings.m_WrapV;importer.wrapModeW=(TextureWrapMode)item.settings.m_WrapW;
                importer.SaveAndReimport();textures[item.id]=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            foreach(var item in input.materials)
            {
                Verified(item.source,item.sha256);var shader=Shader.Find(item.shader);if(shader==null)throw new InvalidDataException(item.shader);
                string path=Destination+"/"+item.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
                material.renderQueue=item.renderQueue;material.enableInstancing=item.instancing;material.doubleSidedGI=item.doubleSidedGI;material.globalIlluminationFlags=(MaterialGlobalIlluminationFlags)item.lightmapFlags;
                foreach(var f in item.floats)material.SetFloat(f.name,f.value);foreach(var c in item.colors)material.SetColor(c.name,c.value);
                material.mainTexture=string.IsNullOrEmpty(item.textureId)?null:textures[item.textureId];material.mainTextureScale=item.scale;material.mainTextureOffset=item.offset;EditorUtility.SetDirty(material);paths[item.id]=path;
            }
            foreach(var item in input.meshes)
            {
                Verified(item.source,item.sha256);string path=Destination+"/"+item.name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(mesh==null){mesh=new Mesh{name=item.name};AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                mesh.vertices=item.vertices;if(item.normals.Length>0)mesh.normals=item.normals;if(item.uv0.Length>0)mesh.uv=item.uv0;if(item.uv1.Length>0)mesh.uv2=item.uv1;
                if(item.colors.Length>0)mesh.colors=item.colors;if(item.tangents.Length>0)mesh.tangents=item.tangents;
                mesh.subMeshCount=item.submeshes.Length;for(int i=0;i<item.submeshes.Length;i++)mesh.SetTriangles(item.submeshes[i].indices,i,false);
                mesh.bounds=new Bounds(item.bounds.m_Center,item.bounds.m_Extent*2);EditorUtility.SetDirty(mesh);paths[item.id]=path;
            }
            AssetDatabase.SaveAssets();
            foreach(var item in input.prefabs)
            {
                string yaml=File.ReadAllText(Verified(item.template,item.sha256));
                foreach(var external in input.externalAssets)yaml=yaml.Replace(external.placeholder,AssetDatabase.AssetPathToGUID(paths[external.id]));
                string path=Destination+"/"+item.name+".prefab";File.WriteAllText(path,yaml);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)throw new InvalidDataException("Native gesture prefab import failed: "+path);
                var importer=AssetImporter.GetAtPath(path);importer.userData="Original Unity type-tree native import. Provenance: "+manifestPath+"; template SHA256="+item.sha256;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original PS modules, curves, materials and native meshes preserved; matched original frame acceptance remains pending."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("gesture-native-particles",()=>{
                var arrow=Resources.Load<GameObject>("Recovered/Gestures/hdzd_eff_SphereTrails");var ring=Resources.Load<GameObject>("Recovered/Gestures/LineArrow");
                Require(arrow!=null&&ring!=null,"original prefabs");var systems=arrow.GetComponentsInChildren<ParticleSystem>(true);Require(systems.Length==5,"five original sphere/trail systems");
                foreach(var ps in systems){Require(!ps.main.loop&&ps.trails.enabled&&!ps.shape.enabled,"native enabled modules");Require(ps.GetComponent<ParticleSystemRenderer>().renderMode==ParticleSystemRenderMode.Mesh,"native mesh renderer");}
                var p=systems[0];Require(Mathf.Abs(p.main.startSize.constant-.08f)<1e-6f&&p.main.maxParticles==2&&p.emission.burstCount==1,"original first child parameters");
                Require(Mathf.Abs(p.trails.lifetime.constant-.15f)<1e-6f&&Mathf.Abs(p.trails.minVertexDistance-.01f)<1e-6f,"native trail parameters");
                var r=ring.GetComponentsInChildren<ParticleSystem>(true);Require(r.Length==1&&r[0].main.loop&&Mathf.Abs(r[0].main.simulationSpeed-.2f)<1e-6f,"native endpoint ring");
            });
            check("gesture-source-dependencies",()=>{
                var source=Read();var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Destination+"/Sphere.asset");var m=source.meshes.Single();
                Require(mesh.vertexCount==515&&mesh.vertices.SequenceEqual(m.vertices)&&mesh.triangles.SequenceEqual(m.submeshes[0].indices),"original Sphere coordinates/winding");
                foreach(var mat in source.materials){var material=AssetDatabase.LoadAssetAtPath<Material>(Destination+"/"+mat.name+".mat");Require(material.shader.name==mat.shader&&material.mainTexture!=null,"original shader/texture");Require(material.GetColor("_TintColor")==mat.colors.Single(c=>c.name=="_TintColor").value,"original material tint");}
            });
            check("gesture-native-cut-trail",()=>{
                var tr=Resources.Load<GameObject>("Recovered/Gestures/LineRanderCut").GetComponentInChildren<TrailRenderer>(true);
                Require(Mathf.Abs(tr.time-.4f)<1e-6f&&Mathf.Abs(tr.minVertexDistance-.002f)<1e-6f,"original lifetime/distance");
                Require(tr.widthCurve.length==2&&Mathf.Abs(tr.widthCurve.keys[0].value-.1755829751f)<1e-6f&&Mathf.Abs(tr.widthCurve.keys[1].value-.0307303816f)<1e-6f,"original width curve");
            });
            check("gesture-source-runtime-preview",()=>{
                var host=new GameObject("gesture runtime golden");var target=new GameObject("target");
                try{
                    var v=host.AddComponent<RecoveredGestureVisuals>();target.transform.position=new Vector3(2,0,3);target.transform.localScale=Vector3.one*.4f;
                    v.DrawPreview(Vector3.zero,Vector3.one,Color.blue,true,target.transform);
                    Require(v.RingRoot.transform.position==new Vector3(2,.02f,3),"target endpoint/up offset");var r=v.RingRoot.GetComponentInChildren<ParticleSystemRenderer>();Require(r.transform.localScale==Vector3.one*.5f,"source scale formula");
                    var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);Require(b.GetColor("_TintColor")==RecoveredGestureVisuals.ValidRingTint,"fixed valid ring tint");
                    Require(!v.PreviewLine.Arrow1.enabled&&v.PreviewLine.Background1.enabled&&!v.PreviewLine.CutCollider.enabled,"background-only non-cuttable preview");
                    v.DrawPreview(Vector3.zero,Vector3.one,Color.blue,false,target.transform);Require(v.RingRoot.transform.position==Vector3.one+Vector3.up*.02f&&r.transform.localScale==Vector3.one*1.25f,"invalid ignores target transform");r.GetPropertyBlock(b);Require(b.GetColor("_TintColor")==Color.red,"invalid red");
                    v.MoveCut(new Vector3(1,0,2),true);Require(v.CutRoot.transform.localPosition==new Vector3(1,.02f,2),"cut local offset");v.EndGesture();Require(!v.CutRoot.activeSelf&&!v.RingRoot.activeSelf&&!v.PreviewLine.gameObject.activeSelf,"gesture clear visibility");
                }finally{UnityEngine.Object.DestroyImmediate(host);UnityEngine.Object.DestroyImmediate(target);}
            });
            check("gesture-projectile-clock-and-release",()=>{
                var host=new GameObject("arrow visual golden");try{
                    var v=host.AddComponent<RecoveredGestureVisuals>();var a=new ArrowState{Id=901,Active=true,LaunchCamp=1,Start=Vector3.zero,Position=new Vector3(1,0,0)};
                    v.SynchronizeArrows(new[]{a});var obj=v.ProjectileObjects[901];Require(obj.transform.position==a.Position&&obj.transform.localScale==Vector3.one*.5f,"source position/scale");Require(Quaternion.Angle(obj.transform.rotation,Quaternion.Euler(0,90,0))<.001f,"source euler");
                    Require(obj.GetComponentsInChildren<ParticleSystem>().Length==1&&obj.transform.GetChild(1).gameObject.activeSelf,"only source-camp child active");
                    v.Tick(.05f);var ps=obj.GetComponentInChildren<ParticleSystem>();Require(ps!=null&&ps.isPaused&&Mathf.Abs(ps.time-.05f)<.001f,"one scaled manual clock");
                    a.Active=false;v.SynchronizeArrows(new[]{a});Require(v.ProjectileObjects.Count==0,"completion releases immediately");
                }finally{UnityEngine.Object.DestroyImmediate(host);}
            });
            check("gesture-preview-physics-only-snap",()=>{
                var host=new GameObject("preview collider golden");try{
                    Func<string,Vector3,GameObject> make=(name,position)=>{var go=new GameObject(name);go.transform.SetParent(host.transform);go.transform.position=position;go.layer=8;go.AddComponent<BoxCollider>().size=Vector3.one*.1f;return go;};
                    var source=make("source",new Vector3(500,0,500));var target=make("tower",new Vector3(502,0,500));
                    Physics.SyncTransforms();var result=RecoveredGestureVisuals.ResolvePreview(source,new Vector3(504,0,500),go=>go==source||go==target);
                    Require(result.Valid&&result.Target==target&&result.End==target.transform.position,"empty-ground drag snaps the first tower along segment");
                    var block=make("obstacle",new Vector3(501,0,500));Physics.SyncTransforms();result=RecoveredGestureVisuals.ResolvePreview(source,new Vector3(504,0,500),go=>go==source||go==target);
                    Require(!result.Valid&&result.Target==null,"first non-tower blocks preview");
                    block.transform.position=target.transform.position;Physics.SyncTransforms();
                    Require(RecoveredGestureVisuals.ClosestCandidate(source,new[]{target.GetComponent<Collider>(),block.GetComponent<Collider>()})==block,"equal-distance tie selects later collider");
                    block.SetActive(false);target.SetActive(false);Physics.SyncTransforms();result=RecoveredGestureVisuals.ResolvePreview(source,new Vector3(504,0,500),go=>go==source);
                    Require(result.Valid&&result.Target==null,"unobstructed ground remains valid free ring");
                }finally{UnityEngine.Object.DestroyImmediate(host);Physics.SyncTransforms();}
            });
            check("gesture-production-cached-target-release",()=>{
                var host=new GameObject("production drag input replay");var trace=new DragTrace();
                try{
                    var view=host.AddComponent<BattleView>();view.InitializeScene(5);Require(view.Initialized,"layout5 production initialization");
                    view.Simulation.Event+=e=>trace.events.Add(e.Kind+":line="+e.LineId+":tower="+e.TowerId);
                    bool completed=false;
                    foreach(var source in view.Simulation.Towers.Where(t=>t.Camp==BattleSimulation.PlayerCampID))
                    {
                        foreach(var line in view.Simulation.GetPotentialLines(source.Id))
                        {
                            int target=line.SmallTowerId==source.Id?line.LargeTowerId:line.SmallTowerId;var targetTower=view.Simulation.Tower(target);
                            if(!view.BeginTowerDrag(source.Id))continue;
                            Vector3 point=targetTower.Position+(targetTower.Position-source.Position).normalized*.2f;
                            var resolution=view.MoveTowerDrag(point,0);int cached=view.CachedDragTarget;
                            bool accepted=view.EndTowerDrag();
                            if(!accepted)continue;
                            trace.source=source.Id;trace.target=cached;trace.point=point;trace.previewValid=resolution.Valid;trace.committed=true;
                            trace.inputs=new[]{"BeginTowerDrag(source)","MoveTowerDrag(point beyond target,directHit=0)","EndTowerDrag() cached target submission","BeginTowerDrag(source)","EndTowerDrag() without move rejects stale target"};
                            Require(resolution.Target!=null&&cached!=0&&resolution.Valid,"source-derived snap cached");
                            Require(view.CachedDragTarget==0,"release clears adapter cache");
                            Require(view.BeginTowerDrag(source.Id)&&!view.EndTowerDrag(),"fresh down does not reuse prior target");
                            view.AdvanceFrame(.05f,.05f);completed=true;break;
                        }
                        if(completed)break;
                    }
                    Require(completed,"one natural production input connection");
                    File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/gesture-input-replay.json"),JsonUtility.ToJson(trace,true));
                }finally{UnityEngine.Object.DestroyImmediate(host);Physics.SyncTransforms();}
            });
            return report;
        }
        static void Require(bool value,string detail){if(!value)throw new Exception(detail);}
    }
}
