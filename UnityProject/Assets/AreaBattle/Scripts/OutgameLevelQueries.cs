using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed partial class OutgameLevelControl
    {
        public float LevelSpeed=>SourceSpeed104;
        public void SetLevelSpeed(float value){SourceSpeed104=value;Time.timeScale=value;} //31498
        public void SetScoreSumChangeOn(){NeedsCampRefresh=true;} //31469: timer untouched.
        public List<IOutgameLevelTower> GetCampAllTower(int camp) //31511, shared sourcefield76.
        {
            ScratchTowers76.Clear();
            foreach(var tower in Towers)if(tower.Active&&tower.CampId==camp)ScratchTowers76.Add(tower);
            return ScratchTowers76;
        }
        public List<IOutgameLevelTower> GetCampAllTowerNonArrow(int camp) //31481
        {
            ScratchTowers76.Clear();
            foreach(var tower in Towers)if(!(tower is IOutgameLevelArrowTower)&&tower.Active&&tower.CampId==camp)ScratchTowers76.Add(tower);
            return ScratchTowers76;
        }
        public List<IOutgameLevelTower> GetOtherTower(int camp) //31493, includes neutral, sharedfield80.
        {
            ScratchTowers80.Clear();
            foreach(var tower in Towers)if(tower.Active&&tower.CampId!=camp)ScratchTowers80.Add(tower);
            return ScratchTowers80;
        }
        public List<IOutgameLevelTower> GetEnemyTower(int camp) //31510, excludes neutral.
        {
            ScratchTowers80.Clear();
            foreach(var tower in Towers)if(tower.Active&&tower.CampId!=camp&&tower.CampId!=0)ScratchTowers80.Add(tower);
            return ScratchTowers80;
        }
        public IOutgameLevelTower GetTowerByGameObj(GameObject entity) //31521, native Unity equality.
        {foreach(var tower in Towers)if(tower.Active&&tower.Entity==entity)return tower;return null;}
        public List<IOutgameLevelTower> GetTowerInRange(int camp,bool sameCamp,Vector3 position,float range,float minimum) //31519
        {
            var result=new List<IOutgameLevelTower>();
            // Preserve original asymmetric formula and comparison direction, including NaN behavior.
            minimum=Mathf.Sqrt(minimum);range*=range;
            foreach(var tower in Towers)
            {
                if(!tower.Active||sameCamp!=(tower.CampId==camp))continue;
                var delta=position-tower.SourcePosition68;float squared=delta.x*delta.x+delta.y*delta.y+delta.z*delta.z;
                if(squared>range||minimum>squared)continue;
                result.Add(tower);
            }
            return result;
        }
        public IOutgameLevelTower GetOneTowerInRange(int camp,bool sameCamp,Vector3 position,float range,float minimum,GameRandomSource random=null) //31523/RandomHelper26198
        {
            var found=GetTowerInRange(camp,sameCamp,position,range,minimum);
            return found.Count>=1?found[(random??GameRandomSource.Shared).Managed.Next(found.Count)]:null;
        }
        public GameObject GetBossObj(int entityId,Action<string> error) //31475, separate from managed Boss pool.
        {
            var entity=loader().GetEntityNow(entityId);
            if(entity==null){error("resObj == null  entityID="+entityId);return null;}
            entity.transform.localScale=Vector3.one;return entity;
        }
    }
    // Source31474/31501 share the same progression field24 used by resource resolution.
    public sealed class OutgameLevelSceneStart
    {
        readonly OutgameLevelResourceState resources;readonly OutgameLevelProgression progression;
        readonly Func<OutgameGameControl> game;readonly Func<OutgameFestRewardSelection> festival;readonly Func<OutgameSkinCatalog> skins;
        public OutgameLevelSceneStart(OutgameLevelResourceState resources,OutgameLevelProgression progression,Func<OutgameGameControl> game,Func<OutgameFestRewardSelection> festival,Func<OutgameSkinCatalog> skins)
        {this.resources=resources;this.progression=progression;this.game=game;this.festival=festival;this.skins=skins;}
        public void LoadCurrentLevel(){resources.LoadLevel(progression.CurrentLevel);InitSpecialScene();}
        public void InitSpecialScene()
        {
            int special=progression.SpecialState;var receiver=game();
            int scene=special==1?festival().GetAwardId(5):skins().UsedSkin(4);
            receiver.LoadGameScene(scene);
        }
    }
}
