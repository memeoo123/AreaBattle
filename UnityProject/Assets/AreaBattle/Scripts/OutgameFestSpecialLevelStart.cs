using System;
using UnityEngine;
namespace AreaBattle
{
    // Controller4504 selected field20, selector34399 and GetAwardId34404.
    public sealed class OutgameFestRewardSelection
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;public int[] itemId,giftWay;}
        readonly Func<string> config;
        public int SelectedAward {get;private set;}=-1;
        public OutgameFestRewardSelection(Func<string> config){this.config=config;}
        Row LimitRow()=>Array.Find(JsonUtility.FromJson<Rows>(config()).Datas,x=>x.id==2);
        public int SelectSpecialLevel(int index)
        {SelectedAward=LimitRow().itemId[index];return LimitRow().giftWay[index];}
        public int GetAwardId(int goodsType)
        {switch(goodsType){case 4:return LimitRow().itemId[0];case 5:return LimitRow().itemId[1];default:return -1;}}
    }
    // FestActUI34020 invokes LevelControl.OnGamePlayState directly, not the global dispatcher.
    public sealed class OutgameFestSpecialLevelStart
    {
        readonly OutgameFestCombatBinding combat;readonly OutgameFestRewardSelection selection;readonly OutgameLevelProgression levels;
        readonly Action<int,int> voice;readonly Action<int,float> moveCamera;readonly Action<int> levelState;readonly Action closeSelf;
        public OutgameFestSpecialLevelStart(OutgameFestCombatBinding combat,OutgameFestRewardSelection selection,OutgameLevelProgression levels,
            Action<int,int> voice,Action<int,float> moveCamera,Action<int> levelState,Action closeSelf)
        {this.combat=combat;this.selection=selection;this.levels=levels;this.voice=voice;this.moveCamera=moveCamera;this.levelState=levelState;this.closeSelf=closeSelf;}
        public void Start(int index)
        {
            combat.ListenCombatWar(true);voice(1,2001);levels.SpecialState=1;
            levels.SpecialLevel=selection.SelectSpecialLevel(index);moveCamera(0,0f);levelState(3);closeSelf();
        }
    }
}
