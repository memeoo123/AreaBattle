using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameControllerPlayModeValidation
    {
        const string Pending="AreaBattle.ControllerPlayModeValidation";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Five real controller bindings with native persistent pool root and deferred Destroy; explicit platform and in-memory storage fixtures. No full38-controller Main, real login or Player claim.";
            public List<string> checks=new List<string>();
        }
        sealed class Platform:IOutgameControllerPlatform
        {public int GetAppChannelId()=>1;public void SetAppChannelId(int n){}public bool IsInstallVersion=>true;public long Stamp=123456789;public long GetServerTimeByServerTimeZone()=>Stamp;public int GetNetworkingState()=>1;public DateTime LocalNow=>new DateTime(2026,9,30);public bool IsReleaseVersion=>true;}
        sealed class Backend:IOutgameStorageBackend
        {public string Get(string k)=>null;public void Set(string k,string v,Action<string> f,Action c)=>c();public void Remove(string k,Action<string> f,Action c)=>c();public void Clear(Action<string> f,Action c)=>c();}
        sealed class StorageHost:IOutgameDataStorageHost
        {public int SourceLoginProgress=>0;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>false;public string MineGameName=>"playmode-fixture";public bool HasToast=>false;public void Log(string s){}public void Error(string s){}public void Toast(string s){}public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;public void QueueUpload(string k,string s){throw new Exception("offline fixture");}}
        static Report report;static int phase,atFrame;static double began;
        static OutgameControllerRegistry registry;static OutgameLogicModule logic;static OutgamePrefabPoolControl poolControl;static OutgameServerTimeControl time;static OutgameLocalDataManager local;static OutgameCommanderManager commander;static OutgameMessageDispatcher messages;static Platform platform;static GameObject root,child,prefab;static Scene transient;
        static OutgameControllerPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change){if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report();began=EditorApplication.timeSinceStartup;phase=0;
            try
            {
                registry=new OutgameControllerRegistry();messages=new OutgameMessageDispatcher();platform=new Platform();
                var profile=new OutgameProfile();var host=new StorageHost();var strings=new OutgameSdkStringStorage(new Backend(),Debug.LogError);var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},Debug.LogError);
                var config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Debug.LogError),null,null,null,null,null,null);
                var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),Debug.LogError);reader.ReadTable(config.dicChannel);reader.ReadTable(config.dicChannelProcedure);
                var dataPool=new OutgameDataManagerPool(()=>{},Debug.LogWarning,s=>{},Debug.LogError);
                commander=new OutgameCommanderManager(profile,BattleView.ReadText("Data/Outgame/CommanderConfig"),new OutgameDataManagerStorage(()=>"CommanderManager",host,strings,versions),host,s=>{});
                local=new OutgameLocalDataManager(profile,BattleView.ReadText("Data/AllSkillConfig"),"{\"Datas\":[]}",()=>platform.LocalNow,new OutgameDataManagerStorage(()=>"LocalDataManager",host,strings,versions),host,s=>{},(m,s)=>{});
                OutgameCoreControllerBindings.Bind(registry,()=>dataPool,()=>config,platform,()=>messages,null,(c,s)=>{},Debug.LogWarning,Debug.LogError);
                logic=new OutgameLogicModule(b=>{},()=>dataPool.OnInit(true,"fixture",new[]{new OutgameManagerRegistration(4028,"fixture",false,false,()=>commander),new OutgameManagerRegistration(4119,"fixture",false,false,()=>local)}),()=>{},Debug.LogWarning,s=>{});logic.Initialize();
                foreach(int type in new[]{4561,4034,4027,3903,4118})logic.RegisterLogicCtr(registry.Resolve(type),false);logic.InitCtrl(true);
                poolControl=(OutgamePrefabPoolControl)registry.Resolve(4561);time=(OutgameServerTimeControl)registry.Resolve(4034);root=poolControl.Root;
                Check(ReferenceEquals(((OutgameCommanderControl)registry.Resolve(4027)).Manager,commander)&&ReferenceEquals(((OutgameLocalDataControl)registry.Resolve(4118)).Manager,local),"Production bindings resolve same initialized data-pool managers");
                Check(((OutgameProcessControl)registry.Resolve(3903)).IsBChannel&&time.Initialized,"Process and clock initialized through real logic module");
                Check(root.scene.name=="DontDestroyOnLoad"&&!root.activeSelf,"Default native pool root persists and stays hidden");
                prefab=new GameObject("native-pool-prefab");poolControl.CreatePool(prefab,1);child=poolControl.Spawn(prefab,null);poolControl.Recycle(child);
                transient=SceneManager.CreateScene("controller-fixture-next");SceneManager.SetActiveScene(transient);
                Check(root&&child.transform.parent==root.transform,"Pool survives active-scene transition");
                messages.SendMessage("GamePlayState",new object[]{3});messages.SendMessage("IapSuccess");
                Check(local.Record.jumpData.jumpDayOfYear==time.GetTodayOfYear()&&local.Record.firstChargeData.firstChargeTime==57600000,"Local-data listeners use the bound server-time instance");
                Time.timeScale=0;atFrame=Time.frameCount;EditorApplication.update+=Poll;
            }
            catch(Exception error){Finish(error);}
        }
        static void Poll()
        {
            try
            {
                if(EditorApplication.timeSinceStartup-began>20)throw new TimeoutException("Native controller test timed out");
                if(Time.frameCount<=atFrame)return;
                if(phase==0)
                {
                    platform.Stamp+=86400000;logic.Update(0,1.125f);
                    Check(time.Timestamp==platform.Stamp&&time.PreviousDay==3,"Unscaled logic update works on native paused frame");
                    logic.Shutdown();
                    Check(!registry.HasInstance(4561)&&!registry.HasInstance(4034)&&!registry.HasInstance(4118)&&root!=null,"Shutdown clears slots before native deferred destruction");
                    messages.SendMessage("GamePlayState",new object[]{9});Check(local.Record.jumpData.defeatNum==0,"Disposed local-data controller no longer receives events");
                    atFrame=Time.frameCount;phase=1;return;
                }
                Check(root==null&&child==null,"Native frame destroys persistent root and its pooled child");
                poolControl=(OutgamePrefabPoolControl)registry.Resolve(4561);poolControl.OnInit();Check(poolControl.Root!=null&&poolControl.Root!=root,"Later resolver creates a new usable native pool");poolControl.OnDispose();Finish(null);
            }
            catch(Exception error){Finish(error);}
        }
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/controller-native-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
