using System;
using System.Collections.Generic;
using UnityEngine.Networking;
namespace AreaBattle
{
    public interface IOutgameLegacyManagerOperation { bool Update(); }
    public delegate OutgameLegacyBundleResult OutgameLegacyBundleLookup(string name,out string error,out int dependencyMissing);
    // AssetBundleLoadOperationFull23795-23799, base IEnumerator23773-23775.
    public sealed class OutgameLegacyBundleOperation:IOutgameLegacyBundleOperation,IOutgameLegacyManagerOperation
    {
        readonly string name;readonly UnityWebRequest request;readonly OutgameLegacyBundleLookup lookup;
        readonly Action<string> logError;readonly Action<Exception> logException;
        string error;OutgameLegacyBundleResult result;
        public OutgameLegacyBundleOperation(string name,Func<string,UnityWebRequest> getRequest,OutgameLegacyBundleLookup lookup,Action<string> logError,Action<Exception> logException)
        {this.name=name;request=getRequest(name);this.lookup=lookup;this.logError=logError;this.logException=logException;}
        public bool Update(){result=lookup(name,out error,out var dependencyMissing);return result==null;}
        public bool IsDone()
        {
            if(result!=null)return true;
            if(request==null||error==null)return false;
            try{if("0".Equals(error))return false;logError(error);return true;}
            catch(Exception ex){logException(ex);return false;}
        }
        public float Progress()=>result!=null?1f:request==null?0f:request.downloadProgress;
        public OutgameLegacyBundleResult GetAssetBundle()=>result;
        public object Current=>null;
        public bool MoveNext()=>!IsDone();
        public void Reset(){}
    }
    // AssetBundleManager.LoadAssetBundleAsync23839. Variant/dependency loading remains the manager's responsibility.
    public sealed class OutgameLegacyBundleRequests
    {
        readonly Func<bool> debug;readonly Action<string> log;readonly Func<string,string> remap;
        readonly Action<string,bool> load;readonly Func<string,OutgameLegacyBundleOperation> create;
        readonly IList<IOutgameLegacyManagerOperation> operations;
        public OutgameLegacyBundleRequests(Func<bool> debug,Action<string> log,Func<string,string> remap,Action<string,bool> load,
            Func<string,OutgameLegacyBundleOperation> create,IList<IOutgameLegacyManagerOperation> operations)
        {this.debug=debug;this.log=log;this.remap=remap;this.load=load;this.create=create;this.operations=operations;}
        // Operation-list phase of AssetBundleManager.Update23843, after download registration/removal.
        public void UpdateOperations()
        {
            int index=0;
            while(index<operations.Count)
            {if(operations[index].Update())index++;else operations.RemoveAt(index);}
        }
        public IOutgameLegacyBundleOperation LoadAsync(string name)
        {
            if(debug())log("Loading "+name+" bundle");
            var mapped=remap(name);load(mapped,false);
            var operation=create(mapped);operations.Add(operation);return operation;
        }
    }
}
