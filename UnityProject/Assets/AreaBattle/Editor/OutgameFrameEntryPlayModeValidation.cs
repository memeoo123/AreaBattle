using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public sealed class OutgameFrameEntryNativeDriver:MonoBehaviour
    {
        public OutgameFrameEntry Frame;
        void Update(){Frame?.Update(Time.deltaTime,Time.unscaledDeltaTime);}
    }
    [InitializeOnLoad] public static class OutgameFrameEntryPlayModeValidation
    {
        const string Pending="AreaBattle.FrameEntryNative";
        public sealed class Platform:IOutgameControllerPlatform
        {
            public long Now=OutgameTimeToRefreshValidation.Stamp(new DateTime(2026,10,3,12,0,0));
            public int GetAppChannelId()=>0;public void SetAppChannelId(int value){}public bool IsInstallVersion=>false;
            public long GetServerTimeByServerTimeZone()=>Now;public int GetNetworkingState()=>0;
            public DateTime LocalNow=>new DateTime(2026,10,3,12,0,0);public bool IsReleaseVersion=>true;
        }
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Concrete FrameEntry core with actual Logic/FSM/Procedure/Time, production StatisticsRuntime/singleton updates/refresh/common messages, real pool release before disposable callbacks and compressed file reload. SDK/account/download/permission/config reset host inputs are explicit fixtures; complete Main/activity/platform/Player not claimed.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameFrameEntry frame;static OutgameStatisticsRuntime runtime;static OutgameDataManagerPool pool;
        static OutgameStatisticsOffNetValidation.Host host;static OutgameSdkStringStorage storage;static OutgameMessageDispatcher messages;static Platform platform;
        static OutgameTimeToRefreshControl refresh;static OutgameFrameEntryNativeDriver driver;static OutgameStatisticsControl owner;
        static int phase,atFrame,downloads,completed;static double started,phaseAt;static bool stopped,configReady;static string path,saved;static long first;static List<string> trace;
        static OutgameFrameEntryPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int next){phase=next;atFrame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Build(bool server)
        {
            trace=new List<string>();configReady=true;messages=new OutgameMessageDispatcher();platform=new Platform();host=new OutgameStatisticsOffNetValidation.Host{Server=server};owner=new OutgameStatisticsControl();
            pool=new OutgameDataManagerPool(()=>trace.Add("pool-init"),s=>{},s=>{},s=>{});
            var modules=new Dictionary<Type,IOutgameFrameModule>();
            frame=new OutgameFrameEntry(new OutgameFrameServices{CreateModule=t=>modules[t],SetCulture=()=>trace.Add("culture"),ShutdownInput=()=>{trace.Add("input");OutgameInputManager.Instance.Shutdown();},
                ClearPermissions=()=>trace.Add("permissions"),SetConfigReadInitialized=b=>{configReady=b;trace.Add("config");},Log=s=>{},Warning=s=>{},SuppressResourceUpdate=()=>false,Resources=()=>throw new Exception("unexpected resource-only update")});
            var logic=new OutgameLogicModule(b=>{},()=>pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>()),()=>{trace.Add("pool-release");pool.OnRelease();},s=>{},s=>{});
            var fsm=new OutgameFsmManager();var procedure=new OutgameProcedureManager(()=>fsm);var time=new OutgameTimeModule(()=>messages,a=>{},a=>{});
            modules.Add(typeof(OutgameLogicModule),logic);modules.Add(typeof(OutgameFsmManager),fsm);modules.Add(typeof(OutgameProcedureManager),procedure);modules.Add(typeof(OutgameTimeModule),time);
            foreach(var type in new[]{typeof(OutgameLogicModule),typeof(OutgameFsmManager),typeof(OutgameProcedureManager),typeof(OutgameTimeModule)})frame.Initialize(frame.GetModule(type),null);
            logic.InitCtrl(false);pool.SourceReadyFlag=false;frame.Start();
            storage=new OutgameSdkStringStorage(new OutgameFileStorageBackend(path,a=>a()),s=>throw new Exception(s));
            runtime=new OutgameStatisticsRuntime(()=>pool,frame,platform,host,storage,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}),s=>downloads++,
                ()=>messages,()=>false,()=>throw new Exception("unexpected HTTP request"),a=>{},a=>{},s=>{},()=>owner);
            runtime.Expansion.RegisteredValueFunc(12345,a=>77);runtime.Initialize(()=>{completed++;trace.Add("statistics-complete");});
            driver=new GameObject("frame-entry-native-driver").AddComponent<OutgameFrameEntryNativeDriver>();driver.Frame=frame;
        }
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;downloads=completed=0;path=Path.Combine(Path.GetTempPath(),"AreaBattleFrameRuntime-"+Guid.NewGuid().ToString("N"));
            try{Time.timeScale=0;Build(true);Phase(0);EditorApplication.update+=Poll;}catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("frame native phase "+phase);
                if(Time.frameCount<=atFrame+2||EditorApplication.timeSinceStartup-phaseAt<.2)return;
                if(phase==0)
                {
                    Check(downloads==1&&completed==0&&runtime.Manager.Data==null,"Runtime preserves real pending server-data gate");
                    Check(frame.Modules.Count==4&&frame.DisposableActions.GetInvocationList().Length==1,"Real recovered modules share frame while statistics registers separate disposable");
                    runtime.Manager.UpdateDataCallBack("");first=runtime.Manager.Data.firstStartTimeStamp;
                    Check(completed==1&&runtime.Expansion.GameValue(12345)==77&&runtime.Control.ValueProviders.Count==20,"Runtime retains pre-init provider and loads nineteen source statistics providers");
                    Phase(1);return;
                }
                if(phase==1)
                {
                    Check(OutgameTimeToRefreshControl.Groups.Count==0&&runtime.Manager.GetEventStatistics(10902)==0,"Concrete runtime coroutine waits for the same pool readiness");pool.SourceReadyFlag=true;Phase(2);return;
                }
                if(phase==2)
                {
                    if(runtime.Manager.GetEventStatistics(10902)!=1||owner.IsDirty)return;
                    refresh=OutgameTimeToRefreshControl.Instance;Check(frame.DisposableActions.GetInvocationList().Length==2&&OutgameTimeToRefreshControl.Groups.Count==1,"Statistics and actual refresh singleton share frame disposal field");
                    Time.timeScale=1;Phase(3);return;
                }
                if(phase==3)
                {
                    if(runtime.Manager.GetEventStatistics(10901)!=1||owner.IsDirty)return;
                    Check(runtime.Manager.Data.lastRefreshTimeStamp==platform.Now-12*3600000L,"Runtime native refresh reaches offnet daily reset automatically");
                    messages.SendMessage("GF_AdsPlayCallBack",new object[]{true});runtime.Expansion.AddEventCount(88,5,9);Phase(4);return;
                }
                if(phase==4)
                {
                    if(owner.IsDirty)return;
                    saved=storage.GetString(host.MineGameName+runtime.Manager.DataKey,"");Check(saved.Length>0&&!saved.StartsWith("{"),"Native owner update saves concrete runtime to compressed file");
                    var oldCommon=OutgameCommonMessageDispatcher.Shared;Time.timeScale=0;trace.Clear();frame.Shutdown();
                    Check(string.Join(",",trace)=="pool-release,input,permissions,config"&&!configReady,"Actual logic module releases data pool before frame input/permission/config tail");
                    Check(pool.Managers.Count==0&&!pool.SourceReadyFlag&&runtime.Manager.Data==null&&runtime.Control.ValueProviders==null&&!ReferenceEquals(oldCommon,OutgameCommonMessageDispatcher.Shared),"Actual frame shutdown releases statistics manager then clears control/common singleton");
                    Check(((OutgameStatisticsOffNetStrategy)runtime.Manager.Strategy).RefreshCoroutine==null&&frame.DisposableActions.GetInvocationList().Length==1,"Pool release stops actual coroutine; control self-unsubscribes while scaled refresh cleanup remains");
                    Check(frame.Started&&frame.Modules.Count==0&&OutgameTimeToRefreshControl.Groups.Count==1,"Source frame ready flag remains while paused refresh registry awaits scaled cleanup");Phase(5);return;
                }
                if(phase==5)
                {
                    if(EditorApplication.timeSinceStartup-phaseAt<.5)return;
                    Check(OutgameUpdateManager.Instance.HandleList.Count==0&&OutgameTimeToRefreshControl.Groups.Count==1,"Next native update processes controller removal without prematurely finishing paused await");Time.timeScale=1;Phase(6);return;
                }
                if(phase==6)
                {
                    if(OutgameTimeToRefreshControl.Groups.Count!=0)return;
                    UnityEngine.Object.DestroyImmediate(refresh.gameObject);UnityEngine.Object.DestroyImmediate(driver.gameObject);
                    Check(frame.DisposableActions==null,"Refresh destruction removes last frame disposal subscriber");
                    Time.timeScale=0;Build(false);Check(runtime.Manager.Data.firstStartTimeStamp==first&&runtime.Manager.GetEventStatistics(88,5)==9&&runtime.Manager.GetEventStatistics(10002)==1&&completed==2,"New production runtime graph reloads compressed file after actual frame teardown");
                    pool.SourceReadyFlag=true;Phase(7);return;
                }
                if(phase==7)
                {
                    if(runtime.Manager.GetEventStatistics(10902)!=2||owner.IsDirty)return;
                    Check(runtime.Manager.GetEventStatistics(10901)==1&&frame.DisposableActions.GetInvocationList().Length==2,"Recreated runtime registers fresh launch without immediate duplicate daily reset");
                    frame.Shutdown();Time.timeScale=1;Phase(8);return;
                }
                if(phase==8){if(OutgameTimeToRefreshControl.Groups.Count!=0)return;UnityEngine.Object.DestroyImmediate(OutgameTimeToRefreshControl.Instance.gameObject);UnityEngine.Object.DestroyImmediate(driver.gameObject);Finish(null);}
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/frame-entry-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
