using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameStorageBackend
    {
        string Get(string key);
        void Set(string key,string value,Action<string> fail,Action complete);
        void Remove(string key,Action<string> fail,Action complete);
        void Clear(Action<string> fail,Action complete);
    }
    // Original unity-sdk/storage.js string API: cache changes synchronously, writes queue asynchronously.
    public sealed class OutgameSdkStringStorage
    {
        sealed class Operation {public int type;public string key,value;}
        readonly IOutgameStorageBackend backend;readonly Action<string> logError;
        readonly Dictionary<string,string> cache=new Dictionary<string,string>();readonly Queue<Operation> pending=new Queue<Operation>();
        bool running,deletedAll;
        public OutgameSdkStringStorage(IOutgameStorageBackend backend,Action<string> logError)
        {this.backend=backend??throw new ArgumentNullException(nameof(backend));this.logError=logError??throw new ArgumentNullException(nameof(logError));}
        public string GetString(string key,string fallback="")
        {
            string value;
            if(cache.TryGetValue(key,out value))return (value??fallback)??"";
            if(deletedAll)return fallback??"";
            try{value=backend.Get(key);cache[key]=value==""?null:value;return (value==""?fallback:value)??"";}
            catch{return fallback??"";}
        }
        public bool HasKey(string key)=>GetString(key,"")!="";
        public void SetString(string key,string value){cache[key]=value;pending.Enqueue(new Operation{type=0,key=key,value=value});Run();}
        public void DeleteKey(string key){cache[key]=null;pending.Enqueue(new Operation{type=1,key=key});Run();}
        public void DeleteAll()
        {
            foreach(var key in new List<string>(cache.Keys))cache[key]=null;
            deletedAll=true;pending.Enqueue(new Operation{type=2});Run();
        }
        void Run()
        {
            if(running||pending.Count==0)return;
            running=true;var op=pending.Dequeue();string key=string.IsNullOrEmpty(op.key)?"defaultKey":op.key;
            if(op.type==0)backend.Set(key,op.value,logError,Completed);
            else if(op.type==1)backend.Remove(key,logError,Completed);
            else backend.Clear(logError,Completed);
        }
        void Completed(){running=false;Run();}
    }
}
