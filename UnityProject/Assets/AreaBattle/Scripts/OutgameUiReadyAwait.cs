using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // BaseUI.Await27335/27361 and predicate27340. Transform readiness is distinct from OpenLater.
    public sealed class OutgameUiReadyAwait
    {
        readonly OutgameUiLifetime lifetime;readonly object page;readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<WaitUntil,Task> wait;
        public OutgameUiReadyAwait(OutgameUiLifetime lifetime,object page,Func<OutgameMessageDispatcher> messages,Func<WaitUntil,Task> wait=null)
        {this.lifetime=lifetime;this.page=page;this.messages=messages;this.wait=wait??NativeWait;}
        static async Task NativeWait(WaitUntil instruction){await OutgameUnityAwait.Await(instruction);}
        public Task Await()=>wait(new WaitUntil(IsReady));
        public bool IsReady()
        {
            if(lifetime.Transform!=null){messages().SendMessage("GF_LoadUIOver",new object[]{page});return true;}
            return lifetime.Transform!=null||lifetime.IsDisposed;
        }
    }
}
