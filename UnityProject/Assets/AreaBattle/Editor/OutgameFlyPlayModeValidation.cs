using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameFlyPlayModeValidation
    {
        const string Pending="AreaBattle.FlyPlayModeValidation";
        [Serializable] sealed class Report {public bool passed;public string scope="Actual Unity Play Mode; isolated inventory and explicit prefab pool fixture, no user account or platform";public List<string> checks=new List<string>();public string error;public int frames,polls,spawned,returned;public float elapsed,scaledElapsed;}
        sealed class Effects:IOutgameToolEffects
        {
            public int saves;public void RefreshTopInfo(){}public void GoldSpent(int id,long n){}public void ToolChanged(int id){}public void UnlockScene(int id){}public void UnlockSoldier(int id){}
            public bool ApplyItemEntity(int id,long n)=>false;public void MissingItemEntity(int id){}public void ReportGet(int id,int category,int n,int balance,string reason){}public void ReportCost(int id,int category,int n,int balance){}public void Save(){saves++;}
        }
        sealed class RecoveredResources:IOutgameNormalPoolResources
        {
            public bool UseNewResourceLoader=>true;
            public void LoadOriginal(string path,Action<IOutgamePoolAssetHandle> complete)
            {
                var provider=new OutgameAssetProvider(()=>false,Debug.LogWarning){AssetObject=OutgameFlyCurrencyAssets.Load(path)};
                var handle=provider.CreateHandle(path,()=>false);handle.Completed+=loaded=>complete(loaded);
                provider.Status=4;provider.InvokeCompletion();
            }
            public void LoadResource(string path,Action<IOutgamePoolResourceInfo> complete)=>throw new InvalidOperationException("Legacy loader not part of this fixture");
            public void LoadSynchronousPrefab(string path,Action<GameObject> complete)=>throw new InvalidOperationException("Legacy loader not part of this fixture");
            public string GetIntactBundleName(string assetName,string bundleName)=>throw new InvalidOperationException("Legacy loader not part of this fixture");
            public void ReleaseUIForm(GameObject target)=>UnityEngine.Object.Destroy(target);
            public void Warning(string message)=>Debug.LogWarning(message);
        }
        static Report report;static OutgameFlyRuntime runtime;static OutgameLocalInventoryState state;static Effects effects;static Text gold,diamonds;
        static OutgameObjectPool pool;static List<GameObject> icons;static List<Vector3> origins;static int completed,phase,firstFrame;static float began,scaledBegan;
        static OutgameFlyPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch validation only");
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {
            if(change!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Pending,false))return;
            Start();
        }
        static Text MakeText(string name,string value,Vector3 position)
        {var node=new GameObject(name,typeof(RectTransform),typeof(Text));node.transform.position=position;var text=node.GetComponent<Text>();text.text=value;return text;}
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report();pool=new OutgameObjectPool("Fly validation",false,20,float.MaxValue,0);icons=new List<GameObject>();origins=new List<Vector3>();completed=0;phase=0;
            try{
                Time.timeScale=0;firstFrame=Time.frameCount;began=Time.realtimeSinceStartup;scaledBegan=Time.time;
                var root=new GameObject("Isolated Fly Runtime");var poolRoot=new GameObject("Isolated Pool Root").transform;runtime=root.AddComponent<OutgameFlyRuntime>();
                var normal=new OutgameNormalPool(pool,poolRoot,true,new RecoveredResources());
                gold=MakeText("Gold","100",new Vector3(10,5,0));diamonds=MakeText("Diamonds","7",new Vector3(-10,5,0));
                state=new OutgameLocalInventoryState{goldNum=100,diamondsNum=7};var inventory=new OutgameLocalInventory(state,"{\"Datas\":[]}");effects=new Effects();
                var tools=new OutgameToolDispatcher(inventory,effects,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                runtime.Bind(tools,inventory,gold,diamonds,root.transform,(group,id)=>{},()=>2099,
                    (path,ready)=>normal.Spawn(path,icon=>{icons.Add(icon);report.spawned++;ready(icon);origins.Add(icon.transform.position);}),
                    icon=>{normal.Unspawn(icon);report.returned++;},new OutgameFlyScatter((min,max)=>min,Mathf.Sin,Mathf.Cos));
                runtime.FlyMoney(50,root.transform,Vector3.zero,true,()=>completed++,true);
                runtime.FlyDiamonds(20,root.transform,new Vector3(0,-2,0),true,()=>completed++,true);
                Check(state.goldNum==150&&state.diamondsNum==27&&effects.saves==2,"Both rewards saved immediately before animation completion");
                Check(completed==0&&runtime.ActiveEffectCount==2,"Two actual Unity coroutines registered concurrently");
                EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            try{
                report.polls++;report.frames=Time.frameCount-firstFrame;report.elapsed=Time.realtimeSinceStartup-began;report.scaledElapsed=Time.time-scaledBegan;
                if(phase==0&&report.elapsed>=.25f){bool moved=false;for(int i=0;i<icons.Count;i++)if(icons[i].transform.position!=origins[i])moved=true;Check(moved,"Native icon Transforms move while Time.timeScale is zero");phase=1;}
                if(report.elapsed<3f)return;
                Check(report.spawned==10&&report.returned==10,"Both original five-icon sequences spawn and return all ten prefabs using recovered lifecycle");
                Check(gold.text=="150"&&diamonds.text=="27"&&completed==2,"Real native labels and both completion callbacks reach their endpoints");
                Check(state.goldNum==150&&state.diamondsNum==27&&effects.saves==2,"Collection does not grant or save rewards twice");
                Check(runtime.ActiveEffectCount==0&&runtime.PendingCleanupCount==0,"Realtime coroutine finish and Update drain cleanup while scaled time is paused");
                Check(report.scaledElapsed==0,"Scaled game time remains paused throughout validation");
                Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-fly-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
