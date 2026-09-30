using UnityEngine;
namespace AreaBattle
{
    // Native lifecycle adapter; source serialized values are applied by the bootstrap importer.
    public sealed class OutgameCanvasAdaptive:MonoBehaviour
    {
        public float whRatioConst,widthControlsHeightFactor,heightControlsWidthFactor=1f;
        public bool isHeightCtrWidthFixedWidth=true;
        OutgameCanvasAdaptation adaptation;
        void Awake()
        {
            adaptation=new OutgameCanvasAdaptation(gameObject,OutgameUiDisplaySettings.Shared,()=>Screen.width,()=>Screen.height,routine=>StartCoroutine(routine),Debug.Log,()=>OutgameMessageDispatcher.Shared)
            {WhRatioConst=whRatioConst,WidthControlsHeightFactor=widthControlsHeightFactor,HeightControlsWidthFactor=heightControlsWidthFactor,IsHeightControlsWidthFixedWidth=isHeightCtrWidthFixedWidth};
            adaptation.Initialize();whRatioConst=adaptation.WhRatioConst;
        }
        void Update()=>adaptation.Update();
    }
}
