using UnityEngine;
namespace AreaBattle
{
    // ResourcesMono.Awake29955. Download helper methods are separate recovered services.
    public sealed class OutgameLegacyResourcesMono:MonoBehaviour
    {
        void Awake()=>DontDestroyOnLoad(this);
    }
}
