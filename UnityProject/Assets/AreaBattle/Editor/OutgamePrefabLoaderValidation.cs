using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgamePrefabLoaderValidation
    {
        sealed class PreloadHost:IOutgamePrefabPreloadHost
        {
            public LevelLayout Level=new LevelLayout();public int Reads,Resets;public readonly List<string> Calls=new List<string>();
            public Func<int,int> Skin=id=>0;public Func<int,int,int> Random=(id,camp)=>0;public Action OnReset;
            public IOutgameLevelLayout CurrentLevel {get{Reads++;return Level;}}
            public int GetSkinEntityByType(int id){Calls.Add("skin:"+id);return Skin(id);}
            public int GetSkinEntityByRandomType(int id,int camp){Calls.Add("random:"+id+":"+camp);return Random(id,camp);}
            public void ResetLoadingProgress(){Resets++;OnReset?.Invoke();}
        }
        sealed class Fixture:IDisposable
        {
            readonly bool oldNext=OutgameLoadPrefabControl.CanLoadNext;readonly int oldCount=OutgameLoadPrefabControl.PreparedCount;readonly int[] oldIds=OutgameLoadPrefabControl.PreloadIds.ToArray();
            public readonly PreloadHost Preload=new PreloadHost();
            public OutgameLoadPrefabControl Control;public OutgameControllerRegistry Registry=new OutgameControllerRegistry();public OutgameLegacyConfigManager Config;public OutgameLegacyResourceScheduler Scheduler;public OutgameResLoadHelper Helper;public List<OutgameLegacyAssetLoader> Pending=new List<OutgameLegacyAssetLoader>();public List<string> Errors=new List<string>();public List<GameObject> Instances=new List<GameObject>();public Dictionary<string,AssetBundle> Bundles=new Dictionary<string,AssetBundle>();public int GuardReads;public bool Modern;
            public Fixture()
            {
                for(int i=0;i<OutgamePrefabLoaderBundles.Keys.Length;i++){var bundle=AssetBundle.LoadFromFile(Path.Combine(OutgamePrefabLoaderBundles.Folder,OutgamePrefabLoaderBundles.Names[i]));if(!bundle)throw new Exception("Missing rebuilt original bundle");Bundles.Add(OutgamePrefabLoaderBundles.Keys[i],bundle);}
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(Errors.Add),null,null,null,null,null,null);var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),Errors.Add);reader.ReadTable(Config.dicEntityModel);reader.ReadTable(Config.dicSceneSkin);OutgameConfigDerivedIndexes.RebuildSceneResources(Config.dicSceneSkin,Config.SceneResources);
                Scheduler=new OutgameLegacyResourceScheduler(()=>Modern,()=>{},asset=>new OutgameLegacyAssetLoader(loader=>Pending.Add(loader),name=>{}),Errors.Add);
                Helper=new OutgameResLoadHelper(()=>{GuardReads++;return Modern;},()=>Scheduler,Errors.Add);
                OutgameCoreControllerBindings.BindPrefabLoader(Registry,()=>Config,Helper,()=>new OutgamePrefabCache(name=>Resources.Load<OutgameLevelEditorConfig>(name)),Errors.Add,Preload);Control=(OutgameLoadPrefabControl)Registry.Resolve(4117);Control.OnInit();
            }
            public void Pump(){Scheduler.Update(0,0);}
            public void Complete(int index){var loader=Pending[index];loader.PendingBundle=Bundles[loader.BundleName];loader.Complete();}
            public void Track(GameObject go,object[] args)=>Instances.Add(go);
            public void Dispose(){OutgameLoadPrefabControl.CanLoadNext=oldNext;OutgameLoadPrefabControl.PreparedCount=oldCount;OutgameLoadPrefabControl.PreloadIds.Clear();OutgameLoadPrefabControl.PreloadIds.AddRange(oldIds);if(Control.EntityRoot)UnityEngine.Object.DestroyImmediate(Control.EntityRoot);foreach(var go in Instances)if(go)UnityEngine.Object.DestroyImmediate(go);foreach(var bundle in Bundles.Values)bundle.Unload(true);}
        }
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-resource-path-thirteen-branches-and-basename",()=>{
                var expected=new[]{"Scene/A","Model/WayLine/A.prefab","Model/Entity/A.prefab","Model/Hero/A.prefab","Data/LevelCfg/A.Json","Materials/materials/A.mat","Materials/Model/A.mat","Animation/Soldier/A.controller","Model/Scene/A.prefab","Data/AnimationTexture/A.bytes","Model/SoldierCommon/A.prefab","Model/SoldierAnimationIns/A","Textures/skinscene/A.png"};
                for(int i=0;i<expected.Length;i++)Require(OutgameResourcePaths.GetPath("A",i)==expected[i],"source path case"+i);
                Require(OutgameResourcePaths.GetPath("A",-1)=="A"&&OutgameResourcePaths.GetPath(null,99)==null&&OutgameResourcePaths.GetPath(null,4)=="Data/LevelCfg/.Json","default and null concatenate semantics");
                Require(OutgameResourcePaths.ToFileName("dir/name.part.prefab")=="name"&&OutgameResourcePaths.ToFileName("dir/")==""&&OutgameResourcePaths.ToFileName("dir\\name.prefab")=="dir\\name","forward slash and first dot only");
            });
            check("source-prefab-load-asset-cold-and-cached-names-arguments",()=>{using(var f=new Fixture()){
                var args=new object[]{"original-context"};object[] received=null;f.Control.LoadAsset(11001,(go,a)=>{f.Instances.Add(go);received=a;},args);f.Pump();
                Require(f.Pending.Count==1&&f.Pending[0].BundleName==OutgamePrefabLoaderBundles.Keys[0]&&ReferenceEquals(f.Pending[0].Arguments,args),"source config11001 constructs asset route and retains parameter array");
                f.Complete(0);Require(f.Control.Contains(11001)&&f.Instances[0].name=="GameScene/HD4_CJ_1"&&ReferenceEquals(args,received),"cold callback uses captured config AssetName and native template cache");
                f.Control.LoadAsset(11001,f.Track,args);Require(f.Pending.Count==1&&f.Instances.Count==2&&f.Instances[1].name=="HD4_CJ_1"&&!ReferenceEquals(f.Instances[0],f.Instances[1]),"warm route clones immediately and names from cached template, not config");
            }});
            check("source-prefab-concurrent-load-cache-replacement-and-captured-args",()=>{using(var f=new Fixture()){
                object[] first={1},second={2};var received=new List<object[]>();
                f.Control.LoadAsset(11001,(go,a)=>{f.Track(go,a);received.Add(a);},first);f.Control.LoadAsset(11001,(go,a)=>{f.Track(go,a);received.Add(a);},second);f.Pump();
                Require(f.Pending.Count==1&&ReferenceEquals(f.Pending[0].Arguments,second),"scheduler coalesces loader and stores last request args");f.Complete(0);
                Require(f.Instances.Count==2&&received[0]==first&&received[1]==second&&f.Control.Cache.Prefabs.Count==1,"callbacks keep own arguments and replace cache entry without duplicate Add");
            }});
            check("source-prefab-load-texture-and-prefab-helper-prefix",()=>{using(var f=new Fixture()){
                Texture2D texture=null;var args=new object[]{7};object[] received=null;f.Control.LoadTexture("scene_skin_idle1",(value,a)=>{texture=value;received=a;},args);f.Pump();f.Complete(0);
                Require(texture&&texture.name=="scene_skin_idle1"&&received==args&&f.Control.Cache.Prefabs.Count==0,"native texture loaded by source basename with no prefab cache write");
                int reads=f.GuardReads;f.Control.LoadPrefab("effect/eff_idle_GuBao",f.Track,args);f.Pump();Require(f.GuardReads==reads+2&&f.Pending[1].BundleName==OutgamePrefabLoaderBundles.Keys[3],"LoadPrefab and LoadPrefabOri both guard; Prefabs prefix appended exactly once");f.Complete(1);
                Require(f.Instances[0].name=="eff_idle_GuBao(Clone)"&&f.Control.Cache.Prefabs.Count==0,"path-prefab route keeps clone suffix and bypasses id cache");
            }});
            check("source-prefab-missing-config-modern-guard-and-null-key",()=>{using(var f=new Fixture()){
                bool called=false;f.Control.LoadAsset(-1,(go,a)=>called=true);f.Control.LoadTexture("missing",(value,a)=>called=true);Require(f.Errors.Count==2&&!called&&f.Scheduler.PendingCount==0,"source missing config only logs without fabricated callback");
                f.Modern=true;f.Control.LoadAsset(11001,(go,a)=>called=true);Require(!called&&f.Scheduler.PendingCount==0&&f.Errors[2]=="当前使用的是新资源加载，请使用NewResLoadHelper","legacy helper refuses modern route");
                bool failed=false;try{f.Control.LoadTexture(null,null);}catch(ArgumentNullException){failed=true;}Require(failed,"dictionary null-key failure preserved");
            }});
            check("source-prefab-null-resource-failure-preserves-template-write",()=>{using(var f=new Fixture()){
                f.Control.Cache.Prefabs.Add(11001,null);Require(f.Control.Contains(11001),"Contains reports key existence even when cached object null");
                f.Control.LoadAsset(11001,f.Track);f.Pump();var loader=f.Pending[0];loader.PendingBundle=null;bool failed=false;try{loader.Complete();}catch(ArgumentException){failed=true;}
                Require(failed&&f.Control.Cache.Prefabs.ContainsKey(11001)&&f.Control.Cache.Prefabs[11001]==null&&f.Instances.Count==0,"null native resource asset is cached before Instantiate throws");
            }});
            check("source-prefab-controller-dispose-and-game-resource-binding",()=>{using(var f=new Fixture()){
                var adapter=new OutgameGameSceneResources(()=>f.Control);var args=new object[]{101,"parent-context"};adapter.LoadAsset(11001,go=>f.Instances.Add(go),args);f.Pump();Require(f.Pending[0].Arguments==args,"game adapter passes original argument array through resource graph");f.Complete(0);
                f.Control.OnDispose();Require(!f.Registry.HasInstance(4117)&&f.Control.Contains(11001)&&f.Instances[0],"source dispose clears registry only");
            }});
            check("source-entity-preparation-cold-template-only-and-warm-inline",()=>{using(var f=new Fixture()){
                int count=0;f.Control.PrepareEntity(11001,()=>count++);Require(count==0,"cold prepare is asynchronous");f.Pump();f.Complete(0);
                Require(count==1&&f.Control.Contains(11001)&&f.Control.Cache.Entities.Count==0&&!f.Control.EntityRoot,"prepare caches template without creating an instance/root");
                f.Control.PrepareEntity(11001,()=>count++);Require(count==2&&f.Scheduler.PendingCount==0,"warm prepare completes inline");
            }});
            check("source-entity-cold-acquisition-naming-world-parent-and-reuse",()=>{using(var f=new Fixture()){
                var args=new object[]{"entity-context"};object[] received=null;
                f.Control.GetEntity(11001,(go,a)=>{f.Track(go,a);received=a;},args);f.Pump();f.Complete(0);var first=f.Instances[0];var root=f.Control.GetEntityRoot(11001);
                Require(first.activeSelf&&first.name=="GameScene/HD4_CJ_1_1"&&first.transform.parent==root&&received==args,"cold template creates, registers, parents, names, activates before callback");
                Require(root.name=="GameScene/HD4_CJ_1"&&root.position==Vector3.zero&&root.parent==f.Control.EntityRoot.transform,"source per-id root is a child of GameObject EntityRoot at world zero");
                first.SetActive(false);f.Control.EntityRoot.SetActive(false);var reused=f.Control.GetEntityNow(11001);
                Require(reused==first&&reused.activeSelf&&!reused.activeInHierarchy&&f.Pending.Count==1,"pool availability uses activeSelf despite inactive hierarchy");
                var second=f.Control.GetEntityNow(11001);f.Instances.Add(second);Require(second!=first&&second.name=="GameScene/HD4_CJ_1_2"&&f.Control.Cache.Entities[11001].Count==2,"active object cannot be reused; child count names new clone");
            }});
            check("source-entity-destroyed-entry-removal-skips-shifted-item",()=>{using(var f=new Fixture()){
                f.Control.PrepareEntity(11001,null);f.Pump();f.Complete(0);var idle=new GameObject("shifted-idle");idle.SetActive(false);f.Instances.Add(idle);
                var destroyed=new GameObject("dead");UnityEngine.Object.DestroyImmediate(destroyed);f.Control.Cache.Entities.Add(11001,new List<GameObject>{destroyed,idle});
                var acquired=f.Control.GetEntityNow(11001);f.Instances.Add(acquired);
                Require(acquired!=idle&&!idle.activeSelf&&f.Control.Cache.Entities[11001].Count==2&&f.Control.Cache.Entities[11001][0]==idle,"remove index0 then increment skips shifted inactive object and creates another");
                Require(f.Control.GetEntityNow(11001)==idle&&idle.activeSelf,"next request can reuse skipped object");
            }});
            check("source-entity-now-cold-null-return-and-invalid-cache-not-replaced",()=>{using(var f=new Fixture()){
                Require(f.Control.GetEntityNow(11001)==null&&f.Errors.Count==1,"cold sync call returns null and emits source missing-cache diagnostic");f.Pump();f.Complete(0);
                Require(f.Control.Contains(11001)&&f.Control.Cache.Entities.Count==0,"completion warms template without creating the deferred caller an entity");
                f.Control.Cache.Prefabs[11001]=null;int completed=0;f.Control.PrepareEntity(11001,()=>completed++);f.Pump();
                Require(completed==1&&f.Control.Cache.Prefabs[11001]==null&&f.Errors.Count==2,"successful reload invokes callback but source ContainsKey prevents replacing invalid entry");
                f.Control.GetEntity(11001,f.Track);f.Pump();Require(f.Instances.Count==1&&f.Instances[0]&&f.Control.Cache.Prefabs[11001]==null,"cold entity callback uses returned native template even when dictionary stays null");
            }});
            check("source-entity-root-world-origin-null-cache-and-missing-config",()=>{using(var f=new Fixture()){
                f.Control.EntityRoot=new GameObject("moved-entity-root");f.Control.EntityRoot.transform.position=new Vector3(10,20,30);
                var root=f.Control.GetEntityRoot(-17);Require(root.position==Vector3.zero&&root.localPosition==new Vector3(-10,-20,-30)&&root.name=="-17"&&f.Errors[0]=="EntityModelConfig 不存在 Id=-17 的数据","missing config logs then names root by id and sets world origin");
                f.Control.Cache.Roots[11001]=null;Require(f.Control.GetEntityRoot(11001)==null,"present null root is not repaired");
                f.Control.Cache.Prefabs[11001]=new GameObject("template");f.Instances.Add(f.Control.Cache.Prefabs[11001]);bool failed=false;
                try{f.Control.GetEntityNow(11001);}catch(NullReferenceException){failed=true;}
                Require(failed&&f.Control.Cache.Entities[11001].Count==1,"instance registration precedes failed naming on null root");f.Instances.Add(f.Control.Cache.Entities[11001][0]);
            }});
            check("source-entity-null-template-retry-count-does-not-increment",()=>{using(var f=new Fixture()){
                bool had=OutgameLoadPrefabControl.LoadFailures.TryGetValue(11001,out int previous);
                try{
                    OutgameLoadPrefabControl.LoadFailures.Remove(11001);bool callback=false;f.Control.PrepareEntity(11001,()=>callback=true);f.Pump();f.Pending[0].PendingBundle=null;f.Pending[0].Complete();
                    for(int i=0;i<4;i++)f.Pump();
                    Require(!callback&&!f.Control.Contains(11001)&&OutgameLoadPrefabControl.LoadFailures[11001]==0&&f.Scheduler.PendingCount==1,"source null-template branch keeps retry pending with count zero beyond nominal threshold");
                    OutgameLoadPrefabControl.LoadFailures[11001]=4;f.Pump();Require(!callback&&f.Scheduler.PendingCount==0&&f.Errors[f.Errors.Count-1]=="资源id = 11001 加载失败","preexisting count above3 logs terminal failure and suppresses callback");
                }finally{if(had)OutgameLoadPrefabControl.LoadFailures[11001]=previous;else OutgameLoadPrefabControl.LoadFailures.Remove(11001);}
            }});
            check("source-entity-success-retains-static-failure-state-and-disposal-cache",()=>{using(var f=new Fixture()){
                bool had=OutgameLoadPrefabControl.LoadFailures.TryGetValue(11001,out int previous);
                try{
                    OutgameLoadPrefabControl.LoadFailures[11001]=3;f.Control.GetEntity(11001,f.Track);f.Pump();f.Complete(0);var root=f.Control.EntityRoot;
                    f.Control.OnDispose();Require(OutgameLoadPrefabControl.LoadFailures[11001]==3&&f.Control.EntityRoot==root&&f.Control.Cache.Entities[11001].Count==1&&!f.Registry.HasInstance(4117),"source success only resets local; disposal retains static state and native pool");
                }finally{if(had)OutgameLoadPrefabControl.LoadFailures[11001]=previous;else OutgameLoadPrefabControl.LoadFailures.Remove(11001);}
            }});
            check("source-preload-list-original-order-dedup-and-level-getter-reads",()=>{using(var f=new Fixture()){
                var template=new GameObject("preload-fixture");f.Instances.Add(template);int[] expected={1,10,11,12,13,14,15,101,9033,9034,2001,3001,821,4001};
                foreach(int id in expected)f.Control.Cache.Prefabs[id]=template;
                f.Preload.Level=new LevelLayout{StarInfoCfgs=new[]{new StarInfoCfg{ShipID=8,CampID=2,isBoss=true},new StarInfoCfg{ShipID=9,CampID=3,isBoss=true}},ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=4001},new ObstacleInfoCfg{EnityID=2001},new ObstacleInfoCfg{EnityID=0}}};
                f.Preload.Skin=id=>id==8?2001:0;f.Preload.Random=(id,camp)=>3001;bool complete=false;OutgameLoadPrefabControl.CanLoadNext=true;OutgameLoadPrefabControl.PreparedCount=92;
                f.Control.PrepareStart(()=>complete=true);
                Require(string.Join(",",OutgameLoadPrefabControl.PreloadIds)==string.Join(",",expected)&&string.Join(",",f.Preload.Calls)=="skin:8,random:8:2,skin:9,random:9:3","fixed ten ids then per-star skin/random/boss then positive unique obstacle EnityID");
                Require(complete&&!OutgameLoadPrefabControl.CanLoadNext&&OutgameLoadPrefabControl.PreparedCount==14&&f.Preload.Resets==14&&f.Preload.Reads==6&&f.Control.Cache.Entities.Count==0,"warm preparation resets counter/flag and progresses inline with three getter reads per array");
            }});
            check("source-prepare-start-cold-tail-waits-and-callback-order",()=>{using(var f=new Fixture()){
                var template=new GameObject("preload-fixture");f.Instances.Add(template);foreach(int id in new[]{1,10,11,12,13,14,15,101,9033,9034})f.Control.Cache.Prefabs[id]=template;
                f.Preload.Level.ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=11001}};bool complete=false;
                f.Preload.OnReset=()=>Require(OutgameLoadPrefabControl.PreparedCount==f.Preload.Resets,"shared count increments before loading page reset");
                f.Control.PrepareStart(()=>complete=true);Require(!complete&&OutgameLoadPrefabControl.PreparedCount==10&&f.Scheduler.PendingCount==1,"PrepareStart stalls until final uncached original asset completes");
                f.Pump();f.Complete(0);Require(complete&&OutgameLoadPrefabControl.PreparedCount==11&&f.Preload.Resets==11&&f.Control.Contains(11001),"template completion advances then resets page before completion callback");
            }});
            check("source-preload-stop-flag-does-not-cancel-inflight-and-retains-cursor",()=>{using(var f=new Fixture()){
                // Synthetic config alias isolates queue semantics against a real native bundle.
                f.Config.dicEntityModel[1]=f.Config.dicEntityModel[11001];OutgameLoadPrefabControl.PreparedCount=77;f.Control.PreLoad();
                Require(OutgameLoadPrefabControl.CanLoadNext&&OutgameLoadPrefabControl.PreparedCount==77&&f.Scheduler.PendingCount==1,"PreLoad sets gate but leaves PrepareStart counter unchanged");
                f.Pump();OutgameLoadPrefabControl.CanLoadNext=false;f.Complete(0);
                Require(f.Control.Contains(1)&&f.Scheduler.PendingCount==0&&f.Preload.Resets==0&&OutgameLoadPrefabControl.PreparedCount==77,"inflight template finishes; false gate prevents scheduling next id without UI reset");
            }});
            check("source-preload-partial-state-on-config-and-page-failure",()=>{using(var f=new Fixture()){
                f.Config.dicEntityModel.Remove(1);bool complete=false;f.Control.PrepareStart(()=>complete=true);
                Require(!complete&&OutgameLoadPrefabControl.PreparedCount==0&&f.Preload.Resets==0&&OutgameLoadPrefabControl.PreloadIds.Count==10&&f.Errors.Count==1,"missing first config leaves queue and callback pending without success");
                var template=new GameObject("preload-fixture");f.Instances.Add(template);f.Control.Cache.Prefabs[1]=template;f.Preload.OnReset=()=>throw new InvalidOperationException("page-reset");bool failed=false;
                try{f.Control.PrepareStart(()=>complete=true);}catch(InvalidOperationException){failed=true;}
                Require(failed&&!complete&&OutgameLoadPrefabControl.PreparedCount==1&&f.Preload.Resets==1,"page failure occurs after cursor increment and prevents next item/completion");
            }});
            return report;
        }
    }
}
