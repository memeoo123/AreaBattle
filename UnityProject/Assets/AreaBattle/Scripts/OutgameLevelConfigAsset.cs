using System;
using UnityEngine;
namespace AreaBattle
{
    // Shared projection consumed by preload; ordinary battle keeps its existing serializable layout.
    public interface IOutgameLevelLayout
    {
        StarInfoCfg[] Stars {get;}
        ObstacleInfoCfg[] Obstacles {get;}
    }
    // Original LevelInfoCfg4099 derives ScriptableObject. Fields retain the original JSON names.
    public sealed class OutgameLevelConfigAsset:ScriptableObject,IOutgameLevelLayout
    {
        public CampInfoCfg[] CampInfoCfgs;
        public StarInfoCfg[] StarInfoCfgs;
        public ObstacleInfoCfg[] ObstacleInfoCfgs;
        public StarInfoCfg[] Stars=>StarInfoCfgs;
        public ObstacleInfoCfg[] Obstacles=>ObstacleInfoCfgs;
    }
}
