using System;
namespace AreaBattle
{
    // Complete source Assembly-CSharp custom-config roster:4668,4675,4713, in metadata order.
    // Platform online/binary acquisition and later ActivityControl readiness remain required hosts.
    public sealed class OutgameActivityConfigRuntime:IDisposable
    {
        public readonly OutgameActivityConfigManager Manager;
        public readonly OutgameActivityConfigReaderServices ReaderServices;
        public readonly OutgameActivityCustomConfigServices CustomServices;
        public OutgameActivityConfigRuntime(OutgameActivityConfigReaderServices reader,OutgameActivityCustomConfigServices custom,Action<object[]> warning)
        {
            ReaderServices=reader;CustomServices=custom;
            Manager=new OutgameActivityConfigManager(new OutgameActivityConfigManagerServices{
                Current=()=>Manager,AssemblyTypes=()=>new[]{typeof(OutgameNoviceTaskConfigManager),typeof(OutgameTaskConfigManager),typeof(OutgameAchievementConfigManager)},
                CreateCustomManager=Create,CreateReader=()=>new OutgameActivityLocalConfigReader(ReaderServices),Error=ReaderServices.Error,Warning=warning});
            ReaderServices.Current=()=>Manager;CustomServices.Current=()=>Manager;
        }
        OutgameActivityConfigCustomManager Create(Type type)
        {
            if(type==typeof(OutgameNoviceTaskConfigManager))return new OutgameNoviceTaskConfigManager(CustomServices);
            if(type==typeof(OutgameTaskConfigManager))return new OutgameTaskConfigManager(CustomServices);
            if(type==typeof(OutgameAchievementConfigManager))return new OutgameAchievementConfigManager(CustomServices);
            throw new InvalidOperationException("Unregistered activity config manager: "+type.FullName);
        }
        public void Read(Action complete)=>Manager.ReadConfig(complete);
        public void Dispose()=>Manager.Dispose();
    }
}
