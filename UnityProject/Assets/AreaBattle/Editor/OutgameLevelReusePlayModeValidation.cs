using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLevelReusePlayModeValidation
    {
        const string Pending="AreaBattle.LevelReuseNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Original native obstacle instance/pool across reset and immediate reentry; native EffectCellection children/root deferred destruction. Tower/Soldier and EffectModule callbacks are explicit fixture services; full battle/Main or real effect-module behavior is not claimed.";public List<string> checks=new List<string>();}
        sealed class ProfileEffects:IOutgameLevelEffects{public void SetStatistic(int id,long value){}public void SaveLocalData(){}}
        sealed class EffectModule:IOutgameCollectionEffectModule
        {
            public readonly List<int> Closed=new List<int>();public void Close(int id)=>Closed.Add(id);
            public IOutgameCollectionEffect Get(int id)=>throw new Exception("Unexpected Display");public int Show(int id,Vector3 p,Transform t,int sortingLayer,int option)=>throw new Exception("Unexpected Spawn");
        }
        sealed class Soldier:IOutgameLevelSoldier
        {
            public int Clears;public OutgameEffectCollection Collection;public bool Active=>false;public bool SourceFlag112=>true;public OutgameEffectCollection Effects=>Collection;
            public void Clear(bool option){if(!option)throw new Exception("Expected source true");Clears++;}
        }
        static Report report;static bool stopped;static int phase;static double began;static OutgameControllerRegistry registry;static OutgameLevelControl level;static OutgameLevelConfigAsset asset;static OutgameLoadPrefabControl loader;static OutgameLevelRuntimeState state;static OutgameEffectCollection effects;static EffectModule module;static Soldier soldier;static GameObject obstacle,effectRoot,first,second;static int lineField;
        static OutgameLevelReusePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange change){if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string text){if(!yes)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=EditorApplication.timeSinceStartup;
            try{
                registry=new OutgameControllerRegistry();state=new OutgameLevelRuntimeState{PlayState=6};var config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Debug.LogError),new OutgameConfigGlobalValues(),null,null,null,null,null);var read=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),Debug.LogError);read.ReadTable(config.dicLevel);read.ReadTable(config.dicEntityModel);
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},a=>new OutgameLegacyAssetLoader(l=>throw new Exception("unexpected request"),n=>{}),Debug.LogError);var helper=new OutgameResLoadHelper(()=>false,()=>scheduler,Debug.LogError);loader=new OutgameLoadPrefabControl(()=>config,registry,helper,new OutgamePrefabCache(n=>Resources.Load<OutgameLevelEditorConfig>(n)),Debug.LogError);loader.Cache.Prefabs[81]=Resources.Load<GameObject>("Recovered/Obstacles/Entity_81");
                var resource=new OutgameLevelResourceState(()=>config,new OutgameLevelProgression(new OutgameProfile{levelID=1},new ProfileEffects()),helper,()=>loader,()=>0,b=>{},Debug.Log);asset=ScriptableObject.CreateInstance<OutgameLevelConfigAsset>();asset.ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=81,scale=new IntVector3{x=100,y=100,z=100}}};resource.Loaded=Tuple.Create(1,0,asset);
                OutgameCoreControllerBindings.BindLevel(registry,state,resource,()=>1,()=>6,()=>config,()=>false,()=>new OutgameMessageDispatcher(),(s,b)=>state.PlayState=s,()=>loader,Debug.Log);level=(OutgameLevelControl)registry.Resolve(4107);level.OnInit();level.InitializeObstacles();obstacle=level.ObstacleObjects[0];
                Check(obstacle&&obstacle.activeSelf&&loader.Cache.Entities[81].Count==1,"Source level initialization creates one pooled original Entity81 instance");module=new EffectModule();effects=new OutgameEffectCollection(()=>module);effects.SetEntity(obstacle.transform);effectRoot=effects.Root.gameObject;first=new GameObject("first-effect-child");second=new GameObject("second-effect-child");first.transform.SetParent(effects.Root,false);second.transform.SetParent(effects.Root,false);effects.Add(1,11);effects.Add(2,22);soldier=new Soldier{Collection=effects};level.Soldiers.Add(soldier);var camp=new OutgameCampInfo();camp.Init(0,1,Color.blue);level.Camps.Add(camp);level.HaveCheckedGuide=level.HaveCheckedSerialActivity=level.HaveCheckedFailReward=true;lineField=99;
                level.InitGameData(v=>lineField=v);
                Check(!obstacle.activeSelf&&level.ObstacleObjects.Count==0&&loader.Cache.Entities[81].Count==1&&loader.Cache.Prefabs.ContainsKey(81),"Reset hides native obstacle and clears level list while preserving pool/template");
                Check(soldier.Clears==1&&string.Join(",",module.Closed)=="11,22"&&effects.Handles.Count==0&&effects.Root==null&&effects.Entity==obstacle.transform,"All-soldier reset closes registered effects and clears native root reference, retaining entity");
                Check(effectRoot&&first&&second,"Effect children and collection root survive until native frame boundary");
                Check(!camp.IsActive&&level.Camps.Count==1&&level.Soldiers.Count==1&&lineField==0&&state.PlayState==6&&!level.HaveCheckedGuide&&!level.HaveCheckedSerialActivity&&!level.HaveCheckedFailReward,"Reset retains source object pools/playstate and clears only source check flags plus line field168");
                level.InitializeObstacles();Check(level.ObstacleObjects[0]==obstacle&&obstacle.activeSelf&&loader.Cache.Entities[81].Count==1,"Immediate reentry reuses the same native obstacle without instantiating a second entity");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>20)throw new TimeoutException("Native reset/reentry timed out");
                if(phase==0){if(effectRoot||first||second)return;Check(obstacle&&obstacle.activeSelf&&asset&&effects.Entity==obstacle.transform,"Frame boundary removes only scheduled effect subtree; reused obstacle and level asset survive");level.InitGameData(v=>lineField=v);Check(soldier.Clears==2&&module.Closed.Count==2&&effects.Root==null&&!obstacle.activeSelf,"Second reset still clears inactive soldier, with no fabricated close for empty native effect root");effects.SetEntity(obstacle.transform);effectRoot=effects.Root.gameObject;Check(effectRoot&&effectRoot.transform.parent==obstacle.transform&&effectRoot.transform.localScale==Vector3.one,"After deferred destruction source SetEntity recreates a correctly parented collection root");effects.Destory();phase=1;return;}
                if(effectRoot)return;Check(obstacle&&asset&&effects.Root==null,"Final effect cleanup does not destroy retained pooled entity or configuration");Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/level-reuse-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
