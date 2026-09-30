using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameUiOpenHost
    {
        bool IsDisposed {get;}bool Cached {get;}object LegacyResource {set;}
        void InitGameObject(GameObject root);void AddCanvas();void CloseLoading();void Refresh();
        int OpenAnimation {get;}float OpenAnimationTime {get;}
        IEnumerator CustomOpenAnimation();IEnumerator StandardOpenAnimation(GameObject root,int kind,float duration);
        object StartCoroutine(IEnumerator routine);void OpenLater();object PageIdentity {get;}
        void Log(string message);
    }
    // BaseUI LoadGameObject27321/27323 and waitOpenAnim27343.
    public sealed class OutgameUiOpenLifecycle
    {
        readonly IOutgameUiOpenHost host;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameUiOpenLifecycle(IOutgameUiOpenHost host,Func<OutgameMessageDispatcher> messages=null)
        {this.host=host;this.messages=messages??(()=>OutgameMessageDispatcher.Shared);}
        public void LoadedModern(GameObject root)=>Loaded(root,null,false);
        public void LoadedLegacy(GameObject root,object resource)=>Loaded(root,resource,true);
        void Loaded(GameObject root,object resource,bool legacy)
        {
            if(host.IsDisposed){UnityEngine.Object.DestroyImmediate(root);return;}
            if(root==null)return;
            if(legacy)host.LegacyResource=resource;
            host.InitGameObject(root);host.AddCanvas();host.CloseLoading();host.Refresh();
            if(host.Cached){host.Log("缓存模式，不播放动画 不抛事件");return;}
            host.Log("非缓存模式，播放动画 抛事件");host.StartCoroutine(WaitOpenAnimation(root));
        }
        public IEnumerator WaitOpenAnimation(GameObject root)
        {
            string name=root.name;
            if(host.OpenAnimation==5)yield return host.StartCoroutine(host.CustomOpenAnimation());
            else yield return host.StartCoroutine(host.StandardOpenAnimation(root,host.OpenAnimation,host.OpenAnimationTime));
            if(host.IsDisposed){host.Log(name+" 播放打开动画的完，就被销毁了");yield break;}
            host.OpenLater();host.Log("抛 OpenUI 事件");messages().SendMessage("OpenUI",new object[]{host.PageIdentity});
        }
    }
}
