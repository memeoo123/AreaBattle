using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameWebRequestManagerServices
    {
        public OutgameWebRequestServices AgentServices;
        public Func<IOutgameWebRequestAgentHelper> CreateHelper;
        public Func<bool> IsKeWan,DebugEnabled;
        public Action<string> Log,NetLog,Error,Warning,Toast;
        public Action<int,string> CategoryLog;
        public Action ClearSingleton,DestroyOwner;
    }
    // WebRequestManager3644 owner. Native singleton ownership is kept by the host below.
    public sealed class OutgameWebRequestManager
    {
        readonly OutgameWebRequestManagerServices services;
        public const string SourceHelperType="GameFramework.WebRequest.UnityWebRequestAgentHelper";
        public OutgameWebRequestTaskPool Pool {get;private set;}
        public Dictionary<string,string> Headers {get;private set;}
        public Dictionary<string,int> RequestLimits {get;private set;}
        public Dictionary<string,int> RequestCounts {get;private set;}
        public float DebugElapsed {get;private set;}=-1;
        public float Timeout {get;set;}
        public int TotalAgentCount=>Pool.TotalAgentCount;
        public int FreeAgentCount=>Pool.FreeAgentCount;
        public int WorkingAgentCount=>Pool.WorkingAgentCount;
        public int WaitingTaskCount=>Pool.WaitingTaskCount;
        public int WorkingTaskCapacity {get=>Pool.Capacity;set=>Pool.Capacity=value;}
        public OutgameWebRequestManager(OutgameWebRequestManagerServices services){this.services=services;}
        public void Initialize(){services.Log("FindWebRequestAgent -> "+SourceHelperType);Pool=new OutgameWebRequestTaskPool();Timeout=30;}
        public void Update(float deltaTime,float unscaledDeltaTime)
        {
            if(services.IsKeWan())return;
            if(services.DebugEnabled()){
                if(DebugElapsed>=60)DebugElapsed=-1;
                else if(DebugElapsed>=0)DebugElapsed+=deltaTime;
            }
            Pool.Update(deltaTime,unscaledDeltaTime);
        }
        public void SetRequestHeader(string name,string value)
        {
            services.NetLog("设置Header["+name+"]："+value);
            if(Headers==null)Headers=new Dictionary<string,string>();
            if(Headers.ContainsKey(name))Headers[name]=value;else Headers.Add(name,value);
        }
        public void ClearRequestHeader(){Headers?.Clear();}
        public void RemoveAllWebRequests()=>Pool.RemoveAllTasks();
        public bool RemoveWebRequest(int id)=>Pool.RemoveTask(id);
        public OutgameWebRequestTaskInfo[] GetAllWebRequestInfos()=>Pool.GetAllTaskInfos();
        public void Shutdown(){Pool.Shutdown();services.ClearSingleton();services.DestroyOwner();}
        public void AddAgentHelper(IOutgameWebRequestAgentHelper helper)
        {
            if(Pool.TotalAgentCount>=Pool.Capacity){services.CategoryLog(131072,string.Format("当前任务池最大任务代理数量为[{0}],已达到最大上线",Pool.Capacity));return;}
            var agent=new OutgameWebRequestAgent(helper,services.AgentServices);
            agent.Started+=OnStart;agent.Succeeded+=OnSuccess;agent.Failed+=OnFailure;Pool.AddAgent(agent);
        }
        public int AddWebRequest(string uri,byte[] data,int priority,object userData,Action<int,object> callback)
        {
            if(string.IsNullOrEmpty(uri))throw new OutgameFrameworkException("无效的web地址");
            // Source <= versus >= mismatch creates an unused helper at full capacity.
            if(TotalAgentCount<=WorkingTaskCapacity&&FreeAgentCount<=0)AddAgentHelper(services.CreateHelper());
            if(services.DebugEnabled())CountRequest(uri);
            var task=OutgameWebRequestTask.Create(uri,data,priority,Timeout,userData,callback,Headers);Pool.AddTask(task);return task.SerialId;
        }
        public int AddWebRequest(string uri,object data,Action<int,object> callback,object userData)
        {
            byte[] bytes=null;
            if(data!=null){
                string json;
                try{json=LitJson.JsonMapper.ToJson(data);}
                catch(Exception e){services.Error("JSON序列化失败：$"+e.Message);json="{}";}
                bytes=Encoding.UTF8.GetBytes(json);
                if(services.DebugEnabled()&&bytes.Length>14336)services.Error(uri+"传输的数据大于14KB，请优化数据存储:"+json);
            }
            return AddWebRequest(uri,bytes,0,userData,callback);
        }
        public int AddWebRequest(string uri,byte[] data,Action<int,object> callback)=>AddWebRequest(uri,data,0,null,callback);
        public int AddWebRequest(string uri,byte[] data,int priority,Action<int,object> callback)=>AddWebRequest(uri,data,priority,null,callback);
        public int AddWebRequest(string uri,Action<int,object> callback)=>AddWebRequest(uri,null,0,null,callback);
        public int AddWebRequest(string uri,object userData,Action<int,object> callback)=>AddWebRequest(uri,null,0,userData,callback);
        public int AddWebRequest(string uri,int priority,Action<int,object> callback)=>AddWebRequest(uri,null,priority,null,callback);
        public int AddWebRequest(string uri,object data,int priority,Action<int,object> callback)
        {return AddWebRequest(uri,data==null?null:Encoding.UTF8.GetBytes(LitJson.JsonMapper.ToJson(data)),priority,null,callback);}
        public int AddWebRequest(string uri,string data,Action<int,object> callback,object userData)
        {return AddWebRequest(uri,string.IsNullOrEmpty(data)?Encoding.UTF8.GetBytes(data):null,0,userData,callback);} //28488 source inverted condition.
        void OnStart(OutgameWebRequestAgent agent)
        {
            var task=agent.Task;var e=new OutgameHttpStart{SerialId=task.SerialId,Uri=task.Uri,UserData=task.UserData};agent.Task.ResultCallback?.Invoke(0,e);e.Clear();
        }
        void OnSuccess(OutgameWebRequestAgent agent,byte[] bytes)
        {
            var task=agent.Task;var e=new OutgameHttpSuccess{SerialId=task.SerialId,Uri=task.Uri,UserData=task.UserData,Bytes=bytes};agent.Task.ResultCallback?.Invoke(1,e);e.Clear();
        }
        void OnFailure(OutgameWebRequestAgent agent,string error)
        {
            var task=agent.Task;var e=new OutgameHttpFailure{SerialId=task.SerialId,Uri=task.Uri,UserData=task.UserData,Error=error};agent.Task.ResultCallback?.Invoke(2,e);e.Clear();
        }
        void CountRequest(string uri)
        {
            if(RequestLimits==null){
                var limits=new Dictionary<string,int>();limits.Add("/data/user/ext",5);limits.Add("/data/private/update",12);limits.Add("/toplist/knock",12);RequestLimits=limits;DebugElapsed=0;
                RequestCounts=new Dictionary<string,int>();foreach(var pair in RequestLimits)RequestCounts.Add(pair.Key,0);
            }
            if(DebugElapsed==-1){DebugElapsed=0;foreach(var pair in RequestLimits)RequestCounts[pair.Key]=0;}
            foreach(var pair in RequestLimits){
                if(!uri.Contains(pair.Key))continue;
                int count=RequestCounts[pair.Key];
                if(count>=pair.Value){string warning=string.Format("{0} 一分钟里请求超过了 {1}",uri,pair.Value);services.Warning(warning);if(count==pair.Value)services.Toast(warning);}
                RequestCounts[pair.Key]=unchecked(RequestCounts[pair.Key]+1);break;
            }
        }
    }
    // Get_Instance28500 uses Find/GetComponent/AddComponent, publishes before Initialize.
    // Composition installs real platform/report endpoints before the first access.
    public sealed class OutgameWebRequestManagerHost:MonoBehaviour
    {
        public static Func<OutgameWebRequestManagerServices> ServicesFactory;
        static OutgameWebRequestManagerHost instance;
        public OutgameWebRequestManager Manager {get;private set;}
        public static OutgameWebRequestManagerHost Instance
        {
            get{
                if(!instance){
                    var root=GameObject.Find("WebRequestManager");if(!root){root=new GameObject();root.name="WebRequestManager";}
                    instance=root.GetComponent<OutgameWebRequestManagerHost>();
                    if(!instance){
                        instance=root.AddComponent<OutgameWebRequestManagerHost>();
                        var host=instance;var services=ServicesFactory();services.ClearSingleton=()=>instance=null;services.DestroyOwner=()=>Destroy(host.gameObject);
                        host.Manager=new OutgameWebRequestManager(services);host.Manager.Initialize();
                    }
                    DontDestroyOnLoad(root);
                }
                return instance;
            }
        }
        public static IOutgameWebRequestAgentHelper CreateNativeHelper(OutgameWebRequestServices services)
        {var helper=new GameObject().AddComponent<OutgameUnityWebRequestAgentHelper>();helper.Services=services;return helper;}
        void Update(){Manager.Update(Time.deltaTime,Time.unscaledDeltaTime);}
    }
}
