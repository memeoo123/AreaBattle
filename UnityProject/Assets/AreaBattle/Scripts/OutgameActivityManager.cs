using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // CommonModuleManagerBase4659 inherits the original account-aware DataManagerBase storage path.
    public abstract class OutgameCommonModuleManager:IOutgameDataManager
    {
        protected readonly OutgameDataManagerStorage Storage;
        readonly IOutgameDataStorageHost storageHost;
        readonly Action<string> download;
        public abstract int ActivityId{get;}
        public abstract string SourceClassName{get;}
        public string DataKey=>"CommonGameModule"+SourceClassName;
        public bool ParticipatesInSync{get=>Storage.AutoSyn;set=>Storage.AutoSyn=value;}
        public bool CompressData{get=>Storage.CompressData;set=>Storage.CompressData=value;}
        protected OutgameCommonModuleManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost storageHost,Action<string> download)
        {Storage=storage;this.storageHost=storageHost;this.download=download;}
        public virtual void InitStrategy(){}
        public void SaveData(string text)=>SaveLocalData(text);
        public virtual void SaveLocalData(string text)=>Storage.SaveLocalData(text);
        public virtual void UpdateData(bool allowServer)
        {
            if(allowServer&&storageHost.IsUseServer&&ParticipatesInSync){download(DataKey);return;}
            UpdateDataCallBack(Storage.ReadLocalData());
        }
        public abstract void UpdateDataCallBack(string text);
        public abstract void OnInit();
        public abstract void OnSave();
        public abstract void OnRelease();
    }
    public sealed class OutgameActivityManagerServices
    {
        public Func<Action> RegistrationCallback;
        public Func<Type[]> AssemblyTypes;
        public Func<Type,OutgameCommonModuleManager> CreateManager;
        public Func<Type,int> SourceTypeIndex;
        public Func<Type,bool> SourceAutoSyn;
        public Func<Dictionary<object,OutgameActivityConfigRow>> Configurations;
        public Func<OutgameDataManagerPool> Pool;
        public Action DataReady;
        public Action<object[]> Error;
        public OutgameActivityOffNetServices OffNet;
    }
    // ActivityManager4658; owner/configuration/assembly bindings are required from the actual activity host.
    public sealed class OutgameActivityManager:OutgameCommonModuleManager
    {
        readonly OutgameActivityManagerServices services;
        public OutgameActivityData Data;
        public OutgameActivityStrategy Strategy;
        public bool Dirty,IsInitStrategy;
        public override int ActivityId=>0;
        public override string SourceClassName=>"ActivityManager";
        public OutgameActivityManager(OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> download,OutgameActivityManagerServices services)
            :base(storage,host,download){this.services=services;}
        public override void OnInit(){}
        public void Init()=>InitData();
        public void InitData()
        {
            Strategy=new OutgameActivityOffNetStrategy(services.OffNet);
            Strategy.Manager=this;
            Strategy.InitData(RefreshData);
        }
        public override void UpdateDataCallBack(string text)=>Strategy.LoadData(text);
        public void RefreshData(OutgameActivityData data)
        {
            Data=data;
            if(data==null){services.Error(new object[]{"活动数据为null"});return;}
            ManagerInit();services.DataReady();
        }
        public void ManagerInit()
        {
            if(IsInitStrategy)return;
            if(services.RegistrationCallback()!=null)services.RegistrationCallback()();
            else
            {
                foreach(var type in services.AssemblyTypes())
                {
                    if(!type.IsSubclassOf(typeof(OutgameCommonModuleManager)))continue;
                    var manager=services.CreateManager(type);
                    if(services.Configurations()!=null&&services.Configurations().ContainsKey(manager.ActivityId))
                    {
                        var runtimeType=manager.GetType();
                        services.Pool().AddModel(services.SourceTypeIndex(runtimeType),manager,services.SourceAutoSyn(runtimeType));
                        manager.InitStrategy();
                    }
                }
            }
            IsInitStrategy=true;
        }
        public override void OnSave(){if(Strategy!=null)Strategy.OnSave(Data);}
        public override void OnRelease(){IsInitStrategy=false;}
    }
}
