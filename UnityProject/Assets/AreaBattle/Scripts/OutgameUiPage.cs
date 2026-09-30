using System;
using UnityEngine;
namespace AreaBattle
{
    // UIObject27434/27435 then BaseUI27322; concrete visibility only, not full resource loading.
    public class OutgameUiPage : IOutgameMenuItem
    {
        readonly Func<GameObject> getObject;
        public GameObject GameObject=>getObject();
        public bool Visible { get; private set; } = true;
        readonly Func<OutgameMessageDispatcher> messages;
        public OutgameUiPage(GameObject gameObject,Func<OutgameMessageDispatcher> messages=null)
         :this(()=>gameObject,messages){}
        public OutgameUiPage(Func<GameObject> getObject,Func<OutgameMessageDispatcher> messages=null)
        {this.getObject=getObject;this.messages=messages??(()=>OutgameMessageDispatcher.Shared);}
        public virtual void SetVisible(bool visible)
        {
            VisibleBefore(visible);VisibleImp(visible);Visible=visible;
            messages().SendMessage("GF_VisibleUI",new object[]{this,visible});
        }
        protected virtual void VisibleBefore(bool visible){}
        protected virtual void VisibleImp(bool visible)
        {if(GameObject)ApplyObjectVisibility(GameObject,visible);}
        // ExtensionMethods27678: hidden objects remain active; no original-scale restoration.
        public static void ApplyObjectVisibility(GameObject target,bool visible)
        {
            var transform=target.transform;
            if(!target.activeSelf)target.SetActive(true);
            transform.localScale=visible?Vector3.one:Vector3.zero;
        }
    }
}
