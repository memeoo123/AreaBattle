using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameUnityAwaitRunner:MonoBehaviour
    {
        void Awake(){gameObject.hideFlags=(HideFlags)61;DontDestroyOnLoad(gameObject);}
    }
}
