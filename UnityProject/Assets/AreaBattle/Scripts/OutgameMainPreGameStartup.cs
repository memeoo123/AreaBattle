using System;
namespace AreaBattle
{
    // MineGameMain30617,30622,30608. Service factories must supply concrete recovered instances.
    public sealed class OutgameMainPreGameStartup
    {
        static readonly int[] ControllerTypes={4561,4025,4065,4064,3875,4060,4058,4034,4027,4018,4507,3982,4009,4005,3904,3903,4080,4093,4097,4107,4117,4118,4165,4228,4134,4239,4240,4256,4296,4453,4462,4504,4470,3874,4076,4502,4038,4047};
        readonly OutgameMainLifecycleState state;
        readonly OutgamePreGameSettings settings;
        readonly Func<OutgameProcedureManager> procedures;
        readonly Func<OutgameLogicModule> logic;
        readonly Func<int,IOutgameLogicControl> control;
        readonly Func<OutgameProcedurePreLoad> preload;
        readonly Func<OutgameProcedureStarGame> star;
        readonly Func<OutgameProcedureExitGame> exit;
        readonly Action audio,langFont,commonSettings,startMineGame;
        readonly Action<string> log;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly OutgameMainExit cleanup;
        public OutgameMainPreGameStartup(OutgameMainLifecycleState state,OutgamePreGameSettings settings,Func<OutgameProcedureManager> procedures,Func<OutgameLogicModule> logic,Func<int,IOutgameLogicControl> control,Func<OutgameProcedurePreLoad> preload,Func<OutgameProcedureStarGame> star,Func<OutgameProcedureExitGame> exit,Action audio,Action langFont,Action commonSettings,Action startMineGame,Action<string> log,Func<OutgameMessageDispatcher> messages,OutgameMainExit cleanup)
        {this.state=state;this.settings=settings;this.procedures=procedures;this.logic=logic;this.control=control;this.preload=preload;this.star=star;this.exit=exit;this.audio=audio;this.langFont=langFont;this.commonSettings=commonSettings;this.startMineGame=startMineGame;this.log=log;this.messages=messages;this.cleanup=cleanup;}
        public void Begin()
        {
            log("开始初始化进游戏前的逻辑");settings.Initialize();
            var initial=new IOutgameFsmState<IOutgameProcedureManager>[]{preload(),star(),exit()};
            procedures().Register(initial);
            foreach(int type in ControllerTypes)logic().RegisterLogicCtr(control(type),false);
            audio();logic().InitCtrl(true);
            state.ResourcesOnly=false;state.ModulesReady=true;
            langFont();log("开始游戏");commonSettings();startMineGame();
            messages().AddListener("ExitGame",cleanup.OnExitGame);
            procedures().StartProcedure<OutgameProcedurePreLoad>();
        }
    }
}
