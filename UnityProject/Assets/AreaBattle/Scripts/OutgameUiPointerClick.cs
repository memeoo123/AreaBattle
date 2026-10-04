using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AreaBattle
{
    // EventListener's pointer-click branch only (source2907); other event branches are separate.
    public sealed class OutgameUiPointerClick:MonoBehaviour,IPointerClickHandler
    {
        public Action<PointerEventData> Click;
        public void OnPointerClick(PointerEventData data)=>Click?.Invoke(data);
        public static OutgameUiPointerClick Get(GameObject root)
        {var listener=root.GetComponent<OutgameUiPointerClick>();return listener?listener:root.AddComponent<OutgameUiPointerClick>();}
    }
    public static class OutgameUiClick
    {
        // UIExtension22718 and its closure: callback first; no message if it throws.
        public static void Add(Button button,Action callback,Func<OutgameMessageDispatcher> messages)
        {button.onClick.AddListener(()=>{callback();messages().SendMessage("GF_UIButtonClick",new object[]{button});});}
        // UIExtension22717 generic Image overload replaces the pointer delegate and has no button message.
        public static void Add(Image image,Action callback)=>OutgameUiPointerClick.Get(image.gameObject).Click=data=>callback();
        // UIExtension22716 uses EventListener.Get(GameObject) with the same replacement semantics.
        public static void Add(GameObject root,Action callback)=>OutgameUiPointerClick.Get(root).Click=data=>callback();
    }
}
