using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // UIModule.LoadUIRootOver<T>27386. Acquisition/Instantiate dispatch is supplied by the original resource route.
    public sealed class OutgameUiRootInitialization
    {
        readonly GameObject canvas;
        readonly Func<bool> isPortrait,fixedWidth;
        readonly Action initialized;
        public RectTransform UiRoot {get;private set;}
        public bool IsInitialized {get;private set;}
        public OutgameUiRootInitialization(GameObject canvas,Func<bool> isPortrait,Func<bool> fixedWidth,Action initialized)
        {this.canvas=canvas;this.isPortrait=isPortrait;this.fixedWidth=fixedWidth;this.initialized=initialized;}
        public void Loaded(Func<string,bool,GameObject> instantiate)
        {
            UiRoot=instantiate("UIRoot",true).GetComponent<RectTransform>();
            var bangs=UiRoot.GetComponent<OutgameAdaptiveBangs>();if(bangs!=null)bangs.ModuleCanvas=canvas;
            UiRoot.transform.SetParent(canvas.transform);
            UiRoot.localScale=Vector3.one;
            UiRoot.anchorMin=Vector2.zero;UiRoot.anchorMax=Vector2.one;
            UiRoot.offsetMin=Vector2.zero;UiRoot.offsetMax=Vector2.zero;
            if(isPortrait()&&canvas.GetComponent<CanvasScaler>().matchWidthOrHeight==1f&&fixedWidth())
            {
                float width=canvas.GetComponent<RectTransform>().sizeDelta.x;
                if(width>720f)
                {
                    float inset=(width-720f)*.5f;
                    UiRoot.offsetMax=new Vector2(-inset,0f);
                    UiRoot.offsetMin=new Vector2(inset,0f);
                }
            }
            IsInitialized=true;
            initialized?.Invoke();
        }
    }
}
