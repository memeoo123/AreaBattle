using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameEffectData
    {
        public int id,type;public string res;public double duration;
    }
    public sealed class OutgameEffectModuleServices
    {
        public Func<bool> HasConfigResource,UseNewResources;
        public Func<List<OutgameEffectData>> ReadConfig;
        public Func<Transform> CanvasRoot,UiRoot;
        public Func<Camera> UiCamera;
        public Action<string> Log,Error;
        public Action<GameObject> Destroy=UnityEngine.Object.Destroy;
        public Action<UnityEngine.Object> Persist=UnityEngine.Object.DontDestroyOnLoad;
        public OutgameEffectServices Effects;
    }
    // EffectModule3483/26796..26816 and shared CreateEffect26809.
    public sealed class OutgameEffectModule:IOutgameCollectionEffectModule,IOutgameStartupModule
    {
        readonly OutgameEffectModuleServices services;
        public readonly Dictionary<int,OutgameBaseEffect> Effects=new Dictionary<int,OutgameBaseEffect>();
        public Dictionary<int,OutgameEffectData> Configs {get;private set;}
        public bool AlreadySetData {get;private set;}
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public int Priority=>20;
        Transform worldRoot,uiRoot;RectTransform lineRoot;
        public OutgameEffectModule(OutgameEffectModuleServices services){this.services=services;}
        public void Initialize(){Configs=new Dictionary<int,OutgameEffectData>();IsInitialized=true;Initialized?.Invoke();}
        public void Start(){}
        public void Update(float elapsed,float realElapsed){}
        public void ReadEffectData()
        {
            if(AlreadySetData)return;
            if(!services.HasConfigResource()&&!services.UseNewResources()){services.Error("Effect:配置文件AB资源为null");return;}
            services.Log("Effect:设置特效数据");var rows=services.ReadConfig();
            if(rows!=null)for(int i=0;i<rows.Count;i++){
                if(Configs.ContainsKey(rows[i].id))services.Error(string.Format("表[EffectConfig]中有相同键({0})",rows[i].id));
                else Configs.Add(rows[i].id,rows[i]);
            }
            AlreadySetData=true;
        }
        public OutgameEffectData GetEffectDataById(int id)
        {if(!Configs.TryGetValue(id,out var row))services.Error(string.Format("未找到ID[{0}]为的特效",id));return row;}
        public Transform WorldEffectRoot
        {
            get{if(worldRoot==null){worldRoot=new GameObject("__WorldEffectRoot").transform;services.Persist(worldRoot);}return worldRoot;}
        }
        public Transform UIEffectRoot
        {
            get{if(uiRoot==null){uiRoot=new GameObject("_UIEffectRoot").transform;uiRoot.SetParent(services.CanvasRoot(),false);}return uiRoot;}
        }
        public RectTransform LineEffectRoot
        {
            get{
                if(lineRoot==null){var go=new GameObject("_LineEffectRoot");go.transform.SetParent(services.UiRoot(),false);lineRoot=go.AddComponent<RectTransform>();
                    var canvas=lineRoot.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=100;canvas.sortingLayerID=SortingLayer.NameToID("Top");}
                return lineRoot;
            }
        }
        public Vector2 WorldToRectLocalPoint(Vector3 position)
        {
            var screen=RectTransformUtility.WorldToScreenPoint(services.UiCamera(),position);var point=Vector2.zero;
            // Source uses the backing field here, without creating LineEffectRoot.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineRoot,screen,services.UiCamera(),out point);return point;
        }
        public T CreateEffect<T>(int id,Vector3 position,Vector3 rotation,Vector3 targetPosition,float flyTime,Transform parent,int sortingLayer=-1,int order=-1)where T:OutgameBaseEffect
        {
            ReadEffectData();var config=GetEffectDataById(id);OutgameBaseEffect effect;bool setComponents=order!=-9999;
            switch(config.type){
                case 1:
                    if(parent==null)parent=UIEffectRoot;
                    effect=new OutgameFlyEffect(config,position,rotation,targetPosition,flyTime,parent,setComponents,services.Effects);break;
                case 2:
                    var origin=parent.position;parent=LineEffectRoot;
                    effect=new OutgameLineEffect(config,position,rotation,origin,targetPosition,parent,setComponents,services.Effects,WorldToRectLocalPoint);break;
                case 3:
                    if(parent==null)parent=WorldEffectRoot;
                    effect=new OutgameBaseEffect(config,position,rotation,parent,setComponents,services.Effects);break;
                default:
                    if(parent==null)parent=UIEffectRoot;
                    effect=new OutgameUIEffect(config,position,rotation,parent,setComponents,services.Effects);break;
            }
            if(order!=-9999&&order!=-1)effect.SetOrder(sortingLayer==-1?"Top":SortingLayer.IDToName(sortingLayer),order);
            return (T)effect;
        }
        public int ShowEffectWithId(int id,Vector3 position,Vector3 rotation,Vector3 targetPosition,float flyTime,Transform parent,int sortingLayer=-1,int order=-1)
        {
            var effect=CreateEffect<OutgameBaseEffect>(id,position,rotation,targetPosition,flyTime,parent,sortingLayer,order);
            Effects.Add(effect.UID,effect);
            // Source checks captured UID but removes the callback argument's UID.
            effect.OnComplete=completed=>{if(Effects.ContainsKey(effect.UID))Effects.Remove(completed.UID);};return effect.UID;
        }
        public int Show(int id,Transform parent)=>ShowEffectWithId(id,Vector3.zero,Vector3.zero,Vector3.zero,0,parent,-1,-1);
        public int Show(int id,Vector3 position,Transform parent,int sortingLayer,int order)=>ShowEffectWithId(id,position,Vector3.zero,Vector3.zero,0,parent,sortingLayer,order);
        public OutgameBaseEffect Get(int handle){Effects.TryGetValue(handle,out var effect);return effect;}
        IOutgameCollectionEffect IOutgameCollectionEffectModule.Get(int handle)=>Get(handle);
        public void Close(int handle){if(Effects.TryGetValue(handle,out var effect)){Effects.Remove(handle);effect.Dispose();}}
        public void ClearChildObj(){foreach(var effect in Effects.Values)effect.Dispose();Effects?.Clear();Configs?.Clear();}
        public void Shutdown()
        {
            ClearChildObj();
            if(worldRoot!=null){services.Destroy(worldRoot.gameObject);worldRoot=null;}
            if(uiRoot!=null){services.Destroy(uiRoot.gameObject);uiRoot=null;}
            if(lineRoot!=null){services.Destroy(lineRoot.gameObject);lineRoot=null;}
        }
    }
}
