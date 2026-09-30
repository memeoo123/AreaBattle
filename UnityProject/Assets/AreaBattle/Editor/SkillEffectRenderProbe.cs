using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class SkillEffectRenderProbe
    {
        [Serializable] public sealed class Snapshot {public int skill;public float time;public List<Draw> draws=new List<Draw>();}
        [Serializable] public sealed class Draw {public string path,type,mesh;public int layer,particles;public Vector3 worldPosition,worldScale,boundsCenter,boundsSize,meshSize;public List<Mat> materials=new List<Mat>();}
        [Serializable] public sealed class Mat {public string name,shader;public bool supported,error;public int passes,queue;}
        [Serializable] public sealed class Report {public List<Snapshot> snapshots=new List<Snapshot>();}
        public static void Run()
        {
            RecoveredHudImporter.Import();
            var report=new Report();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Effect geometry probe");var view=root.AddComponent<BattleView>();
            foreach(int skill in new[]{2,9,10,18})
            {
                view.CommanderMode=(skill-1)/3+1;view.InitializeScene(5,30);view.Simulation.Connect(3,1);
                for(int frame=0;frame<90;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                if(!view.TryUseSkillSlot((skill-1)%3,skill==9?3:0,new Vector3(0,0,2.5f)))throw new Exception("Probe input failed");
                for(int stage=0;stage<2;stage++)
                {
                    for(int frame=0;frame<(stage==0?12:60);frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"probe-skill"+skill+"-"+stage+".png");
                    var snapshot=new Snapshot{skill=skill,time=stage==0?.2f:1.2f};
                    foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        if(!PathOf(renderer.transform).Contains("Skill "))continue;
                        var row=new Draw{path=PathOf(renderer.transform),type=renderer.GetType().Name,layer=renderer.gameObject.layer,worldPosition=renderer.transform.position,worldScale=renderer.transform.lossyScale,boundsCenter=renderer.bounds.center,boundsSize=renderer.bounds.size};
                        var filter=renderer.GetComponent<MeshFilter>();var mesh=filter!=null?filter.sharedMesh:null;if(renderer is ParticleSystemRenderer pr)mesh=pr.mesh;
                        if(mesh!=null){row.mesh=mesh.name;row.meshSize=mesh.bounds.size;}
                        var ps=renderer.GetComponent<ParticleSystem>();if(ps!=null)row.particles=ps.particleCount;
                        foreach(var mat in renderer.sharedMaterials)if(mat!=null)
                        {
                            if(mat.shader==null||!mat.shader.isSupported||ShaderUtil.ShaderHasError(mat.shader)||mat.shader.name=="Hidden/InternalErrorShader")throw new InvalidOperationException("Render-time invalid skill shader "+mat.name);
                            row.materials.Add(new Mat{name=mat.name,shader=mat.shader.name,supported=mat.shader.isSupported,error=ShaderUtil.ShaderHasError(mat.shader),passes=mat.passCount,queue=mat.renderQueue});
                        }
                        snapshot.draws.Add(row);
                    }
                    report.snapshots.Add(snapshot);
                }
            }
            string workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;
            File.WriteAllText(Path.Combine(workspace,"analysis/skill-effect-render-probe.json"),JsonUtility.ToJson(report,true));
            UnityEngine.Object.DestroyImmediate(root);Debug.Log("SKILL_EFFECT_RENDER_PROBE_PASS");
        }
        static string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
    }
}
