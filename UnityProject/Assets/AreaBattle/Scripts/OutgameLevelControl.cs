using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Shared original LevelControl field8; dispatcher and lifecycle must see the same value.
    public sealed class OutgameLevelRuntimeState {public int PlayState;}
    public interface IOutgameLevelTower
    {
        bool Active {get;}int CampId {get;}float Score {get;}
        GameObject Entity {get;}Vector3 SourcePosition68 {get;}
        void Init(int id,StarInfoCfg config);
        void Clear();
        void Updata(float deltaTime,float unscaledDeltaTime);
    }
    public sealed class OutgameCampInfo
    {
        public bool IsActive;public int Index,CampId,TowerCount,Score;public Color Color;
        public void Init(int index,int camp,Color color){CampId=camp;Index=index;IsActive=true;Color=color;TowerCount=0;Score=0;}
        public void Clear(){TowerCount=0;Score=0;Index=-1;CampId=-1;IsActive=false;}
    }
    // Level4107 lifecycle/collections/refresh and initialization loops. Tower implementation,
    // state branch services and complete post-prepare host must be supplied by composition.
    public sealed partial class OutgameLevelControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;readonly Func<int> globalPlayerCamp,globalCampCount;
        readonly Func<OutgameLegacyConfigManager> config;readonly Func<bool> pvpActive;
        readonly Func<OutgameMessageDispatcher> messages;readonly Action<int,bool> setPlayState;
        readonly Func<OutgameLoadPrefabControl> loader;readonly Action<string> log;
        public readonly OutgameLevelRuntimeState State;public readonly OutgameLevelResourceState Resources;
        public int PlayerCampId,TotalTowerCount,TotalScore;public bool NeedsCampRefresh,HasBoss,ObstaclesInitialized;
        public float CampRefreshRemaining,SourceSpeed100,SourceSpeed104=1f;
        public readonly List<IOutgameLevelTower> Towers=new List<IOutgameLevelTower>();
        public readonly List<OutgameCampInfo> Camps=new List<OutgameCampInfo>();
        public readonly List<GameObject> ObstacleObjects=new List<GameObject>();
        public IOutgameLevelTower CurrentBoss;
        public OutgameLevelControl(OutgameControllerRegistry registry,OutgameLevelRuntimeState state,OutgameLevelResourceState resources,Func<int> globalPlayerCamp,Func<int> globalCampCount,Func<OutgameLegacyConfigManager> config,Func<bool> pvpActive,Func<OutgameMessageDispatcher> messages,Action<int,bool> setPlayState,Func<OutgameLoadPrefabControl> loader,Action<string> log)
        {this.registry=registry;State=state;Resources=resources;this.globalPlayerCamp=globalPlayerCamp;this.globalCampCount=globalCampCount;this.config=config;this.pvpActive=pvpActive;this.messages=messages;this.setPlayState=setPlayState;this.loader=loader;this.log=log;}
        public void OnInit(){PlayerCampId=globalPlayerCamp();Resources.InitializeMaximum();}
        public void OnDispose(){registry.Clear(4107);State.PlayState=0;}
        public void Updata(float deltaTime,float unscaledDeltaTime)
        {
            if(State.PlayState!=6)return;
            foreach(var tower in Towers)if(tower.Active)tower.Updata(deltaTime*config().Globals.GameTimeScale,unscaledDeltaTime);
            if(NeedsCampRefresh){CampRefreshRemaining-=deltaTime;if(CampRefreshRemaining<=0){RefreshCampInfo();CampRefreshRemaining+=1f;}}
        }
        public OutgameCampInfo GetCampInfoByCampId(int camp)
        {foreach(var info in Camps)if(info.IsActive&&info.CampId==camp)return info;return null;}
        public OutgameCampInfo AcquireCampInfo()
        {foreach(var info in Camps)if(!info.IsActive)return info;var added=new OutgameCampInfo();Camps.Add(added);return added;}
        public Color GetCampColor(int camp)=>config().LineColors.TryGetValue(camp,out var color)?color:Color.green;
        static int SourceInt(float score)=>Math.Abs((double)score)<2147483648d?(int)score:int.MinValue;
        public void RefreshCampInfo()
        {
            int next=0;
            foreach(var info in Camps)if(info.IsActive){info.TowerCount=0;info.Score=0;if(info.CampId!=0)next=Math.Max(next,info.Index);}
            next=unchecked(next+1);TotalScore=0;
            foreach(var tower in Towers)
            {
                if(!tower.Active)continue;
                var info=GetCampInfoByCampId(tower.CampId);
                if(info==null)
                {
                    int index=0;
                    if(tower.CampId!=PlayerCampId){if(tower.CampId==0)index=globalCampCount();else{index=next;next=unchecked(next+1);}}
                    info=AcquireCampInfo();info.Init(index,tower.CampId,GetCampColor(tower.CampId));
                }
                info.TowerCount=unchecked(info.TowerCount+1);info.Score=unchecked(info.Score+SourceInt(tower.Score));
                if(tower.CampId!=0)TotalScore=unchecked(TotalScore+SourceInt(tower.Score));
            }
            NeedsCampRefresh=false;messages().SendMessage("CampChange",null);
            if(State.PlayState==2||pvpActive())return;
            var mine=GetCampInfoByCampId(PlayerCampId);if(mine==null)return;
            if(mine.TowerCount==0){setPlayState(9,false);return;}
            if(mine.TowerCount>=TotalTowerCount){setPlayState(8,false);return;}
            if(CurrentBoss!=null&&SourceInt(CurrentBoss.Score)<=0)setPlayState(8,false);
        }
        public void InitializeTowers(Func<int,IOutgameLevelTower> normal,Func<IOutgameLevelTower> boss)
        {
            TotalTowerCount=Resources.CurrentLevel.StarInfoCfgs.Length;
            for(int i=0;i<TotalTowerCount;i++)
            {
                var source=Resources.CurrentLevel.StarInfoCfgs[i];
                if(!source.isBoss)normal(source.ShipID).Init(i+1,source);
                else{CurrentBoss=boss();CurrentBoss.Init(i+1,source);HasBoss=true;}
            }
        }
        public void InitializeObstacles()
        {
            ObstaclesInitialized=false;
            if(Resources.CurrentLevel.ObstacleInfoCfgs==null||Resources.CurrentLevel.ObstacleInfoCfgs.Length==0){ObstaclesInitialized=true;return;}
            var obstacles=Resources.CurrentLevel.ObstacleInfoCfgs;
            for(int i=0;i<obstacles.Length;i++)
            {
                var source=obstacles[i];var go=loader().GetEntityNow(source.EnityID);
                if(go){go.transform.position=source.pos.WorldPosition;go.transform.eulerAngles=source.angle.WorldPosition;go.transform.localScale=source.scale.WorldPosition;ObstacleObjects.Add(go);}
            }
            ObstaclesInitialized=true;log("Play: 障碍物初始完毕");
        }
    }
}
