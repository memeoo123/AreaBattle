using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgamePreLoadHost
    {
        void LogProcedure(string message);
        void InitializeUserNetModule();
        int ArenaRankEvent {get;}
        void RegisterStatisticValue(int key,Func<object[],long> provider);
        long EventCount(int key);
        void InitializeStatistics(Action complete);
        // Source ActivityControl.Init(null); original ActivityInitData is not substituted.
        void InitializeActivityWithNullData();
        bool LoginSourceFlag64 {get;}
        bool LoginSourceFlag65 {get;}
        void LogWarning(string message);
        void Handle103VersionBug();
        void ClearStateEvents();
    }
    // Source ProcedurePreLoad34119..34128, async MoveNext34132 and predicate34131.
    public sealed class OutgameProcedurePreLoad:IOutgameFsmState<IOutgameProcedureManager>
    {
        readonly IOutgamePreLoadHost host;
        readonly Type starGameState;
        readonly Func<WaitUntil,Task> wait;
        OutgameFsm<IOutgameProcedureManager> owner;
        const string SourceName="Proj_hdzd.Procedure.ProcedurePreLoad";
        public OutgameProcedurePreLoad(IOutgamePreLoadHost host,Type starGameState,Func<WaitUntil,Task> wait=null)
        {this.host=host;this.starGameState=starGameState;this.wait=wait??WaitNative;}
        static async Task WaitNative(WaitUntil instruction){await OutgameUnityAwait.Await(instruction);}
        public void OnInit(OutgameFsm<IOutgameProcedureManager> fsm){owner=fsm;}
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm)
        {
            host.LogProcedure("Enter '"+SourceName+"' procedure.");
            host.InitializeUserNetModule();
            host.RegisterStatisticValue(host.ArenaRankEvent,ignored=>host.EventCount(host.ArenaRankEvent));
            host.InitializeStatistics(StatisticsComplete);
        }
        // The inherited FsmState parameterized entry is empty; source overrides only no-args entry.
        public void OnEnter(OutgameFsm<IOutgameProcedureManager> fsm,object[] args){}
        async void StatisticsComplete(){await ContinueAfterStatisticsAsync();}
        public async Task ContinueAfterStatisticsAsync()
        {
            host.InitializeActivityWithNullData();
            await wait(new WaitUntil(()=>host.LoginSourceFlag64&&host.LoginSourceFlag65));
            host.LogWarning("LoginComplete");host.Handle103VersionBug();
            owner.ChangeState(starGameState,Array.Empty<object>());
        }
        public void OnUpdate(OutgameFsm<IOutgameProcedureManager> fsm,float dt,float udt){}
        public void OnLeave(OutgameFsm<IOutgameProcedureManager> fsm,bool shutdown){host.LogProcedure("Leave '"+SourceName+"' procedure.");}
        public void OnDestroy(OutgameFsm<IOutgameProcedureManager> fsm){host.ClearStateEvents();}
    }
}
