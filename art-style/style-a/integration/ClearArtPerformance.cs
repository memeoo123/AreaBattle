using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class ClearArtPerformance
    {
        [Serializable] public class Result { public string scope="Editor sample, 200 ordinary soldiers, 540x960, warmed median of 20 frames. CPU synchronization and synchronous Camera.Render only; not mobile GPU/frame-time certification."; public double originalSyncMs,newSyncMs,originalRenderMs,newRenderMs; public int count=200; }
        public static void Run()
        {
            var result=new Result();Measure(false,out result.originalSyncMs,out result.originalRenderMs);Measure(true,out result.newSyncMs,out result.newRenderMs);
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"art-style/style-a/performance.json"),JsonUtility.ToJson(result,true));
        }
        static void Measure(bool clear,out double sync,out double render)
        {
            var root=new GameObject("Art performance fixture");var cameraObject=new GameObject("Performance camera");var rt=new RenderTexture(540,960,24);
            try{
                var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=2.1f;camera.aspect=.5625f;camera.transform.position=new Vector3(0,3,-3);camera.transform.LookAt(Vector3.zero);camera.targetTexture=rt;
                var states=new SoldierState[200];var oldUnits=new RecoveredSoldierVisual[200];var newUnits=new ClearSoldierVisual[200];
                var prefab=Resources.Load<GameObject>("Recovered/Soldiers/soldier_100");
                for(int i=0;i<200;i++){
                    states[i]=new SoldierState{ShipType=1,Camp=i%2+1,LegStart=Vector3.zero,LegEnd=Vector3.right,Position=new Vector3((i%10-4.5f)*.18f,0,(i/10-9.5f)*.12f)};
                    var go=clear?new GameObject("Sample"):UnityEngine.Object.Instantiate(prefab);go.transform.SetParent(root.transform,false);
                    if(clear){newUnits[i]=go.AddComponent<ClearSoldierVisual>();newUnits[i].Begin(0);}else{oldUnits[i]=go.GetComponent<RecoveredSoldierVisual>();oldUnits[i].Begin(0);}
                }
                var syncTimes=new double[20];var renderTimes=new double[20];var timer=new System.Diagnostics.Stopwatch();
                for(int f=0;f<25;f++){
                    timer.Restart();for(int i=0;i<200;i++){if(clear)newUnits[i].Synchronize(states[i],camera,f/60f);else oldUnits[i].Synchronize(states[i],camera,f/60f);}timer.Stop();if(f>=5)syncTimes[f-5]=timer.Elapsed.TotalMilliseconds;
                    timer.Restart();camera.Render();timer.Stop();if(f>=5)renderTimes[f-5]=timer.Elapsed.TotalMilliseconds;
                }
                syncTimes=syncTimes.OrderBy(t=>t).ToArray();renderTimes=renderTimes.OrderBy(t=>t).ToArray();sync=(syncTimes[9]+syncTimes[10])/2;render=(renderTimes[9]+renderTimes[10])/2;
            }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(rt);}
        }
    }
}
