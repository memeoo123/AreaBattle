using System;
using UnityEngine;
namespace AreaBattle
{
    // Controller4504:34390,34398,34405. Host retains the original combat-owner singleton.
    public sealed class OutgameFestCombatBinding
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;public int[] itemId;}
        readonly OutgameFestActManager manager;
        readonly Func<int> selectedAward;readonly Func<int,int> goodsType;readonly Func<string> config;
        readonly Action claimCombatOwner,clearControllerOwner,removeStatisticListener;
        readonly Action<int,int> voice;readonly Action<int,bool> unlockSoldier,unlockScene;
        readonly Action<int,int,int,int,string> report;
        readonly Func<OutgameMessageDispatcher> messages;
        public OutgameFestCombatBinding(OutgameFestActManager manager,Func<int> selectedAward,Func<int,int> goodsType,Func<string> config,
            Action claimCombatOwner,Action clearControllerOwner,Action removeStatisticListener,Action<int,int> voice,
            Action<int,bool> unlockSoldier,Action<int,bool> unlockScene,Action<int,int,int,int,string> report,
            Func<OutgameMessageDispatcher> messages=null)
        {
            this.manager=manager;this.selectedAward=selectedAward;this.goodsType=goodsType;this.config=config;
            this.claimCombatOwner=claimCombatOwner;this.clearControllerOwner=clearControllerOwner;this.removeStatisticListener=removeStatisticListener;
            this.voice=voice;this.unlockSoldier=unlockSoldier;this.unlockScene=unlockScene;this.report=report;
            this.messages=messages??(()=>OutgameMessageDispatcher.Shared);
        }
        public void OnInit()=>messages().AddListener("GamePlayState",OnGamePlayState);
        public void OnDispose()
        {
            removeStatisticListener();messages().RemoveListener("GamePlayState",OnGamePlayState);clearControllerOwner();
            // Source does not remove WarWin here; state11 or the win callback removes it.
        }
        public void ListenCombatWar(bool enabled)
        {
            if(enabled){claimCombatOwner();ListenCombatWar(false);messages().AddListener("WarWin",OnWarWin);}
            else messages().RemoveListener("WarWin",OnWarWin);
        }
        void OnGamePlayState(object[] args){if((int)args[0]==11)ListenCombatWar(false);}
        int Reward(int index)=>Array.Find(JsonUtility.FromJson<Rows>(config()).Datas,x=>x.id==2).itemId[index];
        void OnWarWin(object[] args)
        {
            ListenCombatWar(false);
            switch(goodsType(selectedAward()))
            {
                case 4:manager.CompleteSpecialLevel1();unlockSoldier(Reward(0),true);break;
                case 5:
                    manager.CompleteSpecialLevel2();int id=Reward(1);voice(1,2017);unlockScene(id,true);
                    report(unchecked(id+1001000),3,1,1,"Activity");break;
            }
        }
    }
}
