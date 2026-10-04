using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameActivityOffNetServices
    {
        public OutgameStatisticsExpansion Statistics;
        public Func<Dictionary<object,OutgameActivityConfigRow>> Configurations;
        public Func<int> GetNowTimeInt;
        public Action<bool> SetDirty;
        public Action<string> Warning;
        public Action<object[]> Error;
    }
    // ActivityNetStrategyBase4660: invalid condition text is skipped; successful conditions use statistics.
    public class OutgameActivityStrategy
    {
        protected readonly OutgameActivityOffNetServices Services;
        protected Action<OutgameActivityData> UpdateDataAction;
        public OutgameCommonModuleManager Manager;
        public OutgameActivityStrategy(OutgameActivityOffNetServices services){Services=services;}
        public virtual void InitData(Action<OutgameActivityData> callback){UpdateDataAction=callback;}
        public virtual void OnSave(OutgameActivityData data){}
        public virtual void LoadData(string text){}
        public void UpdateManagerData(bool allowServer)=>Manager.UpdateData(allowServer);
        public bool ConditionsMet(int[] types,string[] parameters)
        {
            for(int i=0;i<types.Length;i++)
            {
                string text=parameters[i];
                if(types[i]==10000)
                {
                    if(!TryParseDate(text,out var date))continue;
                    long now=Services.Statistics.GameValue(10000,Array.Empty<object>());
                    if(OutgameItemTimestamp.FromDateTime(date)>now)return false;
                }
                else if(int.TryParse(text,out int target)&&Services.Statistics.GameValue(types[i],Array.Empty<object>())<target)return false;
            }
            return true;
        }
        public bool TryParseDate(string text,out DateTime date)=>DateTime.TryParseExact(text,"yyyyMMddHHmmss",CultureInfo.InvariantCulture,DateTimeStyles.None,out date);
    }
    public sealed class OutgameActivityOffNetStrategy:OutgameActivityStrategy
    {
        public OutgameActivityOffNetStrategy(OutgameActivityOffNetServices services):base(services){}
        public override void InitData(Action<OutgameActivityData> callback)
        {UpdateDataAction=callback;Manager.UpdateData(true);}
        public override void OnSave(OutgameActivityData data)=>Manager.SaveLocalData(OutgameActivityCodec.CompressString(JsonUtility.ToJson(data)));
        public override void LoadData(string text)
        {
            OutgameActivityData data=null;
            try{data=JsonUtility.FromJson<OutgameActivityData>(OutgameActivityCodec.DecompressString(text,Services.Warning));}
            catch
            {
                try{data=JsonUtility.FromJson<OutgameActivityData>(text);}
                catch(Exception exception){Services.Error(new object[]{exception.Message,exception.StackTrace});}
            }
            if(data==null)
            {
                data=new OutgameActivityData();
                foreach(var config in Services.Configurations().Values)data.datas.Add(NewRecord(config));
                data.firstLoginDay=Services.GetNowTimeInt();
                Services.SetDirty(true);
            }
            else
            {
                foreach(var config in Services.Configurations().Values)
                {
                    bool found=false;
                    for(int i=0;i<data.datas.Count;i++)
                    {
                        if(data.datas[i].id!=config.id)continue;
                        var record=data.datas[i];found=true;
                        if(ConditionsMet(config.launchType,config.launchParams))
                        {
                            if(record.state==4&&!ConditionsMet(config.overType,config.overParams))Reset(record);
                        }
                        else if(ConditionsMet(config.noticeType,config.noticeParams))
                        {
                            if(record.state==4)
                            {if(!ConditionsMet(config.overType,config.overParams))Reset(record);}
                            else if(record.state==3&&!ConditionsMet(config.launchType,config.launchParams))Reset(record);
                        }
                        break;
                    }
                    if(!found){data.datas.Add(NewRecord(config));Services.SetDirty(true);}
                }
            }
            UpdateDataAction?.Invoke(data);
        }
        static OutgameActivityItemData NewRecord(OutgameActivityConfigRow config)=>new OutgameActivityItemData{id=config.id,uniqueId=config.uniqueId};
        void Reset(OutgameActivityItemData record){record.state=1;Services.SetDirty(true);}
    }
}
