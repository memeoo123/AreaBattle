using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameWebRequestTaskPoolValidation
    {
        sealed class Agent:IOutgameWebRequestAgent
        {
            public OutgameWebRequestTask Task {get;private set;}
            public float FreeWaitTimes {get;set;}
            public int Starts,Resets,Shutdowns,Updates,Initializes,Result=1;
            public Action OnInitialize,OnStart,OnReset,OnShutdown,OnUpdate;
            public void Initialize(){Initializes++;FreeWaitTimes=0;OnInitialize?.Invoke();}
            public int Start(OutgameWebRequestTask task){Starts++;Task=task;OnStart?.Invoke();return Result;}
            public void Update(float delta,float unscaled){Updates++;OnUpdate?.Invoke();}
            public void Reset(){Resets++;OnReset?.Invoke();Task=null;}
            public void Shutdown(){Shutdowns++;OnShutdown?.Invoke();}
        }
        static OutgameWebRequestTask Task(int priority=0)=>OutgameWebRequestTask.Create("task",null,priority,2,null,null,null);
        static void Need(bool b,string why){if(!b)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source generic task pool specialized for actual HTTP task/agent; original FIFO node cache and update failure/reentry ordering. WebRequestManager/platform composition pending."};
            Action<string,Action> check=(id,a)=>{try{a();report.checks.Add(new BattleBuild.Check{id="web-request-pool-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="web-request-pool-"+id,result="fail",detail=e.ToString()});}};
            check("stable-priority-order-and-working-first-info",()=>{
                var pool=new OutgameWebRequestTaskPool();var first=Task(1);var high=Task(10);var equal=Task(10);pool.AddTask(first);pool.AddTask(high);pool.AddTask(equal);var agent=new Agent();pool.AddAgent(agent);pool.Update(0,0);
                var info=pool.GetAllTaskInfos();Need(agent.Task==high&&info.Select(x=>x.SerialId).SequenceEqual(new[]{high.SerialId,equal.SerialId,first.SerialId})&&info.Select(x=>x.Status).SequenceEqual(new[]{1,0,0}),"descending stable order and active before waiting");Need(pool.HaveDownTask(first.SerialId)&&pool.HaveDownTask(high.SerialId)&&!pool.HaveDownTask(-1),"source membership method includes all active/pending");
            });
            check("priority-change-only-waiting-and-equal-no-reorder",()=>{
                var pool=new OutgameWebRequestTaskPool();var a=Task(2);var b=Task(2);var c=Task(1);pool.AddTask(a);pool.AddTask(b);pool.AddTask(c);pool.ChangeTaskPriority(a.SerialId,2);pool.ChangeTaskPriority(c.SerialId,3);Need(pool.GetAllTaskInfos().Select(x=>x.SerialId).SequenceEqual(new[]{c.SerialId,a.SerialId,b.SerialId}),"equal leaves position; changed priority reinserts");var agent=new Agent();pool.AddAgent(agent);pool.Update(0,0);pool.ChangeTaskPriority(c.SerialId,100);Need(c.Priority==3,"working priority remains unchanged");
            });
            check("completion-clears-next-tick-and-starts-next-without-update",()=>{
                var pool=new OutgameWebRequestTaskPool();var a=Task();var b=Task();pool.AddTask(a);pool.AddTask(b);var agent=new Agent();agent.OnUpdate=()=>agent.Task.Done=true;pool.AddAgent(agent);pool.Update(0,0);Need(agent.Updates==0,"new task not updated on start tick");pool.Update(0,0);Need(a.Done&&a.SerialId!=0&&pool.WorkingAgentCount==1&&pool.WaitingTaskCount==1,"done during update stays active this tick");Need(pool.GetAllTaskInfos()[0].Status==2,"info distinguishes completed active task");pool.Update(0,0);Need(a.SerialId==0&&a.Uri==null&&agent.Task==b&&agent.Updates==1&&pool.WaitingTaskCount==0,"next tick clears old then starts new");
            });
            check("start-outcomes-recycle-or-retain",()=>{
                foreach(int result in new[]{0,1,2,3,4,-1}){var pool=new OutgameWebRequestTaskPool();var agent=new Agent{Result=result};var task=Task();pool.AddAgent(agent);pool.AddTask(task);pool.Update(0,0);bool recycle=result==0||result==2||result==3;Need(pool.FreeAgentCount==(recycle?1:0)&&pool.WorkingAgentCount==(recycle?0:1)&&pool.WaitingTaskCount==(result==2||result==4||result==-1?1:0),"source outcome state "+result);Need((task.SerialId==0)==(result==0||result==3),"clear only Done/UnknownError "+result);}
            });
            check("start-exception-keeps-both-queue-references",()=>{
                var pool=new OutgameWebRequestTaskPool();var task=Task();var agent=new Agent{OnStart=()=>throw new InvalidOperationException()};pool.AddAgent(agent);pool.AddTask(task);Throws<InvalidOperationException>(()=>pool.Update(0,0));Need(pool.FreeAgentCount==0&&pool.WorkingAgentCount==1&&pool.WaitingTaskCount==1&&pool.GetAllTaskInfos().All(x=>x.SerialId==task.SerialId),"working published before Start, pending removed only afterward");
            });
            check("pause-gates-updates-dispatch-and-expiry",()=>{
                var pool=new OutgameWebRequestTaskPool{Paused=true};var agent=new Agent();pool.AddAgent(agent);pool.AddTask(Task());pool.Update(300,300);Need(agent.Starts==0&&agent.FreeWaitTimes==0&&agent.Shutdowns==0,"paused gate all three phases");pool.Paused=false;pool.Update(0,0);pool.Paused=true;pool.Update(300,300);Need(agent.Updates==0,"paused working update");
            });
            check("free-expiry-scaled-inclusive-and-return-reset",()=>{
                var pool=new OutgameWebRequestTaskPool();var agent=new Agent();pool.AddAgent(agent);pool.Update(299,1000);Need(agent.Shutdowns==0&&agent.FreeWaitTimes==299,"expiry ignores unscaled");pool.Update(1,0);Need(agent.Shutdowns==1&&pool.TotalAgentCount==0,"300s inclusive");pool.AddAgent(agent);var t=Task();pool.AddTask(t);pool.Update(0,0);t.Done=true;pool.Update(5,0);Need(agent.FreeWaitTimes==5&&pool.FreeAgentCount==1,"return resets before same-tick free aging");
            });
            check("capacity-is-manager-policy-not-pool-cap",()=>{var pool=new OutgameWebRequestTaskPool{Capacity=0};pool.AddAgent(new Agent());pool.AddAgent(new Agent());pool.AddTask(Task());pool.AddTask(Task());pool.Update(0,0);Need(pool.WorkingAgentCount==2&&pool.Capacity==0,"source pool does not enforce capacity in AddAgent/start");});
            check("cancel-waiting-before-active-and-clear-no-callback",()=>{
                var pool=new OutgameWebRequestTaskPool();var a=Task();var b=Task();int aid=a.SerialId,bid=b.SerialId;var agent=new Agent();pool.AddAgent(agent);pool.AddTask(a);pool.AddTask(b);pool.Update(0,0);Need(pool.RemoveTask(bid)&&b.SerialId==0&&agent.Resets==0,"waiting clear without agent reset");Need(pool.RemoveTask(aid)&&a.SerialId==0&&agent.Task==null&&pool.FreeAgentCount==1&&pool.WorkingAgentCount==0,"active cancel reset/return/remove/clear");Need(!pool.RemoveTask(aid),"cancel absent returns false");
            });
            check("remove-all-failure-preserves-partial-state",()=>{
                var pool=new OutgameWebRequestTaskPool();var agent=new Agent();pool.AddAgent(agent);var active=Task();var pending=Task();pool.AddTask(active);pool.AddTask(pending);pool.Update(0,0);agent.OnReset=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(pool.RemoveAllTasks);Need(pending.SerialId==0&&pool.WaitingTaskCount==0&&pool.WorkingAgentCount==1&&active.SerialId!=0&&pool.FreeAgentCount==0,"pending cleared before active reset failure");
            });
            check("shutdown-failure-keeps-current-free-agent",()=>{
                var pool=new OutgameWebRequestTaskPool();var agent=new Agent{OnShutdown=()=>throw new InvalidOperationException()};pool.AddAgent(agent);Throws<InvalidOperationException>(pool.Shutdown);Need(pool.FreeAgentCount==1,"shutdown call before removal");agent.OnShutdown=null;pool.Shutdown();Need(pool.TotalAgentCount==0&&agent.Shutdowns==2,"retry shuts retained agent");
            });
            check("start-reentry-new-tail-waits-until-next-update",()=>{
                var pool=new OutgameWebRequestTaskPool();var first=new Agent();var spare=new Agent();var task=Task();first.OnStart=()=>pool.AddTask(Task(100));pool.AddAgent(first);pool.AddAgent(spare);pool.AddTask(task);pool.Update(0,0);Need(first.Starts==1&&spare.Starts==0&&pool.WaitingTaskCount==1,"next node captured before callback inserts another task");pool.Update(0,0);Need(spare.Starts==1,"later tick starts newly enqueued task");
            });
            check("original-node-fifo-clear-and-reuse",()=>{
                var list=new OutgameRequestLinkedList<object>();var a=list.AddLast(new object());var b=list.AddLast(new object());list.Remove(a);Need(a.Value==null&&a.List==null,"released node cleared and detached");var c=list.AddFirst(new object());Need(c==a,"per-list FIFO node reused");list.Clear();Need(b.Value==null&&c.Value==null&&list.Count==0,"clear releases nodes before clearing BCL list");Need(list.AddLast(new object())==c&&list.AddLast(new object())==b,"clear cache preserves forward order");
            });
            check("actual-agent-timeout-then-next-tick-reuse",()=>{
                var pool=new OutgameWebRequestTaskPool();var h=new OutgameWebRequestAgentValidation.Helper();var a=new OutgameWebRequestAgent(h,new OutgameWebRequestServices{Report=(x,y,z,c,e,t)=>{}});pool.AddAgent(a);var task=Task();var next=Task();pool.AddTask(task);pool.AddTask(next);pool.Update(0,0);pool.Update(0,2);Need(task.Done&&task.Status==3&&pool.WaitingTaskCount==1&&a.Task==task,"real agent timeout stays active until next queue tick");pool.Update(0,0);Need(task.SerialId==0&&task.Headers==null&&a.Task==next&&next.Status==1&&h.Trace.SequenceEqual(new[]{"get","reset","reset","get"}),"real helper reset on timeout and again on queue recycle");pool.Shutdown();
            });
            return report;
        }
    }
}
