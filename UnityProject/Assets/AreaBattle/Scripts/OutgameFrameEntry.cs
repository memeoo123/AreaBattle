using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // GameFrameModule3436 slots4..8 and fields8/12. Initialization arguments remain live.
    public interface IOutgameFrameModule:IOutgameStartupModule
    {
        int Priority{get;}
        bool IsInitialized{get;}
        void Initialize(object[] args);
        void Start();
        void Update(float delta,float unscaled);
        void Shutdown();
    }
    public sealed class OutgameFrameServices
    {
        // Restored modules require constructor dependencies; supplied factories replace Activator only.
        public Func<Type,IOutgameFrameModule> CreateModule;
        public Action SetCulture,ShutdownInput,ClearPermissions;
        public Action<bool> SetConfigReadInitialized;
        public Action<string> Log,Warning;
        public Func<bool> SuppressResourceUpdate;
        public Func<IOutgameFrameModule> Resources;
    }
    // One original static GameFrameEntry3433 domain, owned by the app entry.
    // Module dispatch26440..45/49/50 and shared generic26446/47; no fake startup modules.
    public sealed partial class OutgameFrameEntry
    {
        readonly OutgameFrameServices services;
        public readonly LinkedList<IOutgameFrameModule> Modules=new LinkedList<IOutgameFrameModule>();
        public bool Started;
        public string GameMenuSceneName="";
        public bool IsGoGameMenu;
        public Action DisposableActions;
        public OutgameFrameEntry(OutgameFrameServices services){this.services=services;}
        public T GetModule<T>()where T:class,IOutgameFrameModule=>(T)GetModule(typeof(T));
        public IOutgameFrameModule GetModule(Type type)
        {
            for(var node=Modules.Last;node!=null;node=node.Previous)if(node.Value.GetType()==type)return node.Value;
            var module=services.CreateModule(type);
            // Source failed base-type cast reaches null.GetType before its nominal error message.
            if(module==null){_=module.GetType();}
            services.Log("创建模块==="+type.Name);
            for(var node=Modules.First;node!=null;node=node.Next)
                if(module.Priority>node.Value.Priority){Modules.AddBefore(node,module);return module;}
            Modules.AddLast(module);return module;
        }
        public T HaveModule<T>()where T:class,IOutgameFrameModule
        {foreach(var module in Modules)if(module.GetType()==typeof(T))return (T)module;return null;}
        public List<IOutgameFrameModule> GetAllModule()
        {var result=new List<IOutgameFrameModule>();for(var node=Modules.First;node!=null;node=node.Next)result.Add(node.Value);return result;}
        public void Initialize(IOutgameFrameModule module,Action complete,params object[] args)
        {module.Initialized=complete;module.Initialize(args);}
        public void Start()
        {services.SetCulture();foreach(var module in Modules)module.Start();Started=true;}
        public void Update(float delta,float unscaled)
        {if(!Started)return;for(var node=Modules.First;node!=null;node=node.Next)node.Value.Update(delta,unscaled);}
        public void ResourcesModuleUpdate(float delta,float unscaled)
        {if(services.SuppressResourceUpdate()||!Started)return;services.Resources()?.Update(delta,unscaled);}
        public void Shutdown()
        {
            for(var node=Modules.Last;node!=null;node=node.Previous)
            {
                if(node.Value.IsInitialized)node.Value.Shutdown();
                else services.Warning(node.Value.GetType().Name+"模块没有初始化，不进行Shutdown操作");
            }
            Modules.Clear();services.ShutdownInput();services.ClearPermissions();services.SetConfigReadInitialized(false);
            DisposableActions?.Invoke();
            // Original retains Started and the delegate field; subscribers remove themselves.
        }
    }
}
