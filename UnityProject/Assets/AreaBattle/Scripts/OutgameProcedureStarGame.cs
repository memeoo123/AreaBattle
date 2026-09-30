using System;
using System.Collections.Generic;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public sealed class OutgameProcedureStarGame:IOutgameFsmState<IOutgameProcedureManager>
    {
        readonly Func<IEnumerable<WXAMSConfig>> ads;
        readonly Action<Dictionary<string,int>> setVideoMapping;
        readonly OutgameLegacyBundleRuntime resources;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Action<string,int> registerDebug;
        readonly Action<string> log;
        readonly Action startGameModel,checkNewDay,touchReport,clearStateEvents;
        readonly OutgameStartupEntry entry;
        readonly Type exitState;
        OutgameFsm<IOutgameProcedureManager> owner;
        public float Elapsed {get;private set;}
        public OutgameProcedureStarGame(Func<IEnumerable<WXAMSConfig>> ads,Action<Dictionary<string,int>> setVideoMapping,OutgameLegacyBundleRuntime resources,Func<OutgameMessageDispatcher> messages,Action<string,int> registerDebug,Action<string> log,Action startGameModel,Action checkNewDay,Action touchReport,Action clearStateEvents,OutgameStartupEntry entry,Type exitState)
        {this.ads=ads;this.setVideoMapping=setVideoMapping;this.resources=resources;this.messages=messages;this.registerDebug=registerDebug;this.log=log;this.startGameModel=startGameModel;this.checkNewDay=checkNewDay;this.touchReport=touchReport;this.clearStateEvents=clearStateEvents;this.entry=entry;this.exitState=exitState;}
        public void OnInit(OutgameFsm<IOutgameProcedureManager> fsm){owner=fsm;}
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm)
        {
            log("Enter 'Proj_hdzd.Procedure.ProcedureStarGame' procedure.");
            var mapping=new Dictionary<string,int>();foreach(var row in ads())mapping.Add(row.ReportLable,row.PlacementId);
            setVideoMapping(mapping);resources.DisableUnload=true;
            messages().AddListener("ReadyExitGame",ReadyExitGame);
            registerDebug("Add Item - 添加道具id count",34155);
            registerDebug("JumpLevel - 跳到对应关卡",34151);
            registerDebug("Add Gold - 增加n金币",34139);
            registerDebug("Add Dimon - 增加n钻石",34152);
            registerDebug("Win - 一键胜利",34147);
            registerDebug("Fail - 一键失败",34157);
            registerDebug("Unlock All Soldier Skin - 解锁小兵全皮肤",34159);
            registerDebug("Unlock All Scene Skin - 解锁场景全皮肤",34137);
            registerDebug("SetPigBankDemon - 设置存钱罐钻石数量",30171);
            registerDebug("SetPigBankFull - 设置存钱罐为可领取",34153);
            registerDebug("Set Arena Cup调整玩家奖杯",34140);
            registerDebug("AddDice - 添加骰子",34156);
            registerDebug("AddDicePoint - 添加冒险骰子积分",34143);
            registerDebug("设置移动步数(1-6设置, 0关闭)",34138);
            registerDebug("设置TimeScale",34148);
            registerDebug("Unlock All Headbox - 解锁全部头像框",34146);
            registerDebug("Add LevelRank - 打榜",34150);
            registerDebug("Add LevelRank Bot - 添加排行榜机器人",34145);
            messages().AddListener("GamePause",GamePause);
            startGameModel();checkNewDay();entry.Enter();
        }
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm,object[] args){}
        public void OnLeave(OutgameFsm<IOutgameProcedureManager> fsm,bool shutdown)
        {log("Leave 'Proj_hdzd.Procedure.ProcedureStarGame' procedure.");messages().RemoveListener("GamePause",GamePause);messages().RemoveListener("ReadyExitGame",ReadyExitGame);}
        public void OnDestroy(OutgameFsm<IOutgameProcedureManager> fsm){clearStateEvents();}
        public void OnUpdate(OutgameFsm<IOutgameProcedureManager> fsm,float dt,float udt){Accumulate(dt,false);}
        void ReadyExitGame(object[] args){messages().SendMessage("MineGameExitLogic");owner.ChangeState(exitState,Array.Empty<object>());}
        void GamePause(object[] args){if(args!=null&&args.Length>0&&(bool)args[0])Accumulate(0,true);}
        void Accumulate(float dt,bool flush)
        {
            Elapsed+=dt;
            if(flush){int whole=Math.Abs((double)Elapsed)<2147483648d?(int)Elapsed:int.MinValue;if(whole==0)return;touchReport();Elapsed-=whole;}
            else if(Elapsed>=30f){touchReport();Elapsed-=30f;}
        }
    }
}
