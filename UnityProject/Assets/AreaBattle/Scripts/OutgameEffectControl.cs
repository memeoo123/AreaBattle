using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameEffectControlServices
    {
        public Func<IOutgameSingleSpawnPoolFactory> PoolManager;
        public IOutgameNormalPoolResources Resources;
        public IOutgameNormalPoolLifetime PoolLifetime;
        public Func<OutgameToolDispatcher> Tools;
        public Func<int,int> InventoryCount;
        public Func<Text> GoldText,DiamondsText;
        public Func<string,Transform> UiNode;
        public Action<int,int> Voice;
        public Func<int> DiamondVoice;
        public Func<IEnumerator,object> StartCoroutine=routine=>OutgameUpdateManager.Instance.StartCoroutine(routine);
        public Action<object> StopCoroutine=coroutine=>OutgameUpdateManager.Instance.StopCoroutine((Coroutine)coroutine);
        public Func<float> Time=()=>UnityEngine.Time.time;
        public Func<Transform,object> CreateTargetTween;
        public Action<object> PlayTargetTween;
        public Action<Transform,Vector3,float,int,string,bool,Action> Move;
        public Action<string,bool> KillTween;
        public OutgameFlyScatter Scatter=new OutgameFlyScatter();
        public void BindUi(Func<OutgameUiControl> ui,Func<string,Transform> node)
        {GoldText=()=>ui().GoldText;DiamondsText=()=>ui().DiamondsText;UiNode=node;}
        public void BindNativeTweens(OutgameFlyTweenRunner runner)
        {
            CreateTargetTween=target=>runner.CreateTargetTween(target);
            PlayTargetTween=tween=>runner.PlayForward((OutgameFlyTweenRunner.Handle)tween);
            Move=(target,position,duration,ease,id,independent,complete)=>runner.Move(target,position,duration,ease,id,independent,complete);KillTween=runner.Kill;
        }
    }
    // Original4058 owns the pool, two realtime waits (animation), tracking and deferred cleanup.
    public sealed class OutgameEffectControl:IOutgameLogicControl,IOutgameFlyAnimationHost,IOutgameFlyCleanupHost,IOutgameShopCurrencyEffects
    {
        readonly OutgameControllerRegistry registry;readonly OutgameEffectControlServices services;
        readonly OutgameFlyToolCleanup cleanup;readonly OutgameFlyToolAnimation animation;
        readonly string fallbackNode=OutgameUiLayerNames.Get(6);
        public OutgameNormalPool Pool {get;private set;}
        public int ActiveEffectCount=>cleanup.ActiveCount;
        public int PendingCleanupCount=>cleanup.PendingCount;
        public float NextCleanupCheck=>cleanup.NextCheck;
        public OutgameEffectControl(OutgameControllerRegistry registry,OutgameEffectControlServices services)
        {
            this.registry=registry;this.services=services;cleanup=new OutgameFlyToolCleanup(this,false);
            animation=new OutgameFlyToolAnimation(this,item=>services.InventoryCount(item));
        }
        public void OnInit()
        {
            cleanup.Initialize();
            if(Pool==null){Pool=new OutgameNormalPool(services.Resources);Pool.CreateObjectPool(services.PoolManager,"moneyPool",300,5f,null,false);}
        }
        public void Updata(float deltaTime,float unscaledDeltaTime)=>cleanup.Update();
        public void DestoryPool()=>Pool.DestroyObjectPool(services.PoolLifetime);
        public void OnDispose(){DestoryPool();registry.Clear(4058);}
        public int FlyMoney(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {services.Voice(1,2019);return NewFlyTool(1001,services.GoldText(),amount,root,position,applyInventory,completion,updateDisplayedValue);}
        public int FlyDiamonds(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {services.Voice(1,services.DiamondVoice());return NewFlyTool(1002,services.DiamondsText(),amount,root,position,applyInventory,completion,updateDisplayedValue);}
        void IOutgameShopCurrencyEffects.FlyMoney(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)=>FlyMoney(amount,root,position,applyInventory,completion,updateDisplayedValue);
        void IOutgameShopCurrencyEffects.FlyDiamonds(int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)=>FlyDiamonds(amount,root,position,applyInventory,completion,updateDisplayedValue);
        public int NewFlyTool(int item,Text target,int amount,Transform root,Vector3 position,bool applyInventory,Action completion,bool updateDisplayedValue)
        {
            if(target==null)return -1;
            int id=OutgameFlyEffectIds.Shared.NextId();if(root==null)root=services.UiNode(fallbackNode);
            var request=new OutgameFlyToolRequest{Id=id,ItemId=item,Target=target.transform,Amount=amount,Root=root,Position=position,ApplyInventory=applyInventory,Completion=completion,UpdateDisplayedValue=updateDisplayedValue};
            if(request.ApplyInventory)services.Tools().Change(item,amount,false,"",true);
            var coroutine=services.StartCoroutine(animation.Run(request));cleanup.TrackCoroutine(id,coroutine);cleanup.TrackTime(id,services.Time());return id;
        }
        public void PlaySequenceEffect(int[] ids,int[] counts,Action completed,Func<int,Vector3> onceFinish)
        {new OutgameSequenceEffect(()=>((OutgameEffectControl)registry.Resolve(4058)),id=>services.Tools().GoodsType(id),ids,counts,completed,onceFinish).Play();}
        public void StopEffect(int id)=>cleanup.StopEffect(id);
        float IOutgameFlyCleanupHost.Time=>services.Time();
        public int SubIdCount(int id)=>OutgameFlyEffectIds.Shared.SubIdCount(id);
        public void RemoveSubIds(int id)=>OutgameFlyEffectIds.Shared.RemoveSubIds(id);
        void IOutgameFlyCleanupHost.StopCoroutine(object coroutine)=>services.StopCoroutine(coroutine);
        public object CreateTargetTween(Transform target)=>services.CreateTargetTween(target);
        public void PlayTargetTween(object tween)=>services.PlayTargetTween(tween);
        public void Spawn(string path,Action<GameObject> ready)=>Pool.Spawn(path,ready);
        public void Unspawn(GameObject item)=>Pool.Unspawn(item);
        public Vector2 ScatterPosition(Vector2 origin)=>services.Scatter.Position(origin);
        public string NextSubId(int id)=>OutgameFlyEffectIds.Shared.NextSubId(id);
        public void Move(Transform item,Vector3 destination,float duration,int ease,string id,bool independent,Action completed)=>services.Move(item,destination,duration,ease,id,independent,completed);
        public void KillTween(string id,bool complete)=>services.KillTween(id,complete);
    }
    // Original nested4052. No skip for unsupported goods and no optional position fallback.
    public sealed class OutgameSequenceEffect
    {
        readonly Func<OutgameEffectControl> control;readonly Func<int,int> goodsType;
        readonly int[] ids,counts;readonly Action completed;readonly Func<int,Vector3> onceFinish;
        public int Index {get;private set;}
        public Vector3 Position {get;private set;}
        public OutgameSequenceEffect(Func<OutgameEffectControl> control,Func<int,int> goodsType,int[] ids,int[] counts,Action completed,Func<int,Vector3> onceFinish)
        {this.control=control;this.goodsType=goodsType;this.ids=ids;this.counts=counts;this.completed=completed;this.onceFinish=onceFinish;}
        public void Play()
        {
            if(Index>=ids.Length){Index=0;completed?.Invoke();return;}
            Vector3? position=onceFinish?.Invoke(Index);Position=position.Value;
            int kind=goodsType(ids[Index]);int amount=counts[Index];
            if(kind==1)control().FlyMoney(amount,null,Position,false,Play,true);
            else if(kind==2)control().FlyDiamonds(amount,null,Position,false,Play,true);
            Index=unchecked(Index+1);
        }
    }
}
