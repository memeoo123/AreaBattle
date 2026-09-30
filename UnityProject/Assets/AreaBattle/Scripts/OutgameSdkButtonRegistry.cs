using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameVideoButtonState {void SetButtonState(int state);}
    // DBTSDKManager23907/23908/23910/23912, shared with original function-state owner.
    public sealed class OutgameSdkButtonRegistry
    {
        readonly Dictionary<int,List<Button>> buttons=new Dictionary<int,List<Button>>();
        readonly IDictionary<int,UnityAction> actions;
        readonly Func<int,bool,bool> isOpen;
        readonly Action<int,bool> setState;
        readonly Func<bool> isVideoReady;
        readonly Action<int,Button,object[]> platformBind;
        public OutgameSdkButtonRegistry(IDictionary<int,UnityAction> actions,Func<int,bool,bool> isOpen,Action<int,bool> setState,Func<bool> isVideoReady,Action<int,Button,object[]> platformBind)
        {this.actions=actions;this.isOpen=isOpen;this.setState=setState;this.isVideoReady=isVideoReady;this.platformBind=platformBind;}
        public void BindFunctionBtn(int function,Button[] targets,object[] args=null)
        {
            if(!buttons.TryGetValue(function,out var list)){list=new List<Button>();buttons.Add(function,list);}
            foreach(var button in targets)
            {
                if(actions.TryGetValue(function,out var action))button.onClick.AddListener(action);
                list.Add(button);
                button.gameObject.SetActive(isOpen(function,false));
                platformBind?.Invoke(function,button,args);
            }
            CheckNull(function);
        }
        // DBTSDKManager.ReshsdkFunctionBtnState23911: retain dead entries and cached capability semantics.
        public void ReshsdkFunctionBtnState(int function)
        {
            if(!buttons.TryGetValue(function,out var list))return;
            foreach(var button in list)
                if(button!=null)button.gameObject.SetActive(isOpen(function,false));
        }
        public void CheckNull(int function)
        {
            if(!buttons.TryGetValue(function,out var list))return;
            for(int i=list.Count-1;i>=0;i--)if(list[i]==null)list.RemoveAt(i);
        }
        public void CheckVideoIsReady()
        {if(isOpen(5,false))ChangeVideoBtnState(isVideoReady());}
        public void ChangeVideoBtnState(bool ready)
        {
            if(!isOpen(5,false))return;
            setState(15,ready);CheckNull(5);
            if(buttons.TryGetValue(5,out var list))foreach(var button in list)
                if(button is IOutgameVideoButtonState video)video.SetButtonState(ready?2:1);
        }
    }
}
