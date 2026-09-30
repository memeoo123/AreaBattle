using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Original single-yield ReturnVoid, SimpleCoroutineAwaiter and Unity-context dispatch.
    public static class OutgameUnityAwait
    {
        static SynchronizationContext unityContext;
        static OutgameUnityAwaitRunner runner;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install(){unityContext=SynchronizationContext.Current;runner=null;}
        internal static void Dispatch(Action action)
        {
            if(SynchronizationContext.Current==unityContext)action();
            else unityContext.Post(_=>action(),null);
        }
        public static OutgameSimpleCoroutineAwaiter Await(object instruction)
        {
            var awaiter=new OutgameSimpleCoroutineAwaiter();
            Dispatch(()=>{
                if(runner==null)runner=new GameObject("AsyncCoroutineRunner").AddComponent<OutgameUnityAwaitRunner>();
                runner.StartCoroutine(ReturnVoid(instruction,awaiter));
            });
            return awaiter;
        }
        // IEnumeratorAwaitExtensions22357/CoroutineWrapper22377 specialized to the sealed asset operation.
        // Its Current is always null and has no compiler-generated coroutine parents. Catch MoveNext
        // errors and complete the task immediately; yielding the enumerator directly to Unity loses them.
        public static OutgameSimpleCoroutineAwaiter Await(OutgameLegacyAssetOperation operation)
        {
            var awaiter=new OutgameSimpleCoroutineAwaiter();
            Dispatch(()=>{
                if(runner==null)runner=new GameObject("AsyncCoroutineRunner").AddComponent<OutgameUnityAwaitRunner>();
                runner.StartCoroutine(WaitAssetOperation(operation,awaiter));
            });
            return awaiter;
        }
        static IEnumerator WaitAssetOperation(OutgameLegacyAssetOperation operation,OutgameSimpleCoroutineAwaiter awaiter)
        {
            while(true)
            {
                bool next=false;Exception error=null;
                try{next=operation.MoveNext();}catch(Exception ex){error=ex;}
                if(error!=null){awaiter.Complete(error);yield break;}
                if(!next){awaiter.Complete(null);yield break;}
                yield return operation.Current;
            }
        }
        static IEnumerator ReturnVoid(object instruction,OutgameSimpleCoroutineAwaiter awaiter)
        {yield return instruction;awaiter.Complete(null);}
        public static async Task WaitRealtime(WaitForSecondsRealtime instruction){await Await(instruction);}
    }
    public sealed class OutgameSimpleCoroutineAwaiter:INotifyCompletion
    {
        public bool IsCompleted {get;private set;}
        Exception exception;Action continuation;
        static void Assert(bool value){if(!value)throw new Exception("Assert hit in UnityAsyncUtil package!");}
        public OutgameSimpleCoroutineAwaiter GetAwaiter()=>this;
        public void OnCompleted(Action action)
        {Assert(continuation==null);Assert(!IsCompleted);continuation=action;}
        public void Complete(Exception error)
        {Assert(!IsCompleted);exception=error;IsCompleted=true;if(continuation!=null)OutgameUnityAwait.Dispatch(continuation);}
        public void GetResult(){Assert(IsCompleted);if(exception!=null)ExceptionDispatchInfo.Capture(exception).Throw();}
    }
}
