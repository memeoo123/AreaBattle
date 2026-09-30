using System;
namespace AreaBattle
{
    // Manager boundary: implementations must forward to their recovered platform owner.
    public interface IOutgameSdkActionManagers
    {
        void LoginComStatic(bool show);void ShowToast(string text);
        void ShowFeedback();
        void ShowGDPRDialogStatic();
        void GotoPrivacyPolicyStatic();
        void GotoTermsServiceStatic();
        void ShowGameBanHao();
        void StartRestoreStatic();
        void OpenOppoGameCenterStatic();
        void ShowDrawVideoStatic();
        void OpenPrivacyRecallActStatic();
    }
    // DBTSDKManager23916-23927. Source Share action is intentionally empty.
    public sealed class OutgameSdkFunctionActions:IOutgameSdkFunctionActions
    {
        readonly IOutgameSdkActionManagers managers;
        readonly Func<bool> closeAdsTips;
        readonly Func<int,bool,bool> isOpen;
        readonly Func<string,string> language;
        public OutgameSdkFunctionActions(IOutgameSdkActionManagers managers,Func<bool> closeAdsTips,Func<int,bool,bool> isOpen,Func<string,string> language)
        {this.managers=managers;this.closeAdsTips=closeAdsTips;this.isOpen=isOpen;this.language=language;}
        public void AdsVideoFunction()
        {if(!closeAdsTips()&&!isOpen(15,false))managers.ShowToast(language("Sdk_NoAdsTips"));}
        public void ShowShareFunction(){}
        public void ShowLoginFunction()=>managers.LoginComStatic(true);
        public void FeedbackFunction()=>managers.ShowFeedback();
        public void GDPRUserFunction()=>managers.ShowGDPRDialogStatic();
        public void ShowPolicyFunction()=>managers.GotoPrivacyPolicyStatic();
        public void ShowUserProtocolFunction()=>managers.GotoTermsServiceStatic();
        public void ShowGameBanHaoFunction()=>managers.ShowGameBanHao();
        public void StartRestore()=>managers.StartRestoreStatic();
        public void ShowOppoGameCenter()=>managers.OpenOppoGameCenterStatic();
        public void DrawVideo()=>managers.ShowDrawVideoStatic();
        public void OpenPrivacyRecall()=>managers.OpenPrivacyRecallActStatic();
    }
}
