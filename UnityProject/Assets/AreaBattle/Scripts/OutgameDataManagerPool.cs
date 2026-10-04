using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameDataManager:IOutgameVersionManager
    {
        new bool ParticipatesInSync {get;set;}
        bool CompressData {get;set;}
        void OnInit();
        void OnSave();
        void OnRelease();
    }
    public sealed class OutgameManagerRegistration
    {
        public readonly int SourceTypeIndex;
        public readonly string GameName;
        public readonly bool AutoSyn,CompressData;
        public readonly Func<IOutgameDataManager> Create;
        public OutgameManagerRegistration(int sourceTypeIndex,string gameName,bool autoSyn,bool compressData,Func<IOutgameDataManager> create)
        {SourceTypeIndex=sourceTypeIndex;GameName=gameName;AutoSyn=autoSyn;CompressData=compressData;Create=create??throw new ArgumentNullException(nameof(create));}
    }
    // One source DataManagerPool session. Registrations retain original type identity;
    // required factories provide concrete recovered managers, not placeholder data.
    public sealed class OutgameDataManagerPool
    {
        public Dictionary<int,IOutgameDataManager> Managers {get;private set;}
        public bool SaveDisabled {get;private set;}
        public bool SourceReadyFlag {get;set;}=true;
        public bool IsEnableSaveData=>SourceReadyFlag&&!SaveDisabled&&Managers!=null;
        readonly Action initializeUploadQueue;
        readonly Action<string> warning,log,error;
        public OutgameDataManagerPool(Action initializeUploadQueue,Action<string> warning,Action<string> log,Action<string> error)
        {this.initializeUploadQueue=initializeUploadQueue??throw new ArgumentNullException(nameof(initializeUploadQueue));this.warning=warning??throw new ArgumentNullException(nameof(warning));this.log=log??throw new ArgumentNullException(nameof(log));this.error=error??throw new ArgumentNullException(nameof(error));}
        // OnInit f12656 and automatic registration f12652.
        public void OnInit(bool autoRegister,string mineGameName,IEnumerable<OutgameManagerRegistration> registrations)
        {
            SaveDisabled=false;Managers=new Dictionary<int,IOutgameDataManager>();initializeUploadQueue();
            if(autoRegister)
            {
                foreach(var entry in registrations)
                {
                    if(!string.Equals(entry.GameName,mineGameName))continue;
                    var manager=entry.Create();manager.ParticipatesInSync=entry.AutoSyn;manager.CompressData=entry.CompressData;
                    manager.OnInit();Managers[entry.SourceTypeIndex]=manager;
                }
            }
            log("初始化DataManagerPool完成");
        }
        // Original generic GetModel26750: missing/uninitialized pool warns and returns default.
        public T GetModel<T>(int sourceType,string sourceName) where T:class,IOutgameDataManager
        {
            if(Managers!=null&&Managers.ContainsKey(sourceType))return (T)Managers[sourceType];
            warning("未找到["+sourceName+"]数据管理器");return null;
        }
        // Shared generic26734: publish before initialization; duplicate returns the supplied object.
        // Manual AddModel only applies the source attribute's AutoSyn (false when absent).
        public T AddModel<T>(int sourceType,T manager,bool sourceAutoSyn,bool initialize=true) where T:class,IOutgameDataManager
        {
            _=manager.GetType();
            if(!Managers.ContainsKey(sourceType))
            {
                manager.ParticipatesInSync=sourceAutoSyn;
                Managers.Add(sourceType,manager);
                if(initialize)manager.OnInit();
            }
            else error("已添加过数据管理器:"+manager);
            return manager;
        }
        public void SetSaveDisabled(bool disabled)
        {warning(disabled?"禁用数据全局存储":"激活数据全局存储");SaveDisabled=disabled;}
        // Source26740 closes readiness, saves, then releases the live value collection.
        // Release errors propagate; Clear is reached only after every manager returns.
        public void OnRelease()
        {
            SourceReadyFlag=false;SaveData();
            if(Managers!=null){foreach(var manager in Managers.Values)manager.OnRelease();Managers.Clear();}
        }
        // SaveData f2323 checks only the disable flag and dictionary, not SourceReadyFlag.
        public void SaveData()
        {
            if(SaveDisabled){warning("数据存储功能被禁用了");return;}
            if(Managers==null)return;
            foreach(var manager in Managers.Values)
            {
                if(manager==null)continue;
                try{manager.OnSave();}
                catch(Exception exception){error("保存数据["+manager.DataKey+"]时发生异常="+exception);}
            }
        }
    }
}
