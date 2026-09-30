using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class BossProjectileValidation
    {
        public static void ImportAndProbe(){RecoveredBossEmbeddedImporter.Import();Probe();}
        public static void Probe()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
            var host=new GameObject("Boss fire GPU probe");var view=host.AddComponent<BattleView>();view.InitializeSpecialScene(99999);
            var boss=view.Simulation.Towers.Single(t=>t.IsBoss);view.Simulation.ExecuteBossAction(boss.Id,2);
            for(int i=0;i<81;i++)view.AdvanceFrame(1f/60f,1f/60f);
            foreach(var renderer in view.TowerPresentationTransform(boss.Id).Find("bulletRoot").GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)Debug.Log("BOSS_FIRE_MATERIAL path="+renderer.transform.name+" enabled="+renderer.enabled+" active="+renderer.gameObject.activeInHierarchy+" mat="+(material==null?"NULL":material.name)+" shader="+(material==null?"NULL":material.shader.name)+" supported="+(material!=null&&material.shader.isSupported)+" asset="+UnityEditor.AssetDatabase.GetAssetPath(material));
            BattleBuild.Capture(view,"boss-action2-fire-gpu.png");
            view.Simulation.CastSkill(1,1);
            for(int i=0;i<=600;i++)
            {
                view.AdvanceFrame(i==0?0:1f/60f,i==0?0:1f/60f);
                if(i!=0&&i!=15&&i!=30&&i!=60&&i!=120&&i!=360&&i!=600)continue;
                var ice=view.TowerPresentationTransform(boss.Id).Find("skill_TM");var skin=ice.GetComponentInChildren<SkinnedMeshRenderer>(true);var a=ice.GetComponent<Animator>();var mesh=new Mesh();skin.BakeMesh(mesh);
                Debug.Log("BOSS_ICE_GPU time="+(i/60f)+" active="+ice.gameObject.activeInHierarchy+" enabled="+skin.enabled+" bounds="+skin.bounds+" rootBone="+skin.rootBone.position+" rootScale="+skin.rootBone.lossyScale+" meshBounds="+mesh.bounds+" meshV0="+mesh.vertices[0]+" shader="+skin.sharedMaterial.shader.name+" supported="+skin.sharedMaterial.shader.isSupported+" animState="+a.GetCurrentAnimatorStateInfo(0).fullPathHash+" normalized="+a.GetCurrentAnimatorStateInfo(0).normalizedTime+" parameter="+a.GetInteger("skill"));
                UnityEngine.Object.DestroyImmediate(mesh);BattleBuild.Capture(view,"boss-source-ice-"+i+"frames.png");
                if(i==120)
                {
                    Debug.Log("BOSS_ICE_DETAIL world="+skin.transform.position+" scale="+skin.transform.lossyScale+" rotation="+skin.transform.rotation+" bone0="+skin.bones[0].localToWorldMatrix+" bind0="+skin.sharedMesh.bindposes[0]+" material="+skin.sharedMaterial.name+" tex="+skin.sharedMaterial.GetTexture("_TextureSample0")+" triangles="+skin.sharedMesh.triangles.Length);
                    var c=view.BattleCamera;var p=c.transform.position;float size=c.orthographicSize;
                    c.transform.position+=boss.Position-new Vector3(0,0,2.5f);c.orthographicSize=.6f;
                    BattleBuild.Capture(view,"boss-source-ice-closeup.png");c.transform.position=p;c.orthographicSize=size;
                    IsolatedIce(skin,false);IsolatedIce(skin,true);
                }
            }
            UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;
            var report=Run();System.IO.File.WriteAllText(System.IO.Path.GetFullPath("../analysis/boss-projectile-validation.json"),JsonUtility.ToJson(report,true));
            if(Application.isBatchMode)UnityEditor.EditorApplication.Exit(report.passed?0:1);
        }
        static void IsolatedIce(SkinnedMeshRenderer skin,bool baked)
        {
            var cameraObject=new GameObject("Isolated source ice camera");var camera=cameraObject.AddComponent<Camera>();
            camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;camera.orthographic=true;camera.orthographicSize=.3f;
            var mesh=new Mesh();skin.BakeMesh(mesh);var obj=new GameObject("Source baked diagnostic");obj.layer=30;
            obj.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);obj.transform.localScale=skin.transform.lossyScale;
            var mf=obj.AddComponent<MeshFilter>();mf.sharedMesh=mesh;var mr=obj.AddComponent<MeshRenderer>();mr.sharedMaterials=skin.sharedMaterials;mr.enabled=baked;
            int layer=skin.gameObject.layer;skin.gameObject.layer=30;bool enabled=skin.enabled;skin.enabled=!baked;
            var center=skin.transform.TransformPoint(mesh.bounds.center);camera.transform.position=center+new Vector3(0,1,-2);camera.transform.LookAt(center);
            var rt=new RenderTexture(512,512,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(512,512,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();
            System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath("../analysis/captures/boss-ice-isolated-"+(baked?"baked":"native")+".png"),image.EncodeToPNG());
            skin.gameObject.layer=layer;skin.enabled=enabled;RenderTexture.active=null;camera.targetTexture=null;
            foreach(var o in new UnityEngine.Object[]{image,rt,obj,mesh,cameraObject})UnityEngine.Object.DestroyImmediate(o);
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("boss-fire-original-clone-pool",()=>{
                var boss=RecoveredBossVisual.Create(801);
                try{
                    var source=boss.transform.Find("Hdzd_Effect_Wy_Fire");var pool=RecoveredBossProjectilePool.For(boss.transform);
                    var fire=pool.Acquire(new Vector3(1,0,2));var clock=fire.GetComponent<RecoveredEffectVisual>();
                    Require(fire.transform.parent.name=="bulletRoot"&&Quaternion.Angle(fire.transform.rotation,Quaternion.Euler(0,90,0))<.001f,"source parent/initial rotation");
                    Require(fire.GetComponentsInChildren<ParticleSystem>(true).Length==source.GetComponentsInChildren<ParticleSystem>(true).Length,"original entire fire subtree");
                    Require(Vector3.Distance(fire.transform.localScale,source.localScale)<1e-6f,"source scale retained");
                    clock.Step(.2f);Require(fire.GetComponentsInChildren<ParticleSystem>(true).Any(p=>p.time>.1f),"production particle clock");
                    fire.transform.Rotate(new Vector3(7,11,13));var rotation=fire.transform.rotation;pool.Release(fire);
                    Require(pool.AvailableCount==1&&!fire.activeSelf,"callback hides/enqueues");var reused=pool.Acquire(Vector3.zero);
                    Require(reused==fire&&pool.CreatedCount==1&&Quaternion.Angle(rotation,reused.transform.rotation)<.001f,"pool retains rotation");pool.ClearSourcePool();Require(pool.CreatedCount==0&&pool.transform.childCount==0,"source clear destroys bullets");
                }finally{UnityEngine.Object.DestroyImmediate(boss.gameObject);}
            });
            check("boss-action2-production-flight-hit-retry",()=>{
                var host=new GameObject("Boss action2 production probe");var view=host.AddComponent<BattleView>();
                try{
                    view.InitializeSpecialScene(99999);var w=view.Simulation;var boss=w.Towers.Single(t=>t.IsBoss);var events=new List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                    w.ExecuteBossAction(boss.Id,2);for(int i=0;i<21;i++)view.AdvanceFrame(.05f,.05f);
                    var shot=w.SkillProjectiles.First(p=>p.SkillId==102);var root=view.TowerPresentationTransform(boss.Id).Find("bulletRoot");
                    var fire=root.Cast<Transform>().First(t=>t.gameObject.activeSelf);
                    Require(Vector3.Distance(fire.position,w.SkillProjectileVisualPosition(shot))<1e-5f,"visual follows same simulation projectile");
                    // Original Fire root PSRenderer is disabled and has one null material
                    // (source CAB-a5cbc...:-7556168621690325670). Its four children render.
                    var renderers=fire.GetComponentsInChildren<Renderer>(true);
                    var sourceRootRenderer=fire.GetComponent<ParticleSystemRenderer>();
                    Require(renderers.Length==5&&sourceRootRenderer!=null&&!sourceRootRenderer.enabled&&sourceRootRenderer.sharedMaterials.Length==1&&sourceRootRenderer.sharedMaterial==null,"original disabled root renderer/null slot retained");
                    Require(renderers.Where(r=>r!=sourceRootRenderer).Count()==4&&renderers.Where(r=>r!=sourceRootRenderer).All(r=>r.enabled&&r.sharedMaterials.Length==1&&r.sharedMaterials.All(m=>m!=null&&m.shader.isSupported)),"all four source fire child materials supported");
                    for(int i=0;i<200&&!events.Any(e=>e.SkillId==102&&e.Kind=="effect-show");i++)view.AdvanceFrame(.05f,.05f);
                    var hit=events.First(e=>e.SkillId==102&&e.Kind=="effect-show");Require(hit.EffectId==404||hit.EffectId==408,"original effect callback");
                    Require(host.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Skill 102 / "+(hit.EffectId==404?"Hdzd_Effect_Wy_Restore":"Hdzd_Effect_Wy_Blast")),"original hit effect instantiated");
                    view.RestartCurrentLevel();view.AdvanceFrame(0,0);Require(root.childCount==0,"retry destroys all pooled and active fire objects");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("boss-source-skill-tm-ice-and-marker-bindings",()=>{
                foreach(string name in new[]{"skill_TM","hero_sanjiao_red","hero_sanjiao_blue"})Require(Resources.Load<GameObject>("Recovered/BossEmbedded/"+name)!=null,"source subtree "+name);
                var host=new GameObject("Boss original ice probe");var view=host.AddComponent<BattleView>();
                try{view.InitializeSpecialScene(99999);var boss=view.Simulation.Towers.Single(t=>t.IsBoss);Require(view.Simulation.CastSkill(1,1),"ice cast");view.AdvanceFrame(.1f,.1f);
                    var ice=view.TowerPresentationTransform(boss.Id).Find("skill_TM");Require(ice!=null&&ice.gameObject.activeSelf,"Boss uses skill_TM");
                    var skin=ice.GetComponentInChildren<SkinnedMeshRenderer>(true);Require(skin!=null&&skin.bones.Length==56&&skin.sharedMesh.bindposes.Length==56,"original skinned ice topology");
                    var bind=skin.sharedMesh.bindposes[0];var weight=skin.sharedMesh.boneWeights[0];
                    Require(Mathf.Abs(bind.m00-(-.580571711f))<1e-7f&&Mathf.Abs(bind.m13-(-.101418495f))<1e-7f&&bind.m33==1,"original bind-pose source floats survive serialization");
                    Require(weight.boneIndex0==1&&weight.boneIndex1==2&&Mathf.Abs(weight.weight0-.782352686f)<1e-7f,"original vertex0 weights survive serialization");
                    view.AdvanceFrame(.5f,.5f);var baked=new Mesh();skin.BakeMesh(baked);
                    try{Require(baked.vertices.Length>0&&baked.vertices.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z)&&!float.IsInfinity(v.x)&&!float.IsInfinity(v.y)&&!float.IsInfinity(v.z)),"real skinned mesh has finite vertices");}finally{UnityEngine.Object.DestroyImmediate(baked);}
                    var animator=ice.GetComponent<Animator>();Require(animator!=null&&!animator.enabled&&animator.GetInteger("skill")==0,"same source manual Animator contract");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("source-bat-skin-matrix-weight-roundtrip",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/SkillEffects/hdzd_eff_zhg04_01");Require(prefab!=null,"original skill7 bat prefab");
                var obj=UnityEngine.Object.Instantiate(prefab);
                try{
                    obj.SetActive(true);var clock=RecoveredEffectVisual.Attach(obj);clock.Step(0);clock.Step(.25f);
                    var skin=obj.GetComponentInChildren<SkinnedMeshRenderer>(true);Require(skin!=null&&skin.sharedMesh.bindposes.Length==10&&skin.sharedMesh.boneWeights.Length==244,"original bat skin counts");
                    var bind=skin.sharedMesh.bindposes[0];var weight=skin.sharedMesh.boneWeights[0];
                    Require(bind.m00==1&&bind.m33==1&&Mathf.Abs(bind.m13-.0119785331f)<1e-8f&&weight.boneIndex0==1&&weight.weight0==1,"original bat matrix/weight values");
                    var mesh=new Mesh();skin.BakeMesh(mesh);try{Require(mesh.vertices.Length==244&&mesh.vertices.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z)&&!float.IsInfinity(v.x)&&!float.IsInfinity(v.y)&&!float.IsInfinity(v.z)),"real bat skin vertices finite");}finally{UnityEngine.Object.DestroyImmediate(mesh);}
                }finally{UnityEngine.Object.DestroyImmediate(obj);}
            });
            return report;
        }
        static void Require(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    }
}
