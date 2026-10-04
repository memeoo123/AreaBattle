using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Server=AreaBattle.EditorTools.OutgameWebRequestPlayModeValidation.Server;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameWebRequestManagerPlayModeValidation
    {
        const string Pending="AreaBattle.WebRequestManagerNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Native original WebRequestManager singleton/Update/helper creation/queue/callback events with actual AES rank HTTP, request priority, cancellation, headers, KeWan gate, unscaled timeout and shutdown/recreation. Platform HttpManager/NetTool/domain-key/report endpoints remain explicit test composition.";
            public List<string> checks=new List<string>();
        }
        static Report report;static Server server;static OutgameWebRequestManagerHost host,oldHost;static OutgameWebRequestManager manager;
        static OutgameRanklistTransmitter rank;static OutgameHttpCipher cipher;static OutgameGetRankResponse response;static OutgameHttpEvent held;
        static readonly List<OutgameUnityWebRequestAgentHelper> helpers=new List<OutgameUnityWebRequestAgentHelper>();
        static readonly List<string> trace=new List<string>();static bool keWan,stopped;static int phase,frame,factoryCalls,categories,highResponses;static string failure,timeout;static double started;
        static OutgameWebRequestManagerPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string why){if(!condition)throw new Exception(why);report.checks.Add(why);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;factoryCalls=0;categories=0;highResponses=0;keWan=false;held=null;response=null;failure=null;timeout=null;helpers.Clear();trace.Clear();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                var tx=new OutgameHttpTransmitterServices{EncryptionEnabled=()=>true,LogEnabled=()=>false,Key=()=>"abc",NetLog=a=>{},Warning=a=>{},Log=a=>{},Messages=()=>new OutgameMessageDispatcher(),NewErrorReport=()=>new OutgameHttpErrorRecord(),ReportError=r=>{throw new Exception("unexpected rank error");},ReportCryptoError=(c,m)=>{throw new Exception("crypto "+c+":"+m);},CryptoError=e=>{throw e;}};
                cipher=new OutgameHttpCipher(tx);server=new Server(LitJson.JsonMapper.ToJson(new OutgameHttpEncryptedData{encrypt=cipher.Encrypt("{\"code\":0,\"data\":{\"count\":2,\"no\":7,\"list\":[{\"no\":1,\"score\":999}]}}","abc")}));
                var agentServices=new OutgameWebRequestServices{EncryptionEnabled=()=>true,Log=s=>trace.Add("helper-log"),Error=s=>trace.Add("helper-error:"+s),Report=(a,b,c,d,e,f)=>trace.Add("report:"+c+":"+d)};
                OutgameWebRequestManagerHost.ServicesFactory=()=>{
                    factoryCalls++;return new OutgameWebRequestManagerServices{AgentServices=agentServices,IsKeWan=()=>keWan,DebugEnabled=()=>false,CreateHelper=()=>{var h=(OutgameUnityWebRequestAgentHelper)OutgameWebRequestManagerHost.CreateNativeHelper(agentServices);helpers.Add(h);return h;},Log=s=>trace.Add("manager-log"),NetLog=s=>trace.Add("header"),Error=s=>{throw new Exception(s);},Warning=s=>trace.Add("warning"),CategoryLog=(c,s)=>{if(c!=131072)throw new Exception("category");categories++;},Toast=s=>trace.Add("toast")};
                };
                host=OutgameWebRequestManagerHost.Instance;manager=host.Manager;
                Check(host.gameObject.name=="WebRequestManager"&&host.gameObject.scene.name=="DontDestroyOnLoad"&&manager.Timeout==30&&manager.WorkingTaskCapacity==20,"Native singleton creates persistent named owner and initializes original defaults");
                Check(OutgameWebRequestManagerHost.Instance==host&&factoryCalls==1,"Repeated singleton access reuses owner without reinitialization");manager.WorkingTaskCapacity=1;
                tx.AddWebRequest=manager.AddWebRequest;rank=new OutgameRanklistTransmitter(tx,()=>20){RanklistName="native-manager",Uid="native-user",GetRankResponse=r=>{response=r;trace.Add("rank");}};rank.SetUrl(server.Url);rank.InitProtocol();rank.GetRankDataRequest(1,2,2);
                manager.AddWebRequest(server.Url+"/priority",(byte[])null,9,"high",(s,e)=>{held=(OutgameHttpEvent)e;if(s==1){Check(held.Uri.EndsWith("/priority")&&(string)held.UserData=="high"&&held.SerialId>0,"Native manager callback preserves serial, URI and user data during dispatch");highResponses++;}});
                int cancel=manager.AddWebRequest(server.Url+"/cancel",20,(s,e)=>{throw new Exception("cancelled request callback");});Check(manager.RemoveWebRequest(cancel)&&manager.WaitingTaskCount==2&&helpers.Count==1,"Native queued requests share one free helper and cancelled task never starts");
                EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>60)throw new TimeoutException("manager phase "+phase);
                if(server.Errors.TryDequeue(out var error))throw error;
                if(phase==0&&response!=null&&highResponses==1&&manager.WorkingAgentCount==0){
                    var first=server.Take();var second=server.Take();Check(first.Path=="/priority"&&second.Path=="/mini-toplist/query"&&manager.WaitingTaskCount==0,"Native manager Update dispatches source priority queue and reclaims completed tasks");
                    var envelope=LitJson.JsonMapper.ToObject<OutgameHttpEncryptedData>(second.Body);var request=LitJson.JsonMapper.ToObject<OutgameGetRankRequest>(cipher.Decrypt(envelope.encrypt,"abc"));
                    Check(second.Method=="POST"&&second.Headers["X-Encrypted"]=="true"&&request.name=="native-manager"&&request.uid=="native-user"&&request.count==2,"Real rank transmitter uses manager object serialization and actual encrypted HTTP bytes");
                    Check(response.data.no==7&&response.data.list[0].score==999&&held.SerialId==0&&held.Uri==null&&held.UserData==null&&((OutgameHttpSuccess)held).Bytes==null,"Real encrypted rank response parses while manager callback event is cleared after dispatch");
                    Check(helpers.Count==1&&helpers[0].RequestHandle==null&&manager.FreeAgentCount==1,"Native manager reuses the same actual helper across both requests");
                    keWan=true;manager.SetRequestHeader("X-Native","first");manager.AddWebRequest(server.Url+"/fail",(object)"native-failure",(s,e)=>{held=(OutgameHttpEvent)e;if(s==2)failure=((OutgameHttpFailure)e).Error;});manager.SetRequestHeader("X-Native","second");frame=Time.frameCount;phase=1;return;
                }
                if(phase==1&&Time.frameCount>frame+4){
                    Check(manager.WaitingTaskCount==1&&manager.WorkingAgentCount==0&&helpers[0].RequestHandle==null&&server.Requests.IsEmpty,"Actual KeWan setting prevents manager Update from starting queued HTTP");keWan=false;phase=2;return;
                }
                if(phase==2&&failure!=null&&manager.WorkingAgentCount==0){
                    var wire=server.Take();Check(wire.Path=="/fail"&&wire.Headers["X-Native"]=="first"&&manager.Headers["X-Native"]=="second","Native task snapshots manager headers before later changes");
                    Check(held.SerialId==0&&((OutgameHttpFailure)held).Error==null&&trace.Contains("report:Fail:503"),"Actual HTTP503 reports and clears the failure event after callback");
                    Time.timeScale=0;manager.Timeout=.2f;manager.ClearRequestHeader();manager.AddWebRequest(server.Url+"/slow",(s,e)=>{
                        held=(OutgameHttpEvent)e;
                        if(s==0){int id=manager.AddWebRequest(server.Url+"/orphan-cancel",(Action<int,object>)null);manager.RemoveWebRequest(id);}
                        if(s==2)timeout=((OutgameHttpFailure)e).Error;
                    });phase=3;return;
                }
                if(phase==3&&timeout!=null&&manager.WorkingAgentCount==0){
                    Check(server.Take().Path=="/slow"&&timeout=="Timeout"&&Time.timeScale==0&&trace.Contains("report:Fail:0"),"Native manager unscaled update times out stalled HTTP while game time is paused");
                    Check(categories==1&&helpers.Count==2&&manager.TotalAgentCount==1&&helpers[1]&&helpers[1].RequestHandle==null&&!helpers[1].IsDisposed,"Source capacity equality creates unused native helper and logs category131072 without destroying it");
                    Check(server.Requests.IsEmpty&&manager.WaitingTaskCount==0&&held.SerialId==0,"Reentrant queued request cancels without sending; timeout event clears normally");
                    Time.timeScale=1;oldHost=host;manager.Shutdown();Check(manager.TotalAgentCount==0&&helpers[0].IsDisposed,"Manager shutdown disposes owned helpers before destroying singleton owner");phase=4;frame=Time.frameCount;return;
                }
                if(phase==4&&Time.frameCount>frame+2){
                    Check(!oldHost&&!helpers[0]&&helpers[1],"Native frame destroys owned manager/helper while source unused helper remains");
                    host=OutgameWebRequestManagerHost.Instance;manager=host.Manager;Check(host&&host!=oldHost&&factoryCalls==2&&manager.Timeout==30&&manager.TotalAgentCount==0,"Fresh singleton access after native destruction creates independently initialized owner");
                    manager.Shutdown();Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{server?.Dispose();foreach(var helper in helpers)if(helper){helper.Reset();UnityEngine.Object.Destroy(helper.gameObject);}if(host)UnityEngine.Object.Destroy(host.gameObject);OutgameWebRequestManagerHost.ServicesFactory=null;}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/web-request-manager-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_WEB_REQUEST_MANAGER_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
