using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class BossPresentationCapture
    {
        public static void CaptureScreens()
        {
            try{
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var host=new GameObject("Boss source presentation captures");var view=host.AddComponent<BattleView>();
                foreach(int layout in new[]{99998,99999}){
                    view.InitializeSpecialScene(layout);if(!view.Initialized)throw new Exception("Boss scene failed: "+layout);
                    var initialBoss=view.Simulation.Towers.Single(t=>t.IsBoss);var debugVisual=view.TowerPresentationTransform(initialBoss.Id).GetComponent<RecoveredBossVisual>();var debugRenderer=debugVisual.MainSkeleton.GetComponent<MeshRenderer>();var debugMesh=debugVisual.MainSkeleton.GetComponent<MeshFilter>().sharedMesh;
                    Debug.Log("BOSS_ATLAS_PROBE "+EditorJsonUtility.ToJson(debugVisual.MainSkeleton.skeletonDataAsset)+" nativeId="+debugVisual.MainSkeleton.skeletonDataAsset.GetInstanceID());
                    Debug.Log("BOSS_RENDER_PROBE "+layout+" enabled="+debugRenderer.enabled+" active="+debugRenderer.gameObject.activeInHierarchy+" layer="+debugRenderer.gameObject.layer+" bounds="+debugRenderer.bounds+" vertices="+(debugMesh==null?-1:debugMesh.vertexCount)+" color="+(debugMesh==null?Color.magenta:debugMesh.colors[0])+" material="+(debugRenderer.sharedMaterial==null?"NULL":debugRenderer.sharedMaterial.name)+" shader="+(debugRenderer.sharedMaterial==null?"NULL":debugRenderer.sharedMaterial.shader.name)+" supported="+(debugRenderer.sharedMaterial!=null&&debugRenderer.sharedMaterial.shader.isSupported)+" tex="+(debugRenderer.sharedMaterial==null||debugRenderer.sharedMaterial.mainTexture==null?"NULL":debugRenderer.sharedMaterial.mainTexture.name)+" skeletonScale="+debugVisual.MainSkeleton.Skeleton.ScaleX);
                    BattleBuild.Capture(view,"boss"+layout+"-spine-initial.png");
                    for(int frame=0;frame<300;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"boss"+layout+"-spine-5seconds.png");
                    var boss=view.Simulation.Towers.Single(t=>t.IsBoss);
                    // Controlled source action entry for presentation sampling, no score edits.
                    view.Simulation.ExecuteBossAction(boss.Id,boss.Camp==6?5:2);
                    for(int frame=0;frame<18;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"boss"+layout+"-spine-action-03seconds.png");
                    view.RestartCurrentLevel();BattleBuild.Capture(view,"boss"+layout+"-spine-retry.png");
                }
                UnityEngine.Object.DestroyImmediate(host);Debug.Log("AREABATTLE_BOSS_PRESENTATION_CAPTURE_PASS");
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
    }
}

