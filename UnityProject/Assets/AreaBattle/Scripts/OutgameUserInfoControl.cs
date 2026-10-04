using System;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // Original4228. Selection checks config membership, not unlock membership.
    public sealed class OutgameUserInfoControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;readonly Func<OutgameDataManagerPool> pool;
        readonly Func<OutgameLegacyConfigManager> config;readonly Action<string> reportCreateRole;
        public OutgameUserInfoManager Manager {get;private set;}
        public OutgameUserInfoControl(OutgameControllerRegistry registry,Func<OutgameDataManagerPool> pool,
            Func<OutgameLegacyConfigManager> config,Action<string> reportCreateRole)
        {this.registry=registry;this.pool=pool;this.config=config;this.reportCreateRole=reportCreateRole;}
        public void OnInit(){Manager=pool().GetModel<OutgameUserInfoManager>(4229,"UserInfoManager");Manager.CheckInstallVersion();}
        public void Updata(float deltaTime,float unscaledDeltaTime){} //32213 source empty.
        public void OnDispose()=>registry.Clear(4228);
        public string Name=>Manager.Name;
        public int Icon=>Manager.Icon;
        public int IconBox=>Manager.IconBox;
        public long InstallTime=>Manager.InstallTime;
        public bool IsHeadPortUnlock(int id)=>Manager.HaveHeadPort(id);
        public bool IsHeadBoxUnlock(int id)=>Manager.HaveHeadBox(id);
        public void UnlockHeadBox(int id)=>Manager.HeadBoxes.UnlockHeadBox(id);
        public HeadportConfig GetHeadportConfig(int id=-1)
        {if(id==-1)id=Manager.Icon;return config().dicHeadport.TryGetValue(id,out var row)?row:null;}
        public HeadBoxConfig GetHeadBoxConfig(int id=-1)
        {if(id==-1)id=Manager.IconBox;return config().dicHeadBox.TryGetValue(id,out var row)?row:null;}
        public void ApplyHeadport(int id){if(config().dicHeadport.ContainsKey(id)){Manager.Icon=id;Manager.OnSave();}}
        public void ApplyHeadbox(int id){if(config().dicHeadBox.ContainsKey(id)){Manager.IconBox=id;Manager.OnSave();}}
        public void ApplyName(string value){Manager.Name=value;Manager.OnSave();reportCreateRole(value);}
    }
}
