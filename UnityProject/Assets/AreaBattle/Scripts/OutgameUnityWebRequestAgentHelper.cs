using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace AreaBattle
{
    // ReportManager.ReportGameNetInfo boundary; platform report delivery is composed separately.
    public sealed class OutgameWebRequestServices
    {
        public Func<bool> EncryptionEnabled;
        public Action<int,string,string,int,string,int> Report;
        public Action<string> Log,Error;
        public static int Milliseconds(float seconds)
        {
            double value=seconds*1000f;
            return Math.Abs(value)<2147483648d?(int)value:int.MinValue;
        }
    }
    // Source3634/3635 allocate fresh event args, then Clear after the callback returns.
    // They do not use the unrelated rank row reference pool.
    public sealed class OutgameWebRequestCompleteEventArgs:EventArgs
    {
        public byte[] Bytes {get;private set;}
        public static OutgameWebRequestCompleteEventArgs Create(byte[] bytes)=>new OutgameWebRequestCompleteEventArgs{Bytes=bytes};
        public void Clear(){Bytes=null;}
    }
    public sealed class OutgameWebRequestErrorEventArgs:EventArgs
    {
        public string ErrorMessage {get;private set;}
        public static OutgameWebRequestErrorEventArgs Create(string error)=>new OutgameWebRequestErrorEventArgs{ErrorMessage=error};
        public void Clear(){ErrorMessage=null;}
    }
    public interface IOutgameWebRequestAgentHelper
    {
        event EventHandler<OutgameWebRequestCompleteEventArgs> Complete;
        event EventHandler<OutgameWebRequestErrorEventArgs> Error;
        void Request(string uri,Dictionary<string,string> headers);
        void Request(string uri,byte[] bytes,Dictionary<string,string> headers);
        void Reset();
        void Shutdown();
    }
    // UnityWebRequestAgentHelper3642 and base3643; the Unity Update owns completion polling.
    public class OutgameUnityWebRequestAgentHelper:MonoBehaviour,IOutgameWebRequestAgentHelper,IDisposable
    {
        public OutgameWebRequestServices Services;
        public UnityWebRequest RequestHandle {get;private set;}
        public bool IsDisposed {get;private set;}
        float started;
        public event EventHandler<OutgameWebRequestCompleteEventArgs> Complete;
        public event EventHandler<OutgameWebRequestErrorEventArgs> Error;
        void Awake(){DontDestroyOnLoad(this);}
        public void Request(string uri,Dictionary<string,string> headers)
        {
            if(Complete==null||Error==null){Services.Error("Web request agent helper handler is invalid.");return;}
            RequestHandle=UnityWebRequest.Get(uri);
            RequestHandle.SetRequestHeader("Content-Type","application/json;charset=UTF-8");
            Send(headers);
        }
        public void Request(string uri,byte[] bytes,Dictionary<string,string> headers)
        {
            if(Complete==null||Error==null)return;
            RequestHandle=new UnityWebRequest(uri,"POST");
            RequestHandle.uploadHandler=new UploadHandlerRaw(bytes);
            RequestHandle.downloadHandler=new DownloadHandlerBuffer();
            RequestHandle.SetRequestHeader("Content-Type","application/json;charset=UTF-8");
            if(Services.EncryptionEnabled())RequestHandle.SetRequestHeader("X-Encrypted","true");
            Send(headers);
        }
        void Send(Dictionary<string,string> headers)
        {
            if(headers!=null)foreach(var pair in headers)RequestHandle.SetRequestHeader(pair.Key,pair.Value);
            started=Time.unscaledTime;
            RequestHandle.SendWebRequest();
        }
        int ResponseCode()
        {
            try{return unchecked((int)RequestHandle.responseCode);}
            catch(Exception e){Services.Error(string.Format("获取错误码失败：{0}，详细错误: {1}",RequestHandle.responseCode,e.Message));return -9999;}
        }
        public void Update()
        {
            if(RequestHandle==null||!RequestHandle.isDone)return;
#pragma warning disable CS0618 // Preserve the original network/http error predicates.
            if(RequestHandle.isNetworkError||RequestHandle.isHttpError){
#pragma warning restore CS0618
                float elapsed=Time.unscaledTime-started;int code=ResponseCode();
                Services.Report(0,RequestHandle.url,"Fail",code,RequestHandle.error,OutgameWebRequestServices.Milliseconds(elapsed));
                var e=OutgameWebRequestErrorEventArgs.Create(RequestHandle.error);
                Error(this,e);e.Clear();
            }else if(RequestHandle.downloadHandler.isDone){
                float elapsed=Time.unscaledTime-started;int code=ResponseCode();
                Services.Report(0,RequestHandle.url,"Success",code,string.Empty,OutgameWebRequestServices.Milliseconds(elapsed));
                var e=OutgameWebRequestCompleteEventArgs.Create(RequestHandle.downloadHandler.data);
                Complete(this,e);e.Clear();
            }
            // Source leaves the request here: the agent callback, not this helper, resets it.
        }
        public void Reset(){if(RequestHandle!=null){RequestHandle.Dispose();RequestHandle=null;}}
        protected virtual void Dispose(bool disposing)
        {
            if(IsDisposed)return;
            if(disposing&&RequestHandle!=null){RequestHandle.Dispose();RequestHandle=null;}
            IsDisposed=true;
        }
        public void Dispose(){Dispose(true);GC.SuppressFinalize(this);}
        public void Shutdown(){Services.Log("摧毁下载辅助器");Dispose();Destroy(gameObject);}
    }
    // Original obfuscated task3645 and inherited3808. Headers copy; body and user data alias.
    public sealed class OutgameWebRequestTask
    {
        static int serial;
        public int SerialId {get;private set;}
        public int Priority;
        public byte Status;
        public string Description=>Uri;
        public bool Done;
        public string Uri {get;private set;}
        public byte[] PostData {get;private set;}
        public float Timeout {get;private set;}
        public object UserData {get;private set;}
        public Action<int,object> ResultCallback {get;private set;}
        public Dictionary<string,string> Headers {get;private set;}
        public static OutgameWebRequestTask Create(string uri,byte[] postData,int priority,float timeout,object userData,Action<int,object> callback,Dictionary<string,string> headers)
        {
            var task=new OutgameWebRequestTask{SerialId=unchecked(++serial),Priority=priority,ResultCallback=callback,Uri=uri,PostData=postData,Timeout=timeout,UserData=userData};
            if(headers!=null){task.Headers=new Dictionary<string,string>();foreach(var pair in headers)task.Headers.Add(pair.Key,pair.Value);}
            return task;
        }
        public void Clear(){SerialId=0;Priority=0;ResultCallback=null;Done=false;Uri=null;Status=0;PostData=null;Timeout=0;UserData=null;} //28519 retains Headers.
    }
    // Original agent3640. Queue ownership and manager events are separate from helper IO.
    public sealed class OutgameWebRequestAgent:IOutgameWebRequestAgent
    {
        readonly IOutgameWebRequestAgentHelper helper;
        readonly OutgameWebRequestServices services;
        float elapsed;
        public OutgameWebRequestTask Task {get;private set;}
        public float FreeWaitTimes {get;set;}
        public Action<OutgameWebRequestAgent> Started;
        public Action<OutgameWebRequestAgent,byte[]> Succeeded;
        public Action<OutgameWebRequestAgent,string> Failed;
        public OutgameWebRequestAgent(IOutgameWebRequestAgentHelper helper,OutgameWebRequestServices services)
        {this.helper=helper??throw new OutgameFrameworkException("Web request agent helper is invalid.");this.services=services;}
        public void Initialize(){FreeWaitTimes=0;helper.Complete+=OnComplete;helper.Error+=OnError;}
        public void Reset(){helper.Reset();Task=null;elapsed=0;}
        public void Shutdown(){Reset();helper.Complete-=OnComplete;helper.Error-=OnError;helper.Shutdown();}
        public int Start(OutgameWebRequestTask task)
        {
            if(task==null)throw new OutgameFrameworkException("Task is invalid.");
            Task=task;Task.Status=1;Started?.Invoke(this);
            var headers=task.Headers;var current=Task;
            if(current.PostData==null)helper.Request(current.Uri,headers);else helper.Request(current.Uri,current.PostData,headers);
            elapsed=0;return 1;
        }
        public void Update(float deltaTime,float unscaledDeltaTime)
        {
            if(Task.Status!=1)return;
            elapsed+=unscaledDeltaTime;
            if(elapsed>=Task.Timeout){
                services.Report(0,Task.Uri,"Fail",0,"Timeout",OutgameWebRequestServices.Milliseconds(Task.Timeout));
                var e=OutgameWebRequestErrorEventArgs.Create("Timeout");OnError(this,e);e.Clear();
            }
        }
        void OnComplete(object sender,OutgameWebRequestCompleteEventArgs e)
        {helper.Reset();Task.Status=2;Succeeded?.Invoke(this,e.Bytes);Task.Done=true;}
        void OnError(object sender,OutgameWebRequestErrorEventArgs e)
        {helper.Reset();Task.Status=3;Failed?.Invoke(this,e.ErrorMessage);Task.Done=true;}
    }
}
