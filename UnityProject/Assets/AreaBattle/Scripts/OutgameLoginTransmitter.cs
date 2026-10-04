using System;
using System.Collections.Generic;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameRequestSendGameData {public Dictionary<string,string> Datas=new Dictionary<string,string>();}
    [Serializable] public sealed class OutgameRequestList {public string[] keys;}
    [Serializable] public sealed class OutgameCutoverLoginData {public string userId,guestUserId;public long guestUid;}
    [Serializable] public sealed class OutgameUploadUserExtData {public string userExt;}
    public sealed class OutgameLoginTransmitter:OutgameBaseHttpNetTransmitter
    {
        readonly IReadOnlyDictionary<int,Action<string>> responseActions;
        readonly Func<OutgameDataManagerPool> dataManagers;
        Dictionary<string,string> pendingData=new Dictionary<string,string>();
        bool uploadPending;int remainingUpdates=3;
        public bool UploadPending=>uploadPending;
        public int RemainingUpdates=>remainingUpdates;
        public IReadOnlyDictionary<string,string> PendingData=>pendingData;
        // HttpNetAcion and DataManagerPool are explicit composition dependencies.
        public OutgameLoginTransmitter(OutgameHttpTransmitterServices services,IReadOnlyDictionary<int,Action<string>> responseActions,Func<OutgameDataManagerPool> dataManagers):base(services)
        {this.responseActions=responseActions;this.dataManagers=dataManagers;}
        public override void InitProtocol()
        {
            string[] paths={"/sys/nowTime","/auth/login","/data/private/update","/data/private/getall","/data/private/get","/data/private/delete","/data/visit/get","/data/public/update","/data/public/get","/data/public/delete","/data/private/version","/data/private/exists","/data/bind","/data/unreg","/data/user/ext","/auth/unsafe/bind","/data/user/uploaded","/data/attachment/get"};
            var urls=new string[paths.Length];for(int i=0;i<paths.Length;i++)urls[i]=string.Format("{0}{1}",BaseUrl,paths[i]);
            for(int i=0;i<urls.Length;i++)AddHttpProtocol(i+2,urls[i],responseActions[i+2]);
        }
        //29229 overwrites repeated keys without extending the existing countdown.
        public void QueuePlayerData(string key,string value)
        {
            if(pendingData.ContainsKey(key))pendingData[key]=value;else pendingData.Add(key,value);
            if(!uploadPending){remainingUpdates=3;uploadPending=true;}
        }
        public int UploadPlayerData(Dictionary<string,string> data)=>Send(4,new OutgameRequestSendGameData{Datas=data});
        public void RequestServerTime(){Send(2,null);}
        public void RequestLogin(object request){Send(3,request);}
        public void RequestDataUploaded(){Send(18,null);}
        public void UploadUserExtNet(string text){Send(16,new OutgameUploadUserExtData{userExt=text});}
        public void RequestCutoverLoginData(string userId,string guestUserId,long guestUid)
        {Send(14,new OutgameCutoverLoginData{userId=userId,guestUserId=guestUserId,guestUid=guestUid});}
        public void RequestDataVersions(string[] keys){Send(12,new OutgameRequestList{keys=keys});}
        public void RequestPlayerData(string[] keys){Send(6,new OutgameRequestList{keys=keys});}
        //29239 enumerates the live pool, includes only managers with AutoSyn, keeps duplicates.
        public void RequestDataVersions()
        {
            var keys=new List<string>();foreach(var manager in dataManagers().Managers.Values)if(manager.ParticipatesInSync)keys.Add(manager.DataKey);
            RequestDataVersions(keys.ToArray());
        }
        public override void Update()
        {
            if(!uploadPending)return;
            if(remainingUpdates<=0){uploadPending=false;UploadPlayerData(pendingData);pendingData=new Dictionary<string,string>();}
            else remainingUpdates--;
        }
    }
}
