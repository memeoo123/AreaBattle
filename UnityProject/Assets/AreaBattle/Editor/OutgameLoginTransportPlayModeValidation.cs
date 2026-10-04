using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Server=AreaBattle.EditorTools.OutgameWebRequestPlayModeValidation.Server;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLoginTransportPlayModeValidation
    {
        const string Pending="AreaBattle.LoginTransportNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Native LoginTransmitter frame countdown -> WebRequestManager -> Unity HTTP -> controlled loopback server, encrypted storage upload and plaintext server-time protocol. Test explicitly supplies frame ticks, domain result and response actions; production HttpManager/HttpNetAcion/ServerTimeSync/platform/Main not claimed.";
            public List<string> checks=new List<string>();
        }
        static Report report;static Server server;static OutgameWebRequestManagerHost host;static OutgameWebRequestManager manager;
        static OutgameLoginTransmitter login;static OutgameHttpCipher cipher;static OutgameNetTool net;
        static int phase,ticks,lastFrame,queries;static bool stopped;static double started;static string uploadReply,timeReply,key;
        static OutgameLoginTransportPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string why){if(!condition)throw new Exception(why);report.checks.Add(why);}
        static void Start()
        {
            report=new Report();phase=0;ticks=0;lastFrame=-1;queries=0;stopped=false;uploadReply=null;timeReply=null;started=EditorApplication.timeSinceStartup;
            try{
                var tx=new OutgameHttpTransmitterServices{EncryptionEnabled=()=>true,LogEnabled=()=>false,Key=()=>key,NetLog=a=>{},Warning=a=>{},Log=a=>{},Messages=()=>new OutgameMessageDispatcher(),NewErrorReport=()=>new OutgameHttpErrorRecord(),ReportError=r=>{throw new Exception("unexpected HTTP error");},ReportCryptoError=(c,m)=>{throw new Exception("crypto "+c);},CryptoError=e=>{throw e;}};
                cipher=new OutgameHttpCipher(tx);key=cipher.DecodeKey("Y2RlL3JjZjQx",27);
                string encrypted=LitJson.JsonMapper.ToJson(new OutgameHttpEncryptedData{encrypt=cipher.Encrypt("{\"code\":0,\"msg\":\"upload-ack\"}",key)});
                server=new Server(null,r=>r.Path=="/sys/nowTime"?"{\"code\":0,\"msg\":\"time-ack\"}":encrypted);
                net=new OutgameNetTool(new OutgameNetToolServices{OnlineConfig=k=>"2",GameBizDomain=(a,b,c)=>{queries++;return server.Url;},Warning=a=>{}});
                var agent=new OutgameWebRequestServices{EncryptionEnabled=()=>true,Log=s=>{},Error=s=>{throw new Exception(s);},Report=(a,b,c,d,e,f)=>{}};
                OutgameWebRequestManagerHost.ServicesFactory=()=>new OutgameWebRequestManagerServices{AgentServices=agent,CreateHelper=()=>OutgameWebRequestManagerHost.CreateNativeHelper(agent),IsKeWan=()=>false,DebugEnabled=()=>false,Log=s=>{},NetLog=s=>{},Error=s=>{throw new Exception(s);},Warning=s=>{},CategoryLog=(c,s)=>{},Toast=s=>{}};
                host=OutgameWebRequestManagerHost.Instance;manager=host.Manager;tx.AddWebRequest=manager.AddWebRequest;
                var actions=new Dictionary<int,Action<string>>();for(int i=2;i<=19;i++)actions.Add(i,s=>{throw new Exception("unexpected protocol");});actions[4]=s=>uploadReply=s;actions[2]=s=>timeReply=s;
                login=new OutgameLoginTransmitter(tx,actions,()=>null);login.SetUrl(net.BaseUrl,false);login.InitProtocol();
                Check(net.BaseUrl==server.Url&&queries==1&&login.EncryptRequests&&key=="abc","Platform-domain boundary caches actual loopback URL; decoded key and global encryption flag configure login transmitter");
                login.QueuePlayerData("player","old");Time.timeScale=0;EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("login transport phase "+phase);
                if(server.Errors.TryDequeue(out var error))throw error;
                if(phase==0&&Time.frameCount!=lastFrame){
                    lastFrame=Time.frameCount;login.Update();ticks++;
                    if(ticks==2){login.QueuePlayerData("player","latest");login.QueuePlayerData("other","second");}
                    if(ticks==3)Check(login.RemainingUpdates==0&&manager.WaitingTaskCount==0&&manager.WorkingAgentCount==0&&server.Requests.IsEmpty,"Three native frames decrement without sending; repeated data does not reset countdown even at timeScale zero");
                    if(ticks==4){Check(!login.UploadPending&&login.PendingData.Count==0&&manager.WaitingTaskCount==1,"Fourth native frame queues exactly one request before manager dispatch");phase=1;}
                    return;
                }
                if(phase==1&&uploadReply!=null&&manager.WorkingAgentCount==0){
                    var wire=server.Take();var envelope=LitJson.JsonMapper.ToObject<OutgameHttpEncryptedData>(wire.Body);var data=LitJson.JsonMapper.ToObject<OutgameRequestSendGameData>(cipher.Decrypt(envelope.encrypt,key));
                    Check(wire.Path=="/data/private/update"&&wire.Method=="POST"&&wire.Headers["X-Encrypted"]=="true"&&data.Datas.Count==2&&data.Datas["player"]=="latest"&&data.Datas["other"]=="second","Actual encrypted HTTP upload contains merged latest values and original Datas field");
                    Check(LitJson.JsonMapper.ToObject<OutgameHttpResponse>(uploadReply).msg=="upload-ack"&&manager.FreeAgentCount==1&&manager.WaitingTaskCount==0,"Encrypted server reply decrypts before registered protocol callback; manager reclaims helper");
                    login.RequestServerTime();phase=2;return;
                }
                if(phase==2&&timeReply!=null&&manager.WorkingAgentCount==0){
                    var wire=server.Take();Check(wire.Path=="/sys/nowTime"&&wire.Method=="GET"&&wire.Body==string.Empty&&!wire.Headers.ContainsKey("X-Encrypted"),"Server-time protocol bypasses encryption; null data follows the original manager GET path without encrypted header");
                    Check(LitJson.JsonMapper.ToObject<OutgameHttpResponse>(timeReply).msg=="time-ack"&&server.Requests.IsEmpty&&manager.TotalAgentCount==1&&net.BaseUrl==server.Url&&queries==1,"Plain server-time reply dispatches successfully through reused helper and cached domain");
                    manager.Shutdown();Check(manager.TotalAgentCount==0,"Native transport shutdown releases owned helper");manager=null;Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{manager?.Shutdown();server?.Dispose();if(host)UnityEngine.Object.Destroy(host.gameObject);OutgameWebRequestManagerHost.ServicesFactory=null;}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/login-transport-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_LOGIN_TRANSPORT_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
