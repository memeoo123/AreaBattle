using System;
using System.Globalization;
using System.Numerics;
using System.Text;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameRankData
    {public int playerCurRank;public string curPlayerScore;public long lastTime;}
    public sealed class OutgameRankScoreServices
    {
        public OutgameRankHomeServices Home;
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<OutgameRankControl> Current;
        public Func<long> Now;
        public OutgameBigNumberSymbols Symbols;
        public Action<object[]> Warning;
        public Func<int,int,int> UnityRange=UnityEngine.Random.Range;
        public GameRandomSource Random=GameRandomSource.Shared;
        public string Format(BigInteger score)=>Symbols.Format(score,Warning);
        // ConfigHelper32596: contiguous 1-based keys, first area >= rank; the last
        // row is retained beyond the final area. Range includes its upper endpoint.
        public int RandomRankDown(int rank)
        {
            int minimum=1,maximum=2;
            for(int i=1;i<=Config().dicRankSub.Count;i++){
                var row=Config().dicRankSub[i];minimum=row.randValue[0];maximum=row.randValue[1];
                if(row.areaValue>=rank)break;
            }
            return Random.Inclusive(minimum,maximum);
        }
        // ConfigHelper32597 returns the LAST visited magnitude when no name matches.
        public int Magnitude(string name)
        {
            int result=-1;if(!string.IsNullOrEmpty(name))foreach(var row in Config().dicLargeNum.Values){result=row.mag;if(name==row.magName)break;}return result;
        }
    }
    // Source RankControl4134 score and persistence operations. Home lifecycle is in OutgameRankControl.cs.
    public sealed partial class OutgameRankControl
    {
        readonly OutgameRankScoreServices scoreServices;
        public BigInteger CurPlayerScore;
        public OutgameRankControl(OutgameRankScoreServices services){scoreServices=services;}
        public OutgameRankManager Manager=>scoreServices.Pool().GetModel<OutgameRankManager>(4137,"RankManager");
        public int CurPlayerRank=>Manager.Data.playerCurRank;
        public int AddScore()
        {
            int value=unchecked(scoreServices.UnityRange(150,400)*100);
            var addition=BigInteger.Parse(Math.Max(value,0).ToString(CultureInfo.InvariantCulture));
            CurPlayerScore+=addition;return ImproveRank();
        }
        public void ReduceScore()
        {
            int value=unchecked(scoreServices.UnityRange(100,200)*100);
            var reduction=BigInteger.Parse(Math.Max(value,0).ToString(CultureInfo.InvariantCulture));
            bool clear=reduction>=scoreServices.Current().CurPlayerScore;
            var current=scoreServices.Current();current.CurPlayerScore=clear?BigInteger.Zero:current.CurPlayerScore-reduction;
        }
        public int ImproveRank()
        {
            int down=scoreServices.RandomRankDown(CurPlayerRank);
            int rank=unchecked(CurPlayerRank-Math.Max(down,0));var manager=Manager;rank=Math.Max(rank,1);manager.SetRank(rank);return rank;
        }
        public void InitPlayerRank()
        {
            if(CurPlayerRank>999)return;
            long now=scoreServices.Now();long then=Manager.Data.lastTime;
            int hours=unchecked((int)(unchecked(now-then)/3600000));if(hours<2)return;
            int loss=unchecked(scoreServices.UnityRange(1,10)*hours);
            int rank=unchecked(CurPlayerRank+loss);Manager.SetRank(rank);
        }
    }
    public sealed class OutgameRankManager:IOutgameDataManager
    {
        readonly OutgameDataManagerStorage storage;readonly IOutgameDataStorageHost host;
        readonly Action<string> download;readonly OutgameRankScoreServices services;
        StringBuilder builder;
        public OutgameRankData Data;
        public string DataKey=>"RankManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public int PlayerRank=>Data.playerCurRank;
        public long LastTime=>Data.lastTime;
        public OutgameRankManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,OutgameRankScoreServices services)
        {this.storage=storage;this.host=host;this.download=download;this.services=services;}
        public void OnInit()=>UpdateData(true);
        public void UpdateData(bool allowServer)
        {if(allowServer&&host.IsUseServer&&ParticipatesInSync){download(DataKey);return;}UpdateDataCallBack(storage.ReadLocalData());}
        public void UpdateDataCallBack(string text)
        {
            if(string.IsNullOrEmpty(text))CreateNewData();
            else{Data=LitJson.JsonMapper.ToObject<OutgameRankData>(text);if(Data==null)CreateNewData();}
            if(!string.IsNullOrEmpty(Data.curPlayerScore)){
                string score=Data.curPlayerScore;var control=services.Current();BigInteger.TryParse(score,out control.CurPlayerScore);
            }else services.Current().CurPlayerScore=BigInteger.Zero;
        }
        public void CreateNewData()
        {
            Data=new OutgameRankData();var config=services.Config();Data.playerCurRank=config.newRankSettingConfig.intPlayerRank;
            Data.curPlayerScore=string.Empty;long now=services.Now();Data.lastTime=now;
        }
        public void SetRank(int rank)
        {var data=Data;data.playerCurRank=Math.Min(rank,22000);data.lastTime=services.Now();}
        public void OnRelease(){} //31695 is empty.
        public string GetLargeNumZero(string symbol,out bool isMagnitude)
        {
            isMagnitude=false;int length=2;
            if(!int.TryParse(symbol,out int value)){value=services.Magnitude(symbol);isMagnitude=true;length=Math.Max(unchecked(value+2),2);}
            if(builder==null)builder=new StringBuilder();builder.Clear();for(int i=0;i<length;i++)builder.Append("0");return builder.ToString();
        }
        // ExtesionMethod32605. Original has no NaN/infinity termination guard.
        public static int FloatToInt(float value,out int decimals)
        {
            decimals=0;int integer;
            while((float)(integer=WasmInt(value))!=value){value*=10;decimals=unchecked(decimals+1);}return integer;
        }
        static int WasmInt(float value)=>Math.Abs((double)value)<2147483648d?(int)value:int.MinValue;
        public string GetLargeNumStr(string text)
        {
            if(string.IsNullOrEmpty(text))return string.Empty;
            string zero=GetLargeNumZero(text[text.Length-1].ToString(),out bool isMagnitude);
            float.TryParse(text.Substring(0,isMagnitude?text.Length-1:text.Length),out float value);
            value=FloatToInt(value,out int decimals);
            if(zero.Length-decimals<0){int excess=unchecked(decimals-zero.Length);value/=unchecked(excess*10);decimals=unchecked(decimals-excess);}
            string integer=WasmInt(value).ToString(CultureInfo.InvariantCulture);
            if(builder==null)builder=new StringBuilder();builder.Clear();builder.Append(integer);
            if(zero.Length-decimals>0)builder.Append(zero.Substring(0,zero.Length-decimals));return builder.ToString();
        }
        public void OnSave()
        {
            var data=Data;var control=services.Current();var score=control.CurPlayerScore;data.curPlayerScore=services.Format(score);
            data=Data;data.curPlayerScore=GetLargeNumStr(data.curPlayerScore);
            storage.SaveLocalData(LitJson.JsonMapper.ToJson(Data));
        }
    }
    public sealed class OutgameRankScoreRuntime
    {
        public readonly OutgameRankScoreServices Services;public readonly OutgameManagerRegistration Registration;
        public OutgameRankScoreRuntime(OutgameRankScoreServices services,IOutgameDataStorageHost host,OutgameSdkStringStorage storage,OutgameDataVersionState versions,Action<string> download)
        {
            Services=services;Registration=new OutgameManagerRegistration(4137,"Proj_hdzd",true,false,()=>new OutgameRankManager(
                new OutgameDataManagerStorage(()=>"RankManager",host,storage,versions),host,download,services));
        }
        public void BindTopInfo(OutgameTopInfoServices top){top.RankScore=()=>Services.Current().CurPlayerScore;}
    }
}
