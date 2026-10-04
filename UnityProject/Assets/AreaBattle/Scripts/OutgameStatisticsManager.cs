using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public sealed class OutgameStatisticsManager:IOutgameDataManager
    {
        public const string RegistrationMessage="CommonGameModule_StatisticsEventResiger",SetMessage="CommonGameModule_StatisticsEventSet",RefreshMessage="CommonModule_StatisticsData_Refresh";
        public static Action<OutgameStatisticsJsonData,Dictionary<int,OutgameGameStatisticsData>> SaveDataDel;
        readonly OutgameDataManagerStorage storage;
        readonly IOutgameDataStorageHost storageHost;
        readonly Action<string> download;
        readonly Func<OutgameStatisticsStrategy> createOffNetStrategy;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<OutgameCommonMessageDispatcher> common;
        readonly Func<OutgameStatisticsControl> control;
        readonly OutgameStatisticsExpansion expansion;
        readonly Action<object[]> log;
        public readonly OutgameStatisticsRecords Records;
        public OutgameStatisticsJsonData Data;
        public OutgameStatisticsStrategy Strategy;
        public bool IsInit=true;
        public Dictionary<int,OutgameGameStatisticsData> Datas{get=>Records.Datas;set=>Records.Datas=value;}
        public string DataKey=>"CommonGameModuleGameStatisticsManager";
        public bool ParticipatesInSync{get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData{get=>storage.CompressData;set=>storage.CompressData=value;}
        // The required factory must provide the original off-net strategy. No fallback strategy is supplied.
        public OutgameStatisticsManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost storageHost,Action<string> download,
            Func<OutgameStatisticsStrategy> createOffNetStrategy,Func<OutgameMessageDispatcher> messages,
            Func<OutgameCommonMessageDispatcher> common,Func<OutgameStatisticsControl> control,
            OutgameStatisticsExpansion expansion,Action<object[]> log)
        {
            this.storage=storage;this.storageHost=storageHost;this.download=download;this.createOffNetStrategy=createOffNetStrategy;
            this.messages=messages;this.common=common;this.control=control;this.expansion=expansion;this.log=log;
            Records=new OutgameStatisticsRecords(common);
        }
        public void OnInit()
        {
            Strategy=createOffNetStrategy();
            messages().AddListener(RegistrationMessage,RegisterFromMessage);
            messages().AddListener(SetMessage,SetFromMessage);
        }
        public void InitStrategy(){if(Strategy!=null)Strategy.InitData(RefreshData);}
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&storageHost.IsUseServer&&ParticipatesInSync){download(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {Strategy.LoadData(text);common().SendMessage(RefreshMessage);}
        public void SaveData(string text)=>storage.SaveLocalData(text);
        public void RefreshData(OutgameStatisticsJsonData data)
        {
            Data=data;
            if(data==null)log(new object[]{"游戏统计数据为null"});
            Records.IndexRecords(data);
            RegisteredGameEventValues();
            if(IsInit){IsInit=false;control().InitOver?.Invoke();}
        }
        public void RegisteredGameEventValues()
        {
            expansion.RegisteredValueFunc(10000,Strategy.Value10000);
            expansion.RegisteredValueFunc(10015,Strategy.Value10015);
            expansion.RegisteredValueFunc(10011,Strategy.Value10011);
            expansion.RegisteredValueFunc(10901,Strategy.Value10901);
            expansion.RegisteredValueFunc(10900,Strategy.Value10900);
            expansion.RegisteredValueFunc(10800,Strategy.Value10800);
            expansion.RegisteredValueFunc(10003,Strategy.Value10003);
            expansion.RegisteredValueFunc(10002,Strategy.Value10002);
            expansion.RegisteredValueFunc(10001,Strategy.Value10001);
            expansion.RegisteredValueFunc(10902,Strategy.Value10902);
            expansion.RegisteredValueFunc(10008,Strategy.Value10008);
            expansion.RegisteredValueFunc(10007,Strategy.Value10007);
            expansion.RegisteredValueFunc(20000,Strategy.Value20000);
            expansion.RegisteredValueFunc(10500,Strategy.Value10500);
            expansion.RegisteredValueFunc(10501,Strategy.Value10501);
            expansion.RegisteredValueFunc(10502,Strategy.Value10502);
            expansion.RegisteredValueFunc(10600,Strategy.Value10600);
            expansion.RegisteredValueFunc(10700,Strategy.Value10700);
            expansion.RegisteredValueFunc(10013,Strategy.Value10013);
        }
        public void RegisterFromMessage(object[] args)
        {
            if(args==null||args.Length<2)return;
            object id=args[0];var provider=(Func<object[],long>)args[1];
            expansion.RegisteredValueFunc((int)id,provider);
        }
        public void SetFromMessage(object[] args)
        {
            if(args==null||args.Length<2)return;
            int id=(int)args[0];
            if(control().ValueProviders.ContainsKey(id))return;
            if(long.TryParse(args[1].ToString(),out long value))
            {
                if(args.Length==2)expansion.SetEventCount(id,value);
                else if(args.Length==3)expansion.SetEventCount(id,(int)args[2],value);
                return;
            }
            if(int.TryParse(args[1].ToString(),out int intValue))
            {
                if(args.Length==2)expansion.SetEventCount(id,(long)intValue);
                else if(args.Length==3)expansion.SetEventCount(id,(int)args[2],(long)intValue);
            }
        }
        public void AddEventStatistics(int id,long value)=>Records.AddEventStatistics(id,value);
        public void AddEventStatistics(int id,int itemId,long value)=>Records.AddEventStatistics(id,itemId,value);
        public void SetEventStatistics(int id,long value)=>Records.SetEventStatistics(id,value);
        public void SetEventStatistics(int id,int itemId,long value)=>Records.SetEventStatistics(id,itemId,value);
        public void ResetEventStatistics(int id)=>Records.ResetEventStatistics(id);
        public long GetEventStatistics(int id)=>Records.GetEventStatistics(id);
        public long GetEventStatistics(int id,int itemId)=>Records.GetEventStatistics(id,itemId);
        public OutgameGameStatisticsData GetGameStatisticsData(int id)=>Records.GetGameStatisticsData(id);
        public void Update(){if(Strategy!=null)Strategy.Update();}
        public void OnSave(){SaveDataDel?.Invoke(Data,Datas);if(Strategy!=null)Strategy.OnSave(Data,Datas);}
        public void OnRelease()
        {
            messages().RemoveListener(RegistrationMessage,RegisterFromMessage);
            messages().RemoveListener(SetMessage,SetFromMessage);
            if(Strategy!=null)Strategy.Dispose();
            Datas?.Clear();Data=null;IsInit=true;
        }
    }
}
