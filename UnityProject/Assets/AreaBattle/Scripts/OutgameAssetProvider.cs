using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Provider2954 handle bookkeeping/completion; bundle acquisition/loading remain separate.
    public sealed class OutgameAssetProvider:IOutgameAssetProvider
    {
        readonly List<OutgameAssetHandle> handles=new List<OutgameAssetHandle>();
        readonly Func<bool> suppressCompletion;readonly Action<string> warning;
        TaskCompletionSource<object> taskSource;
        public int Status {get;set;}
        public float Progress {get;set;}
        public int RefCount {get;private set;}
        public bool IsDestroyed {get;set;}
        public UnityEngine.Object AssetObject {get;set;}
        public OutgameBundleReference OwnerBundle {get;set;}
        public OutgameBundleDependencies DependBundles {get;set;}
        public bool IsDone=>!suppressCompletion()&&(Status&~1)==4;
        public bool CanDestroy=>IsDone&&RefCount<1;
        public OutgameAssetProvider(Func<bool> suppressCompletion,Action<string> warning)
        {this.suppressCompletion=suppressCompletion;this.warning=warning;}
        public OutgameAssetHandle CreateHandle(string path,Func<bool> compatibilityMode)
        {
            RefCount=unchecked(RefCount+1);
            var handle=new OutgameAssetHandle(path,this,compatibilityMode,warning);handles.Add(handle);return handle;
        }
        public void ReleaseHandle(OutgameAssetHandle handle)
        {
            if(RefCount<=0)warning("Asset provider reference count is already zero. There may be resource leaks !");
            if(!handles.Remove(handle))throw new Exception("Should never get here !");
            RefCount=unchecked(RefCount-1);
        }
        public Task Task
        {
            get{if(taskSource==null){taskSource=new TaskCompletionSource<object>();if(IsDone)taskSource.SetResult(null);}return taskSource.Task;}
        }
        // Source23028 marks destroyed before releasing ownership; it does not check CanDestroy.
        public void Destroy()
        {
            IsDestroyed=true;
            if(OwnerBundle!=null){OwnerBundle.RefCount=unchecked(OwnerBundle.RefCount-1);OwnerBundle=null;}
            if(DependBundles!=null){DependBundles.Release();DependBundles=null;}
        }
        public void InvokeCompletion()
        {
            if(suppressCompletion())return;
            Progress=1;
            foreach(var handle in new List<OutgameAssetHandle>(handles))
                if(handle.HasLiveProvider)handle.InvokeCompleted();
            taskSource?.TrySetResult(null);
        }
    }
}
