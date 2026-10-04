using System;
using UnityEngine;
namespace AreaBattle
{
    // MatchPlayerData4127: field names are the original LitJSON wire/storage schema.
    [Serializable] public sealed class OutgameMatchPlayerData:IOutgameMatchPlayerInfo
    {
        public string uid,nick,url;
        public int VN_d,DN_d,VN_w,DN_w,VN_t,DN_t,playNum;
        public bool isGetWechatInfo;
        string IOutgameMatchPlayerInfo.Nickname {get=>nick;set=>nick=value;}
        string IOutgameMatchPlayerInfo.AvatarUrl {get=>url;set=>url=value;}
        bool IOutgameMatchPlayerInfo.HasWechatInfo {get=>isGetWechatInfo;set=>isGetWechatInfo=value;}
    }
    [Serializable] public sealed class OutgameMatchData
    {public OutgameMatchPlayerData rankData=new OutgameMatchPlayerData();}
    // MatchManager4130. Deserialize before assignment, with no empty/null guard.
    public sealed class OutgameMatchManager:IOutgameDataManager
    {
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;
        readonly Action<string> download;readonly Func<string> openId,randomName;readonly Func<int> level;
        public OutgameMatchData Data {get;set;}
        public string DataKey=>"MatchManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameMatchManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,
            Action<string> download,Func<string> openId,Func<int> level,Func<string> randomName)
        {this.storage=storage;this.host=host;this.download=download;this.openId=openId;this.level=level;this.randomName=randomName;}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {if(allowServer&&host.IsUseServer&&ParticipatesInSync){download(DataKey);return;}UpdateDataCallBack(storage.ReadLocalData());}
        public void UpdateDataCallBack(string text)
        {
            Data=LitJson.JsonMapper.ToObject<OutgameMatchData>(text);
            if(Data!=null)return;
            Data=new OutgameMatchData();var player=Data.rankData;player.uid=openId();
            player=Data.rankData;int value=unchecked(level()-3),maximum=unchecked(level()-3);
            player.VN_t=Mathf.Clamp(value,0,maximum);
            player=Data.rankData;player.DN_t=0;player.nick=randomName();Data.rankData.url=string.Empty;
        }
        public void OnSave()=>storage.SaveLocalData(LitJson.JsonMapper.ToJson(Data));
        public void OnRelease(){} //31659 retains the data reference.
    }
    // Network transmitter remains an explicit production dependency. A request does
    // not imply success; only the supplied source response callback changes wins/losses.
    public interface IOutgameMatchRankTransmitter
    {
        Action SendResponseCallback {get;set;}
        void SendRankDataRequest(string uid,string nick,string url,int wins,int losses,int rankType,int weight);
    }
    public sealed class OutgameMatchControl:IOutgameLogicControl
    {
        readonly Func<OutgameDataManagerPool> pool;readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<int> level;readonly Func<string> openId;readonly Action initializeAvatar;
        OutgameMatchManager manager;
        public OutgameMatchManager Manager=>manager??(manager=pool().GetModel<OutgameMatchManager>(4130,"MatchManager"));
        public IOutgameMatchRankTransmitter Transmitter;
        public int Rate=20;
        public bool ActiveUpdate {get;set;}
        // The source constructor's unused rank dictionaries/camp list have no readers
        // in these seven methods; rank list presentation is a separate pending scope.
        public OutgameMatchControl(Func<OutgameDataManagerPool> pool,Func<OutgameMessageDispatcher> messages,
            Func<int> level,Func<string> openId,Action initializeAvatar)
        {this.pool=pool;this.messages=messages;this.level=level;this.openId=openId;this.initializeAvatar=initializeAvatar;}
        public void OnInit(){initializeAvatar();messages().AddListener("GamePlayState",OnGamePlayerState);}
        public void OnDispose()=>messages().RemoveListener("GamePlayState",OnGamePlayerState);
        public void Updata(float deltaTime,float unscaledDeltaTime){}
        public void OnGamePlayerState(object[] args)
        {
            if(args==null||args.Length==0)return;int state=(int)args[0];
            if(state!=8&&state!=9)return;if(level()<3)return;
            var player=Manager.Data.rankData;player.playNum=unchecked(player.playNum+1);SendRank(state==8);
        }
        public void SendRank(bool won)
        {
            var data=Manager.Data.rankData;int weight=unchecked(data.VN_t*Rate);
            var target=Transmitter;string uid=openId();
            target.SendRankDataRequest(uid,data.nick,data.url,unchecked(data.VN_d+(won?1:0)),unchecked(data.DN_d+(won?0:1)),2,weight);
            target=Transmitter;uid=openId();
            target.SendRankDataRequest(uid,data.nick,data.url,unchecked(data.VN_w+(won?1:0)),unchecked(data.DN_w+(won?0:1)),3,weight);
            target=Transmitter;uid=openId();
            target.SendRankDataRequest(uid,data.nick,data.url,unchecked(data.VN_t+(won?1:0)),unchecked(data.DN_t+(won?0:1)),1,weight);
            Transmitter.SendResponseCallback=won?(Action)(()=>{
                Transmitter.SendResponseCallback=null;var current=Manager.Data.rankData;
                current.VN_d=unchecked(current.VN_d+1);current.VN_w=unchecked(current.VN_w+1);current.VN_t=unchecked(current.VN_t+1);
            }):(()=>{
                Transmitter.SendResponseCallback=null;
                data.DN_d=unchecked(data.DN_d+1);data.DN_w=unchecked(data.DN_w+1);data.DN_t=unchecked(data.DN_t+1);
            });
        }
    }
    public sealed class OutgameMatchRuntime
    {
        readonly Func<OutgameDataManagerPool> pool;readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<int> level;readonly Func<string> openId;readonly Action initializeAvatar;
        public readonly OutgameManagerRegistration Registration;
        public OutgameMatchRuntime(Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,
            IOutgameDataStorageHost host,OutgameSdkStringStorage storage,OutgameDataVersionState versions,
            Action<string> download,Func<string> openId,Func<int> level,Func<OutgameMessageDispatcher> messages,Action initializeAvatar)
        {
            this.pool=pool;this.messages=messages;this.level=level;this.openId=openId;this.initializeAvatar=initializeAvatar;
            Registration=new OutgameManagerRegistration(4130,"Proj_hdzd",true,false,()=>new OutgameMatchManager(
                new OutgameDataManagerStorage(()=>"MatchManager",host,storage,versions),host,download,openId,level,
                ()=>GameRandomSource.Shared.Element(config().ChineseNames)));
        }
        public void BindController(OutgameControllerRegistry registry)
        {registry.Bind(4129,()=>new OutgameMatchControl(pool,messages,level,openId,initializeAvatar));}
        public static void BindTopInfo(OutgameTopInfoServices top,Func<OutgameMatchControl> control)
        {top.MatchPlayer=()=>control().Manager.Data.rankData;}
    }
}
