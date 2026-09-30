using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgamePrefabLoaderPlayModeValidation
    {
        const string Pending="AreaBattle.PrefabLoaderNative";const string ManifestKey="prefab-loader-native-bundles";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="GameControl -> LoadPrefabControl -> ResLoadHelper -> legacy scheduler/package/download runtime -> native AssetBundle, using rebuilt original assets and local file patch URLs. Full Main/account/Player and original remote URLs remain unverified.";public int requests;public List<string> checks=new List<string>();}
        static Report report;static OutgameLegacyBundleRuntime runtime;static OutgameLegacyResourceScheduler scheduler;static OutgameLoadPrefabControl loader;static OutgameGameControl game;static OutgameControllerRegistry registry;static OutgameUiAnimation runner;static GameObject root,effect;static int phase;static bool stopped;static double began;static object[] effectArgs;static OutgameLegacyConfigManager config;
        static OutgamePrefabLoaderPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange change){if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string label){if(!condition)throw new Exception(label);report.checks.Add(label);}
        static void Start()
        {
            report=new Report();began=EditorApplication.timeSinceStartup;phase=0;stopped=false;
            try{
                string patch="1|fixture|native\n"+ManifestKey+"|"+ManifestKey+"|fixture|0";
                for(int i=0;i<OutgamePrefabLoaderBundles.Keys.Length;i++)patch+="\n"+OutgamePrefabLoaderBundles.Keys[i]+"|"+OutgamePrefabLoaderBundles.Names[i]+"|fixture|0";
                var services=new OutgameLegacyPatchBundleServices(patch,new Uri(OutgamePrefabLoaderBundles.Folder+"/").AbsoluteUri.TrimEnd('/'),1,Debug.LogError,Debug.LogWarning,()=>false,Debug.Log);
                runtime=new OutgameLegacyBundleRuntime(()=>services,Debug.Log,Debug.LogWarning,Debug.LogError,Debug.LogException,create:url=>{report.requests++;return UnityWebRequestAssetBundle.GetAssetBundle(url);},dependencyQuery:key=>{
                    int index=Array.IndexOf(OutgamePrefabLoaderBundles.Keys,key);var deps=runtime.Manifest.GetAllDependencies(OutgamePrefabLoaderBundles.Names[index]);
                    for(int i=0;i<deps.Length;i++)deps[i]=OutgamePrefabLoaderBundles.Keys[Array.IndexOf(OutgamePrefabLoaderBundles.Names,deps[i])];return deps;
                });runtime.DisableUnload=true;OutgameLegacyBundleManager.SharedRuntime=runtime;OutgameLegacyBundleManager.Initialize();
                runner=new GameObject("source-package-runner").AddComponent<OutgameUiAnimation>();var messages=new OutgameMessageDispatcher();messages.AddListener("GF_ResLoadError",args=>throw new Exception("native package error"));
                var package=new OutgameLegacyPackageLoad(new Dictionary<string,AssetBundle>(),runtime.LoadAsync,routine=>runner.StartCoroutine(Guard(routine)),Debug.Log,Debug.LogError,messages);
                scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(package.Start,runtime.Unload),Debug.LogWarning);
                config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Debug.LogError),null,null,null,null,null,null);var read=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),Debug.LogError);read.ReadTable(config.dicEntityModel);read.ReadTable(config.dicSceneSkin);read.ReadTable(config.dicSceneEffect);OutgameConfigDerivedIndexes.RebuildSceneResources(config.dicSceneSkin,config.SceneResources);
                registry=new OutgameControllerRegistry();var helper=new OutgameResLoadHelper(()=>false,()=>scheduler,Debug.LogError);OutgameCoreControllerBindings.BindPrefabLoader(registry,()=>config,helper,()=>new OutgamePrefabCache(name=>Resources.Load<OutgameLevelEditorConfig>(name)),Debug.LogError);
                OutgameCoreControllerBindings.BindGame(registry,()=>config,new OutgameGameSceneResources(()=>((OutgameLoadPrefabControl)registry.Resolve(4117))),()=>1,()=>0,id=>{});
                loader=(OutgameLoadPrefabControl)registry.Resolve(4117);game=(OutgameGameControl)registry.Resolve(4064);loader.OnInit();game.OnInit();
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));var owner=root.GetComponentInChildren<OutgameGameSceneMono>(true);owner.Scene_home_CJroot.sharedMaterial=new Material(owner.Scene_home_CJroot.sharedMaterial);
                runtime.LoadManifest(ManifestKey);Check(runtime.Manifest==null&&report.requests==1,"Native manifest starts asynchronously before scene loading");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static IEnumerator Guard(IEnumerator routine){while(true){object current;try{if(!routine.MoveNext())yield break;current=routine.Current;}catch(Exception ex){Finish(ex);yield break;}yield return current;}}
        static void Poll()
        {
            if(stopped)return;
            try{
                scheduler.Update(0,Time.unscaledDeltaTime);if(EditorApplication.timeSinceStartup-began>40)throw new TimeoutException("Native prefab resource graph timed out");
                if(phase==0){if(runtime.Manifest==null)return;game.InitScene();Check(game.HomeTextures[0]==null&&scheduler.PendingCount==1,"Game InitScene queues original texture path through concrete loader");phase=1;return;}
                if(phase==1){if(game.HomeTextures[0]==null)return;Check(game.SceneRoot.Scene_home_CJroot.sharedMaterial.mainTexture==game.HomeTextures[0]&&scheduler.GetBundleInfo(OutgamePrefabLoaderBundles.Keys[1]).Bundle!=null,"Native downloaded home texture reaches GameControl through source callbacks");
                    game.LoadGameScene(1);effectArgs=new object[]{101,game.SceneRoot.Scene_game};loader.LoadPrefab("effect/eff_idle_GuBao",(value,args)=>{effect=value;Check(ReferenceEquals(args,effectArgs),"Prefab resource callback retains original arguments");},effectArgs);phase=2;return;}
                if(game.GameBackground==null||!game.GameSprites.ContainsKey(1)||effect==null||runtime.Downloads.Count!=0)return;
                Check(loader.Contains(11001)&&game.GameBackground.transform.IsChildOf(game.SceneRoot.Scene_game)&&game.GameBackground.sprite.texture.name=="scene_skin_game1","Source EntityModel11001 prefab and battle texture load through complete native graph");
                Check(effect.name=="eff_idle_GuBao(Clone)"&&game.GameBackground.transform.root==root.transform,"String prefab and id-based scene path preserve distinct names/ownership");
                int requests=report.requests;GameObject warm=null;loader.LoadAsset(11001,(value,args)=>warm=value);game.LoadGameScene(1);
                Check(warm&&warm.name=="HD4_CJ_1"&&report.requests==requests&&game.GameSprites.Count==1,"Warm prefab and game-sprite caches reuse templates without new downloads");
                loader.OnDispose();game.OnDispose();Check(!registry.HasInstance(4117)&&!registry.HasInstance(4064)&&effect&&game.GameBackground,"Controller disposal preserves loaded native assets while clearing singleton slots");Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/prefab-loader-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
