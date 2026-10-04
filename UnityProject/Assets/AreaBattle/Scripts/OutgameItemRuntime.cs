using System;
using UnityEngine;
namespace AreaBattle
{
    // The account/Main owner supplies actual storage, the legacy config resource,
    // reports and virtual-item owners. This group supplies no synthetic login state.
    public sealed class OutgameItemRuntime
    {
        public readonly OutgameItemConfigSlot Config;
        public readonly OutgameGlobalItemSlot Global;
        public readonly OutgameItemManagerSlot Manager;
        public readonly OutgameItemEntityServices Entities;
        public readonly OutgameManagerRegistration Registration;
        public OutgameItemRuntime(Func<OutgameDataManagerPool> pool,OutgameLegacyConfigReadState legacy,
            IOutgameDataStorageHost storageHost,OutgameSdkStringStorage storage,OutgameDataVersionState versions,
            Action<string> download,Func<OutgameToolControl> tool,Func<int,int> goodsType,
            OutgameVirtualItemServices virtualItems,Func<OutgameMessageDispatcher> messages,
            Func<IOutgameItemReports> reports,Action<string> error,Action<object[]> warning,Action<object[]> itemError,
            Func<Action,int> addUpdate=null,Action<int> removeUpdate=null)
        {
            Config=new OutgameItemConfigSlot(itemError);
            Manager=new OutgameItemManagerSlot(pool);
            Entities=new OutgameItemEntityServices {Virtual=virtualItems,
                Packages=new OutgamePackageServices {Manager=()=>Manager.Instance,Error=error}};
            Global=new OutgameGlobalItemSlot(h=>new OutgameProductServices(h,()=>Config.Instance,
                    ()=>DateTime.Now,()=>Time.deltaTime,Mathf.Pow,messages,warning,itemError).Lifecycle,
                ()=>Config.Instance,()=>legacy.Reader,messages,reports,error,
                addUpdate??OutgameUpdateManager.AddHandle,removeUpdate??OutgameUpdateManager.RemoveHandleById);
            Global.BindEntities(Entities);
            // Original4500 attribute: Proj_hdzd, autoSyn=true, compressData=false.
            // Pool initializes the manager before publishing it under type4500.
            Registration=new OutgameManagerRegistration(4500,"Proj_hdzd",true,false,()=>
                new OutgameItemManager(new OutgameDataManagerStorage(()=>"ItemManager",storageHost,storage,versions),
                    storageHost,download,()=>Global.Instance.Lifecycle,id=>Global.CreateFactory(id,Entities),
                    tool,goodsType,()=>DateTime.Now));
        }
        public void BindController(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,
            Func<OutgameMessageDispatcher> messages,Action<string> warning)
        {OutgameCoreControllerBindings.BindItems(registry,pool,()=>Config.Instance,messages,warning);}
    }
}
