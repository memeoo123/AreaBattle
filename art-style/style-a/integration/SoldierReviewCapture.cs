using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
 public static class SoldierReviewCapture
 {
  public static void RunBatch()
  {
   var root=new GameObject("Soldier review fixture");RenderTexture rt=null;Texture2D image=null;
   try{
    string dir=Path.Combine(BattleBuild.Workspace,"art-style/style-a/soldier-review/frames");Directory.CreateDirectory(dir);
    var cam=root.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=.75f;cam.aspect=2;
    cam.transform.rotation=Quaternion.Euler(65,0,0);cam.transform.position=-cam.transform.forward*10;
    cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.73f,.77f,.49f);
    rt=new RenderTexture(720,360,24);cam.targetTexture=rt;image=new Texture2D(720,360,TextureFormat.RGB24,false);
    var units=new ClearSoldierVisual[4];var states=new SoldierState[4];
    for(int i=0;i<4;i++){
     var go=new GameObject("Review soldier");go.transform.SetParent(root.transform);
     units[i]=go.AddComponent<ClearSoldierVisual>();units[i].Begin(0);
     float x=i%2==0?.28f:.72f,y=i<2?.42f:.10f;
     states[i]=new SoldierState{ShipType=1,Camp=i%2+1,LegStart=Vector3.zero,LegEnd=i%2==0?Vector3.right:Vector3.left,Position=cam.ViewportToWorldPoint(new Vector3(x,y,10))-Vector3.up*.006f};
     if(i<2)go.transform.localScale*=7;
    }
    for(int f=0;f<24;f++){
     float clock=f*.1f;
     for(int i=0;i<4;i++){if(f==12)units[i].BeginDeath(clock);units[i].Synchronize(states[i],cam,clock);}
     cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,720,360),0,0);image.Apply();
     File.WriteAllBytes(Path.Combine(dir,f.ToString("D2")+".png"),image.EncodeToPNG());
    }
    Debug.Log("SOLDIER_REVIEW_CAPTURE_PASS frames=24 fps=10 magnification=7 actualHeight=18px");
   }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);return;}
   finally{RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(root);if(image!=null)UnityEngine.Object.DestroyImmediate(image);if(rt!=null)UnityEngine.Object.DestroyImmediate(rt);}
   EditorApplication.Exit(0);
  }
 }
}
