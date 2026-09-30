using System;
namespace AreaBattle
{
    // ProcedureExitGame34113..34118. Teardown is delegated by the original ExitGame message.
    public sealed class OutgameProcedureExitGame:IOutgameFsmState<IOutgameProcedureManager>
    {
        readonly OutgameUserPreferences preferences;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Action<string> log;
        readonly Action clearStateEvents;
        public OutgameFsm<IOutgameProcedureManager> Owner {get;private set;}
        public OutgameProcedureExitGame(OutgameUserPreferences preferences,Func<OutgameMessageDispatcher> messages,Action<string> log,Action clearStateEvents)
        {this.preferences=preferences;this.messages=messages;this.log=log;this.clearStateEvents=clearStateEvents;}
        public void OnInit(OutgameFsm<IOutgameProcedureManager> fsm){Owner=fsm;}
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm)
        {log("Enter 'Proj_hdzd.Procedure.ProcedureExitGame' procedure.");preferences.OnSave();messages().SendMessage("ExitGame");}
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm,object[] args){}
        public void OnUpdate(OutgameFsm<IOutgameProcedureManager> fsm,float dt,float udt){}
        public void OnLeave(OutgameFsm<IOutgameProcedureManager> fsm,bool shutdown){log("Leave 'Proj_hdzd.Procedure.ProcedureExitGame' procedure.");}
        public void OnDestroy(OutgameFsm<IOutgameProcedureManager> fsm){clearStateEvents();}
    }
}
