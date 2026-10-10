using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    // Unsaved visual fixture; no gameplay resource or profile writes.
    public static class BuildingArtPreview
    {
        static string Output=>Path.Combine(BattleBuild.Workspace,"art-style/towers-first");
        static float calibrationWidth;
        public static void RunBatch()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!tex.LoadImage(File.ReadAllBytes(Path.Combine(Output,"compact-towers.png"))))throw new Exception("Image decode failed");
                tex.filterMode=FilterMode.Bilinear;
                int[] cuts={0,tex.width/4,tex.width/2,tex.width*3/4,tex.width};var sprites=new Sprite[4];
                for(int i=0;i<4;i++)
                {
                    int minX=cuts[i+1],maxX=0,minY=tex.height,maxY=0;
                    for(int y=0;y<tex.height;y++)for(int x=cuts[i];x<cuts[i+1];x++)if(tex.GetPixel(x,y).a>.125f)
                    {minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                    sprites[i]=Sprite.Create(tex,new Rect(minX,minY,maxX-minX+1,maxY-minY+1),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
                }
                var report=new List<string>{"Unity temporary visual fixture, all preview towers blue; faction recoloring not implemented.","Types cycle basic/assault/split/arrow for silhouette testing, not simulation specialization.","Common pixel scale calibrated to median original tower width. Existing obstacle art retained."};
                calibrationWidth=sprites[0].bounds.size.x;
                var plain=new Texture2D(2,2,TextureFormat.RGBA32,false);plain.LoadImage(File.ReadAllBytes(Path.Combine(Output,"basic-plain.png")));plain.filterMode=FilterMode.Bilinear;
                int bx=plain.width,by=plain.height,ex=0,ey=0;
                for(int y=0;y<plain.height;y++)for(int x=0;x<plain.width;x++)if(plain.GetPixel(x,y).a>.125f){bx=Math.Min(bx,x);by=Math.Min(by,y);ex=Math.Max(ex,x);ey=Math.Max(ey,y);}
                UnityEngine.Object.DestroyImmediate(sprites[0]);sprites[0]=Sprite.Create(plain,new Rect(bx,by,ex-bx+1,ey-by+1),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
                Render(30,true,sprites,report);Render(104,false,sprites,report);
                File.WriteAllLines(Path.Combine(Output,"plain-basic-measurements.txt"),report);
                foreach(var s in sprites)UnityEngine.Object.DestroyImmediate(s);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(plain);
                EditorApplication.Exit(0);
            }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        static void Render(int level,bool campaign,Sprite[] sprites,List<string> report)
        {
            var host=new GameObject("Building preview fixture");
            try
            {
                var v=host.AddComponent<BattleView>();v.EvolutionCampaign=campaign;
                if(campaign)v.InitializeNormalLevel(level);else v.InitializeScene(level);
                if(!v.Initialized)throw new Exception("View initialization failed");
                v.Simulation.AIEnabled=false;v.RefreshPresentation();
                var camera=v.BattleCamera;camera.aspect=.5625f;camera.orthographicSize=2.1f;
                string prefix=campaign?"campaign30":"layout104";Capture(v,prefix+"-before.png");
                var towers=v.Simulation.Towers.Where(t=>!t.IsBoss).ToArray();
                var widths=towers.Select(t=>{var r=v.TowerPresentationTransform(t.Id).Find("Tower Art").GetComponent<SpriteRenderer>();return VisibleRect(r.sprite).width*r.transform.lossyScale.x;}).OrderBy(x=>x).ToArray();
                float scale=widths[widths.Length/2]/calibrationWidth;
                for(int i=0;i<towers.Length;i++)
                {
                    var root=v.TowerPresentationTransform(towers[i].Id);var sr=root.Find("Tower Art").GetComponent<SpriteRenderer>();
                    var visible=VisibleRect(sr.sprite);float oldWidth=visible.width*sr.transform.lossyScale.x;
                    var bottom=sr.transform.TransformPoint(new Vector3(visible.center.x,visible.yMin,0));
                    sr.sprite=sprites[i%4];
                    // Shared screen-space envelope: at most original median width and 1.25 times that height.
                    float heightLimit=i%4==0?1f:1.25f;
                    float actualScale=Mathf.Min(scale,widths[widths.Length/2]/sr.sprite.bounds.size.x,widths[widths.Length/2]*heightLimit/sr.sprite.bounds.size.y);
                    sr.transform.rotation=camera.transform.rotation;sr.transform.localScale=Vector3.one*actualScale;sr.transform.position=bottom;
                    var equipment=root.GetComponent<AdvancementVisual>();if(equipment!=null)UnityEngine.Object.DestroyImmediate(equipment);
                    float ppw=960f/(2*camera.orthographicSize);
                    report.Add(prefix+" tower="+towers[i].Id+" kind="+(i%4)+" oldWidthPx="+(oldWidth*ppw).ToString("F1")+" newSizePx="+(sr.sprite.bounds.size.x*actualScale*ppw).ToString("F1")+"x"+(sr.sprite.bounds.size.y*actualScale*ppw).ToString("F1"));
                }
                Capture(v,prefix+"-plain-basic.png");
            }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
        }
        static Rect VisibleRect(Sprite sprite)
        {
            var texture=sprite.texture;var rt=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;Graphics.Blit(texture,rt);RenderTexture.active=rt;
            var readable=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            readable.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);readable.Apply();
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
            var rect=sprite.textureRect;int minX=(int)rect.width,minY=(int)rect.height,maxX=0,maxY=0;
            for(int y=0;y<(int)rect.height;y++)for(int x=0;x<(int)rect.width;x++)if(readable.GetPixel((int)rect.x+x,(int)rect.y+y).a>.125f)
            {minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
            UnityEngine.Object.DestroyImmediate(readable);
            return new Rect((minX+sprite.textureRectOffset.x-sprite.pivot.x)/sprite.pixelsPerUnit,(minY+sprite.textureRectOffset.y-sprite.pivot.y)/sprite.pixelsPerUnit,(maxX-minX+1)/sprite.pixelsPerUnit,(maxY-minY+1)/sprite.pixelsPerUnit);
        }
        static void Capture(BattleView v,string filename)
        {
            var camera=v.BattleCamera;var rt=new RenderTexture(540,960,24);camera.targetTexture=rt;
            if(v.Hud!=null)
            {
                v.Hud.UICamera.targetTexture=rt;Canvas.ForceUpdateCanvases();v.Hud.Synchronize();
                foreach(var tr in v.Hud.Canvas.GetComponentsInChildren<Transform>(true))if(tr.name.StartsWith("AdvanceTower_"))tr.gameObject.SetActive(false);
            }
            camera.Render();if(v.Hud!=null)v.Hud.UICamera.Render();RenderTexture.active=rt;
            var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(Output,filename),image.EncodeToPNG());camera.targetTexture=null;if(v.Hud!=null)v.Hud.UICamera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
