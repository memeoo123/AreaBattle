using System;
namespace AreaBattle
{
    // GameDataVersionMgr.UserLogin f12178, before manager reload and version/HTTP work.
    // This describes source routing; it does not perform or claim network synchronization.
    public readonly struct OutgameLoginDataPlan
    {
        public readonly string UserId;
        public readonly bool ChangedUser,ForceDownload,ForceUpload,ReloadManagers;
        OutgameLoginDataPlan(string userId,bool changed,bool download,bool upload)
        {UserId=userId;ChangedUser=changed;ForceDownload=download;ForceUpload=upload;ReloadManagers=!upload&&(download||changed);}
        public static OutgameLoginDataPlan Resolve(string previousUserId,long uid,bool isNewDevice,bool isNewPlayer,int isSynLocalData)
        {
            string userId=uid.ToString();
            bool changed=!string.Equals(userId,previousUserId,StringComparison.Ordinal);
            return new OutgameLoginDataPlan(userId,changed,isNewDevice,isSynLocalData==0&&isNewPlayer);
        }
    }
}
