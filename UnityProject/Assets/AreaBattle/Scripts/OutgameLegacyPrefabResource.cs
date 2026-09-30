using System;
using UnityEngine;
namespace AreaBattle
{
    // ResourcesInfo.Instantiate29863. Acquisition and ownership remain with ResourcesModule.
    public sealed class OutgameLegacyPrefabResource
    {
        Func<string,UnityEngine.Object> loadAsset;
        UnityEngine.Object mainObject;Sprite sprite;
        public string BundleName;
        public object[] Arguments;
        public bool IsReady;
        public bool IsUnloaded {get;private set;}
        public Action<OutgameLegacyPrefabResource> OnUnloaded;
        public AssetBundle Bundle {get;internal set;}
        public OutgameLegacyPrefabResource(AssetBundle bundle)
            :this(bundle,null){}
        // Explicit adapter for imported assets; this does not emulate bundle acquisition.
        public OutgameLegacyPrefabResource(AssetBundle bundle,Func<string,UnityEngine.Object> loadAsset)
        {Bundle=bundle;this.loadAsset=loadAsset;}
        // ResourcesInfo29864 clears references before notification, marks unloaded afterward.
        public void Unload(bool notify)
        {
            Arguments=null;sprite=null;mainObject=null;Bundle=null;loadAsset=null;
            if(notify){OnUnloaded?.Invoke(this);OnUnloaded=null;}
            IsUnloaded=true;
        }
        // ResourcesInfo29869: missing arguments return default; present values use a cast.
        public T GetArg<T>(int index)
        {return Arguments!=null&&Arguments.Length>index?(T)Arguments[index]:default;}
        // ResourcesInfo29862 native route; imported prefab adapter is deliberately separate.
        public T LoadAsset<T>(string name) where T:UnityEngine.Object
        {return Bundle?Bundle.LoadAsset<T>(name):null;}
        public GameObject Instantiate(string name,bool active)
        {
            var asset=loadAsset!=null?loadAsset(name):Bundle.LoadAsset(name);
            if(asset==null||!(asset is GameObject original))return null;
            var instance=UnityEngine.Object.Instantiate(original);
            instance.SetActive(active);
            instance.name=original.name;
            if(instance==null)throw new Exception("Please set the user!");
            return instance;
        }
    }
}
