using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameFrameEntryValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public class Probe:IOutgameFrameModule
        {
            public int Rank;public int Priority=>Rank;public bool IsInitialized{get;set;}
            public Action Initialized{get;set;}public Action Init,Begin,Tick,End;public object[] Args;public float Delta,Unscaled;
            public void Initialize()=>Initialize(Array.Empty<object>());
            public void Initialize(object[] args){Args=args;Init?.Invoke();IsInitialized=true;Initialized?.Invoke();}
            public void Start()=>Begin?.Invoke();public void Update(float delta,float unscaled){Delta=delta;Unscaled=unscaled;Tick?.Invoke();}public void Shutdown()=>End?.Invoke();
        }
        public sealed class High:Probe{}public sealed class Equal:Probe{}public sealed class Low:Probe{}
        sealed class DataProbe:IOutgameDataManager
        {
            public string DataKey=>"frame-pool-probe";public bool ParticipatesInSync{get;set;}public bool CompressData{get;set;}
            public Action Save,Release;public void OnInit(){}public void OnSave()=>Save?.Invoke();public void OnRelease()=>Release?.Invoke();
        }
        public sealed class Fixture
        {
            public readonly List<string> Trace=new List<string>();public readonly OutgameFrameEntry Frame;public readonly OutgameFrameServices Services;
            public readonly Dictionary<Type,IOutgameFrameModule> Factories=new Dictionary<Type,IOutgameFrameModule>();
            public bool Config=true,Suppress;public int ResourceQueries;
            public Fixture()
            {
                Services=new OutgameFrameServices{CreateModule=t=>Factories[t],SetCulture=()=>Trace.Add("culture"),ShutdownInput=()=>Trace.Add("input"),
                    ClearPermissions=()=>Trace.Add("permission"),SetConfigReadInitialized=b=>{Config=b;Trace.Add("config");},Log=s=>Trace.Add("create:"+s),Warning=s=>Trace.Add("warning:"+s),
                    SuppressResourceUpdate=()=>Suppress,Resources=()=>{ResourceQueries++;return Factories[typeof(Probe)];}};Frame=new OutgameFrameEntry(Services);
            }
            public T Add<T>(T value)where T:class,IOutgameFrameModule{Factories[typeof(T)]=value;return Frame.GetModule<T>();}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="GameFrameEntry module dispatch core and concrete recovered module contracts, plus separate native statistics runtime teardown. Full CommonSettings/StartGame/pause/GameFrameWorkMono, production SDK/account/remaining modules/Main remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("frame-entry-priority-stable-ties-exact-type-and-snapshot",()=>{
                var f=new Fixture();var low=f.Add(new Low{Rank=1});var equal=f.Add(new Equal{Rank=10});var high=f.Add(new High{Rank=10});
                var all=f.Frame.GetAllModule();Require(ReferenceEquals(all[0],equal)&&ReferenceEquals(all[1],high)&&ReferenceEquals(all[2],low),"descending stable priorities");
                Require(f.Frame.HaveModule<Probe>()==null&&ReferenceEquals(f.Frame.HaveModule<High>(),high),"Have uses exact type and never creates");
                int count=f.Trace.Count;Require(ReferenceEquals(f.Frame.GetModule<High>(),high)&&f.Trace.Count==count,"Get reuses without logging");
                f.Frame.Modules.AddLast(new High());Require(!ReferenceEquals(f.Frame.GetModule<High>(),high)&&ReferenceEquals(f.Frame.HaveModule<High>(),high),"Get searches last while Have searches first");
                all.Clear();Require(f.Frame.Modules.Count==4,"GetAll returns independent list");
            });
            check("frame-entry-initialize-live-args-and-partial-state",()=>{
                var f=new Fixture();var p=new Probe();var args=new object[]{3};bool complete=false;
                p.Init=()=>{Require(ReferenceEquals(p.Args,args)&&p.Initialized!=null,"assign completion before initialize and preserve args");throw new InvalidOperationException();};
                Throws<InvalidOperationException>(()=>f.Frame.Initialize(p,()=>complete=true,args));Require(!complete&&!p.IsInitialized&&p.Initialized!=null,"failure retains assigned callback");
                p.Init=null;f.Frame.Initialize(p,()=>complete=true,(object[])null);Require(complete&&p.IsInitialized&&p.Args==null,"null params forwarded exactly");
            });
            check("frame-entry-start-enumerates-live-and-publishes-ready-last",()=>{
                var f=new Fixture();var p=f.Add(new Probe());p.Begin=()=>{Require(!f.Frame.Started,"not started inside first callback");f.Add(new Low());};
                Throws<InvalidOperationException>(f.Frame.Start);Require(!f.Frame.Started&&f.Frame.Modules.Count==2,"Start list mutation leaves new module but does not publish ready");
                p.Begin=()=>f.Trace.Add("start");f.Frame.Start();Require(f.Frame.Started,"ready after all starts");
                p.Begin=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Frame.Start);Require(f.Frame.Started,"failed repeated Start does not clear prior ready");
            });
            check("frame-entry-update-live-nodes-not-initialization-gated",()=>{
                var f=new Fixture();var p=f.Add(new Probe{Rank=10});int ticks=0;
                p.Tick=()=>{ticks++;if(f.Frame.HaveModule<Low>()==null)f.Add(new Low{Rank=0,Tick=()=>ticks++});};
                f.Frame.Update(2,3);Require(ticks==0,"global Start gate");f.Frame.Start();f.Frame.Update(2,3);
                Require(ticks==2&&!p.IsInitialized&&p.Delta==2&&p.Unscaled==3,"new trailing node visited same tick with independent delta args");
                p.Tick=()=>{ticks++;f.Frame.Modules.RemoveFirst();};f.Frame.Update(4,5);Require(ticks==3,"removing held node clears Next and ends traversal");
            });
            check("frame-entry-resources-short-circuit-and-uninitialized-update",()=>{
                var f=new Fixture();var p=new Probe();f.Factories[typeof(Probe)]=p;int ticks=0;p.Tick=()=>ticks++;
                f.Frame.ResourcesModuleUpdate(1,2);Require(f.ResourceQueries==0,"not started skips resource getter");f.Frame.Start();f.Suppress=true;f.Frame.ResourcesModuleUpdate(1,2);
                Require(f.ResourceQueries==0,"suppressed mode skips resource getter");f.Suppress=false;f.Frame.ResourcesModuleUpdate(1,2);Require(ticks==1&&!p.IsInitialized,"resource instance receives update without own ready check");
                f.Factories[typeof(Probe)]=null;f.Frame.ResourcesModuleUpdate(1,2);Require(f.ResourceQueries==2,"null resource accepted after lookup");
            });
            check("frame-entry-shutdown-reverse-skip-tail-order-and-retained-fields",()=>{
                var f=new Fixture();f.Add(new High{Rank=10,IsInitialized=true,End=()=>f.Trace.Add("high")});f.Add(new Low{Rank=1,IsInitialized=true,End=()=>f.Trace.Add("low")});f.Add(new Equal{Rank=5});
                f.Frame.Start();f.Trace.Clear();Action first=null;first=()=>{Require(!f.Config&&f.Frame.Modules.Count==0,"global cleanup precedes disposables");f.Trace.Add("dispose1");f.Frame.DisposableActions-=first;};
                Action last=()=>f.Trace.Add("dispose2");f.Frame.DisposableActions=first+last;f.Frame.Shutdown();
                Require(f.Trace[0]=="low"&&f.Trace[1].StartsWith("warning:Equal")&&string.Join(",",f.Trace.GetRange(2,6))=="high,input,permission,config,dispose1,dispose2","source reverse order and global tail");
                Require(f.Frame.Started&&f.Frame.DisposableActions==last,"Started/delegate not reset by Shutdown");
            });
            check("frame-entry-shutdown-failure-keeps-prefix-and-skips-global-tail",()=>{
                var f=new Fixture();var high=f.Add(new High{Rank=10,IsInitialized=true});var low=f.Add(new Low{Rank=1,IsInitialized=true});low.End=()=>throw new InvalidOperationException("module failure");
                f.Trace.Clear();Throws<InvalidOperationException>(f.Frame.Shutdown);Require(f.Frame.Modules.Count==2&&f.Config&&f.Trace.Count==0,"module exception aborts before list clear/global cleanup");
                low.End=null;f.Frame.DisposableActions=()=>throw new InvalidOperationException("dispose failure");Throws<InvalidOperationException>(f.Frame.Shutdown);
                Require(f.Frame.Modules.Count==0&&!f.Config,"dispose failure happens after committed global cleanup");
            });
            check("frame-entry-create-log-failure-does-not-publish",()=>{
                var f=new Fixture();f.Factories[typeof(Probe)]=new Probe();f.Services.Log=s=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Frame.GetModule<Probe>());
                Require(f.Frame.Modules.Count==0,"constructor result not inserted when source log fails");f.Services.CreateModule=t=>null;Throws<NullReferenceException>(()=>f.Frame.GetModule<Probe>());
            });
            check("frame-entry-concrete-logic-fsm-procedure-time-lifecycle",()=>{
                var f=new Fixture();var messages=new OutgameMessageDispatcher();bool poolReleased=false;
                var logic=f.Add(new OutgameLogicModule(b=>{},()=>{},()=>poolReleased=true,s=>{},s=>{}));var fsm=f.Add(new OutgameFsmManager());
                var procedure=f.Add(new OutgameProcedureManager(()=>fsm));var time=f.Add(new OutgameTimeModule(()=>messages,a=>{},a=>{}));
                foreach(var module in f.Frame.GetAllModule())f.Frame.Initialize(module,null);f.Frame.Start();f.Frame.Update(.1f,.2f);
                var all=f.Frame.GetAllModule();Require(ReferenceEquals(all[0],procedure)&&ReferenceEquals(all[1],fsm)&&ReferenceEquals(all[2],time)&&ReferenceEquals(all[3],logic),"actual source priorities90/80/60/12");
                f.Frame.Shutdown();Require(poolReleased&&!time.IsInitialized&&!procedure.IsInitialized&&!fsm.IsInitialized&&f.Frame.Modules.Count==0,"actual modules release through frame entry");
            });
            check("frame-pool-release-saves-before-live-release-and-retains-dictionary",()=>{
                var trace=new List<string>();var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>trace.Add("error"));pool.OnInit(false,"test",Array.Empty<OutgameManagerRegistration>());
                pool.AddModel(1,new DataProbe{Save=()=>{Require(!pool.SourceReadyFlag,"readiness closes before SaveData");trace.Add("save1");throw new InvalidOperationException();},Release=()=>trace.Add("release1")},false);
                pool.AddModel(2,new DataProbe{Save=()=>trace.Add("save2"),Release=()=>trace.Add("release2")},false);var dictionary=pool.Managers;pool.OnRelease();
                Require(string.Join(",",trace)=="save1,error,save2,release1,release2"&&ReferenceEquals(dictionary,pool.Managers)&&dictionary.Count==0,"save catches per manager; release then clears retained dictionary");
            });
            check("frame-pool-release-failure-and-live-mutation-stop-cleanup",()=>{
                var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});pool.OnInit(false,"test",Array.Empty<OutgameManagerRegistration>());var row=new DataProbe();pool.AddModel(1,row,false);
                row.Release=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(pool.OnRelease);Require(pool.Managers.Count==1&&!pool.SourceReadyFlag,"release failure retains dictionary and closed readiness");
                row.Release=()=>pool.AddModel(2,new DataProbe(),false);Throws<InvalidOperationException>(pool.OnRelease);Require(pool.Managers.Count==2,"mutating live dictionary during release propagates enumeration failure");
            });
            return report;
        }
    }
}
