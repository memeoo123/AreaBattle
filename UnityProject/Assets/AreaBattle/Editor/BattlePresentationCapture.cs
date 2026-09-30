using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class BattlePresentationCapture
    {
        public static void CaptureScreens()
        {
            try{
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Presentation traversal");var view=root.AddComponent<BattleView>();
                foreach(int normal in new[]{0,14,25})
                {
                    view.InitializeNormalLevel(normal);if(!view.Initialized)throw new Exception("Scene failed: "+normal);
                    BattleBuild.Capture(view,"normal"+normal+"-initial-hud.png");
                    view.Guide.Tick(0,1.1f);view.Guide.Confirm();
                    for(int frame=0;frame<42;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"normal"+normal+"-guide-hand-07seconds.png");
                }
                view.RequestedNormalLevel=-1;view.InitializeScene(5);
                view.Simulation.Connect(3,1);view.Simulation.Connect(3,2);view.Simulation.Connect(3,5);
                for(int frame=0;frame<300;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                BattleBuild.Capture(view,"ordinary5-running-5seconds.png");
                view.InitializeSpecialScene(99998);BattleBuild.Capture(view,"boss99998-initial-hud.png");
                foreach(var boss in view.Simulation.Towers)if(boss.IsBoss)view.Simulation.ChangeScore(boss.Id,1,-boss.Score*.5f);
                view.RefreshPresentation();BattleBuild.Capture(view,"boss99998-half-health-hud.png");
                view.InitializePvPMap(1,30,1,1,new[]{1,1,1});BattleBuild.Capture(view,"pvp-map1-initial-hud.png");
                foreach(int skill in new[]{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18})
                {
                    view.CommanderMode=(skill-1)/3+1;view.InitializeScene(5,30);
                    view.Simulation.Connect(3,1);
                    for(int frame=0;frame<90;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    int target=skill==3||skill==12?1:skill==6||skill==9||skill==15?3:0;
                    if(!view.TryUseSkillSlot((skill-1)%3,target,new Vector3(0,0,2.5f)))throw new Exception("Skill input failed "+skill);
                    for(int frame=0;frame<12;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"skill"+skill+"-02seconds.png");
                    for(int frame=0;frame<60;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                    BattleBuild.Capture(view,"skill"+skill+"-12seconds.png");
                    if(skill==3||skill==6||skill==9||skill==12||skill==15)
                    {
                        view.SkillTargets.BeginDrag(view.SkillInput.Rule((skill-1)%3).targetType);
                        view.SkillTargets.ShowTarget(view.SkillInput.Rule((skill-1)%3).targetType,view.Simulation.Tower(target));
                        view.SkillTargets.Step(.2f);BattleBuild.Capture(view,"skill"+skill+"-target-indicator.png");view.SkillTargets.EndDrag();
                    }
                    if(view.SkillPresentation.MissingResourceCount!=0)throw new Exception("Missing skill resource "+skill);
                }
                foreach(var asset in Resources.LoadAll<TextAsset>("Data/Levels"))
                {
                    var layout=JsonUtility.FromJson<LevelLayout>(asset.text);
                    if(layout.ObstacleInfoCfgs==null||layout.ObstacleInfoCfgs.Length<3)continue;
                    int id=int.Parse(asset.name.Substring("level_".Length));view.InitializeScene(id,30);
                    BattleBuild.Capture(view,"obstacles-layout"+id+"-original-meshes.png");break;
                }
                UnityEngine.Object.DestroyImmediate(root);
                Debug.Log("AREABATTLE_PRESENTATION_CAPTURE_PASS");if(Application.isBatchMode)EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
    }
}
