using System;
namespace AreaBattle
{
    // Host supplies original inherited FSM/error handling and external SDK/data services.
    // No success is inferred from loading a local profile.
    public interface IOutgameLoginSyncHost
    {
        event Action SynDataOver;
        event Action DataUploaded;
        string LoginUserId {get;set;}
        long Uid {get;}
        bool IsNewDevice {get;}
        bool IsNewPlayer {get;}
        float Timer {get;set;}
        bool LoginSuccess {get;set;}
        void EnterBase();
        void UpdateBase(float elapsed,float realElapsed);
        void LeaveBase(bool shutdown);
        void UserLogin(long uid,bool newDevice,bool newPlayer);
        void ReportSyncSuccess();
        void RequestDataUploaded();
        void ChangeToLoginSuccess();
        void RefreshUserDataPath();
        void CloseSyncData();
        void HandleFailure(int code,string message);
    }
    // LoginSynData: f18743/f18741/f18742/f18740/f18739/f7596.
    public sealed class OutgameLoginSync
    {
        readonly IOutgameLoginSyncHost host;
        string previousUserId;
        public OutgameLoginSync(IOutgameLoginSyncHost host){this.host=host??throw new ArgumentNullException(nameof(host));}
        public void Enter()
        {
            host.EnterBase();
            host.SynDataOver+=OnSyncOver;host.DataUploaded+=OnUploaded;
            previousUserId=host.LoginUserId;
            host.UserLogin(host.Uid,host.IsNewDevice,host.IsNewPlayer);
        }
        public void Update(float elapsed,float realElapsed)
        {
            host.UpdateBase(elapsed,realElapsed);
            host.Timer=(float)(host.Timer+elapsed);
            if(host.Timer>=30f)
            {
                host.LoginUserId=previousUserId;host.RefreshUserDataPath();host.CloseSyncData();
                host.HandleFailure(4,"同步数据超时");
            }
        }
        public void Leave(bool shutdown)
        {
            host.LeaveBase(shutdown);
            host.SynDataOver-=OnSyncOver;host.DataUploaded-=OnUploaded;
        }
        void OnSyncOver()
        {
            host.ReportSyncSuccess();
            if(host.IsNewPlayer){host.RequestDataUploaded();return;}
            OnUploaded();
        }
        void OnUploaded(){host.LoginSuccess=true;host.ChangeToLoginSuccess();}
    }
}
