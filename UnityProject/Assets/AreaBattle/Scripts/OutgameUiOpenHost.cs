using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    // Concrete owner of the previously recovered BaseUI load-completion contract.
    public sealed class OutgameUiOpenHost:IOutgameUiOpenHost
    {
        readonly OutgameUiLifetime lifetime;readonly OutgameUiPage page;
        readonly OutgameUiObjectInitialization initialization;readonly OutgameUiCanvas canvas;
        readonly OutgameUiAnimation animation;readonly Action closeLoading,refresh,openLater;
        readonly Func<IEnumerator> customAnimation;readonly Action<string> log;
        public bool IsDisposed=>lifetime.IsDisposed;public bool Cached {get;set;}
        public object LegacyResource {get;set;}public int Layer {get;set;}
        public int OpenAnimation {get;set;}public float OpenAnimationTime {get;set;}
        public object PageIdentity=>page;
        public OutgameUiOpenHost(OutgameUiLifetime lifetime,OutgameUiPage page,OutgameUiObjectInitialization initialization,
            OutgameUiCanvas canvas,OutgameUiAnimation animation,Action closeLoading,Action refresh,Action openLater,
            Func<IEnumerator> customAnimation,Action<string> log)
        {
            this.lifetime=lifetime;this.page=page;this.initialization=initialization;this.canvas=canvas;this.animation=animation;
            this.closeLoading=closeLoading;this.refresh=refresh;this.openLater=openLater;this.customAnimation=customAnimation;this.log=log;
        }
        public void InitGameObject(GameObject root)=>initialization.Init(root);
        public void AddCanvas()=>canvas.Add(lifetime.GameObject,Layer);
        public void CloseLoading()=>closeLoading();public void Refresh()=>refresh();public void OpenLater()=>openLater();
        public IEnumerator CustomOpenAnimation()=>customAnimation();
        public IEnumerator StandardOpenAnimation(GameObject root,int kind,float duration)=>animation.PlayRoutine(root,kind,duration);
        public object StartCoroutine(IEnumerator routine)=>animation.StartCoroutine(routine);
        public void Log(string message)=>log(message);
    }
}
