using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameFrameLaunchPlayModeValidation
    {
        const string Pending="AreaBattle.FrameLaunchNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual GameFrameWorkMono singleton, Init and Unity SendMessage focus/pause entry points, real Time.timeScale and recovered TimeModule driven by frame Update. Inputs are deliberate test messages; OS focus behavior, actual SDK/transition/GameFrameworkLoad/Main and Player/audiovisual are not claimed.";
            public List<string> checks=new List<string>();
        }
        static Report report;static GameObject root;static OutgameFrameWorkMono mono;static OutgameFrameEntry frame;static OutgameTimeModule time;static OutgameFrameEntryNativeDriver driver;
        static int phase,atFrame,timerId,flushes,sdkChecks;static float flushed;static double started,phaseAt;static bool stopped;
        static OutgameFrameLaunchPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string why){if(!value)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int next){phase=next;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;flushes=sdkChecks=0;Time.timeScale=1;
            try
            {
                var messages=new OutgameMessageDispatcher();OutgameFrameWorkMono.Services=new OutgameFrameMonoServices{Messages=()=>messages,Log=a=>{},ReleaseLog=a=>{},IsReleaseVersion=()=>true,CheckSdkLogSupport=()=>sdkChecks++};
                root=new GameObject("GameFrameWorkMono");var existing=root.AddComponent<OutgameFrameWorkMono>();mono=OutgameFrameWorkMono.Instance;
                Check(!ReferenceEquals(existing,mono)&&root.GetComponents<OutgameFrameWorkMono>().Length==2&&root.scene.name!="DontDestroyOnLoad","Original singleton adds a component to existing root without moving that root to persistent scene");
                Check(!mono.IsInitialized&&mono.HasFocus&&mono.NetworkSamplingEnabled&&mono.DomainData.Datas.Count==0,"Native component retains original constructor defaults and independent domain list");
                frame=new OutgameFrameEntry(new OutgameFrameServices{CreateModule=t=>time,SetCulture=()=>{},ShutdownInput=()=>{},ClearPermissions=()=>{},SetConfigReadInitialized=b=>{},Log=s=>{},Warning=s=>{}});
                frame.PauseGame(true,true);Check(Time.timeScale==0&&!frame.IsPauseGame(),"TimeScale changes even when uninitialized Mono rejects active pause");frame.PauseGame(false,true);
                OutgameFrameWorkMono.Init();Check(mono.IsInitialized&&sdkChecks==1&&OutgameFrameWorkMono.MainThreadId==Thread.CurrentThread.ManagedThreadId,"Init publishes readiness then captures actual Unity thread and checks release SDK log support endpoint");
                time=new OutgameTimeModule(()=>messages,a=>{},a=>{});frame.Initialize(frame.GetModule<OutgameTimeModule>(),null);frame.Start();
                timerId=time.AddLoopTimer(1000f,true,true,true,v=>{flushes++;flushed=v;});
                driver=new GameObject("frame-launch-time-driver").AddComponent<OutgameFrameEntryNativeDriver>();driver.Frame=frame;Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>30)throw new TimeoutException("frame launch phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.35)return;
                if(phase==0)
                {
                    if(!time.LoopTimers.ContainsKey(timerId)||time.LoopTimers[timerId].Elapsed<.1f)return;
                    Check(time.LoopTimers[timerId].Elapsed>0&&OutgameTimeClock.IsFocus,"Actual frame Update advances recovered scaled loop timer");
                    frame.PauseGame(true,true);Check(frame.IsPauseGame()&&Time.timeScale==0&&flushes==1&&flushed>0&&time.LoopTimers[timerId].Elapsed==0,"Active pause sends real source event and TimeModule flushes elapsed before resetting timer");Phase(1);return;
                }
                if(phase==1)
                {
                    Check(time.LoopTimers[timerId].Elapsed==0&&flushes==1,"Native scaled frames stay frozen without repeated pause notification");
                    mono.SendMessage("OnApplicationFocus",false);mono.SendMessage("OnApplicationPause",false);
                    Check(!mono.HasFocus&&!OutgameTimeClock.IsFocus&&frame.IsPauseGame()&&mono.PauseSource==OutgamePauseSource.ActiveTriggerPause,"Unity message dispatch updates real TimeModule focus while lower-priority resume cannot clear active pause");
                    frame.PauseGame(false,true);Check(!frame.IsPauseGame()&&Time.timeScale==1&&!OutgameTimeClock.IsFocus,"Explicit active resume restores timeScale but retains unfocused state");Phase(2);return;
                }
                if(phase==2)
                {
                    Check(time.LoopTimers[timerId].Elapsed==0,"Focus gate keeps actual timer stopped after timeScale resume");mono.SendMessage("OnApplicationFocus",true);Phase(3);return;
                }
                if(phase==3)
                {
                    if(time.LoopTimers[timerId].Elapsed<.1f)return;
                    Check(OutgameTimeClock.IsFocus&&time.LoopTimers[timerId].Elapsed>0,"Focus restoration message resumes native timer accumulation");
                    frame.PauseGame(true,false);Check(Time.timeScale==1&&frame.IsPauseGame()&&flushes==2,"changeTimeScale=false preserves clock while emitting source pause flush");Phase(4);return;
                }
                if(phase==4)
                {
                    Check(time.LoopTimers[timerId].Elapsed>0&&frame.IsPauseGame(),"Source pause-state alone does not freeze scaled timer when timeScale stays1");frame.PauseGame(false,false);
                    frame.Shutdown();bool focus=OutgameTimeClock.IsFocus;mono.SendMessage("OnApplicationFocus",false);Check(OutgameTimeClock.IsFocus==focus,"Actual TimeModule Shutdown unregisters focus callback");
                    UnityEngine.Object.DestroyImmediate(mono);var next=OutgameFrameWorkMono.Instance;Check(!ReferenceEquals(next,mono)&&!next.IsInitialized&&root.GetComponents<OutgameFrameWorkMono>().Length==2,"Destroyed singleton recreates another original-default component on surviving root");
                    UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(driver.gameObject);Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/frame-launch-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
