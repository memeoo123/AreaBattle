using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameLevelBoss:IOutgameLevelTower {}
    public interface IOutgameLevelArrowTower:IOutgameLevelTower {}
    public interface IOutgameLevelSoldier
    {
        bool Active {get;}bool SourceFlag112 {get;}
        OutgameEffectCollection Effects {get;}
        void Clear(bool sourceOption);
    }
    public sealed partial class OutgameLevelControl
    {
        public bool HaveCheckedGuide,HaveCheckedSerialActivity,HaveCheckedFailReward;
        public readonly List<IOutgameLevelSoldier> Soldiers=new List<IOutgameLevelSoldier>();
        public readonly List<IOutgameLevelTower> ScratchTowers76=new List<IOutgameLevelTower>(),ScratchTowers80=new List<IOutgameLevelTower>(),ScratchTowers112=new List<IOutgameLevelTower>();
        public void InitGameData(Action<int> setWayLineSource168) //31515, ordered partial mutations.
        {
            CurrentBoss=null;
            foreach(var obstacle in ObstacleObjects)obstacle.SetActive(false);
            ObstacleObjects.Clear();HaveCheckedFailReward=false;HaveCheckedGuide=false;HaveCheckedSerialActivity=false;
            foreach(var camp in Camps)if(camp.IsActive)camp.Clear();
            foreach(var tower in Towers)if(tower.Active)tower.Clear();
            foreach(var soldier in Soldiers){soldier.Clear(true);soldier.Effects.Destory();}
            setWayLineSource168(0);
        }
        public IOutgameLevelSoldier GetEmptySoldier(Func<IOutgameLevelSoldier> create) //31472
        {
            foreach(var soldier in Soldiers)if(!soldier.Active&&soldier.SourceFlag112)return soldier;
            var added=create();Soldiers.Add(added);return added;
        }
        public IOutgameLevelBoss GetEmptyBoss(Func<IOutgameLevelBoss> create) //31478
        {
            foreach(var tower in Towers)if(!tower.Active&&tower is IOutgameLevelBoss boss)return boss;
            var added=create();Towers.Add(added);return added;
        }
        public IOutgameLevelTower GetEmptyTower(int shipType,Func<IOutgameLevelTower> createNormal,Func<IOutgameLevelArrowTower> createArrow) //31531
        {
            foreach(var tower in Towers)
            {
                if(tower.Active||tower is IOutgameLevelBoss)continue;
                if(shipType==4){if(tower is IOutgameLevelArrowTower)return tower;}
                else if(!(tower is IOutgameLevelArrowTower))return tower;
            }
            IOutgameLevelTower added=shipType==4?createArrow():createNormal();Towers.Add(added);return added;
        }
    }
}
