using System;
namespace AreaBattle
{
    public interface IOutgameStartupModule
    {
        Action Initialized {get;set;}
        void Initialize();
    }
    // MineGameMain30610/30609/30629/30613/30625. SDK/prefs precede Begin; coreReady starts30612.
    public sealed class OutgameCoreModuleStartup
    {
        readonly Func<string,IOutgameStartupModule> module;
        readonly Action<string> log;readonly Action coreReady;
        public OutgameCoreModuleStartup(Func<string,IOutgameStartupModule> module,Action<string> log,Action coreReady)
        {this.module=module;this.log=log;this.coreReady=coreReady;}
        public void Begin()=>Initialize("VersionMondule",VersionReady);
        void Initialize(string name,Action callback)
        {var current=module(name);current.Initialized=callback;current.Initialize();}
        void VersionReady()
        {log("VersionMondule初始化完成");Initialize("AssetbundleModule",AssetBundlesReady);}
        void AssetBundlesReady()
        {log("资源模块初始化完成，开始初始化其他次级模块");Initialize("ResourcesModule",ResourcesReady);}
        void ResourcesReady()=>Initialize("UIModule",UiReady);
        void UiReady()
        {
            Initialize("LangModule",null);
            Initialize("FsmManager",null);
            Initialize("MineGameLogicModule",null);
            Initialize("TimeModule",null);
            Initialize("EffectModule",null);
            Initialize("ObjectPoolManager",null);
            Initialize("ProcedureManager",coreReady);
        }
    }
}
