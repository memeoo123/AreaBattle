using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameAssetProvider
    {
        bool IsDestroyed {get;}
        bool IsDone {get;}
        UnityEngine.Object AssetObject {get;}
        void ReleaseHandle(OutgameAssetHandle handle);
    }
    // AssetOperationHandle mainObject/Release/default Instantiate and OperationHandleBase validity.
    // Completion subscriptions and provider loading itself are separate source contracts.
    public sealed class OutgameAssetHandle:IOutgamePoolAssetHandle
    {
        IOutgameAssetProvider provider;
        Action<OutgameAssetHandle> completed;
        readonly Func<bool> compatibilityMode;
        readonly Action<string> warning;
        readonly string assetPath;
        public UnityEngine.Object DirectMainObject {get;set;}
        public OutgameAssetHandle(string assetPath,IOutgameAssetProvider provider,Func<bool> compatibilityMode,Action<string> warning)
        {this.assetPath=assetPath;this.provider=provider;this.compatibilityMode=compatibilityMode;this.warning=warning;}
        internal bool HasLiveProvider=>provider!=null&&!provider.IsDestroyed;
        bool IsValid()
        {
            if(compatibilityMode())return true;
            if(provider==null){warning("Operation handle is released : "+assetPath);return false;}
            if(provider.IsDestroyed){warning("Provider is destroyed : "+assetPath);return false;}
            return true;
        }
        public event Action<OutgameAssetHandle> Completed
        {
            add{
                if(!IsValid())throw new Exception("RuntimeAssetHandle is invalid");
                if(provider.IsDone){value(this);return;}
                completed+=value;
            }
            remove{
                if(!IsValid())throw new Exception("RuntimeAssetHandle is invalid");
                completed-=value;
            }
        }
        // Original completion dispatch retains the delegate list and does not catch callback errors.
        public void InvokeCompleted()=>completed?.Invoke(this);
        public UnityEngine.Object MainObject=>compatibilityMode()?DirectMainObject:(IsValid()?provider.AssetObject:null);
        public void Release()
        {
            if(!compatibilityMode()&&IsValid()){provider.ReleaseHandle(this);provider=null;}
        }
        public GameObject Instantiate()
        {
            UnityEngine.Object original;
            if(compatibilityMode())original=DirectMainObject;
            else{if(!IsValid())return null;original=provider.AssetObject;if(original==null)return null;}
            if(original==null)return null;
            var instance=UnityEngine.Object.Instantiate(original as GameObject);
            instance.SetActive(true);instance.name=original.name;
            return instance;
        }
    }
}
