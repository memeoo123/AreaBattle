using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameAudioPlaybackValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAudioManagerPlayModeValidation
    {
        const string Pending="AreaBattle.AudioManagerNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Actual source AudioManager/55row AudioConfig/53row OnceAudioConfig/root/listener/node ownership and UI/controller/action factory, native UpdateManager, source TimeModule countdown. Resource clip transport and config TextAsset acquisition use local fixture adapters; full Main/platform/Player not claimed.";public List<string> checks=new List<string>();}
        static Report report;static Fixture fixture;static OutgameAudioManager manager;static OutgameAudioManagerSlot slot;static OutgameUiAudioManager ui;static OutgameAudioControl control;static OutgameTimeModule time;static OutgameUpdateManager updates;
        static OutgameAudioAction action;static OutgameAudioParallel node;static AudioSource source;static GameObject cameraRoot,oldRoot,oldListener;static bool stopped;static int phase,started,ended;static double began;
        static OutgameAudioManagerPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;phase=started=ended=0;began=EditorApplication.timeSinceStartup;
            try{
                fixture=new Fixture(true);fixture.Services.Realtime=()=>Time.realtimeSinceStartup;OutgameUpdateManager.BindAudio(fixture.Services);updates=OutgameUpdateManager.Instance;
                time=new OutgameTimeModule(()=>fixture.Messages);time.Initialize();time.Start();time.BindAudio(fixture.Services);
                cameraRoot=new GameObject("OriginalUiCameraProbe",typeof(Camera));cameraRoot.transform.position=new Vector3(3,4,5);
                var reader=new OutgameAudioConfigReader(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),()=>true,()=>fixture.NewResources,a=>throw new Exception((string)a[0]));
                var services=new OutgameAudioManagerServices{Actions=fixture.Services,HasConfigResource=()=>true,UseNewResources=()=>fixture.NewResources,ReadAudioConfig=reader.ReadAudio,ReadOnceConfig=reader.ReadOnce,UiCamera=()=>cameraRoot.GetComponent<Camera>()};
                slot=new OutgameAudioManagerSlot(()=>services);manager=slot.Instance;manager.InitAudioManager();ui=manager.UiSlot.Instance;
                Check(manager.Initialized&&manager.AudioData.Count==55&&manager.OnceDurations.Count==53&&manager.UseOnceAudio,"Native manager initialized from all original55 audio rows and53 OnceAudio rows");
                Check(manager.Root&&manager.Root.name=="AudioRoot"&&manager.Root.scene.name=="DontDestroyOnLoad","Source AudioRoot is a real persistent native root");
                Check(manager.Listener&&manager.Listener.transform.parent==cameraRoot.transform&&manager.Listener.transform.position==Vector3.zero,"Source AudioListener created under UI camera preserving world position");
                fixture.Messages.AddListener("AudioPlayStart",a=>started++);fixture.Messages.AddListener("AudioPlayEnd",a=>ended++);control=new OutgameAudioControl(null,()=>fixture.Settings,()=>manager.UiSlot.Instance);
                fixture.NewResources=true;fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/1001");fixture.Provider.AssetObject=fixture.Clip;control.OnGamePlayState(2);Capture();
                Check(action is OutgameAudioLoopFadeAction&&source&&source.loop&&source.clip==fixture.Clip&&source.volume==0&&started==1,"Source1001 Ptype4 resolves via manager config/node/factory into original native loop-fade");
                Check(manager.Nodes.Count==1&&ReferenceEquals(node,manager.Nodes[node.NodeId])&&node.Root.transform.parent==manager.Root.transform&&node.Root.name=="parallel_"+node.NodeId,"UI lazy parallel node is created and tracked by actual manager");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Capture(){node=(OutgameAudioParallel)manager.UiSlot.Instance.ParallelNode;foreach(var a in node.Actions.Values)action=(OutgameAudioAction)a;source=action.AudioSource;}
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-began>30)throw new TimeoutException("AudioManager native phase"+phase);time.Update();
                if(phase==0){if(source.time<.15f||time.CompleteHandles.Count!=0)return;Check(source.isPlaying&&Mathf.Abs(source.volume-.4f)<.0001f&&updates.IndexDict.ContainsKey(action.UpdateId),"Native source timer completes fade-in to original .4 volume while actual UpdateManager owns action");control.IsMusicEffect=false;Check(source.volume==0,"Controller settings event reaches manager-configured live action");control.IsMusicEffect=true;control.OnGamePlayState(8);Check(((OutgameAudioLoopFadeAction)action).Stopping&&node.Actions.Count==1,"Controller state8 retains manager node while source fade-out runs");phase=1;return;}
                if(phase==1){if(ended<1||action.AudioSource||updates.HandleList.Count!=0)return;Check(node.Actions.Count==0&&fixture.Provider.Releases==1&&time.CompleteHandles.Count==0,"Fade ends, releases actual handle and deferred shared update registration");fixture.NewResources=false;fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/1002");control.OnGamePlayState(5);Capture();Check(action is OutgameAudioLoopAction&&source&&source.loop&&Mathf.Abs(source.volume-.5f)<.0001f&&source.clip==fixture.Clip&&started==2,"State5 uses original1002 Ptype2 and .5 volume through same manager node");phase=2;return;}
                if(phase==2){if(source.time<.1f)return;var retainedNode=node;control.OnGamePlayState(3);Check(ended==2&&ui.ParallelNode==null&&manager.Nodes.ContainsKey(retainedNode.NodeId),"State3 clears UI parallel reference while AudioManager retains stopped node registration");fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/2001");ui.Play(1,2001);Capture();Check(action is OutgameAudioOnceAction&&source&&!source.loop&&source.clip==fixture.Clip&&started==3&&manager.Nodes.Count==2,"Original2001 Ptype1 creates ordinary one-shot under newly registered parallel node");phase=3;return;}
                if(phase==3){if(ended<3||updates.HandleList.Count!=0)return;Check(!action.AudioSource&&node.Actions.Count==0&&manager.GetOnceAudioDuration("Audio/Once/ui/button_normal.mp3")==.12f,"Native one-shot naturally finishes via actual Update; original once duration lookup retained");Check(fixture.Calls.Contains("new:Audio/Loop/hcrzd_GameBGM.wav")&&fixture.Calls.Contains("legacy:Audio/Loop/hcrzd_MainBGM.wav")&&fixture.Calls.Contains("legacy:Audio/once/ui/button_normal.mp3"),"Manager passes all original configured paths to explicit clip transport boundary");oldRoot=manager.Root;oldListener=manager.Listener.gameObject;manager.Destroy();Check(manager.Nodes.Count==0&&manager.AudioData.Count==0&&!manager.Initialized&&manager.Root==null&&manager.Listener==null&&manager.OnceDurations.Count==53,"Destroy clears node/config ownership and references while retaining OnceAudio cache");Check(ReferenceEquals(slot.Instance,manager)&&!ReferenceEquals(ui,manager.UiSlot.Instance),"Destroy retains source manager singleton but resets UI audio singleton");phase=4;return;}
                if(phase==4){if(oldRoot||oldListener)return;manager.InitAudioManager();Check(manager.Initialized&&manager.AudioData.Count==55&&manager.Root&&manager.Listener&&manager.Nodes.Count==0,"Destroyed roots disappear natively; same manager reinitializes original config and fresh root/listener");Check(manager.UiSlot.Instance.ParallelNode==null&&updates.HandleList.Count==0,"Reentry begins with fresh UI node ownership and no stale action update handles");manager.Destroy();time.Shutdown();UnityEngine.Object.Destroy(updates.gameObject);UnityEngine.Object.Destroy(cameraRoot);Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/audio-manager-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
