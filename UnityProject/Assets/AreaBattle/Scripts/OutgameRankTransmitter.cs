using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace AreaBattle
{
    public enum OutgameHttpProtocolType {Heart=1,ServerTime=2,Login=3,UpLoadPlayerData=4,GetAllPlayerData=5,GetPlayerData=6,DeletePlayerData=7,GetOtherPlayerData=8,UploadCommonData=9,GetCommonData=10,DeleteCommonData=11,Version=12,UserIsExists=13,CutoverLogin=14,LoginOut=15,UploadUserExt=16,LoginForcedBind=17,DataUploaded=18,GetUserAttachment=19}
    [Serializable] public class OutgameHttpResponse {public string msg;public int code;}
    [Serializable] public sealed class OutgameHttpEncryptedData {public string encrypt;}
    public sealed class OutgameHttpErrorRecord {public int? Code;public string Message;}
    public class OutgameHttpEvent:EventArgs
    {
        public int SerialId;public string Uri;public object UserData;
        public virtual void Clear(){SerialId=0;Uri=null;UserData=null;}
    }
    public sealed class OutgameHttpStart:OutgameHttpEvent {}
    public sealed class OutgameHttpSuccess:OutgameHttpEvent {public byte[] Bytes;public override void Clear(){base.Clear();Bytes=null;}}
    public sealed class OutgameHttpFailure:OutgameHttpEvent {public string Error;public override void Clear(){base.Clear();Error=null;}}
    public sealed class OutgameHttpTransmitterServices
    {
        // Original WebRequestManager boundary: request object and protocol UserData.
        // A transport must deliver actual event1/success or2/failure, never infer success.
        public Func<string,object,Action<int,object>,object,int> AddWebRequest;
        public Func<bool> EncryptionEnabled,LogEnabled;
        public Func<string> Key;
        public Action<object[]> NetLog,Warning,Log;
        public Func<OutgameMessageDispatcher> Messages;
        public Func<OutgameHttpErrorRecord> NewErrorReport;
        public Action<OutgameHttpErrorRecord> ReportError;
        public Action<int,string> ReportCryptoError;
        public Action<Exception> CryptoError;
    }
    // PngDataHandler28200/28201/28202: source names obscure AES-CBC; no image data.
    public sealed class OutgameHttpCipher
    {
        readonly OutgameHttpTransmitterServices services;
        public OutgameHttpCipher(OutgameHttpTransmitterServices services){this.services=services;}
        public static string NormalizeKey(string key)=>key.Length<16?key.PadRight(16,'0'):key.Length==16?key:key.Substring(0,16);
        // PngDataHandler28199: the server-time key envelope, before AES is used.
        public string DecodeKey(string encoded,long timestamp)
        {
            try{
                long quotient=timestamp/26;int first=(int)(quotient%26),second=(int)(timestamp-quotient*26);
                char[] chars=Encoding.UTF8.GetString(Convert.FromBase64String(encoded)).ToCharArray();
                for(int i=0;i<chars.Length;i++)chars[i]=unchecked((char)(chars[i]-first));
                Array.Reverse(chars);string text=new string(chars);
                if(!int.TryParse(text.Substring(0,2),out int length))throw new FormatException("The original length cannot be parsed.");
                chars=text.Substring(2).ToCharArray();
                for(int i=0;i<chars.Length;i++)chars[i]=unchecked((char)(chars[i]-second));
                Array.Reverse(chars);return new string(chars).Substring(0,length);
            }catch(Exception e){services.NetLog(new object[]{"Decoding error: "+e.Message});return string.Empty;}
        }
        public string Encrypt(string text,string key)
        {
            try{
                byte[] bytes=Encoding.UTF8.GetBytes(NormalizeKey(key));
                using(var aes=Aes.Create()){
                    aes.Key=bytes;aes.IV=bytes;aes.Mode=CipherMode.CBC;aes.Padding=PaddingMode.PKCS7;
                    using(var transform=aes.CreateEncryptor(aes.Key,aes.IV))using(var memory=new MemoryStream()){
                        using(var crypto=new CryptoStream(memory,transform,CryptoStreamMode.Write))using(var writer=new StreamWriter(crypto,Encoding.UTF8))writer.Write(text);
                        return Convert.ToBase64String(memory.ToArray());
                    }
                }
            }catch(Exception e){services.ReportCryptoError(1008,e.Message??string.Empty);services.CryptoError(e);return null;}
        }
        public string Decrypt(string text,string key)
        {
            try{
                byte[] bytes=Encoding.UTF8.GetBytes(NormalizeKey(key));
                using(var aes=Aes.Create()){
                    aes.Key=bytes;aes.IV=bytes;aes.Mode=CipherMode.CBC;aes.Padding=PaddingMode.PKCS7;
                    using(var transform=aes.CreateDecryptor(aes.Key,aes.IV))using(var memory=new MemoryStream(Convert.FromBase64String(text)))
                    using(var crypto=new CryptoStream(memory,transform,CryptoStreamMode.Read))using(var reader=new StreamReader(crypto,Encoding.UTF8))return reader.ReadToEnd();
                }
            }catch(Exception e){services.ReportCryptoError(1007,e.Message??string.Empty);services.CryptoError(e);return null;}
        }
    }
    public class OutgameBaseHttpNetTransmitter
    {
        protected readonly OutgameHttpTransmitterServices Services;
        readonly OutgameHttpCipher cipher;
        public bool EncryptRequests=true,IsDisposed;
        public int Id;public string BaseUrl;
        public readonly Dictionary<int,string> Urls=new Dictionary<int,string>();
        public readonly Dictionary<int,Action<string>> Actions=new Dictionary<int,Action<string>>();
        public readonly OutgameHttpEncryptedData EncryptedRequest=new OutgameHttpEncryptedData();
        public OutgameBaseHttpNetTransmitter(OutgameHttpTransmitterServices services){Services=services;cipher=new OutgameHttpCipher(services);}
        public void SetUrl(string url){BaseUrl=url;EncryptRequests=Services.EncryptionEnabled();}
        public void SetUrl(string url,bool encryptionEnabled){SetUrl(url);} //29098 ignores the supplied flag.
        public virtual void InitProtocol(){} //29104 empty.
        public virtual void Update(){} //29105 empty; manager dispatches virtual slot6 without arguments.
        public void AddHttpProtocol(int protocol,string url,Action<string> action)
        {
            if(Urls.ContainsKey(protocol)){Services.NetLog(new object[]{string.Format("[{0}]添加协议失败，重复的协议ID[{1}]",Id,protocol)});return;}
            Urls.Add(protocol,url);Actions.Add(protocol,action);
        }
        public virtual void Dispose(){Urls.Clear();Actions.Clear();IsDisposed=true;}
        public void EncryptData(object data)
        {
            if(data==null){EncryptedRequest.encrypt=string.Empty;return;}
            string json=LitJson.JsonMapper.ToJson(data);EncryptedRequest.encrypt=cipher.Encrypt(json,Services.Key());
        }
        public string DecryptData(string text)
        {var data=LitJson.JsonMapper.ToObject<OutgameHttpEncryptedData>(text);return cipher.Decrypt(data.encrypt,Services.Key());}
        public int Send(int protocol,object data)
        {
            if(IsDisposed){Services.NetLog(new object[]{string.Format("http发射器[{0}]已经摧毁",Id)});return -1;}
            if(!Urls.ContainsKey(protocol)){Services.NetLog(new object[]{string.Format("[{0}]未找到协议号[{1}] 的地址URL",Id,protocol)});return -1;}
            string url=Urls[protocol];if(string.IsNullOrEmpty(url)){Services.NetLog(new object[]{string.Format("[{0}]协议号[{1}]的地址为空",Id,protocol)});return -1;}
            if(Services.LogEnabled()){string json=data==null?string.Empty:LitJson.JsonMapper.ToJson(data);Services.NetLog(new object[]{string.Format("[{0}]Send:[{1}] URL[{2} 内容：{3}] ",Id,protocol,url,json)});}
            if(protocol!=2&&EncryptRequests){EncryptData(data);data=EncryptedRequest;}
            return Services.AddWebRequest(url,data,RequestCallBack,protocol);
        }
        public void RequestCallBack(int state,object eventData)
        {
            if(IsDisposed){Services.NetLog(new object[]{string.Format("http发射器[{0}]已经摧毁",Id)});return;}
            if(state==2){
                var e=(OutgameHttpFailure)eventData;int protocol=(int)e.UserData;
                Services.Messages().SendMessage("GF_HttpWebRequestError",new object[]{e.Error,e.Uri,protocol,e.SerialId});
                var report=Services.NewErrorReport();report.Code=-9999;report.Message=e.Error;Services.ReportError(report);
                Services.Warning(new object[]{string.Format("NetHtpp:[{0}]协议号{1} 请求错误:{2} Url={3}",Id,(OutgameHttpProtocolType)protocol,e.Error,e.Uri)});return;
            }
            if(state!=1)return;
            var success=(OutgameHttpSuccess)eventData;int id=(int)success.UserData;string text=Encoding.UTF8.GetString(success.Bytes);
            if(id!=2&&EncryptRequests){try{string decoded=DecryptData(text);if(!string.IsNullOrEmpty(decoded))text=decoded;}catch(Exception){Services.Log(new object[]{"解密失败"});}}
            Services.NetLog(new object[]{string.Format("[{0}]：[{1}]收到服务器响应:[{2}]",Id,(OutgameHttpProtocolType)id,text)});
            var response=LitJson.JsonMapper.ToObject<OutgameHttpResponse>(text);
            if(response.code!=0){
                Services.Messages().SendMessage("GF_HttpServerErrorCode",new object[]{response.code,response.msg,id,success.SerialId});
                Services.Warning(new object[]{string.Format("NetHtpp:[{0}]错误的响应：协议ID[{1}]Code:{2} Msg:{3}",Id,(OutgameHttpProtocolType)id,response.code,response.msg)});
                var report=Services.NewErrorReport();report.Code=response.code;report.Message=((OutgameHttpProtocolType)id).ToString();Services.ReportError(report);
            }else Services.Messages().SendMessage("GF_HttpServerSuccess",new object[]{success.SerialId});
            var callback=Actions[id];callback?.Invoke(text); // Even nonzero business code dispatches.
        }
    }
    [Serializable] public sealed class OutgameGetRankRequest {public string name;public int category;public string uid,offset;public int page,count;}
    [Serializable] public struct OutgameRankExtendedData {public string uid,nick,headUrl;public int victoryNum,defeatNum,totalScore;}
    [Serializable] public sealed class OutgameRankStruct {public int no,score;public OutgameRankExtendedData ext;}
    [Serializable] public struct OutgameRankResponseRows {public string name;public int category,page,count,no,score;public List<OutgameRankStruct> list;}
    [Serializable] public sealed class OutgameGetRankResponse:OutgameHttpResponse {public OutgameRankResponseRows data;}
    [Serializable] public sealed class OutgameSendRankRequest {public string name;public int category;public bool incr;public string uid;public int score;public bool forceUpdate;public OutgameRankExtendedData ext;}
    public sealed class OutgameRanklistTransmitter:OutgameBaseHttpNetTransmitter,IOutgameMatchRankTransmitter
    {
        public const int SendRank=8101,GetRank=8102,DeleteRank=8103;
        readonly Func<int> winScore;
        public string RanklistName {get;set;}public string Uid {get;set;}
        public Action<OutgameGetRankResponse> GetRankResponse {get;set;}public Action<OutgameGetRankResponse> MineRankResponse {get;set;}
        public Action SendResponseCallback {get;set;}public Action DelResponseCallback {get;private set;}
        OutgameGetRankRequest getRequest;OutgameSendRankRequest sendRequest;
        public OutgameRanklistTransmitter(OutgameHttpTransmitterServices services,Func<int> winScore):base(services){this.winScore=winScore;}
        public override void InitProtocol()
        {
            AddHttpProtocol(SendRank,BaseUrl+"/mini-toplist/knock",text=>SendResponseCallback?.Invoke());
            AddHttpProtocol(GetRank,BaseUrl+"/mini-toplist/query",RankResponse);
            AddHttpProtocol(DeleteRank,BaseUrl+"/mini-toplist/delete",text=>DelResponseCallback?.Invoke());
        }
        void RankResponse(string text)
        {
            if(string.IsNullOrEmpty(text))return;var response=JsonUtility.FromJson<OutgameGetRankResponse>(text);if(response==null||response.code!=0)return;
            if(response.data.count==0)MineRankResponse?.Invoke(response);else GetRankResponse?.Invoke(response);
        }
        public void GetRankDataRequest(int category,int page,int count)
        {
            if(getRequest==null)getRequest=new OutgameGetRankRequest{category=category,name=RanklistName};
            getRequest.uid=Uid;getRequest.count=count;getRequest.page=page;Send(GetRank,getRequest);getRequest=null;
        }
        public void GetPlayerRankDataRequest(int category,int offset)
        {
            if(getRequest==null)getRequest=new OutgameGetRankRequest{category=category,name=RanklistName};
            getRequest.uid=Uid;var data=getRequest;data.page=0;data.count=0;data.offset=offset.ToString();Send(GetRank,getRequest);
        }
        public void SendRankDataRequest(string uid,string nick,string url,int wins,int losses,int rankType,int weight)
        {
            if(sendRequest==null){
                if(string.IsNullOrEmpty(uid))uid=Uid;var data=new OutgameSendRankRequest{uid=uid,category=rankType,name=RanklistName,forceUpdate=true,incr=false};
                data.score=unchecked(winScore()*wins);data.ext=new OutgameRankExtendedData{nick=nick,headUrl=url,victoryNum=wins,defeatNum=losses,totalScore=weight};sendRequest=data;
            }
            sendRequest.incr=false;Send(SendRank,sendRequest);sendRequest=null;
        }
        public void SendLevelRankDataRequest(string uid,string nick,string url,int rankType,int score)
        {
            if(sendRequest==null){if(string.IsNullOrEmpty(uid))uid=Uid;sendRequest=new OutgameSendRankRequest{uid=uid,category=rankType,name=RanklistName,forceUpdate=false,incr=false,score=score,ext=new OutgameRankExtendedData{nick=nick,headUrl=url,totalScore=score}};}
            if(rankType!=1)sendRequest.incr=true;Send(SendRank,sendRequest);sendRequest=null;
        }
    }
}
