using System;
using UnityEngine;
using UnityEngine.Events;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameRedDotEvent:UnityEvent<OutgameRedDotItem>{}
    public enum OutgameRedDotAnchorType {RightTop,RightBottom,LeftTop,LeftBottom}
    // Proj_hdzd.RedDot.RedDotItemBase4457; preserve source typo CheackRedDot and serialized fields.
    public class OutgameRedDotItem:MonoBehaviour
    {
        public Sprite DotSprite;
        public GameObject DotPrefab;
        public Vector2 Scale=Vector2.one;
        public OutgameRedDotAnchorType RectAnchorType;
        public Vector2 PosOffset;
        public OutgameRedDotEvent CheckActionBool;
        public bool IsShowRedDot {get;set;}
        public object DotRedParam {get;private set;}
        Func<OutgameRedDotControl> control;
        // Supplied by the owning module before native Start; binding does not create a controller or invoke lifecycle.
        public void BindController(Func<OutgameRedDotControl> resolver)=>control=resolver;
        public void InitRedDot(object parameter)=>DotRedParam=parameter;
        void Start(){if(CheckActionBool!=null)control().AddItem(this);}
        public virtual void CheackRedDot(){IsShowRedDot=false;CheckActionBool?.Invoke(this);}
        public virtual void ShowRedDot()=>DotPrefab.SetActive(IsShowRedDot);
        void OnDestroy()=>control().RemoveItem(this);
    }
}
