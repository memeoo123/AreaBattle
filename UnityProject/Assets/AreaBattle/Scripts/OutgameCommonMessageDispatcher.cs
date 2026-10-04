using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // CommonMsgDispatcher4569 owns a different singleton/listener map from MsgDispatcher3485.
    public sealed class OutgameCommonMessageDispatcher
    {
        static OutgameCommonMessageDispatcher shared;
        public static OutgameCommonMessageDispatcher Shared=>shared??(shared=new OutgameCommonMessageDispatcher());
        public static void ClearEvent(){var current=Shared;shared=new OutgameCommonMessageDispatcher();}
        readonly Dictionary<string,Action<object[]>> listeners=new Dictionary<string,Action<object[]>>();
        public void AddListener(string key,Action<object[]> callback)
        {if(listeners.ContainsKey(key))listeners[key]+=callback;else listeners.Add(key,callback);}
        public void RemoveListener(string key,Action<object[]> callback)
        {
            if(!listeners.ContainsKey(key))return;
            listeners[key]-=callback;if(listeners[key]==null)listeners.Remove(key);
        }
        public void SendMessage(string key,object[] args=null)
        {if(listeners.ContainsKey(key))listeners[key](args);}
        public void SendMessageGetKey(string key,object[] args)
        {
            if(!listeners.ContainsKey(key))return;
            var expanded=new object[args.Length+1];
            for(int i=0;i<args.Length;i++)expanded[i]=args[i];
            expanded[expanded.Length-1]=key;listeners[key](expanded);
        }
    }
}
