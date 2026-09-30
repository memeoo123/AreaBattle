using System;
using UnityEngine;
namespace AreaBattle
{
    // MineGameMain30618 ExitGame listener. Unity destruction is deferred by default.
    public sealed class OutgameMainExit
    {
        readonly Action<string> log;
        readonly Action destroyAudio,clearIapProcedure;
        readonly Action<bool> setExitFlag,setEnterGame;
        readonly Func<GameObject> mainObject,updateObject;
        readonly OutgameUserPreferences preferences;
        readonly Action<GameObject> destroy;
        public OutgameMainExit(Action<string> log,Action destroyAudio,Action<bool> setExitFlag,Action clearIapProcedure,Func<GameObject> mainObject,Func<GameObject> updateObject,OutgameUserPreferences preferences,Action<bool> setEnterGame,Action<GameObject> destroy=null)
        {this.log=log;this.destroyAudio=destroyAudio;this.setExitFlag=setExitFlag;this.clearIapProcedure=clearIapProcedure;this.mainObject=mainObject;this.updateObject=updateObject;this.preferences=preferences;this.setEnterGame=setEnterGame;this.destroy=destroy??(value=>UnityEngine.Object.Destroy(value));}
        public void OnExitGame(object[] args)
        {
            log("客户端逻辑退出完成，开始释放资源");destroyAudio();setExitFlag(true);clearIapProcedure();
            destroy(mainObject());destroy(updateObject());preferences.OnSave();
            OutgameMessageDispatcher.ClearEvent();setEnterGame(false);
        }
    }
}
