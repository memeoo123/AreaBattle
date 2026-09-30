using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace AreaBattle
{
    // BuffBase4024. Source Equals compares TargetId's hash with the supplied object's hash,
    // not with another buff's TargetId. CLR identity hashing replaces WASM address hashing.
    public abstract class OutgameBuffBase
    {
        public abstract object TargetId {get;set;}
        public override bool Equals(object other)=>TargetId.GetHashCode()==other.GetHashCode();
        public override int GetHashCode()=>RuntimeHelpers.GetHashCode(this);
        public virtual void Clear(){} // Verified empty source30977.
    }
    // BuffControl4025: all five concrete source methods recovered.
    public sealed class OutgameBuffControl:IOutgameLogicControl
    {
        readonly Func<OutgameMessageDispatcher> messages;readonly OutgameControllerRegistry registry;
        public Dictionary<int,OutgameBuffBase> Buffs=new Dictionary<int,OutgameBuffBase>();
        public OutgameBuffControl(OutgameControllerRegistry registry,Func<OutgameMessageDispatcher> messages)
        {this.registry=registry;this.messages=messages;}
        public void OnInit()=>messages().AddListener("GamePlayState",OnGamePlayState);
        public void OnGamePlayState(object[] args){} // Source30981: nop/end, no argument reads.
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Source30983 empty.
        public void OnDispose()
        {messages().RemoveListener("GamePlayState",OnGamePlayState);Buffs.Clear();registry.Clear(4025);}
    }
}
