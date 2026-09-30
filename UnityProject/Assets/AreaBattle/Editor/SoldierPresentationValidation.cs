using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class SoldierPresentationValidation
    {
        [Serializable] sealed class Report {public bool passed;public int changedPixels,visiblePixels;public string scope="Recovered default soldier GPU animation renders and advances. No original-game image comparison asserted.";}
        public static void CaptureAndValidate()
        {
            var root=new GameObject("Recovered soldier render check");var report=new Report();
            try{
                RecoveredSoldierImporter.Import();
                var shader=Shader.Find("AreaBattle/Recovered SkeletonMeshBaker");
                foreach(var msg in ShaderUtil.GetShaderMessages(shader))
                    if(msg.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)throw new Exception(msg.message);
                var camera=root.AddComponent<Camera>();camera.transform.position=new Vector3(0,1.2f,-1.5f);
                camera.transform.LookAt(new Vector3(0,0,.35f));camera.orthographic=true;camera.orthographicSize=.75f;camera.aspect=1;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.86f,.89f,.92f);camera.cullingMask=1<<9;
                var visuals=new List<RecoveredSoldierVisual>();var states=new List<SoldierState>();
                for(int camp=1;camp<=3;camp++)for(int type=1;type<=3;type++)
                {
                    var prefab=Resources.Load<GameObject>("Recovered/Soldiers/soldier_"+(type*100));
                    if(prefab==null)throw new Exception("Missing recovered prefab "+type);
                    var obj=UnityEngine.Object.Instantiate(prefab,root.transform);obj.layer=9;
                    var state=new SoldierState{Camp=camp,ShipType=type,Position=new Vector3((type-2)*.42f,0,(camp-1)*.38f),LegStart=Vector3.zero,LegEnd=camp==2?Vector3.right:Vector3.left};
                    var visual=obj.GetComponent<RecoveredSoldierVisual>();visual.Begin(0);visual.Synchronize(state,camera,0);visuals.Add(visual);states.Add(state);
                }
                var first=Capture(camera,"soldiers-original-frame0.png");
                for(int i=0;i<visuals.Count;i++)visuals[i].Synchronize(states[i],camera,.2f);
                var second=Capture(camera,"soldiers-original-frame02.png");
                var bg=first[0];
                for(int i=0;i<first.Length;i++){
                    if(!first[i].Equals(second[i]))report.changedPixels++;
                    if(!first[i].Equals(bg))report.visiblePixels++;
                }
                if(report.changedPixels<100||report.visiblePixels<1000)throw new Exception("GPU animation did not render/advance: "+report.visiblePixels+" visible, "+report.changedPixels+" changed");
                report.passed=true;
                File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/soldier-render-validation.json"),JsonUtility.ToJson(report,true));
                Debug.Log("AREABATTLE_SOLDIER_RENDER_PASS "+report.changedPixels);
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static Color32[] Capture(Camera camera,string file)
        {
            var rt=new RenderTexture(600,600,24);var texture=new Texture2D(600,600,TextureFormat.RGBA32,false);
            try{
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,600,600),0,0);texture.Apply();
                string dir=Path.Combine(BattleBuild.Workspace,"analysis/captures");Directory.CreateDirectory(dir);File.WriteAllBytes(Path.Combine(dir,file),texture.EncodeToPNG());
                return texture.GetPixels32();
            }finally{camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
        }
    }
}
