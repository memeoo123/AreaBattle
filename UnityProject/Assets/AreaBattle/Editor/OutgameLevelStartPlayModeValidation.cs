using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLevelStartPlayModeValidation
    {
        const string Pending="AreaBattle.LevelStartNative";
        [Serializable] sealed class Report {public bool passed;public string error;public string scope="Native Unity coroutine scheduling for source4106 two scaled0.2second waits, native obstacle prefab acquisition/transform and actual LevelControl registry/lifecycle. WayLine/AI effects are explicitly recorded fixture services; full Level/Player/Main and tower resource graph remain unverified.";public List<string> checks=new List<string>();public List<float> scaledTimes=new List<float>();}
        sealed class Effects:IOutgameLevelEffects{public void SetStatistic(int id,long count){}public void SaveLocalData(){}}
        sealed class Lines:IOutgameLevelWayLines {public void Clear()=>Record("clear");public void InitPrePrefabLoad()=>Record("prefabs");public void InitStarLine()=>Record("lines");}
        static readonly List<string> calls=new List<string>();static Report report;static OutgameUiAnimation runner;static OutgameLevelControl level;static OutgameControllerRegistry registry;static OutgameLevelConfigAsset asset;static OutgameLoadPrefabControl loader;static OutgameLevelRuntimeState state;static GameObject obstacle,root;static bool stopped;static int phase;static double began,pausedAt;
        static OutgameLevelStartPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange change){if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string message){if(!yes)throw new Exception(message);report.checks.Add(message);}
        static void Record(string label){calls.Add(label);report.scaledTimes.Add(Time.time);}
        static void Start()
        {
            report=new Report();calls.Clear();stopped=false;phase=0;began=EditorApplication.timeSinceStartup;
            try{
                registry=new OutgameControllerRegistry();state=new OutgameLevelRuntimeState();var config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Debug.LogError),new OutgameConfigGlobalValues(),null,null,null,null,null);
                var read=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),Debug.LogError);read.ReadTable(config.dicLevel);read.ReadTable(config.dicEntityModel);
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},a=>new OutgameLegacyAssetLoader(l=>throw new Exception("Unexpected missing native obstacle template"),n=>{}),Debug.LogError);var helper=new OutgameResLoadHelper(()=>false,()=>scheduler,Debug.LogError);
                loader=new OutgameLoadPrefabControl(()=>config,registry,helper,new OutgamePrefabCache(n=>Resources.Load<OutgameLevelEditorConfig>(n)),Debug.LogError);loader.Cache.Prefabs[81]=Resources.Load<GameObject>("Recovered/Obstacles/Entity_81");
                var resources=new OutgameLevelResourceState(()=>config,new OutgameLevelProgression(new OutgameProfile{levelID=1},new Effects()),helper,()=>loader,()=>0,b=>{},Debug.Log);
                asset=ScriptableObject.CreateInstance<OutgameLevelConfigAsset>();asset.ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=81,pos=new IntVector3{x=150,y=10,z=-100},angle=new IntVector3{y=9000},scale=new IntVector3{x=100,y=200,z=100}}};resources.Loaded=Tuple.Create(1,0,asset);
                OutgameCoreControllerBindings.BindLevel(registry,state,resources,()=>1,()=>6,()=>config,()=>false,()=>new OutgameMessageDispatcher(),(s,b)=>state.PlayState=s,()=>loader,Debug.Log);level=(OutgameLevelControl)registry.Resolve(4107);level.OnInit();
                Check(level.PlayerCampId==1&&resources.MaxLevel==560&&registry.HasInstance(4107),"Concrete LevelControl factory initializes original global camp and561-row config maximum560");
                level.InitializeObstacles();obstacle=level.ObstacleObjects[0];root=loader.GetEntityRoot(81).gameObject;
                Check(obstacle.transform.position==new Vector3(1.5f,.1f,-1)&&obstacle.transform.localScale==new Vector3(1,2,1)&&Mathf.Abs(obstacle.transform.eulerAngles.y-90)<.001f&&obstacle.GetComponentInChildren<MeshFilter>().sharedMesh,"Actual source obstacle prefab gets native world position/rotation and local scale through level initialization");
                var lines=new Lines();var prepared=new OutgameLevelPreparedStart(level,null,null,null,null,null,null,null,()=>lines,()=>Record("ai"));runner=new GameObject("source-level-coroutine-runner").AddComponent<OutgameUiAnimation>();Time.timeScale=0;runner.StartCoroutine(prepared.InitializeLines());pausedAt=EditorApplication.timeSinceStartup;
                Check(string.Join(",",calls)=="clear","Native StartCoroutine executes Clear before first yield even while scaled time is paused");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                double now=EditorApplication.timeSinceStartup;if(now-began>20)throw new TimeoutException("Native level-start coroutine timed out");
                if(phase==0){if(now-pausedAt<.2)return;Check(calls.Count==1,"First WaitForSeconds does not advance during0.2 realtime seconds at timeScale0");Time.timeScale=1;phase=1;return;}
                if(phase==1){if(calls.Count<3)return;Check(string.Join(",",calls)=="clear,prefabs,lines"&&report.scaledTimes[1]-report.scaledTimes[0]>=.19f,"First scaled0.2second wait precedes prefab and star-line initialization");Time.timeScale=0;pausedAt=now;phase=2;return;}
                if(phase==2){if(now-pausedAt<.2)return;Check(calls.Count==3,"Second WaitForSeconds remains paused before AI initialization");Time.timeScale=1;phase=3;return;}
                if(phase==3){if(calls.Count<4)return;Check(string.Join(",",calls)=="clear,prefabs,lines,ai"&&report.scaledTimes[3]-report.scaledTimes[2]>=.19f,"AI initialization follows the second native scaled0.2second wait");state.PlayState=6;level.OnDispose();Check(state.PlayState==0&&!registry.HasInstance(4107)&&obstacle&&level.ObstacleObjects.Count==1&&asset,"Level disposal clears shared state/registry while preserving source-owned native objects and config");UnityEngine.Object.Destroy(root);Check(root&&obstacle,"Native pooled obstacle root destruction is deferred");phase=4;return;}
                if(root||obstacle)return;Check(loader.EntityRoot&&asset,"Frame boundary destroys scheduled obstacle subtree and keeps main entity root/config");Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;Time.timeScale=1;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/level-start-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
