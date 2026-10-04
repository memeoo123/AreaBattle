using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameWebRequestAgentValidation
    {
        public sealed class Helper:IOutgameWebRequestAgentHelper
        {
            public event EventHandler<OutgameWebRequestCompleteEventArgs> Complete;
            public event EventHandler<OutgameWebRequestErrorEventArgs> Error;
            public readonly List<string> Trace=new List<string>();
            public Action OnReset,OnRequest;
            public string Uri;public byte[] Bytes;public Dictionary<string,string> Headers;
            public void Request(string uri,Dictionary<string,string> headers){Uri=uri;Headers=headers;Trace.Add("get");OnRequest?.Invoke();}
            public void Request(string uri,byte[] bytes,Dictionary<string,string> headers){Uri=uri;Bytes=bytes;Headers=headers;Trace.Add("post");OnRequest?.Invoke();}
            public void Reset(){Trace.Add("reset");OnReset?.Invoke();}
            public void Shutdown(){Trace.Add("shutdown");}
            public void CompleteWith(OutgameWebRequestCompleteEventArgs e){Complete?.Invoke(this,e);}
            public void ErrorWith(OutgameWebRequestErrorEventArgs e){Error?.Invoke(this,e);}
        }
        static void Need(bool b,string reason){if(!b)throw new Exception(reason);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameWebRequestTask Task(byte[] bytes=null,float timeout=1)=>OutgameWebRequestTask.Create("http://fixture.invalid",bytes,4,timeout,new object(),null,null);
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original task and agent ordering with explicit helper fixtures; native HTTP IO is checked separately. Queue/manager/platform composition remains pending."};
            Action<string,Action> check=(id,a)=>{try{a();report.checks.Add(new BattleBuild.Check{id="web-request-agent-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="web-request-agent-"+id,result="fail",detail=e.ToString()});}};
            check("task-copy-alias-and-clear-retains-headers",()=>{
                var headers=new Dictionary<string,string>{{"a","first"}};var body=new byte[]{1};var user=new object();Action<int,object> cb=(i,o)=>{};
                var t=OutgameWebRequestTask.Create("uri",body,3,2,user,cb,headers);int id=t.SerialId;headers["a"]="second";body[0]=9;
                Need(t.Headers["a"]=="first"&&t.PostData[0]==9&&ReferenceEquals(t.UserData,user)&&t.ResultCallback==cb,"headers snapshot but data aliases");
                Need(Task().SerialId==unchecked(id+1),"serial increments globally");t.Status=3;t.Done=true;var copy=t.Headers;t.Clear();
                Need(t.SerialId==0&&t.Priority==0&&!t.Done&&t.Status==0&&t.Uri==null&&t.PostData==null&&t.Timeout==0&&t.ResultCallback==null&&t.UserData==null&&t.Headers==copy,"source clear preserves header dictionary");
            });
            check("fresh-events-clear-reference-only",()=>{
                var bytes=new byte[]{7};var a=OutgameWebRequestCompleteEventArgs.Create(bytes);var b=OutgameWebRequestCompleteEventArgs.Create(bytes);a.Clear();
                var err=OutgameWebRequestErrorEventArgs.Create("bad");err.Clear();Need(a!=b&&a.Bytes==null&&ReferenceEquals(b.Bytes,bytes)&&bytes[0]==7&&err.ErrorMessage==null,"fresh args clear without changing aliased data");
            });
            check("invalid-helper-task-and-idle-update",()=>{Throws<OutgameFrameworkException>(()=>new OutgameWebRequestAgent(null,null));var a=new OutgameWebRequestAgent(new Helper(),null);Throws<OutgameFrameworkException>(()=>a.Start(null));Throws<NullReferenceException>(()=>a.Update(0,1));});
            check("start-event-before-dispatch-and-null-versus-empty-body",()=>{
                var h=new Helper();var a=new OutgameWebRequestAgent(h,null);a.Initialize();a.Started=x=>{Need(x.Task.Status==1,"status before start event");h.Trace.Add("start");};var t=Task();Need(a.Start(t)==1,"Start returns source enum value1");a.Start(Task(new byte[0]));
                Need(h.Trace.SequenceEqual(new[]{"start","get","start","post"})&&h.Bytes.Length==0,"empty byte array uses POST; null uses GET");
            });
            check("start-throw-retains-working-task",()=>{var h=new Helper();var a=new OutgameWebRequestAgent(h,null);var t=Task();a.Started=x=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>a.Start(t));Need(a.Task==t&&t.Status==1&&!t.Done&&h.Trace.Count==0,"start failure keeps published task without request");});
            check("completion-reset-before-callback-and-done-after",()=>{
                var h=new Helper();var a=new OutgameWebRequestAgent(h,null);a.Initialize();var t=Task();a.Start(t);var bytes=new byte[]{3};a.Succeeded=(x,b)=>{Need(h.Trace.Last()=="reset"&&t.Status==2&&!t.Done&&b==bytes,"prefix before success");};h.CompleteWith(OutgameWebRequestCompleteEventArgs.Create(bytes));Need(t.Done&&a.Task==t,"done after callback, task retained for queue");
            });
            check("completion-throw-skips-done",()=>{var h=new Helper();var a=new OutgameWebRequestAgent(h,null);a.Initialize();var t=Task();a.Start(t);a.Failed=(x,e)=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>h.ErrorWith(OutgameWebRequestErrorEventArgs.Create("http")));Need(t.Status==3&&!t.Done&&h.Trace.Last()=="reset","error callback throw preserves failed status and false Done");});
            check("reset-throw-keeps-task-and-status",()=>{var h=new Helper();var a=new OutgameWebRequestAgent(h,null);a.Initialize();var t=Task();a.Start(t);h.OnReset=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>h.CompleteWith(OutgameWebRequestCompleteEventArgs.Create(null)));Need(a.Task==t&&t.Status==1&&!t.Done,"helper reset before all task mutations");});
            check("callback-reentry-done-applies-to-current-task",()=>{
                var h=new Helper();var a=new OutgameWebRequestAgent(h,null);a.Initialize();var old=Task();var next=Task();a.Start(old);a.Succeeded=(x,b)=>x.Start(next);h.CompleteWith(OutgameWebRequestCompleteEventArgs.Create(null));Need(old.Status==2&&!old.Done&&next.Status==1&&next.Done&&a.Task==next,"done target reread after callback");
            });
            check("timeout-unscaled-equality-report-before-reset",()=>{
                var h=new Helper();int count=0;var s=new OutgameWebRequestServices{Report=(kind,uri,state,code,error,ms)=>{Need(h.Trace.Last()=="get"&&state=="Fail"&&code==0&&error=="Timeout"&&ms==1000,"source timeout report prefix");count++;}};
                var a=new OutgameWebRequestAgent(h,s);a.Initialize();var t=Task();a.Start(t);a.Update(100,.75f);Need(count==0,"scaled time ignored");a.Update(0,.25f);Need(count==1&&t.Done&&t.Status==3,"inclusive unscaled deadline");a.Update(0,100);Need(count==1,"failed task ignores further updates");
            });
            check("timeout-report-failure-retries-before-reset",()=>{
                var h=new Helper();int count=0;var s=new OutgameWebRequestServices{Report=(a,b,c,d,e,f)=>{count++;throw new InvalidOperationException();}};var agent=new OutgameWebRequestAgent(h,s);agent.Initialize();var t=Task();agent.Start(t);Throws<InvalidOperationException>(()=>agent.Update(0,1));Throws<InvalidOperationException>(()=>agent.Update(0,0));Need(count==2&&t.Status==1&&!t.Done&&h.Trace.Count==1,"elapsed and task remain for next retry");
            });
            check("start-reset-timeout-and-shutdown-unsubscribe",()=>{
                var h=new Helper();int count=0;var s=new OutgameWebRequestServices{Report=(a,b,c,d,e,f)=>count++};var agent=new OutgameWebRequestAgent(h,s);agent.Initialize();agent.Start(Task());agent.Update(0,.9f);var t=Task();agent.Start(t);agent.Update(0,.2f);Need(count==0&&!t.Done,"new start resets elapsed after dispatch");agent.FreeWaitTimes=7;agent.Shutdown();Need(agent.Task==null&&agent.FreeWaitTimes==7&&h.Trace.TakeLast(2).SequenceEqual(new[]{"reset","shutdown"}),"shutdown retains free wait but removes current task");h.CompleteWith(OutgameWebRequestCompleteEventArgs.Create(null));h.ErrorWith(OutgameWebRequestErrorEventArgs.Create(null));
            });
            check("original-wasm-millisecond-conversion",()=>{Need(OutgameWebRequestServices.Milliseconds(.1239f)==123&&OutgameWebRequestServices.Milliseconds(-.1239f)==-123&&OutgameWebRequestServices.Milliseconds(float.NaN)==int.MinValue&&OutgameWebRequestServices.Milliseconds(float.PositiveInfinity)==int.MinValue,"f32 multiply, truncate and WASM overflow sentinel");});
            return report;
        }
    }
}
