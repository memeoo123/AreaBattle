using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameEffectServices
    {
        public Func<bool> UseNewResources;
        public Func<string,Task<OutgameAssetHandle>> LoadNewPrefab;
        public Func<string,Task<GameObject>> LoadLegacyPrefab;
        public Func<object,Task> Wait=WaitNative;
        public Action<GameObject> Destroy=UnityEngine.Object.Destroy;
        public Action<Transform,Vector3,float,Action> Move;
        static async Task WaitNative(object instruction){await OutgameUnityAwait.Await(instruction);}
    }
    // BaseEffect3474, including the four async state machines3470..3473.
    public class OutgameBaseEffect:IOutgameCollectionEffect
    {
        static readonly object idLock=new object();static int nextId;
        protected readonly OutgameEffectServices Services;
        public int UID;public OutgameEffectData Config;public GameObject GameObject;
        public Transform Parent;public Vector3 Position,Rotation;
        public bool IsActive=true,IsLoaded=true,IsDisposed;
        public Action<OutgameBaseEffect> OnComplete;
        public bool SetComponents=true,PendingOrder;
        public int Order=-1;public string SortingLayerName=string.Empty;
        public OutgameAssetHandle Handle {get;private set;}
        public OutgameBaseEffect(Transform parent,OutgameEffectServices services){Parent=parent;Services=services;}
        public OutgameBaseEffect(OutgameEffectData config,Vector3 position,Vector3 rotation,Transform parent,bool setComponents,OutgameEffectServices services):this(parent,services)
        {
            lock(idLock){UID=unchecked(++nextId);}
            Config=config;Position=position;Rotation=rotation;SetComponents=setComponents;Start();
        }
        public void SetActive(bool active){if(GameObject!=null)GameObject.SetActive(active);IsActive=active;}
        public void SetOrder(string layer,int order)
        {
            SortingLayerName=layer;Order=order;bool pending=true;
            if(GameObject!=null){
                foreach(var renderer in GameObject.GetComponentsInChildren<Renderer>()){renderer.sortingLayerName=SortingLayerName;renderer.sortingOrder=Order;}
                pending=false;
                foreach(var canvas in GameObject.GetComponentsInChildren<Canvas>()){canvas.sortingLayerName=SortingLayerName;canvas.sortingOrder+=Order;}
            }
            PendingOrder=pending;
        }
        public string GetEffectFolder(){switch(Config.type){case 0:return "UI/";case 1:return "Fly/";case 2:return "Line/";case 3:return "Scene/";default:return string.Empty;}}
        public void Stop()=>Dispose();
        public virtual Task Await()=>Services.Wait(new WaitUntil(()=>IsLoaded||IsDisposed));
        public virtual void SetComponent()
        {
            if(!SetComponents||GameObject==null)return;
            if(Order==-1){var canvas=GameObject.GetComponentInParent<Canvas>();if(canvas!=null){SortingLayerName=canvas.sortingLayerName;Order=canvas.sortingOrder+1;}}
            else if(!PendingOrder)return;
            SetOrder(SortingLayerName,Order);
        }
        async void Load()
        {
            IsLoaded=false;
            if(Services.UseNewResources()){Handle=await Services.LoadNewPrefab("Effect/"+GetEffectFolder()+Config.res);GameObject=Handle.Instantiate();}
            else GameObject=await Services.LoadLegacyPrefab("Effect/"+GetEffectFolder()+Config.res);
            // Original deliberately has no disposed gate, null guard or cancellation here.
            GameObject.transform.SetParent(Parent,false);GameObject.transform.localPosition=Position;GameObject.transform.localEulerAngles=Rotation;
            SetComponent();IsLoaded=true;
        }
        public virtual async void Start(){Load();await Await();Play();}
        public virtual void Dispose(){IsDisposed=true;if(GameObject!=null)Services.Destroy(GameObject);Handle?.Release();}
        public virtual async void Play()
        {
            if(GameObject==null){Stop();OnComplete?.Invoke(this);return;}
            GameObject.SetActive(IsActive);
            if(Config.duration>0){await Services.Wait(new WaitForSeconds((float)Config.duration));if(IsDisposed)return;Stop();OnComplete?.Invoke(this);}
        }
    }
    public sealed class OutgameUIEffect:OutgameBaseEffect
    {
        public OutgameUIEffect(OutgameEffectData c,Vector3 p,Vector3 r,Transform parent,bool setComponents,OutgameEffectServices s):base(c,p,r,parent,setComponents,s){}
        public override void Dispose()=>base.Dispose();
    }
    public sealed class OutgameFlyEffect:OutgameBaseEffect
    {
        public Vector3 Target;public float FlyTime;
        public OutgameFlyEffect(OutgameEffectData c,Vector3 p,Vector3 r,Vector3 target,float flyTime,Transform parent,bool setComponents,OutgameEffectServices s):base(c,p,r,parent,setComponents,s){Target=target;FlyTime=flyTime;}
        public override void Play()
        {
            GameObject.SetActive(IsActive);GameObject.transform.rotation=Quaternion.FromToRotation(Vector3.up,Target-GameObject.transform.position);
            Services.Move(GameObject.transform,Target,FlyTime,()=>{Dispose();OnComplete?.Invoke(this);});
        }
    }
    public sealed class OutgameLineEffect:OutgameBaseEffect
    {
        public Vector3 Target,Origin;readonly Func<Vector3,Vector2> toLocal;
        public OutgameLineEffect(OutgameEffectData c,Vector3 p,Vector3 r,Vector3 origin,Vector3 target,Transform parent,bool setComponents,OutgameEffectServices s,Func<Vector3,Vector2> toLocal):base(c,p,r,parent,setComponents,s){Target=target;Origin=origin;this.toLocal=toLocal;}
        public override async void Play()
        {
            base.Play();GameObject.transform.rotation=Quaternion.FromToRotation(Vector3.up,Target-Origin);
            float distance=(toLocal(Origin)-toLocal(Target)).magnitude;var scale=GameObject.transform.localScale;GameObject.transform.localScale=new Vector3(scale.x,distance/256,scale.z);
            GameObject.transform.position=Origin;GameObject.SetActive(IsActive);
            if(Config.duration>0){await Services.Wait(new WaitForSeconds((float)Config.duration));Stop();OnComplete?.Invoke(this);}
        }
    }
}
