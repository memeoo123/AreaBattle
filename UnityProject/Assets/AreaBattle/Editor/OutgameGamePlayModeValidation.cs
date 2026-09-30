using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameGamePlayModeValidation
    {
        const string Pending="AreaBattle.GameControlPlayMode";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Real GameControl and imported original scene references, native recovered home textures/effects and deferred destruction. Local synchronous resource adapter; battle asset11001/loading transport, full Main and Player not claimed.";public List<string> checks=new List<string>();}
        sealed class LocalResources:IOutgameGameSceneResources
        {
            public void LoadAsset(int id,Action<GameObject> callback,object[] arguments=null){throw new InvalidOperationException("Battle prefab transport is outside this native home fixture");}
            public void LoadTexture(string path,Action<Texture2D> callback,object[] arguments=null)=>OutgameSceneTextures.Load(path,texture=>callback((Texture2D)texture));
            public void LoadPrefab(string path,Action<GameObject> callback,object[] arguments=null)=>OutgameSceneEffectAssets.Load(path,callback);
        }
        static Report report;static OutgameGameControl control;static OutgameControllerRegistry registry;static GameObject root,child;static GameObject[] effects;static int frame;static double began;
        static OutgameGamePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string description){if(!condition)throw new Exception(description);report.checks.Add(description);}
        static void Start()
        {
            report=new Report();began=EditorApplication.timeSinceStartup;
            try{
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));var owner=root.GetComponentInChildren<OutgameGameSceneMono>(true);owner.Scene_home_CJroot.sharedMaterial=new Material(owner.Scene_home_CJroot.sharedMaterial);
                var config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Debug.LogError),null,null,null,null,null,null);var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),Debug.LogError);reader.ReadTable(config.dicSceneSkin);reader.ReadTable(config.dicSceneEffect);
                registry=new OutgameControllerRegistry();int selected=7;OutgameCoreControllerBindings.BindGame(registry,()=>config,new LocalResources(),()=>selected,()=>0,id=>{});control=(OutgameGameControl)registry.Resolve(4064);control.OnInit();control.InitScene();
                Check(control.SceneRoot==owner&&control.UpgradeCamera==owner.UpgradeCamera&&control.HomeTextures.Length==config.dicSceneSkin.Count,"Production binding discovers original tagged scene and all four cameras");
                effects=new GameObject[3];effects[0]=control.SceneEffects[7];control.ChangeSceneStyle(8,true);effects[1]=control.SceneEffects[8];control.ChangeSceneStyle(9,true);effects[2]=control.SceneEffects[9];
                Check(!effects[0].activeSelf&&!effects[1].activeSelf&&effects[2].activeSelf&&effects[2].transform.parent==owner.Scene_home_CJroot.transform,"Three recovered original native effects share one selection cache");
                control.ChangeSceneStyle(7,true);Check(control.SceneEffects.Count==3&&effects[0].activeSelf&&!effects[2].activeSelf&&owner.Scene_home_CJroot.sharedMaterial.mainTexture==control.HomeTextures[6],"Returning to skin7 reuses native texture/effect identity");
                control.PrepareGameScene();control.PrepareHomeScene();Check(owner.Scene_home.gameObject.activeSelf&&!owner.Scene_game.gameObject.activeSelf&&owner.CommanderCamera.gameObject.activeSelf&&!owner.GameCamera.gameObject.activeSelf,"Native home/game camera and root cycle");
                child=new GameObject("unlock-preview-fixture");child.transform.SetParent(owner.objUnlockCommanderRoot,false);control.ClearRenderModel();Check(child!=null,"ClearRenderModel uses deferred native destruction");
                control.OnDispose();Check(!registry.HasInstance(4064)&&root&&effects[0],"Source disposal releases singleton but preserves scene/resources");frame=Time.frameCount;EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll(){try{if(EditorApplication.timeSinceStartup-began>20)throw new TimeoutException("Native game frame timed out");if(Time.frameCount<=frame)return;Check(child==null&&control.SceneRoot.objUnlockCommanderRoot.childCount==0,"Next native frame removes unlock model while scene remains");Finish(null);}catch(Exception ex){Finish(ex);}}
        static void Finish(Exception ex){EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/game-controller-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
