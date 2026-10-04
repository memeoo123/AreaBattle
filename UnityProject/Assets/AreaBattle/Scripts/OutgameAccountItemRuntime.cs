using System;
namespace AreaBattle
{
    // Required presentation/report/skin endpoints; Save is owned by the actual account data pool.
    public sealed class OutgameAccountToolEndpoints
    {
        public Action RefreshTopInfo;
        public Action<int,long> GoldSpent;
        public Action<int> ToolChanged,UnlockScene,UnlockSoldier,MissingItemEntity;
        public Action<int,int,int,int,string> ReportGet;
        public Action<int,int,int,int> ReportCost;
    }
    public sealed class OutgameAccountItemServices
    {
        public Func<OutgameDataManagerPool> Pool;
        public OutgameProfile Profile;
        public OutgameLegacyConfigReadState Legacy;
        public IOutgameDataStorageHost StorageHost;
        public OutgameSdkStringStorage Storage;
        public OutgameDataVersionState Versions;
        public string Skills,Products,GameItems,SceneSkins,SoldierSkins;
        public Func<DateTime> Now;
        public Action<string> Download;
        public Action<OutgameLocalDataManager,string> RegisterSaveData;
        public OutgameVirtualItemServices VirtualItems;
        public Func<OutgameMessageDispatcher> Messages;
        public Func<IOutgameItemReports> Reports;
        public Func<string> PurchaseCost;
        public Action<string> Error;
        public Action<object[]> Warning,ItemError;
        public OutgameAccountToolEndpoints Tools;
        public Func<Action,int> AddUpdate=OutgameUpdateManager.AddHandle;
        public Action<int> RemoveUpdate=OutgameUpdateManager.RemoveHandleById;
    }
    // Composition of original LocalDataManager4119, ItemManager4500 and ToolControl4256.
    // The account owner registers/initializes these using its real login and storage state.
    public sealed class OutgameAccountItemRuntime
    {
        readonly OutgameAccountItemServices services;
        OutgameLocalInventoryState inventoryState;OutgameLocalInventory inventory;OutgameToolDispatcher dispatcher;
        public readonly OutgameLocalDataManager Local;
        public readonly OutgameManagerRegistration LocalRegistration;
        public readonly OutgameItemRuntime Items;
        public readonly OutgameToolControl Tool;
        public OutgameAccountItemRuntime(OutgameAccountItemServices services)
        {
            this.services=services;
            Local=new OutgameLocalDataManager(services.Profile,services.Skills,services.Products,services.Now,
                new OutgameDataManagerStorage(()=>"LocalDataManager",services.StorageHost,services.Storage,services.Versions),services.StorageHost,services.Download,services.RegisterSaveData);
            LocalRegistration=new OutgameManagerRegistration(4119,"Proj_hdzd",true,false,()=>Local);
            Tool=new OutgameToolControl(()=>Dispatcher,()=>Inventory,id=>Items.Manager.Instance.GetItemNum(id),services.PurchaseCost);
            Items=new OutgameItemRuntime(services.Pool,services.Legacy,services.StorageHost,services.Storage,services.Versions,services.Download,
                ()=>Tool,id=>Dispatcher.GoodsType(id),services.VirtualItems,services.Messages,services.Reports,services.Error,services.Warning,services.ItemError,services.AddUpdate,services.RemoveUpdate);
        }
        public OutgameLocalInventory Inventory
        {
            get
            {
                var current=services.Profile.inventory;
                // LocalDataManager's server callback replaces the profile projection. Subsequent
                // ToolControl calls must resolve the current local record, not a stale inventory.
                if(!ReferenceEquals(current,inventoryState)){inventory=new OutgameLocalInventory(current,services.Skills);inventoryState=current;dispatcher=null;}
                return inventory;
            }
        }
        public OutgameToolDispatcher Dispatcher
        {
            get
            {
                var current=Inventory;
                return dispatcher??(dispatcher=new OutgameToolDispatcher(current,new Effects(this),services.GameItems,services.SceneSkins,services.SoldierSkins));
            }
        }
        sealed class Effects:IOutgameToolEffects
        {
            readonly OutgameAccountItemRuntime owner;
            OutgameAccountToolEndpoints Host=>owner.services.Tools;
            public Effects(OutgameAccountItemRuntime owner){this.owner=owner;}
            public void RefreshTopInfo()=>Host.RefreshTopInfo();
            public void GoldSpent(int id,long amount)=>Host.GoldSpent(id,amount);
            public void ToolChanged(int id)=>Host.ToolChanged(id);
            public void UnlockScene(int id)=>Host.UnlockScene(id);
            public void UnlockSoldier(int id)=>Host.UnlockSoldier(id);
            public bool ApplyItemEntity(int id,long amount)
            {
                var config=owner.Items.Config.Instance.GetGameItemConfig(id);var item=owner.Items.Manager.Instance.GetItem(config);
                if(item==null)return false;item.AddItem(amount);return true;
            }
            public void MissingItemEntity(int id)=>Host.MissingItemEntity(id);
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>Host.ReportGet(id,category,amount,balance,reason);
            public void ReportCost(int id,int category,int amount,int balance)=>Host.ReportCost(id,category,amount,balance);
            // Original ToolChange32533 saves the entire pool on every normal return, including
            // notifyAndReport=false reward delivery. No transaction or claim-state reordering.
            public void Save()=>owner.services.Pool().SaveData();
        }
    }
}
