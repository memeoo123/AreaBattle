using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLoginTransportValidation
    {
        sealed class Model:IOutgameDataManager
        {
            public string Key;public Action OnRead;
            public string DataKey{get{OnRead?.Invoke();return Key;}}
            public bool ParticipatesInSync{get;set;}public bool CompressData{get;set;}
            public void OnInit(){}public void OnSave(){}public void OnRelease(){}
        }
        sealed class Fixture
        {
            public readonly List<(string url,object body,int protocol)> Requests=new List<(string,object,int)>();
            public readonly List<object[]> Logs=new List<object[]>();
            public readonly Dictionary<int,Action<string>> Actions=new Dictionary<int,Action<string>>();
            public readonly OutgameHttpTransmitterServices Services;
            public readonly OutgameLoginTransmitter Login;
            public bool Encrypt;public Action OnSend;
            public Fixture(){
                Services=new OutgameHttpTransmitterServices{EncryptionEnabled=()=>Encrypt,LogEnabled=()=>false,Key=()=>"abc",NetLog=a=>Logs.Add(a),Warning=a=>{},Log=a=>{},Messages=()=>new OutgameMessageDispatcher(),NewErrorReport=()=>new OutgameHttpErrorRecord(),ReportError=r=>{},ReportCryptoError=(c,m)=>{throw new Exception(m);},CryptoError=e=>{throw e;},AddWebRequest=(u,b,c,p)=>{Requests.Add((u,b,(int)p));OnSend?.Invoke();return 47;}};
                for(int i=2;i<=19;i++)Actions.Add(i,s=>{});
                Login=new OutgameLoginTransmitter(Services,Actions,()=>null);Login.SetUrl("http://fixture");Login.InitProtocol();
            }
        }
        static void Need(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static void Tick(OutgameBaseHttpNetTransmitter tx,int count){for(int i=0;i<count;i++)tx.Update();}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source NetTool cache/domain configuration, server-time key decoder and LoginTransmitter requests/coalescing. HttpNetAcion response business, HttpManager owner, actual platform domain/login/server time and production Main remain pending."};
            Action<string,Action> check=(id,a)=>{try{a();report.checks.Add(new BattleBuild.Check{id="login-transport-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="login-transport-"+id,result="fail",detail=e.ToString()});}};
            check("domain-default-exact-zero-and-cache",()=>{
                foreach(string configured in new string[]{null,"","0","1"}){
                    int queries=0,warnings=0;var net=new OutgameNetTool(new OutgameNetToolServices{OnlineConfig=k=>{Need(k=="NetDtype","config key");return configured;},GameBizDomain=(app,d,kind)=>{queries++;Need(app=="hcrzd-u-cn-wx"&&d==OutgameDomainType.RD&&kind=="default","original platform query arguments");return "fixture.invalid";},Warning=a=>{warnings++;if(warnings==1)Need(a[1] is OutgameDomainType,"boxed source enum");}});
                    Need(net.BaseUrl=="https://fixture.invalid:20150"&&net.BaseUrl=="https://fixture.invalid:20150"&&queries==1&&warnings==2,"first nonempty URL cached");
                }
            });
            check("domain-parse-failure-and-whitespace-zero",()=>{
                foreach(string configured in new[]{"garbage"," 0 ","2147483648","-4","2","3"}){
                    int expected=int.TryParse(configured,out int parsed)?parsed:0;
                    var net=new OutgameNetTool(new OutgameNetToolServices{OnlineConfig=k=>configured,GameBizDomain=(app,d,kind)=>{Need((int)d==expected,"TryParse out value, including unknown enum");return "verbatim";},Warning=a=>{}});
                    Need(net.BaseUrl=="verbatim","non-RD query result receives no scheme or port");
                }
            });
            check("domain-empty-retry-and-rd-null-string-concat",()=>{
                int calls=0;var services=new OutgameNetToolServices{OnlineConfig=k=>"2",GameBizDomain=(a,b,c)=>{calls++;return null;},Warning=a=>{}};var net=new OutgameNetTool(services);
                Need(net.BaseUrl==null&&net.BaseUrl==null&&calls==2,"null query result remains retryable");services.OnlineConfig=k=>"1";
                Need(net.BaseUrl=="https://:20150"&&net.BaseUrl=="https://:20150"&&calls==3,"RD wraps null into nonempty cached URL");
            });
            check("domain-log-failure-before-query",()=>{
                int queries=0;var services=new OutgameNetToolServices{OnlineConfig=k=>"2",GameBizDomain=(a,b,c)=>{queries++;return "url";},Warning=a=>throw new InvalidOperationException()};var net=new OutgameNetTool(services);
                Throws<InvalidOperationException>(()=>{_=net.BaseUrl;});Need(queries==0,"first warning precedes domain lookup");services.Warning=a=>{};Need(net.BaseUrl=="url"&&queries==1,"retry after logging failure");
            });
            check("domain-query-failure-does-not-cache",()=>{
                int calls=0;var net=new OutgameNetTool(new OutgameNetToolServices{OnlineConfig=k=>"2",GameBizDomain=(a,b,c)=>{if(++calls==1)throw new InvalidOperationException();return "url";},Warning=a=>{}});
                Throws<InvalidOperationException>(()=>{_=net.BaseUrl;});Need(net.BaseUrl=="url"&&calls==2,"failed query permits retry");
            });
            check("domain-final-log-failure-retains-cache",()=>{
                int calls=0;var net=new OutgameNetTool(new OutgameNetToolServices{OnlineConfig=k=>"2",GameBizDomain=(a,b,c)=>{calls++;return "url";},Warning=a=>{if((string)a[0]=="NetTool URL")throw new InvalidOperationException();}});
                Throws<InvalidOperationException>(()=>{_=net.BaseUrl;});Need(net.BaseUrl=="url"&&calls==1,"assignment precedes second warning");
            });
            check("key-decoder-independent-known-vectors",()=>{
                var f=new Fixture();var cipher=new OutgameHttpCipher(f.Services);
                Need(cipher.DecodeKey("c2VjcmV0LXRhaWw2MA==",0)=="secret","length truncates decoded tail");
                Need(cipher.DecodeKey("Y2RlL3JjZjQx",27)=="abc"&&cipher.DecodeKey("X2BhK25fYjIv",-27)=="abc","both signed shifts");
                Need(cipher.DecodeKey("5Lit5paH6KGl5L2NNDA=",676)=="中文补位","26 squared shift period");
                Need(cipher.DecodeKey("wo/CjsKAS1BH8KW4mkfCkjEy",51)=="utf16-😀-x","UTF16 units reversed, including surrogate pair");
                Need(cipher.DecodeKey("MDA=",0)==""&&f.Logs.Count==0,"zero length is valid");
            });
            check("key-malformed-base64-null-and-short-payload",()=>{
                var f=new Fixture();var cipher=new OutgameHttpCipher(f.Services);
                foreach(string text in new[]{null,"!","","eA=="})Need(cipher.DecodeKey(text,long.MinValue)==string.Empty,"decode errors return empty");
                Need(f.Logs.Count==4&&f.Logs.All(a=>((string)a[0]).StartsWith("Decoding error: ")),"all failures log exactly once");
            });
            check("key-length-parse-and-bounds-errors",()=>{
                var f=new Fixture();var cipher=new OutgameHttpCipher(f.Services);
                Need(cipher.DecodeKey("enphYmM=",0)==""&&((string)f.Logs[0][0])=="Decoding error: The original length cannot be parsed.","source length format exception message");
                Need(cipher.DecodeKey("OTk=",0)==""&&f.Logs.Count==2,"declared length beyond payload errors");
            });
            check("key-error-log-exception-propagates",()=>{var f=new Fixture();f.Services.NetLog=a=>throw new ApplicationException();Throws<ApplicationException>(()=>new OutgameHttpCipher(f.Services).DecodeKey("!",0));});
            check("source-url-flag-ignored-global-snapshot",()=>{
                var f=new Fixture();f.Login.SetUrl("first",true);Need(!f.Login.EncryptRequests,"parameter ignored");f.Encrypt=true;f.Login.SetUrl("second",false);Need(f.Login.EncryptRequests,"global flag sampled");f.Encrypt=false;Need(f.Login.EncryptRequests,"later global change does not alter transmitter");
                f.Services.EncryptionEnabled=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Login.SetUrl("published",false));Need(f.Login.BaseUrl=="published"&&f.Login.EncryptRequests,"URL publishes before global read");
            });
            check("all-eighteen-protocols-and-captured-actions",()=>{
                var f=new Fixture();Need(f.Login.Urls.Count==18&&f.Login.Urls[2]=="http://fixture/sys/nowTime"&&f.Login.Urls[12]=="http://fixture/data/private/version"&&f.Login.Urls[19]=="http://fixture/data/attachment/get","protocol range and routes");var old=f.Login.Actions[3];f.Actions[3]=s=>throw new Exception();Need(f.Login.Actions[3]==old,"init binds current callback");f.Login.InitProtocol();Need(f.Login.Urls.Count==18&&f.Logs.Count==18,"duplicate init only reports duplicate registrations");
            });
            check("queue-overwrite-without-reset-and-fourth-update",()=>{
                var f=new Fixture();f.Login.QueuePlayerData("a","old");Tick(f.Login,2);f.Login.QueuePlayerData("a","new");f.Login.QueuePlayerData("b","two");Need(f.Login.RemainingUpdates==1,"later writes do not reset countdown");Tick(f.Login,1);Need(f.Requests.Count==0&&f.Login.RemainingUpdates==0,"third update only decrements");var old=f.Login.PendingData;Tick(f.Login,1);
                var body=(OutgameRequestSendGameData)f.Requests.Single().body;Need(body.Datas==old&&body.Datas["a"]=="new"&&body.Datas["b"]=="two"&&!f.Login.UploadPending&&f.Login.PendingData.Count==0&&f.Login.PendingData!=old,"fourth update sends old dictionary then replaces it");Tick(f.Login,10);Need(f.Requests.Count==1,"idle updates do not resend");
            });
            check("new-batch-restarts-three-update-delay",()=>{var f=new Fixture();f.Login.QueuePlayerData("a","one");Tick(f.Login,4);f.Login.QueuePlayerData("b","two");Need(f.Login.RemainingUpdates==3,"new batch delay");Tick(f.Login,3);Need(f.Requests.Count==1,"still waiting");Tick(f.Login,1);Need(f.Requests.Count==2,"second batch sends");});
            check("null-key-does-not-arm-upload",()=>{var f=new Fixture();Throws<ArgumentNullException>(()=>f.Login.QueuePlayerData(null,"value"));Need(!f.Login.UploadPending&&f.Login.PendingData.Count==0,"dictionary failure before state changes");});
            check("send-failure-leaves-data-and-clears-pending",()=>{
                var f=new Fixture();f.Login.QueuePlayerData("a","one");var old=f.Login.PendingData;f.OnSend=()=>throw new InvalidOperationException();Tick(f.Login,3);Throws<InvalidOperationException>(()=>f.Login.Update());Need(!f.Login.UploadPending&&f.Login.PendingData==old&&old.Count==1,"failed send retains data after pending reset");Tick(f.Login,4);Need(f.Requests.Count==1,"failure not automatically retried");f.OnSend=null;f.Login.QueuePlayerData("b","two");Tick(f.Login,4);Need(((OutgameRequestSendGameData)f.Requests[1].body).Datas.Count==2,"next queued change retries old and new data together");
            });
            check("disposed-flush-clears-data-without-send",()=>{var f=new Fixture();f.Login.QueuePlayerData("a","one");f.Login.Dispose();Tick(f.Login,4);Need(f.Requests.Count==0&&!f.Login.UploadPending&&f.Login.PendingData.Count==0,"Send returns -1 normally; flush still replaces dictionary");});
            check("reentrant-submit-before-dictionary-replacement",()=>{
                var f=new Fixture();f.Login.QueuePlayerData("a","one");f.OnSend=()=>f.Login.QueuePlayerData("b","two");Tick(f.Login,4);Need(((OutgameRequestSendGameData)f.Requests[0].body).Datas["b"]=="two"&&f.Login.UploadPending&&f.Login.RemainingUpdates==3&&f.Login.PendingData.Count==0,"reentrant write reaches outgoing dictionary then replacement loses queued data");f.OnSend=null;Tick(f.Login,4);Need(((OutgameRequestSendGameData)f.Requests[1].body).Datas.Count==0,"source sends empty reentrant batch");
            });
            check("immediate-upload-alias-and-return-value",()=>{var f=new Fixture();var data=new Dictionary<string,string>();Need(f.Login.UploadPlayerData(data)==47&&((OutgameRequestSendGameData)f.Requests[0].body).Datas==data&&!f.Login.UploadPending,"direct upload uses alias and preserves transport return value");});
            check("request-routing-and-json-field-shapes",()=>{
                var f=new Fixture();var login=new object();var keys=new[]{"a","a",null};f.Login.RequestServerTime();f.Login.RequestLogin(login);f.Login.RequestDataVersions(keys);f.Login.RequestPlayerData(keys);f.Login.RequestCutoverLoginData("main","guest",long.MaxValue);f.Login.UploadUserExtNet("ext");f.Login.RequestDataUploaded();
                Need(f.Requests.Select(x=>x.protocol).SequenceEqual(new[]{2,3,12,6,14,16,18})&&f.Requests[0].body==null&&f.Requests[1].body==login&&f.Requests[6].body==null,"source protocols and null payloads");Need(((OutgameRequestList)f.Requests[2].body).keys==keys&&((OutgameRequestList)f.Requests[3].body).keys==keys,"request retains keys alias");var bind=(OutgameCutoverLoginData)f.Requests[4].body;Need(bind.userId=="main"&&bind.guestUserId=="guest"&&bind.guestUid==long.MaxValue,"cutover source fields");Need(LitJson.JsonMapper.ToJson(f.Requests[5].body)=="{\"userExt\":\"ext\"}","original user extension field");
            });
            check("server-time-bypasses-encryption-login-uses-aes",()=>{
                var f=new Fixture();f.Encrypt=true;f.Login.SetUrl("http://fixture");f.Login.RequestServerTime();f.Login.RequestLogin(new OutgameHttpResponse{code=3,msg="login"});Need(f.Requests[0].body==null,"server time remains plaintext");var envelope=(OutgameHttpEncryptedData)f.Requests[1].body;var decoded=new OutgameHttpCipher(f.Services).Decrypt(envelope.encrypt,"abc");Need(LitJson.JsonMapper.ToObject<OutgameHttpResponse>(decoded).msg=="login","login encrypted before transport");
            });
            check("version-pool-filter-order-and-duplicates",()=>{
                var f=new Fixture();var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});pool.OnInit(false,"",null);
                pool.Managers.Add(1,new Model{Key="same",ParticipatesInSync=true});pool.Managers.Add(2,new Model{Key="ignored"});pool.Managers.Add(3,new Model{Key="same",ParticipatesInSync=true});pool.Managers.Add(4,new Model{Key=null,ParticipatesInSync=true});
                var login=new OutgameLoginTransmitter(f.Services,f.Actions,()=>pool);login.SetUrl("url");login.InitProtocol();login.RequestDataVersions();Need(((OutgameRequestList)f.Requests.Single().body).keys.SequenceEqual(new[]{"same","same",null})&&f.Requests[0].protocol==12,"live pool preserves order, duplicates and null keys");
            });
            check("version-pool-null-and-mutation-propagate",()=>{
                var f=new Fixture();Throws<NullReferenceException>(f.Login.RequestDataVersions);var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});pool.OnInit(false,"",null);var login=new OutgameLoginTransmitter(f.Services,f.Actions,()=>pool);login.SetUrl("url");login.InitProtocol();
                pool.Managers.Add(1,null);Throws<NullReferenceException>(login.RequestDataVersions);pool.Managers.Clear();pool.Managers.Add(1,new Model{Key="a",ParticipatesInSync=true,OnRead=()=>pool.Managers.Add(2,new Model())});Throws<InvalidOperationException>(login.RequestDataVersions);Need(f.Requests.Count==0,"enumeration failures prevent request dispatch");
            });
            return report;
        }
    }
}
