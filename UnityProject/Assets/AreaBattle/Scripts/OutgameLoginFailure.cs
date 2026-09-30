using System;
namespace AreaBattle
{
    public interface IOutgameLoginFailureHost
    {
        int SourceLoginMode {get;}
        bool LoginSuccess {get;set;}
        float Timer {get;set;}
        void SendLoginProgress(int progress);
        void LogFailure(bool switchingAccount,int sourceError);
        void ChangeToLoginSuccess();
        void ChangeToIdle();
        void LoginFail(int error,string message);
    }
    // LoginStateBase error handler: wasmcode1 f3351, virtual LoginFail slot14.
    public sealed class OutgameLoginFailure
    {
        readonly IOutgameLoginFailureHost host;
        public OutgameLoginFailure(IOutgameLoginFailureHost host){this.host=host??throw new ArgumentNullException(nameof(host));}
        public void Handle(int error,string message)
        {
            host.SendLoginProgress(4);
            if(host.SourceLoginMode==0)
            {
                host.LogFailure(true,error);host.LoginSuccess=false;
                host.ChangeToLoginSuccess();host.LoginFail(5,message);
            }
            else
            {
                host.LogFailure(false,error);host.Timer=0;
                host.ChangeToIdle();host.LoginFail(error,message);
            }
        }
    }
}
