using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameAudioPlaybackValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAudioResourcePlayModeValidation
    {
        const string Pending="AreaBattle.AudioResourceNative",BundleName="audio/once/ui/button_normal.mp3.unity3d",ClipPath="Audio/once/ui/button_normal.mp3";
        static string Folder=>Path.Combine(BattleBuild.Workspace,"analysis/audio-resource-native-bundles");
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Original audio2001 rebuilt into a native Windows bundle. Actual first-pack branch uses an explicit test mapping, not an assertion of original firstpack membership. Legacy file transport, AssetBundleRequest, reference release, UpdateManager, AudioManager and native one-shot/reentry are actual. Config is read from rebuilt original firstpack. Modern module acquisition and full Main/Player remain pending.";public List<string> checks=new List<string>();}
        sealed class Locations:IOutgameLegacyBundleServices{public OutgameLegacyBundleLocation GetAssetBundleInfo(string n)=>new OutgameLegacyBundleLocation{BundleName=n,LocalPath=new Uri(Path.Combine(Folder,n)).AbsoluteUri};}
        static Report report;static AssetBundle configBundle,manifestBundle;static OutgameLegacyBundleRuntime runtime;static OutgameAudioResources resources;static OutgameAssetbundleAsyncLoader loader;static Fixture fixture;static OutgameAudioManager manager;static OutgameAudioAction action;static OutgameUpdateManager updates;static GameObject camera;static Task<AudioClip> pending;static AudioClip clip;static List<string> logs;static int phase,ended;static bool stopped;static double began;
        static OutgameAudioResourcePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Batch only");Directory.CreateDirectory(Folder);
            var manifest=BuildPipeline.BuildAssetBundles(Folder,new[]{new AssetBundleBuild{assetBundleName=BundleName,assetNames=new[]{"Assets/AreaBattle/Resources/Recovered/Audio/2001.wav"},addressableNames=new[]{"assets/audio/once/ui/button_normal.mp3"}}},BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);
            if(!manifest)throw new Exception("Native audio test bundle build failed");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static bool Legacy(){return true;}
        static void Start()
        {
            report=new Report();logs=new List<string>();phase=ended=0;stopped=false;began=EditorApplication.timeSinceStartup;
            try{
                var locations=new Locations();runtime=new OutgameLegacyBundleRuntime(()=>locations,logs.Add,logs.Add,logs.Add,ex=>throw ex);runtime.UnloadInterval=0;
                manifestBundle=AssetBundle.LoadFromFile(Path.Combine(Folder,Path.GetFileName(Folder)));runtime.Manifest=manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                OutgameLegacyBundleManager.SharedRuntime=runtime;OutgameLegacyBundleManager.Initialize();
                loader=new OutgameAssetbundleAsyncLoader(Legacy,(b,a,t)=>runtime.LoadAssetAsync(b,a,t,Legacy),a=>logs.Add((string)a[0]));
                var packs=new Dictionary<string,AssetBundle>();resources=new OutgameAudioResources(()=>false,()=>packs,p=>throw new Exception("Unexpected modern route"),loader.LoadAsset<AudioClip>,runtime.Unload,logs.Add,a=>logs.Add((string)a[0]));
                var first=AssetBundle.LoadFromFile(Path.Combine(Folder,BundleName));packs.Add(BundleName,first);var direct=resources.LoadAudioClip(ClipPath);Check(direct.IsCompletedSuccessfully&&direct.Result&&direct.Result.length>0,"First-pack test mapping uses real synchronous AssetBundle.LoadAsset<AudioClip> basename route");
                Check(runtime.Loaded.Count==0&&runtime.Operations.Count==0&&logs.Contains("触发firstpackLoad:+"+BundleName),"First-pack branch bypasses async manager acquisition");first.Unload(true);packs.Clear();
                configBundle=AssetBundle.LoadFromFile(Path.Combine(OutgameFirstPackBundleBuild.Folder,"firstpack.unity3d"));Check(configBundle,"Rebuilt original firstpack configuration bundle loads natively");
                fixture=new Fixture(true);resources.Bind(fixture.Services);OutgameUpdateManager.BindAudio(fixture.Services);updates=OutgameUpdateManager.Instance;
                fixture.Services.Realtime=()=>Time.realtimeSinceStartup;var reader=new OutgameAudioConfigReader(n=>configBundle.LoadAsset<TextAsset>(n),()=>configBundle!=null,()=>false,a=>throw new Exception((string)a[0]));camera=new GameObject("AudioResourceCamera",typeof(Camera));
                manager=new OutgameAudioManager(new OutgameAudioManagerServices{Actions=fixture.Services,HasConfigResource=()=>configBundle!=null,UseNewResources=()=>false,ReadAudioConfig=reader.ReadAudio,ReadOnceConfig=reader.ReadOnce,UiCamera=()=>camera.GetComponent<Camera>()});manager.InitAudioManager();
                Check(manager.AudioData.Count==55&&manager.OnceDurations.Count==53,"AudioManager reads actual original55+53 configs directly from native AssetBundle");fixture.Messages.AddListener("AudioPlayEnd",a=>ended++);
                var errorOperation=new OutgameLegacyAssetOperation("b","a",typeof(AudioClip),(string n,out string e,out int m)=>{e="load-error";m=0;return null;},s=>throw new InvalidOperationException("native-await-error"));errorOperation.Update();
                var failure=OutgameUnityAwait.Await(errorOperation);Check(failure.IsCompleted,"IEnumerator await catches synchronous MoveNext failure without hanging");bool propagated=false;try{failure.GetResult();}catch(InvalidOperationException e){propagated=e.Message=="native-await-error";}Check(propagated,"Native await retains original asset operation exception");
                pending=resources.LoadAudioClip(ClipPath);Check(!pending.IsCompleted&&runtime.Downloads.Count==1&&runtime.Operations.Count==1,"Legacy route starts actual file UnityWebRequest and registered native asset operation");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Play(){manager.UiSlot.Instance.Play(1,2001);}
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-began>40)throw new TimeoutException("Native audio resource phase "+phase);
                if(phase==0){if(!pending.IsCompleted)return;clip=pending.GetAwaiter().GetResult();Check(clip&&runtime.Loaded[BundleName].ReferenceCount==1&&runtime.Downloads.Count==0,"Actual native request produces original button clip with one bundle reference");Check(runtime.Operations.Count==0,"Asset operation is detached from manager while await reaches completion");Play();phase=1;return;}
                if(phase==1){var node=(OutgameAudioParallel)manager.UiSlot.Instance.ParallelNode;if(node.Actions.Count==0)return;foreach(var item in node.Actions.Values)action=(OutgameAudioAction)item;Check(action is OutgameAudioOnceAction&&action.AudioSource&&action.AudioSource.clip==clip&&action.AudioSource.isPlaying,"Original2001 plays through manager, resource helper, actual AssetBundleRequest and native source");Check(runtime.Loaded[BundleName].ReferenceCount==2,"Second load retains shared bundle ownership");phase=2;return;}
                if(phase==2){if(ended!=1||action.AudioSource)return;Check(runtime.Loaded[BundleName].ReferenceCount==1&&runtime.PendingUnload.Count==0,"Natural one-shot completion releases exactly its bundle reference");pending=loader.LoadAsset<AudioClip>(BundleName,"missing-audio");phase=3;return;}
                if(phase==3){if(!pending.IsCompleted)return;Check(pending.GetAwaiter().GetResult()==null&&logs.Contains("加载资源失败:"+BundleName+" AssName:missing-audio"),"Missing native asset returns null with source diagnostic after await");resources.UnloadAudioClip(ClipPath);resources.UnloadAudioClip(ClipPath);Check(runtime.Loaded[BundleName].ReferenceCount==0&&runtime.PendingUnload.Count==1,"Explicit owners release to zero and queue source delayed unload");phase=4;return;}
                if(phase==4){if(runtime.Loaded.Count!=0||clip)return;Check(runtime.PendingUnload.Count==0&&!clip,"Native delayed unload destroys original loaded clip");pending=resources.LoadAudioClip(ClipPath);Check(!pending.IsCompleted&&runtime.Downloads.Count==1,"Same original path starts a fresh download after release");phase=5;return;}
                if(phase==5){if(!pending.IsCompleted)return;clip=pending.GetAwaiter().GetResult();Check(clip&&runtime.Loaded[BundleName].ReferenceCount==1,"Reentry obtains fresh native clip and reference");resources.UnloadAudioClip(ClipPath);manager.Destroy();phase=6;return;}
                if(phase==6){if(runtime.Loaded.Count!=0||updates.HandleList.Count!=0)return;Check(!clip&&manager.Nodes.Count==0&&!manager.Initialized,"Reentry cleanup leaves no clip, action update or manager node ownership");Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();
            if(configBundle)configBundle.Unload(true);if(manifestBundle)manifestBundle.Unload(true);if(OutgameLegacyBundleManager.ManagerObject)UnityEngine.Object.Destroy(OutgameLegacyBundleManager.ManagerObject);if(updates)UnityEngine.Object.Destroy(updates.gameObject);if(camera)UnityEngine.Object.Destroy(camera);
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/audio-resource-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);
        }
    }
}
