using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // MsgDispatcher3485 string event path26822/26828/26830; other indexed event APIs pending.
    public sealed class OutgameMessageDispatcher
    {
        static OutgameMessageDispatcher shared;
        public static OutgameMessageDispatcher Shared=>shared??(shared=new OutgameMessageDispatcher());
        public static void ClearEvent(){var current=Shared;shared=new OutgameMessageDispatcher();}
        readonly Dictionary<string,Action<object[]>> listeners=new Dictionary<string,Action<object[]>>();
        public void AddListener(string name,Action<object[]> callback)
        {
            if(listeners.ContainsKey(name))listeners[name]+=callback;
            else listeners.Add(name,callback);
        }
        public void RemoveListener(string name,Action<object[]> callback)
        {
            if(!listeners.ContainsKey(name))return;
            listeners[name]-=callback;if(listeners[name]==null)listeners.Remove(name);
        }
        public void SendMessage(string name,object[] args=null)
        {
            if(listeners.ContainsKey(name))listeners[name](args);
        }
    }
    public sealed class OutgameShopEventHost:IOutgameShopEventHost
    {
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Action<Action<bool>> gold,diamonds;
        readonly Action<object> stop;readonly Action selectCard;
        public OutgameShopEventHost(Action<Action<bool>> gold,Action<Action<bool>> diamonds,Action<object> stop,Action selectCard,Func<OutgameMessageDispatcher> messages=null)
        {this.gold=gold;this.diamonds=diamonds;this.stop=stop;this.selectCard=selectCard;this.messages=messages??(()=>OutgameMessageDispatcher.Shared);}
        public void AddListener(string name,Action<object[]> callback)=>messages().AddListener(name,callback);
        public void RemoveListener(string name,Action<object[]> callback)=>messages().RemoveListener(name,callback);
        public void AddGoldVideoCallback(Action<bool> callback)=>gold(callback);
        public void AddDiamondVideoCallback(Action<bool> callback)=>diamonds(callback);
        public void StopInitializationCoroutine(object coroutine)=>stop(coroutine);
        public void EnsureSelectCardControl()=>selectCard();
    }
}
