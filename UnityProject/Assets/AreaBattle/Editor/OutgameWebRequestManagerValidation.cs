using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Helper=AreaBattle.EditorTools.OutgameWebRequestAgentValidation.Helper;
namespace AreaBattle.EditorTools
{
    public static class OutgameWebRequestManagerValidation
    {
        public sealed class BadJson {public string Value=>throw new InvalidOperationException("bad-json");}
        sealed class Fixture
        {
            public readonly List<string> Trace=new List<string>();public readonly List<Helper> Helpers=new List<Helper>();
            public bool Debug,KeWan;public Action<string> Logging,Error;
            public readonly OutgameWebRequestManager Manager;
            public readonly OutgameWebRequestManagerServices Services;
            public Fixture(){Services=new OutgameWebRequestManagerServices{AgentServices=new OutgameWebRequestServices{Report=(a,b,c,d,e,f)=>Trace.Add("report")},IsKeWan=()=>KeWan,DebugEnabled=()=>Debug,CreateHelper=()=>{var h=new Helper();Helpers.Add(h);Trace.Add("create");return h;},Log=s=>{Trace.Add("log:"+s);Logging?.Invoke(s);},NetLog=s=>{Trace.Add("net:"+s);Logging?.Invoke(s);},Error=s=>{Trace.Add("error:"+s);Error?.Invoke(s);},CategoryLog=(c,s)=>{Trace.Add("category:"+c+":"+s);Logging?.Invoke(s);},Warning=s=>{Trace.Add("warning:"+s);Logging?.Invoke(s);},Toast=s=>Trace.Add("toast:"+s),ClearSingleton=()=>Trace.Add("clear"),DestroyOwner=()=>Trace.Add("destroy")};Manager=new OutgameWebRequestManager(Services);Manager.Initialize();Trace.Clear();}
        }
        static void Need(bool b,string why){if(!b)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original WebRequestManager owner API, object/string/byte serialization, event clearing, capacity and debug request counting. Native singleton and actual HTTP tested separately. HttpManager/platform composition pending."};
            Action<string,Action> check=(id,a)=>{try{a();report.checks.Add(new BattleBuild.Check{id="web-request-manager-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="web-request-manager-"+id,result="fail",detail=e.ToString()});}};
            check("init-log-before-pool-and-source-defaults",()=>{var f=new Fixture();Need(f.Manager.Timeout==30&&f.Manager.WorkingTaskCapacity==20&&f.Manager.DebugElapsed==-1&&f.Manager.Headers==null,"source defaults");var old=f.Manager.Pool;f.Logging=s=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Manager.Initialize);Need(f.Manager.Pool==old,"failed reinit log retains old pool");});
            check("invalid-url-before-helper-and-task",()=>{var f=new Fixture();Throws<OutgameFrameworkException>(()=>f.Manager.AddWebRequest("",(Action<int,object>)null));Throws<OutgameFrameworkException>(()=>f.Manager.AddWebRequest(null,(Action<int,object>)null));Need(f.Helpers.Count==0&&f.Manager.WaitingTaskCount==0,"invalid URI never allocates helper/task");});
            check("headers-log-before-mutation-and-queue-snapshot",()=>{
                var f=new Fixture();f.Manager.SetRequestHeader("A","one");f.Manager.AddWebRequest("uri",(Action<int,object>)null);f.Manager.SetRequestHeader("A","two");f.Manager.ClearRequestHeader();f.Manager.Update(0,0);Need(f.Helpers[0].Headers["A"]=="one"&&f.Manager.Headers.Count==0,"enqueued request retains header snapshot");f.Logging=s=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Manager.SetRequestHeader("B","x"));Need(!f.Manager.Headers.ContainsKey("B"),"log failure precedes dictionary mutation");
            });
            check("capacity-boundary-orphan-helper-and-pending-request",()=>{
                var f=new Fixture();f.Manager.WorkingTaskCapacity=1;f.Manager.AddWebRequest("first",(Action<int,object>)null);f.Manager.Update(0,0);f.Manager.AddWebRequest("second",(Action<int,object>)null);Need(f.Helpers.Count==2&&f.Manager.TotalAgentCount==1&&f.Manager.WaitingTaskCount==1&&f.Helpers[1].Trace.Count==0&&f.Trace.Any(x=>x.StartsWith("category:131072:")),"source creates helper at equality then declines agent without destroying helper");
                f.Manager.WorkingTaskCapacity=0;f.Manager.AddWebRequest("third",(Action<int,object>)null);Need(f.Helpers.Count==2,"over capacity skips helper creation entirely");
            });
            check("null-helper-and-initialize-failure",()=>{var f=new Fixture();f.Services.CreateHelper=()=>null;Throws<OutgameFrameworkException>(()=>f.Manager.AddWebRequest("uri",(Action<int,object>)null));Need(f.Manager.WaitingTaskCount==0,"helper failure precedes task allocation");f.Manager.WorkingTaskCapacity=0;f.Manager.AddAgentHelper(null);Need(f.Manager.TotalAgentCount==0,"capacity rejection precedes helper null check");});
            check("object-json-default-priority-and-user-data",()=>{
                var f=new Fixture();var user=new object();OutgameHttpEvent held=null;int id=0;id=f.Manager.AddWebRequest("uri",(object)new OutgameHttpResponse{code=8,msg="原版"},(s,e)=>{held=(OutgameHttpEvent)e;Need(s==0&&held.SerialId==id&&held.UserData==user,"start args");},user);
                Need(f.Manager.GetAllWebRequestInfos()[0].Priority==0,"object default priority");f.Manager.Update(0,0);var json=Encoding.UTF8.GetString(f.Helpers[0].Bytes);Need(LitJson.JsonMapper.ToObject<OutgameHttpResponse>(json).msg=="原版"&&held.SerialId==0&&held.UserData==null&&held.Uri==null,"JSON UTF8 and start event cleared after callback");
            });
            check("serialization-error-fallback-and-error-handler-failure",()=>{
                var f=new Fixture();f.Manager.AddWebRequest("uri",(object)new BadJson(),null,null);f.Manager.Update(0,0);Need(Encoding.UTF8.GetString(f.Helpers[0].Bytes)=="{}"&&f.Trace.Any(x=>x.StartsWith("error:JSON序列化失败：$")),"object serializer catches and sends literal empty object");var g=new Fixture();g.Error=s=>throw new ApplicationException();Throws<ApplicationException>(()=>g.Manager.AddWebRequest("uri",(object)new BadJson(),null,null));Need(g.Helpers.Count==0,"logging error aborts before helper allocation");
            });
            check("priority-object-overload-does-not-catch-serializer",()=>{var f=new Fixture();Throws<TargetInvocationException>(()=>f.Manager.AddWebRequest("uri",(object)new BadJson(),7,null));Need(f.Helpers.Count==0&&!f.Trace.Any(x=>x.StartsWith("error:")),"priority overload propagates serialization error");});
            check("string-overload-inverted-null-empty-nonempty",()=>{
                var f=new Fixture();Throws<ArgumentNullException>(()=>f.Manager.AddWebRequest("uri",(string)null,null,null));Need(f.Helpers.Count==0,"null string fails UTF8 encode");f.Manager.AddWebRequest("nonempty","body",null,null);f.Manager.Update(0,0);Need(f.Helpers[0].Trace.Single()=="get","nonempty string produces null body GET");var g=new Fixture();g.Manager.AddWebRequest("empty",string.Empty,null,null);g.Manager.Update(0,0);Need(g.Helpers[0].Trace.Single()=="post"&&g.Helpers[0].Bytes.Length==0,"empty string encodes to empty POST");
            });
            check("success-event-field-lifetime-and-fresh-allocation",()=>{
                var f=new Fixture();var events=new List<OutgameHttpEvent>();var bytes=new byte[]{1,2};int successId=0;f.Manager.AddWebRequest("uri",(byte[])null,0,"user",(state,e)=>{events.Add((OutgameHttpEvent)e);if(state==1){var s=(OutgameHttpSuccess)e;successId=s.SerialId;Need(s.Uri=="uri"&&(string)s.UserData=="user"&&s.Bytes==bytes,"success fields valid during callback");}});f.Manager.Update(0,0);f.Helpers[0].CompleteWith(OutgameWebRequestCompleteEventArgs.Create(bytes));Need(events.Count==2&&events[0]!=events[1]&&successId!=0&&events.All(x=>x.SerialId==0)&&((OutgameHttpSuccess)events[1]).Bytes==null&&f.Manager.GetAllWebRequestInfos()[0].Status==2,"events clear after normal callback, task done only afterward");
            });
            check("failure-callback-throw-retains-event-and-undone-task",()=>{
                var f=new Fixture();OutgameHttpFailure held=null;f.Manager.AddWebRequest("uri",(byte[])null,0,"user",(state,e)=>{if(state==2){held=(OutgameHttpFailure)e;throw new InvalidOperationException();}});f.Manager.Update(0,0);Throws<InvalidOperationException>(()=>f.Helpers[0].ErrorWith(OutgameWebRequestErrorEventArgs.Create("bad")));Need(held.SerialId!=0&&held.Error=="bad"&&(string)held.UserData=="user"&&f.Manager.GetAllWebRequestInfos()[0].Status==1,"callback throw skips both event Clear and task Done");
            });
            check("start-callback-throw-keeps-working-and-waiting",()=>{
                var f=new Fixture();OutgameHttpStart held=null;f.Manager.AddWebRequest("uri",(state,e)=>{held=(OutgameHttpStart)e;throw new InvalidOperationException();});Throws<InvalidOperationException>(()=>f.Manager.Update(0,0));Need(held.SerialId!=0&&f.Manager.WaitingTaskCount==1&&f.Manager.WorkingAgentCount==1&&f.Helpers[0].Trace.Count==0,"source publish before callback, no request send or event cleanup");
            });
            check("debug-counts-warn-after-limit-toast-once",()=>{
                var f=new Fixture{Debug=true};for(int i=0;i<7;i++)f.Manager.AddWebRequest("https://fixture/data/user/ext",(Action<int,object>)null);Need(f.Manager.RequestCounts["/data/user/ext"]==7&&f.Trace.Count(x=>x.StartsWith("warning:"))==2&&f.Trace.Count(x=>x.StartsWith("toast:"))==1,"sixth/seventh warn; sixth only toast");f.Manager.AddWebRequest("https://fixture/mini-toplist/knock",(Action<int,object>)null);Need(f.Manager.RequestCounts["/toplist/knock"]==0,"legacy toplist monitor does not match mini-toplist");
            });
            check("debug-warning-failure-before-count-and-task",()=>{
                var f=new Fixture{Debug=true};for(int i=0;i<5;i++)f.Manager.AddWebRequest("/data/user/ext",(Action<int,object>)null);f.Logging=s=>{if(s.Contains("一分钟"))throw new InvalidOperationException();};Throws<InvalidOperationException>(()=>f.Manager.AddWebRequest("/data/user/ext",(Action<int,object>)null));Need(f.Manager.RequestCounts["/data/user/ext"]==5&&f.Manager.WaitingTaskCount==5,"warning failure preserves count and skips task");
            });
            check("debug-clock-ke-wan-gate-and-reset-on-next-request",()=>{
                var f=new Fixture{Debug=true,KeWan=true};f.Manager.AddWebRequest("/data/user/ext",(Action<int,object>)null);f.Manager.Update(100,100);Need(f.Manager.DebugElapsed==0&&f.Manager.WaitingTaskCount==1,"KeWan gates clock and pool");f.KeWan=false;f.Manager.Update(60,0);Need(f.Manager.DebugElapsed==60,"timer increments to60 without same-tick reset");f.Manager.Update(0,0);Need(f.Manager.DebugElapsed==-1&&f.Manager.RequestCounts["/data/user/ext"]==1,"next update sets sentinel retaining counters");f.Manager.AddWebRequest("untracked",(Action<int,object>)null);Need(f.Manager.DebugElapsed==0&&f.Manager.RequestCounts.Values.All(x=>x==0),"next debug request resets counters even if URL untracked");
            });
            check("large-object-byte-threshold-debug-only",()=>{
                var f=new Fixture{Debug=true};f.Manager.AddWebRequest("uri",(object)new OutgameHttpResponse{msg=new string('界',5000)},null,null);Need(f.Trace.Count(x=>x.Contains("大于14KB"))==1,"UTF8 byte threshold");var g=new Fixture();g.Manager.AddWebRequest("uri",(object)new OutgameHttpResponse{msg=new string('界',5000)},null,null);Need(!g.Trace.Any(x=>x.Contains("大于14KB")),"warning debug-gated");
            });
            check("shutdown-pool-before-singleton-before-destroy",()=>{var f=new Fixture();f.Manager.AddWebRequest("uri",(Action<int,object>)null);f.Manager.Update(0,0);f.Helpers[0].OnReset=()=>{Need(!f.Trace.Contains("clear"),"reset before singleton clear");};f.Manager.Shutdown();Need(f.Manager.TotalAgentCount==0&&f.Manager.WaitingTaskCount==0&&f.Trace.TakeLast(2).SequenceEqual(new[]{"clear","destroy"}),"shutdown ordering");});
            check("source-empty-node-error",()=>{try{new OutgameRequestLinkedList<object>().RemoveFirst();throw new Exception("expected exception");}catch(OutgameFrameworkException e){Need(e.Message=="First is invalid.","original empty-node message");}});
            return report;
        }
    }
}
