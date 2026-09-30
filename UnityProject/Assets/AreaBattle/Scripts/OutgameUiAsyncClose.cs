using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameUiAsyncCloseHost
    {
        void CloseBefore();int CloseAnimation {get;}float CloseAnimationTime {get;}GameObject GameObject {get;}
        Task CustomCloseAnimation();Task StandardCloseAnimation(GameObject root,int animation,float duration);
        void MarkDisposed();Task EndOfFrame(WaitForEndOfFrame instruction);void SetVisible(bool visible);void Destroy(GameObject root);void Dispose();
        bool UsesNewResources {get;}void ReleaseMainHandle();void UnloadUnusedAssets();string UIPath {get;}
        void UnloadUnusedBundle(string path);void ReleaseDynamicResources();void ReleaseCustomResources();
        object PageIdentity {get;}
    }
    // BaseUI._closeUI state machine27347. Await boundaries and resource branches mirror the original.
    public sealed class OutgameUiAsyncClose
    {
        readonly IOutgameUiAsyncCloseHost host;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameUiAsyncClose(IOutgameUiAsyncCloseHost host,Func<OutgameMessageDispatcher> messages=null)
        {this.host=host;this.messages=messages??(()=>OutgameMessageDispatcher.Shared);}
        public async void Close()=>await CloseAsync();
        public async Task CloseAsync()
        {
            host.CloseBefore();
            if(host.CloseAnimation==5)await host.CustomCloseAnimation();
            else {float duration=host.CloseAnimationTime;int animation=host.CloseAnimation;var root=host.GameObject;await host.StandardCloseAnimation(root,animation,duration);}
            host.MarkDisposed();await host.EndOfFrame(new WaitForEndOfFrame());
            host.SetVisible(false);host.Destroy(host.GameObject);host.Dispose();
            await host.EndOfFrame(new WaitForEndOfFrame());
            if(host.UsesNewResources){host.ReleaseMainHandle();host.UnloadUnusedAssets();}
            else host.UnloadUnusedBundle("UI/"+host.UIPath+".prefab");
            host.ReleaseDynamicResources();host.ReleaseCustomResources();
            messages().SendMessage("CloseUI",new object[]{host.PageIdentity});
        }
    }
}
