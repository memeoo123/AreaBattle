using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameUiRootResources
    {
        void LoadPrefab(string path,Action<Func<string,bool,GameObject>> loaded);
        void Update(float deltaTime,float unscaledDeltaTime);
    }
    // UIModule.Initialize27385; generic resource Instantiate dispatch is provided by each source resource route.
    public sealed class OutgameUiModuleInitialization:IOutgameStartupModule
    {
        readonly Func<bool> modern;readonly Action<string,Action<Func<string,bool,GameObject>>> loadModern;
        readonly Func<IOutgameUiRootResources> legacy;readonly OutgameUiDisplaySettings display;
        readonly Func<string,GameObject> find;readonly Action<GameObject> persist;readonly Action<string> log,error;
        OutgameUiRootInitialization root;
        public GameObject CanvasRoot {get;private set;}
        public Camera UiCamera {get;private set;}
        public RectTransform UiRoot=>root?.UiRoot;
        public bool IsInitialized=>root!=null&&root.IsInitialized;
        public Action Initialized {get;set;}
        public OutgameUiModuleInitialization(Func<bool> modern,Action<string,Action<Func<string,bool,GameObject>>> loadModern,Func<IOutgameUiRootResources> legacy,OutgameUiDisplaySettings display,Action<string> log,Action<string> error,Func<string,GameObject> find=null,Action<GameObject> persist=null)
        {this.modern=modern;this.loadModern=loadModern;this.legacy=legacy;this.display=display;this.log=log;this.error=error;this.find=find??GameObject.FindGameObjectWithTag;this.persist=persist??(obj=>UnityEngine.Object.DontDestroyOnLoad(obj));}
        public void Initialize()
        {
            if(IsInitialized){Initialized?.Invoke();return;}
            log("开始加载UIModule");
            CanvasRoot=find("GFUICanvas");
            if(CanvasRoot==null)error("UIcanvas未找到");
            GameObject camera=null;
            if(CanvasRoot!=null)camera=CanvasRoot.transform.Find("UICamera").gameObject;
            if(camera==null)error("UI相机未找到");
            UiCamera=ReferenceEquals(camera,null)?null:camera.GetComponent<Camera>();
            persist(CanvasRoot);
            root=new OutgameUiRootInitialization(CanvasRoot,()=>display.IsPortrait,()=>display.HeightControlsWidthFixedWidth,()=>Initialized?.Invoke());
            if(modern()){loadModern("UI/UIRoot",Loaded);return;}
            legacy().LoadPrefab("UI/UIRoot",Loaded);
            legacy().Update(0f,0f);
        }
        void Loaded(Func<string,bool,GameObject> instantiate)=>root.Loaded(instantiate);
    }
}
