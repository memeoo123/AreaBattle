using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameLogicControl
    {
        void OnInit();
        void Updata(float deltaTime,float unscaledDeltaTime);
        void OnDispose();
    }
    // MineGameLogicModule26939..26946. Live indexed initialization, enumerated update/disposal.
    public sealed class OutgameLogicModule:IOutgameStartupModule
    {
        List<IOutgameLogicControl> controls;
        readonly Action<bool> setAutoRegister;readonly Action initializeDataPool,releaseDataPool;
        readonly Action<string> warning,log;
        public int Priority=>12;
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public int ControllerCount=>controls.Count;
        public OutgameLogicModule(Action<bool> setAutoRegister,Action initializeDataPool,Action releaseDataPool,Action<string> warning,Action<string> log)
        {this.setAutoRegister=setAutoRegister;this.initializeDataPool=initializeDataPool;this.releaseDataPool=releaseDataPool;this.warning=warning;this.log=log;}
        public void Initialize(){controls=new List<IOutgameLogicControl>();IsInitialized=true;Initialized?.Invoke();}
        public void RegisterLogicCtr(IOutgameLogicControl control,bool initializeNow)
        {
            if(controls.Contains(control)){warning("逻辑控制类["+control.GetType().Name+"]已经被注册");return;}
            controls.Add(control);if(initializeNow)control.OnInit();
        }
        public void InitCtrl(bool autoRegister)
        {
            setAutoRegister(autoRegister);initializeDataPool();
            if(!autoRegister){log("框架急速模式启动");return;}
            for(int i=0;i<controls.Count;i++)controls[i].OnInit();
        }
        public void Update(float deltaTime,float unscaledDeltaTime)
        {foreach(var control in controls)control.Updata(deltaTime,unscaledDeltaTime);}
        public void Shutdown(){releaseDataPool();foreach(var control in controls)control.OnDispose();controls.Clear();}
        public void Start(){}
    }
}
