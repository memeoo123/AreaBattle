using System;
using UnityEngine;
namespace AreaBattle
{
    // MineGameMain30604. The global flag suppresses only GamePause dispatch, not persistence.
    public sealed class OutgameMainFocus
    {
        readonly OutgameMainLifecycleState state;
        readonly Func<bool> globalFlag28,hasPlayUi,pvpFlag8;
        readonly Func<int> playState;
        readonly Action<object[]> showPause;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly OutgameUserPreferences preferences;
        readonly OutgameDataManagerPool data;
        readonly Action<string> log;
        readonly Action<Color,string> colorLog;
        public OutgameMainFocus(OutgameMainLifecycleState state,Func<bool> globalFlag28,Func<bool> hasPlayUi,Func<int> playState,Func<bool> pvpFlag8,Action<object[]> showPause,Func<OutgameMessageDispatcher> messages,OutgameUserPreferences preferences,OutgameDataManagerPool data,Action<string> log,Action<Color,string> colorLog)
        {this.state=state;this.globalFlag28=globalFlag28;this.hasPlayUi=hasPlayUi;this.playState=playState;this.pvpFlag8=pvpFlag8;this.showPause=showPause;this.messages=messages;this.preferences=preferences;this.data=data;this.log=log;this.colorLog=colorLog;}
        public void OnApplicationFocus(bool focus)
        {
            log("OnApplicationFocus："+focus);
            if(!state.EnterGame)return;
            if(!globalFlag28())messages().SendMessage("GamePause",new object[]{!focus});
            if(focus)return;
            if(hasPlayUi()&&playState()==6&&!pvpFlag8())showPause(Array.Empty<object>());
            colorLog(Color.red,"游戏失去焦点，开始上传数据");
            preferences.OnSave();data.SaveData();log("数据存储完成");
        }
    }
}
