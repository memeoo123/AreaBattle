using System;
namespace AreaBattle
{
    public sealed class OutgameGuideBookRuntime
    {
        public readonly OutgameManagerRegistration Registration;
        public OutgameGuideBookRuntime(IOutgameDataStorageHost host,OutgameSdkStringStorage storage,
            OutgameDataVersionState versions,Action<string> download)
        {
            Registration=new OutgameManagerRegistration(4078,"Proj_hdzd",true,false,()=>
                new OutgameGuideBookManager(new OutgameDataManagerStorage(()=>"GuideBookDataManager",host,storage,versions),host,download));
        }
        public void BindController(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,
            Func<OutgameLegacyConfigManager> config,Func<int> currentLevel,Func<OutgameMessageDispatcher> messages)
        {OutgameCoreControllerBindings.BindGuideBook(registry,pool,config,currentLevel,messages);}
    }
}
