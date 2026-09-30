using UnityEngine;
namespace AreaBattle
{
    // AssetBundleManager.Initialize23815/helper23856 and native Update23843.
    // The composition root assigns the one shared runtime before initializing the native manager.
    public sealed class OutgameLegacyBundleManager:MonoBehaviour
    {
        public static OutgameLegacyBundleRuntime SharedRuntime {get;set;}
        public static GameObject ManagerObject {get;private set;}
        public static void Initialize()
        {
            ManagerObject=new GameObject("AssetBundleManager",typeof(OutgameLegacyBundleManager));
            DontDestroyOnLoad(ManagerObject);
        }
        void Update()=>SharedRuntime.Update();
    }
}
