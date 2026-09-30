using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgamePreviewCapture
    {
        // Explicit isolated rendering fixture. Never creates or loads a user account.
        public static void Run()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Preview Camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=960;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.16f,.18f,.2f);camera.cullingMask=1<<5;
                var texture=new RenderTexture(540,960,24);camera.targetTexture=texture;
                var root=new GameObject("Outgame Preview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.layer=5;
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;
                canvas.scaleFactor=.5f;Canvas.ForceUpdateCanvases();
                string cfg=BattleView.ReadText("Data/Outgame/CommanderConfig"),skills=BattleView.ReadText("Data/AllSkillConfig");
                var rules=new OutgameCommanderProgression(cfg,BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                var view=root.AddComponent<OutgameMenuView>();view.Initialize((RectTransform)root.transform,6,rules);
                Capture(camera,texture,"outgame-main-fixture.png");
                var held=new OutgameProfile();OutgameProfileInitialization.Initialize(held,cfg,skills);held.inventory.goldNum=10250;
                var inventory=new OutgameLocalInventory(held.inventory,skills);var effects=new OutgameCommanderActionValidation.Effects();
                var actions=new OutgameCommanderActions(held,rules,inventory.Count,(id,d)=>inventory.Change(id,d),effects,BattleView.ReadText("Data/Outgame/StatisticEventConfig"));
                var commander=view.Page(OutgameMenuPage.Commander).gameObject.AddComponent<OutgameCommanderView>();
                commander.Bind(held,rules,actions,()=>6,inventory.Count,new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig")).Chinese,2,()=>6);
                view.RequestPage(OutgameMenuPage.Commander);view.AdvanceTransition(.3f);
                Capture(camera,texture,"outgame-commander-fixture.png");
                commander.ShowSkillDetail(1);Capture(camera,texture,"outgame-skill-detail-fixture.png");
                string soldierJson=BattleView.ReadText("Data/Outgame/SkinConfig"),sceneJson=BattleView.ReadText("Data/Outgame/SceneSkinConfig"),statistics=BattleView.ReadText("Data/Outgame/StatisticEventConfig");
                var skins=OutgameSkinCatalog.FromOriginal(null,soldierJson,sceneJson);held.skins=skins.State;
                var shopEffects=new OutgameSkinItemValidation.Effects{skins=skins,profile=held,store=new OutgameProfileStore(Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"preview.json"))};
                shopEffects.unlock=new OutgameSkinActions(skins,(id,value)=>{},statistics);
                var tools=new OutgameToolDispatcher(inventory,shopEffects,BattleView.ReadText("Data/Outgame/GameItemConfig"),sceneJson,soldierJson);
                var skinActions=new OutgameSkinItemActions(tools,skins,shopEffects,soldierJson,sceneJson,statistics);var sceneActions=new OutgameSceneSkinActions(skinActions,tools,skins,shopEffects,statistics);
                var shop=view.Page(OutgameMenuPage.Skins);var sprites=new OutgameSkinSprites();var language=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));
                var lists=shop.gameObject.AddComponent<OutgameShopSkinLists>();lists.Build(skins,1,(card,id,type)=>card.Bind(id,type,skinActions,skins,language.Chinese,name=>sprites.ForSkin(id),()=>false,()=>5,(videoId,callback)=>callback(false),"preview-only",()=>{},type==4?sceneActions:null));
                foreach(var toggle in shop.GetComponentsInChildren<Toggle>(true))for(int i=0;i<toggle.onValueChanged.GetPersistentEventCount();i++)toggle.onValueChanged.SetPersistentListenerState(i,UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
                var modelRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots")).GetComponent<OutgameModelRoots>();
                modelRoot.Connect(skins,soldierJson,()=>1);
                using var menuSceneBinding=new OutgameMenuSceneBinding(view,modelRoot);
                var rotation=new OutgameModelRotation(modelRoot);
                modelCamera=modelRoot.HomeCamera;modelCamera.targetTexture=texture;modelCamera.aspect=540f/960;
                camera.clearFlags=CameraClearFlags.Depth;
                foreach(var animator in modelRoot.GetComponentsInChildren<OutgameBakedAnimator>(true)){animator.Clock=()=>0;animator.Play("idle",true);animator.Tick();}
                Capture(modelCamera,texture,"outgame-models-camera-only.png");
                var tabs=shop.gameObject.AddComponent<OutgameShopTabs>();tabs.Bind(skins,1,language.Chinese,id=>{},rotation.Select);
                view.RequestPage(OutgameMenuPage.Skins);view.AdvanceTransition(.3f);tabs.InitializeSourceSelection();
                Capture(camera,texture,"outgame-shop-soldier-fixture.png");
                shop.Find("bottom/ToggleArr/tog_attack").GetComponent<Toggle>().isOn=true;Capture(camera,texture,"outgame-shop-attack-fixture.png");
                shop.Find("bottom/ToggleArr/tog_MapSkin").GetComponent<Toggle>().isOn=true;Capture(camera,texture,"outgame-shop-scene-fixture.png");
                var sceneEffects=new OutgameSceneEffects(BattleView.ReadText("Data/Outgame/SceneEffectConfig"),OutgameSceneEffectAssets.Load);
                var originalMaterial=modelRoot.HomeBackdrop.GetComponent<Renderer>().sharedMaterial;
                var previewMaterial=new Material(originalMaterial);
                modelRoot.HomeBackdrop.GetComponent<Renderer>().sharedMaterial=previewMaterial;
                modelRoot.ModelBackdrop.GetComponent<Renderer>().sharedMaterial=previewMaterial;
                var sceneStyle=new OutgameSceneStyle(sceneJson,modelRoot.HomeBackdrop.GetComponent<Renderer>(),OutgameSceneTextures.Load,id=>sceneEffects.Hide(id),(id,parent)=>sceneEffects.Show(id,parent),id=>{});
                new OutgameScenePresentation(modelRoot).MoveCamera(false,0);
                foreach(int id in new[]{7,8,9})
                {
                    sceneStyle.Change(id,true);
                    foreach(var skeleton in modelRoot.GetComponentsInChildren<Spine.Unity.SkeletonAnimation>()){skeleton.Update(.5f);skeleton.LateUpdate();}
                    foreach(var particles in modelRoot.GetComponentsInChildren<ParticleSystem>())particles.Simulate(.5f,false,true,true);
                    Capture(modelCamera,texture,"outgame-scene-"+id+"-original-effects.png");
                }
                UnityEngine.Object.DestroyImmediate(previewMaterial);
                camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(texture);
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static Camera modelCamera;
        static void Capture(Camera camera,RenderTexture rt,string name)
        {
            Canvas.ForceUpdateCanvases();if(modelCamera!=null)modelCamera.Render();camera.Render();RenderTexture.active=rt;
            var pixels=new Texture2D(540,960,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,540,960),0,0);pixels.Apply();
            string folder=Path.Combine(BattleBuild.Workspace,"analysis/captures");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,name),pixels.EncodeToPNG());
            RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
