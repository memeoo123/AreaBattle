using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Composition of the recovered economic entry, animation coroutine, native tween runner and cleanup.
    // Original pool and audio providers are required; they are never simulated as successful here.
    public sealed class OutgameFlyRuntime:MonoBehaviour,IOutgameFlyToolHost,IOutgameFlyAnimationHost,IOutgameFlyCleanupHost,IOutgameShopCurrencyEffects
    {
        OutgameFlyToolStart start;OutgameFlyToolAnimation animation;OutgameFlyToolCleanup cleanup;OutgameFlyTweenRunner tweens;
        OutgameFlyScatter scatter;Text gold,diamonds;Transform fallbackRoot;Action<int,int> voice;Func<int> diamondVoice;
        Action<string,Action<GameObject>> spawn;Action<GameObject> unspawn;
        public void Bind(OutgameToolDispatcher tools,OutgameLocalInventory inventory,Text gold,Text diamonds,Transform fallbackRoot,
            Action<int,int> voice,Func<int> diamondVoice,Action<string,Action<GameObject>> spawn,Action<GameObject> unspawn,OutgameFlyScatter scatter=null)
        {
            this.gold=gold;this.diamonds=diamonds;this.fallbackRoot=fallbackRoot;
            this.voice=voice??throw new ArgumentNullException(nameof(voice));this.diamondVoice=diamondVoice??throw new ArgumentNullException(nameof(diamondVoice));
            this.spawn=spawn??throw new ArgumentNullException(nameof(spawn));this.unspawn=unspawn??throw new ArgumentNullException(nameof(unspawn));this.scatter=scatter??new OutgameFlyScatter();
            tweens=GetComponent<OutgameFlyTweenRunner>()??gameObject.AddComponent<OutgameFlyTweenRunner>();
            cleanup=new OutgameFlyToolCleanup(this);animation=new OutgameFlyToolAnimation(this,inventory.Count);start=new OutgameFlyToolStart(tools,this);
        }
        public void FlyMoney(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {voice(1,2019);start.Begin(1001,gold==null?null:gold.transform,amount,root,position,applyInventory,completion,updateDisplayedValue);}
        public void FlyDiamonds(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {voice(1,diamondVoice());start.Begin(1002,diamonds==null?null:diamonds.transform,amount,root,position,applyInventory,completion,updateDisplayedValue);}
        public int ActiveEffectCount=>cleanup==null?0:cleanup.ActiveCount;
        public int PendingCleanupCount=>cleanup==null?0:cleanup.PendingCount;
        public int NextId()=>OutgameFlyEffectIds.Shared.NextId();
        public Transform DefaultRoot()=>fallbackRoot;
        public object StartAnimation(OutgameFlyToolRequest request)=>StartCoroutine(animation.Run(request));
        public void TrackCoroutine(int id,object coroutine)=>cleanup.TrackCoroutine(id,coroutine);
        public float Time=>UnityEngine.Time.time;
        public void TrackTime(int id,float time)=>cleanup.TrackTime(id,time);
        public object CreateTargetTween(Transform target)=>tweens.CreateTargetTween(target);
        public void PlayTargetTween(object tween)=>tweens.PlayForward((OutgameFlyTweenRunner.Handle)tween);
        public void Spawn(string path,Action<GameObject> ready)=>spawn(path,ready);
        public void Unspawn(GameObject item)=>unspawn(item);
        public Vector2 ScatterPosition(Vector2 origin)=>scatter.Position(origin);
        public string NextSubId(int id)=>OutgameFlyEffectIds.Shared.NextSubId(id);
        public void Move(Transform item,Vector3 destination,float duration,int ease,string id,bool independent,Action completed)=>tweens.Move(item,destination,duration,ease,id,independent,completed);
        public void StopEffect(int id)=>cleanup.StopEffect(id);
        public void KillTween(string id,bool complete)=>tweens.Kill(id,complete);
        public int SubIdCount(int id)=>OutgameFlyEffectIds.Shared.SubIdCount(id);
        public void RemoveSubIds(int id)=>OutgameFlyEffectIds.Shared.RemoveSubIds(id);
        void IOutgameFlyCleanupHost.StopCoroutine(object coroutine)=>StopCoroutine((Coroutine)coroutine);
        void Update(){if(cleanup!=null)cleanup.Update();}
    }
}
