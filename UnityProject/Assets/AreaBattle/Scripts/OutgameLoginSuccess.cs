using System;
namespace AreaBattle
{
    public interface IOutgameLoginSuccessHost
    {
        bool LoginSuccess {get;}
        void EnterBase();
        void LeaveBase(bool shutdown);
        void Subscribe(int eventId,Action callback);
        void Unsubscribe(int eventId,Action callback);
        void ChangeToSdkXyx();
        void ChangeToIdle();
        void LoginComplete(bool success,string message);
    }
    // LoginSuccess.OnEnter f18744 and OnLeave wasmcode1 f57694.
    public sealed class OutgameLoginSuccess
    {
        readonly IOutgameLoginSuccessHost host;
        public OutgameLoginSuccess(IOutgameLoginSuccessHost host){this.host=host??throw new ArgumentNullException(nameof(host));}
        public void Enter()
        {
            host.EnterBase();
            host.Subscribe(3,OnSdk);host.Subscribe(5,OnIdleFive);host.Subscribe(7,OnIdleSeven);
            if(host.LoginSuccess)host.LoginComplete(true,string.Empty);
        }
        public void Leave(bool shutdown)
        {
            host.Unsubscribe(3,OnSdk);host.Unsubscribe(5,OnIdleFive);
            // Source leaves event7 to the surrounding FSM's own event lifetime.
            host.LeaveBase(shutdown);
        }
        void OnSdk()=>host.ChangeToSdkXyx();
        void OnIdleFive()=>host.ChangeToIdle();
        void OnIdleSeven()=>host.ChangeToIdle();
    }
}
