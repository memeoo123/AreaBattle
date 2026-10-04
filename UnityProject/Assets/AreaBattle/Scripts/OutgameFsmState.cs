using System.Collections.Generic;
namespace AreaBattle
{
    public delegate void OutgameFsmEventHandler<T>(OutgameFsm<T> fsm,object sender,object userData) where T:class;
    public interface IOutgameFsmEventState<T>:IOutgameFsmState<T> where T:class
    {void OnEvent(OutgameFsm<T> fsm,object sender,int eventId,object userData);}
    // Original FsmState<T>29292..29302. Subscriptions belong to the state, not to its current FSM.
    public class OutgameFsmState<T>:IOutgameFsmEventState<T> where T:class
    {
        readonly Dictionary<int,OutgameFsmEventHandler<T>> events=new Dictionary<int,OutgameFsmEventHandler<T>>();
        public virtual void OnInit(OutgameFsm<T> fsm){}
        public virtual void OnEnter(OutgameFsm<T> fsm){}
        public virtual void OnEnter(OutgameFsm<T> fsm,object[] args){}
        public virtual void OnUpdate(OutgameFsm<T> fsm,float deltaTime,float unscaledDeltaTime){}
        public virtual void OnLeave(OutgameFsm<T> fsm,bool shutdown){}
        public virtual void OnDestroy(OutgameFsm<T> fsm){events.Clear();}
        public void SubscribeEvent(int eventId,OutgameFsmEventHandler<T> handler)
        {
            if(handler==null)throw new OutgameFrameworkException("Event handler is invalid.");
            if(!events.ContainsKey(eventId)){events[eventId]=handler;return;}
            events[eventId]+=handler;
        }
        public void UnsubscribeEvent(int eventId,OutgameFsmEventHandler<T> handler)
        {
            if(handler==null)throw new OutgameFrameworkException("Event handler is invalid.");
            if(events.ContainsKey(eventId))events[eventId]-=handler;
        }
        public void OnEvent(OutgameFsm<T> fsm,object sender,int eventId,object userData)
        {if(events.TryGetValue(eventId,out var handler)&&handler!=null)handler(fsm,sender,userData);}
        public void ChangeState<TState>(OutgameFsm<T> fsm,params object[] args) where TState:IOutgameFsmState<T>
        {
            if(fsm==null)throw new OutgameFrameworkException("FSM is invalid.");
            fsm.ChangeState<TState>(args);
        }
    }
}
