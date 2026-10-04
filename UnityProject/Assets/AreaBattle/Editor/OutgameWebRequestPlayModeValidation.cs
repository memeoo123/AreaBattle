using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameWebRequestPlayModeValidation
    {
        const string Pending="AreaBattle.WebRequestNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Real UnityWebRequest GET/POST/HTTP503/timeout with original helper/task/agent/queue, and real AES/rank protocol exchange over a controlled loopback TCP HTTP server. Manager/platform URL-key/report delivery and production Main are not claimed.";
            public List<string> checks=new List<string>();
        }
        public sealed class Received
        {public string Method,Path,Body;public Dictionary<string,string> Headers=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);}
        public sealed class Server:IDisposable
        {
            readonly TcpListener listener=new TcpListener(IPAddress.Loopback,0);
            readonly CancellationTokenSource cancel=new CancellationTokenSource();
            public readonly ConcurrentQueue<Received> Requests=new ConcurrentQueue<Received>();
            public readonly ConcurrentQueue<Exception> Errors=new ConcurrentQueue<Exception>();
            public readonly string Url,RankReply;
            readonly Func<Received,string> responseBody;
            public Server(string rankReply,Func<Received,string> responseBody=null){RankReply=rankReply;this.responseBody=responseBody;listener.Start();Url="http://127.0.0.1:"+((IPEndPoint)listener.LocalEndpoint).Port;_=Accept();}
            async Task Accept()
            {
                while(!cancel.IsCancellationRequested){try{var client=await listener.AcceptTcpClientAsync();_=Serve(client);}catch(Exception e){if(!cancel.IsCancellationRequested)Errors.Enqueue(e);}}
            }
            async Task Serve(TcpClient client)
            {
                using(client)try{
                    var stream=client.GetStream();var raw=new List<byte>();int value;
                    while((value=stream.ReadByte())!=-1){raw.Add((byte)value);int n=raw.Count;if(n>=4&&raw[n-4]==13&&raw[n-3]==10&&raw[n-2]==13&&raw[n-1]==10)break;if(n>65536)throw new Exception("header too long");}
                    var lines=Encoding.ASCII.GetString(raw.ToArray()).Split(new[]{"\r\n"},StringSplitOptions.None);var first=lines[0].Split(' ');var request=new Received{Method=first[0],Path=first[1]};
                    foreach(var line in lines.Skip(1)){int at=line.IndexOf(':');if(at>0)request.Headers[line.Substring(0,at)]=line.Substring(at+1).Trim();}
                    int length=request.Headers.TryGetValue("Content-Length",out var size)?int.Parse(size):0;
                    if(request.Headers.TryGetValue("Expect",out var expect)&&expect.Equals("100-continue",StringComparison.OrdinalIgnoreCase)){var interim=Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");await stream.WriteAsync(interim,0,interim.Length);}
                    var body=new byte[length];int offset=0;while(offset<length){int read=await stream.ReadAsync(body,offset,length-offset);if(read==0)throw new EndOfStreamException();offset+=read;}
                    request.Body=Encoding.UTF8.GetString(body);Requests.Enqueue(request);
                    if(request.Path=="/slow")await Task.Delay(30000,cancel.Token);
                    string response=responseBody!=null?responseBody(request):request.Path.StartsWith("/mini-toplist/")?RankReply:"{\"code\":0,\"msg\":\"本地真实响应\"}";
                    byte[] bytes=Encoding.UTF8.GetBytes(response);string status=request.Path=="/fail"?"503 Service Unavailable":"200 OK";
                    var head=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\nContent-Type: application/json; charset=UTF-8\r\nContent-Length: "+bytes.Length+"\r\nConnection: close\r\n\r\n");await stream.WriteAsync(head,0,head.Length);await stream.WriteAsync(bytes,0,bytes.Length);
                }catch(Exception e){if(!cancel.IsCancellationRequested&&!(e is IOException))Errors.Enqueue(e);}
            }
            public Received Take(){if(!Requests.TryDequeue(out var r))throw new Exception("HTTP server did not receive expected request");return r;}
            public void Dispose(){cancel.Cancel();listener.Stop();}
        }
        static Report report;static Server server;static OutgameUnityWebRequestAgentHelper helper;static OutgameWebRequestAgent agent;static OutgameWebRequestServices services;
        static readonly List<string> trace=new List<string>();static readonly List<int> codes=new List<int>();
        static OutgameWebRequestTaskPool pool;static OutgameWebRequestTask queueLow,queueHigh;static OutgameWebRequestTask task;static OutgameWebRequestCompleteEventArgs held;static byte[] received;static string failure;static int phase,lastFrame,callbacks;static double started;static bool stopped;
        static OutgameRanklistTransmitter rank;static OutgameHttpCipher cipher;static OutgameGetRankResponse rankResponse;
        static OutgameWebRequestPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string why){if(!condition)throw new Exception(why);report.checks.Add(why);}
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;phase=0;lastFrame=-1;callbacks=0;trace.Clear();codes.Clear();Time.timeScale=1;
            try{
                var txServices=new OutgameHttpTransmitterServices{EncryptionEnabled=()=>true,LogEnabled=()=>false,Key=()=>"abc",NetLog=a=>{},Warning=a=>{},Log=a=>{},Messages=()=>new OutgameMessageDispatcher(),NewErrorReport=()=>new OutgameHttpErrorRecord(),ReportError=r=>{throw new Exception("unexpected rank error");},ReportCryptoError=(c,m)=>{throw new Exception("crypto "+c+":"+m);},CryptoError=e=>{throw e;}};
                cipher=new OutgameHttpCipher(txServices);string reply="{\"code\":0,\"data\":{\"count\":2,\"no\":7,\"score\":123,\"list\":[{\"no\":1,\"score\":999}]}}";
                server=new Server(LitJson.JsonMapper.ToJson(new OutgameHttpEncryptedData{encrypt=cipher.Encrypt(reply,"abc")}));
                services=new OutgameWebRequestServices{EncryptionEnabled=()=>true,Log=s=>trace.Add("log:"+s),Error=s=>trace.Add("error:"+s),Report=(kind,uri,state,code,error,ms)=>{if(ms<0)throw new Exception("negative duration");trace.Add("report:"+state+":"+code);codes.Add(code);}};
                helper=new GameObject("OriginalHttpHelper").AddComponent<OutgameUnityWebRequestAgentHelper>();helper.Services=services;
                helper.Request(server.Url+"/unused",new byte[]{1},null);Check(helper.RequestHandle==null&&trace.Count==0,"POST with missing handlers silently leaves request null");
                helper.Request(server.Url+"/unused",null);Check(helper.RequestHandle==null&&trace.Single()=="error:Web request agent helper handler is invalid.","GET with missing handlers logs original error without sending");trace.Clear();
                agent=new OutgameWebRequestAgent(helper,services);pool=new OutgameWebRequestTaskPool();pool.AddAgent(agent);
                agent.Started=a=>trace.Add("start");agent.Succeeded=(a,b)=>{Check(helper.RequestHandle==null&&a.Task.Status==2&&!a.Task.Done,"Native success disposes request and sets status before callback");received=b;callbacks++;trace.Add("success");};
                agent.Failed=(a,e)=>{Check(helper.RequestHandle==null&&a.Task.Status==3&&!a.Task.Done,"Native failure disposes request and sets status before callback");failure=e;callbacks++;trace.Add("failure");};
                helper.Complete+=(s,e)=>held=e;
                txServices.AddWebRequest=(url,data,cb,user)=>{
                    task=OutgameWebRequestTask.Create(url,Encoding.UTF8.GetBytes(LitJson.JsonMapper.ToJson(data)),0,5,user,cb,null);pool.AddTask(task);return task.SerialId;
                };
                rank=new OutgameRanklistTransmitter(txServices,()=>20);rank.RanklistName="local-rank";rank.Uid="local-user";rank.SetUrl(server.Url);rank.InitProtocol();
                task=OutgameWebRequestTask.Create(server.Url+"/get",null,0,5,null,null,new Dictionary<string,string>{{"X-Test","source"}});pool.AddTask(task);
                EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Next(int value){phase=value;trace.Clear();held=null;received=null;failure=null;}
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>60)throw new TimeoutException("HTTP phase "+phase);
                if(server.Errors.TryDequeue(out var error))throw error;
                if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
                if(!task.Done)pool.Update(Time.deltaTime,Time.unscaledDeltaTime);
                if(phase==0&&task.Done){
                    var wire=server.Take();Check(wire.Method=="GET"&&wire.Path=="/get"&&wire.Headers["Content-Type"]=="application/json;charset=UTF-8"&&!wire.Headers.ContainsKey("X-Encrypted")&&wire.Headers["X-Test"]=="source","Real GET preserves default content type and custom header without POST encryption flag");
                    Check(Encoding.UTF8.GetString(received).Contains("本地真实响应")&&held!=null&&held.Bytes==null,"Real downloaded UTF8 bytes survive event Clear while retained event data is cleared");
                    Check(trace.SequenceEqual(new[]{"start","report:Success:200","success"})&&task.Status==2,"Real HTTP200 reports before callback and marks task done afterward");
                    Next(1);task=OutgameWebRequestTask.Create(server.Url+"/post",Encoding.UTF8.GetBytes("{\"value\":\"原版\"}"),0,5,null,null,null);pool.AddTask(task);return;
                }
                if(phase==1&&task.Done){
                    var wire=server.Take();Check(wire.Method=="POST"&&wire.Headers["X-Encrypted"]=="true"&&wire.Headers["Content-Type"]=="application/json;charset=UTF-8"&&wire.Body=="{\"value\":\"原版\"}","Real POST transmits exact UTF8 body and original encrypted header");
                    Next(2);task=OutgameWebRequestTask.Create(server.Url+"/override",new byte[0],0,5,null,null,new Dictionary<string,string>{{"Content-Type","text/plain"},{"X-Encrypted","custom"}});pool.AddTask(task);return;
                }
                if(phase==2&&task.Done){
                    var wire=server.Take();Check(wire.Method=="POST"&&wire.Body==string.Empty&&wire.Headers["Content-Type"]=="text/plain"&&wire.Headers["X-Encrypted"]=="custom","Custom headers override defaults and empty bytes still produce POST");
                    Next(3);task=OutgameWebRequestTask.Create(server.Url+"/fail",null,0,5,null,null,null);pool.AddTask(task);return;
                }
                if(phase==3&&task.Done){
                    Check(server.Take().Path=="/fail"&&task.Status==3&&!string.IsNullOrEmpty(failure)&&trace.SequenceEqual(new[]{"start","report:Fail:503","failure"}),"Actual HTTP503 reaches failure callback with source report-before-callback order");
                    Next(4);Time.timeScale=0;task=OutgameWebRequestTask.Create(server.Url+"/slow",null,0,.3f,null,null,null);pool.AddTask(task);return;
                }
                if(phase==4&&task.Done){
                    Check(server.Take().Path=="/slow"&&failure=="Timeout"&&Time.timeScale==0&&trace.SequenceEqual(new[]{"start","report:Fail:0","failure"}),"Source unscaled timeout disposes actual stalled request while game time is paused");Time.timeScale=1;
                    Next(5);rank.GetRankResponse=r=>{rankResponse=r;trace.Add("rank");};
                    agent.Succeeded=(a,b)=>{trace.Add("success");a.Task.ResultCallback(1,new OutgameHttpSuccess{SerialId=a.Task.SerialId,UserData=a.Task.UserData,Bytes=b});};
                    rank.GetRankDataRequest(1,2,2);return;
                }
                if(phase==5&&task.Done){
                    var wire=server.Take();var envelope=LitJson.JsonMapper.ToObject<OutgameHttpEncryptedData>(wire.Body);var request=LitJson.JsonMapper.ToObject<OutgameGetRankRequest>(cipher.Decrypt(envelope.encrypt,"abc"));
                    Check(wire.Path=="/mini-toplist/query"&&wire.Headers["X-Encrypted"]=="true"&&request.name=="local-rank"&&request.uid=="local-user"&&request.category==1&&request.page==2&&request.count==2,"Actual rank request goes through AES envelope, task, queue, agent, Unity HTTP and independent server bytes");
                    Check(rankResponse!=null&&rankResponse.data.no==7&&rankResponse.data.list[0].score==999&&trace.SequenceEqual(new[]{"start","report:Success:200","success","rank"}),"Encrypted server response traverses real HTTP, AES decode and original rank query parser");
                    Next(6);agent.Succeeded=(a,b)=>received=b;
                    queueLow=OutgameWebRequestTask.Create(server.Url+"/queue-low",null,1,5,null,null,null);
                    queueHigh=OutgameWebRequestTask.Create(server.Url+"/queue-high",null,9,5,null,null,null);
                    var cancelled=OutgameWebRequestTask.Create(server.Url+"/queue-cancel",null,20,5,null,null,null);
                    pool.AddTask(queueLow);pool.AddTask(queueHigh);pool.AddTask(cancelled);int cancelId=cancelled.SerialId;
                    Check(pool.WaitingTaskCount==3&&pool.RemoveTask(cancelId)&&!pool.HaveDownTask(cancelId)&&cancelled.SerialId==0,"Native pending cancellation clears task without sending an HTTP request");
                    task=queueHigh;return;
                }
                if(phase==6&&task.Done){
                    Check(server.Take().Path=="/queue-high"&&pool.WaitingTaskCount==1&&!queueLow.Done,"Native source queue sends higher priority first with one actual helper");task=queueLow;phase=7;return;
                }
                if(phase==7&&task.Done){
                    Check(server.Take().Path=="/queue-low"&&queueHigh.SerialId==0&&pool.WaitingTaskCount==0&&server.Requests.IsEmpty,"Native queue reuses helper on next tick, clears prior task and never sends cancelled request");
                    pool.Shutdown();Check(helper.IsDisposed&&helper.RequestHandle==null&&agent.Task==null&&pool.TotalAgentCount==0,"Native queue shutdown clears active ownership and disposes helper");phase=8;return;
                }
                if(phase==8){Check(!helper,"Helper GameObject is destroyed at native frame boundary");Finish(null);}
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{server?.Dispose();if(helper){helper.Reset();UnityEngine.Object.Destroy(helper.gameObject);}}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/web-request-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_WEB_REQUEST_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
