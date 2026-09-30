using System;
using UnityEngine;
namespace AreaBattle
{
    // UIObject source references8/12/16/20, BaseUI args44; collections/arguments remain opaque held references.
    public class OutgameUiLifetime
    {
        public object ObjectList {get;set;}
        public GameObject GameObject {get;private set;}
        public Transform Transform {get;private set;}
        public RectTransform RectTransform {get;private set;}
        public object Arguments {get;set;}
        public bool IsDisposed {get;private set;}
        readonly Action closeBefore;readonly Action<GameObject> destroy;
        public OutgameUiLifetime(GameObject root,Action closeBefore,Action<GameObject> destroy=null)
        {this.closeBefore=closeBefore;this.destroy=destroy??(value=>UnityEngine.Object.Destroy(value));if(root!=null)Attach(root,null);}
        public void Attach(GameObject root,Action normalize)
        {GameObject=root;Transform=root.transform;normalize?.Invoke();RectTransform=root.GetComponent<RectTransform>();}
        // BaseUI.CloseUINow27328. No idempotence guard or invented resource release.
        public void CloseUINow(){CloseBefore();MarkDisposed();DestroyObject(GameObject);Dispose();}
        public void CloseBefore()=>closeBefore();
        public void MarkDisposed()=>IsDisposed=true;
        public void DestroyObject(GameObject root)=>destroy(root);
        // BaseUI.Dispose27338 retains rectTransform20, which is not cleared in the source.
        public virtual void Dispose(){ObjectList=null;IsDisposed=true;GameObject=null;Transform=null;Arguments=null;}
    }
    public sealed class OutgameFestUiLifetime:OutgameUiLifetime
    {
        readonly Func<OutgameMessageDispatcher> messages;
        public OutgameFestUiLifetime(GameObject root,Action closeBefore,Func<OutgameMessageDispatcher> messages=null,Action<GameObject> destroy=null):base(root,closeBefore,destroy)
        {this.messages=messages??(()=>OutgameMessageDispatcher.Shared);}
        // FestActUI.Dispose34025.
        public override void Dispose(){base.Dispose();messages().SendMessage("CheckUISortAfterStartUIShow");}
    }
}
