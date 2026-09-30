using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameUiLoader
    {
        OutgameAssetHandle LoadModern(string path,string layer,Action<GameObject,OutgameAssetHandle> complete);
        void LoadLegacy(string path,string layer,Action<GameObject,object> complete);
    }
    // BaseUI._openUI27320: the returned handle assignment is after LoadUI returns.
    public sealed class OutgameUiLoadRequest
    {
        readonly OutgameUiLifetime lifetime;readonly OutgameUiOpenLifecycle completion;
        readonly Action showLoading;readonly Func<bool> modern;readonly Func<IOutgameUiLoader> module;
        readonly Func<string> path,layer;readonly Action<OutgameAssetHandle> storeMainHandle;
        public OutgameUiLoadRequest(OutgameUiLifetime lifetime,OutgameUiOpenLifecycle completion,Action showLoading,
            Func<bool> modern,Func<IOutgameUiLoader> module,Func<string> path,Func<string> layer,Action<OutgameAssetHandle> storeMainHandle)
        {this.lifetime=lifetime;this.completion=completion;this.showLoading=showLoading;this.modern=modern;this.module=module;this.path=path;this.layer=layer;this.storeMainHandle=storeMainHandle;}
        public void Open(object[] arguments)
        {
            lifetime.Arguments=arguments;showLoading();bool useModern=modern();var loader=module();string uiPath=path();
            if(useModern)storeMainHandle(loader.LoadModern(uiPath,layer(),(root,handle)=>completion.LoadedModern(root)));
            else loader.LoadLegacy(uiPath,layer(),completion.LoadedLegacy);
        }
    }
}
