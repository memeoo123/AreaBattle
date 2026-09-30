using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // DBTSDKManager23913/23914 and SDKExtension.IsOpen: second argument is refresh, NOT fallback.
    public sealed class OutgameSdkFunctionStates
    {
        readonly Dictionary<int,bool> states=new Dictionary<int,bool>();
        readonly Func<int,bool> resolve;
        readonly Func<int,string> name;
        readonly Action<string> trace;
        public OutgameSdkFunctionStates(Func<int,bool> resolve,Func<int,string> name,Action<string> trace)
        {this.resolve=resolve;this.name=name;this.trace=trace;}
        public bool IsOpen(int function,bool refresh=false)
        {
            trace(name(function)+"==IsRefresh=="+refresh);
            if(!states.TryGetValue(function,out bool value)||refresh)
            {value=resolve(function);Set(function,value);}
            trace(name(function)+"==Get=="+value);
            return value;
        }
        public void Set(int function,bool value)
        {
            bool exists=states.ContainsKey(function);
            trace(name(function)+(exists?"======":"Add======")+value);
            if(exists)states[function]=value;else states.Add(function,value);
        }
    }
}
