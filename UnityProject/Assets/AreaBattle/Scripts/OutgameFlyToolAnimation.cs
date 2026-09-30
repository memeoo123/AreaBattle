using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameFlyAnimationHost
    {
        // Target tween: scale to Vector2.one*1.2 (z=0) over .2s, independent update,
        // on complete scale to Vector2.one (z=0) over .2s; paused, autoKill=false, id Target_Tween.
        object CreateTargetTween(Transform target);
        void PlayTargetTween(object tween);
        void Spawn(string assetPath,Action<GameObject> ready);
        void Unspawn(GameObject item);
        Vector2 ScatterPosition(Vector2 origin);
        string NextSubId(int id);
        // Ease uses original DG.Tweening.Ease numeric values:21 OutCirc,20 InCirc.
        void Move(Transform item,Vector3 destination,float duration,int ease,string id,bool independentUpdate,Action completed);
        void StopEffect(int id);
        void KillTween(string id,bool complete);
    }
    // Original EffectControl coroutine31241 and closures31235/31237/31238.
    public sealed class OutgameFlyToolAnimation
    {
        readonly IOutgameFlyAnimationHost host;readonly Func<int,int> count;
        // Source EffectControl owns and shares both WaitForSecondsRealtime instances.
        readonly WaitForSecondsRealtime spawnWait=new WaitForSecondsRealtime(.025f),finishWait=new WaitForSecondsRealtime(1.1f);
        public OutgameFlyToolAnimation(IOutgameFlyAnimationHost host,Func<int,int> currentInventory){this.host=host;count=currentInventory;}
        public IEnumerator Run(OutgameFlyToolRequest request)
        {
            var text=request.Target.GetComponent<Text>();var target=text.transform;
            object targetTween=null;
            var collection=new OutgameFlyToolCollection(request,text.text,count,v=>text.text=v.ToString(),()=>host.PlayTargetTween(targetTween));
            if(collection.AssetPath==null)yield break;
            targetTween=host.CreateTargetTween(target);
            for(int i=0;i<collection.CreateCount;i++)
            {
                host.Spawn(collection.AssetPath,item=>{
                    item.transform.SetParent(request.Root);item.transform.position=request.Position;item.transform.localScale=Vector3.one;
                    Vector2 scattered=host.ScatterPosition(request.Position);
                    host.Move(item.transform,new Vector3(scattered.x,scattered.y,0),.4f,21,host.NextSubId(request.Id),true,()=>{
                        host.Move(item.transform,target.position,.5f,20,host.NextSubId(request.Id),true,
                            ()=>collection.Arrived(()=>host.Unspawn(item)));
                    });
                });
                yield return spawnWait;
            }
            yield return finishWait;
            host.StopEffect(request.Id);host.KillTween("Target_Tween",false);
        }
    }
}
