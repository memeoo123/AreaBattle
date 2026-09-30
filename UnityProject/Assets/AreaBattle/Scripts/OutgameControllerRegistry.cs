using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Reference-type projection of ControlBase<T>.get_I26934. Each source type owns its slot.
    // Factories must be concrete; unresolved types fail instead of registering empty controls.
    public sealed class OutgameControllerRegistry
    {
        readonly Dictionary<int,Func<IOutgameLogicControl>> factories=new Dictionary<int,Func<IOutgameLogicControl>>();
        readonly Dictionary<int,IOutgameLogicControl> instances=new Dictionary<int,IOutgameLogicControl>();
        public void Bind(int sourceType,Func<IOutgameLogicControl> factory)=>factories.Add(sourceType,factory??throw new ArgumentNullException(nameof(factory)));
        public IOutgameLogicControl Resolve(int sourceType)
        {
            if(instances.TryGetValue(sourceType,out var instance)&&instance!=null)return instance;
            instance=factories[sourceType]();
            if(instance==null)throw new InvalidOperationException("Controller factory returned null: "+sourceType);
            instances[sourceType]=instance;return instance;
        }
        public bool HasInstance(int sourceType)=>instances.TryGetValue(sourceType,out var value)&&value!=null;
        // Source disposal clears the static slot unconditionally, even from an older instance.
        public void Clear(int sourceType)=>instances.Remove(sourceType);
    }
    // CommanderControl30993/30998/31005. Business actions remain in recovered commander services.
    public sealed class OutgameCommanderControl:IOutgameLogicControl
    {
        readonly Func<OutgameDataManagerPool> pool;readonly OutgameControllerRegistry registry;
        public OutgameCommanderManager Manager {get;private set;}
        public OutgameCommanderControl(Func<OutgameDataManagerPool> pool,OutgameControllerRegistry registry)
        {this.pool=pool;this.registry=registry;}
        public void OnInit()=>Manager=pool().GetModel<OutgameCommanderManager>(4028,"CommanderManager");
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Verified original empty body30998.
        public void OnDispose()=>registry.Clear(4027); // Source retains instance field8.
    }
}
