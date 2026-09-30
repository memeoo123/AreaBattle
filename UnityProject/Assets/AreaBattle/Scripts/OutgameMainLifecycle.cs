using System;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameMainLifecycleState
    {
        public bool ModulesReady; // MineGameMain instance24
        public bool ExitRequested; // instance37
        public bool ResourcesOnly; // instance38
        public bool EnterGame; // original static0, shared by startup/focus/exit composition
    }
    // MineGameMain30624 and30620. Resource initialization updates are outside the catch.
    public sealed class OutgameMainLifecycle
    {
        readonly OutgameMainLifecycleState state;
        readonly Action<float,float> resourcesUpdate,frameUpdate;
        readonly Func<float> delta,unscaledDelta;
        readonly Action<Exception> warning;
        readonly OutgameUserPreferences preferences;
        readonly OutgameDataManagerPool data;
        readonly Action<string> log;
        public OutgameMainLifecycle(OutgameMainLifecycleState state,Action<float,float> resourcesUpdate,Action<float,float> frameUpdate,Action<Exception> warning,OutgameUserPreferences preferences,OutgameDataManagerPool data,Action<string> log,Func<float> delta=null,Func<float> unscaledDelta=null)
        {this.state=state;this.resourcesUpdate=resourcesUpdate;this.frameUpdate=frameUpdate;this.warning=warning;this.preferences=preferences;this.data=data;this.log=log;this.delta=delta??(()=>Time.deltaTime);this.unscaledDelta=unscaledDelta??(()=>Time.unscaledDeltaTime);}
        public void Update()
        {
            if(state.ExitRequested)return;
            if(state.ResourcesOnly)resourcesUpdate(delta(),unscaledDelta());
            if(!state.ModulesReady)return;
            try{frameUpdate(delta(),unscaledDelta());}catch(Exception exception){warning(exception);}
        }
        public void OnApplicationQuit()
        {preferences.OnSave();data.SaveData();log("游戏退出，数据存储完成");}
    }
}
