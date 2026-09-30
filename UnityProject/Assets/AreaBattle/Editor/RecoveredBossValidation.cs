using System;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class RecoveredBossValidation
    {
        public static void RunBatch()
        {
            var r=Run();System.IO.File.WriteAllText(System.IO.Path.GetFullPath("../analysis/unity-boss-validation.json"),JsonUtility.ToJson(r,true));
            if(!r.passed)throw new InvalidOperationException("Original Boss validation failed; see analysis/unity-boss-validation.json");
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original Boss prefab and Spine data checks; original matched-frame visual acceptance remains pending."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("boss-four-original-spine-models",()=>{
                foreach(int id in new[]{801,802,9001,9002}) {
                    var p=Resources.Load<GameObject>("Recovered/Bosses/"+RecoveredBossVisual.ResourceForEntity(id));Require(p!=null,"prefab "+id);
                    var instance=RecoveredBossVisual.Create(id);try {var main=instance.MainSkeleton;var data=main.skeletonDataAsset.GetSkeletonData(false);
                    Require(data.Version=="4.1.16"&&Mathf.Abs(main.skeletonDataAsset.scale-.01f)<1e-7f,"original version/scale");
                    Require(main.skeletonDataAsset.atlasAssets.Length>0&&main.skeletonDataAsset.atlasAssets.All(a=>a!=null),"persistent original atlas references");
                    Require(main.GetComponent<MeshRenderer>().sharedMaterials.All(m=>m!=null&&m.mainTexture!=null&&m.shader.name!="Spine/Special/HiddenPass"),"visible source atlas material, no regionless fallback");
                    Require(data.Bones.Count==(id==801||id==9001?94:60)&&data.Slots.Count==(id==801||id==9001?25:17),"original skeleton topology");
                    Require(data.IkConstraints.Count==2&&data.PathConstraints.Count==(id==801||id==9001?2:0),"original constraints");
                    foreach(string a in new[]{"idle","run","attack","dead","victory"})Require(data.FindAnimation(a)!=null,"original animation "+a);} finally {UnityEngine.Object.DestroyImmediate(instance.gameObject);}
                }
            });
            check("boss-source-colliders-and-mesh-transform",()=>{
                foreach(int id in new[]{801,802}) {
                    var p=Resources.Load<GameObject>("Recovered/Bosses/"+RecoveredBossVisual.ResourceForEntity(id));var c=p.GetComponent<BoxCollider>();
                    Require(c!=null&&c.enabled&&!c.isTrigger&&Near(c.size,new Vector3(.28f,.3f,.2f))&&Near(c.center,new Vector3(0,.07f,0)),"source BoxCollider");
                    var mesh=p.transform.Find("mesh");Require(Near(mesh.localPosition,new Vector3(0,0,-.014f))&&Near(mesh.localScale,Vector3.one*.05f),"source mesh local transform");
                    Require(Quaternion.Angle(mesh.localRotation,new Quaternion(.30070576f,0,0,.953717f))<.001f,"source mesh rotation");
                }
                foreach(int id in new[]{9001,9002})Require(Resources.Load<GameObject>("Recovered/Bosses/"+RecoveredBossVisual.ResourceForEntity(id)).GetComponentsInChildren<Collider>(true).Length==0,"source soldier has no collider");
            });
            check("boss-spine-manual-clock-and-deformation",()=>{
                foreach(int id in new[]{801,802,9001,9002}) {
                    var v=RecoveredBossVisual.Create(id);
                    try{Require(v.Skeletons.All(s=>!s.enabled),"automatic clock disabled");v.Play("attack",false);var mf=v.MainSkeleton.GetComponent<MeshFilter>();var first=mf.sharedMesh.vertices.ToArray();v.Step(.25f);var second=mf.sharedMesh.vertices;Require(Mathf.Abs(v.MainSkeleton.AnimationState.GetCurrent(0).TrackTime-.25f)<1e-5f,"one source scaled tick");Require(first.Length==second.Length&&first.Where((p,i)=>Vector3.Distance(p,second[i])>1e-5f).Any(),"real skeletal mesh changed");v.Step(0);Require(Mathf.Abs(v.MainSkeleton.AnimationState.GetCurrent(0).TrackTime-.25f)<1e-5f,"zero scaled time");}
                    finally{UnityEngine.Object.DestroyImmediate(v.gameObject);}
                }
            });
            check("boss-soldier-source-scale-and-death",()=>{
                var cameraObject=new GameObject("Boss source camera");var cam=cameraObject.AddComponent<Camera>();cam.transform.eulerAngles=new Vector3(35,0,0);var v=RecoveredBossVisual.Create(9002);
                try{v.SynchronizeSoldier(new SoldierState{Position=new Vector3(2,0,3),ShipType=12},cam);Require(Near(v.transform.localScale,Vector3.one*.016f)&&Near(v.transform.position,new Vector3(2,0,3)),"source dynamic scale/position");Require(Quaternion.Angle(v.transform.rotation,cam.transform.rotation)<.001f,"camera tilt without regular reverse");v.BeginSoldierDeath(new Vector3(.05f,0,-.05f));Require(v.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="dead"&&!v.MainSkeleton.AnimationState.GetCurrent(0).Loop,"source death animation");v.Step(10);Require(v.SoldierDeathFinished&&Near(v.transform.position,new Vector3(2.05f,0,2.95f)),"death drift completion");}
                finally{UnityEngine.Object.DestroyImmediate(v.gameObject);UnityEngine.Object.DestroyImmediate(cameraObject);}
            });
            check("boss-cloud-original-spine",()=>{
                var p=RecoveredBossVisual.Create(802);try {var cloud=p.GetComponentsInChildren<SkeletonAnimation>(true).Single(s=>s.transform.name=="cloud");var data=cloud.skeletonDataAsset.GetSkeletonData(false);Require(data.Version=="4.1.16"&&data.Bones.Count==3&&data.Slots.Count==2&&data.FindAnimation("animation")!=null,"original rain cloud skeleton");} finally {UnityEngine.Object.DestroyImmediate(p.gameObject);}
            });
            check("boss-production-view-model-collider-clock",()=>{
                foreach(int layout in new[]{99998,99999}){
                    var host=new GameObject("Boss production validation");var view=host.AddComponent<BattleView>();
                    try{view.InitializeSpecialScene(layout);Require(view.Initialized,"special scene initialized");var boss=view.Simulation.Towers.Single(t=>t.IsBoss);var root=view.TowerPresentationTransform(boss.Id);var visual=root.GetComponent<RecoveredBossVisual>();Require(visual!=null&&root.GetComponents<Collider>().Length==1,"original model owns source collider, no debug duplicate");Require(Near(root.position,boss.Position)&&Near(root.localScale,Vector3.one),"source world transform");
                        view.Simulation.ExecuteBossAction(boss.Id,boss.Camp==5?1:6);Require(visual.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="attack","production action starts attack");view.AdvanceFrame(.5f,.5f);Require(visual.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="attack","wait before source callback");view.AdvanceFrame(.5f,.5f);Require(visual.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="idle","source 1s callback returns idle");
                        if(boss.Camp==6){view.Simulation.ExecuteBossAction(boss.Id,5);view.AdvanceFrame(.05f,.05f);Require(view.Simulation.TryGetBossRain(boss.Id,out var point,out bool raining)&&raining,"source chosen rain target");var rain=root.Find("hdzd_effect_boss05_01_1");Require(rain.gameObject.activeSelf&&Near(rain.position,point),"same core target without resampling");Require(Mathf.Abs(rain.Find("Rain").localPosition.y-(1.2f-.696f*rain.localPosition.z))<1e-5f,"source Rain localY correction");}
                        view.RestartCurrentLevel();Require(visual.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="idle","source retry reinitializes idle");var rainAfter=root.Find("hdzd_effect_boss05_01_1");Require(rainAfter==null||!rainAfter.gameObject.activeSelf,"retry clears rain");view.AdvanceFrame(.2f,.2f);Require(visual.MainSkeleton.AnimationState.GetCurrent(0).Animation.Name=="idle","cancelled old coroutine cannot restore attack");
                    }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
                }
            });
            return report;
        }
        static bool Near(Vector3 a,Vector3 b)=>Vector3.Distance(a,b)<.000001f;
        static void Require(bool yes,string message){if(!yes)throw new InvalidOperationException(message);}
    }
}

