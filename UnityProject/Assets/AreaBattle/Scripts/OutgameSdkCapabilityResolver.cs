using System;
namespace AreaBattle
{
    // DBTSDKManager.GetState23915; platform values must come from the corresponding recovered managers.
    public sealed class OutgameSdkCapabilityResolver
    {
        readonly Func<string,int> query;
        public OutgameSdkCapabilityResolver(Func<string,int> query){this.query=query;}
        public bool GetState(int function)
        {
            switch(function)
            {
                case 0:return query("AppInfoManager.IsInstallVersion")!=0;
                case 1:return query("AppInfoManager.IsFirstStartVer")!=0;
                case 2:return query("AppInfoManager.IsWifi")!=0;
                case 3:return query("AppInfoManager.IsRootSystem")!=0;
                case 4:return query("AppInfoManager.ShowMoreGameStatic")!=0;
                case 5:return true;
                case 6:return query("AppInfoManager.GetDesignModeStatic")==0;
                case 7:return query("AppUserManager.IsShowShare_Override")!=0;
                case 8:return query("AppUserManager.ShowLogin_Override")!=0;
                case 9:return query("AppUserManager.IsShowFeedback")!=0;
                case 10:return query("AppUserManager.IsRequestLocationInEeaOrUnknownStatic")!=0;
                case 11:case 12:return query("SDKToolManager.IsShowPolicy")!=0;
                case 13:return query("SDKToolManager.IsShowRealNameRegistration")!=0;
                case 14:return query("SDKToolManager.IsShowContactInformation")!=0;
                case 15:return query("AdsManager.isVideoReady")!=0;
                case 16:return query("AppUserManager.IsShowEvaluate")!=0;
                case 17:return query("SDKToolManager.GetGameBanHaoType")==0;
                case 18:return query("IapManager.IsSupportPayStatic")!=0;
                case 19:return query("IapManager.IsNeedResotreStatic")!=0;
                case 20:return query("SDKToolManager.CanShowOppoGameCenterStatic")!=0;
                case 21:return query("AdsManager.GetDrawVideoButtonStatusStatic")==1;
                case 22:return query("AppInfoManager.ShowPrivacyRecallStatic")!=0;
                default:return false;
            }
        }
    }
}
