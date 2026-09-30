using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameFirstPackStartupValidation
    {
        const string Pending="AreaBattle.FirstPackStartupValidation", ManifestKey="firstpack-native-bundles";
        [Serializable] sealed class Report {public bool passed;public string error;public string scope="Rebuilt original firstpack through native manager Update, local synthetic patch URLs and recovered initialization/package/UI chain. Scheduler is editor-pumped; online values are absent and material requests are collected; SDK and post-config account/platform/full production startup remain explicit fixture boundaries.";public int requests,aliases,configs,initializedFrame,tipFrame;public List<string> checks=new List<string>();}
        [Serializable] sealed class BuildManifest {public Row[] assets;}
        [Serializable] sealed class Row {public string bundlePath,type,sha256;}
        static Report report;static bool finished;static float began;static OutgameLegacyBundleRuntime runtime;
        static OutgameLegacyResourceScheduler scheduler;static OutgameLegacyBundleManager manager;
        static readonly List<UnityWebRequest> requests=new List<UnityWebRequest>();
        static OutgameFirstPackStartupValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report();finished=false;began=Time.realtimeSinceStartup;requests.Clear();Time.timeScale=0;
            try{
                string folder=OutgameFirstPackBundleBuild.Folder;
                var services=new OutgameLegacyPatchBundleServices("1|fixture|native\n"+ManifestKey+"|"+ManifestKey+"|fixture|0\nfirstpack.unity3d|firstpack.unity3d|fixture|0",new Uri(folder+"/").AbsoluteUri.TrimEnd('/'),1,Debug.LogError,Debug.LogWarning,()=>false,Debug.Log);
                runtime=new OutgameLegacyBundleRuntime(()=>services,Debug.Log,Debug.LogWarning,Debug.LogError,Debug.LogException,create:url=>{var request=UnityWebRequestAssetBundle.GetAssetBundle(url);requests.Add(request);report.requests++;return request;});
                OutgameLegacyBundleManager.SharedRuntime=runtime;OutgameLegacyBundleManager.Initialize();manager=OutgameLegacyBundleManager.ManagerObject.GetComponent<OutgameLegacyBundleManager>();
                var runner=new GameObject("Firstpack startup fixture").AddComponent<OutgameUiAnimation>();
                Func<IEnumerator,object> start=routine=>runner.StartCoroutine(Guard(routine));
                EditorApplication.update+=Poll;start(Startup(services,start,runner));
            }catch(Exception ex){Finish(ex);}
        }
        static IEnumerator Startup(IOutgameLegacyBundleServices services,Func<IEnumerator,object> start,OutgameUiAnimation runner)
        {
            var manifest=runtime.LoadManifest(ManifestKey);while(manifest.MoveNext())yield return null;while(runtime.Manifest==null)yield return null;
            Check(runtime.Manifest.GetAllAssetBundles().Length==1&&runtime.Manifest.GetAllAssetBundles()[0]=="firstpack.unity3d","Native asynchronously published manifest contains original firstpack bundle name");
            var packs=new Dictionary<string,AssetBundle>();int initialized=0;
            OutgameLegacyResourceInitialization initialization=null;
            var firstPack=new OutgameLegacyFirstPack(runtime.LoadAsync,start,new OutgameLegacyPackRegistry(packs,Debug.LogError),Debug.Log,Debug.LogError,()=>initialization.Initialized?.Invoke());
            initialization=new OutgameLegacyResourceInitialization(()=>{},()=>manager,()=>services,null,firstPack.Run,routine=>start(routine),Debug.Log);
            initialization.Initialized=()=>{initialized++;report.initializedFrame=Time.frameCount;};initialization.Initialize();
            Check(initialization.IsInitialized&&initialized==0,"Nonempty firstpack path marks initialized before asynchronous completion");
            while(initialized==0)yield return null;
            Check(initialized==1&&initialization.MonoObject.scene.name=="DontDestroyOnLoad","Firstpack coroutine completes once with native persistent ResourcesMono");
            var bundle=runtime.Loaded["firstpack.unity3d"].Bundle;
            var aliases=OutgameLegacyPackListJson.Deserialize(Resources.Load<TextAsset>("firstpack").text);
            Check(packs.Count==7&&aliases.TrueForAll(name=>packs.ContainsKey(name)&&ReferenceEquals(packs[name],bundle)),"All seven original aliases share the downloaded native firstpack");report.aliases=packs.Count;
            var events=new OutgameMessageDispatcher();
            var package=new OutgameLegacyPackageLoad(packs,runtime.LoadAsync,start,Debug.Log,Debug.LogError,events);
            scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(package.Start,runtime.Unload),Debug.LogWarning);
            var canvas=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
            var root=new OutgameUiRootInitialization(canvas,()=>true,()=>false,null);bool rootReady=false,configReady=false;
            scheduler.LoadPrefab("UI/UIRoot",resource=>{Check(ReferenceEquals(resource.Bundle,bundle),"UIRoot alias resolves native firstpack owner");root.Loaded(resource.Instantiate);rootReady=true;});
            var configState=new OutgameLegacyConfigReadState(message=>throw new Exception(message));
            OutgameLegacyConfigManager configs=null;int materialRequests=0,sdkCalls=0,afterCalls=0;
            configs=new OutgameLegacyConfigManager(configState,new OutgameConfigGlobalValues(),key=>null,()=>9,()=>200,()=>configs.dicGuide,
                (path,ready,args)=>{materialRequests++;}); // Explicit fixture boundary: actual camp bundles are not loaded here.
            OutgameMainConfigStartup mainConfig=null;
            mainConfig=new OutgameMainConfigStartup(Debug.Log,()=>sdkCalls++,
                (path,ready,args)=>scheduler.LoadAsset(path,resource=>{
                    Check(ReferenceEquals(resource.Bundle,bundle),"Configuration alias resolves native firstpack owner");
                    var rows=JsonUtility.FromJson<BuildManifest>(File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/firstpack-bundle-build.json")));
                    foreach(var row in rows.assets)if(row.type=="TextAsset"){var asset=resource.Bundle.LoadAsset<TextAsset>(row.bundlePath);if(asset==null||Hash(asset.bytes)!=row.sha256)throw new Exception("Config bytes differ: "+row.bundlePath);report.configs++;}
                    Check(report.configs==85,"All 85 configuration payloads remain byte-exact after asynchronous alias loading");ready(resource);
                },args),()=>configs,()=>{
                    afterCalls++;Check(mainConfig.ConfigLoaded&&configs.IsInitialized&&configState.Reader.ReadCount==58&&configs.newRankSettingConfig!=null,"Main config completion initializes all64 original configurations before advancing");
                    Check(configs.SceneResources.Count==30&&configs.GuideLevels.Count>0&&configs.ChineseNames.Count==configs.dicAIName.Count&&configs.Globals.ShipTimeScale==.8f,"Asynchronously downloaded configuration builds original derived indexes and globals");
                    Check(materialRequests==10&&configs.CampMaterials.Count==0&&sdkCalls==1&&afterCalls==1,"Main SDK/config transition advances without waiting for ten fixture material requests");configReady=true;
                });
            mainConfig.LoadingShown();
            while(!rootReady||!configReady)yield return null;
            Check(root.IsInitialized&&root.UiRoot.parent==canvas.transform,"Bundled UIRoot initializes under native scene canvas");
            var nodes=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>root.UiRoot,()=>canvas,message=>throw new Exception(message));
            int assetFrame=0;bool tipReady=false;
            var loader=new OutgameUiLegacyLoader((path,ready)=>scheduler.LoadPrefab(path,resource=>{assetFrame=Time.frameCount;ready(resource);}),nodes.Get,routine=>start(routine),Debug.Log);
            loader.Load("MainMenu/TipUI","UIWindow",(go,resource)=>{
                report.tipFrame=Time.frameCount;Check(report.tipFrame>=assetFrame+2&&!go.activeSelf&&Time.timeScale==0,"TipUI alias preserves two-frame hidden loading while paused");
                Check(ReferenceEquals(((OutgameLegacyPrefabResource)resource).Bundle,bundle)&&go.name=="TipUI","TipUI prefab instantiates from shared firstpack owner");
                var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FirstPack/TipUI/hud-import").text,"TipUI");
                var life=new OutgameUiLifetime(null,()=>{});var page=new OutgameUiPage(()=>life.GameObject,()=>events);
                var init=new OutgameUiObjectInitialization(life,page,outlets.Read,()=>{},()=>{},()=>{},()=>{});
                var host=new OutgameUiOpenHost(life,page,init,new OutgameUiCanvas(()=>0,()=>false),runner,()=>{},()=>{},()=>{},null,Debug.Log){Cached=true,Layer=1};
                new OutgameUiOpenLifecycle(host,()=>events).LoadedLegacy(go,resource);
                Check(init.Objects.Count==6&&go.activeSelf&&page.Visible,"Native TipUI opens with all six original outlet bindings");
                var animation=go.transform.Find("Content").GetComponent<Animation>();Check(animation!=null&&animation.GetClipCount()==2,"Native TipUI retains both bundled legacy animation clips");tipReady=true;
            });
            while(!tipReady)yield return null;
            foreach(var name in new[]{"PublicUI","PublicBtn"}){var atlas=bundle.LoadAsset<SpriteAtlas>(name);Check(atlas!=null&&atlas.GetSprite(name=="PublicUI"?"Public_title":"Pulbic_add")!=null,"Downloaded atlas sprite resolves: "+name);}
            Check(requests.Count==2&&requests.TrueForAll(request=>request.result==UnityWebRequest.Result.Success&&request.downloadedBytes>0)&&runtime.Downloads.Count==0&&runtime.Operations.Count==0,"Only manifest and firstpack are downloaded; alias loads issue no extra requests");Finish(null);
        }
        static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static IEnumerator Guard(IEnumerator routine){while(true){object value;try{if(!routine.MoveNext())yield break;value=routine.Current;}catch(Exception ex){Finish(ex);yield break;}yield return value;}}
        static void Poll(){if(finished)return;try{scheduler?.Update(0,0);if(Time.realtimeSinceStartup-began>45)throw new TimeoutException("Firstpack startup timeout");}catch(Exception ex){Finish(ex);}}
        static void Finish(Exception error){if(finished)return;finished=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/firstpack-native-startup-validation.json"),JsonUtility.ToJson(report,true));if(manager!=null)manager.enabled=false;foreach(var request in requests)request.Dispose();if(runtime!=null)foreach(var value in runtime.Loaded.Values)if(value.Bundle!=null)value.Bundle.Unload(false);if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);}
    }
}
