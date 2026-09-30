using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace AreaBattle
{
    // Desktop persistence adapter. Caller supplies an isolated reconstruction directory and dispatcher.
    public sealed class OutgameFileStorageBackend:IOutgameStorageBackend
    {
        readonly string root;readonly Action<Action> dispatch;
        public OutgameFileStorageBackend(string directory,Action<Action> dispatch)
        {root=Path.GetFullPath(directory);this.dispatch=dispatch??throw new ArgumentNullException(nameof(dispatch));Directory.CreateDirectory(root);}
        string PathFor(string key)
        {
            using(var hash=SHA256.Create())return Path.Combine(root,BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-","")+".value");
        }
        public string Get(string key){string path=PathFor(key);return File.Exists(path)?File.ReadAllText(path,Encoding.UTF8):"";}
        void Queue(Action action,Action<string> fail,Action complete)
        {
            dispatch(()=>{try{action();}catch(Exception e){fail(e.Message);}finally{complete();}});
        }
        public void Set(string key,string value,Action<string> fail,Action complete)=>Queue(()=>File.WriteAllText(PathFor(key),value,new UTF8Encoding(false)),fail,complete);
        public void Remove(string key,Action<string> fail,Action complete)=>Queue(()=>File.Delete(PathFor(key)),fail,complete);
        public void Clear(Action<string> fail,Action complete)=>Queue(()=>{foreach(string path in Directory.EnumerateFiles(root,"*.value",SearchOption.TopDirectoryOnly))File.Delete(path);},fail,complete);
    }
}
