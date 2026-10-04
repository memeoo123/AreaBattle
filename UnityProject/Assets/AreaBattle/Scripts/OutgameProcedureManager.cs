using System;
namespace AreaBattle
{
    // Source ProcedureManager28658..28664. FSM manager owns ticking.
    public interface IOutgameProcedureManager {}
    public sealed class OutgameProcedureManager:IOutgameFrameModule,IOutgameProcedureManager
    {
        readonly Func<OutgameFsmManager> getFsmManager;
        OutgameFsmManager manager;
        public OutgameFsm<IOutgameProcedureManager> ProcedureFsm {get;private set;}
        public int Priority=>90;
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public OutgameProcedureManager(Func<OutgameFsmManager> getFsmManager){this.getFsmManager=getFsmManager;}
        public void Initialize(){IsInitialized=true;Initialized?.Invoke();}
        public void Register(params IOutgameFsmState<IOutgameProcedureManager>[] procedures)
        {
            manager=getFsmManager();if(manager==null)throw new OutgameFrameworkException("FSM manager is invalid.");
            ProcedureFsm=manager.CreateFsm<IOutgameProcedureManager>(this,procedures);
        }
        public void StartProcedure<T>() where T:IOutgameFsmState<IOutgameProcedureManager>
        {if(ProcedureFsm==null)throw new OutgameFrameworkException("You must initialize procedure first.");ProcedureFsm.Start<T>(Array.Empty<object>());}
        void IOutgameFrameModule.Initialize(object[] args)=>Initialize();
        public void Start(){}
        public void Update(float deltaTime,float unscaledDeltaTime){}
        public void Shutdown()
        {
            if(manager!=null){if(ProcedureFsm!=null){manager.DestroyFsm(ProcedureFsm);ProcedureFsm=null;}manager=null;}
            IsInitialized=false;
        }
    }
}
