using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameFsmValidation
    {
        class State:IOutgameFsmState<IOutgameProcedureManager>
        {
            public Action<OutgameFsm<IOutgameProcedureManager>> Init=f=>{},Enter=f=>{},Destroy=f=>{};
            public Action<OutgameFsm<IOutgameProcedureManager>,object[]> EnterArgs=(f,a)=>{};
            public Action<OutgameFsm<IOutgameProcedureManager>,float,float> Tick=(f,a,b)=>{};
            public Action<OutgameFsm<IOutgameProcedureManager>,bool> Leave=(f,b)=>{};
            public void OnInit(OutgameFsm<IOutgameProcedureManager> f)=>Init(f);
            public void OnEnter(OutgameFsm<IOutgameProcedureManager> f)=>Enter(f);
            public void OnEnter(OutgameFsm<IOutgameProcedureManager> f,object[] a)=>EnterArgs(f,a);
            public void OnUpdate(OutgameFsm<IOutgameProcedureManager> f,float a,float b)=>Tick(f,a,b);
            public void OnLeave(OutgameFsm<IOutgameProcedureManager> f,bool b)=>Leave(f,b);
            public void OnDestroy(OutgameFsm<IOutgameProcedureManager> f)=>Destroy(f);
        }
        sealed class Second:State{}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-procedure-fsm-registration-start-update-shutdown",()=>{
                var trace=new List<string>();var manager=new OutgameFsmManager();var procedure=new OutgameProcedureManager(()=>manager);
                manager.Initialize();procedure.Initialize();Require(manager.Priority==80&&procedure.Priority==90,"priorities");
                var first=new State{Init=f=>{Require(f.GetState(typeof(State))!=null&&f.GetState(typeof(Second))==null,"incremental registration before OnInit");trace.Add("init1");},Enter=f=>{Require(f.CurrentStateTime==0&&f.CurrentState!=null,"state published before enter");trace.Add("enter1");},Tick=(f,dt,udt)=>{Require(f.CurrentStateTime==.25f&&dt==.25f&&udt==.5f,"time advances before callback");trace.Add("tick");},Leave=(f,shut)=>trace.Add("leave1:"+shut),Destroy=f=>trace.Add("destroy1")};
                var second=new Second{Init=f=>trace.Add("init2"),EnterArgs=(f,args)=>{Require((int)args[0]==7&&f.CurrentStateTime==0,"args forwarded and time reset");trace.Add("enter2args");},Leave=(f,shut)=>trace.Add("leave2:"+shut),Destroy=f=>trace.Add("destroy2")};
                procedure.Register(first,second);procedure.StartProcedure<State>();procedure.Update(10,10);manager.Update(.25f,.5f);procedure.ProcedureFsm.ChangeState<Second>(7);var fsm=procedure.ProcedureFsm;procedure.Shutdown();
                Require(string.Join(",",trace)=="init1,init2,enter1,tick,leave1:False,enter2args,leave2:True,destroy1,destroy2","source lifecycle order");Require(fsm.IsDestroyed&&!fsm.IsRunning&&fsm.CurrentStateTime==0&&manager.Count==0&&!procedure.IsInitialized&&procedure.ProcedureFsm==null,"shutdown registry and owner state");
            });
            test("original-fsm-failure-retains-source-partial-state",()=>{
                var manager=new OutgameFsmManager();var owner=new OutgameProcedureManager(()=>manager);var first=new State();var second=new Second();var fsm=manager.CreateFsm<IOutgameProcedureManager>(owner,first,second);
                first.Enter=f=>throw new InvalidOperationException("enter");Throws<InvalidOperationException>(()=>fsm.Start<State>());Require(fsm.CurrentState==first,"enter failure retains current");
                first.Tick=(f,a,b)=>throw new InvalidOperationException("tick");Throws<InvalidOperationException>(()=>fsm.Update(2,3));Require(fsm.CurrentStateTime==2,"tick failure keeps elapsed time");
                first.Leave=(f,b)=>throw new InvalidOperationException("leave");Throws<InvalidOperationException>(()=>fsm.ChangeState<Second>());Require(fsm.CurrentState==first&&fsm.CurrentStateTime==2,"failed leave prevents transition");
                first.Leave=(f,b)=>{};first.Destroy=f=>throw new InvalidOperationException("destroy");Throws<InvalidOperationException>(()=>manager.DestroyFsm(fsm));Require(!fsm.IsRunning&&!fsm.IsDestroyed&&fsm.GetState(typeof(State))==first&&manager.Count==1,"failed destroy stops final clear and registry removal");
            });
            test("original-fsm-invalid-registration-and-start-guards",()=>{
                var manager=new OutgameFsmManager();var owner=new OutgameProcedureManager(()=>manager);
                Throws<OutgameFrameworkException>(()=>owner.StartProcedure<State>(),"You must initialize procedure first.");
                var bad=new OutgameProcedureManager(()=>null);Throws<OutgameFrameworkException>(()=>bad.Register(new State()),"FSM manager is invalid.");
                int init=0;Throws<OutgameFrameworkException>(()=>manager.CreateFsm<IOutgameProcedureManager>(owner,new State{Init=f=>init++},new State()));Require(init==1&&manager.Count==0,"duplicate after earlier init leaves no registry entry");
                var fsm=manager.CreateFsm<IOutgameProcedureManager>(owner,new State());Throws<OutgameFrameworkException>(()=>fsm.ChangeState<State>(),"Current state is invalid.");Throws<OutgameFrameworkException>(()=>fsm.Start<Second>());Require(!fsm.IsRunning,"missing target leaves current unset");fsm.Start<State>();Throws<OutgameFrameworkException>(()=>fsm.Start<State>(),"FSM is running, can not start again.");Throws<OutgameFrameworkException>(()=>manager.CreateFsm<IOutgameProcedureManager>(owner,new State()));
            });
            test("original-fsm-manager-live-update-and-name-registry",()=>{
                var manager=new OutgameFsmManager();var owner=new OutgameProcedureManager(()=>manager);var trace=new List<string>();
                var first=manager.CreateFsm<IOutgameProcedureManager>("one",owner,new State{Tick=(f,a,b)=>{trace.Add("one");var next=manager.CreateFsm<IOutgameProcedureManager>("two",owner,new State{Tick=(g,c,d)=>trace.Add("two")});next.Start<State>();}});first.Start<State>();manager.Update(1,2);
                Require(string.Join(",",trace)=="one,two"&&manager.HasFsm<IOutgameProcedureManager>("two"),"newly added FSM updates during same live loop");Require(first.FullName==typeof(IOutgameProcedureManager).FullName+".one","source full name");manager.Initialize();manager.Shutdown();Require(manager.Count==0&&!manager.IsInitialized&&first.IsDestroyed,"manager clears and resets flag");
            });return report;
        }
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        static void Throws<T>(Action run,string message=null) where T:Exception
        {try{run();}catch(T ex){if(message!=null)Require(ex.Message==message,"source error text");return;}throw new Exception("Expected "+typeof(T).Name);}
    }
}
