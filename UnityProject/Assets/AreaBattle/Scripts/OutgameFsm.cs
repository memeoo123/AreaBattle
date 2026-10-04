using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public sealed class OutgameFrameworkException:Exception
    {public OutgameFrameworkException(string message):base(message){}}
    public interface IOutgameFsmState<T> where T:class
    {
        void OnInit(OutgameFsm<T> fsm);
        void OnEnter(OutgameFsm<T> fsm);
        void OnEnter(OutgameFsm<T> fsm,object[] args);
        void OnUpdate(OutgameFsm<T> fsm,float deltaTime,float unscaledDeltaTime);
        void OnLeave(OutgameFsm<T> fsm,bool shutdown);
        void OnDestroy(OutgameFsm<T> fsm);
    }
    public abstract class OutgameFsmBase
    {
        public string Name {get;protected set;}
        public string FullName {get;internal set;}
        public abstract bool IsDestroyed {get;}
        public abstract void Update(float deltaTime,float unscaledDeltaTime);
        public abstract void Shutdown();
        internal static string QualifiedName(Type owner,string name)=>string.IsNullOrEmpty(name)?owner.FullName:owner.FullName+"."+name;
    }
    // Source Fsm<T>29257..29270 lifecycle and current-state event dispatch.
    public sealed class OutgameFsm<T>:OutgameFsmBase where T:class
    {
        readonly Dictionary<string,IOutgameFsmState<T>> states=new Dictionary<string,IOutgameFsmState<T>>();
        public T Owner {get;}
        public IOutgameFsmState<T> CurrentState {get;private set;}
        public float CurrentStateTime {get;private set;}
        public bool IsRunning=>CurrentState!=null;
        bool destroyed;
        public override bool IsDestroyed=>destroyed;
        public OutgameFsm(string name,T owner,IOutgameFsmState<T>[] initialStates)
        {
            Name=name??string.Empty;
            if(owner==null)throw new OutgameFrameworkException("FSM owner is invalid.");
            if(initialStates==null||initialStates.Length==0)throw new OutgameFrameworkException("FSM states is invalid.");
            Owner=owner;
            foreach(var state in initialStates)
            {
                if(state==null)throw new OutgameFrameworkException("FSM states is invalid.");
                string key=state.GetType().FullName;
                if(states.ContainsKey(key))throw new OutgameFrameworkException(string.Format("FSM '{0}' state '{1}' is already exist.",QualifiedName(typeof(T),name),key));
                states.Add(key,state);state.OnInit(this);
            }
            CurrentState=null;CurrentStateTime=0;destroyed=false;
        }
        public IOutgameFsmState<T> GetState(Type type)=>states.TryGetValue(type.FullName,out var state)?state:null;
        public void Start<TState>(params object[] args) where TState:IOutgameFsmState<T> =>Start(typeof(TState),args);
        public void Start(Type type,params object[] args)
        {
            if(IsRunning)throw new OutgameFrameworkException("FSM is running, can not start again.");
            var next=GetState(type);
            if(next==null)throw new OutgameFrameworkException(string.Format("FSM '{0}' can not start state '{1}' which is not exist.",QualifiedName(typeof(T),Name),type.FullName));
            CurrentState=next;CurrentStateTime=0;Enter(args);
        }
        void Enter(object[] args){if(args!=null&&args.Length>0)CurrentState.OnEnter(this,args);else CurrentState.OnEnter(this);}
        public void ChangeState<TState>(params object[] args) where TState:IOutgameFsmState<T> =>ChangeState(typeof(TState),args);
        public void ChangeState(Type type,params object[] args)
        {
            if(CurrentState==null)throw new OutgameFrameworkException("Current state is invalid.");
            var next=GetState(type);
            if(next==null)throw new OutgameFrameworkException(string.Format("FSM '{0}' can not change state to '{1}' which is not exist.",QualifiedName(typeof(T),Name),type.FullName));
            CurrentState.OnLeave(this,false);CurrentState=next;CurrentStateTime=0;Enter(args);
        }
        public void FireEvent(object sender,int eventId)=>FireEvent(sender,eventId,null);
        public void FireEvent(object sender,int eventId,object userData)
        {
            var state=CurrentState;
            if(state==null)throw new OutgameFrameworkException("Current state is invalid.");
            if(state is IOutgameFsmEventState<T> events)events.OnEvent(this,sender,eventId,userData);
        }
        public override void Update(float deltaTime,float unscaledDeltaTime)
        {if(CurrentState!=null){CurrentStateTime+=deltaTime;CurrentState.OnUpdate(this,deltaTime,unscaledDeltaTime);}}
        public override void Shutdown()
        {
            if(CurrentState!=null){CurrentState.OnLeave(this,true);CurrentState=null;CurrentStateTime=0;}
            foreach(var pair in states)pair.Value.OnDestroy(this);
            states.Clear();destroyed=true;
        }
    }
    // Original FsmManager: name/type registry plus live indexed update list.
    public sealed class OutgameFsmManager:IOutgameFrameModule
    {
        readonly Dictionary<string,OutgameFsmBase> fsms=new Dictionary<string,OutgameFsmBase>();
        readonly List<OutgameFsmBase> updateList=new List<OutgameFsmBase>();
        public int Priority=>80;
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public int Count=>fsms.Count;
        public void Initialize(){IsInitialized=true;Initialized?.Invoke();}
        void IOutgameFrameModule.Initialize(object[] args)=>Initialize();
        public void Start(){}
        public bool HasFsm<T>(string name) where T:class=>fsms.ContainsKey(OutgameFsmBase.QualifiedName(typeof(T),name));
        public OutgameFsm<T> CreateFsm<T>(T owner,params IOutgameFsmState<T>[] states) where T:class=>CreateFsm(string.Empty,owner,states);
        public OutgameFsm<T> CreateFsm<T>(string name,T owner,params IOutgameFsmState<T>[] states) where T:class
        {
            string key=OutgameFsmBase.QualifiedName(typeof(T),name);
            if(HasFsm<T>(name))throw new OutgameFrameworkException(string.Format("Already exist FSM '{0}'.",key));
            var fsm=new OutgameFsm<T>(name,owner,states);fsm.FullName=key;fsms.Add(key,fsm);updateList.Add(fsm);return fsm;
        }
        public bool DestroyFsm<T>(OutgameFsm<T> fsm) where T:class
        {if(fsm==null)throw new OutgameFrameworkException("FSM is invalid.");return DestroyFsm<T>(fsm.Name);}
        public bool DestroyFsm<T>(string name) where T:class
        {
            string key=OutgameFsmBase.QualifiedName(typeof(T),name);
            if(!fsms.TryGetValue(key,out var fsm))return false;
            fsm.Shutdown();fsms.Remove(key);updateList.Remove(fsm);return true;
        }
        public void Update(float dt,float udt){for(int i=0;i<updateList.Count;i++){var fsm=updateList[i];if(!fsm.IsDestroyed)fsm.Update(dt,udt);}}
        public void Shutdown(){foreach(var pair in fsms)pair.Value.Shutdown();fsms.Clear();updateList.Clear();IsInitialized=false;}
    }
}
