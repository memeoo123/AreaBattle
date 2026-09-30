using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Concrete composition for BaseUI27347; platform resource routing stays supplied
    // by its owning module, while lifetime/visibility/handles use recovered objects.
    public sealed class OutgameUiCloseHost:IOutgameUiAsyncCloseHost
    {
        readonly OutgameUiLifetime lifetime;readonly OutgameUiPage page;
        readonly OutgameUiAnimation animation;readonly OutgameUiResourceLists resources;
        readonly Func<bool> usesNewResources;readonly Func<Task> customAnimation;
        readonly Action unloadUnused;readonly Action<string> unloadBundle;
        readonly Func<WaitForEndOfFrame,Task> frame;
        public int CloseAnimation {get;set;}public float CloseAnimationTime {get;set;}
        public string UIPath {get;}public OutgameAssetHandle MainHandle {get;set;}
        public GameObject GameObject=>lifetime.GameObject;public object PageIdentity=>page;
        public bool UsesNewResources=>usesNewResources();
        public OutgameUiCloseHost(OutgameUiLifetime lifetime,OutgameUiPage page,OutgameUiAnimation animation,
            OutgameUiResourceLists resources,string path,Func<bool> usesNewResources,Func<Task> customAnimation,
            Action unloadUnused,Action<string> unloadBundle,Func<WaitForEndOfFrame,Task> frame=null)
        {
            this.lifetime=lifetime;this.page=page;this.animation=animation;this.resources=resources;UIPath=path;
            this.usesNewResources=usesNewResources;this.customAnimation=customAnimation;this.unloadUnused=unloadUnused;
            this.unloadBundle=unloadBundle;this.frame=frame??WaitFrame;
        }
        static async Task WaitFrame(WaitForEndOfFrame instruction){await OutgameUnityAwait.Await(instruction);}
        public void CloseBefore()=>lifetime.CloseBefore();
        public Task CustomCloseAnimation()=>customAnimation();
        public Task StandardCloseAnimation(GameObject root,int kind,float duration)=>animation.Play(root,kind,duration);
        public void MarkDisposed()=>lifetime.MarkDisposed();
        public Task EndOfFrame(WaitForEndOfFrame instruction)=>frame(instruction);
        public void SetVisible(bool value)=>page.SetVisible(value);
        public void Destroy(GameObject root)=>lifetime.DestroyObject(root);
        public void Dispose()=>lifetime.Dispose();
        public void ReleaseMainHandle()=>MainHandle.Release();
        public void UnloadUnusedAssets()=>unloadUnused();
        public void UnloadUnusedBundle(string path)=>unloadBundle(path);
        public void ReleaseDynamicResources()=>resources.ReleaseDynamic();
        public void ReleaseCustomResources()=>resources.ReleaseCustom();
    }
}
