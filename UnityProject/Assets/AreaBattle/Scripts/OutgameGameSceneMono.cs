using UnityEngine;
namespace AreaBattle
{
    // Original GameSceneMono4219: source serialized references, without preview-only ownership.
    public sealed class OutgameGameSceneMono:MonoBehaviour
    {
        public Camera GameCamera,HomeCamera,UpgradeCamera,CommanderCamera;
        public Transform Scene_home,Scene_game;
        public Renderer Scene_home_CJroot;
        public Transform startui_root,soldierRoot,NormalGroup,DefenseGroup,AttackGroup,objCommanderRoot,objUnlockCommanderRoot,objCameraParent,objCameraCacheParent,objUIModelCacheParent;
    }
}
