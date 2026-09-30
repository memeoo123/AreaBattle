using System;
using System.IO;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class ObstacleValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{unityVersion=Application.unityVersion,passed=true,
                limitations="Source collider geometry tested; original visual replay still pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("all-639-layout-obstacle-import",()=>{
                int levels=0,obstacles=0;
                foreach(var text in Resources.LoadAll<TextAsset>("Data/Levels"))
                {
                    var layout=JsonUtility.FromJson<LevelLayout>(text.text);var root=new GameObject("geometry validation");
                    try{
                        var result=BattleObstacles.Create(layout,root.transform);levels++;obstacles+=result.Count;
                        if(result.Count!=(layout.ObstacleInfoCfgs?.Length??0))throw new Exception(text.name+" incomplete obstacles");
                        foreach(var obj in result)if(obj.GetComponentsInChildren<BoxCollider>(true).Length==0)throw new Exception(text.name+" missing source collider");
                    }finally{UnityEngine.Object.DestroyImmediate(root);}
                }
                if(levels!=639 || obstacles!=2862)throw new Exception("Traversal counts: "+levels+" levels, "+obstacles+" obstacles");
            });
            check("rotated-obstacle-capsule-block-and-clear",()=>{
                var root=new GameObject("physics fixture");
                try{
                    var layout=new LevelLayout{ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=81,
                        pos=new IntVector3{x=1000,y=0,z=1000},angle=new IntVector3{y=9000},scale=new IntVector3{x=100,y=100,z=100}}}};
                    BattleObstacles.Create(layout,root.transform);Physics.SyncTransforms();
                    var center=new Vector3(10,0,10);
                    if(Physics.OverlapCapsule(center+Vector3.forward,center-Vector3.forward,.03f,1<<8).Length==0)
                        throw new Exception("Cross-wall source query must block");
                    if(Physics.OverlapCapsule(center+Vector3.right+Vector3.forward,center+Vector3.right-Vector3.forward,.03f,1<<8).Length!=0)
                        throw new Exception("Clear path beside wall must remain open");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/unity-obstacle-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
    }
}
