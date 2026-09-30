using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    // Read-only presentation experiment; no gameplay fixture changes.
    public static class PresentationCaptureAudit
    {
        [Serializable] sealed class Sample
        {
            public string image;
            public int width,height;
            public float canvasScale;
            public Vector2 displaySize;
            public List<string> textMetrics=new List<string>();
        }
        [Serializable] sealed class Report { public List<Sample> samples=new List<Sample>(); }
        public static void Run()
        {
            GameObject root=null;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                root=new GameObject("Presentation capture audit");
                var view=root.AddComponent<BattleView>();view.RandomSourceOverride=new System.Random(4305);
                view.InitializeNormalLevel(871);
                if(!view.Initialized)throw new Exception("Scene initialization failed");
                Canvas.ForceUpdateCanvases();view.Simulation.AIEnabled=false;view.AdvanceFrame(3.2f,3.2f);
                var report=new Report();
                foreach(int width in new[]{720,723})
                {
                    int height=width==720?1280:1282;
                    var rt=new RenderTexture(width,height,24);
                    view.BattleCamera.targetTexture=rt;view.Hud.UICamera.targetTexture=rt;
                    view.BattleCamera.aspect=width/(float)height;
                    view.BattleCamera.orthographicSize=2.1f*.5625f/view.BattleCamera.aspect;
                    view.SynchronizeBackground(view.BattleCamera.aspect);
                    for(int refresh=0;refresh<3;refresh++)
                    {
                        if(refresh==1)view.Hud.Canvas.GetComponent<CanvasScaler>().SendMessage("Handle");
                        if(refresh==2)foreach(var text in view.Hud.Canvas.GetComponentsInChildren<Text>())text.SetAllDirty();
                        Canvas.ForceUpdateCanvases();view.Hud.Synchronize();
                        view.BattleCamera.Render();view.Hud.UICamera.Render();
                        var sample=new Sample{image="capture-"+width+"-"+refresh+".png",width=width,height=height,
                            canvasScale=view.Hud.Canvas.scaleFactor,displaySize=view.Hud.Canvas.renderingDisplaySize};
                        foreach(var text in view.Hud.Canvas.GetComponentsInChildren<Text>())
                            if(text.text.Contains("871")||text.text.Contains("道具"))sample.textMetrics.Add(text.name+"="+text.text+" fontSize="+text.fontSize+" generated="+text.cachedTextGenerator.fontSizeUsedForBestFit+" ppu="+text.pixelsPerUnit);
                        var prior=RenderTexture.active;RenderTexture.active=rt;
                        var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                        image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                        string dir=Path.Combine(BattleBuild.Target,"generated/video-20260928/capture-audit");Directory.CreateDirectory(dir);
                        File.WriteAllBytes(Path.Combine(dir,sample.image),image.EncodeToPNG());
                        RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(image);report.samples.Add(sample);
                    }
                    view.BattleCamera.targetTexture=null;view.Hud.UICamera.targetTexture=null;
                    UnityEngine.Object.DestroyImmediate(rt);
                }
                File.WriteAllText(Path.Combine(BattleBuild.Target,"generated/video-20260928/capture-audit/report.json"),JsonUtility.ToJson(report,true));
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}

