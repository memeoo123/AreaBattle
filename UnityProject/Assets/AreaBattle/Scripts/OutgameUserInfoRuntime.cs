using System;
namespace AreaBattle
{
    // Production UserInfo registration and source name provider. Account/Main supplies
    // real timestamp, install-state, storage/download and role-report endpoints.
    public sealed class OutgameUserInfoRuntime
    {
        readonly Func<OutgameDataManagerPool> pool;
        readonly Func<OutgameLegacyConfigManager> config;
        readonly Action<string> reportCreateRole;
        public readonly OutgameAiNames Names;
        public readonly OutgameManagerRegistration Registration;
        public OutgameUserInfoRuntime(Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,
            IOutgameDataStorageHost host,OutgameSdkStringStorage storage,OutgameDataVersionState versions,
            Action<string> download,Func<long> timestamp,Func<bool> isInstallVersion,
            Func<OutgameMessageDispatcher> messages,Action<string> reportCreateRole,Action<object[]> randomLog)
        {
            this.pool=pool;this.config=config;this.reportCreateRole=reportCreateRole;
            Names=new OutgameAiNames(config,randomLog);
            Registration=new OutgameManagerRegistration(4229,"Proj_hdzd",true,false,()=>
                new OutgameUserInfoManager(new OutgameDataManagerStorage(()=>"UserInfoManager",host,storage,versions),
                    host,download,timestamp,isInstallVersion,Names.Generate,messages));
        }
        public void BindController(OutgameControllerRegistry registry,OutgameVirtualItemServices items)
        {
            OutgameCoreControllerBindings.BindUserInfo(registry,pool,config,reportCreateRole);
            // Item entity follows UserInfoControl's current singleton/manager, including
            // the source null pre-init field; it does not create a second profile.
            items.HeadBoxes=()=>((OutgameUserInfoControl)registry.Resolve(4228)).Manager.HeadBoxes;
        }
    }
}
