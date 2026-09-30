using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class Original871Capture
    {
        public static void Run()
        {
            try {
                RecoveredCommanderImporter.Import();
                RecoveredHudImporter.Import();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Original level871 comparison");var view=root.AddComponent<BattleView>();
                string fixture=Path.Combine(BattleBuild.Workspace,"analysis/captures/original871-loadout.json");
                File.WriteAllText(fixture,JsonUtility.ToJson(BattleLoadout.CreateFixture(871,1,1,0,27),true));
                view.Loadout=BattleLoadout.LoadOrCreate(fixture);
                UnityEngine.Random.InitState(4305); // Explicit local replay fixture; original entry RNG is unknown.
                view.RandomSourceOverride=new System.Random(4305);view.CommanderMode=1;view.InitializeNormalLevel(871);
                if(!view.Initialized||view.LevelId!=120)throw new Exception("Original871 layout binding failed");
                Capture(view,"original871-reconstruction-initial.png");
                for(int i=0;i<60;i++)view.AdvanceFrame(1f/60f,1f/60f);
                Capture(view,"original871-reconstruction-1second.png");
                for(int i=0;i<30;i++)view.AdvanceFrame(1f/60f,1f/60f);
                Capture(view,"original871-reconstruction-1_5seconds.png");
                view.Simulation.Pause(true);Capture(view,"original871-reconstruction-paused.png");
                view.Simulation.Pause(false);
                // Controlled production result events; these frames have no original reference yet.
                foreach(var tower in view.Simulation.Towers.Where(t=>t.Camp!=BattleSimulation.PlayerCampID).ToArray())
                    view.Simulation.ChangeScore(tower.Id,BattleSimulation.PlayerCampID,-tower.Score-1);
                view.AdvanceFrame(.5f,.5f);Capture(view,"original871-reconstruction-victory-entry.png");
                view.AdvanceFrame(.3f,.3f);Capture(view,"original871-reconstruction-victory.png");
                view.RestartCurrentLevel();Capture(view,"original871-reconstruction-victory-retry.png");
                foreach(var tower in view.Simulation.Towers.Where(t=>t.Camp==BattleSimulation.PlayerCampID).ToArray())
                    view.Simulation.ChangeScore(tower.Id,2,-tower.Score-1);
                view.AdvanceFrame(.5f,.5f);Capture(view,"original871-reconstruction-defeat.png");
                view.RestartCurrentLevel();Capture(view,"original871-reconstruction-defeat-retry.png");
                UnityEngine.Object.DestroyImmediate(root);
                Debug.Log("ORIGINAL871_CAPTURE_PASS");if(Application.isBatchMode)EditorApplication.Exit(0);
            } catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static void Capture(BattleView view,string name)
        {
            const int width=723,height=1282;
            var camera=view.BattleCamera;camera.aspect=width/(float)height;camera.orthographicSize=2.1f*.5625f/camera.aspect;
            view.SynchronizeBackground(camera.aspect);
            var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;view.Hud.UICamera.targetTexture=rt;
            Canvas.ForceUpdateCanvases();view.Hud.Synchronize();camera.Render();view.Hud.UICamera.Render();
            RenderTexture.active=rt;var img=new Texture2D(width,height,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,width,height),0,0);img.Apply();
            File.WriteAllBytes(Path.Combine(BattleBuild.Workspace,"analysis/captures",name),img.EncodeToPNG());
            camera.targetTexture=null;view.Hud.UICamera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(img);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
