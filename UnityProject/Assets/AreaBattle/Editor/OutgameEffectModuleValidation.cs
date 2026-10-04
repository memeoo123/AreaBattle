using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameEffectModuleValidation
    {
        public sealed class Provider:IOutgameAssetProvider
        {
            public GameObject Prefab;public int Releases;public Action Released;
            public bool IsDestroyed=>false;public bool IsDone=>true;public UnityEngine.Object AssetObject=>Prefab;
            public void ReleaseHandle(OutgameAssetHandle h){Releases++;Released?.Invoke();}
        }
        public sealed class Wait
        {
            public object Instruction;public TaskCompletionSource<bool> Completion=new TaskCompletionSource<bool>();
            public void Complete()=>Completion.SetResult(true);
        }
        public sealed class Fixture:IDisposable
        {
            public readonly GameObject CanvasRoot=new GameObject("canvas-root"),UiRoot=new GameObject("ui-root",typeof(RectTransform)),Prefab=new GameObject("effect-fixture");
            public readonly List<GameObject> LoadedObjects=new List<GameObject>();public readonly List<Wait> Waits=new List<Wait>();
            public readonly List<string> Trace=new List<string>(),Errors=new List<string>(),Warnings=new List<string>();
            public readonly List<TaskCompletionSource<OutgameAssetHandle>> Loads=new List<TaskCompletionSource<OutgameAssetHandle>>();
            public readonly List<TaskCompletionSource<GameObject>> LegacyLoads=new List<TaskCompletionSource<GameObject>>();
            public List<OutgameEffectData> Rows=new List<OutgameEffectData>();public bool Modern=true,HasConfig;public int Reads;
            public OutgameEffectModule Module;public OutgameEffectModuleServices Services;public Provider Provider;
            public Action<Transform,Vector3,float,Action> Move;
            public Fixture(bool native=false)
            {
                Prefab.AddComponent<SpriteRenderer>();var child=new GameObject("canvas",typeof(Canvas));child.transform.SetParent(Prefab.transform,false);child.GetComponent<Canvas>().sortingOrder=3;
                Provider=new Provider{Prefab=Prefab};
                var effects=new OutgameEffectServices{UseNewResources=()=>Modern,LoadNewPrefab=path=>{Trace.Add(path);var load=new TaskCompletionSource<OutgameAssetHandle>();Loads.Add(load);return load.Task;},LoadLegacyPrefab=path=>{Trace.Add(path);var load=new TaskCompletionSource<GameObject>();LegacyLoads.Add(load);return load.Task;},Move=(t,p,d,c)=>Move(t,p,d,c)};
                if(!native){effects.Wait=instruction=>{var wait=new Wait{Instruction=instruction};Waits.Add(wait);return wait.Completion.Task;};effects.Destroy=UnityEngine.Object.DestroyImmediate;}
                Services=new OutgameEffectModuleServices{HasConfigResource=()=>HasConfig,UseNewResources=()=>Modern,ReadConfig=()=>{Reads++;return Rows;},CanvasRoot=()=>CanvasRoot.transform,UiRoot=()=>UiRoot.transform,UiCamera=()=>null,Log=Trace.Add,Error=Errors.Add,Effects=effects};
                if(!native){Services.Destroy=UnityEngine.Object.DestroyImmediate;Services.Persist=o=>Trace.Add("persist:"+o.name);}
                Module=new OutgameEffectModule(Services);Module.Initialize();
            }
            public OutgameEffectData Add(int id,int type=0,double duration=0){var row=new OutgameEffectData{id=id,type=type,res="original-path-"+id,duration=duration};Rows.Add(row);return row;}
            public OutgameBaseEffect Show(int id,Transform parent=null,int order=-1)=>Module.Get(Module.ShowEffectWithId(id,new Vector3(2,3,4),new Vector3(0,0,25),Vector3.zero,0,parent,-1,order));
            public OutgameAssetHandle CompleteLoad(int index=0)
            {
                var handle=new OutgameAssetHandle("effect",Provider,()=>false,Warnings.Add);Loads[index].SetResult(handle);
                foreach(var effect in Module.Effects.Values)if(effect.GameObject!=null&&!LoadedObjects.Contains(effect.GameObject))LoadedObjects.Add(effect.GameObject);return handle;
            }
            public void Dispose()
            {
                // Fixture cleanup bypasses failure injections and reclaims source late-load orphans.
                Services.Destroy=Services.Effects.Destroy=UnityEngine.Object.DestroyImmediate;
                foreach(var go in LoadedObjects)if(go)UnityEngine.Object.DestroyImmediate(go);
                Module.Shutdown();if(Prefab)UnityEngine.Object.DestroyImmediate(Prefab);if(CanvasRoot)UnityEngine.Object.DestroyImmediate(CanvasRoot);if(UiRoot)UnityEngine.Object.DestroyImmediate(UiRoot);
            }
        }
        public static void Require(bool value,string detail){if(!value)throw new Exception(detail);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Actual recovered EffectModule/Base/UI/Fly/Line effects, native GameObject/Canvas and real AssetOperationHandle ownership. Load/delay/tween endpoints explicitly controlled to test ordering/failures. Original effect1016 asset/particles, full production resource/EffectControl/Main composition and audiovisual/Player acceptance remain pending."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="effect-module-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="effect-module-"+id,result="fail",detail=e.ToString()});}};
            check("config-gates-retry-duplicate-first-wins-and-read-once",()=>{using(var f=new Fixture()){
                f.Modern=false;f.Add(1);f.Module.ReadEffectData();Require(!f.Module.AlreadySetData&&f.Reads==0&&f.Errors.Single()=="Effect:配置文件AB资源为null","source absent resource gate");f.HasConfig=true;var first=f.Rows[0];f.Add(1,3);f.Add(2);f.Module.ReadEffectData();f.Module.ReadEffectData();Require(f.Reads==1&&f.Module.Configs.Count==2&&ReferenceEquals(f.Module.Configs[1],first)&&f.Errors[1]=="表[EffectConfig]中有相同键(1)","first row retained and source duplicate error");
            }});
            check("null-config-list-and-shutdown-retain-source-read-flags",()=>{using(var f=new Fixture()){
                f.Rows=null;f.Module.ReadEffectData();f.Module.Shutdown();Require(f.Module.AlreadySetData&&f.Module.IsInitialized,"source flags retained");f.Rows=new List<OutgameEffectData>{new OutgameEffectData{id=1}};f.Module.Initialize();f.Module.ReadEffectData();Require(f.Reads==1&&f.Module.Configs.Count==0,"reinitialize does not reset AlreadySetData");
            }});
            check("lazy-root-hierarchy-sorting-and-shutdown-destruction",()=>{using(var f=new Fixture()){
                f.UiRoot.AddComponent<Canvas>();var w=f.Module.WorldEffectRoot;var u=f.Module.UIEffectRoot;var l=f.Module.LineEffectRoot;Require(w.name=="__WorldEffectRoot"&&u.parent==f.CanvasRoot.transform&&l.parent==f.UiRoot.transform&&l.GetComponent<Canvas>().overrideSorting&&l.GetComponent<Canvas>().sortingOrder==100,"three original roots");Require(ReferenceEquals(w,f.Module.WorldEffectRoot)&&f.Trace.Count(s=>s.StartsWith("persist:"))==1,"world root persisted once");f.Module.Shutdown();Require(!w&&!u&&!l,"all original root objects destroyed");
            }});
            check("missing-config-errors-before-any-effect-registration",()=>{using(var f=new Fixture()){
                Throws<NullReferenceException>(()=>f.Show(99));Require(f.Errors.Single()=="未找到ID[99]为的特效"&&f.Module.Effects.Count==0&&f.Loads.Count==0,"no fallback for unknown id");
            }});
            check("async-load-parent-position-rotation-order-and-retained-scale",()=>{using(var f=new Fixture()){
                f.Add(1);f.Prefab.transform.localScale=new Vector3(2,3,4);var e=f.Show(1);e.SetOrder("Default",8);e.SetActive(false);Require(!e.IsLoaded&&e.PendingOrder&&f.Module.Get(e.UID)==e&&f.Trace.Last()=="Effect/UI/original-path-1","published before async load");f.CompleteLoad();Require(e.IsLoaded&&e.GameObject.transform.parent==f.Module.UIEffectRoot&&e.GameObject.transform.localPosition==e.Position&&e.GameObject.transform.localScale==new Vector3(2,3,4)&&Mathf.Abs(e.GameObject.transform.localEulerAngles.z-25)<.01f,"source transforms, prefab scale retained");Require(e.GameObject.GetComponent<SpriteRenderer>().sortingOrder==8&&e.GameObject.GetComponentInChildren<Canvas>().sortingOrder==11&&!e.PendingOrder,"deferred sorting");f.Waits[0].Complete();Require(!e.GameObject.activeSelf&&f.Module.Get(e.UID)==e,"zero duration stays registered, requested active applied at Play");
            }});
            check("canvas-inherited-order-inactive-children-and-additive-repeat",()=>{using(var f=new Fixture()){
                f.Add(1);var parent=f.CanvasRoot.AddComponent<Canvas>();parent.sortingOrder=10;var hidden=new GameObject("hidden",typeof(SpriteRenderer));hidden.GetComponent<SpriteRenderer>().sortingOrder=40;hidden.transform.SetParent(f.Prefab.transform,false);hidden.SetActive(false);var e=f.Show(1,f.CanvasRoot.transform);f.CompleteLoad();Require(e.Order==11&&e.GameObject.GetComponent<SpriteRenderer>().sortingOrder==11&&e.GameObject.transform.Find("hidden").GetComponent<SpriteRenderer>().sortingOrder==40,"parent plus one, inactive omitted");var childCanvas=e.GameObject.transform.Find("canvas").GetComponent<Canvas>();childCanvas.overrideSorting=true;int before=childCanvas.sortingOrder;e.SetOrder("Default",4);Require(e.GameObject.GetComponent<SpriteRenderer>().sortingOrder==4&&e.GameObject.transform.Find("canvas").GetComponent<Canvas>().sortingOrder==before+4,"renderer absolute, child canvas additive");
            }});
            check("minus9999-disables-component-order-but-explicit-order-still-works",()=>{using(var f=new Fixture()){
                f.Add(1);var e=f.Show(1,order:-9999);f.CompleteLoad();Require(!e.SetComponents&&e.Order==-1&&e.GameObject.GetComponentInChildren<Canvas>().sortingOrder==3,"component sentinel");e.SetOrder("Default",7);Require(e.GameObject.GetComponentInChildren<Canvas>().sortingOrder==10,"direct explicit SetOrder remains callable");
            }});
            check("duration-dispose-release-then-completion-removes-handle",()=>{using(var f=new Fixture()){
                f.Add(1,duration:5);var e=f.Show(1);f.CompleteLoad();f.Waits[0].Complete();Require(f.Waits[1].Instruction is WaitForSeconds&&f.Provider.Releases==0,"scaled duration wait");f.Provider.Released=()=>Require(e.IsDisposed&&f.Module.Get(e.UID)==e&&!e.GameObject,"destruction and disposed write precede release and callback removal");f.Waits[1].Complete();Require(f.Provider.Releases==1&&f.Module.Get(e.UID)==null,"natural completion unregisters afterwards");
            }});
            check("close-removes-before-dispose-failure-no-auto-completion",()=>{using(var f=new Fixture()){
                f.Add(1);var e=f.Show(1);f.CompleteLoad();int callbacks=0;e.OnComplete=x=>callbacks++;f.Services.Effects.Destroy=go=>{Require(f.Module.Get(e.UID)==null&&e.IsDisposed,"remove and disposed first");throw new InvalidOperationException("destroy");};Throws<InvalidOperationException>(()=>f.Module.Close(e.UID));Require(f.Provider.Releases==0&&callbacks==0&&e.GameObject,"failure prefix retained without fallback release or callback");f.Services.Effects.Destroy=UnityEngine.Object.DestroyImmediate;e.Dispose();
            }});
            check("early-close-does-not-cancel-original-late-load",()=>{using(var f=new Fixture()){
                f.Add(1,duration:5);var e=f.Show(1);f.Module.Close(e.UID);Require(e.IsDisposed&&!((WaitUntil)f.Waits[0].Instruction).keepWaiting,"dispose satisfies original Await predicate");f.Waits[0].Complete();f.CompleteLoad();f.LoadedObjects.Add(e.GameObject);Require(e.IsLoaded&&e.GameObject&&e.GameObject.activeSelf&&f.Module.Get(e.UID)==null&&f.Provider.Releases==0,"source late load can instantiate an unregistered orphan; no invented cancellation");e.Dispose();Require(f.Provider.Releases==1,"explicit disposal releases late acquired handle");
            }});
            check("legacy-route-no-handle-and-unknown-type-uses-ui-with-empty-folder",()=>{using(var f=new Fixture()){
                f.Modern=false;f.HasConfig=true;f.Add(1,99);var e=f.Show(1);Require(e is OutgameUIEffect&&f.Trace.Last()=="Effect/original-path-1"&&f.Loads.Count==0,"default UI subtype but unknown folder stays empty");var go=UnityEngine.Object.Instantiate(f.Prefab);f.LegacyLoads[0].SetResult(go);Require(e.Handle==null&&e.GameObject==go,"legacy result is already instantiated");f.Module.Close(e.UID);Require(!go&&f.Provider.Releases==0,"legacy path destroys without modern release");
            }});
            check("collection-reuses-real-module-handle-and-clears-native-children",()=>{using(var f=new Fixture()){
                f.Add(1);var c=new OutgameEffectCollection(()=>f.Module,UnityEngine.Object.DestroyImmediate);c.SetEntity(f.UiRoot.transform);c.Spawn(1,9);int id=c.Handles[1];f.CompleteLoad();f.Waits[0].Complete();var e=f.Module.Get(id);e.SetActive(false);c.Spawn(1,3);Require(e.GameObject.activeSelf&&e.Order==9&&f.Loads.Count==1,"original collection reactivates existing handle with original order");c.Clear();Require(f.Module.Get(id)==null&&c.Handles.Count==0&&c.Root.childCount==0&&f.Provider.Releases==1,"real collection/module/resource lifetime");
            }});
            check("clear-failure-retains-live-dictionary-and-configs",()=>{using(var f=new Fixture()){
                f.Add(1);var e=f.Show(1);f.CompleteLoad();f.Services.Effects.Destroy=go=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Module.ClearChildObj);Require(f.Module.Effects.Count==1&&f.Module.Configs.Count==1&&e.IsDisposed&&f.Provider.Releases==0,"foreach failure prevents both dictionary clears");f.Services.Effects.Destroy=UnityEngine.Object.DestroyImmediate;f.Module.ClearChildObj();Require(f.Module.Effects.Count==0&&f.Module.Configs.Count==0&&f.Module.AlreadySetData,"successful clear retains config-read flag");
            }});
            check("fly-world-target-duration-and-dispose-before-callback",()=>{using(var f=new Fixture()){
                f.Add(1,1,99);Action finish=null;Vector3 target=new Vector3(12,4,0);f.Move=(t,p,d,c)=>{Require(p==target&&d==.75f&&Vector3.Dot(t.up,(target-t.position).normalized)>.999f,"original world-space orientation and tween parameters");finish=c;};int id=f.Module.ShowEffectWithId(1,Vector3.zero,Vector3.zero,target,.75f,f.UiRoot.transform);f.CompleteLoad();f.Waits[0].Complete();Require(f.Waits.Count==1&&finish!=null,"Fly ignores config-duration timer");finish();Require(f.Module.Get(id)==null&&f.Provider.Releases==1,"fly completion disposes and unregisters");
            }});
            check("line-geometry-two-source-timers-and-repeated-disposal",()=>{using(var f=new Fixture()){
                f.UiRoot.AddComponent<Canvas>();f.UiRoot.transform.position=new Vector3(10,20,0);f.Add(1,2,.5);
                int id=f.Module.ShowEffectWithId(1,Vector3.zero,Vector3.zero,new Vector3(266,20,0),0,f.UiRoot.transform);f.CompleteLoad();f.Waits[0].Complete();var e=f.Module.Get(id);
                Require(e is OutgameLineEffect&&e.GameObject.transform.parent==f.Module.LineEffectRoot&&Vector3.Distance(e.GameObject.transform.position,new Vector3(10,20,0))<.01f&&Mathf.Abs(e.GameObject.transform.localScale.y-1)<.001f&&f.Waits.Count==3,"line geometry and base.Play plus line.Play duration waits");
                f.Waits[1].Complete();Require(f.Provider.Releases==1&&f.Module.Get(id)==null,"base timer completes first");f.Waits[2].Complete();Require(f.Provider.Releases==1&&f.Warnings.Single().StartsWith("Operation handle is released"),"line own timer calls Dispose again without disposed gate");
            }});
            return r;
        }
    }
}
