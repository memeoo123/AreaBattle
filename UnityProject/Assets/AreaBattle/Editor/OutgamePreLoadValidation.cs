using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgamePreLoadValidation
    {
        sealed class Host:IOutgamePreLoadHost
        {
            public List<string> Trace=new List<string>();public int Key=12;public bool A,B;public Action Complete;public Func<object[],long> Provider;
            public int ArenaRankEvent=>Key;public bool LoginSourceFlag64=>A;public bool LoginSourceFlag65 {get{Trace.Add("flag65");return B;}}
            public void LogProcedure(string m){Trace.Add(m);}
            public void InitializeUserNetModule(){Trace.Add("net");}
            public void RegisterStatisticValue(int key,Func<object[],long> provider){Trace.Add("register:"+key);Provider=provider;}
            public long EventCount(int key){Trace.Add("count:"+key);return key*10L;}
            public void InitializeStatistics(Action complete){Trace.Add("statistics");Complete=complete;}
            public void InitializeActivityWithNullData(){Trace.Add("activity:null");}
            public void LogWarning(string m){Trace.Add(m);}
            public void Handle103VersionBug(){Trace.Add("repair103");}
            public void ClearStateEvents(){Trace.Add("clear-events");}
        }
        sealed class Next:IOutgameFsmState<IOutgameProcedureManager>
        {
            public int Enters;
            public void OnInit(OutgameFsm<IOutgameProcedureManager> f){}
            public void OnEnter(OutgameFsm<IOutgameProcedureManager> f){Enters++;}
            public void OnEnter(OutgameFsm<IOutgameProcedureManager> f,object[] a){throw new Exception("wrong overload");}
            public void OnUpdate(OutgameFsm<IOutgameProcedureManager> f,float a,float b){}
            public void OnLeave(OutgameFsm<IOutgameProcedureManager> f,bool b){}
            public void OnDestroy(OutgameFsm<IOutgameProcedureManager> f){}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-preload-statistics-dynamic-provider-and-native-predicate",()=>{
                var h=new Host();var m=new OutgameFsmManager();var p=new OutgameProcedureManager(()=>m);var next=new Next();
                var pre=new OutgameProcedurePreLoad(h,typeof(Next),wait=>{
                    Require(wait.keepWaiting&&!h.Trace.Contains("flag65"),"short circuit flag64");h.A=true;Require(wait.keepWaiting,"flag65 also required");h.B=true;Require(!wait.keepWaiting,"both flags finish wait");return Task.CompletedTask;
                });p.Register(pre,next);p.StartProcedure<OutgameProcedurePreLoad>();
                Require(h.Trace.Count==4&&h.Trace[1]=="net"&&h.Trace[2]=="register:12"&&h.Trace[3]=="statistics"&&next.Enters==0,"source enter order, statistics callback gates activity");
                h.Key=21;Require(h.Provider(new object[]{"ignored"})==210,"provider reads current config key and ignores args");h.Complete();Require(next.Enters==1&&p.ProcedureFsm.CurrentState==next,"actual FSM transition");
                Require(h.Trace.IndexOf("activity:null")<h.Trace.IndexOf("LoginComplete")&&h.Trace.IndexOf("LoginComplete")<h.Trace.IndexOf("repair103")&&h.Trace[h.Trace.Count-1]=="Leave 'Proj_hdzd.Procedure.ProcedurePreLoad' procedure.","completion order");
            });
            test("original-preload-pending-await-preserves-state",()=>{
                var saved=SynchronizationContext.Current;SynchronizationContext.SetSynchronizationContext(null);
                try{
                    var h=new Host();var m=new OutgameFsmManager();var p=new OutgameProcedureManager(()=>m);var next=new Next();var completion=new TaskCompletionSource<object>();var pre=new OutgameProcedurePreLoad(h,typeof(Next),wait=>completion.Task);p.Register(pre,next);p.StartProcedure<OutgameProcedurePreLoad>();
                    var task=pre.ContinueAfterStatisticsAsync();Require(!task.IsCompleted&&p.ProcedureFsm.CurrentState==pre&&!h.Trace.Contains("repair103"),"suspended wait cannot enter next or repair");completion.SetResult(null);task.GetAwaiter().GetResult();Require(next.Enters==1,"resumes only after await completion");
                }finally{SynchronizationContext.SetSynchronizationContext(saved);}
            });
            test("original-preload-await-failure-does-not-repair-or-transition",()=>{
                var h=new Host();var m=new OutgameFsmManager();var p=new OutgameProcedureManager(()=>m);var next=new Next();var pre=new OutgameProcedurePreLoad(h,typeof(Next),wait=>Task.FromException(new InvalidOperationException("await failure")));p.Register(pre,next);p.StartProcedure<OutgameProcedurePreLoad>();
                bool failed=false;try{pre.ContinueAfterStatisticsAsync().GetAwaiter().GetResult();}catch(InvalidOperationException){failed=true;}
                Require(failed&&next.Enters==0&&!h.Trace.Contains("LoginComplete")&&!h.Trace.Contains("repair103")&&p.ProcedureFsm.CurrentState==pre,"awaiter exception propagates before completion actions");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
