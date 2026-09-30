using System;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameFlyToolRequest
    {
        public int Id,ItemId,Amount;
        public Action Completion;
        public Transform Target,Root;
        public Vector3 Position;
        public bool ApplyInventory,UpdateDisplayedValue;
    }
    public interface IOutgameFlyToolHost
    {
        int NextId();
        Transform DefaultRoot();
        object StartAnimation(OutgameFlyToolRequest request);
        void TrackCoroutine(int id,object coroutine);
        float Time {get;}
        void TrackTime(int id,float time);
    }
    // EffectControl.NewFlyTool31217. Animation coroutine remains a separate source contract.
    public sealed class OutgameFlyToolStart
    {
        readonly OutgameToolDispatcher tools;readonly IOutgameFlyToolHost host;
        public OutgameFlyToolStart(OutgameToolDispatcher tools,IOutgameFlyToolHost host){this.tools=tools;this.host=host;}
        public int Begin(int item,Transform target,int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {
            if(target==null)return -1;
            int id=host.NextId();if(root==null)root=host.DefaultRoot();
            var request=new OutgameFlyToolRequest{Id=id,ItemId=item,Target=target,Amount=amount,Root=root,Position=position,ApplyInventory=applyInventory,Completion=completion,UpdateDisplayedValue=updateDisplayedValue};
            if(request.ApplyInventory)tools.Change(item,amount,false,"",true);
            var coroutine=host.StartAnimation(request);host.TrackCoroutine(id,coroutine);host.TrackTime(id,host.Time);return id;
        }
    }
}
