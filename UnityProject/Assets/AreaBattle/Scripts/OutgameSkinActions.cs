using System;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameSkinActions
    {
        [Serializable] sealed class Statistics {public int HaveSkinNum;}
        readonly OutgameSkinCatalog catalog;
        readonly Action<int,long> setStatistic;
        readonly Statistics statistics;
        public OutgameSkinActions(OutgameSkinCatalog catalog,Action<int,long> setStatistic,string statisticsJson)
        {
            this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));
            this.setStatistic=setStatistic??throw new ArgumentNullException(nameof(setStatistic));
            statistics=JsonUtility.FromJson<Statistics>(statisticsJson);
        }
        public void UnlockSoldier(int id,bool isNew)
        {
            var skin=catalog.SoldierSkin(id);if(skin==null||skin.u)return;
            skin.u=true;skin.isNew=isNew;
            setStatistic(statistics.HaveSkinNum,catalog.UnlockedSoldierCount());
        }
        public void UnlockScene(int id,bool isNew)
        {
            var skin=catalog.SceneSkin(id);if(skin==null||skin.u)return;
            skin.u=true;skin.isNew=isNew;
        }
    }
}
