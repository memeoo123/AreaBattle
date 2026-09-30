using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameAdaptiveBangs:MonoBehaviour
    {
        public bool IsNeedAdaptiveBangs,IsDoubleEnded;
        // Local composition reference to UIModule.canvas; assigned by its root-load callback before Start.
        public GameObject ModuleCanvas {get;set;}
        OutgameBangsAdaptation adaptation;
        public static void SetBangsPixel(int pixel)=>OutgameBangsState.Shared.SetBangsPixel(pixel);
        void Start()
        {
            adaptation=new OutgameBangsAdaptation(GetComponent<RectTransform>(),OutgameUiDisplaySettings.Shared,OutgameBangsState.Shared,()=>ModuleCanvas.GetComponent<RectTransform>(),Debug.Log)
            {IsNeedAdaptiveBangs=IsNeedAdaptiveBangs,IsDoubleEnded=IsDoubleEnded};
            adaptation.Start(Screen.width,Screen.height);
        }
    }
}
