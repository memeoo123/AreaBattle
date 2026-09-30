using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameGameControlValidation
    {
        sealed class Loads:IOutgameGameSceneResources
        {
            public readonly List<(int id,Action<GameObject> callback)> Assets=new List<(int,Action<GameObject>)>();
            public readonly List<(string name,Action<Texture2D> callback)> Textures=new List<(string,Action<Texture2D>)>();
            public readonly List<(string name,Action<GameObject> callback)> Prefabs=new List<(string,Action<GameObject>)>();
            public void LoadAsset(int id,Action<GameObject> loaded,object[] arguments=null)=>Assets.Add((id,loaded));public void LoadTexture(string name,Action<Texture2D> loaded,object[] arguments=null)=>Textures.Add((name,loaded));public void LoadPrefab(string name,Action<GameObject> loaded,object[] arguments=null)=>Prefabs.Add((name,loaded));
        }
        sealed class Fixture:IDisposable
        {
            public GameObject Root;public OutgameGameSceneMono Scene;public Loads Loads=new Loads();public OutgameControllerRegistry Registry=new OutgameControllerRegistry();public OutgameLegacyConfigManager Config;public OutgameGameControl Control;public int Notices;readonly Material material;
            public Fixture(bool init=true)
            {
                Root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));Scene=Root.GetComponentInChildren<OutgameGameSceneMono>(true);
                material=new Material(Scene.Scene_home_CJroot.sharedMaterial);Scene.Scene_home_CJroot.sharedMaterial=material;
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),null,null,null,null,null,null);
                Config.dicSceneSkin.Add(1,new SceneSkinConfig{id=1,idleIconName="idle1",gameIconName="game1"});Config.dicSceneSkin.Add(2,new SceneSkinConfig{id=2,idleIconName="idle2",gameIconName="game2"});
                Registry.Bind(4064,()=>new OutgameGameControl(()=>Config,Registry,Loads,()=>1,()=>0,id=>Notices++,null,()=>1080,()=>1920));Control=(OutgameGameControl)Registry.Resolve(4064);
                if(init){Control.OnInit();Control.InitScene();}
            }
            public void Dispose(){foreach(var sprite in Control.GameSprites.Values)if(sprite)UnityEngine.Object.DestroyImmediate(sprite);UnityEngine.Object.DestroyImmediate(Root);UnityEngine.Object.DestroyImmediate(material);}
        }
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-game-controller-original-scene-owner-and-lifecycle",()=>{using(var f=new Fixture(false)){
                var c=f.Control;c.Boss3Skill1First=42;c.UseAnimationIns=true;c.OnInit();c.InitScene();
                Require(c.HomeTextures is Texture2D[]&&c.HomeTextures.Length==2&&!c.UseAnimationIns&&c.Boss3Skill1First==42,"source init resets only flag64 and texture array");
                Require(ReferenceEquals(c.SceneRoot,f.Scene)&&c.SceneRoot.gameObject.CompareTag("Tag10")&&c.SceneRoot.name=="objSceneRoot"&&c.UpgradeCamera.name=="UpgradeCamera","native tag/name discovery and four original cameras");
                foreach(var field in typeof(OutgameGameSceneMono).GetFields())Require(field.GetValue(f.Scene)!=null,"original serialized reference "+field.Name);
                Require(f.Scene.soldierRoot.GetChild(1)==f.Root.GetComponent<OutgameModelRoots>().ModelBackdrop&&c.Aspect==.5625f&&f.Loads.Textures[0].name=="idle1"&&f.Notices==1,"source sibling1 and startup style");
                var textures=c.HomeTextures;c.OnInit();Require(!ReferenceEquals(textures,c.HomeTextures)&&c.CurrentSceneId==1&&c.SceneRoot==f.Scene,"reinit replaces cache but retains current scene and root");c.OnDispose();Require(!f.Registry.HasInstance(4064)&&c.SceneRoot==f.Scene&&c.Boss3Skill1First==42,"dispose only clears singleton slot");
            }});
            check("source-game-controller-shared-selection-home-callback-race",()=>{using(var f=new Fixture()){
                f.Config.dicSceneEffect.Add(2,new SceneEffectConfig{sceneId=2,idleEffect="effect2"});var texture=new Texture2D(8,8);
                try{f.Control.LoadGameScene(2);f.Control.ChangeSceneStyle(2,true);Require(f.Loads.Textures.Count==1&&f.Loads.Assets[0].id==11001,"home and battle share selection: same id skips home request");f.Loads.Textures[0].callback(texture);
                    Require(f.Control.HomeTextures[0]==texture&&f.Scene.Scene_home_CJroot.sharedMaterial.mainTexture==texture&&f.Loads.Prefabs[0].name=="effect2","home callback stores captured index but effects use current scene");}
                finally{UnityEngine.Object.DestroyImmediate(texture);}
            }});
            check("source-game-controller-game-texture-completion-current-key",()=>{using(var f=new Fixture()){
                f.Control.LoadGameScene(2);var asset=new GameObject("asset11001",typeof(SpriteRenderer));f.Loads.Assets[0].callback(asset);
                Require(asset.transform.parent==f.Scene.Scene_game&&asset.transform.localPosition==Vector3.zero&&asset.transform.localScale==Vector3.one&&f.Loads.Textures[1].name=="game2","asset callback binds original scene and requests captured skin texture");
                var texture=new Texture2D(12,18);try{f.Control.CurrentSceneId=1;f.Loads.Textures[1].callback(texture);var sprite=f.Control.GameSprites[1];Require(!f.Control.GameSprites.ContainsKey(2)&&sprite.texture==texture&&sprite.rect.width==12&&sprite.pixelsPerUnit==100&&sprite.pivot==new Vector2(6,9),"game callback uses current dictionary key and source Sprite.Create defaults");
                    int count=f.Loads.Textures.Count;f.Control.LoadGameScene(1);Require(f.Loads.Textures.Count==count&&f.Control.GameBackground.sprite==sprite,"sprite cache reused");bool duplicate=false;try{f.Loads.Textures[1].callback(texture);}catch(ArgumentException){duplicate=true;}Require(duplicate,"duplicate pending callback preserves Dictionary.Add failure");}
                finally{UnityEngine.Object.DestroyImmediate(texture);}
            }});
            check("source-game-controller-effect-cache-return-home-and-cameras",()=>{using(var f=new Fixture()){
                f.Config.dicSceneEffect.Add(1,new SceneEffectConfig{sceneId=1,idleEffect="idle",gameEffect="game"});f.Control.ShowSceneEffect(100,f.Scene.Scene_game,true);var effect=new GameObject("effect");f.Loads.Prefabs[0].callback(effect);
                f.Control.PrepareGameScene();Require(f.Scene.Scene_game.gameObject.activeSelf&&!f.Scene.Scene_home.gameObject.activeSelf&&f.Control.GameCamera.gameObject.activeSelf&&!f.Control.HomeCamera.gameObject.activeSelf&&!f.Control.CommanderCamera.gameObject.activeSelf,"source game camera activation");
                f.Control.PrepareHomeScene();Require(!effect.activeSelf&&!f.Scene.Scene_game.gameObject.activeSelf&&f.Scene.Scene_home.gameObject.activeSelf&&!f.Control.GameCamera.gameObject.activeSelf&&f.Control.HomeCamera.gameObject.activeSelf&&f.Control.CommanderCamera.gameObject.activeSelf,"home hides battle-offset effect in shared cache");
                f.Control.ShowSceneEffect(100,f.Scene.Scene_home,false);Require(effect.activeSelf&&effect.transform.parent==f.Scene.Scene_game&&f.Loads.Prefabs.Count==1,"cache hit activates without reparent or reinitialization");
            }});
            check("source-game-controller-scene-movement-and-camera-aspect",()=>{using(var f=new Fixture()){
                f.Control.MoveCamera(true,.5f);Require(f.Scene.soldierRoot.localPosition==Vector3.up*.5f&&!f.Scene.Scene_home_CJroot.gameObject.activeSelf&&f.Scene.soldierRoot.GetChild(1).gameObject.activeSelf,"shop route updates original child1");f.Control.MoveCamera(false,0);Require(f.Scene.soldierRoot.localPosition==Vector3.zero&&f.Scene.Scene_home_CJroot.gameObject.activeSelf&&!f.Scene.soldierRoot.GetChild(1).gameObject.activeSelf,"home route restores backdrop");
                f.Control.SetCameraSize();Require(f.Control.GameCamera.orthographicSize==2.1f&&f.Scene.Scene_game.localScale==new Vector3(.25f,0,.4f),"source baseline scale including zero-y component");
            }});
            check("source-game-controller-init-partial-failure-and-old-instance-dispose",()=>{using(var f=new Fixture(false)){
                var c=new OutgameGameControl(()=>null,f.Registry,f.Loads,()=>1,()=>0,id=>{});c.UseAnimationIns=true;bool failed=false;try{c.OnInit();}catch(NullReferenceException){failed=true;}Require(failed&&!c.UseAnimationIns&&c.HomeTextures==null,"flag reset precedes missing config failure");
                f.Control.OnDispose();var replacement=(OutgameGameControl)f.Registry.Resolve(4064);f.Control.OnDispose();Require(!f.Registry.HasInstance(4064)&&!ReferenceEquals(f.Control,replacement),"old instance clears replacement slot");
            }});
            return report;
        }
    }
}
