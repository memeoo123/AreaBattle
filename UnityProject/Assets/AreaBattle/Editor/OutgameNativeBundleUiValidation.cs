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
    [InitializeOnLoad] public static class OutgameNativeBundleUiValidation
    {
        const string Pending="AreaBattle.NativeBundleUiValidation";
        const string ManifestKey="native-legacy-ui-bundles";
        const string Key="ui/festactui/valentineui.prefab.unity3d";
        static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/native-legacy-ui-bundles");
        [Serializable] sealed class Report
        {
            public bool passed,batchMode;public string error;
            public string scope="Native file-URL UnityWebRequestAssetBundle of rebuilt original imported ValentineUI through recovered download registry/operation/package/resource/page initialization; local fixture route and zero-second unload delay, no original server/variant/dependency/production account composition";
            public int requestFrame,assetFrame,pageFrame,manifestRequestFrame,manifestReadyFrame;public long downloadedBytes,manifestBytes;public List<string> checks=new List<string>();
        }
        static Report report;static float began;static bool done,finished,unloadQueued;
        static AssetBundle releasedBundle;static OutgameLegacyBundleManager manager;
        static OutgameLegacyBundleUnload unloadManager;static OutgameLegacyPrefabResource pageResource;
        static UnityWebRequest manifestRequest;
        static UnityWebRequest request;static GameObject canvas;static OutgameLegacyResourceScheduler scheduler;
        static Dictionary<string,OutgameLegacyBundleResult> loaded=new Dictionary<string,OutgameLegacyBundleResult>();
        static OutgameNativeBundleUiValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void BuildFixture()
        {
            try{
                Directory.CreateDirectory(Folder);
                var manifest=BuildPipeline.BuildAssetBundles(Folder,new[]{new AssetBundleBuild{assetBundleName="valentine",assetNames=new[]{"Assets/AreaBattle/Resources/Recovered/FestActivity/ValentineUI.prefab"}}},BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);
                if(manifest==null||!File.Exists(Path.Combine(Folder,"valentine")))throw new Exception("Bundle build failed");
                File.WriteAllText(Path.Combine(Folder,"fixture.json"),"{\"sourcePrefab\":\"Assets/AreaBattle/Resources/Recovered/FestActivity/ValentineUI.prefab\",\"platform\":\"StandaloneWindows64\",\"bundle\":\"valentine\",\"originalPath\":\"UI/FestActUI/ValentineUI\"}");
                EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
        }
        public static void Run()
        {
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report{batchMode=Application.isBatchMode};began=Time.realtimeSinceStartup;done=false;finished=false;unloadQueued=false;loaded.Clear();
            try{
                Time.timeScale=0;
                var runner=new GameObject("Native legacy package runner").AddComponent<OutgameUiAnimation>();
                Func<IEnumerator,object> start=routine=>runner.StartCoroutine(Guard(routine));
                canvas=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                var root=new OutgameUiRootInitialization(canvas,()=>true,()=>false,null);
                root.Loaded(new OutgameLegacyPrefabResource(null,name=>Resources.Load<GameObject>("Recovered/UiRoot/"+name)).Instantiate);
                var nodes=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>root.UiRoot,()=>canvas,message=>throw new Exception(message));
                var services=new OutgameLegacyPatchBundleServices("1|fixture|native\n"+Key+"|valentine|fixture|0\n"+ManifestKey+"|"+ManifestKey+"|fixture|0",new Uri(Folder+"/").AbsoluteUri.TrimEnd('/'),1,Debug.LogError,Debug.LogWarning,()=>false,Debug.Log);
                OutgameLegacyBundleRuntime runtime=null;
                runtime=new OutgameLegacyBundleRuntime(()=>services,Debug.Log,Debug.LogWarning,Debug.LogError,Debug.LogException,
                    create:url=>{
                        var nativeRequest=UnityWebRequestAssetBundle.GetAssetBundle(url);
                        if(url==new Uri(Path.Combine(Folder,ManifestKey)).AbsoluteUri){manifestRequest=nativeRequest;report.manifestRequestFrame=Time.frameCount;}
                        else {Check(url==new Uri(Path.Combine(Folder,"valentine")).AbsoluteUri,"Parsed patch manifest and recovered service/resolver supply native file URL");request=nativeRequest;report.requestFrame=Time.frameCount;}
                        return nativeRequest;
                    },dependencyQuery:name=>runtime.Manifest.GetAllDependencies("valentine")){UnloadInterval=0};
                loaded=runtime.Loaded;var downloads=runtime.Downloads;var operations=runtime.Operations;
                unloadManager=runtime.Unloader;
                OutgameLegacyBundleManager.SharedRuntime=runtime;OutgameLegacyBundleManager.Initialize();
                manager=OutgameLegacyBundleManager.ManagerObject.GetComponent<OutgameLegacyBundleManager>();
                Check(manager!=null&&manager.gameObject.name=="AssetBundleManager"&&manager.gameObject.scene.name=="DontDestroyOnLoad","Original manager object is native and persists across scene changes");
                var resourceInitialization=new OutgameLegacyResourceInitialization(()=>{},()=>OutgameLegacyBundleManager.ManagerObject.GetComponent<OutgameLegacyBundleManager>(),()=>services,null,names=>throw new Exception("Fixture intentionally has no firstpack mapping"),routine=>start(routine),Debug.Log);
                int initialized=0;resourceInitialization.Initialized=()=>initialized++;resourceInitialization.Initialize();
                Check(resourceInitialization.IsInitialized&&initialized==1&&ReferenceEquals(resourceInitialization.BundleManager,manager),"ResourcesModule resolves actual native manager and completes no-firstpack branch");
                Check(resourceInitialization.Mono!=null&&resourceInitialization.MonoObject.name=="ResourcesMono"&&resourceInitialization.MonoObject.scene.name=="DontDestroyOnLoad","ResourcesMono native Awake persists the resource coroutine host");
                Check(runtime.ActiveVariants.Length==0&&!runtime.DisableUnload,"Unified runtime uses recovered initial variant/unload state");
                var events=new OutgameMessageDispatcher();events.AddListener("GF_ResLoadError",args=>throw new Exception("Unexpected package error"));
                var package=new OutgameLegacyPackageLoad(new Dictionary<string,AssetBundle>(),runtime.LoadAsync,start,Debug.Log,Debug.LogError,events);
                scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(package.Start,unloadManager.Unload),Debug.LogWarning);
                var pageLoader=new OutgameUiLegacyLoader((path,ready)=>scheduler.LoadPrefab(path,res=>{
                    report.assetFrame=Time.frameCount;
                    Check(res.IsReady&&res.Bundle!=null&&ReferenceEquals(res.Bundle,loaded[Key].Bundle),"Native downloaded AssetBundle reaches ready ResourcesInfo without imported-asset lookup");ready(res);
                }),nodes.Get,routine=>start(routine),Debug.Log);
                Action openPage=()=>pageLoader.Load("FestActUI/ValentineUI","UIWindow",(go,res)=>{
                    pageResource=(OutgameLegacyPrefabResource)res;
                    report.pageFrame=Time.frameCount;report.downloadedBytes=(long)request.downloadedBytes;
                    Check(request.result==UnityWebRequest.Result.Success&&report.downloadedBytes>0&&downloads.Count==0&&operations.Count==0,"Native request succeeds and recovered manager removes request/operation");
                    Check(report.pageFrame>=report.assetFrame+2&&Time.timeScale==0&&!go.activeSelf&&go.name=="ValentineUI","Bundle prefab stays hidden until two actual frame yields while paused");
                    var life=new OutgameUiLifetime(null,()=>{});var page=new OutgameUiPage(()=>life.GameObject,()=>events);
                    var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FestActivity/hud-import").text,"ValentineUI");
                    var init=new OutgameUiObjectInitialization(life,page,outlets.Read,()=>{},()=>{},()=>{},()=>{});
                    var host=new OutgameUiOpenHost(life,page,init,new OutgameUiCanvas(()=>0,()=>false),runner,()=>{},()=>{},()=>{},null,Debug.Log){Cached=true,Layer=1};
                    new OutgameUiOpenLifecycle(host,()=>events).LoadedLegacy(go,res);
                    Check(init.Objects.Count==13&&go.activeSelf&&page.Visible&&go.GetComponent<Canvas>().sortingOrder==10&&ReferenceEquals(host.LegacyResource,res),"Bundle-loaded page binds thirteen original outlets, activates and retains original resource owner");
                    int spriteCount=0;foreach(var image in go.GetComponentsInChildren<UnityEngine.UI.Image>(true))if(image.sprite!=null)spriteCount++;
                    Check(spriteCount>0&&go.GetComponentsInChildren<UnityEngine.UI.Text>(true).Length>0,"Bundled page carries native sprite and text components");done=true;
                });
                EditorApplication.update+=Poll;
                start(LoadManifestThenPage(runtime,openPage));
            }catch(Exception ex){Finish(ex);}
        }
        static IEnumerator LoadManifestThenPage(OutgameLegacyBundleRuntime runtime,Action openPage)
        {
            Check(runtime.Manifest==null,"Manifest is absent before native startup request");
            var operation=runtime.LoadManifest(ManifestKey);
            Check(runtime.Downloads.ContainsKey(ManifestKey)&&runtime.Operations.Contains(operation)&&runtime.Manifest==null,"LoadManifest starts native download before manifest publication");
            while(operation.MoveNext())yield return null;
            // The asset request can finish before the manager's next Update publishes it.
            while(runtime.Manifest==null)yield return null;
            report.manifestReadyFrame=Time.frameCount;report.manifestBytes=(long)manifestRequest.downloadedBytes;
            Check(manifestRequest.result==UnityWebRequest.Result.Success&&report.manifestBytes>0&&runtime.Operations.Count==0,"Native manifest download and asset request publish through manager update");
            Check(runtime.Manifest.GetAllAssetBundles().Length==1&&runtime.Manifest.GetAllAssetBundles()[0]=="valentine","Published native manifest supplies subsequent dependency and variant queries");
            openPage();
        }
        static IEnumerator Guard(IEnumerator routine)
        {while(true){object value;try{if(!routine.MoveNext())yield break;value=routine.Current;}catch(Exception ex){Finish(ex);yield break;}yield return value;}}
        static void Poll()
        {
            if(finished)return;
            try{
                // Download/operation/unload pumping now runs exclusively in the native manager.Update.
                scheduler.Update(0,0);
                if(done&&!unloadQueued){
                    releasedBundle=pageResource.Bundle;scheduler.RemoveBundleInfo(pageResource);unloadQueued=true;
                    Check(pageResource.IsUnloaded&&releasedBundle!=null&&loaded[Key].ReferenceCount==0,"Resource disposal queues manager unload after clearing loader owner");
                }else if(unloadQueued&&releasedBundle==null){
                    Check(loaded.Count==1&&loaded.ContainsKey(ManifestKey),"Native manager Update completes AssetBundle.Unload(true) with zero-delay fixture");
                    Finish(null);return;
                }
                if(Time.realtimeSinceStartup-began>30)throw new TimeoutException("Native bundle UI pipeline timed out");
            }
            catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            if(finished)return;finished=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-native-bundle-ui-validation.json"),JsonUtility.ToJson(report,true));
            if(manager!=null)manager.enabled=false;
            manifestRequest?.Dispose();
            request?.Dispose();foreach(var value in loaded.Values)if(value.Bundle!=null)value.Bundle.Unload(false);
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
