using UnityEngine;
namespace AreaBattle
{
    // AutoCanvasLayer30193 and TransformTools.FindNearCanvas: parent walk excludes self.
    public sealed class OutgameAutoCanvasLayer:MonoBehaviour
    {
        public int OrderOffer;public Canvas OwnedCanvas;
        void Start()=>Apply();
        public void Apply()
        {
            if(!OwnedCanvas)OwnedCanvas=GetComponent<Canvas>();
            Canvas parent=null;var current=transform.parent;
            while(current){parent=current.GetComponent<Canvas>();if(parent)break;current=current.parent;}
            if(OwnedCanvas&&parent){OwnedCanvas.sortingLayerID=parent.sortingLayerID;OwnedCanvas.overrideSorting=true;OwnedCanvas.sortingOrder=unchecked(parent.sortingOrder+OrderOffer);}
        }
    }
}
